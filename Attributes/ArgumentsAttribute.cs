using System;

namespace Bugtracker.Attributes
{
    /// <summary>
    /// Arguments Attribute. Used to describe arguments for methods and classes as commands.
    /// </summary>
    internal class ArgumentsAttribute : Attribute
    {
        public ArgumentsAttribute(string[] reqArguments)
        {
            RequiredParams = reqArguments;
        }
        public ArgumentsAttribute(string[] reqArguments, string[] optArguments)
        {
            RequiredParams = reqArguments;
            OptionalParams = optArguments;
        }

        public string[] RequiredParams { get; set; }
        public string[] OptionalParams { get; set; }
    }
}