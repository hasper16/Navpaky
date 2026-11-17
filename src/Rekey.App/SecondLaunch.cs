namespace Rekey.App {
  // What a second Rekey tells the user before it exits: only arguments get lost, so only then
  // (spec app-lifecycle, add-final-acceptance).
  internal static class SecondLaunch {
    public const string RestartedAfterFreeze = "--restarted-after-freeze";

    public static string? Message(IReadOnlyList<string> args) {
      return args.Count == 0 || args[0] == RestartedAfterFreeze ? null : Strings.AlreadyRunning;
    }
  }
}
