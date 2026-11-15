using CommunityToolkit.Mvvm.ComponentModel;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Languages;

namespace Rekey.App.Onboarding {
  // The welcome window (spec onboarding): the main choices read and write the hub's snapshot,
  // and closing the window in any way completes the welcome.
  internal sealed class WelcomeViewModel : ObservableObject, IDisposable {
    private readonly SettingsHub _hub;

    public WelcomeViewModel(SettingsHub hub, IReadOnlyList<LayoutId> layouts) {
      _hub = hub;
      _hub.Changed += OnHubChanged;
    }

    public static bool OpensAtStart(Settings settings) {
      return !settings.WelcomeCompleted;
    }

    // Launch at sign-in is on unless the user turns it off in the welcome window.
    public static Settings FirstStartDefaults(Settings settings) {
      return OpensAtStart(settings) ? settings with { LaunchAtSignIn = true } : settings;
    }

    public void Close() {
      _hub.Update(settings => settings with { WelcomeCompleted = true });
    }

    public void Dispose() {
      _hub.Changed -= OnHubChanged;
    }

    private void OnHubChanged(Settings settings) {
      OnPropertyChanged(string.Empty);
    }
  }
}
