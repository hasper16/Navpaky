using System.Reflection;
using System.Text.Json;
using Rekey.Core.Telemetry.Schemas;
using Xunit;

namespace Rekey.Core.Tests.Telemetry {
  // Task 4.2 of add-telemetry: docs/privacy.md lists every field Rekey sends, so a field added
  // to a schema record fails here until the document names it.
  public sealed class PrivacyDocumentTests {
    [Fact]
    public void PrivacyDocumentNamesEverySentField() {
      string document = File.ReadAllText(Path.Join(RepositoryRoot(), "docs", "privacy.md"));

      string[] missing = [.. typeof(UsageDigest).Assembly.GetTypes()
          .Where(type => type.Namespace == typeof(UsageDigest).Namespace && type.IsPublic)
          .SelectMany(type => type.GetProperties())
          .Where(property => property.Name != "EqualityContract")
          .Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name))
          .Distinct()
          .Where(name => !document.Contains($"`{name}`", StringComparison.Ordinal))];

      Assert.Empty(missing);
    }

    private static string RepositoryRoot() {
      DirectoryInfo? folder = new(AppContext.BaseDirectory);
      while (folder is not null && !File.Exists(Path.Join(folder.FullName, "Rekey.slnx"))) {
        folder = folder.Parent;
      }

      return folder?.FullName ?? throw new InvalidOperationException("No Rekey.slnx above.");
    }
  }
}
