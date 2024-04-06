using System;
using GeneralGameDevKit.KeyTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Stat Constraints/ValueRangeSO", fileName = "StatConstraints_ValueRange")]
    public class StatConstraints_ValueRangeSO : StatConstraintsSO
    {
        [SerializeField] public ValueRangeSource minValueSource;
        [SerializeField] public ValueRangeSource maxValueSource;
        
        [SerializeField] public float minValueConst;
        [SerializeField] public float maxValueConst;

        [SerializeField, KeyTable("KeyTableAsset_Stats")] public string minValueTargetStat;
        [SerializeField, KeyTable("KeyTableAsset_Stats")] public string maxValueTargetStat;
        
        public override void ApplyConstraintsToSystem(StatSystemCore core)
        {
            var constraintsToAdd = new StatConstraints_ValueRange(this);
            core.AddStatConstraints(constraintsToAdd);
            if(minValueSource is not ValueRangeSource.Const)
                core.AddConstraintDependency(minValueTargetStat, constraintsToAdd);
            if (maxValueSource is not ValueRangeSource.Const)
                core.AddConstraintDependency(maxValueTargetStat, constraintsToAdd);
        }

        public enum ValueRangeSource
        {
            Const,
            TargetStatBaseValue,
            TargetStatApplyValue
        }
    }

    public class StatConstraints_ValueRange : StatConstraints
    {
        private readonly StatConstraints_ValueRangeSO _baseSO;
        public StatConstraints_ValueRange(StatConstraints_ValueRangeSO baseSO)
        {
            _baseSO = baseSO;
            targetStatID = baseSO.targetStatId;
            isBaseStatConstraintsActivated = baseSO.applyBaseValue;
            isApplyStatConstraintsActivated = baseSO.applyApplyValue;
        }
        
        public override float ProcessBaseStat(StatSystemCore targetSystem, float value)
        {
            var (min, max) = GetMinMax(targetSystem);
            return Mathf.Clamp(value, min, max);
        }

        public override float ProcessApplyStat(StatSystemCore targetSystem, float value)
        {
            var (min, max) = GetMinMax(targetSystem);
            return Mathf.Clamp(value, min, max);
        }

        private (float, float) GetMinMax(StatSystemCore targetSystem)
        {
            var min = _baseSO.minValueSource switch
            {
                StatConstraints_ValueRangeSO.ValueRangeSource.Const => _baseSO.minValueConst,
                StatConstraints_ValueRangeSO.ValueRangeSource.TargetStatBaseValue => targetSystem.GetBaseValue(_baseSO.minValueTargetStat),
                StatConstraints_ValueRangeSO.ValueRangeSource.TargetStatApplyValue => targetSystem.GetStatApplyValue(_baseSO.maxValueTargetStat),
                _ => throw new ArgumentOutOfRangeException()
            };

            var max = _baseSO.maxValueSource switch
            {
                StatConstraints_ValueRangeSO.ValueRangeSource.Const => _baseSO.maxValueConst,
                StatConstraints_ValueRangeSO.ValueRangeSource.TargetStatBaseValue => targetSystem.GetBaseValue(_baseSO.maxValueTargetStat),
                StatConstraints_ValueRangeSO.ValueRangeSource.TargetStatApplyValue => targetSystem.GetStatApplyValue(_baseSO.maxValueTargetStat),
                _ => throw new ArgumentOutOfRangeException()
            };

            return (min, max);
        }
    }
}