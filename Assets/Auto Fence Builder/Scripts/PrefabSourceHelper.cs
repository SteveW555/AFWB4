using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AFWB
{
    public partial class AutoFenceCreator
    {
        //===============================================
        //           Main Prefab Assignments
        //===============================================
        //---------------------------
        public PrefabTypeAFWB GetPrefabTypeFromLayer(LayerSet layer)
        {
            PrefabTypeAFWB prefabType = PrefabTypeAFWB.postPrefab;
            if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
                prefabType = PrefabTypeAFWB.railPrefab;
            else if (layer == LayerSet.extraLayerSet)
                prefabType = PrefabTypeAFWB.extraPrefab;

            return prefabType;
        }
        //---------------------------
        // this is only used in situations where the post/subpost, or railA/RailB distinction doesn't matter
        public LayerSet GetLayerFromPrefabType(PrefabTypeAFWB prefabType)
        {
            LayerSet layer = LayerSet.postLayerSet;
            if (prefabType == PrefabTypeAFWB.postPrefab)
                layer = LayerSet.postLayerSet;
            else if (prefabType == PrefabTypeAFWB.railPrefab)
                layer = LayerSet.railALayerSet;
            else if (prefabType == PrefabTypeAFWB.extraPrefab)
                layer = LayerSet.extraLayerSet;
            return layer;
        }
        //---------------------
        public void SetPrefabMenuForLayer(int prefabIndex, LayerSet layer)
        {
            int menuIndex = ConvertPrefabIndexToMenuIndexForLayer(prefabIndex, layer);

            SetPrafabMenuIndexForLayer(menuIndex, layer);
        }
        public void SetPrafabMenuIndexForLayer(int menuIndex, LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                currentRail_PrefabMenuIndex[kRailALayerInt] = menuIndex;
            else if (layer == LayerSet.railBLayerSet)
                currentRail_PrefabMenuIndex[kRailBLayerInt] = menuIndex;
            else if (layer == LayerSet.postLayerSet)
                currentPost_PrefabMenuIndex = menuIndex;
            else if (layer == LayerSet.subpostLayerSet)
                currentSubpost_PrefabMenuIndex = menuIndex;
            else if (layer == LayerSet.extraLayerSet)
                currentExtra_PrefabMenuIndex = menuIndex;
        }

        //===============================================
        //           Variations
        //===============================================
        public void PopulateSourceVariantMenuForLayer(int prefabIndex, LayerSet layer)
        {
            SourceVariant sourceVariant = null;

        }
        public bool GetUseLayerVariations(LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                return useRailVariations[0];
            else if (layer == LayerSet.railBLayerSet)
                return useRailVariations[1];
            else if (layer == LayerSet.postLayerSet)
                return usePostVariations;
            return false;
        }

        //===============================================
        //           Source Variants
        //===============================================
        #region SourceVariants

        //---------------
        public List<string> GetSourceVariantGoNamesForLayer(LayerSet layer)
        {
            List<string> names = new List<string>();
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            int count = NumPostVariantsInUse;
            if (IsRailLayer(layer))
                count = numRailVariantsInUse[(int)layer];

            for (int i = 0; i < count + 1; i++)
            {
                SourceVariant thisVariant = sourceVariants[i];
                if (thisVariant == null)
                    names.Add("Null SourceVariant");
                else if (thisVariant.Go == null)
                    names.Add("Null GameObject on SourceVariant");
                else
                    names.Add(thisVariant.Go.name);
            }
            return names;
        }
        //----------------------------------------

        //-- Optionally warns if an individual SourceVariant[0] == null
        public List<SourceVariant> GetSourceVariantsForLayer(LayerSet layer, bool warn = true, [CallerMemberName] string caller = null)
        {
            List<SourceVariant> sourceVariants = null;

            if (layer == LayerSet.subpostLayerSet)
                return null;//-- For v4.1

            if (layer == LayerSet.postLayerSet)
                sourceVariants = postSourceVariants;
            else if (layer == LayerSet.railALayerSet)
                sourceVariants = railSourceVariants[kRailALayerInt];
            else if (layer == LayerSet.railBLayerSet)
                sourceVariants = railSourceVariants[kRailBLayerInt];

            if (sourceVariants == null && warn == true)
                Debug.LogWarning($"{GetLayerNameAsString(layer)} sourceVariants  was null  in GetSourceVariantsForLayer()      Called by {caller}\n");
            else if (sourceVariants.Count == 0 && warn == true)
                Debug.LogWarning($"{GetLayerNameAsString(layer)} sourceVariants.Count was 0  in  GetSourceVariantsForLayer()      {caller}\n");
            else if (sourceVariants[0] == null && warn == true)
                Debug.LogWarning($"{GetLayerNameAsString(layer)} sourceVariants exists with Count = {sourceVariants.Count}, " +
                    $"but sourceVariant[0] was  NULL   in  GetSourceVariantsForLayer()      {caller} \n");

            return sourceVariants;
        }
        //-----------------------------
        public string GetSourceVariantGONameAtIndexForLayer(int sourceVariantIndex, LayerSet layer, [CallerMemberName] string caller = null)
        {
            GameObject go = GetSourceVariantGOAtIndexForLayer(sourceVariantIndex, layer, caller);
            if (go == null)
                return "NULL";
            return go.name;
        }
        //---------------------------
        public GameObject GetSourceVariantGOAtIndexForLayer(int sourceVariantIndex, LayerSet layer, [CallerMemberName] string caller = null)
        {
            SourceVariant sourceVariant = GetSourceVariantAtIndexForLayer(sourceVariantIndex, layer, caller);
            GameObject go = sourceVariant.Go;
            if (go == null)
                Debug.LogWarning($"sourceVariant.Go ({sourceVariantIndex})  {GetLayerNameAsString(layer)} was null  in GetSourceVariantGOAtIndexForLayer() \n");
            return go;
        }
        //---------------------------
        public SourceVariant GetSourceVariantAtIndexForLayer(int sourceVariantIndex, LayerSet layer, [CallerMemberName] string caller = null)
        {
            int layerIndex = (int)layer;
            string layerName = GetLayerNameAsString(layer);
            PrefabTypeAFWB prefabTypeAFWB = GetPrefabTypeFromLayer(layer);
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer, true, caller);

            //-- Check the index is in range


            if (sourceVariants.Count <= sourceVariantIndex)
            {
                Debug.LogWarning($"Index ({sourceVariantIndex})  out of range for {layerName} sourceVariants (count = {sourceVariants.Count})  in GetSourceVariantAtIndexForLayer()  ");
                return null;
            }

            SourceVariant sourceVariant = sourceVariants[sourceVariantIndex];
            if (sourceVariant == null)
                Debug.LogWarning($"sourceVariant ({sourceVariantIndex})  {layerName} was null  in GetSourceVariantAtIndexForLayer()  ");

            return sourceVariant;
        }

        // returns ONLY the number of sourceVariants, does not include Main
        // unless incMain = true
        public int GetNumSourceVariantsInUseForLayer(LayerSet layer, bool incMain)
        {
            int num = 0;
            if (layer == LayerSet.railALayerSet)
                num = numRailVariantsInUse[kRailALayerInt];
            else if (layer == LayerSet.railBLayerSet)
                num = numRailVariantsInUse[kRailBLayerInt];
            else if (layer == LayerSet.postLayerSet)
                num = NumPostVariantsInUse;
            if (incMain == true)
                num += 1;
            return num;
        }
        public void SetNumVariationsInUseForLayer(LayerSet layer, int num)
        {
            if (num > AutoFenceCreator.kMaxNumSourceVariants)
                num = AutoFenceCreator.kMaxNumSourceVariants;

            if (layer == LayerSet.railALayerSet)
                numRailVariantsInUse[kRailALayerInt] = num;
            else if (layer == LayerSet.railBLayerSet)
                numRailVariantsInUse[kRailBLayerInt] = num;
            else if (layer == LayerSet.postLayerSet)
                NumPostVariantsInUse = num;
        }

        //----------------------
        //-- These are the kMaxNumSourceVariants menu indices for each layer
        public List<int> GetSourceVariantMenuIndicesForLayer(LayerSet layer)
        {
            List<int> menuIndices = new List<int>();


            if (layer == LayerSet.railALayerSet)
                menuIndices = railASourceVariant_MenuIndices;
            else if (layer == LayerSet.railBLayerSet)
                menuIndices = railBSourceVariant_MenuIndices;
            else if (layer == LayerSet.postLayerSet)
                menuIndices = postSourceVariant_MenuIndices;

            return menuIndices;
        }
        //-----------------------------------------------------------
        // if only active is true it only prints numInUse and notkMaxNumVariants
        public void PrintSourceVariantGOsForAllLayers(bool activeOnly)
        {
            PrintSourceVariantGOsForLayer(LayerSet.railALayerSet, activeOnly);
            PrintSourceVariantGOsForLayer(LayerSet.railBLayerSet, activeOnly);
            PrintSourceVariantGOsForLayer(LayerSet.postLayerSet, activeOnly);
        }
        //-----------------------------------------------------------
        public void PrintCurrentPrefabsForAllLayer()
        {
            Debug.Log($"\n     -----     Current Layer Prefabs:     ----- \n");
            PrintCurrentPrefabForLayer(LayerSet.railALayerSet);
            PrintCurrentPrefabForLayer(LayerSet.railBLayerSet);
            PrintCurrentPrefabForLayer(LayerSet.postLayerSet);
            PrintCurrentPrefabForLayer(LayerSet.subpostLayerSet);
            PrintCurrentPrefabForLayer(LayerSet.extraLayerSet);
        }
        //-----------------------------------------------------------
        public void PrintCurrentPrefabForLayer(LayerSet layer)
        {
            Debug.Log($"Current Prefab for {GetLayerNameAsString(layer)} is {GetMainPrefabForLayer(layer).name} \n");
        }
        //-----------------------------------------------------------
        // if only active is true it only prints numInUse and notkMaxNumVariants
        public void PrintSourceVariantGOsForLayer(LayerSet layer, bool activeOnly, bool warn = false)
        {
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            int numInUse = GetNumSourceVariantsInUseForLayer(layer, incMain: true);
            int numToShow = kMaxNumSourceVariants;
            string layerName = GetLayerNameAsString(layer);
            if (activeOnly)
                numToShow = numInUse;

            Debug.Log($"\n                       ------  sourceVariant.Gos for  {layerName}  ------ \n");

            if (numToShow == 0)
                Debug.Log($"PrintSourceVariantGOsForLayer() 0 to show \n");

            for (int i = 0; i < numToShow; i++)
            {
                string nameStr = "", inUseStr = i < numInUse ? "  used" : " (__not in use__)";

                string goName = GetSourceVariantGONameAtIndexForLayer(i, layer);
                if (goName == "")
                    goName = " **  NULL  ** ";
                //else
                goName = StripLayerTypeFromNameStatic(goName);

                string inUseStrCol = "<color=#53D353>";
                string notInUseStrCol = "<color=#979F97>";
                string colorStr = inUseStrCol;


                colorStr = (i < numInUse) ? inUseStrCol : notInUseStrCol;
                Debug.Log($"{colorStr}         Post = {GetMainPrefabForLayer(LayerSet.postLayerSet).name} \n</color>");


                Debug.Log($"{colorStr}    ***   Source Variant {i}:    {goName}    {inUseStr} \n</color>");


                if (warn)
                    Debug.Log($"   ***   Source Variant {i}:    {nameStr}    {inUseStr} \n");
            }
        }
        //-----------------------------------------------------------
        // if only active is true it only prints numInUse and notkMaxNumVariants
        public string GetSourceVariantGOsForLayerAsString(LayerSet layer)
        {
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            string layerName = GetLayerNameAsString(layer);
            string sourceVariantListStr = layerName + ":  ";
            for (int i = 0; i < kMaxNumSourceVariants; i++)
            {
                string goName = GetSourceVariantGONameAtIndexForLayer(i, layer);
                sourceVariantListStr += goName + ",  ";
            }
            return sourceVariantListStr;
        }
        public void PrintSourceVariantMenuGos(LayerSet layer, bool activeOnly)
        {
            if ((int)layer > 2)
                Debug.Log($"layer {GetLayerNameAsString(layer)} has no sourceVariants in PrintSourceVariantMenuGos()");

            GameObject go = null;
            int numInUse = GetNumSourceVariantsInUseForLayer(layer, incMain: true);
            int numToShow = kMaxNumSourceVariants, prefabIndex;
            if (activeOnly)
                numToShow = numInUse;
            List<int> sourceVar_PrefabIndices = GetSourceVariantMenuIndicesListForLayer(layer);
            List<GameObject> prefabList = GetPrefabsForLayer(layer);
            for (int i = 0; i < numToShow; i++)
            {
                prefabIndex = sourceVar_PrefabIndices[i];
                go = prefabList[prefabIndex];
                string goName = StripLayerTypeFromNameStatic(go.name);
                //Debug.Log($"Variant {i}     {goName}  { (i < numInUse) ? \"  used\" : \" (__not in use__)\" } \n");
                Debug.Log($"Variant {i}     {goName}  {(i < numInUse ? "    used" : "    (__not in use__)")} \n");
            }
        }
        #endregion
        //===============================================
        //           Sequencer
        //===============================================
        public int GetNumSeqStepsForLayer(LayerSet layer)
        {
            Sequencer sequencer = GetSequencerForLayer(layer);
            if (sequencer == null)
                return 0;
            return sequencer.Length();
        }
        //-- Note difference between this GetSequence: List<SeqItem>
        //-- and GetSequenceR : Sequencer
        public List<SeqItem> GetSequenceForLayer(LayerSet layer)
        {
            Sequencer sequencer = GetSequencerForLayer(layer);
            if (sequencer == null)
                return null;

            return sequencer.seqList;
        }
        //---------------------
        public int GetStepNumAtSectionIndexForLayer(int sectionIndex, LayerSet layer)
        {
            List<SeqItem> seqList = GetSequenceForLayer(layer);
            int numSeqSteps = GetNumSeqStepsForLayer(layer);
            int stepNumForSectionIndex = sectionIndex % numSeqSteps;

            return stepNumForSectionIndex;
        }
        public SeqItem GetSeqItemAtSectionIndexForLayer(int sectionIndex, LayerSet layer)
        {
            //-- First work out what the step num is from the section index
            int stepNumForSectionIndex = GetStepNumAtSectionIndexForLayer(sectionIndex, layer);

            //-- Then get the SeqItem at that step
            SeqItem SeqItem = GetSeqItemAtStepForLayer(stepNumForSectionIndex, layer);

            return SeqItem;
        }
        //--------------
        public SeqItem GetSeqItemAtStepForLayer(int seqStepNum, LayerSet layer)
        {
            List<SeqItem> seqList = GetSequenceForLayer(layer);
            if (seqStepNum >= seqList.Count)
            {
                Debug.LogError($"seqStepNum {seqStepNum} is out of range for sequencer count {seqList.Count} for layer {layer} \n");
                return null;
            }
            SeqItem SeqItem = seqList[seqStepNum];
            return SeqItem;
        }
        //--------------
        public SourceVariant GetSourceVariantAtStepForLayer(int seqStepNum, LayerSet layer)
        {
            List<SeqItem> seqList = GetSequenceForLayer(layer);

            if (seqStepNum >= seqList.Count)
            {
                Debug.LogError($"seqStepNum {seqStepNum} is out of range for sequencer count {seqList.Count} for layer {layer} \n");
                return null;
            }

            SeqItem SeqItem = seqList[seqStepNum];
            int sourceVariantIndex = SeqItem.sourceVariantIndex;

            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);

            if (sourceVariantIndex >= sourceVariants.Count)
            {
                Debug.LogError($"sourceVariantIndex {sourceVariantIndex} is out of range for sourceVariants count {sourceVariants.Count} for layer {layer} \n");
                return null;
            }

            SourceVariant sourceVariant = sourceVariants[sourceVariantIndex];
            return sourceVariant;
        }
        public GameObject GetSourceVariantGOAtStepForLayer(int seqStepNum, LayerSet layer)
        {
            SourceVariant sourceVariant = GetSourceVariantAtStepForLayer(seqStepNum, layer);
            GameObject go = sourceVariant.Go;
            return go;
        }
        /*public bool GetUseSequencerForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                return useRailSequencer[0];
            else if (layer == LayerSet.railBLayerSet)
                return useRailSequencer[1];
            else if (layer == LayerSet.postLayerSet)
                return usePostSequencer;
            return false;
        }*/
        public void PrintSeqGOs(LayerSet layer, bool onlyShowStepsInUse = true)
        {
            List<SeqItem> seq = GetSequenceForLayer(layer);
            SeqItem.PrintSeqFieldListByName(seq, "singleVarGO", layer, this, onlyShowStepsInUse);

        }


        public void PrintSeqStepGOs(LayerSet layer, bool activeOnly)
        {
            List<SeqItem> SeqItems = GetSequenceForLayer(layer);
            int numSteps = GetNumSeqStepsForLayer(layer);
            List<SourceVariant> variants = GetSourceVariantsForLayer(layer);


            for (int i = 0; i < numSteps; i++)
            {
                int varIndex = SeqItems[i].sourceVariantIndex;
                GameObject go = variants[i].Go;

                Debug.Log($"Seq Step {i}:     [{varIndex}]{go.name}  \n");
            }
        }




        // will use the index of step 0 if its index > numVariants for the layer
        public int GetVariantIndexForStep(int stepNum, LayerSet layer, bool limitToNumVariants = true)
        {
            int numSteps = GetNumSeqStepsForLayer(layer);
            int seqStep = stepNum % numSteps;
            List<SeqItem> seq = GetSequenceForLayer(layer);
            int variantIndex = 0;
            if (seq != null)
                variantIndex = seq[seqStep].sourceVariantIndex;
            if (limitToNumVariants == true && variantIndex > GetNumSourceVariantsInUseForLayer(layer, incMain: true))
                variantIndex = seq[0].sourceVariantIndex;
            return variantIndex;
        }
        public SourceVariant GetSourceVariantForSeqStep(int stepNum, LayerSet layer, bool limitToNumVariants = true)
        {
            int numSteps = GetNumSeqStepsForLayer(layer);
            int seqStep = stepNum % numSteps;
            List<SeqItem> seq = GetSequenceForLayer(layer);
            int sourceVariantIndex = 0;
            if (seq != null)
                sourceVariantIndex = seq[seqStep].sourceVariantIndex;
            if (limitToNumVariants == true && sourceVariantIndex > GetNumSourceVariantsInUseForLayer(layer, incMain: true))
                sourceVariantIndex = seq[0].sourceVariantIndex;

            SourceVariant variant = GetSourceVariantAtIndexForLayer(sourceVariantIndex, layer);
            return variant;
        }

        // equivalent to: singleVarGO =  SeqItems[sourceVariantIndex].singleVarGO; but safer
        public GameObject GetGoForSeqGoIndex(int goIndex, LayerSet layer, bool limitToNumVariants = true)
        {
            if (limitToNumVariants == true && goIndex > GetNumSourceVariantsInUseForLayer(layer, incMain: true))
                goIndex = GetVariantIndexForStep(0, layer);

            List<SourceVariant> variants = GetSourceVariantsForLayer(layer);

            GameObject go = variants[goIndex].Go;

            return go;
        }
        public GameObject GetGoForSequencerStep(int stepNum, LayerSet layer)
        {
            SourceVariant sourceVariant = GetSourceVariantForSeqStep(stepNum, layer);
            return sourceVariant.Go;
        }
        public int GetVariantIndexForSequencerStep(int stepNum, LayerSet layer)
        {
            List<SeqItem> seq = GetSequenceForLayer(layer);
            SeqItem SeqItem = seq[stepNum];
            return SeqItem.sourceVariantIndex;
        }

        public VariationMode GetVariationModeForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                return variationModeRailA;
            else if (layer == LayerSet.railBLayerSet)
                return variationModeRailA;
            else if (layer == LayerSet.postLayerSet)
                return variationModePost;

            else return VariationMode.sequenced;
        }

        public bool GetAllowRandomizationForLayer(LayerSet layer)
        {
            bool allow = false;
            if (layer == LayerSet.railALayerSet)
                allow = allowRailRandomization[kRailALayerInt];
            else if (layer == LayerSet.railBLayerSet)
                allow = allowRailRandomization[kRailBLayerInt];
            else if (layer == LayerSet.postLayerSet)
                allow = allowPostRandomization;
            /*else if (layer == LayerSet.subpostLayerSet) // For v4.0
                allow = allowSubpostRandomization;
            else if (layer == LayerSet.extraLayerSet)
                allow = allowExtraRandomization;*/
            return allow;
        }
        public void ToggleRandomizationForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                allowRailRandomization[kRailALayerInt] = !allowRailRandomization[kRailALayerInt];
            else if (layer == LayerSet.railBLayerSet)
                allowRailRandomization[kRailBLayerInt] = !allowRailRandomization[kRailBLayerInt];
            else if (layer == LayerSet.postLayerSet)
                allowPostRandomization = !allowPostRandomization;
            /*else if (layer == LayerSet.subpostLayerSet) // For v4.0
                 allowSubpostRandomization = !allowSubpostRandomization;
            else if (layer == LayerSet.extraLayerSet)
                 allowExtraRandomization = !allowExtraRandomization;*/
        }
        public bool GetUseVariationsForLayer(LayerSet layer)
        {
            bool use = false;
            if (layer == LayerSet.railALayerSet)
                use = useRailVariations[0];
            else if (layer == LayerSet.railBLayerSet)
                use = useRailVariations[1];
            else if (layer == LayerSet.postLayerSet)
                use = usePostVariations;
            /*else if (layer == LayerSet.subpostLayerSet) 
                allow = allowSubpostRandomization;*/
            return use;
        }
        public void ToggleUseVariationsForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                useRailVariations[0] = !useRailVariations[0];
            else if (layer == LayerSet.railBLayerSet)
                useRailVariations[1] = !useRailVariations[1];
            else if (layer == LayerSet.postLayerSet)
                usePostVariations = !usePostVariations;

        }


        public void ToggleUseSinglesForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayerSet)
                useRailSingles[0] = !useRailSingles[0];
            else if (layer == LayerSet.railBLayerSet)
                useRailSingles[1] = !useRailSingles[1];
            else if (layer == LayerSet.postLayerSet)
                usePostSingles = !usePostSingles;
        }





    }
}
