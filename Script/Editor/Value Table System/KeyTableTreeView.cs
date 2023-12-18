using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace GeneralGameDevKit.ValueTableSystem.Internal.Editor
{
    /// <summary>
    /// TreeView for KeyTable
    /// </summary>
    public class KeyTableTreeView : TreeView
    {
        private KeyTableAsset _container;
        private List<TreeViewItem> _allItems = new();
        
        public KeyTableTreeView(TreeViewState state) : base(state)
        {
            Reload();
        }

        public void SetContainer(KeyTableAsset cont)
        {
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

            var rootNode = _container.BuildKeyTree();
            _currentIdOrder = 1;
            _allItems.Clear();
            BuildTreeViewItems(rootNode);
            SetupParentsAndChildrenFromDepths(root, _allItems);

            return root;
        }

        private int _currentIdOrder;
        private void BuildTreeViewItems(ValueKeyNode node)
        {
            if (node.CurrentDepth >= 0)
            {
                _allItems.Add(new TreeViewItem
                {
                    id = _currentIdOrder++,
                    depth = node.CurrentDepth,
                    displayName = node.KeyOfCurrentDepth
                });
            }

            foreach (var nextNode in node.Siblings)
            {
                BuildTreeViewItems(nextNode);
            }
        }
    }
}