using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Sentry.Extensibility;
using Sentry.Protocol.Envelopes;

namespace Navpaky.App.Tests.Telemetry {
  // Stands in for the network: keeps each envelope exactly as Sentry would send it.
  internal sealed class CapturingTransport : ITransport {
    public ConcurrentQueue<string> Sent { get; } = new();

    public async Task SendEnvelopeAsync(
        Envelope envelope, CancellationToken cancellationToken = default) {
      using var stream = new MemoryStream();
      await envelope.SerializeAsync(stream, null, cancellationToken);
      Sent.Enqueue(Encoding.UTF8.GetString(stream.ToArray()));
    }
  }
}
