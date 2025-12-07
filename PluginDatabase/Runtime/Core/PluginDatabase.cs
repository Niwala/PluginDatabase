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

#define PROJECT_SETTINGS_OPTION
#define USER_SETTINGS_OPTION

using UnityEngine;

using UnityEditor;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace PluginDatabaseNamespace
{
    public static class PluginDatabase
    {
        public const string pluginName = "MyUnityPlugin";
        public const string resourcesFileType = "asset";


        //Project settings
#if !PROJECT_SETTINGS_NONE && (!PROJECT_SETTINGS_EDITOR_ONLY || UNITY_EDITOR)
        public static Action OnProjectSettingsChanged;
        public static Action OnProjectSettingsSaved;

        public static PluginProjectSettings ProjectSettings
        {
#if UNITY_EDITOR
            get => PluginProjectSettings.instance;
#else
        get
        {
            if (runtimeProjectSettings == null)
            {
                runtimeProjectSettings = ScriptableObject.CreateInstance<PluginProjectSettings>();
                Resources.serializedProjectSettings.Deserialize(runtimeProjectSettings);
            }

            return runtimeProjectSettings;
        }
#endif
        }

#if !UNITY_EDITOR
    private static PluginProjectSettings runtimeProjectSettings;
#endif
#endif

        //User settings
#if !USER_SETTINGS_NONE
#if UNITY_EDITOR
        public static Action OnUserSettingsChanged;
        public static Action OnUserSettingsSaved;

        public static PluginUserSettings UserSettings
        {
            get => PluginUserSettings.instance;
        }
#endif
#endif

        //Resources
        public static Action OnResourcesChanged;

        public static PluginResources Resources
        {
            get
            {
                if (resources == null)
                    LoadResources();
                return resources;
            }
        }

        private static PluginResources resources;

#if UNITY_EDITOR
        private static void LoadResources([CallerFilePath] string filePath = "")
        {
            //Load preloaded asset
            if (PluginResources.instance != null)
            {
                resources = PluginResources.instance;
                return;
            }

            //Try to load ressources with file search starting from script location
            foreach (var file in RecursiveBubbleUpSearch(filePath, resourcesFileType))
            {
                string localPath = UnityRelativePath(file);
                if (AssetDatabase.GetMainAssetTypeAtPath(localPath) == typeof(PluginResources))
                {
                    resources = AssetDatabase.LoadAssetAtPath<PluginResources>(localPath);
                    if (resources != null)
                    {
                        RegisterAsPeloadedAsset(resources);
                        return;
                    }
                }
            }


            //Unable to find the plugin resources
            throw new Exception(
                $"Unable to load resources from the {pluginName} plugin." +
                $"\nThe resource asset is probably missing or cannot be imported by Unity.");
        }

#else
    private static void LoadResources()
    {
        resources = PluginResources.instance;
        if (resources != null)
            return;

        //Unable to find the plugin resources.
        throw new Exception(
            $"Unable to load resources from the {pluginName} plugin from preloaded assets.\n" +
            $"This is an unexpected behavior.\n" +
            $"Could an external script have removed the plugin resources during the build? ");
    }
#endif

        private static IEnumerable<string> RecursiveBubbleUpSearch(string directory, string fileType)
        {
            string localPath = UnityRelativePath(directory);
            if (string.IsNullOrEmpty(localPath))
                yield break;

            string parentDirectory = Path.GetDirectoryName(directory);

            string[] files = Directory.GetFiles(parentDirectory, "*." + fileType);
            foreach (var file in files)
            {
                yield return file;
            }

            string[] subDirectories = Directory.GetDirectories(parentDirectory);
            for (int i = 0; i < subDirectories.Length; i++)
            {
                if (subDirectories[i] == directory)
                    continue;

                foreach (var file in RecursiveTrickleDownSearch(subDirectories[i], fileType))
                {
                    yield return file;
                }
            }

            foreach (var file in RecursiveBubbleUpSearch(parentDirectory, fileType))
            {
                yield return file;
            }
        }

        private static IEnumerable<string> RecursiveTrickleDownSearch(string directory, string fileType)
        {
            string localPath = UnityRelativePath(directory);
            if (string.IsNullOrEmpty(localPath))
                yield break;

            string[] files = Directory.GetFiles(directory, "*." + fileType);
            foreach (var file in files)
            {
                yield return file;
            }

            string[] subDirectories = Directory.GetDirectories(directory);
            foreach (var sub in subDirectories)
            {
                foreach (var file in RecursiveTrickleDownSearch(sub, fileType))
                {
                    yield return file;
                }
            }
        }

        public static string UnityRelativePath(string absolutePath)
        {
            string project = Path.GetFullPath(Application.dataPath + "/..") + Path.DirectorySeparatorChar;
            absolutePath = Path.GetFullPath(absolutePath);

            //Assets
            string assets = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
            if (absolutePath.StartsWith(assets))
            {
                return "Assets/" + absolutePath.Substring(assets.Length).Replace('\\', '/');
            }

            //Packages
            string packages = Path.GetFullPath(Path.Combine(project, "Packages")) + Path.DirectorySeparatorChar;
            if (absolutePath.StartsWith(packages))
            {
                return "Packages/" + absolutePath.Substring(packages.Length).Replace('\\', '/');
            }
            return null;
        }

        private static void RegisterAsPeloadedAsset(UnityEngine.Object obj)
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            if (!preloadedAssets.Contains(obj))
            {
                preloadedAssets.Add(obj);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
        }

#if UNITY_EDITOR
        private class BuildSetup : IPreprocessBuildWithReport
        {
            public int callbackOrder => -100;

            public void OnPreprocessBuild(BuildReport report)
            {
                var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();

#if !PROJECT_SETTINGS_NONE
                Resources.serializedProjectSettings = new PluginSerializedSettings(ProjectSettings);
                EditorUtility.SetDirty(Resources);
#endif

                RegisterAsPeloadedAsset(Resources);
            }
        }
#endif
    }
}