using System;
using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    public abstract class BaseStatObject : MonoBehaviour
    {
        [SerializeField] protected int id;
        [SerializeField] protected DevKitTagContainer permanentTags;
        [SerializeField] protected List<StatConstraintsSO> statConstraints; 

        protected Dictionary<string, Action<StatInfo, float>> applyStatChangedCallbacks = new();
        protected Dictionary<string, Action<StatInfo, float>> baseStatChangedCallbacks = new();
        
        protected const string PersonalTablePrefix = "%StatSystem%ptable%";
        
        protected DevKitTagContainer _temporaryTags = new();
        protected DevKitTagContainer _manualAddedTags = new();
        
        protected List<StatEffectInstance> currentEffectInstances = new();
        protected List<StatEffectInstance> tempCollectionToStatEffectProcessing = new();
        
        protected List<StatModifier> temporaryModifiers = new();
        
        public readonly StatSystemCore StatSystemCore = new();

        protected void Awake()
        {
            KeyValueTableManager.Instance.AddNewTable(GetPersonalTableKey());
            foreach (var constraintsSO in statConstraints)
            {
                constraintsSO.ApplyConstraintsToSystem(StatSystemCore);
            }

            StatSystemCore.OnStatApplyValueChanged -= BroadcastApplyStatChangedEvent;
            StatSystemCore.OnStatApplyValueChanged += BroadcastApplyStatChangedEvent;
            StatSystemCore.OnStatBaseValueChanged -= BroadcastBaseStatChangedEvent;
            StatSystemCore.OnStatBaseValueChanged += BroadcastBaseStatChangedEvent;
        }

        public virtual void InitializeTask()
        {
        }

        public void ForceUpdate(string targetStat)
        {
            var statInfo = StatSystemCore.GetStatInfo(targetStat);
            if (statInfo == null)
                return;
            BroadcastApplyStatChangedEvent(statInfo, statInfo.StatValue);
        }
        
        public void InitializeStats(Dictionary<string, float> statCollections)
        {
            foreach (var (statId, val) in statCollections)
            {
                StatSystemCore.ModifyStatBaseValue(statId, val);
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

        public List<StatEffectInstance> GetAllStatEffectInstances() => new(currentEffectInstances);
        public List<StatModifier> FindModifiersWithTargetStat(string identifier) => new(StatSystemCore.GetAllStatModifiers().Where(m => m.TargetStatID.Equals(identifier)));
        
        public bool ApplyStatEffect(StatEffectInstance instanceToAdd)
        {
            temporaryModifiers.Clear();
            StatSystemCore.GetAllStatModifiers(ref temporaryModifiers);

            var isStackable = instanceToAdd.UseStacking;
            if (isStackable)
            {
                //find stackable effect and try add stack
                var targetIdx = currentEffectInstances.FindIndex(i => i.EffectId.Equals(instanceToAdd.EffectId));
                if (targetIdx > 0)
                    return currentEffectInstances[targetIdx].TryAddStack(instanceToAdd);

                ApplyStatEffectInstance(instanceToAdd, true);
                return true;
            }

            //or add new effect instance
            ApplyStatEffectInstance(instanceToAdd, false);

            return true;
        }

        public void AddManualTag(DevKitTag tag, int cnt =1)
        {
            for (var i = 0; i > cnt; i++)
            {
                _manualAddedTags.AddTag(tag);
            }
        }

        public void RemoveManualTag(DevKitTag tag, int cnt =1)
        {
            for (var i = 0; i > cnt; i++)
            {
                _manualAddedTags.RemoveTag(tag);
            }
        }

        public int GetTagCount(DevKitTag tag)
        {
            return permanentTags.GetTagCount(tag) + _temporaryTags.GetTagCount(tag) + _manualAddedTags.GetTagCount(tag);
        }

        public void GetApplyTags(ref DevKitTagContainer tagCollectionToReceive)
        {
            tagCollectionToReceive.ClearContainer();
            tagCollectionToReceive.MergeContainer(permanentTags);
            tagCollectionToReceive.MergeContainer(_manualAddedTags);
            foreach (var fxInstance in currentEffectInstances)
            {
                tagCollectionToReceive.AddTags(fxInstance.EffectTagsToApply);
            }
        }

        public DevKitTagContainer GetApplyTags()
        {
            var ret = new DevKitTagContainer();
            ret.MergeContainer(permanentTags);
            ret.MergeContainer(_manualAddedTags);
            foreach (var fxInstance in currentEffectInstances)
            {
                ret.AddTags(fxInstance.EffectTagsToApply);
            }

            return ret;
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

        private void BroadcastApplyStatChangedEvent(StatInfo statInfo, float val)
        {
            if (!applyStatChangedCallbacks.ContainsKey(statInfo.ID))
                return;
            
            applyStatChangedCallbacks[statInfo.ID]?.Invoke(statInfo, val);
        }
        
        private void BroadcastBaseStatChangedEvent(StatInfo statInfo, float val)
        {
            if (!applyStatChangedCallbacks.ContainsKey(statInfo.ID))
                return;
            
            applyStatChangedCallbacks[statInfo.ID]?.Invoke(statInfo, val);
        }

        public void SetOnTagUpdatedEventCallback(Action<DevKitTag, int, int> callback)
        {
            permanentTags.OnTagUpdate += callback;
            _temporaryTags.OnTagUpdate += callback;
        }

        public void RemoveOnTagUpdatedEventCallback(Action<DevKitTag, int, int> callback)
        {
            permanentTags.OnTagUpdate -= callback;
            _temporaryTags.OnTagUpdate -= callback;
        }

        public void SetOnApplyStatUpdatedEventCallback(string targetId, Action<StatInfo, float> callback)
        {
            if (!applyStatChangedCallbacks.ContainsKey(targetId))
            {
                applyStatChangedCallbacks.Add(targetId, callback);
                return;
            }
            applyStatChangedCallbacks[targetId] += callback;
        }
        
        public bool RemoveOnApplyStatUpdatedEventCallback(string targetId, Action<StatInfo, float> callback)
        {
            if (!applyStatChangedCallbacks.ContainsKey(targetId))
                return false;

            applyStatChangedCallbacks[targetId] -= callback;
            return true;
        }

        public void SetOnBaseStatUpdatedEventCallback(Action<StatInfo, float> callback)
        {
            StatSystemCore.OnStatBaseValueChanged += callback;
        }
        
        public void RemoveOnBaseStatUpdatedEventCallback(Action<StatInfo, float> callback)
        {
            StatSystemCore.OnStatBaseValueChanged -= callback;
        }
        
        public abstract string GetUniqueObjectKey();
        protected virtual string GetPersonalTableKey() => $"{PersonalTablePrefix}{GetUniqueObjectKey()}";
        
    }
}
