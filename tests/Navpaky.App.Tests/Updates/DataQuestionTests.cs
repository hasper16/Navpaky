using System.Diagnostics;
using System.IO;
using System.Text;
using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class DataQuestionTests {
    private const string _install = @"C:\Users\Invented\AppData\Local\navpaky-app";

    private static readonly DataFolders _data = new(
        @"C:\Users\Invented\AppData\Roaming\Navpaky",
        @"C:\Users\Invented\AppData\Local\Navpaky");

    [Fact]
    public void RunsWindowsPowerShellFromSystem32() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);

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
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);

      Assert.Equal(_data.Settings, start.Environment["NAVPAKY_SETTINGS"]);
      Assert.Equal(_data.Local, start.Environment["NAVPAKY_LOCAL"]);
      Assert.Equal(_install, start.Environment["NAVPAKY_INSTALL"]);
      Assert.Equal(Strings.UninstallDataKept, start.Environment["NAVPAKY_KEPT"]);
      Assert.Equal(Strings.UninstallDeleteData, start.Environment["NAVPAKY_QUESTION"]);
      Assert.Equal(Strings.UninstallTitle, start.Environment["NAVPAKY_TITLE"]);
      Assert.Contains("{0}", start.Environment["NAVPAKY_NOT_DELETED"], StringComparison.Ordinal);
      Assert.DoesNotContain(start.ArgumentList, argument => argument.Contains("Invented"));
    }

    [Fact]
    public void ScriptAsksThenDeletesBothFoldersFromTheEnvironment() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);
      int encoded = start.ArgumentList.IndexOf("-EncodedCommand");
      string script = Encoding.Unicode.GetString(
          Convert.FromBase64String(start.ArgumentList[encoded + 1]));

      Assert.Contains("$env:NAVPAKY_QUESTION", script, StringComparison.Ordinal);
      Assert.Contains(
          "@($env:NAVPAKY_SETTINGS, $env:NAVPAKY_LOCAL)", script, StringComparison.Ordinal);
      Assert.Contains(
          "Remove-Item -LiteralPath $folder -Recurse", script, StringComparison.Ordinal);
    }

    // Fix 2.12 of add-final-acceptance: data goes only once the old installation is gone, so a
    // Navpaky installed again while the question was open keeps its data.
    [Fact]
    public void ScriptDeletesOnlyAfterTheInstallationIsGone() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);
      int encoded = start.ArgumentList.IndexOf("-EncodedCommand");
      string script = Encoding.Unicode.GetString(
          Convert.FromBase64String(start.ArgumentList[encoded + 1]));

      int check = script.IndexOf("$env:NAVPAKY_INSTALL", StringComparison.Ordinal);
      int delete = script.IndexOf("Remove-Item", StringComparison.Ordinal);
      Assert.InRange(check, 0, delete);
      Assert.Contains("$env:NAVPAKY_KEPT", script, StringComparison.Ordinal);
    }
  }
}
