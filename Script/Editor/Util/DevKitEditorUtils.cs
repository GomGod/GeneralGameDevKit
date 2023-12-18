using System;
using System.Reflection;
using UnityEditor;

namespace Developer.GeneralGameDevKit.Editor
{
    public static class DevKitEditorUtils
    {
        private const BindingFlags AllBindingFlags = (BindingFlags)(-1);

        /// <summary>
        /// Returns attributes of type <typeparamref name="T"/> on <paramref name="serializedProperty"/>.
        /// </summary>
        public static T[] GetAttributes<T>(this SerializedProperty serializedProperty, bool inherit) where T : Attribute
        {
            if (serializedProperty == null)
            {
                throw new ArgumentNullException(nameof(serializedProperty));
            }

            var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

            if (targetObjectType == null)
            {
                throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
            }

            foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
            {
                var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
                if (fieldInfo != null)
                {
                    return (T[])fieldInfo.GetCustomAttributes<T>(inherit);
                }

                var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
                if (propertyInfo != null)
                {
                    return (T[])propertyInfo.GetCustomAttributes<T>(inherit);
                }
            }

            throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
        }
    }
}