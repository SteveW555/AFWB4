using MeshUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCT.PrintUtils;
using UnityEditor;
using UnityEngine;

//using System;

namespace AFWB
{
    public class PrefabLoader
    {
        private AutoFenceCreator af = null;

        public PrefabLoader(AutoFenceCreator af)
        {
            this.af = af;
        }

        //-------------------------------------
        // Only called by ed.LoadPrefabs. Keep it that way!
        public bool LoadAllPrefabLayers(AutoFenceEditor ed)
        {
            string prefabsFolderPath = GetPrefabsFolderPath(ed.af);
            if (prefabsFolderPath == "")
                return false;
            string userPrefabsFolderPath = GetUserPrefabsFolderPath(ed.af);
            if (userPrefabsFolderPath == "")
                return false;

            
            //      Load System Objects 
            //===================================
            string systemDefaultPath = af.currAutoFenceBuilderDir + "/System_Do_Not_Remove";
            string[] systemDefaultsFilePaths = Directory.GetFiles(systemDefaultPath, "*.*", SearchOption.AllDirectories);
            LoadSystemObjects(systemDefaultPath);
            CreateFallbackObjects();
            
            //    Load Post Prefabs
            //=====================================
            LoadAllPrefabsForLayer(af, LayerSet.postLayer);
            
            //    Load Rail Prefabs
            //=====================================
            string[] railFilePaths = GetCombinedFilePaths(prefabsFolderPath + "/_Rails_AFWB/", userPrefabsFolderPath + "/UserPrefabs_Rails/");
            if (railFilePaths == null) return false;
            LoadAllPrefabsForLayer(af, LayerSet.railALayer);
            
            //    Load Extra Prefabs
            //=====================================
            string[] extrasFilePaths = GetCombinedFilePaths(prefabsFolderPath + "/_Extras_AFWB/", userPrefabsFolderPath + "/UserPrefabs_Extras/");
            if (extrasFilePaths == null) return false;
            LoadAllPrefabsForLayer(af, LayerSet.extraLayer);


            
            //-- Share and copy some of the blayers with each other

            //    Add Extras to Post Prefabs
            //=====================================
            AddExtraPrefabsToPosts();
            
            //    Add Posts to Extra Prefabs
            //=====================================
            AddPostPrefabsToExtras();

            //    Add Rails to Extra Prefabs
            //=====================================
            AddRailPrefabsToExtras();

            af.postPrefabs.Sort((x, y) => string.Compare(x.name, y.name));

            // Load SubJoiners
            foreach(string filePath in systemDefaultsFilePaths)
            {
                if (filePath.EndsWith(".prefab"))
                {
                    string fileName = Path.GetFileName(filePath);
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(filePath) as GameObject;
                    if (go != null && fileName.Contains("_SubJoiner"))
                        af.subJoinerPrefabs.Add(go);
                }
            }
            return true;
        }

        //-----------------------------------------------
        public static List<GameObject> LoadAllPrefabsForLayer(AutoFenceCreator af, LayerSet layer)
        {
            //string[] prefabsPaths = AssetDatabase.FindAssets("AFWB_Prefabs");
           // string[] userPrefabsPaths = AssetDatabase.FindAssets("UserAssets_AFWB");

           /* string prefabsFolderPath = GetPrefabsFolderPath(af);
            if (prefabsFolderPath == "")
                return null;
            string userPrefabsFolderPath = GetUserPrefabsFolderPath(af);
            if (userPrefabsFolderPath == "")
                return null;*/

            

            /*string[] filePaths = null;
            try
            {
                filePaths = Directory.GetFiles(prefabsFolderPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing FencePrefabs Folder. AFWB_Prefabs folder must be at Auto Fence Builder/AFWB_Prefabs   " + e.ToString());
                return null;
            }*/

            List<GameObject> prefabsForLayer = af.GetPrefabsForLayer(layer, warn: false);
            prefabsForLayer.Clear();
            List<PrefabDetails> prefabDetails = PrefabDetails.GetPrefabDetailsForLayer(layer, af);

            prefabDetails.Clear();
            string[] prefabFilePathsForLayer = GetPrefabFilePathsForLayer(layer, af);
            foreach (string layerFilePath in prefabFilePathsForLayer)
            {
                if (layerFilePath.EndsWith(".prefab") && layerFilePath.Contains("[Pure]") == false)
                {
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(layerFilePath) as GameObject;
                    //if (go.name.Contains("Boul"))
                        //Debug.Log("Boul");
                    if (go != null)
                    {
                        //      Rails
                        //=========================
                        if ((layer == LayerSet.railALayer || layer == LayerSet.railBLayer) && GoHasRailSuffix(go) == true)
                        {
                            if (MeshUtilitiesAFWB.GetFirstMeshInGameObject(go) != null)
                            {
                                prefabsForLayer.Add(go);
                                string parentFolder = System.IO.Path.GetDirectoryName(layerFilePath);
                                prefabDetails.Add(new PrefabDetails(parentFolder)); 
                            }
                            //else
                                //Debug.Log($"Missing Mesh while Loading Prefab  {layerFilePath}  Look in the path directory and delete or fix the prefab\n");
                        }

                        //      Posts
                        //=========================
                        else if (layer == LayerSet.postLayer && go.name.EndsWith("_Post"))
                        {
                            if (MeshUtilitiesAFWB.GetFirstMeshInGameObject(go) != null)
                            {
                                prefabsForLayer.Add(go);
                                string parentFolder = System.IO.Path.GetDirectoryName(layerFilePath);
                                prefabDetails.Add(new PrefabDetails(parentFolder));
                            }
                            //else
                                //Debug.Log($"Missing Mesh while Loading Prefab  {layerFilePath}  Look in the path directory and delete or fix the prefab\n");
                        }
                        //      Extras
                        //=========================
                        else if (layer == LayerSet.extraLayer && go.name.EndsWith("_Extra"))
                        {
                            if (MeshUtilitiesAFWB.GetFirstMeshInGameObject(go) != null)
                            {
                                prefabsForLayer.Add(go);
                                //--GetFileName(). Stupid name, actually gets the thing at the end of the path, even if not a file.
                                string parentFolder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(layerFilePath));

                                prefabDetails.Add(new PrefabDetails(parentFolder)); 
                            }
                            //else
                               //Debug.Log($"Missing Mesh while Loading Prefab  {layerFilePath}  Look in the path directory and delete or fix the prefab\n");
                        }
                    }
                    else if (go == null)
                        Debug.LogWarning($"{layer.ToString()} was null in  LoadAllPrefabLayers()  " + layerFilePath + "\n");
                    else if (LayerAndSuffixMatch(layer, go) == false)
                        Debug.LogWarning($"Prefab in {layer.ToString()} folder not named{layer.ToString()}:  " + go.name + "    " + layerFilePath + "\n");
                }
            }
			
		 return prefabsForLayer;
        }
        //--------------
        void LoadAllPrefabsFromDirectory()
        {



        }
        //---------
        //-- This is a parallel List to the prefabs Lists. Dont want the hassle of embedding in the prefabs Lists inside another wrapper
        public void AddPrefabDetails(GameObject prefab, string parentDir, LayerSet layer)
        {
            PrefabDetails prefabDetails = new PrefabDetails(parentDir);

            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                af.railPrefabDetails.Add(prefabDetails);
            else if (layer == LayerSet.postLayer)
                af.postPrefabDetails.Add(prefabDetails);
            else if (layer == LayerSet.extraLayer)
                af.extraPrefabDetails.Add(prefabDetails);
        }
        
        //-------------------------------------
        private void AddExtraPrefabsToPosts()
        {
            List<GameObject> posts = af.GetPrefabsForLayer(LayerSet.postLayer, warn: false);
            List<GameObject> exs = af.GetPrefabsForLayer(LayerSet.extraLayer, warn: false);
            posts.AddRange(exs);
        }
        //-------------------------------------
        private void AddPostPrefabsToExtras()
        {
            List<GameObject> posts = af.GetPrefabsForLayer(LayerSet.postLayer, warn: false);
            List<GameObject> exs = af.GetPrefabsForLayer(LayerSet.extraLayer, warn: false);

            for (int i = 0; i < posts.Count; i++)
            {
                string prefabName = posts[i].name;
                if (prefabName.Contains("_Extra") == false)
                {
                    exs.Add(posts[i]);
                }
            }
        }
        //-------------------------------------
        private void AddRailPrefabsToExtras()
        {
            List<GameObject> rails = af.GetPrefabsForLayer(LayerSet.railALayer, warn: false);
            List<GameObject> exs = af.GetPrefabsForLayer(LayerSet.extraLayer, warn: false);

            for (int i = 0; i < rails.Count; i++)
            {
                string prefabName = rails[i].name;
                //-- It shouldn't be _Extra as we didn't add extras to rails, but leave for future use
                if (prefabName.Contains("_Extra") == false)
                {
                    exs.Add(rails[i]);
                }
            }
        }
        //------------------------------------
        public static bool GoHasRailSuffix(GameObject go)
        {
            bool hasRailSuffix = false;
            if (go.name.EndsWith("_Rail") || go.name.EndsWith("_Panel"))
                hasRailSuffix = true;
            return hasRailSuffix;
        }
        //-------------------------------------
        public static bool LayerAndSuffixMatch(LayerSet layer, GameObject go)
        {
            bool match = false;

            if ((layer == LayerSet.railALayer || layer == LayerSet.railBLayer) && GoHasRailSuffix(go))
                match = true;
            else if (layer == LayerSet.postLayer && go.name.EndsWith("_Post"))
                match = true;
            else if (layer == LayerSet.extraLayer && go.name.EndsWith("_Extra"))
                match = true;
            return match;
        }
        //-------------------------------------
        [MenuItem("GameObject/AutoFence: Add To Posts", false, 10)]
        private static void CopyGameObjectToPrefabsFolder(MenuCommand menuCommand)
        {
            //============= Add To Posts From Context Menu ==============

            GameObject selectedGameObject = menuCommand.context as GameObject;

            if (selectedGameObject == null)
            {
                Debug.LogError("No GameObject selected!");
                return;
            }

            AutoFenceCreator af = AutoFenceCreator.GetAutoFenceCreator();
            if (af == null)
            {
                Debug.LogError("Couldn't find AutoFenceCreator. Giving up");
                return;
            }

            string postsPrefabsFolder = af.currPostPrefabsDir;
            string prefabName = "AAA" + selectedGameObject.name; // Prepare new prefab name with prefix
            if (!prefabName.EndsWith("_Post")) prefabName += "_Post"; // Ensure it ends with "_Post"

            string path = AssetDatabase.GenerateUniqueAssetPath($"{postsPrefabsFolder}/{prefabName}.prefab");

            // Check if the Prefabs folder exists, if not, create it
            if (!System.IO.Directory.Exists(postsPrefabsFolder))
            {
                System.IO.Directory.CreateDirectory(postsPrefabsFolder);
                AssetDatabase.Refresh();
            }

            // Create the new prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(selectedGameObject, path);

            if (savedPrefab != null)
            {
                Debug.Log($"Prefab copied successfully: {path}");
                LoadAllPrefabsForLayer(af, LayerSet.postLayer);
                int indexOfNewPrefab = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, prefabName);
                if (indexOfNewPrefab != -1)
                {
                    af.currentPost_PrefabIndex = indexOfNewPrefab;
                    af.currentPost_PrefabMenuIndex = af.ConvertPostPrefabIndexToMenuIndex(indexOfNewPrefab);
                    Debug.Log("currentPost_PrefabIndex " + af.currentPost_PrefabIndex);
                    Debug.Log("currentPost_PrefabMenuIndex " + af.currentPost_PrefabMenuIndex);
                    af.ForceRebuildFromClickPoints();
                }
            }
            else
            {
                Debug.LogError("Failed to copy GameObject to prefabs folder.");
            }
        }

        //-------------------------------------
        // Validate the menu item
        [MenuItem("GameObject/AutoFence: Add To Posts", true)]
        private static bool ValidateCopyGameObjectToPrefabsFolder(MenuCommand menuCommand)
        {
            // The option is available only if a GameObject is selected
            return Selection.activeGameObject != null;
        }

        //-------------------------------------
        public static string GetPrefabsFolderPath(AutoFenceCreator af)
        {
            string[] prefabsPaths = AssetDatabase.FindAssets("AFWB_Prefabs");
            string[] userPrefabsPaths = AssetDatabase.FindAssets("UserAssets_AFWB");
            string prefabsFolderPath;

            if (prefabsPaths.Length == 0 || prefabsPaths[0] == "")
            {
                Debug.LogWarning("Couldn't find prefabsPaths   Length " + prefabsPaths.Length + "\n");
                return "";
            }
            else
            {
                prefabsFolderPath = AssetDatabase.GUIDToAssetPath(prefabsPaths[0]);
                if (prefabsFolderPath != af.prefabsDefaultDir && prefabsFolderPath != af.currPrefabsDir)
                {
                    Debug.LogWarning("prefabsPaths is not at Current OR Default prefabs path" + "\n");
                    //ed.CheckFolderLocations(false);
                }
                return prefabsFolderPath;
            }
        }

        //-------------------------------------
        public static string GetUserPrefabsFolderPath(AutoFenceCreator af)
        {
            string[] prefabsPaths = AssetDatabase.FindAssets("AFWB_Prefabs");
            string[] userPrefabsPaths = AssetDatabase.FindAssets("UserAssets_AFWB");
            string prefabsFolderPath, userPrefabsFolderPath;

            if (prefabsPaths.Length == 0 || prefabsPaths[0] == "")
            {
                Debug.LogWarning("Couldn't find prefabsPaths   Length " + prefabsPaths.Length + "\n");
                return "";
            }
            else
            {
                prefabsFolderPath = AssetDatabase.GUIDToAssetPath(prefabsPaths[0]);
                if (prefabsFolderPath != af.prefabsDefaultDir)
                {
                    Debug.LogWarning("prefabsPaths is not at Current OR Default prefabs path" + "\n");
                    //ed.CheckFolderLocations(false);
                }
            }
            if (userPrefabsPaths.Length == 0 || userPrefabsPaths[0] == "")
            {
                Debug.LogWarning("Couldn't find userPrefabsPaths   Length " + userPrefabsPaths.Length + "\n");
                return "";
            }
            else
            {
                userPrefabsFolderPath = AssetDatabase.GUIDToAssetPath(userPrefabsPaths[0]);
                return userPrefabsFolderPath;
            }
        }

        public static string GetUserRailsFolderPath(AutoFenceCreator af)
        {
            string userPrefabsFolderPath = GetUserPrefabsFolderPath(af);
            if (userPrefabsFolderPath == "")
                return "";
            string userRailPrefabsFolderPath = userPrefabsFolderPath + "/UserPrefabs_Rails/";
            return userRailPrefabsFolderPath;
        }



        //------------------------------------
        public static string[] GetPrefabFilePathsForLayer(LayerSet layer, AutoFenceCreator af)
        {
            string[] filePaths = new string[0];
            string prefabsFolderPath = GetPrefabsFolderPath(af);
            string userPrefabsFolderPath = GetUserPrefabsFolderPath(af);
            string[] userPostFilePaths = null;

            //==== Posts ====
            if (layer == LayerSet.postLayer)
            {
                string postPrefabsFolderPath = prefabsFolderPath + "/_Posts_AFWB/";
                try
                {
                    filePaths = Directory.GetFiles(postPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Missing FencePrefabs Posts Folder. The _Posts_AFWB folder must be within [...]/Auto Fence Builder/AFWB_Prefabs/  " + e.ToString());
                    return new string[0];
                }
                //      User Posts
                //=======================
                string userPostPrefabsFolderPath = userPrefabsFolderPath + "/UserPrefabs_Posts/";
                userPostFilePaths = Directory.GetFiles(userPostPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
                //-- Combine AFWB & User
                System.Array.Resize(ref filePaths, filePaths.Length + userPostFilePaths.Length);
                System.Array.Copy(userPostFilePaths, 0, filePaths, filePaths.Length - userPostFilePaths.Length, userPostFilePaths.Length);
            }
            //==== Rails ====
            else if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
            {
                string railPrefabsFolderPath = prefabsFolderPath + "/_Rails_AFWB/";
                try
                {
                    filePaths = Directory.GetFiles(railPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Missing FencePrefabs Rails Folder. The _Rails_AFWB folder must be within [...]/Auto Fence Builder/AFWB_Prefabs/  " + e.ToString());
                    return new string[0];
                }
                //      User Rails
                //=========================
                string userRailPrefabsFolderPath = userPrefabsFolderPath + "/UserPrefabs_Rails/";
                string[] userRailFilePaths = Directory.GetFiles(userRailPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
                System.Array.Resize(ref filePaths, filePaths.Length + userRailFilePaths.Length);
                System.Array.Copy(userRailFilePaths, 0, filePaths, filePaths.Length - userRailFilePaths.Length, userRailFilePaths.Length);
            }
            //==== Extras ====
            else if (layer == LayerSet.extraLayer)
            {
                string extrasPrefabsFolderPath = prefabsFolderPath + "/_Extras_AFWB/";
                try
                {
                    filePaths = Directory.GetFiles(extrasPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Missing FencePrefabs Extras Folder. The _Extras_AFWB folder must be within [...]/Auto Fence Builder/AFWB_Prefabs/  " + e.ToString());
                    return new string[0];
                }
                //- User Extras -
                string userExtraPrefabsFolderPath = userPrefabsFolderPath + "/UserPrefabs_Extras/";
                string[] userExtraFilePaths = Directory.GetFiles(userExtraPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
                //Combine AFWB & User
                System.Array.Resize(ref filePaths, filePaths.Length + userExtraFilePaths.Length);
                System.Array.Copy(userExtraFilePaths, 0, filePaths, filePaths.Length - userExtraFilePaths.Length, userExtraFilePaths.Length);
            }
            return filePaths;
        }



        private static void HandleMissingMesh(AutoFenceEditor ed, List<GameObject> extraPrefabs, List<GameObject> postPrefabs, List<GameObject> subPrefabs, string filePath, GameObject go)
        {
            string debug = DebugUtilitiesTCT.GetClassAndMethodDetails();

            List<string> presetsWithGo = Housekeeping.FindPresetsUsingGameObjectSimple(go);
            if (presetsWithGo.Count == 0)
                Debug.Log($"Missing Mesh on prefab  {filePath}  :   Unused\n {debug}");
            else
                Debug.Log($"Missing Mesh on prefab  {filePath}  :  {presetsWithGo.Count} Uses\n{debug}");
            PrintUtilities.PrintList(presetsWithGo, "Presets using this GameObject: ");

            // As the Mesh is Missing, we will add a default mesh to the prefab
            Mesh newMesh = MeshUtilitiesAFWB.CreateBoxMesh(0.25f, 2, 0.25f);
            string newMeshName = go.name + "Mesh";
            newMesh.name = newMeshName;
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf == null)
                mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = newMesh;

            //FBXExportAFWB.SimpleExportMeshAsGameObject(ed.af, filePath, go, newMeshName); //TODO

            postPrefabs.Add(go);
            subPrefabs.Add(go); //Posts are also used as subpostsPool
            extraPrefabs.Add(go);  //Posts are also used as extrasPool
        }

        private static void HandleNullGo(string filePath, GameObject go, LayerSet layer, AutoFenceCreator af)
        {
            if (go == null)
            {
                Debug.LogWarning($"{af.GetLayerNameAsString(layer)} was null in  LoadAllPrefabLayers() {filePath} \n");
            }
            else if (go.name.EndsWith("_Rail") == false)
            {
                Debug.LogWarning("  Prefab in Post folder not named _Post:  " + go.name + "    " + filePath + "\n");
            }
        }

        //-----------------------
        //-- Gets AFWB Prefabs abd User Prefabs and combines them
        private string[] GetCombinedFilePaths(string systemFolderPath, string userFolderPath)
        {
            string[] prefabFilePaths = null;
            string[] userFilePaths = null;

            try
            {
                prefabFilePaths = Directory.GetFiles(systemFolderPath, "*.*", SearchOption.AllDirectories);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Missing Folder: {systemFolderPath}. {e.ToString()}");
                return null;
            }

            if (userFolderPath != "")
            {
                try
                {
                    userFilePaths = Directory.GetFiles(userFolderPath, "*.*", SearchOption.AllDirectories);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Missing User Folder: {userFolderPath}. {e.ToString()}");
                }
                // Combine system and user file paths
                if (userFilePaths != null)
                {
                    System.Array.Resize(ref prefabFilePaths, prefabFilePaths.Length + userFilePaths.Length);
                    System.Array.Copy(userFilePaths, 0, prefabFilePaths, prefabFilePaths.Length - userFilePaths.Length, userFilePaths.Length);
                }
            }
            return prefabFilePaths;
        }

        //-------------------------------------
        /*public bool LoadAllPrefabLayers(AutoFenceEditor ed, List<GameObject> extraPrefabs, List<GameObject> postPrefabs, List<GameObject> subPrefabs,
            List<GameObject> railPrefabs, List<GameObject> subJoinerPrefabs, ref GameObject nodeMarkerObj)
        {//Debug.Log("LoadAllPrefabLayers\n");
            string[] prefabsPaths = AssetDatabase.FindAssets("AFWB_Prefabs");
            string[] userPrefabsPaths = AssetDatabase.FindAssets("UserAssets_AFWB");

            string prefabsPaths = GetPrefabsFolderPath(ed.af);
            if (prefabsPaths == "")
                return false;
            string userPrefabsPaths = GetUserPrefabsFolderPath(ed.af);
            if (userPrefabsPaths == "")
                return false;

            string[] filePaths = null, postFilePaths = null, railFilePaths = null, extrasFilePaths = null;
            string[] userPostFilePaths = null, userRailFilePaths = null, userExtraFilePaths = null;
            try
            {
                filePaths = Directory.GetFiles(prefabsPaths);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing FencePrefabs Folder. The FencePrefabs folder must be at Assets/Auto Fence Builder/FencePrefabs   " + e.ToString());
                return false;
            }

            //==== Posts ====
            string postPrefabsFolderPath = prefabsPaths + "/_Posts_AFWB/";
            try
            {
                postFilePaths = Directory.GetFiles(postPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing FencePrefabs Posts Folder. The _Posts_AFWB folder must be within [...]/Auto Fence Builder/AFWB_Prefabs/  " + e.ToString());
                return false;
            }

            //      User Posts
            //=======================
            string userPostPrefabsFolderPath = userPrefabsPaths + "/UserPrefabs_Posts/";
            userPostFilePaths = Directory.GetFiles(userPostPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            //-- Combine AFWB & User
            System.Array.Resize(ref postFilePaths, postFilePaths.Length + userPostFilePaths.Length);
            System.Array.Copy(userPostFilePaths, 0, postFilePaths, postFilePaths.Length - userPostFilePaths.Length, userPostFilePaths.Length);

            //      Rails
            //=========================
            string railPrefabsFolderPath = prefabsPaths + "/_Rails_AFWB/";
            try
            {
                railFilePaths = Directory.GetFiles(railPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing FencePrefabs Rails Folder. The _Rails_AFWB folder must be within [...]/Auto Fence Builder/AFWB_Prefabs/  " + e.ToString());
                return false;
            }
            //      User Rails
            //=========================
            string userRailPrefabsFolderPath = userPrefabsPaths + "/UserPrefabs_Rails/";
            userRailFilePaths = Directory.GetFiles(userRailPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            //Combine AFWB & User
            System.Array.Resize(ref railFilePaths, railFilePaths.Length + userRailFilePaths.Length);
            System.Array.Copy(userRailFilePaths, 0, railFilePaths, railFilePaths.Length - userRailFilePaths.Length, userRailFilePaths.Length);

            //      Extras
            //=========================
            string extrasPrefabsFolderPath = prefabsPaths + "/_Extras_AFWB/";
            try
            {
                extrasFilePaths = Directory.GetFiles(extrasPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing FencePrefabs Extras Folder. The _Extras_AFWB folder must be within [...]/Auto Fence Builder/AFWB_Prefabs/  " + e.ToString());
            }
            //- User Extras -
            string userExtraPrefabsFolderPath = userPrefabsPaths + "/UserPrefabs_Extras/";
            userExtraFilePaths = Directory.GetFiles(userExtraPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            //Combine AFWB & User
            System.Array.Resize(ref extrasFilePaths, extrasFilePaths.Length + userExtraFilePaths.Length);
            System.Array.Copy(userExtraFilePaths, 0, extrasFilePaths, extrasFilePaths.Length - userExtraFilePaths.Length, userExtraFilePaths.Length);

            //      Load System Objects first
            //===================================
            string sysPrefabsFolderPath = prefabsPaths + "/Protected_Do_Not_Remove/System Prefabs_AFWB/";
            string[] sysFilePaths = Directory.GetFiles(sysPrefabsFolderPath, "*.*", SearchOption.AllDirectories);
            CreateFallbackObjects();
            foreach (string filePath in sysFilePaths)
            {
                if (filePath.EndsWith(".prefab"))
                {
                    string fileName = Path.GetFileName(filePath);
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(sysPrefabsFolderPath + "/" + fileName) as GameObject;
                    if (go != null && fileName.Contains("ClickMarkerObj"))
                        nodeMarkerObj = go;
                    else if (go != null && fileName.Contains("_SubJoiner"))
                        subJoinerPrefabs.Add(go);
                    if (go != null && fileName.Contains("Marker_Post"))
                        af.markerPost = go;
                }
            }

            //=========== Load Posts ============
            foreach (string filePath in postFilePaths)
            {
                if (filePath.EndsWith(".prefab") && filePath.Contains("[Pure]") == false)
                {
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(filePath) as GameObject;
                    if (go != null && go.name.EndsWith("_Post"))
                    {
                        if (MeshUtilitiesAFWB.GetFirstMeshInGameObject(go) != null)
                        {
                            postPrefabs.Add(go);
                            subPrefabs.Add(go); //Posts are also used as subpostsPool
                            extraPrefabs.Add(go);  //Posts are also used as extrasPool
                        }
                        else
                        {
                            List<ScriptablePresetAFWB> presets = ed.mainPresetList;
                            List<string> presetsWithGo = Housekeeping.FindPresetsUsingGameObjectSimple(go, presets);
                            if (presetsWithGo.Count == 0)
                                Debug.Log($"Missing Mesh on prefab {filePath}  :   Unused\n");
                            else
                                Debug.Log($"Missing Mesh on prefab {filePath}  :  {presetsWithGo.Count} Uses\n");
                            PrintUtilities.PrintList(presetsWithGo, "Presets using this GameObject: ");

                            //As the Mesh is Missing, we will add a default mesh to the prefab

                            postPrefabs.Add(go);
                            subPrefabs.Add(go); //Posts are also used as subpostsPool
                            extraPrefabs.Add(go);  //Posts are also used as extrasPool
                        }
                    }
                    else if (go == null)
                    {
                        Debug.LogWarning("Post was null in  LoadAllPrefabLayers()  " + filePath + "\n");
                    }
                    else if (go.name.EndsWith("_Rail") == false)
                    {
                        Debug.LogWarning("  Prefab in Post folder not named _Post:  " + go.name + "    " + filePath + "\n");
                    }
                }
            }
            //========== Load Rails ================
            foreach (string filePath in railFilePaths)
            {
                if (filePath.EndsWith(".prefab") && filePath.Contains("[Pure]") == false)
                {
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(filePath) as GameObject;
                    if (go != null && go.name.EndsWith("_Rail") || go.name.EndsWith("_Panel"))
                    {
                        if (MeshUtilitiesAFWB.GetFirstMeshInGameObject(go) != null && go.name.EndsWith("_OrigRot_Rail") == false)
                        {
                            railPrefabs.Add(go);
                            extraPrefabs.Add(go);//Rails are also used as extrasPool
                        }
                        else
                            Debug.Log("Missing Mesh on prefab:  " + filePath +
                                " Delete" + "\n");
                    }
                    else if (go == null)
                    {
                        Debug.LogWarning("Rail was null in  LoadAllPrefabLayers()  " + filePath + "\n");
                    }
                    else if (go.name.EndsWith("_Rail") == false && go.name.EndsWith("_Rail") == false)
                    {
                        Debug.LogWarning("  Prefab in Rails folder not named '_Rail' or '_Panel':  " + go.name + "    " + filePath + "\n");
                    }
                }
            }
            //============ Load Extras ==============
            foreach (string filePath in extrasFilePaths)
            {
                if (filePath.EndsWith(".prefab") && filePath.Contains("[Pure]") == false)
                {
                    string fileName = Path.GetFileName(filePath);
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(filePath) as GameObject;
                    if (go != null)
                    {
                        if (go.name.EndsWith("_Extra") == false)
                        {
                            go.name += "_Extra";
                            string newFilePath = System.IO.Path.ChangeExtension(filePath, "_Extra.prefab");
                            Debug.Log("Prefab in Extras folder did not end in '_Extra'. This has been appended." + newFilePath);
                            //--Re-save
                            AssetDatabase.RenameAsset(filePath, go.name);
                            AssetDatabase.SaveAssets(); // Save changes to the asset database
                        }

                        if (MeshUtilitiesAFWB.GetFirstMeshInGameObject(go) != null)
                        {
                            extraPrefabs.Add(go);
                            postPrefabs.Add(go); //Also add Extras to Posts
                        }
                        else
                            Debug.Log("Missing Mesh on prefab  " + filePath + "\n");
                    }
                    else Debug.Log("Prefab was null:  " + filePath);
                }
            }
            ed.af.AddAllPrefabDetails();
            return true;
        }*/

        //----------------------------
        // For use when creating a minimal setup with zero prefab content
        public bool LoadAllPrefabsMinimal(AutoFenceEditor ed, List<GameObject> extraPrefabs, List<GameObject> postPrefabs,
            List<GameObject> railPrefabs, List<GameObject> subJoinerPrefabs, ref GameObject nodeMarkerObj)
        {
            string[] zeroContentPaths = AssetDatabase.FindAssets("ZeroPrefabContentVersion");
            string zeroContentsFolderPath;

            if (zeroContentPaths.Length == 0 || zeroContentPaths[0] == "")
            {
                Debug.LogWarning("Couldn't find ZeroPrefabContentVersion   Length " + zeroContentPaths.Length + "\n");
                return false;
            }
            else
            {
                zeroContentsFolderPath = AssetDatabase.GUIDToAssetPath(zeroContentPaths[0]);
                if (zeroContentsFolderPath == "")
                {
                    Debug.LogWarning("Couldn't create zeroContentsFolderPath \n");
                }
            }

            string[] filePaths = null;//, postFilePaths = null, railFilePaths = null, extrasFilePaths = null;
            try
            {
                filePaths = Directory.GetFiles(zeroContentsFolderPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing ZeroPrefabContentVersion Folder. The ZeroPrefabContentVersion folder must be at Auto Fence Builder/ZeroPrefabContentVersion   " + e.ToString());
                return false;
            }
            //===================================================
            //         CreateMergedPrefabs new prefabs folders if necessary
            //===================================================
            string mainPrefabsFolderPath = ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs";
            bool folderExists = AssetDatabase.IsValidFolder(mainPrefabsFolderPath);
            if (folderExists == false)
            {
                string guid = AssetDatabase.CreateFolder(ed.af.currAutoFenceBuilderDir, "AFWB_Prefabs");
                mainPrefabsFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                if (mainPrefabsFolderPath == "")
                {
                    Debug.LogWarning("Couldn't create AFWB_Prefabs folder \n");
                    return false;
                }
            }

            string postsFolderPath = ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs/_Posts_AFWB";
            folderExists = AssetDatabase.IsValidFolder(postsFolderPath);
            if (folderExists == false)
            {
                string guid = AssetDatabase.CreateFolder(ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs", "_Posts_AFWB");
                postsFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                if (postsFolderPath == "")
                    Debug.LogWarning("Couldn't create _Posts_AFWB folder \n");
            }

            string railsFolderPath = ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs/_Rails_AFWB";
            folderExists = AssetDatabase.IsValidFolder(railsFolderPath);
            if (folderExists == false)
            {
                string guid = AssetDatabase.CreateFolder(ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs", "_Rails_AFWB");
                railsFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            }

            string extrasFolderPath = ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs/_Extras_AFWB";
            folderExists = AssetDatabase.IsValidFolder(extrasFolderPath);
            if (folderExists == false)
            {
                string guid = AssetDatabase.CreateFolder(ed.af.currAutoFenceBuilderDir + "/AFWB_Prefabs", "_Extras_AFWB");
                extrasFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            }

            //===============================================================
            //       Load the minimal prefabs from ZeroContent Folder
            //===============================================================

            /*foreach (string filePath in filePaths)
            {
                if (filePath.EndsWith(".prefab"))
                {
                    string fileName = Path.GetFileName(filePath);
                    GameObject go = AssetDatabase.LoadMainAssetAtPath(zeroContentsFolderPath + "/" + fileName) as GameObject;
                    if (go != null && fileName.Contains("ClickMarkerObj"))
                        nodeMarkerObj = go;
                    else if (go != null && fileName.Contains("_SubJoiner"))
                        subJoinerPrefabs.Add(go);
                    else if (go != null && fileName.Contains("CorePost_Post"))
                    {
                        postPrefabs.Add(go);
                        subPrefabs.Add(go);
                        extraPrefabs.Add(go);
                    }
                    else if (go != null && fileName.Contains("Marker_Post"))
                    {
                        postPrefabs.Add(go);
                        subPrefabs.Add(go);
                        extraPrefabs.Add(go);
                    }
                    else if (go != null && fileName.Contains("CoreRail_Panel_Rail"))
                    {
                        railPrefabs.Add(go);
                        extraPrefabs.Add(go);
                    }
                    else
                    {
                        Debug.LogWarning(" Unknown prefab in ZeroPrefabContentVersion folder:  " + fileName + "\n");
                    }
                    if (go == null)
                        Debug.Log("GameObject was null \n");
                }
            }*/
            //==================================================================================
            //       Resave the minimal prefabs back to the regular AFWB_Prefabs folders
            //==================================================================================
            for (int i = 0; i < postPrefabs.Count; i++)
            {
                GameObject go = postPrefabs[i];
                if (go != null)
                {
                    CopyPrefabToDirectory(go, postsFolderPath);
                }
            }
            for (int i = 0; i < railPrefabs.Count; i++)
            {
                GameObject go = railPrefabs[i];
                if (go != null)
                {
                    CopyPrefabToDirectory(go, railsFolderPath);
                }
            }

            if (nodeMarkerObj != null)
                CopyPrefabToDirectory(nodeMarkerObj, mainPrefabsFolderPath);
            if (subJoinerPrefabs.Count > 0)
                CopyPrefabToDirectory(subJoinerPrefabs[0], mainPrefabsFolderPath);

            AssetDatabase.Refresh();

            return true;
        }

        //-------------
        // ATM, you can only save from an instanciated go
        public static bool CopyPrefabToDirectory(GameObject go, string savePath)
        {
            GameObject instantiatedGO = GameObject.Instantiate(go);
            instantiatedGO.name = go.name; //we don't want "(CLone)"
            savePath += "/" + instantiatedGO.name + ".prefab";

            bool fileExists = File.Exists(savePath);
            if (fileExists)
            {
                Debug.LogWarning("File already exists " + go.name + "   " + savePath);
                if (instantiatedGO)
                    GameObject.DestroyImmediate(instantiatedGO);
                return true;
            }

            //GameObject prefab = PrefabUtility.CreatePrefab(savePath, instantiatedGO);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, savePath);
            /*try
            {
                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.Default);
            }
            catch (System.Exception e){
                Debug.LogWarning("Couldn't ReplacePrefab " + go.name + " to " + savePath + "  " + e.ToString());
                return false;
            }*/
            if (instantiatedGO)
                GameObject.DestroyImmediate(instantiatedGO);

            return true;
        }

        //--------------------------------------
        // Will replace if it already exists
        public GameObject LoadSinglePostPrefabByName(List<GameObject> postPrefabs, string postName, bool replace = false)
        {
            string prefabsFolderPath = "Assets/Auto Fence Builder/FencePrefabs/_Posts";
            string[] filePaths = null;
            try
            {
                filePaths = Directory.GetFiles(prefabsFolderPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Missing FencePrefabs Posts Folder. The FencePrefabs folder must be at Assets/Auto Fence Builder/FencePrefabs   " + e.ToString());
                return null;
            }
            postName = postName + ".prefab";
            foreach (string filePath in filePaths)
            {
                if (filePath.EndsWith(".prefab"))
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName.Equals(postName))
                    {
                        UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(prefabsFolderPath + fileName);
                        GameObject newPost = data[0] as GameObject;
                        if (replace == true && newPost != null && MeshUtilitiesAFWB.GetFirstMeshInGameObject(newPost) != null)
                        {
                            ReplacePrefabInList(postPrefabs, newPost);
                        }
                        return newPost;
                    }
                }
            }
            return null;
        }

        //---------------
        public bool ReplacePrefabInList(List<GameObject> prefabs, GameObject replacement)
        {
            GameObject removeOld = prefabs.Where(obj => obj.name == replacement.name).First();
            var index = prefabs.IndexOf(removeOld);
            if (index != -1)
            {
                prefabs[index] = replacement;
                return true;
            }
            return false;
        }

        public static GameObject LoadPrefabNamed(string prefabName, string directory)
        {
            string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { directory });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    return prefab;
                }
            }
            Debug.LogError($"Prefab named '{prefabName}' not found in directory '{directory}'.");
            return null;
        }

        private Material blueMaterial;
        /// <summary>
        /// Load the mandatory prefabs for AFWB use
        /// </summary>
        /// <param name="sysPrefabsFolderPath"></param>
        /// <remarks>Marker Post, Node Marker, Drag_Game_Obj, SubJoiner. Default: Post, Wall ExtraCube</remarks>
        private void LoadSystemObjects(string sysPrefabsFolderPath)
        {
            //-- Marker Post
            af.markerPost = AssetDatabase.LoadMainAssetAtPath(sysPrefabsFolderPath + "/" + "Marker_Post.prefab") as GameObject;
            if (af.markerPost == null)
            {
                af.markerPost = CreateGoWithBoxMesh("Marker_Post", 0.25f, 2.0f, 0.25f);
                Material transparentGreenMat = MaterialUtilities.CreateTransparentMaterial(.3f, .9f, .3f, .8f);
            }
            af.markerPostMesh = MeshUtilitiesAFWB.GetMeshFromGameObject(af.markerPost);
            af.markerPostMeshLarge = MeshUtilitiesAFWB.DuplicateMesh(af.markerPostMesh);
            if (af.markerPostMeshLarge != null)
                MeshUtilitiesAFWB.ScaleMesh(af.markerPostMeshLarge, new Vector3(1.8f, 1f, 1.8f));

            //-- Node Marker
            af.nodeMarkerObj = AssetDatabase.LoadMainAssetAtPath(sysPrefabsFolderPath + "/" + "NodeMarkerObj.prefab") as GameObject;
            if (af.nodeMarkerObj == null)
            {
                af.nodeMarkerObj = CreateGoWithSphereMesh("NodeMarkerObj", 0.25f);
                Material transparentYellowMat = MaterialUtilities.CreateTransparentMaterial(.99f, .713f, .102f, .95f);
                af.nodeMarkerObj.GetComponent<Renderer>().material = transparentYellowMat;
            }
        }

        // Method to create fallback objects with predefined dimensions
        private void CreateFallbackObjects()
        {
            af.fallbackPost = CreateGoWithBoxMesh("Fallback_Post", 0.25f, 2.0f, 0.25f);
            af.fallbackRail = CreateGoWithBoxMesh("Fallback__Rail", 3.0f, 2.0f, 0.25f);
            af.fallbackExtra = CreateGoWithBoxMesh("Fallback__Extra", 0.5f, 0.5f, 0.5f); // Adding the extra marker
        }

        public GameObject CreateGoWithBoxMesh(string name, float width, float height, float depth)
        {
            GameObject goBox = new GameObject(name);
            goBox.SetActive(false);
            goBox.hideFlags = HideFlags.HideInHierarchy;
            MeshFilter meshFilter = goBox.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = goBox.AddComponent<MeshRenderer>();

            meshFilter.mesh = MeshUtilitiesAFWB.CreateBoxMesh(width, height, depth);
            // Assign the blue material to the renderer
            Material blueMaterial = new Material(Shader.Find("Standard")) { color = Color.blue };
            meshRenderer.material = blueMaterial;
            return goBox;
        }

        public GameObject CreateGoWithSphereMesh(string name, float radius)
        {
            GameObject goSphere = new GameObject(name);
            MeshFilter meshFilter = goSphere.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = goSphere.AddComponent<MeshRenderer>(); ;
            //meshFilter.mesh = OctahedronSphereCreator.Create(6, 0.5f);
            Material transparentYellowMat = MaterialUtilities.CreateTransparentMaterial(.99f, .713f, .102f, .95f);
            meshRenderer.material = transparentYellowMat;
            return goSphere;
        }
    }
}