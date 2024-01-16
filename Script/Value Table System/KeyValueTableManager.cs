using System.Collections.Generic;
using GeneralGameDevKit.Utils;

namespace GeneralGameDevKit.ValueTableSystem
{
    /// <summary>
    /// Manager class for KeyValueTable.
    /// </summary>
    public class KeyValueTableManager : NonMonoSingleton<KeyValueTableManager>
    {
        private readonly Dictionary<string, KeyValueTable> _keyValueTables = new();

        /// <summary>
        /// Add new table.
        /// </summary>
        /// <param name="tableName">table name</param>
        public void AddNewTable(string tableName)
        {
            if (!_keyValueTables.ContainsKey(tableName))
            {
                _keyValueTables.Add(tableName, new KeyValueTable(tableName));
                return;
            }
            
            //todo : error handling
        }

        public bool IsTableSet(string tableName)
        {
            return _keyValueTables.ContainsKey(tableName);
        }

        /// <summary>
        /// Remove table in system table collection
        /// </summary>
        /// <param name="tableName">table name to remove</param>
        /// <returns>T: remove success, F: remove fail</returns>
        public bool RemoveTable(string tableName)
        {
            return _keyValueTables.Remove(tableName);
        }

        /// <summary>
        /// Get KeyValueTable matches with tableName.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public KeyValueTable GetKeyValueTable(string tableName)
        {
            if (_keyValueTables.TryGetValue(tableName, out var table))
                return table;
            
            //todo : error handling
            return null;
        }
    }
}