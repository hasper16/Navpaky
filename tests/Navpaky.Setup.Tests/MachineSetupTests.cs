using System.IO;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  // Task 5.2 of add-machine-wide-install: the elevated wizard, with fakes for the elevation, the
  // setup process and the machine registration.
  public sealed class MachineSetupTests : IDisposable {
    private static readonly Guid _attempt = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-machine").FullName;
    private readonly string _temp;
    private readonly string _machine;
    private readonly string _work;
    private readonly Launcher _launcher = new();
    private readonly List<(string Root, string Version, bool SignIn)> _registered = [];

    public MachineSetupTests() {
      _temp = Directory.CreateDirectory(Path.Join(_root, "Temp")).FullName;
      _machine = Path.Join(_root, "Program Files", "Navpaky");
      _work = Path.Join(_temp, $"navpaky-setup-{_attempt:N}");
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void ArgumentsCarryOnlyTheAttemptAndTheSignInFlag() {
      Assert.Equal(
          ["--all-users-setup", "6f9619ff8b86d011b42d00c04fc964ff", "off"],
          MachineSetup.Arguments(_attempt, signIn: false));
      string[] args = MachineSetup.Arguments(_attempt, signIn: false);

      Assert.Equal((_attempt, false), MachineSetup.Parse(args));
    }

    [Theory]
    [InlineData("--all-users-setup", "not-a-guid", "on")]
    [InlineData("--all-users-setup", "6f9619ff8b86d011b42d00c04fc964ff", "maybe")]
    [InlineData("--other", "6f9619ff8b86d011b42d00c04fc964ff", "on")]
    public void OtherArgumentsAreNotOurs(string first, string second, string third) {
      Assert.Null(MachineSetup.Parse([first, second, third]));
    }

    [Fact]
    public async Task RefusesToRunUnelevated() {
      Assert.Equal(1, await Setup(elevated: false).RunAsync(_attempt, signIn: true));
      Assert.Empty(_launcher.Runs);
    }

    [Fact]
    public async Task RefusesAWorkFolderThatAlreadyExists() {
      Directory.CreateDirectory(_work);

      Assert.Equal(1, await Setup().RunAsync(_attempt, signIn: true));
      Assert.Empty(_launcher.Runs);
    }

    [Fact]
    public async Task RunsTheSetupIntoProgramFilesThenRegisters() {
      _launcher.Install = () => Installed("0.2.0");

      Assert.Equal(0, await Setup().RunAsync(_attempt, signIn: true));

      string exe = Path.Join(_work, "Setup.exe");
      Assert.Equal(
          [(exe, $"--silent|--log|{Path.Join(_work, "setup.log")}|--installto|{_machine}")],
          _launcher.Runs.Select(run => (run.Exe, string.Join('|', run.Args))));
      Assert.Equal([(_machine, "0.2.0", true)], _registered);
      Assert.False(File.Exists(exe));
    }

    [Fact]
    public async Task AFailedSetupRegistersNothing() {
      _launcher.ExitCode = 1;

      Assert.Equal(1, await Setup().RunAsync(_attempt, signIn: true));
      Assert.Empty(_registered);
      Assert.False(File.Exists(Path.Join(_work, "Setup.exe")));
    }

    [Fact]
    public async Task ASetupThatLeftNoVersionFails() {
      Assert.Equal(1, await Setup().RunAsync(_attempt, signIn: true));
      Assert.Empty(_registered);
    }

    private MachineSetup Setup(bool elevated = true) {
      return new MachineSetup(
          elevated,
          folder => {
            string exe = Path.Join(folder, "Setup.exe");
            File.WriteAllText(exe, string.Empty);
            return exe;
          },
          _launcher,
          (root, version, signIn) => _registered.Add((root, version, signIn)),
          _machine,
          _temp);
    }

    private void Installed(string version) {
      string current = Directory.CreateDirectory(Path.Join(_machine, "current")).FullName;
      File.WriteAllText(
          Path.Join(current, "sq.version"),
          $"<package><metadata><version>{version}</version></metadata></package>");
    }

    private sealed class Launcher : ISetupLauncher {
      public List<(string Exe, IReadOnlyList<string> Args)> Runs { get; } = [];

      public int ExitCode { get; set; }

      public Action Install { get; set; } = () => { };

      public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
        Runs.Add((exe, args));
        Install();
        return new Done(ExitCode);
      }

      private sealed class Done(int exitCode) : ISetupProcess {
        public int ExitCode => exitCode;

        public Task<bool> WaitForExitAsync(TimeSpan timeout) {
          return Task.FromResult(true);
        }

        public void KillTree() {
        }

        public void Dispose() {
        }
      }
    }
  }
}
