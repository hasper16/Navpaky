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
    public const int LastStep = 2;

    private readonly SettingsHub _hub;
    private string[] _languages;
    private bool _tryItWorked;
    private bool _closedWithDone;
    private int _step;

    public WelcomeViewModel(SettingsHub hub, IReadOnlyList<LayoutId> layouts) {
      _hub = hub;
      _hub.Changed += OnHubChanged;
      _languages = LanguagesOf(layouts);
    }

    // Spec telemetry: the welcome window asks about crash reports only, off by default.
    public bool CrashReportsAgreed {
      get => _hub.Current.DiagnosticsConsent.CrashReports;
      set => _hub.Update(settings => settings with { DiagnosticsConsent = Crash(settings, value) });
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

    // The window leads through three steps: what it does, try it, your choices (design D9).
    public int Step {
      get {
        return _step;
      }

      private set {
        if (SetProperty(ref _step, value)) {
          OnPropertyChanged(nameof(IsLastStep));
          OnPropertyChanged(nameof(CanGoBack));
        }
      }
    }

    public bool IsLastStep => Step == LastStep;

    public bool CanGoBack => Step > 0;

    public static bool OpensAtStart(Settings settings) {
      return !settings.WelcomeCompleted;
    }

    // Launch at sign-in is on unless the user turns it off in the welcome window. When the
    // install wizard already asked, its answer stands (add-install-wizard).
    public static Settings FirstStartDefaults(Settings settings) {
      return OpensAtStart(settings) && !settings.InstallQuestionsAnswered
          ? settings with { LaunchAtSignIn = true }
          : settings;
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

    public void Next() {
      Step = Math.Min(Step + 1, LastStep);
    }

    public void Back() {
      Step = Math.Max(Step - 1, 0);
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

    private static DiagnosticsConsent Crash(Settings settings, bool agreed) {
      return settings.DiagnosticsConsent with { CrashReports = agreed };
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
      OnPropertyChanged(nameof(CrashReportsAgreed));
    }
  }
}
