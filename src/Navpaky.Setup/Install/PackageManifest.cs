using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Navpaky.Setup.Install {
  // The manifest Velopack keeps beside the installed app, current\sq.version (a nuspec).
  internal static class PackageManifest {
    public static string? VersionIn(string installFolder) {
      string manifest = Path.Join(installFolder, "current", "sq.version");
      try {
        string? version = XDocument.Load(manifest).Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "version")?.Value.Trim();
        return string.IsNullOrEmpty(version) ? null : version;
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException
          or XmlException) {
        return null;
      }
    }
  }
}
