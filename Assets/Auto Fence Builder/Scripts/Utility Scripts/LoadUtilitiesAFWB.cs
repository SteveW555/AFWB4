using AFWB;
using MeshUtils;
using UnityEditor;
using UnityEngine;

public class LoadUtilitiesAFWB
{
    //-------------------------------
    //loads a backup and reassigns its mesh to the current prefab
    public static void ResetMeshOnUserPrefab(AutoFenceCreator af, LayerSet layer)
    {
        GameObject curr = af.GetMainPrefabForLayer(layer);
        GameObject user = af.GetUserPrefabForLayer(layer);

        Debug.Log(curr);
        Debug.Log(user);

        if (curr != user)
        {
            af.SetCurrentPrefabForLayer(user, layer);
            Debug.Log("Not the same in ResetMeshOnUserPrefab()");
        }

        string backupPath = af.GetUserPrefabBackupPath(layer);

        if (backupPath == "")
        {
            Debug.Log("Couldn't reset mesh as backup path was invalid\n");
            return;
        }
        //GameObject backupGo = Resources.Load<GameObject>(Path.GetFileName(backupPath));

        //-- Load & Duplicate original mesh
        GameObject origGo = AssetDatabase.LoadMainAssetAtPath(backupPath) as GameObject;
        if (origGo == null || MeshUtilitiesAFWB.GetFirstMeshInGameObject(origGo) == null)
        {
            Debug.Log("Couldn't reset mesh as backup GameObject was invalid\n");
            return;
        }
        Mesh origMesh = MeshUtilitiesAFWB.DuplicateMesh(origGo);

        if (layer == LayerSet.postLayer)
        {
            //-- replace the mesh on the current prefab
            GameObject currPrefab = af.postPrefabs[af.currentPost_PrefabIndex];
            currPrefab.GetComponent<MeshFilter>().sharedMesh = origMesh;
            af.postPrefabs[af.currentPost_PrefabIndex] = currPrefab;
        }
        if (layer == LayerSet.railALayer)
        {
            GameObject currPrefab = af.railPrefabs[af.currentRail_PrefabIndex[0]];
            currPrefab.GetComponent<MeshFilter>().sharedMesh = origMesh;
            af.railPrefabs[af.currentRail_PrefabIndex[0]] = currPrefab;
        }
        if (layer == LayerSet.railBLayer)
        {
            GameObject currPrefab = af.railPrefabs[af.currentRail_PrefabIndex[1]];
            currPrefab.GetComponent<MeshFilter>().sharedMesh = origMesh;
            af.railPrefabs[af.currentRail_PrefabIndex[1]] = currPrefab;
        }
        if (layer == LayerSet.extraLayer)
        {
            GameObject currPrefab = af.extraPrefabs[af.currentExtra_PrefabIndex];
            currPrefab.GetComponent<MeshFilter>().sharedMesh = origMesh;
            af.extraPrefabs[af.currentExtra_PrefabIndex] = currPrefab;
        }

        af.ResetPoolForLayer(layer);
        af.ForceRebuildFromClickPoints();
    }

    //
}