using Navpaky.Core.Languages;

namespace Navpaky.Core {
  /// <summary>
  /// Asks the host to switch the foreground window to a Layout and confirm the switch.
  /// </summary>
  /// <param name="Target">The Layout to switch to.</param>
  public sealed record SwitchLayout(LayoutId Target) : EngineAction;
}
