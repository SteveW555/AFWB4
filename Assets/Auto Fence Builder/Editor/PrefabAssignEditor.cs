//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using AFWB;
using MeshUtils;
using System;
using static UnityEditor.Experimental.GraphView.GraphView;
using TCT.PrintUtils;
using UnityEngine.Rendering;
using System.Linq;

public class PrefabAssignEditor
{

    AutoFenceCreator af;
    AutoFenceEditor ed;

    LayerSet kRailALayer = LayerSet.railALayerSet; // to save a lot of typing
    LayerSet kRailBLayer = LayerSet.railBLayerSet;
    LayerSet kPostLayer = LayerSet.postLayerSet;
    LayerSet kSubpostLayer = LayerSet.subpostLayerSet;
    LayerSet kExtraLayer = LayerSet.extraLayerSet;
    int kRailAIndex = (int)LayerSet.railALayerSet, kRailBIndex = (int)LayerSet.railBLayerSet;

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
            ed.userObjectProp = ed.userObjectPostProp;
            ed.importScaleModeProp = ed.postImportScaleModeProp;
        }
        else if (layer == kRailALayer || layer == kRailBLayer)
        {
            currMenuIndex = af.currentRail_PrefabMenuIndex[layerIndex];
            numMenuNames = af.railMenuNames.Count;
            int prefabIndex = af.currentRail_PrefabIndex[layerIndex];
            ed.userObjectProp = ed.userObjectRailProp[layerIndex];
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
            ed.userObjectProp = ed.userObjectExtraProp;
            ed.importScaleModeProp = ed.extraImportScaleModeProp;
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
            int indexForName = fullPrefabMenuNames.IndexOf(currSelectedPrefabName);
            af.SetMainPrefabMenuIndexForLayer(layer, indexForName);
        }
        MeshCollider userMeshCol = null;
        bool importAttempted = false;

        //===============================================================
        //                      User Custom Prefab
        //===============================================================
        
        GameObject newUserObject = HandleUserImport(layer, ref mainPrefabChanged, layerIndex, out importAttempted, out userMeshCol);
        if(newUserObject != null)
            Debug.Log ($"User Object: {newUserObject.name}\n");
        
        
        //Debug.Log($"{af.userPrefabRail[0]} \n");
        if (newUserObject != null && af.GetUserPrefabForLayer(layer) != null && af.GetUserPrefabForLayer(layer) != af.userPrefabPlaceholder )
        {
            GameObject userPrefab = af.GetUserPrefabForLayer(layer);


            if (userPrefab != null)
                EditorGUILayout.LabelField(new GUIContent($"{userPrefab.name} was successfully added to {layer.String()}"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(300));
            else
                EditorGUILayout.LabelField(new GUIContent($"Import of {userPrefab.name} Failed for {layer.String()}"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(300));
        }
        if (layer == kPostLayer)
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), ed.uiLineGreyCol2);
        //-- If we've added a new object, then early exit
        //if (newUserObject != null)
            //return userMeshCol;

        //================================================================
        //              Optional Prefab Override for Posts
        //================================================================

        PostPrefabOverrides(currMenuIndex, numMenuNames, layer);

        //--------------------------
        if (mainPrefabChanged)
        {
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


    //--------------------------
    private GameObject HandleUserImport(LayerSet layer, ref bool mainPrefabChanged, int layerIndex, out bool importAttempted, out MeshCollider userMeshCol)
    {
        importAttempted = false;
        if (ed.userObjectProp == null)
        {
            userMeshCol = null;
            return null;
        }

        // Create a GUIStyle with a faint color
        GUILayout.Space(4);

        EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), ed.uiLineGreyCol2);

        GUI.backgroundColor = new Color(0.8f, 0.88f, 1f);
        GameObject userAddedPrefab = af.userPrefabPost;
        userMeshCol = null;
        if (layer != kPostLayer)
            userAddedPrefab = af.userPrefabRail[kRailAIndex];

        GUILayout.Space(2);

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        string layerStr = af.GetLayerNameAsString(layer);

        //  Custom Import Label
        //=======================
        GUIStyle subtleBlueLabelStyle = new GUIStyle(EditorStyles.label);
        //subtleBlueLabelStyle.fontSize = 12;
        subtleBlueLabelStyle.normal.textColor = new Color(0.75f, 0.84f, 1f);
        EditorGUILayout.LabelField(new GUIContent("Custom Import - Drag: ",
            $"Drag a GameObject from the Hierarchy in to this slot to use as a {layerStr}" +
            $"\n\n This custom prefab can then be found in the prefab menu under 'User'. \n\nYour original asset will be left unaltered, and the copy" +
            $"that AFWB will use can be found in the UserAssets_AFWB Folder"), subtleBlueLabelStyle, GUILayout.Width(152));

        //===============================
        //          Drag Box
        //===============================

        //-- userObjectProp links to the userPrefabPost, or userPrefabRail[2], or userPrefabExtra
        EditorGUILayout.PropertyField(ed.userObjectProp, new GUIContent(""), GUILayout.Width(189));

        //      User Scale Options
        //================================
        GUILayout.Space(8);
        string[] scaleNames = System.Enum.GetNames(typeof(UserObjectImportOptions));
        EditorGUILayout.LabelField(new GUIContent("Size:", $"Determines how the model is scaled to fit within the fence. The scaling will not affect your" +
            $" original mesh, only the AutoFence copy.\nWhichever option is chosen, the scaling can still be adjusted in the {layer.String()} Scale box below.\n\n" +
            $"Match:   Approximates the size and scale of the existing {layer.String()} in use" +
            $"\n\nAutoFit:   Scales to fit within the existing section size, while trying to keep some of the overall scale" +
            $"\n\nRaw Size:   This will not scale the mesh. \nFor large meshes this can create problems when they are substantially bigger " +
            $"than what would fit in the section size.\n For example, adding a house model to a regular fence will" +
            $"result in a mass of overlapping meshes. " +
            $"\nThis can be corrected for in the components Scale box below, but will initially be difficult to work with."), GUILayout.Width(31));
        ed.importScaleModeProp.intValue = EditorGUILayout.Popup(new GUIContent("", "bobobobob"), ed.importScaleModeProp.intValue,
            scaleNames, GUILayout.Width(56));

        GameObject importedPrefab = null;
        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            userAddedPrefab = (GameObject)ed.userObjectProp.objectReferenceValue;

            if (layer == kPostLayer)
            {
                importedPrefab = ed.resEd.HandleImportedCustomPrefab(userAddedPrefab, kPostLayer);
            }
            else if (layer == kRailALayer || layer == kRailBLayer)
            {
                importedPrefab = ed.resEd.HandleImportedCustomPrefab(userAddedPrefab, layer);
                af.keepRailGrounded[(int)layer] = false;
                af.slopeMode[(int)layer] = SlopeMode.shear;
                af.GroundRails(layer);
                // Centralize
                if (MeshUtilitiesAFWB.GetMeshSize(af.userPrefabRail[layerIndex]).y < 0.25f)
                    af.railAPositionOffset.y = 0.25f;
            }
            else if (layer == kExtraLayer)
            {
                //=============== User-Added Custom Extra ================
                //userAddedPrefab = (GameObject)ed.userObjectExtraProp.objectReferenceValue;
                importedPrefab = ed.resEd.HandleImportedCustomPrefab(userAddedPrefab, kExtraLayer);
            }
            mainPrefabChanged = true;
            importAttempted = true;
            if(importedPrefab != null)
            {
                //-- remove any transform scaling and set it to the Layer controls transdfor box instead
                //af.SetPositionTransformForLayer(importedPrefab.transform.localPosition, layer);
                //af.SetRotationTransformForLayer(importedPrefab.transform.rotation.eulerAngles, layer);
                af.SetScaleTransformForLayer(importedPrefab.transform.localScale, layer);
                
                importedPrefab.transform.localScale = Vector3.one;
                //importedPrefab.transform.localPosition = Vector3.zero;
                //importedPrefab.transform.localRotation = Quaternion.identity;

                AutoRotateImportedMesh(importedPrefab, layer, af, true);

            }
            return importedPrefab;
        }

        GUILayout.Space(10);
        //    Rotate Label
        //=======================
        EditorGUILayout.LabelField(new GUIContent("Rotate:", $"Rotates the mesh to best suit the intended use\n\n" +
            $"For example, a tall pole imported as a Rail will probably need to be rotated on its Z axis to create a horizontal fence Rail\n\n" +
            $"This is also necessary when models from Blender/Cinema4D/Max etc. have different orientation conventions.\n" +
            $"Some Asset Store models may need to be adjusted."), GUILayout.Width(41));

        //    Rotate Buttons
        //=======================
        if (GUILayout.Button(new GUIContent($"X", "Rotate on X-axis in 90 increments"), GUILayout.Width(26)))
            MeshUtilitiesAFWB.RotateMesh(ed.af.userPrefabPost, new Vector3(90, 0, 0), recalcBounds: true, recalcNormals: true);
        if (GUILayout.Button(new GUIContent($"Y", "Rotate on Y-axis in 90 increments"), GUILayout.Width(26)))
            MeshUtilitiesAFWB.RotateMesh(ed.af.userPrefabPost, new Vector3(0, 90, 0), recalcBounds: true, recalcNormals: true);
        if (GUILayout.Button(new GUIContent($"Z", "Rotate on Z-axis in 90 increments"), GUILayout.Width(26)))
            MeshUtilitiesAFWB.RotateMesh(ed.af.userPrefabPost, new Vector3(0, 0, 90), recalcBounds: true, recalcNormals: true);

        //      Adjust Mesh Button
        //===================================
        /*bool hasCustomObject = LayerHasUserObject(layer);
        EditorGUI.BeginDisabledGroup(hasCustomObject == false);
        if (GUILayout.Button(new GUIContent("Adjust Mesh...", "The custom mesh rotation & scaling can be adjusted and baked-in. " +
            "\nIf the custom object is rotated incorrectly, you can fix it by applying scaling & rotations, " +
            "either automatically or by specifying rotations to be baked in to the mesh.\n" +
            "Although you can apply rotations in the Inspector, it becomes difficult and unintuitive to apply " +
            "creative rotations when you also have to compensate for the source being in the wrong orientation." +
            "\n\nThe scale does not have to be perfect as you will shape it in the Controls section." +
            "\n\nAs AFWB works on copies, it will not affect your original mesh."),
                EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Width(90)))
        {
            ed.rotWindow = (BakeRotationsWindow)ScriptableObject.CreateInstance(typeof(BakeRotationsWindow));
            ed.rotWindow.Init(ed, layer);
            ed.rotWindow.minSize = new Vector2(690, 500); ed.rotWindow.maxSize = new Vector2(720, 550);
            ed.rotWindow.ShowUtility();
        }*/
        EditorGUI.EndDisabledGroup();
        GUILayout.Space(2);
        GUILayout.EndHorizontal();

        /*if (EditorGUI.EndChangeCheck())
        {
            //ed.serializedObject.ApplyModifiedProperties();
            if (af.autoRotateImports)
                af.railBakeRotationMode = 1;
            else
                af.railBakeRotationMode = 2;
        }*/
        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);

        return importedPrefab;

    }
    /// <summary>
    /// Automatically rotates an imported mesh based on its prefab type, bakes the transform scale, adjusts the pivot for posts and rails, and returns its effective size.
    /// </summary>
    /// <param name="go">The GameObject containing the mesh.</param>
    /// <param name="layer">The layer set to determine the prefab type.</param>
    /// <param name="af">The AutoFenceCreator instance.</param>
    /// <param name="incChildren">Whether to include children in the size calculation.</param>
    /// <returns>The effective size of the GameObject after rotation.</returns>
    public static Vector3 AutoRotateImportedMesh(GameObject go, LayerSet layer, AutoFenceCreator af, bool incChildren = true)
    {
        PrefabTypeAFWB prefabType = layer.ToPrefabType();

        //--Reset the GameObject's rotation to identity
        go.transform.rotation = Quaternion.identity;

        //--Calculate the initial effective size of the GameObject
        Vector3 effectiveSize = MeshUtilitiesAFWB.GetWorldSizeOfGameObject(go, layer, af, incChildren);

        //--Determine the necessary rotation to align the prefab
        Vector3 rotation = Vector3.zero;

        if (prefabType == PrefabTypeAFWB.postPrefab)
        {
            //--Align longest axis with y=up
            if (effectiveSize.x > effectiveSize.y && effectiveSize.x > effectiveSize.z)
            {
                rotation = new Vector3(0, 0, 90); //--Rotate around Z axis
                Debug.Log("Rotating around Z axis by 90 degrees to align the longest axis (x) with Y for postPrefab.\n");
            }
            else if (effectiveSize.z > effectiveSize.y && effectiveSize.z > effectiveSize.x)
            {
                rotation = new Vector3(90, 0, 0); //--Rotate around X axis
                Debug.Log("Rotating around X axis by 90 degrees to align the longest axis (z) with Y for postPrefab.\n");
            }
            else
                Debug.Log("No rotation needed for postPrefab as the longest axis is already aligned with Y.\n");
        }
        else if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            //--Align longest axis along the x axis
            if (effectiveSize.y > effectiveSize.x && effectiveSize.y > effectiveSize.z)
            {
                rotation = new Vector3(0, 0, 90); //--Rotate around Z axis
                Debug.Log("Rotating around Z axis by 90 degrees to align the longest axis (y) with X for railPrefab.\n");
            }
            else if (effectiveSize.z > effectiveSize.x && effectiveSize.z > effectiveSize.y)
            {
                rotation = new Vector3(90, 0, 0); //--Rotate around X axis
                Debug.Log("Rotating around X axis by 90 degrees to align the longest axis (z) with X for railPrefab.\n");
            }
            else
                Debug.Log("No rotation needed for railPrefab as the longest axis is already aligned with X.\n");
        }

        //--Apply the rotation to the mesh and its normals
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        Mesh prefabMesh = meshFilter.sharedMesh;
        MeshUtilitiesAFWB.RotateMeshAndNormals(prefabMesh, rotation, recentre: false);

        //--Bake the transform scale into the mesh
        Vector3 originalScale = go.transform.localScale;
        Vector3[] vertices = prefabMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = Vector3.Scale(vertices[i], originalScale);
        prefabMesh.vertices = vertices;
        prefabMesh.RecalculateBounds();
        prefabMesh.RecalculateNormals();

        //--Reset the GameObject's scale to Vector3.one
        go.transform.localScale = Vector3.one;

        //--Move the pivot for postPrefab to the bottom
        if (prefabType == PrefabTypeAFWB.postPrefab)
        {
            float minY = float.MaxValue;
            float offsetX = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y < minY)
                    minY = vertices[i].y;
                offsetX += vertices[i].x;
            }
            offsetX /= vertices.Length; //-- Average x-offset

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(vertices[i].x - offsetX, vertices[i].y - minY, vertices[i].z);

            prefabMesh.vertices = vertices;
            prefabMesh.RecalculateBounds();
            prefabMesh.RecalculateNormals();

            Debug.Log("Adjusted pivot to the bottom for postPrefab.\n");
        }
        //--Move the pivot for railPrefab to the right edge (max x) and center y and z, scale to 3m length
        else if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            float maxX = float.MinValue;
            float sumY = 0, sumZ = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x > maxX)
                    maxX = vertices[i].x;
                sumY += vertices[i].y;
                sumZ += vertices[i].z;
            }
            float centerY = sumY / vertices.Length;
            float centerZ = sumZ / vertices.Length;

            float length = maxX - vertices.Min(v => v.x);
            float scaleFactor = 3f / length;

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3((vertices[i].x - maxX) * scaleFactor, vertices[i].y - centerY, vertices[i].z - centerZ);

            prefabMesh.vertices = vertices;
            prefabMesh.RecalculateBounds();
            prefabMesh.RecalculateNormals();

            Debug.Log("Adjusted pivot to the right edge, centered y and z, and scaled to 3m length for railPrefab.\n");
        }

        //--Recalculate the effective size after rotation and scaling
        effectiveSize = MeshUtilitiesAFWB.GetWorldSizeOfGameObject(go, layer, af, incChildren);

        Debug.Log($"Effective size after rotation and baking scale: {effectiveSize}\n");

        return effectiveSize;
    }






    private void PostPrefabOverrides(int currMenuIndex, int numMenuNames, LayerSet layer)
    {
        if (layer == LayerSet.postLayerSet)
        {
            GUI.backgroundColor = new Color(0.96f, 0.92f, 1f);
            string mainPostName = af.GetMainPrefabNameForLayer(LayerSet.postLayerSet);
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
                    List<string> shortPrefabMenuNames = af.GetShortPrefabMenuNamesForLayer(LayerSet.postLayerSet);
                    nodePostsMenuIndexProp.intValue = EditorGUILayout.Popup("", nodePostsMenuIndexProp.intValue, shortPrefabMenuNames.ToArray(),
                        ed.lilacPopupStyle, GUILayout.Width(200));

                    //      Set to Main
                    //==============================
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("M", $"Set to the same Prefab as the Main Post:  \n" +
                        $"[   {mainPostName}   ]"), EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        nodePostsMenuIndexProp.intValue = af.GetMainPrefabMenuIndexForLayer(LayerSet.postLayerSet);
                    }


                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    ed.serializedObject.ApplyModifiedProperties(); //sets nodePostsOverrideMenuIndex to nodePostsMenuIndexProp.intValue
                    af.nodePostsOverridePrefabIndex = af.ConvertPostMenuIndexToPrefabIndex(af.nodePostsOverrideMenuIndex);
                    af.ResetPoolForLayer(LayerSet.postLayerSet);
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
                    List<string> shortPrefabMenuNames = af.GetShortPrefabMenuNamesForLayer(LayerSet.postLayerSet);
                    endsMenuIndexProp.intValue = EditorGUILayout.Popup("", endsMenuIndexProp.intValue, shortPrefabMenuNames.ToArray(), ed.lilacPopupStyle,
                        GUILayout.Width(200));

                    //      Set to Main
                    //==============================
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("M", $"Set to the same Prefab as the Main Post:  \n" +
                        $"[   {mainPostName}   ]"), EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        endsMenuIndexProp.intValue = af.GetMainPrefabMenuIndexForLayer(LayerSet.postLayerSet);
                    }

                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    ed.serializedObject.ApplyModifiedProperties();//sets endPostsOverrideMenuIndex to endsMenuIndexProp.intValue
                    af.endPostsOverridePrefabIndex = af.ConvertPostMenuIndexToPrefabIndex(af.endPostsOverrideMenuIndex);
                    af.ResetPoolForLayer(LayerSet.postLayerSet);
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

    public void SetExtraMainParameters()
    {
        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();

        EditorShowTransforms.ShowTransformEditor(LayerSet.extraLayerSet, ed);

        if (EditorGUI.EndChangeCheck())
        {
            ed.extraSizeProp.vector3Value = ed.EnforceVectorMinimums(ed.extraSizeProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
    }
    //------------------------------------
    public void SetSubpostMainParameters()
    {
        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();
        EditorShowTransforms.ShowTransformEditor(LayerSet.subpostLayerSet, ed);

        if (EditorGUI.EndChangeCheck())
        {
            ed.subpostScaleProp.vector3Value = ed.EnforceVectorMinimums(ed.subpostScaleProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
    }

    public int GetMenuIndexFromQuantizedRotAngle(float quantAngle)
    {
        int index = 0;
        switch (quantAngle)
        {
            case 30:
                index = 1; break;
            case 45:
                index = 2; break;
            case 60:
                index = 3; break;
            case 90:
                index = 4; break;
            case 120:
                index = 5; break;
            case 180:
                index = 6; break;
            case -90:
                index = 7; break;
            case -180:
                index = 8; break;
            case -1:
                index = 9; break;

            default:
                index = 0; break;
        }
        return index;
    }

}
