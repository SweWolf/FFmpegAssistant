using System.Runtime.InteropServices;

namespace FFmpegAssistant
{
    // =========================================================================
    // TaskbarFlash — flashes a window's taskbar button to get the user's attention
    // =========================================================================
    // Drop this file into any C# WinForms project (adjust the namespace).
    //
    // Quick-start:
    //   TaskbarFlash.FlashIfInactive(this);   // e.g. when a long job has finished
    //   TaskbarFlash.Stop(this);              // call from the form's Activated event
    //
    // Standard Windows behaviour: the button flashes a few times, then stays
    // highlighted (orange) until the window is activated.
    // =========================================================================

    /// <summary>
    /// Flashes a form's taskbar button, but only when the user is not already looking
    /// at the form (another window is in front, or the form is minimized).
    /// Fails silently: a failure only means the button does not flash.
    /// </summary>
    internal static class TaskbarFlash
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_STOP      = 0x0;
        private const uint FLASHW_ALL       = 0x3;  // caption + taskbar button
        private const uint FlashCount       = 5;    // then the button stays highlighted

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>Flashes the taskbar button of <paramref name="form"/> if it is not the active window.</summary>
        public static void FlashIfInactive(Form form)
        {
            if (!form.IsHandleCreated) return;
            if (form.WindowState != FormWindowState.Minimized && GetForegroundWindow() == form.Handle) return;
            Flash(form, FLASHW_ALL);
        }

        /// <summary>
        /// Stops the flashing and removes the highlight. Call it from the form's <c>Activated</c>
        /// event, so the button is also cleared when the user returns to the app through one of
        /// its message boxes rather than the form itself.
        /// </summary>
        public static void Stop(Form form)
        {
            if (form.IsHandleCreated) Flash(form, FLASHW_STOP);
        }

        private static void Flash(Form form, uint flags)
        {
            try
            {
                var info = new FLASHWINFO
                {
                    cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
                    hwnd = form.Handle,
                    dwFlags = flags,
                    uCount = flags == FLASHW_STOP ? 0 : FlashCount,
                    dwTimeout = 0 // default cursor blink rate
                };
                FlashWindowEx(ref info);
            }
            catch { /* never crash the host app */ }
        }
    }
}
