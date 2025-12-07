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

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using Object = UnityEngine.Object;

#if !PROJECT_SETTINGS_NONE && !(PROJECT_SETTINGS_EDITOR_ONLY && !UNITY_EDITOR)
namespace PluginDatabaseNamespace
{
    [Serializable]
    public struct PluginSerializedSettings
    {
        public string json;
        public ObjectReference[] objects;

        public PluginSerializedSettings(Object target)
        {
            //Write json
            json = JsonUtility.ToJson(target);

            //Store references
            List<ObjectReference> references = new List<ObjectReference>();
            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {

#if PROJECT_SETTINGS_PARTIAL
                if (field.GetCustomAttribute<IncludeInBuildAttribute>() == null)
                    continue;
#endif

                if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    Object obj = field.GetValue(target) as Object;
                    if (obj == null)
                        continue;

                    references.Add(new ObjectReference
                    {
                        name = field.Name,
                        type = field.FieldType.ToString(),
                        obj = obj
                    });
                }
            }
            objects = references.ToArray();
        }

        public void Deserialize<T>(T target) where T : Object
        {
            //Set serializable data
            JsonUtility.FromJsonOverwrite(json, target);


            //Set object references
            Dictionary<(string, string), Object> dictio = new Dictionary<(string, string), Object>();
            foreach (var reference in objects)
            {
                dictio.Add((reference.name, reference.type), reference.obj);
            }


            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {

#if PROJECT_SETTINGS_PARTIAL
                if (field.GetCustomAttribute<IncludeInBuildAttribute>() == null)
                    continue;
#endif

                if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    (string, string) key = (field.Name, field.FieldType.ToString());
                    if (dictio.ContainsKey(key))
                        field.SetValue(target, dictio[key]);
                }
            }
        }

        [Serializable]
        public struct ObjectReference
        {
            public string name;
            public string type;
            public Object obj;
        }
    }
}
#endif