namespace Navpaky.App.Feedback {
  // One step of the launch splash, from the start of the splash.
  internal sealed record SplashFrame(TimeSpan At, string Text, SplashLook Look, bool Pill);
}
