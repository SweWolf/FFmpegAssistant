using System.Runtime.InteropServices;

namespace FFmpegAssistant
{
    // =========================================================================
    // TaskbarProgress — reusable Windows taskbar progress helper
    // =========================================================================
    // Drop this file into any C# WinForms project (adjust the namespace).
    // Requires Windows 7 or later. Fails silently on unsupported systems.
    //
    // Quick-start:
    //   TaskbarProgress.SetNormal(this, 50, 100);   // green bar at 50 %
    //   TaskbarProgress.SetIndeterminate(this);      // pulsing bar
    //   TaskbarProgress.SetError(this, 75, 100);     // red bar at 75 %
    //   TaskbarProgress.SetPaused(this, 75, 100);    // yellow bar at 75 %
    //   TaskbarProgress.Clear(this);                 // remove bar
    // =========================================================================

    /// <summary>Taskbar button progress state.</summary>
    public enum TaskbarProgressState
    {
        /// <summary>No progress indicator.</summary>
        None          = 0x0,
        /// <summary>Pulsing green bar — use when progress % is unknown.</summary>
        Indeterminate = 0x1,
        /// <summary>Solid green bar.</summary>
        Normal        = 0x2,
        /// <summary>Solid red bar — use on error.</summary>
        Error         = 0x4,
        /// <summary>Solid yellow bar — use when paused.</summary>
        Paused        = 0x8
    }

    /// <summary>
    /// Controls the progress indicator shown on a window's taskbar button.
    /// Wraps the Windows <c>ITaskbarList3</c> COM interface.
    /// Thread-safe: all public methods marshal back to the UI thread if needed.
    /// Generic and reusable — no dependency on any specific form or application.
    /// </summary>
    public static class TaskbarProgress
    {
        // ---------------------------------------------------------------------
        // COM interface definition
        // Each method must appear in vtable order (ITaskbarList → List2 → List3)
        // ---------------------------------------------------------------------

        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            void HrInit();
            void AddTab(IntPtr hwnd);
            void DeleteTab(IntPtr hwnd);
            void ActivateTab(IntPtr hwnd);
            void SetActiveAlt(IntPtr hwnd);
            // ITaskbarList2
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
            // ITaskbarList3
            void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
            void SetProgressState(IntPtr hwnd, TaskbarProgressState tbpFlags);
        }

        [ComImport]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance { }

        private static readonly ITaskbarList3? _taskbar;

        static TaskbarProgress()
        {
            try
            {
                _taskbar = (ITaskbarList3)new TaskbarInstance();
                _taskbar.HrInit();
            }
            catch
            {
                _taskbar = null; // COM not available (pre-Win7 or sandboxed)
            }
        }

        // ---------------------------------------------------------------------
        // Core API — takes a raw window handle
        // ---------------------------------------------------------------------

        /// <summary>Sets the progress state on a taskbar button.</summary>
        /// <param name="hwnd">The window handle (<c>form.Handle</c>).</param>
        /// <param name="state">The desired <see cref="TaskbarProgressState"/>.</param>
        public static void SetState(IntPtr hwnd, TaskbarProgressState state)
        {
            try { _taskbar?.SetProgressState(hwnd, state); } catch { }
        }

        /// <summary>
        /// Sets the progress value on a taskbar button.
        /// Also sets the state to <see cref="TaskbarProgressState.Normal"/> if it
        /// is currently <see cref="TaskbarProgressState.None"/> or
        /// <see cref="TaskbarProgressState.Indeterminate"/>.
        /// </summary>
        /// <param name="hwnd">The window handle (<c>form.Handle</c>).</param>
        /// <param name="current">Current progress value.</param>
        /// <param name="total">Total (maximum) value.</param>
        public static void SetValue(IntPtr hwnd, long current, long total)
        {
            try { _taskbar?.SetProgressValue(hwnd, (ulong)current, (ulong)total); } catch { }
        }

        // ---------------------------------------------------------------------
        // Convenience API — takes a Form directly
        // ---------------------------------------------------------------------

        /// <summary>Shows a green progress bar.</summary>
        public static void SetNormal(Form form, long current, long total)
        {
            SetState(form.Handle, TaskbarProgressState.Normal);
            SetValue(form.Handle, current, total);
        }

        /// <summary>Shows a pulsing green bar (use when progress % is unknown).</summary>
        public static void SetIndeterminate(Form form)
            => SetState(form.Handle, TaskbarProgressState.Indeterminate);

        /// <summary>Shows a red progress bar (use on error).</summary>
        public static void SetError(Form form, long current, long total)
        {
            SetState(form.Handle, TaskbarProgressState.Error);
            SetValue(form.Handle, current, total);
        }

        /// <summary>Shows a yellow progress bar (use when paused).</summary>
        public static void SetPaused(Form form, long current, long total)
        {
            SetState(form.Handle, TaskbarProgressState.Paused);
            SetValue(form.Handle, current, total);
        }

        /// <summary>Removes the progress indicator from the taskbar button.</summary>
        public static void Clear(Form form)
            => SetState(form.Handle, TaskbarProgressState.None);
    }
}
