using System.IO;
using System.IO.Compression;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  // Task 9.2 of add-install-wizard, design D5: the space the Folder page asks for is what
  // Velopack's setup checks before it installs.
  public sealed class SetupPackageTests {
    private const int _headerAt = 1000;
    private const int _zipAt = 5000;

    [Fact]
    public void TheRequirementIsThePackagesCompressedAndExtractedSizePlusTheOverhead() {
      byte[] zip = Zip(out long compressed, out long extracted);
      using var setup = new MemoryStream(Setup(zip, trailer: 300));

      Assert.Equal(
          compressed + extracted + SetupPackage.Overhead, SetupPackage.RequiredBytes(setup));
    }

    [Fact]
    public void ASetupWithoutAPackageNeedsNothingKnown() {
      using var setup = new MemoryStream(new byte[20_000]);

      Assert.Equal(0, SetupPackage.RequiredBytes(setup));
    }

    [Fact]
    public void AHeaderPointingPastTheEndIsIgnored() {
      byte[] bytes = Setup(Zip(out _, out _), trailer: 0);
      BitConverter.GetBytes((long)bytes.Length).CopyTo(bytes, _headerAt + 8);
      using var setup = new MemoryStream(bytes);

      Assert.Equal(0, SetupPackage.RequiredBytes(setup));
    }

    private static byte[] Setup(byte[] zip, int trailer) {
      var bytes = new byte[_zipAt + zip.Length + trailer];
      new Random(7).NextBytes(bytes.AsSpan(0, _zipAt));
      BitConverter.GetBytes((long)_zipAt).CopyTo(bytes, _headerAt);
      BitConverter.GetBytes((long)zip.Length).CopyTo(bytes, _headerAt + 8);
      SetupPackage.Signature.CopyTo(bytes.AsSpan(_headerAt + 16));
      zip.CopyTo(bytes, _zipAt);
      return bytes;
    }

    private static byte[] Zip(out long compressed, out long extracted) {
      using var memory = new MemoryStream();
      using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true)) {
        (string Name, int Size)[] files = [("lib/app.dll", 40_000), ("app.nuspec", 900)];
        foreach ((string name, int size) in files) {
          using Stream entry = archive.CreateEntry(name).Open();
          entry.Write(new byte[size]);
        }
      }

      memory.Position = 0;
      using (var archive = new ZipArchive(memory, ZipArchiveMode.Read, leaveOpen: true)) {
        compressed = archive.Entries.Sum(entry => entry.CompressedLength);
        extracted = archive.Entries.Sum(entry => entry.Length);
      }

      return memory.ToArray();
    }
  }
}
