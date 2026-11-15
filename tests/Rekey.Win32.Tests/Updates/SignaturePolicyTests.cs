using Rekey.Win32.Updates;
using Xunit;

namespace Rekey.Win32.Tests.Updates {
  public sealed class SignaturePolicyTests {
    private const string _subject = "CN=Invented Publisher, O=Invented Publisher, C=UA";
    private const string _issuer = "1111111111111111111111111111111111111111";

    private static readonly PublisherPin _pin = new(_subject, [_issuer, new string('2', 40)]);

    [Fact]
    public void TrustedFileFromThePinnedPublisherIsValid() {
      Assert.Equal(SignatureVerdict.Valid, SignaturePolicy.Judge(Trusted(_subject, _issuer), _pin));
    }

    [Fact]
    public void IssuerThumbprintIsComparedWithoutCase() {
      var facts = Trusted(_subject, "abcdefabcdefabcdefabcdefabcdefabcdefabcd");
      var pin = new PublisherPin(_subject, ["ABCDEFABCDEFABCDEFABCDEFABCDEFABCDEFABCD"]);

      Assert.Equal(SignatureVerdict.Valid, SignaturePolicy.Judge(facts, pin));
    }

    [Theory]
    [InlineData(nameof(TrustResult.NotSigned), nameof(SignatureVerdict.NotSigned))]
    [InlineData(nameof(TrustResult.Untrusted), nameof(SignatureVerdict.Untrusted))]
    [InlineData(nameof(TrustResult.Revoked), nameof(SignatureVerdict.Revoked))]
    public void FileWindowsDoesNotTrustFails(string trust, string verdict) {
      var facts = new SignatureFacts(Enum.Parse<TrustResult>(trust), _subject, _issuer);

      Assert.Equal(Enum.Parse<SignatureVerdict>(verdict), SignaturePolicy.Judge(facts, _pin));
    }

    [Fact]
    public void OtherPublisherFails() {
      SignatureFacts facts = Trusted("CN=Someone Else, O=Someone Else, C=US", _issuer);

      Assert.Equal(SignatureVerdict.WrongPublisher, SignaturePolicy.Judge(facts, _pin));
    }

    [Fact]
    public void PinnedNameFromAnotherIssuerFails() {
      SignatureFacts facts = Trusted(_subject, new string('3', 40));

      Assert.Equal(SignatureVerdict.WrongPublisher, SignaturePolicy.Judge(facts, _pin));
    }

    [Fact]
    public void EmptyPinAcceptsNothing() {
      Assert.Equal(
          SignatureVerdict.WrongPublisher,
          SignaturePolicy.Judge(Trusted(_subject, _issuer), PublisherPin.None));
    }

    private static SignatureFacts Trusted(string subject, string issuer) {
      return new SignatureFacts(TrustResult.Trusted, subject, issuer);
    }
  }
}
