using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceMeshMerge : MonoBehaviour
{
    public GameObject CreateMergedPrefabs(string hourMinSecStr, string name, bool addBoxColliders = true)
    {
        List<GameObject> finishedMergedObjects = new List<GameObject>();

        //-- CreateMergedPrefabs New Folder for the copy -----
        GameObject mergedCopyFolder = new GameObject(gameObject.name + "[Merged]");
        //Vector3 adjustedPosition = Vector3.zero; // we'll set the final position of everything to the first post position
        mergedCopyFolder.transform.position = gameObject.transform.position;
        //=========== Rails ==============
        List<Transform> railsMergedFolders = GetAllDividedFolders("Rails");
        for (int i = 0; i < railsMergedFolders.Count; i++)
        {
            List<GameObject> allRails = GetAllGameObjectsFromDividedFolder(railsMergedFolders[i]);
            Debug.Log("Merging " + allRails.Count + " Rails");

            if (allRails != null && allRails.Count > 0)
            {
                string nameBase = "Rails A Merged ";
                if (i == 1)
                    nameBase = "Rails B Merged ";
                GameObject mergedObj = CombineNestedGameObjects(allRails, nameBase + i, gameObject.transform.position);
                mergedObj.transform.parent = mergedCopyFolder.transform;
                finishedMergedObjects.Add(mergedObj);
                //Creating Colliders
                if (addBoxColliders)
                {
                    for (int j = 0; j < allRails.Count; j++)
                    {
                        GameObject thisRail = allRails[j];
                        BoxCollider coll = thisRail.GetComponent<BoxCollider>(); // does the original have a collider
                        if (coll != null)
                        {
                            //CreateMergedPrefabs Colliders by duplicating the rails, then destroying everything except the new colliders
                            // this will only work on unnested parts that have box colliders. Else makes more sense to add custom colliders
                            GameObject colliderDummy = Instantiate(thisRail) as GameObject; // debugging
                            colliderDummy.name = thisRail.name + "_BoxCollider";
                            colliderDummy.transform.parent = mergedObj.transform;
                            Vector3 pos = colliderDummy.transform.position;
                            colliderDummy.transform.position = gameObject.transform.position + pos;
                            MeshFilter mf = colliderDummy.GetComponent<MeshFilter>();//debug
                            if (mf)
                                DestroyImmediate(mf);
                            MeshRenderer mr = colliderDummy.GetComponent<MeshRenderer>();
                            if (mr)
                                DestroyImmediate(mr);
                        }
                    }
                }
            }
        }
        //=========== Posts ==============
        List<Transform> postsMergedFolders = GetAllDividedFolders("Posts");
        if (postsMergedFolders != null)
        {
            for (int i = 0; i < postsMergedFolders.Count; i++)
            {
                List<GameObject> allPosts = GetAllGameObjectsFromDividedFolder(postsMergedFolders[i]);
                if (allPosts != null && allPosts.Count > 0)
                {
                    GameObject mergedObj = CombineNestedGameObjects(allPosts, "Posts Merged " + i, gameObject.transform.position);
                    if (mergedObj != null)
                    {
                        mergedObj.transform.parent = mergedCopyFolder.transform;
                        finishedMergedObjects.Add(mergedObj);
                    }
                }
            }
        }
        //=========== Subs ==============
        List<Transform> subsMergedFolders = GetAllDividedFolders("Subs");
        for (int i = 0; i < subsMergedFolders.Count; i++)
        {
            List<GameObject> allSubs = GetAllGameObjectsFromDividedFolder(subsMergedFolders[i]);
            if (allSubs != null && allSubs.Count > 0)
            {
                GameObject mergedObj = CombineNestedGameObjects(allSubs, "Subposts Merged " + i, gameObject.transform.position);
                if (mergedObj != null)
                {
                    mergedObj.transform.parent = mergedCopyFolder.transform;
                    finishedMergedObjects.Add(mergedObj);
                }
            }
        }
        //=========== Extras ==============
        List<Transform> extrasMergedFolders = GetAllDividedFolders("Extras");
        for (int i = 0; i < extrasMergedFolders.Count; i++)
        {
            List<GameObject> allExtras = GetAllGameObjectsFromDividedFolder(extrasMergedFolders[i]);
            if (allExtras != null && allExtras.Count > 0)
            {
                GameObject mergedObj = CombineNestedGameObjects(allExtras, "Extras Merged " + i, gameObject.transform.position);
                if (mergedObj != null)
                {
                    mergedObj.transform.parent = mergedCopyFolder.transform;
                    finishedMergedObjects.Add(mergedObj);
                }
            }
        }

        /*if (saveMergedMeshes == true)
        {
            //====  Save a folder with the merged mesh in to Auto Fence Builder/FinishedData
            string folderName = "MergedMeshes-" + hourMinSecStr;
            SaveMeshes(finishedMergedObjects, folderName);
        }*/
        return mergedCopyFolder;
    }

    //--------------------------------
    private List<Mesh> GetAllMeshesFromGameObject(GameObject inGO)
    {
        Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
        List<Mesh> meshes = new List<Mesh>();
        foreach (Transform child in allObjects)
        {
            MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
            if (mf != null)
            {
                Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                if (thisObjectMesh != null)
                    meshes.Add(thisObjectMesh);
            }
        }
        return meshes;
    }

    //----------
    private List<GameObject> GetAllMeshGameObjectsFromGameObject(GameObject inGO)
    {
        Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
        List<GameObject> allGameObjects = new List<GameObject>();
        foreach (Transform child in allObjects)
        {
            MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
            if (mf != null)
            {
                allGameObjects.Add(child.gameObject);
            }
        }
        return allGameObjects;
    }

    //---------------------------
    private GameObject CombineNestedGameObjects(List<GameObject> allGameObjects, string name, Vector3 positionOffset = default(Vector3))
    {
        GameObject combinedObject = new GameObject(name);
        ArrayList combineInstanceArrays = new ArrayList();
        ArrayList mats = new ArrayList();

        GameObject thisGO = null;

        for (int i = 0; i < allGameObjects.Count; i++)
        {
            thisGO = allGameObjects[i];
            if (thisGO == null)
            {
                Debug.LogWarning("CombineNestedGameObjects():  ** GameObject Missing **  [" + i + "]");
                continue;
            }
            //else
            // Debug.Log("CombineNestedGameObjects():  GameObject OK   " + thisGO.name + "[" + i + "]");

            thisGO.transform.position -= positionOffset;
            MeshFilter[] meshFilters = thisGO.GetComponentsInChildren<MeshFilter>(true);

            if (meshFilters == null || meshFilters.Length == 0)
            {
                Debug.LogWarning("CombineNestedGameObjects():  ** meshFilters Count is 0 for GameObject [" + i + "]");
                continue;
            }

            //int count = 0;
            //foreach (MeshFilter meshFilter in meshFilters)
            for (int j = 0; j < meshFilters.Length; j++)
            {
                MeshRenderer meshRenderer = meshFilters[j].GetComponent<MeshRenderer>();
                if (!meshRenderer)
                {
                    Debug.LogWarning("Missing MeshRenderer.");
                    continue;
                }

                if (meshFilters[j] == null)
                {
                    Debug.LogWarning("CombineNestedGameObjects():  ** MeshFilter Missing! **   " + thisGO.name + "[" + i + "] meshFilters[" + j + "] was null");
                    continue;
                }
                //else
                //Debug.Log("CombineNestedGameObjects():  MeshFilter OK   " + thisGO.name + "[" + i + "] meshFilters[" + j + "]");

                if (meshFilters[j].sharedMesh == null)
                {
                    Debug.LogWarning("CombineNestedGameObjects():  ** Mesh Missing! **   " + thisGO.name + "[" + i + "] mesh[" + j + "] was null");
                    continue;
                }
                //else
                //Debug.Log("CombineNestedGameObjects():  Mesh OK   " + thisGO.name + "[" + i + "] mesh[" + j + "]");

                if (meshRenderer.sharedMaterials.Length != meshFilters[j].sharedMesh.subMeshCount)
                {
                    Debug.LogWarning("Incorrect materials count: " + meshRenderer.sharedMaterials.Length + " Materials,  " +
                    meshFilters[j].sharedMesh.subMeshCount + " subMeshes");
                    continue;
                }

                for (int s = 0; s < meshFilters[j].sharedMesh.subMeshCount; s++)
                {
                    //Debug.Log("i=" + i + " j=" + j + "   s = " + s + "/" + meshFilters[j].sharedMesh.subMeshCount);

                    int materialArrayIndex = MaterialsContain(mats, meshRenderer.sharedMaterials[s].name);
                    if (materialArrayIndex == -1)
                    {
                        mats.Add(meshRenderer.sharedMaterials[s]);
                        materialArrayIndex = mats.Count - 1;
                    }
                    combineInstanceArrays.Add(new ArrayList());
                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = meshFilters[j].sharedMesh;
                    (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
                }
            }
            thisGO.transform.position += positionOffset; // because we temorarily modified the source to get the local position
        }

        MeshFilter meshFilterCombine = combinedObject.GetComponent<MeshFilter>();
        if (!meshFilterCombine)
            meshFilterCombine = combinedObject.AddComponent<MeshFilter>();

        Mesh[] meshes = new Mesh[mats.Count];
        CombineInstance[] combineInstances = new CombineInstance[mats.Count];

        for (int m = 0; m < mats.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        //Combine
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);
        meshFilterCombine.sharedMesh.name = name;

        foreach (Mesh mesh in meshes)
        {
            mesh.Clear();
            DestroyImmediate(mesh);
        }

        MeshRenderer meshRendererCombine = combinedObject.GetComponent<MeshRenderer>();
        if (!meshRendererCombine)
            meshRendererCombine = combinedObject.AddComponent<MeshRenderer>();

        Material[] materialsArray = mats.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        combinedObject.transform.position += positionOffset;
        return combinedObject;
    }

    //----
    private int MaterialsContain(ArrayList matArray, string searchName)
    {
        for (int i = 0; i < matArray.Count; i++)
        {
            if (((Material)matArray[i]).name == searchName)
            {
                return i;
            }
        }
        return -1;
    }

    //------------------------------------------
    private List<GameObject> GetAllGameObjectsFromDividedFolder(Transform dividedFolder)
    {
        int numChildren = dividedFolder.childCount;
        List<GameObject> goList = new List<GameObject>();
        for (int i = 0; i < numChildren; i++)
        {
            goList.Add(dividedFolder.GetChild(i).gameObject);
        }
        return goList;
    }

    //------------------------------------------
    private List<Transform> GetAllDividedFolders(string folderName)
    {
        GameObject masterFolder = gameObject;

        Transform mainFolder = masterFolder.transform.Find(folderName);

        if (mainFolder == null) return null;

        int numChildren = mainFolder.childCount;

        Transform thisChild;
        List<Transform> dividedFolders = new List<Transform>();
        for (int i = 0; i < numChildren; i++)
        {
            thisChild = mainFolder.GetChild(i);
            if (folderName == "Rails" && thisChild.name.StartsWith("RailsAGroupedFolder"))
            {
                dividedFolders.Add(thisChild);
            }
            if (folderName == "Rails" && thisChild.name.StartsWith("RailsBGroupedFolder"))
            {
                dividedFolders.Add(thisChild);
            }
            else if (folderName == "Posts" && thisChild.name.StartsWith("PostsGroupedFolder"))
            {
                dividedFolders.Add(thisChild);
            }
            else if (folderName == "Subs" && thisChild.name.StartsWith("SubsGroupedFolder"))
            {
                dividedFolders.Add(thisChild);
            }
            else if (folderName == "Extras" && thisChild.name.StartsWith("ExtrasGroupedFolder"))
            {
                dividedFolders.Add(thisChild);
            }
        }

        return dividedFolders;
    }

    //---------------------------
    private string GetPartialTimeString(bool includeDate = false)
    {
        DateTime currentDate = System.DateTime.Now;
        string timeString = currentDate.ToString();
        timeString = timeString.Replace("/", "-"); // because the / in that will upset the path
        timeString = timeString.Replace(":", "-"); // because the / in that will upset the path
        if (timeString.EndsWith(" AM") || timeString.EndsWith(" PM"))
        { // windows??
            timeString = timeString.Substring(0, timeString.Length - 3);
        }
        if (includeDate == false)
            timeString = timeString.Substring(timeString.Length - 8);
        return timeString;
    }
}