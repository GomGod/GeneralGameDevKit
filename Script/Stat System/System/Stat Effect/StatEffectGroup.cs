using System;
using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    /// <summary>
    /// Combine multiple StatEffectProfiles.
    /// </summary>
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Stat Effect Group", fileName = "StatEffectProfileGroup")]
    public class StatEffectGroup : ScriptableObject
    {
        [Header("Essential")] 
        [SerializeField] private string groupId;

        [Header("Text Resources")] 
        [SerializeField] private string groupIconId;
        [SerializeField] private string groupName;
        [SerializeField, TextArea] public string groupDesc;
        
        [Header("Profiles")]
        [SerializeField] private List<StatEffectProfile> effectProfilesToApply;
        [SerializeField] private List<ModifierOverrideData> effectModifierOverrideData;
        
        [Header("Override Settings")]
        [SerializeField, KeyTable("KeyTableAsset_Tags")]
        public List<DevKitTag> effectTags;

        [Header("Duration")]
        [SerializeField] public StatEffectProfile.DurationPolicy durationPolicy;
        [SerializeField] public OverrideSourceType durationOverrideSourceType;
        [SerializeField] public float constDuration;
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] public string dynamicDuration;

        [Header("Stacking")] 
        [SerializeField] public OverrideSourceType stackOverrideSourceType;
        [SerializeField] public int constMaxStack;
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] public string dynamicMaxStack;
        [SerializeField] public StatEffectProfile.StackingPolicy stackingPolicy;
        [SerializeField] public StatEffectProfile.StackOutPolicy stackOutPolicy;
        [SerializeField] public StatEffectProfile.StackDurationPolicy stackDurationPolicy;
        
        
    }

    [Serializable]
    public struct ModifierOverrideData
    {
        [SerializeField] public int targetProfileIdx;
        [SerializeField] public int targetModifierIdx;

        [SerializeField] public float value;
        [SerializeField, KeyTable("KeyTableAsset_DynamicParameters")] public string valueSourceKey;
    }
    
    public enum OverrideSourceType
    {
        Const,
        Dynamic
    }
}