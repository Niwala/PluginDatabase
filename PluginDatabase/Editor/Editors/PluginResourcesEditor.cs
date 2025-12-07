using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace PluginDatabaseNamespace
{
    [CustomEditor(typeof(PluginResources))]
    public class PluginResourcesEditor : Editor
    {
        //Create default UI + change tracking
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();


            //Track changes
            root.TrackSerializedObjectValue(serializedObject, (SerializedObject e) => PluginDatabase.OnResourcesChanged?.Invoke());


            //Create default fields
            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                SerializedProperty property = serializedObject.FindProperty(field.Name);
                if (property != null && field.GetCustomAttribute<HideInInspector>() == null)
                {
                    PropertyField propertyField = new PropertyField(property);
                    root.Add(propertyField);
                }
            }

            
            return root;
        }
    }
}