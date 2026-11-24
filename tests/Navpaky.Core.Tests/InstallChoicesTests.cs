using Xunit;

namespace Navpaky.Core.Tests {
  public sealed class InstallChoicesTests : IDisposable {
    private static readonly Guid _attempt = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("navpaky-choices-");

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public void RoundTripsEveryChoice() {
      var choices = new InstallChoices(
          _attempt, UiLanguage.English, LaunchAtSignIn: false, DesktopShortcut: true,
          KeptData.Fresh, @"D:\Apps\Navpaky");

      Assert.Equal(choices, InstallChoices.Parse(choices.ToJson()));
    }

    [Fact]
    public void ReadsTheVersionOneFormat() {
      const string json = """
          { "version": 1, "attempt": "6f9619ff-8b86-d011-b42d-00c04fc964ff",
            "uiLanguage": "uk", "launchAtSignIn": true, "desktopShortcut": false,
            "data": "keep", "installFolder": "C:\\Users\\olena\\AppData\\Local\\Programs\\Navpaky" }
          """;

      InstallChoices? choices = InstallChoices.Parse(json);

      Assert.Equal(
          new InstallChoices(
              _attempt, UiLanguage.Ukrainian, LaunchAtSignIn: true, DesktopShortcut: false,
              KeptData.Keep, @"C:\Users\olena\AppData\Local\Programs\Navpaky"),
          choices);
    }

    [Theory]
    [InlineData("{ broken")]
    [InlineData("null")]
    [InlineData("[]")]
    public void TextThatIsNoObjectReadsAsNoChoices(string json) {
      Assert.Null(InstallChoices.Parse(json));
    }

    [Theory]
    [InlineData("version", "2")]
    [InlineData("version", null)]
    [InlineData("attempt", null)]
    [InlineData("attempt", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("uiLanguage", "\"fr\"")]
    [InlineData("launchAtSignIn", null)]
    [InlineData("desktopShortcut", null)]
    [InlineData("data", "\"later\"")]
    [InlineData("installFolder", null)]
    [InlineData("installFolder", "\"\"")]
    public void AnIncompleteOrUnknownFileReadsAsNoChoices(string field, string? value) {
      var fields = new Dictionary<string, string> {
        ["version"] = "1",
        ["attempt"] = "\"6f9619ff-8b86-d011-b42d-00c04fc964ff\"",
        ["uiLanguage"] = "\"en\"",
        ["launchAtSignIn"] = "true",
        ["desktopShortcut"] = "false",
        ["data"] = "\"new\"",
        ["installFolder"] = "\"C:\\\\Apps\\\\Navpaky\"",
      };
      if (value is null) {
        fields.Remove(field);
      } else {
        fields[field] = value;
      }

      string json = "{" + string.Join(", ", fields.Select(f => $"\"{f.Key}\": {f.Value}")) + "}";

      Assert.Null(InstallChoices.Parse(json));
    }

    [Fact]
    public void AMissingFileReadsAsNoChoices() {
      Assert.Null(InstallChoices.ReadFrom(_root.FullName));
    }

    [Fact]
    public void WrittenChoicesAreReadBackFromTheDataFolder() {
      var choices = new InstallChoices(
          _attempt, UiLanguage.System, LaunchAtSignIn: true, DesktopShortcut: false,
          KeptData.New, @"C:\Apps\Navpaky");

      choices.WriteTo(_root.FullName);

      Assert.Equal(choices, InstallChoices.ReadFrom(_root.FullName));
      Assert.True(File.Exists(Path.Join(_root.FullName, InstallChoices.FileName)));
    }

    [Fact]
    public void TheAcknowledgementIsNamedAfterTheAttempt() {
      var choices = new InstallChoices(
          _attempt, UiLanguage.System, LaunchAtSignIn: true, DesktopShortcut: false,
          KeptData.New, @"C:\Apps\Navpaky");

      Assert.Equal("install-done-6f9619ff8b86d011b42d00c04fc964ff", choices.AcknowledgementName);
    }
  }
}
