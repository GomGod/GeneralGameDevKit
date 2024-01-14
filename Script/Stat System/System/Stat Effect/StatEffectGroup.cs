using System;
using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    /// <summary>
    /// Set of StatEffectProfiles.
    /// Stat effect instances created by profiles will be applied with the same group policy settings.
    /// </summary>
    [CreateAssetMenu(menuName = "General Game Dev Kit/Stat System/Stat Effect Group", fileName = "StatEffectProfileGroup")]
    public class StatEffectGroup : ScriptableObject
    {
        [Header("Essential")] 
        [SerializeField, Tooltip("Member Effect profiles will replace by group Id & index")] 
        private string groupId; //Id of this effect group. Ids must be unique or stacking will not work properly.

        //Resource fields can be used differently depending on the system.
        [Header("Text Resources")] 
        [SerializeField] private string groupIconId; //resource id
        [SerializeField] private string groupName; //name of effect group
        [SerializeField, TextArea] public string groupDesc; //description of effect group
        
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
        [SerializeField] public bool useStacking;
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