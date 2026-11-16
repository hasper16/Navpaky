using Rekey.Core.Telemetry;
using Xunit;

namespace Rekey.Core.Tests.Telemetry {
  // Spec telemetry: "Show what was sent" keeps the last 10 requests exactly as sent.
  public sealed class SentLogTests : IDisposable {
    private readonly string _folder = Directory.CreateTempSubdirectory("rekey-sent-").FullName;

    [Fact]
    public void KeepsTheLastTenRequestsInOrder() {
      var log = new SentLog(_folder);

      for (int i = 1; i <= 12; i++) {
        log.Append($"{{\"n\":{i}}}");
      }

      IReadOnlyList<string> sent = log.Read();
      Assert.Equal(10, sent.Count);
      Assert.Equal("{\"n\":3}", sent[0]);
      Assert.Equal("{\"n\":12}", sent[^1]);
    }

    [Fact]
    public void MissingLogReadsEmpty() {
      Assert.Empty(new SentLog(_folder).Read());
    }

    [Fact]
    public void MultiLineRequestIsKeptOnOneLine() {
      var log = new SentLog(_folder);

      log.Append("{\n\"a\": 1\n}");

      Assert.Equal("{ \"a\": 1 }", Assert.Single(log.Read()));
    }

    public void Dispose() {
      Directory.Delete(_folder, recursive: true);
    }
  }
}
