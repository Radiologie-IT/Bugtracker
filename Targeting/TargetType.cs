using System;

namespace Bugtracker.Targeting
{
    /// <summary>
    /// Attribute to mark target classes with their type identifier for automatic discovery.
    /// The type identifier must match the XML configuration attribute value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TargetType : Attribute
    {
        /// <summary>
        /// The type identifier used in XML configuration (e.g., "folder", "mail", "powershell")
        /// </summary>
        public string TypeIdentifier { get; }

        /// <summary>
        /// Creates a new TargetType attribute
        /// </summary>
        /// <param name="typeIdentifier">The type identifier for this target type</param>
        public TargetType(string typeIdentifier)
        {
            TypeIdentifier = typeIdentifier;
        }
    }
}
