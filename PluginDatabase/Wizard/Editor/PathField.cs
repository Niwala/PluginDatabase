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

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class PathField : VisualElement
{
    [UxmlAttribute]
    public string bindingPath 
    { 
        get
        {
            return textField.bindingPath;
        }
        set
        {
            textField.bindingPath = value;
        }
    }

    [UxmlAttribute]
    public string label
    {
        get
        {
            return textField.label;
        }
        set
        {
            textField.label = value;
        }
    }

    private TextField textField;
    private Button browseBtn;

    public PathField()
    {
        //Styles
        style.flexDirection = FlexDirection.Row;
        style.flexShrink = 0;


        //Add text field
        textField = new TextField();
        textField.AddToClassList("path-field");
        textField.label = "My path";
        textField.Q<VisualElement>("unity-text-input").AddToClassList("path-field-input");
        Add(textField);


        //Add button
        browseBtn = new Button();
        browseBtn.text = "...";
        browseBtn.clicked += Browse;
        browseBtn.AddToClassList("path-field-button");
        browseBtn.focusable = false;
        Add(browseBtn);
    }

    private void Browse()
    {
        string directory = EditorUtility.OpenFolderPanel("Plugin Database Wizard", "Assets", "");

        //Cancel
        if (string.IsNullOrEmpty(directory))
            return;

        string relativePath = UnityRelativePath(directory);

        if (string.IsNullOrEmpty(relativePath))
        {
            EditorUtility.DisplayDialog("Plugin Database Wizard", "The path must be located in the Assets directory.", "Ok");
            return;
        }

        textField.value = UnityRelativePath(directory);
    }

    private static string UnityRelativePath(string absolutePath)
    {
        string project = Path.GetFullPath(Application.dataPath + "/..") + Path.DirectorySeparatorChar;
        absolutePath = Path.GetFullPath(absolutePath);

        //Assets
        string assets = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
        if (absolutePath.StartsWith(assets))
        {
            return "Assets/" + absolutePath.Substring(assets.Length).Replace('\\', '/');
        }
        else if (absolutePath == Path.GetFullPath(Application.dataPath))
        {
            return "Assets";
        }

        return null;
    }
}
