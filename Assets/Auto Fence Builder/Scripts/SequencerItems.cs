using AFWB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;


namespace AFWB
{

    [System.Serializable]
    public class Sequencer
    {
        public const int kMaxNumSeqSteps = 20; //TODO
        public int numSteps = 2;
        LayerSet layer = LayerSet.noneLayerSet;
        public bool useSeq = false;

        public List<SeqItem> seqList = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);

        /// <summary>Private default constructor to prevent its use.</summary>
        private Sequencer() { }

        /// <summary>Initializes a new instance of the Sequencer class with a specified layer.</summary>
        /// <param name="layer">The layer set to initialize the sequencer with.</param>
        public Sequencer(LayerSet layer, int size = 2)
        {
            Init(layer, size);
        }

        public void Init(LayerSet layer, int size = 2)
        {
            this.layer = layer;
            numSteps = size;
            if (numSteps < 2)
                numSteps = 2; // For now, ensure there's always a minimum seq length of 2 steps.
            CreateNewSeqList(numSteps);
            useSeq = false;
        }

        public void SetUseSeq(bool use)
        {
            useSeq = use;
        }
        public bool GetUseSeq()
        {
            return useSeq;
        }
        public void ToggleUseSeq()
        {
             useSeq = !useSeq;
        }

        public SeqItem GetSeqItem(int index)
        {
            if (index < seqList.Count)
                return seqList[index];
            else
            {
                Debug.LogWarning("GetSeqItem: index out of range");
                return null;
            }
        }
        public void EnforceSeqListBounds()
        {
            EnforceSeqMinLength(2);
            if (seqList.Count > kMaxNumSeqSteps)
                seqList.RemoveRange(kMaxNumSeqSteps, seqList.Count - kMaxNumSeqSteps);
        }
        public void EnforceSeqMinLength(int newLength)
        {
            if (seqList.Count < newLength)
            {
                int numToAdd = newLength - seqList.Count;
                for (int i = 0; i < numToAdd; i++)
                {
                    seqList.Add(new SeqItem());
                }
            }   

        }
        public void AddSeqItems(int numToAdd)
        {
            if(seqList.Count + numToAdd > kMaxNumSeqSteps)
                numSteps = kMaxNumSeqSteps - seqList.Count;

            seqList.AddRange(Enumerable.Range(0, numToAdd).Select(_ => new SeqItem()));

        }

        public void RemoveLastNSeqItems(int numToRemove)
        {
            // Ensure there are at least 2 items remaining after removal
            int minItemsRemaining = 2;
            int itemsToRemove = Mathf.Min(numToRemove, seqList.Count - minItemsRemaining);
            if (itemsToRemove > 0)
            {
                seqList.RemoveRange(seqList.Count - itemsToRemove, itemsToRemove);
            }
        }


        //-- Static so it can be used elsewhere
        public static List<SeqItem> CreateNewSeqList(int numSteps)
        {
            List<SeqItem> seqList = new List<SeqItem>(numSteps);
            for (int i = 0; i < numSteps; i++)
            {
                seqList.Add(new SeqItem());
            }
            return seqList;
        }

        public int Length()
        {
            return seqList.Count;
        }

        public List<SeqItem> SeqList
        {
            get { return seqList; }
            set
            {
                if (seqList != value)
                {
                    //Debug.LogWarning($"seqList changed from {seqList} to {value}");
                    seqList = value;
                }
            }
        }

        /// <summary>Derives the correct sequence step number from the section index of the post, by using the modulo operation:
        ///  sectionIndex % numSteps</summary>
        /// <param name="sectionIndex">The index of the section of the post or rail.</param>
        /// <returns>The corresponding sequence step number.</returns>
        public int GetSeqStepNumFromSectionIndex(int sectionIndex)
        {
            if (numSteps <= 0) return 0; //-- Ensure there's at least one step to avoid division by zero
            return sectionIndex % numSteps;
        }
        //--------------------------------------------------------------------------------
        /// <summary>Gets the correct SeqItem based on the section index by using the modulo operation.</summary>
        /// <param name="sectionIndex">The index of the section of the post.</param>
        /// <returns>The corresponding SeqItem or null</returns>
        public SeqItem GetSeqItemForSectionIndex(int sectionIndex)
        {
            int seqStep = GetSeqStepNumFromSectionIndex(sectionIndex);
            return GetSeqItem(seqStep);
        }

        /// <summary>  Get the seqItem.size for a given section index, which in turn derives the correct seq step number </summary>
        /// <param name= "sectionIndex": the section index of the post or rail,Note: NOT the step number, which is first derived from it></param>
        /// <returns>Vector3 seqItem.size</returns>
        /// <remarks> Will return Vector3.one if sequencer isn't used, or any other reason to disqualify it</remarks>
        public Vector3  GetSeqScaleForSectionIndex(int sectionIndex)
        {
            Vector3 scaling = Vector3.one;
            SeqItem seqItem = GetSeqItemForSectionIndex(sectionIndex);
            if(seqItem != null)
                scaling = seqItem.size;
            else
                Debug.LogWarning($"GetSeqScaleForSectionIndex: {sectionIndex}.     seqItem is null\n");
            return scaling;
        }
        
        /// <summary>  Get the seqItem.pos offset for a given section index, which in turn derives the correct seq step number </summary>
        /// <param name= "sectionIndex": the section index of the post or rail,Note: NOT the step number, which is first derived from it></param>
        /// <returns>Vector3 seqItem.pos</returns>
        /// <remarks> Will return Vector3.zero if sequencer isn't used, or any other reason to disqualify it</remarks>
        public Vector3 GetSeqOffsetForSectionIndex(int sectionIndex)
        {
            Vector3 offset = Vector3.zero;
            SeqItem seqItem = GetSeqItemForSectionIndex(sectionIndex);
            if (seqItem != null)
                offset = seqItem.pos;
            else
                Debug.LogWarning($"GetSeqOffsetForSectionIndex: {sectionIndex}.     seqItem is null\n");
            return offset;
        }

        public Vector3 GetSeqRotationForSectionIndex(int sectionIndex)
        {
            Vector3 rotation = Vector3.zero;
            SeqItem seqItem = GetSeqItemForSectionIndex(sectionIndex);
            if (seqItem != null)
                rotation = seqItem.rot;
            else
                Debug.LogWarning($"GetSeqRotationForSectionIndex: {sectionIndex}.     seqItem is null\n");
            return rotation;
        }
        public bool GetSeqStepEnabledForSectionIndex(int sectionIndex)
        {
            bool enabled = true;
            SeqItem seqItem = GetSeqItemForSectionIndex(sectionIndex);
            if (seqItem != null)
                enabled = seqItem.stepEnabled;
            else
                Debug.LogWarning($"GetSeqEnabledForSectionIndex: {sectionIndex}.     seqItem is null\n");
            return enabled;
        }
    }

    //==========================================================================================================================
    //  SeqItems hold info about each step of a Sequence
    //  They store all modifiable parameters and a link - sourceVariantIndex - to the SourceVariant that defines the GameObject used for this step

    /// <summary>
    /// Settings for each Step in the Step Sequencer. (Each Layer has a List of these that define the sequence)
    /// <br>    - sourceVariantIndex: -  Index into the List of defined SourceVariants (between 1 and kMaxNumSourceVariants). Not a direct Prefab index.</br>
    /// <br>    - Also: pos / svSize / rot / svInvert / svBackToFront / svMirrorZ / stepEnabled / probability </br>
    /// </summary>
    /// //=========================================================================================================================
    [System.Serializable]
    public class SeqItem
    {
        [FormerlySerializedAs("objIndex")] //maintains compatibility with older presets where the was called objIndex
        public int sourceVariantIndex; // an index into the list of SourceVariants for this layer

        public int unavailableIndex = 0; // this is used when the user has set a variant for a step, e.g. i=5, but then reduced the number of variants to e.g. 3,

        //  so the variant at i=5 is no longer available. This is the variant index to be used when that happens. Normally = 0
        public Vector3 pos;

        public Vector3 size = Vector3.one;
        public Vector3 rot;
        public bool invert, backToFront, mirrorZ;
        public bool stepEnabled = true;
        public float probability;
        public LayerSet layer;

        public SeqItem()
        {
            Init();
        }

        public void Init()
        {
            invert = false; backToFront = false; mirrorZ = false;
            sourceVariantIndex = 0;
            pos = Vector3.zero; size = Vector3.one; rot = Vector3.zero;
            stepEnabled = true;
            probability = 1;
            //go = null;
            stepEnabled = true;
            unavailableIndex = 0;
            layer = LayerSet.noneLayerSet;
        }

        // Initialize and set to first object in SourceVariants List
        public void InitWithBaseVariant(List<SourceVariant> variantList)
        {
            Init();
            sourceVariantIndex = unavailableIndex = 0;
        }

        public SeqItem(int sourceVarIndex)
        {
            Init();
            sourceVariantIndex = sourceVarIndex;
        }

        public SeqItem(int sourceVarIndex, SourceVariant variant, LayerSet inLayer)
        {
            Init();
            sourceVariantIndex = sourceVarIndex;
            layer = inLayer;
            //Debug.LogWarning("SeqItem(int objectIndex, SourceVariant variant):  sourceVariant.Go is null");
            //pos = variant.svPositionOffset;
            //size = variant.svSize;
            //rot = variant.svRotation;
        }

        public SeqItem(GameObject inGO)
        {
            Init();
            //go = inGO;
            //Debug.LogWarning("No Object Index for GO");
        }

        public SeqItem(SeqItem inSeqItem)
        {
            stepEnabled = inSeqItem.stepEnabled;

            invert = inSeqItem.invert;
            backToFront = inSeqItem.backToFront;
            mirrorZ = inSeqItem.mirrorZ;

            pos = inSeqItem.pos;
            size = inSeqItem.size;
            rot = inSeqItem.rot;

            sourceVariantIndex = inSeqItem.sourceVariantIndex;
            //variantEnabled = inSeqItem.variantEnabled;
            probability = inSeqItem.probability;
            layer = inSeqItem.layer;
        }

        public SeqItem(bool inv, bool x, bool z, int index, Vector3 p, Vector3 s, Vector3 r, float prob, bool enable)
        {
            invert = inv; backToFront = x; mirrorZ = z;
            sourceVariantIndex = index;
            pos = p; size = s; rot = r;
            //variantEnabled = enable;
            probability = prob;
            //go = inGO;
            if (size.x == 0)
                UnityEngine.Debug.Log("SeqItem svSize 0");
        }

        public int GetSourceVariantIndex(AutoFenceCreator af, LayerSet layer)
        {
            List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
            if (sourceVariantIndex >= sourceVariants.Count)
                Debug.LogWarning($"GetSourceVariantIndex: sourceVariantIndex [{sourceVariantIndex}] was invalid");
            return sourceVariantIndex;
        }

        public SourceVariant GetSourceVariant(AutoFenceCreator af, LayerSet layer)
        {
            List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
            if (sourceVariantIndex < sourceVariants.Count)
            {
                SourceVariant sv = sourceVariants[sourceVariantIndex];
                if (sv != null)
                    return sv; //already warned above;
                else
                    Debug.LogWarning($"GetSourceVariant: sourceVariantIndex [{sourceVariantIndex}] was null");
            }
            else
                Debug.LogWarning($"GetSourceVariant: sourceVariantIndex [{sourceVariantIndex}] out of range");

            return null;
        }

        public GameObject GetSourceVariantGO(AutoFenceCreator af, LayerSet layer)
        {
            List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
            if (sourceVariantIndex < sourceVariants.Count)
            {
                SourceVariant sv = sourceVariants[sourceVariantIndex];
                GameObject go = sv.Go;
                if (go != null)
                    return go; //already warned above;
                else
                    Debug.LogWarning($"GetSourceVariantGO: sourceVariantIndex [{sourceVariantIndex}] has null GameObject");
            }
            else
                Debug.LogWarning($"GetLinkedSourceVariant: sourceVariantIndex [{sourceVariantIndex}] out of range");

            return null;
        }

        //==================================================
        //     SeqItem Static Methods
        //==================================================

        public static SeqItem[] CreateSeqItemArray(int size)
        {
            SeqItem[] array = new SeqItem[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = new SeqItem();
            }
            return array;
        }



        /// <summary>
        /// Returns a list of values of the specified field type (fieldName) from a list of sequence variants.
        /// <br>Throws an ArgumentException if the specified field name is not found in the SeqItem class.</br>
        /// </summary>
        /// <typeparam name="T">The type of the field to retrieve.</typeparam>
        /// <param name="SeqItemList">The list of sequence variants to retrieve the field values from.</param>
        /// <param name="fieldName">The name of the field to retrieve.</param>
        /// <returns>A list of values from the specified field.</returns>
        public static List<T> GetFieldListByName<T>(List<SeqItem> SeqItemList, string fieldName)
        {
            FieldInfo fieldInfo = typeof(SeqItem).GetField(fieldName);
            if (fieldInfo == null)
            {
                throw new ArgumentException("Field '" + fieldName + "' not found in SeqItem class.");
            }
            Type type = fieldInfo.FieldType;
            return SeqItemList.Select(x => (T)fieldInfo.GetValue(x)).ToList();
        }

        public static List<object> GetFieldValuesByName(List<SeqItem> SeqItems, string fieldName)
        {
            List<object> extractedValues = new List<object>();

            foreach (SeqItem variant in SeqItems)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(SeqItem).GetField(fieldName);

                if (fieldInfo != null)
                {
                    object value = fieldInfo.GetValue(variant);
                    extractedValues.Add(value);
                }
                else
                {
                    Debug.LogError($"Field '{fieldName}' not found in SeqItem class.");
                    return null;
                }
            }

            return extractedValues;
        }

        public static void PrintSeqFieldListByName(List<SeqItem> SeqItemList, string fieldName, LayerSet layer, AutoFenceCreator af, bool onlyShowStepsInUse = true)
        {
            List<object> fieldList = GetFieldValuesByName(SeqItemList, fieldName);
            int num = fieldList.Count;
            if (onlyShowStepsInUse)
            {
                num = af.GetNumSeqStepsForLayer(layer);
                //num = fieldList.Count;
            }
            // this is a special case because we can't get a go's name with [go].ToString()
            if (fieldName == "go")
            {
                // cast fieldList to List<GameObject>
                List<GameObject> goList = fieldList.Cast<GameObject>().ToList();
                for (int i = 0; i < num; i++)
                {
                    string goName = "Null GO";
                    if (goList[i] != null)
                        goName = goList[i].name;

                    Debug.Log("Step " + i.ToString() + " " + goName + ": " + fieldList[i] + "\n");
                }
                return;
            }

            for (int i = 0; i < num; i++)
            {
                Debug.Log(i.ToString() + " " + fieldName + ": " + fieldList[i] + "\n");
            }
        }

        //-----------------------
        public static List<T> GetPropertyListByLambda<T>(List<SeqItem> SeqItemList, Func<SeqItem, T> propertySelector)
        {
            return SeqItemList.Select(propertySelector).ToList();
        }

        public static void TimeGetPropertyListMethods()
        {
            // Create a list of 10,000 SeqItem objects
            List<SeqItem> SeqItemList = new List<SeqItem>();
            for (int i = 0; i < 1000000; i++)
            {
                SeqItemList.Add(new SeqItem { sourceVariantIndex = i });
            }

            // Time the call to GetFieldListByName
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            List<int> goIndexListByName = GetFieldListByName<int>(SeqItemList, "sourceVariantIndex");
            stopwatch.Stop();
            double milliseconds = stopwatch.ElapsedTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            Debug.Log("GetFieldListByName: " + milliseconds + "ms");

            // Time the call to GetPropertyListByLambda
            stopwatch.Restart();
            List<int> goIndexListByLambda = GetPropertyListByLambda(SeqItemList, x => x.sourceVariantIndex);
            stopwatch.Stop();
            milliseconds = stopwatch.ElapsedTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            Debug.Log("GetPropertyListByLambda: " + milliseconds + "ms");
        }

        //------------------------------------------------
        public static int[] GetShuffledIndexArray(int length, int loopLength, LayerSet layer)
        {
            int[] shuffledIndexList = new int[length];

            RandomLookupAFWB rTable = RandomLookupAFWB.randForRailA;
            int rIndex = RandomLookupAFWB.railARandLookupIndex;
            if (layer == LayerSet.railBLayerSet)
            {
                rTable = RandomLookupAFWB.randForRailA;
                rIndex = RandomLookupAFWB.railARandLookupIndex;
            }
            else if (layer == LayerSet.postLayerSet)
            {
                rTable = RandomLookupAFWB.randForPost;
                rIndex = RandomLookupAFWB.postRandLookupIndex;
            }
            if (rTable == null)
            {
                Debug.LogWarning("Missing Table for " + layer);
                return shuffledIndexList;
            }

            int loopIndex = 0;
            for (int i = 0; i < length; i++)
            {
                shuffledIndexList[i] = loopIndex++;
                if (loopIndex == loopLength)
                    loopIndex = 0;
            }
            // Now shuffle them
            for (int i = 0; i < length; i++)
            {
                //int j = UnityEngine.Random.Range(i, length); //numFenceSections is exclusive because  Random.Range(int, int)

                int j = rTable.RandomRange(i, length);
                rIndex++;

                int t = shuffledIndexList[j];
                shuffledIndexList[j] = shuffledIndexList[i];
                shuffledIndexList[i] = t;
            }
            return shuffledIndexList;
        }

        //------------------------------------------------
        // Creates a list of sourceVariants with the correct proportion of each type, based on their probabiity.
        public static int[] CreateShuffledIndices(List<SourceVariant> variants, int numFenceSections)
        {
            float totalProb = 0;
            int numVariants = variants.Count;
            int indexCount = 0;
            int[] shuffledIndexList = new int[numFenceSections];

            for (int i = 0; i < variants.Count; i++)
            {
                totalProb += variants[i].probability;
            }
            // make a list with the correct proportion
            for (int i = 0; i < numVariants; i++)
            {
                if (indexCount == numFenceSections)
                    break;
                float proportion = variants[i].probability / totalProb;
                int numOfThisType = (int)((proportion * numFenceSections) + 0.5f);
                for (int j = 0; j < numOfThisType; j++)
                {
                    shuffledIndexList[indexCount++] = i;
                    if (indexCount == numFenceSections)
                        break;
                }
            }
            // Now shuffle them
            for (int i = 0; i < numFenceSections; i++)
            {
                int j = UnityEngine.Random.Range(i, numFenceSections); //numFenceSections is exclusive because  Random.Range(int, int)
                int t = shuffledIndexList[j];
                shuffledIndexList[j] = shuffledIndexList[i];
                shuffledIndexList[i] = t;
            }
            return shuffledIndexList;
        }

        //------------------------------------------------
        public static void ShuffleObjectIndicesInSequence2(List<SeqItem> sequence, int numSteps, List<SourceVariant> variantsList, bool shuffleStepSettingsAlso,
                                                            LayerSet layer)
        {
            // create an array of int of size
        }

        //------------------------------------------------
        public static void ShuffleObjectIndicesInSequence(List<SeqItem> sequence, int numSteps, List<SourceVariant> variantsList, bool shuffleStepSettingsAlso,
                                                        LayerSet layer)
        {
            //AssignAllDifferentObjectIndicesInSequence(SeqItemList, numSteps, variantsList);

            int numVariants = variantsList.Count;
            int seqLength = sequence.Count;

            List<SeqItem> cloneSourceList = new List<SeqItem>(sequence);

            for (int i = 0; i < seqLength; i++)
            {
                cloneSourceList[i] = new SeqItem(sequence[i]);
            }

            //-- First do the entire list, so that if the user increases numSteps, they are still good entries
            int[] shuffledVariantIndexList = GetShuffledIndexArray(seqLength, numVariants, layer);
            int[] shuffledStepIndexList = GetShuffledIndexArray(seqLength, numSteps, layer);
            for (int i = 0; i < seqLength; i++)
            {
                int shuffledStepIndex = i;
                if (shuffleStepSettingsAlso == true)
                    shuffledStepIndex = shuffledStepIndexList[i];

                int shuffledVariantIndex = shuffledVariantIndexList[i];

                sequence[i] = cloneSourceList[shuffledStepIndex];
                sequence[i].sourceVariantIndex = shuffledVariantIndex;
            }

            //-- Now do it just for the numSteps, if it's small, the above list might be clumped and not have a fair mix
            //-- for the first numSteps
            int[] shuffledVariantIndexListNumStepsOnly = GetShuffledIndexArray(numSteps, numVariants, layer);
            int[] shuffledStepIndexListNumStepsOnly = GetShuffledIndexArray(numSteps, numSteps, layer);
            for (int i = 0; i < numSteps; i++)
            {
                int stepIndex = i;
                if (shuffleStepSettingsAlso == true)
                    stepIndex = shuffledStepIndexListNumStepsOnly[i];

                int variantIndex = shuffledVariantIndexListNumStepsOnly[i];
                sequence[i] = cloneSourceList[stepIndex];
                sequence[i].sourceVariantIndex = variantIndex;
            }
        }

        //------------------------------------------------
        public static void AssignAllDifferentObjectIndicesInSequence(AutoFenceCreator af, LayerSet layer, List<SeqItem> SeqItemList)
        {
            List<SourceVariant> sourceVariantsList = af.GetSourceVariantsForLayer(layer);
            int numVariantsInUseIncMain = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true);
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                int idx = i;
                if (i >= numVariantsInUseIncMain)
                {
                    idx = i % numVariantsInUseIncMain;
                }
                SeqItemList[i].sourceVariantIndex = idx;

                //Debug.Log("SeqItemList[" + i + "].sourceVariantIndex = " + SeqItemList[i].sourceVariantIndex + "\n");
                //SeqItemList[i].go = sourceVariantsList[SeqItemList[i].sourceVariantIndex].go;
            }
        }

        public static List<GameObject> GetGOsFromSeqItemList(AutoFenceCreator af, List<SeqItem> SeqItems, LayerSet layer)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            foreach (SeqItem variant in SeqItems)
            {
                GameObject go = variant.GetSourceVariantGO(af, layer);
                gameObjectList.Add(go);
            }

            return gameObjectList;
        }

        public static List<string> GetGoNamesFromSeqItemList(AutoFenceCreator af, List<SeqItem> SeqItems, LayerSet layer, bool strip = true)
        {
            List<string> goNameList = new List<string>();
            string goName = "";
            foreach (SeqItem SeqItem in SeqItems)
            {
                GameObject go = SeqItem.GetSourceVariantGO(af, layer);
                if (go != null)
                {
                    goName = go.name;
                    if (strip)
                        goName = AutoFenceCreator.StripLayerTypeFromNameStatic(goName);
                    goNameList.Add(goName);
                }
                else
                {
                    goNameList.Add($"null {af.GetLayerNameAsString(layer)} GO  (variant.sourceVariantIndex = {SeqItem.sourceVariantIndex}");
                }
            }
            return goNameList;
        }

        //--------------------------------------------------
        // CreateMergedPrefabs Optimally different series of orientations, then repeat once more, slightly different
        // all sequences are initialized to false, some are restated for clarity
        public static List<SeqItem> CreateOptimalSequence(AutoFenceCreator af, List<SourceVariant> objects, LayerSet layer)
        {
            bool backToFront = af.allowBackToFrontRailA;
            bool mirrorZ = af.allowMirrorZRailA;
            bool invertZ = af.allowInvertRailA;

            if (layer == LayerSet.railBLayerSet)
            {
                backToFront = af.allowBackToFrontRailB;
                mirrorZ = af.allowMirrorZRailB;
                invertZ = af.allowInvertRailB;
            }

            List<SeqItem> seq = new List<SeqItem>();

            //--------------------------
            int numObjects = objects.Count; ;
            int variantIndex;
            int numVariants;
            // No Orientation Changes
            if (backToFront == false && mirrorZ == false && invertZ == false)
            {
                numVariants = numObjects;
                SeqItem[] x = new SeqItem[numVariants];
                for (int p = 0; p < numVariants; p++)
                {
                    variantIndex = p % numObjects;
                    x[p] = new SeqItem(variantIndex, objects[variantIndex], layer); // initialize with objectIndex
                    seq.Add(x[p]); //Set the object index to cycle around the number available
                }
            }
            int numOrients;
            // svBackToFront only
            if (backToFront == true && mirrorZ == false && invertZ == false)
            {
                numOrients = 2;
                if (numObjects == 1)
                {
                    numVariants = numOrients * numObjects;
                    SeqItem[] x = new SeqItem[numVariants];
                    for (int p = 0; p < numVariants; p++)
                    {
                        variantIndex = p % numObjects;
                        x[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    x[0].backToFront = false;
                    x[1].backToFront = true;

                    seq = new List<SeqItem>(x);
                }
                if (numObjects > 1)
                {
                    numVariants = numOrients * numObjects * 2;
                    SeqItem[] x = new SeqItem[numVariants];
                    for (int p = 0; p < numVariants; p++)
                    {
                        variantIndex = p % numObjects;
                        x[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    x[0].backToFront = false;
                    x[1].backToFront = true;
                    x[2].backToFront = true;
                    x[3].backToFront = false;

                    x[4].backToFront = true;
                    x[5].backToFront = false;
                    x[6].backToFront = false;
                    x[7].backToFront = true;
                    seq = new List<SeqItem>(x);
                }
            }
            // svMirrorZ only
            if (backToFront == false && mirrorZ == true && invertZ == false)
            {
                numOrients = 2;
                if (numObjects >= 1)
                {
                    numVariants = numOrients * numObjects;
                    SeqItem[] z = new SeqItem[numVariants];
                    for (int p = 0; p < numVariants; p++)
                    {
                        variantIndex = p % numObjects;
                        z[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    z[0].mirrorZ = false;
                    z[1].mirrorZ = true;
                    seq = new List<SeqItem>(z);
                }
                if (numObjects > 1)
                {
                    numVariants = numOrients * numObjects * 2;
                    SeqItem[] z = new SeqItem[numVariants];
                    for (int p = 0; p < numVariants; p++)
                    {
                        variantIndex = p % numObjects;
                        z[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    z[0].mirrorZ = false;
                    z[1].mirrorZ = true;
                    z[2].mirrorZ = true;
                    z[3].mirrorZ = false;

                    z[4].mirrorZ = true;
                    z[5].mirrorZ = false;
                    z[6].mirrorZ = false;
                    z[7].mirrorZ = true;
                    seq = new List<SeqItem>(z);
                }
            }
            // invertZ
            if (backToFront == false && mirrorZ == false && invertZ == true)
            {
                numOrients = 2;
                if (numObjects == 1)
                {
                    numVariants = numOrients * numObjects;
                    SeqItem[] invert_z = new SeqItem[numVariants];
                    for (int p = 0; p < numVariants; p++)
                    {
                        variantIndex = p % numObjects;
                        invert_z[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    invert_z[0].invert = false;
                    invert_z[1].invert = true;
                    seq = new List<SeqItem>(invert_z);
                }
                if (numObjects > 1)
                {
                    numVariants = numOrients * numObjects * 2;
                    SeqItem[] invert_z = new SeqItem[numVariants];
                    for (int p = 0; p < numVariants; p++)
                    {
                        variantIndex = p % numObjects;
                        invert_z[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    invert_z[0].invert = false;
                    invert_z[1].invert = true;
                    invert_z[2].invert = true;
                    invert_z[3].invert = false;

                    invert_z[4].invert = false;
                    invert_z[5].invert = true;
                    invert_z[6].invert = true;
                    invert_z[7].invert = false;
                    seq = new List<SeqItem>(invert_z);
                }
            }

            int numVariantsDoubled;
            // svBackToFront svMirrorZ
            if (backToFront == true && mirrorZ == true && invertZ == false)
            {
                numOrients = 4;

                if (numObjects >= 1)
                {
                    numVariants = numOrients * numObjects;
                    numVariantsDoubled = numVariants * 2;
                    SeqItem[] x_z = new SeqItem[numVariantsDoubled];
                    for (int p = 0; p < numVariantsDoubled; p++)
                    {
                        variantIndex = p % numObjects;
                        x_z[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    x_z[0].backToFront = false;
                    x_z[0].mirrorZ = false;

                    x_z[1].backToFront = true;

                    x_z[2].mirrorZ = true;

                    x_z[3].backToFront = true;
                    x_z[3].mirrorZ = true;
                    //----------------------
                    x_z[4].backToFront = true;

                    x_z[5].backToFront = false;
                    x_z[5].mirrorZ = false;

                    x_z[6].backToFront = true;
                    x_z[6].mirrorZ = true;

                    x_z[7].mirrorZ = true;

                    // Repeat Backwards
                    for (int i = numVariants; i < numVariantsDoubled; i++)
                    {
                        int indexToCopy = i % numVariants;
                        //indexToCopy /= 2;

                        indexToCopy = ((numVariants - 1) - indexToCopy) + 1;
                        if (indexToCopy == numVariantsDoubled)
                            indexToCopy = numVariants - 1;
                        x_z[i] = x_z[indexToCopy];
                    }

                    seq = new List<SeqItem>(x_z);
                }
            }
            // svBackToFront invertX
            if (backToFront == true && mirrorZ == false && invertZ == true)
            {
                numOrients = 4;

                if (numObjects >= 1)
                {
                    numVariants = numOrients * numObjects;
                    numVariantsDoubled = numVariants * 2;
                    SeqItem[] x_invert = new SeqItem[numVariantsDoubled];
                    for (int p = 0; p < numVariantsDoubled; p++)
                    {
                        variantIndex = p % numObjects;
                        x_invert[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    x_invert[0].backToFront = false;
                    x_invert[0].invert = false;

                    x_invert[1].backToFront = true;

                    x_invert[2].invert = true;

                    x_invert[3].backToFront = true;
                    x_invert[3].invert = true;
                    //----------------------
                    x_invert[4].backToFront = true;

                    x_invert[5].backToFront = false;
                    x_invert[5].invert = false;

                    x_invert[6].backToFront = true;
                    x_invert[6].invert = true;

                    x_invert[7].invert = true;

                    // Repeat Backwards
                    for (int i = numVariants; i < numVariantsDoubled; i++)
                    {
                        int indexToCopy = i % numVariants;
                        //indexToCopy /= 2;

                        indexToCopy = ((numVariants - 1) - indexToCopy) + 1;
                        if (indexToCopy == numVariantsDoubled)
                            indexToCopy = numVariants - 1;
                        x_invert[i] = x_invert[indexToCopy];
                    }

                    seq = new List<SeqItem>(x_invert);
                }
            }
            // svMirrorZ invertX
            if (backToFront == false && mirrorZ == true && invertZ == true)
            {
                numOrients = 4;

                if (numObjects >= 1)
                {
                    numVariants = numOrients * numObjects;
                    numVariantsDoubled = numVariants * 2;
                    SeqItem[] z_invert = new SeqItem[numVariantsDoubled];
                    for (int p = 0; p < numVariantsDoubled; p++)
                    {
                        variantIndex = p % numObjects;
                        z_invert[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    z_invert[0].mirrorZ = false;
                    z_invert[0].invert = false;

                    z_invert[1].mirrorZ = true;

                    z_invert[2].invert = true;

                    z_invert[3].mirrorZ = true;
                    z_invert[3].invert = true;
                    //----------------------
                    z_invert[4].mirrorZ = true;

                    z_invert[5].mirrorZ = false;
                    z_invert[5].invert = false;

                    z_invert[6].mirrorZ = true;
                    z_invert[6].invert = true;

                    z_invert[7].invert = true;

                    // Repeat Backwards
                    for (int i = numVariants; i < numVariantsDoubled; i++)
                    {
                        int indexToCopy = i % numVariants;
                        indexToCopy = ((numVariants - 1) - indexToCopy) + 1;
                        if (indexToCopy == numVariantsDoubled)
                            indexToCopy = numVariants - 1;
                        z_invert[i] = z_invert[indexToCopy];
                    }

                    seq = new List<SeqItem>(z_invert);
                }
            }

            // svBackToFront  svMirrorZ  invertZ
            if (backToFront == true && mirrorZ == true && invertZ == true)
            {
                numOrients = 8;

                if (numObjects >= 1)
                {
                    numVariants = numOrients * numObjects;
                    numVariantsDoubled = numVariants * 2;
                    SeqItem[] invert_x_z = new SeqItem[numVariantsDoubled];
                    for (int p = 0; p < numVariantsDoubled; p++)
                    {
                        variantIndex = p % numObjects;
                        invert_x_z[p] = new SeqItem(variantIndex, objects[variantIndex], layer);
                    }
                    invert_x_z[1].backToFront = true;

                    invert_x_z[2].mirrorZ = true;

                    invert_x_z[3].invert = true;

                    invert_x_z[4].invert = true;
                    invert_x_z[4].backToFront = true;

                    invert_x_z[5].backToFront = true;
                    invert_x_z[5].mirrorZ = true;

                    invert_x_z[6].invert = true;
                    invert_x_z[6].mirrorZ = true;

                    invert_x_z[7].invert = true;
                    invert_x_z[7].mirrorZ = true;
                    invert_x_z[7].backToFront = true;

                    invert_x_z[8].invert = true;
                    invert_x_z[9].mirrorZ = true;
                    invert_x_z[10].backToFront = true;

                    invert_x_z[11].invert = true;
                    invert_x_z[11].backToFront = true;

                    invert_x_z[12].backToFront = false;
                    invert_x_z[12].mirrorZ = false;
                    invert_x_z[12].mirrorZ = false;

                    invert_x_z[13].backToFront = true;
                    invert_x_z[13].mirrorZ = true;

                    invert_x_z[14].invert = true;
                    invert_x_z[14].mirrorZ = true;
                    invert_x_z[14].backToFront = true;

                    invert_x_z[15].invert = true;
                    invert_x_z[15].mirrorZ = true;

                    // Repeat Backwards
                    for (int i = numVariants; i < numVariantsDoubled; i++)
                    {
                        int indexToCopy = i % numVariants;
                        indexToCopy = ((numVariants - 1) - indexToCopy) + 1;
                        if (indexToCopy == numVariantsDoubled)
                            indexToCopy = numVariants - 1;
                        invert_x_z[i] = invert_x_z[indexToCopy];
                    }

                    seq = new List<SeqItem>(invert_x_z);
                }
            }
            //Debug.Log("numObjects = " + numObjects + "        numOrients = " + numOrients + "        numSourceVariants = " + numSourceVariants);
            return seq;
        }

        //------------

        public static void PrintSeqVariationList(List<SeqItem> SeqItemListRailA, string prefix = "", bool nameOnly = true)
        {
            Debug.Log("\n_____  Sequencer Variations  _____\n");
            for (int i = 0; i < SeqItemListRailA.Count; i++)
            {
                SeqItem seqVar = SeqItemListRailA[i];
                PrintSeqVariation(seqVar, i.ToString(), nameOnly);
            }
        }

        private static void PrintSeqVariation(SeqItem seqVar, string prefix = "", bool nameOnly = true)
        {
            /*string nameStr = "null go";
            if (seqVar.go != null)
                nameStr = seqVar.go.name;

            if (prefix != "")
            {
                if (seqVar.go != null)
                    Debug.Log("------  Seq " + prefix + "  ------\n" + nameStr + "\n");
                else
                    Debug.Log("------  Seq " + prefix + ":   Null GO   ------\n");
            }
            if (nameOnly)
                return;

            //Debug.Log(seqVar.go.name + "\n");
            Debug.Log("GO index = " + seqVar.sourceVariantIndex + "\n");
            Debug.Log("svBackToFront = " + seqVar.backToFront + "     svInvert = " + seqVar.invert + "     svMirrorZ = " + seqVar.mirrorZ + "\n");
            Debug.Log("pos = " + seqVar.pos + "\n");
            Debug.Log("svSize = " + seqVar.size + "\n");
            Debug.Log("stepEnabled = " + seqVar.stepEnabled + "\n");
            Debug.Log("probability = " + seqVar.probability + "\n");*/
        }
    }
}