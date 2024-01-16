using System;
using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.TagSystem;

namespace GeneralGameDevKit.StatSystem
{
    public class StatEffectInstance
    {
        public event Action OnExpired;
        public BaseStatObject CasterObject;
        
        public string GroupId;
        public string EffectId;
        
        public string EffectName;
        public string EffectDesc;
        public string EffectIconId;
        
        public List<DevKitTag> EffectTagsToApply = new();
        public List<StatModifier> ModifiersToApply = new();

        public StatEffectProfile.DurationPolicy DurationPolicy;
        public float Duration;
        
        public int MaxStack;
        public bool UseStacking;
        public StatEffectProfile.StackOutPolicy StackOutPolicy;
        public StatEffectProfile.StackDurationPolicy StackDurationPolicy;
        
        public readonly List<float> CurrentStackDuration = new();
        private int _currentStackCnt;

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

        public bool AddStack()
        {
            if (!UseStacking)
                return false;
            if (_currentStackCnt >= MaxStack)
                return false;

            _currentStackCnt += 1;
            return true;
        }

        private void NoticeExpired()
        {
            OnExpired?.Invoke();
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

            if (_currentStackCnt <= 0)
            {
                NoticeExpired();
            }

            return _currentStackCnt == 0;
        }
    }
}
