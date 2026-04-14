using Bugtracker.Attributes;
using Bugtracker.Configuration;
using Bugtracker.Globals_and_Information;
using Bugtracker.Plugin;
using System.Text;

namespace Bugtracker.Console.Commands.pcinfo
{
    [Command("pcinfo", "pcinf", "Shows PC information (ip, mac, hostname, domain, user)")]
    class PcInfoCommand : Command
    {
        public override string Execute()
        {
            PCInfo pcinfo = new PCInfo();
            return PCInfo.Summary() + Globals.EOL_CHARACTER;
        }
    }

    [Command("variables", "vars", "Shows Bugtracker variable substitution values", typeof(PcInfoCommand))]
    class PcInfoVariablesCommand : Command
    {
        public override string Execute()
        {
            var variables = RunningConfiguration.GetInstance().Variables.VariableDictionary;
            var bugtrackerVars = new System.Collections.Generic.Dictionary<string, (dynamic value, bool isDynamic)>();
            int envVarCount = 0;

            foreach (var kv in variables)
            {
                // Bugtracker [Key] variables always start with a lowercase letter (camelCase).
                // Windows environment variables are typically ALL_CAPS or PascalCase.
                bool isBugtrackerVar = !string.IsNullOrEmpty(kv.Key) && char.IsLower(kv.Key[0]);
                if (isBugtrackerVar)
                    bugtrackerVars[kv.Key] = kv.Value;
                else
                    envVarCount++;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Bugtracker variables ({bugtrackerVars.Count}):");
            sb.AppendLine();

            foreach (var kv in bugtrackerVars)
            {
                string dynamic = kv.Value.isDynamic ? "  (dynamic)" : "";
                sb.AppendLine($"  %{kv.Key}%{dynamic}");
                sb.AppendLine($"    = {kv.Value.value}");
            }

            sb.AppendLine();
            sb.AppendLine($"  + {envVarCount} environment variables also usable as %VAR_NAME%");
            sb.AppendLine($"    Use 'pcinfo variables all' to list them.");

            return sb.ToString();
        }
    }

    [Command("all", "all", "Shows all variables including environment variables", typeof(PcInfoVariablesCommand))]
    class PcInfoAllVariablesCommand : Command
    {
        public override string Execute()
        {
            var variables = RunningConfiguration.GetInstance().Variables.VariableDictionary;
            var sb = new StringBuilder();
            sb.AppendLine($"All variables ({variables.Count}):");
            sb.AppendLine();

            foreach (var kv in variables)
            {
                string dynamic = kv.Value.isDynamic ? "  (dynamic)" : "";
                sb.AppendLine($"  %{kv.Key}%{dynamic} = {kv.Value.value}");
            }

            return sb.ToString();
        }
    }

    [Command("plugins", "plug", "Lists all currently loaded plugins", typeof(PcInfoCommand))]
    class PcInfoPluginsCommand : Command
    {
        public override string Execute()
        {
            var plugins = RunningConfiguration.GetInstance().LoadedPlugins;

            if (plugins.Count == 0)
                return "No plugins loaded.";

            var sb = new StringBuilder();
            sb.AppendLine($"Loaded plugins ({plugins.Count}):");
            sb.AppendLine();

            foreach (IPlugin plugin in plugins)
            {
                sb.AppendLine($"  {plugin.Name} v{plugin.Version}");
                sb.AppendLine($"    Author: {plugin.Author}");
            }

            return sb.ToString();
        }
    }
}
