using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Win32;
using Navpaky.App.Localization;
using Navpaky.App.Updates;
using Navpaky.App.ViewModels;
using Navpaky.Win32;
using SymbolIcon = Wpf.Ui.Controls.SymbolIcon;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace Navpaky.App.Tray {
  // The notification-area icon and its menu, redrawn from the TrayViewModel whenever something
  // it shows may have changed.
  internal sealed class TrayIconView : IDisposable {
    private const string _themeKey =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private readonly TaskbarIcon _icon;
    private readonly TrayViewModel _model;
    private readonly TrayCommands _commands;
    private (string Code, TrayState State, bool Dark, int Size)? _drawn;

    // What to do once Windows shows the latest balloon.
    private Action? _onShown;

    // What the menu was last built from; a refresh with the same content leaves it alone.
    private (bool AutoCorrection, bool Paused, string? Exclude, int Extra, UpdateStatus Updates,
        CultureInfo? Language)? _built;

    public TrayIconView(TaskbarIcon icon, TrayViewModel model, TrayCommands commands) {
      _icon = icon;
      _model = model;
      _commands = commands;

      // A left click opens the quick panel, a right click the menu (redesign-visual-identity).
      _icon.MenuActivation = PopupActivationMode.RightClick;
      _icon.TrayLeftMouseUp += (_, _) => _commands.OpenQuickPanel();
      _icon.ContextMenu = new ContextMenu();
      Localizer.BindLanguage(_icon.ContextMenu);

      // A change that came while the menu was open shows once it closes.
      _icon.ContextMenu.Closed += (_, _) => BuildMenu();

      // Windows reports a balloon it showed (NIN_BALLOONSHOW); one it dropped stays silent.
      _icon.TrayBalloonTipShown += (_, _) => {
        Action? shown = _onShown;
        _onShown = null;
        shown?.Invoke();
      };
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

    // A Windows notification from the tray icon; shown runs once Windows really showed it, as
    // in Do Not Disturb it drops the balloon, also from the notification centre (fix 2.11).
    public void Notify(string title, string message, Action? shown = null) {
      _onShown = shown;
      _icon.ShowNotification(title, message);
    }

    public void Dispose() {
      _icon.Dispose();
    }

    // The taskbar has its own light or dark setting, apart from apps.
    private static bool TaskbarIsDark() {
      using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_themeKey);
      return key?.GetValue("SystemUsesLightTheme") is int light && light == 0;
    }

    // One icon family with the Settings sections (Fluent System Icons via WPF UI), in the text
    // colour, so it reads in light and dark (owner request).
    private static SymbolIcon Icon(SymbolRegular symbol) {
      return new SymbolIcon { Symbol = symbol };
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

      MenuItem autoCorrection = Item(
          Strings.MenuAutoCorrection, _model.ToggleAutoCorrection,
          SymbolRegular.TextGrammarCheckmark24);
      autoCorrection.IsCheckable = true;
      autoCorrection.IsChecked = _model.AutoCorrectionEnabled;
      items.Add(autoCorrection);
      items.Add(_model.IsPaused
          ? Item(Strings.MenuResume, _model.Resume, SymbolRegular.Play24)
          : PauseMenu());
      if (_model.ExcludeItemText is { } exclude) {
        items.Add(Item(exclude, _model.ToggleCurrentApp, SymbolRegular.AppsList24));
      }

      items.Add(new Separator());
      items.Add(Item(Strings.MenuSettings, _commands.OpenSettings, SymbolRegular.Settings24));
      items.Add(Item(
          Strings.MenuStatistics, _commands.OpenStatistics, SymbolRegular.DataBarVertical24));
      items.Add(Item(Strings.MenuWelcome, _commands.OpenWelcome, SymbolRegular.Wand24));
      items.Add(new Separator());
      MenuItem updates = Item(
          _model.UpdateItemText, _commands.Updates, SymbolRegular.ArrowDownload24);
      updates.IsEnabled = _model.UpdateItemEnabled;
      items.Add(updates);
      items.Add(Item(Strings.MenuAbout, _commands.OpenAbout, SymbolRegular.Info24));
      items.Add(Item(Strings.MenuQuit, _commands.Quit, SymbolRegular.Power24));
    }

    private MenuItem PauseMenu() {
      var pause = new MenuItem { Header = Strings.MenuPause, Icon = Icon(SymbolRegular.Pause24) };
      (string Text, PausePreset Preset)[] presets = [
          (Strings.MenuPause15, PausePreset.FifteenMinutes),
          (Strings.MenuPause30, PausePreset.ThirtyMinutes),
          (Strings.MenuPause60, PausePreset.OneHour),
          (Strings.MenuPause180, PausePreset.ThreeHours),
          (Strings.MenuPauseUntilRestart, PausePreset.UntilRestart)];
      foreach ((string text, PausePreset preset) in presets) {
        SymbolRegular symbol = preset == PausePreset.UntilRestart
            ? SymbolRegular.ArrowRepeatAll24
            : SymbolRegular.Timer24;
        pause.Items.Add(Item(text, () => _model.Pause(preset), symbol));
      }

      return pause;
    }

    private MenuItem Item(string text, Action action, SymbolRegular symbol) {
      var item = new MenuItem { Header = text, Icon = Icon(symbol) };
      item.Click += (_, _) => {
        action();
        Refresh();
      };
      return item;
    }
  }
}
