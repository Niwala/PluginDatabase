# PluginDatabase
PluginDatabase is a small utility designed to quickly generate Resources, UserSettings, and ProjectSettings for your Unity project.
```csharp
PluginResources resources = PluginDatabase.Resources;
PluginProjectSettings projectSettings = PluginDatabase.ProjectSettings;
PluginUserSettings userSettings = PluginDatabase.UserSettings;
```

## Installation
1. Download the repository and copy the PluginDatabase folder directly into your Assets directory.
2. Inside the PluginDatabase folder, select the "Plugin Database Wizard.asset" file.
3. Fill in the wizard fields and click Apply.

**Important:** The PluginDatabase folder will delete itself automatically. Do not place any of your own files inside it.
You can run the wizard multiple times in the same project. As long as you specify a different plugin name and/or namespace, you won't encounter conflicts.

## Usage
*In the examples below, replace "Plugin" with the plugin name you defined in the wizard.*

### Resources
Resources are simple ScriptableObjects automatically generated in your project’s Assets.
They allow you to quickly reference assets that can be accessed anywhere in your code.

- To edit the object fields, modify Runtime/Objects/PluginResources.cs.
- To edit the inspector layout, modify Editor/Editors/PluginResourcesEditor.cs.
- To access the instance at runtime or in the editor, use PluginDatabase.Resources.
- The instance is always accessible, both in editor and at runtime.
- You may move the asset anywhere in your project. *(If the reference is lost, the plugin will search for it starting from the Runtime/Core folder.)*

### Project Settings
ProjectSettings provide a dedicated settings page under
Edit → Project Settings…, and can be accessed easily from any C# script.

They are saved inside Unity’s ProjectSettings folder at the root of your project.

- To edit the content, modify Runtime/Objects/PluginProjectSettings.cs.
- To edit the inspector UI, modify Editor/Editors/PluginProjectSettingsEditor.cs.
- To retrieve the instance in C#, use PluginDatabase.ProjectSettings.
- If AccessInBuild = Partial, only fields marked with [IncludeInBuild] will be included in the build *(others use default values)*.
- If AccessInBuild = Full, all field values are copied and available at runtime in builds.
- Data included for builds is temporarily stored in Resources to ensure persistence.

### User Settings
UserSettings provide a page under
Edit → Preferences…, and can be accessed easily from any C# script.

- They are saved inside Unity’s UserSettings folder at the root of your project.
- To edit the content, modify Runtime/Objects/PluginUserSettings.cs.
- To edit the inspector UI, modify Editor/Editors/PluginUserSettingsEditor.cs.
- To retrieve the instance in C#, use PluginDatabase.UserSettings.
- UserSettings are never available in builds.

#### Note
All instances are accessible through the runtime API to simplify integration and testing.
However, you should never access UserSettings *(or ProjectSettings when AccessInBuild is set to None)* in runtime scripts without wrapping the call in:
```csharp
#if UNITY_EDITOR
#endif
```
Failing to do so may prevent your project from building.
