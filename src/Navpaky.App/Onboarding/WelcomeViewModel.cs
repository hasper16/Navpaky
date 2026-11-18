using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.App.ViewModels;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Navpaky.Core.Telemetry.Schemas;

namespace Navpaky.App.Onboarding {
  // The welcome window (spec onboarding): the main choices read and write the hub's snapshot,
  // and closing the window in any way completes the welcome.
  internal sealed class WelcomeViewModel : ObservableObject, IDisposable {
    private readonly SettingsHub _hub;
    private string[] _languages;
    private bool _tryItWorked;
    private bool _closedWithDone;

    public WelcomeViewModel(SettingsHub hub, IReadOnlyList<LayoutId> layouts) {
      _hub = hub;
      _hub.Changed += OnHubChanged;
      _languages = LanguagesOf(layouts);
    }

    public bool AutoCorrectionEnabled {
      get => _hub.Current.AutoCorrectionEnabled;
      set => _hub.Update(settings => settings with { AutoCorrectionEnabled = value });
    }

    public bool LaunchAtSignIn {
      get => _hub.Current.LaunchAtSignIn;
      set => _hub.Update(settings => settings with { LaunchAtSignIn = value });
    }

    // Spec telemetry: one checkbox, unticked by default, agrees to every kind of report.
    public bool DiagnosticsAgreed {
      get => _hub.Current.DiagnosticsConsent == DiagnosticsConsent.All(true);
      set => _hub.Update(settings => settings with { DiagnosticsConsent = Agreed(value) });
    }

    public WelcomeProgress Progress => new(TryItWorked, _closedWithDone);

    // Navpaky corrects between languages, so two Layouts of one language are not enough. No
    // Layouts at all means the worker has not read them yet.
    public bool NeedsAnotherLayout => _languages.Length == 1;

    public string LayoutNames => string.Join(", ", _languages);

    public bool TryItWorked {
      get => _tryItWorked;
      private set => SetProperty(ref _tryItWorked, value);
    }

    public static bool OpensAtStart(Settings settings) {
      return !settings.WelcomeCompleted;
    }

    // Launch at sign-in is on unless the user turns it off in the welcome window.
    public static Settings FirstStartDefaults(Settings settings) {
      return OpensAtStart(settings) ? settings with { LaunchAtSignIn = true } : settings;
    }

    public void UseLayouts(IReadOnlyList<LayoutId> layouts) {
      _languages = LanguagesOf(layouts);
      OnPropertyChanged(nameof(NeedsAnotherLayout));
      OnPropertyChanged(nameof(LayoutNames));
    }

    // A Correction Navpaky made in the try-it field proves the whole pipeline works.
    public void NoteTryItCorrection() {
      TryItWorked = true;
    }

    public void NoteDone() {
      _closedWithDone = true;
    }

    public void Close() {
      _hub.Update(settings => settings with { WelcomeCompleted = true });
    }

    public void Dispose() {
      _hub.Changed -= OnHubChanged;
    }

    private static DiagnosticsConsent Agreed(bool agreed) {
      return DiagnosticsConsent.All(agreed);
    }

    private static string[] LanguagesOf(IReadOnlyList<LayoutId> layouts) {
      return [.. layouts.Select(layout => layout.LanguageId).Distinct().Select(NameOf)];
    }

    private static string NameOf(ushort languageId) {
      try {
        return CultureInfo.GetCultureInfo(languageId).DisplayName;
      } catch (CultureNotFoundException) {
        return languageId.ToString("X4", CultureInfo.InvariantCulture);
      }
    }

    private void OnHubChanged(Settings settings) {
      OnPropertyChanged(nameof(AutoCorrectionEnabled));
      OnPropertyChanged(nameof(LaunchAtSignIn));
      OnPropertyChanged(nameof(DiagnosticsAgreed));
    }
  }
}
