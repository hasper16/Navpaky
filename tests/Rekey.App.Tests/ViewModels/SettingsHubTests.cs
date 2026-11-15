using System.IO;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.ViewModels {
  public sealed class SettingsHubTests : IDisposable {
    private readonly ManualClock _clock = new();
    private readonly List<Settings> _saved = [];
    private readonly List<Exception> _failures = [];
    private readonly SettingsHub _hub;
    private bool _diskFull;

    public SettingsHubTests() {
      _hub = new SettingsHub(new Settings(), Save, _clock, _failures.Add);
    }

    public void Dispose() {
      _hub.Dispose();
    }

    [Fact]
    public void AFailedTimedSaveIsReportedAndKeptForTheNextSave() {
      _diskFull = true;
      _hub.Update(settings => settings with { PlaySwitchSound = true });

      _clock.Advance(TimeSpan.FromSeconds(1));

      Assert.IsType<IOException>(Assert.Single(_failures));
      _diskFull = false;
      _hub.Flush();
      Assert.True(Assert.Single(_saved).PlaySwitchSound);
    }

    [Fact]
    public void ANewerChangeWinsOverTheFailedSnapshot() {
      _diskFull = true;
      _hub.Update(settings => settings with { PlaySwitchSound = true });
      _clock.Advance(TimeSpan.FromSeconds(1));
      _diskFull = false;

      _hub.Update(settings => settings with { ShowCursorIndicator = false });
      _clock.Advance(TimeSpan.FromSeconds(1));

      Settings saved = Assert.Single(_saved);
      Assert.True(saved.PlaySwitchSound);
      Assert.False(saved.ShowCursorIndicator);
    }

    private void Save(Settings settings) {
      if (_diskFull) {
        throw new IOException("disk full");
      }

      _saved.Add(settings);
    }
  }
}
