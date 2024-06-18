#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using AFWB;
using MeshUtils;
using System.Collections.Generic;
//using UnityEditor.Formats.Fbx.Exporter;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

public static class PrefabMeshExporterAF
{
    /// <summary>
    /// Exports the mesh to the specified path if FBX Exporter is available.
    /// </summary>
    /// <param name="mesh">The mesh to export.</param>
    /// <param name="exportPath">The path where the mesh should be exported.</param>
    public static GameObject ExportMesh(GameObject mesh, PrefabTypeAFWB objType, AutoFenceCreator af)
    {
#if UNITY_EDITOR
        if (FBXExporterChecker.IsFBXExporterAvailable())
        {
            GameObject newPrefab = ExportWithFBXExporter(mesh, objType, af);
            return newPrefab;
        }
        else
        {
            Debug.LogWarning("FBX Exporter package is not available.\n");
        }
#else
        Debug.LogError("This function can only be used in the Unity Editor.\n");
#endif
        return null;
    }

    private static GameObject ExportWithFBXExporter(GameObject userObj, PrefabTypeAFWB objType, AutoFenceCreator af, GameObject master = null,
                                                bool isVariant = false, bool addUserPrefix = true)
    {

        var modelExporterType = Type.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter, Unity.Formats.Fbx.Editor");
        if (modelExporterType != null)
        {
            var exportObjectMethod = modelExporterType.GetMethod("ExportObject", new[] { typeof(string), typeof(GameObject) });
            if (exportObjectMethod != null)
            {
                //exportObjectMethod.Invoke(null, new object[] { exportPath, tempObject });
                //================================================================================================


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
                if (userObj.name.StartsWith("[U") == false && addUserPrefix == true)
                    objName += "[U]";
                objName += userObj.name;

                string meshExtnStr = ".fbx";
                string fbxExpPath = "";
                GameObject meshGO = null, exportedModel = null, prefab = null;

                if (objType == PrefabTypeAFWB.railPrefab)
                {
                    if (objName.EndsWith("_Panel") == false && objName.EndsWith("_Rail") == false)
                    {
                        objName += "_Panel";
                    }
                    //===========================
                    //      Save Prefab Path
                    //============================
                    prefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Rails/" + objName + ".prefab";

                    //============================
                    //      Save Mesh Path
                    //============================
                    fbxExpPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserMeshes_Rails/" + objName + meshExtnStr;
                }
                if (objType == PrefabTypeAFWB.postPrefab)
                {
                    if (objName.EndsWith("_Post") == false)
                        objName += "_Post";
                    prefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Posts/" + objName + ".prefab";
                    fbxExpPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserMeshes_Posts/" + objName + meshExtnStr;
                    isVariant = false;
                }
                if (objType == PrefabTypeAFWB.extraPrefab)
                {
                    if (objName.EndsWith("_Extra") == false)
                        objName += "_Extra";
                    prefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Extras/" + objName + ".prefab";
                    fbxExpPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserMeshes_Extras/" + objName + meshExtnStr;
                    isVariant = false;
                }


                //============================
                //      Export the Mesh
                //============================
                //-- Default useBinaryFBX = true
                if (af.useBinaryFBX == true)
                {
                    ExportBinaryFBX(fbxExpPath, userObj);
                }
                else
                {
                    ExportNonBinaryFBX(userObj, objType, af, exportObjectMethod, fbxExpPath);
                }

                //--- Load the exported fbx so we can attaxh it to our prefab
                GameObject exportedGO = AssetDatabase.LoadMainAssetAtPath(fbxExpPath) as GameObject;
                GameObject instantiatedExpGO = null;
                if (isVariant == false)
                    instantiatedExpGO = GameObject.Instantiate(exportedGO) as GameObject; // can't SaveAsPrefabAsset() persistent disk asset directlyif (inst == null)
                                                                         //else
                                                                         //instantiatedExpGO = GameObject.Instantiate(exportedGO) as GameObject; // yup, I know

                if (instantiatedExpGO == null)
                {
                    Debug.LogWarning("Something weird happened with the fbx export/import");
                    return null;
                }

                //-- No idea why this should be necessary
                List<GameObject> allSource = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(userObj);
                List<GameObject> allFBX = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(exportedGO);
                List<GameObject> allNew = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(instantiatedExpGO);

                for (int i = 0; i < allSource.Count; i++)
                {
                    MeshRenderer sourceRend = allSource[i].GetComponent<MeshRenderer>();
                    MeshRenderer newRend = allNew[i].GetComponent<MeshRenderer>();
                    if (sourceRend != null && newRend != null)
                        newRend.GetComponent<MeshRenderer>().sharedMaterial = sourceRend.GetComponent<MeshRenderer>().sharedMaterial;
                }

                //============================
                //      Save the Prefab
                //============================
                if (isVariant == false)
                    prefab = PrefabUtility.SaveAsPrefabAsset(instantiatedExpGO, prefabPath);
                else if (isVariant == true)
                    SaveAsPrefabVariant(master, prefabPath, out prefab, allSource, allFBX, out allNew);
                GameObject.DestroyImmediate(instantiatedExpGO);
                AssetDatabase.Refresh();
                return prefab;

                //================================================================================================
                //Debug.Log($"GameObject exported successfully to '{exportPath}'.\n");
            }
            else
            {
                Debug.LogError("ExportObject method not found in FBX Exporter library.\n");
                return null;
            }
        }
        else
        {
            Debug.LogError("ModelExporter type not found in FBX Exporter library.\n");
            return null;
        }
        return null;
    }
    //------------------------
    //-- Default is to use binary FBX, not this
    private static void ExportNonBinaryFBX(GameObject userObj, PrefabTypeAFWB objType, AutoFenceCreator af, MethodInfo exportObjectMethod, string fbxExpPath)
    {
        if (objType == PrefabTypeAFWB.railPrefab)
        {
            // At this point the Mesh name in the Exporter is determined by the userObjName, prefab name given by master name
            // all non-alphanumeric chars will be replaced with '_'
            string meshName = userObj.name.Replace('-', 'n');
            meshName = meshName.Replace('+', 'p');
            meshName = meshName.Replace("[", "");
            meshName = meshName.Replace("]", "");
            meshName = af.StripPanelRailFromName(meshName);
            userObj.name = meshName;
            //string successPath = ModelExporter.ExportObject(fbxExpPath, userObj);
            exportObjectMethod.Invoke(null, new object[] { fbxExpPath, userObj });
        }
        else if (objType == PrefabTypeAFWB.postPrefab)
        {
            string meshName = userObj.name;
            //string successPath = ModelExporter.ExportObject(fbxExpPath, userObj);
            exportObjectMethod.Invoke(null, new object[] { fbxExpPath, userObj });
        }
    }
    //------------------------
    private static void SaveAsPrefabVariant(GameObject master, string prefabPath, out GameObject prefab, List<GameObject> allSource, List<GameObject> allFBX, out List<GameObject> allNew)
    {
        prefab = PrefabUtility.SaveAsPrefabAsset(master, prefabPath);
        allNew = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(prefab);
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
    //-----------------------
    private static void ExportBinaryFBX(string filePath, UnityEngine.Object singleObject)
    {
        // Ensure FBX Exporter is available
        if (!FBXExporterChecker.IsFBXExporterAvailable())
        {
            Debug.LogError("FBX Exporter package is not available.\n");
            return;
        }

        // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.FullName.StartsWith("Unity.Formats.Fbx.Editor"));
        if (assembly == null)
        {
            Debug.LogError("FBX Exporter assembly not found.\n");
            return;
        }

        var types = assembly.GetTypes();
        var optionsInterfaceType = types.FirstOrDefault(x => x.Name == "IExportOptions");
        var optionsType = types.FirstOrDefault(x => x.Name == "ExportOptionsSettingsSerializeBase");

        if (optionsInterfaceType == null || optionsType == null)
        {
            Debug.LogError("FBX Exporter types not found.\n");
            return;
        }

        // Instantiate a settings object instance
        var modelExporterType = types.FirstOrDefault(x => x.Name == "ModelExporter");
        if (modelExporterType == null)
        {
            Debug.LogError("FBX Exporter ModelExporter type not found.\n");
            return;
        }

        var optionsProperty = modelExporterType.GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic)?.GetGetMethod(true);
        if (optionsProperty == null)
        {
            Debug.LogError("FBX Exporter DefaultOptions property not found.\n");
            return;
        }

        var optionsInstance = optionsProperty.Invoke(null, null);

        // Change the export setting from ASCII to binary
        var exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        if (exportFormatField == null)
        {
            Debug.LogError("FBX Exporter exportFormat field not found.\n");
            return;
        }

        exportFormatField.SetValue(optionsInstance, 1);

        // Invoke the ExportObject method with the settings param
        var exportObjectMethod = modelExporterType.GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
        if (exportObjectMethod == null)
        {
            Debug.LogError("FBX Exporter ExportObject method not found.\n");
            return;
        }

        exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });
        Debug.Log($"Object exported successfully to '{filePath}'.\n");
    }


    /*private static void ExportBinaryFBX(string filePath, UnityEngine.Object singleObject)
    {
        // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
        System.Type[] types = System.AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
        System.Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
        System.Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

        // Instantiate a settings object gizmoSingletonInstance
        MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
        object optionsInstance = optionsProperty.Invoke(null, null);

        // Change the export setting from ASCII to binary
        FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        exportFormatField.SetValue(optionsInstance, 1);

        // Invoke the ExportObject method with the settings param
        MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, System.Type.DefaultBinder, new System.Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
        exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });
    }*/
}
