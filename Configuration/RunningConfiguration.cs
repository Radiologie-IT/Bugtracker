using Bugtracker.Attributes;
using Bugtracker.Globals_and_Information;
using Bugtracker.InternalApplication;
using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using Bugtracker.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Bugtracker.Utils;
using Bugtracker.Variables;
using Timer = System.Windows.Forms.Timer;
using Bugtracker.Plugin;
using System.Threading.Tasks;
using System.Linq;

namespace Bugtracker.Configuration
{
    /// <summary>
    /// The current status of the Server of the running instance server connection
    /// </summary>
    public enum ServerStatus
    {
        /// <summary>
        /// Server is available.
        /// </summary>
        Available,
        /// <summary>
        /// Server is not available.
        /// </summary>
        NotAvailable
    }

    /// <summary>
    /// Type of Config Source
    /// </summary>
    public enum ConfigSource
    {
        Webserver,
        Server,
        Client
    }

    /// <summary>
    /// The Running Configuration of the Application.
    /// </summary>
    public class RunningConfiguration : Singleton<RunningConfiguration>
    {

        //public Task GetServerStatus
        //{
        //    return Task.Run(() => ServerStatus)
        //}

        public static event EventHandler InitiliazedRunningConfiguration;

        /// <summary>
        /// The Manager Object for modifying Applications
        /// </summary>
        public ApplicationManager Applications { get; set; }

        /// <summary>
        /// The Manager Object for modifying Targets
        /// </summary>
        public TargetManager Targets { get; set; }

        /// <summary>
        /// The Manager Object for modifying Problems and their categories
        /// </summary>
        public ProblemManager ProblemCategories { get; set; }

        /// <summary>
        /// The Manager Object for storing variables that can be used in configuration
        /// </summary>
        public VariableManager Variables { get; set; }

        /// <summary>
        /// The Manager Object reading and writing config-file data to and from running Configuration
        /// </summary>
        public ConfigurationManager Configurations { get; protected set; }

        [Key("version")]
        public string Version
        {
            get
            {
                return null;
            }
            //get
            //{
            //    if (ApplicationDeployment.IsNetworkDeployed)
            //        myVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
            //}
        }


        /// <summary>
        /// For Folder 
        /// </summary>
        [Key("idString")]
        public String IdentificationString
        { 
            get
            {
                if(PCInfo.IsRemoteSession)
                {
                    return PCInfo.Clientname + "-on-" + PCInfo.Hostname;
                }
                else
                {
                    return PCInfo.Clientname;
                }
            }
        } 

        /// <summary>
        /// The current status of the Server, where configurations are loaded from and Captures are sent
        /// </summary>
        [Key("serverstatus")]
        public ServerStatus ServerStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key("mainserver")]
        public string ServerAddress { get; set; }

        /// <summary>
        /// The last successful connection time to the main server
        /// </summary>
        [Key("serverLastConnectionTime")]
        public DateTime ServerLastConnectionTime 
        { 
            get; set; 
        }
        /// <summary>
        ///
        /// </summary>
        [Key("serverPath")]
        public string ServerPath { get; set; }

        /// <summary>
        /// URL to the Django web server for downloading configuration via HTTP
        /// </summary>
        [Key("configWebserverUrl")]
        public string ConfigWebserverUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Key("configSourceType")]
        public ConfigSource ConfigSource { get; set; }

        /// <summary>
        /// PC Info Object containing useful information about the host PC
        /// </summary>
        public PCInfo PcInfo { get; protected set; }

        /// <summary>
        /// The Main Server Object containing ServerStatus and other networking information
        /// </summary>
        public Server MainServer { get; set; }

        /// <summary>
        /// LoggerEnabled
        /// </summary>
        public bool LoggerEnabled { get; set; }

        /// <summary>
        /// Returns the currently selected problem category, either selcted via the console or gui
        /// </summary>
        public ProblemCategory SelectedProblemCategory { get; set; }

        /// <summary>
        /// Abrreviation of Selected Problem Category used for variable replacement in configuration
        /// </summary>
        [Key("abbrev", true)]
        public string SelectedProblemCategoryAbbrev
        {
            get
            {
                if (SelectedProblemCategory != null)
                    return SelectedProblemCategory.TicketAbbreviation;
                else
                    return "non-selected";
            }
        }

        /// <summary>
        /// Get the current LogSeverity of Bugtracker
        /// </summary>
        public LoggingSeverity LogSeverity { get; protected set; }

        /// <summary>
        /// Timeout in milliseconds for file/directory existence checks to prevent hanging on unreachable network shares.
        /// Default: 1000ms
        /// </summary>
        public int FileCheckTimeoutMs { get; protected set; } = 1000;

        /// <summary>
        ///
        /// </summary>
        public string TargetPath { get; protected set; }

        /// <summary>
        /// The current Main GUI of the Application
        /// </summary>
        public Form MainGui { get; set; }

        /// <summary>
        /// If the Console should be hidden
        /// </summary>
        public bool HideConsole { get; set; } = false;

        /// <summary>
        /// If the Console should be hidden. With property.
        /// </summary>
        [Key("firststartup")]
        public bool FirstStartup { get; set; } = (bool) ConfigurationManager.GetStartupValue("firstStartup");

        /// <summary>
        /// The currernt configuration folder path.
        /// </summary>
        [Key("configurationFolderPath")]
        public string ConfigurationFolderPath { get; protected set; }

        /// <summary>
        /// The startup time of the application
        /// </summary>
        [Key("startupTime")]
        public DateTime StartupTime { get; protected set; }

        private List<DirectoryInfo> _bugtrackerFolders;

        /// <summary>
        /// All Bugtrack Folders of current Session
        /// </summary>
        public List<DirectoryInfo> BugtrackerFolders
        {
            get => BugtrackerUtils.GetAllExisitingDirectories(_bugtrackerFolders);

            set => _bugtrackerFolders = value;
        }

        /// <summary>
        /// List of all current loaded Plugins
        /// </summary>
        public List<IPlugin> LoadedPlugins = new();

        public string dateString;

        /// <summary>
        /// Date used for variable replacement in configuration
        /// </summary>
        [Key("date", true)]
        public string DateString 
        { 
            set => dateString = value;
            get => DateTime.Now.ToString("MM-dd-yyyy"); 
        }

        public string timeString;


        /// <summary>
        /// Time used for variable replacement in configuration
        /// </summary>
        [Key("time", true)]
        public string TimeString 
        {
            set => timeString = value;
            get => DateTime.Now.ToString("HH-mm-ss"); 
        }

        /// <summary>
        /// The most recently created bugtrack folder
        /// </summary>
        public DirectoryInfo NewestBugtrackerFolder
        {
            get
            {
                if (BugtrackerFolders.Count > 0)
                    return BugtrackerFolders[^1];

                BugtrackerFolders.Add(BugtrackerUtils.CreateBugtrackFolder());
                return NewestBugtrackerFolder;

            }

            set => BugtrackerFolders.Add(value);
        }

        /// <summary>
        /// Returns the name of the most recent bugtrack folder
        /// </summary>
        public string BugtrackerFolderName => NewestBugtrackerFolder.Name;


        /// <summary>
        /// Default constructor of Running Configuration, use the InitStartupProcedure Method to begin
        /// </summary>
        public RunningConfiguration()
        {
            //first, initialize pcinfo object - collects usefull pc info
            PcInfo                = new PCInfo();
            //second, initialize variable manager - load in all variables that could be used in the configuration
            Variables             = new VariableManager(this, PcInfo); // remove ,PcInfo as argument
            //after initializing data that can be used in the configuration files, load in all config files and replace all placeholders with values stored in variables
            Configurations        = new ConfigurationManager(this);
        }

        public void InitStartupProcedure()
        {
            //Initialize logging before anything else so config loading is logged
            Logger.InitializeLogging();

            //Load logging severity from config (if present), defaults to Info if not configured
            try
            {
                string severityValue = ConfigurationManager.GetStartupValue("loggingSeverity");
                if (int.TryParse(severityValue, out int severityInt))
                {
                    Logger.MinimumSeverity = (LoggingSeverity)severityInt;
                    Logger.Log($"Logging severity set to: {Logger.MinimumSeverity} ({severityInt})", LoggingSeverity.Info);
                }
            }
            catch
            {
                // Attribute not found in config - use default (Info)
                Logger.Log("No logging severity configured, using default: Info (3)", LoggingSeverity.Info);
            }

            //Load file check timeout from config (if present), defaults to 1000ms if not configured
            try
            {
                string timeoutValue = ConfigurationManager.GetStartupValue("fileCheckTimeoutMs");
                if (int.TryParse(timeoutValue, out int timeoutMs) && timeoutMs > 0)
                {
                    FileCheckTimeoutMs = timeoutMs;
                    Logger.Log($"File check timeout set to: {FileCheckTimeoutMs}ms", LoggingSeverity.Info);
                }
                else
                {
                    Logger.Log($"Invalid fileCheckTimeoutMs value, using default: {FileCheckTimeoutMs}ms", LoggingSeverity.Warning);
                }
            }
            catch
            {
                // Attribute not found in config - use default (1000ms)
                Logger.Log($"No file check timeout configured, using default: {FileCheckTimeoutMs}ms", LoggingSeverity.Info);
            }

            //sets server address according to configuration file
            ServerAddress               = Configurations.GetMainServerAddress();
            //set and initializes new server object with server address loaded in from configuration
            MainServer                  = new Server(ServerAddress);

            //variable manager loads in all variables for the first time
            Variables.FullRefresh();
            //sets configuration folder path according to configuration file (optional - may be null/empty)
            ConfigurationFolderPath = ConfigurationManager.GetStartupValue("loadConfigsFrom");
            //after setting the configuraion folder path, refreshes all variables
            Variables.FullRefresh();
            //sets server path according to configuration file
            ServerPath = ConfigurationManager.GetStartupValue("mainserver");
            //after setting the server path, refreshes all variables
            Variables.FullRefresh();
            //sets config webserver URL (optional - may be null/empty)
            ConfigWebserverUrl = ConfigurationManager.GetStartupValue("configWebserverUrl");
            //after setting the web server URL, refreshes all variables
            Variables.FullRefresh();

            //Init new List for BugtrackerFolders
            BugtrackerFolders     = new List<DirectoryInfo>();
            //Initializes all other Manager Objects
            Applications          = new ApplicationManager();
            Targets               = new TargetManager();
            ProblemCategories     = new ProblemManager();

            StartupTime = DateTime.Now;

            Load();
            SetupConnectionStatusTimer();

            InitiliazedRunningConfiguration?.Invoke(null, null);
        }

        private Timer _serverConnectionStatusTimer;
        private readonly ProblemCategory _selectedProblemCategory;

        /// <summary>
        /// Sets up the timer that checks the server connection status.
        /// </summary>
        public void SetupConnectionStatusTimer()
        {
            _serverConnectionStatusTimer             = new Timer();
            _serverConnectionStatusTimer.Tick        += new EventHandler(CheckServerConnectionStatusAsync);
            _serverConnectionStatusTimer.Interval    = 10000;
            _serverConnectionStatusTimer.Start();
        }

        public event CheckServerConnectionCompletedEventHandler CheckServerConnectionCompleted;

        public delegate void CheckServerConnectionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <summary>
        /// used by the timer to check the server connection status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckServerConnectionStatusAsync(object sender, EventArgs e)
        {
            switch (MainServer.ServerStatus)
            {
                case ServerStatus.NotAvailable:
                    ServerStatus = ServerStatus.NotAvailable;
                    break;
                default:
                    ServerLastConnectionTime = DateTime.Now;
                    ServerStatus = ServerStatus.Available;
                    break;
            }
        }

        /// <summary>
        /// Loads all configuration files with priority: Web > SMB > Local
        /// </summary>
        private void Load()
        {
            string[] configPaths = null;
            bool loadedSuccessfully = false;

            // Priority 1: Try downloading from web server (if configured)
            if (!string.IsNullOrEmpty(ConfigWebserverUrl) && Uri.TryCreate(ConfigWebserverUrl, UriKind.Absolute, out Uri webUri) && (webUri.Scheme == Uri.UriSchemeHttp || webUri.Scheme == Uri.UriSchemeHttps))
            {
                Logger.Log($"Attempting to download config from web server: {ConfigWebserverUrl}", LoggingSeverity.Info);

                if (TryDownloadConfigFromWeb(ConfigWebserverUrl))
                {
                    configPaths = Directory.GetFiles(Globals.INTERNAL_CONFIG_FOLDER_PATH, "*.xml");
                    ConfigSource = ConfigSource.Webserver;
                    loadedSuccessfully = true;
                    Logger.Log("Successfully downloaded config from web server", LoggingSeverity.Info);
                }
                else
                {
                    Logger.Log("Web download failed, trying SMB share or local fallback", LoggingSeverity.Warning);
                }
            }
            else if (!string.IsNullOrEmpty(ConfigWebserverUrl))
            {
                Logger.Log($"Invalid web server URL configured: '{ConfigWebserverUrl}', skipping web download", LoggingSeverity.Warning);
            }

            // Priority 2: Try SMB share (only if web download didn't succeed and path is configured)
            if (!loadedSuccessfully && !string.IsNullOrEmpty(ConfigurationFolderPath) && Directory.Exists(ConfigurationFolderPath))
            {
                configPaths = Directory.GetFiles(ConfigurationFolderPath, "*.xml");
                ConfigSource = ConfigSource.Server;
                loadedSuccessfully = true;

                // Copy to internal folder for future fallback
                foreach (var filePath in configPaths)
                {
                    try
                    {
                        File.Copy(
                            Path.Join(ConfigurationFolderPath, Path.GetFileName(filePath)),
                            Path.Join(Globals.INTERNAL_CONFIG_FOLDER_PATH, Path.GetFileName(filePath)),
                            true
                        );
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to copy config file {Path.GetFileName(filePath)}: {ex.Message}", LoggingSeverity.Warning);
                    }
                }
                Logger.Log("Loaded config from SMB share", LoggingSeverity.Info);
            }

            // Priority 3: Fallback to local cached/embedded config
            if (!loadedSuccessfully)
            {
                configPaths = Directory.GetFiles(Globals.INTERNAL_CONFIG_FOLDER_PATH, "*.xml");
                ConfigSource = ConfigSource.Client;
                Logger.Log("Using local fallback config", LoggingSeverity.Warning);
            }

            // Log the final configuration source
            Logger.Log($"Configuration loaded from: {ConfigSource}", LoggingSeverity.Info);

            // Load configurations from determined source
            foreach (var filePath in configPaths)
            {
                LoggerEnabled   = ConfigurationManager.IsLoggingEnabled(filePath);
                LogSeverity     = Configurations.GetLoggingSeverity(filePath);

                Applications.Applications.AddRange(Configurations.GetSpecifiedApplications(filePath));
                Targets.Targets.AddRange(Configurations.GetSpecifiedTargets(filePath));
                ProblemCategories.ProblemCategories.AddRange(Configurations.GetSpecifiedProblemCategories(filePath));
            }
        }

        /// <summary>
        /// Downloads configuration XML from web server and saves to local folder
        /// </summary>
        /// <param name="webserverUrl">Base URL of the Django backend</param>
        /// <returns>True if download succeeded, false otherwise</returns>
        private bool TryDownloadConfigFromWeb(string webserverUrl)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // Build URL with query parameters (hostname and IP)
                    string hostname = PCInfo.Hostname;
                    string ip = PCInfo.IPAddress;
                    string domain = PCInfo.DomainName;

                    string url = $"{webserverUrl.TrimEnd('/')}/config.xml?hostname={Uri.EscapeDataString(hostname)}&ip={Uri.EscapeDataString(ip)}&domain={Uri.EscapeDataString(domain)}";

                    Logger.Log($"Downloading config from: {url}", LoggingSeverity.Info);

                    // Download config.xml
                    var response = client.GetAsync(url).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log($"HTTP request failed with status: {response.StatusCode}", LoggingSeverity.Error);
                        return false;
                    }

                    string xmlContent = response.Content.ReadAsStringAsync().Result;

                    // Validate that we received XML (basic check)
                    if (!xmlContent.TrimStart().StartsWith("<?xml"))
                    {
                        Logger.Log("Response is not valid XML", LoggingSeverity.Error);
                        return false;
                    }

                    // Save to internal config folder
                    string targetPath = Path.Join(
                        Globals.INTERNAL_CONFIG_FOLDER_PATH,
                        "bugtracker_config_main.xml"
                    );

                    File.WriteAllText(targetPath, xmlContent, System.Text.Encoding.UTF8);
                    Logger.Log($"Config saved to: {targetPath}", LoggingSeverity.Info);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to download config from web: {ex.Message}", LoggingSeverity.Error);
                return false;
            }
        }

        /// <summary>
        /// Summary of the RunningConfiguration Object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var returnString = "";

            returnString += "PcInfo: \n \n";
            returnString += PCInfo.Summary() + Environment.NewLine;
            returnString += "Current Bugtracker Folder Name: " + NewestBugtrackerFolder + Environment.NewLine;
            returnString += "Logger Enabled: " + LoggerEnabled + Environment.NewLine;
            returnString += "Log Severity: " + Enum.GetName(typeof(LoggingSeverity), LogSeverity) + Environment.NewLine;
            returnString += "Target Path: " + TargetPath;

            return returnString;
        }

        public List<Targeting.Target> GetApplicableTargets()
        {
            List<Targeting.Target> targets = new List<Targeting.Target>();

            if (SelectedProblemCategory != null)
            {
                if (SelectedProblemCategory.Targets != null && SelectedProblemCategory.Targets.Count > 0)
                {
                    targets.AddRange(SelectedProblemCategory.Targets);
                    Logging.Logger.Log($"Using {targets.Count} problem category specific targets: {string.Join(", ", targets.Select(t => t.Name))}", Logging.LoggingSeverity.Debug);
                }
                else
                {
                    var defaultTargets = Targets.GetDefaultTargets();
                    targets.AddRange(defaultTargets);
                    Logging.Logger.Log($"Using {defaultTargets.Count} default targets: {string.Join(", ", defaultTargets.Select(t => t.Name))}", Logging.LoggingSeverity.Debug);
                }
            } else
            {
                var defaultTargets = Targets.GetDefaultTargets();
                targets.AddRange(defaultTargets);
                Logging.Logger.Log($"No problem category selected, using {defaultTargets.Count} default targets: {string.Join(", ", defaultTargets.Select(t => t.Name))}", Logging.LoggingSeverity.Debug);
            }

            List<Targeting.Target> obligatoryTargets = Targets.GetObligatoryTargets();
            Logging.Logger.Log($"Found {obligatoryTargets.Count} obligatory targets: {string.Join(", ", obligatoryTargets.Select(t => t.Name))}", Logging.LoggingSeverity.Debug);

            int addedObligatory = 0;
            foreach (Targeting.Target ot in obligatoryTargets)
            {
                if (!targets.Any(t => t.Name == ot.Name))
                {
                    targets.Add(ot);
                    addedObligatory++;
                    Logging.Logger.Log($"Added obligatory target: {ot.Name}", Logging.LoggingSeverity.Debug);
                }
                else
                {
                    Logging.Logger.Log($"Obligatory target '{ot.Name}' already in list, skipping", Logging.LoggingSeverity.Debug);
                }
            }

            Logging.Logger.Log($"Final target list ({targets.Count} total): {string.Join(", ", targets.Select(t => t.Name))}", Logging.LoggingSeverity.Info);

            return targets;
        }
    }
}
