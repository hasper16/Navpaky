using Rekey.Core.Languages;

namespace Rekey.Win32.Layouts {
  /// <summary>
  /// Keeps the installed Layouts and their handles, refreshed by polling.
  /// </summary>
  /// <remarks>
  /// Windows tells only the window that changed about a new Layout, so the worker polls this list
  /// every 2 seconds (design D5). Not thread-safe: the worker thread owns it. The Layouts follow
  /// the user's language list, the order Windows switches in; handles come in load order, which
  /// can differ (fix 2.1 of add-final-acceptance).
  /// </remarks>
  internal sealed class LayoutList {
    private readonly Func<IReadOnlyList<uint>> _readHandles;
    private readonly Func<ILayoutRegistry> _readRegistry;
    private readonly Func<IReadOnlyList<LayoutId>> _readUserOrder;
    private uint[] _handles = [];
    private Dictionary<uint, LayoutId> _byHandle = [];

    public LayoutList(
        Func<IReadOnlyList<uint>> readHandles,
        Func<ILayoutRegistry> readRegistry,
        Func<IReadOnlyList<LayoutId>> readUserOrder) {
      _readHandles = readHandles;
      _readRegistry = readRegistry;
      _readUserOrder = readUserOrder;
    }

    /// <summary>
    /// Gets the installed Layouts in the user's language-list order; Layouts the list does not
    /// name follow in handle order.
    /// </summary>
    public IReadOnlyList<LayoutId> Layouts { get; private set; } = [];

    /// <summary>
    /// Reads the installed Layouts again and returns whether they changed.
    /// </summary>
    public bool Refresh() {
      uint[] handles = [.. _readHandles()];
      if (!handles.SequenceEqual(_handles)) {
        ReadHandles(handles);
      }

      List<LayoutId> order = [.. _readUserOrder()];
      IReadOnlyList<LayoutId> layouts = [.. _byHandle.Values.Distinct()
          .OrderBy(layout => order.IndexOf(layout) is int at and >= 0 ? at : int.MaxValue)];
      bool changed = !layouts.SequenceEqual(Layouts);
      Layouts = layouts;
      return changed;
    }

    /// <summary>Returns the Layout of a handle, or <see langword="null" /> when unknown.</summary>
    public LayoutId? LayoutOf(uint handle) {
      return _byHandle.TryGetValue(handle, out LayoutId layout) ? layout : null;
    }

    /// <summary>
    /// Returns a handle of a Layout, or <see langword="null" /> when it is not installed.
    /// </summary>
    public uint? HandleOf(LayoutId layout) {
      foreach ((uint handle, LayoutId candidate) in _byHandle) {
        if (candidate == layout) {
          return handle;
        }
      }

      return null;
    }

    // The registry changes only with the handles, so it is read again only then.
    private void ReadHandles(uint[] handles) {
      var resolver = new LayoutIdResolver(_readRegistry());
      var byHandle = new Dictionary<uint, LayoutId>();
      var resolved = handles
          .Select(handle => (Handle: handle, Layout: resolver.Resolve(handle)))
          .Where(pair => pair.Layout is not null);
      foreach ((uint handle, LayoutId? layout) in resolved) {
        byHandle[handle] = layout!.Value;
      }

      _handles = handles;
      _byHandle = byHandle;
    }
  }
}
