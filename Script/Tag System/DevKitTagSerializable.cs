using System;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace Developer.GeneralGameDevKit.TagSystem
{
    [Serializable]
    public class DevKitTagSerializable
    {
        [SerializeField, KeyTable("KeyTableAsset_Tags")]
        private string keyValue;
        public DevKitTag GetTagInstance() => DevKitTag.RequestTag(keyValue);
    }
}