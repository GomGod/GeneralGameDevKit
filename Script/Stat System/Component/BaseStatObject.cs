using System;
using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    public abstract class BaseStatObject : MonoBehaviour
    {
        [SerializeField] protected int id;
        [SerializeField] protected DevKitTagContainer _permanentTags;
        
        protected const string PersonalTablePrefix = "%StatSystem%ptable%";
        
        protected DevKitTagContainer _temporaryTags;
        
        protected List<StatEffectInstance> currentEffectInstances = new();
        protected List<StatModifier> temporaryModifiers = new();
        
        protected readonly StatSystemCore StatSystemCore = new();
        
        public void Awake()
        {
            KeyValueTableManager.Instance.AddNewTable(GetPersonalTableKey());
            InitializeTask();
        }

        public void InitializeStats(Dictionary<string, float> statCollections)
        {
            foreach (var (id, val) in statCollections)
            {
                StatSystemCore.ModifyStatBaseValue(id, val);
            }
        }

        public KeyValueTable GetPersonalTable()
        {
            var tableKey = GetPersonalTableKey();
            if (KeyValueTableManager.Instance.IsTableSet(tableKey))
            {
                KeyValueTableManager.Instance.AddNewTable(tableKey);
            }
            return KeyValueTableManager.Instance.GetKeyValueTable(tableKey);  
        }
        
        public float GetStatBaseValue(string identifier)
        {
            return StatSystemCore.GetBaseValue(identifier);
        }

        public float GetStatApplyValue(string identifier)
        {
            return StatSystemCore.GetStatApplyValue(identifier);
        }

        public bool ApplyStatEffect(StatEffectInstance instanceToAdd)
        {
            temporaryModifiers.Clear();
            StatSystemCore.GetAllStatModifiers(ref temporaryModifiers);

            var isStackable = instanceToAdd.UseStacking;
            if (isStackable)
            {//find stackable effect and try add stack
                var targetIdx = currentEffectInstances.FindIndex(i => i.EffectId.Equals(instanceToAdd.EffectId));
                if (targetIdx <= 0) return true;
                
                var result = currentEffectInstances[targetIdx].TryAddStack(instanceToAdd);
                if (!result)
                    return false;
                
                ApplyStatEffectInstance(instanceToAdd, true);
                return true;
            }
            
            //or add new effect instance
            ApplyStatEffectInstance(instanceToAdd, false);

            return true;
        }

        public void GetApplyTags(ref DevKitTagContainer tagCollectionToReceive)
        {
            tagCollectionToReceive.MergeContainer(_permanentTags);
            foreach (var fxInstance in currentEffectInstances)
            {
                tagCollectionToReceive.AddTags(fxInstance.EffectTagsToApply);
            }
        }

        protected void ApplyStatEffectInstance(StatEffectInstance instanceToAdd, bool isStackOperation)
        {
            instanceToAdd.OnExpired += RemoveStatEffect;
            foreach (var modifier in instanceToAdd.ModifiersToApply)
            {
                StatSystemCore.AddStatModifier(modifier);
            }

            _temporaryTags.AddTags(instanceToAdd.EffectTagsToApply);
            if (!isStackOperation) return;
            currentEffectInstances.Add(instanceToAdd);
        }

        public void RemoveStatEffect(StatEffectInstance instanceToRemove)
        {
            foreach (var modifier in instanceToRemove.ModifiersToApply)
            {
                StatSystemCore.RemoveStatModifier(modifier);
            }
            
            _temporaryTags.RemoveTags(instanceToRemove.EffectTagsToApply);
            if (!instanceToRemove.UseStacking || instanceToRemove.GetCurrentStackCount() == 0)
            {
                currentEffectInstances.Remove(instanceToRemove);
            }
        }

        public void SetOnTagUpdatedEventCallback(Action<DevKitTag, int, int> callback)
        {
            _permanentTags.OnTagUpdate += callback;
            _temporaryTags.OnTagUpdate += callback;
        }

        public void RemoveOnTagUpdatedEventCallback(Action<DevKitTag, int, int> callback)
        {
            _permanentTags.OnTagUpdate -= callback;
            _temporaryTags.OnTagUpdate -= callback;
        }

        public void SetOnApplyStatUpdatedEventCallback(Action<StatInfo, float> callback)
        {
            StatSystemCore.OnStatApplyValueChanged += callback;
        }
        
        public void RemoveOnApplyStatUpdatedEventCallback(Action<StatInfo, float> callback)
        {
            StatSystemCore.OnStatApplyValueChanged -= callback;
        }

        public void SetOnBaseStatUpdatedEventCallback(Action<StatInfo, float> callback)
        {
            StatSystemCore.OnStatBaseValueChanged += callback;
        }
        
        public void RemoveOnBaseStatUpdatedEventCallback(Action<StatInfo, float> callback)
        {
            StatSystemCore.OnStatBaseValueChanged -= callback;
        }
        
        public virtual void InitializeTask() {}
        public abstract string GetUniqueObjectKey();
        protected virtual string GetPersonalTableKey() => $"{PersonalTablePrefix}{GetUniqueObjectKey()}";
        
    }
}
