/*
 * MIT License
 * 
 * Copyright (c) 2025 Sam's Backpack
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace PluginDatabaseNamespace
{
    [CustomEditor(typeof(PluginDatabaseWizard))]
    public class PluginDatabaseWizardEditor : Editor
    {
        private PluginDatabaseWizard wizard;

        private TextElement generalInfo;
        private TextField pluginNameField;
        private TextField namespaceField;
        public EnumField projectIntegration;
        public EnumField projectBuildAccess;
        public EnumField userIntegration;
        public TextElement resourcesPath;
        public TextElement projectPath;
        public TextElement userPath;
        public Toggle keepWizard;
        public Button applyButton;

        private const string generalInfoMsg = "Once the wizard is complete, the scripts will be customized according to your preferences";

        public override VisualElement CreateInspectorGUI()
        {
            wizard = (PluginDatabaseWizard)target;

            //Add & Bind document
            VisualElement root = new VisualElement();
            root.Add(wizard.document.CloneTree());
            BindWizard(root);

            return root;
        }

        private void BindWizard(VisualElement root)
        {
            generalInfo = root.Q<TextElement>("general-info");
            pluginNameField = root.Q<TextField>("plugin-name");
            namespaceField = root.Q<TextField>("namespace");

            projectIntegration = root.Q<EnumField>("project-integration");
            projectBuildAccess = root.Q<EnumField>("project-build-access");
            userIntegration = root.Q<EnumField>("user-integration");

            resourcesPath = root.Q<TextElement>("resources-path");
            projectPath = root.Q<TextElement>("project-path");
            userPath = root.Q<TextElement>("user-path");

            keepWizard = root.Q<Toggle>("keep-wizard");
            applyButton = root.Q<Button>("apply-btn");

            pluginNameField.RegisterValueChangedCallback(OnPluginNameChanged);
            projectIntegration.RegisterValueChangedCallback(OnChangeProjectIntegration);
            keepWizard.RegisterValueChangedCallback(OnChangeKeepWizard);

            applyButton.clicked += Apply;
        }

        private void OnPluginNameChanged(ChangeEvent<string> e)
        {
            resourcesPath.text = e.newValue + "Database.Resources";
            projectPath.text = e.newValue + "Database.ProjectSettings";
            userPath.text = e.newValue + "Database.UserSettings";
        }

        private void Apply()
        {
            //Check plugin name
            if (string.IsNullOrEmpty(wizard.pluginName))
            {
                EditorUtility.DisplayDialog("Plugin Database Wizard", "You must specify a name for your plugin.", "Ok");
                return;
            }

            if (!IsValidClassName(wizard.pluginName))
            {
                EditorUtility.DisplayDialog("Plugin Database Wizard", "The plugin name must be a valid C# class name.", "Ok");
                return;
            }

            //Check namespace
            if (!string.IsNullOrEmpty(wizard.namespaceName) && !IsValidNamespace(wizard.namespaceName))
            {
                EditorUtility.DisplayDialog("Plugin Database Wizard", "The namespace is not a valid namespace.\nPlease use a standard C# namespace such as \"Company.PluginName.Database\".", "Ok");
                return;
            }

            //Check conflicts
            if (GetConflicts(out string conflictMsg))
            {
                if (EditorUtility.DisplayDialog("Plugin Database Wizard", conflictMsg, "Abort", "Override and Continue"))
                    return;
            }

            //Start dialogs
            if (!EditorUtility.DisplayDialog("Plugin Database Wizard", "Once the wizard has been applied, there will be no option to undo.\n\nPlease check that everything is correct.", "Next", "Cancel"))
            {
                return;
            }

            //Summary
            string summary = "You are about to setup your database as follows:\n\n";
            summary += $"Plugin name :\n{wizard.pluginName}\n(Main class name : {wizard.pluginName}Database)\n\n";

            if (string.IsNullOrWhiteSpace(wizard.namespaceName))
                summary += $"There will be no namespace.";
            else
                summary += $"Namespace:\n{wizard.namespaceName}";

            if (!EditorUtility.DisplayDialog("Plugin Database Wizard", summary, "Next", "Cancel"))
                return;


            //Summary > Project settings
            summary = "";
            switch (wizard.projectSettings)
            {
                case PluginDatabaseWizard.ProjectSettings.None:
                    summary += "No project settings\n";
                    break;
                case PluginDatabaseWizard.ProjectSettings.ScriptsOnly:
                    switch (wizard.projectSettingsInBuild)
                    {
                        case PluginDatabaseWizard.ProjectSettingsInBuild.None:
                            summary += $"Script-only & Editor-only project settings\n";
                            break;
                        case PluginDatabaseWizard.ProjectSettingsInBuild.Partial:
                            summary += $"Script-only & Partial runtime project settings\n";
                            break;
                        case PluginDatabaseWizard.ProjectSettingsInBuild.Full:
                            summary += $"Script-only & Full runtime project settings\n";
                            break;
                    }
                    break;
                case PluginDatabaseWizard.ProjectSettings.ScriptsAndTab:
                    switch (wizard.projectSettingsInBuild)
                    {
                        case PluginDatabaseWizard.ProjectSettingsInBuild.None:
                            summary += $"Script + Tab & Editor-only project settings\n";
                            break;
                        case PluginDatabaseWizard.ProjectSettingsInBuild.Partial:
                            summary += $"Script + Tab & Partial runtime project settings\n";
                            break;
                        case PluginDatabaseWizard.ProjectSettingsInBuild.Full:
                            summary += $"Script + Tab & Full runtime project settings\n";
                            break;
                    }
                    break;
            }

            //Summary > User settings
            switch (wizard.userSettings)
            {
                case PluginDatabaseWizard.UserSettings.None:
                    summary += "No user settings.";
                    break;
                case PluginDatabaseWizard.UserSettings.ScriptsOnly:
                    summary += $"Script-only user settings";
                    break;
                case PluginDatabaseWizard.UserSettings.ScriptsAndTab:
                    summary += $"Script + Tab user settings";
                    break;
            }

            string applyLabel = wizard.keepWizard ? "Apply" : "Apply and Delete Wizard";
            if (!EditorUtility.DisplayDialog("Plugin Database Wizard", summary, applyLabel, "Cancel"))
                return;


            //Apply
            CopyPluginFiles();
            if (!wizard.keepWizard)
                DeleteWizard();
            AssetDatabase.Refresh();
        }

        private static bool IsValidClassName(string name)
        {
            if (name.Contains("@")) return false;

            const string pattern =
                @"^[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}]*$";

            return Regex.IsMatch(name, pattern);
        }

        private static bool IsValidNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns)) return false;
            if (ns.Contains("@")) return false;
            if (ns.StartsWith(".") || ns.EndsWith(".")) return false;
            if (ns.Contains("..")) return false;

            const string identPattern =
                @"^[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}]*$";

            var parts = ns.Split('.');

            foreach (var p in parts)
            {
                if (!Regex.IsMatch(p, identPattern))
                    return false;
            }

            return true;
        }

        private bool GetConflicts(out string conflictFiles)
        {
            conflictFiles = "The operation cannot be performed because some files already exist at the target location.\n";
            bool hasConflict = false;

            string root = Path.GetDirectoryName(AssetDatabase.GetAssetPath(wizard.editorDirectory));

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(AssetDatabase.GetAssetPath(wizard.editorDirectory), "*", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(AssetDatabase.GetAssetPath(wizard.runtimeDirectory), "*", SearchOption.AllDirectories));
            
            string exportDir = wizard.exportDirectory;
            exportDir = exportDir.Replace("/", "\\");
            if (!exportDir.EndsWith("\\"))
                exportDir += "\\";

            foreach (var file in files)
            {
                if (file.EndsWith(".meta"))
                    continue;

                string relativePath = Path.GetRelativePath(root, file);
                relativePath = relativePath.Replace("Plugin", wizard.pluginName);
                string newPath = exportDir + relativePath;

                if (File.Exists(newPath))
                {
                    conflictFiles += " - " + newPath + "\n";
                    hasConflict = true;
                }
            }

            return hasConflict;
        }

        private void CopyPluginFiles()
        {
            if (!Directory.Exists(wizard.exportDirectory))
                Directory.CreateDirectory(wizard.exportDirectory);

            string root = Path.GetDirectoryName(AssetDatabase.GetAssetPath(wizard.editorDirectory));

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(AssetDatabase.GetAssetPath(wizard.editorDirectory), "*", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(AssetDatabase.GetAssetPath(wizard.runtimeDirectory), "*", SearchOption.AllDirectories));

            string exportDir = wizard.exportDirectory;
            exportDir = exportDir.Replace("/", "\\");
            if (!exportDir.EndsWith("\\"))
                exportDir += "\\";

            foreach (var file in files)
            {
                if (file.EndsWith(".meta"))
                    continue;

                string minName = wizard.pluginName.Substring(0, 1).ToLower() + wizard.pluginName.Substring(1);
                string majName = wizard.pluginName.Substring(0, 1).ToUpper() + wizard.pluginName.Substring(1);
                bool hasTab = false;

                string relativePath = Path.GetRelativePath(root, file);
                relativePath = relativePath.Replace("Plugin", wizard.pluginName);
                string newPath = exportDir + relativePath;

                string fileContent = File.ReadAllText(file);
                fileContent = fileContent.Replace("PluginDatabaseNamespace", wizard.namespaceName);
                fileContent = fileContent.Replace("plugin", minName);
                fileContent = fileContent.Replace("Plugin", majName);

                //Project settings integration
                switch (wizard.projectSettings)
                {
                    case PluginDatabaseWizard.ProjectSettings.None:

                        if (relativePath.Contains("ProjectSettings"))
                            continue;

                        fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_NONE");
                        break;

                    case PluginDatabaseWizard.ProjectSettings.ScriptsOnly:

                        if (relativePath.Contains("ProjectSettingsTab") || relativePath.Contains("ProjectSettingsEditor"))
                            continue;

                        switch (wizard.projectSettingsInBuild)
                        {
                            case PluginDatabaseWizard.ProjectSettingsInBuild.None:
                                fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_EDITOR_ONLY");
                                break;

                            case PluginDatabaseWizard.ProjectSettingsInBuild.Partial:
                                fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_PARTIAL");
                                break;
                            case PluginDatabaseWizard.ProjectSettingsInBuild.Full:
                                fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_FULL");
                                break;
                        }

                        break;

                    default:

                        hasTab = true;
                        switch (wizard.projectSettingsInBuild)
                        {
                            case PluginDatabaseWizard.ProjectSettingsInBuild.None:
                                fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_EDITOR_ONLY");
                                break;

                            case PluginDatabaseWizard.ProjectSettingsInBuild.Partial:
                                fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_PARTIAL");
                                break;
                            case PluginDatabaseWizard.ProjectSettingsInBuild.Full:
                                fileContent = fileContent.Replace("PROJECT_SETTINGS_OPTION", "PROJECT_SETTINGS_FULL");
                                break;
                        }
                        break;
                }

                //User settings integration
                switch (wizard.userSettings)
                {
                    case PluginDatabaseWizard.UserSettings.None:

                        if (relativePath.Contains("UserSettings"))
                            continue;

                        fileContent = fileContent.Replace("USER_SETTINGS_OPTION", "USER_SETTINGS_NONE");
                        break;

                    case PluginDatabaseWizard.UserSettings.ScriptsOnly:

                        if (relativePath.Contains("UserSettingsTab") || relativePath.Contains("UserSettingsEditor"))
                            continue;

                        fileContent = fileContent.Replace("USER_SETTINGS_OPTION", "USER_SETTINGS_EDITOR_ONLY");
                        break;

                    default:
                        hasTab = true;
                        fileContent = fileContent.Replace("USER_SETTINGS_OPTION", "USER_SETTINGS_EDITOR_ONLY");
                        break;
                }

                if (!hasTab && relativePath.Contains("SettingsTab"))
                    continue;

                string directory = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(newPath, fileContent);
            }
        }

        private void DeleteWizard()
        {
            string root = Path.GetDirectoryName(AssetDatabase.GetAssetPath(wizard.editorDirectory));
            string rootMeta = root + ".meta";
            Directory.Delete(root, true);
            File.Delete(rootMeta);
        }

        private void OnChangeProjectIntegration(ChangeEvent<Enum> e)
        {
            projectBuildAccess.SetEnabled(wizard.projectSettings != PluginDatabaseWizard.ProjectSettings.None);
        }

        private void OnChangeKeepWizard(ChangeEvent<bool> e)
        {
            generalInfo.text = generalInfoMsg + (e.newValue ? "." : " and the wizard will be deleted.");
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}