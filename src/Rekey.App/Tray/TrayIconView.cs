using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Win32;
using Rekey.App.Localization;
using Rekey.App.Updates;
using Rekey.App.ViewModels;
using Rekey.Win32;

namespace Rekey.App.Tray {
  // The notification-area icon and its menu, redrawn from the TrayViewModel whenever something
  // it shows may have changed.
  internal sealed class TrayIconView : IDisposable {
    private const string _themeKey =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private readonly TaskbarIcon _icon;
    private readonly TrayViewModel _model;
    private readonly TrayCommands _commands;
    private (string Code, TrayState State, bool Dark, int Size)? _drawn;

    // What the menu was last built from; a refresh with the same content leaves it alone.
    private (bool AutoCorrection, bool Paused, string? Exclude, int Extra, UpdateStatus Updates,
        CultureInfo? Language)? _built;

    public TrayIconView(TaskbarIcon icon, TrayViewModel model, TrayCommands commands) {
      _icon = icon;
      _model = model;
      _commands = commands;
      _icon.MenuActivation = PopupActivationMode.LeftOrRightClick;
      _icon.ContextMenu = new ContextMenu();
      Localizer.BindLanguage(_icon.ContextMenu);

      // A change that came while the menu was open shows once it closes.
      _icon.ContextMenu.Closed += (_, _) => BuildMenu();
    }

    // For the desktop checks: the debug items the app adds go first.
    public IList<MenuItem> ExtraItems { get; } = [];

    public void Refresh() {
      _icon.ToolTipText = _model.ToolTip;
      DrawIcon();
      BuildMenu();
    }

    public MenuItem? FindItem(string header) {
      return _icon.ContextMenu.Items.OfType<MenuItem>()
          .FirstOrDefault(item => Equals(item.Header, header));
    }

    // A Windows notification from the tray icon. It ignores quiet time, so in Do Not Disturb
    // Windows keeps it in the notification centre instead of dropping it (fix 2.11).
    public void Notify(string title, string message) {
      _icon.ShowNotification(title, message, respectQuietTime: false);
    }

    public void Dispose() {
      _icon.Dispose();
    }

    // The taskbar has its own light or dark setting, apart from apps.
    private static bool TaskbarIsDark() {
      using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_themeKey);
      return key?.GetValue("SystemUsesLightTheme") is int light && light == 0;
    }

    private void DrawIcon() {
      var look = (_model.LanguageCode ?? "··", _model.State, TaskbarIsDark(),
          Display.SmallIconSize());
      if (_drawn == look) {
        return;
      }

      _drawn = look;
      _icon.Icon = IconFile.From(Keycap.Render(look.Item1, look.State, look.Item3, look.Item4));
    }

    private void BuildMenu() {
      var content = (_model.AutoCorrectionEnabled, _model.IsPaused, _model.ExcludeItemText,
          ExtraItems.Count, _model.Updates, Strings.Culture);

      // The WPF build of H.NotifyIcon shows this ContextMenu itself, so rebuilding it while
      // open would drop the user's place in it.
      if (_built == content || _icon.ContextMenu.IsOpen) {
        return;
      }

      _built = content;
      ItemCollection items = _icon.ContextMenu.Items;
      items.Clear();
      foreach (MenuItem extra in ExtraItems) {
        items.Add(extra);
      }

      MenuItem autoCorrection = Item(Strings.MenuAutoCorrection, _model.ToggleAutoCorrection);
      autoCorrection.IsCheckable = true;
      autoCorrection.IsChecked = _model.AutoCorrectionEnabled;
      items.Add(autoCorrection);
      items.Add(_model.IsPaused ? Item(Strings.MenuResume, _model.Resume) : PauseMenu());
      if (_model.ExcludeItemText is { } exclude) {
        items.Add(Item(exclude, _model.ToggleCurrentApp));
      }

      items.Add(new Separator());
      items.Add(Item(Strings.MenuSettings, _commands.OpenSettings));
      items.Add(Item(Strings.MenuStatistics, _commands.OpenStatistics));
      MenuItem updates = Item(_model.UpdateItemText, _commands.Updates);
      updates.IsEnabled = _model.UpdateItemEnabled;
      items.Add(updates);
      items.Add(Item(UiText.Format(Strings.MenuWelcome, Product.Name), _commands.OpenWelcome));
      items.Add(Item(Strings.MenuAbout, _commands.OpenAbout));
      items.Add(new Separator());
      items.Add(Item(Strings.MenuQuit, _commands.Quit));
    }

    private MenuItem PauseMenu() {
      var pause = new MenuItem { Header = Strings.MenuPause };
      (string Text, PausePreset Preset)[] presets = [
          (Strings.MenuPause15, PausePreset.FifteenMinutes),
          (Strings.MenuPause30, PausePreset.ThirtyMinutes),
          (Strings.MenuPause60, PausePreset.OneHour),
          (Strings.MenuPause180, PausePreset.ThreeHours),
          (Strings.MenuPauseUntilRestart, PausePreset.UntilRestart)];
      foreach ((string text, PausePreset preset) in presets) {
        pause.Items.Add(Item(text, () => _model.Pause(preset)));
      }

      return pause;
    }

    private MenuItem Item(string text, Action action) {
      var item = new MenuItem { Header = text };
      item.Click += (_, _) => {
        action();
        Refresh();
      };
      return item;
    }
  }
}
