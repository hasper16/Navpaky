using System.Text;
using Rekey.Core.Detection;
using Rekey.Core.Languages;

namespace Rekey.Core {
  /// <summary>
  /// Asks the host to add an analysed Word to the word-analysis log, which the user turned on.
  /// </summary>
  /// <remarks>
  /// It carries typed text: only the word-analysis log may receive it, and its text form shows
  /// lengths only.
  /// </remarks>
  /// <param name="Word">The Word as typed.</param>
  /// <param name="Layout">The Layout the Word was typed in.</param>
  /// <param name="Renderings">The Word's Renderings in the other installed Layouts.</param>
  /// <param name="Decision">One of the enumeration values that specifies the Decision.</param>
  /// <param name="ProtectedWord">
  /// <see langword="true" /> if the Word was kept because it is a Protected Word; otherwise,
  /// <see langword="false" />.
  /// </param>
  public sealed record LogAnalysis(
      string Word,
      LayoutId Layout,
      IReadOnlyList<TargetRendering> Renderings,
      DecisionKind Decision,
      bool ProtectedWord) : EngineAction {
    /// <summary>
    /// Appends the lengths of the texts to the text form, so typed text cannot reach logs.
    /// </summary>
    /// <param name="builder">The builder that receives the text form.</param>
    /// <returns><see langword="true" /> because members were appended.</returns>
    protected override bool PrintMembers(StringBuilder builder) {
      builder.Append("Word.Length = ").Append(Word.Length)
          .Append(", Renderings.Count = ").Append(Renderings.Count)
          .Append(", Decision = ").Append(Decision)
          .Append(", ProtectedWord = ").Append(ProtectedWord);
      return true;
    }
  }
}
