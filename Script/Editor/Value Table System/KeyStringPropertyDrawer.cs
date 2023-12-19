using System.Linq;
using Developer.GeneralGameDevKit.Editor;
using UnityEditor;
using UnityEngine;

namespace GeneralGameDevKit.ValueTableSystem.Internal.Editor
{
    /// <summary>
    /// Property drawer script for KeyString
    /// </summary>
    [CustomPropertyDrawer(typeof(KeyString))]
    public class KeyStringPropertyDrawer : PropertyDrawer
    {
        private const float Margin = 1;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var defaultHeight = base.GetPropertyHeight(property, label);
            return defaultHeight * 2 + Margin * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            var keyValueProperty = property.FindPropertyRelative(KeyString.GetKeyStringFieldName());
            var valueManagerAttributes = property.GetAttributes<KeyTableAttribute>(false);
            var paramsList = KeyTableAssetManager.Instance.GetAllKeys(valueManagerAttributes[0].AssetName)?.ToList();

            if (paramsList == null || valueManagerAttributes.Length == 0)
            {
                EditorGUI.LabelField(position,"There is no table match with name. Use ", EditorStyles.boldLabel);
                return;
            }

            var currentSelected = paramsList.IndexOf(keyValueProperty.stringValue);
            if (currentSelected < 0)
            {
                currentSelected = 0;
            }

            var drawerRect = position;
            drawerRect.height = base.GetPropertyHeight(property, label);
            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(drawerRect,$"Value : {label}", EditorStyles.boldLabel);
            drawerRect.y += drawerRect.height + Margin;
            currentSelected = EditorGUI.Popup(drawerRect,"   Key", currentSelected, paramsList.ToArray());
            
            if (EditorGUI.EndChangeCheck())
            {
                keyValueProperty.stringValue = paramsList[currentSelected];
            }
        }
    }
}