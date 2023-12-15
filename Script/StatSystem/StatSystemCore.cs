using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

public class StatSystemCore
{
    private Dictionary<string, StatInfo> _statMap = new();
    private List<StatModifier> _currentModifiers = new();
    
    private List<StatModifier> _modifiersForCalc = new();

    private uint _currentModifierTimestamp;

    /// <summary>
    /// Modify the base stat value.
    /// </summary>
    /// <param name="targetID"></param>
    /// <param name="value"></param>
    public void ModifyStatBaseValue(string targetID, float value)
    {
        if (!_statMap.TryGetValue(targetID, out var targetStat))
        {
            Debug.LogWarning("There is no matching stat value."); //todo : replace to error handling method
            return;
        }

        targetStat.value = value;
    }

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

    /// <summary>
    /// Add stat modifier
    /// </summary>
    /// <param name="mod"></param>
    public void AddStatModifier(StatModifier mod)
    {
        mod.TimeStamp = IssueTimestamp();
        _modifiersForCalc.Add(mod);
    }

    /// <summary>
    /// Get the stat value not modified. (Base Stat Value)
    /// </summary>
    /// <param name="targetId"></param>
    /// <returns></returns>
    public float GetBaseValue(string targetId)
    {
        if (!_statMap.TryGetValue(targetId, out var targetStat))
        {
            Debug.LogWarning("There is no matching stat value."); //todo : replace to error handling method
            return 0.0f;
        }

        return targetStat.value;
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

        var baseValue = targetStat.value;
        var ret = targetStat.value;
        foreach (var mod in _modifiersForCalc)
        {
            switch (mod.CalcChainType)
            {
                case StatModifier.ModifyChainType.CalcWithBase:
                    ret += CalcTwoValue(baseValue, mod.Coefficient, mod.CalcOperator);
                    break;
                case StatModifier.ModifyChainType.CalcWithResult:
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
            StatCalcOperator.Add => valA+valB,
            StatCalcOperator.Mul => valA*valB,
            StatCalcOperator.Div => valA/valB,
            _ => throw new ArgumentOutOfRangeException(nameof(operatorFlag), operatorFlag, null)
        };
    }
}

/// <summary>
/// Stat Information.
/// </summary>
public class StatInfo
{
    public string ID; //Unique Id for indicate stat. (hp, atk, def.... or unique numbers)
    public float value;
    
    //todo : tag of stats
}

/// <summary>
/// Modifier Of Stat. It can be used for temporarily buffs.
/// </summary>
public class StatModifier
{
    public uint TimeStamp; //In same priority, smaller will run first. It will be assigned by core system.
    public int Priority; //Bigger will run first.
    
    public string TargetStatID; //This id must be unique. It will used for find specific modifier.
    public float Coefficient; //Value for calculation.
    
    public StatCalcOperator CalcOperator;
    public ModifyChainType CalcChainType; //Type of calculate chain.

    public enum ModifyChainType
    {
        CalcWithBase, //calculate with base value, and result will be add to apply stat value.
        CalcWithResult //calculate with result value.
    }
}

//Operator for calculation.
public enum StatCalcOperator
{
    Add,
    Mul,
    Div
}