using System;

namespace Bugtracker.Attributes
{
    /// <summary>
    /// Key Attribute. Used to mark properties as keys. Unsophisticated.
    /// </summary>
    internal class KeyAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Dynamic { get; set; }

        public KeyAttribute(string keyName, bool keyDynamic = false)
        {
            Name = keyName;
            Dynamic = keyDynamic;
        }
    }
}