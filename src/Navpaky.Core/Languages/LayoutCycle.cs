namespace Navpaky.Core.Languages {
  /// <summary>
  /// Picks the Layout that comes after the current one in the installed order.
  /// </summary>
  public static class LayoutCycle {
    /// <summary>
    /// Returns the Layout after <paramref name="current" /> in <paramref name="installed" />,
    /// wrapping round after the last.
    /// </summary>
    /// <param name="installed">The installed Layouts in the user's order.</param>
    /// <param name="current">The active Layout, or <see langword="null" /> when unknown.</param>
    /// <returns>
    /// The next Layout; the first one when <paramref name="current" /> is unknown or not
    /// installed; <see langword="null" /> when no Layout is installed.
    /// </returns>
    public static LayoutId? Next(IReadOnlyList<LayoutId> installed, LayoutId? current) {
      if (installed.Count == 0) {
        return null;
      }

      int index = current is { } layout ? installed.ToList().IndexOf(layout) : -1;
      return installed[(index + 1) % installed.Count];
    }
  }
}
