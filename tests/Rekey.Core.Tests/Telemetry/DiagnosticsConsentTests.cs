using System.Text.Json;
using Xunit;

namespace Rekey.Core.Tests.Telemetry {
  public sealed class DiagnosticsConsentTests {
    [Fact]
    public void NewSettingsConsentToNothing() {
      DiagnosticsConsent consent = new Settings().DiagnosticsConsent;

      Assert.False(consent.CrashReports);
      Assert.False(consent.UsageDigest);
    }

    [Fact]
    public void FileWithoutConsentConsentsToNothing() {
      Settings? restored = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": true }", SettingsJsonContext.Default.Settings);

      Assert.Equal(DiagnosticsConsent.All(false), restored!.DiagnosticsConsent);
    }

    [Fact]
    public void GivenConsentRoundTripsThroughJson() {
      var settings = new Settings(
          DiagnosticsConsent: new DiagnosticsConsent(CrashReports: true, UsageDigest: false));

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.Equal(settings, restored);
    }

    [Fact]
    public void ConsentToBothSetsBoth() {
      Assert.Equal(new DiagnosticsConsent(true, true), DiagnosticsConsent.All(true));
      Assert.Equal(default, DiagnosticsConsent.All(false));
    }
  }
}
