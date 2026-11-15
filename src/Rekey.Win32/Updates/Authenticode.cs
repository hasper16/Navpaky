using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security.WinTrust;

namespace Rekey.Win32.Updates {
  // Reads a file's Authenticode signature the way Windows judges it, revocation included.
  [SuppressMessage(
      "Security",
      "S6640:Avoid using unsafe code blocks",
      Justification = "WinVerifyTrust takes its arguments through pointers.")]
  internal static unsafe class Authenticode {
    public static SignatureFacts Read(string path, bool checkRevocation = true) {
      TrustResult trust = Verify(path, checkRevocation);
      if (trust == TrustResult.NotSigned) {
        return new SignatureFacts(trust, string.Empty, string.Empty);
      }

      try {
#pragma warning disable SYSLIB0057 // The loader has no API for the signer of a signed file.
        using var signer = new X509Certificate2(X509Certificate.CreateFromSignedFile(path));
#pragma warning restore SYSLIB0057
        return new SignatureFacts(trust, signer.Subject, IssuerThumbprint(signer));
      } catch (CryptographicException) {
        return new SignatureFacts(TrustResult.Untrusted, string.Empty, string.Empty);
      }
    }

    // WinVerifyTrust already checked the chain and revocation; this only names the issuer.
    private static string IssuerThumbprint(X509Certificate2 signer) {
      using var chain = new X509Chain();
      chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
      chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid;
      chain.Build(signer);
      return chain.ChainElements.Count > 1
          ? chain.ChainElements[1].Certificate.Thumbprint
          : string.Empty;
    }

    private static TrustResult Verify(string path, bool checkRevocation) {
      fixed (char* file = path) {
        var fileInfo = new WINTRUST_FILE_INFO {
          cbStruct = (uint)sizeof(WINTRUST_FILE_INFO),
          pcwszFilePath = file,
        };
        var data = new WINTRUST_DATA {
          cbStruct = (uint)sizeof(WINTRUST_DATA),
          dwUIChoice = WINTRUST_DATA_UICHOICE.WTD_UI_NONE,
          fdwRevocationChecks = checkRevocation
              ? WINTRUST_DATA_REVOCATION_CHECKS.WTD_REVOKE_WHOLECHAIN
              : WINTRUST_DATA_REVOCATION_CHECKS.WTD_REVOKE_NONE,
          dwUnionChoice = WINTRUST_DATA_UNION_CHOICE.WTD_CHOICE_FILE,
          dwStateAction = WINTRUST_DATA_STATE_ACTION.WTD_STATEACTION_VERIFY,
          dwProvFlags = checkRevocation
              ? WINTRUST_DATA_PROVIDER_FLAGS.WTD_REVOCATION_CHECK_CHAIN
              : WINTRUST_DATA_PROVIDER_FLAGS.WTD_REVOCATION_CHECK_NONE,
        };
        data.Anonymous.pFile = &fileInfo;
        Guid action = PInvoke.WINTRUST_ACTION_GENERIC_VERIFY_V2;
        int result = PInvoke.WinVerifyTrust(HWND.Null, &action, &data);

        // The verify step leaves state that only a close step releases.
        data.dwStateAction = WINTRUST_DATA_STATE_ACTION.WTD_STATEACTION_CLOSE;
        _ = PInvoke.WinVerifyTrust(HWND.Null, &action, &data);
        return Classify(result);
      }
    }

    private static TrustResult Classify(int result) {
      var code = new HRESULT(result);
      if (code.Succeeded) {
        return TrustResult.Trusted;
      }

      if (code == HRESULT.TRUST_E_NOSIGNATURE || code == HRESULT.TRUST_E_SUBJECT_FORM_UNKNOWN
          || code == HRESULT.TRUST_E_PROVIDER_UNKNOWN) {
        return TrustResult.NotSigned;
      }

      return code == HRESULT.CERT_E_REVOKED ? TrustResult.Revoked : TrustResult.Untrusted;
    }
  }
}
