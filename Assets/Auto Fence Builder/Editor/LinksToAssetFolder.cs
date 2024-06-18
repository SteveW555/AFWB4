//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 //3.4
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using AFWB;
using static AFWB.AutoFenceCreator;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LinksToAssetFolder
{
    AutoFenceCreator af;
    AutoFenceEditor ed;

    public LinksToAssetFolder(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    public void Show_Prefab_Mesh_Material_InAssetFolder(LayerSet layer, float horizSpaceOffset)
    {
        string layerName = af.GetLayerNameAsString(layer);
        //GUILayout.BeginHorizontal();
        
        //   Link to Prefab in Assets Folder
        //===========================================
        GUILayout.Space(horizSpaceOffset);
        if (GUILayout.Button(new GUIContent("Pfb", "Show the Prefab in the Assets Folder\n\n" +
            "It may be necessary to Deselect / Reselect to refresh Inspector"), EditorStyles.miniButton, GUILayout.Width(36)))
        {
            Object selectedObj = ShowPrefabInAssetsFolder(layer);
            Selection.activeObject = selectedObj;

        }
        GUILayout.Space(4);

        //  Link to Mesh in Assets Folder
        //===========================================
        if (GUILayout.Button(new GUIContent("Mesh", "Show the Mesh in the Assets Folder"), EditorStyles.miniButton, GUILayout.Width(44)))
        {
            ShowMeshInAssetsFolder(layer);
        }
        GUILayout.Space(4);

        //  Link to Material in Assets Folder
        //===========================================
        if (GUILayout.Button(new GUIContent("Mat", "Show the Material in the Assets Folder"), EditorStyles.miniButton, GUILayout.Width(36)))
        {
            ShowMaterialInAssetsFolder(layer);
        }
        //GUILayout.EndHorizontal();
    }

    private Object ShowMaterialInAssetsFolder(LayerSet layer)
    {
        // Ensure the file exists before trying to select it
        string pathOfCurrLayerPrefab = af.GetCurrentPrefabPathForLayer(layer);
        GameObject currLayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathOfCurrLayerPrefab);
        string layerName = af.GetLayerNameAsString(layer);
        if (currLayerPrefab != null)
        {
            //get the name of the material
            MeshRenderer mr = currLayerPrefab.GetComponent<MeshRenderer>();
            string matName = mr.sharedMaterial.name;
            //Find an asset path as a atring in the following path that contains the mesh name
            string[] guids = AssetDatabase.FindAssets(matName, new[] { "Assets/Auto Fence Builder/AFWB_Materials" });
            if (guids.Length > 0)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                EditorGUIUtility.PingObject(Selection.activeObject);
                //print the mesh path
                Debug.Log(layerName + "  Material Path:  " + matPath + "\n");
                //Selection.activeGameObject = af.gameObject;
            }
            else
                Debug.Log("Material " + matName + " failed to exist in Show_Prefab_Mesh_Material_InAssetFolder()");
        }
        else
            Debug.Log(af.GetMainPrefabForLayer(layer).name + " failed to exist. It had one job.");

        return Selection.activeObject;
    }

    private Object ShowMeshInAssetsFolder(LayerSet layer)
    {
        // Ensure the file exists before trying to select it
        string pathOfCurrLayerPrefab = af.GetCurrentPrefabPathForLayer(layer);
        GameObject currLayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathOfCurrLayerPrefab);
        string layerName = af.GetLayerNameAsString(layer);
        if (currLayerPrefab != null)
        {
            MeshFilter mf = currLayerPrefab.GetComponent<MeshFilter>();
            string meshName = mf.sharedMesh.name;
            //Find an asset path as a atring in the following path that contains the mesh name
            string[] guids = AssetDatabase.FindAssets(meshName, new[] { "Assets/Auto Fence Builder/AFWB_Meshes" });
            if (guids.Length > 0)
            {
                string meshPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<MeshFilter>(meshPath);
                EditorGUIUtility.PingObject(Selection.activeObject);
                Debug.Log(layerName + "  Mesh Path:  " + meshPath + "\n");
                //Selection.activeGameObject = af.gameObject;
            }
            else
                Debug.Log("Mesh " + meshName + " failed to exist");
        }
        else
            Debug.Log(af.GetMainPrefabForLayer(layer).name + " failed to exist in Show_Prefab_Mesh_Material_InAssetFolder()\"");

        return Selection.activeObject;
    }

    public Object ShowPrefabInAssetsFolder(LayerSet layer)
    {
        string pathOfCurrLayerPrefab = af.GetCurrentPrefabPathForLayer(layer);
        string layerName = af.GetLayerNameAsString(layer);
        // Ensure the file exists before trying to select it
        if (AssetDatabase.LoadAssetAtPath<GameObject>(pathOfCurrLayerPrefab))
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(pathOfCurrLayerPrefab);
            EditorGUIUtility.PingObject(Selection.activeObject);
            //print the name of the mesh filter attached to the prefab
            GameObject currLayerPrefab = af.GetMainPrefabForLayer(layer);
            Debug.Log(layerName + "  Prefab Path:  " + pathOfCurrLayerPrefab + "\n");
            //select af in the hierarchy
            //Selection.activeGameObject = af.gameObject;
        }
        else
            Debug.Log(af.GetMainPrefabForLayer(layer).name + " failed to exist in Show_Prefab_Mesh_Material_InAssetFolder()\"");

        return Selection.activeObject;

    }
}