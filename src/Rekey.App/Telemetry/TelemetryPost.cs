using System.Net.Http;
using System.Text;

namespace Rekey.App.Telemetry {
  // Posts one telemetry request over HTTPS with the platform's certificate validation (spec
  // telemetry, "Secure transport") and records it as sent once the server accepted it.
  // A failure of any kind returns false; the caller's rules decide whether it is retried.
  internal sealed class TelemetryPost : IDisposable {
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    private readonly HttpClient _client;
    private readonly Action<string> _recordSent;

    public TelemetryPost(HttpMessageHandler handler, Action<string> recordSent) {
      _client = new HttpClient(handler) { Timeout = _timeout };
      _recordSent = recordSent;
    }

    // No custom certificate callback: an invalid certificate fails the request. No redirects:
    // a request reaches only the endpoint fixed in the app.
    public static HttpMessageHandler NetworkHandler() {
      return new SocketsHttpHandler { AllowAutoRedirect = false };
    }

    public async Task<bool> SendAsync(Uri? endpoint, string json, CancellationToken token) {
      if (endpoint is null || endpoint.Scheme != Uri.UriSchemeHttps) {
        return false;
      }

      try {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _client.PostAsync(endpoint, content, token);
        if (!response.IsSuccessStatusCode) {
          return false;
        }
      } catch (Exception e) when (e is HttpRequestException or OperationCanceledException) {
        return false;
      }

      _recordSent(json);
      return true;
    }

    public void Dispose() {
      _client.Dispose();
    }
  }
}
