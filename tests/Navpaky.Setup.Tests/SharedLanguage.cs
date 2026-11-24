using Xunit;

namespace Navpaky.Setup.Tests {
  // Tests that change the process-wide interface language or make WPF objects run one at a time.
  [CollectionDefinition(nameof(SharedLanguage), DisableParallelization = true)]
  public sealed class SharedLanguage {
    private SharedLanguage() {
    }
  }
}
