/* Auto Fence & Wall Builder v3.5 twoclicktools@gmail.com May 2023*/
#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0414 // same for private fields

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace AFWB
{
    /// <summary>Class to manage random seeds for different elements in Auto Fence Builder.
    /// These are stored in the presets so the randomization is repeatable
    /// </summary>
    [System.Serializable]
    public class RandomSeededValuesAF
    {
        public AutoFenceCreator af = null;


        /// <summary>Seed for height</summary>
        public int heightSeed;
        /// <summary>Random values for height which will be generated from: heightSeed </summary>
        //[System.NonSerialized]
        public List<float> rHeight = new List<float>();

        /// <summary>Seed for small rotation</summary>
        public int smallRotSeed;
        /// <summary>Random values for small rotation</summary>
        [SerializeField]
        public List<Vector3> rSmallRot = new List<Vector3>();

        /// <summary>Seed for quantized rotation</summary>
        public int quantRotSeed;
        /// <summary>Random values for quantized rotation</summary>
        //[System.NonSerialized]
        public List<float> rQuantRot = new List<float>();

        /// <summary>Seed for chance of missing elements</summary>
        public int chanceMissingSeed;
        /// <summary>Random values for chance of missing elementswhich will be generated from: chanceMissingSeed</summary>
        //[System.NonSerialized]
        public List<float> rChanceMissing = new List<float>();


        //========    Posts Exclusive    ============

        /// <summary>Seed for global spacing</summary>
        public int globalSpacingSeed;

        /// <summary>Seed for size X and Z</summary>
        public int postSizeXZSeed;
        /// <summary>Random values for Post size X</summary>
        public List<float> rPostSizeX = new List<float>();
        public List<float> rPostSizeZ = new List<float>();

        /// <summary>Seed for shift X and Z</summary>
        public int postShiftXZSeed;
        /// <summary>Random values for shift X</summary>
        public List<float> rPostShiftX = new List<float>();
        public List<float> rPostShiftZ = new List<float>();


        //========    Extras Exclusive    ============

        /// <summary>Seed for extra variable index</summary>
        public int extraVarIndexSeed;
        /// <summary>Values for extra variable index which will be generated from: extraVarIndexSeed</summary>
        public List<int> rExtraVarIndex = new List<int>();

        /// <summary>Seed for extra random value</summary>
        //public int extraRandomSeed;
        /// <summary>Random values for extra randomwhich will be generated from: extraRandomSeed </summary>
        //public List<float> rExtraRandom = new List<float>();

        public int scatterPosSeed, scatterScaleSeed, scatterRotSeed;
        public List<Vector3> rScatterPos = new List<Vector3>();
        public List<Vector3> rScatterScale = new List<Vector3>();
        public List<Vector3> rScatterRot = new List<Vector3>();




        /// <summary>Layer set associated with this seed</summary>
        public LayerSet layer;
        /// <summary>Unused variable, placeholder</summary>
        public int unused = 1;


        //==========  Constructor    ==============

        /// <summary>Initializes the random seeds using an input value.</summary>
        /// <param name="layerSet">Layer set associated with the seeds</param>
        /// <param name="autoFence">Reference to the AutoFenceCreator</param>
        /// <param name="allSeeds">Seed value for all parameters</param>
        public RandomSeededValuesAF(LayerSet layerSet, AutoFenceCreator af, int allSeeds = 12345)
        {
            globalSpacingSeed = heightSeed = smallRotSeed = quantRotSeed = chanceMissingSeed = allSeeds;
            postShiftXZSeed = postSizeXZSeed = allSeeds;
            extraVarIndexSeed = allSeeds;
            scatterPosSeed = scatterScaleSeed = scatterRotSeed = allSeeds;

            layer = layerSet;
            InitLists();
            this.af = af;
        }
        void InitLists()
        {
            //rExtraRandom = new List<float>();
            rHeight = new List<float>();
            rChanceMissing = new List<float>();
            rQuantRot = new List<float>();
            rSmallRot = new List<Vector3>();

            rPostSizeX = new List<float>();
            rPostSizeZ = new List<float>();
            rPostShiftX = new List<float>();
            rPostShiftZ = new List<float>();

            rExtraVarIndex = new List<int>();
            rScatterPos = new List<Vector3>();
            rScatterScale = new List<Vector3>();
            rScatterRot = new List<Vector3>();
        }
        //---------------------------------------
        /// <summary>Checks if the seed lists are populated and generates them if necessary.</summary>
        /// <param name="af">Reference to the AutoFenceCreator</param>
        public void CheckSeedValues([CallerMemberName] string caller = null)
        {
            if (af == null)
            {
                Debug.LogWarning($"AutoFenceCreator was null in CheckSeedValues()    Caller = {caller}\n");
                return;
            }

            int count = af.GetPoolForLayer(layer).Count;
            //-- If the pool is empty, add the default pool size number of items anyway to avoid more null checks.
            count = count < 1 ? af.GetDefaultPoolSizeForLayer(layer) : count;

            bool notEnoughSeeds = false;

            if (rHeight == null || rHeight.Count < count)
                GenerateRandomHeightValues();
            if (rSmallRot == null || rSmallRot.Count < count)
                GenerateRandomSmallRotValues();
            if (rQuantRot == null || rQuantRot.Count < count)
                GenerateRandomQuantRotValues();
            if (rChanceMissing == null || rChanceMissing.Count < count)
                GenerateRandomChanceMissingValues();

            if (layer == LayerSet.postLayer)
            {
                if (rPostSizeX == null || rPostSizeX.Count < count || rPostSizeZ == null || rPostSizeZ.Count < count)
                    GenerateRandomPostSizeXZValues();

                if (rPostShiftX == null || rPostShiftX.Count < count || rPostShiftZ == null || rPostShiftZ.Count < count)
                    GenerateRandomPostShiftXZ();
            }

            if (layer == LayerSet.extraLayer)
            {
                if (rExtraVarIndex == null || rExtraVarIndex.Count == 0)
                    GenerateRandomExtraVarIndex();
                if (rScatterPos == null || rScatterPos.Count == 0)
                    GenerateRandomExtraScatterPos();

            }
        }
        public void ReseedScatterPos()
        {
            scatterPosSeed = (int)System.DateTime.Now.Ticks;
        }
        public void GenerateRandomExtraScatterPos(int inputSeed = 0)
        {
            if (layer != LayerSet.extraLayer)
                return;

            int seed = (inputSeed != 0) ? inputSeed : scatterPosSeed;
            int count = af.GetPoolForLayer(layer).Count;

            UnityEngine.Random.InitState(scatterPosSeed);
            rScatterPos = new List<Vector3>();
            //-- These are bthe values in the transform box of 'Scatter 'Randomization'

            Vector3 randPosRange = af.ex.scatterExtraRandPosRange;

            for (int i = 0; i < count; i++)
            {
                float rX = UnityEngine.Random.Range(-randPosRange.x, randPosRange.x) * af.ex.scatterRandomStrength;
                float rY = UnityEngine.Random.Range(-randPosRange.y, randPosRange.y) * af.ex.scatterRandomStrength;
                float rZ = UnityEngine.Random.Range(-randPosRange.z, randPosRange.z) * af.ex.scatterRandomStrength;

                rScatterPos.Add(new Vector3(rX, rY, rZ));
            }
        }
        //-------------
        public void ReseedScatterScale()
        {
            scatterScaleSeed = (int)System.DateTime.Now.Ticks;
        }
        public void GenerateRandomExtraScatterScale(Vector3 min, Vector3 max, int inputSeed = 0)
        {
            if (layer != LayerSet.extraLayer)
                return;

            int seed = (inputSeed != 0) ? inputSeed : scatterScaleSeed;
            int count = af.GetPoolForLayer(layer).Count;

            UnityEngine.Random.InitState(scatterScaleSeed);
            rScatterScale = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                float rxScale = UnityEngine.Random.Range(min.x, max.x);
                float ryScale = UnityEngine.Random.Range(min.y, max.y);
                float rzScale = UnityEngine.Random.Range(min.z, max.z);

                Vector3 randScale = new Vector3(rxScale, ryScale, rzScale);

                rScatterScale.Add(randScale);
            }
        }
        //--------------
        public void ReseedScatterRot()
        {
            scatterRotSeed = (int)System.DateTime.Now.Ticks;
        }
        public void GenerateRandomExtraScatterRotation(int inputSeed = 0)
        {
            if (layer != LayerSet.extraLayer)
                return;

            int seed = (inputSeed != 0) ? inputSeed : scatterRotSeed;
            int count = af.GetPoolForLayer(layer).Count;

            //-- These are bthe values in the transform box of 'Scatter 'Randomization'
            Vector3 randRotRange = af.ex.scatterExtraRandRotRange;

            UnityEngine.Random.InitState(scatterRotSeed);
            rScatterRot = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {

                float rxRot = UnityEngine.Random.Range(-randRotRange.x, randRotRange.x);
                float ryRot = UnityEngine.Random.Range(-randRotRange.y, randRotRange.y);
                float rzRot = UnityEngine.Random.Range(-randRotRange.z, randRotRange.z);

                Vector3 randRot = new Vector3(rxRot, ryRot, rzRot);

                rScatterRot.Add(randRot);
            }
        }


        public void GenerateRandomExtraVarIndex(int inputSeed = 0)
        {
            if (layer != LayerSet.extraLayer)
                return;

            int seed = (inputSeed != 0) ? inputSeed : extraVarIndexSeed;
            int count = af.GetPoolForLayer(layer).Count;

            UnityEngine.Random.InitState(extraVarIndexSeed);
            rExtraVarIndex = new List<int>();
            for (int i = 0; i < count; i++)
            {
                rExtraVarIndex.Add(UnityEngine.Random.Range(1, 3));
            }
        }




        //---------------------------------------
        /// <summary>Seeds the height with the current time ticks.</summary>
        public void ReseedHeight()
        {
            heightSeed = (int)System.DateTime.Now.Ticks;
            GenerateRandomHeightValues();

        }
        /// <summary>
        /// Generates random height values for the current layer.
        /// It's possible these lists could be null if we're using an old pre-seeds Preset
        /// </summary>
        /// <param name="inputSeed"></param>
        public void GenerateRandomHeightValues(int inputSeed = 0)
        {
            if (rHeight == null)
                rHeight = new List<float>();

            int seed = (inputSeed != 0) ? inputSeed : heightSeed;
            int count = af.GetPoolForLayer(layer).Count;
            UnityEngine.Random.InitState(seed);

            rHeight.Clear();
            var (min, max) = GetRandomHeightRangeForLayer(layer);
            for (int i = 0; i < count; i++)
            {
                rHeight.Add(UnityEngine.Random.Range(min, max));
                //if (i < 4 && layer == LayerSet.postLayer)
                //Debug.Log($"Random Height  {layer.String()} {i}:   {rHeight[i]}\n");
            }
        }
        /// <summary>Seeds the quantized rotations with the current time ticks.</summary>
        public void ReseedQuantizedRotations()
        {
            quantRotSeed = (int)System.DateTime.Now.Ticks;
        }
        //---------------------------------------
        public void GenerateRandomQuantRotValues(int inputSeed = 0)
        {
            if (rQuantRot == null)
                rQuantRot = new List<float>();

            int seed = (inputSeed != 0) ? inputSeed : quantRotSeed;
            int count = af.GetPoolForLayer(layer).Count;

            UnityEngine.Random.InitState(seed);
            rQuantRot.Clear();
            for (int i = 0; i < count; i++)
            {
                rQuantRot.Add(UnityEngine.Random.value);
            }
        }
        //---------------------------------------
        /// <summary>Seeds the chance of missing elements with the current time ticks.</summary>
        public void ReeedChanceOfMissing()
        {
            chanceMissingSeed = (int)System.DateTime.Now.Ticks;
            GenerateRandomChanceMissingValues();
        }
        public void GenerateRandomChanceMissingValues(int inputSeed = 0)
        {
            if (rChanceMissing == null)
                rChanceMissing = new List<float>();

            int seed = (inputSeed != 0) ? inputSeed : quantRotSeed;
            int count = af.GetPoolForLayer(layer).Count;

            UnityEngine.Random.InitState(seed);
            rChanceMissing.Clear();
            for (int i = 0; i < count; i++)
            {
                rChanceMissing.Add(UnityEngine.Random.value);
            }
        }
        //----------------------------------------
        /// <summary>Seeds the size X and Z with the current time ticks.</summary>
        public void ReseedPostSizeXZ()
        {
            postSizeXZSeed = (int)System.DateTime.Now.Ticks;
            GenerateRandomPostSizeXZValues();
        }
        //---------------------------------------
        public void GenerateRandomPostSizeXZValues(int inputSeed = 0)
        {
            if (layer != LayerSet.postLayer)
                return;

            int seed = (inputSeed != 0) ? inputSeed : postSizeXZSeed;
            int count = af.GetPoolForLayer(layer).Count;

            if(count < af.allPostPositions.Count)
                count = af.allPostPositions.Count+1;

            //-- Post SizeXZ
            UnityEngine.Random.InitState(postSizeXZSeed);
            rPostSizeX = new List<float>();
            rPostSizeZ = new List<float>();
            for (int i = 0; i < count; i++)
            {
                rPostSizeX.Add(UnityEngine.Random.Range(af.minSizeXZPost, af.maxSizeXZPost));
                rPostSizeZ.Add(UnityEngine.Random.Range(af.minSizeXZPost, af.maxSizeXZPost));
            }
        }
        //---------------------------
        /// <summary>Seeds the shift X and Z with the current time ticks.</summary>
        public void ReseedPostShiftXZ()
        {
            postShiftXZSeed = (int)System.DateTime.Now.Ticks;
            GenerateRandomPostShiftXZ();
        }
        //---------------------------------------
        public void GenerateRandomPostShiftXZ(int inputSeed = 0)
        {
            if (layer != LayerSet.postLayer)
                return;

            int seed = (inputSeed != 0) ? inputSeed : postShiftXZSeed;
            int count = af.GetPoolForLayer(layer).Count;
            if (count < af.allPostPositions.Count)
                count = af.allPostPositions.Count + 1;

            //-- Post ShiftXZ
            UnityEngine.Random.InitState(postShiftXZSeed);
            rPostShiftX = new List<float>();
            rPostShiftZ = new List<float>();
            for (int i = 0; i < count; i++)
            {
                rPostShiftX.Add(UnityEngine.Random.Range(af.minShiftXZPost, af.maxShiftXZPost));
                rPostShiftZ.Add(UnityEngine.Random.Range(af.minShiftXZPost, af.maxShiftXZPost));
            }
        }
        //--------------------------
        /// <summary>Seeds the small rotations with the current time ticks.</summary>
        public void ReseedSmallRotations()
        {
            smallRotSeed = (int)System.DateTime.Now.Ticks;
            GenerateRandomSmallRotValues();
        }
        
        //---------------------------------------
        public void GenerateRandomSmallRotValues(int inputSeed = 0)
        {
            if (rSmallRot == null)
                rSmallRot = new List<Vector3>();

            int seed = (inputSeed != 0) ? inputSeed : smallRotSeed;
            int count = af.GetPoolForLayer(layer).Count;
            UnityEngine.Random.InitState(seed);

            Vector3 rot = GetSmallRandRotForLayer(layer);
            rSmallRot.Clear();
            for (int i = 0; i < count; i++)
            {
                float x = UnityEngine.Random.Range(-rot.x, rot.x);
                float y = UnityEngine.Random.Range(-rot.y, rot.y);
                float z = UnityEngine.Random.Range(-rot.z, rot.z);
                rSmallRot.Add(new Vector3(x, y, z));
            }
        }
        Vector3 GetSmallRandRotForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return af.smallRotationAmountRailA;
            if (layer == LayerSet.railBLayer)
                return af.smallRotationAmountRailB;
            if (layer == LayerSet.postLayer)
                return af.smallRotationAmountPost;
            //if (layer == LayerSet.extraLayer)
            //return af.smallRotationAmountExtra;
            if (layer == LayerSet.subpostLayer)
                return af.smallRotationAmountSubpost;
            return Vector3.zero;
        }
        //-------------------------------------
        public (float min, float max) GetRandomHeightRangeForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return (af.minRandHeightRailA, af.maxRandHeightRailA);
            if (layer == LayerSet.railBLayer)
                return (af.minRandHeightRailB, af.maxRandHeightRailB);
            if (layer == LayerSet.postLayer)
                return (af.minRandHeightPost, af.maxRandHeightPost);
            if (layer == LayerSet.extraLayer)
                return (af.minRandHeightExtra, af.maxRandHeightExtra);
            if (layer == LayerSet.subpostLayer)
                return (af.minRandHeightSubpost, af.maxRandHeightSubpost);

            return (0f, 0f);
        }
        //----------------------------------------



        /// <summary>Seeds the extra random value with the current time ticks.
        /// I have no idea what this is for, and the man who does seems to be on a permanent holiday.
        /// </summary>
        /*public void SeedExtraRandom()
        {
            extraRandomSeed = (int)System.DateTime.Now.Ticks;
        }*/

        /// <summary>Seeds the extra variable index with the current time ticks.</summary>
        public void SeedExtraVar()
        {
            extraVarIndexSeed = (int)System.DateTime.Now.Ticks;
        }
        public void SeedScatterPos()
        {
            scatterPosSeed = (int)System.DateTime.Now.Ticks;
        }
        public void SeedScatterScale()
        {
            scatterScaleSeed = (int)System.DateTime.Now.Ticks;
        }
        public void SeedScatterRot()
        {
            scatterRotSeed = (int)System.DateTime.Now.Ticks;
        }

        /// <summary>Seeds the global spacing with the current time ticks.</summary>
        public void SeedGlobalSpacing()
        {
            globalSpacingSeed = (int)System.DateTime.Now.Ticks;
        }


        /// <summary>Gets the chance of missing elements at the specified index for the given layer.</summary>
        /// <param name="index">Index of the element</param>
        /// <param name="layer">Layer set</param>
        /// <returns>Chance of missing elements</returns>
        internal float GetChanceOfMissingAtSectionIndex(int index, LayerSet layer)
        {
            int count = rChanceMissing.Count;
            if (index >= count)
            {
                Debug.LogWarning($"GetChanceOfMissingAtSectionIndex() index out of range: {index} count: {count} for layer {layer}\n");
                return 0f;
            }
            return rChanceMissing[index];
        }

    }

    //=======================================================================================================

    public partial class AutoFenceCreator
    {
        // Wrap Awake() and Reset() so we can control the order of loading, and ensure the prefabs have been loaded via AutoFenceEditor
        /// <summary>
        /// Creates a RandomSeededValuesAF for each layer set and initializes the seeds. Rails AB, Post Extras, Subposts.
        /// Uses 12345 by default, otherwise if useDateTime is true, uses the current time ticks.
        /// </summary>
        public void InitializeAllSeededValues(bool useDateTime = false)
        {
            int seedValue = 12345;
            if (useDateTime)
                seedValue = (int)System.DateTime.Now.Ticks;

            railASeeds = new RandomSeededValuesAF(LayerSet.railALayer, this, seedValue);
            railBSeeds = new RandomSeededValuesAF(LayerSet.railBLayer, this, seedValue);
            postAndGlobalSeeds = new RandomSeededValuesAF(LayerSet.postLayer, this, seedValue);
            extraSeeds = new RandomSeededValuesAF(LayerSet.extraLayer, this, seedValue);
            subpostSeeds = new RandomSeededValuesAF(LayerSet.subpostLayer, this, seedValue);
        }

        /// <summary>
        /// If seeds are null, rebuild
        /// </summary>
        public void ValidateAllSeeds()
        {
            ValidateSeedsForLayer(LayerSet.railALayer);
            ValidateSeedsForLayer(LayerSet.railBLayer);
            ValidateSeedsForLayer(LayerSet.postLayer);
            ValidateSeedsForLayer(LayerSet.extraLayer);
            ValidateSeedsForLayer(LayerSet.subpostLayer);

        }

        public void ValidateSeedsForLayer(LayerSet layer)
        {
            switch (layer)
            {
                case LayerSet.railALayer:
                    if (railASeeds == null)
                        railASeeds = new RandomSeededValuesAF(LayerSet.railALayer, this, 12345);
                    railASeeds.CheckSeedValues();
                    break;

                case LayerSet.railBLayer:
                    if (railBSeeds == null)
                        railBSeeds = new RandomSeededValuesAF(LayerSet.railBLayer, this, 12345);
                    railBSeeds.CheckSeedValues();
                    break;

                case LayerSet.postLayer:
                    if (postAndGlobalSeeds == null)
                        postAndGlobalSeeds = new RandomSeededValuesAF(LayerSet.postLayer, this, 12345);
                    postAndGlobalSeeds.CheckSeedValues();
                    break;

                case LayerSet.extraLayer:
                    if (extraSeeds == null)
                        extraSeeds = new RandomSeededValuesAF(LayerSet.extraLayer, this, 12345);
                    extraSeeds.CheckSeedValues();
                    break;

                case LayerSet.subpostLayer:
                    if (subpostSeeds == null)
                        subpostSeeds = new RandomSeededValuesAF(LayerSet.subpostLayer, this, 12345);
                    subpostSeeds.CheckSeedValues();
                    break;
                default:
                    break;
            }
        }
        //--------------------------------
        //create method GetSeedsForLayer(LayerSet layer) to return the seeds for a given layer
        public RandomSeededValuesAF GetSeedsForLayer(LayerSet layer)
        {
            switch (layer)
            {
                case LayerSet.railALayer:
                    return railASeeds;
                case LayerSet.railBLayer:
                    return railBSeeds;
                case LayerSet.postLayer:
                    return postAndGlobalSeeds;
                case LayerSet.extraLayer:
                    return extraSeeds;
                case LayerSet.subpostLayer:
                    return subpostSeeds;
                default:
                    return null;
            }
        }
        public void SetSeededValuesForLayer(LayerSet layer, RandomSeededValuesAF seededVals)
        {
            switch (layer)
            {
                case LayerSet.railALayer:
                    railASeeds = seededVals;
                    railASeeds.af = this;
                    break;

                case LayerSet.railBLayer:
                    railBSeeds = seededVals;
                    railBSeeds.af = this;
                    break;
                case LayerSet.postLayer:
                    postAndGlobalSeeds = seededVals;
                    postAndGlobalSeeds.af = this;
                    break;
                case LayerSet.extraLayer:
                    extraSeeds = seededVals;
                    extraSeeds.af = this;
                    break;
                case LayerSet.subpostLayer:
                    subpostSeeds = seededVals;
                    subpostSeeds.af = this;
                    break;

                default:
                    break;
            }
        }
        //---------------------------------------
        public void GenerateRandomSmallRotValuesForLayer(LayerSet layer, int inputSeed = 0)
        {
            if (layer == LayerSet.railALayer)
                railASeeds.GenerateRandomSmallRotValues(inputSeed);
            else if (layer == LayerSet.railBLayer)
                railBSeeds.GenerateRandomSmallRotValues(inputSeed);
            else if (layer == LayerSet.postLayer)
                postAndGlobalSeeds.GenerateRandomSmallRotValues(inputSeed);
            else if (layer == LayerSet.extraLayer)
                extraSeeds.GenerateRandomSmallRotValues(inputSeed);
            else if (layer == LayerSet.subpostLayer)
                subpostSeeds.GenerateRandomSmallRotValues(inputSeed);
        }

        //------------
        public void ReSeed()
        {
            af.SeedRandom(false);
            /*if (af.railSetToolbarChoice == 0)//A
            {
                af.shuffledRailAIndices = SourceVariations.CreateShuffledIndices(af.nonNullRailSourceVariants[0], af.allPostPositions.Count - 1);
                af.ResetRailAPool();
            }
            else if (af.railSetToolbarChoice == 1)//B
            {
                af.shuffledRailBIndices = SourceVariations.CreateShuffledIndices(af.nonNullRailSourceVariants[1], af.allPostPositions.Count - 1);
                af.ResetRailBPool();
            }*/
            af.ForceRebuildFromClickPoints();
        }
    }
}