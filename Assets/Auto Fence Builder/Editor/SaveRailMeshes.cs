//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 //
using AFWB;
using MeshUtils;
using System.Collections.Generic;

//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 //3.4
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveRailMeshes : MonoBehaviour
{
    //Saves the procedurally generated Rail meshes produced when using Sheared mode as prefabs, in order to create a working prefab from the Finished AutoFence
    public static string SaveProcRailMeshesAsAssets(AutoFenceCreator af, string dirPath, string hourMinSecStr)
    {//Debug.Log("SaveProcRailMeshesAsAssets()\n");
        if (af.railABuiltCount == 0 && af.railBBuiltCount == 0)
            Debug.Log("No rail meshes needed saving \n");

        List<Transform> rails = af.railsAPool;
        int numRails = 0;
        string meshesFolderName = "Meshes";
        bool cancelled = false;
        int numCreatedA = 0, numUpdatedA = 0, numCreatedB = 0, numUpdatedB = 0;

        //Do the meshes already exist, if so might not need to create folder
        Mesh meshA, meshB;
        bool meshAExists = false, meshBExists = false, createdFolder = false;
        if (af.railsAPool.Count > 0)
        {
            List<Mesh> meshesA = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(af.railsAPool[0].gameObject);
            if (meshesA.Count > 0)
            {
                meshA = meshesA[0];
                meshAExists = AssetDatabase.Contains(meshA);
            }
        }
        if (af.railsBPool.Count > 0)
        {
            List<Mesh> meshesB = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(af.railsBPool[0].gameObject);
            if (meshesB.Count > 0)
            {
                meshB = meshesB[0];
                meshBExists = AssetDatabase.Contains(meshB);
            }
        }
        string meshesDir = dirPath + "/" + meshesFolderName;
        if (!Directory.Exists(meshesDir))
        {
            if (meshAExists == false && meshBExists == false)
            {
                createdFolder = true;
                AssetDatabase.CreateFolder(dirPath, meshesFolderName);
            }
        }

        string railSetStr = "", meshNumStr = "";
        try
        {
            AssetDatabase.StartAssetEditing();
            for (int railSet = 0; railSet < 2; railSet++)
            {
                if (railSet == 0)
                {
                    rails = af.railsAPool;
                    railSetStr = "A";
                    numRails = af.railABuiltCount;
                }
                else if (railSet == 1)
                {
                    rails = af.railsBPool;
                    railSetStr = "B";
                    numRails = af.railBBuiltCount;
                }
                if (numRails > 0 && rails[0] != null)
                {
                    for (int i = 0; i < numRails; i++)
                    {
                        List<Mesh> meshes = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(rails[i].gameObject);
                        int meshCount = meshes.Count;

                        if (railSet == 0)
                            cancelled = EditorUtility.DisplayCancelableProgressBar("Saving Rail-A Meshes...", i.ToString() + " of " + numRails, (float)i / numRails);
                        else if (railSet == 1)
                            cancelled = EditorUtility.DisplayCancelableProgressBar("Saving Rail-B Meshes...", i.ToString() + " of " + numRails, (float)i / numRails);
                        if (cancelled)
                        {
                            //EditorUtility.ClearProgressBar();
                            return "";
                        }
                        if (rails[i] != null && meshCount > 0)
                        {
                            for (int m = 0; m < meshCount; m++)
                            {
                                Mesh mesh = meshes[m];
                                if (mesh == null)
                                {
                                    Debug.LogWarning(rails[i].gameObject.name + ": Mesh " + m + " was null. Not saved");
                                    continue;
                                }
                                if (meshCount == 1)
                                    meshNumStr = "";
                                else
                                    meshNumStr = "(m" + m.ToString() + ")";

                                string meshName = mesh.name;
                                if (meshName == "")
                                { // a sheared mesh was not made because it intersected with the ground, so omit it (set in 'Auto Hide Buried Rails')
                                    continue;
                                }
                                else
                                {
                                    string newMeshName = mesh.name + "-" + railSetStr + i + meshNumStr + "-" + hourMinSecStr;
                                    try
                                    {
                                        if (AssetDatabase.Contains(mesh))
                                        {
                                            AssetDatabase.SaveAssets();
                                            if (railSet == 0)
                                                numUpdatedA++;
                                            else if (railSet == 1)
                                                numUpdatedB++;
                                        }
                                        else
                                        {
                                            if (Directory.Exists(meshesDir) == false)
                                            {
                                                EditorUtility.ClearProgressBar();
                                                Debug.Log("Directory Missing! : " + meshesDir + " Meshes not saved.");
                                            }
                                            string path = meshesDir + "/" + newMeshName + ".asset";
                                            AssetDatabase.CreateAsset(mesh, path);
                                            //Debug.Log("path:   " + meshesDir + "---------------\n");
                                            if (railSet == 0)
                                                numCreatedA++;
                                            else if (railSet == 1)
                                                numCreatedB++;
                                        }
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.LogWarning("Problem Creating mesh asset in SaveProcRailMeshesAsAssets(). " + e.ToString() + "\n");
                                        ReportSavedMeshes(numUpdatedA, numUpdatedB, numCreatedA, numCreatedB, af.railABuiltCount, af.railBBuiltCount);
                                        AssetDatabase.StopAssetEditing();
                                        EditorUtility.ClearProgressBar();
                                        return "";
                                    }
                                }
                            }
                        }
                    }
                    EditorUtility.ClearProgressBar();
                }
                if (railSet == 0 && numRails > 0)
                {
                    if (numUpdatedA != numRails && numCreatedA != numRails)
                    {
                        Debug.LogWarning("Expected " + numRails + " Rails A.    Created: " + numCreatedA + "   Updated; " + numUpdatedA + "\n");
                    }
                }
                if (railSet == 1 && numRails > 0)
                {
                    if (numUpdatedB != numRails && numCreatedB != numRails)
                    {
                        Debug.LogWarning("Expected " + numRails + " Rails B.    Created: " + numCreatedB + "   Updated; " + numUpdatedB + "\n");
                    }
                }
            }
            EditorUtility.ClearProgressBar();

            AssetDatabase.StopAssetEditing();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Problem Creating mesh asset in SaveProcRailMeshesAsAssets() 2. " + e.ToString() + "\n");
            return "";
        }
        ReportSavedMeshes(numUpdatedA, numUpdatedB, numCreatedA, numCreatedB, af.railABuiltCount, af.railBBuiltCount);
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        return meshesDir;
    }

    //-------------------
    private static void ReportSavedMeshes(int numUpdatedA, int numUpdatedB, int numCreatedA, int numCreatedB, int railsCountA, int railsCountB)
    {
        if (numUpdatedA == 0 && numUpdatedB == 0 && numCreatedA == 0 && numCreatedB == 0)
        {
            Debug.Log("No meshes were created or updated\n");
            return;
        }

        string expectedStr = "Expected:  " + railsCountA + " Rails A  &  " + railsCountB + " Rails B.        ";

        //Debug.Log("Expected:  " + railsCountA + " Rails A  &  " + railsCountB  + " Rails B \n");

        if (numUpdatedA > 0)
            Debug.Log(expectedStr + "Updated " + numUpdatedA + " Rails A \n");
        if (numUpdatedB > 0)
            Debug.Log(expectedStr + "Updated " + numUpdatedB + " Rails B \n");
        if (numCreatedA > 0)
            Debug.Log(expectedStr + "Created " + numCreatedA + " Rails A \n");
        if (numCreatedB > 0)
            Debug.Log(expectedStr + "Created " + numCreatedB + " Rails B \n");
    }

    //-------------------
    private static string GetRailNameWithoutSuffix(Transform rail)
    {
        int index = rail.gameObject.name.IndexOf("_Panel_Rail");
        if (index == -1)
            index = rail.gameObject.name.IndexOf("_Rail");
        if (index == -1)
            index = rail.gameObject.name.Length > 10 ? 9 : rail.gameObject.name.Length - 1;

        string newMeshName = rail.gameObject.name.Remove(index);
        return newMeshName;
    }
}