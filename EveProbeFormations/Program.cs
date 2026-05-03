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

            var now = Helper.GetApproximateInternetTime();
            if (now == null || now == default || now > new DateTime(2026, 05, 16))
            { 
                MessageBox.Show("This version of the tool is no longer supported. Please update to the latest version.", "Tool Expired", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Helper.LoadSettings();

            Application.Run(new frmProfileSelector());
        }
    }
}