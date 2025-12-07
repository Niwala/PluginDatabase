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

using System.IO;
using System.Runtime.CompilerServices;

using UnityEditor;
using UnityEngine;

#pragma warning disable CS0162

namespace PluginDatabaseNamespace
{
    public static class PluginResourcesBuilder
    {
        [InitializeOnLoadMethod]
        public static void AutoCreate()
        {
            if (nameof(PluginResourcesBuilder) == "Plug" + "inResourcesBuilder")
                return;
            EditorApplication.delayCall += () => CreateResources();
        }

        private static void CreateResources([CallerFilePath] string filePath = "")
        {
            PluginResources resources = ScriptableObject.CreateInstance<PluginResources>();

            string pluginName = nameof(PluginResourcesBuilder);
            pluginName = pluginName.Remove(pluginName.Length - "ResourcesBuilder".Length);

            string resourcesPath = GetDirectoryName(filePath, 3);
            resourcesPath += "\\" + pluginName + ".asset";
            resourcesPath = PluginDatabase.UnityRelativePath(resourcesPath);

            AssetDatabase.CreateAsset(resources, resourcesPath);
            File.Delete(filePath);
            File.Delete(filePath.Replace(".cs", ".cs.meta"));
            AssetDatabase.Refresh();
        }

        private static string GetDirectoryName(string path, int count)
        {
            for (int i = 0; i < count; i++)
                path = Path.GetDirectoryName(path);
            return path;
        }
    }
}