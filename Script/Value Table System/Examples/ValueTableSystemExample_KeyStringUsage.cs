using System.Collections.Generic;
using UnityEngine;

namespace GeneralGameDevKit.ValueTableSystem.Examples
{
    /// <summary>
    /// This class is for examples of ValueTableSystem.
    /// You can remove this scripts if you don't need.
    /// </summary>
    [CreateAssetMenu(fileName = "KeyTableAsset", menuName = "General Game Dev Kit/Value Table System/Example/Example_KeyStringUsage")]
    public class ValueTableSystemExample_KeyStringUsage : ScriptableObject
    {
        //How to declare KeyString examples.
        [SerializeField, KeyTable("KeyTableAsset_Example")]
        private string keyStringForExample;

        //You can also apply KeyTable Attribute to List<KeyString>.
        [SerializeField, KeyTable("KeyTableAsset_Example")]
        private List<string> keyStringList;

        private const string TableKeyForExample = "ExampleTable";

        /// <summary>
        /// Write value on table.
        /// </summary>
        public void ExampleWrite()
        {
            var tableManager = KeyValueTableManager.Instance;
            tableManager.AddNewTable(TableKeyForExample);
            var tableToUse = tableManager.GetKeyValueTable(TableKeyForExample);

            tableToUse.WriteDataOnTableInt(keyStringForExample, 1234);
        }

        /// <summary>
        /// Read value on table and print log.
        /// </summary>
        public void ExampleRead()
        {
            var tableManager = KeyValueTableManager.Instance;
            tableManager.AddNewTable(TableKeyForExample);
            var tableToUse = tableManager.GetKeyValueTable(TableKeyForExample);

            Debug.Log($"{tableToUse.GetTableValueInt(keyStringForExample)}");
        }
    }
}