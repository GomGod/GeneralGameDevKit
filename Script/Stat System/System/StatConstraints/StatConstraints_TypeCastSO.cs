using System;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Stat Constraints/TypeCastSO", fileName = "StatConstraints_TypeCast")]
    public class StatConstraints_TypeCastSO : StatConstraintsSO
    {
        [SerializeField] public ValueType valueTypeToCast;
        
        public override void ApplyConstraintsToSystem(StatSystemCore core)
        {
            core.AddStatConstraints(new StatConstraints_TypeCast(this));
        }

        public enum ValueType
        {
            IntFloor,
            IntCeil,
            IntRoundHalfUp,
            IntRoundBanker,
            Bool
        }
    }

    public class StatConstraints_TypeCast : StatConstraints
    {
        private readonly StatConstraints_TypeCastSO _baseSO;
        
        public StatConstraints_TypeCast(StatConstraints_TypeCastSO baseSO)
        {
            _baseSO = baseSO;
            targetStatID = baseSO.targetStatId;
            isBaseStatConstraintsActivated = baseSO.applyBaseValue;
            isApplyStatConstraintsActivated = baseSO.applyApplyValue;
        }
        
        public override float ProcessBaseStat(StatSystemCore targetSystem, float value) => Cast(value);
        public override float ProcessApplyStat(StatSystemCore targetSystem, float value) => Cast(value);

        private float Cast(float value) =>
            _baseSO.valueTypeToCast switch
            {
                StatConstraints_TypeCastSO.ValueType.IntFloor => Mathf.FloorToInt(value),
                StatConstraints_TypeCastSO.ValueType.IntCeil => Mathf.CeilToInt(value),
                StatConstraints_TypeCastSO.ValueType.IntRoundHalfUp => Mathf.FloorToInt(value) + (value % 1 >= 0.5f ? 1 : 0),
                StatConstraints_TypeCastSO.ValueType.IntRoundBanker => Mathf.RoundToInt(value),
                StatConstraints_TypeCastSO.ValueType.Bool => value < 0 ? -1 : 1,
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}