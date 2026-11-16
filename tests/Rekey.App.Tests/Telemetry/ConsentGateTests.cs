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
    public async Task ClosingTheGateCancelsARequestInFlight() {
      var inner = new HangingHandler();
      var gate = new ConsentGate { InnerHandler = inner };
      using var client = new HttpClient(gate);
      using var content = new StringContent("{}");
      Task<HttpResponseMessage> sending = client.PostAsync(
          _endpoint, content, TestContext.Current.CancellationToken);
      await inner.Started.Task;

      gate.Close();

      // Within seconds, not the client's 100 s timeout: the gate itself cancels.
      await Assert.ThrowsAnyAsync<OperationCanceledException>(
          () => sending.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void NetworkGateFollowsNoRedirect() {
      using ConsentGate gate = ConsentGate.ForNetwork();

      Assert.False(Assert.IsType<SocketsHttpHandler>(gate.InnerHandler).AllowAutoRedirect);
    }

    // Waits on the network until its request is cancelled.
    private sealed class HangingHandler : HttpMessageHandler {
      public TaskCompletionSource Started { get; } = new();

      protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, CancellationToken cancellationToken) {
        Started.SetResult();
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK);
      }
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
