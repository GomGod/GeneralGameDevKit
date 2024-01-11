using System;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace Developer.GeneralGameDevKit.TagSystem
{
    [Serializable]
    public class DevKitTagSerializable
    {
        [SerializeField]
        private KeyString keyValue;
        public DevKitTag GetTagInstance() => DevKitTag.RequestTag(keyValue.GetKeyString());
    }
}