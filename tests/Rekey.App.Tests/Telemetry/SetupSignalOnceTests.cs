using Rekey.App.Telemetry;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Telemetry.Schemas;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Spec telemetry: "One-time setup signal", "Nothing is sent without consent".
  public sealed class SetupSignalOnceTests : IDisposable {
    private static readonly WelcomeProgress _progress = new(TriedCorrection: true, true);

    private readonly ManualClock _clock = new();
    private readonly List<string> _sent = [];
    private SettingsHub _hub;

    public SetupSignalOnceTests() {
      _hub = Hub(new Settings(DiagnosticsConsent: DiagnosticsConsent.All(true)));
    }

    [Fact]
    public void ClosingTheWelcomeWithConsentSendsTheSignal() {
      SetupSignalOnce().OnWelcomeClosed(_progress);

      Assert.Equal(["signal True"], _sent);
    }

    [Fact]
    public void ReopeningAndClosingTheWelcomeSendsNothingMore() {
      SetupSignalOnce().OnWelcomeClosed(_progress);
      Settings saved = _hub.Current;
      _hub.Dispose();
      _hub = Hub(saved);

      SetupSignalOnce().OnWelcomeClosed(_progress);

      Assert.Single(_sent);
    }

    [Fact]
    public void ClosingWithoutConsentSendsNothing() {
      _hub.Update(settings => settings with { DiagnosticsConsent = DiagnosticsConsent.All(false) });

      SetupSignalOnce().OnWelcomeClosed(_progress);

      Assert.Empty(_sent);
      Assert.False(_hub.Current.SetupSignalSent);
    }

    public void Dispose() {
      _hub.Dispose();
    }

    private SettingsHub Hub(Settings settings) {
      return new SettingsHub(settings, _ => { }, _clock);
    }

    private SetupSignalOnce SetupSignalOnce() {
      return new SetupSignalOnce(
          _hub,
          progress => $"signal {progress.TriedCorrection}",
          json => {
            _sent.Add(json);
            return Task.FromResult(true);
          });
    }
  }
}
