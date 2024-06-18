//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 // variable assigned but not used.
using AFWB;
using UnityEditor;
using UnityEngine;

public class FinishWindow : EditorWindow
{
    private string fenceName = "Finished Fence";
    private string modeString;
    private AutoFenceCreator afb = null;
    private Transform parentFolder = null;

    public void Init(AutoFenceCreator inAFB, string inModeString, Transform inParentFolder)
    {
        /*afb = inAFB;
		modeString = inModeString;
		parentFolder = inParentFolder;

		if(modeString == "FinishAndStartNew")
			fenceName = "Finished Fence";
		else if(modeString == "FinishAndDuplicate")
			fenceName = "Finished Duplicated Fence";*/
    }

    private void OnGUI()
    {
        //=================================
        //	 Parent Folder for Finished
        //=================================
        /*GUILayout.BeginVertical("Box");
		EditorGUILayout.LabelField("Optional Parent for Finished Folders");
		GUILayout.Space(10);
		EditorGUILayout.LabelField("If you want your Finished Fence folders to be parented to an object in your hierarchy");
		EditorGUILayout.LabelField("drag the parent object here\n");

		EditorGUI.BeginChangeCheck();
		parentFolder = EditorGUILayout.ObjectField(parentFolder, typeof(Transform), true) as Transform;
		if(EditorGUI.EndChangeCheck() ){
			afb.finishedFoldersParent = parentFolder;
		}

		GUILayout.Space(10);
		GUILayout.EndVertical();
		GUILayout.Space(10);GUILayout.Space(10);

		fenceName = EditorGUILayout.TextField("Fence Name", fenceName);

		if (GUILayout.Button("OK")) {
			Close();
			if(modeString == "FinishAndStartNew")
				afb.FinishAndStartNew(parentFolder, false, fenceName);
			else if(modeString == "FinishAndDuplicate")
				afb.FinishAndDuplicate(parentFolder, fenceName);
			GUIUtility.ExitGUI();
		}*/
    }
}