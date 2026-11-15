using System.IO;
using Rekey.App.Onboarding;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
  public sealed class ExecutableWatchTests : IDisposable {
    private static readonly TimeSpan _wait = TimeSpan.FromSeconds(5);

    private readonly ManualClock _clock = new();
    private readonly string _root;
    private readonly string _folder;
    private readonly string _exe;
    private readonly ManualResetEventSlim _gone = new();
    private int _notices;

    public ExecutableWatchTests() {
      _root = Path.Join(Path.GetTempPath(), "rekey-watch-" + Guid.NewGuid().ToString("N"));
      _folder = Path.Join(_root, "app");
      Directory.CreateDirectory(_folder);
      _exe = Path.Join(_folder, "Invented.exe");
      File.WriteAllText(_exe, "x");
    }

    public void Dispose() {
      _gone.Dispose();
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void DeletingTheExecutableIsNoticed() {
      using ExecutableWatch watch = Start();

      File.Delete(_exe);

      Assert.True(_gone.Wait(_wait, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void RenamingTheExecutableIsNoticed() {
      using ExecutableWatch watch = Start();

      File.Move(_exe, _exe + ".old");

      Assert.True(_gone.Wait(_wait, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void MovedFolderIsNoticedByTheMinuteCheck() {
      using ExecutableWatch watch = Start();
      Directory.Move(_folder, _folder + "-moved");

      _clock.Advance(ExecutableWatch.CheckPeriod);

      Assert.True(_gone.IsSet);
    }

    [Fact]
    public void ExecutableInPlaceIsNotNoticed() {
      using ExecutableWatch watch = Start();

      _clock.Advance(ExecutableWatch.CheckPeriod);
      File.WriteAllText(Path.Join(_folder, "other.txt"), "y");

      Assert.False(
          _gone.Wait(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void DisappearanceIsReportedOnce() {
      using ExecutableWatch watch = Start();
      File.Delete(_exe);
      _gone.Wait(_wait, TestContext.Current.CancellationToken);

      _clock.Advance(ExecutableWatch.CheckPeriod);

      Assert.Equal(1, Volatile.Read(ref _notices));
    }

    [Fact]
    public void FolderGoneBeforeStartIsNoticedByTheMinuteCheck() {
      Directory.Delete(_folder, recursive: true);
      using ExecutableWatch watch = Start();

      _clock.Advance(ExecutableWatch.CheckPeriod);

      Assert.True(_gone.IsSet);
    }

    private ExecutableWatch Start() {
      var watch = new ExecutableWatch(_exe, _clock);
      watch.Disappeared += () => {
        Interlocked.Increment(ref _notices);
        _gone.Set();
      };
      watch.Start();
      return watch;
    }
  }
}
