using Navpaky.Core;

namespace Navpaky.App.ViewModels {
  // The app the user last typed in, with the name the tray menu shows for it.
  internal sealed record CurrentApp(AppId Id, string Name);
}
