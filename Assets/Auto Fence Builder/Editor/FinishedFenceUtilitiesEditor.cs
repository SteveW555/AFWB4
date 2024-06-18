//#pragma warning disable 0219
//#pragma warning disable 0414

using AFWB;
using MeshUtils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FinishedFenceUtilities))]
public class FinishedFenceUtilitiesEditor : Editor
{
    public SerializedProperty presetID;
    public FinishedFenceUtilities finishedUtils;
    public GameObject rootFolder = null;

    //private Vector3 finishedPos, livePosition;
    private string editButtonText = "Edit                  [Replaces contents of current Auto Fence Builder session]",
        editButtonTextSure = "Are you sure?  This will Replace the current contents of Auto Fence Builder", currEditButtonText;

    private void OnEnable()
    {
        finishedUtils = (FinishedFenceUtilities)target;
        rootFolder = finishedUtils.finishedFolderRoot.gameObject;

        if (rootFolder.transform.Find("Rails") == null && finishedUtils.transform.Find("Rails") != null)
        {
            rootFolder = finishedUtils.gameObject;
        }

        presetID = serializedObject.FindProperty("presetID");
        currEditButtonText = editButtonText;
    }

    //------------------------------------------
    public void CreateCurrentFromFinished()
    {
        AutoFenceCreator af = GameObject.FindObjectOfType<AutoFenceCreator>();

        rootFolder.SetActive(false);

        //====  Look for a ScriptableClickPoints file
        ScriptableClickPoints scriptableClickPoints = ScriptableClickPoints.LoadScriptableClickPoints(finishedUtils.presetID, af);
        if (scriptableClickPoints == null)
            Debug.Log("Couldn't find the Click Points file for " + finishedUtils.presetID);
        else
            af.CopyLayoutFromScriptableClickPoints(scriptableClickPoints.clickPoints, scriptableClickPoints.gapPoints);

        //====  Look for a Preset file
        string[] presets = AssetDatabase.FindAssets("Preset-" + finishedUtils.presetID);
        string presetPath;
        if (presets.Length == 0 || presets[0] == "")
        {
            Debug.LogWarning("Couldn't find finished preset in CreateCurrentFromFinished(). It should be in FinishedData folder. " +
                             "Fence will be rebuilt with current settings instead. \n");
        }
        else
        {
            presetPath = AssetDatabase.GUIDToAssetPath(presets[0]);
            ScriptablePresetAFWB preset = AssetDatabase.LoadMainAssetAtPath(presetPath) as ScriptablePresetAFWB;
            preset.BuildFromPreset(af);
        }

        af.ResetAllPools();
        af.ForceRebuildFromClickPoints();

        //====  copy click points to handle points  ====
        af.handles.Clear();
        for (int i = 0; i < af.clickPoints.Count; i++)
        {
            af.handles.Add(af.clickPoints[i]);
        }
        //====  Select AFWB in hierarchy  ====
        Selection.activeGameObject = af.gameObject;
    }

    //----------------------------------------------------------------
    public static void CreateFinishedFromCurrent(AutoFenceCreator af, string userFenceName)
    {
        List<Transform> posts = af.postsPool;
        for (int p = 0; p < af.allPostPositions.Count - 1; p++)
        {
            if (posts[p] != null)
                posts[p].gameObject.layer = 0;
        }

        //====  Reposition handle at base of first post
        if (af.allPostPositions.Count > 0)
        {
            Vector3 currPos = af.currentFencesFolder.transform.position;
            Vector3 delta = af.allPostPositions[0] - currPos;
            af.currentFencesFolder.transform.position = af.allPostPositions[0];
            af.postsFolder.transform.position = af.allPostPositions[0] - delta;
            af.railsFolder.transform.position = af.allPostPositions[0] - delta;
            af.subpostsFolder.transform.position = af.allPostPositions[0] - delta;
            af.extrasFolder.transform.position = af.allPostPositions[0] - delta;
        }
        string dateStr = af.GetShortPartialTimeString(true);
        string hourMinSecStr = dateStr.Substring(dateStr.Length - 6, 6);

        //====  Create an Assets folder for this Finished Fence  ====
        string folderName = userFenceName + "-" + hourMinSecStr;
        string finishedAssetsFolderPath = ResourceUtilities.CreateFolderInAutoFenceBuilder(af, path: "FinishedData", folderName: folderName);

        GameObject finishedHierarchyFolder = null;

        //== If Merging
        GameObject mergedParent = null;
        if (af.finishMerged == true)
        {
            if (af.currentFencesFolder != null)
            {
                FenceMeshMerge merger = af.currentFencesFolder.AddComponent<FenceMeshMerge>();
                //====   Make a duplicate of currentFencesFolder with merged meshes
                mergedParent = merger.CreateMergedPrefabs(hourMinSecStr, userFenceName, addBoxColliders: false);
                mergedParent.name = userFenceName;
                finishedHierarchyFolder = mergedParent;
                if (af.finishedFoldersParent != null)
                {
                    mergedParent.transform.parent = af.finishedFoldersParent;
                    //finishedHierarchyFolder = af.finishedFoldersParent.gameObject;
                }
            }
        }
        else if (af.currentFencesFolder != null)
        {
            af.currentFencesFolder.name = userFenceName;
            finishedHierarchyFolder = af.currentFencesFolder;
            if (af.finishedFoldersParent != null)
            {
                af.currentFencesFolder.transform.parent = af.finishedFoldersParent;
                //finishedHierarchyFolder = af.finishedFoldersParent.gameObject;
            }
        }
        af.AddLODGroup(af.currentFencesFolder);

        string finishedName = userFenceName + "-" + hourMinSecStr;

        //====  Save the Click Points  ====
        ScriptableClickPoints clickPoints = ScriptableClickPoints.CreateData(af.currentFencesFolder, af);
        ScriptableClickPoints.SaveScriptableClickPoints(af, clickPoints, finishedAssetsFolderPath, "ClickPoints-" + finishedName);

        //====  Save the procedural rail meshes as real meshes  ===
        if (af.finishMerged == false)
        {
            SaveRailMeshes.SaveProcRailMeshesAsAssets(af, finishedAssetsFolderPath, hourMinSecStr);
        }
        else
        {
            List<GameObject> mergedMeshes = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(mergedParent);
            ResourceUtilities.SaveGOMeshes(mergedMeshes, finishedAssetsFolderPath, "MergedMeshes");
            //====  Destroy the original currentFencesFolder
            af.DestroyCurrentFencesFolder();
            //====  Set currentFencesFolder to be the merged copy
            af.currentFencesFolder = mergedParent;
        }

        if (af.finishDuplicate == true)
            finishedHierarchyFolder.SetActive(false);

        //====  Save Preset  ====
        ScriptablePresetAFWB preset = PresetFiles.SaveFinishedPreset(finishedName, finishedAssetsFolderPath, af);
        //string savedPresetFilename = finishedAssetsFolderPath + "/" + preset.name + "-" + hourMinSecStr;

        //====  Remove the old, and start a new  ====
        af.FinishAndStartNew(af.finishDuplicate, af.finishMerged, hourMinSecStr, userFenceName);

        //====  Add ID to the finished fence  ====
        FinishedFenceUtilities finishedUtils = finishedHierarchyFolder.AddComponent<FinishedFenceUtilities>();
        finishedUtils.presetID = finishedName;

        //====  Create any requested Mesh Colliders for Merged
        if (af.finishMerged == true)
        {
            foreach (Transform child in finishedHierarchyFolder.transform)
            {
                if (af.createMergedPostsCollider == true && child.name.Contains("Posts"))
                    child.gameObject.AddComponent<MeshCollider>();
                if (af.createMergedRailACollider == true && child.name.Contains("Rails A"))
                    child.gameObject.AddComponent<MeshCollider>();
                if (af.createMergedRailBCollider == true && child.name.Contains("Rails B"))
                    child.gameObject.AddComponent<MeshCollider>();
                if (af.createMergedSubpostsCollider == true && child.name.Contains("Subposts"))
                    child.gameObject.AddComponent<MeshCollider>();
                if (af.createMergedExtrasCollider == true && child.name.Contains("Extras"))
                    child.gameObject.AddComponent<MeshCollider>();
            }
        }
    }

    //------------------------------------------
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (finishedUtils.af == null)
            finishedUtils.af = GameObject.FindObjectOfType<AutoFenceCreator>();

        GUILayout.Label("Preset ID = " + presetID.stringValue);

        GUILayout.Space(10); GUILayout.Space(10);

        //======   Edit & Replace   ======
        if (GUILayout.Button(new GUIContent(currEditButtonText, "The current Auto Fence Builder will be overwritten with the settings to edit this fence." +
                                                                "Use this if Auto Fence Builder is empty, or it's OK to discard contents." +
                                                                "Use the option below: 'Edit   [First create Finished fence...]' if you would like to save" +
                                                                "that session first as a Finished fence"), GUILayout.Width(500)))
        {
            if (currEditButtonText == editButtonText)
            {
                currEditButtonText = editButtonTextSure;
            }
            else if (currEditButtonText == editButtonTextSure)
            {
                if (finishedUtils != null)
                {
                    CreateCurrentFromFinished();
                    Selection.activeGameObject = finishedUtils.af.transform.gameObject;
                    rootFolder.name += " [Pre Edit]";
                    rootFolder.SetActive(false);
                }

                currEditButtonText = editButtonText;
            }
        }

        GUILayout.Space(10);
        //======   Edit & Finish Current   ======
        if (currEditButtonText == editButtonText)
        {
            if (GUILayout.Button(new GUIContent("Edit        [Will first create Finished fence from the current Auto Fence Builder session]",
                "This will save your current Auto Fence Builder as a Finished fence, and replace the settings in order to edit this fence."), GUILayout.Width(500)))
            {
                AutoFenceCreator af = GameObject.FindObjectOfType<AutoFenceCreator>();
                if (af == null)
                    return;
                //finishedPos = finishedUtils.af.currentFencesFolder.transform.position;
                string defaultFenceName = "[AF] " + af.scrPresetSaveName;
                CreateFinishedFromCurrent(af, defaultFenceName);

                af.ClearAllFences();
                if (finishedUtils != null)
                {
                    CreateCurrentFromFinished();
                    Selection.activeGameObject = af.transform.gameObject;
                    rootFolder.name += " [Pre Edit]";
                    rootFolder.SetActive(false);
                }
                currEditButtonText = editButtonText;
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Editing a Finished fence will place its settings in to the current Auto Fence Builder session.");
        GUILayout.Label("If the current session is not empty, you can Finish it manually, or choose the 2nd Edit button which will Finish it.");
        GUILayout.Label("Alternatively, you can choose to overwrite it.");
        GUILayout.Space(10);
        GUILayout.Label("This Finished version will be de-activated, and you may safely delete it afterwards.");
        GUILayout.Space(10);
        GUILayout.Label("Note: This will work fully for any Finished fence created in v3.2+");
        GUILayout.Label("Only layout & build positions - not design - are available in earlier versions");
        GUILayout.Space(10);
        //======   Cancel  ======
        GUILayout.Space(10);
        if (currEditButtonText == editButtonTextSure)
        {
            if (GUILayout.Button("Cancel", GUILayout.Width(110)))
            {
                currEditButtonText = editButtonText;
            }
        }
    }
}