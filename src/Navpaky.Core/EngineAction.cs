namespace Navpaky.Core {
  /// <summary>
  /// Describes one thing the engine asks the Windows side to do.
  /// </summary>
  public abstract record EngineAction {
    // Only Navpaky.Core defines actions, so adapters can rely on handling every kind.
    private protected EngineAction() { }
  }
}
