using Rekey.App.Telemetry;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Spec telemetry: "Daily Usage Digest", "Consent withdrawn"; design D2 (once a day, a random
  // minute after the first hour of activity).
  public sealed class DigestSchedulerTests : IDisposable {
    private static readonly TimeSpan _delay = TimeSpan.FromMinutes(75);

    private readonly ManualClock _clock = new();
    private readonly SettingsHub _hub;
    private readonly List<DateOnly> _built = [];
    private readonly List<CancellationToken> _sends = [];
    private bool _online = true;
    private TaskCompletionSource<bool>? _hanging;

    public DigestSchedulerTests() {
      _hub = new SettingsHub(
          new Settings(DiagnosticsConsent: DiagnosticsConsent.All(true)), _ => { }, _clock);
    }

    [Fact]
    public void DigestWaitsForTheDelayAfterStart() {
      using DigestScheduler scheduler = Started();

      _clock.Advance(_delay - TimeSpan.FromMinutes(1));
      Assert.Empty(_sends);

      _clock.Advance(TimeSpan.FromMinutes(2));
      Assert.Single(_sends);
    }

    [Fact]
    public void AtMostOneDigestPerDay() {
      using DigestScheduler scheduler = Started();

      _clock.Advance(TimeSpan.FromHours(8));

      Assert.Single(_sends);
      Assert.Equal(Today(), _hub.Current.LastDigestDay);
    }

    [Fact]
    public void DigestCoversTheDayBefore() {
      using DigestScheduler scheduler = Started();

      _clock.Advance(TimeSpan.FromHours(2));

      Assert.Equal([Today().AddDays(-1)], _built);
    }

    [Fact]
    public void OfflineDigestIsNotRetriedTheSameDay() {
      _online = false;
      using DigestScheduler scheduler = Started();

      _clock.Advance(TimeSpan.FromHours(8));

      Assert.Single(_sends);
    }

    [Fact]
    public void NextDaySendsThatDaysCountersOnly() {
      _online = false;
      using DigestScheduler scheduler = Started();
      _clock.Advance(TimeSpan.FromHours(2));
      DateOnly first = Today();
      _online = true;

      _clock.Advance(TimeSpan.FromDays(1));

      Assert.Equal([first.AddDays(-1), first], _built);
    }

    [Fact]
    public void RestartTheSameDaySendsNothing() {
      _hub.Update(settings => settings with { LastDigestDay = Today() });
      using DigestScheduler scheduler = Started();

      _clock.Advance(TimeSpan.FromHours(8));

      Assert.Empty(_sends);
    }

    [Fact]
    public void NoDigestWithoutConsent() {
      _hub.Update(settings => settings with { DiagnosticsConsent = DiagnosticsConsent.All(false) });
      using DigestScheduler scheduler = Started();

      _clock.Advance(TimeSpan.FromDays(3));

      Assert.Empty(_sends);
      Assert.Empty(_built);
    }

    [Fact]
    public void WithdrawnConsentCancelsTheUnsentDigest() {
      _hanging = new TaskCompletionSource<bool>();
      using DigestScheduler scheduler = Started();
      _clock.Advance(TimeSpan.FromHours(2));

      _hub.Update(settings => settings with { DiagnosticsConsent = DiagnosticsConsent.All(false) });
      scheduler.Apply(_hub.Current);

      Assert.True(Assert.Single(_sends).IsCancellationRequested);
    }

    public void Dispose() {
      _hub.Dispose();
    }

    private DateOnly Today() {
      return DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
    }

    private DigestScheduler Started() {
      var scheduler = new DigestScheduler(_clock, action => action(), _hub, Build, Send, _delay);
      scheduler.Start();
      return scheduler;
    }

    private string Build(DateOnly day) {
      _built.Add(day);
      return "{}";
    }

    private Task<bool> Send(string json, CancellationToken token) {
      _sends.Add(token);
      return _hanging?.Task ?? Task.FromResult(_online);
    }
  }
}
