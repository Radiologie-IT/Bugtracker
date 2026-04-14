using System;

namespace Bugtracker.Attributes
{
    /// <summary>
    /// Command Attribute. Used to mark methods and classes as commands.
    /// </summary>
    internal class CommandAttribute : Attribute
    {
        public CommandAttribute(string commandName)
        {
            CommandName = commandName;
            ParentCommand = null;
        }

        public CommandAttribute(string commandName, string commandAlias)
        {
            CommandName = commandName;
            CommandAlias = commandAlias;
            ParentCommand = null;
        }

        public CommandAttribute(string commandName, string commandAlias, string commandHelpMessage)
        {
            CommandName = commandName;
            CommandAlias = commandAlias;
            CommandHelpMessage = commandHelpMessage;
            ParentCommand = null;
        }

        public CommandAttribute(string commandName, string commandHelpMessage, Type parentCommand)
        {
            CommandName = commandName;
            CommandHelpMessage = commandHelpMessage;
            ParentCommand = parentCommand;
        }

        public CommandAttribute(string commandName, string commandAlias, string commandHelpMessage, Type parentCommand)
        {
            CommandName = commandName;
            CommandAlias = commandAlias;
            CommandHelpMessage = commandHelpMessage;
            ParentCommand = parentCommand;
        }

        public string CommandName { get; set; }
        public string CommandAlias { get; set; }
        public string CommandHelpMessage { get; set; }
        public Type ParentCommand { get; set; }
    }
}