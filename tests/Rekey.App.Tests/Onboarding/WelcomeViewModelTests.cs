using Rekey.App.Onboarding;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
  public sealed class WelcomeViewModelTests : IDisposable {
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

    [Fact]
    public void ClosingTheWindowCompletesTheWelcome() {
      using var model = new WelcomeViewModel(_hub, []);

      model.Close();

      Assert.True(_hub.Current.WelcomeCompleted);
      Assert.False(WelcomeViewModel.OpensAtStart(_hub.Current));
    }
  }
}
