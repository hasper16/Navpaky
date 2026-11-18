using System.IO;
using System.Text;
using Navpaky.App.ViewModels;
using Xunit;

namespace Navpaky.App.Tests.ViewModels {
  // Spec statistics-and-diagnostics: the preview is exactly what is saved or copied; the
  // word-analysis log is in it only when the user ticks it.
  public sealed class ExportViewModelTests : IDisposable {
    private readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("navpaky-export-");
    private readonly List<string> _copied = [];
    private readonly ExportViewModel _model;

    public ExportViewModelTests() {
      _model = new ExportViewModel(
          includeLog => includeLog ? "report\nанализ ghbdtn\n" : "report\n", _copied.Add);
    }

    public void Dispose() {
      _folder.Delete(recursive: true);
    }

    [Fact]
    public void PreviewLeavesTheLogOutByDefault() {
      Assert.False(_model.IncludeAnalysisLog);
      Assert.Equal("report\n", _model.Preview);
    }

    [Fact]
    public void TickingTheLogAddsItToThePreview() {
      var changed = new List<string?>();
      _model.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

      _model.IncludeAnalysisLog = true;

      Assert.Equal("report\nанализ ghbdtn\n", _model.Preview);
      Assert.Contains(nameof(ExportViewModel.Preview), changed);
    }

    [Fact]
    public void SavedFileEqualsThePreview() {
      _model.IncludeAnalysisLog = true;
      string path = Path.Join(_folder.FullName, "out.txt");

      _model.SaveTo(path);

      Assert.Equal(_model.Preview, File.ReadAllText(path));
      Assert.Equal(
          Encoding.UTF8.GetBytes(_model.Preview), File.ReadAllBytes(path));
    }

    [Fact]
    public void CopyPlacesThePreviewOnTheClipboard() {
      _model.Copy();

      Assert.Equal(["report\n"], _copied);
    }

    [Fact]
    public void DefaultFileNameCarriesTheTime() {
      var time = new DateTimeOffset(2026, 11, 16, 9, 5, 7, TimeSpan.Zero);

      Assert.Equal(
          "navpaky-diagnostics-20261116-090507.txt", ExportViewModel.DefaultFileName(time));
    }
  }
}
