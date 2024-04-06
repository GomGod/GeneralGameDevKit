using System.Collections.Generic;
using System.Linq;
using GeneralGameDevKit.Script.Editor;
using GeneralGameDevKit.KeyTableSystem.Internal;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class KeyTableEditor : EditorWindow
{
    [SerializeField] private StyleSheet mStyleSheet;
    [SerializeField] private VisualTreeAsset mVisualTreeAsset;

    private const string k_KeyTableField = "KeyTableAssetField";
    private static KeyTableAsset _assetOnOpen; 
    
    private ObjectField _keyTableField;
    private KeyTableStructureView _structureView;
    private KeyTableInspectorView _inspectorView;
    private KeyTableControlView _controlView;

    private KeyTableAsset _currentEditTarget;
    
    [MenuItem("GGDK/Key Table System/Table Editor")]
    public static void OpenWindow()
    {
        var wnd = GetWindow<KeyTableEditor>();
        wnd.titleContent = new GUIContent("KeyTableEditor");
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (Selection.activeObject is KeyTableAsset asset)
        {
            _assetOnOpen = asset;
            OpenWindow();
            return true;
        }

        return false;
    }
    
    public void CreateGUI()
    {
        var root = rootVisualElement;
        
        mVisualTreeAsset.CloneTree(root);
        root.styleSheets.Add(mStyleSheet);

        _structureView = root.Q<KeyTableStructureView>();
        _inspectorView = root.Q<KeyTableInspectorView>();
        _controlView = root.Q<KeyTableControlView>();
        _keyTableField = root.Q<ObjectField>(k_KeyTableField);
        
        _structureView.OnSelectedChange = OnTargetKeySelectionChanged;

        _controlView.OnButtonAddClicked = AddKey;
        _controlView.OnButtonEditClicked = EditKey;
        _controlView.OnButtonRemoveClicked = RemoveKey;

        _keyTableField.RegisterCallback<ChangeEvent<Object>>(OnEditTargetChanged);
        _inspectorView.ChangeCurrentTargetKeyEntity(null);
        
        OnEditTargetChanged(null);
        UpdateViewEnableState();
        
        if (!_assetOnOpen) return;
        
        SwitchEditTarget(_assetOnOpen);
        _assetOnOpen = null;
    }

    private void AddKey()
    {
        _currentEditTarget.AddNewKey(_inspectorView.GetPathToEdit());
        _structureView.PopulateView(_currentEditTarget);
    }

    private void EditKey()
    {
        var keyToEdit = _structureView.GetSelectedKeyEntity().FirstOrDefault();
        if (!keyToEdit)
            return;

        var deferredChanges = _currentEditTarget.EditKey(_inspectorView.GetPathToEdit(), keyToEdit.guid);
        if (deferredChanges != null)
        {
            foreach (var keyEntity in deferredChanges)
            {
                if (EditorUtility.DisplayDialog("Deferred Changes Detected"
                        , $"Key({keyEntity.guid})\n Path - {keyEntity.pathOfKey} \n Duplicate key found during edit. Overwrite?",
                        "Overwrite",
                        "Ignore"))
                {
                    _currentEditTarget.OverwriteKey(keyEntity);
                }
                else
                {
                    _currentEditTarget.ThrowKey(keyEntity);
                }
            }
        }

        _structureView.PopulateView(_currentEditTarget);
    }

    private void RemoveKey()
    {
        var targetKeyToRemove = _structureView.GetSelectedKeyEntity().FirstOrDefault();
        if (!targetKeyToRemove)
            return;

        _currentEditTarget.RemoveKey(targetKeyToRemove.guid);
        _structureView.PopulateView(_currentEditTarget);
    }

    private void OnTargetKeySelectionChanged(List<KeyEntity> obj)
    {
        var targetKey = obj?.FirstOrDefault();
        _inspectorView.ChangeCurrentTargetKeyEntity(targetKey);
        UpdateViewEnableState();
    }

    private void OnEditTargetChanged(ChangeEvent<Object> evt)
    {
        var targetTable = evt?.newValue as KeyTableAsset;
        _currentEditTarget = targetTable;
        if (_currentEditTarget)
            _structureView.PopulateView(_currentEditTarget);
        UpdateViewEnableState();
    }

    private void SwitchEditTarget(KeyTableAsset newTarget)
    {
        _keyTableField.value = newTarget;
    }

    private void UpdateViewEnableState()
    {
        var enableEditor = _currentEditTarget != null;

        _structureView.SetEnabled(enableEditor);
        _inspectorView.SetEnabled(enableEditor);
        _controlView.SetEnabled(enableEditor);

        if (!enableEditor)
        {
            _structureView.ClearTreeView();
        }

        if (!enableEditor)
        {
            _inspectorView.ChangeCurrentTargetKeyEntity(null);
        }

        if (!enableEditor)
        {
            _controlView.SetButtonEnable(false, false, false);
        }
        else
        {
            var editBtnEnabled = _structureView.GetSelectedKeyEntity().Any();

            _controlView.SetButtonEnable(editBtnEnabled, true, editBtnEnabled);
        }
    }
}
