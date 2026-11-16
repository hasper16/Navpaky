using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Telemetry.Schemas;

namespace Rekey.App.Telemetry {
  // Spec telemetry "One-time setup signal": sent when the welcome window closes with consent.
  // It is marked sent before it goes out, so a failed delivery is never repeated either.
  internal sealed class SetupSignalOnce {
    private readonly SettingsHub _hub;
    private readonly Func<WelcomeProgress, string> _build;
    private readonly Func<string, Task<bool>> _send;

    public SetupSignalOnce(
        SettingsHub hub, Func<WelcomeProgress, string> build, Func<string, Task<bool>> send) {
      _hub = hub;
      _build = build;
      _send = send;
    }

    public void OnWelcomeClosed(WelcomeProgress progress) {
      Settings settings = _hub.Current;
      if (!settings.DiagnosticsConsent.UsageDigest || settings.SetupSignalSent) {
        return;
      }

      _hub.Update(current => current with { SetupSignalSent = true });
      _ = _send(_build(progress));
    }
  }
}
