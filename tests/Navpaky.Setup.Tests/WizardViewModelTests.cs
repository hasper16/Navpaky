using System.Globalization;
using System.IO;
using Navpaky.Core;
using Navpaky.Setup.Install;
using Navpaky.Setup.Wizard;
using Navpaky.Ui;
using Xunit;

namespace Navpaky.Setup.Tests {
  // Tasks 6.2–6.6 and 6.8 of add-install-wizard (spec install-wizard). The interface language is
  // process-wide, so these tests run one at a time.
  [Collection(nameof(SharedLanguage))]
  public sealed class WizardViewModelTests : IDisposable {
    private const long _needed = 230_000_000;
    private const string _settingsJson = "{ \"version\": 1 }";

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-wizard").FullName;
    private readonly FakeFolders _folders = new();
    private readonly DataFolderPair _data;
    private readonly List<InstallChoices> _ran = [];
    private readonly List<bool> _ranForAllUsers = [];
    private readonly List<string> _started = [];
    private readonly string _defaultFolder;
    private readonly string _machineFolder;
    private SetupOutcome _outcome = SetupOutcome.Succeeded;
    private Exception? _fails;

    public WizardViewModelTests() {
      _data = new DataFolderPair(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
      _defaultFolder = Path.Join(_root, "Programs", "Navpaky");
      _machineFolder = Path.Join(_root, "Program Files", "Navpaky");
      Localizer.Instance.Use(CultureInfo.GetCultureInfo("en"));
    }

    public void Dispose() {
      Localizer.Instance.Use(CultureInfo.GetCultureInfo("en"));
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void ANewInstallHasNoWelcomeBackPage() {
      WizardViewModel model = Wizard();

      WizardPage[] expected = [
          WizardPage.Language, WizardPage.Licence, WizardPage.Folder, WizardPage.Options,
          WizardPage.Installing, WizardPage.Done];
      Assert.Equal(expected, model.Pages);
      Assert.Equal(WizardPage.Language, model.Page);
      Assert.Equal((1, 6), (model.StepNumber, model.StepCount));
      Assert.False(model.CanGoBack);
    }

    [Fact]
    public void SameAsWindowsIsTheDefaultLanguage() {
      Assert.True(Wizard().UsesSystemLanguage);
    }

    [Fact]
    public void PickingALanguageSwitchesEveryTextAtOnce() {
      WizardViewModel model = Wizard();
      model.UsesEnglish = true;
      string english = Localizer.Instance["SetupNext"];

      model.UsesUkrainian = true;

      Assert.Equal(UiLanguage.Ukrainian, model.Language);
      Assert.Equal("uk", Localizer.Instance.Culture.Name);
      Assert.NotEqual(english, Localizer.Instance["SetupNext"]);
    }

    [Fact]
    public async Task NextOnTheLicenceWaitsForAcceptance() {
      WizardViewModel model = Wizard();
      await model.Next();

      Assert.Equal(WizardPage.Licence, model.Page);
      Assert.False(model.CanGoNext);
      model.LicenceAccepted = true;
      Assert.True(model.CanGoNext);
    }

    [Fact]
    public void TheLicenceTextIsTheRepositorysLicence() {
      Assert.Contains("GNU GENERAL PUBLIC LICENSE", WizardViewModel.LicenceText);
    }

    // The wizard ships as one file, so the fonts' licences travel inside it and show here.
    [Fact]
    public void TheLicenceTextNamesTheFontLicences() {
      Assert.Contains("Inter Project Authors", WizardViewModel.LicenceText);
      Assert.Contains("Grunt Grotesk", WizardViewModel.LicenceText);
    }

    [Fact]
    public async Task KeptDataShowsWelcomeBackWithPreviousSettingsSelected() {
      WizardViewModel model = Wizard(Kept(new Settings()));

      await ToPage(model, WizardPage.WelcomeBack);

      Assert.Equal(7, model.StepCount);
      Assert.True(model.KeepsData);
      Assert.NotNull(model.Kept);
    }

    [Fact]
    public async Task StartFreshAsksFirstAndCancelReturnsToTheChoice() {
      WizardViewModel model = Wizard(Kept(new Settings()));
      await ToPage(model, WizardPage.WelcomeBack);

      model.StartsFresh = true;
      await model.Next();

      Assert.Equal(WizardPage.WelcomeBack, model.Page);
      Assert.True(model.ConfirmingFresh);
      Assert.False(model.CanGoNext);
      model.CancelFresh();
      Assert.False(model.ConfirmingFresh);
      Assert.True(model.KeepsData);
    }

    [Fact]
    public async Task DeleteAndContinueInstallsFresh() {
      WizardViewModel model = Wizard(Kept(new Settings()));
      await ToPage(model, WizardPage.WelcomeBack);
      model.StartsFresh = true;
      await model.Next();

      model.ConfirmFresh();

      Assert.Equal(WizardPage.Folder, model.Page);
      await ToPage(model, WizardPage.Done);
      Assert.Equal(KeptData.Fresh, _ran[0].Data);
    }

    [Fact]
    public async Task TheFolderPageShowsTheProposedFolderAndSpace() {
      _folders.Free = 9_000_000_000;
      WizardViewModel model = Wizard();

      await ToPage(model, WizardPage.Folder);

      Assert.Equal(_defaultFolder, model.Folder.Folder);
      Assert.Null(model.FolderProblem);
      Assert.Equal((_needed, 9_000_000_000), (model.NeededBytes, model.FreeBytes));
      Assert.True(model.CanGoNext);
    }

    [Theory]
    [InlineData(nameof(FolderVerdict.NotEmpty), "FolderNotEmpty")]
    [InlineData(nameof(FolderVerdict.NotWritable), "FolderNotWritable")]
    [InlineData(nameof(FolderVerdict.NotEnoughSpace), "FolderNotEnoughSpace")]
    [InlineData(nameof(FolderVerdict.OverlapsData), "FolderOverlapsData")]
    public async Task EachRefusalDisablesNextWithItsReason(string refusal, string reason) {
      FolderVerdict verdict = Enum.Parse<FolderVerdict>(refusal);
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Folder);
      string pick = Path.Join(_root, "Tools");

      switch (verdict) {
        case FolderVerdict.NotEmpty:
          _folders.Add(Path.Join(pick, "Navpaky", "keep-me.txt"));
          break;
        case FolderVerdict.NotWritable:
          _folders.ReadOnly(pick);
          break;
        case FolderVerdict.NotEnoughSpace:
          _folders.Free = 1_000;
          break;
        default:
          pick = _data.Local;
          break;
      }

      model.Pick(pick);

      Assert.Equal(verdict, model.Folder.Verdict);
      Assert.Equal(reason, model.FolderProblem);
      Assert.False(model.CanGoNext);
    }

    [Fact]
    public async Task AnExistingInstallLocksTheFolder() {
      string existing = Path.Join(_root, "navpaky-app");
      _folders.Install(existing);
      WizardViewModel model = Wizard(existing: existing);

      await ToPage(model, WizardPage.Folder);

      Assert.Equal(existing, model.Folder.Folder);
      Assert.True(model.Folder.Locked);
    }

    // Task 4.2 of add-machine-wide-install.
    [Fact]
    public async Task JustForMeIsTheDefault() {
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Folder);

      Assert.False(model.ForAllUsers);
      Assert.False(model.ModeLocked);
      Assert.Equal(_defaultFolder, model.Folder.Folder);
    }

    [Fact]
    public async Task ForAllUsersUsesTheLockedProgramFilesFolder() {
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Folder);

      model.ForAllUsers = true;

      Assert.Equal(_machineFolder, model.Folder.Folder);
      Assert.True(model.Folder.Locked);
      Assert.True(model.CanGoNext);
    }

    [Fact]
    public async Task BackToJustMeRestoresThePickedFolder() {
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Folder);
      model.Pick(Path.Join(_root, "Tools"));

      model.ForAllUsers = true;
      model.ForAllUsers = false;

      Assert.Equal(Path.Join(_root, "Tools", "Navpaky"), model.Folder.Folder);
      Assert.False(model.Folder.Locked);
    }

    [Fact]
    public async Task APerUserInstallKeepsItsMode() {
      string existing = Path.Join(_root, "navpaky-app");
      _folders.Install(existing);
      WizardViewModel model = Wizard(existing: existing);
      await ToPage(model, WizardPage.Folder);

      model.ForAllUsers = true;

      Assert.False(model.ForAllUsers);
      Assert.True(model.ModeLocked);
      Assert.Equal(existing, model.Folder.Folder);
    }

    [Fact]
    public async Task AnAllUsersInstallKeepsItsMode() {
      _folders.Install(_machineFolder);
      WizardViewModel model = Wizard(existing: _machineFolder, existingForAllUsers: true);
      await ToPage(model, WizardPage.Folder);

      model.ForAllUsers = false;

      Assert.True(model.ForAllUsers);
      Assert.True(model.ModeLocked);
      Assert.Equal(_machineFolder, model.Folder.Folder);
    }

    [Fact]
    public async Task TheInstallRunsInTheChosenMode() {
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Options);
      model.Back();
      model.ForAllUsers = true;
      await ToPage(model, WizardPage.Options);

      await model.Next();

      Assert.Equal([true], _ranForAllUsers);
      Assert.Equal(_machineFolder, _ran[0].InstallFolder);
    }

    // 1223: the UAC prompt was declined or timed out.
    [Fact]
    public async Task ADeclinedPromptFailsAndOffersRetry() {
      _fails = new System.ComponentModel.Win32Exception(1223);
      WizardViewModel model = Wizard();
      model.ForAllUsers = true;
      await ToPage(model, WizardPage.Options);

      await model.Next();

      Assert.True(model.Failed);
      Assert.True(model.CanRetry);
    }

    // Task 2.1 of warn-reinstall-extra-files: the setup would delete the listed entries.
    [Fact]
    public async Task ExtraEntriesHoldNextUntilConfirmed() {
      string existing = Path.Join(_root, "navpaky-app");
      _folders.Install(existing);
      _folders.Add(Path.Join(existing, "notes.txt"));
      WizardViewModel model = Wizard(existing: existing);
      await ToPage(model, WizardPage.Folder);

      Assert.Equal(["notes.txt"], model.Folder.Extras);
      Assert.False(model.CanGoNext);

      model.ExtrasConfirmed = true;

      Assert.True(model.CanGoNext);
    }

    [Fact]
    public async Task PickingAnotherFolderAsksAgain() {
      string first = Path.Join(_root, "First", "Navpaky");
      string second = Path.Join(_root, "Second", "Navpaky");
      foreach (string folder in new[] { first, second }) {
        _folders.Install(folder);
        _folders.Add(Path.Join(folder, "notes.txt"));
      }

      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Folder);
      model.Pick(first);
      model.ExtrasConfirmed = true;

      model.Pick(second);

      Assert.False(model.ExtrasConfirmed);
      Assert.False(model.CanGoNext);
    }

    [Fact]
    public async Task WithoutExtrasNextFollowsTheVerdictAlone() {
      string existing = Path.Join(_root, "navpaky-app");
      _folders.Install(existing);
      WizardViewModel model = Wizard(existing: existing);

      await ToPage(model, WizardPage.Folder);

      Assert.Empty(model.Folder.Extras);
      Assert.False(model.ExtrasConfirmed);
      Assert.True(model.CanGoNext);
    }

    [Fact]
    public async Task OptionsDefaultToSignInOnAndShortcutOff() {
      WizardViewModel model = Wizard();

      await ToPage(model, WizardPage.Options);

      Assert.True(model.LaunchAtSignIn);
      Assert.False(model.DesktopShortcut);
      Assert.Null(model.SignInBefore);
      Assert.True(model.IsInstallStep);
    }

    [Fact]
    public async Task PreviousSettingsPresetTheOptionsAndShowWhatTheyWere() {
      var settings = new Settings {
        LaunchAtSignIn = false,
        DesktopShortcut = true,
        UiLanguage = UiLanguage.English,
      };
      WizardViewModel model = Wizard(Kept(settings));

      await ToPage(model, WizardPage.Options);

      Assert.Equal(UiLanguage.English, model.Language);
      Assert.Equal((false, true), (model.LaunchAtSignIn, model.DesktopShortcut));
      Assert.Equal((false, true), (model.SignInBefore, model.ShortcutBefore));
    }

    [Fact]
    public async Task InstallRunsSetupWithTheChoicesAndReachesDone() {
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Options);
      model.DesktopShortcut = true;

      await model.Next();

      Assert.Equal(WizardPage.Done, model.Page);
      InstallChoices choices = Assert.Single(_ran);
      Assert.Equal(
          (UiLanguage.System, true, true, KeptData.New, _defaultFolder),
          (choices.UiLanguage, choices.LaunchAtSignIn, choices.DesktopShortcut, choices.Data,
              choices.InstallFolder));
      Assert.True(model.StartNow);
      Assert.False(model.CanGoBack);
    }

    [Fact]
    public async Task AFailureOffersRetryAndCloseAndKeepsTheLog() {
      _outcome = SetupOutcome.Failed;
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Options);

      await model.Next();

      Assert.Equal(WizardPage.Installing, model.Page);
      Assert.True(model.Failed);
      Assert.True(model.CanRetry);
      Assert.False(model.CanGoBack);
      Assert.NotNull(model.LogPath);
      _outcome = SetupOutcome.Succeeded;
      await model.Retry();
      Assert.Equal(WizardPage.Done, model.Page);
      Assert.NotEqual(_ran[0].Attempt, _ran[1].Attempt);
    }

    [Fact]
    public async Task ASetupThatMayStillRunOffersOnlyClose() {
      _outcome = SetupOutcome.MayStillRun;
      WizardViewModel model = Wizard();
      await ToPage(model, WizardPage.Options);

      await model.Next();

      Assert.True(model.Failed);
      Assert.True(model.MayStillRun);
      Assert.False(model.CanRetry);
    }

    [Fact]
    public async Task AFailedFreshInstallPutsTheKeptDataBack() {
      KeepSettingsFile();
      _outcome = SetupOutcome.Failed;
      WizardViewModel model = Wizard(Kept(new Settings()), hook: MoveAside);
      await ToPage(model, WizardPage.WelcomeBack);
      model.StartsFresh = true;
      await model.Next();
      model.ConfirmFresh();

      await ToPage(model, WizardPage.Installing);

      string settings = await File.ReadAllTextAsync(
          Path.Join(_data.Roaming, "settings.json"), TestContext.Current.CancellationToken);
      Assert.Equal(_settingsJson, settings);
      Assert.False(File.Exists(Path.Join(_data.Local, InstallChoices.FileName)));
    }

    [Fact]
    public async Task ASuccessfulFreshInstallDiscardsTheBackup() {
      KeepSettingsFile();
      WizardViewModel model = Wizard(Kept(new Settings()), hook: MoveAside);
      await ToPage(model, WizardPage.WelcomeBack);
      model.StartsFresh = true;
      await model.Next();
      model.ConfirmFresh();

      await ToPage(model, WizardPage.Done);

      Assert.False(Directory.Exists(_data.Roaming + FreshBackups.Suffix(_ran[0].Attempt)));
    }

    [Fact]
    public async Task ABackupThatCannotBeDeletedKeepsTheAcknowledgement() {
      KeepSettingsFile();
      FileStream? locked = null;
      WizardViewModel model = Wizard(Kept(new Settings()), hook: choices => {
        MoveAside(choices);
        File.WriteAllText(Path.Join(_data.Local, choices.AcknowledgementName), string.Empty);
        locked = File.Open(
            Path.Join(_data.Roaming + FreshBackups.Suffix(choices.Attempt), "settings.json"),
            FileMode.Open,
            FileAccess.Read,
            FileShare.None);
      });
      await ToPage(model, WizardPage.WelcomeBack);
      model.StartsFresh = true;
      await model.Next();
      model.ConfirmFresh();

      try {
        await ToPage(model, WizardPage.Done);
      } finally {
        if (locked is not null) {
          await locked.DisposeAsync();
        }
      }

      Assert.True(File.Exists(Path.Join(_data.Local, _ran[0].AcknowledgementName)));
      Assert.False(File.Exists(Path.Join(_data.Local, InstallChoices.FileName)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FinishStartsNavpakyOnlyWhenAsked(bool startNow) {
      WizardViewModel model = Wizard();
      bool finished = false;
      model.Finished += (_, _) => finished = true;
      await ToPage(model, WizardPage.Done);
      model.StartNow = startNow;

      model.Finish();

      Assert.True(finished);
      Assert.Equal(
          startNow ? [Path.Join(_defaultFolder, "Navpaky.exe")] : [], _started);
    }

    private static KeptItems Kept(Settings settings) {
      return new KeptItems(settings, LearnedWords: 3, Statistics: true);
    }

    private static async Task ToPage(WizardViewModel model, WizardPage page) {
      while (model.Page != page) {
        if (model.Page == WizardPage.Licence) {
          model.LicenceAccepted = true;
        }

        await model.Next();
      }
    }

    private void KeepSettingsFile() {
      Directory.CreateDirectory(_data.Roaming);
      File.WriteAllText(Path.Join(_data.Roaming, "settings.json"), _settingsJson);
    }

    // What the install hook does for Start fresh: the kept data moves aside.
    private void MoveAside(InstallChoices choices) {
      if (choices.Data == KeptData.Fresh && Directory.Exists(_data.Roaming)) {
        Directory.Move(_data.Roaming, _data.Roaming + FreshBackups.Suffix(choices.Attempt));
      }
    }

    private WizardViewModel Wizard(
        KeptItems? kept = null,
        string? existing = null,
        Action<InstallChoices>? hook = null,
        bool existingForAllUsers = false) {
      var folders = new WizardFolders(
          existing is null ? null : new ExistingInstall(existing, existingForAllUsers),
          _defaultFolder,
          _data,
          _needed,
          _folders,
          _machineFolder);
      return new WizardViewModel(
          kept,
          folders,
          (choices, forAllUsers, log, progress) => {
            if (_fails is not null) {
              throw _fails;
            }

            choices.WriteTo(_data.Local);
            _ran.Add(choices);
            _ranForAllUsers.Add(forAllUsers);
            hook?.Invoke(choices);
            return Task.FromResult(_outcome);
          },
          _started.Add);
    }
  }
}
