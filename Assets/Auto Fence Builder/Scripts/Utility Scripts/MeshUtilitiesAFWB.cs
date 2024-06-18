#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0168
#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AFWB;
using System;
using UnityEngine.UIElements;
using System.Security.Cryptography;

// A structure to hold the verticesSides UVs and triangle-indices of a quad
public struct QuadData
{
    public Vector3[] v;
    public Vector2[] uv;
    public int[] t;

    public QuadData(Vector3[] v, Vector2[] uv, int[] t)
    {
        v = new Vector3[4];
        this.v = v;
        uv = new Vector2[4];
        this.uv = uv;
        t = new int[6];
        this.t = t;
    }
    public void OffsetQuad(Vector3 offset)
    {
        for (int i = 0; i < 4; i++)
            v[i] += offset;
    }
}
namespace ProceduralToolkitTCT
{
    public class ProcGenAFWB : MonoBehaviour
    {
        public static void Test(float radius, int numSegments, float height)
        {
            // Create a cylinder
            var cylinder = MeshDraftTCT.Cylinder(radius, numSegments, height);
            // Create a mesh from the draft
            var mesh = cylinder.ToMesh();
            // Create a game object with MeshFilter and MeshRenderer
            var go = new GameObject("Cylinder");
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>();
            Instantiate(go);

        }
    }
}
namespace MeshUtils
{
    public class MeshUtilitiesAFWB : MonoBehaviour
    {
        public static Mesh ScaleMesh(Mesh m, Vector3 scale, bool adjustForPivot = false, bool recalculateBounds = true)
        {
            if (scale.x == 0 || scale.y == 0 || scale.z == 0)
            {
                Debug.LogWarning("Atempt to scale mesh by zero in ScaleMesh(). Returned without scaling");
                return m;
            }

            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3[] verts = m.vertices;
            Vector3 v;
            int n = m.vertices.Length;

            float sizeX = m.bounds.size.x;
            float sizez = m.bounds.size.z;

            // will ensure that it's scaled from centre instead of pos
            if (adjustForPivot)
            {
                for (int i = 0; i < n; i++)
                {
                    newVerts[i] = Vector3.Scale(verts[i], scale);
                    newVerts[i].x -= sizeX;
                }
            }
            else // to avoid an if in every loop
            {
                for (int i = 0; i < n; i++)
                {
                    newVerts[i] = Vector3.Scale(verts[i], scale);
                }
            }
            m.vertices = newVerts;

            if (recalculateBounds)
                m.RecalculateBounds();

            return m;
        }
        public static Mesh ScaleMesh2(Mesh m, Vector3 scale, bool adjustForPivot = false, bool recalculateBounds = true)
        {
            if (scale.x == 0 || scale.y == 0 || scale.z == 0)
            {
                Debug.LogWarning("Attempt to scale mesh by zero in ScaleMesh2(). Returned without scaling");
                return m;
            }

            Vector3[] verts = m.vertices;
            Vector3[] newVerts = new Vector3[verts.Length];
            Vector3[] normals = m.normals;
            Vector3[] newNormals = new Vector3[normals.Length];
            Vector4[] tangents = m.tangents;
            Vector4[] newTangents = new Vector4[tangents.Length];
            int n = verts.Length;

            float sizeX = m.bounds.size.x;

            // Scale vertices and optionally adjust for pivot
            for (int i = 0; i < n; i++)
            {
                newVerts[i] = Vector3.Scale(verts[i], scale);
                if (adjustForPivot)
                    newVerts[i].x -= sizeX;
            }
            m.vertices = newVerts;

            bool reverseWindingOrder = (scale.x < 0) || (scale.y < 0) || (scale.z < 0);

            if (reverseWindingOrder)
            {
                // Reverse winding order
                int[] triangles = m.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i];
                    triangles[i] = triangles[i + 2];
                    triangles[i + 2] = temp;
                }
                m.triangles = triangles;

                // Flip normals and tangents for each axis individually
                for (int i = 0; i < normals.Length; i++)
                {
                    newNormals[i] = -normals[i];
                    newTangents[i] = -tangents[i];

                    /*if (scale.x < 0)
                    {
                        newNormals[i].x = -newNormals[i].x;
                        newTangents[i].x = -newTangents[i].x;
                    }
                    if (scale.y < 0)
                    {
                        newNormals[i].y = -newNormals[i].y;
                        newTangents[i].y = -newTangents[i].y;
                    }
                    if (scale.z < 0)
                    {
                        newNormals[i].z = -newNormals[i].z;
                        newTangents[i].z = -newTangents[i].z;
                    }*/
                }
            }
            else
            {
                for (int i = 0; i < normals.Length; i++)
                {
                    newNormals[i] = normals[i];
                    newTangents[i] = tangents[i];
                }
            }
            m.normals = newNormals;
            m.tangents = newTangents;

            if (recalculateBounds)
                m.RecalculateBounds();

            // Ensure the normals and tangents are recalculated properly
            m.RecalculateNormals();
            m.RecalculateTangents();

            return m;
        }




        public static Mesh FlipNormalZ(Mesh m, bool recalculateBounds = true)
        {
            Vector3[] normals = m.normals;
            Vector3[] newNormals = new Vector3[normals.Length];
            int n = newNormals.Length;


            // Flip normals
            for (int i = 0; i < n; i++)
            {
                newNormals[i] = normals[i];
                newNormals[i].z = -newNormals[i].z;
            }
            m.normals = newNormals;

            if (recalculateBounds)
                m.RecalculateBounds();

            return m;
        }

        //--------------------------------
        //Uses the mesh svSize, and the various transforms and scale factors to find the highest point of the GO in the scene
        //Returns the total height of the GO, and the y position of the base of the model
        // Useful for positioning a marker or something that must be visible above the highest point of the model
        public static (float, float) CalculateHighestPointOfLayer(AutoFenceCreator af, LayerSet layer)
        {
            //Is layer enabled
            if (af.IsLayerEnabled(layer) == false)
                return (0, 0);


            float meshHeight = 0, goTransformScaleY = 0, goTransformPosY = 0, totalHeight = 0, totalYOffset = 0;
            Mesh mesh = null;

            GameObject go = af.GetMainPrefabForLayer(layer);

            Vector3 localScale = af.GetScaleTransformForLayer(layer);
            Vector3 posOffset = af.GetPositionTransformForLayer(layer);

            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null)
            {
                mesh = mf.sharedMesh;
                //get the height of the mesh
                meshHeight = mesh.bounds.size.y;
                //get the lowest point of the mesh, this is relative to the pos point
                float lowestPoint = mesh.bounds.min.y;
            }

            totalHeight = meshHeight * localScale.y * af.globalScale.y;

            if (layer == LayerSet.postLayerSet)
            {
                totalHeight *= af.mainPostsSizeBoost.y;
                totalHeight *= af.endPostsSizeBoost.y;
            }


            totalYOffset = posOffset.y;
            return (totalHeight, totalYOffset);
        }
        //--------------------------------
        public static Mesh ScaleMesh(GameObject go, Vector3 scale, bool adjustForPivot = false, bool recalculateBounds = true)
        {
            Mesh m = ScaleMesh(go.GetComponent<MeshFilter>().sharedMesh, scale, adjustForPivot, recalculateBounds);

            return m;
        }
        //--------------------------------
        public static void ScaleAllMeshesInGO(GameObject go, Vector3 scale, bool adjustForPivot = false, bool recalculateBounds = true)
        {

            List<Mesh> meshList = GetAllMeshesFromGameObject(go);

            foreach (var mesh in meshList)
            {
                //ScaleMesh(go.GetComponent<MeshFilter>().sharedMesh, scale, adjustForPivot, recalculateBounds);
                ScaleMesh(mesh, scale, adjustForPivot, recalculateBounds);
            }

        }
        /// <summary>
        /// Calculates the final world size of a GameObject, combing Total messhes size with any Transform Scaling
        /// </summary>
        /// <param name="incChildren">A boolean value indicating whether to include the size of child GameObjects in the calculation. Default is true.</param>
        /// <returns>A Vector3 representing the world size of the GameObject.</returns>
        public static Vector3 GetWorldSizeOfGameObject(GameObject go, LayerSet layer, AutoFenceCreator af, bool incChildren = true)
        {
            Vector3 size = Vector3.zero;
            size = GetWorldSizeOfGameObject(go, af.GetScaleTransformForLayer(layer), af, incChildren);
            return size;
        }
        public static Vector3 GetWorldSizeOfGameObject(GameObject go, Vector3 transformScale , AutoFenceCreator af, bool incChildren = true)
        {
            Vector3 size = Vector3.zero;
            if (incChildren)
                size = GetTotalMeshesSizeRecursive(go);
            else
                size = go.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            size = Vector3.Scale(size, transformScale);
            return size;
        }
        //--------------------------------
        public static Vector3 GetMeshSize(GameObject go, bool incChildren = true)
        {
            Vector3 size = Vector3.zero;
            if (incChildren)
                size = GetTotalMeshesSizeRecursive(go);
            else
                size = go.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            return size;
        }
        //-------------------------------
        private static Vector3 GetCombinedMeshSize(GameObject gameObject)
        {
            Bounds combinedBounds = new Bounds(gameObject.transform.position, Vector3.zero);
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf.sharedMesh != null)
                    combinedBounds.Encapsulate(mf.sharedMesh.bounds);
            }

            // Include all SkinnedMeshRenderers
            /*SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
            {
                if (smr.sharedMesh != null)
                    combinedBounds.Encapsulate(smr.sharedMesh.bounds);
            }*/
            // Log the combined size
            Vector3 size = combinedBounds.size;
            return size;
        }

        //--------------------------------
        /// <summary>
        /// Calculates the overall total size of all meshes within a GameObject and its children.
        /// </summary>
        /// <returns>A Vector3 representing the size of the bounding box that encompasses all meshes.</returns>
        public static Vector3 GetTotalMeshesSizeRecursive(GameObject go)
        {
            List<Mesh> allmeshes = GetAllMeshesFromGameObject(go);
            // Calculate the min and max dimensions for all the meshes
            Vector3 min = new Vector3(1000000, 1000000, 1000000);
            Vector3 max = new Vector3(-1000000, -1000000, -1000000);
            Vector3 v;
            foreach (var mesh in allmeshes)
            {
                v = mesh.bounds.min;
                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.z < min.z) min.z = v.z;

                v = mesh.bounds.max;
                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
                if (v.z > max.z) max.z = v.z;
            }
            Vector3 size = max - min;
            return size;
        }

        //--------------------------------
        public static void PrintMeshSize(GameObject go)
        {
            Vector3 size = go.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            Debug.Log(go.name + "Mesh svSize: " + size + "\n");
        }
        //-----------
        public static Mesh ScaleAndTranslateMesh(Mesh m, Vector3 scale, Vector3 translate, bool recalculateBounds)
        {
            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3[] verts = m.vertices;
            Vector3 v;
            int n = m.vertices.Length;

            for (int i = 0; i < n; i++)
            {
                newVerts[i] = Vector3.Scale(verts[i], scale) + translate;
            }
            m.vertices = newVerts;
            if (recalculateBounds)
                m.RecalculateBounds();
            return m;
        }
        //----------------------------------
        public static List<Mesh> ScaleMeshList(List<Mesh> meshList, Vector3 scale, bool recalculateBounds = true)
        {

            Vector3[] verts, newVerts = new Vector3[meshList[0].vertices.Length];
            Vector3 v;
            int numMeshes = meshList.Count;

            for (int n = 0; n < numMeshes; n++)
            {
                newVerts = new Vector3[meshList[n].vertices.Length];
                verts = meshList[n].vertices;
                int len = meshList[n].vertices.Length;
                for (int i = 0; i < len; i++)
                {
                    v = verts[i];
                    v = Vector3.Scale(v, scale);
                    newVerts[i] = v;
                }
                meshList[n].vertices = newVerts;
                if (recalculateBounds)
                    meshList[n].RecalculateBounds();
            }
            return meshList;
        }
        //--------------------------------
        public static Mesh TranslateMesh(Mesh m, float x, float y, float z)
        {
            m = TranslateMesh(m, new Vector3(x, y, z));
            return m;
        }
        //--------------------------------
        public static Mesh TranslateMesh(GameObject go, Vector3 translate, bool recalculateBounds = true)
        {
            Mesh m = GetFirstMeshInGameObject(go);
            if (m != null)
                m = TranslateMesh(m, translate);
            else
                Debug.LogWarning(go.name + " Mesh was null in TranslateMesh()\n");
            return m;
        }
        //--------------------------------
        public static Mesh TranslateMesh(Mesh m, Vector3 translate, bool recalculateBounds = true)
        {
            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3[] verts = m.vertices;
            Vector3 v;
            int n = m.vertices.Length;

            for (int i = 0; i < n; i++)
            {
                v = verts[i];
                v += translate;
                newVerts[i] = v;
            }
            m.vertices = newVerts;
            if (recalculateBounds)
                m.RecalculateBounds();
            return m;
        }
        //--------------------------------
        public static Mesh AddRandomVertexOffsets(Mesh m, Vector3 jitterAmount)
        {
            return AddRandomVertexOffsets(m, jitterAmount.x, jitterAmount.y, jitterAmount.z);
        }
        public static Mesh AddRandomVertexOffsets(Mesh m, float x, float y, float z)
        {

            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3 v;
            int index;


            List<List<int>> vertSets = FindDuplicateVertices(m.vertices);

            for (int i = 0; i < vertSets.Count; i++)
            {
                List<int> thisVertSet = vertSets[i];

                Vector3 randVec = new Vector3(UnityEngine.Random.Range(-x, x),
                                                UnityEngine.Random.Range(-y, y),
                                                UnityEngine.Random.Range(-z, z));
                v = m.vertices[thisVertSet[0]];
                v += randVec;
                //Vector3[] newVerts = new Vector3[mesh.verticesSides.Length];
                for (int j = 0; j < thisVertSet.Count; j++)
                {
                    index = thisVertSet[j];
                    newVerts[index] = v;
                }
            }
            m.vertices = newVerts;
            return m;
        }
        //------
        public static List<List<int>> FindDuplicateVertices(Vector3[] vertices)
        {
            List<int> newSet = new List<int>();
            List<List<int>> vertSets = new List<List<int>>();
            Vector3 v;
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                v = vertices[i];

                bool alreadyInList = false;
                foreach (List<int> set in vertSets)
                {
                    foreach (int index in set)
                    {
                        if (vertices[i] == vertices[index])
                        {
                            alreadyInList = true;
                            break;
                        }
                    }
                    if (alreadyInList)
                        break;
                }
                if (alreadyInList == false)
                {
                    newSet = new List<int>();
                    newSet.Add(i);
                    for (int j = i + 1; j < vertices.Length; j++)
                    {
                        if (vertices[j] == v)
                        {
                            newSet.Add(j);
                        }
                    }
                }
                vertSets.Add(newSet);
            }
            return vertSets;
        }
        //--------------------------------
        public static float FindMinYInMesh(Mesh m)
        {

            Vector3 v;
            float minY = 10000000000;
            Vector3[] verts = m.vertices;
            for (int i = 0; i < m.vertices.Length; i++)
            {
                v = verts[i];
                if (v.y < minY)
                    minY = v.y;
            }
            return minY;
        }


        //------------------------------
        public static void RemoveAllColliders(GameObject go)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            RemoveAllColliders(ref allMeshGameObjects);
        }
        //------------------------------
        public static void RemoveAllColliders(ref List<GameObject> allMeshGameObjects)
        {
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                BoxCollider boxColl = (BoxCollider)allMeshGameObjects[i].GetComponent<BoxCollider>();
                if (boxColl != null)
                    DestroyImmediate(boxColl);
                MeshCollider meshColl = (MeshCollider)allMeshGameObjects[i].GetComponent<MeshCollider>();
                if (meshColl != null)
                    DestroyImmediate(meshColl);
                SphereCollider sphereColl = (SphereCollider)allMeshGameObjects[i].GetComponent<SphereCollider>();
                if (sphereColl != null)
                    DestroyImmediate(sphereColl);
                CapsuleCollider capsuleColl = (CapsuleCollider)allMeshGameObjects[i].GetComponent<CapsuleCollider>();
                if (capsuleColl != null)
                    DestroyImmediate(capsuleColl);
            }

        }
        //------------------------------
        public static void PrintEnabledStatusAllColliders(GameObject go)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            PrintEnabledStatusAllColliders(ref allMeshGameObjects);
        }
        //------------------------------
        public static void PrintEnabledStatusAllColliders(ref List<GameObject> allMeshGameObjects)
        {
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                BoxCollider boxColl = (BoxCollider)allMeshGameObjects[i].GetComponent<BoxCollider>();
                if (boxColl != null)
                    Debug.Log(boxColl.enabled);
                MeshCollider meshColl = (MeshCollider)allMeshGameObjects[i].GetComponent<MeshCollider>();
                if (meshColl != null)
                    Debug.Log(meshColl.enabled);
                SphereCollider sphereColl = (SphereCollider)allMeshGameObjects[i].GetComponent<SphereCollider>();
                if (sphereColl != null)
                    Debug.Log(sphereColl.enabled);
                CapsuleCollider capsuleColl = (CapsuleCollider)allMeshGameObjects[i].GetComponent<CapsuleCollider>();
                if (capsuleColl != null)
                    Debug.Log(capsuleColl.enabled);
            }
        }
        //------------------------------
        public static void SetEnabledStatusAllColliders(GameObject go, bool status)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            SetEnabledStatusAllColliders(ref allMeshGameObjects, status);
        }
        //------------------------------
        public static void SetEnabledStatusAllColliders(ref List<GameObject> allMeshGameObjects, bool status)
        {
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                SetEnabledStatusOfCollider(allMeshGameObjects[i], status);
            }
        }
        public static void SetEnabledStatusOfCollider(GameObject go, bool status)
        {
            BoxCollider boxColl = (BoxCollider)go.GetComponent<BoxCollider>();
            if (boxColl != null)
                boxColl.enabled = status;
            MeshCollider meshColl = (MeshCollider)go.GetComponent<MeshCollider>();
            if (meshColl != null)
                meshColl.enabled = status;
            SphereCollider sphereColl = (SphereCollider)go.GetComponent<SphereCollider>();
            if (sphereColl != null)
                sphereColl.enabled = status;
            CapsuleCollider capsuleColl = (CapsuleCollider)go.GetComponent<CapsuleCollider>();
            if (capsuleColl != null)
                capsuleColl.enabled = status;
        }
        //------------------------------
        public static void UpdateAllColliders(GameObject go)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            UpdateAllColliders(ref allMeshGameObjects);
        }
        //------------------------------
        public static void UpdateAllColliders(ref List<GameObject> allMeshGameObjects)
        {
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                GameObject go = allMeshGameObjects[i];
                Vector3 size = GetMeshSize(go);
                BoxCollider boxColl = (BoxCollider)go.GetComponent<BoxCollider>();
                if (boxColl != null)
                {
                    boxColl.size = size;
                    //DestroyImmediate(boxCollider);
                    //go.AddComponent<BoxCollider>();
                }
                MeshCollider meshColl = (MeshCollider)go.GetComponent<MeshCollider>();
                if (meshColl != null)
                {
                    //DestroyImmediate(meshCollider);
                    //go.AddComponent<MeshCollider>();
                }
                SphereCollider sphereColl = (SphereCollider)go.GetComponent<SphereCollider>();
                if (sphereColl != null)
                {
                    //DestroyImmediate(sphereColl);
                    go.AddComponent<SphereCollider>();
                }
                CapsuleCollider capsuleColl = (CapsuleCollider)go.GetComponent<CapsuleCollider>();
                if (capsuleColl != null)
                {
                    //DestroyImmediate(capsuleColl);
                    go.AddComponent<CapsuleCollider>();
                }
            }
        }
        public static void UpdateAllUserColliders(ref List<GameObject> allMeshGameObjects)
        {
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                GameObject go = allMeshGameObjects[i];
                Vector3 size = GetMeshSize(go);
                BoxCollider boxColl = (BoxCollider)go.GetComponent<BoxCollider>();
                if (boxColl != null)
                {
                    boxColl.size = size;
                    //DestroyImmediate(boxCollider);
                    //go.AddComponent<BoxCollider>();
                }
                MeshCollider meshColl = (MeshCollider)go.GetComponent<MeshCollider>();
                if (meshColl != null)
                {
                    go.GetComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().mesh;
                }
                SphereCollider sphereColl = (SphereCollider)go.GetComponent<SphereCollider>();
                if (sphereColl != null)
                {
                    //DestroyImmediate(sphereColl);
                    //go.AddComponent<SphereCollider>();
                }
                CapsuleCollider capsuleColl = (CapsuleCollider)go.GetComponent<CapsuleCollider>();
                if (capsuleColl != null)
                {
                    //DestroyImmediate(capsuleColl);
                    //go.AddComponent<CapsuleCollider>();
                }
            }
        }
        //------------------------------
        public static void CreateCombinedBoxCollider(GameObject go, bool removeExistingColliders = true)
        {
            Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(go, true);
            if (removeExistingColliders == true)
                RemoveAllColliders(go);
            BoxCollider boxColl = go.AddComponent<BoxCollider>();
            boxColl.center = combinedBounds.center;
            boxColl.size = combinedBounds.size;
        }

        //---------------------------------
        public static List<float> GetRelativePositionsOfAllGameObjects(GameObject inGO)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(inGO);
            List<Mesh> allMeshes = GetAllMeshesFromGameObject(inGO);
            Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(inGO, true);

            List<float> positions = new List<float>();

            GameObject thisGO = null;
            float maxLocalGOPos = -1000000;
            float maxLocalMin = -1000000;
            float globalRealMaxScaledAndPositioned = -1000000;
            for (int i = 0; i < allMeshes.Count; i++)
            {
                thisGO = allMeshGameObjects[i];
                float realLocalPosX = (thisGO.transform.position.x - inGO.transform.position.x);
                float realMaxScaledAndPositioned = realLocalPosX + (allMeshes[i].bounds.max.x * thisGO.transform.lossyScale.x);
                if (realMaxScaledAndPositioned > globalRealMaxScaledAndPositioned)
                {
                    globalRealMaxScaledAndPositioned = realMaxScaledAndPositioned;
                }
            }

            if (allMeshes.Count > 0 && allMeshes.Count == allMeshGameObjects.Count)
            {
                for (int i = 0; i < allMeshes.Count; i++)
                {
                    thisGO = allMeshGameObjects[i];
                    float realLocalPosX = (thisGO.transform.position.x - inGO.transform.position.x);
                    float maxX = allMeshes[i].bounds.max.x;
                    float maxXScaled = (maxX * thisGO.transform.lossyScale.x);
                    float maxXScaledAndPositioned = maxXScaled + realLocalPosX;
                    float offsetWhole = -(maxXScaledAndPositioned - globalRealMaxScaledAndPositioned);
                    float offset = offsetWhole / combinedBounds.size.x;
                    positions.Add(offset);
                }
            }
            else
                Debug.Log("Incorrect mesh Count in GetRelativePositionsOfAllGameObjects()");

            return positions;
        }
        //-------------------------
        public static int CountNonLODMeshes(GameObject inGO)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(inGO);
            int count = 0;
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                if (allMeshGameObjects[i].name.Contains("_LOD") == false)
                {
                    count++;
                }
            }
            return count;
        }
        //=======================================================================
        public static GameObject CreateAFBExtraFromGameObject(GameObject inGO, GameObject inRefMesh = null, Color inColor = default(Color), bool recalcNormals = false)
        {

            //--- Instantiate a copy and zero its rotations----
            GameObject thisGO = null, copyGO = GameObject.Instantiate(inGO);
            copyGO.transform.rotation = Quaternion.identity;
            //--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(copyGO);
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(copyGO);
            List<Mesh> allMeshes = GetAllMeshesFromGameObject(copyGO);
            Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
            Vector3 goScale = copyGO.transform.localScale, center, max, refSize = Vector3.one;
            Mesh newMesh = null;

            List<Mesh> newMeshes = new List<Mesh>();
            for (int i = 0; i < allMeshes.Count; i++)
            {
                newMesh = DuplicateMesh(allMeshes[i]);
                newMeshes.Add(newMesh);
            }

            //========== CreateMergedPrefabs Clones and reposition the mesh verticesSides so pos is central and at base  ============
            center = combinedBounds.center;
            max = combinedBounds.max;
            float yMove = combinedBounds.min.y;
            float scaleFactorHeight = 1.0f / combinedBounds.size.y; // change to scaling by biggest individual dimension
            if (scaleFactorHeight < 0.9f || scaleFactorHeight > 1.1f)
            {
                Debug.Log("Custom Extra was scaled by " + scaleFactorHeight * 100 + "% to fit AFWB's standardized 1m height. You can change this in Post Size: Y \n");
            }
            Vector3 scale = Vector3.zero;
            for (int i = 0; i < allMeshes.Count; i++)
            {
                thisGO = allMeshGameObjects[i];
                scale = thisGO.transform.lossyScale;
                newMesh = newMeshes[i];
                newMesh = TranslateMesh(newMesh, new Vector3(-center.x / scale.x, -yMove / scale.y, -center.z / scale.z));
                newMesh = ScaleMesh(newMesh, new Vector3(scaleFactorHeight, scaleFactorHeight, scaleFactorHeight)); // scale everything

                newMesh.RecalculateBounds();
                allMeshFilters[i].sharedMesh = newMesh;
            }
            //----scale the localPosition all objects that aren't the parent, to maintain the correct relationship------
            //--- Do it in a seperate loop as we need all gos, even if they're empty folders, as they may have transform offsets ----
            Transform[] allObjects = copyGO.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allObjects.Length; i++)
            {
                thisGO = allObjects[i].gameObject;
                if (thisGO != copyGO)
                    thisGO.transform.localPosition *= scaleFactorHeight;
            }
            //========= Update Colliders =======================
            UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
            SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
            Debug.Log("Created new user Extra:  " + copyGO.name);
            return copyGO;
        }
        //=======================================================================
        // Creates a cleaned up GameObject with any children
        public static GameObject CreateCleanUncombinedAFBPostFromGameObject(GameObject inGO, AFWB.AutoFenceCreator af, GameObject inRefMesh = null)
        {
            if (inGO == null)
                return null;
            //--- Instantiate a copy and zero its rotations----
            GameObject thisGO = null, copyGO = GameObject.Instantiate(inGO);
            copyGO.transform.rotation = Quaternion.identity;

            //--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(copyGO);
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(copyGO);
            List<Mesh> allMeshes = GetAllMeshesFromGameObject(copyGO);
            Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
            Vector3 goScale = copyGO.transform.localScale, center, max, refSize = Vector3.one;
            Mesh newMesh = null;

            List<Mesh> newMeshes = new List<Mesh>();
            for (int i = 0; i < allMeshes.Count; i++)
            {
                newMesh = DuplicateMesh(allMeshes[i]);
                newMeshes.Add(newMesh);
            }

            //=========== Should we Rotate?  =====================
            float xRot = 0, yRot = 0, zRot = 0;
            if (af.postBakeRotationMode == 1 || af.postBakeRotationMode == 0)// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
            {
                //---------- Z ---------------
                if (af.postBakeRotationMode == 1 && combinedBounds.size.x > combinedBounds.size.y * 1.99f)
                    zRot = 90;
                else if (af.postBakeRotationMode == 0)
                    zRot = af.postUserMeshBakeRotations.z;
                if (zRot != 0)// its length is along z instead of x, this is the most common error, so do it first
                {
                    for (int i = 0; i < newMeshes.Count; i++)
                    {
                        RotateMesh(newMeshes[i], new Vector3(0, 0, zRot), true);
                        allMeshFilters[i].sharedMesh = newMeshes[i];
                        af.autoRotationResults.z = af.railUserMeshBakeRotations.z = zRot;
                        newMeshes[i].RecalculateNormals();
                        if (af.postBakeRotationMode == 1)
                            Debug.Log(copyGO.name + " was Auto rotated 90 on the Z axis to suit post orientation\n");
                    }
                    combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
                }
                //---------- X ---------------
                if (af.postBakeRotationMode == 1 && combinedBounds.size.z > combinedBounds.size.y * 1.99f)
                    xRot = 90;
                else if (af.postBakeRotationMode == 0)
                    xRot = af.postUserMeshBakeRotations.x;
                if (xRot != 0)// its length is along z instead of x, this is the most common error, so do it first
                {
                    for (int i = 0; i < newMeshes.Count; i++)
                    {
                        RotateMesh(newMeshes[i], new Vector3(xRot, 0, 0), true);
                        allMeshFilters[i].sharedMesh = newMeshes[i];
                        af.autoRotationResults.z = af.railUserMeshBakeRotations.z = xRot;
                        newMeshes[i].RecalculateNormals();
                        if (af.postBakeRotationMode == 1)
                            Debug.Log(copyGO.name + " was Auto rotated 90 on the X axis to suit post orientation\n");
                    }
                    combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
                }
                //---------- Y ---------------
                if (af.postBakeRotationMode == 0) // Y is only user-rotated, never Auto
                    yRot = af.postUserMeshBakeRotations.y;
                if (yRot != 0)// its length is along z instead of x, this is the most common error, so do it first
                {
                    for (int i = 0; i < newMeshes.Count; i++)
                    {
                        RotateMesh(newMeshes[i], new Vector3(0, yRot, 0), true);
                        newMeshes[i].RecalculateNormals();
                        allMeshFilters[i].sharedMesh = newMeshes[i];
                        af.autoRotationResults.y = af.railUserMeshBakeRotations.z = yRot;
                    }
                    combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
                }
            }
            //========== CreateMergedPrefabs Clones and reposition the mesh verticesSides so pos is central and at base  ============
            center = combinedBounds.center;
            max = combinedBounds.max;
            float yMove = combinedBounds.min.y;
            Vector3 scale = Vector3.zero;
            float scaleFactorHeight = 1.0f / combinedBounds.size.y;
            if (scaleFactorHeight < 0.99f || scaleFactorHeight > 1.01f)
            {
                Debug.Log("Custom Post was scaled by " + scaleFactorHeight * 100 + "% to fit AFWB's standardized 1m height. " +
                          "You can change this in Post Size or Settings->Custom Object Scaling \n");
                if (af.autoScaleImports)
                {
                    float rescale = 1.0f / scaleFactorHeight;
                    af.postScale = new Vector3(rescale, rescale, rescale);
                    Debug.Log("The Post Size settings were changed to adjust for this.\n");
                }
            }


            for (int i = 0; i < allMeshes.Count; i++)
            {
                thisGO = allMeshGameObjects[i];
                scale = thisGO.transform.lossyScale;
                newMesh = newMeshes[i];
                newMesh = TranslateMesh(newMesh, new Vector3(-center.x / scale.x, -yMove / scale.y, -center.z / scale.z));
                newMesh = ScaleMesh(newMesh, new Vector3(scaleFactorHeight, scaleFactorHeight, scaleFactorHeight)); // scale everything

                newMesh.RecalculateBounds();
                allMeshFilters[i].sharedMesh = newMesh;
            }
            //======== scale the localPosition all objects that aren't the parent, to maintain the correct relationship =====
            //Do it in a seperate loop as we need all gos, even if they're empty folders as they may have transform offsets
            Transform[] allObjects = copyGO.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allObjects.Length; i++)
            {
                thisGO = allObjects[i].gameObject;
                if (thisGO != copyGO)
                    thisGO.transform.localPosition *= scaleFactorHeight;
            }
            //========= Remove Colliders =======================
            UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
            SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
            Debug.Log("Created new user Post:  " + copyGO.name);
            return copyGO;
        }
        //-----------------------------------------
        // Will also clone childrens meshes
        public static GameObject CreateClonedGameObjectWithDuplicateMeshes(GameObject go)
        {
            if (go == null)
                return null;
            //--- Instantiate a copy and zero its rotations----
            GameObject copyGO = GameObject.Instantiate(go); //copyGO.name = "copyGO"; //named to track during debug

            Mesh mesh, newMesh;
            copyGO.transform.rotation = Quaternion.identity;
            if (MeshUtilitiesAFWB.HasSingleMesh(go))
            {
                MeshFilter mf = copyGO.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mesh = mf.sharedMesh;
                    newMesh = DuplicateMesh(mesh, autoNameDup: false);
                    newMesh.name += "_d";
                    mf.sharedMesh = newMesh;
                }
            }
            else
            {
                foreach (Transform child in copyGO.transform)
                {
                    MeshFilter mf = child.gameObject.GetComponent<MeshFilter>();
                    if (mf != null)
                    {
                        mesh = mf.sharedMesh;
                        newMesh = DuplicateMesh(mesh, autoNameDup: false);
                        newMesh.name += "_d";
                        mf.sharedMesh = newMesh;
                    }
                }
            }

            return copyGO;
        }
        public static void AdjustImportedRailMeshes(GameObject go, AutoFenceCreator af, GameObject inRefMesh = null)
        {
            FixYRotation(go, 90);
        }
        //=======================================================================
        // Creates a cleaned up GameObject with any children
        //-- Optionally pass in a reference mesh to hint at its svSize (i.e. the mesh you will be replacing)
        // If so, we take it that we want the new user mesh to be modified to fit in witht the current fence design
        public static GameObject CreateCleanUncombinedAFBRailFromGameObject(GameObject inGO, AutoFenceCreator af, GameObject inRefMesh = null)
        {
            if (inGO == null)
                return null;

            //GameObject resetGO = MeshTransformReset.ResetMesh(inGO);
            //return resetGO;

            //--- Instantiate a copy and zero its rotations----
            GameObject thisGO = null, copyGO = GameObject.Instantiate(inGO); //copyGO.name = "copyGO"; //named to track during debug
            copyGO.transform.rotation = Quaternion.identity;
            //--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(copyGO);
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(copyGO);
            List<Mesh> allMeshes = GetAllMeshesFromGameObject(copyGO);
            if (allMeshes == null || allMeshes.Count == 0)
                return null;

            Bounds bounds, combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
            Vector3 goScale = copyGO.transform.localScale, center, max, min, refSize = Vector3.one;
            int numNonLODMeshes = CountNonLODMeshes(copyGO);
            Mesh newMesh, thisMesh;
            List<Mesh> newMeshes = new List<Mesh>();
            for (int i = 0; i < allMeshes.Count; i++)
            {
                newMesh = DuplicateMesh(allMeshes[i]);
                newMeshes.Add(newMesh);
            }
            Vector3[] origNormals = GetNormals(inGO);
            Vector4[] origTangents = GetTangents(inGO);

            //=========== Should we Rotate?  =====================
            float xRot = 0, yRot = 0, zRot = 0;
            if (af.railBakeRotationMode == 1 || af.railBakeRotationMode == 0)// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
            {
                /*if (af.railBakeRotationMode == 1 && combinedBounds.svSize.z > combinedBounds.svSize.x * 1.5f)
                    yRot = 90;
                else if (af.railBakeRotationMode == 0)
                    yRot = af.railUserMeshBakeRotations.y;
                if (yRot != 0)// its length is along z instead of x, this is the most common error, so do it first
                {
                    // The worst case scenario is when you have multiple along the z-axis. They need to be mesh-rotated and GO-rotated separately, then re-aligned 
                    Vector3 groupCentre = combinedBounds.center;
                    for (int i = 0; i < allMeshes.Count; i++) // need to loop each set seperately so that we get the correct new bounds svSize
                    {
                        RotateMesh(newMeshes[i], new Vector3(0, yRot, 0), true);
                        RecentreMeshOnAxis(newMeshes[i], "z");
                        allMeshFilters[i].sharedMesh = newMeshes[i];// put back in to the GO
                        af.autoRotationResults.y = af.railUserMeshBakeRotations.y = yRot;
                        if (af.railBakeRotationMode == 1)
                            Debug.Log(copyGO.name + " was Auto rotated " + yRot + " on the Y axis to suit wall/rail orientation (See 'XYZ') \n");

                        thisGO = allMeshGameObjects[i];
                        Vector3 realLocalPos = thisGO.transform.position - copyGO.transform.position;
                        Vector3 newLocalPos = RotatePointAroundPivot(realLocalPos, Vector3.zero, new Vector3(0, yRot, 0));

                        float xTrans = (-realLocalPos.x + newLocalPos.x);
                        float zTrans = (-realLocalPos.z + newLocalPos.z);
                        float x2 = (newMeshes[i].bounds.svSize.x / 2) * thisGO.transform.localScale.z;
                        if (yRot == 90)
                            xTrans -= x2;
                        else if (yRot == -90)
                            xTrans += x2;

                        thisGO.transform.Translate(xTrans, 0, zTrans);
                        Vector3 newLocalScale = new Vector3(thisGO.transform.localScale.z, thisGO.transform.localScale.y, thisGO.transform.localScale.x);
                        thisGO.transform.localScale = newLocalScale;

                        bounds = newMeshes[i].bounds;
                        Debug.Log(bounds);
                    }
                    combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
                }*/
                //---------- X ---------------
                /*if (af.railBakeRotationMode == 1 && combinedBounds.svSize.z > combinedBounds.svSize.y * 1.99f)
                    xRot = 90;
                else if (af.railBakeRotationMode == 0)
                    xRot = af.railUserMeshBakeRotations.x;
                if (xRot != 0)// seems to be lying on its side
                {
                    for (int i = 0; i < allMeshes.Count; i++)
                    {
                        RotateMesh(newMeshes[i], new Vector3(xRot, 0, 0), true);
                        allMeshFilters[i].sharedMesh = newMeshes[i];
                        af.autoRotationResults.x = af.railUserMeshBakeRotations.x = xRot;
                        Debug.Log(copyGO.name + " was Auto rotated " + xRot + " on the X axis to suit wall/rail orientation (See 'XYZ') \n");
                    }
                    combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
                }*/
                //---------- Z ---------------
                /*if (af.railBakeRotationMode == 1 && combinedBounds.svSize.y > combinedBounds.svSize.x * 1.99f)
                    zRot = 90;
                else if (af.railBakeRotationMode == 0)
                    zRot = af.railUserMeshBakeRotations.z;
                if (zRot != 0) // seems to be standing up on its end
                {
                    for (int i = 0; i < allMeshes.Count; i++)
                    {
                        RotateMesh(newMeshes[i], new Vector3(0, 0, zRot), true);
                        allMeshFilters[i].sharedMesh = newMeshes[i];
                        af.autoRotationResults.z = af.railUserMeshBakeRotations.z = zRot;
                        Debug.Log(copyGO.name + " was Auto rotated " + zRot + " on the Z axis to suit wall/rail orientation (See 'XYZ') \n");

                    }
                    combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
                }*/
            }

            combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
            af.userSubMeshRailOffsets = GetRelativePositionsOfAllGameObjects(copyGO);

            //========== CreateMergedPrefabs Clones and reposition the mesh verticesSides so pos is central and at base  ============
            center = combinedBounds.center;
            max = combinedBounds.max;
            min = combinedBounds.min;
            float yMove = combinedBounds.min.y;
            Vector3 scale = Vector3.zero;
            float scaleFactorX = 3.0f / combinedBounds.size.x; // Set x scaling, 3 = default rail length
                                                               //float scaleFactorZ = scaleFactorX;
            float scaleFactorZ = 1;
            float scalefactorY = (1 + scaleFactorX) / 2; // scalefactorY is just an average that gives useable height, no matter what the design
            float scaleFactorHeight = 1.0f;
            float proportionXZ = combinedBounds.size.x / combinedBounds.size.z;// restrict the thickness to 1/3 of the length as an initial default
            if (proportionXZ < 3)
            {
                scaleFactorZ = scaleFactorX * proportionXZ / 3;
            }

            for (int i = 0; i < allMeshes.Count; i++)
            {
                newMesh = newMeshes[i];
                thisGO = allMeshGameObjects[i];
                scale = thisGO.transform.lossyScale;

                float xShift = -newMesh.bounds.max.x;
                float yMeshShift = -newMesh.bounds.center.y;
                thisGO.transform.position += new Vector3(-max.x, 0, 0); // shift the transforms, so the edges are at the correct starting position
                newMesh = TranslateMesh(newMesh, new Vector3(xShift, yMeshShift, 0)); //shift the pos
                thisGO.transform.position += new Vector3(-xShift * scale.x, 0, 0); // move the transform again to compensate for the pos move
                newMesh = ScaleMesh(newMesh, new Vector3(scaleFactorX, scaleFactorHeight, scaleFactorZ)); // scale everything
                newMesh.RecalculateBounds();
                allMeshFilters[i].sharedMesh = newMesh;// put back in to the GO            
            }
            //CompareNormals(inGO.GetComponent<MeshFilter>().sharedMesh, allMeshFilters[0].sharedMesh);
            //CompareTangents(inGO.GetComponent<MeshFilter>().sharedMesh, allMeshFilters[0].sharedMesh);

            //----scale the localPosition all objects that aren't the parent, to maintain the correct relationship------
            //--- Do it in a seperate loop as we need all gos, even if they're empty folders, as they may have transform offsets ----
            Transform[] allObjects = copyGO.GetComponentsInChildren<Transform>(true); //orig
            for (int i = 0; i < allObjects.Length; i++)
            {
                thisGO = allObjects[i].gameObject;
                if (thisGO != copyGO)
                {
                    thisGO.transform.localPosition = new Vector3(thisGO.transform.localPosition.x * scaleFactorX, thisGO.transform.localPosition.y * scaleFactorHeight, thisGO.transform.localPosition.z);
                }
            }
            //========= Remove Colliders =======================
            UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
            SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
            Debug.Log("Cleaned User Rail:  " + copyGO.name);
            return copyGO;
        }
        public static void FlipNormals(Mesh mesh)
        {
            Debug.Log("Normals\n");

            Vector3[] normCopy = GetNormals(mesh);

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                normCopy[i] *= -1;
            }
            mesh.normals = normCopy;
        }
        public static void PrintNormals(Mesh mesh)
        {
            Debug.Log("Normals\n");
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Debug.Log(mesh.normals[i] + "\n");
            }
        }
        public static void PrintNormals(GameObject go)
        {
            Debug.Log("Normals\n");
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Debug.Log(mesh.normals[i] + "\n");
            }
        }
        public static void CompareNormals(Mesh mesh1, Mesh mesh2)
        {
            Debug.Log("Normals\n");

            if (mesh1.vertexCount != mesh2.vertexCount)
            {
                Debug.Log("Mismatched verex counts " + mesh1.vertexCount + "  " + mesh2.vertexCount + "\n");
                return;
            }

            for (int i = 0; i < mesh1.vertexCount; i++)
            {
                Debug.Log("  " + i + "  " + mesh1.normals[i] + "  " + mesh2.normals[i] + "\n");
                if (Vector3.Magnitude(mesh1.normals[i] - mesh2.normals[i]) > 0.000001f)
                    Debug.Log("*************************************************\n");
            }
        }
        public static void CompareTangents(Mesh mesh1, Mesh mesh2)
        {
            Debug.Log("Tangents\n");

            if (mesh1.vertexCount != mesh2.vertexCount)
            {
                Debug.Log("Mismatched verex counts " + mesh1.vertexCount + "  " + mesh2.vertexCount + "\n");
                return;
            }

            for (int i = 0; i < mesh1.vertexCount; i++)
            {
                Debug.Log("  " + i + "  " + mesh1.tangents[i] + "  " + mesh2.tangents[i] + "\n");
                if (Vector4.Magnitude(mesh1.tangents[i] - mesh2.tangents[i]) > 0.000001f)
                    Debug.Log("*************************************************\n");
            }
        }
        //-----------------------------------------
        public static GameObject CorrectMeshesXPositionForAFWB(GameObject inGO)
        {
            if (inGO == null)
                return null;
            //--- Instantiate a copy and zero its rotations----
            GameObject thisGO = null;//copyGO = GameObject.Instantiate(inGO); //copyGO.name = "copyGO"; //named to track during debug
                                     //copyGO.transform.svRotation = Quaternion.identity;
                                     //--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(inGO);
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(inGO);
            List<Mesh> allMeshes = GetAllMeshesFromGameObject(inGO);
            if (allMeshes == null || allMeshes.Count == 0)
                return null;

            Bounds bounds, combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(inGO, true);
            Vector3 center, max;
            Mesh newMesh, thisMesh;
            /*List<Mesh> newMeshes = new List<Mesh>();
            for(int i=0; i<allMeshes.Count; i++){
                newMesh = DuplicateMesh(allMeshes[i]);
                newMeshes.Add(newMesh);
            }*/


            float sizeX = combinedBounds.size.x;
            float xScaling = 3.0f / sizeX;

            //newMeshes = ScaleMeshList(newMeshes, new Vector3(xScaling, 1, 1), true);
            //Bounds

            BakeAllGOPositions(inGO);
            allMeshes = GetAllMeshesFromGameObject(inGO);
            List<Mesh> newMeshes = DuplicateMeshList(allMeshes);

            float minX = 1000000;
            float maxX = -1000000;
            int meshWithMinX = 0, meshWithMaxX = 0;
            for (int i = 0; i < allMeshes.Count; i++)
            {
                if (newMeshes[i].bounds.min.x < minX)
                {
                    minX = newMeshes[i].bounds.min.x;
                    meshWithMinX = i;
                }
                if (newMeshes[i].bounds.max.x > maxX)
                {
                    maxX = newMeshes[i].bounds.max.x;
                    meshWithMaxX = i;
                }
            }

            /*float []xOffsets = new float[allMeshes.Count];
            for (int i = 0; i < allMeshes.Count; i++)
            {
                xOffsets[i] = newMeshes[i].bounds.min.x - minX;
            }*/

            for (int i = 0; i < allMeshes.Count; i++)
            {
                newMesh = newMeshes[i];
                minX = minX * 1;
                meshWithMinX = meshWithMinX + 0;
                maxX = maxX * 1;
                meshWithMaxX = meshWithMaxX + 0;
                float xShift = -maxX;
                newMesh = TranslateMesh(newMesh, new Vector3(xShift, 0, 0));

                newMesh.RecalculateBounds();
                allMeshFilters[i].sharedMesh = newMesh;// put back in to the GO
            }

            //----scale the localPosition all objects that aren't the parent, to maintain the correct relationship------
            //--- Do it in a seperate loop as we need all gos, even if they're empty folders, as they may have transform offsets ----
            /*Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true); //orig
            for (int i=0; i<allObjects.Length; i++) {
                thisGO = allObjects[i].gameObject;
                if(thisGO != inGO){ 
                    thisGO.transform.localPosition = new Vector3(thisGO.transform.localPosition.x*scaleFactorX,  thisGO.transform.localPosition.y,  thisGO.transform.localPosition.z); 
                }
            }*/

            return inGO;
        }
        //------------------------
        // Is there more than one mesh attached to this go and its children
        public static bool HasSingleMesh(GameObject go)
        {
            bool isSingle = true;
            Transform[] children = go.transform.GetComponentsInChildren<Transform>();
            if (children.Length == 1)
                return true;
            else
            {
                int count = 0;
                foreach (Transform child in children)
                {
                    MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                    if (mf != null)
                    {
                        Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                        if (thisObjectMesh != null)
                            count++;
                    }
                    if (count > 1)
                        return false;
                }
            }
            return isSingle;
        }
        //--------------------------------
        public static bool GameObjectHasMesh(GameObject inGO)
        {
            MeshFilter mf = (MeshFilter)inGO.gameObject.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh thisObjectMesh = mf.sharedMesh;
                if (thisObjectMesh != null)
                    return true;
            }
            return false;
        }
        //-----------------------------------------
        public static void BakeAllGOPositions(GameObject go)
        {
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            //List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(go);
            //List<Mesh> allMeshes = GetAllMeshesFromGameObject(go);
            Mesh thisMesh = null;
            MeshFilter thisMeshFilter;
            GameObject thisGO = null;
            Vector3 thisRealLocalPos = Vector3.zero;

            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                thisGO = allMeshGameObjects[i];
                thisMeshFilter = thisGO.GetComponent<MeshFilter>();
                thisMesh = DuplicateMesh(thisMeshFilter.sharedMesh);

                thisRealLocalPos = (thisGO.transform.position - go.transform.position);
                TranslateMesh(thisMesh, -thisRealLocalPos, true);
                thisGO.transform.Translate(-thisRealLocalPos);
                thisMeshFilter.sharedMesh = thisMesh;
            }
        }
        //-------------------------------
        // bakes the Go's transform's rotations in to the mesh. Caller should set go svRotation to Quaternion.Identity after calling this
        public static Mesh BakeRotations(Mesh mesh, GameObject inGO)
        {
            Vector3 eulerRotations = inGO.transform.eulerAngles;
            mesh = RotateMesh(mesh, eulerRotations, true);
            return mesh;
        }
        public static void RecalculateNormalsAndTangents(Mesh mesh)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
        public static void RecalculateNormalsAndTangents(List<Mesh> meshList)
        {
            for (int i = 0; i < meshList.Count; i++)
            {
                Mesh mesh = meshList[i];
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
            }
        }
        public static void RecalculateNormalsAndTangents(GameObject go)
        {
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
        //-----------------------------------------
        public static void FixYRotation(GameObject go, float angle)
        {
            Bounds bounds, combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(go);

            float yRot = 0;

            if (combinedBounds.size.z > combinedBounds.size.x * 1.5f)
                yRot = 90;

            Mesh mesh = null;
            if (HasSingleMesh(go))
            {
                mesh = GetMeshFromGameObject(go);
                if (mesh == null)
                    return;
            }


            Vector3[] normals = GetNormals(mesh);

            PrintMeshVerts(mesh, 5, "pre");

            if (angle == 90)
            {
                RecentreMesh(go);
                RotateMesh(mesh, new Vector3(0, angle, 0), true);

                //Normals svRotation needs to be done around centre
                Vector3 centre = CalculateCentreOfMesh(mesh);

                //Rotate normals
                for (int i = 0; i < normals.Length; i++)
                {
                    Vector3 n = normals[i];
                    Vector3 dir = n - centre;
                    dir = Quaternion.Euler(new Vector3(0, angle, 0)) * dir;
                    normals[i] = dir + centre;
                    normals[i].Normalize();
                }
            }
            PrintMeshVerts(mesh, 5, "post");
            mesh.normals = normals;
            mesh.RecalculateTangents();

        }
        public static void RotateMeshAndNormals(Mesh mesh, Vector3 angles, bool recentre)
        {
            RotateMeshAndNormals(mesh, angles.x, angles.y, angles.z, recentre);
        }
        public static void RotateMeshAndNormals(Mesh mesh, float x, float y, float z, bool recentre)
        {
            if (mesh == null)
            {
                Debug.Log("Mesh was null in RotateMeshAndNormals()");
                return;
            }
            if (recentre)
                RecentreMesh(mesh);

            RotateMesh(mesh, new Vector3(x, y, z), true);

            //Normals svRotation needs to be done around centre
            //Vector3 centre = CalculateCentreOfMesh(mesh);
            Vector3 centre = mesh.bounds.center;
            Vector3[] normals = mesh.normals;
            int normLen = normals.Length;
            Quaternion q = Quaternion.Euler(new Vector3(x, y, z));
            for (int i = 0; i < normLen; i++)
            {
                Vector3 dir = normals[i] - centre;
                dir = q * dir;
                normals[i] = (dir + centre);
            }
            mesh.normals = normals;
            mesh.RecalculateTangents();
        }
        //--------------------------------
        public static void RotateX(GameObject go, float angle)
        {
            if (angle == 0)
                return;

            if (HasSingleMesh(go))
            {
                //-- Easy, only one mesh
                Vector3 size = GetMeshSize(go);
                RotateMeshAndNormals(GetMeshFromGameObject(go), angle, 0, 0, recentre: true);
                return;
            }
            List<Mesh> meshList = GetAllMeshesFromGameObject(go);
            for (int i = 0; i < meshList.Count; i++) // need to loop each set seperately so that we get the correct new bounds svSize
            {
                RotateMeshAndNormals(meshList[i], 0, angle, 0, recentre: true);
            }
        }
        //--------------------------------
        public static void RotateZ(GameObject go, float angle)
        {
            if (angle == 0)
                return;

            if (HasSingleMesh(go))
            {
                //-- Easy, only one mesh
                Vector3 size = GetMeshSize(go);
                RotateMeshAndNormals(GetMeshFromGameObject(go), 0, 0, angle, recentre: true);
                return;
            }
            List<Mesh> meshList = GetAllMeshesFromGameObject(go);
            for (int i = 0; i < meshList.Count; i++) // need to loop each set seperately so that we get the correct new bounds svSize
            {
                RotateMeshAndNormals(meshList[i], 0, 0, angle, recentre: true);
            }
        }
        //--------------------------------
        //Not as trivial as it seems as nested game objects must be rotated and then moved relative to the rotated parent
        public static void RotateY(GameObject go, float angle)
        {
            if (angle == 0)
                return;

            if (HasSingleMesh(go))
            {
                //-- Easy, only one mesh
                Vector3 size = GetMeshSize(go);
                RotateMeshAndNormals(GetMeshFromGameObject(go), 0, angle, 0, recentre: true);
                return;
            }
            List<Mesh> meshList = GetAllMeshesFromGameObject(go);
            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(go);
            //Do this with the GO rather than a mesh list as it can then include the offsets of child meshes
            //Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(go);
            // The worst case scenario is when you have multiple along the z-axis. They need to be mesh-rotated and GO-rotated separately, then re-aligned 
            //Vector3 groupCentre = combinedBounds.center;
            for (int i = 0; i < meshList.Count; i++) // need to loop each set seperately so that we get the correct new bounds svSize
            {
                //RotateMesh(meshList[i], new Vector3(0, angle, 0), true);
                RotateMeshAndNormals(meshList[i], 0, angle, 0, recentre: true);
                RecentreMeshOnAxis(meshList[i], "z");
                allMeshFilters[i].sharedMesh = meshList[i];// put back in to the GO
                GameObject thisGO = allMeshGameObjects[i];
                Vector3 realLocalPos = thisGO.transform.position - go.transform.position;
                Vector3 newLocalPos = RotatePointAroundPivot(realLocalPos, Vector3.zero, new Vector3(0, angle, 0));
                float xTrans = (-realLocalPos.x + newLocalPos.x);
                float zTrans = (-realLocalPos.z + newLocalPos.z);
                float x2 = (meshList[i].bounds.size.x / 2) * thisGO.transform.localScale.z;
                if (angle == 90)
                    xTrans -= x2;
                else if (angle == -90)
                    xTrans += x2;
                thisGO.transform.Translate(xTrans, 0, zTrans);
                Vector3 newLocalScale = new Vector3(thisGO.transform.localScale.z, thisGO.transform.localScale.y, thisGO.transform.localScale.x);
                thisGO.transform.localScale = newLocalScale;
            }
        }
        public static void BakeAllTransformRotations(GameObject rootGo, bool shiftTransformsToMatch = false, bool recentre = true)
        {
            GameObject go = null;
            GameObject tempdel = null;
            Vector3 origCubePos, newCubePos;
            List<GameObject> allMeshGameObjects = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(rootGo);

            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                go = allMeshGameObjects[i];

                List<Mesh> allMeshesForGo = GetAllMeshesFromGameObject(go);

                //GetTransformHistory(go, print: true);
                if (VectorUtilitiesTCT.GetMaxAbsVector3Element(go.transform.eulerAngles) < 0.0001f)
                {
                    //Debug.Log("No Transform Rotation Needed Shift  " + go.name + "\n");
                }
                else if (go.GetComponent<MeshFilter>() != null)
                {
                    go = allMeshGameObjects[i];
                    if (VectorUtilitiesTCT.GetMaxAbsVector3Element(go.transform.eulerAngles) < 0.0001f)
                    {
                        //Debug.Log("No Transform Rotation Needed Shift  " + go.name + "\n");
                    }
                    else if (go.GetComponent<MeshFilter>() != null)
                    {
                        Vector3 shift = BakeTransformRotations(go, recentre, Vector3.zero);
                        //Debug.Log("BakeTransformRotations Shift  " + go.name + "  " + shift + "\n");
                        if (shiftTransformsToMatch == true)
                        {
                            if (go != rootGo)
                            {
                                go.transform.Translate(new Vector3(-shift.x, -shift.y, -shift.z));
                                go.transform.eulerAngles = Vector3.zero;
                            }
                            else
                            {
                                //Debug.Log("BakeTransformRotations Shifted Root  " + go.name + "  " + shift + "\n");
                                TranslateMesh(go, shift);
                            }
                        }
                    }
                }
            }
            return;
        }
        //-------------------------------------------
        public static void BakeAllRotations(GameObject rootGo, Vector3 rotations, bool recentre = true)
        {
            // No point doing this if there are no significant rotations
            float maxRot = VectorUtilitiesTCT.GetMaxAbsVector3Element(rotations);
            if (maxRot < 0.001f)
                return;

            GameObject go = null;
            GameObject tempdel = null;
            Vector3 origCubePos, newCubePos;
            List<GameObject> allMeshGameObjects = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(rootGo);

            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                go = allMeshGameObjects[i];
                List<Mesh> allMeshesForGo = GetAllMeshesFromGameObject(go);
                for (int j = 0; j < allMeshesForGo.Count; j++)
                {
                    Mesh mesh = allMeshesForGo[j];
                    if (mesh != null)
                    {
                        RotateMeshAndNormals(mesh, rotations, recentre);
                    }
                }
            }
            return;
        }
        //------------------------------
        // Unity svRotation order: z x y
        public static Vector3 RotateMeshAndNormals(Mesh mesh, Vector3 angles, bool recentre, Vector3 pivot = default(Vector3))
        {
            Vector3 shift = RotateMeshAndNormals(mesh, angles.x, angles.y, angles.z, recentre, pivot);
            return shift;
        }
        public static Vector3 RotateMeshAndNormals(Mesh mesh, float x, float y, float z, bool recentre, Vector3 pivot = default(Vector3))
        {
            if (mesh == null)
            {
                Debug.Log("Mesh was null in RotateMeshAndNormals()");
                return Vector3.zero;
            }
            //Debug.Log(mesh.name + "  pre  " + mesh.bounds + "\n");
            Vector3 shift = Vector3.zero;
            if (recentre)
                shift = RecentreMesh(mesh);
            //Debug.Log(mesh.name + "  post  " + mesh.bounds + "\n");
            RotateMesh(mesh, new Vector3(x, y, z), recalcBounds: true, recalcNormals: false, pivot);

            //============  Rotate Normals  ==============

            //Normals svRotation needs to be done around centre
            //*** Note may need to change centre for meshCentre in some cases
            Vector3 meshCentre = CalculateCentreOfMesh(mesh);
            Vector3[] normals = GetNormals(mesh);
            //Rotate normals
            for (int i = 0; i < normals.Length; i++)
            {
                Vector3 n = normals[i];
                Vector3 dir = n - meshCentre;
                dir = Quaternion.Euler(new Vector3(x, y, z)) * dir;
                normals[i] = dir + meshCentre;
                normals[i].Normalize();
            }
            mesh.normals = normals;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return shift;
        }
        public static void RotateMeshAndNormalsMatrix(Mesh mesh, Transform trans)
        {

            RotateMeshByTransformMatrix(mesh, trans, recalcBounds: true);
        }
        public static float GetHeightOfTallestMeshGameObject(List<GameObject> allMeshGameObjects)
        {
            float max = 0;
            for (int i = 0; i < allMeshGameObjects.Count; i++)
            {
                Mesh m = GetMeshFromGameObject(allMeshGameObjects[i]);
                if (m == null)
                    continue;
                float height = m.bounds.size.y;
                if (height > max)
                    max = height;
            }
            return max;
        }
        //-------------------------
        // Replaces v3 BakeRotations()
        // Rotates the mesh according to the go's transform, then resets the transform rots to zero
        // Unity svRotation order: z x y
        public static Vector3 BakeTransformRotations(GameObject go, bool recentre, Vector3 pivot)
        {
            Vector3 rotAngles = go.transform.localEulerAngles, shift = Vector3.zero;
            float max = VectorUtilitiesTCT.GetMaxAbsVector3Element(rotAngles);
            //-- Don't bother with insignificant rotations
            if (max < .0001f)
                return Vector3.zero;

            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Vector3 combCentre = Vector3.zero;
                Vector3 goOffset = go.transform.localPosition;
                combCentre = combCentre - goOffset;
                shift = RotateMeshAndNormals(mf.sharedMesh, rotAngles, recentre, pivot);
            }
            return shift;
        }
        //--------------------------------
        // Unity svRotation order: z x y
        public static Mesh RotateMeshByTransformMatrix(Mesh m, Transform trans, bool recalcBounds)
        { // default = (0,0,0)

            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3[] verts = m.vertices;

            var thisMatrix = trans.localToWorldMatrix;

            Vector3 v;
            for (int i = 0; i < m.vertices.Length; i++)
            {
                v = verts[i];
                v = thisMatrix.MultiplyPoint3x4(v);
                newVerts[i] = v;
            }
            m.vertices = newVerts;
            if (recalcBounds)
                m.RecalculateBounds();
            return m;
        }
        public static void RecalculateNormalsSeamless(Mesh mesh)
        {
            var trianglesOriginal = mesh.triangles;
            var triangles = trianglesOriginal.ToArray();

            var vertices = mesh.vertices;

            var mergeIndices = new Dictionary<int, int>();

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertexHash = vertices[i].GetHashCode();

                if (mergeIndices.TryGetValue(vertexHash, out var index))
                {
                    for (int j = 0; j < triangles.Length; j++)
                        if (triangles[j] == i)
                            triangles[j] = index;
                }
                else
                    mergeIndices.Add(vertexHash, i);
            }

            mesh.triangles = triangles;

            var normals = new Vector3[vertices.Length];

            mesh.RecalculateNormals();
            var newNormals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
                if (mergeIndices.TryGetValue(vertices[i].GetHashCode(), out var index))
                    normals[i] = newNormals[index];

            mesh.triangles = trianglesOriginal;
            mesh.normals = normals;
        }

        //--------------------------------
        // Assumes the orientation is correct with Rail length along the X axis
        static public void SetRailPivotToLeftCentre(GameObject go)
        {
            Vector3 center, max, min, refSize = Vector3.one;
            Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(go);

            //========== CreateMergedPrefabs Clones and reposition the mesh verticesSides so pos is central and at base  ============
            center = combinedBounds.center;
            max = combinedBounds.max;
            min = combinedBounds.min;
            Vector3 scale = Vector3.zero;


            List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(go);
            List<Mesh> meshList = GetAllMeshesFromGameObject(go);
            GameObject thisGo;
            Mesh thisMesh;
            for (int i = 0; i < meshList.Count; i++)
            {
                thisMesh = meshList[i];
                thisGo = allMeshGameObjects[i];
                scale = thisGo.transform.lossyScale;

                float xShift = -thisMesh.bounds.max.x;
                float yMeshShift = -thisMesh.bounds.center.y;
                thisGo.transform.position += new Vector3(-max.x, 0, 0); // shift the transforms, so the edges are at the correct starting position
                thisMesh = TranslateMesh(thisMesh, new Vector3(xShift, yMeshShift, 0)); //shift the pos
                thisGo.transform.position += new Vector3(-xShift * scale.x, 0, 0); // move the transform again to compensate for the pos move
                thisMesh.RecalculateBounds();
                allMeshFilters[i].sharedMesh = thisMesh;// put back in to the GO            
            }
        }
        //--------------------------------
        // Assumes the orientation is correct with Rail length along the X axis
        static public Mesh SetRailMeshPivotToLeftCentre(Mesh mesh)
        {
            float xShift = -mesh.bounds.max.x;
            float yMeshShift = -mesh.bounds.center.y;
            mesh = TranslateMesh(mesh, new Vector3(xShift, yMeshShift, 0)); //shift the pos
            mesh.RecalculateBounds();
            return mesh;
        }
        //--------------------------------
        static public Mesh SetPivotToCentreBase(GameObject go, bool recalcBounds = true)
        {
            return SetPivotToCentreBase(GetFirstMeshInGameObject(go));
        }
        //--------------------------------
        static public Mesh SetPivotToCentreBase(Mesh m, bool recalcBounds = true)
        {
            Bounds bounds = m.bounds;
            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3[] verts = m.vertices;
            Vector3 v, translate = Vector3.zero;

            float centreOffsetX = (bounds.max.x + bounds.min.x) / 2;
            translate.x = -centreOffsetX;
            float baseY = bounds.min.y;
            translate.y = -baseY;
            float centreOffsetZ = (bounds.max.z + bounds.min.z) / 2;
            translate.z = -centreOffsetZ;
            int n = m.vertices.Length;
            for (int i = 0; i < n; i++)
            {
                v = verts[i];
                v += translate;
                newVerts[i] = v;
            }
            m.vertices = newVerts;

            if (recalcBounds)
                m.RecalculateBounds();

            return m;
        }
        //--------------------------------
        public static void RotateMesh(GameObject go, Vector3 angles, bool recalcBounds, bool recalcNormals = false, Vector3 centre = default(Vector3))
        {
            // get all the meshes from the game object
            List<MeshFilter> mfList = GetAllMeshFiltersFromGameObject(go);
            for (int i = 0; i < mfList.Count; i++)
            {
                Mesh mesh = mfList[i].sharedMesh;
                mesh = RotateMesh2(mesh, angles, recalcBounds, recalcNormals, centre);
                mfList[i].sharedMesh = mesh;
            }
        }

        //--------------------------------
        public static Mesh RotateMesh(Mesh mesh, Vector3 angles, bool recalcBounds, bool recalcNormals = false, Vector3 centre = default(Vector3))
        {
            if (angles == Vector3.zero)
                return mesh;

            if (centre == Vector3.zero)
                centre = mesh.bounds.center;
            //centre = mesh.bounds.center;

            Vector3[] newVerts = new Vector3[mesh.vertices.Length];
            Vector3[] verts = mesh.vertices;
            int n = mesh.vertices.Length;

            Quaternion q = Quaternion.Euler(angles);
            Vector3 v;
            for (int i = 0; i < n; i++)
            {
                v = verts[i];
                Vector3 dir = v - centre;
                dir = q * dir;
                verts[i] = dir + centre;
            }
            mesh.vertices = verts;
            if (recalcBounds)
                mesh.RecalculateBounds();


            if (recalcNormals)
            {
                Vector3[] normals = mesh.normals;
                int normLen = normals.Length;
                q = Quaternion.Euler(angles);
                for (int i = 0; i < normLen; i++)
                {
                    Vector3 dir = normals[i] - centre;
                    dir = q * dir;
                    normals[i] = (dir + centre);
                }
                mesh.normals = normals;
                mesh.RecalculateTangents();
            }
            return mesh;
        }
        /// <summary>
        /// Rotates a mesh by the given angles, with options to recalculate bounds and normals, and to specify a center point.
        /// </summary>
        public static Mesh RotateMesh2(Mesh mesh, Vector3 angles, bool recalcBounds, bool recalcNormals = false, Vector3 centre = default(Vector3))
        {
            if (angles == Vector3.zero)
                return mesh;

            // If centre is not specified, use the mesh bounds center
            if (centre == Vector3.zero)
                centre = mesh.bounds.center;

            Vector3[] verts = mesh.vertices;
            Quaternion rotation = Quaternion.Euler(angles);

            // Rotate vertices around the center point
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] -= centre;
                verts[i] = rotation * verts[i];
                verts[i] += centre;
            }
            mesh.vertices = verts;

            if (recalcBounds)
                mesh.RecalculateBounds();

            if (recalcNormals)
            {
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = rotation * normals[i];
                }
                mesh.normals = normals;
                mesh.RecalculateTangents();
            }

            return mesh;
        }

        //------------
        // the pos is perpendicular to the vector between point & pos
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pos (typically v3.up
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point;
        }
        //------------
        // Rotating in the XZ plane around the Y axis
        public static Vector3 RotatePointAroundPivotY(Vector3 point, Vector3 pivot, float angle)
        {
            Vector3 angles = new Vector3(0, angle, 0);
            Vector3 dir = point - pivot; // get point direction relative to pos (typically v3.up
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point;
        }
        // if you ask for 90, you'll get a point east, 3 oclock, 1,0,0. If you ask for -90, you'll get west, 9 oclock, -1,0,0 etc
        public static Vector3 SetPointAtCompassAngleArountPivotY(Vector3 pivot, float angle)
        {
            // Start at 12 oclock and rotate clockwise
            Vector3 startPoint = pivot + new Vector3(0, 0, 1);
            Vector3 angles = new Vector3(0, angle, 0);
            Vector3 dir = startPoint - pivot; // get point direction relative to pos (typically v3.up
            dir = Quaternion.Euler(angles) * dir; // rotate it
            Vector3 endPoint = dir + pivot; // calculate rotated point
            return endPoint;
        }
        // assuming XZ plane, do it without quaternions
        public static Vector3 SetPointAtCompassAngleAroundPivotYNoQ(Vector3 pivot, float angle)
        {

            angle *= -1; //because Unity, Geomety and AFWB are not in agreement about the Universe
            angle += 90;

            float angleRad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad);
            float z = Mathf.Sin(angleRad);

            Vector3 rotatedPoint = new Vector3(x, 0, z) + pivot;
            return rotatedPoint;
        }
        public static Vector3 RotatePointAroundPivotYFromLocalForwardNoQ(Vector3 localForward, Vector3 pivot, float angle, float length = 1.0f)
        {
            localForward = localForward - pivot;
            Vector3 localForwardNormalized = localForward.normalized;
            //float startAngleOfLocalForward = Vector3.Angle(Vector3.forward, localForward); //relative to north forward
            float startAngleOfLocalForward = Vector3.SignedAngle(Vector3.forward, localForwardNormalized, Vector3.up);

            angle *= -1; //because Unity, Geomety and AFWB are not in agreement about the Universe
            angle += 90;
            angle -= startAngleOfLocalForward;

            float angleRad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad);
            float z = Mathf.Sin(angleRad);

            Vector3 rotatedPoint = new Vector3(x, 0, z) * length;
            rotatedPoint += pivot;
            return rotatedPoint;

            /*float startAngleOfLocalForward = Vector3.Angle(Vector3.forward, localForward); //relative to north forward

            angle *= -1; //because Unity, Geomety and AFWB are not in agreement about the Universe
            angle += 90;
            angle -= startAngleOfLocalForward;

            float angleRad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad);
            float z = Mathf.Sin(angleRad);

            Vector3 rotatedPoint = new Vector3(x, 0, z) * length;
            rotatedPoint += pos;
            return rotatedPoint;*/
        }
        //-- Faster for axis = up 
        public static Vector3 RotatePointAroundUpVector(Vector3 point, Vector3 angles)
        {
            Vector3 dir = point - Vector3.up; // get point direction relative to pos (typically v3.up
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + Vector3.up; // calculate rotated point
            return point;
        }
        // Use with care for simple xz groundplane rotations only
        public static Vector3 RotateVectorAroundUpAxis(Vector3 vec, float angle)
        {
            Vector3 dir = vec - Vector3.up; // get vec direction relative to up
            dir = Quaternion.Euler(new Vector3(0, angle, 0)) * dir; // rotate it
            vec = dir + Vector3.up; // calculate rotated vec
            return dir;
        }

        //-----
        public static Mesh RecentreMeshOnAxis(Mesh m, string axis, bool recalcBounds = true)
        {
            Bounds bounds = m.bounds;
            Vector3[] newVerts = new Vector3[m.vertices.Length];
            Vector3[] verts = m.vertices;
            Vector3 v, translate = Vector3.zero;
            if (axis == "z")
            {
                float centreOffset = (bounds.max.z + bounds.min.z) / 2;
                translate.z = -centreOffset;
            }
            int n = m.vertices.Length;
            for (int i = 0; i < n; i++)
            {
                v = verts[i];
                v += translate;
                newVerts[i] = v;
            }
            m.vertices = newVerts;

            if (recalcBounds)
                m.RecalculateBounds();
            return m;
        }
        //-----
        public static Mesh RecentreMesh(GameObject go, bool recalcBounds = true)
        {
            RecentreMesh(go.gameObject.GetComponent<MeshFilter>().sharedMesh);
            //return mesh;
            return go.gameObject.GetComponent<MeshFilter>().sharedMesh;
        }
        //--- Moves verticesSides to centre them on the pos
        //-- return the translation amount so that the GO transform can be repositioned so it appears in the same place if necessary
        public static Vector3 RecentreMesh(Mesh m, bool recalcBounds = true)
        {
            Bounds bounds = m.bounds;
            Vector3 translate = -bounds.center;
            Vector3[] vertices = m.vertices;

            Span<Vector3> verticesSpan = vertices.AsSpan();

            for (int i = 0; i < verticesSpan.Length; i++)
            {
                verticesSpan[i] += translate;
            }
            m.vertices = verticesSpan.ToArray();

            if (recalcBounds)
                m.RecalculateBounds();

            return translate;
        }
        //------------------------------
        public static Mesh SetPivotAtBase(GameObject go, bool recalcBounds = true)
        {
            Mesh m = SetPivotAtBase(go.gameObject.GetComponent<MeshFilter>().sharedMesh);
            return m;
        }
        public static Mesh SetPivotAtBase(Mesh m, bool recalcBounds = true)
        {
            Bounds bounds = m.bounds;
            float baseY = bounds.min.y;
            Vector3 translate = new Vector3(0, -baseY, 0);

            Span<Vector3> vertsSpan = new Span<Vector3>(m.vertices);
            for (int i = 0; i < vertsSpan.Length; i++)
            {
                vertsSpan[i] += translate;
            }
            m.vertices = vertsSpan.ToArray();
            if (recalcBounds)
                m.RecalculateBounds();
            return m;
        }

        //------------------------------
        public static Mesh SetPivotAtCenter(GameObject go, bool recalcBounds = true)
        {
            Mesh m = SetPivotAtCenter(go.gameObject.GetComponent<MeshFilter>().sharedMesh);
            return m;
        }
        public static Mesh SetPivotAtCenter(Mesh m, bool recalcBounds = true)
        {
            Bounds bounds = m.bounds;
            Vector3 translate = new Vector3(0, bounds.center.y, 0);

            Span<Vector3> vertsSpan = new Span<Vector3>(m.vertices);
            for (int i = 0; i < vertsSpan.Length; i++)
            {
                vertsSpan[i] += translate;
            }
            m.vertices = vertsSpan.ToArray();
            if (recalcBounds)
                m.RecalculateBounds();
            return m;


            /*Bounds bounds = mesh.bounds;
            Vector3[] newVerts = new Vector3[mesh.verticesSides.Length];
            Vector3[] verts = mesh.verticesSides;
            Vector3 v, translate = Vector3.zero;

            float centerY = bounds.center.y;
            translate.y = centerY;

            int n = mesh.verticesSides.Length;
            for (int i = 0; i < n; i++)
            {
                v = verts[i];
                v += translate;
                newVerts[i] = v;
            }
            mesh.verticesSides = newVerts;

            if (recalcBounds)
                mesh.RecalculateBounds();

            return mesh;*/
        }
        // Calculate the intended pivot point of the mesh relative to its center
        public static Vector3 GetPivotBasedOnBounds(Mesh mesh)
        {
            Bounds bounds = mesh.bounds;
            Vector3 size = bounds.size;

            // Calculate the inferred pivot point for each axis
            // Calculate the intended pivot point for each axis relative to the center
            float pivotX = bounds.min.x + (size.x / 2) - bounds.center.x;
            float pivotY = bounds.min.y + (size.y / 2) - bounds.center.y;
            float pivotZ = bounds.min.z + (size.z / 2) - bounds.center.z;

            return new Vector3(pivotX, pivotY, pivotZ);
        }
        //------------------------------

        public static Vector3 GetOffsetToMeshCentre(Mesh m)
        {
            Bounds bounds = m.bounds;
            float centreOffsetX = (bounds.max.x + bounds.min.x) / 2;
            float centreOffsetY = (bounds.max.y + bounds.min.y) / 2;
            float centreOffsetZ = (bounds.max.z + bounds.min.z) / 2;

            Vector3 offset = new Vector3(centreOffsetX, centreOffsetY, centreOffsetZ);
            return offset;
        }
        public static Vector3 GetOffsetToMeshCentre(GameObject go)
        {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Bounds bounds = m.bounds;
            float centreOffsetX = (bounds.max.x + bounds.min.x) / 2;
            float centreOffsetY = (bounds.max.y + bounds.min.y) / 2;
            float centreOffsetZ = (bounds.max.z + bounds.min.z) / 2;

            Vector3 offset = new Vector3(centreOffsetX, centreOffsetY, centreOffsetZ);
            return offset;
        }
        //------------------------------
        public static void PrintMeshVerts(GameObject go, string label = "")
        {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            if (label != "")
                Debug.Log(label + "\n");
            Vector3[] verts = m.vertices;
            int n = m.vertexCount;
            for (int v = 0; v < n; v++)
            {
                Debug.Log(verts[v] + "\n");
            }
        }
        //------------------------------
        public static void PrintMeshVerts(Mesh m, int num = 0, string label = "")
        {
            if (label != "")
                Debug.Log(label + "\n");
            Vector3[] verts = m.vertices;
            int n = m.vertexCount;
            if (num != 0 && num > m.vertexCount == false)
                n = num;

            for (int v = 0; v < n; v++)
            {
                Debug.Log(verts[v] + "\n");
            }
        }
        //------------------------------
        public static Mesh ReverseNormals(Mesh mesh)
        {
            mesh.triangles = mesh.triangles.Reverse().ToArray();
            return mesh;
        }
        //------------------------------
        public static Mesh ReverseNormals(GameObject go, bool recalculateNormalsAndTangents = false)
        {
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
            if (recalculateNormalsAndTangents)
            {
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

            }
            return mesh;
        }
        //----------------------------------
        // return false if there's no mesh on the top level go
        public bool CheckGameObjectMeshValid(GameObject go)
        {
            MeshFilter mf = (MeshFilter)go.GetComponent<MeshFilter>();
            if (mf == null)
                return false;
            else if (mf.sharedMesh == null)
                return false;
            return true;
        }
        //----------------------------------
        // return false if there's no mesh on the go or any children
        public bool CheckGameObjectGroupMeshValid(GameObject go)
        {
            List<Mesh> allmeshes = GetAllMeshesFromGameObject(go);
            if (allmeshes.Count == 0)
                return false;
            return true;
        }


        //--------------------------------
        public static Mesh GetFirstMeshInGameObject(GameObject inGO, LayerSet layer = LayerSet.allLayerSet)
        {
            Mesh firstMesh = null;
            if (inGO == null)
            {
                string msg = "GO is null in GetFirstMeshInGameObject() \n";
                if (layer != LayerSet.allLayerSet)
                    msg = $"GO is null in GetFirstMeshInGameObject() for layer {layer} \n";
                Debug.LogWarning(msg);
                return null;
            }

            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allObjects)
            {
                MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                if (mf != null)
                {
                    firstMesh = (Mesh)mf.sharedMesh;
                    if (firstMesh != null)
                        return firstMesh;
                }
            }
            return firstMesh;
        }
        //--------------------------------
        //-- Note: If a user- model imported in to unity has child meshes, the main mesh will be a null parent
        //-- and the actual meshes will be children of the parent. So if mesh == null, always check the children with  GetFirstMeshInGameObject()
        public static Mesh GetMeshFromGameObject(GameObject go)
        {
            if (go == null)
            {
                Debug.Log("go is null in GetMeshFromGameObject() \n");
                return null;
            }
            Mesh mesh = null;
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null)
            {
                mesh = mf.sharedMesh;
                if (mesh == null)
                    Debug.Log("Mesh was null in GetBoundsOfGameObject() " + go.name + "\n");
                else
                    return mesh;
            }
            else
                mesh = GetFirstMeshInGameObject(go);

            return mesh;
        }
        //--------------------------------
        public static string GetMeshNameFromGameObject(GameObject go)
        {
            Mesh mesh = GetMeshFromGameObject(go);
            if (mesh != null)
                return mesh.name;
            return "";
        }
        //--------------------------------
        // Gets main mesh + all child meshes of go+children
        public static List<Mesh> GetAllMeshesFromGameObject(GameObject go)
        {
            List<Mesh> meshes = new List<Mesh>();
            if (go == null)
            {
                Debug.LogWarning("GO is null in GetAllMeshesFromGameObject()");
                return meshes;
            }
            Transform[] allObjects = go.GetComponentsInChildren<Transform>(true);
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
        //--------------------------------
        // Gets main MeshFilter + all child MeshFilters of go+children
        public static List<MeshFilter> GetAllMeshFiltersFromGameObject(GameObject go)
        {
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            if (go == null)
            {
                Debug.LogWarning("GO is null in GetAllMeshFiltersFromGameObject()");
                return meshFilters;
            }
            Transform[] allObjects = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allObjects)
            {
                MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                if (mf != null)
                {
                    meshFilters.Add(mf);
                }
            }
            return meshFilters;
        }
        //--------------------------
        public static int CountAllTrianglesInGameObject(GameObject go)
        {
            if (go == null)
                return 0;
            List<MeshFilter> mfList = MeshUtilitiesAFWB.GetAllMeshFiltersFromGameObject(go);
            if (mfList.Count == 0)
                return 0;

            int triCount = 0;
            int meshCount = mfList.Count;
            for (int m = 0; m < meshCount; m++)
            {
                MeshFilter mf = mfList[m];
                if (mf == null)
                {
                    continue;
                }
                Mesh mesh = mf.sharedMesh;
                if (mesh == null)
                    continue;
                triCount += mesh.triangles.Length;
            }
            return triCount / 3;
        }
        //--------------------------------
        /*public static List<MeshFilter> GetAllMeshFiltersFromGameObject(GameObject inGO)
        {
            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            foreach (Transform child in allObjects)
            {
                MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                if (mf != null)
                {
                    meshFilters.Add(mf);
                }
            }
            return meshFilters;
        }*/
        //--------------------------------
        // Use when you want to safely deform the mesh of oan object without it affecting the same mesh on any other object
        public static bool ReplaceAllMeshesInGameObjectWithUniqueDuplicates(GameObject inGO/*, string appendName = ""*/)
        {
            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allObjects.Length; i++)
            {
                MeshFilter mf = (MeshFilter)allObjects[i].gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                if (mf != null)
                {
                    Mesh newMesh = DuplicateMesh(mf.sharedMesh);
                    mf.sharedMesh = newMesh;
                }
                else
                    return false;
            }
            return true;
        }
        //--------------------------------
        public static bool ReplaceAllMeshesInGameObject(GameObject inGO, List<Mesh> replacementMeshSet)
        {
            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);

            if (replacementMeshSet.Count < allObjects.Length)
                Debug.Log("Warning: Mismatched count in ReplaceAllMeshesInGameObject() : "
                + allObjects.Length + "GameObjects,  " + replacementMeshSet.Count + "replacement meshes");

            if (replacementMeshSet.Count >= allObjects.Length)
            {
                for (int i = 0; i < allObjects.Length; i++)
                {
                    MeshFilter mf = (MeshFilter)allObjects[i].gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                    if (mf != null)
                    {
                        Mesh newMesh = DuplicateMesh(mf.sharedMesh);
                        mf.sharedMesh = replacementMeshSet[i];
                    }
                    else
                        return false;
                }
            }
            return true;
        }
        //--------------------------------
        int FindLargestMeshInGameObject(GameObject inGO)
        {

            float maxDimension = 0;
            int bestIndex = 0;

            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
            int numObjects = allObjects.Length;
            for (int i = 0; i < numObjects; i++)
            {
                Transform child = allObjects[i];
                Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                int numSubMeshes = thisObjectMesh.subMeshCount;
                Vector3 size = thisObjectMesh.bounds.size;
                if (size.x > maxDimension)
                {
                    maxDimension = size.x;
                    bestIndex = 1;
                }
            }
            return bestIndex;
        }

        //--------------------------------
        // For Single gameobjects, use GetCombinedBoundsOfAllMeshesInGameObject() for multi
        public static Bounds GetBoundsOfGameObject(GameObject go)
        {
            Bounds bounds = new Bounds();
            if (go.GetComponent<MeshFilter>() != null)
            {
                Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                {
                    return mesh.bounds;
                }
                else
                    Debug.Log("Mesh was null in GetBoundsOfGameObject() " + go.name + "\n");
            }
            else
                Debug.Log("MeshFilter was null in GetBoundsOfGameObject() " + go.name + "\n");
            return bounds;
        }
        //--------------------------------
        public static float GetBiggestDimensionInMesh(GameObject go)
        {
            Vector3 size = GetCombinedSizeOfAllMeshesInGameObject(go);
            float biggest = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
            return biggest;
        }
        //--------------------------------
        //Finds the BIGGEST dimension and the ensures that this is at least the input svSize
        public static void ScaleMeshToMinimum(GameObject go, float min)
        {
            Vector3 size = GetCombinedSizeOfAllMeshesInGameObject(go);
            float biggest = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
            if (biggest < min)
            {
                float scale = min / biggest;
                ScaleMesh(go, new Vector3(scale, scale, scale));
            }
        }
        //--------------------------------
        //Finds the SMALLEST dimension and the ensures that this is at least the input svSize
        public static void AssertMinimumDimension(GameObject go, float min)
        {
            Vector3 size = GetCombinedSizeOfAllMeshesInGameObject(go);
            float smallest = Mathf.Min(Mathf.Min(size.x, size.y), size.z);
            if (min == 0)
                return;

            float x = 1, y = 1, z = 1;
            if (size.x < min)
                x = min / size.x;
            if (size.y < min)
                y = min / size.y;
            if (size.z < min)
                z = min / size.z;

            if (x != 1 || y != 1 || z != 1)
                ScaleMesh(go, new Vector3(x, y, z));
        }
        //--------------------------------
        //Finds the biggest dimension and the ensures that this is no bigger than input svSize
        public static void ScaleMeshToMaximum(GameObject go, float max)
        {
            Vector3 size = GetCombinedSizeOfAllMeshesInGameObject(go);
            float biggest = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
            if (biggest > max)
            {
                float scale = max / biggest;
                ScaleMesh(go, new Vector3(scale, scale, scale));
            }
        }
        //--------------------------------
        //Finds the biggest dimension and the ensures that this is no bigger/samller than the min max
        public static void ScaleMeshToMinMax(GameObject go, float min, float max)
        {
            Vector3 size = GetCombinedSizeOfAllMeshesInGameObject(go);
            float biggest = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
            if (biggest < min)
            {
                float scale = min / biggest;
                ScaleMesh(go, new Vector3(scale, scale, scale));
            }
            if (biggest > max)
            {
                float scale = max / biggest;
                ScaleMesh(go, new Vector3(scale, scale, scale));
            }
        }
        //--------------------------------
        /*public static Vector3 GetCombinedSizeOfAllMeshesInGameObject(GameObject go, bool compensateForGOScaling = false)
        {
            return GetCombinedBoundsOfAllMeshesInGameObject(go, compensateForGOScaling).size;
        }*/
        //--------------------------------
        // Meshes in Unity do not contain children, instead, when added to the hierarchy (or made into a prefab)
        // the fbx file acts as a folder or model container, which becomes an empty parent with the meshes as children
        // Likewise, a nested mesh hierarchy in an fbx will be translated as a nested hierarchy of GameObjects, all under ann empty parent
        // Therefor you can't look for children of a Mesh, you have to look for children of the GameObject
        /*public static Bounds GetCombinedBoundsOfAllMeshesInGameObject(GameObject inGO, bool compensateForGOScaling = false)
        {
            Vector3 size = Vector3.zero, min = Vector3.zero, max = Vector3.zero;
            float minX = 10000000, maxX = -10000000;
            float minY = 10000000, maxY = -10000000;
            float minZ = 10000000, maxZ = -10000000;

            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);

            List<Mesh> meshList = GetAllMeshesFromGameObject(inGO);

            int numObjects = allObjects.Length, numValidMeshes = 0;
            for (int i = 0; i < numObjects; i++)
            {
                Transform child = allObjects[i];
                MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                if (mf != null)
                {
                    Vector3 goScaling = child.lossyScale;
                    Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    if (thisObjectMesh != null)
                    {
                        numValidMeshes++;
                        min = thisObjectMesh.bounds.min;
                        max = thisObjectMesh.bounds.max;

                        if (compensateForGOScaling == true)
                        {
                            min = Vector3.Scale(min, goScaling);
                            max = Vector3.Scale(max, goScaling);
                        }
                        Vector3 realLocalPos = child.position - inGO.transform.position;
                        min += realLocalPos;
                        max += realLocalPos;

                        if (min.x < minX)
                            minX = min.x;
                        if (max.x > maxX)
                            maxX = max.x;

                        if (min.y < minY)
                            minY = min.y;
                        if (max.y > maxY)
                            maxY = max.y;

                        if (min.z < minZ)
                            minZ = min.z;
                        if (max.z > maxZ)
                            maxZ = max.z;
                    }
                }
            }
            Vector3 combinedMin = new Vector3(minX, minY, minZ);
            Vector3 combinedMax = new Vector3(maxX, maxY, maxZ);
            Vector3 combinedSize = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Vector3 combinedExtents = new Vector3(combinedSize.x / 2, combinedSize.y / 2, combinedSize.z / 2);
            Vector3 combinedCenter = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

            Bounds bounds = new Bounds();
            if (numValidMeshes > 0)
            {
                bounds.min = combinedMin;
                bounds.max = combinedMax;
                bounds.size = combinedSize;
                bounds.extents = combinedExtents;
                bounds.center = combinedCenter;
            }
            return bounds;
        }*/
        // Meshes in Unity do not contain children, instead, when added to the hierarchy (or made into a prefab)
        // the fbx file acts as a folder or model container, which becomes an empty parent with the meshes as children
        // Likewise, a nested mesh hierarchy in an fbx will be translated as a nested hierarchy of GameObjects, all under ann empty parent
        // Therefor you can't look for children of a Mesh, you have to look for children of the GameObject
        public static Bounds GetCombinedBoundsOfAllMeshesInGameObject(GameObject go, bool considerScale = true)
        {
            // Initialize an empty Bounds object with an initial point
            Bounds combinedBounds = new Bounds(go.transform.position, Vector3.zero);

            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh == null) continue;

                // Transform the local mesh bounds to world space considering scale
                Bounds transformedBounds = TransformBounds(meshFilter.sharedMesh.bounds, meshFilter.transform, considerScale);

                // Encapsulate the transformed bounds into the combined bounds
                combinedBounds.Encapsulate(transformedBounds);
            }

            // Return the size of the combined bounds as a Vector3
            return combinedBounds;
        }

        public static Bounds TransformBounds(Bounds localBounds, Transform transform, bool considerScale)
        {
            var center = transform.TransformPoint(localBounds.center);

            // Transform the local extents into world space (scale only if considerScale is true)
            var extents = localBounds.extents;
            var axisX = transform.TransformVector(extents.x, 0, 0) * (considerScale ? transform.lossyScale.x : 1);
            var axisY = transform.TransformVector(0, extents.y, 0) * (considerScale ? transform.lossyScale.y : 1);
            var axisZ = transform.TransformVector(0, 0, extents.z) * (considerScale ? transform.lossyScale.z : 1);

            // Calculate full size based on the magnitudes of the scaled axes
            Vector3 fullSize = new Vector3(axisX.magnitude, axisY.magnitude, axisZ.magnitude) * 2; // Explicitly show 'twice'
            Bounds worldBounds = new Bounds(center, fullSize);

            return worldBounds;
        }
        public static Vector3 GetCombinedSizeOfAllMeshesInGameObject(GameObject go, bool considerScale = true)
        {
            Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(go, considerScale);
            return combinedBounds.size;
        }

        //--------------------------------
        // Meshes in Unity do not contain children, instead, when added to the hierarchy (or made into a prefab)
        // the fbx file acts as a folder or model container, which becomes an empty parent with the meshes as children
        // Gets the bounds of all the meshes of parent mesh and children
        // Note: This is INCOMPATIBLE with GetCombinedBoundsOfAllMeshesInGameObject as it doesn't compensate for GO scaling and positioning
        /*public static Bounds GetCombinedBoundsOfAllMeshesInMesh(Mesh mesh)
        {
            Vector3 size = Vector3.zero, min = Vector3.zero, max = Vector3.zero;
            float minX = 10000000, maxX = -10000000;
            float minY = 10000000, maxY = -10000000;
            float minZ = 10000000, maxZ = -10000000;

            int numObjects = meshList.Count, numValidMeshes = 0;
            for (int i = 0; i < numObjects; i++)
            {
                Mesh thisMesh = meshList[i];
                if (thisMesh != null)
                {
                    numValidMeshes++;
                    min = thisMesh.bounds.min;
                    max = thisMesh.bounds.max;

                    if (min.x < minX)
                        minX = min.x;
                    if (max.x > maxX)
                        maxX = max.x;

                    if (min.y < minY)
                        minY = min.y;
                    if (max.y > maxY)
                        maxY = max.y;

                    if (min.z < minZ)
                        minZ = min.z;
                    if (max.z > maxZ)
                        maxZ = max.z;
                }
            }
            Vector3 combinedMin = new Vector3(minX, minY, minZ);
            Vector3 combinedMax = new Vector3(maxX, maxY, maxZ);
            Vector3 combinedSize = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Vector3 combinedExtents = new Vector3(combinedSize.x / 2, combinedSize.y / 2, combinedSize.z / 2);
            Vector3 combinedCenter = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

            Bounds bounds = new Bounds();
            if (numValidMeshes > 0)
            {
                bounds.min = combinedMin;
                bounds.max = combinedMax;
                bounds.size = combinedSize;
                bounds.extents = combinedExtents;
                bounds.center = combinedCenter;
            }
            return bounds;
        }*/
        //--------------------------------
        public static List<Vector3> GetAllLocalPositionsFromGameObject(GameObject inGO)
        {

            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
            List<Vector3> allLocalPositions = new List<Vector3>();
            foreach (Transform child in allObjects)
            {
                MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                if (mf != null)
                {

                    //-- we need to simplify the localPositions to remove any weird apenting offsets
                    Vector3 realLocalPos = child.position - inGO.transform.position;
                    allLocalPositions.Add(realLocalPos);

                    //allLocalPositions.Add(child.localPosition);
                }
            }
            return allLocalPositions;
        }
        //----------
        public static List<GameObject> GetAllMeshGameObjectsFromGameObject(GameObject inGO)
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
        //------------------------
        public static void ScaleAllLocalPositions(GameObject inGO, Vector3 scale)
        {
            Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
            List<Vector3> allLocalPositions = new List<Vector3>();
            foreach (Transform child in allObjects)
            {

                Vector3 realLocalPos = child.position - inGO.transform.position;
                allLocalPositions.Add(realLocalPos);
                child.localPosition = Vector3.Scale(child.localPosition, scale);
            }
        }
        //-----------------------
        public static void BakeAllRotations(GameObject inGO)
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
                    {
                        thisObjectMesh = RotateMesh(thisObjectMesh, new Vector3(0, -90, 0), true);
                        mf.sharedMesh = thisObjectMesh;
                    }
                }
            }
        }
        //---------------------------

        //--------------------------------
        public static Vector3 CalculateCentreOfMesh(Mesh m)
        {
            Vector3 center = m.bounds.center;
            return center;

            /*Vector3 v, sum = Vector3.zero,  avg = Vector3.zero;
             for(int i=0; i<mesh.verticesSides.Length; i++){
                 v = mesh.verticesSides[i];
                 sum += v;
             }
             avg = sum/mesh.verticesSides.Length;
             return avg;*/
        }
        //--------------------------------
        public static GameObject CreateGameObjectFromMesh(Mesh goMesh)
        {

            GameObject go = new GameObject();
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = goMesh;
            go.AddComponent<MeshRenderer>();
            go.AddComponent<BoxCollider>();
            go.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
            go.GetComponent<Renderer>().sharedMaterial.color = Color.white;
            return go;
        }
        //--------------------------------
        // Replaces the sharedMesh in a MeshFilter with a duplicate of any mesh, so you can later modify it without affecting the other shared meshes
        // If sourceMesh == null, uses the mesh of the MeshFilter   
        public static Mesh ReplaceSharedMeshWithDuplicateOfMesh(MeshFilter meshFilter, Mesh sourceMesh, string name = "", bool autoNameDup = true)
        {
            if (sourceMesh == null)
                sourceMesh = meshFilter.sharedMesh;
            if (name == "")
                name = sourceMesh.name;
            Mesh clonedMesh = MeshUtilitiesAFWB.DuplicateMesh(sourceMesh, name, autoNameDup);
            meshFilter.sharedMesh = clonedMesh;
            return clonedMesh;
        }
        //--------------------------------
        public static Mesh DuplicateMesh(GameObject go, string name = "", bool autoNameDup = true)
        {
            Mesh mesh = GetMeshFromGameObject(go);
            Mesh dupMesh = DuplicateMesh(mesh);
            return dupMesh;
        }
        //--------------------------------
        public static Mesh DuplicateMesh(Mesh sourceMesh, string name = "", bool autoNameDup = true)
        {
            if (sourceMesh == null)
            {
                Debug.LogWarning("sourceMesh was null in DuplicateMesh()");
                return null;
            }

            int numVerts = sourceMesh.vertexCount;
            List<Vector3> normals = new List<Vector3>(numVerts);
            List<Vector4> tangents = new List<Vector4>(numVerts);

            sourceMesh.GetNormals(normals);
            sourceMesh.GetTangents(tangents);

            Mesh newMesh = new Mesh();
            newMesh = Instantiate(sourceMesh);
            if (name == "" && autoNameDup)
                newMesh.name = sourceMesh.name + "[Dup]";
            else
                newMesh.name = name;

            newMesh.normals = normals.ToArray();
            newMesh.tangents = tangents.ToArray();

            return newMesh;
        }
        //--------------------------------
        //Gets a copy of the source mesh's normals
        public static Vector3[] GetNormals(Mesh mesh)
        {
            int numVerts = mesh.vertexCount;
            List<Vector3> normals = new List<Vector3>(numVerts);
            mesh.GetNormals(normals);

            return normals.ToArray();
        }
        public static Vector3[] GetNormals(GameObject go)
        {
            return GetNormals(go.GetComponent<MeshFilter>().sharedMesh);
        }
        //--------------------------------
        public static Vector4[] GetTangents(Mesh mesh)
        {
            int numVerts = mesh.vertexCount;
            List<Vector4> tangents = new List<Vector4>(numVerts);
            mesh.GetTangents(tangents);

            return tangents.ToArray();
        }
        public static Vector4[] GetTangents(GameObject go)
        {
            return GetTangents(go.GetComponent<MeshFilter>().sharedMesh);
        }
        //--------------------------------
        public static List<Mesh> DuplicateMeshList(List<Mesh> sourceMeshList)
        {

            List<Mesh> newMeshList = new List<Mesh>();

            for (int i = 0; i < sourceMeshList.Count; i++)
            {
                Mesh mesh = sourceMeshList[i];
                if (mesh != null)
                {
                    Mesh dupMesh = DuplicateMesh(mesh, mesh.name);
                    newMeshList.Add(dupMesh);
                }
            }
            return newMeshList;
        }
        //--------------------------------
        public static GameObject DuplicateGameObjectHierarchyUniqueMeshAndMaterial(GameObject inGO, bool recalcNormals = false)
        {

            GameObject deepCopy = GameObject.Instantiate(inGO);
            List<GameObject> allGameObjects = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(inGO);
            for (int i = 0; i < allGameObjects.Count; i++)
            {
                GameObject go = allGameObjects[i];
                ReplaceMeshWithDuplicate(go, recalcNormals);
                ReplaceMaterialWithDuplicate(go);
            }

            return deepCopy;
        }
        //--------------------------------
        public static GameObject DuplicateGameObjectUniqueMeshAndMaterial(GameObject inGO, Color inColor = default(Color), bool recalcNormals = false)
        {

            GameObject newGO = new GameObject(inGO.name + "_duplicate");

            Mesh srcMesh = inGO.GetComponent<MeshFilter>().sharedMesh;
            Mesh newMesh = DuplicateMesh(srcMesh);
            newMesh.name = inGO.name + "_dupMesh";

            if (recalcNormals)
                newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
            ;

            newGO.AddComponent<MeshFilter>();
            newGO.GetComponent<MeshFilter>().mesh = newMesh;

            newGO.AddComponent<MeshRenderer>();
            newGO.GetComponent<Renderer>().sharedMaterial = inGO.GetComponent<Renderer>().sharedMaterial;

            return newGO;
        }
        //--------------------------------
        public static GameObject ReplaceMeshWithDuplicate(GameObject inGO, bool recalcNormals = false, string suffix = "_dup")
        {
            if (inGO.GetComponent<MeshFilter>() == null || inGO.GetComponent<MeshFilter>().sharedMesh == null)
            {
                Debug.Log(inGO.name + "  No mesh to duplicate in RelaceMeshWithDuplicate \n");
                return inGO;
            }
            Mesh srcMesh = inGO.GetComponent<MeshFilter>().sharedMesh;
            Mesh newMesh = DuplicateMesh(srcMesh);
            newMesh.name = inGO.name + suffix;

            if (recalcNormals)
                newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            inGO.GetComponent<MeshFilter>().mesh = newMesh;
            return inGO;
        }
        //--------------------------------
        public static GameObject ReplaceMaterialWithDuplicate(GameObject inGO, string suffix = "_dup")
        {
            if (inGO.GetComponent<MeshRenderer>() == null || inGO.GetComponent<MeshRenderer>().sharedMaterial == null)
            {
                Debug.Log(inGO.name + "No Renderer or Material to duplicate in ReplaceMaterialWithDuplicate() \n");
                return inGO;
            }
            inGO.GetComponent<MeshRenderer>().sharedMaterial = new Material(inGO.GetComponent<MeshRenderer>().sharedMaterial);
            return inGO;
        }
        //--------------------------------
        public static GameObject CopyGameObjectWithDuplicateMesh(GameObject inGO, bool recalcNormals = false)
        {
            if (inGO.GetComponent<MeshFilter>() == null || inGO.GetComponent<MeshFilter>().sharedMesh == null)
            {
                Debug.Log("Not copied. No mesh to duplicate in CopyGameObjectWithDuplicateMesh \n");
                return inGO;
            }

            GameObject copyGO = GameObject.Instantiate(inGO);

            Mesh srcMesh = copyGO.GetComponent<MeshFilter>().sharedMesh;
            Mesh newMesh = DuplicateMesh(srcMesh);
            newMesh.name = inGO.name + "_dup";

            if (recalcNormals)
                newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            copyGO.GetComponent<MeshFilter>().mesh = newMesh;

            return copyGO;
        }
        //--------
        // returns total depth inc. root and leaf, as well as tthe deepest leaf transform
        public static (int, Transform) GetObjectHierarchyDepth(Transform root, bool print, int depth, int maxDepth = 0, Transform deepestLeaf = null, int leafDepth = 0)
        {
            if (depth == 0)
            {
                if (print)
                    Debug.Log(root.name + ": 0 \n");
                leafDepth = 0;
                deepestLeaf = root;
            }
            else
            {
                if (depth > leafDepth)
                    leafDepth = depth;
                if (leafDepth > maxDepth)
                {
                    maxDepth = leafDepth;
                    deepestLeaf = root;
                }
            }
            var children = root.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.parent == root)
                {
                    if (print)
                        Debug.Log(child.name + ": " + (depth + 1) + "\n");
                    (maxDepth, deepestLeaf) = GetObjectHierarchyDepth(child, print, depth + 1, maxDepth, deepestLeaf);
                }
            }
            if (children.Length == 1 && print)
                Debug.Log("Leaf Depth: " + leafDepth + "\n");
            return (maxDepth, deepestLeaf);
        }
        //----------
        public static List<GameObject> GetAllMeshGameObjectsFromGameObject(GameObject go, bool includeRootParent = true)
        {
            List<GameObject> allGameObjects = new List<GameObject>();

            if (go == null)
            {
                Debug.Log("inGO was null in GetAllMeshGameObjectsFromGameObject()\n");
                return allGameObjects;
            }

            Transform[] allObjects = go.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allObjects)
            {
                if (child.gameObject != null)
                {
                    MeshFilter mf = (MeshFilter)child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
                    if (mf != null)
                    {
                        if (child.gameObject != go || includeRootParent == true)
                            allGameObjects.Add(child.gameObject);
                    }
                }

            }
            return allGameObjects;
        }
        //----------
        public static List<GameObject> GetAllGameObjectsFromGameObject(GameObject go, bool includeRootParent = true)
        {
            List<GameObject> allGameObjects = new List<GameObject>();
            if (go == null)
            {
                Debug.Log("inGO was null in GetAllMeshGameObjectsFromGameObject()\n");
                return allGameObjects;
            }
            Transform[] allObjects = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allObjects)
            {
                if (child.gameObject != go || includeRootParent == true)
                    allGameObjects.Add(child.gameObject);
            }
            return allGameObjects;
        }
        //------
        static public List<GameObject> GetDirectChildrenOnly(GameObject go)
        {
            // Direct children only, not parent
            List<GameObject> children = new List<GameObject>();
            foreach (Transform trans in go.transform)
            {
                children.Add(trans.gameObject);
            }
            return children;
        }
        static public Mesh CreateBoxMesh(float width, float height, float depth)
        {
            Mesh mesh = new Mesh();

            // Define verticesSides for each face (4 verticesSides per face)
            Vector3[] vertices = new Vector3[]
            {
                // Front face
                new Vector3(-width / 2, -height / 2, depth / 2),  // 0
                new Vector3(width / 2, -height / 2, depth / 2),   // 1
                new Vector3(width / 2, height / 2, depth / 2),    // 2
                new Vector3(-width / 2, height / 2, depth / 2),   // 3

                // Back face
                new Vector3(width / 2, -height / 2, -depth / 2),  // 4
                new Vector3(-width / 2, -height / 2, -depth / 2), // 5
                new Vector3(-width / 2, height / 2, -depth / 2),  // 6
                new Vector3(width / 2, height / 2, -depth / 2),   // 7

                // Top face
                new Vector3(-width / 2, height / 2, depth / 2),   // 8
                new Vector3(width / 2, height / 2, depth / 2),    // 9
                new Vector3(width / 2, height / 2, -depth / 2),   // 10
                new Vector3(-width / 2, height / 2, -depth / 2),  // 11

                // Bottom face
                new Vector3(-width / 2, -height / 2, -depth / 2), // 12
                new Vector3(width / 2, -height / 2, -depth / 2),  // 13
                new Vector3(width / 2, -height / 2, depth / 2),   // 14
                new Vector3(-width / 2, -height / 2, depth / 2),  // 15

                // Left face
                new Vector3(-width / 2, -height / 2, -depth / 2), // 16
                new Vector3(-width / 2, -height / 2, depth / 2),  // 17
                new Vector3(-width / 2, height / 2, depth / 2),   // 18
                new Vector3(-width / 2, height / 2, -depth / 2),  // 19

                // Right face
                new Vector3(width / 2, -height / 2, depth / 2),   // 20
                new Vector3(width / 2, -height / 2, -depth / 2),  // 21
                new Vector3(width / 2, height / 2, -depth / 2),   // 22
                new Vector3(width / 2, height / 2, depth / 2)     // 23
            };

            // Define trianglesSides in counter-clockwise order
            int[] triangles = new int[]
            {
                // Front face
                0, 1, 2, 0, 2, 3,
                // Back face
                4, 5, 6, 4, 6, 7,
                // Top face
                8, 9, 10, 8, 10, 11,
                // Bottom face
                12, 14, 13, 12, 15, 14,
                // Left face
                16, 17, 18, 16, 18, 19,
                // Right face
                20, 21, 22, 20, 22, 23
            };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // Recalculate normals for correct lighting interactions

            return mesh;
        }
        public static Mesh CreateCylinderMesh(float height, float radius, int numSegments = 8, bool smoothed = false)
        {
            Mesh mesh = new Mesh();
            List<Vector3> verticesSides = new List<Vector3>();
            List<Vector3> verticesCaps = new List<Vector3>();
            List<int> trianglesSides = new List<int>();
            List<int> trianglesCaps = new List<int>();

            float heightHalf = height / 2;
            float segmentAngle = 360.0f / numSegments;

            // Add top center and bottom center
            verticesCaps.Add(new Vector3(0, heightHalf, 0)); // Top center vertex at index 0
            verticesCaps.Add(new Vector3(0, -heightHalf, 0)); // Bottom center vertex at index 1

            smoothed = true;

            if (smoothed)
            {
                //=================================
                //      Create Smoothed Vertices             
                //=================================
                // Adding verticesSides for each segment
                for (int i = 0; i <= numSegments; i++)
                {
                    float angleInRadians = Mathf.Deg2Rad * segmentAngle * i;
                    float x = Mathf.Cos(angleInRadians) * radius;
                    float z = Mathf.Sin(angleInRadians) * radius;
                    Vector3 topVertex = new Vector3(x, heightHalf, z);
                    Vector3 bottomVertex = new Vector3(x, -heightHalf, z);

                    verticesSides.Add(topVertex); // Top vertex of segment
                    verticesSides.Add(bottomVertex); // Bottom vertex of segment
                }
                for (int i = 0; i <= numSegments; i++)
                {
                    float angleInRadians = Mathf.Deg2Rad * segmentAngle * i;
                    float x = Mathf.Cos(angleInRadians) * radius;
                    float z = Mathf.Sin(angleInRadians) * radius;
                    Vector3 topVertex = new Vector3(x, heightHalf, z);
                    Vector3 bottomVertex = new Vector3(x, -heightHalf, z);

                    verticesCaps.Add(topVertex); // Top vertex of segment
                    verticesCaps.Add(bottomVertex); // Bottom vertex of segment
                }
                //=================================
                //      Create Smoothed Triangles             
                //=================================
                // Creating trianglesSides for the side faces
                for (int i = 0; i < verticesSides.Count; i += 2)
                {
                    trianglesSides.Add(i);
                    trianglesSides.Add(i + 2);
                    trianglesSides.Add(i + 1);

                    trianglesSides.Add(i + 1);
                    trianglesSides.Add(i + 2);
                    trianglesSides.Add(i + 3);
                }

                // Top cap trianglesSides
                for (int i = 2; i < verticesCaps.Count - 2; i += 2)
                {
                    trianglesCaps.Add(0);
                    trianglesCaps.Add(i + 2);
                    trianglesCaps.Add(i);
                }

                // Bottom cap trianglesSides
                for (int i = 3; i < verticesCaps.Count - 1; i += 2)
                {
                    trianglesCaps.Add(1);
                    trianglesCaps.Add(i);
                    trianglesCaps.Add(i + 2);
                }
            }
            else if (!smoothed)
            {
                // Create Unsmoothed Vertices
                for (int i = 0; i <= numSegments; i++)
                {
                    float angleInRadians = Mathf.Deg2Rad * segmentAngle * i;
                    float x = Mathf.Cos(angleInRadians) * radius;
                    float z = Mathf.Sin(angleInRadians) * radius;
                    // Duplicate verticesSides for each face to achieve hard edges
                    verticesSides.Add(new Vector3(x, heightHalf, z)); // Top vertex of segment
                    verticesSides.Add(new Vector3(x, -heightHalf, z)); // Bottom vertex of segment
                }

                // Create Unsmoothed Triangles
                // Adjusting to ensure we don't share verticesSides between faces
                for (int i = 2; i < verticesSides.Count - 2; i += 2)
                {
                    trianglesSides.Add(i);
                    trianglesSides.Add(i + 1);
                    trianglesSides.Add(i + 3);

                    trianglesSides.Add(i);
                    trianglesSides.Add(i + 3);
                    trianglesSides.Add(i + 2);
                }

                // Correcting Cap Triangles for unsmoothed - Ensuring no vertex sharing
                // Top cap trianglesSides
                for (int i = 2; i < verticesSides.Count - 2; i += 2)
                {
                    trianglesSides.Add(0);
                    trianglesSides.Add(i + 2);
                    trianglesSides.Add(i);
                }

                // Bottom cap trianglesSides - Need to ensure the order is corrected for outward normals
                for (int i = 3; i < verticesSides.Count - 1; i += 2)
                {
                    trianglesSides.Add(1);
                    trianglesSides.Add(i);
                    trianglesSides.Add(i + 2);
                }
            }


            List<Vector3> allVertices = new List<Vector3>(verticesSides);
            allVertices.AddRange(verticesCaps);

            List<int> allTriangles = new List<int>(trianglesSides);
            allTriangles.AddRange(trianglesCaps);



            mesh.vertices = allVertices.ToArray();
            mesh.triangles = allTriangles.ToArray();
            mesh.RecalculateNormals(); // Make sure to recalculate normals

            return mesh;
        }

        //Create a sphere mesh with the given radius and number of segments
        public static Mesh CreateSphereMesh(float radius, int numSegments = 8)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Add top center and bottom center
            vertices.Add(new Vector3(0, radius, 0)); // Top center vertex at index 0
            vertices.Add(new Vector3(0, -radius, 0)); // Bottom center vertex at index 1

            // Create vertices for each segment
            for (int i = 1; i < numSegments; i++)
            {
                float latitude = Mathf.PI * i / numSegments;
                float y = Mathf.Cos(latitude) * radius;
                float radiusAtY = Mathf.Sin(latitude) * radius;

                for (int j = 0; j < numSegments; j++)
                {
                    float longitude = 2 * Mathf.PI * j / numSegments;
                    float x = Mathf.Cos(longitude) * radiusAtY;
                    float z = Mathf.Sin(longitude) * radiusAtY;

                    vertices.Add(new Vector3(x, y, z));
                }
            }

            // Add vertices for the bottom cap
            for (int i = 1; i < numSegments; i++)
            {
                vertices.Add(vertices[vertices.Count - numSegments + i]);
            }

            // Create triangles for the top cap
            for (int i = 2; i < numSegments; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            triangles.Add(0);
            triangles.Add(numSegments);
            triangles.Add(2);

            // Create triangles for the middle
            for (int i = 0; i < numSegments - 2; i++)
            {
                for (int j = 0; j < numSegments - 1; j++)
                {
                    int index = i * numSegments + j + 1;

                    triangles.Add(index);
                    triangles.Add(index + numSegments);
                    triangles.Add(index + numSegments + 1);

                    triangles.Add(index);
                    triangles.Add(index + numSegments + 1);
                    triangles.Add(index + 1);
                }

                int lastIndex = (i + 1) * numSegments;
                int lastTopIndex = i * numSegments + 1;

                triangles.Add(lastIndex);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals(); // Make sure to recalculate normals

            return mesh;
        }
        public static Mesh CreateSmoothSphere(float radius, int subdivisions)
        {
            // Create a new mesh object
            Mesh mesh = new Mesh();

            // Initialize icosphere creator with radius and subdivisions
            IcoSphereCreator creator = new IcoSphereCreator();
            creator.Create(radius, subdivisions);

            // Assign vertices and triangles to the mesh
            mesh.vertices = creator.Vertices.ToArray();
            mesh.triangles = creator.Triangles.ToArray();

            // Recalculate normals for the sphere to render lighting correctly
            mesh.RecalculateNormals();

            return mesh;
        }

        private class IcoSphereCreator
        {
            private List<Vector3> vertices = new List<Vector3>();
            private List<int> triangles = new List<int>();
            private int index = 0;
            private Dictionary<long, int> middlePointIndexCache;

            // Create the sphere
            public void Create(float radius, int subdivisions)
            {
                middlePointIndexCache = new Dictionary<long, int>();
                var t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

                // Add vertices of a icosahedron
                AddVertex(new Vector3(-1, t, 0));
                AddVertex(new Vector3(1, t, 0));
                AddVertex(new Vector3(-1, -t, 0));
                AddVertex(new Vector3(1, -t, 0));
                AddVertex(new Vector3(0, -1, t));
                AddVertex(new Vector3(0, 1, t));
                AddVertex(new Vector3(0, -1, -t));
                AddVertex(new Vector3(0, 1, -t));
                AddVertex(new Vector3(t, 0, -1));
                AddVertex(new Vector3(t, 0, 1));
                AddVertex(new Vector3(-t, 0, -1));
                AddVertex(new Vector3(-t, 0, 1));

                // Create 20 triangles of the icosahedron
                var faces = new List<(int, int, int)>
            {
                (0, 11, 5), (0, 5, 1), (0, 1, 7), (0, 7, 10), (0, 10, 11),
                (1, 5, 9), (5, 11, 4), (11, 10, 2), (10, 7, 6), (7, 1, 8),
                (3, 9, 4), (3, 4, 2), (3, 2, 6), (3, 6, 8), (3, 8, 9),
                (4, 9, 5), (2, 4, 11), (6, 2, 10), (8, 6, 7), (9, 8, 1)
            };

                // Refine triangles
                for (int i = 0; i < subdivisions; i++)
                {
                    List<(int, int, int)> faces2 = new List<(int, int, int)>();
                    foreach (var tri in faces)
                    {
                        int a = GetMiddlePoint(tri.Item1, tri.Item2);
                        int b = GetMiddlePoint(tri.Item2, tri.Item3);
                        int c = GetMiddlePoint(tri.Item3, tri.Item1);

                        faces2.Add((tri.Item1, a, c));
                        faces2.Add((tri.Item2, b, a));
                        faces2.Add((tri.Item3, c, b));
                        faces2.Add((a, b, c));
                    }
                    faces = faces2;
                }

                // Convert refined vertices into spherical coordinates
                for (int i = 0; i < vertices.Count; i++)
                {
                    vertices[i] = vertices[i].normalized * radius;
                }

                // Assign triangles
                foreach (var tri in faces)
                {
                    triangles.Add(tri.Item1);
                    triangles.Add(tri.Item2);
                    triangles.Add(tri.Item3);
                }
            }

            // Add vertex avoiding duplicates
            private void AddVertex(Vector3 position)
            {
                vertices.Add(position.normalized);
            }

            // Get middle point for edge, cache to avoid duplicates
            private int GetMiddlePoint(int p1, int p2)
            {
                long smallerIndex = Mathf.Min(p1, p2);
                long greaterIndex = Mathf.Max(p1, p2);
                long key = (smallerIndex << 32) + greaterIndex;

                if (middlePointIndexCache.TryGetValue(key, out int ret))
                {
                    return ret;
                }

                Vector3 point1 = vertices[p1];
                Vector3 point2 = vertices[p2];
                Vector3 middle = Vector3.Lerp(point1, point2, 0.5f);

                int i = vertices.Count;
                vertices.Add(middle.normalized);
                middlePointIndexCache[key] = i;
                return i;
            }

            public List<Vector3> Vertices { get { return vertices; } }
            public List<int> Triangles { get { return triangles; } }
        }

        //---------------------------------
        // the return List is just for convenience. You could also access them from the updated sourceMFs
        public static List<Mesh> CloneAndReplaceMeshes(List<MeshFilter> sourceMFs)
        {
            Mesh newMesh, thisMesh;
            MeshFilter mf = null;
            List<Mesh> newMeshes = new List<Mesh>();
            for (int i = 0; i < sourceMFs.Count; i++)
            {
                mf = sourceMFs[i];
                thisMesh = mf.sharedMesh;
                if (thisMesh == null)
                {
                    Debug.LogWarning("Missing mesh on " + mf.name + " in CloneMeshes \n");
                    newMesh = null;
                }
                else
                    newMesh = DuplicateMesh(thisMesh);
                newMeshes.Add(newMesh);
                sourceMFs[i].sharedMesh = newMesh;
            }
            return newMeshes;
        }
        //-------------------------
        public static bool DoesRootHaveMesh(GameObject inGO)
        {
            MeshFilter mf = inGO.GetComponent<MeshFilter>();
            if (mf == null)
                return false;

            return true;
        }
        public static void ResizeGameObjectMesh(GameObject go, Vector3 newSize)
        {
            ResizeMeshXYZ(go, newSize.x, newSize.y, newSize.z);

        }

        // Resize mesh to a given Absolute size
        // a newSize of 0 will keep the original size
        public static void ResizeMesh(Mesh mesh, Vector3 newSize)
        {
            Vector3 origSize = mesh.bounds.size;
            float newX = newSize.x;
            float newY = newSize.y;
            float newZ = newSize.z;

            if (newX == 0)
                newX = origSize.x;
            if (newY == 0)
                newY = origSize.y;
            if (newZ == 0)
                newZ = origSize.z;

            float scaleX = newX / origSize.x;
            float scaleY = newY / origSize.y;
            float scaleZ = newZ / origSize.z;

            Vector3[] verts = mesh.vertices;
            for (int v = 0; v < verts.Length; v++)
            {
                verts[v].x *= scaleX;
                verts[v].y *= scaleY;
                verts[v].z *= scaleZ;
            }
            mesh.vertices = verts;
            mesh.RecalculateBounds();
        }
        // Resize All meshes on Gameobject so that the combined size is newSize
        public static (Vector3, Vector3) ResizeMeshXYZ(GameObject go, float newX, float newY, float newZ)
        {
            Vector3 origSize = GetCombinedSizeOfAllMeshesInGameObject(go);

            if (newX == 0)
                newX = origSize.x;
            if (newY == 0)
                newY = origSize.y;
            if (newZ == 0)
                newZ = origSize.z;

            float scaleX = newX / origSize.x;
            float scaleY = newY / origSize.y;
            float scaleZ = newZ / origSize.z;

            List<MeshFilter> allMeshes = GetAllMeshFiltersFromGameObject(go);
            for (int i = 0; i < allMeshes.Count; i++)
            {
                MeshFilter mf = allMeshes[i];
                Mesh mesh = mf.sharedMesh;
                Vector3[] verts = mesh.vertices;
                for (int v = 0; v < verts.Length; v++)
                {
                    verts[v].x *= scaleX;
                    verts[v].y *= scaleY;
                    verts[v].z *= scaleZ;
                }
                mesh.vertices = verts;
                mesh.RecalculateBounds();
                mf.sharedMesh = mesh;
            }

            Vector3 newSize = GetCombinedSizeOfAllMeshesInGameObject(go);

            //Debug.Log("Resized from " + origSize + " to " + newSize + "\n");
            return (origSize, newSize);
        }
        //---------
        //same as above but uses a List<Mesh> instead of go
        public static void ResizeMeshXYZ(List<Mesh> meshGroup, Vector3 origSize, Vector3 newSize)
        {
            // Calculate the scale factors needed to match the new size
            Vector3 scaleFactors = new Vector3(
                origSize.x == 0 ? 0 : newSize.x / origSize.x,
                origSize.y == 0 ? 0 : newSize.y / origSize.y,
                origSize.z == 0 ? 0 : newSize.z / origSize.z);

            // Apply the scale factors to each mesh
            foreach (Mesh mesh in meshGroup)
            {
                if (mesh != null)
                {
                    ScaleSingleMesh(mesh, scaleFactors);
                }
            }
        }
        // Helper method to scale a mesh by given scale factors
        private static void ScaleSingleMesh(Mesh mesh, Vector3 scaleFactors)
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Scale(vertices[i], scaleFactors);
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }
        //------------------------------------------------------
        // Function to find the first GameObject with a specified mesh name in a list of GameObjects.
        // It iterates over each GameObject and uses a recursive search to check all descendants.
        public static GameObject FindFirstGameObjectWithMeshName(List<GameObject> gameObjects, string meshName)
        {
            foreach (GameObject go in gameObjects)
            {
                // Perform a recursive search in the current GameObject's hierarchy.
                GameObject found = SearchInGameObject(go, meshName);
                if (found != null)
                {
                    // Return the first found GameObject with the matching mesh.
                    return found;
                }
            }
            return null;  // Return null if no GameObject with the specified mesh is found.
        }
        //------------
        // Recursive function to search for a mesh within a GameObject and its children.
        // This checks the GameObject's MeshFilter and recurses through its children if needed.
        public static GameObject SearchInGameObject(GameObject go, string targetMeshName)
        {
            // Check the MeshFilter of the current GameObject.
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == targetMeshName)
            {
                // Return the GameObject if its mesh name matches the target.
                return go;
            }
            // If not found, recursively search through each child.
            foreach (Transform child in go.transform)
            {
                // Recursive call to search within the child GameObject.
                GameObject result = SearchInGameObject(child.gameObject, targetMeshName);
                if (result != null)
                {
                    // Return the first found matching GameObject in the child hierarchy.
                    return result;
                }
            }
            return null;  // Return null if no matching mesh is found in this branch of the hierarchy.
        }
    }
}

