using AFWB;
using MeshUtils;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TCT.PrintUtils;
using UnityEditor;
using UnityEngine;

public partial class AutoFenceEditor
{

    public void ManageGlobals()
    {


        if(isDark)
        {
            GUILayout.BeginVertical(boxUIDarkYellowStyle);
            GUILayout.BeginVertical();
        }
        else
        {
            GUILayout.BeginVertical();
            GUILayout.BeginVertical("box");
        }

        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("", GUILayout.Width(8));
            showGlobals.boolValue = EditorGUILayout.Foldout(showGlobals.boolValue, "");
            EditorGUILayout.LabelField("", GUILayout.Width(125));
            EditorGUILayout.LabelField("Globals", cyanBoldStyle, GUILayout.Width(60));
            GUILayout.Space(10);
            //AFWB_HelpText.ShowGlobalsHelp(horizontalScope, cyanBoldStyle, 30);
            GUILayout.Space(300);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        GUILayout.Space(5);

        if (showGlobals.boolValue)
        {
            if (af.globalWidth != 1 || af.globalHeight != 1)
            {
                GUI.backgroundColor = new Color(1.0f, 0.93f, 0.9f);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(200);

            GUIStyle smallButt11Style = new GUIStyle(GUI.skin.button);
            smallButt11Style.fontSize = 11;
            smallButt11Style.normal.textColor = new Color((0.99f), 0.88f, 0.77f);


            GUI.backgroundColor = new Color(1.0f, 0.96f, 0.9f);
            if (GUILayout.Button(new GUIContent("Nothing's Working!", "Unfortunately there's no way for AFWB to detect if you have Gizmos disabled" +
                ", and when disabled, there's also no way for AFWB to present Info or a solution - or anything else - in the Scene View."), smallButt11Style, GUILayout.Width(200), GUILayout.Height(16)))
            {
                showNothingWorking = !showNothingWorking;
            }
            GUILayout.EndHorizontal();
            if (showNothingWorking)
            {
                EditorGUILayout.LabelField(new GUIContent(" - Make sure to have Gizmos Enabled - " +
                    "top right of Scene View, looks like a wireframe sphere.", ""), GUILayout.Width(550));
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(" - If you're building on objects other than Terrain, " +
                    "make sure they have Colliders.", ""), GUILayout.Width(530));

                if (GUILayout.Button("Close", smallButt11Style, GUILayout.Width(52), GUILayout.Height(16)))
                {
                    showNothingWorking = !showNothingWorking;
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.LabelField(new GUIContent(" - Use 'Clean & Rebuild' - " +
                    "will indicate if there are hidden Colliders or other issues.", ""), GUILayout.Width(550));
                EditorGUILayout.LabelField(new GUIContent(" - Deselect and Reselect AFWB in the Hierarchy, " +
                    "this will enforce further checks and log any issues.", ""), GUILayout.Width(560));
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(5);

            //============================
            //      Toolbar
            //============================

            af.currGlobalsToolbarRow1 = GUILayout.Toolbar(af.currGlobalsToolbarRow1, globalsToolbarRow1_GUIContent);
            if (af.currGlobalsToolbarRow1 >= 0)
                af.currGlobalsToolbarRow2 = -1;

            af.currGlobalsToolbarRow2 = GUILayout.Toolbar(af.currGlobalsToolbarRow2, globalsToolbarRow2_GUIContent);
            if (af.currGlobalsToolbarRow2 >= 0)
                af.currGlobalsToolbarRow1 = -1;




            GUILayout.Space(10);

            //====================================
            //          Global Scale
            //====================================
            if (af.currGlobalsToolbarRow1 == 0)
            {
                GUILayout.Space(10);

                EditorGUILayout.LabelField(new GUIContent("Global Scale", "Scales all components of the Fence/Wall, including the Inter-Post spacing." +
                    "(You can re-adjust the Inter-Post spacing if needed after scaling)"), cyanBoldStyle, GUILayout.Width(80));

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(" Re-Span", "Adjusts the inter-post spacing accordingly to suit this scale"), GUILayout.Width(55));
                EditorGUI.BeginChangeCheck();
                respanAfterGlobalScale = EditorGUILayout.Toggle(respanAfterGlobalScale, GUILayout.Width(30));
                //EditorGUILayout.LabelField("", GUILayout.Width(5));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.LabelField(new GUIContent("Link H & W", "Scales Both Height & Width Equally"), GUILayout.Width(64));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("keepGlobalScaleHeightWidthLinked"), new GUIContent(""), GUILayout.Width(12));
                EditorGUILayout.LabelField("", GUILayout.Width(14));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                //     Global Height
                //======================
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Height:", GUILayout.Width(43));
                EditorGUILayout.PropertyField(fenceHeight, new GUIContent(""), GUILayout.Width(130));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (af.keepGlobalScaleHeightWidthLinked)
                        af.globalWidth = af.globalHeight;

                    if (respanAfterGlobalScale)
                        af.interPostDist = af.baseInterPostDistance * af.globalHeight;
                    af.globalScale = new Vector3(af.globalWidth, af.globalHeight, af.globalWidth);
                    af.ForceRebuildFromClickPoints();
                }

                //     Global Width
                //======================
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("   Width:", GUILayout.Width(48));
                EditorGUILayout.PropertyField(fenceWidth, new GUIContent(""), GUILayout.Width(130));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (af.keepGlobalScaleHeightWidthLinked)
                    {
                        af.globalHeight = af.globalWidth;
                        if (respanAfterGlobalScale)
                            af.interPostDist = af.baseInterPostDistance * af.globalHeight;
                    }

                    af.globalScale = new Vector3(af.globalWidth, af.globalHeight, af.globalWidth);
                    af.ForceRebuildFromClickPoints();
                }
                if (GUILayout.Button(new GUIContent("R", "Reset Global Scaling to (1,1,1)"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.baseInterPostDistance = af.interPostDist / af.globalHeight;
                    af.globalWidth = af.globalHeight = 1;
                    af.interPostDist = af.baseInterPostDistance * af.globalHeight;
                    af.globalScale = new Vector3(1, 1, 1);
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.EndHorizontal();


                //== Global Lift
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(globalLiftLower); //this should be 0.0 unless you're layering a fence above another one
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.Space(10);
            }

            //============================
            //      Smoothing 
            //============================
            if (af.currGlobalsToolbarRow1 == 1)
            {
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Smooth", cyanBoldStyle, GUILayout.Width(99));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("smooth"), new GUIContent(""));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(roundingDistance);
                if (GUILayout.Button(new GUIContent("R", "Reset Smooth distance to 2"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.roundingDistance = roundingDistance.intValue = 2;
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tension"), new GUIContent("Corner Tightness"));
                if (GUILayout.Button(new GUIContent("R", "Reset to 0"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.removeIfLessThanAngle = serializedObject.FindProperty("tension").floatValue = 0;
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Use these to reduce the number of Smoothing postsPool for performance:", infoStyle);
                GUILayout.Label("(It helps to temporarily disable 'Interpolate' to see the effect of these)", infoStyle);


                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("removeIfLessThanAngle"), new GUIContent("Remove Where Straight", "Removes unnecessary curved " +
                    "sections where the angle between them is less than this"));
                if (GUILayout.Button(new GUIContent("R", "Reset Remove Where Straight to 7 degrees"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.removeIfLessThanAngle = serializedObject.FindProperty("removeIfLessThanAngle").floatValue = 7;
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stripTooClose"), new GUIContent("Remove Vey Close Posts", "Removes Sections that are closer than this"));
                if (GUILayout.Button(new GUIContent("R", "Reset to 2"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.stripTooClose = serializedObject.FindProperty("stripTooClose").floatValue = 2;
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.Space(10);
            }

            EditorGUI.BeginChangeCheck();
            if (af.currGlobalsToolbarRow1 == 2)
            {
                //============================
                //      Close Loop 
                //============================
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(closeLoopProp);
                if (af.closeLoop != oldCloseLoop)
                {
                    Undo.RecordObject(af, "Change Loop Mode");
                    af.ManageCloseLoop(af.closeLoop);
                    SceneView.RepaintAll();
                }
                oldCloseLoop = af.closeLoop;
                GUILayout.Space(10);
                GUILayout.Space(10);

                //============================
                //      Snapping
                //============================
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(snapMainPostsProp);
                EditorGUILayout.PropertyField(snapSizeProp);
                GUILayout.EndHorizontal();

                //============================
                //      Reverse
                //============================
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Reverse Fence", "Reverses the order of your click-points. This will also make all objects face 180 the other way."), GUILayout.Width(110)))
                {
                    ReverseClickPoints();
                }
                GUILayout.Space(10);
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (snapSizeProp.floatValue < 0.01f)
                    snapSizeProp.floatValue = 0.01f;

                serializedObject.ApplyModifiedProperties();
                af.ForceRebuildFromClickPoints();
            }

            EditorGUI.BeginChangeCheck();
            if (af.currGlobalsToolbarRow1 == 3)
            {
                //============================
                //      Layers
                //============================

                //Get List of all Unity layers
                ArrayList layerNames = new ArrayList();
                layerNames.Add("None");
                for (int i = 0; i <= 31; i++) //user defined layers start with sourceLayerList 8 and unity supports 31 layers
                {
                    string layerN = LayerMask.LayerToName(i); //get the name of the sourceLayerList
                    if (layerN.Length > 0) //only add the sourceLayerList if it has been named (comment this line out if you want every sourceLayerList)
                    {
                        layerNames.Add(layerN);
                        //Debug.Log(layerN);
                    }
                }

                // add checkbox for obeyUnityIgnoreRaycastLayer
                /*GUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("obeyUnityIgnoreRaycastLayer"), new GUIContent("Obey Unity's 'Ignore Raycast' Layer", 
                    "Posts or Click Points will not be laced on objects that have the 'Ignore Raycast' sourceLayerList," +
                    "They will instead be placed on the first valid collider below that is not set to be ignored."));*/

                // Create dropdown menu for layers
                string[] layerNamesArray = (string[])layerNames.ToArray(typeof(string));
                af.ignoreRaycastsLayerIndex = EditorGUILayout.Popup(new GUIContent("Ignore Colliders Layer",
                "When placing Click Points (fence nodes), this sourceLayerList will be ignored and AutoFence will look for the nextPos collider below for placement \n" +
                "This is in addition to the standard Unity 'Ignore Raycast' sourceLayerList. " +
                "\nDefault = None"), af.ignoreRaycastsLayerIndex, layerNamesArray, GUILayout.Width(400));
                //if (layerIndex != af.ignoreRaycastsLayerIndex)
                {
                    af.ignoreRaycastsLayerName = layerNamesArray[af.ignoreRaycastsLayerIndex];
                }


                GUILayout.Space(10);


                GUILayout.Space(10);
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (snapSizeProp.floatValue < 0.01f)
                    snapSizeProp.floatValue = 0.01f;

                serializedObject.ApplyModifiedProperties();
                af.CheckClickPointsForIgnoreLayers();
                af.CheckAllPostPositionsForIgnoreLayers();
                af.ForceRebuildFromClickPoints();
            }

            //======================
            //        ROW 2
            //======================
            //EditorGUI.BeginChangeCheck();
            //==========================================
            //              Cloning & Layout
            //==========================================  
            if (af.currGlobalsToolbarRow2 == 0)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Cloning Options: ", cyanBoldStyle);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Layout", GUILayout.Width(100)) && af.fenceToCopyFrom != null)
                {
                    af.CopyLayoutFromOtherFence();
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fenceToCopyFrom"), new GUIContent("Drag finished fence here:"));

                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                /*if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    string sourceName = af.fenceToCopyFrom.name;

                    Transform mainRailsFolder = af.fenceToCopyFrom.transform.Find("Rails");


                    int railClonePrefabIndex = -1;
                    for (int railLayerIndex = kRailALayerInt; railLayerIndex < kRailALayerInt; railLayerIndex++) //LayerSet rails A & B
                    {
                        LayerSet sourceLayerList = (LayerSet)railLayerIndex;
                        Transform firstRailsFolder = mainRailsFolder.transform.Find("RailsAGroupedFolder0");
                        if (railLayerIndex == kRailBLayerInt)
                            firstRailsFolder = mainRailsFolder.transform.Find("RailsBGroupedFolder0");

                        if (firstRailsFolder != null)
                        {
                            Transform firstChild = firstRailsFolder.GetChild(0);
                            string firstChildName = firstChild.name;

                            int split = firstChildName.IndexOf("_Panel_Rail");
                            if (split == -1)
                                split = firstChildName.IndexOf("_Rail");

                            string shortName = firstChildName.Substring(0, split);

                            Debug.Log("shortName = " + shortName + "\n");
                            for (int i = 0; i < af.railMenuNames.Count; i++)
                            {
                                if (af.railMenuNames[i] == null)
                                    continue;
                                string name = af.railMenuNames[i];
                                if (name.Contains(shortName))
                                {
                                    Debug.Log("shortName = " + shortName + "\n");
                                    railClonePrefabIndex = i;
                                    break;
                                }
                            }
                            if (railClonePrefabIndex != -1)
                            {
                                af.SetRailPrefab(af.currentRail_PrefabIndex[railLayerIndex], sourceLayerList, false, false);
                                int index = af.ConvertRailMenuIndexToPrefabIndex(railClonePrefabIndex);
                                af.currentRail_PrefabIndex[railLayerIndex] = index;
                                af.ResetPoolForLayer(sourceLayerList);
                                af.SetRailPrefab(index, sourceLayerList, false, false);
                                af.railSourceVariants[kRailALayerInt][0].go = af.railPrefabs[af.currentRail_PrefabIndex[0]];
                                af.fenceToCopyFrom.SetActive(false);
                                af.CopyLayoutFromOtherFence(false);
                                af.ForceRebuildFromClickPoints();
                                Debug.Log("Rebuild from " + sourceName + "\n");
                            }

                        }
                    }*/
            }

            //===========================================
            //                Combining & Batching
            //===========================================
            if (af.currGlobalsToolbarRow2 == 1)
            {
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Batching & Combining: Performance Options: ", cyanBoldStyle);
                showBatchingHelp = EditorGUILayout.Foldout(showBatchingHelp, "Show Batching Help");
                if (showBatchingHelp)
                {
                    italicStyle.fontStyle = FontStyle.Italic; italicStyle.normal.textColor = new Color(0.55f, 0.45f, 0.25f);
                    GUILayout.Label("• If using Unity Static Batching, select 'Static Batching'.", italicStyle);
                    GUILayout.Label("   All parts will be marked as 'Static'.", italicStyle);
                    GUILayout.Label("  (You MUST ensure Unity's Static Batching is on [Edit->Project Settings->Player]).", italicStyle);
                    GUILayout.Label("•If not using Unity's Static Batching,", italicStyle);
                    GUILayout.Label("  select 'Add Combine Scripts' to combnine groups of meshes at runtime", italicStyle);
                    GUILayout.Label("•'None' lacks the performance benefits of batching/combining,", italicStyle);
                    GUILayout.Label("  but enables moving/deleting single parts at runtime", italicStyle);
                    GUILayout.Label("  (avoid this on long complex fences as the cost could affect frame rate.", italicStyle);
                }
                string[] batchingMenuNames = { "Static Batching", "Add Combine Scripts", "None" };
                int[] batchingMenuNums = { 0, 1, 2 };
                af.batchingMode = EditorGUILayout.IntPopup("Batching Mode", af.batchingMode, batchingMenuNames, batchingMenuNums);

                if (af.batchingMode == 0)
                {
                    af.addCombineScripts = false;
                    af.usingStaticBatching = true;
                }
                else if (af.batchingMode == 1)
                {
                    af.addCombineScripts = true;
                    af.usingStaticBatching = false;
                }
                else
                {
                    af.addCombineScripts = false;
                    af.usingStaticBatching = false;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    af.ForceRebuildFromClickPoints();
                }
            }
            //===============================================
            //  Refreshing & Unloading Prefabs and Resources
            //===============================================
            if (af.currGlobalsToolbarRow2 == 2)
            {

                if (GUILayout.Button("Refresh Prefabs & Presets", GUILayout.Width(170)))
                {
                    ReloadPrefabs();
                    ReloadPresets();
                    mainPresetListStaticCopy = new List<ScriptablePresetAFWB>(mainPresetList);
                    //mainPresetListStaticCopy.AddRange(amainPresetList);
                }
                italicStyle.fontStyle = FontStyle.Italic; italicStyle.normal.textColor = new Color(0.75f, 0.55f, 0.35f);
                GUILayout.Label("'Refresh Prefabs' will reload all prefabs, including your custom ones. Use this if your custom prefabs " +
                    "\n are not appearing in the preset parts dropdown menus, or you've manually added new ones", italicStyle);


                GUILayout.Space(10);
                if (GUILayout.Button("Unload Unused Assets From Game [Optimize Build Size]", GUILayout.Width(400)))
                {
                    UnloadUnusedAssets();
                }
                GUILayout.Label("'Unload Unused Assets' will remove all unused models and textures from Auto Fence & Wall Builder.", italicStyle);
                GUILayout.Label("It's important to do this before performing a final Unity 'Build' to make the build product as small as possible.", italicStyle);
                GUILayout.Label("This will not remove the assets from your Project Folder. Use the button below to purge your Project Folder", italicStyle);

                GUILayout.Space(10);
                if (GUILayout.Button("Remove Unused Assets From Project Folder [Reduce Project Size]...", GUILayout.Width(400)))
                {
                    removingAssets = true;
                    RemoveUnusedAssetsFromProject();
                }
                if (removingAssets == true)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.LabelField(new GUIContent("[Any Assets used in the hierarchy or current fence will not (and can not) be deleted.]"), GUILayout.Width(500));
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Delete:  ", GUILayout.Width(70));


                    EditorGUILayout.LabelField(new GUIContent(" Rails", "Delete Unused Rail Prefabs, Meshes Materials & Textures"), GUILayout.Width(50));
                    af.deleteRailAssets = EditorGUILayout.Toggle(serializedObject.FindProperty("deleteRailAssets").boolValue, GUILayout.Width(30));

                    EditorGUILayout.LabelField(new GUIContent(" Posts", "Delete Unused Post Prefabs, Meshes Materials & Textures"), GUILayout.Width(50));
                    af.deletePostAssets = EditorGUILayout.Toggle(serializedObject.FindProperty("deletePostAssets").boolValue, GUILayout.Width(30));

                    EditorGUILayout.LabelField(new GUIContent(" Extras", "Delete Unused Extra Prefabs, Meshes Materials & Textures"), GUILayout.Width(50));
                    af.deleteExtraAssets = EditorGUILayout.Toggle(serializedObject.FindProperty("deleteExtraAssets").boolValue, GUILayout.Width(30));


                    EditorGUILayout.LabelField(new GUIContent(" Keep Favorites", "Any assets used in any of the Favorites presetsEd will be retained.\n"), GUILayout.Width(100));
                    af.keepFavorites = EditorGUILayout.Toggle(serializedObject.FindProperty("keepFavorites").boolValue, GUILayout.Width(25));

                    GUILayout.FlexibleSpace();
                    //GUILayout.Space(80);

                    GUILayout.EndHorizontal();


                    if (GUILayout.Button("Cancel", GUILayout.Width(100)))
                    {
                        removingAssets = false;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }


                GUILayout.Label("'Remove Unused Assets' will remove all unused models and textures from your Auto Fence session \n" +
                    "and the Project Folder.\nAny assets being used by your current Auto Fence session will be retained, as well as any used in the \n" +
                    "Hierarchy. This is not undoable. If you confirm in the nextPos step by mistake you will need to re-import \n" +
                    "Auto Fence & Wall Builder.\n" +
                    "Pressing the button will open a panel where you can confirm, cancel,  or choose what to remove", italicStyle);

                GUILayout.Space(10);

                //========================
                //   Save Meshes
                //========================
                if (GUILayout.Button(new GUIContent("Save Meshes"), GUILayout.Width(170)) && af.clickPoints.Count > 0)
                {
                    string dateStr = af.GetShortPartialTimeString(true);
                    string hourMinSecStr = dateStr.Substring(dateStr.Length - 6, 6);
                    string folderName = "SavedMeshes" + "_" + dateStr;
                    string folderPth = ResourceUtilities.CreateFolderInAutoFenceBuilder(af, "FinishedData", folderName);
                    string success = SaveRailMeshes.SaveProcRailMeshesAsAssets(af, folderPth, hourMinSecStr);
                    if (success == "")
                    {
                        Debug.LogWarning(" SaveProcRailMeshesAsAssets() Failed \n");
                    }
                }
                GUILayout.Label("The only occasion this is needed is if you're working with a 3rd-party asset that needs" +
                     " constant access to saved mesh assets, otherwise you never need to use it. (As the rails in AFWB are created procedurally in realtime," +
                      " they normally only become saved mesh assets after performing a 'Finish'. Saved in FinishedFenceAssets)", italicStyle);

                GUILayout.Space(10);

                int totalTrianglesCount = af.railsATotalTriCount + af.railsBTotalTriCount + af.postsTotalTriCount + af.extrasTotalTriCount + af.subPostsTotalTriCount;
                int numRailA = af.railABuiltCount, numRailB = af.railABuiltCount;
                int numPosts = af.postsBuiltCount, numExtras = af.ex.extrasBuiltCount, numSubs = af.subpostsBuiltCount;
                int railATriCount = 0, railBTriCount = 0, postTriCount = 0, extraTriCount = 0, subTriCount = 0, avgTrisPerSection = 0;

                if (af.usePostsLayer == false)
                    numPosts = 0;

                if (af.railABuiltCount > 0 && af.railsATotalTriCount > 0 && numRailA > 0)
                    railATriCount = af.railsATotalTriCount / numRailA;
                if (af.railBBuiltCount > 0 && af.railsBTotalTriCount > 0 && numRailB > 0)
                    railBTriCount = af.railsBTotalTriCount / numRailB;
                if (af.postsBuiltCount > 0 && af.usePostsLayer == true && af.postsTotalTriCount > 0)
                    postTriCount = af.postsTotalTriCount / numPosts;
                if (af.ex.extrasBuiltCount > 0 && af.extrasTotalTriCount > 0)
                    extraTriCount = af.extrasTotalTriCount / numExtras;
                if (af.subpostsBuiltCount > 0 && af.subPostsTotalTriCount > 0)
                    subTriCount = af.subPostsTotalTriCount / numSubs;


                int numSects = (af.allPostPositions.Count - 1);
                if (numSects > 0)
                {
                    avgTrisPerSection = totalTrianglesCount / numSects;
                }
                else
                {
                    numSects = 0;
                }


                GUILayout.Label("Number of Rails A = " + (af.railABuiltCount) + "    Rails B = " + (af.railBBuiltCount) +
                 "    Posts = " + af.allPostPositions.Count + "    SubPosts = " + af.subpostsBuiltCount + "    Extras = " + af.ex.extrasBuiltCount);
                //Handles.Label(GetWorldPos(0, lineHeight, ref wPos, cam), "Pool Sizes:  Posts   " + af.postsPool.Count + "      Rails " + af.railsAPool.Count + "      Extras " + af.extrasPool.Count);
                //GUILayout.Label("Num variations  A: " + af.nonNullRailSourceVariants[0].Count + "     B: " + af.nonNullRailSourceVariants[1].Count);
                GUILayout.Label("Total Triangle Count = " + totalTrianglesCount + " :" +
                                                "     Rails-A: " + af.railsATotalTriCount + " (" + numRailA + " x " + railATriCount + ")" +
                                                "     Rails-B: " + af.railsBTotalTriCount + " (" + numRailB + " x " + railBTriCount + ")" +
                                                ",    Posts: " + af.postsTotalTriCount + " (" + numPosts + " x " + postTriCount + ")\n" +
                                                "Extras: " + af.extrasTotalTriCount + " (" + numExtras + " x " + extraTriCount + ")" +
                                                ",    SubPosts: " + af.subPostsTotalTriCount + " (" + numSubs + " x " + subTriCount + ")" +
                                                "");
                GUILayout.Label("Average Triangles Per Section =  " + avgTrisPerSection + "  (" + numSects + " sections x " + avgTrisPerSection + " = " + totalTrianglesCount + ")");
                GUILayout.Space(4);
            }
            //===============================================
            //          Settings
            //===============================================
            if (af.currGlobalsToolbarRow2 == 3)
            {
                Transform parent = af.finishedFoldersParent;
                bool isDirty = false;

                GUIStyle headingStyle = new GUIStyle(EditorStyles.label);
                headingStyle.fontStyle = FontStyle.Bold;
                headingStyle.normal.textColor = darkCyan;

                GUILayout.Space(10);


                //=================================
                //	 Parent Folder for Finished
                //=================================
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Optional Parent for Finished Folders", headingStyle);
                GUILayout.Space(10);
                EditorGUILayout.LabelField(
                    "If you want your Finished Fence folders to be parented to an object in your hierarchy", infoStyle);
                EditorGUILayout.LabelField("drag the parent object here\n", infoStyle);

                EditorGUI.BeginChangeCheck();
                parent = EditorGUILayout.ObjectField(parent, typeof(Transform), true) as Transform;
                if (EditorGUI.EndChangeCheck())
                {
                    af.finishedFoldersParent = parent;
                }

                GUILayout.Space(10);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.Space(10);


                //=================================
                //			LOD
                //=================================
                GUILayout.Space(10);
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("LOD", headingStyle);
                EditorGUILayout.LabelField("With this option selected, a basic LOD group with cutoff distance set to about 8%.", infoStyle);
                EditorGUILayout.LabelField("This will perform simple culling and provide the LOD group ready to add other levels that you prepare", infoStyle);
                af.addLODGroup = EditorGUILayout.Toggle("Add LOD Group when Finishing fence", af.addLODGroup);

                EditorGUILayout.EndVertical();
                //=================================
                //			User Object Import Scaling
                //=================================
                GUILayout.Space(10);
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Custom Object Scaling", headingStyle);
                EditorGUILayout.LabelField("With this option selected, the Size setting will be changed to try to match the custom object Size", infoStyle);
                af.autoScaleImports = EditorGUILayout.Toggle("Rescale Custom Objects", af.autoScaleImports);

                EditorGUILayout.EndVertical();
                //=================================
                //			Gaps
                //=================================
                GUILayout.Space(10);
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Gaps", headingStyle);
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Control-Right-Click to create gaps in the fence.", infoStyle);

                GUILayout.Space(10);
                af.allowGaps = EditorGUILayout.Toggle("Allow Gaps", af.allowGaps);
                af.showDebugGapLine = EditorGUILayout.Toggle("Show Gap Lines", af.showDebugGapLine);
                GUILayout.Space(10);
                GUILayout.EndVertical();
                //=================================
                //			Layer number
                //=================================
                GUILayout.Space(10);
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Layer Number", headingStyle);
                EditorGUI.BeginChangeCheck();
                af.ignoreControlNodesLayerNum = EditorGUILayout.IntField("ignoreControlsLayerNum", af.ignoreControlNodesLayerNum);
                if (EditorGUI.EndChangeCheck())
                    isDirty = true;
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();


                if (isDirty)
                {
                    List<Transform> posts = af.postsPool;
                    for (int p = 0; p < af.allPostPositions.Count - 1; p++)
                    {
                        if (posts[p] != null)
                            posts[p].gameObject.layer = 0;
                    }
                    af.ForceRebuildFromClickPoints();
                }
                if (af.railAColliderMode < ColliderType.originalCollider || af.postColliderMode < ColliderType.originalCollider || af.extraColliderMode < ColliderType.originalCollider)
                {
                    EditorGUILayout.LabelField("Colliders are being used. It may improve ed performance to leave them  off until ready to Finish the Fence.\n");
                }

                EditorGUILayout.EndVertical();
                isDirty = false;

            }
            if (af.currGlobalsToolbarRow2 == 4)
            {
                Transform parent = af.finishedFoldersParent;
                bool isDirty = false;

                GUIStyle headingStyle = new GUIStyle(EditorStyles.label);
                headingStyle.fontStyle = FontStyle.Bold;
                headingStyle.normal.textColor = darkCyan;

                GUILayout.Space(10);

                //=================================
                //			Colliders
                //=================================

                GUILayout.BeginVertical("Box");

                //EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Colliders", headingStyle);
                GUILayout.Space(10);
                bool w = EditorStyles.label.wordWrap;
                EditorStyles.label.wordWrap = true;
                EditorGUILayout.LabelField(
                    "By default, a single BoxCollider will be placed on the rails/walls, set to the height of the postsPool. " +
                    "For most purposes this gives the expected collision on the fence.\n" +
                    "It's not usually necessary to have colliders on the postsPool.\n" +
                    "You can change this if, for example, the postsPool & rails are radically different thicknesses " +
                    "or if you have postsPool but no rails." +
                    "\nFor best performance, use Single or None where possible. Using 'Keep Original' on " +
                    "custom objects which have MeshColliders, or multiple small colliders is not recommended.", GUILayout.Width(580), GUILayout.Height(100));

                EditorStyles.label.wordWrap = w;

                GUILayout.Space(20);

                //=========== Defaults ============
                if (GUILayout.Button("Set Defaults", GUILayout.Width(100)))
                {
                    af.postColliderMode = ColliderType.originalCollider;
                    af.railAColliderMode = ColliderType.originalCollider;
                    af.extraColliderMode = ColliderType.originalCollider;
                    af.railABoxColliderHeightScale = 1.0f;
                    af.railABoxColliderHeightOffset = 0.0f;
                    isDirty = true;
                }

                GUILayout.Space(25);


                //Collider Modes: 0 = single box, 1 = keep original (user), 2 = no colliders
                string[] subModeNames =
                {
                    "Use Single Box Collider", "Keep Original Colliders (Custom Objects Only)", "No Colliders",
                    "Mesh Colliders"
                };
                int[] subModeNums = { 0, 1, 2, 3 };

                int collPopupWidth = 117, scaleOffsetLabelWidth = 55, valueBoxWidth = 30, horizSpacing = 16, customFieldWidth = 140;
                bool addCustomField = false;
                if (af.railAColliderMode == ColliderType.customCollider || af.railBColliderMode == ColliderType.customCollider ||
                    af.postColliderMode == ColliderType.customCollider || af.extraColliderMode == ColliderType.customCollider || af.subpostColliderMode == ColliderType.customCollider)
                {
                    addCustomField = true;
                    horizSpacing = 10;
                    scaleOffsetLabelWidth = 50;
                }
                //=================================
                //			Rail A
                //=================================

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Rail A Colliders: ", GUILayout.Width(100));
                railAColliderModeProp.intValue = (int)(ColliderType)EditorGUILayout.EnumPopup("", (ColliderType)railAColliderModeProp.intValue, GUILayout.Width(collPopupWidth));

                GUILayout.Space(horizSpacing);

                //======   Custom Mesh   ======
                if (af.railAColliderMode == ColliderType.customCollider)
                {
                    EditorGUILayout.PropertyField(railACustomColliderMeshProp, new GUIContent(""), GUILayout.Width(customFieldWidth));
                    GUILayout.Space(5);
                }
                else if (addCustomField)
                    GUILayout.Space(customFieldWidth + 5);


                EditorGUILayout.LabelField("Y Scale: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.railABoxColliderHeightScale = EditorGUILayout.FloatField("", af.railABoxColliderHeightScale, GUILayout.Width(valueBoxWidth));
                GUILayout.Space(horizSpacing);
                EditorGUILayout.LabelField("Y Offset: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.railABoxColliderHeightOffset = EditorGUILayout.FloatField("", af.railABoxColliderHeightOffset, GUILayout.Width(valueBoxWidth));

                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    isDirty = true;
                }
                //=================================
                //			Rail B
                //=================================

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Rail B Colliders: ", GUILayout.Width(100));
                railBColliderModeProp.intValue = (int)(ColliderType)EditorGUILayout.EnumPopup("", (ColliderType)railBColliderModeProp.intValue, GUILayout.Width(collPopupWidth));

                GUILayout.Space(horizSpacing);

                //======   Custom Mesh   ======
                if (af.railBColliderMode == ColliderType.customCollider)
                {
                    EditorGUILayout.PropertyField(railBCustomColliderMeshProp, new GUIContent(""), GUILayout.Width(customFieldWidth));
                    GUILayout.Space(5);
                }
                else if (addCustomField)
                    GUILayout.Space(customFieldWidth + 5);

                EditorGUILayout.LabelField("Y Scale: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.railBBoxColliderHeightScale = EditorGUILayout.FloatField("", af.railBBoxColliderHeightScale, GUILayout.Width(valueBoxWidth));
                GUILayout.Space(horizSpacing);
                EditorGUILayout.LabelField("Y Offset: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.railBBoxColliderHeightOffset = EditorGUILayout.FloatField("", af.railBBoxColliderHeightOffset, GUILayout.Width(valueBoxWidth));
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    isDirty = true;
                }

                //=================================
                //			Post
                //=================================
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Post Colliders: ", GUILayout.Width(100));
                //af.postColliderMode = EditorGUILayout.IntPopup("", af.postColliderMode, subModeNames, subModeNums, GUILayout.Width(170));
                postColliderModeProp.intValue = (int)(ColliderType)EditorGUILayout.EnumPopup("", (ColliderType)postColliderModeProp.intValue, GUILayout.Width(collPopupWidth));

                GUILayout.Space(horizSpacing);
                if (af.postColliderMode == ColliderType.customCollider)
                    EditorGUILayout.PropertyField(postCustomColliderMeshProp, new GUIContent(""), GUILayout.Width(customFieldWidth));
                else if (addCustomField)
                    GUILayout.Space(customFieldWidth + 5);
                EditorGUILayout.LabelField("Y Scale: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.postBoxColliderHeightScale = EditorGUILayout.FloatField("", af.postBoxColliderHeightScale, GUILayout.Width(valueBoxWidth));
                GUILayout.Space(horizSpacing);
                EditorGUILayout.LabelField("Y Offset: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.postBoxColliderHeightOffset = EditorGUILayout.FloatField("", af.postBoxColliderHeightOffset, GUILayout.Width(valueBoxWidth));
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    isDirty = true;
                }
                //=================================
                //			SubPost
                //=================================
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Subpost Colliders: ", GUILayout.Width(100));
                //af.subpostColliderMode = EditorGUILayout.IntPopup("", af.subpostColliderMode, subModeNames, subModeNums, GUILayout.Width(170));
                subpostColliderModeProp.intValue = (int)(ColliderType)EditorGUILayout.EnumPopup("", (ColliderType)subpostColliderModeProp.intValue, GUILayout.Width(collPopupWidth));

                GUILayout.Space(horizSpacing);
                if (af.subpostColliderMode == ColliderType.customCollider)
                    EditorGUILayout.PropertyField(subpostCustomColliderMeshProp, new GUIContent(""), GUILayout.Width(customFieldWidth));
                else if (addCustomField)
                    GUILayout.Space(customFieldWidth + 5);
                EditorGUILayout.LabelField("Y Scale: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.subpostBoxColliderHeightScale = EditorGUILayout.FloatField("", af.subpostBoxColliderHeightScale, GUILayout.Width(valueBoxWidth));
                GUILayout.Space(horizSpacing);
                EditorGUILayout.LabelField("Y Offset: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.subpostBoxColliderHeightOffset = EditorGUILayout.FloatField("", af.subpostBoxColliderHeightOffset, GUILayout.Width(valueBoxWidth));
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    isDirty = true;
                }

                //=================================
                //			Extras
                //=================================

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Extras Colliders: ", GUILayout.Width(100));
                //af.extraColliderMode = EditorGUILayout.IntPopup("", af.extraColliderMode, subModeNames, subModeNums, GUILayout.Width(170));
                extraColliderModeProp.intValue = (int)(ColliderType)EditorGUILayout.EnumPopup("", (ColliderType)extraColliderModeProp.intValue, GUILayout.Width(collPopupWidth));

                GUILayout.Space(horizSpacing);
                if (af.extraColliderMode == ColliderType.customCollider)
                    EditorGUILayout.PropertyField(extraCustomColliderMeshProp, new GUIContent(""), GUILayout.Width(customFieldWidth));
                else if (addCustomField)
                    GUILayout.Space(customFieldWidth + 5);
                EditorGUILayout.LabelField("Y Scale: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.extraBoxColliderHeightScale = EditorGUILayout.FloatField("", af.extraBoxColliderHeightScale, GUILayout.Width(valueBoxWidth));
                GUILayout.Space(horizSpacing);
                EditorGUILayout.LabelField("Y Offset: ", GUILayout.Width(scaleOffsetLabelWidth));
                af.extraBoxColliderHeightOffset = EditorGUILayout.FloatField("", af.extraBoxColliderHeightOffset, GUILayout.Width(valueBoxWidth));


                if (af.railABoxColliderHeightScale < 0.1f)
                    af.railABoxColliderHeightScale = 0.1f;
                else if (af.railABoxColliderHeightScale > 10f)
                    af.railABoxColliderHeightScale = 10.0f;

                if (af.railBBoxColliderHeightScale < 0.1f)
                    af.railBBoxColliderHeightScale = 0.1f;
                else if (af.railBBoxColliderHeightScale > 10f)
                    af.railBBoxColliderHeightScale = 10.0f;

                if (af.postBoxColliderHeightScale < 0.1f)
                    af.postBoxColliderHeightScale = 0.1f;
                else if (af.postBoxColliderHeightScale > 10f)
                    af.postBoxColliderHeightScale = 10.0f;

                if (af.subpostBoxColliderHeightScale < 0.1f)
                    af.subpostBoxColliderHeightScale = 0.1f;
                else if (af.subpostBoxColliderHeightScale > 10f)
                    af.subpostBoxColliderHeightScale = 10.0f;

                if (af.extraBoxColliderHeightScale < 0.1f)
                    af.extraBoxColliderHeightScale = 0.1f;
                else if (af.extraBoxColliderHeightScale > 10f)
                    af.extraBoxColliderHeightScale = 10.0f;
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    isDirty = true;
                }


                GUILayout.Space(5);
                EditorGUILayout.LabelField("(On long or complex fences, selecting 'No Colliders' will improve performance",
                    infoStyle);
                EditorGUILayout.LabelField("while designing in the Editor. Add them when you're ready to finish.)",
                    infoStyle);

                GUILayout.EndVertical();
                GUILayout.Space(5);


                //=================================
                //			Layer number
                //=================================
                GUILayout.Space(10);
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Layer Number", headingStyle);
                EditorGUI.BeginChangeCheck();
                af.ignoreControlNodesLayerNum = EditorGUILayout.IntField("ignoreControlsLayerNum", af.ignoreControlNodesLayerNum);
                if (EditorGUI.EndChangeCheck())
                    isDirty = true;
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();

                if (isDirty)
                {
                    List<Transform> posts = af.postsPool;
                    for (int p = 0; p < af.allPostPositions.Count - 1; p++)
                    {
                        if (posts[p] != null)
                            posts[p].gameObject.layer = 0;
                    }
                    CleanAndRebuild(); //- Ensure old colliders are destroyed
                }



                GUILayout.Space(5);
                if (af.railAColliderMode < ColliderType.originalCollider || af.postColliderMode < ColliderType.originalCollider || af.extraColliderMode < ColliderType.originalCollider) //-- box or none
                {
                    EditorGUILayout.LabelField("Colliders are being used. It may improve ed performance to leave them  off until ready to Finish the Fence.\n");
                }
                EditorGUILayout.EndVertical();
                isDirty = false;
                EditorStyles.label.wordWrap = false;

            }
            //===========================================
            //             Debug
            //===========================================
            if (af.currGlobalsToolbarRow2 == 5)
            {
                GUILayout.Space(10);

                //     Stack
                //=================================
                GUILayout.BeginHorizontal();
                af.showStackTrace = EditorGUILayout.Toggle("Show Call Stack Trace in Console", af.showStackTrace, GUILayout.Width(300));
                af.showStackTrace = EditorGUILayout.Toggle("Show Call Stack Trace in Console", af.showStackTrace, GUILayout.Width(300));

                //     Clear Console
                //=================================
                if (GUILayout.Button("Clear Console", GUILayout.Width(100)))
                {
                    af.ClearConsole();
                }
                GUILayout.EndHorizontal();

                af.useDB = EditorGUILayout.Toggle("Show Graphical Debugging Markers", af.showStackTrace, GUILayout.Width(300));



                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField(new GUIContent("Debugging Options: ", "Enable this to print the flow important of Function calls to the console"));


                if (EditorGUI.EndChangeCheck())
                {
                    af.ForceRebuildFromClickPoints();
                }
            }
            //==================================================================================
            //
            //                             Dev Housekeeping
            //
            //==================================================================================
            if (af.currGlobalsToolbarRow2 == 6)
            {
                GameObject currPost = af.GetCurrentPrefabForLayer(LayerSet.postLayer);
                GameObject currRailA = af.GetCurrentPrefabForLayer(LayerSet.railALayer);
                GameObject currRailB = af.GetCurrentPrefabForLayer(LayerSet.railBLayer);
                GameObject currExtra = af.GetCurrentPrefabForLayer(LayerSet.extraLayer);
                GameObject currSubPost = af.GetCurrentPrefabForLayer(LayerSet.subpostLayer);
                int rowGap = 7;

                string[] prefabTypeNames = Enum.GetNames(typeof(PrefabTypeAFWB));
                // Remove 'prefab' and capitalize
                for (int i = 0; i < prefabTypeNames.Length; i++)
                {
                    prefabTypeNames[i] = prefabTypeNames[i].Replace("Prefab", "");
                    if (!string.IsNullOrEmpty(prefabTypeNames[i]))
                        prefabTypeNames[i] = char.ToUpper(prefabTypeNames[i][0]) + prefabTypeNames[i].Substring(1).ToLower();
                }

                GUI.backgroundColor = new Color(1f, 0.8f, 0.5f);
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(200);


                //       Choose which Preefab Type to work on
                //=================================================
                modPrefabTypeIndex = EditorGUILayout.Popup("", modPrefabTypeIndex, prefabTypeNames, GUILayout.Width(100));
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUI.backgroundColor = Color.white;

                PrefabTypeAFWB prefabType = PrefabTypeAFWB.postPrefab;
                GameObject mainPrefabOnCurrentlySelectedlayer = af.GetMainPrefabForLayer(currViewedLayer);
                Material mainMaterialOnCurrentlySelectedLayer = mainPrefabOnCurrentlySelectedlayer.GetComponent<Renderer>().sharedMaterial;
                LayerSet layer = LayerSet.postLayer;
                string oldName = currPost.name;
                string prefabDir = "_Posts_AFWB";
                string currPrefabName = currPost.name;
                int currentPrefabIndex = af.currentPost_PrefabIndex;
                if (modPrefabTypeIndex == 1)
                {
                    layer = LayerSet.railALayer;
                    prefabType = PrefabTypeAFWB.railPrefab;
                    oldName = currRailA.name;
                    prefabDir = "_Rails_AFWB";
                    currPrefabName = currRailA.name;
                    currentPrefabIndex = af.currentRail_PrefabIndex[0];
                }
                else if (modPrefabTypeIndex == 2)
                {
                    layer = LayerSet.extraLayer;
                    prefabType = PrefabTypeAFWB.extraPrefab;
                    oldName = currExtra.name;
                    prefabDir = "_Extras_AFWB";
                    currPrefabName = currExtra.name;
                    currentPrefabIndex = af.currentExtra_PrefabIndex;
                }
                string prefabTypeWord = prefabTypeNames[modPrefabTypeIndex];


                //-- Resize Prefabs
                //===================
                GUILayout.Space(rowGap);
                GUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField(new GUIContent("Resize:", "dd"), GUILayout.Width(60));
                if (GUILayout.Button(new GUIContent("Resize Prefabs", "Set Prefabs to a given absolute size in metres." +
                    "\n\nSetting a value to 0 will leave that dimension unchanged" +
                    "\n e.g. to set a new height of 2m, use (0,2,0)"), GUILayout.Width(126)))
                {
                    Housekeeping.ResizePrefabs(af.currAutoFenceBuilderDir + "/AFWB_Prefabs/_Posts_AFWB/Brick", af.resizedPrefabSize);
                }
                GUILayout.Space(rowGap);
                EditorGUILayout.LabelField(new GUIContent("New Size:  ", "dd"), GUILayout.Width(60));
                af.resizedPrefabSize = EditorGUILayout.Vector3Field("", af.resizedPrefabSize, GUILayout.Width(200));
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Reset", "Sets to (0,0,0). A zero value dimension will remain unchanged by the Resize"), GUILayout.Width(80)))
                {
                    af.resizedPrefabSize = Vector3.zero;
                }
                GUILayout.EndHorizontal();



                //-- Rename Prefab
                //===============================
                GUILayout.Space(rowGap);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent($"Rename {prefabTypeWord} Prefab", "Rename {prefabTypeWord} Prefab and update all presets that use it." +
                    $"\n\n Do not append '_{prefabTypeWord}', this will be added automatically."), GUILayout.Width(126)))
                {
                    string newName = newNameForPrefab + "_" + prefabTypeWord;

                    Housekeeping.RenamePrefabAsset(oldName, newName);
                    Housekeeping.UpdatePresetComponentAssetName(layer, oldName, newName, true);
                    AssetDatabase.Refresh();
                }
                GUILayout.Space(rowGap);

                List<string> shortPrefabMenuNames = af.GetShortPrefabMenuNamesForLayer(layer, stripCategory: false);
                if (GUILayout.Button(new GUIContent("Current", $"Use the Current {prefabTypeWord} as the {prefabTypeWord} to Rename"),
                    EditorStyles.miniButton, GUILayout.Width(54)))
                {
                    prefabToRenamePrefabIndex = currentPrefabIndex;
                    prefabToRenameMenuIndex = af.ConvertPrefabIndexToMenuIndexForLayer(prefabToRenamePrefabIndex, layer);
                }
                GUILayout.Space(rowGap);
                newNameForPrefab = EditorGUILayout.TextField(newNameForPrefab, GUILayout.Width(170));

                if (GUILayout.Button(new GUIContent("C", "Copy the Source Name"), GUILayout.Width(20)))
                {
                    prefabToRenameMenuIndex = af.ConvertPrefabIndexToMenuIndexForLayer(prefabToRenamePrefabIndex, layer);
                    prefabToRenameMenuIndex = EditorGUILayout.Popup("", prefabToRenameMenuIndex, shortPrefabMenuNames.ToArray(), GUILayout.Width(192));

                    prefabToRenamePrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(prefabToRenameMenuIndex, prefabType);



                    newNameForPrefab = af.StripPrefabTypeFromNameForType(currPost.name, prefabType);
                }
                GUILayout.EndHorizontal();

                string prefabDirectoryForLayer = af.autoFenceBuilderDefaultDir + "/AFWB_Prefabs/" + prefabDir;



                //-- Rename All Prefab References in Presets
                //================================================
                GUILayout.Space(rowGap);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent($"Rename All Prefab Refs in Presets", "update all presets that use it." +
                    $"\n\n Do not append '_{prefabTypeWord}', this will be added automatically."), GUILayout.Width(200)))
                {
                    string newName = newNameForPrefab + "_" + prefabTypeWord;
                    Housekeeping.UpdatePresetComponentAssetName(layer, oldNameForPrefab, newNameForPrefab);
                }
                //-- Old Name
                oldNameForPrefab = EditorGUILayout.TextField(oldNameForPrefab, GUILayout.Width(width: 170));
                //-- New Name
                newNameForPrefab = EditorGUILayout.TextField(newNameForPrefab, GUILayout.Width(170));

                GUILayout.EndHorizontal();
                AssetDatabase.Refresh();



                //    Rename Prefab Substring
                //===================================
                GUILayout.Space(rowGap);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent($"Replace Substring In All {prefabTypeWord} Prefabs", "Rename Prefabs that contain this substring " +
                    "and update all presets that use it."), GUILayout.Width(270)))
                {
                    List<string> sourceNames = Housekeeping.FindPrefabPathsContainingName(prefabDirectoryForLayer, renameSourceSubtring, returnNamesOnly: true, caseSensitive: false);

                    for (int i = 0; i < sourceNames.Count; i++)
                    {
                        string sourceName = sourceNames[i];
                        string newSourceName = StringUtilsTCT.ReplaceCaseInsensitive(sourceName, renameSourceSubtring, renameNewSubstring);
                        Housekeeping.RenamePrefabAsset(sourceName, newSourceName);
                        Housekeeping.UpdatePresetComponentAssetName(layer, sourceName, newSourceName, true);
                        ReloadPrefabs();
                        ReloadPresets();
                    }
                    AssetDatabase.Refresh();
                }
                GUILayout.Space(rowGap);
                renameSourceSubtring = EditorGUILayout.TextField(renameSourceSubtring, GUILayout.Width(80));
                EditorGUILayout.LabelField("  with ", GUILayout.Width(36));
                renameNewSubstring = EditorGUILayout.TextField(renameNewSubstring, GUILayout.Width(90));
                GUILayout.EndHorizontal();
                GUILayout.Space(rowGap);

                //-- Find Materials outside AFWB Folder
                //=================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"FindUsedMaterialsOutsideAFWBFolder ", ""), GUILayout.Width(300)))
                {
                    List<Material> strayMats = Housekeeping.FindUsedMaterialsOutsideAFWBFolder();
                    //Housekeeping.MoveStrayMaterialsIntoAFWBFolder();

                }

                //-- Find Textures outside AFWB Folder
                //=================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Find Textures outside AFWB Folder", ""), GUILayout.Width(300)))
                {
                    List<Texture2D> strayTex = Housekeeping.FindUsedTexturesOutsideAFWBFolder();
                    //Housekeeping.MoveTexturesIntoFolder(strayTex);

                }

                //-- Find materials with missing textures
                //=================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Find materials with missing textures", ""), GUILayout.Width(300)))
                {
                    List<Material> strayTex = Housekeeping.FindPrefabMaterialsWithMissingTextures(movePrefabs: true);
                    //Housekeeping.MoveTexturesIntoFolder(strayTex);

                }




                GUILayout.Space(rowGap);
                //-- Find Presets Using Prefab name
                //=====================================
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Find Presets Using Prefab Name: ", $"Use the Current {prefabTypeWord} as the {prefabTypeWord} to Rename"),
                    EditorStyles.miniButton, GUILayout.Width(200)))
                {
                    //Housekeeping.PrintPresetsUsingGameObject(presetPrefabName);
                    Housekeeping.PrintPresetsUsingGameObjectSimple(presetPrefabName);
                }
                GUILayout.Space(rowGap);
                presetPrefabName = EditorGUILayout.TextField(presetPrefabName, GUILayout.Width(140));
                GUILayout.Space(rowGap);
                GUILayout.EndHorizontal();


                //-- Find Prefabs Using Mesh name
                //=====================================
                GUILayout.Space(rowGap);
                List<GameObject> prefabsWithMeshName = null;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Find Prefabs Using Mesh Name: ", $"Use the Current {prefabTypeWord} as the {prefabTypeWord} to Rename"),
                    EditorStyles.miniButton, GUILayout.Width(200)))
                {
                    prefabsWithMeshName = Housekeeping.FindPrefabsUsingMeshName(meshName);

                    foreach (GameObject prefab in prefabsWithMeshName)
                    {
                        Debug.Log($"prefab.name: {prefab.name}    Mesh name: {meshName} \n");
                    }
                }
                GUILayout.Space(rowGap);
                meshName = EditorGUILayout.TextField(meshName, GUILayout.Width(140));
                GUILayout.Space(rowGap);
                GUILayout.EndHorizontal();


                //-- Find Presets Using Prefab
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Presets Using Current {prefabTypeWord}", ""), GUILayout.Width(250)))
                {
                    Housekeeping.PrintPresetsUsingGameObject(currPost.name);
                }


                //-- Find Presets Using material
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Presets Using Material Name {prefabTypeWord}", ""), GUILayout.Width(250)))
                {
                    List<ScriptablePresetAFWB> presetsWithMat = Housekeeping.FindPresetsUsingMaterialName("Brickwall_OldEnglish", this);
                    PrintUtilities.PrintList(presetsWithMat, $"presetsWithMat {presetsWithMat.Count}", true, allInOneLine: false);
                }


                //-- Find Prefabs Using material
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Prefabs Using Current Mat {prefabTypeWord}", ""), GUILayout.Width(250)))
                {
                    GameObject gameObject = af.GetCurrentPrefabForLayer(currViewedLayer);
                    Material mat = gameObject.GetComponent<Renderer>().sharedMaterial;
                    string matName = mat.name;
                    
                    List<GameObject> prefabsWithMatName = Housekeeping.FindPrefabsUsingMaterialName(matName);
                    foreach (GameObject prefab in prefabsWithMatName)
                    {
                        Debug.Log(prefab.name + "\n");
                    }
                }

                //-- Find Prefabs Using material on current sourceLayerList
                //==================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Find Prefabs Using material on current sourceLayerList {prefabTypeWord}", ""),
                    GUILayout.Width(450)))
                {
                    List<GameObject> prefabsWithMatName = Housekeeping.GetPrefabsUsingMaterial(mainMaterialOnCurrentlySelectedLayer);
                    foreach (GameObject prefab in prefabsWithMatName)
                    {
                        Debug.Log(prefab.name + "\n");
                    }
                }

                //-- Find Prefabs Using Texture name
                //=====================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Prefabs Using Texture Name on {prefabTypeWord}", ""), GUILayout.Width(250)))
                {
                    List<GameObject> prefabsWithMatName = Housekeeping.FindPrefabsUsingTextureName("BrickWallA_Concrete_CT");
                    foreach (GameObject prefab in prefabsWithMatName)
                    {
                        Debug.Log(prefab.name + "\n");
                    }
                }

                //-- Show Issues with Materials
                //=====================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Show Issues With All Materials", ""), GUILayout.Width(250)))
                {
                    List<Material> matsWithIssues = Housekeeping.ShowIssuesWithAllMaterials(limitToAFWB: true);
                }

                //--Show Materials Using Shader name
                //=====================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Show Materials Using Shader name", ""), GUILayout.Width(250)))
                {
                    List<Material> matsWithIssues = Housekeeping.ShowMaterialsUsingShaderName("Shader Graphs/ParallaxMapping", limitToAFWB: false, printAllNonStandard: true);
                }







                //-- Find Prefabs Using Mesh name
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Prefabs Using Mesh Name", ""), GUILayout.Width(250)))
                {
                    List<GameObject> allMeshFiles = Housekeeping.GetAllMeshesInDirectory(af.currMeshesDir);
                    foreach (GameObject meshGO in allMeshFiles)
                    {
                        MeshFilter meshFilter = meshGO.GetComponent<MeshFilter>();
                        if (meshFilter != null)
                        {
                            string meshName = meshGO.GetComponent<MeshFilter>().sharedMesh.name;
                            prefabsWithMeshName = Housekeeping.FindPrefabsUsingMeshName(meshName);
                            if (prefabsWithMeshName.Count == 0)
                                Debug.Log($"{meshName} is not used on any existing Prefab\n");
                            else if (prefabsWithMeshName.Count > 4)
                                Debug.Log($"{meshName} is used in >4 Prefabs\n");
                        }


                    }
                }

                //-- Get Prefabs Whose Mesh is Less than 1.4m high. Don't ask why.
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Prefabs Of Size Less Than {prefabTypeWord}", ""), GUILayout.Width(250)))
                {
                    List<GameObject> prefabsOfSize = Housekeeping.FindPrefabsWithMeshSmallerThan(1.4f, prefabDirectoryForLayer, "Y");
                    foreach (GameObject prefab in prefabsOfSize)
                    {
                        Debug.Log(prefab.name + "    " + MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(prefab).y + "\n");
                        string moveToDir = af.autoFenceBuilderDefaultDir + "/AFWB_Prefabs/_Posts_AFWB/Too Short/" + prefab.name + ".prefab";
                        ResourceUtilities.MovePrefab(prefab, moveToDir);
                    }
                }

                //-- Get Models Whose Mesh is Less than 1.4m high
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Print Prefabs Of Size Less Than {prefabTypeWord}", ""), GUILayout.Width(250)))
                {
                    List<GameObject> testPrefabs = Housekeeping.FindPrefabsWithMeshSmallerThan(20f, "Assets/Auto Fence Builder/AFWB_Prefabs/_Posts_AFWB/Test", "Y");

                    foreach (GameObject prefab in testPrefabs)
                    {
                        Debug.Log(prefab.name + "    " + MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(prefab).y + "\n");
                        string moveToDir = af.autoFenceBuilderDefaultDir + "/AFWB_Prefabs/_Posts_AFWB/Test2/" + prefab.name + ".prefab";
                        ResourceUtilities.MovePrefab(prefab, moveToDir);
                    }
                }

                //-- Rename FBX Meshes to Match FBX file name
                //==========================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"RenameFBXMeshesConsistent", ""), GUILayout.Width(250)))
                {
                    Housekeeping.RenameFBXMeshesConsistent(af.currMeshesDir);
                }

                //-- Find Prefabs with Missing Meshes
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"FindPrefabsWithMissingMeshes", ""), GUILayout.Width(250)))
                {
                    List<GameObject> prefabsWithMissingMesh = Housekeeping.FindPrefabsWithMissingMesh();
                    foreach (GameObject prefab in prefabsWithMissingMesh)
                    {
                        Debug.Log(prefab.name + "\n");
                    }
                    if (prefabsWithMissingMesh.Count == 0)
                        Debug.Log("No Missing Meshes Found\n");
                }

                //-- Find Presets with Missing Meshes
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Find Presets With Missing Meshes", ""), GUILayout.Width(250)))
                {
                    List<ProblemPreset> prefabsWithMissingMesh = Housekeeping.FindPresetsWithMissingMesh();
                }

                //-- Assign Meshes to Prefabs from PrefabMeshNames.txt
                //==========================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"AssignPrefabMeshNamesFromTextFile", ""), GUILayout.Width(250)))
                {
                    List<GameObject> allPrefabs = Housekeeping.GetAllPrefabsInDirectory(af.currPrefabsDir);
                    List<GameObject> allMeshes = Housekeeping.GetAllMeshesInDirectory(af.currMeshesDir);

                    Housekeeping.AssignMeshToPrefabUsingTextFile(allPrefabs, allMeshes);
                }

                //-- Write Prefab + associated Mesh Names to a Text File
                //==========================================================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"WritePrefabMeshNamesToTextFile", ""), GUILayout.Width(250)))
                {
                    List<GameObject> prefabs = Housekeeping.GetAllPrefabsInDirectory(af.currPrefabsDir);
                    Housekeeping.WritePrefabMeshNamesToTextFile(prefabs);
                }

                //-- Apply Mesh Map
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"ApplyMeshMap", ""), GUILayout.Width(250)))
                {
                    Dictionary<string, string> prefabbMeshMap = Housekeeping.LoadPrefabMeshMappings(af.currAutoFenceBuilderDir + "/PrefabMeshNames.txt");
                    List<GameObject> prefabs = Housekeeping.GetAllPrefabsInDirectory(af.currPrefabsDir);
                    Housekeeping.CheckPrefabs(prefabs, prefabbMeshMap);

                }
                //-- Find Presets with Missing Meshes
                //===============================
                GUILayout.Space(rowGap);
                if (GUILayout.Button(new GUIContent($"Find Unused Prefabs", ""), GUILayout.Width(250)))
                {
                    List<GameObject> unusedPrefabs = Housekeeping.FindUnusedPrefabsInPresets();
                }


                GUILayout.Space(2);
                //----------------------------------------------------------------------------------------  
                //Housekeeping.FixPanelRailNamesInPresets();
                //Housekeeping.UpdatePresetComponentAssetName(LayerSet.railALayer, "Metal_TwoPlusOneGreen_Panel", "Metal_TwoPlusOneGreen_Rail", true);
                //====================================
                //      Test 4 - Comment Out for Release
                //======================================
                /*if (GUILayout.Button("Unused Tex"))
                {
                    //Housekeeping.Tests();
                    List<GameObject> unusedGameObjects = Housekeeping.FindUnusedGameObjects(af.railPrefabs, mainPresetList);
                    Housekeeping.PrintUnusedGameObjects(unusedGameObjects);

                    List<GameObject> goList = af.FindPrefabsByNameContains("background");
                    PrintUtilities.PrintList(goList, $"goList {goList.Count}", true, allInOneLine: false);
                }*/
            }

            //EditorGUILayout.EndFoldoutHeaderGroup();
        }// End of showGlobals
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();
        GUILayout.EndVertical(); //End Of Globals Section

        GUILayout.Space(10);
    }











}