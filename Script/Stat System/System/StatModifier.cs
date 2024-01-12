using System;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    /// <summary>
    /// Modifier Of Stats. It can be used for temporarily buffs.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        [HideInInspector] public uint TimeStamp; //In same priority, smaller will run first. It will be assigned by core system.
        [SerializeField] public int Priority; //Bigger will run first.

        [SerializeField, KeyTable("KeyTableAsset_Stats")] public KeyString TargetStatID; //This id must be unique, used to find specific modifier.
        
        [SerializeField] public float Coefficient; //Value for calculation.

        [SerializeField] public ModificationPolicy ModPolicy; //Modification policy that defines modifier apply as temporary or permanently.
        [SerializeField] public StatCalcOperator CalcOperator;
        [SerializeField] public ModCalculationPolicy CalcPolicy; //Type of calculate chain.

        public StatModifier GetCopy()
        {
            return new StatModifier
            {
                TargetStatID = TargetStatID,
                Coefficient = Coefficient,
                ModPolicy = ModPolicy,
                CalcOperator = CalcOperator,
                CalcPolicy = CalcPolicy
            };
        }
        
        public enum ModificationPolicy
        {
            Instant,
            Temporary
        }
        
        public enum ModCalculationPolicy
        {
            CalcWithBase, //calculate with base value, and result will be add to apply stat value.
            CalcWithResult //calculate with result value.
        }
    }
}