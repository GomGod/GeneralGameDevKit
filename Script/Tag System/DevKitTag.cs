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
        private string _fullPathOfTag;
        private string[] _tagStructure;
        
        public static void LoadTagCollection(IEnumerable<DevKitTag> tagCollection)
        {
            foreach (var tag in tagCollection)
            {
                LoadTag(tag);
            }
        }
        
        public static void LoadTag(DevKitTag tag)
        {
            tag.InitializeTag();
            _globalTagCollection.Add(tag._fullPathOfTag, tag);
        }

        internal DevKitTag(string path)
        {
            _fullPathOfTag = path;
            _tagStructure = _fullPathOfTag.Split('/');
        }
        
        private void InitializeTag()
        {
            _fullPathOfTag = tagKey;
            _tagStructure = _fullPathOfTag.Split('/');
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
            return _fullPathOfTag;
        }

        /// <summary>
        /// Returns string array that make up this tag.
        /// </summary>
        /// <returns>array of strings that make up this tag.</returns>
        public string[] GetTagStructure()
        {
            return _tagStructure;
        }


        /// <summary>
        /// Check this tag is sub tag of other tag.
        /// </summary>
        /// <param name="otherTag">Tag to compare</param>
        /// <returns>Result of sub tag test</returns>
        public bool IsSubTagOf(DevKitTag otherTag)
        {
            var otherTagStructure = otherTag.GetTagStructure();
            if (_tagStructure.Length > otherTagStructure.Length)
            {
                return false;
            }
                
            for (var i = 0; i < _tagStructure.Length && i < otherTagStructure.Length; i++)
            {
                if (!_tagStructure[i].Equals(otherTagStructure[i]))
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
            var otherTagStructure = otherTag.GetTagStructure();
            if (otherTagStructure.Length != _tagStructure.Length)
            {
                return false;
            }

            for (var i = 0; i < _tagStructure.Length && i < otherTagStructure.Length; i++)
            {
                if (!_tagStructure[i].Equals(otherTagStructure[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check this tag is sub tag of any member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsSubTagOfAnyMember(DevKitTagContainer container)
        {
            return container.HasSuperTagAny(this);
        }

        /// <summary>
        /// Check this tag is sub tag of all member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsSubTagOfAllMember(DevKitTagContainer container)
        {
            return container.HasSuperTagAll(this);
        }

        /// <summary>
        /// Check this tag is exact member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsExactMemberOf(DevKitTagContainer container)
        {
            return container.HasExactTag(this);
        }
    }
}