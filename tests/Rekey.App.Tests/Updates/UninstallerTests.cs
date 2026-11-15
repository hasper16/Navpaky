using System.IO;
using Rekey.App.Updates;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class UninstallerTests : IDisposable {
    private readonly string _data = Path.Join(
        Directory.CreateTempSubdirectory("rekey-tests").FullName, "Rekey");

    private int _signInOff;
    private int _questions;

    public UninstallerTests() {
      Directory.CreateDirectory(Path.Join(_data, "analysis"));
      File.WriteAllText(Path.Join(_data, "settings.json"), "{}");
    }

    public void Dispose() {
      Directory.Delete(Path.GetDirectoryName(_data)!, recursive: true);
    }

    [Fact]
    public void DataIsDeletedWhenTheUserAgrees() {
      Run(deleteData: true);

      Assert.False(Directory.Exists(_data));
      Assert.Equal(1, _signInOff);
    }

    [Fact]
    public void DataIsKeptWhenTheUserDeclines() {
      Run(deleteData: false);

      Assert.True(File.Exists(Path.Join(_data, "settings.json")));
      Assert.Equal(1, _signInOff);
    }

    [Fact]
    public void NoQuestionWithoutData() {
      Directory.Delete(_data, recursive: true);

      Run(deleteData: true);

      Assert.Equal(0, _questions);
      Assert.Equal(1, _signInOff);
    }

    private void Run(bool deleteData) {
      new Uninstaller(_data, () => _signInOff++, () => {
        _questions++;
        return deleteData;
      }).Run();
    }
  }
}
