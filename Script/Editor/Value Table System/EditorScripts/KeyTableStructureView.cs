using System.Collections.Generic;
using System.Linq;
using GeneralGameDevKit.ValueTableSystem.Internal;
using UnityEditor;
using UnityEngine.UIElements;

namespace GeneralGameDevKit.Script.Editor
{
    public class KeyTableStructureView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<KeyTableStructureView, UxmlTraits>
        {
        }

        private TreeView _treeView;
        private KeyTableAsset _targetTable;

        public KeyTableStructureView()
        {
            _treeView = new TreeView();
            Insert(0, _treeView);
            _treeView.selectedIndicesChanged += Highligh;
            _treeView.makeItem = () => new Label();
            _treeView.bindItem = (elem, idx) =>
            {
                ((Label) elem).text = _treeView.GetItemDataForIndex<KeyEntity>(idx).GetLeaf();
            };
        }

        private void Highligh(IEnumerable<int> obj)
        {
            foreach (var idx in obj)
            {
                EditorGUIUtility.PingObject(_treeView.GetItemDataForIndex<KeyEntity>(idx));
            }
        }

        public void PopulateView(KeyTableAsset assetToEdit)
        {
            _targetTable = assetToEdit;
            if (!_targetTable)
                return;

            var rootViewData = _targetTable.GetKeysSameDepth(0).Select(k => k.GetTreeViewItemData(_targetTable)).ToList();
            _treeView.SetRootItems(rootViewData);
            _treeView.RefreshItems();
        }

        public List<KeyEntity> GetSelectedKeyEntity()
        {
            return _treeView.GetSelectedItems<KeyEntity>().Select(vd => vd.data).ToList();
        }
    }
}