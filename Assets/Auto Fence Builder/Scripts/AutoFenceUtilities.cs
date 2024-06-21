using MeshUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
//using UnityEditor.Presets;
using UnityEngine;

namespace AFWB
{
    public class AFWBOutOfRangeException : Exception
    {
        public float Max { get; private set; }
        public float Min { get; private set; }
        public int PostCount { get; private set; }
        public LayerSet Layer { get; private set; }
        private readonly string exceptionMessage; // Private field for storing the message

        public AFWBOutOfRangeException(string exceptionMessage, float min, float max, int postCount = 0, LayerSet layer = LayerSet.allLayer)
            : base(exceptionMessage) // Pass the message to the base Exception constructor
        {
            Min = min;
            Max = max;
            PostCount = postCount;
            Layer = layer;
            this.exceptionMessage = exceptionMessage; // Store the custom message
        }

        // Override the Message property to return the custom message
        public override string Message => exceptionMessage;
    }

    public enum UserObjectImportOptions { Match = 0, Auto, Raw }


    public partial class AutoFenceCreator
    {
        /// <summary>Calculates the distance between posts in a list of post positions.</summary>
        /// <param menuName="allPostPositionsIndex">The index of the current post in the list of allPostPositions.</param>
        /// <returns>The distance to the next post if not the last in the list, or to the previous if it is the last.</returns>
        /// <remarks>This method assumes that the list of positions is ordered and contains no null entries.</remarks>
        public float CheckNodePositions()
        {
            // ensure no 2 posts in allPostPositions are cloaser than 0.2f
            // if they are, get their direction vectors 
            // and move the second post to be further away away from the first post, in the direction of 1st -> 2nd
            // then check again
            bool changed = false;
            float min = 1000000;
            for (int i = 0; i < clickPoints.Count - 1; i++)
            {
                for (int j = i + 1; j < clickPoints.Count; j++)
                {
                    Vector3 currPos = clickPoints[i], nextPos = clickPoints[j];
                    float dist = Vector3.Distance(currPos, nextPos);
                    if (dist < min)
                        min = dist;
                    if (dist < minInterPostDist)
                    {
                        //-- The direction from curr towards next
                        Vector3 dir = (nextPos - currPos).normalized;
                        clickPoints[j] = currPos + dir * minInterPostDist;
                        changed = true;
                    }
                }
            }
            if (changed)
                ForceRebuildFromClickPoints();
            else
                return min;
            return min;

        }
        //------------------------------
        /// <summary>Calculates the distance between consecutive posts based on their positions in a list.</summary>
        /// <param menuName="allPostPositionsIndex">The index of the current post in the list of all post positions.</param>
        /// <returns>Returns the distance to the next post if it is not the last one in the list, or the distance to the previous post if it is the last.</returns>
        /// <remarks>Assumes that the allPostPositions list is ordered and contains no null entries. This method does not perform validation checks on the list.</remarks>
        public float GetSectionLength(int allPostPositionsIndex)
        {
            float dist = 0;
            // Get the position of the post in the allPostPositions list
            Vector3 currPos = allPostPositions[allPostPositionsIndex];
            if (allPostPositionsIndex < allPostPositions.Count - 1)
            {
                Vector3 nextPos = allPostPositions[allPostPositionsIndex + 1];
                dist = Vector3.Distance(currPos, nextPos);
            }
            else
            {
                Vector3 prevPos = allPostPositions[allPostPositionsIndex - 1];
                dist = Vector3.Distance(currPos, prevPos);
            }
            return dist;
        }
        public void SwitchToolbarComponentView(LayerSet layer)
        {
            switch (layer)
            {
                case LayerSet.railALayer:
                    componentToolbar = ComponentToolbar.railsA;
                    break;
                case LayerSet.railBLayer:
                    componentToolbar = ComponentToolbar.railsB;
                    break;
                case LayerSet.postLayer:
                    componentToolbar = ComponentToolbar.posts;
                    break;
                case LayerSet.extraLayer:
                    componentToolbar = ComponentToolbar.extras;
                    break;
                case LayerSet.subpostLayer:
                    componentToolbar = ComponentToolbar.subposts;
                    break;
                default:
                    return;
            }
        }
        //----------------------------
        private void PrintClickPoints()
        {
            for (int i = 0; i < clickPoints.Count; i++)
            {
                Debug.Log(clickPoints[i] + "\n");
            }

        }
        //----------------------------
        public Vector3 GetScaleTransformForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return railAScale;
            else if (layer == LayerSet.railBLayer)
                return railBScale;
            else if (layer == LayerSet.extraLayer)
                return ex.extraTransformScale;
            else if (layer == LayerSet.subpostLayer)
                return subpostScale;
            return postScale;
        }
        //----------------------------
        public Vector3 GetPositionTransformForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return railAPositionOffset;
            else if (layer == LayerSet.railBLayer)
                return railBPositionOffset;
            else if (layer == LayerSet.extraLayer)
                return ex.extraTransformPositionOffset;
            else if (layer == LayerSet.subpostLayer)
                return subpostPositionOffset;
            return subpostPositionOffset;
        }
        //--------------------------------

        public Vector3 GetRotationTransformForLayer(LayerSet layer)
        {
            if (layer == LayerSet.postLayer)
                return postRotation;
            else if (layer == LayerSet.railALayer)
                return railARotation;
            else if (layer == LayerSet.railBLayer)
                return railBRotation;
            else if (layer == LayerSet.subpostLayer)
                return subpostRotation;
            else if (layer == LayerSet.extraLayer)
                return ex.extraTransformRotation;

            return Vector3.zero;
        }

        public void SetScaleTransformForLayer(Vector3 scale, LayerSet layer)
        {
            if (layer == LayerSet.postLayer)
                postScale = scale;
            else if (layer == LayerSet.railALayer)
                railAScale = scale;
            else if (layer == LayerSet.railBLayer)
                railBScale = scale;
            else if (layer == LayerSet.subpostLayer)
                subpostScale = scale;
            else if (layer == LayerSet.extraLayer)
                ex.extraTransformScale = scale;
        }
        public void SetPositionTransformForLayer(Vector3 positionOffset, LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                railAPositionOffset = positionOffset;
            else if (layer == LayerSet.railBLayer)
                railBPositionOffset = positionOffset;
            else if (layer == LayerSet.extraLayer)
                ex.extraTransformPositionOffset = positionOffset;
            else if (layer == LayerSet.subpostLayer)
                subpostPositionOffset = positionOffset;

        }
        public void SetRotationTransformForLayer(Vector3 rotation, LayerSet layer)
        {
            if (layer == LayerSet.postLayer)
                postRotation = rotation;
            else if (layer == LayerSet.railALayer)
                railARotation = rotation;
            else if (layer == LayerSet.railBLayer)
                railBRotation = rotation;
            else if (layer == LayerSet.subpostLayer)
                subpostRotation = rotation;
            else if (layer == LayerSet.extraLayer)
                ex.extraTransformRotation = rotation;

        }


        //--------------------------------------------
        public static AutoFenceCreator GetAutoFenceCreator()
        {
            GameObject selectedGameObject = Selection.activeGameObject;
            AutoFenceCreator af = null;

            if (selectedGameObject != null)
                af = selectedGameObject.GetComponent<AutoFenceCreator>();
            if (af == null)
            {
                af = GameObject.Find("Auto Fence Builder").GetComponent<AutoFenceCreator>();
                if (af != null)
                    return af;
            }
            if (af == null)
                Debug.LogError("AutoFenceCreator component not found on the selected GameObject.");
            return af;
        }
        //---------------------------
        public List<string> CheckMasks()
        {
            LayerMask layerMask = GetIgnoreLayerMask();
            List<string> maskedLayers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                if (layerMask == (layerMask | (1 << i)))
                {
                    //Debug.Log("Layer " + stackIdx + " " + LayerMask.LayerToName(stackIdx) + " is in the mask");
                    maskedLayers.Add(LayerMask.LayerToName(i));
                }
            }
            return maskedLayers;
        }

        //--------------------------------------------

        //=========================================
        //          Clean Names
        //=========================================
        //- Needs tidying up
        public static string StripPanelRailFromNameStatic(string name)
        {
            string[] suffixes = { "_Panel_Rail", "_Rail", "_Panel" };
            foreach (var suffix in suffixes)
            {
                if (name.EndsWith(suffix)) return name.Substring(0, name.LastIndexOf(suffix));
            }
            return name;
        }

        public string StripPrefabTypeFromNameForType(string name, PrefabTypeAFWB prefabType)
        {
            if (prefabType == PrefabTypeAFWB.railPrefab)
                return StripPanelRailFromName(name);
            else if (prefabType == PrefabTypeAFWB.postPrefab)
                return StripPostFromName(name);
            else if (prefabType == PrefabTypeAFWB.extraPrefab)
                return StripExtraFromName(name);
            return name;
        }
        public string StripPanelRailFromName(string name)
        {
            return StripPanelRailFromNameStatic(name);
        }
        public string StripPostFromName(string name)
        {
            return StripPostFromNameStatic(name);
        }
        public static string StripPostFromNameStatic(string name)
        {
            string baseName = name;
            int postPos = baseName.LastIndexOf("_Post");
            if (postPos != -1)
                baseName = baseName.Substring(0, postPos);
            return baseName;
        }
        public string StripExtraFromName(string name)
        {
            return StripExtraFromNameStatic(name);
        }
        public static string StripExtraFromNameStatic(string name)
        {
            string baseName = name;
            int extraPos = baseName.LastIndexOf("_Extra");
            if (extraPos != -1)
                baseName = baseName.Substring(0, extraPos);
            //we can't be sure it's Extra, maybe using a post ot rail
            else
            {
                baseName = StripPostFromNameStatic(name);
                if (baseName == name)
                    baseName = StripPanelRailFromNameStatic(name);
            }
            return baseName;
        }
        public static string StripLayerTypeFromNameStatic(string name)
        {
            string baseName = "";
            if (name.EndsWith("Rail") || name.EndsWith("Panel") || name.EndsWith("Panel_Rail"))
                baseName = StripPanelRailFromNameStatic(name);
            else if (name.EndsWith("Post"))
                baseName = StripPostFromNameStatic(name);
            else if (name.EndsWith("Extra"))
                baseName = StripExtraFromNameStatic(name);
            return baseName;
        }
        public static string StripLayerTypeFromNameStatic(LayerSet layer, string name)
        {
            string baseName = "";
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                baseName = StripPanelRailFromNameStatic(name);
            else if (layer == LayerSet.postLayer || layer == LayerSet.subpostLayer)
                baseName = StripPostFromNameStatic(name);
            else if (layer == LayerSet.extraLayer)
                baseName = StripExtraFromNameStatic(name);
            return baseName;
        }
        public static string StripCategoryFromName(string name)
        {
            //Find the first occurence of '/' and return the string after it
            int index = name.IndexOf("/");
            if (index != -1)
                return name.Substring(index + 1, name.Length - (index + 1));

            return name;
        }
        public static bool NameIsAutoFencePart(string name)
        {
            return StringUtilsTCT.StringContainsAutoFencePart(name);
        }


        //=========================================================================

        public void CheckClickPointsForIgnoreLayers()
        {
            /*SetIgnorePartsColliders(true);
            RaycastHit hit;
            LayerMask layerMask = GetIgnoreLayerMask();

            for (int stackIdx = 0; stackIdx < clickPoints.Count; stackIdx++)
            {

                Vector3 rayPosition = clickPoints[stackIdx];
                rayPosition.y += 100;
                bool hitIgnore = false;
                // Test if we were trying to click on a layer that'stackIdx set to be ignored, note re-inversion of mask with ~


                if (Physics.Raycast(rayPosition, Vector3.down, out hit, 1000.0f, ~layerMask))
                {
                    clickPoints[stackIdx] = hit.point;
                    //Debug.Log("hit instead " + hit.transform.gameObject.menuName);
                }

            }
            SetIgnorePartsColliders(false);*/
        }
        public void CheckAllPostPositionsForIgnoreLayers()
        {
            /*RaycastHit hit;
            LayerMask layerMask = GetIgnoreLayerMask();
            SetIgnorePartsColliders(true);
            for (int stackIdx = 0; stackIdx < allPostPositions.Count; stackIdx++)
            {
                Vector3 rayPosition = allPostPositions[stackIdx];
                rayPosition.y += 100;
                bool hitIgnored = false;

                if (Physics.Raycast(rayPosition, Vector3.down, out hit, 1000.0f, ~layerMask))
                {
                    allPostPositions[stackIdx] = hit.point;
                }
            }
            SetIgnorePartsColliders(false);
            CheckMasks();*/
        }

        //----------------------------
        /*public void SetPrefabFavorite(string prefabName, bool status)
        {
            if (prefabDetails.ContainsKey(prefabName))
            {
                PrefabDetailsStruct pd = prefabDetails[prefabName];
                pd.isFavorite = status;
                prefabDetails[prefabName] = pd;
            }
        }*/
        //----------------------------
        /*public void TogglePrefabFavorite(string prefabName)
        {
            if (prefabDetails.ContainsKey(prefabName))
            {
                PrefabDetailsStruct pd = prefabDetails[prefabName];
                pd.isFavorite = !pd.isFavorite;
                prefabDetails[prefabName] = pd;
            }
        }*/
        //----------------------------
        /* public void SetPrefabProtected(string prefabName, bool status)
         {
             if (prefabDetails.ContainsKey(prefabName))
             {
                 PrefabDetailsStruct pd = prefabDetails[prefabName];
                 pd.isProtected = status;
                 prefabDetails[prefabName] = pd;
             }
         }*/
        //----------------------------
        /*public void TogglePrefabProtected(string prefabName)
         {
             if (prefabDetails.ContainsKey(prefabName))
             {
                 PrefabDetailsStruct pd = prefabDetails[prefabName];
                 pd.isProtected = !pd.isProtected;
                 prefabDetails[prefabName] = pd;
             }
         }*/
        public void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        //------------------
        // Make Copies of ALL possible mesh data in case any of it gets mangled
        // These are saved in Lists of GO Lists, as each GO could have children
        public void BackupPrefabMeshes(List<GameObject> sourcePrefabs, List<List<Mesh>> destinationMeshes)
        {
            destinationMeshes.Clear();
            List<Mesh> submeshList;
            for (int i = 0; i < sourcePrefabs.Count(); i++)
            {
                if (sourcePrefabs[i] == null)
                {
                    Debug.LogWarning("curr inPrefab was missing.Index: " + i + " in BackupPrefabMeshes \n");
                    continue;
                }
                submeshList = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(sourcePrefabs[i]);
                if (submeshList.Count == 0)
                    Debug.Log($"Couldn't find mesh for Prefab {sourcePrefabs[i]}.    In GetRailMeshesFromPrefabs() ");

                destinationMeshes.Add(submeshList);
            }
        }
        // Finds the minimum section length between two posts
        public float FindMinimumPostToPostDistance()
        {
            if (allPostPositions == null || allPostPositions.Count < 2)
            {
                Debug.LogError("The list must contain at least two positions.");
                return -1f; // Return a negative value to indicate an error
            }
            float minDistance = float.MaxValue;

            for (int i = 1; i < allPostPositions.Count; i++)
            {
                float distance = Vector3.Distance(allPostPositions[i - 1], allPostPositions[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            return minDistance;
        }
        //

        //-------------
        /*public List<SourceVariant> CreateUsedVariantsList(LayerSet layer)
        {
            List<SourceVariant> uniqueVaraints = null;
            if (layer == LayerSet.railALayer)
            {
                nonNullRailSourceVariants[0] = CreateUniqueVariantList(railSourceVariants[0]);
                return nonNullRailSourceVariants[0];
            }
            else if (layer == LayerSet.railBLayer)
            {
                nonNullRailSourceVariants[1] = CreateUniqueVariantList(railSourceVariants[0]);
                return nonNullRailSourceVariants[1];
            }
            else if (layer == LayerSet.postLayer)
            {
                nonNullPostVariants = CreateUniqueVariantList(postSourceVariants);
                return nonNullPostVariants;
            }
            else if (layer == LayerSet.subpostLayer)
            {
                nonNullSubpostVariants = CreateUniqueVariantList(subpostSourceVariants);
                return nonNullSubpostVariants;
            }

            return null;
        }*/
        //--------------------
        public List<SourceVariant> CreateUniqueVariantList(List<SourceVariant> sourceVariantList)
        {
            List<SourceVariant> uniqueList = SourceVariant.CreateInitialisedSourceVariantList();

            GameObject mainGO = sourceVariantList[0].Go;

            foreach (var source in sourceVariantList)
            {
                bool found = false;
                foreach (var dest in uniqueList)
                {
                    if (source.Go == dest.Go)
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    uniqueList.Add(source);
                }
            }
            return uniqueList;
        }

        //-------------
        public void CheckStatusOfAllClickPoints()
        {
            for (int i = 0; i < postsBuiltCount + 1; i++)
            {
                if (clickPoints.Contains(postsPool[i].position))
                {
                    int index = clickPoints.IndexOf(postsPool[i].position);
                    if (postsPool[i].gameObject.activeInHierarchy == false)
                    {
                        DeleteClickPoint(index);
                    }
                }
            }
        }
        public int GetVariationIndexFromGoName(GameObject thisGO)
        {
            int currVariationIndex;
            string last4CharsBeforeSq = thisGO.name.Substring(thisGO.name.Length - 5);
            if (last4CharsBeforeSq.StartsWith("_sq"))
                thisGO.name = thisGO.name.Remove(thisGO.name.Length - 5);

            string variationStr = "" + thisGO.name[thisGO.name.Length - 2];
            currVariationIndex = int.Parse(variationStr);
            return currVariationIndex;
        }

        //================================================================
        private List<MeshFilter> JitterGameObjectVerts(List<MeshFilter> mfList)
        {
            jitterAmountRail = new Vector3(0.5f, 0.5f, 0.5f);
            int meshCount = mfList.Count;
            for (int m = 0; m < meshCount; m++)
            {
                Mesh thisModdedMesh = mfList[m].sharedMesh;
                thisModdedMesh = MeshUtilitiesAFWB.AddRandomVertexOffsets(thisModdedMesh, jitterAmountRail * 0.05f);
                mfList[m].sharedMesh = thisModdedMesh;
            }
            return mfList;
        }
        //================================================================
        void JitterGameObjectVerts(GameObject go)
        {

        }
        //================================================================
        void DestroyBoxCollider(GameObject go)
        {
            BoxCollider boxCollider = go.GetComponent<BoxCollider>();
            if (boxCollider != null)
                DestroyImmediate(boxCollider);
        }

        //================================================================
        private bool OmitBuriedRails(Vector3 currentVectorDir, float distance, Vector3 size, bool omit, GameObject thisRail)
        {
            RaycastHit hit;
            int origLayer = thisRail.gameObject.layer;
            thisRail.gameObject.layer = 2; //raycast ignore colliders, we turn it on again at the end
            if (keepInterpolatedPostsGrounded && autoHideBuriedRails)
            {
                float bottom = GetMeshMin(thisRail).y;
                bottom *= size.y;
                Vector3 rayPosition = thisRail.transform.position + new Vector3(0, bottom * 0.8f, 0); // bottom * 0.8 tolerance can be adjusted
                if (Physics.Raycast(rayPosition, currentVectorDir, out hit, distance))
                {
                    if (hit.collider.gameObject.name.Contains("_Rail") == false && hit.collider.gameObject.name.Contains("_Post") == false
                        && hit.collider.gameObject.name.Contains("_Extra") == false
                       && hit.collider.gameObject.name.StartsWith("FenceManagerMarker") == false)
                    {
                        thisRail.hideFlags = HideFlags.HideInHierarchy;
                        thisRail.SetActive(false);
                        omit = true;
                    }
                }
            }
            thisRail.gameObject.layer = origLayer; //restore layer
            return omit;
        }

        //------------------------
        public void PrintSeqItem(SeqItem seqVar, LayerSet layer)
        {
            seqVar.GetSourceVariantGO(this, layer);


            Debug.Log(" SourceVariant Index = " + seqVar.sourceVariantIndex + "    Invert = " + seqVar.invert +
                "      BackToFront =  = " + seqVar.backToFront + "      Mirror Z =  = " + seqVar.mirrorZ);
            //Debug.Log("Mirror X =  = " + sv.mirrorX);

        }
        //===================================================
        public Mesh CombineRailMeshes()
        {
            CombineInstance[] combiners = new CombineInstance[railABuiltCount];

            for (int i = 0; i < railABuiltCount; i++)
            {

                GameObject thisRail = railsAPool[i].gameObject;
                MeshFilter mf = thisRail.GetComponent<MeshFilter>();
                Mesh mesh = (Mesh)Instantiate(mf.sharedMesh);

                Vector3[] vertices = mesh.vertices;
                Vector3[] newVerts = new Vector3[vertices.Length];
                int v = 0;
                while (v < vertices.Length)
                {

                    newVerts[v] = vertices[v];
                    v++;
                }
                mesh.vertices = newVerts;

                combiners[i].mesh = mesh;

                Transform finalTrans = Instantiate(thisRail.transform) as Transform;
                finalTrans.position += thisRail.transform.parent.position;
                combiners[i].transform = finalTrans.localToWorldMatrix;
                DestroyImmediate(finalTrans.gameObject);
            }

            Mesh finishedMesh = new Mesh();
            finishedMesh.CombineMeshes(combiners);

            return finishedMesh;
        }
        //==================================================
        //      Handle User's Parts (Delicately)
        //==================================================
        public GameObject HandleUserExtraChange(GameObject newUserExtra)
        {
            //Creates a cleaned up GameObject with any children
            userPrefabExtra = MeshUtilitiesAFWB.CreateAFBExtraFromGameObject(newUserExtra);
            if (userPrefabExtra != null)
                userPrefabExtra.name = newUserExtra.name;
            return userPrefabExtra;
        }
        //--------------------------------
        /*public GameObject HandleUserPostChange(GameObject newUserPost)
        {
            autoRotationResults = Vector3.zero;
            //Creates a cleaned up GameObject with any children
            userPostObject = MeshUtilitiesAFWB.CreateCleanUncombinedAFBPostFromGameObject(newUserPost, this);
            if (userPostObject != null)
                userPostObject.menuName = newUserPost.menuName;
            return userPostObject;
        }*/
        //--------------------------------
        public GameObject HandleUserRailChange(GameObject newUserRail)
        {
            autoRotationResults = Vector3.zero;
            userSubMeshRailOffsets = new List<float>(); // used in MeshUtilitiesAFB and during rails build

            //Creates a cleaned up GameObject with any children
            userPrefabRail[kRailALayerInt] = MeshUtilitiesAFWB.CreateCleanUncombinedAFBRailFromGameObject(newUserRail, this, GetCurrentPrefabForLayer(LayerSet.railALayer));
            if (userPrefabRail[kRailALayerInt] != null)
                userPrefabRail[kRailALayerInt].name = newUserRail.name;
            return userPrefabRail[kRailALayerInt];
        }
        //---------------------------------------------
        /*void CreateInterpolatedParallelNodes(List<ParallelNode> parallelList, int numRows, int numColumns)
        {
            if (parallelList == null || parallelList.Count == 0 || numRows == 0 || numColumns == 0)
                return;

            ParallelNode parallelNode = new ParallelNode();
            int num = parallelList.Count;
            float width = parallelList[0].width;
            float startWidthOffset = width * -0.5f;
            float widthStride = width / (numColumns - 1);

            parallelList.Clear();


            for (int stackIdx = 0; stackIdx < num; stackIdx++)
            {

               //parallelNodes.Add(parallelNode);
            }
        }*/

        //---------------------------------------------
        void CreateParallelNodesList(float inWidth = 2.0f, CornerDirectionMode elbowMode = CornerDirectionMode.averageDirection)
        {
            /*ParallelNode parallelNode = new ParallelNode(), prevParallelNode = new ParallelNode();
            parallelNode.width = inWidth;
            NodeInfo thisNode, prevNode, nextNode;
            int num = nodeDirections.Count;
            float halfWidth = inWidth * 0.5f;
            parallelNodes.Clear();
            float turnAngle = 0, innerAngle = 0, breakMiterAngle = 45;
            for (int stackIdx = 0; stackIdx < num; stackIdx++)
            {
                thisNode = nodeDirections[stackIdx];
                parallelNode.nodeDirection = thisNode;
                parallelNode.center = thisNode.position;
                parallelNode.left = thisNode.position + (thisNode.dirLeft * halfWidth);
                parallelNode.right = thisNode.position + (thisNode.dirRight * halfWidth);
                parallelNode.isInsert = false; parallelNode.excludeLeft = false; parallelNode.excludeRight = false;

                turnAngle = 0;
                if (stackIdx != 0 && stackIdx != num - 1)
                {
                    parallelNode.elbowMode = elbowMode;
                    prevNode = nodeDirections[stackIdx - 1];

                    //Vector3 prevNodePos = prevNode.position;

                    nextNode = nodeDirections[stackIdx + 1];
                    float miterWidth = VectorUtilitiesTCT.GetWidthAtElbow(prevNode.position, thisNode.position, nextNode.position, inWidth);
                    float halfMiterWidth = miterWidth * 0.5f;
                    turnAngle = VectorUtilitiesTCT.GetCornerAngle(prevNode.position, thisNode.position, nextNode.position);
                    turnAngle = Mathf.Abs(turnAngle);
                    innerAngle = Mathf.Abs(180 - turnAngle);
                    //Debug.Log(angle);

                    //parallelNode.left = thisNode.position + (prevNode.dirLeft * halfWidth);
                    //parallelNode.right = thisNode.position + (prevNode.dirRight * halfWidth);

                    //-- Pre Split
                    parallelNode = CreateCornerNodeSplit(parallelNode, prevNode, nextNode, halfWidth, innerAngle, breakMiterAngle, 0);

                    //Mid
                    parallelNode.excludeLeft = parallelNode.excludeRight = false;
                    Vector3 avgDirLeft = ((thisNode.dirLeft + prevNode.dirLeft) / 2).normalized * halfMiterWidth;
                    Vector3 avgDirRight = ((thisNode.dirRight + prevNode.dirRight) / 2).normalized * halfMiterWidth;
                    parallelNode.avgPosLeft = thisNode.position + avgDirLeft;
                    parallelNode.avgPosRight = thisNode.position + avgDirRight;
                    if (innerAngle <= breakMiterAngle)
                    {
                        // for the mid point we exclude the outer node (ooposite side of the corner)
                        if (thisNode.cornerOrientation == CornerOrientation.cornerRight)
                            parallelNode.excludeLeft = true;
                        if (thisNode.cornerOrientation == CornerOrientation.cornerLeft)
                            parallelNode.excludeRight = true;
                    }
                    parallelNodes.Add(parallelNode);

                    //-- Post Split
                    parallelNode = CreateCornerNodeSplit(parallelNode, prevNode, nextNode, halfWidth, innerAngle, breakMiterAngle, 1);
                }
                else
                {
                    parallelNode.avgPosLeft = parallelNode.left;
                    parallelNode.avgPosRight = parallelNode.right;
                    parallelNodes.Add(parallelNode);
                }
                prevParallelNode = parallelNode;
            }*/
        }
        //--------------
        /*private ParallelNode CreateCornerNodeSplit(ParallelNode parallelNode, NodeInfo prevNode, NodeInfo nextNode, float halfWidth, float innerAngle, float breakMiterAngle, int prePost)
        {
            if (innerAngle <= breakMiterAngle)
            {
                NodeInfo thisNode = parallelNode.nodeDirection;

                Vector3 nodeWithPrevDirLeft = Vector3.zero, nodeWithPrevDirRight = Vector3.zero;
                if (prePost == 0)
                {
                    nodeWithPrevDirLeft = thisNode.position + (prevNode.dirLeft * halfWidth);
                    nodeWithPrevDirRight = thisNode.position + (prevNode.dirRight * halfWidth);
                    parallelNode.avgPosLeft = nodeWithPrevDirLeft + (prevNode.forward * halfWidth);
                    parallelNode.avgPosRight = nodeWithPrevDirRight + (prevNode.forward * halfWidth);
                }
                else
                {
                    nodeWithPrevDirLeft = thisNode.position + (thisNode.dirLeft * halfWidth);
                    nodeWithPrevDirRight = thisNode.position + (thisNode.dirRight * halfWidth);
                    parallelNode.avgPosLeft = nodeWithPrevDirLeft - (thisNode.forward * halfWidth);
                    parallelNode.avgPosRight = nodeWithPrevDirRight - (thisNode.forward * halfWidth);
                }


                //-- Mark for exclusion
                parallelNode.excludeLeft = parallelNode.excludeRight = false;
                if (thisNode.cornerOrientation == CornerOrientation.cornerRight)
                    parallelNode.excludeRight = true;
                else if (thisNode.cornerOrientation == CornerOrientation.cornerLeft)
                    parallelNode.excludeLeft = true;

                parallelNode.isInsert = true;
                parallelNodes.Add(parallelNode);
            }
            return parallelNode;
        }*/
        //-------------
        public ParallelNode GetNextNonExcludedNode(List<ParallelNode> parallelList, int n, CornerOrientation c)
        {
            for (int i = n + 1; i < parallelList.Count; i++)
            {
                if (c == CornerOrientation.cornerRight && parallelList[i].excludeRight == false)
                    return parallelList[i];
                else if (c == CornerOrientation.cornerLeft && parallelList[i].excludeLeft == false)
                    return parallelList[i];
            }

            return parallelList[n];
        }
        //---------------------------
        /*public void CheckPresetManager(bool loadPartsAlso = false)
        {//Debug.Log("CheckPresetManager()");

            if (presetManager == null)
            {
                presetManager = new AFBPresetManager();
                presetManager.afb = this;
                ///if(loadPartsAlso == true && postPrefabs.Count == 0)
                ///LoadAllParts();
                presetManager.ReadPresetFiles();
            }
        }*/
        //--------------------------
        // Tidy everything up so the folder handles and parts are in the right place
        public void RepositionFinished(GameObject finishedFolder)
        {
            int numCategoryChildren = finishedFolder.transform.childCount;
            Vector3 finishedFolderPosition = finishedFolder.transform.position;
            Transform categoryChild, groupedChild, meshChild;
            for (int k = 0; k < numCategoryChildren; k++)
            {
                categoryChild = finishedFolder.transform.GetChild(k);
                if (categoryChild.name == "Posts" || categoryChild.name == "Rails" || categoryChild.name == "Subs" || categoryChild.name == "Extras")
                {
                    categoryChild.position = finishedFolderPosition;
                    int numGroupChildren = categoryChild.childCount;
                    for (int i = 0; i < numGroupChildren; i++)
                    {
                        groupedChild = categoryChild.GetChild(i);
                        if (groupedChild.name.StartsWith("PostsGroupedFolder") || groupedChild.name.StartsWith("RailsAGroupedFolder")
                            || groupedChild.name.StartsWith("RailsBGroupedFolder") || groupedChild.name.StartsWith("SubsGroupedFolder") || groupedChild.name.StartsWith("ExtrasGroupedFolder"))
                        {
                            int numMeshChildren = groupedChild.childCount;
                            for (int j = 0; j < numMeshChildren; j++)
                            {
                                meshChild = groupedChild.GetChild(j);
                                if (meshChild.name.StartsWith("Post") || meshChild.name.StartsWith("Rail") || meshChild.name.StartsWith("Sub")
                                    || meshChild.name.Contains("_Extra") || meshChild.name.Contains("_Post") || meshChild.name.Contains("_Rail"))
                                    meshChild.position -= (finishedFolderPosition);
                            }
                        }
                    }
                }
            }
        }
        //=====================================================
        //                  Copy Layout & Clone
        //=====================================================
        public void CopyLayoutFromOtherFence(bool rebuild = true, GameObject sourceFence = null)
        {
            if (sourceFence == null)
                sourceFence = fenceToCopyFrom; //afc class variable
            List<Vector3> copiedClickPoints = null;
            List<Vector3> copiedGapPoints = null;
            if (fenceCloner == null)
                fenceCloner = new FenceCloner();
            if (sourceFence != null)
            {
                copiedClickPoints = fenceCloner.GetClickPointsFromFence(sourceFence);
                copiedGapPoints = fenceCloner.GetGapPointsFromFence(sourceFence);
            }
            if (copiedClickPoints != null)
            {
                ClearAllFences();
                int numClickPoints = copiedClickPoints.Count;
                for (int i = 0; i < numClickPoints; i++)
                {
                    //print(copiedClickPoints[stackIdx]); 
                    clickPoints.Add(copiedClickPoints[i]);
                    clickPointFlags.Add(0); // 0 if normal, 1 if break
                    keyPoints.Add(copiedClickPoints[i]);
                }

                for (int i = 0; i < numClickPoints; i++)
                {
                    for (int j = 0; j < copiedGapPoints.Count - 1; j++)
                    {
                        if (clickPoints[i] == copiedGapPoints[j])
                        {
                            clickPointFlags[i + 1] = 1;
                            break;
                        }
                    }
                }

                ForceRebuildFromClickPoints();
            }
        }
        public void CopyLayoutFromScriptableClickPoints(List<Vector3> sourceClickPoints, List<Vector3> sourceGapPoints, bool rebuild = true)
        {
            if (sourceClickPoints != null)
            {
                ClearAllFences();
                int numClickPoints = sourceClickPoints.Count;
                for (int i = 0; i < numClickPoints; i++)
                {
                    //print(copiedClickPoints[stackIdx]); 
                    clickPoints.Add(sourceClickPoints[i]);
                    clickPointFlags.Add(0); // 0 if normal, 1 if gap
                    keyPoints.Add(clickPoints[i]);
                }

                for (int i = 0; i < numClickPoints; i++)
                {
                    for (int j = 0; j < sourceGapPoints.Count; j++)
                    {
                        if (clickPoints[i] == sourceGapPoints[j])
                        {
                            clickPointFlags[i] = 1;
                            break;
                        }
                    }
                }
                /*if (sourceGapPoints != null)
                {
                    ClearAllFences();
                    int numGapPoints = sourceGapPoints.Count;
                    for (int stackIdx = 0; stackIdx < numGapPoints; stackIdx++)
                    {
                        //print(copiedClickPoints[stackIdx]); 
                        gaps.Add(sourceClickPoints[stackIdx]);
                        clickPointFlags.Add(0); // 0 if normal, 1 if break
                        keyPoints.Add(clickPoints[stackIdx]);
                    }

                    for (int stackIdx = 0; stackIdx < numClickPoints; stackIdx++)
                    {
                        for (int j = 0; j < sourceGapPoints.Count - 1; j++)
                        {
                            if (clickPoints[stackIdx] == sourceGapPoints[j])
                            {
                                clickPointFlags[stackIdx + 1] = 1;
                                break;
                            }
                        }
                    }
                }*/
                ForceRebuildFromClickPoints();
            }
        }
        //-----------------------
        public void RemoveSingleVariantFromList(LayerSet layerSet, int sectionIndex)
        {
            List<SinglesItem> singleVariants = singlesContainer.GetSingleVariantsForLayer(layerSet, af);
            SinglesItem singleVar = singleVariants[sectionIndex];
            if (singleVar != null)
                singleVar.Init();
            else
                Debug.LogWarning($"Tried To remove a null Single in RemoveSingleVariantFromList({layerSet.ToString()}  {sectionIndex})\n");
        }

        //-----------------------
        public void SetAllSourceVariantsToPrefab(LayerSet layer, GameObject go)
        {
            for (int i = 0; i < kMaxNumSourceVariants; i++)
            {
                SetSourceVariantGoAtIndexForLayer(i, go, layer);
            }
        }
        //----------------------------------------
        // Called from AutoFenceCreator Awake()
        public void InitializeSequencesAndSingles()
        {
            /*StackLog(this.GetType().Name);
            if (listsAndArraysCreated)
                return;

            for (int stackIdx = 0; stackIdx < kMaxNumSeqSteps; stackIdx++)
            {
                _SeqItemListRailA[stackIdx] = new SeqItem();//default
                SeqItemListRailB[stackIdx] = new SeqItem();
                SeqItemListPost[stackIdx] = new SeqItem();
                //userSequenceSubpost[stackIdx] = new SeqItem();
            }
            ResetAllRailSingles();
            listsAndArraysCreated = true;*/
        }
        //----
        public void ResetDefaultDirectories()
        {
            currAutoFenceBuilderDir = autoFenceBuilderDefaultDir;
            currPrefabsDir = prefabsDefaultDir;
            currExtraPrefabsDir = extrasDefaultDir;
            currPostPrefabsDir = postsDefaultDir;
            currRailPrefabsDir = railsDefaultDir;
            currMeshesDir = currAutoFenceBuilderDir + "/AFWBMeshes";
            currPresetsDir = presetsDefaultFilePath;
        }
        //----------------------------
        public string GetPresetNameWithoutCategory(string presetName)
        {
            string name = presetName;

            if (name.Contains("/"))
            {
                int index = name.IndexOf("/");
                name = presetName.Substring(index + 1, (presetName.Length) - (index + 1));
            }
            return name;
        }
        //--------------------------
        public Sequencer GetSequencerForLayer(LayerSet layer)
        {
            Sequencer sequencer = null;
            if (layer == LayerSet.railALayer)
                sequencer = railASequencer;
            else if (layer == LayerSet.railBLayer)
                sequencer = railBSequencer;
            else if (layer == LayerSet.postLayer)
                sequencer = postSequencer;
            else
                Debug.Log($"layer {layer.String()} has no sequencer in GetSequencerForLayer()");

            if (sequencer == null)
                Debug.Log($"layer {layer.String()} sequencer is null in GetSequencerForLayer()");

            return sequencer;
        }

        //--------------------------
        /*public int FindPrefabIndexInMenuNamesList(PrefabType prefabType, string prefabName, bool warnMissing = true)
        {
            List<string> namesList = postMenuNames;
            if (prefabType == PrefabTypeAFWB.railPrefab)
                namesList = railMenuNames;
            if (prefabType == PrefabTypeAFWB.extraPrefab)
                namesList = extraMenuNames;
            //Its a Post or Railor Extra inPrefab, look for it in their lists
            if (prefabType != PrefabTypeAFWB.anyPrefab)
            {
                for (int stackIdx = 0; stackIdx < namesList.Count; stackIdx++)
                {
                    if (namesList[stackIdx] == null)
                        continue;
                    string menuName = namesList[stackIdx];
                    if (menuName.Contains(prefabName))
                        return stackIdx;
                }
            }
            //if(warnMissing)
            // re-enable print ("Couldn't find inPrefab with menuName: " + prefabName + ".Is it a User Object that'stackIdx been deleted? currPost default inPrefab will be used instead.\n");
            return 0;
        }*/
        //---------------------------
        public string GetShortPartialTimeString(bool includeDate = false)
        {
            DateTime currentDate = System.DateTime.Now;
            string timeString = currentDate.ToString();
            timeString = timeString.Replace("/", ""); // because the / in that will upset the path
            timeString = timeString.Replace(":", ""); // because the / in that will upset the path
            if (timeString.EndsWith(" AM") || timeString.EndsWith(" PM"))
            { // windows??
                timeString = timeString.Substring(0, timeString.Length - 3);
            }
            if (includeDate == false)
                timeString = timeString.Substring(timeString.Length - 8);

            // we don't need the '20' part of the year so remove it
            timeString = timeString.Remove(4, 2);

            return timeString;
        }

        //---------------------------
        public string GetPartialTimeString(bool includeDate = false)
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

            // we don't need the '20' part of the year so remove it
            timeString = timeString.Remove(6, 2);

            return timeString;
        }
        //---------------------------
        // custom, beacuse by default .ToString() only writes 1 decimal place, we want 3
        // See also Vector3ToStringNeat
        public string VectorToString(Vector3 vec)
        {
            string vecString = "(";
            vecString += vec.x.ToString("F3") + ", ";
            vecString += vec.y.ToString("F3") + ", ";
            vecString += vec.z.ToString("F3") + ")";
            return vecString;
        }
        // values will only show the necessary number of decimal places
        // eg 1.000 will show as 1, but 1.001 will show as 1.001. Can also set a max num of decimal places
        public string Vector3ToStringNeat(Vector3 vector, int maxDecimalPlaces = 2)
        {
            if (maxDecimalPlaces > 5)
                maxDecimalPlaces = 5;

            // Format each component of the vector
            string xFormatted = FloatToStringNeat(vector.x, maxDecimalPlaces);
            string yFormatted = FloatToStringNeat(vector.y, maxDecimalPlaces);
            string zFormatted = FloatToStringNeat(vector.z, maxDecimalPlaces);

            // Create the formatted vector string
            string formattedVector = $"({xFormatted},  {yFormatted},  {zFormatted})";
            return formattedVector;
        }
        //-------------------------------------
        // Format a float value as a string, values will only show the necessary number of decimal places
        public string FloatToStringNeat(float value, int maxDecimalPlaces = 2)
        {
            if (value == 0)
                return "0";

            // First Round the value to the nearest 5 decimal places to eliminate float imprecision
            float dec = Mathf.Pow(10, maxDecimalPlaces);
            value = Mathf.Round(value * dec) / dec;

            // Determine the number of decimal places
            //int decimalPlaces = Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(value)) + 1);
            //int decimalPlaces = Mathf.Max(0, Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(value)) + 1));

            //This works better than trying to do it numerically with log10 due to float imprecision
            int decimalPlaces = 0;
            string valueString = value.ToString();
            if (valueString.Contains("."))
            {
                decimalPlaces = valueString.Length - valueString.IndexOf(".") - 1;
            }

            // Format the string based on the number of decimal places
            string formattedString = value.ToString(); //just in case

            //for(int stackIdx = 0; stackIdx < c; stackIdx++)
            {
                string F_string = "F" + decimalPlaces.ToString();
                formattedString = value.ToString(F_string); // Display as integer
            }
            return formattedString;
        }



        //---------------------------
        // If the input index is -1, we know it's bad, otherwise check index first
        // if the prefabName is not empty, check from that first
        public void FixAndWarnBadPrefab(LayerSet layer, int prefabIndex, string prefabName = "")
        {
            if (prefabName != "")
            {

            }

            else if (prefabIndex != -1)
            {

            }

            Debug.LogWarning($"Couldn't find inPrefab for layer: {GetLayerNameAsString(layer)}. Setting Prefab index to 0\n");
        }
        //---------
        public void ClearAllPrefabs()
        {
            postPrefabs.Clear();
            railPrefabs.Clear();
            subJoinerPrefabs.Clear();
            extraPrefabs.Clear();
        }
        //---------------------------
        public int FindPrefabIndexByName(LayerSet layer, string prefabName, bool warnMissing = true, bool returnMissingDefault = true, string msg = "")
        {
            return FindPrefabIndexByNameForLayer(GetPrefabTypeFromLayer(layer), prefabName, msg, warnMissing, returnMissingDefault);
        }
        //---------------------------
        /*public GameObject FindPrefabByNameAndType(string prefabName, bool warnMissing = true, bool returnMissingDefault = true, string msg = "")
        {
            GameObject go = null;
            go = FindPrefabByNameAndType(PrefabTypeAFWB.postPrefab, prefabName, warnMissing, returnMissingDefault, msg);
            if (go != null)
                return go;
            go = FindPrefabByNameAndType(PrefabTypeAFWB.railPrefab, prefabName, warnMissing, returnMissingDefault, msg);
            if (go != null)
                return go;
            go = FindPrefabByNameAndType(PrefabTypeAFWB.extraPrefab, prefabName, warnMissing, returnMissingDefault, msg);
            if (go != null)
                return go;

            if (warnMissing && prefabName != "-")
                print("Couldn't find inPrefab with menuName: " + prefabName + " of any type. Is it a User Object that'stackIdx been deleted? " + msg + "\n");
            if (returnMissingDefault && prefabName != "-")
            {
                Debug.LogWarning($"Replacing with default inPrefab\n");
                GameObject defaultPrefab = GetPrefabAtIndexForLayer(0, layer);
                return defaultPrefab;
            }
            else
                Debug.LogWarning($"Null Prefab was returned\n");
            return null;
        }*/
        //---------------------------
        // Find a inPrefab by menuName, but only in the given prefabType
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public GameObject FindPrefabByNameAndType(PrefabTypeAFWB prefabType, string prefabName, bool warnMissing = true, bool returnMissingDefault = true, string msg = "")
        {
            LayerSet layer = LayerSet.postLayer;
            if (prefabType == PrefabTypeAFWB.railPrefab)
                layer = LayerSet.railALayer; // any rail layer will do as their prefabs lists are the same
            else if (prefabType == PrefabTypeAFWB.extraPrefab)
                layer = LayerSet.extraLayer;
            else if (prefabType == PrefabTypeAFWB.allPrefab)
                layer = LayerSet.allLayer;

            GameObject go = FindPrefabByName(layer, prefabName, warnMissing, returnMissingDefault, msg);
            return go;
        }
        //---------------------------
        //[MethodImpl(MethodImplOptions.NoInlining)]
        // Find a inPrefab by menuName, but only in the given layerType
        public GameObject FindPrefabByName(LayerSet layer, string prefabName, bool warnMissing = true, bool returnMissingDefault = true, string msg = "", [CallerMemberName] string caller = null)
        {
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] == null)
                    continue;
                string name = prefabs[i].name;
                if (name == prefabName)
                    return prefabs[i];
            }

            //-- If it wasn't found, maybe the Post is using an Extra
            if (layer == LayerSet.postLayer)
            {
                prefabs = GetPrefabsForLayer(LayerSet.extraLayer);
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] == null)
                        continue;
                    string name = prefabs[i].name;
                    if (name == prefabName)
                        return prefabs[i];
                }
            }

            if ((warnMissing || returnMissingDefault) && prefabName != "-")
            {
                string warningStr;
                if (warnMissing)
                {
                    warningStr = $"FindPrefabByNameAndType():   Couldn't find inPrefab with menuName: {prefabName}. " +
                                       $"Is it a User Object that has been deleted or re-named? ({GetLayerNameAsString(layer)}:  {msg})\n";
                    if (returnMissingDefault)
                        warningStr += $"Replacing with default {GetLayerNameAsString(layer)} inPrefab";
                    else
                        warningStr += $"Null Prefab was returned";
                    warningStr += $"   Called from:   {caller}()";
                    Debug.LogWarning($"{warningStr}");
                }
                if (returnMissingDefault)
                {
                    int defaultPrefabIndex = GetDefaultPrefabIndexForLayer(layer);
                    GameObject defaultPrefab = GetPrefabAtIndexForLayer(defaultPrefabIndex, layer);
                    return defaultPrefab;
                }
            }
            return null;
        }
        //---------------------------
        /// <summary>
        /// Find a inPrefab in any layer by menuName
        /// </summary>
        /// <param menuName="prefabName   name of the inPrefab to search for"></param>
        /// <returns>GameObject if found, else null</returns>
        /// <remarks>There are no warnings because it is likely that a inPrefab might not be in a specific layer</remarks>
        public GameObject FindPrefabByName(string prefabName)
        {
            List<GameObject> prefabs = GetAllPrefabs();

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] == null)
                    continue;
                string name = prefabs[i].name;
                if (name == prefabName)
                    return prefabs[i];
            }
            return null;
        }
        /// <summary>
        /// Finds the index of the given prefab in the list of prefabs for the specified layer.
        /// </summary>
        /// <param name="inPrefab">The GameObject representing the prefab to find.</param>
        /// <param name="layer">The layer set to search within.</param>
        /// <returns>The index of the prefab if found; otherwise, -1.</returns>
        /// <remarks>Useful after saving a prefab to find it in the Lists</remarks>
        public int FindPrefabForLayer(GameObject inPrefab, LayerSet layer)
        {
            List<GameObject> prefabs = GetAllPrefabs();
            GameObject prefab = null;
            for (int i = 0; i < prefabs.Count; i++)
            {
                prefab = GetPrefabAtIndexForLayer(i, layer);
                if (prefab == null)
                    continue;
                if (prefab == inPrefab)
                    return i;
            }
            return -1;
        }

        //---------------------------
        // If canContain == true, a partial match with string.Comtains() is good enough
        // because of this there is the potential to find many matches, so we return a list
        public List<GameObject> FindPrefabsByNameContains(PrefabTypeAFWB prefabType, string prefabPartialName, bool warnMissing = true)
        {
            List<GameObject> matches = new List<GameObject>();
            List<GameObject> prefabs = GetPrefabsForPrefabType(prefabType);

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] == null)
                    continue;
                string name = prefabs[i].name;
                if (name.ToLower().Contains(prefabPartialName.ToLower()))
                    matches.Add(prefabs[i]);
            }
            if (warnMissing && prefabPartialName != "-")
                print("Couldn't find any prefabs matching partial menuName: " + prefabPartialName + " " + prefabType + ")\n");
            return matches;
        }
        //---------------------------
        // Same as above but searches all Prefab types
        public List<GameObject> FindPrefabsByNameContains(string prefabPartialName, bool warnMissing = true)
        {
            List<GameObject> matches = new List<GameObject>();
            matches = FindPrefabsByNameContains(PrefabTypeAFWB.railPrefab, prefabPartialName);
            matches.AddRange(FindPrefabsByNameContains(PrefabTypeAFWB.postPrefab, prefabPartialName));
            matches.AddRange(FindPrefabsByNameContains(PrefabTypeAFWB.extraPrefab, prefabPartialName));

            return matches;
        }
        //---------------------------
        public int FindPrefabIndexInMenuNamesList(LayerSet layer, string prefabName, bool warnMissing = true)
        {
            PrefabTypeAFWB prefabType = GetPrefabTypeFromLayer(layer);
            int prefabIndex = FindPrefabIndexInMenuNamesList(prefabType, prefabName, warnMissing);
            if (prefabIndex > GetPrefabsForLayer(layer).Count)
                Debug.LogWarning($"FindPrefabIndexInMenuNamesList: prefabIndex > " +
                    $"GetPrefabsForLayer({GetLayerNameAsString(layer)}).Count  ({prefabIndex} > {GetPrefabsForLayer(layer).Count})\n");
            return prefabIndex;
        }
        //---------------------------
        public int FindPrefabIndexInMenuNamesList(PrefabTypeAFWB prefabType, string prefabName, bool warnMissing = true)
        {
            if (prefabName == null)
            {
                Debug.LogWarning("FindPrefabIndexInMenuNamesList: prefabName is null. Unable to get inPrefab. \n");
                return -1;
            }

            //if(prefabName.Contains("RockAA"))
            //Debug.LogWarning("\n");



            //-- First search through the correct layer type, e.g. assuming that a Post uses a _Post inPrefab
            if (prefabType == PrefabTypeAFWB.railPrefab)
            {
                for (int i = 0; i < railMenuNames.Count; i++)
                {
                    if (railMenuNames[i] == null)
                        continue;
                    string menuName = railMenuNames[i];
                    if (menuName.Contains(prefabName))
                        return i;
                }
            }
            else if (prefabType == PrefabTypeAFWB.postPrefab)
            {
                for (int i = 0; i < postMenuNames.Count; i++)
                {
                    if (postMenuNames[i] == null)
                        continue;
                    string menuName = postMenuNames[i];
                    if (menuName.Contains(prefabName))
                        return i;
                }
            }
            else if (prefabType == PrefabTypeAFWB.extraPrefab)
            {
                for (int i = 0; i < extraMenuNames.Count; i++)
                {
                    if (extraMenuNames[i] == null)
                        continue;
                    string menuName = extraMenuNames[i];
                    if (menuName.Contains(prefabName))
                        return i;
                }
            }


            //-- If it's not found search throught the type defined by the menuName
            //=========================================================================
            PrefabTypeAFWB typeFromName = GetPrefabTypeFromName(prefabName);
            List<string> menuNamesForType = GetPrefabMenuNamesForLayer(typeFromName.ToLayer());
            for (int i = 0; i < menuNamesForType.Count; i++)
            {
                if (menuNamesForType[i] == null)
                    continue;
                string menuName = menuNamesForType[i];
                if (menuName.Contains(prefabName))
                    return i;
            }


            if (warnMissing && prefabName != "-")
            {
                print("Couldn't find inPrefab with menuName: " + prefabName + ".Is it a User Object that's been deleted?  (" + prefabType + ")\n");
            }
            return -1;
        }
        //----------------------------------
        List<Vector3> CreateCubicSpline3D(List<Vector3> inNodes, float numInters,
                                                         SplineFillMode fillMode = SplineFillMode.fixedNumPerSpan,
                                                         float tension = 0, float bias = 0, bool addInputNodesBackToList = true)
        {
            int numNodes = inNodes.Count;
            if (numNodes < 4) return inNodes;

            float mu, interpX, interpZ;
            int numOutNodes = (numNodes - 1) * (int)numInters;
            List<Vector3> outNodes = new List<Vector3>(numOutNodes);

            int numNewPoints = (int)numInters;
            for (int j = 2; j < numNodes - 3; j++) // don't build first  fake ones
            {
                outNodes.Add(inNodes[j]);
                Vector3 a, b, c, d;
                a = inNodes[j - 1];
                b = inNodes[j];
                c = inNodes[j + 1];
                if (j < numNodes - 2)
                    d = inNodes[j + 2];
                else
                    d = inNodes[numNodes - 1];

                if (fillMode == SplineFillMode.equiDistant) //equidistant postsPool, numInters now refers to the requested distanceToNextPost between the new points
                {
                    float dist = Vector3.Distance(b, c);
                    numNewPoints = (int)Mathf.Round(dist / numInters);
                    if (numNewPoints < 1) numNewPoints = 1;
                }

                float t = tension;
                if (IsBreakPoint(inNodes[j]) || IsBreakPoint(inNodes[j + 2]))
                { // this will prevent falsely rounding in to gaps/breakPoints
                    t = 1.0f;
                }

                for (int i = 0; i < numNewPoints; i++)
                {
                    mu = (1.0f / (numNewPoints + 1.0f)) * (i + 1.0f);
                    interpX = HermiteInterpolate(a.x, b.x, c.x, d.x, mu, t, bias);
                    interpZ = HermiteInterpolate(a.z, b.z, c.z, d.z, mu, t, bias);
                    outNodes.Add(new Vector3(interpX, b.y, interpZ));
                }
            }
            if (addInputNodesBackToList)
            {
                outNodes.Add(inNodes[numNodes - 3]);
            }
            return outNodes;
        }
        float HermiteInterpolate(float y0, float y1, float y2, float y3, float mu, float tension, float bias)
        {
            float mid0, mid1, mid2, mid3;
            float a0, a1, a2, a3;
            mid2 = mu * mu;
            mid3 = mid2 * mu;
            mid0 = (y1 - y0) * (1 + bias) * (1 - tension) / 2;
            mid0 += (y2 - y1) * (1 - bias) * (1 - tension) / 2;
            mid1 = (y2 - y1) * (1 + bias) * (1 - tension) / 2;
            mid1 += (y3 - y2) * (1 - bias) * (1 - tension) / 2;
            a0 = 2 * mid3 - 3 * mid2 + 1;
            a1 = mid3 - 2 * mid2 + mu;
            a2 = mid3 - mid2;
            a3 = -2 * mid3 + 3 * mid2;
            return (a0 * y1 + a1 * mid0 + a2 * mid1 + a3 * y2);
        }
        //---------------------------------------
        // Will give warnings if issues
        public void PrintAllMainPrefabs(string presetName = "")
        {
            //ClearConsole();
            if (presetName != "")
                Debug.Log($"{presetName}: \n");

            Color inUseColor = Color.green;
            Color notInUseColor = Color.grey;

            string inUseStr = "<color=#40E740>";
            string notInUseStr = "<color=#8A9B8A>";
            string colorStr = inUseStr;


            colorStr = usePostsLayer ? inUseStr : notInUseStr;
            Debug.Log($"{colorStr}         Post = {GetMainPrefabForLayer(LayerSet.postLayer).name} \n</color>");

            colorStr = useRailLayer[0] ? inUseStr : notInUseStr;
            Debug.Log($"{colorStr}         RailA = {GetMainPrefabForLayer(LayerSet.railALayer).name} \n</color>");

            colorStr = useRailLayer[1] ? inUseStr : notInUseStr;
            Debug.Log($"{colorStr}         RailB = {GetMainPrefabForLayer(LayerSet.railBLayer).name} \n</color>");

            colorStr = useSubpostsLayer ? inUseStr : notInUseStr;
            Debug.Log($"{colorStr}         Subpost = {GetMainPrefabForLayer(LayerSet.subpostLayer).name} \n</color>");

            colorStr = useExtrasLayer ? inUseStr : notInUseStr;
            Debug.Log($"{colorStr}         Extra = {GetMainPrefabForLayer(LayerSet.extraLayer).name} \n</color>");


        }
        public GameObject GetBuiltGOAtSectionIndexForLayer(int sectionIndex, LayerSet layer)
        {
            Transform transform = GetBuiltTransformAtSectionIndexForLayer(sectionIndex, layer);
            if (transform == null)
                return null;
            GameObject go = transform.gameObject;
            if (go == null)
                Debug.LogWarning($"GetBuiltGOAtSectionIndexForLayer()  {GetLayerNameAsString(layer)} gameObject at sectionIndex {sectionIndex} was null\n");
            return go;
        }
        //-------------------------
        void DrawSurfaceNormal(Vector3 pt, float length = 4, float rayHeight = 10)
        {
            Vector3 rayPos = pt;
            rayPos.y += rayHeight;
            RaycastHit hit;
            SetIgnorePartsColliders(true);
            if (Physics.Raycast(rayPos, Vector3.down, out hit, 500))
            {
                if (hit.collider.gameObject != null)
                {
                    Vector3 norm = hit.normal;
                    SetIgnorePartsColliders(false);
                    Gizmos.DrawLine(hit.point, hit.point + (norm * length));
                    //Gizmos.DrawMesh();
                }
            }
            SetIgnorePartsColliders(false);
        }

        //-------------------------
        public Vector3 GetSurfaceNormal(Vector3 pt, float rayHeight = 10)
        {
            Vector3 rayPos = pt;
            rayPos.y += rayHeight;
            RaycastHit hit;
            SetIgnorePartsColliders(true);
            if (Physics.Raycast(rayPos, Vector3.down, out hit, 500))
            {
                if (hit.collider.gameObject != null)
                {
                    Vector3 norm = hit.normal;
                    SetIgnorePartsColliders(false);
                    return norm;
                }
            }
            SetIgnorePartsColliders(false);
            return Vector3.zero;
        }
        //-------------------------
        public Vector3 GetLerpedSurfaceNormal(Vector3 pt, float lerp, float rayHeight = 10)
        {
            Vector3 rayPos = pt;
            rayPos.y += rayHeight;
            RaycastHit hit;
            SetIgnorePartsColliders(true);
            if (Physics.Raycast(rayPos, Vector3.down, out hit, 500))
            {
                if (hit.collider.gameObject != null)
                {
                    Vector3 norm = hit.normal;
                    Vector3 normLerp = Vector3.Lerp(Vector3.up, norm, lerp);
                    SetIgnorePartsColliders(false);
                    return normLerp;
                }
            }
            SetIgnorePartsColliders(false);
            return Vector3.zero;
        }
        //-----------------------------
        void AlignPostToSurfaceNormal(List<Transform> posts, int i)
        {
            if (adaptPostToSurfaceDirection)
            {
                Vector3 normLerp = GetLerpedSurfaceNormal(posts[i].transform.position, postSurfaceNormalAmount);
                posts[i].rotation = Quaternion.FromToRotation(Vector3.up, normLerp);
            }
        }
        //-----------------------------
        void AlignPostToSurfaceNormal(ref GameObject post)
        {
            if (adaptPostToSurfaceDirection)
            {
                Vector3 normLerp = GetLerpedSurfaceNormal(post.transform.position, postSurfaceNormalAmount);
                post.transform.rotation = Quaternion.FromToRotation(Vector3.up, normLerp);
            }
        }
        //-----------------------------
        void AlignSubpostToSurfaceNormal(ref GameObject subpost)
        {
            if (adaptSubpostToSurfaceDirection)
            {
                Vector3 normLerp = GetLerpedSurfaceNormal(subpost.transform.position, subpostSurfaceNormalAmount);
                subpost.transform.rotation = Quaternion.FromToRotation(Vector3.up, normLerp);
            }
        }
        //---------------
        //- Is it a Rail type, either A or B 
        public bool IsRailLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                return true;
            return false;
        }
        //---------------
        //- Is it a Rail type, either A or B 
        public bool IsRailLayer(int layerIndex)
        {
            if (layerIndex == 0 || layerIndex == 1)
                return true;
            return false;
        }
        //---------------
        public int GetRailLayerOfRailAsInt(GameObject go)
        {
            if (GetRailLayerOfRail(go) == LayerSet.railALayer)
                return 0;
            else if (GetRailLayerOfRail(go) == LayerSet.railBLayer)
                return 1;
            return -1;
        }
        //-------------------
        int FindClickPointIndex(Vector3 pos)
        {
            return clickPoints.IndexOf(pos);
        }
        //-------------------
        public GameObject GetUserPrefabForLayer(LayerSet layer)
        {
            GameObject userPrefab = null;

            if (layer == LayerSet.railALayer)
                userPrefab = userPrefabRail[0];
            else if (layer == LayerSet.railBLayer)
                userPrefab = userPrefabRail[1];
            else if (layer == LayerSet.postLayer)
                userPrefab = userPrefabPost;
            else if (layer == LayerSet.extraLayer)
                userPrefab = userPrefabExtra;
            return userPrefab;
        }
        public void SetUserPrefabForLayer(GameObject userPrefab, LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                userPrefabRail[0] = userPrefab;
            else if (layer == LayerSet.railBLayer)
                userPrefabRail[1] = userPrefab;
            else if (layer == LayerSet.postLayer)
                userPrefabPost = userPrefab;
            else if (layer == LayerSet.extraLayer)
                userPrefabExtra = userPrefab;
        }
        //------------
        // This is only used when we are Finishing a fence with no postsPool. We need to save the click-points as postsPool so that the finished fence can be re-edited
        void CreateClickPointPostsForFinishedFence(int n, Vector3 postPoint)
        {
            bool isClickPoint = false;
            if (clickPoints.Contains(postPoint))
            {
                isClickPoint = true;
            }
            bool isGap = false;
            for (int i = 0; i < gaps.Count(); i += 2)
            {
                if (gaps[i] == postPoint)
                    isGap = true;
            }

            if (postsPool == null || postsPool.Count == 0 || postsPool[0] == null)
                Debug.LogWarning("Missing Post Instance in CreateClickPointPostsForFinishedFence()");
            else if (isClickPoint == true)
            {
                GameObject markerPost = FindPrefabByNameAndType(PrefabTypeAFWB.postPrefab, "Marker_Post");
                GameObject thisPost = GameObject.Instantiate(markerPost);

                thisPost.SetActive(false);
                thisPost.hideFlags = HideFlags.None;
                // Name it if it is a click point, remove old menuName first
                bool nameContainsClick = thisPost.name.Contains("_click");
                if (nameContainsClick)
                    thisPost.name = thisPost.name.Remove(thisPost.name.IndexOf("_click"), 6);
                if (isClickPoint == true)
                    thisPost.name += "_click";

                bool nameContainsGap = thisPost.name.Contains("_gap");
                if (nameContainsGap)
                    thisPost.name = thisPost.name.Remove(thisPost.name.IndexOf("_gap"), 4);
                if (isGap == true)
                    thisPost.name += "_gap";

                //Set not to interfere with the picking of the control posts which coincide with Posts. v2.3 removed after Finalize. Editable in Setting Window
                thisPost.layer = ignoreControlNodesLayerNum;


                //=========== Position ==============
                thisPost.transform.position = postPoint;
                thisPost.transform.position += new Vector3(0, postHeightOffset * globalScale.y, 0);


                thisPost.isStatic = false;

                if (usePostsLayer == true)
                    postsTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(thisPost);


                //====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
                int numPostsFolders = (postsBuiltCount / objectsPerFolder) + 1;
                string postsDividedFolderName = "PostsGroupedFolder" + (numPostsFolders - 1);
                GameObject postsDividedFolder = GameObject.Find("Current Fences Folder/Posts/" + postsDividedFolderName);
                if (postsDividedFolder == null)
                {
                    postsDividedFolder = new GameObject(postsDividedFolderName);
                    postsDividedFolder.transform.parent = postsFolder.transform;
                }

                thisPost.transform.parent = postsDividedFolder.transform;
                CreateColliderForLayer(thisPost, thisPost.transform.localPosition, LayerSet.postLayer); // ??? Needs further checking
            }

        }
        //-------------------
        public float GetAngleAtPost(int i, List<Vector3> posts)
        {
            if (i >= posts.Count - 1 || i <= 0) return 0;

            Vector3 vecA = posts[i] - posts[i - 1];
            Vector3 vecB = posts[i + 1] - posts[i];
            float angle = Vector3.Angle(vecA, vecB);
            return angle;
        }
        //------------------
        float CalcDistanceToLine(Vector3 lineStart, Vector3 lineEnd, Vector3 pt)
        {
            Vector3 direction = lineEnd - lineStart;
            Vector3 startingPoint = lineStart;

            Ray ray = new Ray(startingPoint, direction);
            float distance = Vector3.Cross(ray.direction, pt - ray.origin).magnitude;

            if (((lineStart.x > pt.x && lineEnd.x > pt.x) || (lineStart.x < pt.x && lineEnd.x < pt.x)) && // it'stackIdx before or after both x'stackIdx
               ((lineStart.z > pt.z && lineEnd.z > pt.z) || (lineStart.z < pt.z && lineEnd.z < pt.z))) // it'stackIdx before or after both z'stackIdx
            {
                return float.MaxValue;
            }
            return distance;
        }

        //---------------------
        // often we need to know the flat distanceToNextPost, ignoring any height difference
        float CalculateGroundDistance(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            float distance = Vector3.Distance(a, b);

            return distance;
        }
        //------------
        //sometimes the post postion y is offset, so to test for a match only use x & z
        /*bool IsClickPointXZ()
        {
        }*/
        //------------
        void UpdateColliders()
        {
            if (useExtrasLayer)
            {
                MeshUtilitiesAFWB.UpdateAllColliders(extraPrefabs[currentExtra_PrefabIndex]);
            }
        }
        //-------------
        // Find the index of the input GO in the full sourceVariant list
        public int FindFirstInVariants(List<SourceVariant> sourceVariantList, GameObject go, LayerSet layer)
        {
            if (go == null)
            {
                Debug.Log($"FindFirstInVariants()  go is null for {GetLayerNameAsString(layer)}");
                return -1;
            }

            int index = -1, count = sourceVariantList.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject sourceVariantGO = sourceVariantList[i].Go;
                if (sourceVariantGO == null)
                {
                    Debug.Log($"FindFirstInVariants()  sourceVariantGO index {i} is null for {GetLayerNameAsString(layer)}");
                    return -1;
                }
                if (sourceVariantList[i].Go == go)
                    return i;
            }
            return index;
        }
        public int GetNumSectionsBuiltForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return allPostPositions.Count - 1;
            else if (layer == LayerSet.railBLayer)
                return allPostPositions.Count - 1;
            else if (layer == LayerSet.postLayer)
                return allPostPositions.Count;
            return 0;
        }
        private int GetLastPostIndex()
        {
            //get the inex of the last post that is not a click point
            int lastPostIndex = allPostPositions.Count - 1;
            return lastPostIndex;
        }
        public int GetMenuIndexFromQuantizedRotAngle(float quantAngle)
        {
            int index = 0;
            switch (quantAngle)
            {
                case 30:
                    index = 1; break;
                case 45:
                    index = 2; break;
                case 60:
                    index = 3; break;
                case 90:
                    index = 4; break;
                case 120:
                    index = 5; break;
                case 180:
                    index = 6; break;
                case -90:
                    index = 7; break;
                case -180:
                    index = 8; break;
                case -1:
                    index = 9; break;

                default:
                    index = 0; break;
            }
            return index;
        }
    }


    //================================================================================================
    public static class PrefabTypeAFWBExtensions
    {
        public static LayerSet ToLayer(this PrefabTypeAFWB prefabType)
        {
            switch (prefabType)
            {
                case PrefabTypeAFWB.postPrefab:
                    return LayerSet.postLayer;
                case PrefabTypeAFWB.railPrefab:
                    return LayerSet.railALayer;
                case PrefabTypeAFWB.extraPrefab:
                    return LayerSet.extraLayer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(prefabType), prefabType, null);
            }
        }
    }
    public static class LayerSetExtensions
    {
        public static PrefabTypeAFWB ToPrefabType(this LayerSet layerSet)
        {
            if (layerSet == LayerSet.postLayer)
                return PrefabTypeAFWB.postPrefab;
            else if (layerSet == LayerSet.railALayer || layerSet == LayerSet.railBLayer)
                return PrefabTypeAFWB.railPrefab;
            else if (layerSet == LayerSet.extraLayer)
                return PrefabTypeAFWB.extraPrefab;
            else if (layerSet == LayerSet.subpostLayer)
                return PrefabTypeAFWB.postPrefab;
            else
            {
                Debug.LogError($"LayerSetExtensions.ToPrefabType()  Unknown LayerSet: {layerSet}");
            }
            return PrefabTypeAFWB.nonePrefab;
        }
        /// <summary>
        ///  Returns the string menuName of the LayerSet enum.
        ///  <para></para>
        ///  For post  or  railA  use Default(true, true) [is camelCase and spaces removed]
        ///  <para></para>
        ///  For Post, or Rail A call (false, false) 
        /// </summary>
        /// <param menuName="layerSet"></param>
        /// <param menuName="camelCase"></param>
        /// <param menuName="removeSpaces"></param>
        /// <returns></returns>
        public static string String(this LayerSet layerSet, bool removeSpaces = true)
        {
            string result = layerSet switch
            {
                LayerSet.railALayer => "Rail A",
                LayerSet.railBLayer => "Rail B",
                LayerSet.postLayer => "Post",
                LayerSet.extraLayer => "Extra",
                LayerSet.subpostLayer => "Subpost",
                LayerSet.allLayer => "All",
                LayerSet.markerLayer => "Marker",
                LayerSet.None => "None",
                _ => "Unknown"
            };
            if (removeSpaces)
                result = result.Replace(" ", string.Empty);
            return result;
        }
        public static bool IsFence(this LayerSet layerSet)
        {
            //if (layerSet <= LayerSet.subpostLayer)
            if (layerSet <= LayerSet.markerLayer)
                return true;
            return false;
        }
        public static bool IsMarker(this LayerSet layerSet)
        {
            if (layerSet == LayerSet.markerLayer)
                return true;
            return false;
        }

        public static string StringCamel(this LayerSet layerSet)
        {
            string result = String(layerSet);
            result = ToCamelCase(result);
            return result;
        }

        public static int Int(this LayerSet layerSet)
        {
            return (int)layerSet;
        }

        private static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2)
                return str;
            //- Strip white space
            str = str.Replace(" ", string.Empty);
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }


    }
}


