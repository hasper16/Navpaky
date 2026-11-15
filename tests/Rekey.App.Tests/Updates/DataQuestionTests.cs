using System.Diagnostics;
using System.IO;
using System.Text;
using Rekey.App.Updates;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class DataQuestionTests {
    private const string _data = @"C:\Users\Invented\AppData\Local\Rekey";

    [Fact]
    public void RunsWindowsPowerShellFromSystem32() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data);

      Assert.Equal(
          Path.Join(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"),
          start.FileName);
      Assert.False(start.UseShellExecute);
      Assert.True(start.CreateNoWindow);
    }

    // The folder travels in the environment, so no path can change the script.
    [Fact]
    public void FolderAndTextsTravelInTheEnvironment() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data);

      Assert.Equal(_data, start.Environment["REKEY_DATA"]);
      Assert.Equal(Strings.UninstallDeleteData, start.Environment["REKEY_QUESTION"]);
      Assert.Equal(Strings.UninstallTitle, start.Environment["REKEY_TITLE"]);
      Assert.Contains("{0}", start.Environment["REKEY_NOT_DELETED"], StringComparison.Ordinal);
      Assert.DoesNotContain(start.ArgumentList, argument => argument.Contains("Invented"));
    }

    [Fact]
    public void ScriptAsksThenDeletesTheFolderFromTheEnvironment() {
      ProcessStartInfo start = DataQuestion.StartInfo(_data);
      int encoded = start.ArgumentList.IndexOf("-EncodedCommand");
      string script = Encoding.Unicode.GetString(
          Convert.FromBase64String(start.ArgumentList[encoded + 1]));

      Assert.Contains("$env:REKEY_QUESTION", script, StringComparison.Ordinal);
      Assert.Contains(
          "Remove-Item -LiteralPath $env:REKEY_DATA -Recurse", script, StringComparison.Ordinal);
    }
  }
}
