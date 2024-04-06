using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeneralGameDevKit.KeyTableSystem.Internal
{
    /// <summary>
    /// Table asset for storing keys. <br/>
    /// You can access keys with KeyString and KeyTable("name") Attribute. <br/>
    /// Look at the example for more details.
    /// </summary>
    [CreateAssetMenu(fileName = "KeyTableAsset", menuName = "General Game Dev Kit/Value Table System/Management/Key Table Asset")]
    public class KeyTableAsset : ScriptableObject
    {
        [SerializeField, HideInInspector] public char separator = '/';
        [SerializeField, HideInInspector] public List<KeyEntity> keys;
        
        private readonly StringBuilder _sb = new();
        public IEnumerable<KeyEntity> GetAllKeys() => keys;
        private KeyEntity FindKeyEntity(string guid) => keys.FirstOrDefault(k => k.guid.Equals(guid));
        private KeyEntity FindKeyByPath(string path) => keys.FirstOrDefault(k => k.pathOfKey.Equals(path));
        private List<KeyEntity> FindSubKeys(string path) => keys.Where(k => k.IsSubPathOf(path)).ToList();
        
        /// <summary>
        /// Get all keys that is same depth.
        /// </summary>
        /// <param name="depth">depth to get</param>
        /// <returns>collection of found keys</returns>
        public List<KeyEntity> GetKeysSameDepth(int depth)
        {
            return keys?.Where(k => k.splitPath.Length-1 == depth).ToList();
        }
        
        /// <summary>
        /// Clear all keys.
        /// </summary>
        public void ClearAllKeys()
        {
            foreach (var key in keys)
            {
                AssetDatabase.RemoveObjectFromAsset(key);
            }
            keys.Clear();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Refresh key order.
        /// </summary>
        private void RefreshOrder()
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
        private enum ErrCode
        {
            Duplicate,
            InvalidPath
        }
        
        /// <summary>
        /// Report error msg.
        /// </summary>
        /// <param name="code">err code</param>
        /// <exception cref="ArgumentOutOfRangeException">invalid code</exception>
        private static void ReportError(ErrCode code)
        {
            var msg = code switch
            {
                ErrCode.Duplicate => "Paths cannot be duplicate.",
                ErrCode.InvalidPath => "Invalid Paths.",
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
            };

            Debug.LogError(msg);
        }

        /// <summary>
        /// Generate new key.
        /// </summary>
        /// <param name="path">path of key</param>
        /// <returns></returns>
        private KeyEntity GenerateNewKey(string path)
        {
            var newKey = CreateInstance<KeyEntity>();
            newKey.name = nameof(KeyEntity);
            newKey.AllocateGuid();
            newKey.UpdatePath(path, separator);
            return newKey;
        }

        /// <summary>
        /// Overwrite current key's guid
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
        
        /// <summary>
        /// Add if there were no route key.
        /// </summary>
        /// <param name="keyEntityToResolve">Key to resolve</param>
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
            if (string.IsNullOrEmpty(newPath))
            {
                ReportError(ErrCode.InvalidPath);
                return null;
            }
            
            if (FindKeyByPath(newPath))
            {
                ReportError(ErrCode.Duplicate);
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

        /// <summary>
        /// Add new key.
        /// </summary>
        /// <param name="path">path of key</param>
        /// <returns>add result</returns>
        public bool AddNewKey(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ReportError(ErrCode.InvalidPath);
                return false;
            }
            
            if (FindKeyByPath(path))
            {
                ReportError(ErrCode.Duplicate);
                return false;
            }

            var newKey = GenerateNewKey(path);
            keys.Add(newKey);
            AssetDatabase.AddObjectToAsset(newKey, this);
            ResolveRouteKeys(newKey);
            return true;
        }

        /// <summary>
        /// Remove target key
        /// </summary>
        /// <param name="guid">guid of target key</param>
        /// <returns>remove result</returns>
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