using System;

namespace Bugtracker.Targeting
{
    /// <summary>
    /// Attribute to mark properties that should be loaded from XML configuration.
    /// Used by ConfigurationManager to automatically map XML attributes to target properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlConfig : Attribute
    {
        /// <summary>
        /// The XML attribute name to load this property from
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        /// Whether this property is required in the XML configuration
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Whether to apply variable substitution (ReplaceKeywords) to the loaded value
        /// </summary>
        public bool ApplyVariables { get; set; }

        /// <summary>
        /// Creates a new XmlConfig attribute
        /// </summary>
        /// <param name="attributeName">The XML attribute name (e.g., "path", "smtpserver")</param>
        /// <param name="required">Whether this attribute is required (default: false)</param>
        /// <param name="applyVariables">Whether to apply variable substitution (default: true)</param>
        public XmlConfig(string attributeName, bool required = false, bool applyVariables = true)
        {
            AttributeName = attributeName;
            Required = required;
            ApplyVariables = applyVariables;
        }
    }
}
