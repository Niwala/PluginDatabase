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

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace PluginDatabaseNamespace
{
    static class PluginProjectSettingsTab
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            PluginProjectSettingsEditor editor = new PluginProjectSettingsEditor();

            return new SettingsProvider(editor.path, editor.scope)
            {
                label = editor.label,
                activateHandler = (_, root) =>
                {
                    SerializedObject serializedObject = new SerializedObject(PluginDatabase.ProjectSettings);

                    editor.serializedObject = serializedObject;

                    VisualElement editorRoot = editor.Create();
                    editorRoot.RegisterCallback((AttachToPanelEvent e) => editor.Enable());
                    editorRoot.RegisterCallback((DetachFromPanelEvent e) => editor.Disable());
                    editorRoot.TrackSerializedObjectValue(serializedObject, (SerializedObject) =>
                    {
                        editor.Changed();
                        PluginDatabase.OnProjectSettingsChanged?.Invoke();
                    });

                    root.Add(editorRoot);
                    editorRoot.Bind(serializedObject);

                    root.RegisterCallback((DetachFromPanelEvent e) =>
                    {
                        if (serializedObject != null)
                        {
                            serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            PluginDatabase.ProjectSettings.SaveFile();

                            if (editor.dirty)
                                PluginDatabase.OnProjectSettingsSaved?.Invoke();
                        }
                    });
                },
                keywords = editor.GetSearchKeywords()
            };
        }
    }
}