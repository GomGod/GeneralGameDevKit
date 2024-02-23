using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Developer.GeneralGameDevKit.TagSystem
{
    [Serializable]
    public class DevKitTagContainer
    {
        /// <summary>
        /// p1 : tag, p2 : prev, p3 : after
        /// </summary>
        public Action<DevKitTag, int, int> OnTagUpdate;
        
        [SerializeField] private List<DevKitTag> tagCollection = new();

        private Dictionary<DevKitTag, int> _tempCountDictionary =new();
        
        public List<DevKitTag> GetAllTags() => new(tagCollection);

        public void MergeContainer(DevKitTagContainer other)
        {
            AddTags(other.GetAllTags());
        }

        public int GetTagCount(DevKitTag tag)
        {
            return tagCollection.Count(tag.IsEqual);
        }

        public void ClearContainer()
        {
            _tempCountDictionary.Clear();
            foreach (var tag in tagCollection.ToHashSet())
            {
                _tempCountDictionary.Add(tag, GetTagCount(tag));
            }
            
            tagCollection.Clear();
            foreach (var (tag, prevCnt) in _tempCountDictionary)
            {
                OnTagUpdate?.Invoke(tag, prevCnt, 0);
            }
        }
        
        public void AddTag(DevKitTag tag)
        {
            var prevCnt = GetTagCount(tag);
            tagCollection.Add(tag);
            OnTagUpdate?.Invoke(tag, prevCnt, prevCnt+1);
        }
        public void AddTags(IEnumerable<DevKitTag> tags)
        {
            _tempCountDictionary.Clear();
            var tagsToAdd = tags.ToList();
            foreach (var tag in tagsToAdd.ToHashSet())
            {
                _tempCountDictionary.Add(tag, GetTagCount(tag));
            }
            
            foreach (var tag in tagsToAdd)
            {
                tagCollection.Add(tag);
            }

            foreach (var (tag, prevCount) in _tempCountDictionary)
            {
                OnTagUpdate?.Invoke(tag, prevCount, GetTagCount(tag));
            }
        }

        public void RemoveTag(DevKitTag tag)
        {
            var idx = tagCollection.FindIndex(tag.IsEqual);
            if (idx < 0) return;
            
            var prevCnt = GetTagCount(tag);
            tagCollection.RemoveAt(idx);
            OnTagUpdate?.Invoke(tag, prevCnt, prevCnt - 1);
        }

        public void RemoveTags(IEnumerable<DevKitTag> tags)
        {
            _tempCountDictionary.Clear();
            var tagsToRemove = tags.ToList();
            foreach (var tag in tagsToRemove.ToHashSet().Where(tag => tagCollection.FindIndex(tag.IsEqual) >= 0))
            {
                _tempCountDictionary.Add(tag, GetTagCount(tag));
            }

            foreach (var tag in tagsToRemove)
            {
                var idx = tagCollection.FindIndex(tag.IsEqual);
                if (idx >= 0)
                {
                    tagCollection.RemoveAt(idx);
                }
            }

            foreach (var (tag, prev) in _tempCountDictionary.ToList())
            {
                OnTagUpdate?.Invoke(tag, prev, GetTagCount(tag));
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

        public bool IsIntersectionAny(DevKitTagContainer container)
        {
            return tagCollection.Any(container.HasExactTag);
        }
    }
}