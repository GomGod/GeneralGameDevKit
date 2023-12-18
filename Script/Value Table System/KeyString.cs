using System;
using UnityEngine;

namespace GeneralGameDevKit.ValueTableSystem
{
    [Serializable]
    public class KeyString
    {
        public static string GetKeyStringFieldName() => nameof(keyString);
        
        public static string NoneValue = "None";
        
        [SerializeField] private string keyString;
        public string GetKeyString() => keyString;
    }
}