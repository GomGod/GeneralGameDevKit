using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    //todo
    //permanent tags
    //current applying stat effects
    //modify context processing
    //modifier processing (buff/debuff)
    public abstract class BaseStatObject : MonoBehaviour
    {
        protected const string PersonalTablePrefix = "%StatSystem%ptable%";
        
        [SerializeField] protected DevKitTagContainer _permanentTags;
        
        protected readonly StatSystemCore StatSystemCore = new();
        
        public void Awake()
        {
            KeyValueTableManager.Instance.AddNewTable(GetPersonalTableKey());
            InitializeTask();
        }

        public void InitializeStats(Dictionary<string, float> statCollections)
        {
            foreach (var (id, val) in statCollections)
            {
                StatSystemCore.ModifyStatBaseValue(id, val);
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
        
        public virtual void InitializeTask() {}
        public abstract string GetUniqueObjectKey();
        protected virtual string GetPersonalTableKey() => $"{PersonalTablePrefix}{GetUniqueObjectKey()}";
        
        public abstract void OnStatChanged(string targetStat, float prev, float after);
        
    }
}
