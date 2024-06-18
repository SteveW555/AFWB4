using System.IO;

using UnityEditor;

public class CreateTextFile : EditorWindow
{
    [MenuItem("Assets/Create/Text File")]
    public static void CreateText()
    {
        string path = "Assets/NewTextFile.txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "New text file");
        }
        AssetDatabase.Refresh();
    }
}