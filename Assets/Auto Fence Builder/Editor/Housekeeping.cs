using AFWB;
using MeshUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TCT.PrintUtils;
using UnityEditor;
//using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/*
 * Various Development Utilities for Cleaning up the Project
 */

// Temorary parking before deciding which ones to make accesible from the UI
//
//namespace Housekeeping

public static class Housekeeping
{
    private static string targetPresetsFolder = "Assets/Auto Fence Builder/AFWB_Presets";
    private static string targetPresetsFolderBackups = "Assets/Auto Fence Builder/Editor/PresetsAFWB_Backups";
    private static string targetPresetsFolderUser = "Assets/Auto Fence Builder/UserAssets_AFWB/User_Presets";
    public static AutoFenceCreator af;
    public static AutoFenceEditor ed;

    public static void LinkAutoFenceCreatorToHousekeeping(AutoFenceCreator afc, AutoFenceEditor editor)
    {
        af = afc;
        ed = editor;
    }

    //----------------------------

    public static void ResizePrefabs(string sourcePath, Vector3 targetSize)
    {
        /*List<GameObject> prefabs = GetAllPrefabsInDirectory(sourcePath);
        foreach (GameObject originalPrefab in prefabs)
        {
            if (originalPrefab != null)
            {
                GameObject tempPrefab = GameObject.Instantiate(originalPrefab);
                MeshFilter[] meshFilters = tempPrefab.GetComponentsInChildren<MeshFilter>(true);
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    if (meshFilter.sharedMesh != null)
                    {
                        string origMeshName = meshFilter.sharedMesh.name;
                        string origMeshFilterName = meshFilter.name;
                        //Get path to the mesh
                        string meshPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                        Mesh clonedMesh = GameObject.Instantiate(meshFilter.sharedMesh);
                        MeshUtilitiesAFWB.ResizeMesh(clonedMesh, targetSize);
                        clonedMesh.name = origMeshName;
                        meshFilter.mesh = clonedMesh;  // Assign the scaled mesh back to the temporary currPrefab
                        meshFilter.name = origMeshFilterName;
                        FBXExportAFWB.SimpleExportMesh(exportPath: meshPath, mesh: clonedMesh, origMeshName: origMeshName);
                        AssetDatabase.Refresh();
                    }
                }
                // Update the currPrefab
                string prefabPath = Path.Combine(sourcePath, Path.GetFileName(sourcePath)).Replace("\\", "/");
                //PrefabUtility.SaveAsPrefabAsset(tempPrefab, prefabPath);
                UnityEngine.Object.DestroyImmediate(tempPrefab);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();*/
    }

    //================================================================================================
    //- Rename the meshes in all FBX files in the specified directory to be the same as  the file name
    //- Useage: Housekeeping.RenameFBXMeshesConsistent(af.currPostPrefabsDir);
    public static void RenameFBXMeshesConsistent(string path)
    {
        /*string[] fbxFiles = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);
        List<string> changedNames = new List<string>();

        foreach (var fbxFilePath in fbxFiles)
        {
            var importer = AssetImporter.GetAtPath(fbxFilePath) as ModelImporter;
            if (importer != null)
            {
                if (importer.materialImportMode != ModelImporterMaterialImportMode.None)
                {
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.SaveAndReimport();
                }
            }

            GameObject fbxGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(fbxFilePath);
            if (fbxGameObject != null)
            {
                // Clear materials from all renderers
                Renderer[] renderers = fbxGameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.materials = new Material[0]; // Clear any assigned materials
                }

                // Rename meshes if needed
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fbxFilePath);
                MeshFilter[] meshFilters = fbxGameObject.GetComponentsInChildren<MeshFilter>();
                string origFirstMeshName = "";
                if (meshFilters.Length > 0 && meshFilters[0] != null && meshFilters[0].sharedMesh != null)
                {
                    origFirstMeshName = meshFilters[0].sharedMesh.name;
                    // No renaming necessary
                    if (origFirstMeshName == filenameWithoutExtension)
                        continue;
                }
                else
                {
                    Debug.Log("No MeshFilters found in " + fbxFilePath);
                    continue;
                }

                int meshCounter = 0;
                foreach (var meshFilter in meshFilters)
                {
                    string newMeshName = filenameWithoutExtension;
                    if (meshFilters.Length > 1)
                    {
                        newMeshName += (meshCounter + 1).ToString();
                    }
                    meshFilter.sharedMesh.name = newMeshName;
                    changedNames.Add($"{filenameWithoutExtension}:   changed   {origFirstMeshName}   to   {newMeshName}");
                    meshCounter++;
                }

                // Export the modified GameObject back to an FBX file
                string exportPath = fbxFilePath; // Overwrite the existing file
                ModelExporter.ExportObject(exportPath, fbxGameObject);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }*/
    }

    //--------------
    // returns just the parent if it, or any children are missing
    public static List<GameObject> FindPrefabsWithMissingMesh()
    {
        List<GameObject> prefabs = GetAllPrefabsInDirectory(af.currPrefabsDir);

        List<GameObject> missingMeshPrefabs = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            List<GameObject> goList = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(prefab);
            bool missing = false;
            foreach (GameObject go in goList)
            {
                if (missing == true)
                    break;
                MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    if (meshFilter.sharedMesh == null)
                    {
                        missingMeshPrefabs.Add(prefab);
                        missing = true;
                        break;
                    }
                }
            }
        }
        return missingMeshPrefabs;
    }
    public static List<GameObject> FindUnusedPrefabsInPresets(bool print = true)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        List<GameObject> prefabs = GetAllPrefabsInDirectory(af.currPrefabsDir);
        HashSet<string> usedPrefabNames = new HashSet<string>();

        // Collect all used prefab names
        foreach (ScriptablePresetAFWB preset in presetList)
        {
            usedPrefabNames.Add(preset.postName);
            usedPrefabNames.Add(preset.railAName);
            usedPrefabNames.Add(preset.railBName);
            usedPrefabNames.Add(preset.extraName);

            preset.postVariants.ForEach(variant => usedPrefabNames.Add(variant.Go.name));
            preset.railAVariants.ForEach(variant => usedPrefabNames.Add(variant.Go.name));
            preset.railBVariants.ForEach(variant => usedPrefabNames.Add(variant.Go.name));
        }

        // Filter out unused prefabs
        List<GameObject> unusedPrefabs = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            if (!usedPrefabNames.Contains(prefab.name))
            {
                unusedPrefabs.Add(prefab);
                string prefabPath = AssetDatabase.GetAssetPath(prefab);
                Debug.Log($"{prefab.name}  at  {prefabPath}\n");
            }
        }
        Debug.Log($"Found {unusedPrefabs.Count} unused prefabs of {prefabs.Count} total prefabs. in  {presetList.Count} presets\n");

        //PrintUtilities.PrintList(unusedPrefabs, "Unused Prefabs", print, allInOneLine:false);

        return unusedPrefabs;
    }

    //--------------
    public static List<ProblemPreset> FindPresetsWithMissingMesh()
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        List<ProblemPreset> problemPresets = new List<ProblemPreset>();

        List<GameObject> prefabs = GetAllPrefabsInDirectory(af.currPrefabsDir);

        // For each Preset
        for (int i = 0; i < presetList.Count; i++)
        {
            ScriptablePresetAFWB preset = presetList[i];
            // Get the names of all GameObjects associated with this preset
            GameObjectFields presetGameObjectNames = PresetChecker.ExtractGameObjectFieldsFromPreset(preset);

            string postName = presetGameObjectNames.PostName;
            string railAName = presetGameObjectNames.RailAName;
            string railBName = presetGameObjectNames.RailBName;
            string subPostName = presetGameObjectNames.SubPostName;
            string extraName = presetGameObjectNames.ExtraName;

            List<string> postSourceVariants = presetGameObjectNames.PostSourceVariants;
            List<string> railASourceVariants = presetGameObjectNames.RailASourceVariants;
            List<string> railBSourceVariants = presetGameObjectNames.RailBSourceVariants;

            GameObject post = prefabs.Find(prefab => prefab.name == postName);
            if (post != null)
            {
                //Check if the GameObject has a MeshFilter and Mesh
                if (post.GetComponent<MeshFilter>() == null || post.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    ProblemPreset problemPreset = new ProblemPreset();
                    problemPreset.PresetName = preset.name;
                    problemPreset.ProblemGameObject = "Post";
                    problemPreset.ProblemDescriptionMessage = "Post GameObject is missing Mesh";
                    problemPresets.Add(problemPreset);
                }
            }
            else
            {
                ProblemPreset problemPreset = new ProblemPreset();
                problemPreset.PresetName = preset.name;
                problemPreset.ProblemGameObject = "Post";
                problemPreset.ProblemDescriptionMessage = "Post GameObject not found";
                problemPresets.Add(problemPreset);
                Debug.Log($"Post GameObject not found for {preset.categoryName} / {preset.name}");
            }
            //---
            GameObject railA = prefabs.Find(prefab => prefab.name == railAName);
            if (railA != null)
            {
                //Check if the GameObject has a MeshFilter and Mesh
                if (railA.GetComponent<MeshFilter>() == null || railA.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    ProblemPreset problemPreset = new ProblemPreset();
                    problemPreset.PresetName = preset.name;
                    problemPreset.ProblemGameObject = "RailA";
                    problemPreset.ProblemDescriptionMessage = "RailA GameObject is missing Mesh";
                    problemPresets.Add(problemPreset);
                }
            }
            else
            {
                ProblemPreset problemPreset = new ProblemPreset();
                problemPreset.PresetName = preset.name;
                problemPreset.ProblemGameObject = "RailA";
                problemPreset.ProblemDescriptionMessage = "RailA GameObject not found";
                problemPresets.Add(problemPreset);
                Debug.Log($"RailA GameObject not found for {preset.categoryName} / {preset.name}");
            }
            //---
            GameObject railB = prefabs.Find(prefab => prefab.name == railBName);
            if (railB != null)
            {
                //Check if the GameObject has a MeshFilter and Mesh
                if (railB.GetComponent<MeshFilter>() == null || railB.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    ProblemPreset problemPreset = new ProblemPreset();
                    problemPreset.PresetName = preset.name;
                    problemPreset.ProblemGameObject = "RailB";
                    problemPreset.ProblemDescriptionMessage = "RailB GameObject is missing Mesh";
                    problemPresets.Add(problemPreset);
                }
            }
            else
            {
                ProblemPreset problemPreset = new ProblemPreset();
                problemPreset.PresetName = preset.name;
                problemPreset.ProblemGameObject = "RailB";
                problemPreset.ProblemDescriptionMessage = "RailB GameObject not found";
                problemPresets.Add(problemPreset);
                Debug.Log($"RailB GameObject not found for {preset.categoryName} / {preset.name}");
            }
            //---
            GameObject subPost = prefabs.Find(prefab => prefab.name == subPostName);
            if (subPost != null)
            {
                //Check if the GameObject has a MeshFilter and Mesh
                if (subPost.GetComponent<MeshFilter>() == null || subPost.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    ProblemPreset problemPreset = new ProblemPreset();
                    problemPreset.PresetName = preset.name;
                    problemPreset.ProblemGameObject = "SubPost";
                    problemPreset.ProblemDescriptionMessage = "SubPost GameObject is missing Mesh";
                    problemPresets.Add(problemPreset);
                }
            }
            else
            {
                ProblemPreset problemPreset = new ProblemPreset();
                problemPreset.PresetName = preset.name;
                problemPreset.ProblemGameObject = "SubPost";
                problemPreset.ProblemDescriptionMessage = "SubPost GameObject not found";
                problemPresets.Add(problemPreset);
                Debug.Log($"SubPost GameObject not found for {preset.categoryName} / {preset.name}");
            }
            //---
            GameObject extra = prefabs.Find(prefab => prefab.name == extraName);
            if (extra != null)
            {
                //Check if the GameObject has a MeshFilter and Mesh
                if (extra.GetComponent<MeshFilter>() == null || extra.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    ProblemPreset problemPreset = new ProblemPreset();
                    problemPreset.PresetName = preset.name;
                    problemPreset.ProblemGameObject = "Extra";
                    problemPreset.ProblemDescriptionMessage = "Extra GameObject is missing Mesh";
                    problemPresets.Add(problemPreset);
                }
            }
            else
            {
                ProblemPreset problemPreset = new ProblemPreset();
                problemPreset.PresetName = preset.name;
                problemPreset.ProblemGameObject = "Extra";
                problemPreset.ProblemDescriptionMessage = "Extra GameObject not found";
                problemPresets.Add(problemPreset);
                Debug.Log($"The Mesh on Extra GameObject  {extraName}   not found for {preset.categoryName} / {preset.name}");
            }
            //---
        }
        return problemPresets;
    }

    //================================================================================================
    //Write the name of each currPrefab and its asociated fbx mesh file to  a text file: PrefabMeshNames.txt
    //Format is: currPrefabName, meshFBXName
    public static void WritePrefabMeshNamesToTextFile(List<GameObject> prefabs)
    {
        // Define the path of the text file within the Unity Assets directory
        string filePath = Application.dataPath + "/Auto Fence Builder/Editor/PrefabMeshNames.txt";

        // Use StringBuilder for efficient string concatenation in a loop
        StringBuilder sb = new StringBuilder();

        // Loop through each currPrefab in the list
        foreach (GameObject prefab in prefabs)
        {
            string meshName = GetFirstMeshName(prefab);
            string sourceFBXFilePath = GetFBXSourcePath(prefab);
            string sourceFBXFileName = Path.GetFileNameWithoutExtension(sourceFBXFilePath);
            // Append the currPrefab's name and the mesh's name to the StringBuilder
            sb.AppendLine(prefab.name + "," + sourceFBXFileName);
        }
        // Write the concatenated string to the file
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log("File written to " + filePath);
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to show the new file in the Unity Editor
    }

    //================================================================================================
    //Write the name of each currPrefab and its main mesh to a text file: PrefabMeshNames.txt
    //Format is: currPrefabName, meshFBXName
    public static void AssignMeshToPrefabUsingTextFile(List<GameObject> allPrefabs, List<GameObject> allMeshes)
    {
        // Define the path of the text file within the Unity Assets directory
        string filePath = Application.dataPath + "/Auto Fence Builder/Editor/PrefabMeshNames.txt";

        //open the text file and read the lines
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length >= 2)
            {
                string currPrefabName = parts[0].Trim();
                string meshFBXName = parts[1].Trim();
                GameObject currPrefab = allPrefabs.Find(prefab => prefab.name == currPrefabName);
                if (currPrefab != null)
                {
                    MeshFilter[] currPrefabMeshFilters = currPrefab.GetComponentsInChildren<MeshFilter>();
                    if (currPrefabMeshFilters.Length == 0)
                    {
                        currPrefab.AddComponent<MeshFilter>(); //-- No MeshFilter found, add one to the currPrefab
                        currPrefabMeshFilters = currPrefab.GetComponentsInChildren<MeshFilter>();
                    }
                    foreach (MeshFilter currPrefabMeshFilter in currPrefabMeshFilters)
                    {
                        if (currPrefabMeshFilter != null)
                        {
                            //Find the mesh in allMeshes with the same name
                            //m: Represents an individual mesh object from allMeshes during each iteration.
                            //m.name == meshFBXName: the condition checked for each mesh m.If a mesh's name equals meshFBXName, the condition evaluates to true.
                            GameObject meshGO = MeshUtilitiesAFWB.FindFirstGameObjectWithMeshName(allMeshes, meshFBXName);
                            if (meshGO != null)
                            {
                                Mesh foundMesh = meshGO.GetComponent<MeshFilter>().sharedMesh;
                                currPrefabMeshFilter.sharedMesh = foundMesh;
                                Debug.Log("Assigned mesh " + meshFBXName + " to Prefab " + currPrefabName);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"AssignMeshToPrefabUsingTextFile()    Couldn't Find Prefab with Name: {currPrefabName} \n");
                }
            }
        }
    }

    //------------------------------------------------
    // Someimes the mesh within an fbx file may not have the same name as the fbx file
    // In this case we find the source fbx file that contained the mesh
    //Useage : Housekeeping.GetFBXSourcePath(myGameObject);
    public static string GetFBXSourcePath(GameObject go)
    {
        // Check for a MeshFilter component and get the mesh path
        MeshFilter meshFilter = go.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
        }

        return null; // Return null if no MeshFilter component is found
    }

    //------------------------------------------------------------
    // Helper method to get the name of the first MeshRenderer's mesh in a GameObject
    private static string GetFirstMeshName(GameObject go)
    {
        // Get the MeshRenderer component; you might also consider MeshFilter if you need the mesh data
        MeshRenderer renderer = go.GetComponentInChildren<MeshRenderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            // Assuming the mesh is attached to the same GameObject as the MeshRenderer
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                return meshFilter.sharedMesh.name;
            }
        }
        return "No Mesh Found"; // Return a default string if no mesh is found
    }

    //================================================================================================
    public static Dictionary<string, string> LoadPrefabMeshMappings(string filePath)
    {
        Dictionary<string, string> prefabMeshMap = new Dictionary<string, string>();

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    string prefabName = parts[0].Trim();
                    string meshName = parts[1].Trim();
                    if (!prefabMeshMap.ContainsKey(prefabName))
                    {
                        prefabMeshMap.Add(prefabName, meshName);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
        return prefabMeshMap;
    }

    public static void CheckPrefabs(List<GameObject> prefabs, Dictionary<string, string> prefabMeshMap)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefabMeshMap.TryGetValue(prefab.name, out string meshName))
            {
                Debug.Log("Prefab: " + prefab.name + ", Mesh: " + meshName + "\n");
            }
            else
            {
                Debug.Log("Prefab: " + prefab.name + " not found in the text file.\n");
            }
        }
    }

    //================================================================================================

    public static void RenamePrefabAsset(string oldName, string newName)
    {
        string[] guids = AssetDatabase.FindAssets(oldName + " t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name == oldName)
            {
                AssetDatabase.RenameAsset(path, newName);
                AssetDatabase.SaveAssets();
                Debug.Log("Prefab renamed from " + oldName + " to " + newName + "\n");
                return; // Remove this if you want to rename all allPrefabs with the same old name
            }
        }
        Debug.LogWarning("Prefab with name '" + oldName + "' not foundPrefabs.");
    }

    // directoryPath is relative to the project's Assets folder
    // Useage: FindPrefabWithName("MyPrefab", af.currAutoFenceBuilderDir)
    public static List<GameObject> FindPrefabWithName(string name, string directoryPath)
    {
        List<GameObject> found = new List<GameObject>();
        // Ensure the directory path ends with a slash to correctly filter by directory
        if (!directoryPath.EndsWith("/"))
        {
            directoryPath += "/";
        }

        // Filter assets by name and type, and ensure they are within the specified directory including subdirectories
        string filter = name + " t:Prefab";
        string[] guids = AssetDatabase.FindAssets(filter, new[] { directoryPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                found.Add(prefab);
            }
        }
        return found;
    }

    //--------------------------------------------------------------------------------
    public static List<GameObject> FindPrefabsUsingMaterialName(string matName)
    {
        if (string.IsNullOrEmpty(matName)) // Check if the material name provided is null or empty
        {
            return new List<GameObject>(); // Return an empty list if no valid material name is provided
        }

        List<GameObject> found = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab"); // This searches all assets that are allPrefabs
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) // Check if the currPrefab is not null
            {
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null) // Check if renderer is not null
                    {
                        Material[] materials = renderer.sharedMaterials; // Get shared materials to avoid gizmoSingletonInstance-specific materials
                        if (materials != null) // Check if materials array is not null
                        {
                            foreach (Material mat in materials)
                            {
                                if (mat != null && mat.name == matName) // Check if material is not null and names match
                                {
                                    if (!found.Contains(prefab)) // Check if the currPrefab has already been added to avoid duplicates
                                    {
                                        found.Add(prefab);
                                        break; // Break the innermost loop to stop checking once a match is foundPrefabs
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return found; // Return the list of foundPrefabs allPrefabs
    }
    //--------------------------------------------------------------------------------
    /// <summary>
    /// Loads each prefab of layer, and looks at main texture for a match
    /// </summary>
    /// <param name="texName"></param>
    /// <returns>List<GameObject> using the named Texture</returns>
    public static List<GameObject> FindPrefabsUsingTextureName(string texName)
    {
        if (string.IsNullOrEmpty(texName)) // Check if the material name provided is null or empty
            return new List<GameObject>(); // Return an empty list if no valid material name is provided

        List<GameObject> found = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab"); // This searches all assets that are prefabs
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) // Check if the currPrefab is not null
            {
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null) // Check if renderer is not null
                    {
                        Texture tex;
                        Material[] materials = renderer.sharedMaterials; // Get shared materials to avoid gizmoSingletonInstance-specific materials
                        if (materials != null) // Check if materials array is not null
                        {
                            foreach (Material mat in materials)
                            {
                                if (mat == null)
                                {
                                    Debug.Log($" - - - - - Material on {prefab.name}is null\n");
                                    continue;
                                }
                                if (mat.HasProperty("_MainTex"))
                                {
                                    tex = mat.mainTexture;
                                    if (tex == null)
                                    {
                                        Debug.Log($" -  {mat.name} on {prefab.name} has Property _MainTex but does not have a main texture assigned.\n");
                                    }
                                    else
                                    {
                                        Debug.Log($" {mat.name} on {prefab.name} has Property _MainTex has a main texture assigned.\n");
                                    }
                                }
                                else
                                {
                                    Debug.Log($" - -  {mat.name} on {prefab.name} does not have a '_MainTex' property.\n");
                                }

                                /*tex = mat.mainTexture;
                                if (tex != null && tex.name == texName) // Check if material is not null and names match
                                {
                                    if (!found.Contains(prefab)) // Check if the currPrefab has already been added to avoid duplicates
                                    {
                                        found.Add(prefab);
                                        break; // Break the innermost loop to stop checking once a match is foundPrefabs
                                    }
                                }*/
                            }
                        }
                    }
                }
            }
        }
        return found; // Return the list of foundPrefabs allPrefabs
    }


    public static List<Material> ShowIssuesWithAllMaterials(bool limitToAFWB)
    {
        List<Material> found = new List<Material>();
        List<GameObject> prefabsUsingProblemMat = new List<GameObject>();
        List<string> problemStrings = new List<string>();
        string[] guids;
        if (limitToAFWB == false)
            guids = AssetDatabase.FindAssets("t:Material"); // This searches all assets that are prefabs
        else
            guids = AssetDatabase.FindAssets("t:Material", new[] { af.currMaterialsDir }); // This searches all assets that are prefabs

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            prefabsUsingProblemMat.Clear();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            string problemStr = "";
            if (mat != null)
            {
                Texture tex;
                if (mat.HasProperty("_MainTex"))
                {
                    tex = mat.mainTexture;
                    if (tex == null)
                    {
                        problemStr = $"{mat.name}\nhas Property _MainTex but no main texture assigned.";
                        found.Add(mat);
                        prefabsUsingProblemMat.AddRange(FindPrefabsUsingMaterialName(mat.name));
                    }
                }
                else
                {
                    problemStr = $"{mat.name}\ndoes NOT have '_MainTex' property.";

                    //-- Uncomment to see all channel properties
                    /*problemStr += "     Properties:";
                    for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(mat.shader, i);
                        problemStr += $" {propertyName}";
                    }*/
                    found.Add(mat);
                    prefabsUsingProblemMat.AddRange(FindPrefabsUsingMaterialName(mat.name));
                }
                if (problemStr != "" && prefabsUsingProblemMat.Count > 0)
                {
                    // Create a string of the names of all prefabs using the problematic material
                    string prefabsString = string.Join(",  ", prefabsUsingProblemMat.Select(obj => obj.name));
                    problemStr += $"   ** Used By: {prefabsString}";
                }
                else if (problemStr != "" && prefabsUsingProblemMat.Count == 0)
                    problemStr += "   [Not Used]";

                if (problemStr != "")
                    problemStrings.Add(problemStr);

            }
        }

        //-- Sort strins by containing "Not Used" 
        var sortedStrings = problemStrings
            .OrderBy(s => !s.Contains("Not Used")) // First, prioritize "Not Used"
            .ThenBy(s => s) // Then, sort alphabetically within each group
            .ToList();

        foreach (string problemStr in sortedStrings)
            Debug.Log(problemStr);

        return found;
    }
    //---------------------------------------------------
    public static List<Material> ShowMaterialsUsingShaderName(string shaderName, bool limitToAFWB, bool printAllNonStandard = false)
    {
        List<Material> materialsUsingShader = new List<Material>();
        string[] guids;

        // Define the search directory
        string searchDirectory = "Assets/Auto Fence Builder";

        // Determine the scope of the search
        if (limitToAFWB)
            guids = AssetDatabase.FindAssets("t:Material", new[] { searchDirectory });
        else
            guids = AssetDatabase.FindAssets("t:Material");

        // Iterate through each found material GUID
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader matShader = null;
            if (mat != null)
                matShader = mat.shader;

            //-- Print all non-standard shaders
            if (printAllNonStandard == true && matShader != null && matShader.name != "Standard" && matShader.name.Contains("GUI") == false)
            {
                Debug.Log($"{mat.name} uses    Shader:  {matShader.name} \n");
                // Check if the shader is within the search directory
                string shaderPath = AssetDatabase.GetAssetPath(matShader);
                if (!shaderPath.StartsWith(searchDirectory))
                {
                    Debug.LogWarning($"{mat.name} uses a non-standard shader located outside of AFWB dir:\n {shaderPath}");
                }
            }


            if (mat != null && matShader != null && matShader.name == shaderName)
            {
                materialsUsingShader.Add(mat);
            }
        }

        return materialsUsingShader;
    }


    //--------------------------------------------------------------------------------
    public static List<GameObject> FindPrefabsUsingMeshName(string meshName)
    {
        if (string.IsNullOrEmpty(meshName)) // Check if the meshName name provided is null or empty
        {
            return new List<GameObject>(); // Return an empty list if no valid meshName name is provided
        }
        List<GameObject> found = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab"); // This searches all assets that are allPrefabs
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) // Check if the currPrefab is not null
            {
                MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    if (meshFilter != null) // Check if MeshFilter is not null
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        if (mesh != null)

                            if (mesh.name == meshName)
                            {
                                if (!found.Contains(prefab)) // Check if the currPrefab has already been added to avoid duplicates
                                {
                                    found.Add(prefab);
                                    break; // Break the innermost loop to stop checking once a match is foundPrefabs
                                }
                            }
                    }
                }
            }
        }
        return found; // Return the list of foundPrefabs allPrefabs
    }

    //--------------------------------------------------------------------------------
    public static List<GameObject> GetAllPrefabsInDirectory(string directoryPath)
    {
        List<GameObject> foundPrefabs = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("", new[] { directoryPath }); //Find All (="") at the directoryPath

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) // Check if the currPrefab is not null
            {
                foundPrefabs.Add(prefab);
            }
        }
        return foundPrefabs; // Return the list of foundPrefabs allPrefabs
    }

    //--------------------------------------------------------------------------------
    // They are loaded as GameObjects, not as Models or Meshes as Unity Meshes can not have children
    // Loading as a gameobject creates a nested gameobject hierarchy with the meshes as children gos
    public static List<GameObject> GetAllMeshesInDirectory(string meshDirectoryPath)
    {
        List<GameObject> foundMeshes = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("", new[] { meshDirectoryPath }); //Find All (="") at the directoryPath

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject mesh = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (mesh != null) // Check if the mesh is not null
            {
                foundMeshes.Add(mesh);
            }
        }
        return foundMeshes; // Return the list of foundPrefabs allPrefabs
    }

    //--------------------------
    // directoryPath is relative to the project's Assets folder
    // Useage: Housekeeping.FindPrefabsWithMeshSmallerThan(2f, af.currAutoFenceBuilderDir, "Y")
    // or Housekeeping.FindPrefabsWithMeshSmallerThan(2f, "Assets/Auto Fence Builder/AFWB_Prefabs/_Posts_AFWB/Test", "Y");
    public static List<GameObject> FindPrefabsWithMeshSmallerThan(float prefabSize, string directoryPath, string dimension = "Y")
    {
        // Check if the directory exists
        /*bool exists = CheckIfDirectoryExists(directoryPath);
        if (exists)
            Debug.Log("Directory exists: " + directoryPath);
        else
            Debug.Log("Directory does not exist: " + directoryPath);*/

        List<GameObject> found = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("", new[] { directoryPath }); //Find All (="") at the directoryPath

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) // Check if the currPrefab is not null
            {
                Vector3 totalSize = MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(prefab);
                //Can happen if there is no mesh in the currPrefab
                if (totalSize == Vector3.zero)
                    continue;

                dimension = dimension.ToUpper(); // Ensure the dimension parameter is case-insensitive
                if ((dimension == "X" && totalSize.x < prefabSize) ||
                    (dimension == "Y" && totalSize.y < prefabSize) ||
                    (dimension == "Z" && totalSize.z < prefabSize))
                {
                    found.Add(prefab);
                }
            }
        }
        return found; // Return the list of foundPrefabs allPrefabs
    }

    public static bool CheckIfDirectoryExists(string relativePath)
    {
        // Combine the application data path with the relative path to form an absolute path
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        return Directory.Exists(fullPath);
    }

    //------------------------------------------------------
    // directoryPath is relative to the project's Assets folder
    // Meshes in Unity do not contain children, instead, when added to the hierarchy (or made into a currPrefab)
    // the fbx file acts as a folder or model container, which becomes an empty parent with the meshes as children
    // Likewise, a nested mesh hierarchy in an fbx will be translated as a nested hierarchy of GameObjects, all under ann empty parent
    // Also,
    // Use "t:GameObject" in AssetDatabase.FindAssets to find FBX files because Unity does not directly recognize "t:Model" as a valid type.
    // FBX files are imported into Unity as GameObjects, not as a distinct "Model" type. After import, FBX files are represented in Unity as GameObjects,
    // which can include meshes, materials, and other components. Since "t:Model" is not a valid type filter in Unity's asset system,
    // we must use "t:GameObject" and then filter the results by file extension (.fbx) to specifically identify and work with FBX model files.

    public static List<GameObject> FindModelsWithCombinedMeshesSmallerThan(float searchSize, string directoryPath, string dimension = "Y")
    {
        List<GameObject> found = new List<GameObject>();
        // Search for all GameObjects, because Unity does not distinguish FBX files directly in FindAssets
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { directoryPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Check if the asset path ends with ".fbx" to ensure it's an FBX file
            if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    Vector3 totalSize = MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(go, false);
                    dimension = dimension.ToUpper();
                    if ((dimension == "X" && totalSize.x < searchSize) ||
                        (dimension == "Y" && totalSize.y < searchSize) ||
                        (dimension == "Z" && totalSize.z < searchSize))
                    {
                        found.Add(go);
                    }
                }
            }
        }
        return found;
    }

    //----------
    // Useage: GameObject myFBX = LoadFBXFromPath("Assets/Models/MyModel.fbx");
    public static GameObject LoadFBXFromPath(string filePath)
    {
        // Check if the file path ends with the .fbx extension to ensure it is an FBX file
        if (filePath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
        {
            // Load the asset at the specified path
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
            return go;
        }
        return null; // Return null if the file is not an FBX
    }

    public static List<GameObject> FindPrefabsContainingName(string name)
    {
        List<GameObject> foundPrefabs = new List<GameObject>();
        // Search for all allPrefabs but filter by name in the loop
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.Contains(name))
            {
                foundPrefabs.Add(prefab);
            }
        }
        return foundPrefabs;
    }

    //--------------------------------------------------------------------------

    /// <summary>
    /// Searches for allPrefabs within a specified directory whose paths contain a specified name,
    /// and optionally returns only the currPrefab names without paths. Allows for case-sensitive or case-insensitive searches.
    /// </summary>
    /// <param name="directoryPath">The directory path within which to search for allPrefabs. Use relative path from the project's Assets folder.</param>
    /// <param name="name">The substring to search for in the currPrefab paths.</param>
    /// <param name="returnNamesOnly">If true, returns only the names of the allPrefabs without the file paths. Defaults to true.</param>
    /// <param name="caseSensitive">Determines whether the search should be case-sensitive. Default is true.</param>
    /// <returns>A list of strings containing either full paths or just names of the allPrefabs that match the search criterion.</returns>
    public static List<string> FindPrefabPathsContainingName(string directoryPath, string name, bool returnNamesOnly = true, bool caseSensitive = true)
    {
        List<string> foundPrefabPaths = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Normalize the directory and path for comparison to ignore leading/trailing slashes and case if necessary
            string normalizedDirectoryPath = directoryPath.Trim().Replace("\\", "/");
            string normalizedPath = path.Replace("\\", "/");

            if (normalizedPath.StartsWith(normalizedDirectoryPath, System.StringComparison.OrdinalIgnoreCase)) // Always use ordinal ignore case for directory check
            {
                // Perform a case-sensitive or insensitive check on the name within the path
                bool nameMatches = caseSensitive ?
                                    normalizedPath.Contains(name) :
                                    normalizedPath.ToLowerInvariant().Contains(name.ToLowerInvariant());

                if (nameMatches)
                {
                    if (returnNamesOnly)
                        path = System.IO.Path.GetFileNameWithoutExtension(path);

                    foundPrefabPaths.Add(path);
                }
            }
        }
        return foundPrefabPaths;
    }

    //-------------
    // Finds all occurence of a GameObject in all presetsEd, of all Layer types
    public static void PrintPresetsUsingGameObject(string goName)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;

        GameObject railPrefabWithname = af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, goName, warnMissing: false, returnMissingDefault: false);
        GameObject postPrefabWithname = af.FindPrefabByNameAndType(PrefabTypeAFWB.postPrefab, goName, warnMissing: false, returnMissingDefault: false);
        GameObject extraPrefabWithname = af.FindPrefabByNameAndType(PrefabTypeAFWB.extraPrefab, goName, warnMissing: false, returnMissingDefault: false);

        //-- Rails
        List<List<string>> railPresets = Housekeeping.FindPresetsUsingGameObject(railPrefabWithname);
        //-- Posts
        List<List<string>> postPresets = Housekeeping.FindPresetsUsingGameObject(postPrefabWithname);
        //-- Extras
        List<List<string>> extraPresets = Housekeeping.FindPresetsUsingGameObject(extraPrefabWithname);

        int totalRailPresets = railPresets.Sum(list => list.Count);
        int totalPostPresets = postPresets.Sum(list => list.Count);
        int totalExtraPresets = extraPresets.Sum(list => list.Count);
        int total = totalRailPresets + totalPostPresets + totalExtraPresets;

        Debug.Log($"Total Found = {total}      ( Posts[{totalPostPresets}],  Rails[{totalRailPresets}],  Extras[{totalExtraPresets}] )");

        PrettyPrintGameObjectPresets(postPresets, $"Posts: [{totalPostPresets}]");
        PrettyPrintGameObjectPresets(railPresets, $"Rails: [{totalRailPresets}]");
        PrettyPrintGameObjectPresets(extraPresets, $"Extras: [{totalExtraPresets}]");
    }
    public static void PrintPresetsUsingGameObjectSimple(string goName)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;

        List<string> foundPresets = Housekeeping.FindPresetsUsingGameObjectSimple(goName);

        int totalFoundPresets = foundPresets.Count;

        Debug.Log($"Total Found = {totalFoundPresets}  ");

        if (totalFoundPresets > 0)
            PrintUtilities.PrintList(foundPresets, allInOneLine:false);
    }

    public static void PrintPresetsUsingGameObjectFromContextMenu(string prefabName)
    {
        //Find AutoFenceCreator
        AutoFenceCreator af = GameObject.FindObjectOfType<AutoFenceCreator>();
        if (af == null)
        {
            Debug.LogError("AutoFenceCreator not found in the scene.");
            return;
        }

        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;

        GameObject railGo = af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, prefabName, warnMissing: false, returnMissingDefault: false);
        GameObject postGo = af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, prefabName, warnMissing: false, returnMissingDefault: false);
        GameObject extraGo = af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, prefabName, warnMissing: false, returnMissingDefault: false);

        //-- Rails
        List<List<string>> railPresets = Housekeeping.FindPresetsUsingGameObject(railGo);
        //-- Posts
        List<List<string>> postPresets = Housekeeping.FindPresetsUsingGameObject(postGo);
        //-- Extras
        List<List<string>> extraPresets = Housekeeping.FindPresetsUsingGameObject(extraGo);

        af.ClearConsole();

        int totalRailPresets = railPresets.Sum(list => list.Count);
        int totalPostPresets = postPresets.Sum(list => list.Count);
        int totalExtraPresets = extraPresets.Sum(list => list.Count);
        int total = totalRailPresets + totalPostPresets + totalExtraPresets;

        Debug.Log($"Total Found = {total}      ( Posts[{totalPostPresets}],  Rails[{totalRailPresets}],  Extras[{totalExtraPresets}] )\n" +
            $"----------------------------------");

        if (totalPostPresets > 0)
            PrettyPrintGameObjectPresets(postPresets, $"        Posts: [{totalPostPresets}]");
        if (totalRailPresets > 0)
            PrettyPrintGameObjectPresets(railPresets, $"        Rails: [{totalRailPresets}]");
        if (totalExtraPresets > 0)
            PrettyPrintGameObjectPresets(extraPresets, $"       Extras: [{totalExtraPresets}]");
    }



    //-----------------
    public static void PrettyPrintGameObjectPresets(List<List<string>> prestsUsingGameObject, string msg = "")
    {
        if (msg != "")
            Debug.Log(msg + "\n");
        if (prestsUsingGameObject == null || prestsUsingGameObject.Count == 0)
        {
            Debug.Log("No GameObject Presets to display.\n" + "----------\n");
            return;
        }

        foreach (var gameObjectPreset in prestsUsingGameObject)
        {
            foreach (var presetInfo in gameObjectPreset)
            {
                Debug.Log(presetInfo);
            }
            Debug.Log("----------\n"); // Separator for readability
        }
    }

    //----------------------------------------------------
    public static List<string> GetAllFBXPaths(string sourcePath)
    {
        string directoryPath = sourcePath; // Editor path relative to the project folder
        List<string> filePaths = new List<string>(); // List to hold paths

        // Get all FBX files in the directory using the search filter for Model type assets
        string[] fileGUIDs = AssetDatabase.FindAssets("t:Model", new[] { directoryPath });

        foreach (string guid in fileGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            filePaths.Add(path); // Add the foundPrefabs file path to the list
        }

        Debug.Log("Collected FBX file paths: \n");
        foreach (string path in filePaths)
        {
            Debug.Log(path + "\n");
        }
        return filePaths;
    }

    // Get all FBX files in the directory and return a list of Mesh objects
    // Assumes each fbx is a single mesh
    public static List<Mesh> GetAllFBX(string sourcePath)
    {
        List<Mesh> meshes = new List<Mesh>();
        // Get all FBX files in the directory using the search filter for Model type assets
        string[] fileGUIDs = AssetDatabase.FindAssets("t:Model", new[] { sourcePath });
        foreach (string guid in fileGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.EndsWith(".fbx"))
            {
                GameObject fbxGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (fbxGo != null)
                {
                    // Retrieve the MeshFilter component and get the Mesh from it
                    MeshFilter meshFilter = fbxGo.GetComponentInChildren<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        meshes.Add(meshFilter.sharedMesh);
                    }

                    // Additionally check for SkinnedMeshRenderer if it's a skinned mesh
                    SkinnedMeshRenderer skinnedMeshRenderer = fbxGo.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                    {
                        meshes.Add(skinnedMeshRenderer.sharedMesh);
                    }
                }

                Debug.Log($"Loaded mesh from: {path} \n");
            }
        }
        return meshes;
    }

    //--------------------------
    // same as above but will get all meshes in the fbx
    public static List<List<Mesh>> GetAllFBXMeshes(string sourcePath)
    {
        List<List<Mesh>> allMeshes = new List<List<Mesh>>();

        // Get all FBX files in the directory using the search filter for Model type assets
        string[] fileGUIDs = AssetDatabase.FindAssets("t:Model", new[] { sourcePath });
        foreach (string guid in fileGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.EndsWith(".fbx"))
            {
                GameObject fbxGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (fbxGo != null)
                {
                    List<Mesh> meshes = new List<Mesh>();

                    // Collect all MeshFilter meshes
                    MeshFilter[] meshFilters = fbxGo.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter meshFilter in meshFilters)
                    {
                        if (meshFilter.sharedMesh != null)
                        {
                            meshes.Add(meshFilter.sharedMesh);
                        }
                    }

                    // Collect all SkinnedMeshRenderer meshes
                    SkinnedMeshRenderer[] skinnedMeshRenderers = fbxGo.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
                    {
                        if (smr.sharedMesh != null)
                        {
                            meshes.Add(smr.sharedMesh);
                        }
                    }

                    if (meshes.Count > 0)
                    {
                        allMeshes.Add(meshes);
                        Debug.Log("Loaded meshes from: " + path);
                    }
                }
            }
        }
        return allMeshes;
    }

    // same as above but all so returns combined bounds of each mesh group
    public static List<(List<Mesh>, Bounds, string)> GetAllFBXMeshesWithBounds(string sourcePath, bool print = false)
    {
        List<(List<Mesh>, Bounds, string)> allMeshesWithBoundsAndFilenames = new List<(List<Mesh>, Bounds, string)>();

        // Get all FBX files in the directory using the search filter for Model type assets
        string[] fileGUIDs = AssetDatabase.FindAssets("t:Model", new[] { sourcePath });
        foreach (string guid in fileGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.EndsWith(".fbx"))
            {
                GameObject fbxGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (fbxGo != null)
                {
                    List<Mesh> meshes = new List<Mesh>();
                    Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero); // Start with empty bounds

                    // Collect all MeshFilter meshes
                    MeshFilter[] meshFilters = fbxGo.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter meshFilter in meshFilters)
                    {
                        if (meshFilter.sharedMesh != null)
                        {
                            meshes.Add(meshFilter.sharedMesh);
                            if (meshFilter.sharedMesh.bounds.size != Vector3.zero)
                            {
                                if (combinedBounds.size == Vector3.zero)
                                    combinedBounds = meshFilter.sharedMesh.bounds;
                                else
                                    combinedBounds.Encapsulate(meshFilter.sharedMesh.bounds);
                            }
                        }
                    }

                    // Collect all SkinnedMeshRenderer meshes
                    SkinnedMeshRenderer[] skinnedMeshRenderers = fbxGo.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
                    {
                        if (smr.sharedMesh != null)
                        {
                            meshes.Add(smr.sharedMesh);
                            if (smr.sharedMesh.bounds.size != Vector3.zero)
                            {
                                if (combinedBounds.size == Vector3.zero)
                                    combinedBounds = smr.sharedMesh.bounds;
                                else
                                    combinedBounds.Encapsulate(smr.sharedMesh.bounds);
                            }
                        }
                    }

                    if (meshes.Count > 0)
                    {
                        string fileName = Path.GetFileName(path);
                        allMeshesWithBoundsAndFilenames.Add((meshes, combinedBounds, fileName));
                        if (print == true)
                            Debug.Log($"Loaded meshes from: {path}, Combined Bounds: {combinedBounds.size}");
                    }
                }
            }
        }
        return allMeshesWithBoundsAndFilenames;
    }

    //------------------
    //searches the post/rail name in the preset rather than accessing a GameObject
    public static List<List<string>> FindPresetsUsingPrefabName(string prefabName)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        List<List<string>> gameObjectPresetNames = new List<List<string>>();

        //for each preset in presetsList, get the names of the railA, railB, post, subpost, and extra
        foreach (var preset in presetList)
        {
            List<string> presetNames = new List<string> { $"'{prefabName}' is used in:\n" };

            if (preset.railAName == prefabName)
            {
                presetNames.Add($"      - {preset.name}  [in railAName]  / {preset.categoryName}\n");
            }
            if (preset.railBName == prefabName)
            {
                presetNames.Add($"      - {preset.name} [in railBName]  / {preset.categoryName}\n");
            }
            if (preset.postName == prefabName)
            {
                presetNames.Add($"      - {preset.name}  [in postName]  / {preset.categoryName}\n");
            }
            if (preset.subpostName == prefabName)
            {
                presetNames.Add($"      - {preset.name}  [in subpostName]  / {preset.categoryName}\n");
            }
            if (preset.extraName == prefabName)
            {
                presetNames.Add($"      - {preset.name} (in extraName)  / {preset.categoryName}\n");
            }

            if (presetNames.Count == 1) // Only the header was added
            {
                presetNames.Add("None foundPrefabs.\n");
            }
            gameObjectPresetNames.Add(presetNames);
        }
        return gameObjectPresetNames;
    }

    private static string GetFirstMeshFilterName(GameObject go)
    {
        // Get the MeshRenderer component; you might also consider MeshFilter if you need the mesh data
        MeshRenderer renderer = go.GetComponentInChildren<MeshRenderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            // Assuming the mesh is attached to the same GameObject as the MeshRenderer
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                return meshFilter.name;
            }
        }
        return "No MeshFilter Found"; // Return a default string if no mesh is found
    }

    //--------------------------
    public static void ResizeMeshOfPrefab(GameObject go, float newX, float newY, float newZ)
    {
        Vector3 newSize = new Vector3(newX, newY, newZ);
        MeshUtilitiesAFWB.ResizeGameObjectMesh(go, newSize);
    }

    public static void Tests()
    {
        af.ClearConsole();

        //af.PrintSinglesForLayer(LayerSet.railALayerSet);
        //ResourceUtilities.GetMeshNamesFromPrefabs();
        //PresetChecker.ChangePresetCategoryName(af, mainPresetList, "Barrier Conc", "Concrete", "Barriers");

        //presetsEd.PrintAllPresets();

        //Housekeeping.PrintPresetsUsingGameObject("Gen_Cyl_DirtyMetal_Rail", mainPresetList, af);

        //Housekeeping.RenamePrefabAndUpdateAll("Stone_Low_WithConcTop_Panel", "Stone_LowTaperConcTop_Panel", af);
        //Housekeeping.RenamePrefabInFolder("TrackCurbing_Rail", "TrackCurbing_BlackWhite_Rail", af.currAutoFenceBuilderDir);
        //Housekeeping.UpdatePresetComponentAssetName(LayerSet.railALayerSet, "TrackCurbing_Rail", "TrackCurbing_Red_Rail", treatBothRails:false);
        //Housekeeping.UpdatePresetComponentAssetName(LayerSet.railBLayerSet, "TrackCurbing_Rail", "TrackCurbing_Red_Rail", treatBothRails: false);

        //af.ResetPoolForLayer(AutoFenceCreator.kRailALayerInt);
        //af.ForceRebuildFromClickPoints();

        //List<string> l = af.GetSourceVariantGoNamesForLayer(kRailALayer);
        //Debug.Log("----Variants\n");
        //foreach (string s in l)
        //    Debug.Log(af.StripPanelRailFromName(s) + "\n");

        //SeqItem.TimeGetPropertyListMethods();

        // List<GameObject> allPrefabs = PrefabsDebug.CombinePrefabsLists(af.railPrefabs, af.postPrefabs, af.extraPrefabs);
        //List<GameObjectFields> golist = PresetChecker.ExtractGameObjectFieldsFromPresetList(mainPresetList);
        //List<string> missingPrefabNames = PresetChecker.GetMissingGameObjectNames(golist, allPrefabs);

        //foreach (string s in missingPrefabNames)
        //    Debug.Log(s + "\n");

        //Dictionary<int, List<string>> missing =  PresetChecker.FindPresetsWithMissingPrefabs(missingPrefabNames, mainPresetList);

        ////print missing
        //foreach (KeyValuePair<int, List<string>> kvp in missing)
        //{
        //    Debug.Log("Preset " + kvp.Key + " is missing:\n");
        //    foreach (string s in kvp.Value)
        //        Debug.Log(s + "\n");
        //}

        //--PresetChecker.FindPresetsWithMissingNames(missingPrefabNames, mainPresetList);
        //Debug.Log("=============== Presets with Missing Prefabs:  =================\n");
        //List<ProblemPreset> problemPresets = PresetChecker.PrintPresetsWithMissingPrefabs(af, mainPresetList);
        //PresetChecker.TryRepairPreset(af, problemPresets);

        //List<List<string>> presetsForGOs = PresetFiles.FindPresetsUsingGameObjects(af.railPrefabs, mainPresetList);
        //PresetFiles.PrettyPrintGameObjectPresets(presetsForGOs);

        //GameObject testGo = af.FindPrefabByNameAndType("BarrierConcrete1_Old_Rail");

        //List<List<string>> usingGO = PresetFiles.FindPresetsUsingGameObject(af.FindPrefabByNameAndType("Background"), mainPresetList);
        //PresetFiles.PrettyPrintGameObjectPresets(usingGO);

        //List<GameObject> unusedGameObjects = Housekeeping.FindUnusedGameObjects(af.railPrefabs, mainPresetList);
        //Housekeeping.PrintUnusedGameObjects(unusedGameObjects);

        //List<GameObject> goList = af.FindPrefabsByNameContains("background");
        //PrintUtilities.PrintList(goList, $"goList {goList.Count}", true, allInOneLine: false);

        //List<Material> unusedMats = ResourceUtilities.FindUnusedMaterialsInAFWBFolder();
        //ResourceUtilities.MoveMaterialsIntoFolder(unusedMats, "Assets/AFWB_Unused/Materials");

        //List<Texture2D> unusedTex = ResourceUtilities.FindUnusedTexturesInAFWBFolder();
        //ResourceUtilities.MoveTexturesIntoFolder(unusedTex, "Assets/AFWB_Unused/Textures");

        //List<Mesh> unusedMeshes =  ResourceUtilities.FindUnusedMeshesInAFWBFolder();
        //ResourceUtilities.MoveMeshesIntoFolder(unusedMeshes, "Assets/AFWB_Unused/Meshes");

        //ResourceUtilities.MoveStrayMeshesIntoAFWBFolder();
        //ResourceUtilities.MoveStrayMaterialsIntoAFWBFolder();

        //List<Texture2D> strayTextures = ResourceUtilities.FindUsedTexturesOutsideAFWBFolder();
        //ResourceUtilities.MoveTexturesIntoFolder(strayTextures, "Assets/Auto Fence Builder/AFWB_Textures");

        //ResourceUtilities.BuildMaterialList(af);

        //(Texture2D rottenWood, Material mat) = ResourceUtilities.FindFirstMaterialUsingTextureName("WallHigh_New_AlbedoTransparency", af, print:true);

        //TextureUsage texUsage = ResourceUtilities.FindTextureUsageFromName("WallHigh_New_AlbedoTransparency", af, print: true);
        //(Texture2D rottenWood, Material mat) = ResourceUtilities.FindFirstMaterialUsingTextureName("RottenWood2_C", af, print: true);
        //if (mat != null)
        //ResourceUtilities.FindPresetsUsingMaterial(mat, this, true);

        //ResourceUtilities.CheckPresetsForMissingPrefabs(this);
    }

    /*if (GUILayout.Button(new GUIContent("Progressing")))
        {
            int tot = 4000;
            for (int gh = 0; gh < tot; gh++)
            {
                    EditorUtility.DisplayCancelableProgressBar("Progressing...", "Bob", (float)gh / (float)tot);
            }
            EditorUtility.ClearProgressBar();
            GUIUtility.ExitGUI();
        }*/

    //---------------------------------------------------------------------------------
    // Use Unity Tools menu
    // Replace all "_Panel_Rail" with "_Panel" in ScriptablePresetAFWB objects
    private static void FixPanelRailNames()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptablePresetAFWB", new[] { targetPresetsFolder });
        int changedCount = 0; // Counter for the number of changed objects

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ScriptablePresetAFWB preset = AssetDatabase.LoadAssetAtPath<ScriptablePresetAFWB>(assetPath);

            if (preset != null)
            {
                bool isDirty = false;

                if (preset.railAName.Contains("_Panel_Rail"))
                {
                    preset.railAName = preset.railAName.Replace("_Panel_Rail", "_Panel");
                    isDirty = true;
                }

                if (preset.railBName.Contains("_Panel_Rail"))
                {
                    preset.railBName = preset.railBName.Replace("_Panel_Rail", "_Panel");
                    isDirty = true;
                }

                if (isDirty)
                {
                    EditorUtility.SetDirty(preset);
                    changedCount++; // Increment the counter
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Print the number of changed objects to the console
        Debug.Log(changedCount + " ScriptablePresetAFWB objects were updated.\n");
    }
    //---------------------------------------------------------------------------------

    // Update name of component in all presets, e.g. after renaming a currPrefab
    //  treatBothRails = true: update both railA and railB
    public static void UpdatePresetComponentAssetNameAllLayers(string oldName, string newName)
    {
        PrefabTypeAFWB prefabType = af.GetPrefabTypeFromName(oldName);
        LayerSet layer = prefabType.ToLayer();
        UpdatePresetComponentAssetName(layer, oldName, newName);
    }
    //---------------------------------------------------------------------------------

    // Update name of component in all presets, e.g. after renaming a currPrefab
    //  treatBothRails = true: update both railA and railB
    public static void UpdatePresetComponentAssetName(AFWB.LayerSet layer, string oldName, string newName, bool treatBothRails = true)
    {
        //-- It could be that even though we're dealing with a specific Layer, some variants could be using prefabs
        //-- from another layer, e.g. a Post Prefab in an Extra layer. So we need to check all, but look in this layer first

        GameObject newGo = null;
        if (layer != LayerSet.extraLayerSet)
        {
            newGo = af.FindPrefabByNameAndType(layer.ToPrefabType(), newName, warnMissing: true, returnMissingDefault: false);
            if (newGo == null)
            {
                Debug.Log($"New GameObject not found: {newName}\n");
                return;
            }
        }

        List<string[]> presetGuidsDirList = new List<string[]>();
        string[] presetGuids = AssetDatabase.FindAssets("t:ScriptablePresetAFWB", new[] { targetPresetsFolder });
        presetGuidsDirList.Add(presetGuids);
        string[] presetGuidsUser = AssetDatabase.FindAssets("t:ScriptablePresetAFWB", new[] { targetPresetsFolderUser });
        presetGuidsDirList.Add(presetGuidsUser);

        int changedCount = 0; // Counter for the number of changed objects
        List<string> changedPresets = new List<string>();


        foreach (string[] presetsFolder in presetGuidsDirList)
        {
            foreach (string guid in presetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptablePresetAFWB preset = AssetDatabase.LoadAssetAtPath<ScriptablePresetAFWB>(assetPath);

                if (preset != null)
                {
                    bool isDirty = false;
                    List<bool> itemsReplaced = null;

                    if ((layer == LayerSet.railALayerSet || layer == LayerSet.railALayerSet) && treatBothRails == true)
                    {
                        isDirty = preset.ReplaceMainGoNameForLayer(oldName, newName, LayerSet.railALayerSet, af);
                        isDirty = preset.ReplaceMainGoNameForLayer(oldName, newName, LayerSet.railBLayerSet, af);

                        if (preset.ReplaceSourceVariantGosByNameForLayer(oldName, newGo, LayerSet.railALayerSet, af).replaced.Count > 0)
                            isDirty = true;
                        if (preset.ReplaceSourceVariantGosByNameForLayer(oldName, newGo, LayerSet.railBLayerSet, af).replaced.Count > 0)
                            isDirty = true;
                    }
                    else
                    {
                        isDirty = preset.ReplaceMainGoNameForLayer(oldName, newName, layer, af);

                        if (layer == LayerSet.extraLayerSet && newGo == null)
                            newGo = af.FindPrefabByName(newName);

                        if (preset.ReplaceSourceVariantGosByNameForLayer(oldName, newGo, layer, af).isDirty == true)
                            isDirty = true;
                    }

                    if (isDirty)
                    {
                        EditorUtility.SetDirty(preset);
                        changedCount++; // Increment the counter
                        changedPresets.Add(preset.name);
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        // Print the number of changed objects to the console
        Debug.Log(changedCount + " presets were updated:  " + string.Join(", ", changedPresets.ToArray()) + "\n");
        Debug.Log("-----------------------------------------\n");
        //Debug.Log("Changed Presets: " + string.Join(", ", changedPresets.ToArray()) + "\n");
    }

    //---------------------------------
    /// <summary>
    /// Builds a list of all unique materials used in the specified AutoFenceCreator prefabs.
    /// </summary>
    /// <param name="af">The AutoFenceCreator gizmoSingletonInstance containing the prefabs.</param>
    /// <param name="printList">If true, prints the list of materials to the debug log.</param>
    /// <returns>A list of all unique materials used in the prefabs.</returns>
    public static List<Material> BuildListOfAllLoadedMaterials(AutoFenceCreator af, bool printList = false)
    {
        // HashSet to efficiently track unique materials. HashSet provides faster lookups and ensures no duplicates.
        // Unlike List, HashSet uses a hash-based structure which allows O(1) average time complexity for add and contains operations.
        HashSet<Material> allMatsSet = new HashSet<Material>();

        // List to store materials in the order they are added.
        // While List allows duplicates and requires O(n) time complexity for checking existence,
        // it is used here to maintain the order of materials for further processing or display.
        List<Material> allMatsList = new List<Material>();

        // Process post prefabs
        AddMaterialsFromPrefabs(af.postPrefabs, allMatsSet, allMatsList, printList);
        // Process rail prefabs
        AddMaterialsFromPrefabs(af.railPrefabs, allMatsSet, allMatsList, printList);
        // Process extra prefabs
        AddMaterialsFromPrefabs(af.extraPrefabs, allMatsSet, allMatsList, printList);

        // Return the list of unique materials
        return allMatsList;
    }

    /// <summary>
    /// Adds unique materials from the provided prefabs to the material set and list.
    /// </summary>
    /// <param name="prefabs">The list of prefabs to process.</param>
    /// <param name="allMatsSet">The set used to track unique materials for efficient checking.</param>
    /// <param name="allMatsList">The list used to store materials in the order they are added.</param>
    /// <param name="printList">If true, prints the list of materials to the debug log.</param>
    private static void AddMaterialsFromPrefabs(List<GameObject> prefabs, HashSet<Material> allMatsSet, List<Material> allMatsList, bool printList)
    {
        foreach (var go in prefabs)
        {
            // Get the Renderer component from the GameObject
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                // Get the shared material of the Renderer
                Material mat = rend.sharedMaterial;
                if (mat != null && allMatsSet.Add(mat)) // Add to HashSet and check if it was added
                {
                    // Add to List if it was added to the HashSet
                    allMatsList.Add(mat);
                    if (printList)
                    {
                        // Print the material count and name
                        Debug.Log($"{allMatsList.Count}   {mat.name}\n");
                    }
                }
            }
        }
    }

    public static void FindTextureUsage(Texture2D tex, AutoFenceCreator af, bool print = false)
    {
        List<Material> matsList = BuildListOfAllLoadedMaterials(af);
        TextureUsage texUsage = new TextureUsage(tex);

        for (int i = 0; i < matsList.Count; i++)
        {
            Material mat = matsList[i];
            List<Texture2D> texList = GetAllTexturesFromMaterial(mat);
            if (texList.Contains(tex))
            {
                texUsage.matList.Add(mat);
                if (print)
                {
                    Debug.Log(mat.name + "\n");
                }
            }
        }
    }

    public static List<ScriptablePresetAFWB> FindPresetsUsingMaterialName(string matName, AutoFenceEditor ed, bool print = false)
    {
        List<ScriptablePresetAFWB> presetList = new List<ScriptablePresetAFWB>();

        GameObject postGo = null, railGo = null, extraGO = null;
        Material material = null;
        List<Texture2D> texList = new List<Texture2D>();
        for (int i = 0; i < ed.mainPresetList.Count; i++)
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[i];

            postGo = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.postPrefab, preset.postName);
            if (postGo != null)
            {
                Renderer rend = postGo.GetComponent<Renderer>();
                if (rend != null)
                    material = rend.sharedMaterial;
                if (material != null)
                {
                    if (material.name == matName)
                    {
                        presetList.Add(preset);
                        if (print)
                            Debug.Log(preset.name + "\n");
                    }
                    //GetAllTexturesFromMaterial(mat);
                }
            }
            railGo = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, preset.railAName);
            if (railGo != null)
            {
                Renderer rend = railGo.GetComponent<Renderer>();
                if (rend != null)
                    material = rend.sharedMaterial;
                if (material != null)
                {
                    if (material.name == matName)
                    {
                        presetList.Add(preset);
                        if (print)
                            Debug.Log(preset.name + "\n");
                    }
                }
            }
            railGo = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, preset.railBName);
            if (railGo != null)
            {
                Renderer rend = railGo.GetComponent<Renderer>();
                if (rend != null)
                    material = rend.sharedMaterial;
                if (material != null)
                {
                    if (material.name == matName)
                    {
                        presetList.Add(preset);
                        if (print)
                            Debug.Log(preset.name + "\n");
                    }
                }
            }
            extraGO = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.extraPrefab, preset.extraName);
            if (extraGO != null)
            {
                Renderer rend = extraGO.GetComponent<Renderer>();
                if (rend != null)
                    material = rend.sharedMaterial;
                if (material != null)
                {
                    if (material.name == matName)
                    {
                        presetList.Add(preset);
                        if (print)
                            Debug.Log(preset.name + "\n");
                    }
                }
            }
        }
        return presetList;
    }

    //---------------------------------
    //CreateMergedPrefabs List of every material used in all the prefabs
    /*static public List<Material> BuildMaterialList(AutoFenceCreator af)
    {
        List<Material> matList = new List<Material>();
        Material mat = null;
        for (int i = 0; i < af.postPrefabs.Count; i++)
        {
            GameObject go = af.postPrefabs[i];
            if (go != null)
            {
                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    mat = rend.sharedMaterial;
                    if (matList.Contains(mat) == false)
                        matList.Add(mat);
                }
            }
        }

        for (int i = 0; i < matList.Count; i++)
        {
            Debug.Log(matList[i].name + "\n");
        }
        return matList;
    }*/

    //-------------------------------------
    // Useful when checking compatibility in Unity 2018 (old prefab system)
    // assumes af has already loaded the prefabs
    public static List<GameObject> FindPrefabsWithMissingMesh(PrefabTypeAFWB type = PrefabTypeAFWB.allPrefab)
    {
        List<GameObject> brokenPrefabs = new List<GameObject>();
        //List<GameObject> prefabs = af.postPrefabs;

        List<UnityEngine.Object> allPrefabs = null;

        if (type == PrefabTypeAFWB.allPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        if (type == PrefabTypeAFWB.postPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs/_Posts_AFWB", false);
        if (type == PrefabTypeAFWB.railPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs/_Rails_AFWB", false);
        if (type == PrefabTypeAFWB.extraPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs/_Extras_AFWB", false);

        GameObject go = null;
        for (int p = 0; p < allPrefabs.Count; p++)
        {
            go = (GameObject)allPrefabs[p];
            if (go.GetComponent<MeshFilter>() == null || go.GetComponent<MeshFilter>().sharedMesh == null)
            {
                bool found = false;//Maybe it's on a child
                if (go.transform.childCount > 0)
                {
                    foreach (Transform child in go.transform)
                    {
                        if (child.GetComponent<MeshFilter>() != null && child.GetComponent<MeshFilter>().sharedMesh != null)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                        brokenPrefabs.Add(go);
                }
                if (found == false)
                    brokenPrefabs.Add(go);
            }
        }
        return brokenPrefabs;
    }

    //-------------------------------------
    //Creates a dictionary of all the prefab names and their mesh names
    public static Dictionary<string, List<string>> GetMeshNamesFromPrefabs(PrefabTypeAFWB type = PrefabTypeAFWB.allPrefab)
    {
        List<GameObject> brokenPrefabs = new List<GameObject>();
        //List<GameObject> prefabs = af.postPrefabs;

        List<UnityEngine.Object> allPrefabs = null;

        //Create dictionary of all prefab names and their mesh names
        Dictionary<string, List<string>> prefabMeshNames = new Dictionary<string, List<string>>();

        if (type == PrefabTypeAFWB.allPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        if (type == PrefabTypeAFWB.postPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs/_Posts_AFWB", false);
        if (type == PrefabTypeAFWB.railPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs/_Rails_AFWB", false);
        if (type == PrefabTypeAFWB.extraPrefab)
            allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs/_Extras_AFWB", false);

        GameObject go = null;
        for (int p = 0; p < allPrefabs.Count; p++)
        {
            bool foundMesh = false;
            go = (GameObject)allPrefabs[p];
            if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshFilter>().sharedMesh != null)
            {
                //add the prefab name to the dictionary and add the mesh name to the list
                if (prefabMeshNames.ContainsKey(go.name) == false)
                {
                    prefabMeshNames.Add(go.name, new List<string>());
                }
                prefabMeshNames[go.name].Add(go.GetComponent<MeshFilter>().sharedMesh.name);
                //foundMesh = true;

                /*if (go.transform.childCount > 0)
                {
                    foreach (Transform child in go.transform)
                    {
                        if (child.GetComponent<MeshFilter>() != null && child.GetComponent<MeshFilter>().sharedMesh != null)
                        {
                            prefabMeshNames[go.name].Add(child.GetComponent<MeshFilter>().sharedMesh.name);
                            //foundMesh = true;
                        }
                    }
                }*/
            }
            //else
            //prefabMeshNames[go.name].Add("Mesh missing \n");
        }
        SaveMeshNamesToFile(prefabMeshNames);

        return prefabMeshNames;
    }

    private static void SaveMeshNamesToFile(Dictionary<string, List<string>> prefabMeshNames)
    {
        //print out the dictionary
        string allString = "Prefab Name,  Mesh Name, Mesh File Name\n";
        foreach (KeyValuePair<string, List<string>> kvp in prefabMeshNames)
        {
            List<string> meshNamesAndFileNames = new List<string>();
            string prefabNameKey = kvp.Key;
            List<string> value = kvp.Value;
            string meshName = "", fileName = "", meshNames = "", line = "";
            for (int i = 0; i < value.Count; i++)
            {
                meshName = value[i];
                fileName = Housekeeping.MeshNameToFile(meshName);
                //meshNames += "Mesh:" + meshName + ",   (File: " + fileName + ")";
                //meshNames += meshName + "," + fileName + ",";
                //meshNamesAndFileNames.Add(meshName + "," + fileName + ",");
                line += prefabNameKey + "," + meshName + "," + fileName + ",";
                //meshNamesAndFileNames.Add(line);
            }
            //Debug.Log(prefabNameKey + ":      " + meshNames + "\n");

            //File.WriteAllLines("Assets/aaa.txt", prefabMeshNames.Select(kvp => string.Format("{0};{1}", kvp.Key, meshNames)));
            //create comma separated striung from meshNames List
            //string meshNamesString = string.Join("+", meshNamesAndFileNames);

            //File.WriteAllLines("Assets/aaa.txt", meshNamesAndFileNames);

            allString += line + "\n";

            //File.WriteAllText("Assets/aaa.txt", meshNamesString);
        }
        //write allString to file as "Assets/aaa.txt"
        File.WriteAllText("Assets/_PrefabMeshNames.txt", allString);
    }

    //------------------------------------

    // This needs doing any time a prefab is added
    private static void UpdateMeshNamesDictionary()
    {
    }

    //------------------------------------
    // Finds the mesh file that has meshName, as often the .fbx is saved with a different name to the embedded mesh
    public static string MeshNameToFile(string meshName)
    {
        string[] assetGUIDs = AssetDatabase.FindAssets(meshName + " t:mesh");
        string fileName = "";
        if (assetGUIDs.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
            //Debug.Log("Asset Path: " + path);

            // If you just want the filename:
            fileName = System.IO.Path.GetFileName(assetPath);
            //Debug.Log("Filename: " + fileName);
        }
        else
        {
            fileName = "Mesh not foundInMaterial!";
        }
        return fileName;
    }

    //------------------------------------
    // Are there materials being used that haven't been put in the correct directory
    public static List<Material> FindUsedMaterialsOutsideAFWBFolder(bool print = true)
    {
        List<UnityEngine.Object> allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        List<Material> strayMats = new List<Material>();
        HashSet<Material> uniqueStrayMats = new HashSet<Material>();

        for (int i = 0; i < allPrefabs.Count; i++)
        {
            GameObject prefab = (GameObject)allPrefabs[i];
            Renderer renderer = prefab.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material mat = renderer.sharedMaterial;
                if (mat != null)
                {
                    string path = AssetDatabase.GetAssetPath(mat);
                    if (!path.Contains("AFWB_Materials"))
                    {
                        strayMats.Add(mat);
                        uniqueStrayMats.Add(mat);
                        //if (print)
                        //Debug.Log($"{prefab.name}:   {path}\n");
                    }
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(prefab);
                    Debug.Log($"{prefab.name} missing Material    {path}\n");
                }
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(prefab);
                Debug.Log($"{prefab.name} missing renderer    {path}\n");
            }
        }

        List<Material> uniqueStrayMaterialsList = uniqueStrayMats.ToList();
        // If you want to print unique stray materials, uncomment the following lines:

        if (print)
        {
            foreach (var mat in uniqueStrayMaterialsList)
                Debug.Log($"Unique Stray Material: {mat.name}\n");
            Debug.Log($"Total Unique Stray Materials: {uniqueStrayMaterialsList.Count}\n");
        }

        return uniqueStrayMaterialsList;
    }
    //------------------------------------
    // Are there materials being used that don't have a color or normal texture assigned
    public static List<Material> FindPrefabMaterialsWithMissingTextures(bool movePrefabs, bool print = true)
    {
        List<UnityEngine.Object> allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        HashSet<Material> uniqueMissingTextures = new HashSet<Material>();
        string problemPrefabsPath = "Assets/Auto Fence Builder/AFWB_BrokenPrefabs";

        // Ensure the problemPrefabsPath exists
        if (movePrefabs && !AssetDatabase.IsValidFolder(problemPrefabsPath))
        {
            AssetDatabase.CreateFolder("Assets/Auto Fence Builder", "AFWB_BrokenPrefabs");
        }

        for (int i = 0; i < allPrefabs.Count; i++)
        {
            GameObject prefab = (GameObject)allPrefabs[i];
            Renderer renderer = prefab.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material mat = renderer.sharedMaterial;
                if (mat != null)
                {
                    Texture2D mainTex = mat.GetTexture("_MainTex") as Texture2D;
                    Texture2D normalTex = mat.GetTexture("_BumpMap") as Texture2D;

                    if (mainTex == null || normalTex == null)
                    {
                        uniqueMissingTextures.Add(mat);
                        if (print)
                        {
                            if (mainTex == null && normalTex == null)
                                Debug.Log($"{prefab.name} is missing both main color and normal textures in material {mat.name}.\n");
                            else if (mainTex == null)
                                Debug.Log($"{prefab.name} is missing main color texture in material {mat.name}.\n");
                            else if (normalTex == null)
                                Debug.Log($"{prefab.name} is missing normal texture in material {mat.name}.\n");
                        }

                        // Move the prefab if movePrefabs is true
                        if (movePrefabs)
                        {
                            string prefabPath = AssetDatabase.GetAssetPath(prefab);
                            string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath($"{problemPrefabsPath}/{prefab.name}.prefab");
                            AssetDatabase.MoveAsset(prefabPath, newPrefabPath);
                            Debug.Log($"Moved {prefab.name} to {newPrefabPath}\n");
                        }
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return uniqueMissingTextures.ToList();
    }



    //------------------------------------
    // Are there Textures being used that haven't been put in the correct directory
    public static List<Texture2D> FindUsedTexturesOutsideAFWBFolder(bool print = true)
    {
        List<UnityEngine.Object> allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        List<Texture2D> strayTex = new List<Texture2D>();
        HashSet<Texture2D> uniqueStrayTex = new HashSet<Texture2D>();

        for (int i = 0; i < allPrefabs.Count; i++)
        {
            GameObject prefab = (GameObject)allPrefabs[i];
            Renderer renderer = prefab.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material mat = renderer.sharedMaterial;
                if (mat != null)
                {
                    List<Texture2D> matTextures = GetAllTexturesFromMaterial(mat);

                    for (int k = 0; k < matTextures.Count; k++)
                    {
                        Texture2D matTex = matTextures[k];
                        if (matTex != null)
                        {
                            string path = AssetDatabase.GetAssetPath(matTex);
                            if (!path.Contains("AFWB_Textures"))
                            {
                                strayTex.Add(matTex);
                                uniqueStrayTex.Add(matTex);
                                //if (print)
                                // Debug.Log($"{prefab.name}:   {path}\n");
                            }
                        }
                    }
                }
            }
        }

        List<Texture2D> uniqueStrayTexturesList = uniqueStrayTex.ToList();
        // If you want to print unique stray textures, uncomment the following lines:

        if (print)
        {
            foreach (var tex in uniqueStrayTexturesList)
            {
                Debug.Log($"Unique Stray Texture: {tex.name}\n");
            }
            Debug.Log($"Total Unique Stray Textures: {uniqueStrayTexturesList.Count}\n");
        }


        return uniqueStrayTexturesList;
    }

    //------------------------------------
    public static List<Texture2D> GetAllTexturesFromMaterial(Material mat, bool onlyOutsideAFWB = true)
    {
        List<Texture2D> textures = new List<Texture2D>();
        if (mat != null)
        {
            Shader shader = mat.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);
            for (int k = 0; k < propertyCount; k++)
            {
                if (ShaderUtil.GetPropertyType(shader, k) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    Texture matTex = mat.GetTexture(ShaderUtil.GetPropertyName(shader, k));
                    if (matTex != null)
                    {
                        string path = AssetDatabase.GetAssetPath(matTex);
                        if (onlyOutsideAFWB == false || path.Contains("AFWB_Textures") == false)
                        {
                            textures.Add((Texture2D)matTex);
                        }
                    }
                }
            }
        }
        return textures;
    }

    //------------------------------------
    // Are there meshes being used that haven't been put in the correct directory
    public static List<Mesh> FindUsedMeshesOutsideAFWBFolder(bool print = true)
    {
        List<UnityEngine.Object> allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        List<Mesh> strayMeshes = new List<Mesh>();

        foreach (GameObject prefab in allPrefabs)
        {
            // Get all MeshFilters in this prefab and its children
            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter mf in meshFilters)
            {
                Mesh mesh = mf.sharedMesh;
                if (mesh != null)
                {
                    string path = AssetDatabase.GetAssetPath(mesh);
                    if (!path.Contains("AFWB_Meshes"))
                    {
                        strayMeshes.Add(mesh);

                        if (print)
                            Debug.Log(prefab.name + ":   " + path + "\n");
                    }
                }
                else
                {
                    string sourcePath = AssetDatabase.GetAssetPath(prefab);
                    Debug.Log($"{prefab.name} missing Mesh  {sourcePath} \n");
                }
            }
        }

        return strayMeshes;
    }

    //------------------------------------
    /*static public List<Mesh> FindUsedMeshesContainsName(string name, bool print = true)
    {
        List<UnityEngine.Object> allPrefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);
        List<Mesh> strayMeshes = new List<Mesh>();
        for (int i = 0; i < allPrefabs.Count; i++)
        {
            GameObject prefab = (GameObject)allPrefabs[i];
            if (prefab.GetComponent<MeshFilter>() != null)
            {
                Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                {
                    string dirPath = AssetDatabase.GetAssetPath(mesh);
                    if (dirPath.Contains("AFWB_Meshes") == false)
                    {
                        strayMeshes.Add(mesh);

                        if (print)
                            Debug.Log(prefab.name + ":   " + dirPath + "\n");
                    }
                }
                else
                    Debug.Log(prefab.name + " missing Mesh");
            }
            else
                Debug.Log(prefab.name + " missing MeshFilter");
        }
        return strayMeshes;
    }*/
    //------------------------------------

    public static void IsAssetTextureInPrefabs(Texture[] assetsTextures, List<Texture> prefabsTextures)
    {
        for (int i = 0; i < assetsTextures.Length; i++)
        {
            //Debug.Log(i + "   " + assetsTextures[i].name + "\n");
            string assetTexName = assetsTextures[i].name;

            bool foundInPrefab = false;
            for (int j = 0; j < prefabsTextures.Count; j++)
            {
                Texture prefabTex = prefabsTextures[j];
                string prefabTexName = prefabTex.name;

                if (prefabTexName == assetTexName)
                {
                    foundInPrefab = true;
                    break;
                }
            }
            if (foundInPrefab == false)
            {
                Debug.Log(assetTexName + " not foundInMaterial in Prefabs \n");
            }
        }
    }

    //---------------------------------
    public static List<Material> FindUnusedMaterialsInAFWBFolder()
    {
        List<UnityEngine.Object> materials = ResourceUtilities.GetAssetsAtPath<Material>("Assets/Auto Fence Builder/AFWB_Materials", false);
        List<UnityEngine.Object> prefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);

        List<Material> unusedMats = new List<Material>();
        for (int i = 0; i < materials.Count; i++)
        {
            bool found = false;
            Material mat = materials[i] as Material;
            string sourcePath = AssetDatabase.GetAssetPath(mat);
            if (sourcePath.Contains("System"))
                continue;
            for (int j = 0; j < prefabs.Count; j++)
            {
                GameObject go = prefabs[j] as GameObject;
                if (go.GetComponent<Renderer>() != null)
                {
                    //Material goMat = go.GetComponent<Renderer>().sharedMaterial;
                    Material[] goMats = go.GetComponent<Renderer>().sharedMaterials;
                    foreach (Material goMat in goMats)
                    {
                        if (goMat != null)
                        {
                            if (goMat == mat)
                                found = true;
                        }
                        //else
                        //Debug.Log("missing material on " + go.name + "\n");
                    }
                    //else
                    //Debug.Log("missing material on " + go.name + "\n");
                }
                //else
                //Debug.Log("missing Renderer on " + go.name + "\n");
                if (found)
                    break;
            }
            if (found == false)
            {
                unusedMats.Add(mat);
                Debug.Log(mat.name + " not used \n");
            }
        }
        Debug.Log($"Used Materials =  {materials.Count - unusedMats.Count},  Unused Materials =  {unusedMats.Count}  \n");
        return unusedMats;
    }

    //---------------------------------
    /*static public List<Mesh> FindUnusedMeshesInAFWBFolder()
    {
        List<UnityEngine.Object> meshes = ResourceUtilities.GetAssetsAtPath<Mesh>("Assets/Auto Fence Builder/AFWB_Meshes", false);
        List<UnityEngine.Object> prefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);

        List<Mesh> unusedMeshes = new List<Mesh>();
        //int unusedMeshCount = 0;
        for (int i = 0; i < meshes.Count; i++)
        {
            bool found = false;
            Mesh mesh = meshes[i] as Mesh;
            for (int j = 0; j < prefabs.Count; j++)
            {
                GameObject go = prefabs[j] as GameObject;
                if (go.GetComponent<MeshFilter>() != null)
                {
                    Mesh goMesh = go.GetComponent<MeshFilter>().sharedMesh;
                    if (goMesh != null)
                    {
                        if (goMesh == mesh)
                            found = true;
                    }
                    else
                        Debug.Log("missing mesh on " + go.name + "\n");
                }
                //else
                //Debug.Log("missing mesh filter on " + go.name + "\n");
                if (found)
                    break;
            }
            if (found == false)
            {
                unusedMeshes.Add(mesh);
                Debug.Log(mesh.name + " not used \n");
            }
        }
        Debug.Log("Unused Meshes = " + unusedMeshes.Count + "\n");
        return unusedMeshes;
    }*/

    public static List<Mesh> FindUnusedMeshesInAFWBFolder()
    {
        List<UnityEngine.Object> meshes = ResourceUtilities.GetAssetsAtPath<Mesh>("Assets/Auto Fence Builder/AFWB_Meshes", false);
        List<UnityEngine.Object> prefabs = ResourceUtilities.GetAssetsAtPath<GameObject>("Assets/Auto Fence Builder/AFWB_Prefabs", false);

        List<Mesh> unusedMeshes = new List<Mesh>();
        for (int i = 0; i < meshes.Count; i++)
        {
            bool found = false;
            Mesh mesh = meshes[i] as Mesh;
            for (int j = 0; j < prefabs.Count; j++)
            {
                GameObject go = prefabs[j] as GameObject;

                // Get all MeshFilters in this GameObject and its children
                MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter mf in meshFilters)
                {
                    if (mf.sharedMesh != null)
                    {
                        if (mf.sharedMesh == mesh)
                        {
                            found = true;
                            break;
                        }
                    }
                    else
                        Debug.Log("missing mesh on " + mf.gameObject.name + "\n");
                }

                if (found)
                    break;
            }
            if (!found)
            {
                unusedMeshes.Add(mesh);
                Debug.Log(mesh.name + " not used \n");
            }
        }
        Debug.Log($"Used Meshes =  {meshes.Count - unusedMeshes.Count},  Unused Meshes =  {unusedMeshes.Count}  \n");
        return unusedMeshes;
    }

    //-------------------------------
    // checks every texture in AFWB_Textures, to see if it it's used in any Material in AFWB_Materials
    public static List<Texture2D> FindUnusedTexturesInAFWBFolder(bool print = true, bool checkGenericTiledAlso = false)
    {
        List<UnityEngine.Object> textures = ResourceUtilities.GetAssetsAtPath<Texture2D>("Assets/Auto Fence Builder/AFWB_Textures", false);
        List<UnityEngine.Object> materials = ResourceUtilities.GetAssetsAtPath<Material>("Assets/Auto Fence Builder/AFWB_Materials", false);
        List<Texture2D> unusedTex = new List<Texture2D>();
        int usedTexCount = 0;
        for (int i = 0; i < textures.Count; i++)
        {
            bool foundInMaterial = false;
            Texture2D tex = textures[i] as Texture2D;
            string sourcePath = AssetDatabase.GetAssetPath(tex);
            if (sourcePath.Contains("Utility") || (checkGenericTiledAlso == true && sourcePath.Contains("Generic Tiled")))
                continue;

            for (int j = 0; j < materials.Count; j++)
            {
                Material mat = materials[j] as Material;
                Shader shader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int k = 0; k < propertyCount; k++)
                {
                    if (ShaderUtil.GetPropertyType(shader, k) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture matTex = mat.GetTexture(ShaderUtil.GetPropertyName(shader, k));
                        if (matTex != null)
                        {
                            if (matTex == tex)
                            {
                                //Debug.Log(usedTexCount++ + "  " + tex.name + " used\n");
                                usedTexCount++;
                                foundInMaterial = true;
                                break;
                            }
                        }
                    }
                }
                if (foundInMaterial)
                    break;
            }
            if (foundInMaterial == false)
            {
                unusedTex.Add(tex);
                if (print)
                    Debug.Log(AssetDatabase.GetAssetPath(tex) + "\n");
            }
        }
        Debug.Log($"Used Textures =  {usedTexCount},  Unused Textures =  {unusedTex.Count}  \n");
        AssetDatabase.Refresh();
        return unusedTex;
    }

    //-------------------------------
    // checks every loaded texture to find materials and presetsEd use
    public static List<Texture2D> FindUsedTexturesInAFWBFolder(bool print = true)
    {
        List<UnityEngine.Object> textures = ResourceUtilities.GetAssetsAtPath<Texture2D>("Assets/Auto Fence Builder/AFWB_Textures", false);
        List<UnityEngine.Object> materials = ResourceUtilities.GetAssetsAtPath<Material>("Assets/Auto Fence Builder/AFWB_Materials", false);
        List<Texture2D> unusedTex = new List<Texture2D>();
        int usedTexCount = 0;
        for (int i = 0; i < textures.Count; i++)
        {
            bool found = false;
            Texture2D tex = textures[i] as Texture2D;
            for (int j = 0; j < materials.Count; j++)
            {
                Material mat = materials[j] as Material;
                Shader shader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int k = 0; k < propertyCount; k++)
                {
                    if (ShaderUtil.GetPropertyType(shader, k) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture matTex = mat.GetTexture(ShaderUtil.GetPropertyName(shader, k));
                        if (matTex != null)
                        {
                            if (matTex == tex)
                            {
                                //Debug.Log(usedTexCount++ + "  " + tex.name + " used\n");
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (found)
                    break;
            }
            if (found == false)
            {
                unusedTex.Add(tex);
                if (print)
                    Debug.Log(tex.name + "\n");
            }
        }
        Debug.Log("Used Textures = " + usedTexCount + "\n");
        return unusedTex;
    }

    //------------------------------------
    public static void MoveMeshesIntoFolder(List<Mesh> meshes, string path)
    {
        // Ensure the exists and path starts with "Assets/", create if not
        ResourceUtilities.CheckOrCreateFolder(path);

        for (int i = 0; i < meshes.Count; i++)
        {
            Mesh mesh = meshes[i];
            string sourcePath = AssetDatabase.GetAssetPath(mesh);
            string extension = Path.GetExtension(sourcePath);
            string destPath = path + "/" + mesh.name + extension;
            if (destPath != sourcePath)
            {
                bool exists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(destPath));
                if (exists)
                {
                    Debug.LogWarning($"This Asset already exists at {destPath}\n");
                    // Append "_copy" to the filename
                    int randInt = UnityEngine.Random.Range(0, 10000);
                    string copyString = $"_copy{randInt.ToString()}";
                    destPath = $"{Path.Combine(Path.GetDirectoryName(destPath), Path.GetFileNameWithoutExtension(destPath))}{copyString}{Path.GetExtension(destPath)}";
                }

                string result = AssetDatabase.MoveAsset(sourcePath, destPath);
                if (result != "")
                    Debug.Log(mesh.name + "  " + result);
                Debug.Log("sourcePath  " + sourcePath + "    destPath " + AssetDatabase.GetAssetPath(mesh));
            }
        }
    }

    //------------------------------------
    public static void MoveTexturesIntoFolder(List<Texture2D> textures, string path = "")
    {
        // Ensure the exists and path starts with "Assets/", create if not
        ResourceUtilities.CheckOrCreateFolder(path);
        int copies = 0, moved = 0;
        if (path == "")
            path = "Assets/Auto Fence Builder/AFWB_Textures";

        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D tex = textures[i];
            string sourcePath = AssetDatabase.GetAssetPath(tex);
            string extension = Path.GetExtension(sourcePath);
            string destPath = path + "/" + tex.name + extension;
            if (destPath != sourcePath)
            {
                bool exists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(destPath));
                if (exists)
                {
                    Debug.LogWarning($"This Asset already exists at {destPath}\n");
                    copies++;
                    // Append "_copy" to the filename
                    /*int randInt = UnityEngine.Random.Range(0, 10000);
                    string copyString = $"_copy{randInt.ToString()}";
                    destPath = $"{Path.Combine(Path.GetDirectoryName(destPath), 
                    Path.GetFileNameWithoutExtension(destPath))}{copyString}{Path.GetExtension(destPath)}";*/
                }
                string result = AssetDatabase.MoveAsset(sourcePath, destPath);
                if (result != "")
                {
                    Debug.Log($"Moved {tex.name}  result \n");
                    moved++;
                }
                Debug.Log($"sourcePath {sourcePath}    destPath {AssetDatabase.GetAssetPath(tex)}\n");

            }
        }
        Debug.Log($"Moved {moved}  Textures \n");
        Debug.Log($"Ignored Copied {copies}  Textures \n");
    }

    //------------------------------------
    public static void MoveMaterialsIntoFolder(List<Material> materials, string path)
    {
        // Ensure the exists and path starts with "Assets/", create if not
        ResourceUtilities.CheckOrCreateFolder(path);


        for (int i = 0; i < materials.Count; i++)
        {
            Material mat = materials[i];
            string sourcePath = AssetDatabase.GetAssetPath(mat);
            string extension = Path.GetExtension(sourcePath);
            string destPath = path + "/" + mat.name + extension;
            if (destPath != sourcePath)
            {
                bool exists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(destPath));
                if (exists)
                {
                    Debug.LogWarning($"This Asset already exists at {destPath}\n");
                    // Append "_copy" to the filename
                    int randInt = UnityEngine.Random.Range(0, 10000);
                    string copyString = $"_copy{randInt.ToString()}";
                    destPath = $"{Path.Combine(Path.GetDirectoryName(destPath), Path.GetFileNameWithoutExtension(destPath))}{copyString}{Path.GetExtension(destPath)}";
                }

                string result = AssetDatabase.MoveAsset(sourcePath, destPath);
                if (result != "")
                    Debug.Log(mat.name + "  " + result);
                Debug.Log("sourcePath  " + sourcePath + "    destPath " + AssetDatabase.GetAssetPath(mat));
            }
        }
    }

    //------------------------------------
    // Are there materials being used that haven't been put in the correct directory
    public static void MoveStrayMeshesIntoAFWBFolder()
    {
        List<Mesh> strayMeshes = FindUsedMeshesOutsideAFWBFolder(print: false);
        string correctMeshPath = "Assets/Auto Fence Builder/AFWB_Meshes";
        int movedCount = 0;
        for (int i = 0; i < strayMeshes.Count; i++)
        {
            Mesh mesh = strayMeshes[i];
            string sourcePath = AssetDatabase.GetAssetPath(mesh);
            string extension = Path.GetExtension(sourcePath);
            string destPath = correctMeshPath + "/" + mesh.name + extension;

            bool exists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(destPath));
            if (exists)
            {
                Debug.LogWarning($"This Asset already exists at {destPath}\n");
                // Append "_copy" to the filename
                int randInt = UnityEngine.Random.Range(0, 10000);
                string copyString = $"_copy{randInt.ToString()}";
                destPath = $"{Path.Combine(Path.GetDirectoryName(destPath), Path.GetFileNameWithoutExtension(destPath))}{copyString}{Path.GetExtension(destPath)}";
            }

            string result = AssetDatabase.MoveAsset(sourcePath, destPath);
            if (result != "")
            {
                Debug.Log(mesh.name + "  " + result);
                movedCount++;
            }
            Debug.Log("sourcePath  " + sourcePath + "   new dirPath " + AssetDatabase.GetAssetPath(mesh));
        }
        Debug.Log($"Moved {movedCount}  Meshes \n");
    }

    //------------------------------------
    // Are there materials being used that haven't been put in the correct directory
    public static void MoveStrayMaterialsIntoAFWBFolder()
    {
        List<Material> strayMats = FindUsedMaterialsOutsideAFWBFolder(print: false);
        string correctMaterialsPath = "Assets/Auto Fence Builder/AFWB_Materials";
        int movedCount = 0;
        for (int i = 0; i < strayMats.Count; i++)
        {
            Material mat = strayMats[i];
            string sourcePath = AssetDatabase.GetAssetPath(mat);
            string destPath = correctMaterialsPath + "/" + mat.name + ".mat";

            bool exists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(destPath));
            if (exists)
            {
                Debug.LogWarning($"This Asset already exists at {destPath}\n");
                // Append "_copy" to the filename
                int randInt = UnityEngine.Random.Range(0, 10000);
                string copyString = $"_copy{randInt.ToString()}";
                destPath = $"{Path.Combine(Path.GetDirectoryName(destPath), Path.GetFileNameWithoutExtension(destPath))}{copyString}{Path.GetExtension(destPath)}";
            }

            string result = AssetDatabase.MoveAsset(sourcePath, destPath);
            if (result != "")
            {
                Debug.Log(result);
                movedCount++;
            }
            Debug.Log($"sourcePath   {sourcePath}    new dirPath   {AssetDatabase.GetAssetPath(mat)}\n");
        }
        Debug.Log($"Moved {movedCount}  materials \n");
    }

    // input texture name to find all loaded materials that use it
    public static TextureUsage FindTextureUsageFromName(string texName, AutoFenceCreator af, bool print = false)
    {
        List<Material> matsList = BuildListOfAllLoadedMaterials(af);
        Texture2D tex = null;
        TextureUsage texUsage = new TextureUsage(tex);
        for (int i = 0; i < matsList.Count; i++)
        {
            Material mat = matsList[i];
            List<Texture2D> texList = GetAllTexturesFromMaterial(mat, false);
            for (int j = 0; j < texList.Count; j++)
            {
                tex = texList[j];
                if (tex.name == texName)
                {
                    if (print)
                        Debug.Log("Found " + texName + " in " + mat.name + "\n");
                    texUsage.tex = tex;
                    texUsage.matList.Add(mat);
                }
            }
        }
        return texUsage;
    }

    public static (Texture2D, Material) FindFirstMaterialUsingTextureName(string texName, AutoFenceCreator af, bool print = false)
    {
        List<Material> matsList = BuildListOfAllLoadedMaterials(af);
        Texture2D tex = null;
        Material mat = null;
        for (int i = 0; i < matsList.Count; i++)
        {
            mat = matsList[i];
            List<Texture2D> texList = GetAllTexturesFromMaterial(mat, false);
            for (int j = 0; j < texList.Count; j++)
            {
                tex = texList[j];
                if (tex.name == texName)
                {
                    if (print)
                        Debug.Log("Found " + texName + " in " + mat.name + "\n");
                    return (tex, mat);
                }
            }
        }
        return (tex, mat);
    }

    public static void FixPanelRailNamesInPresets()
    {
        List<string[]> presetGuidsDirList = new List<string[]>();
        string[] presetGuids = AssetDatabase.FindAssets("t:ScriptablePresetAFWB", new[] { targetPresetsFolder });
        presetGuidsDirList.Add(presetGuids);
        string[] presetGuidsBackups = AssetDatabase.FindAssets("t:ScriptablePresetAFWB", new[] { targetPresetsFolderBackups });
        presetGuidsDirList.Add(presetGuidsBackups);
        string[] presetGuidsUser = AssetDatabase.FindAssets("t:ScriptablePresetAFWB", new[] { targetPresetsFolderUser });
        presetGuidsDirList.Add(presetGuidsUser);

        int changedCount = 0; // Counter for the number of changed objects
        List<string> changedPresets = new List<string>();

        foreach (string[] presetsFolder in presetGuidsDirList)
        {
            foreach (string guid in presetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptablePresetAFWB preset = AssetDatabase.LoadAssetAtPath<ScriptablePresetAFWB>(assetPath);

                if (preset != null)
                {
                    string origNameA = preset.railAName, origNameB = preset.railBName;
                    preset.railAName = preset.railAName.Replace("_Panel_Rail", "_Panel");
                    preset.railBName = preset.railBName.Replace("_Panel_Rail", "_Panel");
                    if (preset.railAName != origNameA || preset.railBName != origNameB)
                    {
                        EditorUtility.SetDirty(preset);
                        changedCount++;
                        changedPresets.Add(preset.name);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Print the number of changed objects to the console
        Debug.Log(changedCount + " ScriptablePresetAFWB were updated.\n");
        Debug.Log("Changed Presets: " + string.Join(", ", changedPresets.ToArray()) + "\n");
    }

    //====================================================================================================
    //                                 Preset Utilities
    //====================================================================================================
    // return a string of the Layer reference where it was found, or null if not found
    public static string PresetContainsGameObject(ScriptablePresetAFWB preset, GameObject go, SearchType searchType = SearchType.All)
    {
        if (preset == null || go == null)
            return null;

        StringBuilder foundReferences = new StringBuilder();
        List<string> locations = new List<string>();

        // List<string> layerNamesInPreset = a

        // Check direct name references in the preset, if applicable
        if (searchType == SearchType.All || searchType == SearchType.DirectReferencesOnly)
        {
            string[] directLayerReferences = new[] { "postName", "railAName", "railBName", "subpostName", "extraName" };
            string[] directLayerReferenceValues = new[] { preset.postName, preset.railAName, preset.railBName, preset.subpostName, preset.extraName };

            foreach (LayerSet layer in Enum.GetValues(typeof(LayerSet)))
            {
                if (layer == LayerSet.subpostLayerSet)
                    break;

                // Your code here
            }

            for (int i = 0; i < directLayerReferences.Length; i++)
            {
                if (directLayerReferenceValues[i] == go.name)
                {
                    foundReferences.Append($"{directLayerReferences[i]}, ");
                    //locations.Add
                }
            }
        }

        // Search through SourceVariants, if applicable
        if (searchType == SearchType.All || searchType == SearchType.SourceVariantsOnly)
        {
            void SearchSourceVariantGos(List<SourceVariant> sourceVariants, string referenceName)
            {
                for (int i = 0; i < sourceVariants.Count; i++)
                {
                    var sourceVariant = sourceVariants[i];
                    if (sourceVariant != null && sourceVariant.Go != null && (sourceVariant.Go == go || sourceVariant.Go.name == go.name))
                    {
                        foundReferences.Append($"{referenceName}[{i}], ");
                        break;
                    }
                }
            }
            SearchSourceVariantGos(preset.postVariants, "postVariants");
            SearchSourceVariantGos(preset.railAVariants, "railAVariants");
            SearchSourceVariantGos(preset.railBVariants, "railBVariants");
            SearchSourceVariantGos(preset.subpostVariants, "subpostVariants");

            // Search through extraVariants, if applicable
            List<string> extraVarNames = preset.extraVarsStruct.varNames;

            for (int i = 0; i < extraVarNames.Count; i++)
            {
                string extraVarName = extraVarNames[i];
                if (extraVarName == go.name)
                {
                    foundReferences.Append($"extraVarName[{i}], ");
                    break;
                }
            }
        }
        // Remove trailing comma and space, if any
        if (foundReferences.Length > 0)
        {
            foundReferences.Length -= 2;
            return foundReferences.ToString();
        }

        return null; // No references foundPrefabs
    }
    //------------------
    //-- Same as above but uses string goName rather than GameObject go
    public static string PresetContainsGameObjectName(ScriptablePresetAFWB preset, string goName, SearchType searchType = SearchType.All)
    {
        if (preset == null || goName == "")
            return null;

        StringBuilder foundReferences = new StringBuilder();
        List<string> locations = new List<string>();


        // Check direct name references in the preset, if applicable
        if (searchType == SearchType.All || searchType == SearchType.DirectReferencesOnly)
        {
            string[] directLayerReferences = new[] { "postName", "railAName", "railBName", "subpostName", "extraName" };
            string[] directLayerReferenceValues = new[] { preset.postName, preset.railAName, preset.railBName, preset.subpostName, preset.extraName };


            for (int i = 0; i < directLayerReferences.Length; i++)
            {
                if (directLayerReferenceValues[i] == goName)
                {
                    foundReferences.Append($"{directLayerReferences[i]}, ");
                }
            }
        }

        // Search through SourceVariants, if applicable
        if (searchType == SearchType.All || searchType == SearchType.SourceVariantsOnly)
        {
            void SearchSourceVariantGos(List<SourceVariant> sourceVariants, string referenceName)
            {
                for (int i = 0; i < sourceVariants.Count; i++)
                {
                    var sourceVariant = sourceVariants[i];
                    if (sourceVariant != null && sourceVariant.Go != null)
                    {
                        if (sourceVariant.Go.name == goName || sourceVariant.goName == goName)
                            foundReferences.Append($"{referenceName}[{i}], ");
                        break;
                    }
                }
            }
            SearchSourceVariantGos(preset.postVariants, "postVariants");
            SearchSourceVariantGos(preset.railAVariants, "railAVariants");
            SearchSourceVariantGos(preset.railBVariants, "railBVariants");
            SearchSourceVariantGos(preset.subpostVariants, "subpostVariants");

            // Search through extraVariants, if applicable
            List<string> extraVarNames = preset.extraVarsStruct.varNames;

            for (int i = 0; i < extraVarNames.Count; i++)
            {
                string extraVarName = extraVarNames[i];
                if (extraVarName == goName)
                {
                    foundReferences.Append($"extraVarName[{i}], ");
                    break;
                }
            }
        }
        // Remove trailing comma and space, if any
        if (foundReferences.Length > 0)
        {
            foundReferences.Length -= 2;
            return foundReferences.ToString();
        }

        return null; // No references foundPrefabs
    }
    //-----------------------
    public static List<string> FindPresetsUsingGameObjectSimple(string goName)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;

        List<string> presetNames = new List<string>();
        foreach (var preset in presetList)
        {
            if (preset == null)
                continue;
            string foundReferenceLayer = PresetContainsGameObjectName(preset, goName, SearchType.All);
            if (foundReferenceLayer != null)
            {
                presetNames.Add($"{preset.name} (in {foundReferenceLayer}) / {preset.categoryName}\n");
            }
        }
        return presetNames;
    }
    //------------------
    //The simplest version with no layers or printing
    public static List<string> FindPresetsUsingGameObjectSimple(GameObject gameObject)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;

        if (gameObject == null)
            return new List<string>();

        List<string> presetNames = new List<string>();
        foreach (var preset in presetList)
        {
            if (preset == null)
                continue;
            string foundReferenceLayer = PresetContainsGameObject(preset, gameObject, SearchType.All);
            if (foundReferenceLayer != null)
            {
                presetNames.Add($"{preset.name} (in {foundReferenceLayer}) / {preset.categoryName}\n");
            }
        }
        return presetNames;
    }

    //----------------------------------------------------------------------------------------------------
    /*
    * Creates a detailed List of every GameObject, and which Presets use it (and in what context)
    * This can be printed to console with PrettyPrintGameObjectPresets()
    */

    public static List<List<string>> FindPresetsUsingGameObjects(List<GameObject> gameObjects)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        List<List<string>> gameObjectPresetNames = new List<List<string>>();

        foreach (var go in gameObjects)
        {
            if (go == null) continue;

            List<string> presetNames = new List<string> { $"'{go.name}' is used in:\n" };

            foreach (var preset in presetList)
            {
                if (preset == null) continue;

                string foundReferenceLayer = PresetContainsGameObject(preset, go, SearchType.DirectReferencesOnly);
                if (foundReferenceLayer != null)
                {
                    presetNames.Add($"      - {preset.name} / {preset.categoryName} (in {foundReferenceLayer}) \n");
                }
            }

            //if (presetNames.Count == 1) // Only the header was added
            //{
            //    presetNames.Add("None foundPrefabs.\n"); //??? 13/5/24
            //}

            gameObjectPresetNames.Add(presetNames);
        }
        return gameObjectPresetNames;
    }

    //-------------
    // same as above but a wrapper for a single GameObject
    public static List<List<string>> FindPresetsUsingGameObject(GameObject gameObject)
    {

        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
         List<GameObject> gameObjectsList = new List<GameObject> { gameObject };
         List<List<string>> gameObjectPresetNames = FindPresetsUsingGameObjects(gameObjectsList);
         return gameObjectPresetNames;
    }

    // Same as above but by name
    public static List<List<string>> FindPresetsUsingGameObject(string goName, PrefabTypeAFWB prefabType = PrefabTypeAFWB.allPrefab)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        GameObject go = af.FindPrefabByNameAndType(prefabType, "goName");
        List<GameObject> gameObjectsList = new List<GameObject> { go };
        List<List<string>> gameObjectPresetNames = FindPresetsUsingGameObjects(gameObjectsList);
        return gameObjectPresetNames;
    }

    public static List<GameObject> FindPrefabsUsingMesh(string meshName, List<GameObject> allPrefabs)
    {
        List<GameObject> prefabsUsingMesh = new List<GameObject>();

        foreach (var prefab in allPrefabs)
        {
            if (prefab == null)
                continue;

            MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == meshName)
            {
                prefabsUsingMesh.Add(prefab);
            }
        }

        return prefabsUsingMesh;
    }

    //--------------------------------
    public static List<GameObject> FindUnusedGameObjects(List<GameObject> gameObjects)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        List<GameObject> unusedGameObjects = new List<GameObject>();

        foreach (var go in gameObjects)
        {
            bool isUsed = false;
            foreach (var preset in presetList)
            {
                if (PresetContainsGameObject(preset, go) != null)
                {
                    isUsed = true;
                    break;
                }
            }

            if (!isUsed)
            {
                unusedGameObjects.Add(go);
            }
        }

        return unusedGameObjects;
    }

    public static void PrintUnusedGameObjects(List<GameObject> unusedGameObjects)
    {
        if (unusedGameObjects == null || unusedGameObjects.Count == 0)
        {
            Debug.Log("All GameObjects are used in presetsEd.");
            return;
        }

        Debug.Log("Unused GameObjects: \n");
        foreach (var go in unusedGameObjects)
        {
            Debug.Log($"- {go.name} \n");
        }
    }

    //------------------------------------------
    /*public static List<int> FindPresetsContainingPrefab(PrefabTypeAFWB PrefabTypeAFWB, int prefabIndex)
    {
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        List<int> matchingPresetList = new List<int>();
        ScriptablePresetAFWB preset;
        List<GameObject> prefabList = af.GetPrefabsForPrefabType(PrefabTypeAFWB);

        string prefabNameToMatch = prefabList[prefabIndex].name;

        int numPresets = presetList.Count;
        for (int i = 0; i < numPresets; i++)
        {
            preset = presetList[i];
            if (PrefabTypeAFWB == PrefabTypeAFWB.railPrefab)
            {
                if (preset.railAName == prefabNameToMatch)
                    matchingPresetList.Add(i);
                if (preset.railBName == prefabNameToMatch)
                    matchingPresetList.Add(i);
            }
            else if (PrefabTypeAFWB == PrefabTypeAFWB.postPrefab)
            {
                if (preset.postName == prefabNameToMatch)
                    matchingPresetList.Add(i);
            }
            else if (PrefabTypeAFWB == PrefabTypeAFWB.extraPrefab)
            {
                if (preset.extraName == prefabNameToMatch)
                    matchingPresetList.Add(i);
            }
        }
        return matchingPresetList;
    }*/

    public static void RenamePrefabAndUpdateAll(string oldName, string newName)
    {
        //-- First Rewname the Prefab in the Assets folder
        RenamePrefabInFolder(oldName, newName, af.currAutoFenceBuilderDir);

        //-- No ammend all the presets that use it
        if (oldName.Contains("_Rail") || oldName.Contains("_Panel"))
        {
            UpdatePresetComponentAssetName(LayerSet.railALayerSet, oldName, newName, treatBothRails: true);
            //UpdatePresetComponentAssetName(LayerSet.railALayerSet, "TrackCurbing_Rail", "TrackCurbing_Red_Rail", treatBothRails: false);
            //UpdatePresetComponentAssetName(LayerSet.railBLayerSet, "TrackCurbing_Rail", "TrackCurbing_Red_Rail", treatBothRails: false);

            UpdatePresetComponentAssetName(LayerSet.extraLayerSet, oldName, newName, treatBothRails: false);
        }
        else if (oldName.Contains("_Post"))
        {
            UpdatePresetComponentAssetName(LayerSet.postLayerSet, oldName, newName, treatBothRails: false);
            UpdatePresetComponentAssetName(LayerSet.subpostLayerSet, oldName, newName, treatBothRails: false);
            UpdatePresetComponentAssetName(LayerSet.extraLayerSet, oldName, newName, treatBothRails: false);
        }
        else if (oldName.Contains("_Extra"))
            UpdatePresetComponentAssetName(LayerSet.extraLayerSet, oldName, newName, treatBothRails: false);

        //-- Rebuild the menus
        af.CreatePrefabMenuNames();
    }

    public static void RenamePrefabInFolder(string oldName, string newName, string folderPath = "")
    {
        if (string.IsNullOrEmpty(folderPath))
            folderPath = "Assets/Auto Fence Builder";
        else if (!folderPath.StartsWith("Assets/"))
            folderPath = "Assets/" + folderPath;

        string[] folder = new string[] { folderPath };
        string filter = "t:Prefab " + oldName;
        string[] assetGUIDs = AssetDatabase.FindAssets(filter, folder);
        string[] stringPaths = ConvertGUIDArrayToStringPathArray(assetGUIDs);

        if (stringPaths.Length > 0)
        {
            foreach (string thisPath in stringPaths)
            {
                // Extract the file name without extension from the path
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(thisPath);
                // FindAssets finds any name that contains the input name, so check if the file name matches the old name exactly
                if (fileNameWithoutExtension == oldName)
                {
                    string error = AssetDatabase.RenameAsset(thisPath, newName);
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log($"Prefab renamed successfully from {oldName} to {newName} within {folderPath}\n");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to rename currPrefab: {error}\n");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"Prefab {oldName} not foundPrefabs in {folderPath}\n");
        }
    }

    public static string[] ConvertGUIDArrayToStringPathArray(string[] assetGUIDs)
    {
        if (assetGUIDs == null || assetGUIDs.Length == 0)
        {
            Debug.LogWarning("ConvertGUIDArrayToStringPathArray: No GUIDs provided\n");
            return new string[0]; // Return an empty array to indicate no paths were foundPrefabs.
        }

        string[] assetPaths = new string[assetGUIDs.Length];

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            // Convert each GUID to its corresponding asset path.
            assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

            // Optional: Log the conversion result for verification.
            // Debug.Log($"Converted GUID {assetGUIDs[i]} to path {assetPaths[i]}\n");
        }
        return assetPaths;
    }

    public enum SearchType
    {
        All,
        DirectReferencesOnly,
        SourceVariantsOnly
    }

    //--Resizes the Prefab and overwrites it as .fbx
    /*public static void ResizeModels(string sourcePath, Vector3 targetSize)
    {
        Vector3 origSize, newSize;
        string meshesPath = sourcePath;

        List<(List<Mesh>, Bounds, string)> meshesWithBounds = Housekeeping.GetAllFBXMeshesWithBounds(meshesPath, print: false);
        int count = 0;
        for (int i = 0; i < meshesWithBounds.Count; i++)
        {
            string filename = meshesWithBounds[i].Item3;
            List<Mesh> currMeshGroup = meshesWithBounds[i].Item1;
            Mesh mainMesh = currMeshGroup[0];
            Bounds currMeshGroupBounds = meshesWithBounds[i].Item2;
            Vector3 currMeshGroupSize = currMeshGroupBounds.size;

            targetSize.x = targetSize.x == 0 ? currMeshGroupSize.x : targetSize.x;
            targetSize.y = targetSize.y == 0 ? currMeshGroupSize.y : targetSize.y;
            targetSize.z = targetSize.z == 0 ? currMeshGroupSize.z : targetSize.z;

            float deltaX = Mathf.Abs(currMeshGroupSize.x - targetSize.x);
            float deltaY = Mathf.Abs(currMeshGroupSize.y - targetSize.y);
            float deltaZ = Mathf.Abs(currMeshGroupSize.z - targetSize.z);

            //calculate the largest delta
            float maxDelta = Mathf.Max(deltaX, deltaY, deltaZ);

            if (maxDelta > .00001f)
            {
                Debug.Log($"Resizing mesh group {i} -  {mainMesh.name} -  from original size.y = {currMeshGroupSize} to new size = {targetSize}");
                MeshUtilitiesAFWB.ResizeMeshXYZ(currMeshGroup, currMeshGroupSize, targetSize);
                string mainMeshPath = AssetDatabase.GetAssetPath(mainMesh);
                //string filePathWithoutExt = Path.ChangeExtension(mainMeshPath, null);
                //mainMeshPath = filePathWithoutExt + "_Y2" + ".fbx";
                FBXExportAFWB.SimpleExportMesh(mainMeshPath, mainMesh, mainMesh.name, filename);
                count++;
            }
        }
        Debug.Log($"Resized {count} meshes");
    }*/

    //----------
    public class PresetFinder : ScriptableObject
    {
        [MenuItem("Assets/Auto Fence/Find Presets Containing Prefab", false, 10)]
        private static void FindPresetsContainingPrefab()
        {
            // Get the selected prefab
            GameObject selectedPrefab = Selection.activeObject as GameObject;
            if (selectedPrefab == null)
            {
                Debug.LogWarning("No prefab selected.\n");
                return;
            }

            string prefabName = selectedPrefab.name;

            // Assuming PrefabTypeAFWB and other required objects are properly defined and accessible
            PrefabTypeAFWB prefabType = PrefabTypeAFWB.railPrefab; // Set the desired prefab type based on your logic
            List<int> matchingPresets = FindPresetsContainingPrefab(prefabType, prefabName);
            Debug.Log($"Found {matchingPresets.Count} matching presets for prefab '{prefabName}'.\n");

            foreach (int index in matchingPresets)
            {
                Debug.Log($"Preset index: {index}\n");
            }
        }

        //------------------------------------------
        public static List<int> FindPresetsContainingPrefab(PrefabTypeAFWB prefabType, string prefabName)
        {
            List<ScriptablePresetAFWB> presetList = ed.mainPresetList; // Ensure 'ed' and 'af' are accessible here
            List<int> matchingPresetList = new List<int>();
            ScriptablePresetAFWB preset;

            int numPresets = presetList.Count;
            for (int i = 0; i < numPresets; i++)
            {
                preset = presetList[i];
                if (prefabType == PrefabTypeAFWB.railPrefab)
                {
                    if (preset.railAName == prefabName || preset.railBName == prefabName)
                        matchingPresetList.Add(i);
                }
                else if (prefabType == PrefabTypeAFWB.postPrefab)
                {
                    if (preset.postName == prefabName)
                        matchingPresetList.Add(i);
                }
                else if (prefabType == PrefabTypeAFWB.extraPrefab)
                {
                    if (preset.extraName == prefabName)
                        matchingPresetList.Add(i);
                }
            }
            return matchingPresetList;
        }
    }
}