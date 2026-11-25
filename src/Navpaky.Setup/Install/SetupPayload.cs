using System.IO;

namespace Navpaky.Setup.Install {
  // Velopack's Setup.exe, embedded at build time (design D1, D8 of add-install-wizard).
  internal static class SetupPayload {
    private const string _resource = "Navpaky.Setup.Payload.exe";

    // The space the setup requires, which the Folder page shows and checks; 0 in a build
    // without a setup.
    public static long RequiredBytes() {
      using Stream? payload = Open();
      return payload is null ? 0 : SetupPackage.RequiredBytes(payload);
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
