using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using Rekey.App.ViewModels;
using Wpf.Ui.Appearance;

namespace Rekey.App.Views {
  // The diagnostics export dialog (design D5): Save to file is the default action, Copy the
  // secondary one, since clipboard history keeps what is copied.
  internal sealed partial class ExportWindow {
    private readonly ExportViewModel _model;

    public ExportWindow(ExportViewModel model) {
      _model = model;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      DataContext = model;
      SaveButton.Click += (_, _) => Save();
      CopyButton.Click += (_, _) => Copy();
      Loaded += (_, _) => TitleBarButtons.Name(TitleBar);
    }

    // Another app may hold the clipboard open.
    private void Copy() {
      try {
        _model.Copy();
        Status.Text = Strings.ExportCopied;
      } catch (ExternalException) {
        Status.Text = Strings.ExportCopyFailed;
      }
    }

    private void Save() {
      var dialog = new SaveFileDialog {
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        FileName = ExportViewModel.DefaultFileName(DateTimeOffset.Now),
        DefaultExt = ".txt",
        Filter = "Text (*.txt)|*.txt",
      };
      if (dialog.ShowDialog(this) != true) {
        return;
      }

      try {
        _model.SaveTo(dialog.FileName);
        Status.Text = Strings.ExportSaved;
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        Status.Text = Strings.ExportFailed;
      }
    }
  }
}
