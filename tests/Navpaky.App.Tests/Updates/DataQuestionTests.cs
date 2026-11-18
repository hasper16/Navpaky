using System.Diagnostics;
using System.IO;
using System.Text;
using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class DataQuestionTests : IDisposable {
    // Stands in for the message boxes: the question answers Yes, after optionally recording a
    // reinstall, and every other box logs its text.
    private const string _stub = """
        class FakeBox {
          static [string] Show([object]$text, [object]$title, [object]$buttons,
              [object]$icon, [object]$default) {
            if ($env:FAKE_REINSTALL) {
              [IO.File]::WriteAllText($env:NAVPAKY_MARKER, 'new installation')
            }
            return 'Yes'
          }
          static [string] Show([object]$text, [object]$title, [object]$buttons,
              [object]$icon) {
            [IO.File]::AppendAllText($env:FAKE_LOG, "$text`n")
            return 'OK'
          }
        }

        """;

    private static readonly DataFolders _invented = new(
        @"C:\Users\Invented\AppData\Roaming\Navpaky",
        @"C:\Users\Invented\AppData\Local\Navpaky");

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-question").FullName;
    private readonly DataFolders _data;
    private readonly string _log;

    public DataQuestionTests() {
      _data = new DataFolders(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
      Directory.CreateDirectory(_data.Settings);
      Directory.CreateDirectory(_data.Local);
      File.WriteAllText(Path.Join(_data.Settings, "settings.json"), "{}");
      File.WriteAllText(Path.Join(_data.Local, InstallMarker.FileName), "old installation");

      // The uninstall is still running: Velopack removes the install root only after its hook.
      string install = Path.Join(_root, "Local", "navpaky-app");
      Directory.CreateDirectory(install);
      File.WriteAllText(Path.Join(install, "Update.exe"), string.Empty);
      _log = Path.Join(_root, "boxes.txt");
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void RunsWindowsPowerShellFromSystem32() {
      ProcessStartInfo start = DataQuestion.StartInfo(_invented);

      Assert.Equal(
          Path.Join(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"),
          start.FileName);
      Assert.Equal(Environment.SystemDirectory, start.WorkingDirectory);
      Assert.False(start.UseShellExecute);
      Assert.True(start.CreateNoWindow);
    }

    // The folders travel in the environment, so no path can change the script.
    [Fact]
    public void FoldersAndTextsTravelInTheEnvironment() {
      ProcessStartInfo start = DataQuestion.StartInfo(_invented);

      Assert.Equal(_invented.Settings, start.Environment["NAVPAKY_SETTINGS"]);
      Assert.Equal(_invented.Local, start.Environment["NAVPAKY_LOCAL"]);
      Assert.Equal(
          Path.Join(_invented.Local, InstallMarker.FileName), start.Environment["NAVPAKY_MARKER"]);
      Assert.Equal(Strings.UninstallDataKept, start.Environment["NAVPAKY_KEPT"]);
      Assert.Equal(Strings.UninstallDeleteData, start.Environment["NAVPAKY_QUESTION"]);
      Assert.Equal(Strings.UninstallTitle, start.Environment["NAVPAKY_TITLE"]);
      Assert.Contains("{0}", start.Environment["NAVPAKY_NOT_DELETED"], StringComparison.Ordinal);
      Assert.DoesNotContain(start.ArgumentList, argument => argument.Contains("Invented"));
    }

    // The owner's by-hand finding on c435388: a yes while the uninstall was still running was
    // taken for a reinstall and kept the data.
    [Fact]
    public void YesDuringTheUninstallDeletesBothFolders() {
      RunQuestion(reinstall: false);

      Assert.False(Directory.Exists(_data.Settings));
      Assert.False(Directory.Exists(_data.Local));
      Assert.False(File.Exists(_log));
    }

    [Fact]
    public void YesAfterAReinstallKeepsBothFolders() {
      RunQuestion(reinstall: true);

      Assert.True(File.Exists(Path.Join(_data.Settings, "settings.json")));
      Assert.True(Directory.Exists(_data.Local));
      Assert.Equal(Strings.UninstallDataKept + "\n", File.ReadAllText(_log));
    }

    private void RunQuestion(bool reinstall) {
      ProcessStartInfo start = DataQuestion.StartInfo(_data);
      int encoded = start.ArgumentList.IndexOf("-EncodedCommand");
      string script = Encoding.Unicode.GetString(
          Convert.FromBase64String(start.ArgumentList[encoded + 1]));
      string stubbed = _stub + script.Replace(
          "[System.Windows.MessageBox]", "[FakeBox]", StringComparison.Ordinal);
      start.ArgumentList[encoded + 1] =
          Convert.ToBase64String(Encoding.Unicode.GetBytes(stubbed));
      start.Environment["FAKE_LOG"] = _log;
      start.Environment["FAKE_REINSTALL"] = reinstall ? "1" : string.Empty;

      using Process question = Process.Start(start)!;
      Assert.True(question.WaitForExit(TimeSpan.FromSeconds(60)));
    }
  }
}
