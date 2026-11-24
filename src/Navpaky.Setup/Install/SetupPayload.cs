using System.IO;

namespace Navpaky.Setup.Install {
  // Velopack's Setup.exe, embedded at build time (design D1, D8 of add-install-wizard).
  internal static class SetupPayload {
    private const string _resource = "Navpaky.Setup.Payload.exe";

    // The package size the Folder page compares with the free space; 0 in a build without one.
    public static long Length {
      get {
        using Stream? payload = Open();
        return payload?.Length ?? 0;
      }
    }

    public static string ExtractTo(string folder) {
      using Stream payload = Open()
          ?? throw new FileNotFoundException("This build of the wizard holds no setup.");
      string exe = Path.Join(folder, "Setup.exe");
      using FileStream copy = File.Create(exe);
      payload.CopyTo(copy);
      return exe;
    }

    private static Stream? Open() {
      return typeof(SetupPayload).Assembly.GetManifestResourceStream(_resource);
    }
  }
}
