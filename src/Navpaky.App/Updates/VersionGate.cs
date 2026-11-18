using Velopack;

namespace Navpaky.App.Updates {
  // Spec auto-update "No downgrades": a release no newer than the running one is never
  // applied, whatever the feed lists as latest.
  internal static class VersionGate {
    // A Navpaky not installed by Velopack has no running version and takes no updates.
    public static bool Allows(SemanticVersion? running, SemanticVersion candidate) {
      return running is not null && candidate.CompareTo(running) > 0;
    }
  }
}
