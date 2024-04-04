using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeneralGameDevKit.ValueTableSystem.Internal
{
    /// <summary>
    /// Table asset for storing keys. <br/>
    /// This asset must be located at Resources/ValueTableAssets <br/>
    /// You can access keys with KeyString and KeyTable("name") Attribute. <br/>
    /// Look at the example for more details.
    /// </summary>
    [CreateAssetMenu(fileName = "KeyTableAsset", menuName = "General Game Dev Kit/Value Table System/Management/Key Table Asset")]
    public class KeyTableAsset : ScriptableObject
    {
        [SerializeField] public char separator = '/';
        
        [SerializeField, HideInInspector] public List<KeyEntity> keys;
        
        private readonly StringBuilder _sb = new();
        public IEnumerable<KeyEntity> GetAllKeys() => keys;
        private KeyEntity FindKeyEntity(string guid) => keys.FirstOrDefault(k => k.guid.Equals(guid));
        private KeyEntity FindKeyByPath(string path) => keys.FirstOrDefault(k => k.pathOfKey.Equals(path));
        private List<KeyEntity> FindSubKeys(string path) => keys.Where(k => k.IsSubPathOf(path)).ToList();
        
        public List<KeyEntity> GetKeysSameDepth(int depth)
        {
            return keys?.Where(k => k.splitPath.Length-1 == depth).ToList();
        }
        
        public void ClearAllKeys()
        {
            keys.Clear();
        }

        public void RefreshOrder()
        {
            keys.Sort((ak, bk) =>
            {
                var depthCompare = ak.splitPath.Length.CompareTo(bk.splitPath.Length);
                return depthCompare != 0
                    ? depthCompare
                    : string.Compare(ak.pathOfKey, bk.pathOfKey, StringComparison.Ordinal);
            });

            var newOrder = 0;
            keys.ForEach(k => k.order = newOrder++);
        }

#if UNITY_EDITOR
        private KeyEntity GenerateNewKey(string path)
        {
            var newKey = CreateInstance<KeyEntity>();
            newKey.name = nameof(KeyEntity);
            newKey.AllocateGuid();
            newKey.UpdatePath(path, separator);
            return newKey;
        }

        /// <summary>
        /// overwrite current key's guid
        /// </summary>
        /// <param name="keyEntityToOverwrite">key to overwrite</param>
        public void OverwriteKey(KeyEntity keyEntityToOverwrite)
        {
            var currentKey = FindKeyByPath(keyEntityToOverwrite.pathOfKey);
            if (currentKey)
            {
                currentKey.guid = keyEntityToOverwrite.guid;
            }
            
            RefreshOrder();
            AssetDatabase.RemoveObjectFromAsset(keyEntityToOverwrite);
            AssetDatabase.SaveAssets();
        }

        public void ThrowKey(KeyEntity keyEntityToThrow)
        {
            AssetDatabase.RemoveObjectFromAsset(keyEntityToThrow);
            AssetDatabase.SaveAssets();
        }
        
        private void ResolveRouteKeys(KeyEntity keyEntityToResolve)
        {
            var splitPath = keyEntityToResolve.splitPath;
            
            _sb.Clear();
            _sb.Append(splitPath[0]);
            var rootPath = _sb.ToString();
            if (!FindKeyByPath(rootPath))
            {
                var rootKey = GenerateNewKey(rootPath);
                keys.Add(rootKey);
                AssetDatabase.AddObjectToAsset(rootKey, this);
            }
            
            for (var i = 1; i < splitPath.Length - 1; i++)
            {
                _sb.Append(separator);
                _sb.Append(splitPath[i]);
                
                var routePath = _sb.ToString();
                if(FindKeyByPath(routePath))
                    continue;

                var middleRouteKey = GenerateNewKey(routePath);
                keys.Add(middleRouteKey);
                AssetDatabase.AddObjectToAsset(middleRouteKey, this);
            }
            
            RefreshOrder();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Edit path of key.
        /// </summary>
        /// <param name="newPath">path to edit</param>
        /// <param name="targetKeyGuid">target key entity's guid</param>
        /// <returns>deferred changes</returns>
        public List<KeyEntity> EditKey(string newPath, string targetKeyGuid)
        {
            if (FindKeyByPath(newPath))
            {
                Debug.LogError("Paths cannot be duplicate");
                return null;
            }

            var deferredChanges = new List<KeyEntity>();

            var keyToEdit = FindKeyEntity(targetKeyGuid);
            var currentSubKeys = FindSubKeys(keyToEdit.pathOfKey);
            var basePathLen = keyToEdit.splitPath.Length;
            
            currentSubKeys.Remove(keyToEdit);
            keyToEdit.UpdatePath(newPath, separator);

            foreach (var subKey in currentSubKeys)
            {
                _sb.Clear();
                _sb.Append(keyToEdit.pathOfKey);
                
                var subKeyPathLen = subKey.splitPath.Length;
                for (var i = basePathLen; i < subKeyPathLen; i++)
                {
                    _sb.Append(separator);
                    _sb.Append(subKey.splitPath[i]);
                }

                var newSubKeyPath = _sb.ToString();
                var isDuplicate = FindKeyByPath(newSubKeyPath) != null; 
                subKey.UpdatePath(newSubKeyPath, separator);
                
                if (!isDuplicate)
                    continue;
                
                keys.Remove(subKey);
                deferredChanges.Add(subKey);
            }

            ResolveRouteKeys(keyToEdit);
            return deferredChanges;
        }

        public bool AddNewKey(string path)
        {
            if (FindKeyByPath(path))
            {
                Debug.LogError("Paths cannot be duplicate");
                return false;
            }

            var newKey = GenerateNewKey(path);
            keys.Add(newKey);
            AssetDatabase.AddObjectToAsset(newKey, this);
            ResolveRouteKeys(newKey);
            return true;
        }

        public bool RemoveKey(string guid)
        {
            var targetKey = FindKeyEntity(guid);
            if (!targetKey)
                return false;

            var removeTargets = keys.Where(k => k.IsSubPathOf(targetKey.pathOfKey)).ToList();
            foreach (var key in removeTargets)
            {
                AssetDatabase.RemoveObjectFromAsset(key);
            }

            keys.RemoveAll(removeTargets.Contains);
            RefreshOrder();
            AssetDatabase.SaveAssets();
            return true;
        }
#endif
    }
}