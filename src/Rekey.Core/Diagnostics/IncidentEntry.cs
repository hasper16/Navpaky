namespace Rekey.Core.Diagnostics {
  /// <summary>
  /// Holds an Incident as the Incident log keeps it.
  /// </summary>
  /// <param name="Time">The time the Incident happened.</param>
  /// <param name="Type">One of the enumeration values that specifies the kind of Incident.</param>
  /// <param name="Details">
  /// The enum-coded facts about the Incident, each as its type and value name, such as
  /// <c>Language.Russian</c>.
  /// </param>
  public sealed record IncidentEntry(
      DateTimeOffset Time, IncidentType Type, IReadOnlyList<string> Details);
}
