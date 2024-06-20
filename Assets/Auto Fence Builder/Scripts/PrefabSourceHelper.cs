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
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                prefabType = PrefabTypeAFWB.railPrefab;
            else if (layer == LayerSet.extraLayer)
                prefabType = PrefabTypeAFWB.extraPrefab;

            return prefabType;
        }
        //---------------------------
        // this is only used in situations where the post/subpost, or railA/RailB distinction doesn't matter
        public LayerSet GetLayerFromPrefabType(PrefabTypeAFWB prefabType)
        {
            LayerSet layer = LayerSet.postLayer;
            if (prefabType == PrefabTypeAFWB.postPrefab)
                layer = LayerSet.postLayer;
            else if (prefabType == PrefabTypeAFWB.railPrefab)
                layer = LayerSet.railALayer;
            else if (prefabType == PrefabTypeAFWB.extraPrefab)
                layer = LayerSet.extraLayer;
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
            if (layer == LayerSet.railALayer)
                currentRail_PrefabMenuIndex[kRailALayerInt] = menuIndex;
            else if (layer == LayerSet.railBLayer)
                currentRail_PrefabMenuIndex[kRailBLayerInt] = menuIndex;
            else if (layer == LayerSet.postLayer)
                currentPost_PrefabMenuIndex = menuIndex;
            else if (layer == LayerSet.subpostLayer)
                currentSubpost_PrefabMenuIndex = menuIndex;
            else if (layer == LayerSet.extraLayer)
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
            if (layer == LayerSet.railALayer)
                return useRailVariations[0];
            else if (layer == LayerSet.railBLayer)
                return useRailVariations[1];
            else if (layer == LayerSet.postLayer)
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

            if (layer == LayerSet.subpostLayer)
                return null;//-- For v4.1

            if (layer == LayerSet.postLayer)
                sourceVariants = postSourceVariants;
            else if (layer == LayerSet.railALayer)
                sourceVariants = railSourceVariants[kRailALayerInt];
            else if (layer == LayerSet.railBLayer)
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
            if (layer == LayerSet.railALayer)
                num = numRailVariantsInUse[kRailALayerInt];
            else if (layer == LayerSet.railBLayer)
                num = numRailVariantsInUse[kRailBLayerInt];
            else if (layer == LayerSet.postLayer)
                num = NumPostVariantsInUse;
            if (incMain == true)
                num += 1;
            return num;
        }
        public void SetNumVariationsInUseForLayer(LayerSet layer, int num)
        {
            if (num > AutoFenceCreator.kMaxNumSourceVariants)
                num = AutoFenceCreator.kMaxNumSourceVariants;

            if (layer == LayerSet.railALayer)
                numRailVariantsInUse[kRailALayerInt] = num;
            else if (layer == LayerSet.railBLayer)
                numRailVariantsInUse[kRailBLayerInt] = num;
            else if (layer == LayerSet.postLayer)
                NumPostVariantsInUse = num;
        }

        //----------------------
        //-- These are the kMaxNumSourceVariants menu indices for each layer
        public List<int> GetSourceVariantMenuIndicesForLayer(LayerSet layer)
        {
            List<int> menuIndices = new List<int>();


            if (layer == LayerSet.railALayer)
                menuIndices = railASourceVariant_MenuIndices;
            else if (layer == LayerSet.railBLayer)
                menuIndices = railBSourceVariant_MenuIndices;
            else if (layer == LayerSet.postLayer)
                menuIndices = postSourceVariant_MenuIndices;

            return menuIndices;
        }
        //-----------------------------------------------------------
        // if only active is true it only prints numInUse and notkMaxNumVariants
        public void PrintSourceVariantGOsForAllLayers(bool activeOnly)
        {
            PrintSourceVariantGOsForLayer(LayerSet.railALayer, activeOnly);
            PrintSourceVariantGOsForLayer(LayerSet.railBLayer, activeOnly);
            PrintSourceVariantGOsForLayer(LayerSet.postLayer, activeOnly);
        }
        //-----------------------------------------------------------
        public void PrintCurrentPrefabsForAllLayer()
        {
            Debug.Log($"\n     -----     Current Layer Prefabs:     ----- \n");
            PrintCurrentPrefabForLayer(LayerSet.railALayer);
            PrintCurrentPrefabForLayer(LayerSet.railBLayer);
            PrintCurrentPrefabForLayer(LayerSet.postLayer);
            PrintCurrentPrefabForLayer(LayerSet.subpostLayer);
            PrintCurrentPrefabForLayer(LayerSet.extraLayer);
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
                Debug.Log($"{colorStr}         Post = {GetMainPrefabForLayer(LayerSet.postLayer).name} \n</color>");


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
            if (layer == LayerSet.railALayer)
                return useRailSequencer[0];
            else if (layer == LayerSet.railBLayer)
                return useRailSequencer[1];
            else if (layer == LayerSet.postLayer)
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
            if (layer == LayerSet.railALayer)
                return variationModeRailA;
            else if (layer == LayerSet.railBLayer)
                return variationModeRailA;
            else if (layer == LayerSet.postLayer)
                return variationModePost;

            else return VariationMode.sequenced;
        }

        public bool GetAllowRandomizationForLayer(LayerSet layer)
        {
            bool allow = false;
            if (layer == LayerSet.railALayer)
                allow = allowRailRandomization[kRailALayerInt];
            else if (layer == LayerSet.railBLayer)
                allow = allowRailRandomization[kRailBLayerInt];
            else if (layer == LayerSet.postLayer)
                allow = allowPostRandomization;
            /*else if (layer == LayerSet.subpostLayer) // For v4.0
                allow = allowSubpostRandomization;
            else if (layer == LayerSet.extraLayer)
                allow = allowExtraRandomization;*/
            return allow;
        }
        public void ToggleRandomizationForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                allowRailRandomization[kRailALayerInt] = !allowRailRandomization[kRailALayerInt];
            else if (layer == LayerSet.railBLayer)
                allowRailRandomization[kRailBLayerInt] = !allowRailRandomization[kRailBLayerInt];
            else if (layer == LayerSet.postLayer)
                allowPostRandomization = !allowPostRandomization;
            /*else if (layer == LayerSet.subpostLayer) // For v4.0
                 allowSubpostRandomization = !allowSubpostRandomization;
            else if (layer == LayerSet.extraLayer)
                 allowExtraRandomization = !allowExtraRandomization;*/
        }
        public bool GetUseVariationsForLayer(LayerSet layer)
        {
            bool use = false;
            if (layer == LayerSet.railALayer)
                use = useRailVariations[0];
            else if (layer == LayerSet.railBLayer)
                use = useRailVariations[1];
            else if (layer == LayerSet.postLayer)
                use = usePostVariations;
            /*else if (layer == LayerSet.subpostLayer) 
                allow = allowSubpostRandomization;*/
            return use;
        }
        public void ToggleUseVariationsForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                useRailVariations[0] = !useRailVariations[0];
            else if (layer == LayerSet.railBLayer)
                useRailVariations[1] = !useRailVariations[1];
            else if (layer == LayerSet.postLayer)
                usePostVariations = !usePostVariations;

        }


        public void ToggleUseSinglesForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                useRailSingles[0] = !useRailSingles[0];
            else if (layer == LayerSet.railBLayer)
                useRailSingles[1] = !useRailSingles[1];
            else if (layer == LayerSet.postLayer)
                usePostSingles = !usePostSingles;
        }





    }
}
