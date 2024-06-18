#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0414 // same for private fields
#pragma warning disable 0168 // same for named but not assigned

using AFWB;
using MeshUtils;
using System.Collections.Generic;
using UnityEngine;

public class RailGeometryUtilities : MonoBehaviour
{
    //=======================================================================
    // From mark White Mod 'AF Prefab Mode J'
    // Creates a cleaned up GameObject with any children
    //-- Optionally pass in a reference mesh to hint at its svSize (i.e. the mesh you will be replacing)
    // If so, we take it that we want the new user mesh to be modified to fit in witht the current fence design
    public static GameObject CreateAFBRailFromGameObject(GameObject inGO, AutoFenceCreator afb, GameObject inRefMesh = null, bool log = true)
    {
        if (inGO == null)
            return null;
        Debug.Log(" -------------------  " + inGO.name + "  -------------------\n");

        //int depth = GetObjectHierarchyDepth(inGO.transform);
        //Debug.Log("Depth = " + depth);
        int depth = 0, maxDepth = 0;
        Transform deepestGO;
        (maxDepth, deepestGO) = MeshUtilitiesAFWB.GetObjectHierarchyDepth(inGO.transform, print: false, depth);
        //Debug.Log("Max Depth = " + (maxDepth + 1) + "  Deepest Leaf: " + deepestGO.name + "\n");

        //-- Instantiate a copy that will be returned to AFWB  --
        GameObject go = null, rootGO = GameObject.Instantiate(inGO);

        //-- Zero The Root's localPosition and svRotation : they get set during build --
        rootGO.transform.localPosition = Vector3.zero;
        float maxOrigRot = VectorUtilitiesTCT.GetMaxAbsVector3Element(rootGO.transform.localEulerAngles);
        //-- Did Root have a Rotation --
        bool rootHadRotation = maxOrigRot > 0.0001f;
        Vector3 origRootEulerAngles = rootGO.transform.localEulerAngles;
        Vector3 origRootScaling = rootGO.transform.localScale;
        rootGO.transform.rotation = Quaternion.identity;

        //--- Get all GameObjects/MeshFilters and Meshes  ---
        List<GameObject> allMeshGameObjects = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(rootGO);
        List<GameObject> allGameObjects = MeshUtilitiesAFWB.GetAllGameObjectsFromGameObject(rootGO);
        List<GameObject> allMeshGameObjectsNoRoot = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(rootGO, includeRootParent: false);
        List<MeshFilter> allMeshFilters = MeshUtilitiesAFWB.GetAllMeshFiltersFromGameObject(rootGO);
        List<GameObject> allDirectChildren = MeshUtilitiesAFWB.GetDirectChildrenOnly(rootGO);
        List<Vector3> accumAncestorMeshCentreOffsets = new List<Vector3>();
        List<Vector3> accumAncestorTransScaling = new List<Vector3>();

        //-- Clone all meshes --
        List<Mesh> allMeshes = MeshUtilitiesAFWB.CloneAndReplaceMeshes(allMeshFilters);
        if (allMeshes == null || allMeshes.Count == 0)
            return null;

        //-- Does Root have a mesh --
        bool rootHasMesh = MeshUtilitiesAFWB.DoesRootHaveMesh(rootGO);
        //Debug.Log("topParent  HasMesh = " + topParentHasMesh + ",   Had Rotation = " + topParentHadRotation + "\n");

        //-- Get bounds of the entire combined Root & children --
        Bounds bounds, origCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
        Bounds currCombinedBounds = origCombinedBounds;
        Vector3 currCombinedMax = Vector3.zero, currCombinedCentre = Vector3.zero, currCombinedSize = Vector3.zero;
        Vector3 center, max, min;
        int numNonLODMeshes = MeshUtilitiesAFWB.CountNonLODMeshes(rootGO);
        Vector3 shift = Vector3.zero, scaledShift = Vector3.zero, rootShift = Vector3.zero;
        Mesh mesh = null;
        bool didRotate = false;
        int startGO = 0;
        //return rootGO;

        rootGO.transform.localScale = Vector3.one; //Good!!

        //=============================================
        //          Bake GameObjects Rotations
        //=============================================
        MeshUtilitiesAFWB.BakeAllTransformRotations(rootGO, shiftTransformsToMatch: true);
        currCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
        //return rootGO;

        //=============================================
        //          Bake Root Transform Rotations
        //=============================================
        //-- we need a version without Y svRotation as the heading is determined by AF
        //-- It would also get confused if we happen to put the model in the scene at an arbitrary y angle, this would get baked.
        Vector3 origRootEulerAnglesNonY = origRootEulerAngles;
        origRootEulerAnglesNonY.y = 0;
        if (rootHadRotation)
        {
            // From working Post version G
            for (int i = 0; i < allMeshes.Count; i++)
            {
                go = allMeshGameObjects[i];
                mesh = allMeshes[i];
                Vector3 centre = mesh.bounds.center;
                //Debug.Log(go.name + "  centre " + centre + "\n");
                shift = MeshUtilitiesAFWB.RecentreMesh(mesh, recalcBounds: true);//*****
                //Debug.Log(go.name + "  shift " + shift + "\n");
                Vector3 locaScaledlUnrotatedShift = Vector3.Scale(shift, go.transform.localScale);
                go.transform.Translate(-locaScaledlUnrotatedShift); //good so far, centred and correct position *****
            }
            for (int i = 0; i < allMeshes.Count; i++)
            {
                go = allMeshGameObjects[i];
                mesh = allMeshes[i];
                if (go == rootGO)
                {
                    MeshUtilitiesAFWB.RotateMeshAndNormals(allMeshes[0], origRootEulerAngles, recentre: false, rootGO.transform.localPosition);
                }
                else
                {
                    //-- Rotate the (now centred) pos point and move the go to there...
                    Vector3 localPivotPoint = rootGO.transform.localPosition - go.transform.localPosition;
                    Vector3 rotatedPivotPoint = MeshUtilitiesAFWB.RotatePointAroundPivot(localPivotPoint, rootGO.transform.localPosition, -origRootEulerAngles);
                    go.transform.localPosition = rotatedPivotPoint;
                    // then rotate the go on it's on centre
                    MeshUtilitiesAFWB.RotateMeshAndNormals(allMeshes[i], origRootEulerAngles, recentre: false, Vector3.zero);//*****
                }
            }

            //Version G
            /*for (int i = 0; i < allMeshes.Count; i++)
            {
                go = allMeshGameObjects[i];
                if (go == rootGO)
                {
                    RotateMeshAndNormals(allMeshes[0], origRootEulerAnglesNonY, recentre: false, rootGO.transform.localPosition);
                }
                else
                {
                    shift = RotateMeshAndNormals(allMeshes[i], origRootEulerAnglesNonY, recentre: false, -rootGO.transform.localPosition);
                }
            }
            didRotate = true;*/
        }
        //return rootGO;

        //==========================================================
        //                   Centre Mesh Pivots
        //
        //  Note also: A model with multiple paralllel top meshes
        //  will appear as a Null Root with multiple direct children
        //===========================================================
        //==========================
        //      Root Has Mesh
        //==========================
        //===== First deal with the root (object[0]) =====
        startGO = 0;
        if (rootHasMesh)
            startGO = 1;
        if (rootHasMesh == true)
        {
            rootShift = MeshUtilitiesAFWB.RecentreMesh(rootGO.GetComponent<MeshFilter>().sharedMesh, recalcBounds: true);
            //-- If it has a mesh offset and has been shifted,  we cant compensate with a transform on the root,
            //so we add the inverse to the direct children transforms
            // we only need to do the direct children because their movement will take descendant with them
            // It doesn't matter if the root has a transform offset as that was removed at the begining
            if (rootShift != Vector3.zero)
            {
                for (int j = 0; j < allDirectChildren.Count; j++)
                {
                    GameObject directChildGo = allDirectChildren[j];
                    directChildGo.transform.Translate(rootShift);
                }
            }
        }
        //===== Now Recentre the pos on all the children =====
        for (int i = startGO; i < allMeshGameObjects.Count; i++) //******!!!!
        {
            go = allMeshGameObjects[i]; //******!!!!
            if (go.name.Contains("ch1"))
                Debug.Log("");

            shift = MeshUtilitiesAFWB.RecentreMesh(go.GetComponent<MeshFilter>().sharedMesh, recalcBounds: true);

            if (shift != Vector3.zero)
            {
                Vector3 newPos, oldPos = go.transform.localPosition;
                //=== lossyScale is globalScale, i.e. takes into account all of its ancestors scaling
                scaledShift = -Vector3.Scale(shift, go.transform.lossyScale);
                go.transform.Translate(scaledShift);
                //--Shift its children also
                List<GameObject> currDirectChildren = MeshUtilitiesAFWB.GetDirectChildrenOnly(go);
                for (int j = 0; j < currDirectChildren.Count; j++)
                {
                    GameObject currDirectChildGo = currDirectChildren[j];
                    oldPos = currDirectChildGo.transform.localPosition;
                    currDirectChildGo.transform.Translate(-scaledShift);
                    newPos = currDirectChildGo.transform.localPosition;
                }
            }
        }
        if (rootHasMesh == false)
        {
            //== With a Null Root, we could have a situation where the direct children (i.e. the top level mesh objects)
            //== have a pointless transform offset. Remove it here by checking the bounds *with* their transform offsets
            currCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
            currCombinedCentre = currCombinedBounds.center;
            for (int i = 0; i < allDirectChildren.Count; i++)
            {
                go = allDirectChildren[i];
                go.transform.Translate(-currCombinedCentre.x, -currCombinedCentre.y, -currCombinedCentre.z);
            }
        }
        currCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
        //return rootGO;

        //=================================
        //     Bake Transform Scaling
        //=================================

        //== Now that all the pivots are centred and aligned, we can safely bake the scaling without anything shifting

        startGO = 0;
        if (rootHasMesh)
            startGO = 1;
        rootGO.transform.localScale = Vector3.one; // we don't need it as it gets scaled to fit AFWB at the end anyway
        for (int i = startGO; i < allMeshGameObjects.Count; i++)
        {
            mesh = allMeshes[i];
            go = allMeshGameObjects[i];
            Vector3 transScaling = go.transform.lossyScale;
            Vector3 local = go.transform.localScale;
            mesh = MeshUtilitiesAFWB.ScaleMesh(mesh, transScaling, adjustForPivot: false, recalculateBounds: true); // scale everything
            //Debug.Log(go.name + "  transScaling:  " + transScaling + "\n");
            go.transform.localPosition = Vector3.Scale(go.transform.localPosition, go.transform.parent.lossyScale);
        }
        // reseting in loop removes needed scale details, so do it here
        for (int i = 0; i < allMeshGameObjects.Count; i++)
        {
            allMeshGameObjects[i].transform.localScale = Vector3.one; ;
        }
        currCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
        //return rootGO;

        //if (didRotate == false)
        //Debug.Log(" --  No Rotations were done!  --  \n");

        //=============================================
        //    Overall Auto Scaling with compensating-move
        //=============================================
        currCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
        float scaleFactorX = 3.0f / currCombinedBounds.size.x; // Set x scaling, 3 = default rail length
        float scaleFactorZ = scaleFactorX;
        float scaleFactorY = 1.0f, origCombinedHeight = currCombinedBounds.size.y;
        if (didRotate == false)
            scaleFactorY = (1 + (currCombinedBounds.size.y * scaleFactorX)) / 2;
        float tallestMesh = MeshUtilitiesAFWB.GetHeightOfTallestMeshGameObject(allMeshGameObjects);
        if (scaleFactorY * tallestMesh > currCombinedBounds.size.y)
            scaleFactorY = 1;
        float newCombinedHeight = scaleFactorY * currCombinedBounds.size.y;
        //Debug.Log("newCombinedHeight: " + newCombinedHeight + "  (original: " + origCombinedHeight + ")\n");

        //----- try height proportional  to x ---------
        float proportionalHeightScaling = scaleFactorX;
        scaleFactorY = proportionalHeightScaling;
        newCombinedHeight = scaleFactorY * currCombinedBounds.size.y;
        if (newCombinedHeight > currCombinedBounds.size.x)
            scaleFactorY = currCombinedBounds.size.x / currCombinedBounds.size.y;//set it to max of X length (3)
        else if (newCombinedHeight < 1.0f && newCombinedHeight < (currCombinedBounds.size.y / 2))// set it to min of half the original
            scaleFactorY = 0.5f;

        //---------------------------------------------
        //scaleFactorY *= origRootScaling.y;
        //scaleFactorZ *= origRootScaling.z;
        //scaleFactorY = 0.5f;
        float proportionXZ = currCombinedBounds.size.x / currCombinedBounds.size.z;// restrict the thickness to 1/3 of the length as an initial default
        if (proportionXZ < 3)
        {
            scaleFactorZ = scaleFactorX * proportionXZ / 3;
        }

        if (afb.scaleMeshOnImport == false)
        {
            scaleFactorY = scaleFactorZ = 1;
        }

        //scaleFactorZ = 1; //***** TEMP!!!!!!
        Vector3 scaleVec = new Vector3(scaleFactorX, scaleFactorY, scaleFactorZ);
        //Debug.Log("scaleVec: " + scaleVec + "\n");

        //for (int i = 0; i < allGameObjects.Count; i++)
        for (int i = 0; i < allMeshGameObjects.Count; i++)
        {
            go = allMeshGameObjects[i];
            mesh = allMeshes[i];

            MeshUtilitiesAFWB.ScaleMesh(mesh, scaleVec, adjustForPivot: false, recalculateBounds: true); // scale everything
            //-- Scale Transform Position offset
            Vector3 positionInRoot = go.transform.position - rootGO.transform.position;
            Vector3 positionInRootScaled = Vector3.Scale(positionInRoot, scaleVec);
            Vector3 localPositionScaled = Vector3.Scale(go.transform.localPosition, scaleVec);
        }
        for (int i = 0; i < allGameObjects.Count; i++)
        {
            go = allGameObjects[i];
            Vector3 localPositionScaled = Vector3.Scale(go.transform.localPosition, scaleVec);
            go.transform.localPosition = localPositionScaled;
        }
        //return rootGO;

        //======================================================
        //  Move into Position Suitable for AFWB Build process
        //======================================================
        currCombinedBounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(rootGO, true);
        currCombinedMax = currCombinedBounds.max;
        currCombinedCentre = currCombinedBounds.center;
        currCombinedSize = currCombinedBounds.size;
        float xPos = 0, yPos = 0, zPos = 0;
        Transform[] allObjects = rootGO.GetComponentsInChildren<Transform>(true); //orig
        for (int i = 0; i < allMeshGameObjects.Count; i++)
        {
            go = allMeshGameObjects[i].gameObject;
            MeshUtilitiesAFWB.TranslateMesh(go, new Vector3(-currCombinedMax.x, -currCombinedCentre.y, -currCombinedCentre.z));
        }

        //--the only time we respect the original root scaling is when it's negative
        Vector3 rootNegativeScalingAdjustment = rootGO.transform.localScale;
        if (origRootScaling.x < 0)
            rootNegativeScalingAdjustment.x *= -1;
        if (origRootScaling.y < 0)
            rootNegativeScalingAdjustment.y *= -1;
        if (origRootScaling.z < 0)
            rootNegativeScalingAdjustment.z *= -1;
        rootGO.transform.localScale = rootNegativeScalingAdjustment;

        //========= Remove Colliders =======================
        MeshUtilitiesAFWB.UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
        MeshUtilitiesAFWB.SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
        if (log)
            Debug.Log("Created new user Rail:  " + rootGO.name);

        return rootGO;
    }
}