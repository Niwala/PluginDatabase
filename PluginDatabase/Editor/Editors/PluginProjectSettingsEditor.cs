using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace PluginDatabaseNamespace
{
    public class PluginProjectSettingsEditor : PluginSettingsTab
    {
        //Tab properties
        public override string path => "Project/Plugin";
        public override string label => "Plugin";
        public override SettingsScope scope => SettingsScope.Project;


        //Custom inspector
        protected override VisualElement OnCreate()
        {
            VisualElement root = new VisualElement() { name = "root" };

            root.Add(new PropertyField(serializedObject.FindProperty("color")));

            return root;
        }


        //Called when the setting page is opened
        protected override void OnEnable()
        {

        }


        //Called when the setting page is closed
        protected override void OnDisable()
        {

        }
    }
}