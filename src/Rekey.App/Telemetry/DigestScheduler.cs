using Rekey.App.ViewModels;
using Rekey.Core;

namespace Rekey.App.Telemetry {
  // Sends at most one Usage Digest a day, covering the day before, once Rekey has run for the
  // given delay that day (spec telemetry, design D2). An attempt counts whether or not it was
  // delivered: a missed day is never sent later. Its state belongs to the thread `post`
  // returns to, the UI thread in the app.
  internal sealed class DigestScheduler : IDisposable {
    private static readonly TimeSpan _tick = TimeSpan.FromMinutes(1);

    private readonly TimeProvider _clock;
    private readonly Action<Action> _post;
    private readonly SettingsHub _hub;
    private readonly Func<DateOnly, string> _build;
    private readonly Func<string, CancellationToken, Task<bool>> _send;
    private readonly TimeSpan _delay;
    private readonly ITimer _timer;
    private DateTimeOffset _activeSince;
    private CancellationTokenSource? _sending;

    // build returns the digest JSON for a day; delay is an hour plus a random minute, so many
    // computers starting at nine do not all send at once.
    public DigestScheduler(
        TimeProvider clock,
        Action<Action> post,
        SettingsHub hub,
        Func<DateOnly, string> build,
        Func<string, CancellationToken, Task<bool>> send,
        TimeSpan delay) {
      _clock = clock;
      _post = post;
      _hub = hub;
      _build = build;
      _send = send;
      _delay = delay;
      _timer = clock.CreateTimer(
          _ => _post(OnTimer), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Start() {
      _activeSince = _clock.GetLocalNow();
      _timer.Change(_tick, Timeout.InfiniteTimeSpan);
    }

    // Spec telemetry "Consent withdrawn": an unsent digest is dropped, not kept for later.
    public void Apply(Settings settings) {
      if (!settings.DiagnosticsConsent.UsageDigest) {
        _sending?.Cancel();
      }
    }

    public void Dispose() {
      _timer.Dispose();
      _sending?.Cancel();
      _sending?.Dispose();
    }

    private void OnTimer() {
      _timer.Change(_tick, Timeout.InfiniteTimeSpan);
      DateTimeOffset now = _clock.GetLocalNow();
      DateOnly today = DateOnly.FromDateTime(now.DateTime);
      if (DateOnly.FromDateTime(_activeSince.DateTime) != today) {
        _activeSince = now;
      }

      Settings settings = _hub.Current;
      if (!settings.DiagnosticsConsent.UsageDigest || settings.LastDigestDay == today
          || now - _activeSince < _delay) {
        return;
      }

      _hub.Update(current => current with { LastDigestDay = today });
      _sending?.Dispose();
      _sending = new CancellationTokenSource();
      _ = _send(_build(today.AddDays(-1)), _sending.Token);
    }
  }
}
