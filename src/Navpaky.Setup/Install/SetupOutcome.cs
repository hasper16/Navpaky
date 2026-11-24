namespace Navpaky.Setup.Install {
  // How a run of Velopack's setup ended (design D3 of add-install-wizard).
  internal enum SetupOutcome {
    // Exit code 0 and the install hook's acknowledgement for this attempt.
    Succeeded,

    // Anything else, including a setup stopped after the time limit.
    Failed,

    // The setup ran out of time and could not be confirmed stopped: no Retry.
    MayStillRun,
  }
}
