using System;
using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.TagSystem;

namespace GeneralGameDevKit.StatSystem
{
    public class StatEffectInstance
    {
        public event Action OnExpired;
        public readonly BaseStatObject casterObject;
        
        public readonly string groupId;
        public readonly string effectId;
        
        public readonly string effectIconId;
        public readonly string effectName;
        
        public readonly List<DevKitTag> effectTagsToApply = new();
        public readonly List<StatModifier> modifiersToApply = new();

        public readonly StatEffectProfile.DurationPolicy durationPolicyApply;
        public readonly float durationApply;
        
        public readonly int maxStack;
        public bool useStackingApply;
        public readonly StatEffectProfile.StackOutPolicy stackOutPolicyApply;
        public readonly StatEffectProfile.StackDurationPolicy stackDurationPolicyApply;
        
        public readonly List<float> currentStackDuration = new();
        private int currentStackCnt;

        public float GetRepresentDuration()
        {
            return durationPolicyApply switch
            {
                StatEffectProfile.DurationPolicy.Manual => currentStackDuration.Max(),
                StatEffectProfile.DurationPolicy.Infinite => float.MaxValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public int GetCurrentStackCount() => currentStackCnt;

        public bool AddStack()
        {
            if (!useStackingApply)
                return false;
            if (currentStackCnt >= maxStack)
                return false;

            currentStackCnt += 1;
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
            switch (stackDurationPolicyApply)
            {
            case StatEffectProfile.StackDurationPolicy.Independent:
                for (var i = 0; i < maxStack; i++)
                {
                    currentStackDuration[i] -= t;
                }
                break;
            case StatEffectProfile.StackDurationPolicy.Combined:
                currentStackDuration[0] -= t;
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }

            var removedCnt = currentStackDuration.RemoveAll(dur => dur <= 0);
            var isRemovedStack = removedCnt > 0;

            if (isRemovedStack)
            {
                switch (stackDurationPolicyApply)
                {
                case StatEffectProfile.StackDurationPolicy.Independent:
                    currentStackCnt -= removedCnt;
                    break;
                case StatEffectProfile.StackDurationPolicy.Combined:
                    switch (stackOutPolicyApply)
                    {
                    case StatEffectProfile.StackOutPolicy.RemoveSingleStack:
                        currentStackCnt -= 1;
                        break;
                    case StatEffectProfile.StackOutPolicy.ClearAllStack:
                        currentStackCnt = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }

            if (currentStackCnt <= 0)
            {
                NoticeExpired();
            }

            return currentStackCnt == 0;
        }
    }
}
