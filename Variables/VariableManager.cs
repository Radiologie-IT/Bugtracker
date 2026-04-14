using Bugtracker.Attributes;
using Bugtracker.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Win32;
using Bugtracker.Logging;

namespace Bugtracker.Variables
{
    public class VariableManager
    {
        public Dictionary<string, (dynamic value, bool isDynamic)> VariableDictionary { get; set; } = new Dictionary<string, (dynamic value, bool isDynamic)>();
        //private RunningConfiguration rc;
        private readonly Object[] toLoadFrom = Array.Empty<object>(); 

        /// <summary>
        /// Loads 
        /// </summary>
        /// <param name="runningConfiguration"></param>
        public VariableManager(params Object[] toLoadFrom)
        {
            this.toLoadFrom = toLoadFrom;
            //FullRefresh();
        }

        private void SetCustomKeyValues()
        {

        }

        private void LoadInAllEnvironmentVariables()
        {
            foreach (DictionaryEntry dictEntry in Environment.GetEnvironmentVariables())
            {
                //Logger.Log("Getting Environment Variable" + dictEntry.Key + ":" + dictEntry.Value, LoggingSeverity.Info);
                VariableDictionary[(string)dictEntry.Key] = (dictEntry.Value, false);
            }
        }

        private void ReloadSpecificAnnotatedKeyValuePair(Object objectToLoadFrom, string key)
        {
            foreach (PropertyInfo propertyInfo in objectToLoadFrom.GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0)
                {
                    KeyAttribute ka = (KeyAttribute)propertyInfo.GetCustomAttribute(typeof(KeyAttribute), true);

                    if (ka.Name == key)
                    {
                        if (ka.Dynamic)
                            VariableDictionary[ka.Name] = (propertyInfo.GetValue(objectToLoadFrom) ?? "not set.", true);
                        else
                            VariableDictionary[ka.Name] = (propertyInfo.GetValue(objectToLoadFrom) ?? "not set.", false);
                    }
                }
            }
        }

        private void LoadAllAnnotatedKeyValuePairs(Object[] objectsToLoadFrom)
        {
            foreach(Object obj in objectsToLoadFrom)
            {
                foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
                {
                    if (propertyInfo.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0)
                    {
                        KeyAttribute ka = (KeyAttribute)propertyInfo.GetCustomAttribute(typeof(KeyAttribute), true);

                        if (ka.Dynamic)
                            VariableDictionary[ka.Name] = (propertyInfo.GetValue(obj) ?? "not set.", true);
                        else
                            VariableDictionary[ka.Name] = (propertyInfo.GetValue(obj) ?? "not set.", false);
                    }
                }
            }
        }

        private void SetValuesAccordingToVariables(Object[] objectsToLoadFrom)
        {
            foreach(Object obj in objectsToLoadFrom)
            {
                foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
                {
                    if (propertyInfo.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0)
                    {
                        KeyAttribute ka = (KeyAttribute)propertyInfo.GetCustomAttribute(typeof(KeyAttribute), true);

                        foreach (var keyValuePair in VariableDictionary)
                        {
                            if (keyValuePair.Key == ka.Name)
                            {
                                if(propertyInfo.CanWrite)
                                {
                                    propertyInfo.SetValue(obj, keyValuePair.Value.value);
                                }
                            }
                                
                        }
                    }
                }
            }
            
        }


        private void PrintKeyValues()
        {
            foreach (var keyValuePair in VariableDictionary)
            {
                Logging.Logger.Log("Variable - Key: '" + keyValuePair.Key + "', Value: '" + keyValuePair.Value.value + "', IsDynamic: " + keyValuePair.Value.isDynamic, Logging.LoggingSeverity.Debug);
            }
        }

        private void ReplaceKeysInValuesTillKeyless()
        {
            var keysAndNewValues = new List<(string Key, string NewValue)>();

            while (DoValuesContainKeys())
            {
                foreach (var keyValuePairX in VariableDictionary)
                {
                    foreach (var keyValuePairY in VariableDictionary)
                    {
                        if (keyValuePairX.ToString().Contains("%" + keyValuePairY.Key + "%"))
                        {
                            string newString = ((string) keyValuePairX.Value.value).Replace("%" + keyValuePairY.Key + "%",
                                keyValuePairY.Value.value);

                            keysAndNewValues.Add((keyValuePairX.Key, newString));
                        }
                    }
                }

                foreach (var (Key, NewValue) in keysAndNewValues)
                {
                    VariableDictionary[Key] = (NewValue, false);
                }
            }
        }

        private bool DoValuesContainKeys()
        {
            foreach (var keyValuePair in VariableDictionary)
            {
                foreach (var keyValuePairY in VariableDictionary)
                {
                    if (keyValuePair.Value.ToString().Contains("%" + keyValuePairY.Key + "%"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public string ReplaceKeywords(string value)
        {
            string newValue = value;

            if (value != null)
            {

                foreach (var key in VariableDictionary.Keys)
                {
                    if (newValue.Contains("%" + key + "%"))
                    {
                        if (VariableDictionary[key].isDynamic)
                        {
                            foreach(Object obj in toLoadFrom)
                            {
                                ReloadSpecificAnnotatedKeyValuePair(obj, key);
                            }
                        }

                        // Get the replacement value, default to empty string if null
                        string replacementValue = VariableDictionary[key].value?.ToString() ?? "";

                        // Log if we had to use empty string as fallback for null value
                        if (VariableDictionary[key].value == null)
                        {
                            Logging.Logger.Log("Variable %" + key + "% has null value, replacing with empty string", Logging.LoggingSeverity.Debug);
                        }

                        string oldValue = newValue;
                        newValue = newValue.Replace("%" + key + "%", replacementValue);
                        Logging.Logger.Log("Replaced %" + key + "% in string: '" + oldValue + "' -> '" + newValue + "'", Logging.LoggingSeverity.Debug);
                    }
                }
            }
            return newValue;
        }

        

        public void FullRefresh()
        {
            LoadInAllEnvironmentVariables();
            SetCustomKeyValues();
            LoadAllAnnotatedKeyValuePairs(toLoadFrom);
            ReplaceKeysInValuesTillKeyless();
            SetValuesAccordingToVariables(toLoadFrom);
        }
    }
}
