using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace Rekey.Win32 {
  /// <summary>
  /// Tells whether Windows shows notifications to the user now.
  /// </summary>
  public static class NotificationState {
    /// <summary>
    /// Returns whether a notification would be shown now; not in Do Not Disturb, in a
    /// full-screen app or a presentation.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if Windows accepts notifications; otherwise,
    /// <see langword="false" />, also when it cannot tell.
    /// </returns>
    public static bool AcceptsNotifications() {
      return PInvoke.SHQueryUserNotificationState(out QUERY_USER_NOTIFICATION_STATE state)
          .Succeeded && state == QUERY_USER_NOTIFICATION_STATE.QUNS_ACCEPTS_NOTIFICATIONS;
    }
  }
}
