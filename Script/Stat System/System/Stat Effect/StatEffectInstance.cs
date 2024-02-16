using System;
using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.TagSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    public class StatEffectInstance
    {
        public event Action<StatEffectInstance> OnExpired;
        public event Action<StatEffectInstance, int> OnRemovedStack;
        public BaseStatObject CasterObject;
        
        public string GroupId;
        public string EffectId;
        
        public string EffectName;
        public string EffectDesc;
        public string EffectIconId;
        
        public List<DevKitTag> EffectTagsToApply = new();
        public List<StatModifier> ModifiersToApply = new();

        public StatEffectProfile.DurationPolicy DurationPolicy;

        public float DefinedDuration
        {
            get => _definedDuration;
            set
            {
                _definedDuration = Mathf.Max(value, 0);
                for (var i = 0; i < CurrentDurationsEachStack.Count; i++)
                {
                    CurrentDurationsEachStack[i] = Mathf.Min(CurrentDurationsEachStack[i], _definedDuration);
                }
            }
        }

        public int MaxStack;
        public bool UseStacking;
        public StatEffectProfile.StackOutPolicy StackOutPolicy;
        public StatEffectProfile.StackDurationPolicy StackDurationPolicy;
        
        public readonly List<float> CurrentDurationsEachStack = new();
        private float _definedDuration;
        private int _currentStackCnt = 1;

        public StatEffectInstance(float initialDuration)
        {
            DefinedDuration = initialDuration;
            CurrentDurationsEachStack.Add(DefinedDuration);
        }
        
        public float GetRepresentDuration()
        {
            return DurationPolicy switch
            {
                StatEffectProfile.DurationPolicy.Manual => CurrentDurationsEachStack.Min(),
                StatEffectProfile.DurationPolicy.Infinite => float.MaxValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public int GetCurrentStackCount() => _currentStackCnt;

        public StatEffectUpdateResult TryAddStack(StatEffectInstance fxInstance)
        {
            if (!UseStacking)
                return StatEffectUpdateResult.AddFail;

            if (!fxInstance.EffectId.Equals(EffectId))
                return StatEffectUpdateResult.AddFail;

            var isStackCntFull = _currentStackCnt >= MaxStack;

            switch (fxInstance.StackDurationPolicy)
            {
                case StatEffectProfile.StackDurationPolicy.Independent when !isStackCntFull:
                    _currentStackCnt += 1;
                    CurrentDurationsEachStack.Add(fxInstance.DefinedDuration);
                    return StatEffectUpdateResult.Stack;
                case StatEffectProfile.StackDurationPolicy.Independent:
                    var minDur = CurrentDurationsEachStack.Max();
                    var minIdx = -1;

                    for (var i = 0; i < CurrentDurationsEachStack.Count; i++)
                    {
                        var isThisValMin = minDur > CurrentDurationsEachStack[i];
                        if (!isThisValMin) continue;

                        minDur = CurrentDurationsEachStack[i];
                        minIdx = i;
                    }

                    if (minIdx >= 0)
                    {
                        CurrentDurationsEachStack[minIdx] = fxInstance.DefinedDuration;
                    }
                    return StatEffectUpdateResult.ResetDuration;
                case StatEffectProfile.StackDurationPolicy.Combined:
                    if (!isStackCntFull)
                        _currentStackCnt += 1;
                    CurrentDurationsEachStack[0] = fxInstance.DefinedDuration;
                    return isStackCntFull ? StatEffectUpdateResult.ResetDuration : StatEffectUpdateResult.Stack;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Tick duration
        /// </summary>
        /// <param name="t">t value to tick</param>
        /// <returns>
        /// Is effect expired
        /// </returns>
        public bool TickDuration(float t)
        {
            if (DurationPolicy is StatEffectProfile.DurationPolicy.Infinite)
                return false;

            switch (StackDurationPolicy)
            {
                case StatEffectProfile.StackDurationPolicy.Independent:
                    for (var i = 0; i < CurrentDurationsEachStack.Count; i++)
                    {
                        CurrentDurationsEachStack[i] -= t;
                    }
                    var removedCnt = CurrentDurationsEachStack.RemoveAll(dur => dur <= 0);
                    _currentStackCnt -= removedCnt;
                    break;
                case StatEffectProfile.StackDurationPolicy.Combined:
                    CurrentDurationsEachStack[0] -= t;
                    if (CurrentDurationsEachStack[0] <= 0)
                    {
                        switch (StackOutPolicy)
                        {
                            case StatEffectProfile.StackOutPolicy.RemoveSingleStack:
                                _currentStackCnt -= 1;
                                if (_currentStackCnt > 0)
                                {
                                    CurrentDurationsEachStack[0] = DefinedDuration;
                                    OnRemovedStack?.Invoke(this, 1);
                                }
                                break;
                            case StatEffectProfile.StackOutPolicy.ClearAllStack:
                                _currentStackCnt = 0;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var isExpired = _currentStackCnt <= 0;
            if (isExpired)
            {
                OnExpired?.Invoke(this);
            }
            return isExpired;
        }

        public bool ForceRemoveStack(int stackToRemove)
        {
            _currentStackCnt -= stackToRemove;
            return _currentStackCnt <= 0;
        }

        public void ResetDuration()
        {
            for (var i = 0; i < CurrentDurationsEachStack.Count; i++)
            {
                CurrentDurationsEachStack[i] = DefinedDuration;
            }
        }

        public enum StatEffectUpdateResult
        {
            Add,
            Remove,
            Stack,
            RemoveStack,
            ResetDuration,
            Refresh,
            AddFail
        }
    }
}
