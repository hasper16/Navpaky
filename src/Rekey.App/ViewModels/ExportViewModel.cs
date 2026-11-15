using System.Globalization;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Rekey.App.ViewModels {
  // The diagnostics export dialog: the preview is the exact text that is saved or copied
  // (design D5 of add-statistics-and-diagnostics).
  internal sealed class ExportViewModel : ObservableObject {
    private readonly Func<bool, string> _build;
    private readonly Action<string> _copy;
    private bool _includeAnalysisLog;

    public ExportViewModel(Func<bool, string> build, Action<string> copy) {
      _build = build;
      _copy = copy;
      Preview = build(false);
    }

    public string Preview { get; private set; }

    public bool IncludeAnalysisLog {
      get {
        return _includeAnalysisLog;
      }

      set {
        if (SetProperty(ref _includeAnalysisLog, value)) {
          Preview = _build(value);
          OnPropertyChanged(nameof(Preview));
        }
      }
    }

    public static string DefaultFileName(DateTimeOffset time) {
      return string.Create(
          CultureInfo.InvariantCulture, $"rekey-diagnostics-{time:yyyyMMdd-HHmmss}.txt");
    }

    // UTF-8 without a byte order mark, so the file holds the preview's bytes and nothing else.
    public void SaveTo(string path) {
      File.WriteAllText(path, Preview, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // Spec statistics-and-diagnostics: the only place Rekey writes the clipboard, and only
    // when the user asks.
    public void Copy() {
      _copy(Preview);
    }
  }
}
