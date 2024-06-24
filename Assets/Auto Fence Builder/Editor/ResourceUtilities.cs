//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using AFWB;
using MeshUtils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
//using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

public struct TextureUsage
{
    public Texture2D tex;
    public List<Material> matList;
    public List<string> presetNameList;

    public TextureUsage(Texture2D t)
    {
        tex = t;
        matList = new List<Material>();
        presetNameList = new List<string>();
    }
}

public class ResourceUtilities
{
    private AutoFenceCreator af;
    private AutoFenceEditor ed;

    public ResourceUtilities(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    //----------------------------------------------------------------------------------------
    //Save the user-added object in to the FencePrefabs and Meshes folders
    // master can be null if not a variant
    /*public static GameObject SaveUserObjectAsFBX(GameObject userObj, GameObject master, AutoFenceCreator.PrefabTypeAFWB prefabType, AutoFenceCreator af, bool isVariant, bool addUserPrefix)
    {
        if (userObj == null)
            return null;
        if (af.currAutoFenceBuilderDir == null)
        {
            Debug.LogWarning("af.currAutoFenceBuilderDir is null in SaveUserObject()");
            return null;
        }
        GameObject result = userObj; // just in case replace fails
        string meshPath = "", prefabPath = "";
        string objName = "";
        if (userObj.name.StartsWith("[User]") == false && addUserPrefix == true)
            objName += "[User]";
        objName += userObj.name;

        string meshExtnStr = ".fbx";
        GameObject meshGO = null, exportedModel = null, prefab = null;

        if (prefabType == AutoFenceCreator.PrefabTypeAFWB.railPrefab)
        {
            if (objName.EndsWith("_Panel_Rail") == false)
            {
                objName += "_Panel_Rail";
            }
            prefabPath = af.currAutoFenceBuilderDir + "/FencePrefabs_AFWB/_Rails_AFWB/" + objName + ".prefab";
        }
        if (prefabType == AutoFenceCreator.PrefabTypeAFWB.postPrefab)
        {
            if (objName.EndsWith("_Post") == false)
                objName += "_Post";
            prefabPath = af.currAutoFenceBuilderDir + "/FencePrefabs_AFWB/_Posts_AFWB/" + objName + ".prefab";
            isVariant = false;
        }

        //-- Save Meshes
        string fbxExpPath = af.currAutoFenceBuilderDir + "/Meshes/" + objName + "_m" + meshExtnStr;

        if (af.useBinaryFBX == true)
        {
            ExportBinaryFBX(fbxExpPath, userObj);
        }
        else
        {
            if (prefabType == AutoFenceCreator.PrefabTypeAFWB.railPrefab)
            {
                // At this point the Mesh name in the Exporter is determined by the userObjName, prefab name given by master name
                // all non-alphanumeric chars will be replaced with '_'
                string meshName = userObj.name.Replace('-', 'n');
                meshName = meshName.Replace('+', 'p');
                meshName = meshName.Replace("[", "");
                meshName = meshName.Replace("]", "");
                meshName = af.StripPanelRailFromName(meshName);
                userObj.name = meshName;
                string successPath = ModelExporter.ExportObject(fbxExpPath, userObj);
            }
            else if (prefabType == AutoFenceCreator.PrefabTypeAFWB.postPrefab)
            {
                string meshName = userObj.name;
                string successPath = ModelExporter.ExportObject(fbxExpPath, userObj);
            }
        }

        //--- Load the exported fbx so we can attaxh it to our prefab
        GameObject expGO = AssetDatabase.LoadMainAssetAtPath(fbxExpPath) as GameObject;
        GameObject instExpGO = null;
        if (isVariant == false)
            instExpGO = GameObject.Instantiate(expGO) as GameObject; // can't SaveAsPrefabAsset() persistent disk asset directlyif (inst == null)
        else
            instExpGO = GameObject.Instantiate(expGO) as GameObject; // yup, I know

        if (instExpGO == null)
        {
            Debug.LogWarning("Something weird happened with the fbx export/import");
            return null;
        }

        //-- No idea why this should be necessary
        List<GameObject> allSource = MeshUtilitiesAFB.GetAllMeshGameObjectsFromGameObject(userObj);
        List<GameObject> allFBX = MeshUtilitiesAFB.GetAllMeshGameObjectsFromGameObject(expGO);
        List<GameObject> allNew = MeshUtilitiesAFB.GetAllMeshGameObjectsFromGameObject(instExpGO);

        for (int i = 0; i < allSource.Count; i++)
        {
            MeshRenderer sourceRend = allSource[i].GetComponent<MeshRenderer>();
            MeshRenderer newRend = allNew[i].GetComponent<MeshRenderer>();
            if (sourceRend != null && newRend != null)
                newRend.GetComponent<MeshRenderer>().sharedMaterial = sourceRend.GetComponent<MeshRenderer>().sharedMaterial;
        }

        if (isVariant == false)
            prefab = PrefabUtility.SaveAsPrefabAsset(instExpGO, prefabPath);
        else if (isVariant == true)
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(master, prefabPath);
            allNew = MeshUtilitiesAFB.GetAllMeshGameObjectsFromGameObject(prefab);
            for (int i = 0; i < allSource.Count; i++)
            {
                //-- Copy Renderer
                MeshRenderer sourceRend = allSource[i].GetComponent<MeshRenderer>();
                MeshRenderer newRend = allNew[i].GetComponent<MeshRenderer>();
                if (sourceRend != null && newRend != null)
                    newRend.GetComponent<MeshRenderer>().sharedMaterial = sourceRend.GetComponent<MeshRenderer>().sharedMaterial;

                //-- Copy Mesh Filter
                MeshFilter sourceFbxMF = allFBX[i].GetComponent<MeshFilter>();
                MeshFilter newMF = allNew[i].GetComponent<MeshFilter>();
                if (sourceFbxMF != null && newMF != null)
                {
                    newMF.GetComponent<MeshFilter>().sharedMesh = sourceFbxMF.GetComponent<MeshFilter>().sharedMesh;
                    //newMF.GetComponent<MeshFilter>().mesh = sourceFbxMF.GetComponent<MeshFilter>().sharedMesh;
                }
            }
        }

        GameObject.DestroyImmediate(instExpGO);
        AssetDatabase.Refresh();
        return prefab;
    }*/

    //--------------------------------------------------
    /*public static void ReloadPrefabsForLayer(LayerSet sourceLayerList, AutoFenceCreator af, bool fixRailMeshes = true)
    { //Debug.Log("LoadPrefabs()\n");
        af.postPrefabs.Clear();

        PrefabLoader prefabLoader = new PrefabLoader();

        bool prefabLayerFolderFound = prefabLoader.LoadAllPrefabLayers(this, af.extraPrefabs, af.postPrefabs, af.subPrefabs,
                af.railPrefabs, af.subJoinerPrefabs, ref af.nodeMarkerObj);

        af.needsReloading = false;
        af.prefabsLoaded = true;
        userUnloadedAssets = false;
        if (prefabLoadOK)
        {
            af.BackupPrefabMeshes(af.railPrefabs, af.origRailPrefabMeshes);
            af.BackupPrefabMeshes(af.postPrefabs, af.origPostPrefabMeshes);
            af.CreatePrefabMenuNames();
        }
    }*/

    //----------------------------------------------------------------------------------------
    public static string CreateFolderInAutoFenceBuilder(AutoFenceCreator af, string path, string folderName)
    {
        string guid = "", dirPath = af.currAutoFenceBuilderDir + "/" + path;

        //====  if we specified a dirPath, check that exists first
        if (!Directory.Exists(dirPath))
        {
            guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir, path);
        }

        if (!Directory.Exists(dirPath + "/" + folderName))
        {
            guid = AssetDatabase.CreateFolder(dirPath, folderName);
        }
        string fullPath = AssetDatabase.GUIDToAssetPath(guid);
        return fullPath;
    }

    //-------------------------------------
    //Saves the procedurally generated Rail meshes produced when using Sheared mode as prefabs, in order to create a working prefab from the Finished AutoFence
    public static void SaveGOMeshes(List<GameObject> meshGOs, string pathFromAssetsFolder, string folderName)
    {
        string destFolder = pathFromAssetsFolder + "/" + folderName;

        if (!Directory.Exists(destFolder))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(destFolder);
        }

        int numObjects = meshGOs.Count;
        for (int i = 0; i < numObjects; i++)
        {
            if (meshGOs[i] == null)
                continue;
            MeshFilter mf = meshGOs[i].GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh mesh = mf.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                {
                    string assetName = mesh.name + ".asset";
                    string savePath = destFolder + "/" + assetName;
                    if (Directory.Exists(destFolder))
                    {
                        AssetDatabase.CreateAsset(mesh, savePath);
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
    }

   

    
    //-----------
    /*public static void RenameFBXMesh(string fbxFilePath, string newName)
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
                    return;
            }
            else
            {
                Debug.Log("No MeshFilters found in " + fbxFilePath);
                return;
            }

            int meshCounter = 0;
            foreach (var meshFilter in meshFilters)
            {
                string newMeshName = newName;
                if (meshFilters.Length > 1)
                {
                    newMeshName += (meshCounter + 1).ToString();
                }
                meshFilter.sharedMesh.name = newMeshName;
                //changedNames.Add($"{filenameWithoutExtension}:   changed   {origFirstMeshName}   to   {newMeshName}");
                //meshCounter++;
            }

            // Export the modified GameObject back to an FBX file
            string exportPath = fbxFilePath; // Overwrite the existing file
            string successPath = ModelExporter.ExportObject(exportPath, fbxGameObject);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }*/
    //-------------------------------------------------------------------------
    // return false if the asset already existed
    public static GameObject SaveGameObjectToPath(GameObject go, string path)
    {
        bool assetAlreadyExists = AssetDatabase.Contains(go);
        if (assetAlreadyExists == false)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            AssetDatabase.Refresh();
            return prefab;
        }
        else
            Debug.Log(go.name + " already existed. Not saved. \n");

        return null;
    }

    //-------------
    // return false if the asset already existed
    public static bool SaveMeshToPath(Mesh mesh, string path)
    {
        bool assetAlreadyExists = AssetDatabase.Contains(mesh);
        if (assetAlreadyExists == false)
        {
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.Refresh();
            return true;
        }
        else
            Debug.Log(mesh.name + " already existed. Mesh not saved. \n");

        return false;
    }

    private void SavePrefabCopyWithAppendedName(GameObject prefab, string stringToAppend)
    {
        SavePrefabCopyWithName(prefab, prefab.name + stringToAppend);
    }



   

    //---------------------------
    public static List<Texture> BuildListOfAllLoadedTextures(AutoFenceCreator af, bool printList = false)
    {
        List<Texture> allTextures = new List<Texture>();
        for (int i = 0; i < af.railPrefabs.Count; i++)
        {
            GameObject go = af.railPrefabs[i];
            Renderer rend = go.GetComponent<Renderer>();

            if (rend != null)
            {
                Material mat = rend.sharedMaterial;
                if (mat == null)
                    continue;
                Shader shader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                //Debug.Log(i.ToString() + "    ------------\n");
                for (int j = 0; j < propertyCount; j++)
                {
                    if (ShaderUtil.GetPropertyType(shader, j) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture texture = rend.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, j));
                        if (texture != null)
                        {
                            if (allTextures.Contains(texture) == false)
                            {
                                allTextures.Add(texture);
                                if (printList)
                                    Debug.Log(allTextures.Count + "   " + texture.name + "\n");
                            }
                        }
                    }
                }
            }
        }
        for (int i = 0; i < af.postPrefabs.Count; i++)
        {
            GameObject go = af.postPrefabs[i];
            Renderer rend = go.GetComponent<Renderer>();

            if (rend != null)
            {
                Material mat = rend.sharedMaterial;
                if (mat == null)
                    continue;
                Shader shader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                //Debug.Log(i.ToString() + "    ------------\n");
                for (int j = 0; j < propertyCount; j++)
                {
                    if (ShaderUtil.GetPropertyType(shader, j) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture texture = rend.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, j));
                        if (texture != null)
                        {
                            if (allTextures.Contains(texture) == false)
                            {
                                allTextures.Add(texture);
                                //Debug.Log(allTextures.Count + "   " + texture.name + "\n");
                            }
                        }
                    }
                }
            }
        }
        for (int i = 0; i < af.extraPrefabs.Count; i++)
        {
            GameObject go = af.extraPrefabs[i];
            Renderer rend = go.GetComponent<Renderer>();

            if (rend != null)
            {
                Material mat = rend.sharedMaterial;
                if (mat == null)
                    continue;
                Shader shader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                //Debug.Log(i.ToString() + "    ------------\n");
                for (int j = 0; j < propertyCount; j++)
                {
                    if (ShaderUtil.GetPropertyType(shader, j) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture texture = rend.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, j));
                        if (texture != null)
                        {
                            if (allTextures.Contains(texture) == false)
                            {
                                allTextures.Add(texture);
                                //Debug.Log(allTextures.Count + "   " + texture.name + "\n");
                            }
                        }
                    }
                }
            }
        }

        /*string dirPath = "Auto Fence Builder";
        Texture[] assetsTextureArray = GetAssetsAtPath<Texture>(dirPath);
        for (int i = 0; i < assetsTextureArray.Length; i++)
        {
           Debug.Log("Assets//Auto Fence Builder " + i + "   " + assetsTextureArray[i].name + "\n");
        }
        IsAssetTextureInPrefabs(assetsTextureArray, allTextures);*/

        return allTextures;
    }

    //----------------------
    //CreateMergedPrefabs List of every material used in all the prefabs
    public static List<ScriptablePresetAFWB> CheckPresetsForMissingPrefabs(AutoFenceEditor ed, bool print = false)
    {
        List<ScriptablePresetAFWB> presetList = new List<ScriptablePresetAFWB>();
        GameObject postGo = null, railGo = null, extraGO = null;
        Material material = null;
        List<Texture2D> texList = new List<Texture2D>();
        for (int i = 0; i < ed.mainPresetList.Count; i++)
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[i];

            postGo = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.postPrefab, preset.postName);
            if (postGo == null)
            {
                presetList.Add(preset);
                Debug.Log("Post in " + preset.name + "\n");
            }
            railGo = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, preset.railAName);
            if (railGo == null)
            {
                presetList.Add(preset);
                Debug.Log("Rail A in " + preset.name + "\n");
            }
            railGo = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, preset.railBName);
            if (railGo == null)
            {
                presetList.Add(preset);
                Debug.Log("Rail B in " + preset.name + "\n");
            }
            extraGO = ed.af.FindPrefabByNameAndType(PrefabTypeAFWB.extraPrefab, preset.extraName);
            if (extraGO == null)
            {
                presetList.Add(preset);
                Debug.Log("Extra in " + preset.name + "\n");
            }
        }
        return presetList;
    }

    //----------------------
    //CreateMergedPrefabs List of every material used in all the prefabs
    public static List<ScriptablePresetAFWB> FindPresetsUsingMaterial(Material mat, AutoFenceEditor ed, bool print = false)
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
                    if (material == mat)
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
                    if (material == mat)
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
                    if (material == mat)
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
                    if (material == mat)
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

    //----------------------
    public static List<Mesh> GetAllMeshesFromFBX(string assetPath)
    {
        List<Mesh> meshes = new List<Mesh>();

        // Load all assets at the given path
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

        // Iterate through all assets and add meshes to the list
        foreach (UnityEngine.Object asset in assets)
        {
            if (asset is Mesh)
            {
                meshes.Add(asset as Mesh);
            }
        }

        return meshes;
    }

    //----------------------
    public static bool MovePrefab(GameObject prefab, string newPath)
    {
        // Check if the GameObject provided is a prefab
        if (prefab == null || !AssetDatabase.Contains(prefab))
        {
            Debug.LogError("MovePrefab Error: The provided GameObject is not a valid prefab.");
            return false;
        }

        // Get the current path of the prefab
        string currentPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(currentPath))
        {
            Debug.LogError("MovePrefab Error: Failed to locate the prefab in the asset database.");
            return false;
        }

        // Check if there's already a file at the new path
        if (AssetDatabase.LoadAssetAtPath<GameObject>(newPath))
        {
            Debug.LogError("MovePrefab Error: A prefab already exists at the new path.");
            return false;
        }

        // Attempt to move the prefab
        AssetDatabase.StartAssetEditing(); // Start editing the asset database
        string error = AssetDatabase.MoveAsset(currentPath, newPath);
        AssetDatabase.StopAssetEditing(); // Stop editing the asset database

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("MovePrefab Error: " + error);
            return false;
        }

        // Refresh the AssetDatabase to show changes
        AssetDatabase.Refresh();

        Debug.Log("Prefab moved successfully from " + currentPath + " to " + newPath);
        return true;
    }

    //-----------------

    /// <summary>
    /// Checks if a folder exists at the specified path, and if not, creates it, including any necessary intermediate parent folders.
    /// </summary>
    /// <param name="path">The path where the folder should exist. This should start with "Assets/".</param>
    /// <returns>True if the folder already exists or was successfully created, false otherwise.</returns>
    public static bool CheckOrCreateFolder(string path)
    {
        // Ensure the path starts with "Assets/"
        if (!path.StartsWith("Assets/"))
            path = Path.Combine("Assets", path);

        // Split the path into parts
        string[] parts = path.Split('/');
        string currentPath = parts[0];

        // Iterate through each part of the path
        for (int i = 1; i < parts.Length; i++)
        {
            currentPath = Path.Combine(currentPath, parts[i]);

            // Check if the current path is a valid folder
            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                // Create the folder if it doesn't exist
                string parentFolder = Path.GetDirectoryName(currentPath);
                string newFolderName = Path.GetFileName(currentPath);

                // Create the new folder under the parent folder
                AssetDatabase.CreateFolder(parentFolder, newFolderName);

                // Check if the new folder was successfully created
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    Debug.LogWarning($"Failed to create folder at path: {currentPath}");
                    return false;
                }
            }
        }

        return true;
    }

    //-------------
    public static bool SaveRandomLookup(RandomLookupAFWB randLookup, string savePath)
    {
        AssetDatabase.CreateAsset(randLookup, savePath);
        AssetDatabase.SaveAssets();
        return true;
    }

    private void SavePrefabCopyWithName(GameObject prefab, string name = "")
    {
        GameObject copy = GameObject.Instantiate(prefab); // a backup so we can undo any rotations etc
        if (name != "")
            copy.name = name;
        string userRailPrefabsFolderPath = PrefabLoader.GetUserRailsFolderPath(af);
        //string filePath = userRailPrefabsFolderPath + "/" + copy.name + ".prefab";
        GameObject go = PrefabAssignEditor.SaveUserGameObjectInUserAssetsFolder(copy, PrefabTypeAFWB.railPrefab, ed.af);
        GameObject.DestroyImmediate(copy);
    }

    //------------------------------------
    public static List<UnityEngine.Object> GetAssetsAtPath<T>(string path, bool printList = false)
    {
        //List<T> assets = new List<T>();
        List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
        string[] allFilePaths = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (string fileName in allFilePaths)
        {
            //int index = fileName.LastIndexOf("/");
            //string localPath = "Assets/" + dirPath;

            //if (index > 0)
            //dirPath += fileName.Substring(index);

            //UnityEngine.Object t = Resources.LoadAssetAtPath();
            UnityEngine.Object thisAsset = AssetDatabase.LoadAssetAtPath(fileName, typeof(T));
            if (thisAsset != null)
            {
                assets.Add(thisAsset);
                if (printList)
                    Debug.Log(assets.Count + "   " + thisAsset.name + "\n");
            }
        }
        //T[] result = new T[assets.Count];
        //for (int i = 0; i < assets.Count; i++)
        //result[i] = (T)assets[i];

        return assets;

        /*ArrayList arr = new ArrayList();
        string[] allFilePaths = Directory.GetFiles(dirPath);
        //string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + dirPath);
        foreach (string fileName in allFilePaths)
        {
            int index = fileName.LastIndexOf("/");
            string localPath = "Assets/" + dirPath;

            if (index > 0)
                localPath += fileName.Substring(index);

            //UnityEngine.Object t = Resources.LoadAssetAtPath();
            UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));
            if (t != null)
            {
                arr.Add(t);
                if (printList)
                    Debug.Log(arr.Count + "   " + arr.name + "\n");
            }
        }
        T[] result = new T[arr.Count];
        for (int i = 0; i < arr.Count; i++)
            result[i] = (T)arr[i];

        return result;*/
    }

    //------------------------------------
    /*static public T[] GetAssetsInAFWBFolder<T>(AutoFenceCreator af, bool printList = false)
    {
        T[] allAssets = GetAssetsAtPath<T>(af.autoFenceBuilderDefaultDir, printList);
        return allAssets;
    }*/
}