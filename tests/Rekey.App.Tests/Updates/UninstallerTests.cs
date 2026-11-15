using System.IO;
using Rekey.App.Updates;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class UninstallerTests : IDisposable {
    private readonly string _data = Path.Join(
        Directory.CreateTempSubdirectory("rekey-tests").FullName, "Rekey");

    private readonly List<string> _questions = [];
    private int _signInOff;

    public UninstallerTests() {
      Directory.CreateDirectory(_data);
    }

    public void Dispose() {
      Directory.Delete(Path.GetDirectoryName(_data)!, recursive: true);
    }

    [Fact]
    public void SignInIsTurnedOffAndTheDataQuestionStarts() {
      Run();

      Assert.Equal(1, _signInOff);
      Assert.Equal([_data], _questions);
      Assert.True(Directory.Exists(_data));
    }

    [Fact]
    public void NoQuestionWithoutData() {
      Directory.Delete(_data);

      Run();

      Assert.Equal(1, _signInOff);
      Assert.Empty(_questions);
    }

    private void Run() {
      new Uninstaller(_data, () => _signInOff++, _questions.Add).Run();
    }
  }
}
