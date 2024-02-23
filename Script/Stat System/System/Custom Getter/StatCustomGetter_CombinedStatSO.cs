using System;
using System.Collections.Generic;
using System.Linq;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Stat Custom Getter/Combined Stat SO", fileName = "CombinedStatGetterSO")]
    public class StatCustomGetter_CombinedStatSO : StatCustomGetterSO
    {
        [SerializeField] public List<CalculateChunk> calcChunks;

        public override void ApplyCustomGetterToSystem(StatSystemCore core)
        {
            core.SetCustomGetter(new StatCustomGetter_CombinedStat(this), targetStatId);
        }
    }

    public class StatCustomGetter_CombinedStat : StatCustomGetter
    {
        private readonly List<CalculateChunk> _calcChunks = new();

        public StatCustomGetter_CombinedStat(List<CalculateChunk> chunks)
        {
            _calcChunks.AddRange(chunks);
        }

        public StatCustomGetter_CombinedStat(StatCustomGetter_CombinedStatSO baseSO)
        {
            _calcChunks.AddRange(baseSO.calcChunks);
        }

        public override float GetProcessedValue(StatSystemCore processingSystem, string currentProcessingStatKey, float currentValue)
        {
            return _calcChunks.Sum(chunk => chunk.GetResult(processingSystem));
        }
    }

    [Serializable]
    public struct CalculateChunk
    {
        public bool useResultAsValueA;
        
        [SerializeField, KeyTable("KeyTableAsset_Stats")]
        public string sourceStatA;

        [SerializeField, KeyTable("KeyTableAsset_Stats")]
        public string sourceStatB;

        [SerializeField] public StatCalcOperator calcOperator;

        public float GetResult(StatSystemCore processingSystem)
        {
            var statValA = processingSystem.GetStatApplyValue(sourceStatA);
            
            if (useResultAsValueA)
                return statValA;
            
            var statValB = processingSystem.GetStatApplyValue(sourceStatB);
            
            return calcOperator switch
            {
                StatCalcOperator.Add => statValA + statValB,
                StatCalcOperator.Mul => statValA * statValB,
                StatCalcOperator.Div => statValA / statValB,
                StatCalcOperator.Sub => statValA - statValB,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}