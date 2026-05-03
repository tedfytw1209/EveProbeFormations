namespace EveProbeFormations
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Start up in unlocked mode if the file exists called "I accept all risks of running this tool unlocked"
            Helper.RunningInUnlockedMode = File.Exists("I accept all risks of running this tool unlocked");

            Helper.LoadSettings();

            Application.Run(new frmProfileSelector());
        }
    }
}