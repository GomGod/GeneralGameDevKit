using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace GeneralGameDevKit.ValueTableSystem.Internal.Editor
{
    /// <summary>
    /// TreeView for KeyTable
    /// </summary>
    public class KeyTableTreeView : TreeView
    {
        private KeyNode _rootNode;
        private KeyTableAsset _container;
        private List<TreeViewItem> _allItems = new();
        
        
        public KeyTableTreeView(TreeViewState state) : base(state)
        {
            Reload();
        }

        public void SetContainer(KeyTableAsset cont)
        {
            if (cont != null && _container != cont)
            {
                _rootNode = cont.BuildKeyTree();
            }

            _container = cont;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem
            {
                id =0,
                depth = -1,
                displayName = "root"
            };
            
            if (_container == null)
            {
                root.AddChild(new TreeViewItem(1, 0, "Empty"));
                return root;
            }
            
            _currentIdOrder = 1;
            _allItems.Clear();
            BuildTreeViewItems(_rootNode, 0);
            SetupParentsAndChildrenFromDepths(root, _allItems);

            return root;
        }

        private int _currentIdOrder;

        private void BuildTreeViewItems(KeyNode node, int depth)
        {
            _allItems.Add(new TreeViewItem
            {
                id = _currentIdOrder++,
                depth = depth,
                displayName = node.GetPathString()
            });

            foreach (var nextNode in node.GetSiblings())
            {
                BuildTreeViewItems(nextNode, depth + 1);
            }
        }
    }
}