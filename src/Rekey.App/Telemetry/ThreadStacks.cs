using System.IO;
using Microsoft.Diagnostics.Runtime;
using Sentry;

namespace Rekey.App.Telemetry {
  // .NET cannot read another thread's stack, so a freeze report reads it from an in-memory
  // snapshot of this process (design D1). Only assembly and method names leave the snapshot;
  // module file names are cut to the assembly name because they hold the user's folder.
  internal static class ThreadStacks {
    public static IReadOnlyList<SentryStackFrame> Of(int managedThreadId) {
      using DataTarget target = DataTarget.CreateSnapshotAndAttach(Environment.ProcessId);
      using ClrRuntime runtime = target.ClrVersions[0].CreateRuntime();
      ClrThread? thread = runtime.Threads.FirstOrDefault(
          candidate => candidate.ManagedThreadId == managedThreadId);
      if (thread is null) {
        return [];
      }

      return [.. thread.EnumerateStackTrace()
          .Where(frame => frame.Method is not null)
          .Select(frame => new SentryStackFrame {
            Module = Path.GetFileNameWithoutExtension(frame.Method!.Type.Module.Name),
            Function = $"{frame.Method.Type.Name}.{frame.Method.Name}",
          })];
    }
  }
}
