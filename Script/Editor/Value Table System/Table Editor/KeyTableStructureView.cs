using System;
using System.Collections.Generic;
using System.Linq;
using GeneralGameDevKit.ValueTableSystem.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeneralGameDevKit.Script.Editor
{
    public class KeyTableStructureView : VisualElement
    {
        public Action<List<KeyEntity>> OnSelectedChange;

        public new class UxmlFactory : UxmlFactory<KeyTableStructureView, UxmlTraits>
        {
        }

        private TreeView _treeView;

        public KeyTableStructureView()
        {
            _treeView = new TreeView
            {
                makeItem = () => new Label
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.LowerLeft
                    }
                }
            };

            _treeView.bindItem = (elem, idx) => { ((Label) elem).text = _treeView.GetItemDataForIndex<KeyEntity>(idx).GetLeaf(); };
            _treeView.selectedIndicesChanged += OnSelectedItem;
            Insert(0, _treeView);
        }

        private void OnSelectedItem(IEnumerable<int> obj)
        {
            var selectedItems = obj.Select(idx => _treeView.GetItemDataForIndex<KeyEntity>(idx)).ToList();
            OnSelectedChange?.Invoke(selectedItems);
        }

        public void PopulateView(KeyTableAsset assetToEdit)
        {
            if (!assetToEdit)
                return;

            var rootViewData = assetToEdit.GetKeysSameDepth(0)?.Select(k => k.GetTreeViewItemData(assetToEdit)).ToList();
            if (rootViewData == null)
                return;
            
            _treeView.SetRootItems(rootViewData);
            _treeView.RefreshItems();
        }

        public void ClearTreeView()
        {
            _treeView.SetRootItems(new List<TreeViewItemData<KeyEntity>>());
            _treeView.RefreshItems();
        }

        public List<KeyEntity> GetSelectedKeyEntity()
        {
            return _treeView.GetSelectedItems<KeyEntity>().Select(vd => vd.data).ToList();
        }
    }
}