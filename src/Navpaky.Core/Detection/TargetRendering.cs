using Navpaky.Core.Languages;

namespace Navpaky.Core.Detection {
  /// <summary>
  /// Pairs a Layout a Word may be corrected to with the Word's Rendering there.
  /// </summary>
  /// <param name="Layout">The Layout.</param>
  /// <param name="Text">The Rendering of the Word in the Layout.</param>
  public readonly record struct TargetRendering(LayoutId Layout, string Text);
}
