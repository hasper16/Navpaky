namespace Navpaky.App.Feedback {
  // How the launch splash looks at one moment.
  internal enum SplashLook {
    // Typed as if in the wrong Layout: red with a wavy underline.
    Wrong,

    // Turned into the greeting: green.
    Corrected,

    // The greeting as plain text.
    Settled,

    // Faded out; the splash closes.
    Gone,
  }
}
