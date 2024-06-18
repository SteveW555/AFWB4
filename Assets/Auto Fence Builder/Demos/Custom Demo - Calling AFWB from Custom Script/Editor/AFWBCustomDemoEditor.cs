using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AFWBCustomDemo))]
internal class AFWBCustomDemoEditor : Editor
{
    public AFWBCustomDemo demoScript;

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("(If error - click on Auto Fence Builder first to ensure it's loaded and initialized.)");
        GUILayout.Space(10);
        if (GUILayout.Button("Test"))
        {
            Debug.Log("Testing " + target.name + "\n");
            demoScript = (AFWBCustomDemo)target;
            demoScript.TestDemo();
        }
        GUILayout.Space(10);
    }
}