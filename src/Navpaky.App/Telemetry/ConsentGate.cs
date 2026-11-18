using System.Net.Http;

namespace Navpaky.App.Telemetry {
  // Sentry's HTTP goes through this gate: once the user withdraws consent, reports still
  // queued in Sentry's worker fail here, and a report already on the way is cancelled (spec
  // telemetry, "Consent withdrawn"). The UI thread closes it; Sentry's worker sends through it.
  internal sealed class ConsentGate : DelegatingHandler {
    private readonly CancellationTokenSource _closed = new();

    // No redirects, so a report reaches only the endpoint fixed in the app.
    public static ConsentGate ForNetwork() {
      return new ConsentGate {
        InnerHandler = new SocketsHttpHandler { AllowAutoRedirect = false },
      };
    }

    public void Close() {
      _closed.Cancel();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) {
      // Linking before the check leaves no gap: a close after it cancels the send.
      using var linked = CancellationTokenSource.CreateLinkedTokenSource(
          cancellationToken, _closed.Token);
      if (_closed.IsCancellationRequested) {
        throw new HttpRequestException("Crash reports were turned off.");
      }

      return await base.SendAsync(request, linked.Token);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        _closed.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
