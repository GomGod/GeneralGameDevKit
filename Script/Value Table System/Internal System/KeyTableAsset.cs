using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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
        public static readonly string RootName = "Keys";

        [Header("You can edit with this field, but it's not recommended."), SerializeField]
        private List<DefinedKey> keys;

        private KeyNode _root;
        private readonly StringBuilder _sb = new();

        public IEnumerable<string> GetAllKeys() => keys.Select(p => p.GetFullPathOfKey());

        public KeyNode BuildKeyTree()
        {
            SetEmptyRoot();

            var keysToAdd = new List<DefinedKey>(keys);
            foreach (var param in keysToAdd)
            {
                param.RefreshParameterPath();
                AddKeyByPath(param.GetFullPathOfKey());
            }

            return _root;
        }

        private void SetEmptyRoot()
        {
            _root = new KeyNode(RootName);
        }

        public void AddKeyByPath(string path)
        {
            var newDefinedParam = new DefinedKey(path);
            _sb.Clear();
            foreach (var pathStr in newDefinedParam.GetSplitPath())
            {
                _sb.Append(pathStr);
                var pathCurrent = _sb.ToString();
                if (!keys.Any(k => k.GetFullPathOfKey().Equals(pathCurrent)))
                {
                    var newKey = new DefinedKey(pathCurrent);
                    keys.Add(newKey);
                }

                _sb.Append("/");
            }

            if (_root == null)
            {
                SetEmptyRoot();
            }

            _root?.BeginAddNode(newDefinedParam);
            keys.Sort((ka, kb) => string.Compare(ka.GetFullPathOfKey(), kb.GetFullPathOfKey(), StringComparison.Ordinal));
        }

        public void RemoveKeyByPath(string path)
        {
            var definedParamToRemove = new DefinedKey(path);
            var removeResult = _root.BeginRemoveNode(definedParamToRemove);
            if (removeResult)
            {
                var depthOfPath = path.Split('/').Length;
                keys.RemoveAll(p => p.CheckIsContainPath(path, 0, depthOfPath));
            }

            Debug.Log($"Target Key - {path}, Remove Result - {removeResult.ToString()}");
        }
    }

    public class KeyNode
    {
        private static Queue<string> _queueForTreeProcessing = new();

        private readonly string _pathString;
        private readonly List<KeyNode> _siblings = new();

        public KeyNode(string pathString)
        {
            _pathString = pathString;
        }

        public string GetPathString() => _pathString;

        public List<KeyNode> GetSiblings() => new(_siblings);

        public void BeginAddNode(DefinedKey defKey)
        {
            BuildQueueForTreeProcessing(defKey, ref _queueForTreeProcessing);
            AddNode(ref _queueForTreeProcessing);
        }

        public bool BeginRemoveNode(DefinedKey defKey)
        {
            BuildQueueForTreeProcessing(defKey, ref _queueForTreeProcessing);
            return RemoveNode(ref _queueForTreeProcessing);
        }

        private static void BuildQueueForTreeProcessing(DefinedKey defKey, ref Queue<string> stringPathQueue)
        {
            var fullSplitPath = defKey.GetSplitPath();
            stringPathQueue.Clear();
            foreach (var path in fullSplitPath)
            {
                stringPathQueue.Enqueue(path);
            }
        }

        private void AddNode(ref Queue<string> splitPathQueue)
        {
            if (splitPathQueue == null)
            {
                throw new NullReferenceException($"Null stack detected during {MethodBase.GetCurrentMethod()?.ReflectedType?.FullName ?? "AddNode()"} in KeyNode."); //todo : err handling
            }

            if (splitPathQueue.Count == 0)
            {
                return;
            }

            var nextPath = splitPathQueue.Dequeue();
            var siblingIdx = _siblings.FindIndex(n => n._pathString.Equals(nextPath));
            if (siblingIdx >= 0)
            {
                _siblings[siblingIdx].AddNode(ref splitPathQueue);
            }
            else
            {
                var newNode = new KeyNode(nextPath);
                _siblings.Add(newNode);
                newNode.AddNode(ref splitPathQueue);
            }
        }

        private bool RemoveNode(ref Queue<string> splitPathQueue)
        {
            if (splitPathQueue == null)
            {
                throw new NullReferenceException($"Null stack detected during {MethodBase.GetCurrentMethod()?.ReflectedType?.FullName ?? "AddNode()"} in KeyNode."); //todo : err handling
            }

            var nextPath = splitPathQueue.Dequeue();
            var siblingIdx = _siblings.FindIndex(n => n._pathString.Equals(nextPath));

            if (siblingIdx < 0)
            {
                return false;
            }

            if (splitPathQueue.Count == 0)
            {
                _siblings.RemoveAt(siblingIdx);
                return true;
            }

            return _siblings[siblingIdx].RemoveNode(ref splitPathQueue);
        }
    }
}