using System.Text.RegularExpressions;
using Xunit;

namespace Rekey.Core.Tests.Telemetry {
  /// <summary>
  /// Checks that Core builds no exception message from a Word, a Rendering or other typed text,
  /// because exception messages could travel in crash reports (design D1 of add-telemetry).
  /// </summary>
  public sealed partial class ExceptionMessageTests {
    [Fact]
    public void CoreBuildsNoExceptionMessageFromTypedText() {
      string core = Path.Join(RepositoryRoot(), "src", "Rekey.Core");

      string[] offences = [.. Directory.EnumerateFiles(core, "*.cs", SearchOption.AllDirectories)
          .Where(file => !file.Split(Path.DirectorySeparatorChar).Contains("obj"))
          .SelectMany(file => Offences(File.ReadAllText(file))
              .Select(offence => $"{Path.GetFileName(file)}: {offence}"))];

      Assert.Empty(offences);
    }

    [Theory]
    [InlineData("throw new InvalidOperationException($\"Cannot fix {word}.\");")]
    [InlineData("throw new ArgumentException(\"Bad \" + rendering.Value);")]
    [InlineData("throw new InvalidOperationException(_typedText);")]
    public void CheckCatchesTypedTextInMessages(string source) {
      Assert.Single(Offences(source));
    }

    [Theory]
    [InlineData("throw new ArgumentException(\"A Word needs a Layout.\", nameof(word));")]
    [InlineData("throw new ProfileLoadException($\"Format {document.Format} is unknown.\");")]
    [InlineData("throw new JsonException(e.Message, e);")]
    public void CheckAllowsConstantsAndCodes(string source) {
      Assert.Empty(Offences(source));
    }

    private static IEnumerable<string> Offences(string source) {
      foreach (Match construction in Construction().Matches(source)) {
        string arguments = Arguments(source, construction.Index + construction.Length);
        string code = Interpolated().Replace(
            Literal().Replace(NameOf().Replace(arguments, string.Empty), string.Empty),
            match => string.Concat(Holes().Matches(match.Value).Select(hole => hole.Value)));
        if (TypedText().IsMatch(code)) {
          yield return construction.Value + arguments;
        }
      }
    }

    // The text between the opening parenthesis at start - 1 and its matching close.
    private static string Arguments(string source, int start) {
      int depth = 1;
      int end = start;
      while (end < source.Length && depth > 0) {
        depth += source[end] switch { '(' => 1, ')' => -1, _ => 0 };
        end++;
      }

      return source[start..(end - 1)];
    }

    private static string RepositoryRoot() {
      DirectoryInfo? folder = new(AppContext.BaseDirectory);
      while (folder is not null && !File.Exists(Path.Join(folder.FullName, "Rekey.slnx"))) {
        folder = folder.Parent;
      }

      return folder?.FullName ?? throw new InvalidOperationException("No Rekey.slnx above.");
    }

    [GeneratedRegex(@"new\s+\w*Exception\s*\(")]
    private static partial Regex Construction();

    [GeneratedRegex(@"nameof\([^)]*\)")]
    private static partial Regex NameOf();

    [GeneratedRegex(@"(?<!\$)""(?:[^""\\]|\\.)*""")]
    private static partial Regex Literal();

    [GeneratedRegex(@"\$""(?:[^""\\]|\\.)*""")]
    private static partial Regex Interpolated();

    [GeneratedRegex(@"\{[^}]*\}")]
    private static partial Regex Holes();

    [GeneratedRegex(@"(?i)word|rendering|text|typed")]
    private static partial Regex TypedText();
  }
}
