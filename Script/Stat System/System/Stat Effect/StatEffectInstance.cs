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
                StatEffectProfile.DurationPolicy.Manual => CurrentDurationsEachStack.Count <= 0 ? 0 : CurrentDurationsEachStack.Max(),
                StatEffectProfile.DurationPolicy.Infinite => float.MaxValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public int GetCurrentStackCount() => _currentStackCnt;

        public StatEffectUpdateResult TryAddStack(StatEffectInstance fxInstance)
        {
            if (!UseStacking)
                return StatEffectUpdateResult.Fail;

            if (!fxInstance.EffectId.Equals(EffectId))
                return StatEffectUpdateResult.Fail;

            if (_currentStackCnt >= MaxStack)
            {//refresh duration
                switch (fxInstance.StackDurationPolicy)
                {
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
                        break;
                    case StatEffectProfile.StackDurationPolicy.Combined:
                        CurrentDurationsEachStack[0] = fxInstance.DefinedDuration;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return StatEffectUpdateResult.Refresh;
            }

            _currentStackCnt += 1;
            CurrentDurationsEachStack.Add(fxInstance.DefinedDuration);
            return StatEffectUpdateResult.Stack;
        }

        private void NoticeExpired()
        {
            OnExpired?.Invoke(this);
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
                    for (var i = 0; i < MaxStack; i++)
                    {
                        CurrentDurationsEachStack[i] -= t;
                    }

                    break;
                case StatEffectProfile.StackDurationPolicy.Combined:
                    CurrentDurationsEachStack[0] -= t;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var removedCnt = CurrentDurationsEachStack.RemoveAll(dur => dur <= 0);
            var isRemovedStack = removedCnt > 0;

            DefinedDuration = CurrentDurationsEachStack.Count > 0 ? CurrentDurationsEachStack[0] : 0.0f;

            if (isRemovedStack)
            {
                switch (StackDurationPolicy)
                {
                    case StatEffectProfile.StackDurationPolicy.Independent:
                        _currentStackCnt -= removedCnt;
                        break;
                    case StatEffectProfile.StackDurationPolicy.Combined:
                        switch (StackOutPolicy)
                        {
                            case StatEffectProfile.StackOutPolicy.RemoveSingleStack:
                                _currentStackCnt -= 1;
                                break;
                            case StatEffectProfile.StackOutPolicy.ClearAllStack:
                                _currentStackCnt = 0;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var isExpired = _currentStackCnt == 0;
            if (isExpired)
                NoticeExpired();
            return isExpired;
        }

        public bool ForceRemoveStack(int stackToRemove)
        {
            _currentStackCnt -= stackToRemove;
            return _currentStackCnt <= 0;
        }

        public enum StatEffectUpdateResult
        {
            Add,
            Remove,
            Stack,
            RemoveStack,
            Refresh,
            Fail
        }
    }
}
