using System;
using UnityEngine;

namespace GeneralGameDevKit.ValueTableSystem
{
    [Serializable]
    public class TableValueWriter
    {
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] private string targetKey;
        [SerializeField] private TargetDataType targetDataType;
        
        [SerializeField] private float floatVal;
        [SerializeField] private int intVal;
        [SerializeField] private bool boolVal;
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] private string sourceKey;

        public string GetTargetKeyString() => targetKey;
        
        public void WriteData(KeyValueTable targetTable)
        {
            switch (targetDataType)
            {
                case TargetDataType.Float:
                    targetTable.WriteDataOnTableDouble(targetKey, floatVal);
                    break;
                case TargetDataType.Int:
                    targetTable.WriteDataOnTableInt(targetKey, intVal);
                    break;
                case TargetDataType.Bool:
                    targetTable.WriteDataOnTableBool(targetKey, boolVal);
                    break;
                case TargetDataType.Key:
                    targetTable.WriteDataOnTableDouble(targetKey, targetTable.GetTableValueDouble(sourceKey));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void WriteData(KeyValueTable targetTable, KeyValueTable sourceTable)
        {
            switch (targetDataType)
            {
                case TargetDataType.Float:
                    targetTable.WriteDataOnTableDouble(targetKey, floatVal);
                    break;
                case TargetDataType.Int:
                    targetTable.WriteDataOnTableInt(targetKey, intVal);
                    break;
                case TargetDataType.Bool:
                    targetTable.WriteDataOnTableBool(targetKey, boolVal);
                    break;
                case TargetDataType.Key:
                    targetTable.WriteDataOnTableDouble(targetKey, sourceTable.GetTableValueDouble(sourceKey));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum TargetDataType
        {
            Float,
            Int,
            Bool,
            Key
        }
    }
}