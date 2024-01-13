using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralGameDevKit.ValueTableSystem
{
    /// <summary>
    /// Table class designed for ValueTableSystem. <br/>
    /// Similar to Dictionary, but limited and added some features to manage callback and key-value pairs.
    /// </summary>
    public class KeyValueTable
    {
        public readonly string TableName;

        public KeyValueTable(string name)
        {
            TableName = name;
        }
        
        /// <summary>
        /// Clear all table.
        /// </summary>
        public void ClearTable()
        {
            _table.Clear();
            _observers.Clear();
        }
        
        #region observers
        private readonly Dictionary<string, Action> _observers = new();
        private Action GetObservers(string key) => !_observers.ContainsKey(key) ? null : _observers[key];
        
        /// <summary>
        /// Set callback actions react with value change.
        /// </summary>
        /// <param name="key">key to react value changes</param>
        /// <param name="callback">action as callback</param>
        public void SetObserver(string key, Action callback)
        {
            if (!_observers.ContainsKey(key))
            {
                _observers.Add(key,callback);
                return;
            }
            _observers[key] += callback;
        }
        
        /// <summary>
        /// Remove callback action. <br/>
        /// You have to pass same instances of action or it will not remove properly.
        /// </summary>
        /// <param name="key">key to react value changes</param>
        /// <param name="callback">callback action to remove</param>
        public void RemoveObserver(string key, Action callback)
        {
            if (!_observers.ContainsKey(key))
            {
                Debug.LogError($"Key ({key}) is not on value table. Table:{TableName}");
                return;
            }
            _observers[key] -= callback;
        }
        
        /// <summary>
        /// Remove all callbacks on matches key.
        /// </summary>
        /// <param name="key">key to react value changes</param>
        public void RemoveObserver(string key)
        {
            if (!_observers.ContainsKey(key))
            {
                Debug.LogError($"Key ({key}) is not on value table. Table:{TableName}");
                return;
            }
            _observers.Remove(key);
        }
        #endregion

        #region TABLE
        private readonly Dictionary<string, double> _table = new();

        /// <summary>
        /// Get table value as double
        /// </summary>
        /// <param name="key">Key of value</param>
        /// <returns>Returns the value of the table that matches the key value as double type.</returns>
        public double GetTableValueDouble(string key)
        {
            if (_table.TryGetValue(key, out var ret)) 
                return ret;
            
            WriteDataOnTableDouble(key, 0);
            return _table[key];
        }

        /// <summary>
        /// Get table value as int.
        /// Returns rounded value. It can be unwanted result.
        /// </summary>
        /// <param name="key">Key of value</param>
        /// <returns>Returns the value of the table that matches the key value as int type.</returns>
        public int GetTableValueInt(string key)
        {
            var doubleValue = GetTableValueDouble(key);
            return (int) Math.Round(doubleValue);
        }

        /// <summary>
        /// Get table value as bool
        /// </summary>
        /// <param name="key">Key of value</param>
        /// <returns>Returns the value of the table that matches the key value as bool type.<br/>
        /// Returns True or False if greater than 0.</returns>
        public bool GetTableValueBool(string key)
        {
            var floatValue = GetTableValueDouble(key);
            return floatValue > 0;
        }

        /// <summary>
        /// Write value on table with double value.
        /// </summary>
        /// <param name="key">Key of value</param>
        /// <param name="value">value to write</param>
        public void WriteDataOnTableDouble(string key, double value)
        {
            key ??= string.Empty;
            _table[key] = value;

            var observerCallbacks = GetObservers(key);
            observerCallbacks?.Invoke();
        }
        
        /// <summary>
        /// Write value on table with int value.
        /// </summary>
        /// <param name="key">Key of value</param>
        /// <param name="value">value to write</param>
        public void WriteDataOnTableInt(string key, int value)
        {
            WriteDataOnTableDouble(key, value);
        }

        /// <summary>
        /// Write value on table with bool value.
        /// Value changes T to 1.0 or -1.0.
        /// </summary>
        /// <param name="key">Key of value</param>
        /// <param name="value">value to write</param>
        public void WriteDataOnTableBool(string key, bool value)
        {
            WriteDataOnTableDouble(key, value ? 1.0f : -1.0f);
        }

        /// <summary>
        /// Check is key exist on table.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyValueExistOnTable(string key)
        {
            return key != null && _table.ContainsKey(key);
        }
        
        /// <summary>
        /// Remove key-value pair on table.
        /// </summary>
        /// <param name="key"></param>
        public bool RemoveKeyValueOnTable(string key)
        {
            if(key==null) return false;
            if (!_table.ContainsKey(key))
            {
                //todo : error handling
                Debug.LogError($"Key ({key}) is not on value table. Table:{TableName}");
                return false;
            }
            
            GetObservers(key)?.Invoke();
            _table.Remove(key);
            
            if(_observers.ContainsKey(key))
                RemoveObserver(key);

            return true;
        }
        #endregion
    }
}