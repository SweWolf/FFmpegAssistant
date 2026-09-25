using System.Runtime.InteropServices;

namespace FFmpegAssistant
{
    // =========================================================================
    // SleepBlocker — keeps Windows from going to sleep while a job is running
    // =========================================================================
    // Drop this file into any C# WinForms project (adjust the namespace).
    //
    // Quick-start:
    //   using var _ = SleepBlocker.Begin();   // PC stays awake until disposed
    //
    // Only automatic (idle) sleep is blocked: the screen can still turn off,
    // and the user can still choose Sleep, close the lid or shut down.
    // Windows drops the request by itself if the process exits or crashes.
    // =========================================================================

    /// <summary>
    /// Blocks automatic system sleep from <see cref="Begin"/> until the returned
    /// object is disposed. Uses <c>SetThreadExecutionState</c>, which is per thread,
    /// so dispose it on the thread that called <see cref="Begin"/> (in an async
    /// WinForms handler, awaits resume on the UI thread, so a <c>using</c> works).
    /// Fails silently: a failure only means the PC may sleep as usual.
    /// </summary>
    internal sealed class SleepBlocker : IDisposable
    {
        [Flags]
        private enum ExecutionState : uint
        {
            SystemRequired = 0x00000001,
            Continuous     = 0x80000000
        }

        [DllImport("kernel32.dll")]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState flags);

        private bool _disposed;

        private SleepBlocker()
        {
            try { SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired); }
            catch { /* never crash the host app */ }
        }

        /// <summary>Starts blocking automatic sleep. Dispose the result to allow it again.</summary>
        public static SleepBlocker Begin() => new();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { SetThreadExecutionState(ExecutionState.Continuous); }
            catch { /* never crash the host app */ }
        }
    }
}
