using System;
using System.Collections.Generic;
using System.Linq;
using Developer.GeneralGameDevKit.Editor;
using Developer.GeneralGameDevKit.TagSystem;
using UnityEditor;
using UnityEngine;

namespace GeneralGameDevKit.KeyTableSystem.Internal.Editor
{
    [CustomPropertyDrawer(typeof(KeyTableAttribute))]
    public class KeyTablePropertyDrawer : PropertyDrawer
    {
        private const float Margin = 1;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var defaultHeight = base.GetPropertyHeight(property, label);
            return defaultHeight * 2 + Margin * 3;
        }

        private string GetStringValueFromProperty(SerializedProperty property)
        {
            return property.type switch
            {
                nameof(DevKitTag) => property.FindPropertyRelative(DevKitTag.TagKeyFieldName).stringValue,
                _ => property.stringValue
            };
        }

        private void SetStringValueAtProperty(SerializedProperty property, string content)
        {
            switch (property.type)
            {
                case nameof(DevKitTag):
                    property.FindPropertyRelative(DevKitTag.TagKeyFieldName).stringValue = content;
                    break;
                default:
                    property.stringValue = content;
                    break;

            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            var customAttributes = fieldInfo.GetCustomAttributes(false).ToList();
            var keyTableAttributeIdx = customAttributes.FindIndex(o => o is KeyTableAttribute);
            KeyTableAttribute keyTableAttribute;

            if (keyTableAttributeIdx < 0)
            {
                keyTableAttribute = property.GetAttributes<KeyTableAttribute>(false)[0];
            }
            else if (customAttributes[keyTableAttributeIdx] is KeyTableAttribute)
            {
                keyTableAttribute = customAttributes[keyTableAttributeIdx] as KeyTableAttribute;
            }
            else
            {
                throw new Exception("Wrong Attribute Usage Exception");
            }

            var paramsList = keyTableAttribute == null ? new List<KeyEntity>() : KeyTableAssetManager.Instance.GetAllKeys(keyTableAttribute.AssetName).ToList();
            if (paramsList.Count == 0)
            {
                EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
                var noticePos = position;
                noticePos.x += position.width * 0.35f;
                EditorGUI.LabelField(noticePos, "There is no table match with name.", EditorStyles.boldLabel);
                return;
            }

            var targetGuid = GetStringValueFromProperty(property);
            var currentSelectedEntity = paramsList.FirstOrDefault(k => k.guid.Equals(targetGuid));

            var labelRect = position;
            labelRect.height = base.GetPropertyHeight(property, label);
            labelRect.x += position.width * 0.35f;

            var popupRect = position;
            popupRect.height = base.GetPropertyHeight(property, label);
            popupRect.y += popupRect.height + Margin;

            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(labelRect, $"Table Source : {keyTableAttribute.AssetName}", EditorStyles.boldLabel);
            var selectedIdx = paramsList.IndexOf(currentSelectedEntity);
            if (selectedIdx < 0)
                selectedIdx = 0;
            
            selectedIdx = EditorGUI.Popup(popupRect, property.displayName, selectedIdx, paramsList.Select(k => k.pathOfKey).ToArray());
            if (!EditorGUI.EndChangeCheck()) return;
            
            var newSelectedKey = paramsList[selectedIdx];
            SetStringValueAtProperty(property, newSelectedKey.guid);
        }
    }
}
