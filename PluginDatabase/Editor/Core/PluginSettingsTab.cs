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

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace PluginDatabaseNamespace
{
    public abstract class PluginSettingsTab
    {
        public abstract string path { get; }
        public abstract string label { get; }
        public abstract SettingsScope scope { get; }
        public bool dirty { get; set; }

        public SerializedObject serializedObject;

        protected abstract VisualElement OnCreate();

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnChanged() { }


        public virtual IEnumerable<string> GetSearchKeywords() { yield break; }




        public VisualElement Create()
        {
            return OnCreate();
        }

        public void Enable()
        {
            OnEnable();
        }

        public void Disable()
        {
            OnDisable();
        }

        public void Changed()
        {
            dirty = true;
            OnChanged();
        }
    }
}