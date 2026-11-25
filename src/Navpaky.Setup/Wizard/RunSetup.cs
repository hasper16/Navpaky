using Navpaky.Core;
using Navpaky.Setup.Install;

namespace Navpaky.Setup.Wizard {
  // Runs one install attempt with the given choices, for the user or for all users (SetupRunner
  // in the app, a stub in tests).
  internal delegate Task<SetupOutcome> RunSetup(
      InstallChoices choices, bool forAllUsers, string logPath, IProgress<SetupPhase> progress);
}
