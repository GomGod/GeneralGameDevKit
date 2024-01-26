using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    public abstract class StatConstraintsSO : ScriptableObject
    {
        [SerializeField, KeyTable("KeyTableAsset_Stats")] public string targetStatId;
        [SerializeField] public bool applyBaseValue;
        [SerializeField] public bool applyApplyValue;

        public abstract void ApplyConstraintsToSystem(StatSystemCore core);
    }
}