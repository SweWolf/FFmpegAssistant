namespace FFmpegAssistant
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            string? startupCommand = args.Length > 0 ? string.Join(" ", args).Trim() : null;
            Application.Run(new Form1(startupCommand));
        }
    }
}