using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace GeneralGameDevKit.StatSystem
{
    /// <summary>
    /// Core class for using 'Stat system'.
    /// Stat system is used to manage stat values such as hp, atk, and so on. 
    /// It can be used its own or you can make extended system with this class.
    /// </summary>
    public class StatSystemCore
    {
        public event Action<StatInfo, float> OnStatBaseValueChanged; //p0: statInfo, p1: prev-value
        public event Action<StatInfo, float> OnStatApplyValueChanged; //p0: statInfo, p1 :apply-value
         
        private readonly Dictionary<string, StatInfo> _statMap = new();
        private readonly List<StatModifier> _currentModifiers = new();
        private uint _currentModifierTimestamp;

        private readonly List<StatModifier> _modifiersForCalc = new();
        
        /// <summary>
        /// Get all values of stats.
        /// It will provide as 'apply' value.
        /// You must use ref param to avoid GC.
        /// </summary>
        /// <param name="valueCollection">collections to receive stat values. [k:stat id, v:apply value of stat]</param>
        public void GetAllApplyStats(ref Dictionary<string, float> valueCollection)
        {
            valueCollection.Clear();
            foreach (var (statId, _) in _statMap)
            {
                valueCollection.Add(statId, GetStatApplyValue(statId));
            }
        }

        public void GetAllStatModifiers(ref List<StatModifier> collectionToReceive)
        {
            collectionToReceive.AddRange(_modifiersForCalc);
        }

        /// <summary>
        /// Add stat modifier
        /// </summary>
        /// <param name="mod"></param>
        public void AddStatModifier(StatModifier mod)
        {
            switch (mod.ModPolicy)
            {
                case StatModifier.ModificationPolicy.Instant:
                    var valueToApply = mod.CalcPolicy switch
                    {
                        StatModifier.ModCalculationPolicy.CalcWithBase => GetBaseValue(mod.TargetStatID),
                        StatModifier.ModCalculationPolicy.CalcWithResult => GetStatApplyValue(mod.TargetStatID),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    valueToApply = CalcTwoValue(valueToApply, mod.Coefficient, mod.CalcOperator);
                    ModifyStatBaseValue(mod.TargetStatID, valueToApply);
                    break;
                case StatModifier.ModificationPolicy.Temporary:
                    mod.TimeStamp = IssueTimestamp();
                    _currentModifiers.Add(mod);
                    if (_statMap.TryGetValue(mod.TargetStatID, out var statInfo))
                    {
                        OnStatApplyValueChanged?.Invoke(statInfo, GetStatApplyValue(mod.TargetStatID));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        }

        public bool RemoveStatModifier(StatModifier mod)
        {
            var removeResult = _currentModifiers.Remove(mod);
            if (_statMap.TryGetValue(mod.TargetStatID, out var targetStatInfo))
            {
                OnStatApplyValueChanged?.Invoke(targetStatInfo, GetStatApplyValue(mod.TargetStatID));
            }
            return removeResult;
        }

        /// <summary>
        /// Modify the base stat value.
        /// </summary>
        /// <param name="targetID">target stat ID</param>
        /// <param name="value"></param>
        public void ModifyStatBaseValue(string targetID, float value)
        {
            if (!_statMap.TryGetValue(targetID, out var targetStat))
            {
                targetStat = new StatInfo
                {
                    ID = targetID,
                    StatValue = 0
                };
                _statMap.Add(targetID, targetStat);
                Debug.Log($"New stat value added. [{targetID} : {value.ToString(CultureInfo.InvariantCulture)}]");
            }

            var prevValue = targetStat.StatValue;
            targetStat.StatValue = value;
            OnStatBaseValueChanged?.Invoke(targetStat, prevValue);
            OnStatApplyValueChanged?.Invoke(targetStat, GetStatApplyValue(targetStat.ID));
        }

        /// <summary>
        /// Get the stat value not modified. (Base Stat Value)
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public float GetBaseValue(string targetId)
        {
            if (_statMap.TryGetValue(targetId, out var targetStat)) 
                return targetStat.StatValue;
            
            Debug.LogWarning("There is no matching stat value."); //todo : replace to error handling method
            return 0.0f;

        }

        /// <summary>
        /// Get the stat value modified by all modifiers.
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float GetStatApplyValue(string targetId)
        {
            if (!_statMap.TryGetValue(targetId, out var targetStat))
            {
                Debug.LogWarning("There is no matching stat value."); //todo : replace to error handling method
                return 0.0f;
            }

            _modifiersForCalc.Clear();
            _modifiersForCalc.AddRange(_currentModifiers.Where(m => m.TargetStatID.Equals(targetId)));
            _modifiersForCalc.Sort((ma, mb) =>
            {
                var firstCompare = mb.Priority.CompareTo(ma.Priority);
                return firstCompare != 0 ? firstCompare : ma.TimeStamp.CompareTo(mb.TimeStamp);
            });

            var baseValue = targetStat.StatValue;
            var ret = targetStat.StatValue;
            foreach (var mod in _modifiersForCalc)
            {
                switch (mod.CalcPolicy)
                {
                    case StatModifier.ModCalculationPolicy.CalcWithBase:
                        ret += CalcTwoValue(baseValue, mod.Coefficient, mod.CalcOperator);
                        break;
                    case StatModifier.ModCalculationPolicy.CalcWithResult:
                        ret = CalcTwoValue(ret, mod.Coefficient, mod.CalcOperator);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return ret;
        }

        private float CalcTwoValue(float valA, float valB, StatCalcOperator operatorFlag)
        {
            return operatorFlag switch
            {
                StatCalcOperator.Add => valA + valB,
                StatCalcOperator.Mul => valA * valB,
                StatCalcOperator.Div => valA / valB,
                _ => throw new ArgumentOutOfRangeException(nameof(operatorFlag), operatorFlag, null)
            };
        }
        
        /// <summary>
        /// Time stamp used to sorting in stat calculation.
        /// It is second comparision factor.
        /// </summary>
        /// <returns></returns>
        private uint IssueTimestamp()
        {
            if (_currentModifierTimestamp == uint.MaxValue)
            {
                for (var i = 0; i < _currentModifiers.Count; i++)
                {
                    _currentModifiers[i].TimeStamp = (uint) i;
                }

                _currentModifierTimestamp = _currentModifiers.Max(c => c.TimeStamp) + 1;
            }

            var ret = _currentModifierTimestamp;
            _currentModifierTimestamp += 1;
            return ret;
        }
    }

    /// <summary>
    /// Stat Information.
    /// </summary>
    public class StatInfo
    {
        public string ID; //Unique Id for indicate stat. (hp, atk, def.... or unique numbers)
        public float StatValue;
    }

    /// <summary>
    /// Operator value for stat calculation.
    /// </summary>
    public enum StatCalcOperator
    {
        Add,
        Mul,
        Div
    }
}
