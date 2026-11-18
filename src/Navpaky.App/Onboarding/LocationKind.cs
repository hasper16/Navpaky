namespace Navpaky.App.Onboarding {
  // Where the running Navpaky lives, as far as keeping it working goes.
  internal enum LocationKind {
    // Velopack's install root: updates and launch at sign-in keep working.
    Installed,

    // A temporary folder, a ZIP or Downloads: a cleanup may remove this copy.
    Temporary,

    // Anywhere else, or a dev build: no warning.
    Other,
  }
}
