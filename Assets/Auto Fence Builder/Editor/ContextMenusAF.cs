using AFWB;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TCT.PrintUtils;
using NUnit.Framework.Internal;

public class ContextMenusAF
{
    // Adding a new item to the Assets context menu
    [MenuItem("Assets/Auto Fence/Print Preset Usage", true)]
    private static bool PrintPresetUsageValidation()
    {
        // This function decides whether the menu item is enabled or not. It is only enabled if the selected object is a prefab.
        return Selection.activeObject && PrefabUtility.GetPrefabAssetType(Selection.activeObject) != PrefabAssetType.NotAPrefab;
    }

    [MenuItem("Assets/Auto Fence/Print Preset Usage")]
    private static void PrintPresetUsage()
    {
        // Retrieve the selected GameObject
        GameObject selectedPrefab = Selection.activeObject as GameObject;

        if (selectedPrefab != null)
        {
            // Assuming you have a class Housekeeping and a method PrintPresetsUsingGameObject
            // Also assuming you have variables mainPresetList and af defined and accessible here
            Housekeeping.PrintPresetsUsingGameObjectFromContextMenu(selectedPrefab.name);
        }
        else
        {
            Debug.LogError("Selected object is not a GameObject.");
        }
    }

    /*[MenuItem("Assets/Auto Fence/Rename Prefab And Update All", false, 21)]
    private static void RenamePrefabAndUpdateAll()
    {
        GameObject selectedPrefab = Selection.activeObject as GameObject;
        if (selectedPrefab != null)
        {
            string newName = EditorUtility.RenameAsset(AssetDatabase.GetAssetPath(selectedPrefab), selectedPrefab.name);
            if (!string.IsNullOrEmpty(newName))
            {
                AutoFenceCreator afc = FindObjectOfType<AutoFenceCreator>();
                if (afc != null)
                {
                    afc.RenamePrefabAndUpdateAll(selectedPrefab, newName);
                }
                else
                {
                    Debug.LogError("AutoFenceCreator gizmoSingletonInstance not found in the scene.");
                }
            }
        }
        else
        {
            Debug.LogError("No prefab selected.");
        }
    }*/

    [MenuItem("Assets/Find Presets Using GameObject Simple", true)]
    [MenuItem("Assets/Rename Prefab And Update All", true)]
    private static bool ValidateSelectedPrefab()
    {
        return Selection.activeObject is GameObject;
    }






    [MenuItem("Assets/Find Prefabs Using Texture", false, 2000)]
    public static void FindPrefabsUsingTexture()
    {
        // Get the selected texture
        Texture2D selectedTexture = Selection.activeObject as Texture2D;

        if (selectedTexture == null)
        {
            Debug.LogWarning("Selected object is not a texture.\n");
            return;
        }

        List<GameObject> prefabsUsingTexture = new List<GameObject>();

        // Find all prefabs in the Assets folder
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        List<string> problemShaderProperties = new List<string>();
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                string resultStr = PrefabUsesTexture(prefab, selectedTexture);

                // Check if the prefab uses the texture
                if (resultStr == "true")
                    prefabsUsingTexture.Add(prefab);
                else if (!string.IsNullOrEmpty(resultStr))
                    problemShaderProperties.Add(resultStr);
            }
        }

        if (prefabsUsingTexture.Count > 0)
        {
            Debug.Log($"Prefabs using texture '{selectedTexture.name}':\n");
            foreach (GameObject prefab in prefabsUsingTexture)
                Debug.Log($"{prefab.name}\n");
        }
        else
        {
            Debug.Log($"No prefabs found using texture '{selectedTexture.name}'.\n");
        }

        ShowProblemsFound(problemShaderProperties);
    }

    private static string PrefabUsesTexture(GameObject prefab, Texture2D texture)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        string propertyNames = "";

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;

            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    bool usesTexture = false;
                    if (mat.HasProperty("_MainTex") && mat.mainTexture == texture)
                        usesTexture = true;
                    if (mat.HasProperty("_BumpMap") && mat.GetTexture("_BumpMap") == texture)
                        usesTexture = true;

                    if (usesTexture)
                        return "true";
                    else
                    {
                        propertyNames += $"{mat.name} does not use _MainTex or _BumpMap. Properties: ";
                        for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
                            propertyNames += ShaderUtil.GetPropertyName(mat.shader, i) + ", ";
                        propertyNames = propertyNames.TrimEnd(',', ' ') + "\n";
                    }
                }
            }
        }

        return propertyNames;
    }
    private static void ShowProblemsFound(List<string> problemShaderProperties)
    {
        if (problemShaderProperties.Count > 0)
        {
            Debug.Log("--------------------\n");
            Debug.Log("Prefabs with problems:\n");
            foreach (string problem in problemShaderProperties)
                Debug.Log(problem + "\n");
        }
    }

    //==================================================================================================

    [MenuItem("Assets/Find Prefabs Using Texture in Any Channel", false, 2000)]
    public static void FindPrefabsUsingTextureAnyChannel()
    {
        // Get the selected texture
        Texture2D selectedTexture = Selection.activeObject as Texture2D;

        if (selectedTexture == null)
        {
            Debug.LogWarning("Selected object is not a texture.\n");
            return;
        }

        List<string> results = new List<string>();

        // Find all prefabs in the Assets folder
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                PrefabUsesTextureAnyChannel(prefab, selectedTexture, results);
            }
        }

        foreach (string result in results)
            Debug.Log(result + "\n");

        if (results.Count == 0)
        {
            Debug.Log($"No prefabs found using texture '{selectedTexture.name}' in any channel.\n");
        }
    }

    private static void PrefabUsesTextureAnyChannel(GameObject prefab, Texture2D texture, List<string> results)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;

            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(mat.shader, i);
                        if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            if (mat.GetTexture(propertyName) == texture)
                            {
                                string result = $"{prefab.name}  -  [{mat.name}]";
                                if (propertyName != "_MainTex" && propertyName != "_BumpMap")
                                {
                                    result += $"  -  [{propertyName}]";
                                    // Additional check for packed channels
                                    string packedChannels = GetPackedChannels(mat, propertyName, texture);
                                    if (!string.IsNullOrEmpty(packedChannels))
                                        result += $"  (Packed: {packedChannels})";
                                }
                                results.Add(result);
                            }
                        }
                    }
                }
            }
        }
    }

    private static string GetPackedChannels(Material mat, string propertyName, Texture2D texture)
    {
        // Placeholder for actual packed channel check logic
        // You may need to extend this logic based on how your shaders use packed channels
        // Here, we're simply returning "RGBA" for the sake of demonstration

        // Example logic:
        // If the texture is used as a mask or some other packed channel, determine which channel is used
        // This logic can vary widely based on your specific use case and shader implementation

        // For demonstration, assuming the texture is used in an RGBA packed channel
        // You should replace this with actual logic to determine the specific channels

        if (propertyName.Contains("Mask")) // Example condition
            return "R, G, B, A";

        return string.Empty;
    }





}




