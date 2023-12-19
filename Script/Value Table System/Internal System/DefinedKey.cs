using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GeneralGameDevKit.ValueTableSystem.Internal
{
    /// <summary>
    /// Key class for management. <br/>
    /// This class is used only for KeyTableAsset.
    /// </summary>
    [Serializable]
    public class DefinedKey
    {
        [SerializeField] private string keyPath;
        private string[] _splitPath;
    
        public string GetFullPathOfKey() => keyPath;
        public string[] GetSplitPath() => _splitPath;

        public DefinedKey(string path)
        {
            RebuildKeyPath(path);
        }
        
        public void RefreshParameterPath()
        {
            RebuildKeyPath(keyPath);
        }
        
        public void RebuildKeyPath(string path)
        {
            keyPath = path;
            _splitPath = keyPath.Split('/');
        }
        
        /// <summary>
        /// 0 : root
        /// </summary>
        /// <param name="depth"> depth of tree </param>
        /// <returns>Returns single key of depth.<br/>
        /// Returns empty if depth is out of range</returns>
        public string GetSingleKeyFromPath(int depth)
        {
            if(depth < 0 || depth >= _splitPath.Length) 
                return string.Empty;
        
            return _splitPath[depth];
        }

        public bool CheckIsContainPath(string path, int inDepth, int endDepth)
        {
            endDepth = Mathf.Clamp(endDepth, 0, _splitPath.Length);
            var cnt = endDepth - inDepth;
            if (cnt <= 0)
                return false;
            
            var targetPath = _splitPath.ToList().GetRange(inDepth, cnt);
            var splitPath = path.Split('/').ToList();

            if (splitPath.Count > targetPath.Count)
            {
                return false;
            }
            
            
            for (var i = 0; i < targetPath.Count && i < splitPath.Count; i++)
            {
                if (!targetPath[i].Equals(splitPath[i]))
                    return false;
            }
            
            return true;
        }
    }
}