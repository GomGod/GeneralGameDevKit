using System.Linq;
using GeneralGameDevKit.Script.Editor;
using GeneralGameDevKit.ValueTableSystem.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class KeyTableEditor : EditorWindow
{
    [SerializeField] private StyleSheet mStyleSheet;
    [SerializeField] private VisualTreeAsset mVisualTreeAsset;

    private KeyTableStructureView _structureView;
    private KeyTableInspectorView _inspectorView;
    private KeyTableControlView _controlView;
    
    [MenuItem("GGDK/Key Table System/Table Editor")]
    public static void ShowExample()
    {
        KeyTableEditor wnd = GetWindow<KeyTableEditor>();
        wnd.titleContent = new GUIContent("KeyTableEditor");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        
        mVisualTreeAsset.CloneTree(root);
        root.styleSheets.Add(mStyleSheet);

        _structureView = root.Q<KeyTableStructureView>();
        _inspectorView = root.Q<KeyTableInspectorView>();
        _controlView = root.Q<KeyTableControlView>();
        
        OnSelectionChange();
    }
    
    private void OnSelectionChange()
    {
        if (!Selection.activeObject)
        {
            _structureView.Clear();
            return;
        }

        var selectedAsset = Selection.activeObject as KeyTableAsset;
        if (!selectedAsset)
            return;

        if (!selectedAsset.keys.Any())
        {
            selectedAsset.TestAdd();
        }
        
        _structureView.PopulateView(selectedAsset);
    }
}
