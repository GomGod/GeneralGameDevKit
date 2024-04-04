using System;
using System.Collections.Generic;
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

    private KeyTableAsset _currentEditTarget;
    
    [MenuItem("GGDK/Key Table System/Table Editor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<KeyTableEditor>();
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
        
        _structureView.OnSelectedChange = OnTargetKeySelectionChanged;

        _controlView.OnButtonAddClicked = AddKey;
        _controlView.OnButtonEditClicked = EditKey;
        _controlView.OnButtonRemoveClicked = RemoveKey;
        
        _inspectorView.ChangeCurrentTargetKeyEntity(null);
        OnSelectionChange();
        UpdateViewEnableState();
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

    //Unity Event
    private void OnSelectionChange()
    {
        if (!Selection.activeObject)
        {
            _currentEditTarget = null;
            UpdateViewEnableState();
            return;
        }

        var selectedAsset = Selection.activeObject as KeyTableAsset;
        if (!selectedAsset)
        {
            _currentEditTarget = null;
            UpdateViewEnableState();
            return;
        }

        _currentEditTarget = selectedAsset;
        _structureView.PopulateView(selectedAsset);
        UpdateViewEnableState();
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
