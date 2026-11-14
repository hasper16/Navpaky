using System.IO;
using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.ViewModels {
  public sealed class ExcludedAppsViewModelTests : IDisposable {
    private static readonly AppId _terminal =
        new(@"C:\Program Files\WindowsApps\Terminal\WindowsTerminal.exe");

    private static readonly AppId _zebra = new(@"C:\Apps\zebra.exe");

    private readonly SettingsHub _hub = new(new Settings(), _ => { }, new ManualClock());
    private readonly ExcludedAppsViewModel _model;

    public ExcludedAppsViewModelTests() {
      _model = new ExcludedAppsViewModel(_hub, key => Path.GetFileNameWithoutExtension(key));
    }

    public void Dispose() {
      _hub.Dispose();
    }

    [Fact]
    public void AddedAppIsListedWithItsName() {
      _model.Add(_terminal);

      ExcludedAppEntry entry = Assert.Single(_model.Apps);
      Assert.Equal("WindowsTerminal", entry.Name);
      Assert.True(_hub.Current.ExcludedApps.Contains(_terminal));
    }

    [Fact]
    public void AppsAreListedByName() {
      _model.Add(_zebra);
      _model.Add(_terminal);

      Assert.Equal(["WindowsTerminal", "zebra"], _model.Apps.Select(app => app.Name));
    }

    [Fact]
    public void RemovedAppIsNoLongerExcluded() {
      _model.Add(_terminal);

      _model.Remove(_model.Apps[0]);

      Assert.Empty(_model.Apps);
      Assert.False(_hub.Current.ExcludedApps.Contains(_terminal));
    }

    [Fact]
    public void AppExcludedFromTheTrayShowsUpToo() {
      _hub.Update(settings => settings with { ExcludedApps = new ExcludedAppList([_zebra.Key]) });

      Assert.Equal("zebra", Assert.Single(_model.Apps).Name);
    }
  }
}
