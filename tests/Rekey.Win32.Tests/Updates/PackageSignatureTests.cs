using System.IO.Compression;
using Rekey.Win32.Updates;
using Xunit;

namespace Rekey.Win32.Tests.Updates {
  public sealed class PackageSignatureTests : IDisposable {
    private const string _mainExe = "Rekey.App.exe";

    private static readonly string _microsoftSigned = typeof(object).Assembly.Location;
    private static readonly string _unsigned = typeof(PackageSignatureTests).Assembly.Location;

    private readonly string _folder = Directory.CreateTempSubdirectory("rekey-tests").FullName;

    public void Dispose() {
      Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void PackageWhoseCodeIsAllFromThePublisherIsValid() {
      string package = Package(
          ("lib/app/Rekey.App.exe", _microsoftSigned),
          ("lib/app/Squirrel.exe", _microsoftSigned),
          ("lib/app/Rekey.Core.xml", _unsigned));

      Assert.Equal(
          SignatureVerdict.Valid, PackageSignature.Verify(package, _mainExe, MicrosoftPin()));
    }

    [Fact]
    public void UnsignedExecutableFailsThePackage() {
      string package = Package(("lib/app/Rekey.App.exe", _unsigned));

      Assert.Equal(
          SignatureVerdict.NotSigned, PackageSignature.Verify(package, _mainExe, MicrosoftPin()));
    }

    [Fact]
    public void UnsignedLibraryBesideASignedExecutableFailsThePackage() {
      string package = Package(
          ("lib/app/Rekey.App.exe", _microsoftSigned), ("lib/app/helper.dll", _unsigned));

      Assert.Equal(
          SignatureVerdict.NotSigned, PackageSignature.Verify(package, _mainExe, MicrosoftPin()));
    }

    [Fact]
    public void OtherPublisherFailsThePackage() {
      string package = Package(("lib/app/Rekey.App.exe", _microsoftSigned));
      var other = new PublisherPin("CN=Invented Publisher", [new string('1', 40)]);

      Assert.Equal(
          SignatureVerdict.WrongPublisher, PackageSignature.Verify(package, _mainExe, other));
    }

    [Fact]
    public void PackageWithoutTheMainExecutableFails() {
      string package = Package(("lib/app/Other.exe", _microsoftSigned));

      Assert.Equal(
          SignatureVerdict.NotSigned, PackageSignature.Verify(package, _mainExe, MicrosoftPin()));
    }

    [Fact]
    public void FileThatIsNotAPackageFails() {
      string package = Path.Join(_folder, "broken.nupkg");
      File.WriteAllBytes(package, [1, 2, 3]);

      Assert.Equal(
          SignatureVerdict.NotSigned, PackageSignature.Verify(package, _mainExe, MicrosoftPin()));
    }

    // The runtime's own Microsoft signature stands in for the Rekey publisher.
    private static PublisherPin MicrosoftPin() {
      SignatureFacts facts = Authenticode.Read(_microsoftSigned);
      return new PublisherPin(facts.Subject, [facts.Issuer]);
    }

    private string Package(params (string Entry, string Source)[] files) {
      string path = Path.Join(_folder, $"{Guid.NewGuid():N}.nupkg");
      using ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create);
      foreach ((string entry, string source) in files) {
        zip.CreateEntryFromFile(source, entry);
      }

      return path;
    }
  }
}
