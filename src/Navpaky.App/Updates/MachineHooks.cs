namespace Navpaky.App.Updates {
  // Velopack's hooks for an all-users install, which run elevated as the approving administrator
  // (design D4 and D6 of add-machine-wide-install).
  internal sealed class MachineHooks {
    // Design decision: inside the 15 s Velopack gives the obsolete hook.
    public static readonly TimeSpan CloseWait = TimeSpan.FromSeconds(10);

    // Kill returns before the process is gone; 10 s + 3 s stays inside the 15 s limit.
    public static readonly TimeSpan StopWait = TimeSpan.FromSeconds(3);

    private readonly Action<string> _refreshEntry;
    private readonly Action _unregister;
    private readonly Func<IDisposable> _raiseClose;
    private readonly IOtherSessions _others;

    public MachineHooks(
        Action<string> refreshEntry,
        Action unregister,
        Func<IDisposable> raiseClose,
        IOtherSessions others) {
      _refreshEntry = refreshEntry;
      _unregister = unregister;
      _raiseClose = raiseClose;
      _others = others;
    }

    // Velopack has just rewritten its entry in the administrator's profile; ours takes it over.
    public void AfterUpdate(string version) {
      _refreshEntry(version);
    }

    // Velopack's own stop can't reach other sessions, and the update fails while one runs.
    public void BeforeUpdate() {
      CloseOthers();
    }

    public void BeforeUninstall() {
      CloseOthers();
      _unregister();
    }

    // Each Navpaky saves its data as it closes; only the ones still running after the wait stop.
    // When the signal can't be raised (someone else holds the name), they are stopped at once.
    private void CloseOthers() {
      IDisposable? raised = TryRaise();
      using (raised) {
        if (raised is null || !_others.WaitForExit(CloseWait)) {
          _others.Stop();
          _others.WaitForExit(StopWait);
        }
      }
    }

    private IDisposable? TryRaise() {
      try {
        return _raiseClose();
      } catch (Exception e) when (e is UnauthorizedAccessException
          or WaitHandleCannotBeOpenedException or System.IO.IOException) {
        System.Diagnostics.Trace.WriteLine($"Navpaky close request not raised: {e.GetType().Name}");
        return null;
      }
    }
  }
}
