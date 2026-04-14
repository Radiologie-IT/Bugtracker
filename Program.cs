using Bugtracker.Console;
using Bugtracker.Logging;
using System;
using System.Collections.Generic;
using Bugtracker.Configuration;
using Bugtracker.Plugin;
using Bugtracker.Utils;
using System.IO;
using Bugtracker.Properties;
using System.Windows.Forms;
using System.Runtime;
using CommunityToolkit.WinUI.Notifications;

namespace Bugtracker
{

    /// <summary>
    /// This is the starting point of 
    /// bugtracker programming project
    /// </summary>
    static class Program
    {
        /// <summary>
        /// This is the main method.
        /// </summary>
        [STAThread]
        private static void Main(String[] args)
        {
            ProfileOptimization.SetProfileRoot(@"C:\Bugtracker");
            ProfileOptimization.StartProfile("Startup.Profile");

            //ApplicationConfiguration.Initialize();
            //Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            var rand = Application.HighDpiMode;

            // Register AppUserModelID for toast notifications
            // This is required for Windows toast notifications to work
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Handle toast activation if needed
            };

            //Initialize Running Configuration Instance, before everything else
            RunningConfiguration runningConfiguration = RunningConfiguration.GetInstance();
            runningConfiguration.InitStartupProcedure();

            //sends all unhandled exception to console handler unhandled exception trapper
            //System.AppDomain.CurrentDomain.UnhandledException += ConsoleHandler.UnhandledExceptionTrapper;

            // set up basic application directory 
            // and initialize logging
            SetupApplication(runningConfiguration, args);

            if (args.Length > 0 && (args[0].Contains("-sp") || args[0].Contains("-skipplugins")))
            {
                List<string> argsL = new(args);
                argsL.RemoveAt(0);
                args = argsL.ToArray();
            }

            if (!runningConfiguration.HideConsole)
                StartCommandLineApplication(args);
        }

        /// <summary>
        ///
        /// </summary>
        private static void SetupApplication(RunningConfiguration rc, string[] args)
        {
            Logger.Log("Checking if Blackhole Folder exists.", LoggingSeverity.Info);
            System.IO.Directory.CreateDirectory(Globals_and_Information.Globals.LOCAL_BLACKHOLE_FODLER_PATH);

            System.IO.Directory.CreateDirectory(Globals_and_Information.Globals.LOCAL_PLUGIN_FILES_PATH);

            Logger.Log("Deleted Blackhole folder contents", LoggingSeverity.Info);
            BugtrackerUtils.DeleteContentOfDirectory(Globals_and_Information.Globals.LOCAL_BLACKHOLE_FODLER_PATH);

            // log start of application
            Logger.Log("Bugtracker version 2 was started", (LoggingSeverity)2);

            // create tmp folder
            BugtrackerUtils.CreateTempFolder();

            Logger.Log("Checking if Program was started for the first Time. Executing first startup procedure if so.", LoggingSeverity.Info);

            if (FirstStartupProcedure(rc))
                Logger.Log("Began first startup Procedure.", LoggingSeverity.Info);
            else
                Logger.Log("Skipped First Startup Procedure.", LoggingSeverity.Info);

            if (args.Length == 0 || (args[0] != "-sp" && args[0] != "-skipplugins"))
                PluginManager.Load();
        }

        //TODO: Drüber nachdenken ob die Funktion Sinn macht.
        //TODO: GIT!

        /// <summary>
        /// Leiche - > war früher für registry zuständig.
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        private static bool FirstStartupProcedure(RunningConfiguration rc)
        {
            if(rc.FirstStartup)
            {
                //overwrites startup configuration, if this is the first startup of the application
                ConfigurationManager.OverwriteStartupConfig(null, ("firstStartup", "false"));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to start command line interface, as this is included as is
        /// </summary>
        /// <param name="args"></param>
        private static void StartCommandLineApplication(string[] args)
        {
            // create console window
            ConsoleHandler.Create();

            // create new bugtrackerConsole object
            //BugtrackConsole console = new BugtrackConsole();

            // start console version of bugtracker
            BugtrackConsole.StartBugtrackerConsoleLogic(args);

            // close console session
            ConsoleHandler.Destroy();
        }
    }
}
