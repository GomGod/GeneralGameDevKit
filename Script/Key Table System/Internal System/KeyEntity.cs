using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeneralGameDevKit.KeyTableSystem.Internal
{
    /// <summary>
    /// Entity of key. <br/>
    /// 
    /// </summary>
    public class KeyEntity : ScriptableObject
    {
        public int order;
        public string guid;
        public string pathOfKey;
        public string[] splitPath;
        public char currentSeparator;

        public void AllocateGuid()
        {
            guid = Guid.NewGuid().ToString();
        }

        public void UpdatePath(string newPath, char separator)
        {
            pathOfKey = newPath;
            currentSeparator = separator;
            splitPath = pathOfKey.Split(currentSeparator);
        }

        public string GetLeaf() => splitPath.Last();

        public bool IsSubPathOf(string path)
        {
            var splitPathForTest = path.Split(currentSeparator);

            if (splitPathForTest.Length > splitPath.Length)
                return false;

            for (var i = 0; i < splitPathForTest.Length && i < splitPath.Length; i++)
            {
                if (!splitPath[i].Equals(splitPathForTest[i]))
                    return false;
            }

            return true;
        }

#if UNITY_EDITOR
        public TreeViewItemData<KeyEntity> GetTreeViewItemData(KeyTableAsset sourceTable)
        {
            var childrenKeys = sourceTable.GetKeysSameDepth(splitPath.Length).Where(k => k.IsSubPathOf(pathOfKey)).ToList();
            var childrenTreeViewData = childrenKeys.Select(key => key.GetTreeViewItemData(sourceTable)).ToList();
            return new TreeViewItemData<KeyEntity>(order, this, childrenTreeViewData);
        }
#endif
    }
}
