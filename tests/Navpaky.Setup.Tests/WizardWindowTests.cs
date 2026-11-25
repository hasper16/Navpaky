using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Navpaky.Core;
using Navpaky.Setup.Install;
using Navpaky.Setup.Wizard;
using Navpaky.Ui;
using Wpf.Ui.Appearance;
using Xunit;

namespace Navpaky.Setup.Tests {
  // Tasks 6.1 and 6.8 of add-install-wizard: the window shows each page with its buttons. With
  // NAVPAKY_RENDER_DIR set, it also renders every page in light and dark, en and uk, for the
  // comparison with prototype v3.1.
  [Collection(nameof(SharedLanguage))]
  public sealed class WizardWindowTests : IDisposable {
    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-window").FullName;
    private readonly DataFolderPair _data;
    private SetupOutcome _outcome = SetupOutcome.Succeeded;

    public WizardWindowTests() {
      _data = new DataFolderPair(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
    }

    public void Dispose() {
      Sta.Run(() => Localizer.Instance.Use(CultureInfo.GetCultureInfo("en")));
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void EachPageShowsItsOwnButtons() {
      Sta.Run(() => {
        (WizardWindow window, WizardViewModel model) = Open(kept: null);
        try {
          Assert.Equal(["Next"], Buttons(window));
          Walk(model, WizardPage.Licence);
          Assert.Equal(["Back", "Next"], Buttons(window));
          Assert.False(window.Next.IsEnabled);
          Walk(model, WizardPage.Options);
          Assert.Equal(["Back", "Install"], Buttons(window));
          Walk(model, WizardPage.Done);
          Assert.Equal(["Finish"], Buttons(window));
        } finally {
          window.Close();
        }
      });
    }

    [Fact]
    public void AFailedInstallShowsRetryAndClose() {
      _outcome = SetupOutcome.Failed;
      Sta.Run(() => {
        (WizardWindow window, WizardViewModel model) = Open(kept: null);
        try {
          Walk(model, WizardPage.Installing);

          Assert.Equal(["Retry", "CloseWizard"], Buttons(window));
          Assert.Equal(Visibility.Visible, window.LogText.Visibility);
        } finally {
          window.Close();
        }
      });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreviousSettingsPresetTheShownOptions(bool cancelledStartFresh) {
      var kept = new KeptItems(new Settings { DesktopShortcut = true }, null, false);
      Sta.Run(() => {
        (WizardWindow window, WizardViewModel model) = Open(kept);
        try {
          Walk(model, WizardPage.WelcomeBack);
          if (cancelledStartFresh) {
            model.StartsFresh = true;
            model.Next().GetAwaiter().GetResult();
            Sta.Flush();
            model.CancelFresh();
            Sta.Flush();
          }

          Walk(model, WizardPage.Options);

          Assert.Equal((false, true), (model.LaunchAtSignIn, model.DesktopShortcut));
        } finally {
          window.Close();
        }
      });
    }

    [Fact]
    public void RendersEveryPage() {
      string? folder = Environment.GetEnvironmentVariable("NAVPAKY_RENDER_DIR");
      Assert.SkipWhen(folder is null, "Set NAVPAKY_RENDER_DIR to render the pages.");
      Directory.CreateDirectory(folder);
      var kept = new KeptItems(
          new Settings { Hotkey = Hotkey.DoubleCtrl, DesktopShortcut = true }, 42, true);
      Sta.Run(() => {
        ApplicationTheme[] themes = [ApplicationTheme.Light, ApplicationTheme.Dark];
        foreach (ApplicationTheme theme in themes) {
          foreach (UiLanguage language in new[] { UiLanguage.English, UiLanguage.Ukrainian }) {
            RenderAll(folder, theme, language, kept);
          }
        }
      });
    }

    // Task 2.2 of warn-reinstall-extra-files: the list stays bounded.
    [Fact]
    public void TheFolderPageListsEightExtrasAndCountsTheRest() {
      string existing = Path.Join(_root, "navpaky-app");
      var probe = new FakeFolders { Free = 40_000_000_000 };
      probe.Install(existing);
      for (int extra = 1; extra <= 12; extra++) {
        probe.Add(Path.Join(existing, $"extra-{extra:D2}.txt"));
      }

      Sta.Run(() => {
        (WizardWindow window, WizardViewModel model) = Open(kept: null, probe, existing);
        try {
          model.Language = UiLanguage.English;
          Walk(model, WizardPage.Folder);
          Sta.Flush();

          Assert.Equal(Visibility.Visible, window.FolderExtras.Visibility);
          Assert.Equal(
              [.. Enumerable.Range(1, 8).Select(extra => $"extra-{extra:D2}.txt"), "and 4 more"],
              window.FolderExtrasList.Children.OfType<TextBlock>().Select(line => line.Text));
          Assert.False(window.Next.IsEnabled);

          window.FolderExtrasConfirm.IsChecked = true;
          Sta.Flush();

          Assert.True(window.Next.IsEnabled);
        } finally {
          window.Close();
        }
      });
    }

    [Fact]
    public void AFolderWithoutExtrasShowsNoWarning() {
      Sta.Run(() => {
        (WizardWindow window, WizardViewModel model) = Open(kept: null);
        try {
          Walk(model, WizardPage.Folder);
          Sta.Flush();

          Assert.Equal(Visibility.Collapsed, window.FolderExtras.Visibility);
          Assert.True(window.Next.IsEnabled);
        } finally {
          window.Close();
        }
      });
    }

    private static void Walk(WizardViewModel model, WizardPage page) {
      while (model.Page != page && (model.CanGoNext || model.Page == WizardPage.Licence)) {
        model.LicenceAccepted |= model.Page == WizardPage.Licence;
        model.Next().GetAwaiter().GetResult();
        Sta.Flush();
      }

      Assert.Equal(page, model.Page);
    }

    private static string[] Buttons(WizardWindow window) {
      Sta.Flush();
      FrameworkElement[] buttons = [
          window.Back, window.Next, window.Install, window.Finish, window.Retry,
          window.CloseWizard];
      return [.. buttons.Where(b => b.Visibility == Visibility.Visible).Select(b => b.Name)];
    }

    // The window's watcher puts the Windows theme back meanwhile, so the theme comes last.
    private static void Save(Window window, ApplicationTheme theme, string path) {
      Sta.Wait(TimeSpan.FromMilliseconds(600));
      ApplicationThemeManager.Apply(theme, updateAccent: false);
      BrandTheme.Apply(theme);
      Sta.Flush();
      var root = (FrameworkElement)window.Content;
      var bitmap = new RenderTargetBitmap(
          (int)root.ActualWidth, (int)root.ActualHeight, 96, 96, PixelFormats.Pbgra32);
      bitmap.Render(root);
      var png = new PngBitmapEncoder();
      png.Frames.Add(BitmapFrame.Create(bitmap));
      using FileStream file = File.Create(path);
      png.Save(file);
    }

    private void RenderAll(
        string folder, ApplicationTheme theme, UiLanguage language, KeptItems kept) {
      string name = $"{theme}-{language}".ToLowerInvariant();
      (WizardWindow window, WizardViewModel model) = Open(kept);
      try {
        model.Language = language;
        foreach (WizardPage page in model.Pages.Where(p => p != WizardPage.Installing)) {
          Walk(model, page);
          Save(window, theme, Path.Join(folder, $"{name}-{(int)page}-{page}.png"));
          if (page == WizardPage.WelcomeBack) {
            model.StartsFresh = true;
            model.Next().GetAwaiter().GetResult();
            Save(window, theme, Path.Join(folder, $"{name}-{(int)page}-{page}-confirm.png"));
            model.CancelFresh();
          }
        }
      } finally {
        window.Close();
      }

      _outcome = SetupOutcome.Failed;
      (window, model) = Open(kept: null);
      try {
        model.Language = language;
        Walk(model, WizardPage.Installing);
        Save(window, theme, Path.Join(folder, $"{name}-5-Installing-failed.png"));
      } finally {
        window.Close();
        _outcome = SetupOutcome.Succeeded;
      }
    }

    private (WizardWindow Window, WizardViewModel Model) Open(
        KeptItems? kept, FakeFolders? probe = null, string? existing = null) {
      var folders = new WizardFolders(
          existing,
          Path.Join(_root, "Programs", "Navpaky"),
          _data,
          230_000_000,
          probe ?? new FakeFolders { Free = 40_000_000_000 });
      var model = new WizardViewModel(
          kept, folders, (_, _, _) => Task.FromResult(_outcome), _ => { });
      var window = new WizardWindow(model) {
        WindowStartupLocation = WindowStartupLocation.Manual,
        Left = -20_000,
        ShowActivated = false,
      };
      window.Show();
      Sta.Flush();
      return (window, model);
    }
  }
}
