namespace Rekey.App.Telemetry {
  // Where the email sign-up stands, for the message under its button.
  internal enum SignUpState {
    Idle,
    Invalid,
    Sending,
    Sent,
    Failed,
  }
}
