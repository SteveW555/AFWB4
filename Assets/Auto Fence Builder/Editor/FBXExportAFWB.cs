using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
//using UnityEditor.Formats.Fbx.Exporter;
using System.Reflection;
using AFWB;
using MeshUtils;

public class FBXExportAFWB
{
    AutoFenceCreator af;
    AutoFenceEditor ed;
    public FBXExportAFWB(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    /*public static void ExportMeshAndPrefab(Mesh mesh, string exportPath)
    {
#if UNITY_EDITOR
        if (FBXExporterChecker.IsFBXExporterAvailable())
        {
            SaveUserObjectAsFBX(null, PrefabTypeAFWB.postPrefab, null);
        }
        else
        {
            Debug.LogError("FBX Exporter package is not available.\n");
        }
#else
        Debug.LogError("This function can only be used in the Unity Editor.\n");
#endif
    }*/

    //----------------------------------------------------------------------------------------
    //Save the user-added object in to the FencePrefabs and Meshes folders
    // master can be null if not a variant
    /*public static GameObject SaveUserObjectAsFBX(GameObject userObj, PrefabTypeAFWB objType, AutoFenceCreator af, GameObject master = null,
                                                bool isVariant = false, bool addUserPrefix = true)
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
            //prefabPath = af.currAutoFenceBuilderDir + "/FencePrefabs_AFWB/_Rails_AFWB/" + objName + ".prefab";
            prefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Rails/" + objName + ".prefab";
            fbxExpPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserMeshes_Rails/" + objName + meshExtnStr;
        }
        if (objType == PrefabTypeAFWB.postPrefab)
        {
            if (objName.EndsWith("_Post") == false)
                objName += "_Post";
            prefabPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Posts/" + objName + ".prefab";
            //prefabPath = af.currAutoFenceBuilderDir + "/AFWB_Prefabs/_Posts_AFWB/" + objName + ".prefab";
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

        //-- Save Meshes

        if (af.useBinaryFBX == true)
        {
            ExportBinaryFBX(fbxExpPath, userObj);
        }
        else
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
                string successPath = ModelExporter.ExportObject(fbxExpPath, userObj);
            }
            else if (objType == PrefabTypeAFWB.postPrefab)
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
        List<GameObject> allSource = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(userObj);
        List<GameObject> allFBX = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(expGO);
        List<GameObject> allNew = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(instExpGO);

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



        GameObject.DestroyImmediate(instExpGO);
        AssetDatabase.Refresh();
        return prefab;
    }*/

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
    //--------------------------------
    /*public static void SimpleExportMeshAsGameObject(AutoFenceCreator af,  string exportPath, Object obj)
    {
        string objName = "TestExp";


        string fbxExpPath = af.currAutoFenceBuilderDir + "/Exported Meshes/" + objName + "_m" + ".fbx";

        string filePath = Path.Combine(Application.dataPath, "MyGame.fbx");
        //Debug.Log("Exporting to: " + filePath);
        ModelExporter.ExportObject(filePath, obj);
        ModelExporter.ExportObject(fbxExpPath, obj);

        // ModelExporter.ExportObject can be used instead of 
        // ModelExporter.ExportObjects to export a single game object
    }*/
    //--------------------------------
    /*public static void SimpleExportMeshAsGameObject(AutoFenceCreator af, string exportPath, Object obj, string name = "")
    {
        ModelExporter.ExportObject(exportPath, obj);
    }*/
    //--------------------------------
    // same as above, but for exporting a mesh, we need to wrap it in a temp GO
    /*public static void SimpleExportMesh(string exportPath, Mesh mesh, string origMeshName)
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh provided for export.");
            return;
        }
        // Create a temporary GameObject to hold the mesh
        GameObject tempGameObject = new GameObject();
        MeshFilter meshFilter = tempGameObject.AddComponent<MeshFilter>();
        mesh.name = origMeshName;


        meshFilter.sharedMesh = mesh;
        // We have to do this or Unity will add "Instance" to the mesh name
        meshFilter.sharedMesh.name = origMeshName;

        meshFilter.name = origMeshName;

        // Use ModelExporter to export the GameObject
        ModelExporter.ExportObject(exportPath, tempGameObject);

        // Clean up the temporary GameObject after export
        //Object.DestroyImmediate(tempGameObject);
    }*/



}
