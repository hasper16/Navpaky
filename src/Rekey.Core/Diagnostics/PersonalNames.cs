namespace Rekey.Core.Diagnostics {
  /// <summary>
  /// Holds the personal names a diagnostics export replaces with placeholders.
  /// </summary>
  /// <param name="UserProfile">The user's profile folder, such as <c>C:\Users\anna</c>.</param>
  /// <param name="UserName">The Windows user name.</param>
  /// <param name="MachineName">The computer's name.</param>
  public sealed record PersonalNames(string UserProfile, string UserName, string MachineName) {
    /// <summary>
    /// Gets the names of the current user and computer.
    /// </summary>
    public static PersonalNames Current => new(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Environment.UserName,
        Environment.MachineName);
  }
}
