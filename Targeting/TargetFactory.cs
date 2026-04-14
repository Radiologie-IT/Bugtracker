using Bugtracker.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Bugtracker.Logging.Log;

namespace Bugtracker.Targeting
{
    /// <summary>
    /// Factory for creating target instances using reflection-based discovery.
    /// Automatically discovers all target types marked with [TargetType] attribute.
    /// </summary>
    public static class TargetFactory
    {
        private static Dictionary<string, Type> _targetTypes = new Dictionary<string, Type>();
        private static bool _initialized = false;

        /// <summary>
        /// Initialize the factory by discovering all target types in the assembly.
        /// This should be called once at application startup.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            Logger.Log("Initializing TargetFactory - discovering target types...", LoggingSeverity.Info);

            _targetTypes.Clear();

            // Find all types in current assembly that inherit from Target and have TargetType attribute
            var assembly = Assembly.GetExecutingAssembly();
            var targetTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Target)))
                .Where(t => t.GetCustomAttribute<TargetType>() != null);

            foreach (var type in targetTypes)
            {
                var attr = type.GetCustomAttribute<TargetType>();
                _targetTypes[attr.TypeIdentifier] = type;
                Logger.Log($"Registered target type: {attr.TypeIdentifier} -> {type.Name}", LoggingSeverity.Info);
            }

            _initialized = true;
            Logger.Log($"TargetFactory initialized with {_targetTypes.Count} target type(s)", LoggingSeverity.Info);
        }

        /// <summary>
        /// Create a target instance by type identifier
        /// </summary>
        /// <param name="typeIdentifier">The type identifier (e.g., "folder", "mail", "powershell")</param>
        /// <returns>A new instance of the target type</returns>
        /// <exception cref="ArgumentException">If the type identifier is not registered</exception>
        public static Target CreateTarget(string typeIdentifier)
        {
            if (!_initialized)
                Initialize();

            if (string.IsNullOrEmpty(typeIdentifier))
                throw new ArgumentException("Type identifier cannot be null or empty");

            if (!_targetTypes.TryGetValue(typeIdentifier, out Type targetType))
            {
                string availableTypes = string.Join(", ", _targetTypes.Keys);
                throw new ArgumentException($"Unknown target type: '{typeIdentifier}'. Available types: {availableTypes}");
            }

            try
            {
                var target = (Target)Activator.CreateInstance(targetType);
                Logger.Log($"Created target instance: {typeIdentifier}", LoggingSeverity.Info);
                return target;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create target of type '{typeIdentifier}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get all registered target type identifiers
        /// </summary>
        /// <returns>Collection of type identifiers</returns>
        public static IEnumerable<string> GetRegisteredTypes()
        {
            if (!_initialized)
                Initialize();

            return _targetTypes.Keys;
        }

        /// <summary>
        /// Check if a target type is registered
        /// </summary>
        /// <param name="typeIdentifier">The type identifier to check</param>
        /// <returns>True if the type is registered, false otherwise</returns>
        public static bool IsTypeRegistered(string typeIdentifier)
        {
            if (!_initialized)
                Initialize();

            return _targetTypes.ContainsKey(typeIdentifier);
        }
    }
}
