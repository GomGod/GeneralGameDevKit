using System;
using UnityEngine;

namespace GeneralGameDevKit.KeyTableSystem
{
    [Serializable]
    public class TableValueWriter
    {
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] private string targetKey;
        [SerializeField] private TargetDataType targetDataType;
        [SerializeField] private WriteOperator writeOperator;
        
        [SerializeField] private float floatVal;
        [SerializeField] private int intVal;
        [SerializeField] private bool boolVal;
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] private string sourceKey;

        public string GetTargetKeyString() => targetKey;
        
        public void WriteData(KeyValueTable targetTable, KeyValueTable sourceTable = null)
        {
            var valueSourceTable = sourceTable ?? targetTable;
            var valueSource = targetDataType switch
            {
                TargetDataType.Float => floatVal,
                TargetDataType.Int => intVal,
                TargetDataType.Bool => boolVal ? 1.0 : -1.0,
                TargetDataType.Key => valueSourceTable.GetTableValueDouble(sourceKey),
                _ => throw new ArgumentOutOfRangeException()
            };
            var valueToWrite = targetTable.GetTableValueDouble(targetKey);
            switch (writeOperator)
            {
                case WriteOperator.Add:
                    valueToWrite += valueSource;
                    break;
                case WriteOperator.Mul:
                    valueToWrite *= valueSource;
                    break;
                case WriteOperator.Div:
                    valueToWrite /= valueSource;
                    break;
                case WriteOperator.Update:
                    valueToWrite = valueSource;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (targetDataType)
            {
                case TargetDataType.Float:
                case TargetDataType.Key:
                    targetTable.WriteDataOnTableDouble(targetKey, valueToWrite);
                    break;
                case TargetDataType.Int:
                    targetTable.WriteDataOnTableInt(targetKey, (int)valueToWrite);
                    break;
                case TargetDataType.Bool:
                    targetTable.WriteDataOnTableBool(targetKey, valueToWrite > 0);
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

        public enum WriteOperator
        {
            Update,
            Add,
            Mul,
            Div
        }
    }
}