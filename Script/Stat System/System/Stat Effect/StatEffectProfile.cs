using System;
using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Stat Effect Profile", fileName = "StatEffectProfile")]
    public class StatEffectProfile : ScriptableObject
    {
        [Header("Essential")]
        [SerializeField] public string profileID;
        
        [Header("Text Resources")] 
        [SerializeField] public string effectIconId;
        [SerializeField] public string effectName;
        [SerializeField, TextArea] public string effectDesc;
        
        [Header("Stat Modifiers")]
        [SerializeField] public List<StatModifier> statModifiers;

        [Header("Tags")] 
        [SerializeField, KeyTable("KeyTableAsset_Tags")]
        public List<DevKitTag> effectTags;
        
        [Header("Duration")] 
        [SerializeField] public float duration;
        [SerializeField] public DurationPolicy durationPolicy;
        
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

        public enum DurationPolicy
        {
            Manual,
            Infinite
        }
    }
}