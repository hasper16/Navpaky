using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.Core;

namespace Navpaky.App.ViewModels {
  // The Excluded Apps editor of the Apps and words page.
  internal sealed class ExcludedAppsViewModel : ObservableObject, IDisposable {
    private readonly SettingsHub _hub;
    private readonly Func<string, string> _nameOf;

    public ExcludedAppsViewModel(SettingsHub hub, Func<string, string> nameOf) {
      _hub = hub;
      _nameOf = nameOf;
      Apps = List(hub.Current);
      _hub.Changed += OnHubChanged;
    }

    public IReadOnlyList<ExcludedAppEntry> Apps { get; private set; }

    public void Dispose() {
      _hub.Changed -= OnHubChanged;
    }

    public void Add(AppId app) {
      _hub.Update(settings => settings with {
        ExcludedApps = new ExcludedAppList([.. settings.ExcludedApps.Entries, app.Key]),
      });
    }

    public void Remove(ExcludedAppEntry app) {
      _hub.Update(settings => settings with {
        ExcludedApps = new ExcludedAppList(
            settings.ExcludedApps.Entries.Where(entry => entry != app.Key)),
      });
    }

    private void OnHubChanged(Settings settings) {
      Apps = List(settings);
      OnPropertyChanged(nameof(Apps));
    }

    private List<ExcludedAppEntry> List(Settings settings) {
      return [.. settings.ExcludedApps.Entries
          .Select(key => new ExcludedAppEntry(key, _nameOf(key)))
          .OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)];
    }
  }
}
