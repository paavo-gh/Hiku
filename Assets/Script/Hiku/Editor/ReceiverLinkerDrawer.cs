using UnityEditor;
using UnityEngine;
using Hiku.Core;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Hiku.Editor
{
    /// <summary>
    /// Draws a list of the component's receiver methods tagged with [Receive] attribute
    /// and shows a popup menu to choose the received type.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReceiverLinker))]
    public class ReceiverLinkerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var linker = new ReceiverLinker();
            var members = property.FindPropertyRelative(nameof(linker.Members));

            var accessorList = new List<ReceiverLinkerItem>();
            {
                for (int i = 0; i < members.arraySize; i++)
                {
                    accessorList.Add(LoadObject(members.GetArrayElementAtIndex(i)));
                }
            }

            members.ClearArray();

            var targetObject = property.serializedObject.targetObject as MonoBehaviour;
            var targetObjectType = targetObject.GetType();
            foreach (var method in ReceiverMethod.GetAll(targetObject))
            {
                var option = DrawProviderOptions(position, accessorList.Find(item => item.Receiver == method.Name), method, targetObject);
                position.y += EditorGUIUtility.singleLineHeight;

                var linkerItem = new ReceiverLinkerItem();
                linkerItem.Receiver = method.Name;
                linkerItem.ReceiverType = option.ReceiverTypeName;
                linkerItem.Path = option.Path;
                
                if (!string.IsNullOrEmpty(option.ReceiverTypeName))
                {
                    int size = members.arraySize;
                    members.InsertArrayElementAtIndex(size);
                    var obj = members.GetArrayElementAtIndex(size);
                    SaveObject(obj, linkerItem);
                }
                if (linkerItem.ReceiverType != ReceiverLinker.IngoreReceiver)
                    DrawProviderPreview(position, linkerItem, method, option.ReceiverType, targetObject);
                position.y += EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.EndProperty();
        }

        private ReceiverLinkerItem LoadObject(SerializedProperty element)
        {
            var linkerItem = new ReceiverLinkerItem();
            linkerItem.Receiver = element.FindPropertyRelative(nameof(linkerItem.Receiver)).stringValue;
            linkerItem.ReceiverType = element.FindPropertyRelative(nameof(linkerItem.ReceiverType)).stringValue;
            linkerItem.Path = element.FindPropertyRelative(nameof(linkerItem.Path)).stringValue;
            return linkerItem;
        }

        private void SaveObject(SerializedProperty obj, ReceiverLinkerItem linkerItem)
        {
            obj.FindPropertyRelative(nameof(linkerItem.Receiver)).stringValue = linkerItem.Receiver;
            obj.FindPropertyRelative(nameof(linkerItem.ReceiverType)).stringValue = linkerItem.ReceiverType;
            obj.FindPropertyRelative(nameof(linkerItem.Path)).stringValue = linkerItem.Path;
        }

        private string CleanMethodName(string methodName)
        {
            if (methodName.StartsWith("set_"))
                return methodName.Substring(4);
            return methodName;
        }

        PopupOption DrawProviderOptions(Rect position, ReceiverLinkerItem linkerItem, ReceiverMethod method, object targetObject)
        {
            var controlPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(CleanMethodName(method.Name)));

            var options = new List<PopupOption>
            {
                // None-option
                new PopupOption(receiverTypeName: ReceiverLinker.IngoreReceiver),
                // Automatic option where the receiver type is the parameter type
                new PopupOption(method.Type, receiverTypeName: "")
            };
            options.AddRange(GetOptions(targetObject, method.Type));

            var guiOptions = options.ConvertAll(option => option.PopupString);
            //guiOptions[0] = "<none>";
            
            var selectedIndex = options.FindIndex(option => option.MatchesLinker(linkerItem));
            if (selectedIndex < 0)
            {
                // Can happen if class or method name is changed
                selectedIndex = options.Count;
                options.Add(new PopupOption(path: linkerItem.Path.Split('/'), receiverTypeName: linkerItem.ReceiverType));
                guiOptions.Add(linkerItem.ReceiverType + '/' + linkerItem.Path);
            }

            int index = EditorGUI.Popup(
                new Rect(controlPosition.x, controlPosition.y, controlPosition.width, EditorGUIUtility.singleLineHeight),
                selectedIndex,
                guiOptions.ToArray()
            );

            return options[index];
        }

        void DrawProviderPreview(Rect position, ReceiverLinkerItem linkerItem, ReceiverMethod method, Type receiverType, MonoBehaviour targetObject)
        {
            var controlPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(" "));
            
            IDataProviderObject provider = null;
            if (receiverType != null)
                provider = ReceiverComponentBuilder.FindProviderObject(targetObject, receiverType);

            if (provider != null)
            {
                GUI.enabled = false;
                EditorGUI.ObjectField(
                    new Rect(controlPosition.x, controlPosition.y, controlPosition.width, EditorGUIUtility.singleLineHeight),
                    provider as Component,
                    typeof(Component),
                    true
                );
                GUI.enabled = true;
            }
            else
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.red;
                EditorGUI.LabelField(
                    new Rect(controlPosition.x, controlPosition.y, controlPosition.width, EditorGUIUtility.singleLineHeight),
                    "Provider not found!",
                    style
                );
            }
        }

        class PopupOption
        {
            public readonly Type ReceiverType;
            public readonly string Path;
            public readonly string PopupString;
            public readonly string ReceiverTypeName;

            public PopupOption(Type receiverType = null, IEnumerable<string> path = null, string receiverTypeName = null)
            {
                ReceiverType = receiverType;
                Path = path == null ? "" : string.Join("/", path);
                if (ReceiverType == null)
                    PopupString = "<none>";
                else if (string.IsNullOrEmpty(Path))
                    PopupString = ReceiverType.GetFriendlyName();
                else
                    PopupString = ReceiverType.GetFriendlyName() + ("/" + Path).Replace("/get_", "/");
                
                ReceiverTypeName = receiverTypeName ?? ReceiverType?.FullName;
            }

            public override bool Equals(object obj)
                => (obj as PopupOption)?.PopupString == PopupString;

            public override int GetHashCode() => PopupString.GetHashCode();

            public bool MatchesLinker(ReceiverLinkerItem linkerItem)
            {
                if (linkerItem == null)
                    return string.IsNullOrEmpty(ReceiverTypeName);
                if (linkerItem.ReceiverType == ReceiverTypeName)
                    return Path == linkerItem.Path;
                return false;
            }
        }

        List<PopupOption> GetOptions(object obj, Type linkedType)
        {
            var options = new List<PopupOption>();
            var targetObject = obj as Component;
            if (targetObject != null)
            {
                foreach (var provider in targetObject.transform.GetComponentsInParent<IDataProviderObject>(includeInactive: true))
                {
                    if (object.ReferenceEquals(provider, targetObject))
                        continue;
                    
                    var providers = provider.GetProviders();
                    if (providers != null)
                    {
                        foreach (var dataField in providers.All)
                        {
                            GetGetterMethods(new List<Type> { dataField.Type }, new List<string>(), linkedType, options);
                        }
                    }
                }
            }
            return options;
        }

        /// <summary>
        /// Constructs a list of all possible method call chains that result in the given target type.
        /// Some limits set to keep the possible options under control:
        /// - Only look for methods from classes with [Receivable] attribute
        /// - Up to 8 chained method calls
        /// </summary>
        void GetGetterMethods(List<Type> types, List<string> path, Type targetType, List<PopupOption> list)
        {
            Type memberType = types[types.Count - 1];

            if (targetType.IsAssignableFrom(memberType))
            {
                var option = new PopupOption(types[0], path);
                if (!list.Contains(option))
                    list.Add(option);
            }

            if (memberType.GetCustomAttribute<Receivable>() == null)
                return;
            
            foreach (var method in memberType.GetMethods())
            {
                if (method.DeclaringType == typeof(object))
                    continue;
                if (method.ReturnType == typeof(void))
                    continue;
                if (method.GetParameters().Length > 0)
                    continue;
                if (method.IsStatic)
                    continue;
                if (method.ContainsGenericParameters)
                    continue;
                
                Delegate accessor;
                try
                {
                    accessor = DataAccessorHelper.CreateGetter(method);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Couldn't add {method.DeclaringType}:{method.Name}: " + e.Message + e.StackTrace);
                    continue;
                }

                var returnType = GetterChainCall.GetMethodReturnType(accessor.Method);

                if (returnType != typeof(void) && !types.Contains(returnType) && path.Count < 8)
                {
                    types.Add(returnType);
                    path.Add(method.Name);
                    GetGetterMethods(types, path, targetType, list);
                    path.RemoveAt(path.Count - 1);
                    types.RemoveAt(types.Count - 1);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 2 * EditorGUIUtility.singleLineHeight * ReceiverMethod.GetAll(property.serializedObject.targetObject).Count;
        }
    }
}