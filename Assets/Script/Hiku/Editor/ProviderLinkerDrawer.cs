using System.Linq;
using Hiku.Core;
using UnityEditor;
using UnityEngine;

namespace Hiku.Editor
{
    /// <summary>
    /// Draws a read-only list of types provided by the component.
    /// </summary>
    [CustomPropertyDrawer(typeof(ProviderLinker))]
    public class ProviderLinkerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var providerTypes = ProvidersCreator.GetProviderTypes(property.serializedObject.targetObject.GetType());
            int count = providerTypes.Count();
            if (count > 0)
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Provides"));
                
                EditorGUI.LabelField(
                    new Rect(position.x, position.y, position.width, count * EditorGUIUtility.singleLineHeight),
                    string.Join("\n", providerTypes.Select(type => type.GetFriendlyName()))
                );
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ProvidersCreator.GetProviderTypes(property.serializedObject.targetObject.GetType()).Count() * EditorGUIUtility.singleLineHeight;
        }
    }
}