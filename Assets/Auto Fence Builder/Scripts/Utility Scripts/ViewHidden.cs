using UnityEditor;
using UnityEngine;

public class HiddenObjectsViewer : EditorWindow
{
    [MenuItem("Window/Hidden Objects Viewer")]
    public static void ShowWindow()
    {
        GetWindow<HiddenObjectsViewer>("Hidden Objects Viewer");
    }

    void OnGUI()
    {
        GUILayout.Label("Hidden Objects in Hierarchy", EditorStyles.boldLabel);

        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.hideFlags == HideFlags.HideInHierarchy)
            {
                string objectInfo = obj.name;

                Collider collider = obj.GetComponent<Collider>();
                if (collider != null)
                {
                    objectInfo += $" (Collider: {collider.GetType().Name})";
                }

                GUILayout.Label(objectInfo);

                obj.SetActive(true);
                obj.hideFlags = HideFlags.None;
            }
        }
    }
}

