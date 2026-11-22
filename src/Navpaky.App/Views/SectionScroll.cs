namespace Navpaky.App.Views {
  // Which settings section the one-page settings highlight while the user scrolls (design D6).
  internal static class SectionScroll {
    // A heading this close below the top already counts as reached.
    private const double _margin = 24;

    public static int InView(IReadOnlyList<double> tops, double offset, double viewport,
        double extent) {
      if (offset + viewport >= extent - 1) {
        return tops.Count - 1;
      }

      int inView = 0;
      for (int i = 0; i < tops.Count; i++) {
        if (tops[i] <= offset + _margin) {
          inView = i;
        }
      }

      return inView;
    }
  }
}
