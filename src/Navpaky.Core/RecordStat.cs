using Navpaky.Core.Statistics;

namespace Navpaky.Core {
  /// <summary>
  /// Asks the host to count one event in today's usage counters; it carries no text.
  /// </summary>
  /// <param name="Counter">One of the enumeration values that specifies the counter.</param>
  /// <param name="Duration">
  /// The time from the key that asked for a Correction to the end of its replacement, for
  /// <see cref="StatCounter.AutoCorrections" /> and <see cref="StatCounter.ManualCorrections" />;
  /// otherwise, <see langword="null" />.
  /// </param>
  public sealed record RecordStat(StatCounter Counter, TimeSpan? Duration = null) : EngineAction;
}
