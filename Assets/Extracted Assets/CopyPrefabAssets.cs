using System.IO;

using UnityEditor;

using UnityEngine;

public class CopyPrefabAssets : MonoBehaviour
{
    [MenuItem("Assets/Copy Prefab Assets", true)]
    private static bool ValidateCopyPrefabAssets()
    {
        // Validate that the selected objects are prefabs
        foreach (var obj in Selection.objects)
        {
            if (obj is GameObject)
            {
                return true;
            }
        }
        return false;
    }

    [MenuItem("Assets/Copy Prefab Assets")]
    private static void CopyPrefabAssetsMenu()
    {
        // Iterate over all selected prefabs
        foreach (var obj in Selection.objects)
        {
            if (obj is GameObject selectedPrefab)
            {
                CopyAssetsForPrefab(selectedPrefab);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CopyAssetsForPrefab(GameObject selectedPrefab)
    {
        // Get the path to the selected prefab
        string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
        string directoryPath = Path.GetDirectoryName(prefabPath);
        string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

        // Create a new folder with the same name as the prefab
        string newFolderPath = Path.Combine(directoryPath, prefabName);
        if (!AssetDatabase.IsValidFolder(newFolderPath))
        {
            AssetDatabase.CreateFolder(directoryPath, prefabName);
        }

        // Iterate through all renderers in the prefab
        Renderer[] renderers = selectedPrefab.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material != null)
                {
                    CopyMaterial(material, newFolderPath);
                }
            }

            // Copy the FBX model if it's a SkinnedMeshRenderer or MeshRenderer
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                Mesh mesh = skinnedMeshRenderer.sharedMesh;
                if (mesh != null)
                {
                    CopyModel(mesh, newFolderPath);
                }
            }
            else if (renderer is MeshRenderer meshRenderer)
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    if (mesh != null)
                    {
                        CopyModel(mesh, newFolderPath);
                    }
                }
            }
        }
    }

    private static void CopyMaterial(Material material, string directoryPath)
    {
        // Get the path to the source material
        string materialPath = AssetDatabase.GetAssetPath(material);
        if (string.IsNullOrEmpty(materialPath))
        {
            Debug.LogError("Material path not found for " + material.name);
            return;
        }

        // Create the destination path
        string destinationMaterialPath = Path.Combine(directoryPath, material.name + ".mat");
        destinationMaterialPath = AssetDatabase.GenerateUniqueAssetPath(destinationMaterialPath);

        // Copy the material asset
        AssetDatabase.CopyAsset(materialPath, destinationMaterialPath);

        // Copy the textures
        Shader shader = material.shader;
        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture texture = material.GetTexture(propertyName);

                if (texture != null)
                {
                    CopyTexture(texture, directoryPath);
                }
            }
        }
    }

    private static void CopyTexture(Texture texture, string directoryPath)
    {
        // Get the path to the source texture
        string texturePath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(texturePath))
        {
            Debug.LogError("Texture path not found for " + texture.name);
            return;
        }

        // Create the destination path
        string destinationTexturePath = Path.Combine(directoryPath, texture.name + Path.GetExtension(texturePath));
        destinationTexturePath = AssetDatabase.GenerateUniqueAssetPath(destinationTexturePath);

        // Copy the texture asset
        AssetDatabase.CopyAsset(texturePath, destinationTexturePath);
    }

    private static void CopyModel(Mesh mesh, string directoryPath)
    {
        // Get the path to the source mesh
        string meshPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(meshPath))
        {
            Debug.LogError("Mesh path not found for " + mesh.name);
            return;
        }

        // Find the associated FBX model file
        string modelPath = Path.GetDirectoryName(meshPath);
        string fbxFileName = Path.GetFileNameWithoutExtension(meshPath) + ".fbx";
        string fbxFilePath = Path.Combine(modelPath, fbxFileName);

        if (File.Exists(fbxFilePath))
        {
            string destinationModelPath = Path.Combine(directoryPath, fbxFileName);
            destinationModelPath = AssetDatabase.GenerateUniqueAssetPath(destinationModelPath);

            // Copy the FBX model file
            File.Copy(fbxFilePath, destinationModelPath);
            AssetDatabase.ImportAsset(destinationModelPath);
        }
        else
        {
            Debug.LogError("FBX model file not found for " + mesh.name);
        }
    }
}