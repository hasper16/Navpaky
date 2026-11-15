using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using H.NotifyIcon;
using Rekey.App.Feedback;
using Rekey.App.Onboarding;
using Rekey.App.Tray;
using Rekey.App.Updates;
using Rekey.App.ViewModels;
using Rekey.App.Views;
using Rekey.Core;
using Rekey.Core.Diagnostics;
using Rekey.Core.Statistics;
using Rekey.Core.Words;
using Rekey.Win32;
using Rekey.Win32.Diagnostics;
using Rekey.Win32.Focus;
using Rekey.Win32.Hotkeys;
using Velopack.Locators;
#if DEBUG
using Rekey.Win32.Updates;
using Velopack.Sources;
#endif
using Wpf.Ui.Appearance;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace Rekey.App {
  [SuppressMessage(
      "Design",
      "CA1001:Types that own disposable fields should be disposable",
      Justification = "WPF ends the Application through OnExit, which disposes the fields.")]
  internal sealed partial class App {
    private const string _restartedAfterFreeze = "--restarted-after-freeze";
#if DEBUG
    private const string _checkFreeze = "--check-freeze";
    private const string _checkSelfPause = "--check-self-pause";
    private const string _checkAutoResume = "--check-auto-resume";
    private const string _checkQuit = "--check-quit";
    private const string _checkSettings = "--check-settings";
    private const string _updateFeed = "--update-feed";
    private const string _updatePin = "--update-pin";
#endif

    // The tray says how many minutes of a Pause remain and notices its end.
    private static readonly TimeSpan _trayRefreshPeriod = TimeSpan.FromSeconds(20);

    // Design D2 of add-statistics-and-diagnostics: counters reach the disk every 60 s.
    private static readonly TimeSpan _statsFlushPeriod = TimeSpan.FromSeconds(60);

    // How long a restarted Rekey waits for the frozen one to exit before it starts anyway.
    private static readonly TimeSpan _oldProcessExitWait = TimeSpan.FromSeconds(10);

    private static readonly string _dataFolder = DataFolder.Resolve(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        VelopackLocator.IsCurrentSet ? VelopackLocator.Current : null);

    private static readonly string _analysisFolder = Path.Join(_dataFolder, "analysis");

    private static readonly WordListsStore _wordsStore = new(_dataFolder);

    private readonly SettingsStore _settingsStore = new(_dataFolder);
    private readonly RestartStateStore _restartStore = new(_dataFolder);
    private readonly StatsStore _statsStore = new(_dataFolder);
    private readonly WordListsSaver _wordsSaver =
        new(_wordsStore, TimeProvider.System, ReportWordsNotSaved);

    private readonly SignInLaunch _signIn = new();

    // The start-at-sign-in choice last written to or read from Windows; UI thread only.
    private bool _launchAtSignIn;

    private SingleInstanceGuard? _guard;
    private SettingsHub? _hub;
    private TrayViewModel? _tray;
    private TrayIconView? _trayView;
    private ForegroundApps? _foregroundApps;
    private DispatcherTimer? _trayTimer;
    private DispatcherTimer? _statsTimer;
    private WordsViewModel? _words;
    private SettingsWindow? _settingsWindow;
    private StatisticsWindow? _statisticsWindow;
    private WelcomeWindow? _welcomeWindow;
    private WelcomeViewModel? _welcome;
    private ExportWindow? _exportWindow;
    private CursorIndicator? _indicator;
    private SwitchSoundPlayer? _sounds;
#if DEBUG
    private bool _checkingTray;
    private bool _autoResume;
    private bool _autoQuit;
#endif
    private InputWorker? _worker;
    private FreezeGuard? _freezeGuard;
    private QueuedIncidentLog? _incidents;
    private UpdateScheduler? _updates;
    private ExecutableWatch? _executableWatch;

    // The worker's Word counts as of the latest Trigger Key; only the worker thread writes it.
    private volatile StrongBox<WordCounts>? _wordCounts;

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      WaitForFrozenInstance(e.Args);

      // The guard comes first: a second instance must leave without touching settings or the
      // tray.
      _guard = SingleInstanceGuard.TryAcquire(SingleInstanceGuard.NameForCurrentUser());
      if (_guard is null) {
        Shutdown();
        return;
      }

      _incidents = new QueuedIncidentLog(new IncidentLog(_dataFolder), ReportIncidentNotSaved);
      ApplicationThemeManager.ApplySystemTheme();

      // Windows holds the truth about starting at sign-in: Task Manager can change it.
      _launchAtSignIn = _signIn.IsOn();
      Settings loaded = _settingsStore.Load().ForNewSession() with {
        LaunchAtSignIn = _launchAtSignIn,
      };
      _hub = new SettingsHub(
          loaded, _settingsStore.Save, TimeProvider.System, ReportSettingsNotSaved);
      _indicator = new CursorIndicator();
      _sounds = new SwitchSoundPlayer();
      _tray = new TrayViewModel(_hub, TimeProvider.System, () => _worker?.ResumeSelfPause());
      var trayIcon = (TaskbarIcon)FindResource("TrayIcon");
      var commands = new TrayCommands(
          OpenSettings, OpenStatistics, OnUpdatesItem, OpenWelcome, OpenAbout, () => Shutdown());
      _trayView = new TrayIconView(trayIcon, _tray, commands);
#if DEBUG
      AddDebugMenuItems(_trayView);
#endif
      _trayView.Refresh();

      // Efficiency Mode throttles the process; the keyboard hook must stay fast.
      trayIcon.ForceCreate(enablesEfficiencyMode: false);
      StartInput(_hub.Current);
      _hub.Changed += OnSettingsChanged;

      // After the subscription, so the first start's launch at sign-in reaches Windows.
      _hub.Update(WelcomeViewModel.FirstStartDefaults);
      StartTrayFollowers();
      StartUpdates();
      StartLocationChecks();
      if (WelcomeViewModel.OpensAtStart(_hub.Current)) {
        OpenWelcome();
      }
#if DEBUG
      if (e.Args.Contains(_checkFreeze)) {
        FreezeTwiceForCheck();
      }

      if (e.Args.Contains(_checkUi)) {
        StartUiCheck();
      }

      if (e.Args.Contains(_checkSettings)) {
        // For tools/Rekey.DesktopChecks: the settings window open from the start, as after
        // choosing Settings in the tray.
        OpenSettings();
      }

      if (e.Args.Contains(_checkSelfPause)) {
        _checkingTray = true;
        _autoResume = e.Args.Contains(_checkAutoResume);
        _autoQuit = e.Args.Contains(_checkQuit);
        DropHookTwiceForCheck();
      }
#endif
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {
      base.OnSessionEnding(e);

      // Windows may end the process soon after sign-out starts, so write settings now.
      FlushSettings();
      _wordsSaver.Flush();
      SaveStatistics();
    }

    protected override void OnExit(ExitEventArgs e) {
      try {
        FlushSettings();
        TraceWordCounts();
      } finally {
        _freezeGuard?.Dispose();
        _worker?.Dispose();
        _statsTimer?.Stop();
        SaveStatistics();

        // After the worker, so its last change is saved too.
        _wordsSaver.Flush();
        _wordsSaver.Dispose();
        _sounds?.Dispose();

        // After the worker and the freeze guard, so their last Incidents are written too.
        _incidents?.Dispose();
        _trayTimer?.Stop();
        _updates?.Dispose();
        _executableWatch?.Dispose();
        _foregroundApps?.Dispose();
        _trayView?.Dispose();
        _welcome?.Dispose();
        _hub?.Dispose();
        _guard?.Dispose();
        base.OnExit(e);
      }
    }

    private static void BringToFront(Window window) {
      if (window.WindowState == WindowState.Minimized) {
        window.WindowState = WindowState.Normal;
      }

      window.Activate();
    }

    private static T ReadForExport<T>(Func<T> read, T fallback) {
      try {
        return read();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        Trace.WriteLine($"Rekey could not read a log for export: {e.GetType().Name}");
        return fallback;
      }
    }

    private static HotkeyConflict CheckHotkey(Hotkey hotkey) {
      return HotkeyConflicts.Check(hotkey, LayoutSwitchShortcuts.Read(), HotkeyProbe.IsTaken);
    }

    private static void OpenLanguageSettings() {
      OpenWithShell("ms-settings:regionlanguage");
    }

    // Explorer by its full path opens a Settings page or a web page, as a shell link would.
    private static void OpenWithShell(string target) {
      string explorer = Path.Join(
          Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
      try {
        Process.Start(explorer, target)?.Dispose();
      } catch (Win32Exception) {
        Trace.WriteLine("Rekey could not open a Settings or web page.");
      }
    }

    // Runs the setup that came with this copy, or opens the releases page.
    private static void Install(string executable) {
      string source = InstallSource.For(Path.GetDirectoryName(executable)!, File.Exists);
      if (source == InstallSource.ReleasesPage) {
        OpenWithShell(source);
        return;
      }

      try {
        Process.Start(source)?.Dispose();
      } catch (Win32Exception) {
        Trace.WriteLine("Rekey could not start its setup.");
      }
    }

    private static async Task AskToInstall(
        string executable, string title, string text, string closeText) {
      var question = new Wpf.Ui.Controls.MessageBox {
        Title = title,
        Content = text,
        PrimaryButtonText = Strings.LocationInstall,
        CloseButtonText = closeText,
      };
      Wpf.Ui.Controls.MessageBoxResult answer = await question.ShowDialogAsync();
      if (answer == Wpf.Ui.Controls.MessageBoxResult.Primary) {
        Install(executable);
      }
    }

    private static LocationFolders FoldersOf(IVelopackLocator? locator) {
      string localAppData =
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      return new LocationFolders(
          locator?.RootAppDir,
          [Path.GetTempPath(), Path.Join(localAppData, "Temp")],
          KnownFolders.Downloads());
    }

    // The type only: the message names a path inside the user profile.
    private static void ReportStatisticsNotSaved(Exception error) {
      Trace.WriteLine($"Rekey could not save its statistics: {error.GetType().Name}");
    }

    // The type only: the message names a path inside the user profile.
    private static void ReportIncidentNotSaved(Exception error) {
      Trace.WriteLine($"Rekey could not write its Incident log: {error.GetType().Name}");
    }

    // The type only: the message names a path inside the user profile.
    private static void ReportSettingsNotSaved(Exception error) {
      Trace.WriteLine($"Rekey could not save its settings: {error.GetType().Name}");
    }

    // The type only: the message names a path inside the user profile.
    private static void ReportWordsNotSaved(Exception error) {
      Trace.WriteLine($"Rekey could not save its word lists: {error.GetType().Name}");
    }

    // A Rekey restarted after a freeze waits for the old process to leave, so the old one
    // releases the single-instance claim and its hooks first.
    private static void WaitForFrozenInstance(string[] args) {
      int flag = Array.IndexOf(args, _restartedAfterFreeze);
      if (flag < 0 || flag + 1 >= args.Length
          || !int.TryParse(args[flag + 1], CultureInfo.InvariantCulture, out int processId)) {
        return;
      }

      try {
        using var old = Process.GetProcessById(processId);
        old.WaitForExit(_oldProcessExitWait);
      } catch (ArgumentException) {
        // The old process already exited.
      }
    }

    private void StartInput(Settings settings) {
      WordListsDocument? words = _wordsStore.Load();
      _words = new WordsViewModel(
          edit => _worker?.EditWordList(edit),
          () => TimeSpan.FromMilliseconds(Environment.TickCount64));
      _words.Show(words ?? WordListsDocument.Empty);
      _worker = new InputWorker(settings, RecordIncident, TimeProvider.System) {
        InitialWords = words ?? WordListsDocument.Empty,
        InitialStatistics = LoadStatistics(settings),
        AnalysisLogFolder = _analysisFolder,
      };

      // A file Rekey could not read stays as it is: this session learns but does not save.
      if (words is null) {
        Trace.WriteLine("Rekey could not read its word lists and will not save them.");
      } else {
        _worker.WordListsChanged += _wordsSaver.Changed;
      }

      _worker.WordListsChanged += lists => Dispatcher.BeginInvoke(() => _words.Show(lists));
      _worker.SelfPauseChanged += until => Dispatcher.BeginInvoke(() => ShowSelfPause(until));
      _worker.Observed += RememberWordCounts;
      _worker.Observed += FollowLayout;

      // The window in front is read when the Switch happens, before focus can move on.
      _worker.Switched += feedback => {
        nint front = ForegroundApps.CurrentWindow();
        Dispatcher.BeginInvoke(() => RespondToSwitch(feedback, front));
      };
      try {
        _worker.Start();
      } catch (Win32Exception) {
        // Without hooks Rekey cannot see typing; it stays in the tray so the user can quit.
        Trace.WriteLine("Rekey could not install its keyboard hooks.");
        return;
      }

      _freezeGuard = new FreezeGuard(
          _worker,
          settings,
          _restartStore.LoadLastRestart(),
          RecordIncident,
          RestartAfterFreeze,
          TimeProvider.System);
      _freezeGuard.Start();
    }

    // Spec statistics-and-diagnostics: with statistics off, stored counters are deleted.
    private StatsDocument LoadStatistics(Settings settings) {
      try {
        if (settings.StatisticsEnabled) {
          return _statsStore.Load();
        }

        _statsStore.Delete();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        ReportStatisticsNotSaved(e);
      }

      return StatsDocument.Empty;
    }

    // UI thread only, so a save never races the deletion that turning statistics off asks for.
    private void SaveStatistics() {
      if (_worker is null || _hub is null) {
        return;
      }

      try {
        if (_hub.Current.StatisticsEnabled) {
          _statsStore.Save(_worker.Statistics);
        } else {
          _statsStore.Delete();
        }
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        ReportStatisticsNotSaved(e);
      }
    }

    // Runs on the worker thread after each input event.
    private void RememberWordCounts(InputEvent inputEvent) {
      if (inputEvent is KeyDown { Kind: KeyKind.Space or KeyKind.Enter or KeyKind.Tab }
          && _worker is { } worker) {
        _wordCounts = new StrongBox<WordCounts>(worker.WordCounts);
      }
    }

    // How many Words got no evaluation because a key could not be confirmed outside a Password
    // Field (design D8 of add-auto-correction): counters only, never text.
    private void TraceWordCounts() {
      if (_wordCounts?.Value is not { } counts) {
        return;
      }

      string dropped = counts.Unconfirmed.ToString(CultureInfo.InvariantCulture);
      string line = string.Create(
          CultureInfo.InvariantCulture,
          $"words-ended={counts.Ended} evaluated={counts.Evaluated} unconfirmed={dropped}");
      Trace.WriteLine($"Rekey {line}");
#if DEBUG
      File.WriteAllText(Path.Join(_dataFolder, "word-counts.txt"), line);
#endif
    }

    // Incidents carry enum values only, so neither the trace nor the log holds typed text.
    // Called on the worker, the freeze guard and the UI thread.
    private void RecordIncident(Incident incident) {
      string details = string.Join(',', incident.Details);
      Trace.WriteLine(string.Create(
          CultureInfo.InvariantCulture,
          $"Rekey Incident {incident.Time:O} {incident.Type} {details}"));
      _incidents?.Record(incident);
    }

    // Runs on the freeze guard's thread while the worker is stuck; the UI thread may still run.
    private void RestartAfterFreeze() {
      // Without the saved time the next process could not keep the once-per-10-minutes limit,
      // so Rekey keeps running instead; the freeze Incident is already recorded.
      if (!_restartStore.TrySaveLastRestart(DateTimeOffset.UtcNow)) {
        Trace.WriteLine("Rekey could not save the restart time and will not restart.");
        return;
      }

      try {
        Process.Start(
            Environment.ProcessPath!,
            [_restartedAfterFreeze, Environment.ProcessId.ToString(CultureInfo.InvariantCulture)]);
      } catch (Win32Exception) {
        Trace.WriteLine("Rekey could not start its replacement and keeps running.");
        return;
      }

      Dispatcher.Invoke(() => _trayView?.Dispose(), TimeSpan.FromSeconds(1));
      FlushSettings();
      _wordsSaver.Flush();
      Dispatcher.Invoke(SaveStatistics, TimeSpan.FromSeconds(1));

      // The freeze-restart Incident is still queued.
      _incidents?.Dispose();
      Environment.Exit(0);
    }

#if DEBUG
    // For tools/Rekey.DesktopChecks: two 7 s freezes 15 s apart, which the freeze guard answers
    // with a restart while the checks keep typing.
    private void FreezeTwiceForCheck() {
      _ = Task.Run(async () => {
        await Task.Delay(TimeSpan.FromSeconds(3));
        _worker?.FreezeForCheck(TimeSpan.FromSeconds(7));
        await Task.Delay(TimeSpan.FromSeconds(15));
        _worker?.FreezeForCheck(TimeSpan.FromSeconds(7));
      });
    }

    // For the manual tray check: the next key after 3 s and after 10 s stalls the hook past
    // Windows' limit, so Windows drops it twice and the app pauses itself while the user types.
    // The first key typed after start stalls the hook past Windows' limit, and the first key
    // typed 8 s later does again: two drops, so the app pauses itself while the user keeps
    // typing.
    private void DropHookTwiceForCheck() {
      _worker?.StallHookForCheck(TimeSpan.FromSeconds(3));
      _ = Task.Run(async () => {
        await Task.Delay(TimeSpan.FromSeconds(8));
        _worker?.StallHookForCheck(TimeSpan.FromSeconds(3));
      });
      WriteTrayState();
    }

    // For tools/Rekey.DesktopChecks: what the tray shows, written after every change.
    private void WriteTrayState() {
      if (!_checkingTray || _tray is null || _trayView is null) {
        return;
      }

      bool paused = _tray.State == TrayState.SelfPaused;
      bool resume = _trayView.FindItem(Strings.MenuResume) is not null;
      WriteCheckFile("check-tray.txt", $"tooltip-self-paused={paused} resume-item={resume}");
    }

    private void AddDebugMenuItems(TrayIconView trayView) {
      var freeze = new MenuItem { Header = "Debug: freeze input for 7 s" };
      freeze.Click += (_, _) => _worker?.FreezeForCheck(TimeSpan.FromSeconds(7));
      trayView.ExtraItems.Add(freeze);
    }

#endif
    private void StartTrayFollowers() {
      _foregroundApps = new ForegroundApps();
      _foregroundApps.Changed += app => {
        _tray!.CurrentApp = new CurrentApp(app, AppNames.Of(app));
        RefreshTray();
      };
      _foregroundApps.Start();
      _trayTimer = new DispatcherTimer(
          _trayRefreshPeriod, DispatcherPriority.Background, (_, _) => RefreshTray(), Dispatcher);
      _trayTimer.Start();
      _statsTimer = new DispatcherTimer(
          _statsFlushPeriod,
          DispatcherPriority.Background,
          (_, _) => SaveStatistics(),
          Dispatcher);
      _statsTimer.Start();
    }

    private void StartUpdates() {
      VelopackUpdates service = VelopackUpdates.FromReleases(() => Shutdown(), RecordIncident);
#if DEBUG
      // For the by-hand update checks: a local feed folder instead of GitHub Releases, and a
      // test publisher (subject, then issuing CA thumbprint) instead of the Rekey one; a test CA
      // publishes no revocation list.
      string[] args = Environment.GetCommandLineArgs();
      int feed = Array.IndexOf(args, _updateFeed);
      int pin = Array.IndexOf(args, _updatePin);
      if (feed >= 0 && feed + 1 < args.Length) {
        var publisher = pin >= 0 && pin + 2 < args.Length
            ? new PublisherPin(args[pin + 1], [args[pin + 2]])
            : PublisherPin.None;
        var manager = new RekeyUpdateManager(
            new SimpleFileSource(new DirectoryInfo(args[feed + 1])),
            package => PackageSignature.Verify(
                package, RekeyPublisher.MainExe, publisher, checkRevocation: false));
        service = new VelopackUpdates(
            manager, () => Shutdown(), RecordIncident, TimeProvider.System);
      }
#endif
      _updates = new UpdateScheduler(
          service, TimeProvider.System, action => Dispatcher.BeginInvoke(action));
      _updates.StatusChanged += () => {
        _tray!.Updates = _updates.Status;
        RefreshTray();
      };
      _updates.Start(_hub!.Current.AutoCheckForUpdates);
    }

    // Spec onboarding: a warning at start when this copy may be cleaned up, and a notice when
    // the executable disappears while Rekey runs.
    private void StartLocationChecks() {
      if (Environment.ProcessPath is not { } executable) {
        return;
      }

#if DEBUG
      const bool debugBuild = true;
#else
      const bool debugBuild = false;
#endif
      LocationFolders folders =
          FoldersOf(VelopackLocator.IsCurrentSet ? VelopackLocator.Current : null);
      if (AppLocation.Classify(executable, folders, debugBuild) == LocationKind.Temporary) {
        _ = AskToInstall(
            executable, Strings.LocationTitle, Strings.LocationWarning, Strings.LocationNotNow);
      }

      _executableWatch = new ExecutableWatch(executable, TimeProvider.System);
      _executableWatch.Disappeared += () => Dispatcher.BeginInvoke(() => AskToInstall(
          executable, Strings.MovedTitle, Strings.MovedNotice, Strings.MovedClose));
      _executableWatch.Start();
    }

    // The tray's update item checks for updates, or restarts into one that is ready.
    private void OnUpdatesItem() {
      if (_updates?.Status == UpdateStatus.ReadyToRestart) {
        _updates.RestartToUpdate();
      } else {
        _updates?.CheckNow();
      }
    }

    private void RefreshTray() {
      _trayView?.Refresh();
#if DEBUG
      WriteTrayState();
      WriteUiState();
#endif
    }

    // Runs on the worker thread after each input event; only Layout changes reach the UI.
    private void FollowLayout(InputEvent inputEvent) {
      if (inputEvent is LayoutsInstalledChanged installed) {
        Dispatcher.BeginInvoke(() => _welcome?.UseLayouts(installed.Layouts));
      }

      if (inputEvent is LayoutChanged changed) {
        string code = changed.Layout.LanguageCode;
        Dispatcher.BeginInvoke(() => {
          _tray!.LanguageCode = code;
          RefreshTray();
        });
      }
    }

    private void RespondToSwitch(SwitchFeedback feedback, nint front) {
      SwitchResponse response = SwitchResponse.For(feedback, _hub!.Current);
      if (response.Indicator) {
        _indicator!.Show(
            feedback.Target.LanguageCode,
            ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark);
      }

      if (response.Sound) {
        _sounds!.Play(_hub.Current.SwitchSound);
      }

      if (feedback.Cause == SwitchCause.Correction) {
        NoteCorrection(front);
      }
#if DEBUG
      NoteSwitchForCheck(feedback);
      WriteUiState();
#endif
    }

    private void OnSettingsChanged(Settings settings) {
      _worker?.UseSettings(settings);
      _freezeGuard?.UseSettings(settings);
      _updates?.UseAutomaticChecks(settings.AutoCheckForUpdates);
      if (!settings.StatisticsEnabled) {
        SaveStatistics();
      }

      // Only the user's own change of this switch writes to Windows, so an unrelated change
      // never undoes a disable made in Task Manager meanwhile.
      if (settings.LaunchAtSignIn != _launchAtSignIn && Environment.ProcessPath is { } path) {
        _launchAtSignIn = settings.LaunchAtSignIn;
        _signIn.Set(settings.LaunchAtSignIn, path);
      }

      RefreshTray();
    }

    // The try-it field confirms the Correction; a first real one gets the one-time Undo tip.
    private void NoteCorrection(nint front) {
      bool inTryIt = _welcomeWindow?.IsTryItIn(front) == true;
      if (inTryIt) {
        _welcome?.NoteTryItCorrection();
      }

      if (CorrectionTip.IsDue(_hub!.Current, inTryIt)) {
        _hub.Update(CorrectionTip.Shown);
        _trayView?.Notify(Strings.CorrectionTipTitle, Strings.CorrectionTip);
#if DEBUG
        NoteTipForCheck();
#endif
      }
    }

    // One welcome window at a time: asking again brings it to the front.
    private void OpenWelcome() {
      if (_welcomeWindow is null) {
        ReadLaunchAtSignIn();
        var welcome = new WelcomeViewModel(_hub!, _worker?.InstalledLayouts ?? []);
        var hotkey = new HotkeyViewModel(_hub!, CheckHotkey);
        bool dark = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;
        _welcome = welcome;
        _welcomeWindow = new WelcomeWindow(
            welcome, new HotkeyPage(hotkey), dark, OpenLanguageSettings);
        _welcomeWindow.Closed += (_, _) => {
          welcome.Close();
          welcome.Dispose();
          hotkey.Dispose();
          _welcome = null;
          _welcomeWindow = null;
        };
        _welcomeWindow.Show();
      }

      BringToFront(_welcomeWindow);
    }

    // The user may have turned Rekey off under Startup apps in Task Manager meanwhile.
    private void ReadLaunchAtSignIn() {
      _launchAtSignIn = _signIn.IsOn();
      _hub!.Update(settings => settings with { LaunchAtSignIn = _launchAtSignIn });
    }

    private void OpenSettings() {
      ShowSettings(typeof(GeneralPage));
    }

    private void OpenAbout() {
      ShowSettings(typeof(AboutPage));
    }

    // One settings window at a time: asking again brings it to the front.
    private void ShowSettings(Type page) {
      if (_settingsWindow is null) {
        ReadLaunchAtSignIn();
        _settingsWindow = CreateSettingsWindow();
#if DEBUG
        NoteSettingsOpenedForCheck();
#endif
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
      }

      if (_settingsWindow.WindowState == WindowState.Minimized) {
        _settingsWindow.WindowState = WindowState.Normal;
      }

      _settingsWindow.Activate();
      _settingsWindow.Show(page);
    }

    private SettingsWindow CreateSettingsWindow() {
      var settings = new SettingsViewModel(_hub!);
      var hotkey = new HotkeyViewModel(_hub!, CheckHotkey);
      var apps = new ExcludedAppsViewModel(_hub!, AppNames.OfKey);
      bool dark = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;
      SettingsSection[] sections = [
          new(Strings.PageGeneral, SymbolRegular.Settings24,
              new GeneralPage(settings, sound => _sounds!.Play(sound))),
          new(Strings.PageCorrection, SymbolRegular.TextGrammarCheckmark24,
              new CorrectionPage(settings)),
          new(Strings.PageHotkey, SymbolRegular.Keyboard24, new HotkeyPage(hotkey)),
          new(Strings.PageAppsAndWords, SymbolRegular.AppsList24,
              new AppsAndWordsPage(apps, _words!)),
          new(Strings.PageDiagnostics, SymbolRegular.ShieldCheckmark24,
              new DiagnosticsPage(settings, OpenStatistics, OpenExport)),
          new(Strings.PageAbout, SymbolRegular.Info24, new AboutPage(dark)),
      ];
      var window = new SettingsWindow(sections);
      window.Closed += (_, _) => {
        settings.Dispose();
        hotkey.Dispose();
        apps.Dispose();
      };
      return window;
    }

    // One statistics window at a time: asking again brings it to the front.
    private void OpenStatistics() {
      if (_statisticsWindow is null) {
        var model = new StatisticsViewModel(
            () => _worker?.Statistics ?? StatsDocument.Empty,
            () => DateOnly.FromDateTime(DateTime.Now));
        _statisticsWindow = new StatisticsWindow(model);
        _statisticsWindow.Closed += (_, _) => _statisticsWindow = null;
        _statisticsWindow.Show();
      }

      BringToFront(_statisticsWindow);
    }

    private void OpenExport() {
      if (_exportWindow is null) {
        DiagnosticsFacts facts = GatherDiagnostics();
        var model = new ExportViewModel(
            includeLog => DiagnosticsReport.Build(facts, PersonalNames.Current, includeLog),
            Clipboard.SetText);
        _exportWindow = new ExportWindow(model);
        _exportWindow.Closed += (_, _) => _exportWindow = null;
        _exportWindow.Show();
      }

      BringToFront(_exportWindow);
    }

    // Read once when the dialog opens, so the preview does not change under the user.
    private DiagnosticsFacts GatherDiagnostics() {
      string version = Assembly.GetExecutingAssembly()
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
          ?? "0";

      // Each log on its own, so one that cannot be read still leaves the other in the export.
      IReadOnlyList<IncidentEntry> incidents = ReadForExport(
          () => new IncidentLog(_dataFolder).Read(), []);
      IReadOnlyList<string> analysis = ReadForExport(
          () => AnalysisLog.ReadAll(_analysisFolder), []);

      return new DiagnosticsFacts(
          DateTimeOffset.Now,
          version.Split('+')[0],
          Environment.OSVersion.Version.ToString(),
          _worker?.InstalledLayouts ?? [],
          _hub!.Current,
          _worker?.Statistics ?? StatsDocument.Empty,
          incidents,
          analysis);
    }

    private void ShowSelfPause(DateTimeOffset? until) {
      if (_tray is null) {
        return;
      }

      _tray.SelfPausedUntil = until;
      RefreshTray();
#if DEBUG
      if (until is null && _autoQuit) {
        // For the desktop checks: quit the way the tray's Quit does, 1 s after resuming.
        _ = Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(
            _ => Dispatcher.BeginInvoke(() => Shutdown()), TaskScheduler.Default);
      }

      if (until is not null && _autoResume) {
        // For the desktop checks: choose Resume as a person would, 2 s later.
        _ = Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(
            _ => Dispatcher.BeginInvoke(
                () => _trayView?.FindItem(Strings.MenuResume)
                    ?.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent))),
            TaskScheduler.Default);
      }
#endif
    }

    private void FlushSettings() {
      _hub?.Flush();
    }
  }
}
