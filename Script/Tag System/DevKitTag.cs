using System;
using System.Collections.Generic;
using GeneralGameDevKit.ValueTableSystem;
using UnityEngine;

namespace Developer.GeneralGameDevKit.TagSystem
{
    [Serializable]
    public class DevKitTag
    {
        public static string TagKeyFieldName => nameof(tagKey);
        
        [SerializeField, KeyTable("KeyTableAsset_Tags")] private string tagKey;
        private static Dictionary<string, DevKitTag> _globalTagCollection = new();
        private static Dictionary<string, string[]> _cachedPathStructure = new();
        private string _fullPathOfTag;
        private bool _initialized;

        public string TagKey => tagKey;
        
        public static void LoadTagCollection(IEnumerable<DevKitTag> tagCollection)
        {
            foreach (var tag in tagCollection)
            {
                LoadTag(tag);
            }
        }

        public static string[] GetCachedStructure(string tagKey)
        {
            if (_cachedPathStructure.TryGetValue(tagKey, out var ret))
            {
                ret = tagKey.Split('/');
                _cachedPathStructure.Add(tagKey, ret);
            }

            return ret;
        }
        
        public static void LoadTag(DevKitTag tag)
        {
            if (!_globalTagCollection.TryAdd(tag._fullPathOfTag, tag))
            {
                Debug.LogWarning("Duplicated tag loading");
            }
            else
            {
                Debug.Log($"Tag : {tag.tagKey} loaded.");
            }
        }

        internal DevKitTag(string path)
        {
            tagKey = path;
            _fullPathOfTag = path;
            _initialized = true;
        }

        private void CheckInitialization()
        {
            if (_initialized)
                return;
            if (!_globalTagCollection.ContainsKey(tagKey))
            {
                throw new Exception($"There is no tag({tagKey}). Load tag first.");
            }
            
            _fullPathOfTag = tagKey;
            _initialized = true;
        }

        public static DevKitTag RequestTag(string fullPathOfTag)
        {
            if (_globalTagCollection.TryGetValue(fullPathOfTag, out var ret))
            {
                return ret;
            }

            throw new Exception($"There is no tag({fullPathOfTag}). Load tag first.");
        }

        /// <summary>
        /// Returns single string corresponding to the entire path. 
        /// </summary>
        /// <returns></returns>
        public string GetFullPathOfTag()
        {
            CheckInitialization();
            return _fullPathOfTag;
        }

        /// <summary>
        /// Returns string array that make up this tag.
        /// </summary>
        /// <returns>array of strings that make up this tag.</returns>
        public string[] GetTagStructure()
        {
            CheckInitialization();
            return GetCachedStructure(tagKey);
        }


        /// <summary>
        /// Check this tag is sub tag of other tag.
        /// </summary>
        /// <param name="otherTag">Tag to compare</param>
        /// <returns>Result of sub tag test</returns>
        public bool IsSubTagOf(DevKitTag otherTag)
        {
            CheckInitialization();
            var otherTagPath = otherTag.GetFullPathOfTag();
            return IsSubTagOfPath(otherTagPath);
        }

        public bool IsSubTagOfPath(string path)
        {
            CheckInitialization();
            var otherPathStructure = GetCachedStructure(path);
            var tagStructure = GetCachedStructure(tagKey);
            if (tagStructure.Length > otherPathStructure.Length)
            {
                return false;
            }

            for (var i = 0; i < tagStructure.Length && i < otherPathStructure.Length; i++)
            {
                if (!tagStructure[i].Equals(otherPathStructure[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check this tag is equal with other tag.
        /// </summary>
        /// <param name="otherTag">Tag to compare</param>
        /// <returns>Result of equal tag test</returns>
        public bool IsEqual(DevKitTag otherTag)
        {
            CheckInitialization();
            return otherTag.TagKey.Equals(tagKey);
        }

        /// <summary>
        /// Check this tag is sub tag of any member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsSubTagOfAnyMember(DevKitTagContainer container)
        {
            CheckInitialization();
            return container.HasSuperTagAny(this);
        }

        /// <summary>
        /// Check this tag is sub tag of all member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsSubTagOfAllMember(DevKitTagContainer container)
        {
            CheckInitialization();
            return container.HasSuperTagAll(this);
        }

        /// <summary>
        /// Check this tag is exact member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsExactMemberOf(DevKitTagContainer container)
        {
            CheckInitialization();
            return container.HasExactTag(this);
        }
    }
}