using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] public List<KeyEntity> keys;
        [SerializeField] public char separator = '/';
        
        public IEnumerable<KeyEntity> GetAllKeys() => keys;
        private KeyEntity FindKeyEntity(string guid) => keys.FirstOrDefault(k => k.guid.Equals(guid));
        private KeyEntity FindKeyByPath(string path) => keys.FirstOrDefault(k => k.pathOfKey.Equals(path));

        public void TestAdd()
        {
            AddNewKey("Hello/KeyTableSystem");
            AddNewKey("Hello/KeyTableSystem1");
            AddNewKey("Hello/KeyTableSystem2");
            AddNewKey("Hello/KeyTableSystem3");
            AddNewKey("Hello/KeyTableSystem/A");
            AddNewKey("Hello/KeyTableSystem/B");
            AddNewKey("Hello/KeyTableSystem/B/A");
            AddNewKey("Hello/KeyTableSystem/A/B");
            AddNewKey("Hello/KeyTableSystem/A/A/A");
            AddNewKey("Hello/KeyTableSystem/A/A/A");
            AddNewKey("Hello/KeyTableSystem/A/A/A");
        }
        
        public List<KeyEntity> GetKeysSameDepth(int depth)
        {
            return keys.Where(k => k.splitPath.Length-1 == depth).ToList();
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
        public bool AddNewKey(string path)
        {
            if (FindKeyByPath(path))
            {
                Debug.LogError("Paths cannot be duplicate");
                return false;
            }
            
            var newKey = CreateInstance<KeyEntity>();
            newKey.AllocateGuid();
            newKey.UpdatePath(path, separator);

            var superPath = string.Empty;
            foreach (var routePath in newKey.splitPath)
            {
                superPath = string.IsNullOrEmpty(superPath) ? routePath : string.Concat(superPath, separator.ToString(), routePath);

                if (FindKeyByPath(superPath))
                    continue;

                var superPathKey = CreateInstance<KeyEntity>();
                superPathKey.name = nameof(KeyEntity);
                superPathKey.AllocateGuid();
                superPathKey.UpdatePath(superPath, separator);

                keys.Add(superPathKey);
                AssetDatabase.AddObjectToAsset(superPathKey, this);
            }
            RefreshOrder();
            AssetDatabase.SaveAssets();
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

            AssetDatabase.SaveAssets();
            keys.RemoveAll(removeTargets.Contains);
            RefreshOrder();
            return true;
        }
#endif
    }
}