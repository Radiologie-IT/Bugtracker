using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Bugtracker.Globals_and_Information;
using Bugtracker.InternalApplication;
using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using Bugtracker.Targeting;
using Bugtracker.Utils;
using Bugtracker.Variables;
using static Bugtracker.Logging.Log;

namespace Bugtracker.Configuration
{
    /// <summary>
    /// This class is only here to handle all the XML-Magic.... Sadly XML :(
    /// </summary>
    public class ConfigurationManager
    {
        private readonly RunningConfiguration rc;

        /// <summary>
        /// Default Constructor,
        /// does nothing
        /// </summary>
        public ConfigurationManager(RunningConfiguration runningConfiguration)
        {
            rc = runningConfiguration;
        }

        /// <summary>
        /// Returns the main server address specified in the startup configuration
        /// </summary>
        /// <param name="customConfigPath"></param>
        /// <returns></returns>
        public string GetMainServerAddress(
            string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();


            VariableManager vm = rc.Variables;

            string serverAddress = "";

            using(XmlReader reader = XmlReader.Create(customConfigPath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName.Equals("startup"))
                            serverAddress = vm.ReplaceKeywords(reader.GetAttribute("mainserver"));
                    }
                }
            }

            return serverAddress;
        }

        /// <summary>
        /// Function returns the value used for checking if this is the first start of the application.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="customConfigPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static dynamic GetStartupValue(string attribute, string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            using (XmlReader reader = XmlReader.Create(customConfigPath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName.Equals("startup"))
                        {
                            //try parsing into boolean
                            if (Boolean.TryParse(reader.GetAttribute(attribute), out bool startupValue))
                            {
                                return startupValue;
                            } // if it fails, return string
                            else
                            {
                                return reader.GetAttribute(attribute);
                            }
                        }
                    }
                }
            }

            throw new Exception("Didn't find attribute in startup configuration.");
        }

        /// <summary>
        /// Function overwrite every attribute in the startup configuration with the running configurations values.
        /// </summary>
        /// <param name="customConfigPath"></param>
        /// <param name="settings"></param>
        internal static void OverwriteStartupConfig(string customConfigPath = null, params (string attribute, string value)[] settings)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            XmlDocument xmlDocument = new();

            xmlDocument.Load(customConfigPath);

            if (xmlDocument.SelectSingleNode("configuration/startup") is XmlElement node)
            {
                foreach (var (attribute, value) in settings)
                {
                    node.SetAttribute(attribute, value);
                }
            }

            xmlDocument.Save(customConfigPath);
        }

        /// <summary>
        /// Returns a List of applications specified in the logfile
        /// Parameters are loglocation type, path, filename (regex), find (per timeperiod)
        /// </summary>
        /// <returns></returns>
        public List<Application> GetSpecifiedApplications(
            string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            VariableManager vm = rc.Variables;

            List<Application> applications = new();

            // start reading autostart.config.xml
            using (XmlReader reader = XmlReader.Create(customConfigPath))
            {
                Application currentApplication = null;

                IXmlLineInfo lineInfo = (IXmlLineInfo)reader;
                int line = lineInfo.LineNumber;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName.Equals("application"))
                        {
                            Application appToAdd = new();

                            appToAdd.Name = vm.ReplaceKeywords(reader.GetAttribute("name"));
                            appToAdd.ExecutableLocation = vm.ReplaceKeywords(reader.GetAttribute("executable"));
                            appToAdd.IsStandard = Convert.ToBoolean(vm.ReplaceKeywords(reader.GetAttribute("standard")));

                            Enum.TryParse(vm.ReplaceKeywords(reader.GetAttribute("show")), out Application.ShowAppSpecifier show);

                            appToAdd.ShowSpecifier = show;

                            currentApplication = appToAdd;

                            applications.Add(appToAdd);
                        }


                        if (reader.Name.Equals("log"))
                        {
                            Log logToAppend = new();

                            if (Enum.TryParse<LogLocationType>(vm.ReplaceKeywords(reader.GetAttribute("location")), out LogLocationType type))
                                logToAppend.LocationType = type;

                            logToAppend.Path = vm.ReplaceKeywords(reader.GetAttribute("path"));
                            logToAppend.Filename = vm.ReplaceKeywords(reader.GetAttribute("filename"));

                            if (Enum.TryParse<LogFindSpecifier>(vm.ReplaceKeywords(reader.GetAttribute("find")), out LogFindSpecifier findSpec))
                                logToAppend.Find = findSpec;

                            if (logToAppend.Find == LogFindSpecifier.AGE)
                            {
                                string minAgeAttr = reader.GetAttribute("minage");
                                if (!string.IsNullOrEmpty(minAgeAttr) && int.TryParse(minAgeAttr, out int minAge))
                                {
                                    logToAppend.MinAge = minAge;
                                }
                                else
                                {
                                    logToAppend.MinAge = 0;
                                }

                                string maxAgeAttr = reader.GetAttribute("maxage");
                                if (!string.IsNullOrEmpty(maxAgeAttr) && int.TryParse(maxAgeAttr, out int maxAge))
                                {
                                    logToAppend.MaxAge = maxAge;
                                }
                                else
                                {
                                    logToAppend.MaxAge = 60;
                                }
                            }

                            logToAppend.Lines = reader.GetAttribute("lines");

                            string lastLinesAttr = reader.GetAttribute("lastlines");
                            if (!string.IsNullOrEmpty(lastLinesAttr) && int.TryParse(lastLinesAttr, out int lastLines))
                            {
                                logToAppend.LineCount = lastLines;
                            }
                            

                            if (currentApplication != null)
                                currentApplication.LogFiles.Add(logToAppend);
                        }

                        if (reader.Name.Equals("pre-fetch"))
                        {
                            if (currentApplication != null)
                                currentApplication.PreFetchExecutionPath = vm.ReplaceKeywords(reader.GetAttribute("path"));
                        }

                        if (reader.Name.Equals("post-fetch"))
                        {
                            if (currentApplication != null)
                                currentApplication.PostFetchExecutionPath = vm.ReplaceKeywords(reader.GetAttribute("path"));
                        }

                        if (reader.Name.Equals("powershell"))
                        {
                            try
                            {
                                PowershellUtils.PowershellExecution psex = new PowershellUtils.PowershellExecution(vm.ReplaceKeywords(reader.GetAttribute("path")));

                                // Load optional downloadLink and saveAs attributes
                                string downloadLink = reader.GetAttribute("downloadLink");
                                if (!string.IsNullOrWhiteSpace(downloadLink))
                                {
                                    psex.DownloadLink = vm.ReplaceKeywords(downloadLink);
                                }

                                string saveAs = reader.GetAttribute("saveAs");
                                if (!string.IsNullOrWhiteSpace(saveAs))
                                {
                                    psex.SaveAs = vm.ReplaceKeywords(saveAs);
                                }

                                //multiple try catch, as powershell target has default values for all config elements
                                //multiple try catch, as powershell target has default values for all config elements
                                try
                                {
                                    string attr_passvariables = reader.GetAttribute("passvariables");
                                    psex.PassVariables = bool.Parse(vm.ReplaceKeywords(attr_passvariables));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load passvariables attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_passfolders = reader.GetAttribute("passfolders");
                                    psex.PassFolders = bool.Parse(vm.ReplaceKeywords(attr_passfolders));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load passfolders attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_passproblemcat = reader.GetAttribute("passproblemcat");
                                    psex.PassProblemCategory = bool.Parse(vm.ReplaceKeywords(attr_passproblemcat));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load passproblemcat attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_logdefault = reader.GetAttribute("logdefault");
                                    psex.LogDefault = bool.Parse(vm.ReplaceKeywords(attr_logdefault));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load logdefault attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_logerrors = reader.GetAttribute("logerrors");
                                    psex.LogErrors = bool.Parse(vm.ReplaceKeywords(attr_logerrors));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load logerrors attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_logwarnings = reader.GetAttribute("logwarnings");
                                    psex.LogWarnings = bool.Parse(vm.ReplaceKeywords(attr_logwarnings));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load logwarnings attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_loginformations = reader.GetAttribute("loginformations");
                                    psex.LogInformations = bool.Parse(vm.ReplaceKeywords(attr_loginformations));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load loginformations attribute, using default", LoggingSeverity.Info);
                                }

                                try
                                {
                                    string attr_logprogresss = reader.GetAttribute("logprogress");
                                    psex.LogProgress = bool.Parse(vm.ReplaceKeywords(attr_logprogresss));
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("couldn't load logprogress attribute, using default", LoggingSeverity.Info);
                                }

                                if (reader.GetAttribute("execution").ToString().Equals("pre-fetch"))
                                {
                                    currentApplication.PowershellPre.Add(psex);
                                }
                                else if (reader.GetAttribute("execution").ToString().Equals("post-fetch"))
                                {
                                    currentApplication.PowershellPost.Add(psex);
                                }

                            }
                            catch { 
                                Logger.Log("Missing path attribute for powershell capture. Ignoring the capture.", LoggingSeverity.Warning); 
                            }

                           
                        }
                    }
                }
            }

            return applications;
        }

        /// <summary>
        /// Gets the specified problem categories from the startup configuration.
        /// </summary>
        /// <param name="customConfigPath"></param>
        /// <returns></returns>
        public List<ProblemCategory> GetSpecifiedProblemCategories(
            string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            VariableManager vm = rc.Variables;
            TargetManager tm = rc.Targets;

            List<ProblemCategory> problemCategories = new();

            // start reading autostart.config.xml
            using (XmlReader reader = XmlReader.Create(customConfigPath))
            {
                ProblemCategory currentProblemCategory = null;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName.Equals("problem-category"))
                        {
                            ProblemCategory categoryToAdd = new();
                            categoryToAdd.Name = vm.ReplaceKeywords(reader.GetAttribute("name"));
                            categoryToAdd.TicketAbbreviation = vm.ReplaceKeywords(reader.GetAttribute("ticket"));

                            currentProblemCategory = categoryToAdd;
                            problemCategories.Add(categoryToAdd);
                        }

                        if (reader.Name.Equals("description"))
                        {
                            string descriptorText = vm.ReplaceKeywords(reader.GetAttribute("text"));

                            if (currentProblemCategory != null)
                                currentProblemCategory.Descriptions.Add(descriptorText);
                        }

                        if(reader.Name.Equals("app-selection"))
                        {
                            string selection = reader.ReadElementContentAsString();
                            Logger.Log("selection text: " + selection, LoggingSeverity.Info);
                            string[] splitSelect = selection.Split(',');

                            string configurationPath = Globals.GetFittingConfigFilesPath();

                            if (currentProblemCategory != null)
                            {
                                foreach (string s in splitSelect)
                                {
                                    // Trim whitespace from application name
                                    string appName = s.Trim();

                                    if (appName.Equals("All"))
                                        currentProblemCategory.SelectAllApplications = true;

                                    if (appName.Equals("Screen"))
                                        currentProblemCategory.SelectScreenshot = true;

                                    if (!appName.Equals("All") && !appName.Equals("Screen") && !appName.Equals(""))
                                    {
                                        // Use already-determined configurationPath (from line 389)
                                        if (!string.IsNullOrEmpty(configurationPath) && Directory.Exists(configurationPath))
                                        {
                                            foreach (string path in Directory.GetFiles(configurationPath, "*.xml"))
                                            {
                                                foreach (Application a in this.GetSpecifiedApplications(path))
                                                {
                                                    if (a.Name == appName)
                                                    {
                                                        currentProblemCategory.SelectedApplications.Add(a);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Logger.Log($"Configuration path is invalid or doesn't exist: {configurationPath}", LoggingSeverity.Warning);
                                        }
                                    }
                                }

                                string appNames = string.Join(", ", currentProblemCategory.SelectedApplications.Select(a => a.Name));
                                Logger.Log("Content of " + currentProblemCategory.Name + " : " + appNames, LoggingSeverity.Info);
                                Logger.Log("Screenshot selection: " + currentProblemCategory.SelectScreenshot, LoggingSeverity.Info);
                                Logger.Log("Alle apps selected " + currentProblemCategory.SelectScreenshot, LoggingSeverity.Info);
                            }
                        }

                        if(reader.Name.Equals("target"))
                        {
                            string targetName = vm.ReplaceKeywords(reader.GetAttribute("name"));

                            if(currentProblemCategory != null)
                            {
                                if (tm.GetTargetByName(targetName) != null)
                                {
                                    currentProblemCategory.Targets.Add(tm.GetTargetByName(targetName));
                                }
                            }
                        }
                    }
                }
            }

            return problemCategories;
        }

        /// <summary>
        /// Get the specified targets from the startup configuration.
        /// Uses reflection-based discovery and property mapping for extensibility.
        /// </summary>
        /// <param name="customConfigPath"></param>
        /// <returns></returns>
        public List<Target> GetSpecifiedTargets(
            string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            VariableManager vm = rc.Variables;

            List<Target> targets = new();

            // Ensure TargetFactory is initialized
            TargetFactory.Initialize();

            // start reading config file
            using (XmlReader reader = XmlReader.Create(customConfigPath))
            {
                bool inTargetsSection = false;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // Track when we enter the <targets> section
                        if (reader.LocalName.Equals("targets"))
                        {
                            inTargetsSection = true;
                            continue;
                        }

                        // Only process <target> elements that are within the <targets> section
                        if (reader.LocalName.Equals("target") && inTargetsSection)
                        {
                            string targetType = vm.ReplaceKeywords(reader.GetAttribute("type"));

                            if (string.IsNullOrEmpty(targetType))
                            {
                                Logger.Log("Found target without 'type' attribute in targets section, skipping", LoggingSeverity.Warning);
                                continue;
                            }

                        try
                        {
                            // Create target instance using factory
                            Target target = TargetFactory.CreateTarget(targetType);

                            // Load common properties
                            target.Name = vm.ReplaceKeywords(reader.GetAttribute("name"));

                            bool defaultT = false;
                            if (reader.GetAttribute("default") != null)
                                bool.TryParse(vm.ReplaceKeywords(reader.GetAttribute("default")), out defaultT);
                            target.Default = defaultT;

                            bool obligatoryT = false;
                            if (reader.GetAttribute("obligatory") != null)
                                bool.TryParse(vm.ReplaceKeywords(reader.GetAttribute("obligatory")), out obligatoryT);
                            target.Obligatory = obligatoryT;

                            // Load target-specific properties using reflection
                            LoadTargetPropertiesFromXml(target, reader, vm);

                            // Validate configuration
                            if (!target.ValidateConfiguration(out string errorMessage))
                            {
                                Logger.Log($"Target '{target.Name}' validation failed: {errorMessage}", LoggingSeverity.Error);
                                continue;
                            }

                            targets.Add(target);
                            Logger.Log($"Loaded target: {target.Name} (type: {targetType}, default: {target.Default}, obligatory: {target.Obligatory})", LoggingSeverity.Info);
                        }
                        catch (ArgumentException ex)
                        {
                            Logger.Log($"Unknown target type '{targetType}': {ex.Message}", LoggingSeverity.Error);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Failed to load target: {ex.Message}", LoggingSeverity.Error);
                        }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        // Track when we exit the <targets> section
                        if (reader.LocalName.Equals("targets"))
                        {
                            inTargetsSection = false;
                        }
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// Load target properties from XML using reflection and XmlConfig attributes
        /// </summary>
        private void LoadTargetPropertiesFromXml(Target target, XmlReader reader, VariableManager vm)
        {
            // Get all properties with XmlConfig attribute
            var properties = target.GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<XmlConfig>() != null)
                .ToList();

            foreach (var property in properties)
            {
                var xmlConfig = property.GetCustomAttribute<XmlConfig>();
                string xmlValue = reader.GetAttribute(xmlConfig.AttributeName);

                // Check if required attribute is missing
                if (string.IsNullOrEmpty(xmlValue) && xmlConfig.Required)
                {
                    Logger.Log($"Missing required attribute '{xmlConfig.AttributeName}' for target '{target.Name}'", LoggingSeverity.Warning);
                    continue;
                }

                // Skip if attribute not present and not required
                if (xmlValue == null)
                    continue;

                try
                {
                    // Apply variable substitution if enabled
                    if (xmlConfig.ApplyVariables)
                        xmlValue = vm.ReplaceKeywords(xmlValue);

                    // Convert and set property value
                    object convertedValue = ConvertValue(xmlValue, property.PropertyType);
                    property.SetValue(target, convertedValue);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to set property '{property.Name}' on target '{target.Name}': {ex.Message}", LoggingSeverity.Warning);
                }
            }
        }

        /// <summary>
        /// Convert a string value to the target property type
        /// </summary>
        private object ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(bool))
                return bool.Parse(value);

            if (targetType == typeof(long))
                return long.Parse(value);

            if (targetType == typeof(double))
                return double.Parse(value);

            // Add more type conversions as needed
            return Convert.ChangeType(value, targetType);
        }

        /// <summary>
        /// Get Logging Severity from the startup configuration.
        /// </summary>
        /// <param name="customConfigPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public LoggingSeverity GetLoggingSeverity(
            string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            VariableManager vm = rc.Variables;

            using (XmlReader reader = XmlReader.Create(customConfigPath))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name.Equals("logger"))
                        {
                            switch (vm.ReplaceKeywords(reader.GetAttribute("severity")))
                            {
                                case "1":
                                    return LoggingSeverity.Error;

                                case "2:":
                                    return LoggingSeverity.Warning;

                                case "3":
                                    return LoggingSeverity.Info;
                                default:
                                    //TODO: Write Exception for error in logging severity.
                                    System.Diagnostics.Debug.Write("Severity in config not valid.");
                                    throw new Exception("Logging Severity not correctly defined!");
                            }
                        }
                    }
                }
            }

            return LoggingSeverity.Null;
        }

        /// <summary>
        /// To log, or not to log.
        /// Checks if logging is enabled via the logger - enabled xml tag and attribute
        /// </summary>
        /// <returns></returns>
        public static bool IsLoggingEnabled(
            string customConfigPath = null)
        {
            if (customConfigPath == null)
                customConfigPath = Globals_and_Information.Globals.GetFittingStartupConfigPath();

            // value of target aka site/ip to be pinged
            bool log = false;

            // start reading autostart.config.xml
            using (XmlReader reader = XmlReader.Create(customConfigPath))
            {

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name.Equals("logger"))
                        {

                            if (reader.GetAttribute("enabled").Equals("true"))
                            {
                                System.Diagnostics.Debug.Write("Logger is enabled!");
                                log = true;
                                return log;
                            }
                        }
                    }
                }
            }

            return log;
        }

    }

}
