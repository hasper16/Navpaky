using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Ui;
using Navpaky.Win32;

namespace Navpaky.App.Feedback {
  // The splash at start (design D10 of redesign-visual-identity): a card in the middle of the
  // screen plays SplashScript and closes. It never takes focus and lets clicks through, like the
  // cursor indicator, and is not the app's main window.
  internal sealed class LaunchSplash {
    public const string WindowTitle = "Navpaky splash";

    private static readonly Duration _fade = new(TimeSpan.FromMilliseconds(300));
    private static readonly Color _wrong = Color.FromRgb(0xE5, 0x48, 0x4D);
    private static readonly Color _corrected = Color.FromRgb(0x22, 0xA0, 0x6B);

    private readonly Window _window;
    private readonly TextBlock _word;
    private readonly FrameworkElement _pill;
    private readonly IReadOnlyList<SplashFrame> _frames;
    private readonly DispatcherTimer _timer = new();
    private readonly Stopwatch _elapsed = Stopwatch.StartNew();
    private readonly Brush _text;
    private int _next;

    private LaunchSplash(string greeting, string code, bool dark) {
      _frames = SplashScript.Frames(greeting, SystemParameters.ClientAreaAnimation);
      _text = Frozen(dark ? Color.FromRgb(0xE8, 0xEC, 0xF6) : Color.FromRgb(0x11, 0x17, 0x26));
      _word = new TextBlock {
        FontFamily = new FontFamily("Cascadia Mono, Consolas"),
        FontSize = 30,
        FontWeight = FontWeights.SemiBold,
        VerticalAlignment = VerticalAlignment.Center,
      };
      var caret = new Rectangle {
        Width = 2.5,
        Height = 32,
        Fill = Frozen(BrandTheme.Accent(dark)),
        Margin = new Thickness(6, 0, 0, 0),
        VerticalAlignment = VerticalAlignment.Center,
      };
      var line = new StackPanel {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
      };
      line.Children.Add(_word);
      line.Children.Add(caret);
      _pill = IndicatorMark.Build(CursorIndicatorStyle.Pill, code, dark);
      _pill.HorizontalAlignment = HorizontalAlignment.Right;
      _pill.VerticalAlignment = VerticalAlignment.Top;
      _pill.Visibility = Visibility.Hidden;
      var body = new Grid { Width = 420, Height = 110 };
      body.Children.Add(line);
      body.Children.Add(_pill);
      var card = new Border {
        Child = body,
        CornerRadius = new CornerRadius(14),
        BorderThickness = new Thickness(1),
        Background = Frozen(dark ? Color.FromRgb(0x13, 0x1B, 0x2F) : Colors.White),
        BorderBrush = Frozen(dark
            ? Color.FromArgb(0x30, 0x96, 0xAA, 0xE6)
            : Color.FromArgb(0x1F, 0x12, 0x28, 0x3A)),
        Margin = new Thickness(24),
        Effect = new DropShadowEffect { BlurRadius = 40, ShadowDepth = 8, Opacity = 0.3 },
      };
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
        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        Focusable = false,
        IsHitTestVisible = false,
        Content = card,

        // As for the cursor indicator: keep WPF UI's window style and Mica backdrop out.
        Style = new Style(typeof(Window)),
      };
      if (Application.Current is { } app && app.MainWindow == _window) {
        app.MainWindow = null;
      }

      _window.SourceInitialized += (_, _) =>
          ScreenFacts.MakeOverlay(new WindowInteropHelper(_window).Handle);
      _timer.Tick += (_, _) => Play();
    }

    public static void Show(string greeting, string code, bool dark) {
      var splash = new LaunchSplash(greeting, code, dark);
      splash._window.Show();
      splash.Play();
    }

    private static SolidColorBrush Frozen(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }

    // Red dashes under the word stand in for a spelling squiggle.
    private static TextDecorationCollection WrongUnderline() {
      var pen = new Pen(Frozen(_wrong), 1.5) { DashStyle = DashStyles.Dot };
      var decorations = new TextDecorationCollection {
        new TextDecoration(TextDecorationLocation.Underline, pen, 4, TextDecorationUnit.Pixel,
            TextDecorationUnit.Pixel),
      };
      decorations.Freeze();
      return decorations;
    }

    // Shows every frame that is due and waits for the next one.
    private void Play() {
      _timer.Stop();
      TimeSpan now = _elapsed.Elapsed;
      while (_next < _frames.Count && _frames[_next].At <= now) {
        Show(_frames[_next++]);
      }

      if (_next < _frames.Count) {
        _timer.Interval = _frames[_next].At - now;
        _timer.Start();
      }
    }

    private void Show(SplashFrame frame) {
      _word.Text = frame.Text;
      _pill.Visibility = frame.Pill ? Visibility.Visible : Visibility.Hidden;
      _word.Foreground = frame.Look switch {
        SplashLook.Wrong => Frozen(_wrong),
        SplashLook.Corrected => Frozen(_corrected),
        _ => _text,
      };
      _word.TextDecorations = frame.Look == SplashLook.Wrong ? WrongUnderline() : null;
      if (frame.Look == SplashLook.Gone) {
        Close();
      }
    }

    private void Close() {
      if (!SystemParameters.ClientAreaAnimation) {
        Finish();
        return;
      }

      var fade = new DoubleAnimation(0, _fade);
      fade.Completed += (_, _) => Finish();
      _window.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    private void Finish() {
      _window.Close();
    }
  }
}
