using System.ComponentModel;
using System.Globalization;
using System.IO;
using Navpaky.Core;
using Navpaky.Setup.Install;
using Navpaky.Ui;

namespace Navpaky.Setup.Wizard {
  // The install wizard's pages and choices (spec install-wizard, design D3, D4, D7). Every change
  // refreshes every binding: the pages are small and the window shows one at a time.
  internal sealed class WizardViewModel : INotifyPropertyChanged {
    private static readonly Lazy<string> _licence = new(ReadLicence);

    private readonly WizardFolders _folders;
    private readonly RunSetup _runSetup;
    private readonly Action<string> _startApp;
    private readonly CultureInfo _windowsCulture = CultureInfo.CurrentUICulture;

    private UiLanguage _language;
    private bool _licenceAccepted;
    private bool _keepsData = true;
    private bool _confirmingFresh;
    private bool _freshConfirmed;
    private FolderCheck _folder;
    private bool _launchAtSignIn = true;
    private bool _desktopShortcut;
    private SetupPhase _phase;
    private SetupOutcome? _outcome;

    public WizardViewModel(
        KeptItems? kept, WizardFolders folders, RunSetup runSetup, Action<string> startApp) {
      Kept = kept;
      _folders = folders;
      _runSetup = runSetup;
      _startApp = startApp;
      Pages = kept is null
          ? [WizardPage.Language, WizardPage.Licence, WizardPage.Folder, WizardPage.Options,
              WizardPage.Installing, WizardPage.Done]
          : [WizardPage.Language, WizardPage.Licence, WizardPage.WelcomeBack, WizardPage.Folder,
              WizardPage.Options, WizardPage.Installing, WizardPage.Done];
      _folder = Check(InstallLookup.Proposed(
          folders.ExistingInstall, kept?.Settings?.InstallFolder, folders.DefaultFolder));
      Language = kept?.Settings?.UiLanguage ?? UiLanguage.System;
      PresetOptions();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? Finished;

    public static string LicenceText => _licence.Value;

    public IReadOnlyList<WizardPage> Pages { get; }

    public WizardPage Page { get; private set; }

    public int StepNumber => Pages.ToList().IndexOf(Page) + 1;

    public int StepCount => Pages.Count;

    public bool CanGoBack => Page is not (WizardPage.Language or WizardPage.Installing
        or WizardPage.Done);

    public bool CanGoNext => Page switch {
      WizardPage.Licence => LicenceAccepted,
      WizardPage.WelcomeBack => !ConfirmingFresh,
      WizardPage.Folder => Folder.Verdict == FolderVerdict.Ok,
      WizardPage.Installing => false,
      _ => true,
    };

    public bool IsInstallStep => Page == WizardPage.Options;

    public UiLanguage Language {
      get {
        return _language;
      }

      set {
        _language = value;
        Localizer.Instance.Use(UiCulture.For(value, _windowsCulture));
        Changed();
      }
    }

    public bool UsesSystemLanguage {
      get => Language == UiLanguage.System;
      set => Pick(value, UiLanguage.System);
    }

    public bool UsesEnglish {
      get => Language == UiLanguage.English;
      set => Pick(value, UiLanguage.English);
    }

    public bool UsesUkrainian {
      get => Language == UiLanguage.Ukrainian;
      set => Pick(value, UiLanguage.Ukrainian);
    }

    public bool LicenceAccepted {
      get => _licenceAccepted;
      set => Set(ref _licenceAccepted, value);
    }

    public KeptItems? Kept { get; }

    public bool KeepsData {
      get {
        return _keepsData;
      }

      set {
        _keepsData = value;
        _freshConfirmed = false;
        PresetOptions();
        Changed();
      }
    }

    public bool StartsFresh {
      get => !KeepsData;
      set => KeepsData = !value;
    }

    public bool ConfirmingFresh => _confirmingFresh;

    public FolderCheck Folder => _folder;

    public string? FolderProblem => Folder.Verdict switch {
      FolderVerdict.Ok => null,
      FolderVerdict.NotEmpty => "FolderNotEmpty",
      FolderVerdict.OverlapsData => "FolderOverlapsData",
      FolderVerdict.NotWritable => "FolderNotWritable",
      _ => "FolderNotEnoughSpace",
    };

    public long NeededBytes => _folders.NeededBytes;

    public long FreeBytes => _folders.Probe.FreeBytes(Folder.Folder);

    public bool LaunchAtSignIn {
      get => _launchAtSignIn;
      set => Set(ref _launchAtSignIn, value);
    }

    public bool DesktopShortcut {
      get => _desktopShortcut;
      set => Set(ref _desktopShortcut, value);
    }

    // What the kept settings had, shown as a hint after "Use previous settings"; null otherwise.
    public bool? SignInBefore => Previous()?.LaunchAtSignIn;

    public bool? ShortcutBefore => Previous()?.DesktopShortcut;

    public SetupPhase Phase => _phase;

    public bool Failed => _outcome is SetupOutcome.Failed or SetupOutcome.MayStillRun;

    public bool MayStillRun => _outcome == SetupOutcome.MayStillRun;

    public bool CanRetry => _outcome == SetupOutcome.Failed;

    public string? LogPath { get; private set; }

    public bool StartNow { get; set; } = true;

    public Task Next() {
      if (!CanGoNext) {
        return Task.CompletedTask;
      }

      if (Page == WizardPage.WelcomeBack && StartsFresh && !_freshConfirmed) {
        _confirmingFresh = true;
        Changed();
        return Task.CompletedTask;
      }

      if (Page == WizardPage.Options) {
        return InstallAsync();
      }

      Show(Pages[StepNumber]);
      return Task.CompletedTask;
    }

    public void Back() {
      if (CanGoBack) {
        _confirmingFresh = false;
        Show(Pages[StepNumber - 2]);
      }
    }

    public void CancelFresh() {
      _confirmingFresh = false;
      KeepsData = true;
    }

    public void ConfirmFresh() {
      _confirmingFresh = false;
      _freshConfirmed = true;
      Show(WizardPage.Folder);
    }

    public void Pick(string folder) {
      if (!Folder.Locked) {
        _folder = Check(folder);
        Changed();
      }
    }

    public Task Retry() {
      return CanRetry ? InstallAsync() : Task.CompletedTask;
    }

    public void Finish() {
      if (StartNow) {
        _startApp(Path.Join(Folder.Folder, "Navpaky.exe"));
      }

      Finished?.Invoke(this, EventArgs.Empty);
    }

    private static string ReadLicence() {
      using Stream stream = typeof(WizardViewModel).Assembly
          .GetManifestResourceStream("Navpaky.Setup.LICENSE")!;
      using var reader = new StreamReader(stream);
      return reader.ReadToEnd();
    }

    private static void TryDelete(string file) {
      try {
        File.Delete(file);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        System.Diagnostics.Trace.WriteLine($"Hand-off file not deleted: {e.GetType().Name}");
      }
    }

    private async Task InstallAsync() {
      var attempt = Guid.NewGuid();
      LogPath = Path.Join(Path.GetTempPath(), $"NavpakySetup-{attempt:N}.log");
      _outcome = null;
      _phase = SetupPhase.Copying;
      Show(WizardPage.Installing);
      InstallChoices choices = Choices(attempt);
      SetupOutcome outcome;
      try {
        outcome = await _runSetup(choices, LogPath, new Progress<SetupPhase>(phase => {
          _phase = phase;
          Changed();
        }));
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException
          or System.ComponentModel.Win32Exception) {
        outcome = SetupOutcome.Failed;
      }

      Settle(choices, outcome);
    }

    // Design D4: the Start-fresh backup goes only after a confirmed install and comes back after
    // a failed one. A setup that may still run keeps everything; the next wizard start restores.
    private void Settle(InstallChoices choices, SetupOutcome outcome) {
      DataFolderPair data = _folders.Data;
      try {
        if (outcome == SetupOutcome.Succeeded) {
          FreshBackups.Discard(data, choices.Attempt);
        } else if (outcome == SetupOutcome.Failed) {
          FreshBackups.Restore(data, choices.Attempt);
        }
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        outcome = outcome == SetupOutcome.Succeeded ? outcome : SetupOutcome.MayStillRun;
      }

      if (outcome != SetupOutcome.MayStillRun) {
        TryDelete(Path.Join(data.Local, InstallChoices.FileName));
        TryDelete(Path.Join(data.Local, choices.AcknowledgementName));
      }

      _outcome = outcome;
      if (outcome == SetupOutcome.Succeeded) {
        Show(WizardPage.Done);
      } else {
        Changed();
      }
    }

    private InstallChoices Choices(Guid attempt) {
      KeptData data = KeptData.New;
      if (Kept is not null) {
        data = KeepsData ? KeptData.Keep : KeptData.Fresh;
      }

      return new InstallChoices(
          attempt, Language, LaunchAtSignIn, DesktopShortcut, data, Folder.Folder);
    }

    private FolderCheck Check(string pick) {
      return InstallTarget.Check(
          pick, _folders.ExistingInstall, _folders.Data, _folders.NeededBytes, _folders.Probe);
    }

    // The kept settings preset the options after "Use previous settings"; the defaults otherwise.
    private void PresetOptions() {
      _launchAtSignIn = Previous()?.LaunchAtSignIn ?? true;
      _desktopShortcut = Previous()?.DesktopShortcut ?? false;
    }

    private Settings? Previous() {
      return KeepsData ? Kept?.Settings : null;
    }

    private void Pick(bool chosen, UiLanguage language) {
      if (chosen) {
        Language = language;
      }
    }

    private void Show(WizardPage page) {
      Page = page;
      Changed();
    }

    private void Set(ref bool field, bool value) {
      field = value;
      Changed();
    }

    private void Changed() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
  }
}
