using System;
using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Effect Profile", fileName = "StatEffectProfile")]
    public class StatEffectProfile : ScriptableObject
    {
        
        
        [Header("Tags")]
        [SerializeField, KeyTable("KeyTableAsset_Tags" )] 
        public List<DevKitTagSerializable> effectTags;
        
        [Header("Stat Modifiers")]
        public List<StatModifier> ProfileStatModifiers;
        
        [Header("Stacking")]
        [SerializeField] public int maxStack;
        [SerializeField] public StackingPolicy stackingPolicy;
        [SerializeField] public StackOutPolicy stackOutPolicy;
        [SerializeField] public StackDurationPolicy stackDurationPolicy;
        
        public enum StackingPolicy
        {
            Independent,
            CountStackOnCaster,
            CountStackOnReceiver
        }

        public enum StackDurationPolicy
        {
            NeverRefresh,
            RefreshOnApply
        }

        public enum StackOutPolicy
        {
            RemoveSingleStack,
            ClearAllStack
        }
        
        //todo - implements single statEffectInstance
    }
}