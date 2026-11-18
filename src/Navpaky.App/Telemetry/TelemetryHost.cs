using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Diagnostics.Runtime;
using Navpaky.App.ViewModels;
using Navpaky.Core;
using Navpaky.Core.Diagnostics;
using Navpaky.Core.Languages;
using Navpaky.Core.Statistics;
using Navpaky.Core.Telemetry;
using Navpaky.Core.Telemetry.Schemas;

namespace Navpaky.App.Telemetry {
  // Everything Navpaky may send (spec telemetry), started and stopped by the user's consent.
  // Created, updated and disposed on the UI thread; ReportEngineStall runs on the freeze
  // guard's thread.
  internal sealed class TelemetryHost : IDisposable {
    // Spec telemetry: a crash report carries the Incident types of the last 10 minutes.
    private static readonly TimeSpan _incidentWindow = TimeSpan.FromMinutes(10);

    private readonly string _dataFolder;
    private readonly SettingsHub _hub;
    private readonly int _uiThreadId;
    private readonly CrashReports _crashReports;
    private readonly DispatcherFreezeWatch _uiWatch;
    private readonly TelemetryPost _post;
    private readonly TelemetryPost _signUpPost;
    private readonly DigestScheduler _digests;
    private readonly SetupSignalOnce _setupSignal;
    private CancellationTokenSource _usage = new();

    public TelemetryHost(
        string dataFolder,
        Dispatcher dispatcher,
        SettingsHub hub,
        Func<StatsDocument> statistics,
        Func<IReadOnlyList<LayoutId>> layouts) {
      _dataFolder = dataFolder;
      _hub = hub;
      _uiThreadId = dispatcher.Thread.ManagedThreadId;
      Sent = new SentLog(dataFolder);
      _crashReports = new CrashReports(new CrashReportSetup(
          TelemetryEndpoints.SentryDsn, Version, RecentIncidents, RecordSent));
      _uiWatch = new DispatcherFreezeWatch(
          TimeProvider.System,
          ping => dispatcher.BeginInvoke(DispatcherPriority.Send, ping),
          () => ReportFreeze("Ui", _uiThreadId));
      _post = new TelemetryPost(TelemetryPost.NetworkHandler(), RecordSent);

      // Design D4: the sign-up is never kept on this PC, not even in the sent log.
      _signUpPost = new TelemetryPost(TelemetryPost.NetworkHandler(), _ => { });
      _digests = new DigestScheduler(
          TimeProvider.System,
          action => dispatcher.BeginInvoke(action),
          hub,
          day => TelemetryReports.Digest(day, hub.Current, statistics(), layouts()),
          (json, token) => _post.SendAsync(TelemetryEndpoints.UsageDigest, json, token),
          TimeSpan.FromMinutes(60 + Random.Shared.Next(60)));
      _setupSignal = new SetupSignalOnce(
          hub,
          progress => TelemetryReports.Setup(hub.Current, layouts(), LanguageOf(), progress),
          json => _post.SendAsync(TelemetryEndpoints.SetupSignal, json, _usage.Token));
    }

    public static string Version { get; } = (Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0").Split('+')[0];

    public SentLog Sent { get; }

    public void Start() {
      Apply(_hub.Current);
      _uiWatch.Start();
      _digests.Start();
    }

    public void Apply(Settings settings) {
      _crashReports.Apply(settings.DiagnosticsConsent);
      _digests.Apply(settings);
      if (!settings.DiagnosticsConsent.UsageDigest && !_usage.IsCancellationRequested) {
        _usage.Cancel();
        _usage.Dispose();
        _usage = new CancellationTokenSource();
      }
    }

    public void OnWelcomeClosed(WelcomeProgress progress) {
      _setupSignal.OnWelcomeClosed(progress);
    }

    public SignUpViewModel NewSignUp() {
      return new SignUpViewModel(
          json => _signUpPost.SendAsync(TelemetryEndpoints.SignUp, json, CancellationToken.None));
    }

    public IReadOnlyList<string> ReadSent() {
      try {
        return Sent.Read();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return [];
      }
    }

    public void ReportEngineStall(int workerThreadId) {
      ReportFreeze("Engine", workerThreadId);
    }

    public void Flush() {
      _crashReports.Flush();
    }

    public void Dispose() {
      _uiWatch.Dispose();
      _digests.Dispose();
      _usage.Cancel();
      _usage.Dispose();
      _crashReports.Dispose();
      _post.Dispose();
      _signUpPost.Dispose();
    }

    // Read once for the one-time setup signal; the worker keeps its own copy.
    private static Func<LayoutId, Language?> LanguageOf() {
      LanguageSet languages = LanguageSet.LoadBundled(DateTimeOffset.UtcNow).Languages;
      return layout => languages.ProfileFor(layout)?.Language;
    }

    private void ReportFreeze(string thread, int managedThreadId) {
      if (!_crashReports.IsActive) {
        return;
      }

      try {
        _crashReports.ReportFreeze(thread, ThreadStacks.Of(managedThreadId));
      } catch (Exception e) when (e is ClrDiagnosticsException or InvalidOperationException
          or NotSupportedException or IOException) {
        // The snapshot may be refused; the freeze is still worth reporting without its stack.
        _crashReports.ReportFreeze(thread, []);
      }
    }

    private IReadOnlyList<IncidentType> RecentIncidents() {
      DateTimeOffset since = DateTimeOffset.UtcNow - _incidentWindow;
      try {
        return [.. new IncidentLog(_dataFolder).Read()
            .Where(entry => entry.Time >= since)
            .Select(entry => entry.Type)];
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return [];
      }
    }

    private void RecordSent(string json) {
      try {
        Sent.Append(json);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        System.Diagnostics.Trace.WriteLine(
            $"Navpaky could not keep a sent request: {e.GetType().Name}");
      }
    }
  }
}
