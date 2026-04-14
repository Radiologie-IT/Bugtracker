using System;
using System.IO;
using System.Reflection;

namespace Bugtracker.Globals_and_Information
{
    /// <summary>
    /// This Class stores all constants and global variables needed. The variables
    /// are readonly.
    /// </summary>
    public class Globals
    {

        /*
         * TEXT SPECIFIC GLOBALS
         */

        /// End of Line Character
        public static readonly string EOL_CHARACTER = Environment.NewLine;

        /// Whitespace Character
        public const char SPACE_CHARACTER = ' ';

        /*
         * FILE PATHS
         */

        public static readonly string APPLICATION_DIRECTORY                 = "C:\\Bugtracker\\";
        public static readonly string ASSEMBLY_WORKING_DIRECTORY            = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static readonly string TMP_DIRECTORY_NAME                    = "tmp";
        public static readonly string LOG_FILE_NAME                         = $"bugtracker.log";
        public static readonly string STARTUP_CONFIG_NAME                   = "bugtracker_config_startup.xml";
        public static readonly string CONFIG_DIRECTROY_NAME                 = "configs";
        public static readonly string BLACKHOLE_FOLDER_NAME                 = "blackhole";
        public static readonly string PLUGIN_FOLDER_NAME                    = "plugins";
        public static readonly string RESOURCES_FOLDER_NAME                 = "resources";
        public static readonly string TMP_DIRECTORY                         = System.IO.Path.GetTempPath() + "\\bugtracker_temp";
        public static readonly string LOG_DIRECTORY                            = Path.Join(APPLICATION_DIRECTORY, "Logs");
        public static readonly string LOG_FILE_PATH                         = Path.Join(LOG_DIRECTORY, LOG_FILE_NAME);
        public static readonly int LOG_ROTATION_COUNT                       = 10;
        public static readonly string LOCAL_STARTUP_CONFIG_FILE_PATH        = Path.Join(APPLICATION_DIRECTORY, STARTUP_CONFIG_NAME);
        public static readonly string LOCAL_CONFIG_FILES_PATH               = Path.Join(APPLICATION_DIRECTORY,CONFIG_DIRECTROY_NAME);
        public static readonly string LOCAL_BLACKHOLE_FODLER_PATH           = Path.Join(APPLICATION_DIRECTORY,BLACKHOLE_FOLDER_NAME);
        public static readonly string LOCAL_PLUGIN_FILES_PATH               = Path.Join(APPLICATION_DIRECTORY,PLUGIN_FOLDER_NAME);

        public static readonly string INTERNAL_CONFIG_FOLDER_PATH           = Path.Join(ASSEMBLY_WORKING_DIRECTORY, CONFIG_DIRECTROY_NAME);
        public static readonly string INTERNAL_STARTUP_CONFIG_FILE_PATH     = Path.Join(INTERNAL_CONFIG_FOLDER_PATH, STARTUP_CONFIG_NAME);
        public static readonly string INTERNAL_PLUGIN_FOLDER_PATH           = Path.Join(ASSEMBLY_WORKING_DIRECTORY, PLUGIN_FOLDER_NAME);
        public static readonly string INTERNAL_RESOURCES_FOLDER_PATH        = Path.Join(ASSEMBLY_WORKING_DIRECTORY, RESOURCES_FOLDER_NAME);

        public static string GetFittingConfigFilesPath()
        {
             return INTERNAL_CONFIG_FOLDER_PATH;
        }

        public static string GetFittingStartupConfigPath()
        {
             return INTERNAL_STARTUP_CONFIG_FILE_PATH;
        }

        public static string GetFittingPluginFilesPath()
        {
             return INTERNAL_PLUGIN_FOLDER_PATH;
        }

        /// <summary>
        /// Image file format of screenshots, jpg by default
        /// </summary>
        public const string SCREENSHOT_FILE_FORMAT                      = ".JPG";

        /// <summary>
        /// Number of lines that should be saved in log file
        /// </summary>
        public const int MAX_LINES_OF_LOG_FILE                          = 2000;
    }
}
