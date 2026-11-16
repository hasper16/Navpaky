using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using Rekey.Core.Telemetry.Schemas;

namespace Rekey.App.Telemetry {
  // The optional "stay in touch" sign-up (spec telemetry, design D4): sent only on submit, on
  // its own request, and the address is forgotten once the server accepted it.
  internal sealed class SignUpViewModel : ObservableObject {
    private readonly Func<string, Task<bool>> _send;
    private string _email = string.Empty;
    private bool _newVersions;
    private bool _reportedProblems;
    private SignUpState _state;

    public SignUpViewModel(Func<string, Task<bool>> send) {
      _send = send;
    }

    public string Email {
      get => _email;
      set => SetProperty(ref _email, value);
    }

    public bool NewVersions {
      get => _newVersions;
      set => SetProperty(ref _newVersions, value);
    }

    public bool ReportedProblems {
      get => _reportedProblems;
      set => SetProperty(ref _reportedProblems, value);
    }

    public SignUpState State {
      get => _state;
      private set => SetProperty(ref _state, value);
    }

    public async Task SubmitAsync() {
      string email = Email.Trim();
      if (!MailAddress.TryCreate(email, out _) || !(NewVersions || ReportedProblems)) {
        State = SignUpState.Invalid;
        return;
      }

      State = SignUpState.Sending;
      bool sent = await _send(
          TelemetryJson.Write(new SignUp(email, NewVersions, ReportedProblems)));
      if (sent) {
        Email = string.Empty;
      }

      State = sent ? SignUpState.Sent : SignUpState.Failed;
    }
  }
}
