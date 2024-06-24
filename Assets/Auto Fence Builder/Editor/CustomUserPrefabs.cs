using AFWB;
using MeshUtils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Debug = UnityEngine.Debug; // Alias UnityEngine.Debug to Debug

public partial class PrefabAssignEditor
{
    private GameObject DragUserPrefab(LayerSet layer, ref bool mainPrefabChanged, int layerIndex)
    {
        //importAttempted = false;
        if (ed.userPrefabProp == null)
        {
            //userMeshCol = null;
            return null;
        }

        // Create a GUIStyle with a faint color
        GUILayout.Space(4);

        EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), ed.uiLineGreyCol2);

        GUI.backgroundColor = new Color(0.8f, 0.88f, 1f);
        //GameObject userAddedPrefab = af.userPrefabPost;
        //userMeshCol = null;
        //if (layer != kPostLayer)
        //userAddedPrefab = af.userPrefabRail[kRailAIndex];

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
        SerializedProperty propToDisplayInDragBox = ed.userPrefabProp;
        if (IsUserPrefabInUse(layer) == false)
            ed.userPrefabProp = ed.ResetUserPrefabPlaceholder();

        if (af)
            EditorGUILayout.PropertyField(ed.userPrefabProp, new GUIContent(""), GUILayout.Width(189));

        GameObject savedUserPrefab = null;

        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            //-- userAddedPrefab has been updated from ApplyModifiedProperties()
            GameObject userDraggedPrefab = (GameObject)ed.userPrefabProp.objectReferenceValue;
            Debug.Log($"Drag Box: userDraggedPrefab: {userDraggedPrefab}\n");

            //-- Encapsulate this to separate it from the UI code, so we can call it from other places
            savedUserPrefab = AssignUserPrefab(layer, out mainPrefabChanged, userDraggedPrefab);
            ed.ResetUserPrefabPlaceholder();

            return savedUserPrefab;
        }

        GUILayout.Space(10);
        ShowImportScaleAndRotationButtons(savedUserPrefab, layer);

        GUILayout.Space(2);
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);

        return savedUserPrefab;
    }

    public GameObject AssignUserPrefab(LayerSet layer, out bool mainPrefabChanged, GameObject userAddedPrefab)
    {
        GameObject savedUserPrefab = null;
        if (layer == kPostLayer)
        {
            savedUserPrefab = SaveUserPrefab(userAddedPrefab, kPostLayer);
        }
        else if (layer == kRailALayer || layer == kRailBLayer)
        {
            savedUserPrefab = SaveUserPrefab(userAddedPrefab, layer);
            af.keepRailGrounded[(int)layer] = false;
            af.slopeMode[(int)layer] = SlopeMode.shear;
            af.GroundRails(layer);
            // Centralize
            if (MeshUtilitiesAFWB.GetMeshSize(af.userPrefabRail[layer.Int()]).y < 0.25f)
                af.railAPositionOffset.y = 0.25f;
        }
        else if (layer == kExtraLayer)
        {
            //=============== User-Added Custom Extra ================
            //userAddedPrefab = (GameObject)ed.userPrefabExtraProp.objectReferenceValue;
            savedUserPrefab = SaveUserPrefab(userAddedPrefab, kExtraLayer);
        }
        mainPrefabChanged = true;
        //importAttempted = true;
        if (savedUserPrefab != null)
        {
            //-- remove any transform scaling and set it to the Layer controls transdfor box instead
            af.SetScaleTransformForLayer(savedUserPrefab.transform.localScale, layer);
            savedUserPrefab.transform.localScale = Vector3.one;
            AutoRotateImportedMesh(savedUserPrefab, layer, af, true);
        }
        return savedUserPrefab;
    }
    //-----------------
    /// <summary>
    /// Checks if a valid active user prefab is the same as the current prefab
    /// </summary>
    /// <returns>bool  true if active</returns>
    private bool UserPrefabIsValidAndActive(LayerSet layer)
    {
        if (af.GetMainPrefabForLayer(layer) == af.GetUserPrefabForLayer(layer))
        {
            //Debug.Log("User Prefab == current prefab\n");
            return true;
        }
        return false;
    }
    private void ShowImportScaleAndRotationButtons(GameObject userPrefab, LayerSet layer)
    {
        bool activeUserPrefab = UserPrefabIsValidAndActive(layer);

        EditorGUI.BeginDisabledGroup(activeUserPrefab == false);


        EditorGUI.BeginChangeCheck();

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


        //    Rotate Label
        //=======================
        EditorGUILayout.LabelField(new GUIContent("Rotate:", $"Rotates the mesh to best suit the intended use\n\n" +
            $"For example, a tall pole imported as a Rail will probably need to be rotated on its Z axis to create a horizontal fence Rail\n\n" +
            $"This is also necessary when models from Blender/Cinema4D/Max etc. have different orientation conventions.\n" +
            $"Some Asset Store models may need to be adjusted."), GUILayout.Width(41));

        GameObject userPrefabForLayer = af.GetUserPrefabForLayer(layer);

        //    Rotate Buttons
        //=======================
        if (GUILayout.Button(new GUIContent($"X", "Rotate on X-axis in 90 increments"), GUILayout.Width(26)))
            MeshUtilitiesAFWB.RotateMesh(userPrefabForLayer, new Vector3(90, 0, 0), recalcBounds: true, recalcNormals: true);
        if (GUILayout.Button(new GUIContent($"Y", "Rotate on Y-axis in 90 increments"), GUILayout.Width(26)))
            MeshUtilitiesAFWB.RotateMesh(userPrefabForLayer, new Vector3(0, 90, 0), recalcBounds: true, recalcNormals: true);
        if (GUILayout.Button(new GUIContent($"Z", "Rotate on Z-axis in 90 increments"), GUILayout.Width(26)))
            MeshUtilitiesAFWB.RotateMesh(userPrefabForLayer, new Vector3(0, 0, 90), recalcBounds: true, recalcNormals: true);

        if (EditorGUI.EndChangeCheck())
        {
            //-- Posts and some Extras will automatically update as their meshes have been altered
            //-- Rails will need to be updated manually, as the one we see are copies of the original. For consistencey, do them all
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
        }

        EditorGUI.EndDisabledGroup();
    }

    //-----------------------------
    /// <summary>
    /// Automatically rotates an imported mesh based on its prefab type, bakes the transform scale, adjusts the pivot for posts and rails, and returns its effective size.
    /// </summary>
    /// <param name="go">The GameObject containing the mesh.</param>
    /// <param name="layer">The sourceLayerList set to determine the prefab type.</param>
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
    //---------------------------------------
    public GameObject SaveUserPrefab(GameObject userOrigPrefab, LayerSet layer)
    {
        if (IsValidPrefab(userOrigPrefab) == false)
            return null;
        //FBXExportAFWB.SaveUserObjectAsFBX(userOrigPrefab, PrefabTypeAFWB.postPrefab, af);
        GameObject savedPrefab = PrefabMeshExporterAF.ExportMeshAndPrefab(userOrigPrefab, layer.ToPrefabType(), af);
        AssetDatabase.Refresh();
        return savedPrefab;
    }

    private static bool IsValidPrefab(GameObject userOrigPrefab)
    {
        if (userOrigPrefab == null)
        {
            Debug.LogWarning("userOrigPrefab is null\n");
            return false;
        }

        Renderer rend = userOrigPrefab.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"{userOrigPrefab.name} is not a valid prefab \n");
            return false;
        }
        return true;
    }

    //---------------------------
    /// <summary>Loads and returns a prefab from the specified asset path.</summary>
    public GameObject ReloadUserPrefab(string assetPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
            Debug.LogError($"Failed to load prefab at path: {assetPath}\n");
        return prefab;
    }
    //------------
    /// <summary>Gets a list of file names at the specified asset folder path.</summary>
    public List<string> GetAllSavedUserObjectNames()
    {
        string folderPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/User_Meshes";
        string[] filePaths = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
        List<string> fileNames = new List<string>();

        foreach (string guid in filePaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileName(path);
            fileNames.Add(fileName);
        }
        return fileNames;
    }
    //---------------------------------------
    private void AssignPrefabAsCustomObject(GameObject savedAndPreparedPrefab, LayerSet layerSet, bool willUse = true)
    {
        int layerIndex = (int)layerSet;

        if (layerSet == LayerSet.postLayer)
        {
            if (willUse)
                af.useCustomPost = true;
            af.userPrefabPost = savedAndPreparedPrefab;
        }
        else if (layerSet == LayerSet.railALayer || layerSet == LayerSet.railBLayer)
        {
            if (willUse)
                af.useCustomRail[layerIndex] = true;
            af.userPrefabRail[layerIndex] = savedAndPreparedPrefab;
        }
        else if (layerSet == LayerSet.extraLayer)
        {
            if (willUse)
                af.useCustomPost = true;
            af.userPrefabExtra = savedAndPreparedPrefab;
        }
    }
    //----------------------------------------------------------------------------------------
    //Save the user-added object in to the FencePrefabs and Meshes folders
    // will also give a unique duplicate mesh
    public static GameObject SaveUserGameObjectInUserAssetsFolder(GameObject userObject, PrefabTypeAFWB objType, AutoFenceCreator af, string prependName = "")
    {
        if (userObject == null)
            return null;
        if (af.currAutoFenceBuilderDir == null)
        {
            Debug.LogWarning("af.currAutoFenceBuilderDir is null in SaveUserObject()");
            return null;
        }
        List<Mesh> copiedMeshes = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(userObject);
        List<MeshFilter> mfList = MeshUtilitiesAFWB.GetAllMeshFiltersFromGameObject(userObject);
        string prefabString = StringUtilsTCT.GetPrefabTypeString(objType);

        //====================================
        //         Save mesh Copies
        //====================================
        string finalMeshPath = "", afwbMeshPath = "/UserAssets_AFWB/User_Meshes/";
        Mesh thisMesh;
        int incNameCounter = 0;
        //--Naming: u_GoName_i_Rail
        for (int i = 0; i < copiedMeshes.Count; i++)
        {
            thisMesh = copiedMeshes[i];
            thisMesh.name = "";
            if (thisMesh.name.StartsWith("u_") == false)
                thisMesh.name += "u_";
            //thisMesh.name += "u_";
            thisMesh.name += userObject.name;
            if (i > 0)
                thisMesh.name += "_" + i;

            mfList[i].sharedMesh = thisMesh;

            incNameCounter = 0; // because if something goes bad in the asset folder we could get stuck in an infinite loop
            finalMeshPath = af.currAutoFenceBuilderDir + afwbMeshPath + thisMesh.name + prefabString;
            while (File.Exists(finalMeshPath + ".asset") && incNameCounter++ < 100)
            {
                finalMeshPath = StringUtilsTCT.IncrementString(finalMeshPath);
            }
            finalMeshPath += ".asset";
            ResourceUtilities.SaveMeshToPath(thisMesh, finalMeshPath);
        }
        //====================================
        //         Save GameObject
        //====================================
        string objName = "";
        if (userObject.name.StartsWith(prependName) == false)
            objName += prependName;
        objName += userObject.name;
        if (objName.EndsWith(prefabString) == false)
            objName += prefabString;
        string finalPrefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs" + prefabString + "s/" + objName + ".prefab";
        incNameCounter = 0;
        while (File.Exists(finalPrefabPath + ".prefab") && incNameCounter++ < 100)
        {
            finalPrefabPath = StringUtilsTCT.IncrementString(finalPrefabPath);
        }
        GameObject prefab = ResourceUtilities.SaveGameObjectToPath(userObject, finalPrefabPath);
        //prefab.GetComponent<MeshFilter>().sharedMesh = copiedMeshes[0];

        return prefab;
    }
    //----------------------------------------------------------------------------------------
    //Save the user-added object in to the FencePrefabs and Meshes folders
    // will also give a unique duplicate mesh
    public static (GameObject, string) SaveGameObjectDuplicate(GameObject userObject, PrefabTypeAFWB prefabType, AutoFenceCreator af, string prependName = "")
    {
        if (userObject == null)
            return (null, "");
        if (af.currAutoFenceBuilderDir == null)
        {
            Debug.LogWarning("af.currAutoFenceBuilderDir is null in SaveUserObject()");
            return (null, "");
        }
        /*GameObject goDuplicate = MeshUtilitiesAFWB.CreateClonedGameObjectWithDuplicateMeshes(userObject);
        List<Mesh> copiedMeshes = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(goDuplicate);

        string prefabTypeString = StringUtilsTCT.GetPrefabTypeString(prefabType);

        //====================================
        //         Save mesh Copies
        //====================================
        //-- Note: duplicate Meshes will get renamed by Unity to mesh, mesh1, mesh2 etc. when saved.
        //-- The GameObject will overwrite the original prefab, but the mesh it references will be the latest version 'mesh2' etc
        //-- So the best way to reload the user mesh, if needed, is to get a ref from the prefab, so you know it's the latest one

        // TODO Maybe Use a Resources folder so that it can be accesed by utility classes outside the ed
        //string finalMeshPath = "", afwbMeshPath = "/UserAssets_AFWB/User_Meshes/UserMeshBackups/Resources";
        string finalMeshPath = "", finalMeshPathFBX = "", afwbMeshPath = "/UserAssets_AFWB/User_Meshes/";
        Mesh thisMesh = null;
        int incNameCounter = 0;
        //--Naming: u_GoName_i_Rail
        for (int i = 0; i < copiedMeshes.Count; i++)
        {
            thisMesh = copiedMeshes[i];
            thisMesh.name = "";
            if (prependName == "")
                prependName = "uDup_";
            if (thisMesh.name.StartsWith(prependName) == false)
                thisMesh.name += prependName;
            thisMesh.name += goDuplicate.name;
            if (thisMesh.name.Contains("(Clone)"))
                thisMesh.name = StringUtilsTCT.RemoveSubstring(thisMesh.name, "(Clone)");

            if (i > 0)
                thisMesh.name += "_" + i;

            incNameCounter = 0; // because if something goes bad in the asset folder we could get stuck in an infinite loop
            finalMeshPath = af.currAutoFenceBuilderDir + afwbMeshPath + thisMesh.name + prefabTypeString;
            while (File.Exists(finalMeshPath + ".asset") && incNameCounter++ < 100)
            {
                finalMeshPath = finalMeshPathFBX = StringUtilsTCT.IncrementString(finalMeshPath);
            }
            finalMeshPath += ".asset";
            SaveMeshToPath(thisMesh, finalMeshPath);

            //-- Save an fbx version of the mesh
            //finalMeshPathFBX += ".prefab";
            //string successPath = ModelExporter.ExportObject(finalPrefabPath + "_FBX.prefab", prefab);
            //Debug.Log("Exported Mesh: " + successPath);


        }*/
        //====================================
        //         Save GameObject
        //====================================


        //-- Note: duplicate Meshes will get renamed by Unity to mesh, mesh1, mesh2 etc. when saved.
        //-- The GameObject will overwrite the original prefab, but the mesh it references will be the latest version 'mesh2' etc
        //-- So the best way to reload the user mesh, if needed, is to get a ref from the prefab, so you know it's the latest one
        /*string objName = "";
        if (goDuplicate.name.StartsWith(prependName) == false)
            objName += prependName;
        objName += goDuplicate.name;
        if (prependName != "" && objName.Contains("(Clone)"))
            objName = StringUtilsTCT.RemoveSubstring(objName, "(Clone)");

        if (objName.EndsWith(prefabTypeString) == false)
            objName += prefabTypeString;

        string finalPrefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs" + prefabTypeString + "s/" + objName;
        incNameCounter = 0;

        GameObject prefab = SaveGameObjectToPath(goDuplicate, finalPrefabPath + ".prefab");

        AssetDatabase.Refresh();*/

        bool usingFBX = true;
        string fbxPath = "";
        if (usingFBX)
        {
            //-- Save an fbx version of the mesh
            //fbxPath = af.currAutoFenceBuilderDir + afwbMeshPath + userObject.name;
            //string successPath = ModelExporter.ExportObject(fbxPath, userObject);
            //Debug.Log($"Exported Mesh:  {successPath} \n");

            string path = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/User_Meshes/" + "dfgdfg";
            //FBXExportAFWB.SimpleExportMesh(fbxPath, thisMesh, "Helen");


            //ModelExporter.ExportObject(path, userObject);

        }

        AssetDatabase.Refresh();

        //UnityEngine.Object.DestroyImmediate(goDuplicate, false);
        //return (prefab, finalPrefabPath + ".prefab");

        return (null, fbxPath);
    }
}