using System.Net.Http;

namespace Rekey.App.Telemetry {
  // Sentry's HTTP goes through this gate: once the user withdraws consent, reports still
  // queued in Sentry's worker fail here instead of reaching the network (spec telemetry,
  // "Consent withdrawn"). The UI thread closes it; Sentry's worker reads it.
  internal sealed class ConsentGate : DelegatingHandler {
    private volatile bool _closed;

    // No redirects, so a report reaches only the endpoint fixed in the app.
    public static ConsentGate ForNetwork() {
      return new ConsentGate {
        InnerHandler = new SocketsHttpHandler { AllowAutoRedirect = false },
      };
    }

    public void Close() {
      _closed = true;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) {
      return _closed
          ? throw new HttpRequestException("Crash reports were turned off.")
          : base.SendAsync(request, cancellationToken);
    }
  }
}
