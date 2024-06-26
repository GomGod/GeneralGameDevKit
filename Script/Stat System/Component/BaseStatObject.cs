using System;
using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.KeyTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    public abstract class BaseStatObject : MonoBehaviour
    {
        [SerializeField] protected int id;
        [SerializeField] protected DevKitTagContainer permanentTags;
        [SerializeField] protected List<StatConstraintsSO> statConstraints;
        [SerializeField] protected List<StatCustomGetterSO> statCustomGetters;

        protected readonly Dictionary<string, Action<StatInfo, float>> ApplyStatChangedCallbacks = new();
        protected Dictionary<string, Action<StatInfo, float>> BaseStatChangedCallbacks = new();
        
        protected const string PersonalTablePrefix = "%StatSystem%ptable%";
        
        protected readonly DevKitTagContainer TemporaryTags = new();
        protected readonly DevKitTagContainer ManualAddedTags = new();
        
        protected readonly List<StatEffectInstance> CurrentEffectInstances = new();
        protected readonly List<StatEffectInstance> TempCollectionToStatEffectProcessing = new();
        
        protected List<StatModifier> TemporaryModifiers = new();
        
        public readonly StatSystemCore StatSystemCore = new();

        public event Action<StatEffectInstance, StatEffectInstance.StatEffectUpdateResult> OnStatEffectUpdate;
        
        protected void Awake()
        {
            KeyValueTableManager.Instance.AddNewTable(GetPersonalTableKey());
            foreach (var constraintsSO in statConstraints)
            {
                constraintsSO.ApplyConstraintsToSystem(StatSystemCore);
            }

            foreach (var customGetterSO in statCustomGetters)
            {
                customGetterSO.ApplyCustomGetterToSystem(StatSystemCore);
            }

            StatSystemCore.OnStatApplyValueChanged -= BroadcastApplyStatChangedEvent;
            StatSystemCore.OnStatApplyValueChanged += BroadcastApplyStatChangedEvent;
            StatSystemCore.OnStatBaseValueChanged -= BroadcastBaseStatChangedEvent;
            StatSystemCore.OnStatBaseValueChanged += BroadcastBaseStatChangedEvent;
        }

        public virtual void InitializeTask()
        {
        }

        public void ForceInvokeStatUpdateEvent(string targetStat)
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

        public List<StatEffectInstance> GetAllStatEffectInstances() => new(CurrentEffectInstances);
        public List<StatModifier> FindModifiersWithTargetStat(string identifier) => new(StatSystemCore.GetAllStatModifiers().Where(m => m.TargetStatID.Equals(identifier)));
        
        public bool ApplyStatEffect(StatEffectInstance instanceToAdd)
        {
            TemporaryModifiers.Clear();
            StatSystemCore.GetAllStatModifiers(ref TemporaryModifiers);

            var isStackable = instanceToAdd.UseStacking;
            if (isStackable)
            {
                //find stackable effect and try add stack
                var targetIdx = CurrentEffectInstances.FindIndex(i => i.EffectId.Equals(instanceToAdd.EffectId));
                if (targetIdx >= 0)
                {
                    var targetInstance = CurrentEffectInstances[targetIdx];
                    var result = targetInstance.TryAddStack(instanceToAdd);
                    if (result is StatEffectInstance.StatEffectUpdateResult.Stack)
                    {
                        ProcessStackedStatEffectInstance(targetInstance);
                    }
                    OnStatEffectUpdate?.Invoke(CurrentEffectInstances[targetIdx], result);
                    return result is StatEffectInstance.StatEffectUpdateResult.Stack or StatEffectInstance.StatEffectUpdateResult.ResetDuration;
                }

                //first stack of effect
                instanceToAdd.OnRemovedStack += OnRemovedEffectStack;
                AddStatEffectInstance(instanceToAdd);
                OnStatEffectUpdate?.Invoke(instanceToAdd, StatEffectInstance.StatEffectUpdateResult.Add);
                return true;
            }

            //or add new effect instance
            AddStatEffectInstance(instanceToAdd);
            OnStatEffectUpdate?.Invoke(instanceToAdd, StatEffectInstance.StatEffectUpdateResult.Add);
            return true;
        }

        public void AddManualTag(DevKitTag tag, int cnt =1)
        {
            for (var i = 0; i < cnt; i++)
            {
                ManualAddedTags.AddTag(tag);
            }
        }

        public void RemoveManualTag(DevKitTag tag, int cnt =1)
        {
            for (var i = 0; i < cnt; i++)
            {
                ManualAddedTags.RemoveTag(tag);
            }
        }

        public int GetTagCount(DevKitTag tag)
        {
            return permanentTags.GetTagCount(tag) + TemporaryTags.GetTagCount(tag) + ManualAddedTags.GetTagCount(tag);
        }

        public void GetApplyTags(ref DevKitTagContainer tagCollectionToReceive)
        {
            tagCollectionToReceive.ClearContainer();
            tagCollectionToReceive.MergeContainer(permanentTags);
            tagCollectionToReceive.MergeContainer(ManualAddedTags);
            tagCollectionToReceive.MergeContainer(TemporaryTags);
        }

        public DevKitTagContainer GetApplyTags()
        {
            var ret = new DevKitTagContainer();
            GetApplyTags(ref ret);
            return ret;
        }

        private void OnRemovedEffectStack(StatEffectInstance instance, int removedCnt)
        {
            if (!CurrentEffectInstances.Contains(instance))
                return;
            
            for (var i = 0; i < removedCnt; i++)
            {
                RemoveModifiersInStatEffect(instance);
            }
            OnStatEffectUpdate?.Invoke(instance, StatEffectInstance.StatEffectUpdateResult.RemoveStack);
        }

        protected void AddStatEffectInstance(StatEffectInstance instanceToAdd)
        {
            instanceToAdd.OnExpired += RemoveStatEffect;
            foreach (var modifier in instanceToAdd.ModifiersToApply)
            {
                StatSystemCore.AddStatModifier(modifier);
            }
            TemporaryTags.AddTags(instanceToAdd.EffectTagsToApply);
            CurrentEffectInstances.Add(instanceToAdd);
        }

        protected void ProcessStackedStatEffectInstance(StatEffectInstance instanceToStack)
        {
            foreach (var modifier in instanceToStack.ModifiersToApply)
            {
                StatSystemCore.AddStatModifier(modifier);
            }
            TemporaryTags.AddTags(instanceToStack.EffectTagsToApply);
        }

        public void RemoveStatEffect(StatEffectInstance instanceToRemove)
        {
            foreach (var modifier in instanceToRemove.ModifiersToApply)
            {
                StatSystemCore.RemoveAllStatModifiers(modifier);
            }

            TemporaryTags.RemoveTags(instanceToRemove.EffectTagsToApply);
            CurrentEffectInstances.Remove(instanceToRemove);
            OnStatEffectUpdate?.Invoke(instanceToRemove, StatEffectInstance.StatEffectUpdateResult.Remove);
        }

        public void RemoveModifiersInStatEffect(StatEffectInstance instanceSource)
        {
            foreach (var modifier in instanceSource.ModifiersToApply)
            {
                StatSystemCore.RemoveStatModifier(modifier);
            }

            TemporaryTags.RemoveTags(instanceSource.EffectTagsToApply);
        }

        public void TickEffectDuration(float t)
        {
            TempCollectionToStatEffectProcessing.Clear();
            TempCollectionToStatEffectProcessing.AddRange(CurrentEffectInstances);
            
            foreach (var fxInstance in TempCollectionToStatEffectProcessing)
            {
                fxInstance.TickDuration(t);
                OnStatEffectUpdate?.Invoke(fxInstance, StatEffectInstance.StatEffectUpdateResult.Refresh);
            }
        }

        private void BroadcastApplyStatChangedEvent(StatInfo statInfo, float val)
        {
            if (!ApplyStatChangedCallbacks.ContainsKey(statInfo.ID))
                return;
            
            ApplyStatChangedCallbacks[statInfo.ID]?.Invoke(statInfo, val);
        }
        
        private void BroadcastBaseStatChangedEvent(StatInfo statInfo, float val)
        {
            if (!ApplyStatChangedCallbacks.ContainsKey(statInfo.ID))
                return;
            
            ApplyStatChangedCallbacks[statInfo.ID]?.Invoke(statInfo, val);
        }

        public void SetOnTagUpdatedEventCallback(Action<DevKitTag, int, int> callback)
        {
            permanentTags.OnTagUpdate += callback;
            TemporaryTags.OnTagUpdate += callback;
        }

        public void RemoveOnTagUpdatedEventCallback(Action<DevKitTag, int, int> callback)
        {
            permanentTags.OnTagUpdate -= callback;
            TemporaryTags.OnTagUpdate -= callback;
        }

        public void SetOnApplyStatUpdatedEventCallback(string targetId, Action<StatInfo, float> callback)
        {
            if (!ApplyStatChangedCallbacks.ContainsKey(targetId))
            {
                ApplyStatChangedCallbacks.Add(targetId, callback);
                return;
            }
            ApplyStatChangedCallbacks[targetId] += callback;
        }
        
        public bool RemoveOnApplyStatUpdatedEventCallback(string targetId, Action<StatInfo, float> callback)
        {
            if (!ApplyStatChangedCallbacks.ContainsKey(targetId))
                return false;

            ApplyStatChangedCallbacks[targetId] -= callback;
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
