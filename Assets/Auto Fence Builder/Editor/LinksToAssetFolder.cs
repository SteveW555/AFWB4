//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 //3.4
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AFWB;
using TCT.PrintUtils;
using System.Linq;
//using System;

public class PrefabsListWindow : UnityEditor.EditorWindow
{
    private List<FoundPrefabs> prefabsWithMat;
    private LayerSet currentLayer;
    private UnityEngine.Vector2 scrollPosition;
    //private const float rowHeight = 20f;
    private const float windowWidth = 900; // Increased by 20
    private const float maxHeight = 920f;   // Increased by 20
    private Dictionary<UnityEngine.GameObject, string> buttonStates = new Dictionary<UnityEngine.GameObject, string>();
    private AutoFenceCreator af;
    private const float rowHeight = 120f; // Updated row height

    public static void ShowWindow(List<FoundPrefabs> prefabs, LayerSet layer, AutoFenceCreator af)
    {
        PrefabsListWindow window = (PrefabsListWindow)UnityEditor.EditorWindow.GetWindow(typeof(PrefabsListWindow), false, "Prefabs List", true);
        window.InitializeWindow(prefabs, layer, af);
        window.Show();
        window.Focus();
    }

    private void InitializeWindow(List<FoundPrefabs> prefabs, LayerSet layer, AutoFenceCreator af)
    {
        prefabsWithMat = prefabs;
        currentLayer = layer;
        this.af = af;
        CalculateWindowSize();
        InitializeButtonStates();
    }

    private void CalculateWindowSize()
    {
        float height = UnityEngine.Mathf.Min(prefabsWithMat.Count * rowHeight + 50, maxHeight); // Add some extra space for padding
        this.minSize = new UnityEngine.Vector2(windowWidth, height);
        this.maxSize = new UnityEngine.Vector2(windowWidth, height);
    }

    private void InitializeButtonStates()
    {
        buttonStates.Clear();
        foreach (var prefab in prefabsWithMat)
            buttonStates[prefab.prefab] = null;
    }

    private const float brightnessFactor = 1.5f; // Adjust this factor as needed
    private void OnGUI()
    {
        if (prefabsWithMat == null || prefabsWithMat.Count == 0)
        {
            UnityEditor.EditorGUILayout.LabelField("No prefabs to display.");
            return;
        }

        UnityEditor.EditorGUILayout.LabelField("Prefabs using the Material:", UnityEditor.EditorStyles.boldLabel);

        if (prefabsWithMat.Count * rowHeight + 40 > maxHeight)
        {
            scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(scrollPosition, UnityEngine.GUILayout.Width(windowWidth), UnityEngine.GUILayout.Height(maxHeight));
        }
        else
        {
            scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(scrollPosition, UnityEngine.GUILayout.Width(windowWidth), UnityEngine.GUILayout.Height(prefabsWithMat.Count * rowHeight + 40));
        }

        foreach (var foundPrefab in prefabsWithMat)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();

            // Display the thumbnail preview
            UnityEngine.Texture2D preview = UnityEditor.AssetPreview.GetAssetPreview(foundPrefab.prefab);
            if (preview != null)
            {
                UnityEngine.Texture2D brightenedPreview = AdjustBrightness(preview, brightnessFactor);
                UnityEngine.GUILayout.Label(brightenedPreview, UnityEngine.GUILayout.Width(100), UnityEngine.GUILayout.Height(100));
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("No Preview", UnityEngine.GUILayout.Width(100), UnityEngine.GUILayout.Height(100));
            }

            UnityEditor.EditorGUILayout.LabelField(foundPrefab.prefab.name, UnityEngine.GUILayout.Width(150));

            switch (foundPrefab.sourceLayerList)
            {
                case LayerSet.railALayer:
                case LayerSet.railBLayer:
                    ShowMyButton(foundPrefab, "Use As Rail A", UseAsRailA);
                    GUILayout.Space(4);
                    ShowMyButton(foundPrefab, "Use As Rail B", UseAsRailB);
                    GUILayout.Space(4);
                    ShowMyButton(foundPrefab, "Use As Extra", UseAsExtra);
                    break;
                case LayerSet.postLayer:
                case LayerSet.subpostLayer:
                case LayerSet.extraLayer:
                    ShowMyButton(foundPrefab, "Use As Post", UseAsPost);
                    GUILayout.Space(4);
                    ShowMyButton(foundPrefab, "Use As Subpost", UseAsSubpost);
                    GUILayout.Space(4);
                    ShowMyButton(foundPrefab, "Use As Extra", UseAsExtra);
                    break;
            }

            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        UnityEditor.EditorGUILayout.EndScrollView();
    }


    private void ShowMyButton(FoundPrefabs foundPrefab, string buttonText, System.Action<FoundPrefabs> action)
    {
        if (buttonStates[foundPrefab.prefab] == buttonText)
        {
            UnityEngine.GUIStyle magentaStyle = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button);
            magentaStyle.normal.textColor = new UnityEngine.Color(.9f, .6f, .99f);
            magentaStyle.hover.textColor = new UnityEngine.Color(.9f, .6f, .99f);
            magentaStyle.active.textColor = new UnityEngine.Color(.9f, .6f, .99f);
            magentaStyle.focused.textColor = new UnityEngine.Color(.9f, .6f, .99f);

            if (UnityEngine.GUILayout.Button("Sure?", magentaStyle, UnityEngine.GUILayout.Width(105)))
            {
                action(foundPrefab);
                buttonStates[foundPrefab.prefab] = null;
            }
        }
        else
        {
            if (UnityEngine.GUILayout.Button(buttonText, UnityEngine.GUILayout.Width(105)))
            {
                buttonStates[foundPrefab.prefab] = buttonText;
            }
        }
    }



    private void UseAsPost(FoundPrefabs foundPrefab)
    {
        UnityEngine.Debug.Log($"Using prefab: {foundPrefab.prefab.name} as Post\n");

        //int prefabIndex = foundPrefab.index;
        //LayerSet layerContainingThePrefab = foundPrefab.sourceLayerList;
        int prefabIndex = ChangeLayerPrefabByIndex(foundPrefab.index, LayerSet.postLayer);

    }

    private void UseAsRailA(FoundPrefabs foundPrefab)
    {
        int prefabIndex = ChangeLayerPrefabByIndex(foundPrefab.index, LayerSet.railBLayer);
        UnityEngine.Debug.Log($"Using prefab: {foundPrefab.prefab.name} as Rail A\n");
    }

    private void UseAsRailB(FoundPrefabs foundPrefab)
    {
        int prefabIndex = ChangeLayerPrefabByIndex(foundPrefab.index, LayerSet.railBLayer);
        UnityEngine.Debug.Log($"Using prefab: {foundPrefab.prefab.name} as Rail B\n");
    }

    private void UseAsExtra(FoundPrefabs foundPrefab)
    {
        int prefabIndex = ChangeLayerPrefabByIndex(foundPrefab.index, LayerSet.extraLayer);
        UnityEngine.Debug.Log($"Using prefab: {foundPrefab.prefab.name} as Extra\n");
    }

    private void UseAsSubpost(FoundPrefabs foundPrefab)
    {
        int prefabIndex = ChangeLayerPrefabByIndex(foundPrefab.index, LayerSet.subpostLayer);
        UnityEngine.Debug.Log($"Using prefab: {foundPrefab.prefab.name} as Subpost\n");
    }

    /// <summary>
    /// Will change the selected prefab programmatically and update everything necessary
    /// </summary>
    /// <param name="prefabIndex"></param>
    /// <param name="layerContainingThePrefab"></param>
    /// <returns></returns>
    private int ChangeLayerPrefabByIndex(int prefabIndex, LayerSet layerContainingThePrefab)
    {
        prefabIndex = af.SetCurrentPrefabIndexForLayer(prefabIndex, layerContainingThePrefab);
        af.SetMenuIndexFromPrefabIndexForLayer(prefabIndex, layerContainingThePrefab);
        af.ResetPoolForLayer(layerContainingThePrefab);
        af.ForceRebuildFromClickPoints();
        return prefabIndex;
    }

    private UnityEngine.Texture2D AdjustBrightness(UnityEngine.Texture2D original, float brightnessFactor)
    {
        UnityEngine.Texture2D adjustedTexture = new UnityEngine.Texture2D(original.width, original.height);
        UnityEngine.Color[] pixels = original.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i] * brightnessFactor;
        }

        adjustedTexture.SetPixels(pixels);
        adjustedTexture.Apply();
        return adjustedTexture;
    }


}


/*
SyncPrefabMenus();
af.ex.UpdateExtrasFromExtraVariantsStruct(ed.currPreset.extraVarsStruct);
af.singlesContainer.ResetAllRailSingles(af);
af.ResetAllPools();
SetupParametersAfterPresetSelect();
ed.af.userPrefabPost = null;
ed.af.userPrefabRail[kRailAIndex] = null;
ed.af.userPrefabExtra = null;
*/


public class LinksToAssetFolder
{
    AutoFenceCreator af;
    AutoFenceEditor ed;
    private List<GameObject> prefabsWithMat;
    private bool showPrefabsWindow = false;

    public LinksToAssetFolder(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    public void Show_Prefab_Mesh_Material_InAssetFolder(LayerSet layer, float horizSpaceOffset)
    {
        string layerName = af.GetLayerNameAsString(layer);

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
        if (GUILayout.Button(new GUIContent("Mat", "Show the Material in the Assets Folder. " +
            "\n\n Hold control to Show a list of all Prefabs that use this Material, " +
            "and optionally Select the Prefab to Use as a Post or Rail etc."), EditorStyles.miniButton, GUILayout.Width(36)))
        {
            if (Event.current.control)
            {
                Material mat = af.GetMainMaterialForLayer(layer);
                Debug.Log($" -- Prefabs using {af.GetLayerNameAsString(layer)} Material: {mat.name}\n");
                
                //List<GameObject> prefabsWithMat = Housekeeping.GetPrefabsUsingMaterial(mat);
                List<FoundPrefabs> prefabsWithMat = Housekeeping.GetPrefabsUsingMaterialInLists(mat, af.railPrefabs, af.postPrefabs, af.extraPrefabs);





                PrintUtilities.PrintList(prefabsWithMat, allInOneLine: false);
                PrefabsListWindow.ShowWindow(prefabsWithMat, layer, af);
            }
            else
                ShowMaterialInAssetsFolder(layer);

            Event.current.Use(); // -- Consume the event after processing
        }
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

