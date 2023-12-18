using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GeneralGameDevKit.ValueTableSystem.Internal.Editor
{
    /// <summary>
    /// Editor Window script for KeyTableManagementWindow
    /// </summary>
    public class KeyTableManagementWindow : EditorWindow
    {
        [SerializeField] private TreeViewState treeViewState;
        
        private KeyTableAsset _keyTableAsset;
        private KeyTableTreeView _pTreeView;

        private static readonly Vector2 WindowSize = new(945, 525);

        private string _selectedPath;
        private string _parameterPathInput;

        private void OnEnable()
        {
            treeViewState ??= new TreeViewState();
            _pTreeView = new KeyTableTreeView(treeViewState);
        }

        private void OnGUI()
        {
            #region GUI Layout Variables
            var windowWidth = position.width;
            var windowHeight = position.height;
            var defaultPadding = 5.0f;
            var containerFieldHeight = 25.0f;
            var editPanelPropertyHeight = 25.0f;
            
            var treeViewSideWidth = windowWidth / 2.615f - defaultPadding * 2;
            var treeViewSideInitXPosition = defaultPadding;
            var editViewSideWidth = treeViewSideWidth * 1.615f - defaultPadding * 2;
            var editViewSideInitXPosition = treeViewSideWidth + defaultPadding * 2;
            
            var controlPanelSideInitYPosition = containerFieldHeight + defaultPadding;
            var controlPanelHeight = windowHeight - containerFieldHeight - defaultPadding;

            var containerFieldRect = new Rect(treeViewSideInitXPosition, 0, treeViewSideWidth, containerFieldHeight);
            var treeViewRect = new Rect(treeViewSideInitXPosition, controlPanelSideInitYPosition, treeViewSideWidth, controlPanelHeight);
            var editPanelRect = new Rect(editViewSideInitXPosition, controlPanelSideInitYPosition, editViewSideWidth, controlPanelHeight);
            
            var editPanelPropertyWidth = editPanelRect.width;
            #endregion

            _keyTableAsset = EditorGUI.ObjectField(containerFieldRect, _keyTableAsset, typeof(KeyTableAsset), false) as KeyTableAsset;
            _pTreeView.SetContainer(_keyTableAsset);
            if (_keyTableAsset == null)
            {
                var labelRect = containerFieldRect;
                labelRect.y += 30;
                EditorGUI.LabelField(labelRect, "Set container asset.");
                return;
            }
            
            EditorGUIUtility.DrawColorSwatch(treeViewRect,Color.white);
            _pTreeView.OnGUI(treeViewRect);
            
            #region Edition Panels
            var propertyLabelRect = new Rect(editViewSideInitXPosition + defaultPadding, controlPanelSideInitYPosition + defaultPadding, editPanelPropertyWidth * 0.3f, editPanelPropertyHeight);
            var propertyFieldRect = new Rect(editViewSideInitXPosition + propertyLabelRect.width + defaultPadding, controlPanelSideInitYPosition + defaultPadding, editPanelPropertyWidth * 0.7f, editPanelPropertyHeight);
            
            var selected = _pTreeView.GetSelection();
            if (selected.Count > 0)
            {
                var rows = _pTreeView.GetRows();
                var selectedIndex = selected.First();
                if (selectedIndex != 0)
                {
                    var targetItem = rows.ToList().FindIndex(i => i.id == selectedIndex);
                    if (targetItem >= 0)
                    {
                        var currentItem = rows[targetItem];
                        var builtPath = currentItem.displayName;
                        while (!currentItem.parent.displayName.Equals(KeyTableAsset.RootName)
                               && !currentItem.parent.displayName.Equals("root"))
                        {
                            currentItem = currentItem.parent;
                            builtPath = $"{currentItem.displayName}/{builtPath}";
                        }

                        _selectedPath = builtPath;
                    }
                }
                else
                {
                    _selectedPath = string.Empty;
                }
            }
            EditorGUI.LabelField(propertyLabelRect, "Selected");
            EditorGUI.LabelField(propertyFieldRect, _selectedPath);
            propertyLabelRect.y += editPanelPropertyHeight + defaultPadding;
            propertyFieldRect.y += editPanelPropertyHeight+ defaultPadding;
            EditorGUI.LabelField(propertyLabelRect, "Key Name");
            _parameterPathInput = EditorGUI.TextField(propertyFieldRect, _parameterPathInput);
            
            var buttonRect = propertyFieldRect;
            buttonRect.y += editPanelPropertyHeight + defaultPadding;
            var getSelectedPathToInputAction = GUI.Button(buttonRect, "Copy&paste selected path to input field");
            buttonRect.y = windowHeight - (editPanelPropertyHeight*3 + defaultPadding*3);
            var addButtonAction = GUI.Button(buttonRect, "Add");
            buttonRect.y += editPanelPropertyHeight + defaultPadding;
            var modifyButtonAction = GUI.Button(buttonRect, "Edit");
            buttonRect.y += editPanelPropertyHeight + defaultPadding;
            var removeButtonAction = GUI.Button(buttonRect, "Remove");
            buttonRect.y += editPanelPropertyHeight + defaultPadding;
            #endregion

            #region button actions
            if (addButtonAction)
            {
                if (_parameterPathInput == string.Empty)
                {
                    return;
                }
                
                _keyTableAsset.AddKeyByPath(_parameterPathInput);
                EditorUtility.SetDirty(_keyTableAsset);
            }

            if (modifyButtonAction)
            {
                if (_selectedPath == string.Empty)
                {
                    return;
                }
                _keyTableAsset.RemoveKeyByPath(_selectedPath);
                _keyTableAsset.AddKeyByPath(_parameterPathInput);
                EditorUtility.SetDirty(_keyTableAsset);
            }

            if (removeButtonAction)
            {
                if (_selectedPath == string.Empty)
                {
                    return;
                }
                _keyTableAsset.RemoveKeyByPath(_selectedPath);
                EditorUtility.SetDirty(_keyTableAsset);
            }

            if (getSelectedPathToInputAction)
            {
                _parameterPathInput = _selectedPath;
            }
            #endregion
        }

        [MenuItem("General Game Dev Kit/Value Table System/Management/Open Management Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<KeyTableManagementWindow>();
            window.titleContent = new GUIContent("Value Table Management");
            window.minSize = WindowSize;
            window.maxSize = WindowSize;
            window.Show();
        }
    }
}