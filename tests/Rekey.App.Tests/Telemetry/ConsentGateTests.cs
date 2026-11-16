using System.Net;
using System.Net.Http;
using Rekey.App.Telemetry;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Spec telemetry "Consent withdrawn" and "Secure transport" for Sentry's own HTTP: queued
  // reports stop at the gate once consent is withdrawn, and no redirect is followed.
  public sealed class ConsentGateTests {
    private static readonly Uri _endpoint = new("https://sentry.invalid/api/1/envelope/");

    [Fact]
    public async Task OpenGatePassesTheRequestOn() {
      var inner = new CountingHandler();
      using var client = new HttpClient(new ConsentGate { InnerHandler = inner });
      using var content = new StringContent("{}");

      using HttpResponseMessage response = await client.PostAsync(
          _endpoint, content, TestContext.Current.CancellationToken);

      Assert.Equal(1, inner.Requests);
    }

    [Fact]
    public async Task ClosedGateSendsNothing() {
      var inner = new CountingHandler();
      var gate = new ConsentGate { InnerHandler = inner };
      using var client = new HttpClient(gate);
      using var content = new StringContent("{}");

      gate.Close();

      await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync(
          _endpoint, content, TestContext.Current.CancellationToken));
      Assert.Equal(0, inner.Requests);
    }

    [Fact]
    public void NetworkGateFollowsNoRedirect() {
      using ConsentGate gate = ConsentGate.ForNetwork();

      Assert.False(Assert.IsType<SocketsHttpHandler>(gate.InnerHandler).AllowAutoRedirect);
    }

    private sealed class CountingHandler : HttpMessageHandler {
      public int Requests { get; private set; }

      protected override Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, CancellationToken cancellationToken) {
        Requests++;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
      }
    }
  }
}
