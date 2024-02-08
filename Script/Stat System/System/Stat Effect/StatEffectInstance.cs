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
            get => _headDuration;
            set
            {
                _headDuration = value;
                if (CurrentStackDuration.Count == 0)
                {
                    CurrentStackDuration.Add(value);
                }
                else
                {
                    CurrentStackDuration[0] = _headDuration;
                }
            }
        }

        public int MaxStack;
        public bool UseStacking;
        public StatEffectProfile.StackOutPolicy StackOutPolicy;
        public StatEffectProfile.StackDurationPolicy StackDurationPolicy;
        
        public readonly List<float> CurrentStackDuration = new();
        private float _headDuration;
        private int _currentStackCnt = 1;
        
        public float GetRepresentDuration()
        {
            return DurationPolicy switch
            {
                StatEffectProfile.DurationPolicy.Manual => CurrentStackDuration.Max(),
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
                        var minDur = CurrentStackDuration.Max();
                        var minIdx = -1;

                        for (var i = 0; i < CurrentStackDuration.Count; i++)
                        {
                            var isThisValMin = minDur > CurrentStackDuration[i];
                            if (!isThisValMin) continue;

                            minDur = CurrentStackDuration[i];
                            minIdx = i;
                        }

                        if (minIdx >= 0)
                        {
                            CurrentStackDuration[minIdx] = fxInstance.DefinedDuration;
                        }

                        break;
                    case StatEffectProfile.StackDurationPolicy.Combined:
                        CurrentStackDuration[0] = fxInstance.DefinedDuration;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return StatEffectUpdateResult.Refresh;
            }

            _currentStackCnt += 1;
            CurrentStackDuration.Add(fxInstance.DefinedDuration);
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
                        CurrentStackDuration[i] -= t;
                    }

                    break;
                case StatEffectProfile.StackDurationPolicy.Combined:
                    CurrentStackDuration[0] -= t;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var removedCnt = CurrentStackDuration.RemoveAll(dur => dur <= 0);
            var isRemovedStack = removedCnt > 0;

            _headDuration = CurrentStackDuration.Count > 0 ? CurrentStackDuration[0] : 0.0f;

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
