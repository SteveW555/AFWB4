using UnityEditor;
using UnityEngine;

public class DeletePresetWindow : EditorWindow
{
    private AutoFenceEditor ed = null;
    private Color darkCyan = new Color(0, .5f, .75f);
    private string presetName;
    private ScriptablePresetAFWB preset = null;
    private bool deleted = false;

    public void Init(AutoFenceEditor inEditor, ScriptablePresetAFWB preset)
    {
        ed = inEditor;
        if (preset == null)
            return;
        presetName = preset.name;
        this.preset = preset;
    }

    private void OnGUI()
    {
        if (preset == null)
        {
            Debug.Log("Skipping deletion, preset was null");
            Close();
            GUIUtility.ExitGUI();
        }

        GUIStyle headingStyle = new GUIStyle(EditorStyles.label);
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.fontSize = 13;
        headingStyle.normal.textColor = darkCyan;

        GUILayout.BeginVertical("Box");

        EditorGUI.LabelField(new Rect(130, 20, 225, 16), "Delete Preset", headingStyle);
        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);

        if (GUI.Button(new Rect(28, 100, 380, 16), "Delete Preset:  " + presetName))
        {
            deleted = false;
            int index = ed.FindPresetIndexByName(presetName);
            if (index != -1)
            {
                ed.mainPresetList.RemoveAt(index);
                string fileName = presetName;
                string categoryName = preset.categoryName;
                string path = "Assets/Auto Fence Builder/AFWB_Presets/" + categoryName + "/" + fileName + ".asset";
                deleted = AssetDatabase.DeleteAsset(path);
                if (deleted)
                    Debug.Log(fileName + " was deleted.");

                ed.presetsEd.LoadAllScriptablePresets(ed.af.allowContentFreeUse);
                ed.presetsEd.SetupPreset(ed.af.currPresetIndex);
            }
            Close();
            GUIUtility.ExitGUI();
        }
        if (GUI.Button(new Rect(28, 145, 380, 16), "Cancel"))
        {
            Close();
            GUIUtility.ExitGUI();
        }

        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.EndVertical();
        GUILayout.Space(10); GUILayout.Space(10);
    }
}