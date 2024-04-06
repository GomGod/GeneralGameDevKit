using System;
using System.Collections.Generic;
using GeneralGameDevKit.Utils;
using UnityEditor;
using UnityEngine;

namespace GeneralGameDevKit.KeyTableSystem.Internal
{
    /// <summary>
    /// Manager class that manage KeyTableAssets.<br/>
    /// This class is designed for attributes, so don't use this class in your code 
    /// </summary>
    public class KeyTableAssetManager : NonMonoSingleton<KeyTableAssetManager>
    {
        private readonly Dictionary<string, KeyTableAsset> _loadedTableAssets = new();
        
        public KeyTableAssetManager()
        {
            var tableAssetGuids = AssetDatabase.FindAssets($"t:{nameof(KeyTableAsset)}");
            if (tableAssetGuids.Length <= 0) return;
            
            _loadedTableAssets.Clear();
            foreach (var guid in tableAssetGuids)
            {
                var loadedTableAsset = AssetDatabase.LoadAssetAtPath<KeyTableAsset>(AssetDatabase.GUIDToAssetPath(guid));
                _loadedTableAssets.Add(loadedTableAsset.name, loadedTableAsset);
            }
        }

        /// <summary>
        /// Obtain all keys in container.
        /// Returns null if there is no table.
        /// </summary>
        /// <param name="containerName">target container's name</param>
        /// <returns>Collections of key path strings.</returns>
        public IEnumerable<KeyEntity> GetAllKeys(string containerName)
        {
            return _loadedTableAssets.TryGetValue(containerName, out var asset) ? asset.GetAllKeys() : new List<KeyEntity>();
        }
    }
}

namespace GeneralGameDevKit.KeyTableSystem
{
    /// <summary>
    /// Displays the values in the table with the same name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class KeyTableAttribute : PropertyAttribute
    {
        public readonly string AssetName;

        public KeyTableAttribute(string assetName)
        {
            AssetName = assetName;
        }
    }
}