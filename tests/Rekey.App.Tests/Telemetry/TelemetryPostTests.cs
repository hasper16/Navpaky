using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Rekey.App.Telemetry;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Spec telemetry: "Secure transport", "Show what was sent".
  public sealed class TelemetryPostTests {
    private static readonly Uri _endpoint = new("https://telemetry.invalid/digest");

    private readonly List<string> _recorded = [];

    [Fact]
    public async Task DeliveredRequestIsRecordedAsSent() {
      var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
      using var post = new TelemetryPost(handler, _recorded.Add);

      bool sent = await post.SendAsync(_endpoint, "{\"a\":1}", CancellationToken.None);

      Assert.True(sent);
      Assert.Equal(["{\"a\":1}"], _recorded);
      Assert.Equal("{\"a\":1}", handler.Bodies.Single());
    }

    [Fact]
    public async Task InvalidCertificateSendsNothing() {
      var handler = new FakeHandler(_ => throw new HttpRequestException(
          "The SSL connection could not be established.", new AuthenticationException()));
      using var post = new TelemetryPost(handler, _recorded.Add);

      Assert.False(await post.SendAsync(_endpoint, "{}", CancellationToken.None));
      Assert.Empty(_recorded);
    }

    [Fact]
    public async Task ServerErrorIsNotRecorded() {
      var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.BadGateway));
      using var post = new TelemetryPost(handler, _recorded.Add);

      Assert.False(await post.SendAsync(_endpoint, "{}", CancellationToken.None));
      Assert.Empty(_recorded);
    }

    [Theory]
    [InlineData("http://telemetry.invalid/digest")]
    [InlineData(null)]
    public async Task OnlyHttpsEndpointsAreUsed(string? endpoint) {
      var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
      using var post = new TelemetryPost(handler, _recorded.Add);

      bool sent = await post.SendAsync(
          endpoint is null ? null : new Uri(endpoint), "{}", CancellationToken.None);

      Assert.False(sent);
      Assert.Empty(handler.Bodies);
    }

    [Fact]
    public async Task CancelledSendIsNotRecorded() {
      var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
      using var post = new TelemetryPost(handler, _recorded.Add);

      bool sent = await post.SendAsync(_endpoint, "{}", new CancellationToken(canceled: true));

      Assert.False(sent);
      Assert.Empty(_recorded);
    }

    [Fact]
    public void NetworkHandlerKeepsStandardCertificateValidation() {
      using var handler = (SocketsHttpHandler)TelemetryPost.NetworkHandler();

      Assert.Null(handler.SslOptions.RemoteCertificateValidationCallback);
    }

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> answer)
        : HttpMessageHandler {
      public List<string> Bodies { get; } = [];

      protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
        return answer(request);
      }
    }
  }
}
