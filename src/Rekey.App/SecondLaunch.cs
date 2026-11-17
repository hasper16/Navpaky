namespace Rekey.App {
  // What a second Rekey tells the user before it exits: only arguments get lost, so only then
  // (spec app-lifecycle, add-final-acceptance).
  internal static class SecondLaunch {
    public static string? Message(IReadOnlyList<string> args) {
      return args.Count == 0 ? null : Strings.AlreadyRunning;
    }
  }
}
