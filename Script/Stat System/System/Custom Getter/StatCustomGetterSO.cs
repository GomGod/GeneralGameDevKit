using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    public abstract class StatCustomGetterSO : ScriptableObject
    {
        [SerializeField, KeyTable("KeyTableAsset_Stats")] public string targetStatId;

        [SerializeField] public bool IgnoreModifier;
        [SerializeField] public bool UseModifierHooks;

        [SerializeField, KeyTable("KeyTableAsset_Stats")] public string StatIdToHookModifier;


        public abstract void ApplyCustomGetterToSystem(StatSystemCore core);
    }
}