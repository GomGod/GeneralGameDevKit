using System.Collections.Generic;
using Developer.GeneralGameDevKit.TagSystem;
using UnityEngine;

namespace GeneralGameDevKit.StatSystem
{
    //todo
    //permanent tags
    //current applying stat effects
    //modify context processing
    //modifier processing (buff/debuff)
    public class BaseStatObject : MonoBehaviour
    {
        private DevKitTagContainer _permanentTags;
        private StatSystemCore _statSystemCore;

        public float GetStatBaseValue(string identifier) => _statSystemCore.GetBaseValue(identifier);
        public float GetStatApplyValue(string identifier) => _statSystemCore.GetStatApplyValue(identifier);

        public void InitializeStats(Dictionary<string, float> statCollections)
        {
            foreach (var (id, val) in statCollections)
            {
                _statSystemCore.ModifyStatBaseValue(id, val);
            }
        }
        
        
    }
}
