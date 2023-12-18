using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GeneralGameDevKit.ValueTableSystem.Internal
{
    /// <summary>
    /// Table asset for store keys. <br/>
    /// This asset must be located at Resources/ValueTableAssets <br/>
    /// You can access keys with KeyString and KeyTable("name") Attribute. <br/>
    /// Look at the example for more details.
    /// </summary>
    [CreateAssetMenu(fileName = "KeyTableAsset", menuName = "General Game Dev Kit/Value Table System/Management/Key Table Asset")]
    public class KeyTableAsset : ScriptableObject
    {
        public static readonly string RootName = "Table Values"; 
        
        [SerializeField] private List<DefinedKey> keys;
        
        private ValueKeyNode _root;
        
        public IEnumerable<string> GetAllKeys() => keys.Select(p => p.GetFullPathOfKey());
        
        public ValueKeyNode BuildKeyTree()
        {
            SetEmptyRoot();

            foreach (var param in keys)
            {
                param.RefreshParameterPath();
                _root.AddNode(param);
            }

            return _root;
        }

        private void SetEmptyRoot()
        {
            _root = new ValueKeyNode
            {
                CurrentDepth = -1,
                KeyOfCurrentDepth = RootName
            };
        }
        public void AddKeyByPath(string path)
        {
            var newDefinedParam = new DefinedKey(path);
            keys.Add(newDefinedParam);
            if (_root == null)
            {
                SetEmptyRoot();
            }
            _root?.AddNode(newDefinedParam);
        }

        public void RemoveKeyByPath(string path)
        {
            var definedParamToRemove = new DefinedKey(path);
            var depthOfPath = path.Split('/').Length;
            if (_root.RemoveNode(definedParamToRemove))
            {
                keys.RemoveAll(p => p.CheckIsContainPath(path, 0, depthOfPath));
            }
        }

        public void ClearAllParams()
        {
            _root?.ClearNode();
            keys.Clear();
        }
    }

    public class ValueKeyNode
    {
        public int CurrentDepth;
        public string KeyOfCurrentDepth;
        public readonly List<ValueKeyNode> Siblings = new();
        
        private bool _isRemoveTarget;
        public void AddNode(DefinedKey keyToAdd)
        {
            var nextDepth = CurrentDepth + 1;
            var nextKey = keyToAdd.GetSingleKeyFromPath(nextDepth);
            if (nextKey.Equals(string.Empty))
                return;

            var index = Siblings.FindIndex(p => p.KeyOfCurrentDepth == nextKey);
            if (index < 0)
            {
                var newNode = new ValueKeyNode
                {
                    CurrentDepth = nextDepth,
                    KeyOfCurrentDepth = nextKey
                };
                Siblings.Add(newNode);
                newNode.AddNode(keyToAdd);
            }
            else
            {
                Siblings[index].AddNode(keyToAdd);
            }
        }

        public bool RemoveNode(DefinedKey keyToRemove)
        {
            var nextDepth = CurrentDepth + 1;
            var nextKey = keyToRemove.GetSingleKeyFromPath(nextDepth);
            if (nextKey == string.Empty)
            {
                _isRemoveTarget = KeyOfCurrentDepth.Equals(keyToRemove.GetSingleKeyFromPath(CurrentDepth));
                return _isRemoveTarget;
            }

            var index = Siblings.FindIndex(p => p.KeyOfCurrentDepth == nextKey);
            if (index < 0)
            {
                return false;
            }

            var result = Siblings[index].RemoveNode(keyToRemove);
            Siblings.RemoveAll(p => p._isRemoveTarget);
            return result;
        }

        public void ClearNode()
        {
            foreach (var node in Siblings)
            {
                node.ClearNode();
            }
            Siblings.Clear();
        }
    }
}