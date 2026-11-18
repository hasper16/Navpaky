namespace Navpaky.Core.Statistics {
  /// <summary>
  /// Specifies one of the daily usage counters.
  /// </summary>
  /// <remarks>
  /// The Decision breakdown counts each evaluated Word once by how it ended: corrected, deferred,
  /// kept, or kept as a Protected Word.
  /// </remarks>
  public enum StatCounter {
    /// <summary>Words a Trigger Key ended and Navpaky evaluated.</summary>
    WordsAnalysed,

    /// <summary>Auto Corrections applied.</summary>
    AutoCorrections,

    /// <summary>Manual Corrections applied, including converted selections.</summary>
    ManualCorrections,

    /// <summary>Undos applied.</summary>
    Undos,

    /// <summary>Evaluated Words whose Decision was to correct them.</summary>
    CorrectDecisions,

    /// <summary>Evaluated Words whose Decision was to defer them.</summary>
    DeferDecisions,

    /// <summary>Evaluated Words whose Decision was to keep them.</summary>
    KeepDecisions,

    /// <summary>Evaluated Words kept because they are Protected Words.</summary>
    ProtectedWordDecisions,
  }
}
