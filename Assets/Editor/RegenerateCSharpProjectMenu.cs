using System.Reflection;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Forces Unity to generate .csproj / .sln for Cursor IntelliSense.
/// The External Tools checkboxes only appear when External Script Editor
/// is Visual Studio / Visual Studio Code (not a browsed Cursor.app).
/// </summary>
public static class RegenerateCSharpProjectMenu
{
    const string MenuPath = "Tools/Regenerate C# Project Files";

    [MenuItem(MenuPath)]
    public static void Regenerate()
    {
        // When VS / VS Code is the current editor, this is enough.
        CodeEditor.CurrentEditor?.SyncAll();

        // Fallback used when Cursor (or another unknown app) is selected:
        // temporarily uses a discovered VS Code / VS install to write project files.
        var cliType = System.Type.GetType(
            "Microsoft.Unity.VisualStudio.Editor.Cli, Unity.VisualStudio.Editor");
        var generate = cliType?.GetMethod(
            "GenerateSolution",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (generate == null)
        {
            Debug.LogWarning(
                "Visual Studio Editor package CLI not found. " +
                "Set External Script Editor to Visual Studio Code, then use Assets → Open C# Project.");
            return;
        }

        generate.Invoke(null, null);
        AssetDatabase.Refresh();
        Debug.Log(
            "C# project files regenerated. Look for .csproj / .sln in the project root, " +
            "then in Cursor: Developer → Reload Window.");
    }
}
