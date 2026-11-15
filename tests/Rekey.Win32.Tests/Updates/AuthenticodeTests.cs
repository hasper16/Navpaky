using Rekey.Win32.Updates;
using Xunit;

namespace Rekey.Win32.Tests.Updates {
  // Runs WinVerifyTrust with revocation checks, so it needs the network as CI has it.
  public sealed class AuthenticodeTests : IDisposable {
    private static readonly string _microsoftSigned = typeof(object).Assembly.Location;

    private readonly string _folder = Directory.CreateTempSubdirectory("rekey-tests").FullName;

    public void Dispose() {
      Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void SignedFileNamesItsPublisherAndIssuer() {
      SignatureFacts facts = Authenticode.Read(_microsoftSigned);

      Assert.Equal(TrustResult.Trusted, facts.Trust);
      Assert.Contains("O=Microsoft Corporation", facts.Subject, StringComparison.Ordinal);
      Assert.Matches("^[0-9A-F]{40}$", facts.Issuer);
    }

    [Fact]
    public void UnsignedFileIsNotSigned() {
      SignatureFacts facts = Authenticode.Read(typeof(AuthenticodeTests).Assembly.Location);

      Assert.Equal(new SignatureFacts(TrustResult.NotSigned, string.Empty, string.Empty), facts);
    }

    [Fact]
    public void ChangedSignedFileIsUntrusted() {
      string copy = Path.Join(_folder, "changed.dll");
      byte[] bytes = File.ReadAllBytes(_microsoftSigned);
      bytes[bytes.Length / 2] ^= 0xFF;
      File.WriteAllBytes(copy, bytes);

      Assert.Equal(TrustResult.Untrusted, Authenticode.Read(copy).Trust);
    }
  }
}
