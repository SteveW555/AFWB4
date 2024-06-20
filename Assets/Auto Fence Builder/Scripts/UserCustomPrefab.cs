#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0414 // variable assigned but not used.

using AFWB;
using MeshUtils;
using System.Collections.Generic;
using UnityEngine;

public class UserCustomPrefab
{
    //--------------------------------
    //will attempt to put a custom mesh in to the correct orientation for use in AFWB
    public static RotationType AutoRotateY(GameObject go, PrefabTypeAFWB prefabType)
    {
        Vector3 size = MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(go);
        int yAngle = 0;

        if (prefabType == PrefabTypeAFWB.railPrefab && size.z > size.x * 1.5f)
            yAngle = 90;
        if (prefabType == PrefabTypeAFWB.postPrefab && size.z > size.y * 3)
            yAngle = 90;

        if (yAngle != 0)
        {
            MeshUtilitiesAFWB.RotateY(go, yAngle);
            return RotationType.y90;
        }
        return RotationType.none;
    }

    public static RotationType AutoRotateX(GameObject go, PrefabTypeAFWB prefabType)
    {
        Vector3 size = MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(go);
        int xAngle = 0;

        if (prefabType == PrefabTypeAFWB.railPrefab && size.z > size.y * 1.99f)
            xAngle = 90;
        if (prefabType == PrefabTypeAFWB.postPrefab && size.z > size.y * 1.99f)
            xAngle = 90;

        if (xAngle != 0)
        {
            MeshUtilitiesAFWB.RotateX(go, xAngle);
            return RotationType.x90;
        }
        return RotationType.none;
    }

    public static RotationType AutoRotateZ(GameObject go, PrefabTypeAFWB prefabType)
    {
        Vector3 size = MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(go);
        int zAngle = 0;

        if (prefabType == PrefabTypeAFWB.railPrefab && size.y > size.x * 1.99f)
            zAngle = -90;
        if (prefabType == PrefabTypeAFWB.postPrefab && size.x > size.y * 1.99f)
            zAngle = -90;

        if (zAngle != 0)
        {
            MeshUtilitiesAFWB.RotateZ(go, zAngle);
            return RotationType.z90;
        }
        return RotationType.none;
    }

    //-------------------------------
    // This assumes you're working with a copy
    public static RotationType AutoRotate(GameObject go, AutoFenceCreator af, LayerSet layer, bool resetFirst = false)
    {
        if (resetFirst)
            LoadUtilitiesAFWB.ResetMeshOnUserPrefab(af, layer);
        RotationType rotType = RotationType.none;

        rotType = AutoRotateY(go, af.GetPrefabTypeFromLayer(layer));
        if (rotType == RotationType.none || layer == LayerSet.postLayer)
        {
            rotType = AutoRotateX(go, af.GetPrefabTypeFromLayer(layer));
        }
        if (rotType == RotationType.none)
        {
            rotType = AutoRotateZ(go, af.GetPrefabTypeFromLayer(layer));
        }

        return rotType;
    }

    //-------------------------------
    // This assumes you're working with a copy
    public static RotationType InitialSetup(GameObject go, AutoFenceCreator af, LayerSet layer, bool resetFirst = false)
    {
        RotationType rotType = AutoRotate(go, af, layer, resetFirst);

        PrefabTypeAFWB prefabType = af.GetPrefabTypeFromLayer(layer);

        if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            ScaleRailToDefaultX(go);
            MeshUtilitiesAFWB.SetRailPivotToLeftCentre(go);
        }
        if (prefabType == PrefabTypeAFWB.postPrefab)
        {
            MeshUtilitiesAFWB.SetPivotToCentreBase(go);
        }

        MeshUtilitiesAFWB.UpdateAllColliders(go);

        return rotType;
    }

    //--------------------------------
    // Assumes the orientation is correct with Rail length along the X axis
    public static void ScaleCustomObjectSuitableForType(GameObject go, PrefabTypeAFWB prefabType)
    {
        if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            ScaleRailToDefaultX(go);
        }
        else if (prefabType == PrefabTypeAFWB.postPrefab)
        {
            // First make sure it's not massively under/over sized
            MeshUtilitiesAFWB.ScaleMeshToMinMax(go, 0.1f, 5f);
            MeshUtilitiesAFWB.ScaleMesh(go, new Vector3(0.75f, 1.25f, 0.75f));
            MeshUtilitiesAFWB.AssertMinimumDimension(go, 0.1f);
        }
    }

    //--------------------------------
    // Assumes the orientation is correct with Rail length along the X axis
    public static void ScaleRailToDefaultX(GameObject go)
    {
        //Vector3 center= Vector3.zero, max, min, refSize = Vector3.one;
        Bounds combinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(go);

        float scaleFactorX = 3.0f / combinedBounds.size.x; // Set x scaling, 3 = default rail length
        float scaleFactorZ = scaleFactorX, scaleFactorY = scaleFactorX;

        if (combinedBounds.size.z * scaleFactorZ > 1.5f)
        {
            scaleFactorZ = 1.5f / combinedBounds.size.z;
        }
        if (combinedBounds.size.y * scaleFactorZ > 3.0f)
        {
            scaleFactorY = 3.0f / combinedBounds.size.y;
        }

        scaleFactorY = scaleFactorZ = scaleFactorY <= scaleFactorZ ? scaleFactorY : scaleFactorZ;

        List<GameObject> allMeshGameObjects = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(go);
        List<MeshFilter> allMeshFilters = MeshUtilitiesAFWB.GetAllMeshFiltersFromGameObject(go);
        List<Mesh> meshList = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(go);
        Mesh thisMesh;
        for (int i = 0; i < meshList.Count; i++)
        {
            thisMesh = meshList[i];
            thisMesh = MeshUtilitiesAFWB.ScaleMesh(thisMesh, new Vector3(scaleFactorX, scaleFactorY, scaleFactorZ)); // scale everything
            thisMesh.RecalculateBounds();
            allMeshFilters[i].sharedMesh = thisMesh;// put back in to the GO
        }
        Debug.Log(" ScaleRailToDefaultX() " + scaleFactorX.ToString("F2") + "  " + scaleFactorY.ToString("F2") + "  " + scaleFactorZ.ToString("F2") + "\n");
    }

    //-------------------
    public static void ResetMeshOnPrefab(GameObject go, AutoFenceCreator af, LayerSet layer)
    {
        LoadUtilitiesAFWB.ResetMeshOnUserPrefab(af, layer);

        /*string path = af.userBackupPathPost;
        if(layer == LayerSet.railALayer)
            path = af.userBackupPathRailA;
        if (layer == LayerSet.railBLayer)
            path = af.userBackupPathRailB;
        if (layer == LayerSet.extraLayer)
            path = af.userBackupPathExtra;

        GameObject backup = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        if(backup == null)
        {
            Debug.LogWarning("backup was null in ResetMeshOnPrefab   " + path);
            return;
        }

        Mesh origMeshFromBackup = MeshUtilitiesAFWB.DuplicateMesh(backup);

        GameObject currPrefab = af.GetMainPrefabForLayer(layer);
        currPrefab.GetComponent<MeshFilter>().sharedMesh = origMeshFromBackup;
        af.railPrefabs[af.currentRail_PrefabIndex[0]] = currPrefab;

        af.ResetPoolForLayer(layer);
        af.ForceRebuildFromClickPoints();*/
    }
}