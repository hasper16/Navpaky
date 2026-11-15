namespace Rekey.App.Updates {
  // Runs update checks at start and every 24 hours while automatic checks are on, and at once
  // when the user asks (spec auto-update "Automatic update checks"). Its state belongs to the
  // thread that calls it and that `post` returns to, the UI thread in the app.
  internal sealed class UpdateScheduler : IDisposable {
    public static readonly TimeSpan Period = TimeSpan.FromHours(24);

    private readonly IUpdateService _updates;
    private readonly Action<Action> _post;
    private readonly ITimer _timer;
    private bool _automatic;

    public UpdateScheduler(IUpdateService updates, TimeProvider clock, Action<Action> post) {
      _updates = updates;
      _post = post;
      _timer = clock.CreateTimer(
          _ => _post(OnTimer), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public event Action? StatusChanged;

    public UpdateStatus Status { get; private set; }

    public void Start(bool automatic) {
      if (!_updates.IsAvailable) {
        SetStatus(UpdateStatus.Unavailable);
        return;
      }

      UseAutomaticChecks(automatic);
    }

    public void UseAutomaticChecks(bool automatic) {
      if (automatic == _automatic || Status == UpdateStatus.Unavailable) {
        return;
      }

      _automatic = automatic;
      if (automatic) {
        OnTimer();
      } else {
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
      }
    }

    public void CheckNow() {
      if (Status is UpdateStatus.Unavailable or UpdateStatus.Checking
          or UpdateStatus.ReadyToRestart) {
        return;
      }

      _ = CheckAsync();
    }

    public void RestartToUpdate() {
      if (Status == UpdateStatus.ReadyToRestart) {
        _updates.RestartToUpdate();
      }
    }

    public void Dispose() {
      _timer.Dispose();
    }

    private void OnTimer() {
      if (!_automatic) {
        return;
      }

      _timer.Change(Period, Timeout.InfiniteTimeSpan);
      CheckNow();
    }

    private async Task CheckAsync() {
      SetStatus(UpdateStatus.Checking);
      SetStatus(await _updates.CheckAndDownloadAsync());
    }

    private void SetStatus(UpdateStatus status) {
      Status = status;
      StatusChanged?.Invoke();
    }
  }
}
