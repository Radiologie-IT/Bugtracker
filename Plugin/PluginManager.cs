using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Bugtracker.Configuration;
using Bugtracker.Logging;

namespace Bugtracker.Plugin
{
    /// <summary>
    /// Very simple Plugin Manager. Loads all Plugins from the Plugin Folder.
    /// </summary>
    public static class PluginManager
    {
        public static void Load()
        {
            List<string> pluginFiles = new List<string>();

            pluginFiles.AddRange(Directory.GetFiles(Globals_and_Information.Globals.GetFittingPluginFilesPath(), "*.dll"));

            // Pass 1: instantiate and register all plugins before activating any.
            // This ensures LoadedPlugins is fully populated regardless of load order,
            // even if a plugin's OnLoad blocks indefinitely (e.g. Application.Run).
            foreach (String pluginFile in pluginFiles)
            {
                Logger.Log("Trying to Load Plugin: " + pluginFile, LoggingSeverity.Info);

                try
                {
                    Assembly asm = Assembly.LoadFrom(pluginFile);

                    if (asm != null)
                    {
                        var type = typeof(IPlugin);
                        var types = asm.GetTypes().Where(p => type.IsAssignableFrom(p));

                        foreach (var plugin in types)
                        {
                            IPlugin ipl = (IPlugin)Activator.CreateInstance(plugin);
                            RunningConfiguration.GetInstance().LoadedPlugins.Add(ipl);
                            Logger.Log("Loaded Plugin " + plugin, LoggingSeverity.Info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error while loading plugin: " + ex.ToString(), Logging.LoggingSeverity.Error);
                }
            }

            // Pass 2: activate all registered plugins.
            foreach (IPlugin plugin in RunningConfiguration.GetInstance().LoadedPlugins)
            {
                try
                {
                    MethodInfo method = plugin.GetType().GetMethod("OnLoad");
                    method.Invoke(plugin, null);
                }
                catch (Exception ex)
                {
                    Logger.Log("Error while activating plugin " + plugin.Name + ": " + ex.ToString(), Logging.LoggingSeverity.Error);
                }
            }
        }
    }
}
