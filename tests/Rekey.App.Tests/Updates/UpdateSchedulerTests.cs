using Rekey.App.Updates;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class UpdateSchedulerTests {
    private readonly ManualClock _clock = new();
    private readonly FakeUpdates _updates = new();

    [Fact]
    public void AutomaticChecksStartAtOnce() {
      using UpdateScheduler scheduler = Started(automatic: true);

      Assert.Equal(1, _updates.Checks);
    }

    [Fact]
    public void AutomaticChecksRepeatEvery24Hours() {
      using UpdateScheduler scheduler = Started(automatic: true);

      _clock.Advance(TimeSpan.FromHours(23));
      Assert.Equal(1, _updates.Checks);
      _clock.Advance(TimeSpan.FromHours(1));
      Assert.Equal(2, _updates.Checks);
      _clock.Advance(TimeSpan.FromHours(24));
      Assert.Equal(3, _updates.Checks);
    }

    [Fact]
    public void NoCheckRunsWhenAutomaticChecksAreOff() {
      using UpdateScheduler scheduler = Started(automatic: false);

      _clock.Advance(TimeSpan.FromDays(3));

      Assert.Equal(0, _updates.Checks);
    }

    [Fact]
    public void CheckNowChecksAtOnceWhenAutomaticChecksAreOff() {
      using UpdateScheduler scheduler = Started(automatic: false);

      scheduler.CheckNow();

      Assert.Equal(1, _updates.Checks);
    }

    [Fact]
    public void TurningAutomaticChecksOffStopsThem() {
      using UpdateScheduler scheduler = Started(automatic: true);

      scheduler.UseAutomaticChecks(false);
      _clock.Advance(TimeSpan.FromDays(3));

      Assert.Equal(1, _updates.Checks);
    }

    [Fact]
    public void TurningAutomaticChecksOnChecksAtOnceAndEvery24Hours() {
      using UpdateScheduler scheduler = Started(automatic: false);

      scheduler.UseAutomaticChecks(true);
      _clock.Advance(TimeSpan.FromHours(24));

      Assert.Equal(2, _updates.Checks);
    }

    [Fact]
    public void RepeatingTheSameSettingDoesNotCheckAgain() {
      using UpdateScheduler scheduler = Started(automatic: true);

      scheduler.UseAutomaticChecks(true);

      Assert.Equal(1, _updates.Checks);
    }

    [Fact]
    public void CurrentVersionReportsUpToDate() {
      _updates.Result = UpdateStatus.UpToDate;
      using UpdateScheduler scheduler = Started(automatic: false);

      scheduler.CheckNow();

      Assert.Equal(UpdateStatus.UpToDate, scheduler.Status);
    }

    [Fact]
    public void DownloadedUpdateOffersRestart() {
      _updates.Result = UpdateStatus.ReadyToRestart;
      using UpdateScheduler scheduler = Started(automatic: true);

      Assert.Equal(UpdateStatus.ReadyToRestart, scheduler.Status);
    }

    [Fact]
    public void StatusShowsTheCheckWhileItRuns() {
      _updates.Pending = new TaskCompletionSource<UpdateStatus>();
      using UpdateScheduler scheduler = Started(automatic: false);
      var seen = new List<UpdateStatus>();
      scheduler.StatusChanged += () => seen.Add(scheduler.Status);

      scheduler.CheckNow();
      _updates.Pending.SetResult(UpdateStatus.UpToDate);

      Assert.Equal([UpdateStatus.Checking, UpdateStatus.UpToDate], seen);
    }

    [Fact]
    public void CheckNowDuringACheckDoesNotStartAnother() {
      _updates.Pending = new TaskCompletionSource<UpdateStatus>();
      using UpdateScheduler scheduler = Started(automatic: true);

      scheduler.CheckNow();

      Assert.Equal(1, _updates.Checks);
    }

    [Fact]
    public void ReadyUpdateIsNotCheckedAgain() {
      _updates.Result = UpdateStatus.ReadyToRestart;
      using UpdateScheduler scheduler = Started(automatic: true);

      _clock.Advance(TimeSpan.FromHours(24));
      scheduler.CheckNow();

      Assert.Equal(1, _updates.Checks);
    }

    [Fact]
    public void FailedCheckIsReported() {
      _updates.Result = UpdateStatus.Failed;
      using UpdateScheduler scheduler = Started(automatic: false);

      scheduler.CheckNow();

      Assert.Equal(UpdateStatus.Failed, scheduler.Status);
    }

    [Fact]
    public void RestartAppliesAReadyUpdate() {
      _updates.Result = UpdateStatus.ReadyToRestart;
      using UpdateScheduler scheduler = Started(automatic: true);

      scheduler.RestartToUpdate();

      Assert.Equal(1, _updates.Restarts);
    }

    [Fact]
    public void RestartWithoutAReadyUpdateDoesNothing() {
      using UpdateScheduler scheduler = Started(automatic: true);

      scheduler.RestartToUpdate();

      Assert.Equal(0, _updates.Restarts);
    }

    [Fact]
    public void UninstalledRekeyNeverChecks() {
      _updates.Available = false;
      using UpdateScheduler scheduler = Started(automatic: true);

      scheduler.CheckNow();
      _clock.Advance(TimeSpan.FromDays(2));

      Assert.Equal(0, _updates.Checks);
      Assert.Equal(UpdateStatus.Unavailable, scheduler.Status);
    }

    private UpdateScheduler Started(bool automatic) {
      var scheduler = new UpdateScheduler(_updates, _clock, action => action());
      scheduler.Start(automatic);
      return scheduler;
    }

    private sealed class FakeUpdates : IUpdateService {
      public bool Available { get; set; } = true;

      public UpdateStatus Result { get; set; } = UpdateStatus.UpToDate;

      public TaskCompletionSource<UpdateStatus>? Pending { get; set; }

      public int Checks { get; private set; }

      public int Restarts { get; private set; }

      public bool IsAvailable => Available;

      public Task<UpdateStatus> CheckAndDownloadAsync() {
        Checks++;
        return Pending?.Task ?? Task.FromResult(Result);
      }

      public void RestartToUpdate() {
        Restarts++;
      }
    }
  }
}
