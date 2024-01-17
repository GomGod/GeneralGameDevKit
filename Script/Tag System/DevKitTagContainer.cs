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

        public List<DevKitTag> GetAllTags() => new(tagCollection);

        public void MergeContainer(DevKitTagContainer other)
        {
            AddTags(other.GetAllTags());
        }

        public void AddTag(DevKitTag tag)
        {
            tagCollection.Add(tag);
        }
        public void AddTags(IEnumerable<DevKitTag> tags)
        {
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public void RemoveTag(DevKitTag tag)
        {
            var idx = tagCollection.FindIndex(tag.IsEqual);
            if (idx >= 0)
            {
                tagCollection.RemoveAt(idx);
            }
        }

        public void RemoveTags(IEnumerable<DevKitTag> tags)
        {
            foreach (var tag in tags)
            {
                RemoveTag(tag);
            }
        }

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