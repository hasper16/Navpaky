using Microsoft.Win32;

namespace Navpaky.Win32 {
  /// <summary>
  /// A registry hive and the key that stands for its <c>Software</c> key.
  /// </summary>
  /// <param name="Hive">The hive, such as <see cref="Registry.LocalMachine" />.</param>
  /// <param name="Software">
  /// The key that stands for <c>Software</c>; tests use a throwaway one.
  /// </param>
  public sealed record RegistryRoot(RegistryKey Hive, string Software);
}
