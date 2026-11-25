using Navpaky.Core;
using Navpaky.Setup.Install;

namespace Navpaky.Setup.Wizard {
  // Runs one install attempt with the given choices (SetupRunner in the app, a stub in tests).
  internal delegate Task<SetupOutcome> RunSetup(
      InstallChoices choices, string logPath, IProgress<SetupPhase> progress);
}
