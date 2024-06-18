using MeshUtils;
using System.Collections.Generic;
using UnityEngine;

public class MeshTransformReset : MonoBehaviour
{
    public static GameObject ResetMesh(GameObject rootGO)
    {
        GameObject deepCopy = MeshUtilitiesAFWB.DuplicateGameObjectHierarchyUniqueMeshAndMaterial(rootGO);

        List<GameObject> allGameObjects = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(deepCopy);

        Mesh mesh = null;
        for (int i = 0; i < allGameObjects.Count; i++)
        {
            GameObject go = allGameObjects[i];

            if (MeshUtilitiesAFWB.GameObjectHasMesh(go) /*&& go.transform.parent != null*/)
            {
                Transform t = go.transform;

                Matrix4x4 localToWorldMatrix = t.localToWorldMatrix;

                Vector3 point = Vector3.zero;

                mesh = MeshUtilitiesAFWB.GetFirstMeshInGameObject(go);

                Vector3[] newVerts = new Vector3[mesh.vertices.Length];
                Vector3[] verts = mesh.vertices;
                Vector3 v;
                int n = mesh.vertices.Length;

                for (int j = 0; j < n; j++)
                {
                    v = verts[j];
                    Vector3 newPt = localToWorldMatrix.MultiplyPoint3x4(v);
                    newVerts[j] = newPt;
                }
                mesh.vertices = newVerts;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                //Vector3 moved = MeshUtilitiesAFWB.RecentreMesh(mesh);
                //go.transform.Translate(-moved);
            }
        }
        for (int i = 0; i < allGameObjects.Count; i++)
        {
            GameObject go = allGameObjects[i];
            go.transform.localPosition = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }

        //===== Now Recentre the pos on all the children =====
        for (int i = 0; i < allGameObjects.Count; i++) //******!!!!
        {
            GameObject go = allGameObjects[i];
            Vector3 shift = MeshUtilitiesAFWB.RecentreMesh(go.GetComponent<MeshFilter>().sharedMesh, recalcBounds: true);

            if (shift != Vector3.zero)
            {
                Vector3 newPos, oldPos = go.transform.localPosition;
                //=== lossyScale is globalScale, i.e. takes into account all of its ancestors scaling
                //Vector3 scaledShift = -Vector3.Scale(shift, go.transform.lossyScale);
                //go.transform.Translate(scaledShift);
                go.transform.Translate(-shift);
                //--Shift its children also
                List<GameObject> currDirectChildren = MeshUtilitiesAFWB.GetDirectChildrenOnly(go);
                for (int j = 0; j < currDirectChildren.Count; j++)
                {
                    GameObject currDirectChildGo = currDirectChildren[j];
                    oldPos = currDirectChildGo.transform.localPosition;
                    currDirectChildGo.transform.Translate(shift);
                    newPos = currDirectChildGo.transform.localPosition;
                }
            }
        }

        /*for (int i = 0; i < allGameObjects.Count; i++)
        {
            GameObject go = allGameObjects[i];
            go.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            //go.transform.svRotation = Quaternion.Euler(2,2,2);
            go.transform.svRotation = Quaternion.identity;
            go.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            go.transform.svRotation = Quaternion.identity;
        }
        for (int i = allGameObjects.Count-1; i >= 0; i--)
        {
            GameObject go = allGameObjects[i];
            go.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            go.transform.svRotation = Quaternion.identity;
            go.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            go.transform.svRotation = Quaternion.identity;
        }
        for (int i = 0; i < allGameObjects.Count; i++)
        {
            GameObject go = allGameObjects[i];
            go.transform.svRotation = Quaternion.identity;
            go.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            go.transform.svRotation = Quaternion.identity;
            go.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }*/
        /*for (int i = allGameObjects.Count - 1; i >= 0; i--)
        {
            GameObject go = allGameObjects[i];
            go.transform.localEulerAngles = new Vector3(180.0f, 180.02f, 180.0f);
        }*/

        return deepCopy;
    }

    //--------------------------------
    public static void ResetMesh2(GameObject rootGO)
    {
        List<GameObject> allGameObjects = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(rootGO);
        //List<GameObject> allGameObjects = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(deepCopy);

        Mesh mesh = null;
        for (int i = 0; i < allGameObjects.Count; i++)
        {
            GameObject go = allGameObjects[i];

            if (MeshUtilitiesAFWB.GameObjectHasMesh(go) /*&& go.transform.parent != null*/)
            {
                Transform t = go.transform;

                Matrix4x4 localToWorldMatrix = t.localToWorldMatrix;

                Vector3 point = Vector3.zero;

                GameObject newMeshGO = null;
                /*go.transform.localPosition = Vector3.zero;
                go.transform.svRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;*/

                newMeshGO = MeshUtilitiesAFWB.DuplicateGameObjectUniqueMeshAndMaterial(go);
                mesh = MeshUtilitiesAFWB.GetFirstMeshInGameObject(newMeshGO);

                //mesh = MeshUtilitiesAFWB.GetFirstMeshInGameObject(go);

                Vector3[] newVerts = new Vector3[mesh.vertices.Length];
                Vector3[] verts = mesh.vertices;
                Vector3 v;
                int n = mesh.vertices.Length;

                for (int j = 0; j < n; j++)
                {
                    v = verts[j];
                    Vector3 newPt = localToWorldMatrix.MultiplyPoint3x4(v);
                    newVerts[j] = newPt;
                }
                mesh.vertices = newVerts;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                Vector3 moved = MeshUtilitiesAFWB.RecentreMesh(mesh);
                newMeshGO.transform.Translate(-moved);
                //go.transform.Translate(-moved);
            }
        }
    }
}