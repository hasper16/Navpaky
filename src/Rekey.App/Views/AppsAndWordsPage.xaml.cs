using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Rekey.App.Tray;
using Rekey.App.ViewModels;
using Rekey.Core;
using WordLanguage = Rekey.Core.Languages.Language;

namespace Rekey.App.Views {
  // Apps and words: the Excluded Apps, the Protected Words and the Learned Words.
  internal sealed partial class AppsAndWordsPage {
    private readonly ExcludedAppsViewModel _apps;
    private readonly WordsViewModel _words;

    public AppsAndWordsPage(ExcludedAppsViewModel apps, WordsViewModel words) {
      InitializeComponent();
      _apps = apps;
      _words = words;
      Apps.DataContext = apps;
      DataContext = words;
      (WordLanguage Language, string Name)[] languages = [
          (WordLanguage.English, Strings.LanguageEnglish),
          (WordLanguage.Russian, Strings.LanguageRussian),
          (WordLanguage.Ukrainian, Strings.LanguageUkrainian)];
      foreach ((WordLanguage language, string name) in languages) {
        LearnedLanguage.Items.Add(new ComboBoxItem { Content = name, Tag = language });
      }

      LearnedLanguage.SelectedIndex = 0;
      AddRunning.Click += (_, _) => ShowRunningApps();
      Browse.Click += (_, _) => BrowseForApp();
      AddProtected.Click += (_, _) => TakeProtected();
      AddLearned.Click += (_, _) => TakeLearned();
      NewProtected.KeyDown += (_, e) => OnEnter(e, TakeProtected);
      NewLearned.KeyDown += (_, e) => OnEnter(e, TakeLearned);
    }

    private static void OnEnter(KeyEventArgs e, Action action) {
      if (e.Key == Key.Enter) {
        e.Handled = true;
        action();
      }
    }

    private void ShowRunningApps() {
      var menu = new ContextMenu { PlacementTarget = AddRunning };
      foreach ((AppId app, string name) in RunningApps.List()) {
        var item = new MenuItem {
          Header = name,
          Icon = new Image { Source = AppIcons.Of(app.ExePath), Width = 16, Height = 16 },
        };
        item.Click += (_, _) => _apps.Add(app);
        menu.Items.Add(item);
      }

      menu.IsOpen = true;
    }

    private void BrowseForApp() {
      var dialog = new OpenFileDialog {
        Filter = $"{Strings.ProgramFilter}|*.exe",
        CheckFileExists = true,
      };
      if (dialog.ShowDialog(Window.GetWindow(this)) == true) {
        _apps.Add(new AppId(dialog.FileName));
      }
    }

    private void TakeProtected() {
      if (_words.AddProtected(NewProtected.Text)) {
        NewProtected.Clear();
      }
    }

    private void TakeLearned() {
      if (LearnedLanguage.SelectedItem is ComboBoxItem { Tag: WordLanguage language }
          && _words.AddLearned(NewLearned.Text, language)) {
        NewLearned.Clear();
      }
    }

    private void OnRemoveApp(object sender, RoutedEventArgs e) {
      if (sender is FrameworkElement { Tag: ExcludedAppEntry app }) {
        _apps.Remove(app);
      }
    }

    private void OnRemoveProtected(object sender, RoutedEventArgs e) {
      if (sender is FrameworkElement { Tag: string word }) {
        _words.RemoveProtected(word);
      }
    }

    private void OnRemoveLearned(object sender, RoutedEventArgs e) {
      if (sender is FrameworkElement { Tag: LearnedEntry word }) {
        _words.RemoveLearned(word);
      }
    }
  }
}
