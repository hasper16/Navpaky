using System.ComponentModel;
using System.Globalization;
using Rekey.App.Onboarding;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Languages;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
  public sealed class WelcomeViewModelTests : IDisposable {
    private static readonly LayoutId _english = new(0x00000409);
    private static readonly LayoutId _russian = new(0x00000419);
    private static readonly LayoutId _russianTypewriter = new(0x00010419);

    private readonly ManualClock _clock = new();
    private readonly SettingsHub _hub;

    public WelcomeViewModelTests() {
      _hub = new SettingsHub(new Settings(), _ => { }, _clock);
    }

    public void Dispose() {
      _hub.Dispose();
    }

    [Fact]
    public void WelcomeOpensAtTheFirstStart() {
      Assert.True(WelcomeViewModel.OpensAtStart(new Settings()));
    }

    [Fact]
    public void WelcomeStaysClosedOnceCompleted() {
      Assert.False(WelcomeViewModel.OpensAtStart(new Settings(WelcomeCompleted: true)));
    }

    [Fact]
    public void FirstStartTurnsLaunchAtSignInOn() {
      Settings first = WelcomeViewModel.FirstStartDefaults(new Settings());

      Assert.True(first.LaunchAtSignIn);
    }

    [Fact]
    public void LaterStartsKeepTheUsersSignInChoice() {
      var later = new Settings(WelcomeCompleted: true, LaunchAtSignIn: false);

      Assert.Same(later, WelcomeViewModel.FirstStartDefaults(later));
    }

    // Spec telemetry: the welcome window asks, unticked by default, with one choice for both.
    [Fact]
    public void DiagnosticsStartUntickedAndOneTickAgreesToBoth() {
      using var model = new WelcomeViewModel(_hub, []);
      Assert.False(model.DiagnosticsAgreed);

      model.DiagnosticsAgreed = true;

      Assert.Equal(DiagnosticsConsent.All(true), _hub.Current.DiagnosticsConsent);
      Assert.True(model.DiagnosticsAgreed);
    }

    [Fact]
    public void ProgressTellsWhetherTheTryItWorkedAndDoneClosedTheWindow() {
      using var model = new WelcomeViewModel(_hub, []);
      Assert.Equal(new(false, false), model.Progress);

      model.NoteTryItCorrection();
      model.NoteDone();

      Assert.Equal(new(true, true), model.Progress);
    }

    [Fact]
    public void ClosingTheWindowCompletesTheWelcome() {
      using var model = new WelcomeViewModel(_hub, []);

      model.Close();

      Assert.True(_hub.Current.WelcomeCompleted);
      Assert.False(WelcomeViewModel.OpensAtStart(_hub.Current));
    }

    [Fact]
    public void ChoicesChangeTheSettings() {
      using var model = new WelcomeViewModel(_hub, []);

      model.AutoCorrectionEnabled = false;
      model.LaunchAtSignIn = true;

      Assert.False(_hub.Current.AutoCorrectionEnabled);
      Assert.True(_hub.Current.LaunchAtSignIn);
    }

    [Fact]
    public void ChoicesShowSettingsChangedElsewhere() {
      using var model = new WelcomeViewModel(_hub, []);

      _hub.Update(settings => settings with { AutoCorrectionEnabled = false });

      Assert.False(model.AutoCorrectionEnabled);
    }

    [Fact]
    public void CorrectionInTheTryItFieldConfirmsRekeyWorks() {
      using var model = new WelcomeViewModel(_hub, [_english, _russian]);
      var changed = new List<string?>();
      model.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

      model.NoteTryItCorrection();

      Assert.True(model.TryItWorked);
      Assert.Contains(nameof(WelcomeViewModel.TryItWorked), changed);
    }

    [Fact]
    public void TryItIsUnconfirmedAtFirst() {
      using var model = new WelcomeViewModel(_hub, [_english, _russian]);

      Assert.False(model.TryItWorked);
    }

    [Fact]
    public void OneLayoutNeedsAnother() {
      using var model = new WelcomeViewModel(_hub, [_english]);

      Assert.True(model.NeedsAnotherLayout);
    }

    [Fact]
    public void TwoLanguagesNeedNothingMore() {
      using var model = new WelcomeViewModel(_hub, [_english, _russian]);

      Assert.False(model.NeedsAnotherLayout);
    }

    [Fact]
    public void UnknownLayoutsNeedNothingYet() {
      using var model = new WelcomeViewModel(_hub, []);

      Assert.False(model.NeedsAnotherLayout);
    }

    [Fact]
    public void InstructionNamesTheInstalledLanguagesOnce() {
      using var model = new WelcomeViewModel(_hub, [_english, _russian, _russianTypewriter]);

      string english = CultureInfo.GetCultureInfo(0x0409).DisplayName;
      string russian = CultureInfo.GetCultureInfo(0x0419).DisplayName;
      Assert.Equal($"{english}, {russian}", model.LayoutNames);
    }

    [Fact]
    public void NewLayoutsReplaceTheOldOnes() {
      using var model = new WelcomeViewModel(_hub, [_english]);
      var changed = new List<string?>();
      model.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

      model.UseLayouts([_english, _russian]);

      Assert.False(model.NeedsAnotherLayout);
      Assert.Contains(nameof(WelcomeViewModel.NeedsAnotherLayout), changed);
      Assert.Contains(nameof(WelcomeViewModel.LayoutNames), changed);
    }
  }
}
