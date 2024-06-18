using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

public class TextFileEditor : EditorWindow
{
    private string filePath = "";
    private string fileContent = "";
    private Vector2 scrollPosition;
    private static bool isFirstInvocation = true;

    [MenuItem("Assets/Open Text File Editor", true)]
    public static bool ValidateOpenTextFileEditor()
    {
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return Path.GetExtension(path) == ".txt";
        }
        return false;
    }

    [MenuItem("Assets/Open Text File Editor")]
    public static void OpenTextFileEditor()
    {
        TextFileEditor window = GetWindow<TextFileEditor>("Text File Editor");

        if (isFirstInvocation)
        {
            Rect mainWindowRect = EditorGUIUtilityExtensions.GetMainWindowPosition();
            window.position = new Rect(
                mainWindowRect.x + 800,
                mainWindowRect.y + mainWindowRect.height - 900,
                600,
                300
            );
            isFirstInvocation = false;
        }

        window.LoadSelectedFile();
    }

    private void OnGUI()
    {
        GUILayout.Label("Edit Text File", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("File Path:", filePath);

        if (!string.IsNullOrEmpty(filePath))
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            fileContent = EditorGUILayout.TextArea(fileContent, GUILayout.Height(position.height - 100));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Save File"))
            {
                SaveFile();
            }
        }
        else
        {
            GUILayout.Label("No valid text file selected.");
        }
    }

    public void LoadSelectedFile()
    {
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Path.GetExtension(path) == ".txt")
            {
                filePath = path;
                fileContent = File.ReadAllText(filePath);
            }
            else
            {
                filePath = "";
                fileContent = "";
                EditorUtility.DisplayDialog("Invalid File", "Please select a valid text file.", "OK");
            }
        }
        else
        {
            filePath = "";
            fileContent = "";
            EditorUtility.DisplayDialog("No File Selected", "Please select a text file from the Assets folder.", "OK");
        }
    }

    private void SaveFile()
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, fileContent);
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.DisplayDialog("No File Selected", "Please load a text file before saving.", "OK");
        }
    }
}

public static class EditorGUIUtilityExtensions
{
    public static Rect GetMainWindowPosition()
    {
        System.Type containerWindowType = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(type => type.Name == "ContainerWindow");

        if (containerWindowType == null)
        {
            throw new System.Exception("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
        }

        var showModeField = containerWindowType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var positionProperty = containerWindowType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var windows = Resources.FindObjectsOfTypeAll(containerWindowType);
        foreach (var win in windows)
        {
            if ((int)showModeField.GetValue(win) == 4) // main window
            {
                return (Rect)positionProperty.GetValue(win, null);
            }
        }

        throw new System.Exception("Can't find the main container window. Maybe something has changed inside Unity");
    }
}