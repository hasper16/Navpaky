using System.Diagnostics;
using System.IO;
using System.Text;
using Rekey.App.Updates;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class DataQuestionTests {
    private const string _data = @"C:\Users\Invented\AppData\Local\Rekey";
    private const string _install = @"C:\Users\Invented\AppData\Local\key-switcher";

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

    // The folder travels in the environment, so no path can change the script.
    [Fact]
    public void FolderAndTextsTravelInTheEnvironment() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);

      Assert.Equal(_data, start.Environment["REKEY_DATA"]);
      Assert.Equal(_install, start.Environment["REKEY_INSTALL"]);
      Assert.Equal(Strings.UninstallDataKept, start.Environment["REKEY_KEPT"]);
      Assert.Equal(Strings.UninstallDeleteData, start.Environment["REKEY_QUESTION"]);
      Assert.Equal(Strings.UninstallTitle, start.Environment["REKEY_TITLE"]);
      Assert.Contains("{0}", start.Environment["REKEY_NOT_DELETED"], StringComparison.Ordinal);
      Assert.DoesNotContain(start.ArgumentList, argument => argument.Contains("Invented"));
    }

    [Fact]
    public void ScriptAsksThenDeletesTheFolderFromTheEnvironment() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);
      int encoded = start.ArgumentList.IndexOf("-EncodedCommand");
      string script = Encoding.Unicode.GetString(
          Convert.FromBase64String(start.ArgumentList[encoded + 1]));

      Assert.Contains("$env:REKEY_QUESTION", script, StringComparison.Ordinal);
      Assert.Contains(
          "Remove-Item -LiteralPath $env:REKEY_DATA -Recurse", script, StringComparison.Ordinal);
    }

    // Fix 2.12 of add-final-acceptance: data goes only once the old installation is gone, so a
    // Rekey installed again while the question was open keeps its data.
    [Fact]
    public void ScriptDeletesOnlyAfterTheInstallationIsGone() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data, _install);
      int encoded = start.ArgumentList.IndexOf("-EncodedCommand");
      string script = Encoding.Unicode.GetString(
          Convert.FromBase64String(start.ArgumentList[encoded + 1]));

      int check = script.IndexOf("$env:REKEY_INSTALL", StringComparison.Ordinal);
      int delete = script.IndexOf("Remove-Item", StringComparison.Ordinal);
      Assert.InRange(check, 0, delete);
      Assert.Contains("$env:REKEY_KEPT", script, StringComparison.Ordinal);
    }
  }
}
