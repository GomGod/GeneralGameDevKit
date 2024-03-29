﻿using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.TagSystem;
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
        [SerializeField] public List<DevKitTag> effectTags;
        
        [Header("Duration")] 
        [SerializeField] public float duration;
        [SerializeField] public DurationPolicy durationPolicy;
        
        [Header("Stacking")]
        [SerializeField] public bool useStacking;
        [SerializeField] public int maxStack;
        [SerializeField] public StackOutPolicy stackOutPolicy;
        [SerializeField] public StackDurationPolicy stackDurationPolicy;

        public StatEffectInstance BuildEffectInstance(BaseStatObject caster)
        {
            return new StatEffectInstance(duration)
            {
                CasterObject = caster,
                EffectId = profileID,
                EffectIconId = effectIconId,
                EffectName = effectName,
                EffectDesc = effectDesc,
                ModifiersToApply = statModifiers.Select(mod => mod.GetCopy()).ToList(),
                EffectTagsToApply = new List<DevKitTag>(effectTags),
                DurationPolicy = durationPolicy,
                UseStacking = useStacking,
                MaxStack = maxStack,
                StackOutPolicy = stackOutPolicy,
                StackDurationPolicy = stackDurationPolicy
            };
        }
        
        public enum StackDurationPolicy
        {
            Independent,
            Combined
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