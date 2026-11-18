using System.Text.Json;
using Navpaky.App.Telemetry;
using Xunit;

namespace Navpaky.App.Tests.Telemetry {
  // Spec telemetry: "Optional email sign-up"; design D4.
  public sealed class SignUpViewModelTests {
    private readonly List<string> _sent = [];
    private bool _delivered = true;

    [Fact]
    public async Task SubmitSendsOneRequestWithTheTickedPurpose() {
      SignUpViewModel model = NewModel();
      model.Email = " someone@example.invalid ";
      model.NewVersions = true;

      await model.SubmitAsync();

      using JsonDocument json = JsonDocument.Parse(Assert.Single(_sent));
      Assert.Equal("someone@example.invalid", json.RootElement.GetProperty("email").GetString());
      Assert.True(json.RootElement.GetProperty("newVersions").GetBoolean());
      Assert.False(json.RootElement.GetProperty("reportedProblems").GetBoolean());
    }

    [Fact]
    public async Task SuccessfulSignUpForgetsTheEmail() {
      SignUpViewModel model = NewModel();
      model.Email = "someone@example.invalid";
      model.NewVersions = true;

      await model.SubmitAsync();

      Assert.Equal(string.Empty, model.Email);
      Assert.Equal(SignUpState.Sent, model.State);
    }

    [Fact]
    public async Task FailedSignUpKeepsTheEmailForAnotherTry() {
      _delivered = false;
      SignUpViewModel model = NewModel();
      model.Email = "someone@example.invalid";
      model.ReportedProblems = true;

      await model.SubmitAsync();

      Assert.Equal("someone@example.invalid", model.Email);
      Assert.Equal(SignUpState.Failed, model.State);
    }

    [Theory]
    [InlineData("not an address", true)]
    [InlineData("someone@example.invalid", false)]
    public async Task NothingIsSentWithoutAnAddressAndAPurpose(string email, bool purpose) {
      SignUpViewModel model = NewModel();
      model.Email = email;
      model.NewVersions = purpose;

      await model.SubmitAsync();

      Assert.Empty(_sent);
      Assert.Equal(SignUpState.Invalid, model.State);
    }

    private SignUpViewModel NewModel() {
      return new SignUpViewModel(json => {
        _sent.Add(json);
        return Task.FromResult(_delivered);
      });
    }
  }
}
