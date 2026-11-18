using System.IO;
using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class UninstallerTests : IDisposable {
    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-tests").FullName;
    private readonly DataFolders _data;
    private readonly List<DataFolders> _questions = [];
    private int _signInOff;

    public UninstallerTests() {
      _data = new DataFolders(Path.Join(_root, "Navpaky"), Path.Join(_root, "Navpaky Data"));
      Directory.CreateDirectory(_data.Settings);
      Directory.CreateDirectory(_data.Local);
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void SignInIsTurnedOffAndOneQuestionCoversBothFolders() {
      Run();

      Assert.Equal(1, _signInOff);
      Assert.Equal([_data], _questions);
      Assert.True(Directory.Exists(_data.Settings));
      Assert.True(Directory.Exists(_data.Local));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EitherFolderAloneStillAsks(bool keepSettings) {
      Directory.Delete(keepSettings ? _data.Local : _data.Settings);

      Run();

      Assert.Equal([_data], _questions);
    }

    [Fact]
    public void NoQuestionWithoutData() {
      Directory.Delete(_data.Settings);
      Directory.Delete(_data.Local);

      Run();

      Assert.Equal(1, _signInOff);
      Assert.Empty(_questions);
    }

    private void Run() {
      new Uninstaller(_data, () => _signInOff++, _questions.Add).Run();
    }
  }
}
