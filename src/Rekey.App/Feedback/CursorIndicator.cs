using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Rekey.App.Tray;
using Rekey.App.ViewModels;
using Rekey.Win32;

namespace Rekey.App.Feedback {
  // The keycap badge that shows the new Language next to the caret after Rekey switched the
  // Layout (design D5). One window, created once and reused; it never takes focus.
  internal sealed class CursorIndicator {
    public const string WindowTitle = "Rekey indicator";

    private const double _sizeAtStandardDpi = 28;
    private const int _caretTries = 5;
    private static readonly TimeSpan _shown = TimeSpan.FromMilliseconds(900);
    private static readonly Duration _fade = new(TimeSpan.FromMilliseconds(150));

    // Right after Rekey switched the Layout and typed, an edit control may have no caret for a
    // moment, while it has one at rest (owner's probe in Notepad, re-check of fix 2.6): the
    // caret is asked for again for up to 100 ms before the badge goes to the pointer.
    private static readonly TimeSpan _caretRetry = TimeSpan.FromMilliseconds(20);

    private readonly Window _window;
    private readonly Image _image = new();
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _caretTimer;
    private int _caretTriesLeft;

    // Counts the badges shown, so a fade that ends after the next badge appeared leaves it.
    private int _shows;

    public CursorIndicator() {
      _window = new Window {
        Title = WindowTitle,
        WindowStyle = WindowStyle.None,
        AllowsTransparency = true,
        Background = Brushes.Transparent,
        Topmost = true,
        ShowInTaskbar = false,
        ShowActivated = false,
        ResizeMode = ResizeMode.NoResize,
        SizeToContent = SizeToContent.WidthAndHeight,
        Focusable = false,
        IsHitTestVisible = false,
        Content = _image,

        // An own empty style keeps out WPF UI's default window style, whose MinWidth of 460
        // and template made the window a large box with the badge in its middle, far from
        // the caret (re-check of fix 2.6, add-final-acceptance).
        Style = new Style(typeof(Window)),
      };

      // The first window becomes the main window, and WPF UI's theme manager paints a Mica
      // backdrop over the main window whenever a watched window applies the theme: the badge
      // then sat in an opaque grey box (fix 2.6 of add-final-acceptance).
      if (Application.Current is { } app && app.MainWindow == _window) {
        app.MainWindow = null;
      }

      _window.SourceInitialized += (_, _) =>
          ScreenFacts.MakeOverlay(new WindowInteropHelper(_window).Handle);
      _timer = new DispatcherTimer { Interval = _shown };
      _timer.Tick += (_, _) => Hide();
      _caretTimer = new DispatcherTimer { Interval = _caretRetry };
      _caretTimer.Tick += (_, _) => Place();
    }

    // For the desktop checks and the by-hand re-check: the badge is placed, maybe after retries.
    public event Action? Placed;

    // Whether the last badge went to the caret rather than the pointer.
    public bool LastAtCaret { get; private set; }

    public bool IsShown => _window.IsVisible;

    public void Show(string languageCode, bool dark) {
      double scale = VisualTreeHelper.GetDpi(_image).DpiScaleX;
      int size = (int)Math.Round(_sizeAtStandardDpi * scale);
      _image.Source = Keycap.Render(languageCode, TrayState.Active, dark, size);
      _image.Width = size / scale;
      _image.Height = size / scale;
      _caretTriesLeft = _caretTries;
      Place();
    }

    // Shows the badge at the caret, or at the pointer once the caret stayed unknown.
    private void Place() {
      _caretTimer.Stop();
      PixelRect? caret = ScreenFacts.Caret() is { } c
          ? new PixelRect(c.Left, c.Top, c.Right, c.Bottom)
          : null;
      if (caret is null && --_caretTriesLeft > 0) {
        _caretTimer.Start();
        return;
      }

      double scale = VisualTreeHelper.GetDpi(_image).DpiScaleX;
      int size = (int)Math.Round(_sizeAtStandardDpi * scale);
      (int x, int y) = ScreenFacts.Mouse();
      (int left, int top, int right, int bottom) =
          ScreenFacts.WorkAreaAt(caret?.Left ?? x, caret?.Top ?? y);
      PixelPoint at = IndicatorPlacement.Place(
          caret, new PixelPoint(x, y), size, new PixelRect(left, top, right, bottom));
      _window.Left = at.X / scale;
      _window.Top = at.Y / scale;
      LastAtCaret = caret is not null;

      _shows++;
      _window.BeginAnimation(UIElement.OpacityProperty, null);
      _window.Opacity = 1;
      _window.Show();
      _timer.Stop();
      _timer.Start();
      Placed?.Invoke();
    }

    // "Reduce animations" in Windows turns the fade off.
    private void Hide() {
      _timer.Stop();
      if (!SystemParameters.ClientAreaAnimation) {
        _window.Hide();
        return;
      }

      int fading = _shows;
      var fade = new DoubleAnimation(0, _fade);
      fade.Completed += (_, _) => {
        if (fading == _shows) {
          _window.Hide();
        }
      };
      _window.BeginAnimation(UIElement.OpacityProperty, fade);
    }
  }
}
