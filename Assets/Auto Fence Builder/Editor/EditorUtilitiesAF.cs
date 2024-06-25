using AFWB;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public partial class AutoFenceEditor
{


    /*public EditorUtils(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
{
af = autoFenceCreator;
ed = autoFenceEditor;
so = ed.serializedObject;
}*/

    //---------------------------------------
    //-- Ensure the SourceVariants Lists exist and are not null
    //-- Ensure they are all populated with kMaxNumSourceVariants SourceVariants
    //-- Ensure the SourceVariants GameObjectss are not null
    private bool CheckSourceVariantsExist([CallerMemberName] string caller = null)
    {
        //StackLog(this.GetType().Name, true);

        //-- Check/Create The SourceVariants Lists, one for each Layer
        bool neededFixing = CheckAllSourceVariantsListsExist();

        //-- Check/Create kMaxNumSourceVariants SourceVariants For Each Layer's List
        neededFixing = CheckSourceVariantsForEveryLayerAreValid();

        //-- Check that each SourceVariants GOs are not null
        neededFixing = af.CheckSourceVariantGosForAllLayersAreValid();
        return neededFixing;
    }

    //---------------------------------------
    //-- Check for Every Layer, that each SourceVariants List has kMaxNumSourceVariants SourceVariants
    //-- Replace null SVs with valid, but does NOT check that the SV GOs are not null, that's done in CheckSourceVariantGOs()
    private bool CheckSourceVariantsForEveryLayerAreValid()
    {
        bool neededFixing = af.CheckSourceVariantsForLayerAreValid(LayerSet.railALayer);
        neededFixing = af.CheckSourceVariantsForLayerAreValid(LayerSet.railBLayer);
        neededFixing = af.CheckSourceVariantsForLayerAreValid(LayerSet.postLayer);

        return neededFixing;
    }



    //================================================================================
    // Convenience Methods To Get Nested or List Serialized Properties
    //================================================================================
    public SerializedProperty GetSequencerListForLayerProp(LayerSet layer)
    {
        SerializedProperty seqListProp = railASeqListProp;
        if (layer == LayerSet.railBLayer)
            seqListProp = railBSeqListProp;
        else if (layer == LayerSet.postLayer)
            seqListProp = postSeqListProp;

        return seqListProp;
    }

    public SerializedProperty GetNumSourceVariantsInUseForLayerProp(LayerSet layer)
    {
        SerializedProperty numSourceVarsInUseProp = null;

        if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
        {
            SerializedProperty railVarsInUseArray = serializedObject.FindProperty("numRailVariantsInUse");
            numSourceVarsInUseProp = railVarsInUseArray.GetArrayElementAtIndex((int)layer);
        }
        else if (layer == LayerSet.postLayer)
            numSourceVarsInUseProp = serializedObject.FindProperty("_numPostVariantsInUse");
        else if (layer == LayerSet.extraLayer)
            numSourceVarsInUseProp = serializedObject.FindProperty("numExtraVariantsInUse");

        if (numSourceVarsInUseProp == null)
            Debug.Log($"numSourceVarsInUseProp is NULL in GetNumSourceVariantsInUseForLayerProp( {layer} )\n");

        return numSourceVarsInUseProp;
    }

    public SerializedProperty GetSeqItemAtStepIndexProp(LayerSet layer, int stepIndex)
    {
        SerializedProperty seqProp = GetSequencerListForLayerProp(layer); // e.g . railASequencer.seqList
        SerializedProperty SeqItemAtStepProp = seqProp.GetArrayElementAtIndex(stepIndex); // the individual SeqItem for this step

        if (SeqItemAtStepProp == null)
            Debug.Log($"SeqItemProp for Step {stepIndex}  in ( {layer} ) was null\n");

        return SeqItemAtStepProp;
    }

    // same as above but we already have the Sequencer Property
    public SerializedProperty GetSeqItemAtStepIndexProp(SerializedProperty sequenceProp, LayerSet layer, int stepIndex)
    {
        SerializedProperty SeqItemAtStepProp = sequenceProp.GetArrayElementAtIndex(stepIndex); // the individual SeqItem for this step

        if (SeqItemAtStepProp == null)
            Debug.Log($"SeqItemProp for Step {stepIndex}  in ( {layer} ) was null\n");

        return SeqItemAtStepProp;
    }

    public SerializedProperty GetSequencerSourceVariantAtStepIndexProp(LayerSet layer, int stepIndex)
    {
        SerializedProperty SeqItemAtStepProp = GetSeqItemAtStepIndexProp(layer, stepIndex);// the individual SeqItem for this step
        SerializedProperty sourceVariantIndexProp = SeqItemAtStepProp.FindPropertyRelative("sourceVariantIndex");// the index to a SourceVariant this seq step references

        if (sourceVariantIndexProp == null)
            Debug.Log($"SourceVariant Index Prop for Step {stepIndex}  in ( {layer} ) was null\n");

        return sourceVariantIndexProp;
    }

    // same as above but we already have the SeqItemProperty
    public SerializedProperty GetSequencerSourceVariantAtStepIndexProp(SerializedProperty SeqItemAtStepProp, LayerSet layer, int stepIndex)
    {
        SerializedProperty sourceVariantIndexProp = SeqItemAtStepProp.FindPropertyRelative("sourceVariantIndex");// the index to a SourceVariant this seq step references
        if (sourceVariantIndexProp == null)
            Debug.Log($"SourceVariant Index Prop for Step {stepIndex}  in ( {layer} ) was null in GetSequencerNumStepsForLayerProp()\n");
        return sourceVariantIndexProp;
    }

    public SerializedProperty GetSequencerNumStepsForLayerProp(LayerSet layer)
    {
        Sequencer sequence = af.GetSequencerForLayer(layer);

        //SerializedProperty numStepsProp = SeqItemAtStepProp.FindPropertyRelative


        if (layer == LayerSet.railALayer)
            return railASeqListProp;



        SerializedProperty numSeqStepsArrayProp = serializedObject.FindProperty("seqNumSteps");// the index to a SourceVariant this seq step references
        if (numSeqStepsArrayProp != null)
        {
            SerializedProperty numSeqStepsProp = numSeqStepsArrayProp.GetArrayElementAtIndex((int)layer);
            if (numSeqStepsProp == null)
                Debug.Log($"numSeqStepsProp for ( {layer} ) was null in GetSequencerNumStepsForLayerProp()\n");
            return numSeqStepsProp;
        }
        else
        {
            Debug.Log($"numSeqStepsArrayProp for ( {layer} ) was null in GetSequencerNumStepsForLayerProp()\n");
            return null;
        }
    }

    public SerializedProperty GetSourceVariantMenuListForLayer(LayerSet layer)
    {
        SerializedProperty sourceVariant_MenuIndex_List = serializedObject.FindProperty("railASourceVariant_MenuIndices"); // the railVariantMenuIndex[2] array
        if (layer == LayerSet.railALayer)
            sourceVariant_MenuIndex_List = serializedObject.FindProperty("railBSourceVariant_MenuIndices");
        else if (layer == LayerSet.postLayer)
            sourceVariant_MenuIndex_List = serializedObject.FindProperty("postSourceVariant_MenuIndex");

        if (sourceVariant_MenuIndex_List == null)
            Debug.Log($"sourceVariant_MenuIndex_List for ( {layer} ) was null in GetSequencerNumStepsForLayerProp()\n");

        return sourceVariant_MenuIndex_List;
    }

    private void CheckAllSourceVariantsArePoulated([CallerMemberName] string caller = null)
    {
        CheckAllLayerPrefabsExist(caller);

        serializedObject.Update();

        SerializedProperty railASourceVariant_MenuIndexProp = serializedObject.FindProperty("railASourceVariant_MenuIndices");

        int sourceVariantMenuIndexA = railASourceVariant_MenuIndexProp.GetArrayElementAtIndex(0).intValue;
        if (sourceVariantMenuIndexA == 0)
        {
            // the Variant menus haven't been set yet, probably because this is the first time the script has been enabled after instantiating AF
            return;
        }
        sourceVariantMenuIndexA = 0;

        for (int i = 0; i < AutoFenceCreator.kMaxNumSourceVariants; i++)
        {
            sourceVariantMenuIndexA = railASourceVariant_MenuIndexProp.GetArrayElementAtIndex(i).intValue;
            int menuIndexB = railASourceVariant_MenuIndexProp.GetArrayElementAtIndex(i).intValue;
            int menuIndexPost = railASourceVariant_MenuIndexProp.GetArrayElementAtIndex(i).intValue;

            int prefabIndexA = af.ConvertMenuIndexToPrefabIndexForLayer(sourceVariantMenuIndexA, PrefabTypeAFWB.railPrefab);
            int prefabIndexB = af.ConvertMenuIndexToPrefabIndexForLayer(menuIndexB, PrefabTypeAFWB.railPrefab);
            int prefabIndexPost = af.ConvertMenuIndexToPrefabIndexForLayer(menuIndexPost, PrefabTypeAFWB.postPrefab);

            GameObject menuGoA = af.railPrefabs[prefabIndexA];
            GameObject menuGoB = af.railPrefabs[prefabIndexB];
            GameObject menuGoPost = af.railPrefabs[prefabIndexPost];

            List<SourceVariant> variantsA = af.railSourceVariants[0];
            List<SourceVariant> variantsB = af.railSourceVariants[1];
            List<SourceVariant> variantsPost = af.postSourceVariants;

            string wasName = "null";
            if (menuGoA != null)
            {
                if (variantsA[i].Go != null)
                    wasName = variantsA[i].Go.name;
                else if (menuGoA != variantsA[i].Go)
                    variantsA[i].Go = menuGoA;
                //Debug.Log("Was:  " + wasName +  "      Now:  " + go.name + "  ***********************\n");
            }
            if (menuGoB != null)
            {
                if (variantsB[i].Go != null)
                    wasName = variantsB[i].Go.name;
                else if (menuGoB != variantsB[i].Go)
                    variantsB[i].Go = menuGoB;
            }
            if (menuGoPost != null)
            {
                if (variantsB[i].Go != null)
                    wasName = variantsPost[i].Go.name;
                else if (menuGoPost != variantsB[i].Go)
                    variantsPost[i].Go = menuGoPost;
            }

            if (af.railSourceVariants[(int)LayerSet.railALayer][i].Go == null)
            {
                af.railSourceVariants[(int)LayerSet.railALayer][i].Go = af.railPrefabs[af.currentRail_PrefabIndex[(int)LayerSet.railBLayer]];
                Debug.Log($"CheckAllSourceVariantsArePoulated()  railSourceVariants[railALayer]   {i}   game object was missing");
            }
            if (af.railSourceVariants[(int)LayerSet.railBLayer][i].Go == null)
                af.railSourceVariants[(int)LayerSet.railALayer][i].Go = af.railPrefabs[af.currentRail_PrefabIndex[(int)LayerSet.railBLayer]];

            if (af.postSourceVariants[i].Go == null)
                af.postSourceVariants[i].Go = af.postPrefabs[af.currentPost_PrefabIndex];
        }
    }

    private void CheckPostsExist(int num = 0)
    {
        if (af.postsPool == null)
            Debug.LogWarning($"{num}:    af.postsPool is null\n");
        else if (af.postsPool.Count == 0)
            Debug.LogWarning($"{num}:    af.postsPool.Count = 0 \n");
        else if (af.postsPool[0] == null)
            Debug.LogWarning($"{num}:    af.postsPool[0] is null\n");
        else
            Debug.Log($"{num}:   af.postsPool Exist!   Count = {af.postsPool.Count},   af.postsPool[0] = {af.postsPool[0].gameObject.name} \n");
    }

    //-------------------------------------------
    public bool CheckPrefabsExistForLayer(LayerSet layer, [CallerMemberName] string caller = null)
    {
        GameObject currPrefabForLayer = af.GetMainPrefabForLayer(layer);
        if (currPrefabForLayer == null)
        {
            af.SetCurrentPrefabIndexForLayer(0, layer);
            af.SetAllSourceVariantsToMainForLayer(layer);
            Debug.LogWarning($"The current prefab for {af.GetLayerNameAsString(layer)} was null, " +
                $"so it has been set to a default prefab, along with all Variations for this Layer    caller = {caller}\n");
            currPrefabForLayer = af.GetMainPrefabForLayer(layer);
            return true;
        }
        return false;
    }

    /// <summary> In the Prefabs loaded List, check that Current one for each sourceLayerList is valid, if not, set them to the first one in the list</summary>
    /// <param name="caller"></param>
    /// <returns></returns>
    private bool CheckAllLayerPrefabsExist([CallerMemberName] string caller = null)
    {
        bool layerPrefabsChanged = CheckPrefabsExistForLayer(LayerSet.railALayer, caller);
        layerPrefabsChanged = CheckPrefabsExistForLayer(LayerSet.railBLayer, caller);
        layerPrefabsChanged = CheckPrefabsExistForLayer(LayerSet.postLayer, caller);
        layerPrefabsChanged = CheckPrefabsExistForLayer(LayerSet.subpostLayer, caller);
        layerPrefabsChanged = CheckPrefabsExistForLayer(LayerSet.extraLayer, caller);
        return layerPrefabsChanged;
    }

    //---------------------------------------
    // Check if the prefabs need reloading, if not backup their original meshes anyway. can't load ed resources from  main script so check the status here.
    protected bool InitialCheckPrefabs(bool reload = true)
    {
        bool somethingChanged = false;
        CheckPrefabsAndOptionallyReload(reload: true, warn: false);

        AutoFenceConfigurationManager configFile = AutoFenceConfigurationManager.ReadPermissionFile(af);
        if (configFile != null && af.usingMinimalVersion == false)
        {
            af.allowContentFreeUse = configFile.allowContentFreeTool;
        }
        if (af.allowContentFreeUse == true)
        {
            launchPresetIndex = 0;
            af.currPresetIndex = 0;
            af.currentPost_PrefabIndex = 0;
            af.currentRail_PrefabIndex[0] = 0;
            af.currentRail_PrefabIndex[1] = 0;
            af.currentSubpost_PrefabIndex = 0;
            af.currentExtra_PrefabIndex = 0;
            af.allowContentFreeUse = false; //the new directories have been created, so we can access them directly now
            af.usingMinimalVersion = true;
        }
        return somethingChanged;
    }
    //--------------------------
    /// <summary>
    /// Checks the integrity of each sourceLayerList's prefabs, and optionally reloads them if they are missing or empty
    /// </summary>
    /// <returns>If Reloading was necessary</returns>
    /// <remarks>Only one of each Rail or Post set is checked as they use the same prefabs
    /// Warning is not necessary during start up and reenabling, so set to false</remarks>
    protected bool CheckPrefabsAndOptionallyReload(bool reload = true, bool warn = true)
    {
        bool neededReload = false;
        neededReload = CheckPrefabsForLayer(LayerSet.railALayer, warn);
        neededReload = CheckPrefabsForLayer(LayerSet.postLayer, warn);
        neededReload = CheckPrefabsForLayer(LayerSet.extraLayer, warn);

        if (neededReload == true && reload == true)
            this.LoadPrefabs();
        return neededReload;
    }
    /// <summary>
    /// Checks if the prefabs here are null or empoty.
    /// Do not reload here as you risk doing multiple loads for each failed sourceLayerList
    /// </summary>
    protected bool CheckPrefabsForLayer(LayerSet layer, bool warn = false)
    {
        bool needLoadPrefabs = false;
        string warnStr = "";
        List<GameObject> prefabsForLayer = af.GetPrefabsForLayer(layer, warn);
        if (prefabsForLayer == null)
        {
            af.SetPrefabsForLayer(layer, new List<GameObject>());
            warnStr = $"prefabsForLayer {layer.String()} was null. Created new List<GameObject>()";
            needLoadPrefabs = true;
        }
        else if (prefabsForLayer.Count == 0)
        {
            warnStr = $"prefabsForLayer {layer.String()} Count was 0";
            needLoadPrefabs = true;
        }
        else if (prefabsForLayer[0] == null)
        {
            warnStr = $"prefabsForLayer[0] Count was 0.  {layer.String()} ";
            needLoadPrefabs = true;
        }

        if (warn == true && warnStr != "")
            Debug.LogWarning($"CheckPrefabsForLayer() {layer.String()}  {warnStr}\n");


        return needLoadPrefabs;
    }
    //---------------------------------------
    // Check if the prefabs need reloading, if not backup their original meshes anyway. can't load ed resources from  main script so check the status here.
    protected bool CheckPresetsAndOptionReload(bool reload = true, bool warn = true)
    {
        string resultStr = "";

        AutoFenceConfigurationManager configFile = AutoFenceConfigurationManager.ReadPermissionFile(af);
        if (configFile != null && af.usingMinimalVersion == false)
        {
            af.allowContentFreeUse = configFile.allowContentFreeTool;
        }
        bool needsReload = false;

        //      Normal Use
        //=======================
        if (af.allowContentFreeUse == false)
        {
            if (mainPresetList == null)
            {
                resultStr = "mainPresetList was null. Created new List<ScriptablePreset>()";
                mainPresetList = new List<ScriptablePresetAFWB>();
                needsReload = true;
            }
            else if (mainPresetList.Count < 1)
            {
                resultStr = "mainPresetList.Count == 0";
                needsReload = true;
            }
            if (af.currPresetIndex >= mainPresetList.Count)
                needsReload = true;

            if (mainPresetList != null && mainPresetList.Count > 0)
                af.presetSaveName = mainPresetList[af.currPresetIndex].name;

            if (reload && needsReload)
            {
                presetsEd.LoadAllScriptablePresets(false);
                if (af.currPresetIndex >= mainPresetList.Count)
                {
                    resultStr += $" currPresetIndex was {af.currPresetIndex} /  {mainPresetList.Count}. Setting to {0} ";
                    af.currPresetIndex = 0;
                }
            }

            if (resultStr != "" && warn == true)
                Debug.LogWarning($"CheckPresetsAndOptionReload():   {resultStr}\n");
        }
        else
        {
            if (mainPresetList == null || mainPresetList.Count < 1)
                presetsEd.LoadAllScriptablePresets(true);
            launchPresetIndex = 0;
            af.currPresetIndex = 0;
            af.currentPost_PrefabIndex = 0;
            af.currentRail_PrefabIndex[0] = 0;
            af.currentRail_PrefabIndex[1] = 0;
            af.currentSubpost_PrefabIndex = 0;
            af.currentExtra_PrefabIndex = 0;
            af.allowContentFreeUse = false; //the new directories have been created, so we can access them directly now
            af.usingMinimalVersion = true;
        }
        return needsReload;
    }

    //---------------------------------------
    //-- Ensure the SourceVariants Lists exist and are not null. Shouldn't be null as they're initialised as AF's class Variables
    private bool CheckAllSourceVariantsListsExist()
    {
        bool allListsExisted = true;
        //-- Create The Array for 2 Rails, then Create the two Rail Lists
        if (af.railSourceVariants == null)
        {
            af.railSourceVariants = new List<SourceVariant>[2];
            Debug.Log("CheckAllSourceVariantsListsExist()  railSourceVariants was null");
            allListsExisted = false;
        }
        if (af.railSourceVariants[AutoFenceCreator.kRailALayerInt] == null)
        {
            af.railSourceVariants[AutoFenceCreator.kRailALayerInt] = new List<SourceVariant>();
            Debug.Log("CheckAllSourceVariantsListsExist()  railSourceVariants[kRailALayerInt] was null");
            allListsExisted = false;
        }
        if (af.railSourceVariants[AutoFenceCreator.kRailBLayerInt] == null)
        {
            af.railSourceVariants[AutoFenceCreator.kRailBLayerInt] = new List<SourceVariant>();
            Debug.Log("CheckAllSourceVariantsListsExist()  railSourceVariants[kRailBLayerInt] was null");
            allListsExisted = false;
        }
        if (af.postSourceVariants == null)
        {
            af.postSourceVariants = new List<SourceVariant>();
            Debug.Log("CheckAllSourceVariantsListsExist()  postVariants was null");
            allListsExisted = false;
        }
        return allListsExisted;
    }

    // This is called periodically from CheckPeriodicallyFromOnInspectorGUI(), also from SetupEditor() when AutoFenceBuilder is reselected in hierarchy
    // Checks if new prefabs have been added to the AFWB_Prefabs folder in the Assets folder
    public void CheckForNewPrefabs()
    {
        //Timer t = new Timer("CheckForNewPrefabs()");
        InitializeCacheIfNeeded();

        string basePath = af.currPrefabsDir; // Base path to check
        string protectedSubdir = "Protected_Do_Not_Remove"; // Subdirectory to exclude

        //t.Lap("");
        // Get all prefab paths in the base directory, excluding the protected subdirectory
        var allPrefabPaths = AssetDatabase.GetAllAssetPaths()
            .Where(path => path.StartsWith(basePath) && path.EndsWith(".prefab") && !path.Contains("/" + protectedSubdir + "/"))
            .ToList();
        //t.Lap("GetAllAssetPaths");

        // Find and log new prefabs
        //t.Lap("");
        List<string> newPrefabPaths = allPrefabPaths.Where(path => !cachedExistingPrefabPaths.Contains(path)).ToList();
        // Remove all entries containing "Marker_Post"
        newPrefabPaths.RemoveAll(path => path.Contains("Marker_Post"));
        //t.Lap("check cachedExistingPrefabPaths");

        if (newPrefabPaths.Count > 0)
        {
            int oldCcount = cachedExistingPrefabPaths.Count; //debug comment out
            // Update the cache with new prefabs
            for (int i = 0; i < newPrefabPaths.Count; i++)
            {
                string newPath = newPrefabPaths[i];
                cachedExistingPrefabPaths.Add(newPath);
                newPrefabPaths[i] = newPrefabPaths[i].Replace(basePath + "/", "");
            }
            int oldCount = cachedExistingPrefabPaths.Count; //debug comment out
            //Debug.Log($"{newPrefabPaths.Count} New prefabs found:\n" + string.Join(",   ", newPrefabPaths) + $"  {oldCcount} -> {cachedExistingPrefabPaths.Count}");
            ReloadPrefabs();
            ReloadPresets();
            //Debug.Log($"Num Reloaded Prefabs = {af.GetAllPrefabs().Count} \n");
            //Debug.Log("Bob\n");
        }
        else
        {
            //Debug.Log("      --- No new prefabs found.\n"); //??? debug only comment out
        }
        //t.End();
    }

    private void InitializeCacheIfNeeded()
    {
        if (!isCacheInitialized)
        {
            AddPrefabsToHashSet(cachedExistingPrefabPaths, af.postPrefabs);
            AddPrefabsToHashSet(cachedExistingPrefabPaths, af.railPrefabs);
            AddPrefabsToHashSet(cachedExistingPrefabPaths, af.extraPrefabs);
            isCacheInitialized = true;
        }
    }

    private void AddPrefabsToHashSet(HashSet<string> hashSet, List<GameObject> prefabs)
    {
        foreach (var prefab in prefabs)
        {
            var path = AssetDatabase.GetAssetPath(prefab);
            if (!string.IsNullOrEmpty(path))
            {
                hashSet.Add(path);
            }
        }
    }


    //--------------------------------------------
    // Gets SeriealizedProperties for the most used properties and fields
    private void SetupSerializedProperties()
    {
        //Cache the props that are accessed most often
        showGlobals = serializedObject.FindProperty("showGlobals");
        showRailAVariations = serializedObject.FindProperty("showRailAVariations");
        showRailBVariations = serializedObject.FindProperty("showRailBVariations");
        showPostVariations = serializedObject.FindProperty("showPostVariations");
        exProp = serializedObject.FindProperty("ex");
        extraPositionOffsetProp = exProp.FindPropertyRelative("extraTransformPositionOffset");
        extraSizeProp = serializedObject.FindProperty("ex.extraTransformScale");
        extraRotationProp = serializedObject.FindProperty("ex.extraTransformRotation");
        userPrefabExtraProp = serializedObject.FindProperty("userPrefabExtra");
        extraFreq = exProp.FindPropertyRelative("extraFreq");

        gs = serializedObject.FindProperty("gs");
        gs.floatValue = 1.0f;//Global Scale
        scaleInterpolationAlso = serializedObject.FindProperty("scaleInterpolationAlso");
        switchControlsAlso = serializedObject.FindProperty("switchControlsAlso");

        railAPositionOffsetProp = serializedObject.FindProperty("railAPositionOffset");
        railBPositionOffsetProp = serializedObject.FindProperty("railBPositionOffset");
        railASizeProp = serializedObject.FindProperty("railAScale");
        railBSizeProp = serializedObject.FindProperty("railBScale");
        railARotationProp = serializedObject.FindProperty("railARotation");
        railBRotationProp = serializedObject.FindProperty("railBRotation");
        SerializedProperty userObjectRailArray = serializedObject.FindProperty("userPrefabRail");
        if (userObjectRailArray.arraySize >= 2)
        {
            userPrefabRailProp[0] = userObjectRailArray.GetArrayElementAtIndex(0);
            userPrefabRailProp[1] = userObjectRailArray.GetArrayElementAtIndex(1);
        }
        useMainRails = serializedObject.FindProperty("useRailLayer[0]");
        useSecondaryRails = serializedObject.FindProperty("useRailLayer[1]");

        fenceHeight = serializedObject.FindProperty("globalHeight");
        fenceWidth = serializedObject.FindProperty("globalWidth");
        postHeightOffset = serializedObject.FindProperty("postHeightOffset");
        postSizeProp = serializedObject.FindProperty("postScale");
        mainPostsSizeBoostProp = serializedObject.FindProperty("mainPostsSizeBoost");
        endPostsSizeBoostProp = serializedObject.FindProperty("endPostsSizeBoost");
        postRotationProp = serializedObject.FindProperty("postRotation");
        userPrefabPostProp = serializedObject.FindProperty("userPrefabPost");


        postImportScaleModeProp = serializedObject.FindProperty("postImportScaleMode");
        railAImportScaleModeProp = serializedObject.FindProperty("railAImportScaleMode");
        railBImportScaleModeProp = serializedObject.FindProperty("railBImportScaleMode");
        extraImportScaleModeProp = serializedObject.FindProperty("extraImportScaleMode");


        roundingDistance = serializedObject.FindProperty("roundingDistance");

        subSpacing = serializedObject.FindProperty("subSpacing");
        useSubPosts = serializedObject.FindProperty("useSubpostsLayer");
        subpostPositionOffsetProp = serializedObject.FindProperty("subpostPositionOffset");
        subpostScaleProp = serializedObject.FindProperty("subpostScale");
        subpostRotationProp = serializedObject.FindProperty("subpostRotation");
        showControlsProp = serializedObject.FindProperty("showControls");
        closeLoopProp = serializedObject.FindProperty("closeLoop");

        keepInterpolatedPostsGrounded = serializedObject.FindProperty("keepInterpolatedPostsGrounded");
        snapMainPostsProp = serializedObject.FindProperty("snapMainPosts");
        snapSizeProp = serializedObject.FindProperty("snapSizeProp");
        lerpPostRotationAtCorners = serializedObject.FindProperty("lerpPostRotationAtCorners");
        lerpPostRotationAtCornersInters = serializedObject.FindProperty("lerpPostRotationAtCornersInters");
        hideInterpolated = serializedObject.FindProperty("hideInterpolated");

        globalLiftLower = serializedObject.FindProperty("globalLift");

        //==== Variation Parameters ========
        allowVertical180Invert_Post = serializedObject.FindProperty("allowVertical180Invert_Post");
        allowMirroring_X_Post = serializedObject.FindProperty("allowMirroring_X_Post");
        allowMirroring_Z_Post = serializedObject.FindProperty("allowMirroring_Z_Post");
        jitterPostVerts = serializedObject.FindProperty("jitterPostVerts");
        mirrorXPostProbability = serializedObject.FindProperty("mirrorXPostProbability");
        mirrorZPostProbability = serializedObject.FindProperty("mirrorZPostProbability");
        verticalInvertPostProbability = serializedObject.FindProperty("verticalInvertPostProbability");
        postSpacingVariation = serializedObject.FindProperty("postSpacingVariation");

        allowMirroring_X_Rail = serializedObject.FindProperty("allowMirroring_X_Rail");
        allowMirroring_Z_Rail = serializedObject.FindProperty("allowMirroring_Z_Rail");
        jitterRailVerts = serializedObject.FindProperty("jitterRailVerts");
        mirrorXRailProbability = serializedObject.FindProperty("mirrorXRailProbability");
        mirrorZRailProbability = serializedObject.FindProperty("mirrorZRailProbability");
        verticalInvertRailProbability = serializedObject.FindProperty("verticalInvertRailProbability");

        minRailHeightLimit = serializedObject.FindProperty("minRailHeightSlider");
        maxRailHeightLimit = serializedObject.FindProperty("maxRailHeightSlider");
        minRailAHeightVar = serializedObject.FindProperty("minRandHeightRailA");
        maxRailAHeightVar = serializedObject.FindProperty("maxRandHeightRailA");
        minRailBHeightVar = serializedObject.FindProperty("minRandHeightRailB");
        maxRailBHeightVar = serializedObject.FindProperty("maxRandHeightRailB");

        railVariation1 = serializedObject.FindProperty("railVariation1");
        railVariation2 = serializedObject.FindProperty("railVariation2");
        railVariation3 = serializedObject.FindProperty("railVariation3");
        railADisplayVariationGOs = serializedObject.FindProperty("railADisplayVariationGOs");
        useRailASeq = serializedObject.FindProperty("useRailASeq");

        railAProbArray = serializedObject.FindProperty("varRailAProbs");
        railBProbArray = serializedObject.FindProperty("varRailBProbs");
        varRailAPositionOffset = serializedObject.FindProperty("varRailAPositionOffset");
        varRailASize = serializedObject.FindProperty("varRailASize");
        varRailARotation = serializedObject.FindProperty("varRailARotation");
        varRailBPositionOffset = serializedObject.FindProperty("varRailBPositionOffset");
        varRailBSize = serializedObject.FindProperty("varRailBSize");
        varRailBRotation = serializedObject.FindProperty("varRailBRotation");

        varRailABackToFront = serializedObject.FindProperty("varRailABackToFront");
        varRailAMirrorZ = serializedObject.FindProperty("varRailAMirrorZ");
        varRailAInvert = serializedObject.FindProperty("varRailAInvert");
        varRailBBackToFront = serializedObject.FindProperty("varRailBBackToFront");
        varRailBMirrorZ = serializedObject.FindProperty("varRailBMirrorZ");
        varRailBInvert = serializedObject.FindProperty("varRailBInvert");

        varRailABackToFrontBools = serializedObject.FindProperty("varRailABackToFrontBools");
        varRailAMirrorZBools = serializedObject.FindProperty("varRailAMirrorZBools");
        varRailAInvertBools = serializedObject.FindProperty("varRailAInvertBools");
        varRailBBackToFrontBools = serializedObject.FindProperty("varRailBBackToFrontBools");
        varRailBMirrorZBools = serializedObject.FindProperty("varRailBMirrorZBools");
        varRailBInvertBools = serializedObject.FindProperty("varRailBInvertBools");

        railSingleVariantsProp = serializedObject.FindProperty("railSingleVariants");
        railVariantsProp = serializedObject.FindProperty("railSourceVariants");

        quantizeRotIndexPostProp = serializedObject.FindProperty("quantizeRotIndexPost");
        quantizeRotIndexSubpostProp = serializedObject.FindProperty("quantizeRotIndexSubpost");
        quantizeRotIndexRailAProp = serializedObject.FindProperty("quantizeRotIndexRailA");
        quantizeRotIndexRailBProp = serializedObject.FindProperty("quantizeRotIndexRailB");

        randRotAxisPost = serializedObject.FindProperty("quantizeRotAxisPost");
        randRotAxisSubpost = serializedObject.FindProperty("quantizeRotAxisSubpost");
        quantizeRotAxisRailA = serializedObject.FindProperty("quantizeRotAxisRailA");
        quantizeRotAxisRailB = serializedObject.FindProperty("quantizeRotAxisRailB");

        quantizeRotProbPost = serializedObject.FindProperty("quantizeRotProbPost");
        quantizeRotProbRailA = serializedObject.FindProperty("quantizeRotProbRailA");
        quantizeRotProbRailB = serializedObject.FindProperty("quantizeRotProbRailB");
        quantizeRotProbSubpost = serializedObject.FindProperty("quantizeRotProbSubpost");

        autoScaleImports = serializedObject.FindProperty("autoScaleImports");
        autoRotateImports = serializedObject.FindProperty("autoRotateImports");
        componentToolbarProp = serializedObject.FindProperty("componentToolbar");
        allowPostRandomization = serializedObject.FindProperty("allowPostRandomization");
        allowSubpostRandomization = serializedObject.FindProperty("allowSubpostRandomization");
        interPostDistProp = serializedObject.FindProperty("interPostDist");

        subpostDuplicateModeProp = serializedObject.FindProperty("subpostDuplicateMode");


        railACustomColliderMeshProp = serializedObject.FindProperty("railACustomColliderMesh");
        railBCustomColliderMeshProp = serializedObject.FindProperty("railBCustomColliderMesh");
        postCustomColliderMeshProp = serializedObject.FindProperty("postCustomColliderMesh");
        extraCustomColliderMeshProp = serializedObject.FindProperty("extraCustomColliderMesh");
        subpostCustomColliderMeshProp = serializedObject.FindProperty("subpostCustomColliderMesh");

        railAColliderModeProp = serializedObject.FindProperty("railAColliderMode");
        railBColliderModeProp = serializedObject.FindProperty("railBColliderMode");
        postColliderModeProp = serializedObject.FindProperty("postColliderMode");
        extraColliderModeProp = serializedObject.FindProperty("extraColliderMode");
        subpostColliderModeProp = serializedObject.FindProperty("subpostColliderMode");

        railASequencerProp = serializedObject.FindProperty("railASequencer");
        railBSequencerProp = serializedObject.FindProperty("railBSequencer");
        postSequencerProp = serializedObject.FindProperty("postSequencer");
        railASeqListProp = railASequencerProp.FindPropertyRelative("seqList");
        railBSeqListProp = railBSequencerProp.FindPropertyRelative("seqList");
        postSeqListProp = postSequencerProp.FindPropertyRelative("seqList");


        showOptionalPostPrefabsProp = serializedObject.FindProperty("showOptionalPostPrefabsProp");

    }

    //--------------------------
    //- Gets the world position with an x, y offset
    public Vector3 GetWorldPos(float x, float y, ref Vector3 currWorldPos, Camera cam)
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(currWorldPos);
        screenPoint.x += x;
        screenPoint.y += y;
        currWorldPos = cam.ScreenToWorldPoint(screenPoint);
        return currWorldPos;
    }

    //--------------------
    private void ShowBuildInfo()
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 11;

        int lineHeight = 16;

        Camera cam = SceneView.lastActiveSceneView.camera;

        //- working from bottom upwards
        int totalTrianglesCount = af.railsATotalTriCount + af.railsBTotalTriCount + af.postsTotalTriCount
        + af.extrasTotalTriCount + af.subPostsTotalTriCount;
        int numRailA = af.railABuiltCount, numRailB = af.railABuiltCount;
        int numPosts = af.postsBuiltCount, numExtras = af.ex.extrasBuiltCount, numSubs = af.subpostsBuiltCount;
        int railATriCount = 0, railBTriCount = 0, postTriCount = 0, extraTriCount = 0, subTriCount = 0, avgTrisPerSection = 0;

        if (af.usePostsLayer == false)
            numPosts = 0;

        if (af.railABuiltCount > 0 && af.railsATotalTriCount > 0 && numRailA > 0)
            railATriCount = af.railsATotalTriCount / numRailA;
        if (af.railBBuiltCount > 0 && af.railsBTotalTriCount > 0 && numRailB > 0)
            railBTriCount = af.railsBTotalTriCount / numRailB;
        if (af.postsBuiltCount > 0 && af.usePostsLayer == true && af.postsTotalTriCount > 0)
            postTriCount = af.postsTotalTriCount / numPosts;
        if (af.ex.extrasBuiltCount > 0 && af.extrasTotalTriCount > 0)
            extraTriCount = af.extrasTotalTriCount / numExtras;
        if (af.subpostsBuiltCount > 0 && af.subPostsTotalTriCount > 0)
            subTriCount = af.subPostsTotalTriCount / numSubs;

        int numSects = (af.allPostPositions.Count - 1);
        if (numSects > 0)
        {
            avgTrisPerSection = totalTrianglesCount / numSects;
        }
        else
        {
            numSects = 0;
        }

        //Vector3 baseScreenPos = new Vector3(10, 85, 20);
        //Vector3 screenPos = baseScreenPos;
        //Vector3 wPos = cam.ScreenToWorldPoint(screenPos);//world position
    }

    //---------------
    public void CreateScriptablePresetStringsForMenus(List<ScriptablePresetAFWB> presetList)
    {
        presetMenuNames.Clear();
        for (int i = 0; i < presetList.Count; i++)
        {
            string menuName = presetList[i].categoryName + "/" + presetList[i].name;
            presetMenuNames.Add(menuName);
        }
    }
    public void AddSinglePresetStringForPresetMenu(ScriptablePresetAFWB preset)
    {
        string menuName = preset.categoryName + "/" + preset.name;
        presetMenuNames.Add(menuName);
    }

    //---------------
    public void GetCategoryNamesFromLoadedPresetList()
    {
        ScriptablePresetAFWB preset;
        string categoryName = "";
        for (int i = 0; i < mainPresetList.Count; i++)
        {
            preset = mainPresetList[i];
            categoryName = preset.categoryName;
            //Debug.Log(categoryName + "\n");
            if (af.categoryNames.Contains(categoryName) == false)
            {
                af.categoryNames.Add(categoryName);
            }
        }
    }

    //----------------------------
    // Loads an image file as a Texture2D. needs to know folder locations so call after checking folders
    private void LoadGUITextures()
    {
        shapesTex = EditorGUIUtility.Load(af.currTexturesDir + "/Utility/shapes.png") as Texture2D;
        saveTex = EditorGUIUtility.Load(af.currTexturesDir + "/Utility/saveiconGreen.png") as Texture2D;
    }
    //-------------------------------
    protected void SetupStyles()
    {
        isDark = EditorGUIUtility.isProSkin;

        unityBoldLabel = new GUIStyle(EditorStyles.label);
        //-- Comment out on release, but this is an easy useful alert if there were initialisation problems by the time we got here
        //-- can be caused by unexpected System/Unity callbacks or unexpected MonoBehaviour calls leading here
        if (unityBoldLabel == null)
        {
            Debug.LogWarning("SetupStyles():   Editor Class Variables not initialized\n");
            return;
        }
        SetupColors();
        SetupLabelStyles();
        SetupPopupStyles();
        SetupButtonAndToolbarStyles();
        SetupBoxAndPanelStyles();
    }

    private void SetupBoxAndPanelStyles()
    {
        boxUIStyle = new GUIStyle();
        boxUIStyleNoBorder = new GUIStyle();
        boxUIStyleNoBorderLight = new GUIStyle();
        boxUIStyleNoBorderDark = new GUIStyle();
        boxUIStyleDark = new GUIStyle();
        if (EditorGUIUtility.isProSkin)
        {
            //panelBg = new Color(.25f, .25f, .25f);
            panelBg = new Color(.28f, .28f, .29f);
            panelBorder = new Color(.28f, .28f, .29f);
            boxUIStyle.normal.background = bgBoxTex;
            bgBoxSmallTex = boxUIStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 50, panelBg, new RectOffset(1, 1, 1, 1), panelBorder);
            bgBoxTex = boxUIStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 150, panelBg, new RectOffset(1, 1, 1, 1), panelBorder);
            bgBoxLargeTex = boxUIStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 500, panelBg, new RectOffset(1, 1, 1, 1), panelBorder);
        }
        else
        {
            panelBg = new Color(.25f, .25f, .25f);
            panelBorder = new Color(.70f, .7f, .7f);
            boxUIStyle.normal.background = bgBoxTex;
            bgBoxTex = boxUIStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 120, panelBg, new RectOffset(1, 1, 1, 1), panelBorder);
            bgBoxLargeTex = boxUIStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 500, panelBg, new RectOffset(1, 1, 1, 1), panelBorder);
        }
        //__ Used By the Main Presets Panel
        boxUIGreenStyle = new GUIStyle();
        panelGreenBorder = new Color(.28f, .36f, .29f);
        bgGreenBoxTex = boxUIGreenStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 120,
            panelBg, new RectOffset(1, 1, 1, 1), panelGreenBorder);
        boxUIGreenStyle.normal.background = bgGreenBoxTex;

        //__ Used By the Main Components Panel
        boxUIDarkCyanStyle = new GUIStyle();
        panelDarkCyanBorder = new Color(.24f, .27f, .32f);
        bgDarkCyanBoxTex = boxUIDarkCyanStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 120,
            panelBg, new RectOffset(1, 1, 1, 1), panelDarkCyanBorder);
        boxUIDarkCyanStyle.normal.background = bgDarkCyanBoxTex;

        //__ Used By the  Globals Panel
        boxUIDarkYellowStyle = new GUIStyle();
        panelDarkYellowBorder = new Color(.41f, .38f, .27f);
        bgDarkYellowBoxTex = boxUIDarkCyanStyle.normal.background = EditorToolsTCT.MakeEditorTexWithBorder(600, 120,
            panelBg, new RectOffset(1, 1, 1, 1), panelDarkYellowBorder);
        boxUIDarkYellowStyle.normal.background = bgDarkYellowBoxTex;
    }

    //--------------------------
    private void SetupButtonAndToolbarStyles()
    {
        sty2 = new GUIStyle(EditorStyles.toolbarButton);
        sty2.normal.textColor = new Color(0.58f, .73f, .94f);

        smallButtonStyle7 = new GUIStyle(EditorStyles.miniButton);
        smallButtonStyle7.fontSize = 7;

        smallButtonStyle9 = new GUIStyle(EditorStyles.miniButton);
        smallButtonStyle9.fontSize = 10;

        smallToolbarStyle = new GUIStyle(EditorStyles.miniButton);
        smallToolbarStyle.fontSize = 12;

        miniBold = new GUIStyle(EditorStyles.miniButton);
        miniBold.fontStyle = FontStyle.Bold;

        smallToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        smallToolbarButtonStyle.fontSize = 11;

        sceneViewScaleButtonOn = new GUIStyle(EditorStyles.miniButton);
        sceneViewScaleButtonOn.fontSize = 13;
        sceneViewScaleButtonOn.normal.textColor = new Color(0.9f, 1f, 0.9f, 1);
        sceneViewScaleButtonOn.hover.textColor = new Color(0.9f, 1f, 0.9f, 1);

        sceneViewScaleButtonOff = new GUIStyle(EditorStyles.miniButton);
        sceneViewScaleButtonOff.fontSize = 11;


        //--Currently unused
        sty3 = new GUIStyle("button");
        sty3.normal.textColor = cyanBoldStyle.normal.textColor;
        if (isDark)
            sty3.normal.background = TextureUtilitiesTCT.CreateTex2D(2, 2, new Color(0.34f, 0.34f, 0.36f));
        sty3.fontStyle = FontStyle.Bold;
        sty3.fontSize = 14;
    }

    private void SetupPopupStyles()
    {
        popup13Style = new GUIStyle(EditorStyles.popup);
        popup13Style.fontSize = 13;
        popup12Style = new GUIStyle(EditorStyles.popup);
        popup12Style.fontSize = 12;
        popup11Style = new GUIStyle(EditorStyles.popup);
        popup11Style.fontSize = 11;

        biggerPopupStyle = new GUIStyle(EditorStyles.popup);
        biggerPopupStyle.fontSize = 12;
        if (isDark)
            biggerPopupStyle.normal.textColor = new Color(0.59f, 0.74f, 0.98f);

        greenPopupStyle = new GUIStyle(EditorStyles.popup);
        greenPopupStyle.fontSize = 12;
        if (isDark)
            greenPopupStyle.normal.textColor = new Color(0.74f, .78f, .67f);

        orangePopupStyle = new GUIStyle(EditorStyles.popup);
        orangePopupStyle.normal.textColor = new Color(0.9f, 0.6f, 0.0f);
        orangePopupStyle.active.textColor = orangePopupStyle.normal.textColor;
        orangePopupStyle.focused.textColor = orangePopupStyle.normal.textColor;
        orangePopupStyle.hover.textColor = orangePopupStyle.normal.textColor;


        lilacPopupStyle = new GUIStyle(EditorStyles.popup);
        lilacPopupStyle.normal.textColor = new Color(0.83f, 0.83f, 0.95f);
    }

    private void SetupLabelStyles()
    {
        unityBoldLabel.fontStyle = FontStyle.Bold;
        unityBoldLabelLarge = new GUIStyle(EditorStyles.label);
        unityBoldLabelLarge.fontStyle = FontStyle.Bold;
        unityBoldLabelLarge.fontSize = 14;

        moduleHeaderLabelStyle = new GUIStyle(EditorStyles.label);
        moduleHeaderLabelStyle.fontStyle = FontStyle.Bold;
        if (isDark == true)
            moduleHeaderLabelStyle.normal.textColor = new Color(0.58f, .73f, .5f);

        smallModuleLabelStyle = new GUIStyle(EditorStyles.label);
        //moduleHeaderLabelStyle.fontStyle = FontStyle.Bold;
        if (isDark == true)
            smallModuleLabelStyle.normal.textColor = new Color(0.58f, .73f, .5f);
        smallModuleLabelStyle.fontSize = 11;

        cyanBoldStyle = new GUIStyle(EditorStyles.label);// { alignment = TextAnchor.LowerCenter };
        cyanBoldStyle.fontStyle = FontStyle.Bold;
        cyanBoldStyle.fontSize = 13;
        if (isDark)
            cyanBoldStyle.normal.textColor = new Color(0.20f, .50f, .85f);
        cyanBoldStyle.wordWrap = false; //although this is the default, put it here as it's getting changed somewhere

        cyanBoldStyle.normal.textColor *= 1.01f;

        cyanBoldStyle.normal.textColor += new Color(0.05f, 0.0f, 0.0f);

        //For use against a lighter background
        greenStyle2 = new GUIStyle(EditorStyles.label);
        greenStyle2.fontSize = 13;
        if (isDark)
            greenStyle2.normal.textColor = new Color(0.6f, .71f, .52f);
        greenStyle2.wordWrap = false;

        cyanNonBoldStyle = new GUIStyle(EditorStyles.label);
        cyanNonBoldStyle.fontSize = 13;
        if (isDark)
            cyanNonBoldStyle.normal.textColor = new Color(0.80f, .80f, .9f);
        cyanNonBoldStyle.wordWrap = false;

        cyanBoldStyleBigger = new GUIStyle(EditorStyles.label);
        cyanBoldStyleBigger.fontStyle = FontStyle.Bold;
        cyanBoldStyleBigger.normal.textColor = darkCyan;
        cyanBoldStyleBigger.fontSize = 13;

        darkRed = new Color(.8f, .35f, .35f);
        warningStyle = new GUIStyle(EditorStyles.label);
        warningStyle.fontStyle = FontStyle.Bold;
        if (isDark)
            warningStyle.normal.textColor = new Color(0.9f, .7f, .7f);

        warningStyle2 = new GUIStyle(EditorStyles.label);
        if (isDark)
            warningStyle2.normal.textColor = new Color(0.9f, .72f, .5f);
        warningStyle2.fontStyle = FontStyle.Normal;

        warningStyleLarge = new GUIStyle(EditorStyles.label);
        warningStyleLarge.fontStyle = FontStyle.Bold;
        warningStyleLarge.normal.textColor = darkRed;
        warningStyleLarge.fontSize = 16;

        mildWarningStyle = new GUIStyle(EditorStyles.label);
        mildWarningStyle.normal.textColor = new Color(0.7f, 0.2f, 0.2f);

        infoStyle = new GUIStyle(EditorStyles.label);
        infoStyle.fontStyle = FontStyle.Italic;
        infoStyle.normal.textColor = darkCyan;

        infoStyleSmall = new GUIStyle(EditorStyles.label);
        infoStyleSmall.fontStyle = FontStyle.Italic;
        infoStyleSmall.fontSize = af.infoStyleSmallSize;
        af.infoStyleSmallColor = new Color(0, .3f, .6f);
        infoStyleSmall.normal.textColor = af.infoStyleSmallColor;

        italicHintStyle = new GUIStyle(EditorStyles.label);
        italicHintStyle.fontStyle = FontStyle.Italic;
        italicHintStyle.fontSize = 11;
        if (isDark)
            italicHintStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

        smallBoldBlack = new GUIStyle(EditorStyles.label);
        smallBoldBlack.fontStyle = FontStyle.Bold;
        smallBoldBlack.fontSize = 12;
        smallBoldBlack.normal.textColor = Color.black;

        italicStyle = new GUIStyle(EditorStyles.label);
        italicStyle.fontStyle = FontStyle.Italic;
        italicStyle.normal.textColor = new Color(0.6f, 0.4f, 0.2f);

        italicStyle2 = new GUIStyle(EditorStyles.label);
        italicStyle2.fontStyle = FontStyle.Italic;
        italicStyle2.normal.textColor = new Color(0.7f, 0.74f, 0.7f);
        italicStyle2.fontSize = 11;

        lightGrayStyle = new GUIStyle(EditorStyles.label);
        lightGrayStyle.fontStyle = FontStyle.Italic;
        lightGrayStyle.normal.textColor = new Color(0.5f, 0.5f, 0.6f);
        lightGrayStyle.fontSize = 10;

        darkGrayStyle10 = new GUIStyle(EditorStyles.label);
        darkGrayStyle10.fontStyle = FontStyle.Italic;
        darkGrayStyle10.normal.textColor = new Color(0.35f, 0.35f, 0.4f);
        darkGrayStyle10.fontSize = 10;

        darkGrayStyle11 = new GUIStyle(EditorStyles.label);
        darkGrayStyle11.fontStyle = FontStyle.Italic;
        darkGrayStyle11.normal.textColor = new Color(0.35f, 0.35f, 0.4f);
        darkGrayStyle11.fontSize = 11;

        grayHelpStyle = new GUIStyle(EditorStyles.label);
        grayHelpStyle.fontStyle = FontStyle.Italic;
        grayHelpStyle.normal.textColor = new Color(0.15f, 0.2f, 0.3f);
        grayHelpStyle.fontSize = 11;

        smallOrangeItalicLabelStyle = new GUIStyle(EditorStyles.label);
        smallOrangeItalicLabelStyle.fontStyle = FontStyle.Italic;
        smallOrangeItalicLabelStyle.normal.textColor = new Color(1f, 0.8f, 0.4f);
        smallOrangeItalicLabelStyle.fontSize = 10;

        small10Style = new GUIStyle(EditorStyles.label);
        small10Style.fontSize = 10;

        smallLabel = new GUIStyle(EditorStyles.miniLabel);

        label14Style = new GUIStyle(EditorStyles.label);
        label14Style.fontSize = 14;

        label13Style = new GUIStyle(EditorStyles.label);
        label13Style.fontSize = 13;

        label12Style = new GUIStyle(EditorStyles.label);
        label12Style.fontSize = 12;

        label11Style = new GUIStyle(EditorStyles.label);
        label11Style.fontSize = 11;

        label10Style = new GUIStyle(EditorStyles.label);
        label10Style.fontSize = 10;

        label9Style = new GUIStyle(EditorStyles.label);
        label9Style.fontSize = 9;

        greyStyle = new GUIStyle(EditorStyles.label);
        greyStyle.fontStyle = FontStyle.Italic;
        greyStyle.normal.textColor = Color.gray;
        greyStyle.fontSize = 11;

        lilacUnityStyle = new GUIStyle(EditorStyles.label);
        lilacUnityStyle.normal.textColor = new Color(0.88f, 0.78f, 0.96f);

        lightGreyStyle2 = new GUIStyle(EditorStyles.label);
        lightGreyStyle2.fontStyle = FontStyle.Italic;
        lightGreyStyle2.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        lightGreyStyle2.fontSize = 11;

        borderedLabelStyle = GuiItemsUtilities.CreateLabelStyleWithBorder(300);
        // To make a Label look similar to a popup
        popupLabelStyle = GuiItemsUtilities.CreateLabelStyleWithBorder(300);

        extrasNormalModeLabelStyle = new GUIStyle(EditorStyles.label);

        extrasNormalModeLabelStyle.fontSize = 12;

        extrasScatterModeLabelStyle = new GUIStyle(EditorStyles.label);
        extrasScatterModeLabelStyle.normal.textColor = new Color(.999f, .9f, .999f);
        extrasScatterModeLabelStyle.fontSize = 12;
    }
    //-----------
    public void SetupColors()
    {
        switchRed = new Color(1.0f, 0.8f, 0.8f);
        switchGreen = new Color(0.63f, 0.99f, 0.67f);
        switchGreenSceneView = new Color(.01f, .999f, .01f, .58f);
        seedGreen = new Color(.8f, 1f, .8f, 0.6f);
        seedGreenTextCol = new Color(.9f, 1f, .9f);

        darkCyan = new Color(0.2f, .45f, .7f);
        darkMagenta = new Color(.35f, 0.05f, .95f);
        darkRed = new Color(0, 0, 0);
        darkerRed = new Color(0.75f, .0f, .0f);
        darkGrey = new Color(.3f, .3f, .3f, .8f);
        lightRedBackgroundColor = new Color(.99f, 0.94f, 0.94f, 1.0f);
        lightBlueBackgroundColor = new Color(0.94f, 0.95f, .96f, 1.0f);
        lightYellowBackgroundColor = new Color(.99f, 0.98f, 0.97f, 1.0f);
        transRed = new Color(0.999f, .0f, .0f, 0.5f);

        panelBg = default; // assign as needed
        panelBorder = default; // assign as needed
        panelGreenBg = default; // assign as needed
        panelGreenBorder = default; // assign as needed
        panelDarkCyanBorder = default; // assign as needed
        defaultBackgroundColor = Color.white;

        midGrayColor = new Color(.8f, 0.8f, 0.8f, 1.0f);
        lightGrayColor = new Color(.92f, 0.92f, 0.92f, 1.0f);
        grey20percent_80alpha = new Color(.2f, .2f, .2f, .8f);
        grey50alpha = new Color(.2f, .2f, .2f, .5f);

        UILineGrey = new Color(.67f, .67f, .67f);
        UILineLightGrey = new Color(.67f, .67f, .67f);
        UILineDarkGrey = new Color(.57f, .57f, .57f);
        lineColor = Color.gray;
        uiLineGreyCol = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        uiLineGreyCol2 = new Color(0.45f, 0.45f, 0.45f, 0.35f);
        uiLineGreyCol3 = new Color(0.5f, 0.5f, 0.5f, 0.4f);


        if (isDark)
        {
            lineColor = new Color(0.2f, 0.2f, 0.2f);
            UILineGrey = new Color(.37f, .37f, .37f);
            UILineLightGrey = new Color(.53f, .53f, .53f);
            UILineDarkGrey = new Color(.29f, .29f, .29f);
            sceneViewBoxColorMoreOpaque = new Color(0.13f, 0.13f, 0.13f, 0.85f);
            sceneViewBoxColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        }
        else
        {
            lineColor = new Color(0.7f, 0.7f, 0.7f);
            UILineGrey = new Color(.82f, .82f, .82f);
            UILineLightGrey = new Color(.86f, .86f, .86f);
            UILineDarkGrey = new Color(.82f, .82f, .82f);
            sceneViewBoxColor = new Color(0.7f, 0.7f, 0.7f, 0.75f);
        }

        bgTrans2 = new Color(.9f, .9f, .9f, .4f);
        topScreenButtonsCol = new Color(.1f, .1f, .5f, 0.5f);
    }
    /// <summary>
    /// GUILayout Button with a conditional background color. Resets the background color after drawing the button.
    /// Uses a passed in delegate method to perform the action
    /// </summary>

    private void DrawConitionalColoredButton(bool condition, string text, string tooltip, Color backgroundColor,
        GUIStyle style, float width, System.Action onClick)
    {
        Color originalBackgroundColor = GUI.backgroundColor;

        if (condition)
            GUI.backgroundColor = backgroundColor;

        if (GUILayout.Button(new GUIContent(text, tooltip), style, GUILayout.Width(width)))
        {
            //-- the passed in delegate method to perform the action
            onClick?.Invoke();
        }
        if (condition)
            GUI.backgroundColor = originalBackgroundColor;
    }
    /// <summary>
    /// Sets the background and content (often text) colors for a GUI control based on a condition.
    /// </summary>
    /// <param name="condition">The condition that determines whether to change the colors.</param>
    /// <param name="bg">The background color to use if the condition is true.</param>
    /// <param name="content">The content color to use if the condition is true.</param>
    public void SetContentAndBGColorForControlOnCondition(bool condition, Color contentColor, Color bgColor)
    {
        if (condition == true)
        {
            GUI.contentColor = contentColor;
            GUI.backgroundColor = bgColor;
        }
    }
    //-------------
    /// <summary>
    /// Assumes they're the deafult Unity color white. If not use SaveContentAndBGColor() first, then RestoreContentAndBGColor()
    /// </summary>
    public void RestoreDefaultContentAndBGColorForControl()
    {
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
    }
    public void SaveContentAndBGColor(out Color contentColor, out Color bgColor)
    {
        contentColor = GUI.contentColor;
        bgColor = GUI.backgroundColor;
    }
    public void RestoreContentAndBGColor(Color contentColor, Color bgColor)
    {
        GUI.contentColor = contentColor;
        GUI.backgroundColor = bgColor;
    }
    //---------------------------------------
    public bool CheckFolderLocations(bool recreateIfNeeded = true)
    {
        //Timer t = new Timer("CheckFolderLocations()");

        bool changed = false;
        string changedStr = "";

        //-- Auto Fence Builder
        string oldCurrAFBLocation = af.currAutoFenceBuilderDir;

        string[] afbLocation = AssetDatabase.FindAssets("Auto Fence Builder");
        if (afbLocation.Length == 0 || afbLocation[0] == "")
        {
            Debug.LogWarning("Couldn't find afbLocation   Length " + afbLocation[0].Length + "\n");
            changedStr += "Couldn't find afbLocation\n";
        }
        else
            af.currAutoFenceBuilderDir = AssetDatabase.GUIDToAssetPath(afbLocation[0]);

        if (af.currAutoFenceBuilderDir != oldCurrAFBLocation)
        {
            af.ForceRebuildFromClickPoints();
            changedStr += "currAutoFenceBuilderDir  ";
        }

        //-- Materials
        string[] materialsLocation = AssetDatabase.FindAssets("AFWB_Materials");
        if (materialsLocation.Length == 0 || materialsLocation[0] == "")
        {
            Debug.LogWarning("Couldn't find materialsLocation in CheckFolderLocations()  \n");
            changedStr += "materialsLocation  ";
        }
        else
            af.currMaterialsDir = AssetDatabase.GUIDToAssetPath(materialsLocation[0]);

        //-- Textures
        string[] texturesLocation = AssetDatabase.FindAssets("AFWB_Textures");
        if (texturesLocation.Length == 0 || texturesLocation[0] == "")
        {
            Debug.LogWarning("Couldn't find texturesLocation in CheckFolderLocations()  \n");
            changedStr += "texturesLocation  ";
        }
        else
            af.currTexturesDir = AssetDatabase.GUIDToAssetPath(texturesLocation[0]);

        //-- Prefabs
        string[] prefabsLocation = AssetDatabase.FindAssets("AFWB_Prefabs");
        if (prefabsLocation.Length == 0 || prefabsLocation[0] == "")
        {
            Debug.LogWarning("Couldn't find prefabsLocation in CheckFolderLocations()  \n");
            changedStr += "prefabsLocation  ";
        }
        else
            af.currPrefabsDir = AssetDatabase.GUIDToAssetPath(prefabsLocation[0]);

        //-- System Files
        string[] systemFilesLocation = AssetDatabase.FindAssets("System_Do_Not_Remove");
        if (prefabsLocation.Length == 0 || prefabsLocation[0] == "")
        {
            Debug.LogWarning("Couldn't find systemFilesLocation in CheckFolderLocations()  \n");
            changedStr += "systemFilesLocation  ";
        }
        else
            af.systemFilesDir = AssetDatabase.GUIDToAssetPath(systemFilesLocation[0]);

        //-- Presets
        string[] presetsLocation = AssetDatabase.FindAssets("AFWB_Presets");
        if (presetsLocation.Length == 0 || presetsLocation[0] == "")
        {
            Debug.LogWarning("Couldn't find presetsLocation in CheckFolderLocations() \n");
            changedStr += "presetsLocation  ";
        }
        else
            af.currPresetsDir = AssetDatabase.GUIDToAssetPath(presetsLocation[0]);

        if (recreateIfNeeded)
            changedStr = RebuildFoldersIfNeeded(changedStr);

        if (changedStr != "")
        {
            AssetDatabase.Refresh();
            Debug.Log("Something Changed in CheckFolderLocations().   Rebuilding \n");
            Debug.Log(changedStr + "\n");
        }
        //t.End();
        return changed;
    }

    //---------------------------------------
    public string RebuildFoldersIfNeeded(string changedStr)
    {
        // User Folder
        string userAssetsPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB";
        bool folderExists = AssetDatabase.IsValidFolder(userAssetsPath);
        if (folderExists == false)
        {
            string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir, "UserAssets_AFWB");
            changedStr += "userAssetsPath  ";
        }
        // User Presets Folder
        string userPresetsPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/User_Presets";
        folderExists = AssetDatabase.IsValidFolder(userPresetsPath);
        if (folderExists == false)
        {
            string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir + "/UserAssets_AFWB", "User_Presets");
            changedStr += "userPresetsPath  ";
        }
        // User Prefabs
        string userPostsPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Posts";
        folderExists = AssetDatabase.IsValidFolder(userPostsPath);
        if (folderExists == false)
        {
            string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir + "/UserAssets_AFWB", "UserPrefabs_Posts");
            changedStr += "userPostsPath  ";
        }
        string userRailsPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Rails";
        folderExists = AssetDatabase.IsValidFolder(userRailsPath);
        if (folderExists == false)
        {
            string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir + "/UserAssets_AFWB", "UserPrefabs_Rails");
            changedStr += "userRailsPath  ";
        }
        string userExtrasPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/UserPrefabs_Extras";
        folderExists = AssetDatabase.IsValidFolder(userExtrasPath);
        if (folderExists == false)
        {
            string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir + "/UserAssets_AFWB", "UserPrefabs_Extras");
            changedStr += "userExtrasPath  ";
        }
        string userMeshesPath = af.currAutoFenceBuilderDir + "/UserAssets_AFWB/User_Meshes";
        folderExists = AssetDatabase.IsValidFolder(userMeshesPath);
        if (folderExists == false)
        {
            string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir + "/UserAssets_AFWB", "User_Meshes");
            changedStr += "userMeshesPath  ";
        }
        return changedStr;

    }
    //--------------
    private void ConvertAllPresets()
    {
        /*int numPresets = af.presetsEd.Count;
        for (int i = 0; i < numPresets; i++)
        {
            Debug.Log("Converting " + i + "/" + af.presetsEd.Count + "   " + af.presetsEd[i].name);
            ConvertOldPresetToNew(i, false, false);
        }
        AssetDatabase.SaveAssets();
        h.LoadAllScriptablePresets();
        af.ForceRebuildFromClickPoints();*/
    }

    //----------
    /*void HandleImportRotation()
    {
        if (rotationsWindowIsOpen == false)
        {
            rotWindow = new BakeRotationsWindow(af, kRailALayer);
            rotWindow.position = new Rect(300, 300, 690, 500);
            rotWindow.ShowPopup();
        }
        else
        {
            rotationsWindowIsOpen = false;
            if (rotWindow != null)
                rotWindow.Close();
        }
    }*/

    //--------------------------------
    private void PrintSequencerInfo(List<SeqItem> seqList, int start = 0, int end = AutoFenceCreator.kMaxNumSeqSteps)
    {
        for (int i = start; i < end; i++)
        {
            SeqItem seqStep = seqList[i];
            //Debug.Log(i + " svSize = " + seqStep.svSize + "    offset = " + seqStep.pos +  "\n");
            //Debug.Log(i + "  seqList offset = " + seqStep.pos + "        seqRailAOffset" +  af.seqRailAOffset[i] + "\n");
            // Debug.Log(i + "   svBackToFront: " + seqStep.svBackToFront + "   svMirrorZ: " + seqStep.svMirrorZ + "   svInvert: " + seqStep.svInvert);
        }
    }
    //====================================================
    // Prints the name of the Class and the Method that called it
    // There is also a version of this in AutoFenceCreator, it's just convenient to have it here too to avoid extra referencing
    // Useage:  Stack(this.GetType().Name) or Stack(T()); 
    public void StackLog(string classType, bool verbose = true, [CallerMemberName] string caller = null)
    {
        bool show = false;
        if (af != null)
            show = af.showStackTrace;
        if (caller == "Awake")
            show = true;
        if (show)
        {
            if (verbose == true)
                Debug.Log($"                         _____   [{classType}]  :  {caller}( )   _____\n");
            else
                Debug.Log($"{caller}( )\n");
        }
    }
    //====================================================
    public static List<ScriptablePresetAFWB> GetPrestsListStaticCopy()
    {
        return mainPresetListStaticCopy;
    }

    //--------------------------
    /*private static void InitializeGizmoDrawManager()
    {
        if (GizmoDrawManager.gizmoSingletonInstance == null)
        {
            GameObject gizmoManager = GameObject.Find("GizmoDrawManager");
            if (gizmoManager == null)
            {
                gizmoManager = new GameObject("GizmoDrawManager");
                gizmoManager.AddComponent<GizmoDrawManager>();
                Debug.Log("GizmoDrawManager created in the scene.");
            }
            else
            {
                Debug.Log("GizmoDrawManager already exists in the scene.");
            }
        }

        EditorApplication.update -= InitializeGizmoDrawManager;
    }*/
    //--------------------------
    void InitializeGizmoDrawManager()
    {
        /*if (af.gizmoManager == null)
        {
            af.gizmoManager = new GizmoDrawManager(af);
            //Debug.Log("GizmoDrawManager created.\n");
        }*/
        //EditorApplication.update -= InitializeGizmoDrawManager;
    }

    public void ResetState()
    {
        Debug.Log("AutoFenceCreator ResetState");

        //-- Clear and Init Prefab Lists
        if (af.postPrefabs != null)
        {
            af.postPrefabs.Clear();
            af.postPrefabs = null;
        }
        if (af.railPrefabs != null)
        {
            af.railPrefabs.Clear();
            af.railPrefabs = null;
        }
        if (af.extraPrefabs != null)
        {
            af.extraPrefabs.Clear();
            af.extraPrefabs = null;
        }
        //-- Clear and Init Pools
        if (af.postsPool != null)
        {
            af.postsPool.Clear();
            af.postsPool = null;
        }
        if (af.railsAPool != null)
        {
            af.railsAPool.Clear();
            af.railsAPool = null;
        }
        if (af.railsBPool != null)
        {
            af.railsBPool.Clear();
            af.railsBPool = null;
        }
        if (af.ex.extrasPool != null)
        {
            af.ex.extrasPool.Clear();
            af.ex.extrasPool = null;
        }
        if (af.subpostsPool != null)
        {
            af.subpostsPool.Clear();
            af.subpostsPool = null;
        }

        //-- Clear scriptable presets
        if (mainPresetList != null)
        {
            mainPresetList.Clear();
            mainPresetList = null;
        }

        af.CreateAllObjectsLists();
    }

    void SimulateFirstEnable()
    {
        ResetState();
        PostVector.LinkPostVectorParentList(af.postVectors);
        SetupEditor();
        af.CheckPostDirectionVectors(logMissing: true);
        af.ForceRebuildFromClickPoints();
    }
    //-------------------------------
    /// <summary>Registed to EditorApplication.update in OnEnable to check something every frame 
    /// We subscribe and unsubscribe to it depending on situation. If nothing subscribed to it, it won't be called
    /// Currently this used to stop showing the Unlock Mouse Button after a few seconds
    /// </summary>
    /// 
    private void OnCustomUpdate()
    {
        //-- Stop showing the Unlock Mouse Button after a few seconds
        //Debug.Log($"unlockMouseButtonTimer {unlockMouseButtonTimer.GetTime()} \n");
        /*if (unlockMouseButtonTimer.GetTime() > 3000)
        {
            showingUnlockMouseFromAFButton = false;
            dblClickScreenPoint = Vector2.zero;
            EditorApplication.update -= OnCustomUpdate;
            SceneView.RepaintAll();
            EditorWindow.GetWindow<SceneView>().Repaint();
        }*/
    }
    private void SetAndSyncInitialPresetIndices()
    {

        int launchPresetIndex = 0, currPresetIndex = af.currPresetIndex;
        if (currPresetIndex == 0)
        {
            launchPresetIndex = AssignLaunchPreset();
            //-- Note: CreatePools() via SetupPreset here.
            presetsEd.SetupPreset(launchPresetIndex, forceRebuild: false);
        }
        //Get the preset Menu Index for the current preset
        for (int i = 0; i < presetMenuNames.Count; i++)
        {
            if (presetMenuNames[i] == null)
                continue;
            string name = presetMenuNames[i];
            if (name.Contains(af.currPresetName))
                af.presetMenuIndexInDisplayList = i;
        }
        // find a preset in mainPresetList that matches the current preset name and return the index
        for (int i = 0; i < mainPresetList.Count; i++)
        {
            if (mainPresetList[i] == null)
                continue;
            string name = mainPresetList[i].name;
            if (name == af.currPresetName)
                af.currPresetIndex = i;
        }
    }
    //---------------------------------------
    //-- Assign/Changes the preset to "Template  1 Post and 1 Wall
    //-- Do this on first launch, but obviously not after hierarchy re-emable or compile
    int AssignLaunchPreset()
    {
        if (launchPresetIndex == -1)
            launchPresetIndex = FindPresetIndexByName("Template  1 Post and 1 Wall");
        if (launchPresetIndex == -1)
            launchPresetIndex = 0;

        int presetIndex = 0;
        if (af.launchPresetAssigned == false)
        {
            presetIndex = launchPresetIndex;
            if (af.allowContentFreeUse == true)
                presetIndex = 0;
            af.launchPresetAssigned = true;
        }
        return presetIndex;
    }
    //---------------------------------------
    public void ReloadPrefabs(bool rebuild = true)
    {
        LoadPrefabs();
        if (rebuild)
        {
            af.ResetAllPools();
            af.ForceRebuildFromClickPoints();
        }
    }
    //---------------------------------------
    public void ReloadPresets(bool rebuild = true)
    {
        presetsEd.LoadAllScriptablePresets(af.allowContentFreeUse);
        if (rebuild)
        {
            af.ResetAllPools();
            af.ForceRebuildFromClickPoints();
        }
    }
    //----------
    public int FindPresetIndexByName(string name)
    {
        if (mainPresetList == null)
        {
            Debug.LogError("mainPresetList is null in FindPresetIndexByName()\n");
            return -1;
        }
        for (int i = 0; i < mainPresetList.Count; i++)
        {
            if (mainPresetList[i].name == name)
                return i;
        }
        return -1;
    }

    //----------------
    public void DrawUILine(Color color, int widthStartPadding, int widthEndPadding, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.x += widthStartPadding;
        r.width -= widthEndPadding;
        EditorGUI.DrawRect(r, color);
    }
    //----------------
    public static void DrawUILine2(Color color, int widthStartPadding, int widthEndPadding, int thickness = 2, int heightPadding = 0)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(heightPadding + thickness));
        r.height = thickness;
        r.y += heightPadding / 2;
        r.x += widthStartPadding;
        r.x -= 1;
        r.width -= (widthStartPadding + widthEndPadding);
        EditorGUI.DrawRect(r, color);
    }

    public void TidyUp()
    {
        //========================
        //  Tidy up after Undo
        //========================
        if (Event.current.commandName == "UndoRedoPerformed")
        {
            af.ResetAllPools();
            af.ForceRebuildFromClickPoints();
        }
        if (isDark)
        {
            GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
        }


    }

    //---------------------------------------
    // Reversing the order also has the effect of making all objects face 180 the other way.
    void ReverseClickPoints()
    {
        af.clickPoints.Reverse();
        af.handles.Reverse();
        af.clickPointFlags.Reverse();
        af.ForceRebuildFromClickPoints();
    }

    LayerSet GetLayerSetFromToolbarChoice()
    {
        //create a switch statment for all the values of ComponentToolbar
        LayerSet layer = LayerSet.railALayer;
        switch (af.componentToolbar)
        {
            case ComponentToolbar.railsA:
                layer = LayerSet.railALayer;
                break;
            case ComponentToolbar.railsB:
                layer = LayerSet.railBLayer;
                break;
            case ComponentToolbar.posts:
                layer = LayerSet.postLayer;
                break;
            case ComponentToolbar.extras:
                layer = LayerSet.extraLayer;
                break;
            case ComponentToolbar.subposts:
                layer = LayerSet.subpostLayer;
                break;
        }
        return layer;
    }
    //-------------------------------------------
    private void SwitchToolbarComponentViewOnClick(Event currentEvent, LayerSet hoveredLayer)
    {
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.shift == false && currentEvent.control == false)
        {
            if (hoveredLayer == LayerSet.railALayer)
                ShowRailAControls();
            else if (hoveredLayer == LayerSet.railBLayer)
                ShowRailBControls();
            else if (hoveredLayer == LayerSet.postLayer)
                ShowPostControls();
            else if (hoveredLayer == LayerSet.extraLayer)
                ShowExtraControls();
            else if (hoveredLayer == LayerSet.subpostLayer)
                ShowSubpostControls();
        }
    }
    //-------------------------------------------
    private void SwitchToolbarComponentView(LayerSet layer)
    {
        switch (layer)
        {
            case LayerSet.railALayer:
                ShowRailAControls();
                break;
            case LayerSet.railBLayer:
                ShowRailBControls();
                break;
            case LayerSet.postLayer:
                ShowPostControls();
                break;
            case LayerSet.extraLayer:
                ShowExtraControls();
                break;
            case LayerSet.subpostLayer:
                ShowSubpostControls();
                break;
            default:
                return;
        }
    }

    //---------------------------------
    private void ShowRailAControls()
    {
        af.componentToolbar = ComponentToolbar.railsA;
        componentToolbarProp.enumValueIndex = (int)af.componentToolbar;
    }
    private void ShowRailBControls()
    {
        af.componentToolbar = ComponentToolbar.railsB;
        componentToolbarProp.enumValueIndex = (int)af.componentToolbar;
    }
    private void ShowPostControls()
    {
        af.componentToolbar = ComponentToolbar.posts;
        componentToolbarProp.enumValueIndex = (int)af.componentToolbar;
    }
    private void ShowExtraControls()
    {
        af.componentToolbar = ComponentToolbar.extras;
        componentToolbarProp.enumValueIndex = (int)af.componentToolbar;
    }
    private void ShowSubpostControls()
    {
        af.componentToolbar = ComponentToolbar.subposts;
        componentToolbarProp.enumValueIndex = (int)af.componentToolbar;
    }
    public static void CreateClickPointAtPostPosition(Vector3 position, AutoFenceCreator af, bool rebuild = true)
    {
        af.InsertPost(position);
        af.ResetPoolForLayer(LayerSet.postLayer);
        if (rebuild)
            af.ForceRebuildFromClickPoints();
    }
    public static void ConvertToClickPoint(Vector3 position, AutoFenceCreator af)
    {
        CreateClickPointAtPostPosition(position, af);
    }
    //-----------------------------------------------

    // Section Index is the sequential position of the part along the whole fence length
    // It's found by parsing the name of the GameObject which had the section num added during Build.
    private static int GetSectionIndexFromName(GameObject go)
    {

        //--  Rail
        int sectStart = go.name.IndexOf("_Rail[");

        //--  Post
        if (FindLayerFromParentName(go) == LayerSet.postLayer)
            sectStart = go.name.IndexOf("_Post[");

        if (sectStart == -1)
            return -1;

        string sectionStr = go.name.Substring(sectStart + 6);
        int sectEnd = sectionStr.IndexOf("v");
        sectionStr = sectionStr.Remove(sectEnd - 1);
        int sectionIndex = int.Parse(sectionStr);
        return sectionIndex;
    }

    private static LayerSet FindLayerFromParentName(GameObject go)
    {
        GameObject parent = go.transform.parent.gameObject;
        LayerSet layerSet = LayerSet.railALayer;

        if (parent.name.Contains("RailsAGrouped"))
            layerSet = LayerSet.railALayer;
        else if (parent.name.Contains("RailsBGrouped"))
            layerSet = LayerSet.railBLayer;
        if (parent.name.Contains("PostsGrouped"))
            layerSet = LayerSet.postLayer;

        return layerSet;
    }

    //------------------------------------------
    public Vector3 EnforceVectorNonZero(Vector3 inVec, float nonZeroValue)
    {
        if (inVec.x == 0) inVec.x = nonZeroValue;
        if (inVec.y == 0) inVec.y = nonZeroValue;
        if (inVec.z == 0) inVec.z = nonZeroValue;
        return inVec;
    }
    public Vector3 EnforceVectorMinimums(Vector3 inVec, Vector3 mins)
    {
        if (inVec.x < mins.x) inVec.x = mins.x;
        if (inVec.y < mins.y) inVec.y = mins.y;
        if (inVec.z < mins.z) inVec.z = mins.z;
        return inVec;
    }
    public Vector3 EnforceRange360(Vector3 inVec)
    {
        inVec.x = inVec.x % 360;
        inVec.y = inVec.y % 360;
        inVec.z = inVec.z % 360;
        return inVec;
    }
    public Vector3 EnforceVectorMaximums(Vector3 inVec, Vector3 maxs)
    {
        if (inVec.x > maxs.x) inVec.x = maxs.x;
        if (inVec.y > maxs.y) inVec.y = maxs.y;
        if (inVec.z > maxs.z) inVec.z = maxs.z;
        return inVec;
    }
    public Vector3 EnforceVectorMinMax(Vector3 inVec, float globalMin, float globalMax)
    {
        if (inVec.x < globalMin) inVec.x = globalMin;
        if (inVec.y < globalMin) inVec.y = globalMin;
        if (inVec.z < globalMin) inVec.z = globalMin;

        if (inVec.x > globalMax) inVec.x = globalMax;
        if (inVec.y > globalMax) inVec.y = globalMax;
        if (inVec.z > globalMax) inVec.z = globalMax;
        return inVec;
    }
    public Vector3 EnforceVectorMinMax(Vector3 inVec, Vector3 minsXYZ, Vector3 maxs)
    {
        inVec.x = Mathf.Clamp(inVec.x, minsXYZ.x, maxs.x);
        inVec.y = Mathf.Clamp(inVec.y, minsXYZ.y, maxs.y);
        inVec.z = Mathf.Clamp(inVec.z, minsXYZ.z, maxs.z);
        return inVec;
    }

    //---------------------------------------------------------------------
    Vector3 SnapHandles(Vector3 inVec, float val)
    {
        Vector3 snapVec = Vector3.zero;
        snapVec.y = inVec.y;

        snapVec.x = Mathf.Round(inVec.x / val) * val;
        snapVec.z = Mathf.Round(inVec.z / val) * val;

        return snapVec;
    }
    //---------------------------------------------------------------------
    // move the folder handles out of the way of the real moveable handles
    void RepositionFolderHandles(Vector3 clickPoint)
    {
        Vector3 pos = clickPoint;
        if (af.clickPoints.Count > 0)
        {
            pos = af.clickPoints[0];
        }
        af.gameObject.transform.position = pos + new Vector3(0, 4, 0);
    }

    //================================================================================
    //
    //                  Checks & Pre-fligh2
    //
    //================================================================================

    /// <summary> will get called every frameFreq that OnInspectorGUI is called
    /// Currently used ti check for new prefabs and to check the folder locations, and to remove the unlock-mouse button
    /// </summary>
    /// <param name="frameFreq"></param>
    private void CheckPeriodicallyFromOnInspectorGUI(int frameFreq)
    {
        // print frame count
        //Debug.Log($"frameCount: {frameCount}");
        if (frameCount > frameFreq)
        {
            frameCount = 0;
            CheckFolderLocations(true);
            //af.ClearConsole();
        }
        if (frameCount % 200 == 0)
        {
            CheckForNewPrefabs();
            frameCount = 0;
        }
        //showingUnlockMouseFromAFButton = false;
    }


    //-----------------------------------------
    void UnloadUnusedAssets()
    {
        af.postPrefabs.Clear();
        af.railPrefabs.Clear();
        af.subPrefabs.Clear();
        af.subJoinerPrefabs.Clear();
        af.extraPrefabs.Clear();
        userUnloadedAssets = true;
        af.needsReloading = true;
    }
    //-----------------------------------------
    void RemoveUnusedAssetsFromProject()
    {
        if (removingAssets == true)
        {
            GUILayout.Label("'Unload Unused Assets' LABEL");
        }
    }


}