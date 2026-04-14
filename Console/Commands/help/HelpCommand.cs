using Bugtracker.Attributes;
using Bugtracker.Globals_and_Information;
using static Bugtracker.Console.BugtrackConsole;

namespace Bugtracker.Console.Commands.help
{
    [Command("help", "hlp", "Shows this Message")]
    class HelpCommand : Command
    {
        /// <summary>
        /// Print Help Message and show + describe
        /// available commands
        /// </summary>
        public override string Execute()
        {
            string helpMessage = "Bugtracker v2.1" + Globals.EOL_CHARACTER + Globals.EOL_CHARACTER;

            Logging.Logger.Log("Generating help message with " + BugtrackConsole.commandRegestry.Count + " registered commands", Logging.LoggingSeverity.Debug);

            foreach (var item in BugtrackConsole.commandRegestry)
            {
                helpMessage += ConsoleUtilites.GettHelpMessageCommandDescription(item.Key[0], item.Key[2]);
            }

            return helpMessage;
        }
    }
}
