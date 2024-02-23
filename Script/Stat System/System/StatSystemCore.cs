using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Dictionary<string, HashSet<StatConstraints>> _statConstraints = new();
        private readonly Dictionary<string, HashSet<StatConstraints>> _statConstraintsDependencies = new();
        private readonly Dictionary<string, StatCustomGetter> _statCustomGetters = new();
        
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

        public List<StatModifier> GetAllStatModifiers() => new(_currentModifiers);

        public void AddStatConstraints(StatConstraints constraintToAdd)
        {
            var targetStatID = constraintToAdd.targetStatID;
            if (!_statMap.ContainsKey(targetStatID))
            {
                AddNewStat(targetStatID, 0.0f);
            }
            
            if (!_statConstraints.ContainsKey(targetStatID))
            {
                _statConstraints.Add(targetStatID, new HashSet<StatConstraints>());
            }
            _statConstraints[targetStatID].Add(constraintToAdd);
            ModifyStatBaseValue(targetStatID, GetBaseValue(targetStatID));
        }

        public void SetCustomGetter(StatCustomGetter customGetter, string targetStat)
        {
            _statCustomGetters[targetStat] = customGetter;
        }

        public void RemoveCustomGetter(string targetStat)
        {
            _statCustomGetters.Remove(targetStat);
        }
        
        public void RemoveStatConstraints(StatConstraints constraintToRemove)
        {
            var targetStatID = constraintToRemove.targetStatID;
            if (!_statConstraints.ContainsKey(targetStatID))
                return;

            _statConstraints[targetStatID].Remove(constraintToRemove);
        }

        public void AddConstraintDependency(string targetReactId, StatConstraints constraintsToAdd)
        {
            if (!_statMap.ContainsKey(targetReactId))
            {
                AddNewStat(targetReactId, 0.0f);
            }

            if (!_statMap.ContainsKey(constraintsToAdd.targetStatID))
            {
                AddNewStat(constraintsToAdd.targetStatID, 0.0f);
            }
            
            if (targetReactId.Equals(constraintsToAdd.targetStatID))
            {
                Debug.LogWarning("Recursive dependency is not allowed. Add failed");
                return;
            }
            
            if (_statConstraintsDependencies.ContainsKey(constraintsToAdd.targetStatID) && _statConstraintsDependencies.ContainsKey(targetReactId))
            {
                Debug.LogWarning("Cyclic dependency detected. Add failed");
                return;
            }
            
            if (!_statConstraintsDependencies.ContainsKey(targetReactId))
            {
                _statConstraintsDependencies.Add(targetReactId, new HashSet<StatConstraints>());
            }
            _statConstraintsDependencies[targetReactId].Add(constraintsToAdd);
            ResolveConstraintsDependency(targetReactId);
        }

        public void RemoveConstraintDependency(string targetReactId, StatConstraints constrainsToRemove)
        {
            if (!_statConstraints.ContainsKey(targetReactId))
                return;

            _statConstraints[targetReactId].Remove(constrainsToRemove);
        }
        
        /// <summary>
        /// Add stat modifier
        /// </summary>
        /// <param name="mod"></param>
        public void AddStatModifier(StatModifier mod)
        {
            if (!_statMap.ContainsKey(mod.TargetStatID))
            {
                AddNewStat(mod.TargetStatID, 0.0f);
            }
            
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

        public StatInfo GetStatInfo(string target)
        {
            _statMap.TryGetValue(target, out var ret);
            return ret;
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

        public int RemoveAllStatModifiers(StatModifier baseMod)
        {
            var removeCnt = 0;
            while (_currentModifiers.Remove(baseMod))
            {
                removeCnt++;
            }
            
            if (_statMap.TryGetValue(baseMod.TargetStatID, out var targetStatInfo))
            {
                OnStatApplyValueChanged?.Invoke(targetStatInfo, GetStatApplyValue(baseMod.TargetStatID));
            }

            return removeCnt;
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
                AddNewStat(targetID, 0.0f);
                targetStat = _statMap[targetID];
            }

            var prevValue = targetStat.StatValue;
            if (_statConstraints.TryGetValue(targetID, out var constraints))
            {
                value = constraints
                    .Where(c => c.isBaseStatConstraintsActivated)
                    .Aggregate(value, (current, constraint) => constraint.ProcessBaseStat(this, current));
            }

            targetStat.StatValue = value;
            ResolveConstraintsDependency(targetID);
            
            OnStatBaseValueChanged?.Invoke(targetStat, prevValue);
            OnStatApplyValueChanged?.Invoke(targetStat, GetStatApplyValue(targetStat.ID));
        }

        public void AddNewStat(string targetID, float initialValue)
        {
            var newStat = new StatInfo
            {
                ID = targetID,
                StatValue = initialValue
            };
            _statMap.Add(targetID, newStat);
            Debug.Log($"New stat value added. [{targetID} : {initialValue.ToString(CultureInfo.InvariantCulture)}]");
        }

        private readonly Dictionary<string, float> _pendedBaseValueChanges = new();
        public void ResolveConstraintsDependency(string reactId)
        {
            if (!_statConstraintsDependencies.TryGetValue(reactId, out var constraintsList))
                return;
            _pendedBaseValueChanges.Clear();
            
            foreach (var constraints in constraintsList)
            {
                if (!_statMap.TryGetValue(constraints.targetStatID, out var targetStat)) 
                    return;
                _pendedBaseValueChanges.TryAdd(targetStat.ID, targetStat.StatValue);
                targetStat.StatValue = constraints.ProcessBaseStat(this, targetStat.StatValue);
            }

            foreach (var (id, prev) in _pendedBaseValueChanges)
            {
                OnStatBaseValueChanged?.Invoke(_statMap[id], prev);
                OnStatApplyValueChanged?.Invoke(_statMap[id], GetStatApplyValue(id));
            }
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
            
            AddNewStat(targetId, 0f);
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
            var isCustomGetterExist = _statCustomGetters.TryGetValue(targetId, out var customGetter);

            if (!isCustomGetterExist && !_statMap.TryGetValue(targetId, out _))
            {
                Debug.Log($"There is no matching stat value.: {targetId}"); //todo : replace to error handling method
                return 0.0f;
            }

            var baseValue = GetBaseValue(targetId);
            var ret = isCustomGetterExist ? customGetter.GetProcessedValue(this, targetId, baseValue) : baseValue;
            
            if (isCustomGetterExist)
            {
                if (!customGetter.IgnoreModifier)
                {
                    FillModifiers(customGetter.UseModifierHooks ? customGetter.StatIdToHookModifier : targetId);
                }
            }
            else
            {
                FillModifiers(targetId);
            }
            ProcessModifiers();
            ProcessConstraints();
            return ret;

            //---- local methods
            void ProcessConstraints()
            {
                if (_statConstraints.TryGetValue(targetId, out var constraints))
                {
                    ret = constraints
                        .Where(c => c.isApplyStatConstraintsActivated)
                        .Aggregate(ret, (current, constraint) => constraint.ProcessApplyStat(this, current));
                }
            }

            void ProcessModifiers()
            {
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
            }

            void FillModifiers(string targetStatId)
            {
                _modifiersForCalc.Clear();
                _modifiersForCalc.AddRange(_currentModifiers.Where(m => m.TargetStatID.Equals(targetStatId)));
                _modifiersForCalc.Sort((ma, mb) =>
                {
                    var firstCompare = mb.Priority.CompareTo(ma.Priority);
                    return firstCompare != 0 ? firstCompare : ma.TimeStamp.CompareTo(mb.TimeStamp);
                });
            }
        }

        private float CalcTwoValue(float valA, float valB, StatCalcOperator operatorFlag)
        {
            return operatorFlag switch
            {
                StatCalcOperator.Add => valA + valB,
                StatCalcOperator.Mul => valA * valB,
                StatCalcOperator.Div => valA / valB,
                StatCalcOperator.Sub => valA - valB,
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
        Div,
        Sub
    }
}
