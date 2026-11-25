namespace Navpaky.App.Updates {
  // Velopack's hooks for an all-users install, which run elevated as the approving administrator
  // (design D4 and D6 of add-machine-wide-install).
  internal sealed class MachineHooks {
    // Design decision: inside the 15 s Velopack gives the obsolete hook.
    public static readonly TimeSpan CloseWait = TimeSpan.FromSeconds(10);

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
    private void CloseOthers() {
      using IDisposable raised = _raiseClose();
      if (!_others.WaitForExit(CloseWait)) {
        _others.Stop();
      }
    }
  }
}
