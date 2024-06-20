using MeshUtils;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace AFWB
{
    public partial class AutoFenceCreator
    {
        //================================================
        public Collider CreateColliderForLayer(GameObject go, Vector3 centrePos, LayerSet layer) // centrePos only needed when creating a box collider for a user object
        {//0 = single box, 1 = keep original (user), 2 = no colliders, 3 = mesh colliders
            StackLog(GetType().Name, verbose: false);

            if (IsLayerEnabled(layer) == false)
                return null;

            BoxCollider existingBoxCollider = null;
            ColliderType colliderType = railAColliderMode;
            float scaleY = railABoxColliderHeightScale, offsetY = railABoxColliderHeightOffset;
            bool useCustomPrefab = useCustomRail[kRailALayerInt];

            if (layer == LayerSet.railBLayer)
            {
                colliderType = railBColliderMode;
                scaleY = railBBoxColliderHeightScale;
                offsetY = railBBoxColliderHeightOffset;
                useCustomPrefab = useCustomRail[kRailBLayerInt];
            }
            else if (layer == LayerSet.postLayer)
            {
                colliderType = postColliderMode;
                scaleY = postBoxColliderHeightScale;
                offsetY = postBoxColliderHeightOffset;
                useCustomPrefab = useCustomPost;
            }
            else if (layer == LayerSet.extraLayer)
            {
                colliderType = extraColliderMode;
                scaleY = extraBoxColliderHeightScale;
                offsetY = extraBoxColliderHeightOffset;
                useCustomPrefab = useCustomExtra;
            }
            else if (layer == LayerSet.subpostLayer)
            {
                colliderType = subpostColliderMode;
                scaleY = subpostBoxColliderHeightScale;
                offsetY = subpostBoxColliderHeightOffset;
                //useCustomPrefab = useCustomSubpost;
            }

            //      Get all transforms, parent aSnd children
            //===============================================
            //-- Prefer this to GetChild as it includes the parent
            Transform[] allTransforms = go.GetComponentsInChildren<Transform>(true);

            // Box Collider Exists but is not wanted, so destroy and return
            //=================================================================
            if (go.GetComponent<BoxCollider>() != null && colliderType == ColliderType.noCollider) //0 = single box, 1 = keep original (user), 2 = no colliders
            {
                foreach (Transform t in allTransforms)
                {
                    existingBoxCollider = t.gameObject.GetComponent<BoxCollider>();
                    if (existingBoxCollider != null)
                        DestroyImmediate(existingBoxCollider);
                }
                return null;
            }

            //    No Colliders, so we make a basic box collider anyway
            //    (for hit detection in Scene) and remove when doing a 'Finish'
            //    No need to do Children
            //===================================================================
            if (colliderType == ColliderType.noCollider)// 2 = no colliders
            {
                if (go.GetComponent<BoxCollider>() == null)
                    existingBoxCollider = (BoxCollider)go.AddComponent<BoxCollider>();
                return existingBoxCollider;
            }

            //    Simple single BoxCollider
            //==================================
            if (colliderType == ColliderType.boxCollider && useCustomPrefab == false) //0 = single box, 1 = keep original (user), 2 = no colliders
            {
                foreach (Transform child in allTransforms)
                {
                    GameObject childGO = child.gameObject;
                    /*xistingBoxCollider = childGO.GetComponent<BoxCollider>();
                    if(existingBoxCollider != null)
                        DestroyImmediate(existingBoxCollider); //destroys old box collider*/

                    //- it'i an ordinary single AFB singleVarGO
                    if (useCustomPrefab == false)
                    {
                        existingBoxCollider = (BoxCollider)childGO.AddComponent<BoxCollider>();
                        if (existingBoxCollider == null)
                            childGO.AddComponent<BoxCollider>();

                        existingBoxCollider.enabled = true;
                        Vector3 newSize = existingBoxCollider.size;
                        newSize.y *= scaleY;
                        existingBoxCollider.size = newSize;
                        Vector3 newCenter = existingBoxCollider.center;
                        newCenter.y += offsetY;
                        existingBoxCollider.center = newCenter;
                    }
                    //- it'i a user object, possibly grouped
                    else
                    {
                        existingBoxCollider = (BoxCollider)childGO.AddComponent<BoxCollider>();
                        if (existingBoxCollider != null)
                            childGO.AddComponent<BoxCollider>();

                        existingBoxCollider.enabled = true;
                        Vector3 newSize = existingBoxCollider.size;
                        newSize.y *= scaleY;
                        existingBoxCollider.size = newSize;
                        Vector3 newCenter = existingBoxCollider.center;
                        newCenter.y += offsetY;
                        existingBoxCollider.center = newCenter;
                    }
                }
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(go, true);
                if (colliderType == 0)
                    return go.GetComponent<Collider>();
            }



            //    Custom Collider Mesh
            //===============================================
            if (colliderType == ColliderType.customCollider)
            {
                if (existingBoxCollider != null)
                    DestroyImmediate(existingBoxCollider); //destroys unwanted BOX collider

                Mesh customMesh = GetCustomColliderMeshForLayer(layer);

                if (go != null && customMesh != null)
                {
                    RemoveAllCollidersFromGO(go);

                    MeshCollider meshCollider = go.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = customMesh;

                    if (layer == LayerSet.railALayer)
                    {
                        //Vector3 goSize =  MeshUtilitiesAFWB.GetWorldSizeOfGameObject(go, layer, this);
                        //Vector3 collMeshSize = customMesh.bounds.size;

                        //scale the mesh to fit the object
                        //Vector3 scale = new Vector3(goSize.x / collMeshSize.x, goSize.y / collMeshSize.y, goSize.z / collMeshSize.z);
                        //meshCollider.transform.localScale = scale;


                    }

                    Debug.Log($"Added custom collider to {go.name} \n");
                }
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(go, true);
                return go.GetComponent<MeshCollider>();
            }


            //      Original collider on user'i custom singleVarGO
            //===============================================
            if (colliderType == ColliderType.originalCollider && useCustomPrefab == true)
            {
                if (existingBoxCollider != null)
                    DestroyImmediate(existingBoxCollider); //destroys unwanted BOX collider
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(go, true);
                return go.GetComponent<Collider>();
            }

            //      Mesh Colliders
            //==========================
            if (colliderType == ColliderType.meshCollider)
            {
                foreach (Transform child in allTransforms)
                {
                    GameObject childGO = child.gameObject;
                    //- Remove an old unwanted Box collider
                    existingBoxCollider = childGO.GetComponent<BoxCollider>();
                    if (existingBoxCollider != null)
                        DestroyImmediate(existingBoxCollider);

                    //- Rebuild the Mesh Collider as the mesh may have been modified
                    MeshCollider meshCollider = childGO.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                        DestroyImmediate(meshCollider);
                    meshCollider = (MeshCollider)childGO.AddComponent<MeshCollider>();
                }
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(go, true);
                return go.GetComponent<MeshCollider>();
            }
            return null;
        }
        //---
        public Mesh GetCustomColliderMeshForLayer(LayerSet layer)
        {
            Mesh mesh = null;
            if (layer == LayerSet.railALayer)
                mesh = railACustomColliderMesh;
            else if (layer == LayerSet.railBLayer)
                mesh = railBCustomColliderMesh;
            else if (layer == LayerSet.postLayer)
                mesh = postCustomColliderMesh;
            else if (layer == LayerSet.extraLayer)
                mesh = extraCustomColliderMesh;
            else if (layer == LayerSet.subpostLayer)
                mesh = subpostCustomColliderMesh;

            return mesh;
        }
            
        //--------------------------
        //Independently of the user's Collider preferences, during builing/editing there needs to be a
        //single box collider on each post and rail, for click detection in the Scene View
        private void CheckCollidersAfterBuild()
        {
            StackLog(GetType().Name, verbose: false);
            //==  Check Posts  ==
            int numPosts = postsPool.Count;
            if (numPosts > 0 && IsLayerEnabled(LayerSet.postLayer))
            {
                for (int i = 0; i < 1; i++) // Pointless loop, but might extend to all postsPool later
                {
                    GameObject post = postsPool[i].gameObject;
                    BoxCollider boxCollider = post.GetComponent<BoxCollider>();
                    if (boxCollider == null)
                    {
                        boxCollider = (BoxCollider)post.AddComponent<BoxCollider>();
                        //Debug.Log("Adding BoxCollider to " + post.name + " " + i + "\n");
                    }
                    else
                    {
                        if (boxCollider.enabled == false)
                        {
                            boxCollider.enabled = true;
                            Debug.Log("Enabling BoxCollider on " + post.name + " " + i + "\n");
                        }
                        Vector3 boxColliderSize = boxCollider.size;
                        if (boxColliderSize != Vector3.one)
                        {
                            //Debug.Log("Fixed Collider Size on " + post.name + " " + i + "  was  " + boxColliderSize + "\n"); //Check this agaom
                            boxCollider.size = Vector3.one;
                            boxCollider.center = Vector3.zero;
                        }
                        Vector3 boxColliderCenter = boxCollider.center;
                        if (boxColliderCenter != Vector3.zero)
                        {
                            boxCollider.center = Vector3.zero;
                            Debug.Log("Fixed Collider Center on " + post.name + " " + i + "  was " + boxColliderCenter + "\n");
                        }
                    }
                }
            }
        }

        //=====================================================================
        private void AddBoxCollidersToPostAndRailsForSceneVieweDetection()
        {
            return;
            
            int numPosts = GetNumBuiltForLayer(LayerSet.postLayer);
            if (numPosts > 0 && IsLayerEnabled(LayerSet.postLayer))
            {
                //__Check if we need to remove any old ones
                RemoveAllCollidersOfTypeFromLayer<BoxCollider>(LayerSet.postLayer);
                for (int i = 0; i < numPosts; i++)
                {
                    GameObject go = postsPool[i].gameObject;
                    AddColliderOfTypeToGameObjectAndChildren<BoxCollider>(go);
                }
            }
            int numRailsA = GetNumBuiltForLayer(LayerSet.railALayer);
            if (numRailsA > 0 && IsLayerEnabled(LayerSet.railALayer))
            {
                RemoveAllCollidersOfTypeFromLayer<BoxCollider>(LayerSet.railALayer);
                for (int i = 0; i < numRailsA; i++)
                {
                    GameObject go = railsAPool[i].gameObject;

                    AddColliderOfTypeToGameObjectAndChildren<BoxCollider>(go);
                }
            }
            int numRailsB = GetNumBuiltForLayer(LayerSet.railBLayer);
            if (numRailsB > 0 && IsLayerEnabled(LayerSet.railBLayer))
            {
                RemoveAllCollidersOfTypeFromLayer<BoxCollider>(LayerSet.railBLayer);
                for (int i = 0; i < numRailsB; i++)
                {
                    GameObject go = railsBPool[i].gameObject;
                    AddColliderOfTypeToGameObjectAndChildren<BoxCollider>(go);
                }
            }
        }

        //=====================================================================
        //If there's more than one of a given type of collider, remove all but the first. Do the same to children
        private void RemoveExtraCollidersFromGameObject(GameObject go)
        {
            // Remove extra colliders for each child GameObject, including the parent
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
            {
                RemoveExtraCollidersFromSingleGameObject(child.gameObject);
            }
        }

        private void RemoveExtraCollidersFromSingleGameObject(GameObject go)
        {
            RemoveExtraCollidersOfTypeFromGO<BoxCollider>(go);
            RemoveExtraCollidersOfTypeFromGO<SphereCollider>(go);
            RemoveExtraCollidersOfTypeFromGO<CapsuleCollider>(go);
            RemoveExtraCollidersOfTypeFromGO<MeshCollider>(go);
        }

        private void RemoveExtraCollidersOfTypeFromGO<T>(GameObject go) where T : Collider
        {
            T[] colliders = go.GetComponents<T>();
            for (int i = 1; i < colliders.Length; i++) // Start from 1 to keep the first collider
            {
                DestroyImmediate(colliders[i]);
            }
        }

        private void RemoveAllCollidersOfTypeFromGO<T>(GameObject go) where T : Collider
        {
            T[] colliders = go.GetComponents<T>();
            for (int i = 0; i < colliders.Length; i++)
            {
                DestroyImmediate(colliders[i]);
            }
        }

        private void RemoveAllCollidersOfTypeFromLayer<T>(LayerSet layer) where T : Collider
        {
            if (layer == LayerSet.postLayer)
            {
                for (int i = 0; i < GetNumBuiltForLayer(LayerSet.postLayer); i++)
                {
                    GameObject go = postsPool[i].gameObject;
                    RemoveAllCollidersOfTypeFromGO<T>(go);
                }
            }
            else if (layer == LayerSet.railALayer)
            {
                for (int i = 0; i < GetNumBuiltForLayer(LayerSet.railALayer); i++)
                {
                    GameObject go = railsAPool[i].gameObject;
                    RemoveAllCollidersOfTypeFromGO<T>(go);
                }
            }
            else if (layer == LayerSet.railBLayer)
            {
                for (int i = 0; i < GetNumBuiltForLayer(LayerSet.railBLayer); i++)
                {
                    GameObject go = railsBPool[i].gameObject;
                    RemoveAllCollidersOfTypeFromGO<T>(go);
                }
            }
            else if (layer == LayerSet.subpostLayer)
            {
                for (int i = 0; i < GetNumBuiltForLayer(LayerSet.subpostLayer); i++)
                {
                    GameObject go = subpostsPool[i].gameObject;
                    RemoveAllCollidersOfTypeFromGO<T>(go);
                }
            }
            else if (layer == LayerSet.extraLayer)
            {
                for (int i = 0; i < GetNumBuiltForLayer(LayerSet.extraLayer); i++)
                {
                    GameObject go = ex.extrasPool[i].gameObject;
                    RemoveAllCollidersOfTypeFromGO<T>(go);
                }
            }
        }

        private void AddColliderOfTypeToGameObjectAndChildren<T>(GameObject go) where T : Collider
        {
            // Add the collider to the parent GameObject
            go.AddComponent<T>();
            // Add the collider to each child GameObject
            foreach (Transform child in go.transform)
            {
                go.AddComponent<T>();
            }
        }

        //-----------------------------
        private void RemoveUnwantedColliders()
        {
            // Go around twice in case the user added a second collider to the prefab
            for (int j = 0; j < 2; j++)
            {
                if (railAColliderMode == ColliderType.noCollider) //0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
                {
                    for (int i = 0; i < railsAPool.Count; i++)
                    {
                        GameObject rail = railsAPool[i].gameObject;
                        BoxCollider boxCollider = rail.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            DestroyImmediate(boxCollider);

                        MeshCollider meshCollider = rail.GetComponent<MeshCollider>();
                        if (meshCollider != null)
                            DestroyImmediate(meshCollider);
                    }
                }
                if (railBColliderMode == ColliderType.noCollider) //0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
                {
                    for (int i = 0; i < railsBPool.Count; i++)
                    {
                        GameObject rail = railsBPool[i].gameObject;
                        BoxCollider boxCollider = rail.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            DestroyImmediate(boxCollider);

                        MeshCollider meshCollider = rail.GetComponent<MeshCollider>();
                        if (meshCollider != null)
                            DestroyImmediate(meshCollider);
                    }
                }
                if (postColliderMode == ColliderType.noCollider) //0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
                {
                    for (int i = 0; i < postsPool.Count; i++)
                    {
                        GameObject rail = postsPool[i].gameObject;
                        BoxCollider boxCollider = rail.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            DestroyImmediate(boxCollider);

                        MeshCollider meshCollider = rail.GetComponent<MeshCollider>();
                        if (meshCollider != null)
                            DestroyImmediate(meshCollider);
                    }
                }
                if (extraColliderMode == ColliderType.noCollider) //0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
                {
                    for (int i = 0; i < ex.extrasPool.Count; i++)
                    {
                        GameObject rail = ex.extrasPool[i].gameObject;
                        BoxCollider boxCollider = rail.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            DestroyImmediate(boxCollider);

                        MeshCollider meshCollider = rail.GetComponent<MeshCollider>();
                        if (meshCollider != null)
                            DestroyImmediate(meshCollider);
                    }
                }
            }
        }

        //--------------------------
        private void RemoveAllCollidersForAllLayers()
        {
            RemoveAllCollidersForLayer(LayerSet.railALayer);
            RemoveAllCollidersForLayer(LayerSet.railBLayer);
            RemoveAllCollidersForLayer(LayerSet.postLayer);
            RemoveAllCollidersForLayer(LayerSet.extraLayer);
            RemoveAllCollidersForLayer(LayerSet.subpostLayer);
        }

        //--------------------------
        private void RemoveAllCollidersForLayer(LayerSet layer)
        {
            
            List<Transform> pool = GetPoolForLayer(layer);
            if(pool == null)
                return;
            int count = pool.Count;
            for (int i = 0; i < count; i++)
            {
                if(pool[i] == null)
                    return;
                GameObject go = pool[i].gameObject;
                if(go == null)
                    return;
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    DestroyImmediate(collider);
                }
            }
        }

        //--------------------------
        //- Also removes children
        private void RemoveAllCollidersFromGO(GameObject go, ColliderType colliderType = ColliderType.allColliders)
        {
            foreach (Collider collider in go.GetComponentsInChildren<Collider>())
            {
                DestroyImmediate(collider);
            }
        }
    }
}