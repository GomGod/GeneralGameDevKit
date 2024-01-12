using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Developer.GeneralGameDevKit.TagSystem
{
    [Serializable]
    public class DevKitTagContainer
    {
        [SerializeField] private List<DevKitTag> tagCollection = new();

        public bool HasSuperTagAny(DevKitTag tag)
        {
            return tagCollection.Any(tag.IsSubTagOf);
        }

        public bool HasSuperTagAll(DevKitTag tag)
        {
            return tagCollection.All(tag.IsSubTagOf);
        }

        public bool HasExactTag(DevKitTag tag)
        {
            return tagCollection.Any(tag.IsEqual);
        }

        public bool IsSubContainerOf(DevKitTagContainer container)
        {
            return tagCollection.All(container.HasExactTag);
        }
    }
}