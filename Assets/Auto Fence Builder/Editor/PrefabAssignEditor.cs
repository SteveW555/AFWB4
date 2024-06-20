//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using AFWB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug; // Alias UnityEngine.Debug to Debug

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public partial class PrefabAssignEditor
{

    AutoFenceCreator af;
    AutoFenceEditor ed;

    LayerSet kRailALayer = LayerSet.railALayer; // to save a lot of typing
    LayerSet kRailBLayer = LayerSet.railBLayer;
    LayerSet kPostLayer = LayerSet.postLayer;
    LayerSet kSubpostLayer = LayerSet.subpostLayer;
    LayerSet kExtraLayer = LayerSet.extraLayer;
    int kRailAIndex = (int)LayerSet.railALayer, kRailBIndex = (int)LayerSet.railBLayer;

    RandomLookupAFWB randTableRailA = null, randTableRailB = null, randTablePost = null;
    int randTableSize = 0;
    //RandomRecords railARandRecords = new RandomRecords();
    private string layerStr, prefabFilterString = "";

    List<string> filteredMenuNames = new List<string>();
    List<string> displayMenuNames = new List<string>();

    int realMenuIndexForLayer = 0;

    public PrefabAssignEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    //===================================
    public List<GameObject> GetListOfPrefabsWithCategory(PrefabTypeAFWB postPrefab, string categoryString)
    {
        List<GameObject> categoryList = new List<GameObject>();


        if (postPrefab == PrefabTypeAFWB.postPrefab)
        {
            for (int i = 0; i < af.postPrefabs.Count; i++)
            {
                if (af.postMenuNames[i].Contains(categoryString))
                {
                    int prefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(i, postPrefab);
                    categoryList.Add(af.postPrefabs[prefabIndex]);
                    Debug.Log(af.postMenuNames[i] + "     " + af.postPrefabs[prefabIndex].name + "\n");
                }
            }
        }

        return categoryList;
    }
    public List<int> GetListOfPrefabMenuIndicesWithCategory(PrefabTypeAFWB PrefabTypeAFWB, string categoryString)
    {
        List<int> categoryList = new List<int>();


        if (PrefabTypeAFWB == PrefabTypeAFWB.postPrefab)
        {
            for (int i = 0; i < af.postPrefabs.Count; i++)
            {
                if (af.postMenuNames[i].Contains(categoryString))
                {
                    categoryList.Add(i);
                }
            }
        }
        else if (PrefabTypeAFWB == PrefabTypeAFWB.railPrefab)
        {
            for (int i = 0; i < af.railPrefabs.Count; i++)
            {
                if (af.railMenuNames[i].Contains(categoryString))
                {
                    categoryList.Add(i);
                }
            }
        }
        return categoryList;
    }
    /// <summary>
    /// Loops through all items in the list and compares each item with the search string.
    /// </summary>
    /// <param name="searchString">The search string to compare against.</param>
    /// <param name="list">The list of strings to search through.</param>
    private int CompareStringWithList(string searchString, List<string> list)
    {
        bool found = false;

        for (int i = 0; i < list.Count; i++)
        {
            string listItem = list[i];

            // Compare the strings character by character
            if (searchString == listItem)
            {
                Debug.Log($"Exact match found at index {i}: '{listItem}'");
                found = true;
                return i;
                break;
            }
            else
            {
                CompareStringsCharacterByCharacter(searchString, listItem);
            }
        }

        if (!found)
        {
            Debug.Log("No exact match found in the list.");
        }
        return -1;
    }

    /// <summary>
    /// Compares two strings character by character and prints out any differences.
    /// </summary>
    /// <param name="str1">The first string to compare.</param>
    /// <param name="str2">The second string to compare.</param>
    private bool CompareStringsCharacterByCharacter(string str1, string str2)
    {
        // Print lengths
        //Debug.Log($"String 1 Length: {str1.Length}");
        //Debug.Log($"String 2 Length: {str2.Length}");

        // Compare characters
        for (int i = 0; i < Math.Max(str1.Length, str2.Length); i++)
        {
            char char1 = i < str1.Length ? str1[i] : '\0';
            char char2 = i < str2.Length ? str2[i] : '\0';
            if (char1 != char2)
            {
                //Debug.Log($"Difference at index {i}: char1='{char1}' (U+{(int)char1:X4}), char2='{char2}' (U+{(int)char2:X4})");
            }
        }

        // Print message if no differences found
        if (str1.Equals(str2))
        {
            Debug.Log("Strings are identical.");
            return true;
        }
        return false;
    }

    

    //===================================
    public MeshCollider ChooseMainPrefab(LayerSet layer)
    {
        bool mainPrefabChanged = false;
        string layerName = af.GetLayerNameAsString(layer);
        PrefabTypeAFWB prefabType = af.GetPrefabTypeFromLayer(layer);
        int layerIndex = (int)layer;

        int currMenuIndex = 0;
        int numMenuNames = 0;


        if (layer == kPostLayer)
        {
            currMenuIndex = af.currentPost_PrefabMenuIndex;
            numMenuNames = af.postMenuNames.Count;
            ed.usePrefabProp = ed.userPrefabPostProp;
            ed.importScaleModeProp = ed.postImportScaleModeProp;
        }
        else if (layer == kRailALayer || layer == kRailBLayer)
        {
            currMenuIndex = af.currentRail_PrefabMenuIndex[layerIndex];
            numMenuNames = af.railMenuNames.Count;
            int prefabIndex = af.currentRail_PrefabIndex[layerIndex];
            ed.usePrefabProp = ed.userPrefabRailProp[layerIndex];
            ed.importScaleModeProp = ed.railAImportScaleModeProp;
            if (layer == kRailBLayer)
                ed.importScaleModeProp = ed.railBImportScaleModeProp;
        }
        else if (layer == kSubpostLayer)
        {
            currMenuIndex = af.currentSubpost_PrefabMenuIndex;
            numMenuNames = af.postMenuNames.Count;
        }
        else if (layer == kExtraLayer)
        {
            currMenuIndex = af.currentExtra_PrefabMenuIndex;
            numMenuNames = af.extraMenuNames.Count;
            ed.usePrefabProp = ed.userPrefabExtraProp;
            ed.importScaleModeProp = ed.extraImportScaleModeProp;
        }
        //    Current prefab is no longer the user prefab, use the placeholder and disable the import controls
        //========================================================================================================
        if (af.GetMainPrefabForLayer(layer) != af.GetUserPrefabForLayer(layer))
        {
            //Debug.Log("User Prefab != current prefab\n");
            af.userPrefabPost = af.userPrefabPlaceholder;

        }

        //========================================================================
        //                          Choose Prefab
        //========================================================================
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        string helpStr = $"Choose a prefab to be used as the {layerName}.#n";
        if (prefabType == PrefabTypeAFWB.railPrefab)
            helpStr += "\nThese are the sections that span between the postsPool or click-point nodes \ne.g. walls, fence panels, or fence rails";


        //              Filter Calculation
        //==============================================
        realMenuIndexForLayer = af.GetMainPrefabMenuIndexForLayer(layer);

        List<string> fullPrefabMenuNames = af.GetPrefabMenuNamesForLayer(layer);
        List<string> filteredMenuNames = new List<string>();

        if (prefabFilterString != "")
        {
            filteredMenuNames.AddRange(fullPrefabMenuNames);
        }
        else
        {
            for (int i = 0; i < fullPrefabMenuNames.Count; i++)
            {
                if (fullPrefabMenuNames[i].ToLower().Contains(prefabFilterString.ToLower()))
                    filteredMenuNames.Add(fullPrefabMenuNames[i]);
            }
        }

        //--  Choose what menu names to display, all or filtered
        if (prefabFilterString != "" && filteredMenuNames.Count > 0)
            displayMenuNames = filteredMenuNames;
        else
            displayMenuNames = fullPrefabMenuNames;

        //-- Calculate the menu index to use
        if (prefabFilterString != "" && filteredMenuNames.Count > 0)
        {
            if (ed.af.componentDisplayMenuIndex >= filteredMenuNames.Count)
                ed.af.componentDisplayMenuIndex = 0;
        }
        else
            ed.af.componentDisplayMenuIndex = realMenuIndexForLayer;

        //-------------------------------------------------

        EditorGUI.BeginChangeCheck();

        //    Choose Prefab Label
        //==============================
        EditorGUILayout.LabelField(new GUIContent("Choose Prefab Type:", helpStr), ed.moduleHeaderLabelStyle, GUILayout.Width(145));

        //     < > Buttons
        //==============================
        GUILayout.Space(8);
        if (GUILayout.Button(new GUIContent("<", "Choose Previous Prefab"), EditorStyles.miniButton, GUILayout.Width(17)) && currMenuIndex > 0)
        {
            ed.af.componentDisplayMenuIndex -= 1;
            mainPrefabChanged = true;
        }
        GUILayout.Space(2);
        if (GUILayout.Button(new GUIContent(">", "Choose Next Prefab"), EditorStyles.miniButton, GUILayout.Width(17)) && currMenuIndex < numMenuNames - 1)
        {
            ed.af.componentDisplayMenuIndex += 1;
            mainPrefabChanged = true;
        }
        GUILayout.Space(3);

        //  Main Choose prefab Popup Menu
        //===================================
        //-- The menu names are different to the prefab names, as they have a "category/" added, so we have to do some conversion between the two
        ed.mediumPopup.fontSize = 11;
        ed.mediumPopup.normal.textColor = new Color(.7f, .38f, 0.0f);
        int popupWidth = 230;
        ed.af.componentDisplayMenuIndex = EditorGUILayout.Popup("", ed.af.componentDisplayMenuIndex, displayMenuNames.ToArray(), GUILayout.Width(popupWidth));


        //      Filter Box
        //=======================
        GUILayout.Space(7);
        prefabFilterString = EditorGUILayout.TextField(prefabFilterString, GUILayout.Width(97));
        if (GUILayout.Button(new GUIContent("X", "Clear the search filter"), EditorStyles.miniButton, GUILayout.Width(20)))
        {
            prefabFilterString = "";
        }
        EditorGUILayout.LabelField(new GUIContent("Filter", helpStr), ed.smallModuleLabelStyle, GUILayout.Width(30));

        GUILayout.Space(4);

        //  For Subposts, add option to use Post prefab
        //==============================================
        if (layer == kSubpostLayer && GUILayout.Button(new GUIContent("Use Post Prefab",
           $"Subpost will use the same prefab as the main Post:\n '{af.postPrefabs[af.currentPost_PrefabIndex].name}'"), ed.smallToolbarButtonStyle,
            GUILayout.Width(92)))
        {
            af.currentSubpost_PrefabMenuIndex = af.currentPost_PrefabMenuIndex;
            af.currentSubpost_PrefabIndex = af.currentPost_PrefabIndex;

        }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();

        //        EndChangeCheck
        //==============================
        if (EditorGUI.EndChangeCheck())
        {
            mainPrefabChanged = true;
            //-- If we're using a filtered list, find the name of the selected in the full list of prefabs
            string currSelectedPrefabName = displayMenuNames[ed.af.componentDisplayMenuIndex];
            //int indexForName = fullPrefabMenuNames.IndexOf(currSelectedPrefabName);
            int menuIndexForName = CompareStringWithList(currSelectedPrefabName, fullPrefabMenuNames);
            af.SetMainPrefabMenuIndexForLayer(layer, menuIndexForName);
        }
        MeshCollider userMeshCol = null;
        bool importAttempted = false;

        //===============================================================
        //                      User Custom Prefab
        //===============================================================

        //-- savedUserPrefab is the one saved in the prefabs folder and which will be loaded into the prefab lists from LoadPrefabs() below 
        GameObject savedUserPrefab = HandleUserImport(layer, ref mainPrefabChanged, layerIndex, out importAttempted, out userMeshCol);
        ShowImportMessage(layer, savedUserPrefab);
        bool addedUserPrefab = false;
        int indexOfNewPrefab = -1;
        
        //     Do necessary setup for a succesfully added User Prefab
        //===============================================================
        if (savedUserPrefab != null)
        {
            ed.LoadPrefabs();
            addedUserPrefab = true;
            
            //-- Get the index of the newly added prefab in the prefab list
            int prefabIndex = af.FindPrefabIndexByNameForLayer(layer.ToPrefabType(), savedUserPrefab.name, "Looking for just-saved User Prefab inPrefabAssignEdotor ChooseMainPrefab()", 
                warnMissing: true, replaceMissingWithDefault: false);
            Debug.Log("prefabIndex: " + prefabIndex + "\n");
            af.RebuildPoolWithNewUserPrefab(savedUserPrefab, layer);

            //-- Get the index of the new prefab in the reloaded List
            indexOfNewPrefab = af.FindPrefabForLayer(savedUserPrefab, layer);
            
            //-- Set this as the current index 
            af.SetCurrentPrefabIndexForLayer(indexOfNewPrefab, layer);

            //-- Update the menu index from this
            af.SetMenuIndexFromPrefabIndexForLayer(indexOfNewPrefab, layer);

            //-- Update the properties
            if (layer == kPostLayer)
            {
                af.userPrefabPost = savedUserPrefab;
                ed.userPrefabPostProp = ed.serializedObject.FindProperty("userPrefabPost");
            }
        }
        if (af.GetMainPrefabForLayer(layer) != af.GetUserPrefabForLayer(layer))
        {
            //Debug.Log("User Prefab != current prefab\n");
            af.userPrefabPost = af.userPrefabPlaceholder;

        }


        if (layer == kPostLayer)
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), ed.uiLineGreyCol2);


        //================================================================
        //              Optional Prefab Override for Posts
        //================================================================

        PostPrefabOverrides(currMenuIndex, numMenuNames, layer);
        //--------------------------
        if (mainPrefabChanged)
        {
            
            if (addedUserPrefab == true)
            {
                //-- As the prefabs were reloaded after saving, the new one should now be in the prefab Lists
                //indexOfNewPrefab = af.FindPrefabForLayer(savedUserPrefab, sourceLayerList);

                //-- Usually we choose a prefabfrom the menu, then have to sync the prebIndex with it.
                //-- But as we added a known custom prefab directly, we have to do the reverse and sync the menu index with the prefab index
                //af.SetCurrentPrefabIndexForLayer(indexOfNewPrefab, sourceLayerList);
                //af.SetMenuIndexFromPrefabIndexForLayer(indexOfNewPrefab, sourceLayerList);
                //ed.postprop
            }
            if (layer == kPostLayer)
            {
                int prefabIndex = af.currentPost_PrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentPost_PrefabMenuIndex, PrefabTypeAFWB.postPrefab);
                af.SetPostPrefab(prefabIndex, false);
                af.SetSourceVariantGoAtIndexForLayer(0, prefabIndex, layer);
            }
            else if (layer == kRailALayer || layer == kRailBLayer)
            {
                int prefabIndex = af.currentRail_PrefabIndex[layerIndex] = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentRail_PrefabMenuIndex[layerIndex], PrefabTypeAFWB.railPrefab);
                af.SetRailPrefab(prefabIndex, layer, false);
                af.SetSourceVariantGoAtIndexForLayer(0, prefabIndex, layer);
            }
            else if (layer == kSubpostLayer)
            {
                af.currentSubpost_PrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentSubpost_PrefabMenuIndex, PrefabTypeAFWB.postPrefab);
                af.SetSubpostPrefab(af.currentSubpost_PrefabIndex, false);
            }
            else if (layer == kExtraLayer)
            {
                af.currentExtra_PrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentExtra_PrefabMenuIndex, PrefabTypeAFWB.extraPrefab);
                af.SetExtraPrefab(af.currentExtra_PrefabIndex, false);
            }

            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
        }
        if (layer == kPostLayer)
            GUILayout.Space(5);
        else
            GUILayout.Space(3);

        

        return userMeshCol;
    }

    private void ShowImportMessage(LayerSet layer, GameObject newUserObject)
    {
        if (newUserObject != null)
            Debug.Log($"User Object: {newUserObject.name}\n");

        //Debug.Log($"{af.userPrefabRail[0]} \n");
        if (newUserObject != null && af.GetUserPrefabForLayer(layer) != null && af.GetUserPrefabForLayer(layer) != af.userPrefabPlaceholder)
        {
            GameObject userPrefab = af.GetUserPrefabForLayer(layer);
            if (userPrefab != null)
                EditorGUILayout.LabelField(new GUIContent($"{userPrefab.name} was successfully added to {layer.String()}"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(300));
            else
                EditorGUILayout.LabelField(new GUIContent($"Import of {userPrefab.name} Failed for {layer.String()}"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(300));
        }
    }

    //--------------------------

    private void PostPrefabOverrides(int currMenuIndex, int numMenuNames, LayerSet layer)
    {
        if (layer == LayerSet.postLayer)
        {
            GUI.backgroundColor = new Color(0.96f, 0.92f, 1f);
            string mainPostName = af.GetMainPrefabNameForLayer(LayerSet.postLayer);
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(9);

            //   Show Optional Post Prefabs
            //===================================

            ed.showOptionalPostPrefabsProp.boolValue = EditorGUILayout.Foldout(ed.showOptionalPostPrefabsProp.boolValue,
                new GUIContent("Show Alternative Post Prefabs", "Option to choose different Prefabs for the Nodes or End Posts"));
            if (ed.showOptionalPostPrefabsProp.boolValue == false && (af.allowNodePostsPrefabOverride || af.allowEndPostsPrefabOverride))
                EditorGUILayout.LabelField(new GUIContent(" [ Override Active ]"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(420));


            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                ed.serializedObject.ApplyModifiedProperties();
            }

            if (ed.showOptionalPostPrefabsProp.boolValue)
            {
                //    Optional MAIN-NODE Prefab Override for Posts
                //====================================================

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);

                //-- Label
                EditorGUILayout.LabelField(new GUIContent("Optional Prefab Override for Main Node Posts", "This sets the prefab to use for the " +
                    "Main Node Posts (Clickpoints). It will Override all other settings at those locations, " +
                    "except where 'Singles' have specified a particular Prefab for a Post."), ed.lilacUnityStyle, GUILayout.Width(264));

                //-- Enable
                SerializedProperty allowNodePostPrefabOverrideProp = ed.serializedObject.FindProperty("allowNodePostsPrefabOverride");
                EditorGUILayout.PropertyField(allowNodePostPrefabOverrideProp, new GUIContent("", "gg"), GUILayout.Width(24));

                SerializedProperty nodePostsMenuIndexProp = ed.serializedObject.FindProperty("nodePostsOverrideMenuIndex");
                EditorGUI.BeginDisabledGroup(allowNodePostPrefabOverrideProp.boolValue == false);
                {
                    //     < > Buttons
                    //==============================
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("<", "Choose Previous Prefab"), EditorStyles.miniButton, GUILayout.Width(17)) && currMenuIndex > 0)
                    {
                        nodePostsMenuIndexProp.intValue -= 1;
                    }
                    GUILayout.Space(2);
                    if (GUILayout.Button(new GUIContent(">", "Choose Next Prefab"), EditorStyles.miniButton, GUILayout.Width(17)) && currMenuIndex < numMenuNames - 1)
                    {
                        nodePostsMenuIndexProp.intValue += 1;
                    }
                    GUILayout.Space(3);

                    //    Choose prefab Popup Menu
                    //===================================
                    List<string> shortPrefabMenuNames = af.GetShortPrefabMenuNamesForLayer(LayerSet.postLayer);
                    nodePostsMenuIndexProp.intValue = EditorGUILayout.Popup("", nodePostsMenuIndexProp.intValue, shortPrefabMenuNames.ToArray(),
                        ed.lilacPopupStyle, GUILayout.Width(200));

                    //      Set to Main
                    //==============================
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("M", $"Set to the same Prefab as the Main Post:  \n" +
                        $"[   {mainPostName}   ]"), EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        nodePostsMenuIndexProp.intValue = af.GetMainPrefabMenuIndexForLayer(LayerSet.postLayer);
                    }


                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    ed.serializedObject.ApplyModifiedProperties(); //sets nodePostsOverrideMenuIndex to nodePostsMenuIndexProp.intValue
                    af.nodePostsOverridePrefabIndex = af.ConvertPostMenuIndexToPrefabIndex(af.nodePostsOverrideMenuIndex);
                    af.ResetPoolForLayer(LayerSet.postLayer);
                    af.ForceRebuildFromClickPoints();
                }

                //    Optional ENDS Prefab Override for Posts
                //====================================================

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);

                //-- Label
                EditorGUILayout.LabelField(new GUIContent("Optional Prefab Override for Start/End Posts", "This sets the prefab to use for the " +
                    "First and Last Posts. It will Override all other settings at those locations, " +
                    "except where 'Singles' have specified a particular Prefab for a Post."), ed.lilacUnityStyle, GUILayout.Width(264));

                //-- Enable
                SerializedProperty allowEndPostsPrefabOverrideProp = ed.serializedObject.FindProperty("allowEndPostsPrefabOverride");
                EditorGUILayout.PropertyField(allowEndPostsPrefabOverrideProp, new GUIContent("", "gg"), GUILayout.Width(24));


                //-- Popup and < > buttons
                SerializedProperty endsMenuIndexProp = ed.serializedObject.FindProperty("endPostsOverrideMenuIndex");
                EditorGUI.BeginDisabledGroup(allowEndPostsPrefabOverrideProp.boolValue == false);
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("<", "Choose Previous Prefab"), EditorStyles.miniButton, GUILayout.Width(17)) && currMenuIndex > 0)
                    {
                        endsMenuIndexProp.intValue -= 1;
                    }
                    GUILayout.Space(2);
                    if (GUILayout.Button(new GUIContent(">", "Choose Next Prefab"), EditorStyles.miniButton, GUILayout.Width(17)) && currMenuIndex < numMenuNames - 1)
                    {
                        endsMenuIndexProp.intValue += 1;
                    }
                    //    Choose prefab Popup Menu
                    //===================================
                    GUILayout.Space(3);
                    List<string> shortPrefabMenuNames = af.GetShortPrefabMenuNamesForLayer(LayerSet.postLayer);
                    endsMenuIndexProp.intValue = EditorGUILayout.Popup("", endsMenuIndexProp.intValue, shortPrefabMenuNames.ToArray(), ed.lilacPopupStyle,
                        GUILayout.Width(200));

                    //      Set to Main
                    //==============================
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("M", $"Set to the same Prefab as the Main Post:  \n" +
                        $"[   {mainPostName}   ]"), EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        endsMenuIndexProp.intValue = af.GetMainPrefabMenuIndexForLayer(LayerSet.postLayer);
                    }

                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    ed.serializedObject.ApplyModifiedProperties();//sets endPostsOverrideMenuIndex to endsMenuIndexProp.intValue
                    af.endPostsOverridePrefabIndex = af.ConvertPostMenuIndexToPrefabIndex(af.endPostsOverrideMenuIndex);
                    af.ResetPoolForLayer(LayerSet.postLayer);
                    af.ForceRebuildFromClickPoints();
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }


    //------------------------------------
    public bool LayerHasUserObject(LayerSet layer)
    {
        bool hasCustomObject = false;

        if (layer == kPostLayer && ed.af.userPrefabPost != null)
            hasCustomObject = true;
        else if (layer == kRailALayer && ed.af.userPrefabRail[kRailAIndex] != null)
            hasCustomObject = true;
        else if (layer == kRailBLayer && ed.af.userPrefabRail[kRailBIndex] != null)
            hasCustomObject = true;
        else if (layer == kExtraLayer && ed.af.userPrefabExtra != null)
            hasCustomObject = true;

        return hasCustomObject;
    }


    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
