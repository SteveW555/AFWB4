using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace AFWB
{


    // Stores a pair of floating-point numbers and provides multiplication.
    internal struct NumberPair
    {
        public float a; // First floating-point number
        public float b; // Second floating-point number

        public NumberPair(float a, float b) // Constructor to initialize the numbers
        {
            this.a = a;
            this.b = b;
        }
    }

    public partial class AutoFenceCreator
    {
        //==================================================================
        //      Create a Pool of Posts and Rails
        //      We only need the most basic psuedo-pool to allocate enough GameObjects (and resize when needed)
        //      They get activated/deactivated when necessary
        //      As memory isn't an issue at runtime (once the fence is built/finalized, there is NO pool, only the actual objects used), allocating 25% more
        //      GOs each time reduces the need for constant pool-resizing and laggy performance in the editor.
        //===================================================================
        ///<summary>Calls Destroy() then Create() for each pool</summary>
        public void ResetAllPools()
        {
            ResetPoolForLayer(LayerSet.railALayerSet);
            ResetPoolForLayer(LayerSet.railBLayerSet);
            ResetPoolForLayer(LayerSet.postLayerSet);
            ResetPoolForLayer(LayerSet.subpostLayerSet);
            ResetPoolForLayer(LayerSet.extraLayerSet);
        }

        //---------------
        public void ResetPoolForLayer(LayerSet layer)
        {
            DestroyPoolForLayer(layer);
            CreatePoolForLayer(layer);

            if (layer == LayerSet.postLayerSet)
            {
                DestroyNodeMarkers();
                ResetNodeMarkerPool();
            }
        }

        //-------------------------------------------------------
        public void CreateAllPools([CallerMemberName] string caller = null)
        {
            bool enableDebugLogs = false;
            bool onlyCreateWhenLayerEnabled = false;
            int requiredPoolSize = GetDefaultPoolSizeForLayer(LayerSet.postLayerSet);
            bool append = true;

            if (caller == "OnInspectorGUI")
            {
                Debug.LogWarning("CreateAllPools() - Called by OnInspectorGUI");
            }

            CreatePoolForLayer(LayerSet.postLayerSet, requiredPoolSize, append, enableDebugLogs, onlyCreateWhenLayerEnabled, caller);
            CreatePoolForLayer(LayerSet.subpostLayerSet, requiredPoolSize, append, enableDebugLogs, onlyCreateWhenLayerEnabled, caller);
            CreatePoolForLayer(LayerSet.railALayerSet, requiredPoolSize, append, enableDebugLogs, onlyCreateWhenLayerEnabled, caller);
            CreatePoolForLayer(LayerSet.railALayerSet, requiredPoolSize, append, enableDebugLogs, onlyCreateWhenLayerEnabled, caller);
            CreatePoolForLayer(LayerSet.extraLayerSet, requiredPoolSize, append, enableDebugLogs, onlyCreateWhenLayerEnabled, caller);
        }



        /////-----------------------
        private void CreatePostsPool(LayerSet layer, int requiredPoolSize, bool enableDebugLogs, string caller, List<Transform> pool, bool append = false)
        {
            int origPoolSize = pool.Count;
            try
            {
                int start = 0;
                if (append == true)
                    start = origPoolSize;

                // Add new items to the pool if required
                if (origPoolSize < requiredPoolSize)
                {
                    for (int postIndex = start; postIndex < requiredPoolSize; postIndex++)
                    {
                        GameObject postPrefab = null;
                        postPrefab = GetCurrentPrefabForLayer(layer);

                        //-- Set the Post Node or Ends override Prefab
                        if (layer != LayerSet.subpostLayerSet)
                        {
                            postPrefab = GetNodePostsOverridePrefab(postIndex, postPrefab);

                            //if (postIndex < allPostPositions.Count)
                            //Debug.Log($"Node PostPrefab: {postPrefab.name}\n");
                            postPrefab = GetEndPostsOverridePrefab(postIndex, postPrefab);
                        }

                        if (allowEndPostsPrefabOverride && (postIndex == 0 || postIndex == allPostPositions.Count - 1))
                            postPrefab = GetEndPostsOverridePrefabForLayer(layer);

                        GameObject item = Instantiate(postPrefab);
                        item.SetActive(false);
                        item.hideFlags = HideFlags.HideInHierarchy;
                        pool.Add(item.transform);
                    }
                }
                else if (enableDebugLogs)
                {
                    Debug.Log("CreatePoolForLayer() - No new items added; existing pool is sufficient.\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: CreatePoolForLayer() - Error: {ex.Message}. Called by {caller}\n");
            }
        }
        //--------------------
        GameObject GetNodePostsOverridePrefab(int postIndex, GameObject currPrefab)
        {
            GameObject postPrefab = null;
            //-- At this point the PostVector might not exist if it's for pool items in excess of the number of posts So test for clickpoint carefully
            bool poolItemIsMoreThanNeeded = postIndex >= allPostPositions.Count;
            if (poolItemIsMoreThanNeeded == false && allowNodePostsPrefabOverride && PostVector.IndexIsClickPointNode(postIndex))
                postPrefab = GetMainPostOverridePrefabForLayer(LayerSet.postLayerSet);
            else
                postPrefab = currPrefab;
            return postPrefab;
        }
        //
        ////--------------------
        GameObject GetEndPostsOverridePrefab(int postIndex, GameObject currPrefab)
        {
            GameObject postPrefab = null;
            if (allowEndPostsPrefabOverride && (postIndex == 0 || postIndex == allPostPositions.Count - 1))
                postPrefab = GetEndPostsOverridePrefabForLayer(LayerSet.postLayerSet);
            else
                postPrefab = currPrefab;
            return postPrefab;
        }
        //----------------------------------
        // caled from  CreateAllPools(), ResetPoolForLayer(), ValidatePoolForLayer( Fails ), RequestSub(), RebuildPoolWithNewUserPrefab()
        public int CreatePoolForLayer(LayerSet layer, int requiredPoolSize = 0, bool append = false,
                              bool enableDebugLogs = false, bool onlyCreateWhenLayerEnabled = false,
                              [CallerMemberName] string caller = null)
        {
            // Set required pool size to default if not provided
            if (requiredPoolSize == 0)
                requiredPoolSize = GetDefaultPoolSizeForLayer(layer);

            //-- Initialize or retrieve the current pool
            List<Transform> pool = CheckAndInitPool(layer, append, enableDebugLogs, caller);
            int origPoolSize = pool.Count, newPoolCount = 0; ;

            //      Rails
            //====================
            if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
                pool = CreateRailsPool(layer, requiredPoolSize, append, enableDebugLogs, onlyCreateWhenLayerEnabled);

            //      Post & Subposts
            //==========================
            else if (layer == LayerSet.postLayerSet || layer == LayerSet.subpostLayerSet)
            {
                CreatePostsPool(layer, requiredPoolSize, enableDebugLogs, caller, pool, append: false);
            }

            //      Extras
            //====================
            //-- Now called direct from BuildExtras() as it's a special case which might need frequent Mesh updates
            /*else if (layer == LayerSet.extraLayerSet)
                pool = ex.CreateExtrasPool(requiredPoolSize, append, true);*/


            //Debug.Log($"CreatePoolForLayer() - {GetLayerNameAsString(layer)} Pool size: {pool.Count} (was {origPoolSize}) Created {pool.Count - origPoolSize} items\n");
            if (pool.Count != GetPoolForLayer(layer).Count)
            {
                Debug.LogError($"Mismatched Pool Reference!  {layer}   {pool.Count}     {GetPoolForLayer(layer).Count} \n");
                // Ensure the original pool reference is updated
                SetPoolForLayer(layer, pool);
            }
            return pool.Count;
        }
        //--------------------------------
        public List<Transform> CreateRailsPool(LayerSet layer, int requiredPoolSize, bool append = false, bool enableDebugLogs = false, bool onlyCreateWhenLayerEnabled = false, [CallerMemberName] string caller = null)
        {
            List<Transform> railPool = GetPoolForLayer(layer);
            try
            {
                int currentPoolSize = railPool.Count;
                //- Main function to create the pool
                if (append == false || currentPoolSize < requiredPoolSize)
                {
                    AddRailsToPool(railPool, layer, currentPoolSize, requiredPoolSize, enableDebugLogs);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"CreateRailsPool() - Error: {ex.Message}\n");
            }
            return railPool;
        }
        private void AddRailsToPool(List<Transform> railPool, LayerSet layer, int currentPoolSize, int requiredPoolSize, bool enableDebugLogs)
        {
            List<SourceVariant> variants = GetSourceVariantsForLayer(layer);
            List<SeqItem> seqItems = GetSequenceForLayer(layer);
            bool useRailVar = useRailVariations[layer.Int()];
            Sequencer sequencer = GetSequencerForLayer(layer);
            bool useSequencer = sequencer.useSeq;
            bool useSingles = useRailSingles[layer.Int()];
            SinglesItem single = null;
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);


            GameObject go = null, mainGo = GetMainPrefabForLayer(layer);
            SourceVariant variant = null;
            for (int i = currentPoolSize; i < requiredPoolSize; i++)
            {
                if (useRailVar == true && (useSequencer == true || useSingles == true))
                {
                    //-- Get the Go from the source variant for this step
                    if (useSequencer == true)
                    {
                        /*if (variant == null || variant.Go == null)
                        {
                            if (enableDebugLogs)
                                Debug.Log($"CreateRailsPool() - Variant or GameObject is null at index {i}");
                            continue;
                        }*/
                        variant = GetVariantAtSeqIndexForLayer(variants, seqItems, i);
                        go = variant.Go;
                    }
                    else if (useSingles == true)
                    {
                        go = GetGoForSingleAtIndexForLayer(i, layer);
                    }
                    if (go == null)
                        go = mainGo;
                }
                else
                    go = mainGo;
                GameObject rail = Instantiate(go);
                rail.SetActive(false);
                rail.hideFlags = HideFlags.HideInHierarchy;
                if (useRailVariations[layer.Int()] == true)
                    ApplyTransformations(rail, variant, seqItems, i);
                railPool.Add(rail.transform);
            }

            if (enableDebugLogs)
                Debug.Log($"CreateRailsPool() - Added {requiredPoolSize - currentPoolSize} new rails to the pool.");
        }

        private GameObject GetGoForSingleAtIndexForLayer(int index, LayerSet layer)
        {
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            SinglesContainer singlesContainer = GetSinglesContainerForLayer(layer);
            SinglesItem single = singlesContainer.GetSingleAtSectionIndex(index);
            int sourceVariantIndex = single.sourceVariantIndex;
            SourceVariant variant = sourceVariants[sourceVariantIndex];
            GameObject go = variant.Go;
            return go;
        }

        private SinglesContainer GetSinglesContainerForLayer(LayerSet layer)
        {
            SinglesContainer singlesContainer = null;
            if (layer == LayerSet.railALayerSet)
                singlesContainer = railSinglesContainer[0];
            else if (layer == LayerSet.railBLayerSet)
                singlesContainer = railSinglesContainer[1];
            else if (layer == LayerSet.postLayerSet)
                singlesContainer = postSinglesContainer;

            return singlesContainer;
        }

        //-------------------------------
        // Validate the pool List
        /// <summary> Called before creating/appending a Pool. Checks the validity of the List() for the Pool first, not the contents of the Pool.
        /// Once the List() is healthy, and depending on 'append' we can  either Add() to any existing content (the transforms), or Destroy the content for a fresh start
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="append"></param>
        /// <param name="enableDebugLogs"></param>
        /// <returns>The fresh or Appendable Pool</returns>
        /// called from CreatePoolForLayer(), CreateRailsPool(),
        private List<Transform> CheckAndInitPool(LayerSet layer, bool append, bool enableDebugLogs = true, [CallerMemberName] string caller = null)
        {
            List<Transform> pool = railsAPool;
            string layerName = GetLayerNameAsString(layer);

            if (pool == null)
            {
                Debug.LogWarning($"CheckAndInitPool() - {layerName} Pool is null, creating new Pool List.  Called by {caller}\n");
                pool = new List<Transform>();
            }
            else if (pool.Count == 0)
                // No need to do anything, just return the empty pool and it will get build in the CreatePoolForLayer() method

                //- Are there any null objects in the List
                if (pool.Count > 0 && pool[0] == null)
                {
                    Debug.LogWarning($"CheckAndInitPool() - {layerName} Pool's first object is null, creating new Pool List.  Called by {caller}\n");
                    pool = new List<Transform>(); // Just rebuild it from scratch
                }
            if (append == false && pool.Count > 0)
            {
                DestroyPoolForLayer(layer, enableDebugLogs: false);
                pool = new List<Transform>();
            }
            SetPoolForLayer(layer, pool);
            return pool;
        }

        //---------------------------------------------
        // Do not Fix here, could cause infinite loop
        public List<Transform> GetPoolForLayer(LayerSet layer, [CallerMemberName] string caller = null)
        {
            List<Transform> pool = null;

            switch (layer)
            {
                case LayerSet.railALayerSet:
                    pool = railsAPool;
                    break;

                case LayerSet.railBLayerSet:
                    pool = railsBPool;
                    break;

                case LayerSet.postLayerSet:
                    pool = postsPool;
                    break;

                case LayerSet.extraLayerSet:
                    pool = ex.extrasPool;
                    break;

                case LayerSet.subpostLayerSet:
                    pool = subpostsPool;
                    break;
            }

            if (pool == null)
                Debug.LogWarning($"GetPoolForLayer() - {layer.String()} Pool is null.\n Called from {caller}\n");

            else if (pool.Count > 0 && pool[0] == null)
            {
                Debug.LogWarning($"GetPoolForLayer() - {layer.String()}     Pool's first object is null. Nullyfying Pool\n" +
                    $"Called from {caller}\n");
                pool = null;
            }
            return pool;
        }

        //-----------------------

        private void SetPoolForLayer(LayerSet layer, List<Transform> pool)
        {
            switch (layer)
            {
                case LayerSet.railALayerSet:
                    railsAPool = pool;
                    break;

                case LayerSet.railBLayerSet:
                    railsBPool = pool;
                    break;

                case LayerSet.postLayerSet:
                    postsPool = pool;
                    break;

                case LayerSet.extraLayerSet:
                    ex.extrasPool = pool;
                    break;

                case LayerSet.subpostLayerSet:
                    subpostsPool = pool;
                    break;
            }
        }


        /*private List<string> InitializeAndCheckEarlyReturn(LayerSet layer, bool enableDebugLogs, bool onlyCreateWhenLayerEnabled)
        {
            List<string> debugMessages = new List<string>();

            if (onlyCreateWhenLayerEnabled && !IsLayerEnabled(layer))
            {
                if (enableDebugLogs)
                    Debug.Log("CreateRailsPool() - Layer is not enabled, returning early.");
            }
            return debugMessages;
        }*/

        //-----------------------
        private int CalculateRequiredPoolSize(LayerSet layer)
        {
            List<SourceVariant> variants = GetSourceVariantsForLayer(layer);
            List<SeqItem> seqItems = GetSequenceForLayer(layer);

            float excessFactor = 1.2f;
            int numVariants = variants.Count;
            int numSeqSteps = seqItems.Count;

            return Mathf.CeilToInt(numVariants * numSeqSteps * excessFactor);
        }



        private SourceVariant GetVariantAtSeqIndexForLayer(List<SourceVariant> variants, List<SeqItem> seqItems, int index)
        {
            if (index < seqItems.Count)
            {
                int variantIndex = seqItems[index].sourceVariantIndex;
                if (variantIndex < variants.Count)
                {
                    return variants[variantIndex];
                }
            }
            return variants.FirstOrDefault() ?? new SourceVariant();
        }
        /// <summary>
        /// Applies Sequencer transformations to the rail GameObject.transform. Does not modify mesh
        /// </summary>
        private void ApplyTransformations(GameObject rail, SourceVariant variant, List<SeqItem> seqItems, int index)
        {
            if (index < seqItems.Count)
            {
                SeqItem seqItem = seqItems[index];
                rail.transform.position += seqItem.pos;
                rail.transform.localScale = Vector3.Scale(rail.transform.localScale, seqItem.size);
                rail.transform.rotation *= Quaternion.Euler(seqItem.rot);

                if (seqItem.invert)
                {
                    rail.transform.localScale = new Vector3(rail.transform.localScale.x, rail.transform.localScale.y, -rail.transform.localScale.z);
                }
                if (seqItem.backToFront)
                {
                    rail.transform.Rotate(0, 180, 0);
                }
                if (seqItem.mirrorZ)
                {
                    rail.transform.localScale = new Vector3(rail.transform.localScale.x, rail.transform.localScale.y, -rail.transform.localScale.z);
                }
            }
        }

        //-----------------------------
        public int GetDefaultPoolSizeForLayer(LayerSet layer)
        {
            //- Calc this as the others are based on it
            int numPosts = allPostPositions.Count;
            //This keeps the calculation consistent at startup or after a ClearAll()
            if (numPosts == 0)
                numPosts = 1;
            //-- Aim to make the pool approx 10% bigger than the number of posts, and then another 10 on top of that
            float excessFactor = 1.05f;
            int defaultNumPosts = (int)((allPostPositions.Count + 1) * excessFactor + 10);
            int requiuredPoolSize = 0;
            switch (layer)
            {
                case LayerSet.postLayerSet:
                    requiuredPoolSize = defaultNumPosts;
                    break;

                case LayerSet.railALayerSet:
                    requiuredPoolSize = defaultNumPosts * (int)numStackedRails[kRailALayerInt] * 2;
                    break;

                case LayerSet.railBLayerSet:
                    requiuredPoolSize = defaultNumPosts * (int)numStackedRails[kRailBLayerInt];
                    break;

                case LayerSet.subpostLayerSet:
                    requiuredPoolSize = defaultNumPosts * numSubpostsPerSection;
                    break;

                case LayerSet.extraLayerSet:
                    //return numSubpostsPerSection * 10;
                    requiuredPoolSize = 5;
                    break;

                default:
                    requiuredPoolSize = 7;
                    break;
            }
            return requiuredPoolSize;
        }

        private Transform RequestSub(int index)
        {
            if (index >= subpostsPool.Count - 1)
            {
                //CreateSubpostsPool((int)(subpostsPool.Count * 0.25f), true); // add 25% more, append is true
                CreatePoolForLayer(LayerSet.subpostLayerSet, subpostsPool.Count);
            }
            return subpostsPool[index];
        }

        //---------------------------
        public int DestroyPoolForLayer(LayerSet layer, bool enableDebugLogs = false)
        {
            List<Transform> pool = GetPoolForLayer(layer);
            string layerName = GetLayerNameAsString(layer);

            if (pool == null)
            {
                if (enableDebugLogs) Debug.Log($"Destroy{layerName}s()  called but {layerName} Pool is null");
                return 0;
            }

            int destroyedCount = 0;
            foreach (var item in pool)
            {
                if (item != null)
                {
                    DestroyImmediate(item.gameObject);
                    destroyedCount++;
                }
                else
                {
                    if (enableDebugLogs) Debug.Log($"Destroy{layerName}s() encountered a null {layerName} in the pool.");
                }
            }
            pool.Clear();
            return destroyedCount;
        }

        //-----------------------

        public void DestroyAllPools([CallerMemberName] string caller = null, bool enableDebugLogs = true)
        {
            // List to collect debug messages
            List<string> debugMessages = new List<string>();

            // Counter for tracking destroyed items in each pool
            int destroyedNodeMarkersCount = 0;
            int destroyedRailsACount = 0, destroyedRailsBCount = 0;
            int destroyedSubpostsCount = 0;
            int destroyedExtrasCount = 0;

            try
            {
                int destroyedPostsCount = DestroyPoolForLayer(LayerSet.postLayerSet);
                destroyedPostsCount = DestroyPoolForLayer(LayerSet.railALayerSet);
                destroyedPostsCount = DestroyPoolForLayer(LayerSet.railBLayerSet);
                destroyedSubpostsCount = DestroyPoolForLayer(LayerSet.subpostLayerSet);
                destroyedExtrasCount = DestroyPoolForLayer(LayerSet.extraLayerSet);
                DestroyNodeMarkers();
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred in DestroyAllPools(): {ex.Message} \n Called by {caller}\n");
            }
        }

        //----------------------
        private int DestroySubjoiners()
        {
            int destroyedSubJoinersCount = 0;
            foreach (var subJoiner in subJoiners)
            {
                if (subJoiner != null)
                {
                    DestroyImmediate(subJoiner.gameObject);
                    destroyedSubJoinersCount++;
                }
                else
                    Debug.Log("DestroySubposts() encountered a null subJoiner in the pool.");
            }
            subJoiners.Clear();
            return destroyedSubJoinersCount;
        }

        //-----------------------------

        //Called from SetupEditor(),  ForceRebuildFromClickPoints(),  RebuildFromFinalList() ( and OnInspectorGUI() for posts)

        /// <summary>Validates the pools for each layer and optionally Updates them by creating new pools if validation fails.</summary>
        /// <param name="caller">The name of the method or property that called this method. This parameter is optional and is automatically provided by the compiler if not specified.</param>
        /// <param name="update">A boolean indicating whether to update the pool if validation fails. If set to true (default), the method will create a new pool for the layer if the validation fails.</param>
        public void ValidateAndUpdatePools(bool warn = true, [CallerMemberName] string caller = null, bool update = true)
        {

            // Validate and optionally update the pool for the rail A layer
            ValidatePoolForLayer(LayerSet.railALayerSet, rebuildBadPool: true, warn, caller);
            ValidatePoolForLayer(LayerSet.railBLayerSet, rebuildBadPool: true, warn, caller);
            ValidatePoolForLayer(LayerSet.postLayerSet, rebuildBadPool: true, warn, caller);
            ValidatePoolForLayer(LayerSet.extraLayerSet, rebuildBadPool: true, warn, caller);
            ValidatePoolForLayer(LayerSet.subpostLayerSet, rebuildBadPool: true, warn, caller);

        }

        //--------------------------
        /// <summary>
        /// Check if the pool for the given layer is valid. Checks null Pool, Pool length, and Go is not null.
        /// Reorts any issues to the console. Does not update. Call CreatePoolForLayer() to update.
        /// It's ok for the pool to be empty, but not null.
        /// <para></para>
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>

        public bool ValidatePoolForLayer(LayerSet layer, bool rebuildBadPool = false, bool warn = true, [CallerMemberName] string caller = null)
        {
            //Debug.Log($"ValidatePoolForLayer( {layer}) - Called by {caller}\n");

            bool poolOK = true;
            string warnString = "";
            int defaultPoolSize = GetDefaultPoolSizeForLayer(layer);

            //-- Check Null Pool
            List<Transform> pool = GetPoolForLayer(layer);
            if (pool == null)
            {
                warnString = $"ValidatePoolForLayer() - {GetLayerNameAsString(layer)} Pool is null.  Caller:  {caller}\n";
                if (rebuildBadPool == true)
                    CreatePoolForLayer(layer, defaultPoolSize, false, false, false, caller);
                poolOK = false;
            }

            int poolCount = pool.Count;

            //-- Check Count
            if ((pool.Count < defaultPoolSize) || pool.Count == 0)
            {
                if (layer != LayerSet.extraLayerSet)
                    warnString += $"ValidatePoolForLayer() - {GetLayerNameAsString(layer)}  pool count ( {poolCount} ) " +
                        $"is less than default size ( {defaultPoolSize} )    Called by {caller}\n";
                poolOK = false;
            }

            //-- Check for null object in the pool
            if (pool.Count > 0 && pool[0] == null)
            {
                warnString += $"ValidatePoolForLayer() - {GetLayerNameAsString(layer)} first object in pool is null. " +
                    $"There are {poolCount} objects in the pool   Called by {caller}\n";
                pool.Clear(); //- we should clear it as it only contains nulls
                poolOK = false;
            }

            if (poolOK == false && rebuildBadPool == true)
                warnString += ($"Rebuilding {GetLayerNameAsString(layer)} Pool [{defaultPoolSize}]\n");

            if (rebuildBadPool == true)
                CreatePoolForLayer(layer, defaultPoolSize, false, false, false, caller);

            if (warn && poolOK == false && layer != LayerSet.extraLayerSet)
                Debug.LogWarning(warnString);

            return poolOK;
        }

        //--------------
        public void DeactivateEntirePoolForLayer(LayerSet layerSet = LayerSet.allLayerSet)
        {
            CheckAllPrefabsLists();

            if (layerSet == kAllLayer || layerSet == kPostLayer)
            {
                for (int i = 0; i < postsPool.Count; i++)
                {
                    if (postsPool[i] != null)
                    {
                        postsPool[i].gameObject.SetActive(false);
                        postsPool[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                        postsPool[i].position = Vector3.zero;
                    }
                }
            }

            if (layerSet == kAllLayer || layerSet == kRailALayer)
            {
                for (int i = 0; i < railsAPool.Count; i++)
                {
                    if (railsAPool[i] != null)
                    {
                        railsAPool[i].gameObject.SetActive(false);
                        railsAPool[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
            }

            if (layerSet == kAllLayer || layerSet == kRailBLayer)
            {
                for (int i = 0; i < railsBPool.Count; i++)
                {
                    if (railsBPool[i] != null)
                    {
                        railsBPool[i].gameObject.SetActive(false);
                        railsBPool[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
            }

            if (layerSet == kAllLayer || layerSet == kSubpostLayer)
            {
                for (int i = 0; i < subpostsPool.Count; i++)
                {
                    if (subpostsPool[i] != null)
                    {
                        subpostsPool[i].gameObject.SetActive(false);
                        subpostsPool[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
                for (int i = 0; i < subJoiners.Count; i++)
                {
                    if (subJoiners[i] != null)
                    {
                        subJoiners[i].gameObject.SetActive(false);
                        subJoiners[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
            }
            // extra objects
            if (layerSet == kAllLayer || layerSet == kExtraLayer)
            {
                for (int i = 0; i < ex.extrasPool.Count; i++)
                {
                    if (ex.extrasPool[i] != null)
                    {
                        ex.extrasPool[i].gameObject.SetActive(false);
                        ex.extrasPool[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                        ex.extrasPool[i].position = Vector3.zero;
                    }
                }
            }
            // postVizMarkers
            /*for (int postIndex = 0; postIndex < nodeMarkersPool.Count; postIndex++)
            {
                if (nodeMarkersPool[postIndex] != null)
                {
                    nodeMarkersPool[postIndex].gameObject.SetActive(false);
                    nodeMarkersPool[postIndex].gameObject.hideFlags = HideFlags.HideInHierarchy;
                    nodeMarkersPool[postIndex].position = Vector3.zero;
                }
            }*/
        }

        //--------------------------
        //We created pools of rails/postsPool/af.ex.extrasPool for efficiency, and hid/set inactive the one we weren't using.
        //This destroys all those unused game objetcs.
        public void DestroyUnused()
        {
            for (int i = 0; i < postsPool.Count; i++)
            {
                if (postsPool[i].gameObject != null)
                {
                    if (postsPool[i].gameObject.hideFlags == HideFlags.HideInHierarchy && postsPool[i].gameObject.activeSelf == false)
                        DestroyImmediate(postsPool[i].gameObject);
                }
            }
            for (int i = 0; i < railsAPool.Count; i++)
            {
                if (railsAPool[i].gameObject != null)
                {
                    if (railsAPool[i].gameObject.hideFlags == HideFlags.HideInHierarchy && railsAPool[i].gameObject.activeSelf == false)
                        DestroyImmediate(railsAPool[i].gameObject);
                }
            }
            for (int i = 0; i < railsBPool.Count; i++)
            {
                if (railsBPool[i].gameObject != null)
                {
                    if (railsBPool[i].gameObject.hideFlags == HideFlags.HideInHierarchy && railsBPool[i].gameObject.activeSelf == false)
                        DestroyImmediate(railsBPool[i].gameObject);
                }
            }
            for (int i = 0; i < subpostsPool.Count; i++)
            {
                if (subpostsPool[i].gameObject != null)
                {
                    if (subpostsPool[i].gameObject.hideFlags == HideFlags.HideInHierarchy && subpostsPool[i].gameObject.activeSelf == false)
                    {
                        DestroyImmediate(subpostsPool[i].gameObject);
                        if (subJoiners[i].gameObject != null)
                            DestroyImmediate(subJoiners[i].gameObject);
                    }
                }
            }
            for (int i = 0; i < ex.extrasPool.Count; i++)
            {
                if (ex.extrasPool[i].gameObject != null)
                {
                    if (ex.extrasPool[i].gameObject.hideFlags == HideFlags.HideInHierarchy && ex.extrasPool[i].gameObject.activeSelf == false)
                        DestroyImmediate(ex.extrasPool[i].gameObject);
                }
            }

            DestroyNodeMarkers();
        }

        //---------------------------------------------
        /// <summary>Prints debug information about the pools for each layer up to subpostLayerSet.</summary>
        /// <param name="msg">A message to include in the debug output.</param>
        public bool PrintPoolDebugInfo(string msg, bool printDetailed = true, bool noPrintOnSuccess = false)
        {
            bool allPopulated = true; // Variable to track if all pools are populated
            string emptyLayers = ""; // String to store layers with count 0
            string nullLayers = ""; // String to store layers with null contents
            int[] poolCounts = new int[5];
            int emptyPoolCount = 0;

            int i = 0;
            foreach (LayerSet layer in Enum.GetValues(typeof(LayerSet)))
            {
                if (layer > LayerSet.subpostLayerSet)
                    break;

                List<Transform> pool = GetPoolForLayer(layer);
                if (pool == null)
                {
                    poolCounts[i] = -1;
                    nullLayers += $"{layer.ToString()} Pool was null";
                    Debug.LogWarning($"{layer.ToString()} Pool was null");
                    continue;
                }

                int count = pool.Count;
                poolCounts[i] = count;
                bool isPopulated = pool.All(item => item != null);
                if (printDetailed == true && noPrintOnSuccess == false)
                    Debug.LogWarning($"[{msg}]  Pool Layer({layer}),   Count: {count},   Is Populated with Transforms: {isPopulated}\n");

                if (count == 0)
                {
                    emptyLayers += $"{layer.ToString()}, ";
                    emptyPoolCount++;
                }
                else if (!isPopulated)
                    nullLayers += $"{layer.ToString()} had count of {count} but had null contents, ";

                // updates the value of the "allPopulated" variable by  a logical AND operation with "isPopulated". "allPopulated" is used to track if
                // ALL elements in a layer are populated. If any element is not populated (isPopulated = false), then the "allPopulated" variable will be set to false.
                allPopulated = allPopulated && isPopulated;
                i++;
            }

            if (nullLayers != "")
                Debug.LogWarning($"{nullLayers}\n");

            if (allPopulated == true && noPrintOnSuccess == true)
                return allPopulated;

            string resultStr = $"[{msg}]  All Pool Layers Populated =  {allPopulated}  ";
            if (emptyPoolCount > 0)
                resultStr += $"There were {emptyPoolCount} Empty Layers";
            else
                resultStr += "\n";
            //if (printDetailed == false)
            //Debug.LogWarning($"{resultStr}    A[{poolCounts[0]}]  B[{poolCounts[1]}]  " +$"Post[{poolCounts[2]}]  Extra[{poolCounts[3]}]  Sub[{poolCounts[4]}] \n");

            return allPopulated;
        }

        //---------------------------------------------
        public void PrintPoolInfo()
        {
            //Print information about all the pools, one line for each layer

            Debug.Log($"Pool Info:  for {allPostPositions.Count} sections \n");
            Debug.Log($"Posts:   {postsPool.Count} \n");
            Debug.Log($"RailsA:  {railsAPool.Count} \n");
            Debug.Log($"RailsB:  {railsBPool.Count} \n");
            Debug.Log($"Subposts:  {subpostsPool.Count} \n");
            Debug.Log($"Extras:  {ex.extrasPool.Count} \n");
            Debug.Log($"Markers:  {nodeMarkersPool.Count} \n");
        }

        //-------------
        private void ResetNodeMarkerPool()
        {
            int numToCreate = clickPoints.Count(); //v4.0 11/12/23

            DestroyNodeMarkers();
            nodeMarkersPool = new List<Transform>();

            if (nodeMarkerObj != null)
            {
                for (int i = 0; i < numToCreate; i++)
                {
                    GameObject nodeMarker = Instantiate(nodeMarkerObj, Vector3.zero, Quaternion.identity) as GameObject;
                    nodeMarker.SetActive(false);
                    nodeMarker.hideFlags = HideFlags.HideInHierarchy;
                    nodeMarkersPool.Add(nodeMarker.transform);
                    nodeMarker.transform.parent = postsFolder.transform;
                }
            }
            else
                Debug.Log("ResetNodeMarkerPool() - nodeMarkerObj GameObject is null");
        }

        //--------------
        public void DestroyNodeMarkers()
        {
            //destroy all the node markers
            if (nodeMarkersPool != null)
            {
                for (int i = 0; i < nodeMarkersPool.Count; i++)
                {
                    if (nodeMarkersPool[i] != null)
                    {
                        DestroyImmediate(nodeMarkersPool[i].gameObject);
                    }
                }
                nodeMarkersPool.Clear();
            }
            else
                Debug.Log("DestroyNodeMarkers() - nodeMarkersPoo was null");
        }

        //--------------------------
        public GameObject GetGOFromPoolAtIndexForLayer(int sectionIndex, LayerSet layer)
        {
            List<Transform> layerPool = GetPoolForLayer(layer);
            if (layerPool == null)
                return null;
            if (layerPool.Count == 0 || layerPool.Count < sectionIndex)
            {
                Debug.LogWarning($"GetGOFromPoolAtIndexForLayer()  {GetLayerNameAsString(layer)} sectionIndex: {sectionIndex}   layerPool.Count = {layerPool.Count}\n");
                return null;
            }
            if (layerPool[sectionIndex] == null)
            {
                Debug.LogWarning($"GetGOFromPoolAtIndexForLayer()  {GetLayerNameAsString(layer)} layerPool[ {sectionIndex} ] was null\n");
                return null;
            }
            return layerPool[sectionIndex].gameObject;
        }

        //--------------------------
        public int GetPoolCountForLayer(LayerSet layer)
        {
            List<Transform> layerPool = GetPoolForLayer(layer);
            if (layerPool != null)
                return layerPool.Count;
            return 0;
        }

        public List<string> CreateSubpostJoinerPool(int requiredPoolSize, bool append = true, bool enableDebugLogs = false, bool onlyCreateWhenLayerEnabled = false)
        {
            // List to collect debug messages
            List<string> debugMessages = new List<string>();

            // Early return if only creating when layer is enabled and the layer is not enabled
            if (onlyCreateWhenLayerEnabled && !IsLayerEnabled(LayerSet.subpostLayerSet))
            {
                if (enableDebugLogs)
                    Debug.Log("CreateSubpostJoinerPool() - Layer is not enabled, returning early.");

                return debugMessages;
            }

            try
            {
                // Initialize subJoiners if it is null
                if (subJoiners == null)
                    subJoiners = new List<Transform>();

                // Check if we need to clear the existing pool
                if (!append)
                {
                    DestroySubpostJoiners();

                    if (enableDebugLogs)
                        Debug.Log("CreateSubpostJoinerPool() - Existing pool cleared.");
                }

                int currentPoolSize = subJoiners.Count;

                // Add new subpost joiners to the pool if required
                if (currentPoolSize < requiredPoolSize)
                {
                    for (int i = currentPoolSize; i < requiredPoolSize; i++)
                    {
                        GameObject subJoiner = Instantiate(subJoinerPrefabs[0]);
                        subJoiner.SetActive(false);
                        subJoiners.Add(subJoiner.transform);
                    }

                    if (enableDebugLogs)
                        Debug.Log($"CreateSubpostJoinerPool() - Added {requiredPoolSize - currentPoolSize} new subpost joiners to the pool.");
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.Log("CreateSubpostJoinerPool() - No new subpost joiners added; existing pool is sufficient.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"CreateSubpostJoinerPool() - Error: {ex.Message}\n");
            }

            // Print all collected debug messages at the end
            foreach (var message in debugMessages)
                Debug.Log(message);

            // Return the debug messages
            return debugMessages;
        }

        private void DestroySubpostJoiners()
        {
            if (subJoiners != null)
            {
                for (int i = 0; i < subJoiners.Count; i++)
                {
                    if (subJoiners[i] != null)
                    {
                        DestroyImmediate(subJoiners[i].gameObject);
                    }
                }
                subJoiners.Clear();
            }
            else
                Debug.Log("DestroySubpostJoiners() - subJoiners was null");
        }

        //---------------------- Allocation is handled by Subs ---------
        private GameObject RequestSubJoiner(int index)
        {
            if (subJoiners[index] == null || subJoiners[index].gameObject == null) return null;
            GameObject thisSubJoiner = subJoiners[index].gameObject;
            thisSubJoiner.hideFlags = HideFlags.None;
            thisSubJoiner.SetActive(true);
            thisSubJoiner.name = "SubJoiner " + index.ToString();
            thisSubJoiner.transform.parent = subpostsFolder.transform;
            return thisSubJoiner;
        }
    }
}