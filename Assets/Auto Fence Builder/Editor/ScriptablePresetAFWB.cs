using AFWB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using static AFWB.AutoFenceCreator;

[CreateAssetMenu(fileName = "preset", menuName = "AutoFence/Preset", order = 1)]
public class ScriptablePresetAFWB : ScriptableObject
{
    //allFollowPostRaiseLower
    //keppinterpolated grounded

    public string categoryName = "";

    /// <summary>
    /// There is a RandomSeededValuesAF for each sourceLayerList
    /// </summary>

    public RandomSeededValuesAF railASeeds;
    public RandomSeededValuesAF railBSeeds;
    public RandomSeededValuesAF postAndGlobalSeeds;
    public RandomSeededValuesAF extraSeeds;
    public RandomSeededValuesAF subpostSeeds;

    //=========  Posts  =========
    public string postName = "ConcreteOldSquare_Post";

    public bool usePosts = true;
    public Vector3 postSize = Vector3.one, postRotation = Vector3.zero;
    public Vector3 mainPostsSizeBoost = Vector3.one;
    public Vector3 endPostsSizeBoost = Vector3.one;

    public bool allFollowPostRaiseLower = false;

    public bool allowNodePostsPrefabOverride = false;
    public int nodePostsOverridePrefabIndex = 1;

    public bool allowEndPostsPrefabOverride = false;
    public int endPostsOverridePrefabIndex = 1;


    //-- Posts Randomization
    public bool allowPostRandomization = true;
    public bool allowPostHeightVariation = false;
    public float minPostHeightVar = 0.95f, maxPostHeightVar = 1.05f;
    public bool allowRandPostRotationVariation = false;
    public Vector3 smallRandRotationAmountPost = Vector3.zero;
    public float quantizeRotAnglePost = 90;
    public bool allowQuantizedRandomPostRotation = false;
    public float chanceOfMissingPost = 0;
    public bool allowPostXZShift = false, allowPostSizeVariation = false; //added 31/12/21
    public float minPostXZShift = -.1f, maxPostXZShift = .1f;
    public float minPostSizeVar = 0.8f, maxPostSizeVar = 1.2f;
    public float postSurfaceNormalAmount = 0;
    public bool stretchPostWidthAtMitreJoint = false;
    public int postQuantizeRotAxis = 1;
    public float postQuantizeRotProb = 0;
    public int postRandomScope = 2; // 2 = all


    //-- Posts Variation
    public bool usePostVariations = false;
    public List<SourceVariant> postVariants = new List<SourceVariant>();
    public List<SeqItem> userSequencePost = Sequencer.CreateNewSeqList(AutoFenceCreator.kMaxNumSeqSteps);
    public bool usePostSequencer = false;
    public int numUserSeqStepsPost = 5;
    public int numPostVariants = 2;

    //=========  Rails A  =========
    public bool useRailsA = true;

    public string railAName = "Wall-High_Brick_Panel_Rail";
    public int numStackedRailsA = 1;
    public float railASpread = 0.6f;
    public int spreadModeRailA = 0;
    public Vector3 railAPositionOffset = Vector3.zero, railASize = Vector3.one, railARotation = Vector3.zero;
    public bool railAKeepGrounded = false;
    public bool rotateFromBaseRailA = false;
    public bool overlapAtCorners;
    public bool autoHideBuriedRails;
    public SlopeMode slopeModeRailA;
    public JointStyle jointStyleRailA = JointStyle.overlap;

    //-- Rails A Randomization
    public int railARandomScope = 2; // 2 = Main & Vars

    public bool allowRailAHeightVariation = false;
    public float minRailAHeightVar = 0.57f, maxRailAHeightVar = 1.47f;
    public bool allowRandRailARotationVariation = false;
    public Vector3 smallRotationAmountRailA = Vector3.zero;
    public float chanceOfMissingRailA = 0;
    public float quantizeRotAngleRailA = 90;
    public bool allowQuantizedRandomRailARotation = false;

    public int quantizeRotAxisRailA = 1;
    public float quantizeRotProbRailA = 0f;

    //-- Rails A Variants
    public List<SourceVariant> railAVariants;

    public bool useRailAVariations = false;
    public bool scaleVariationHeightToMainHeightA = false;
    public bool allowIndependentSubmeshVariationA = false;
    public VariationMode variationModeRailA = VariationMode.sequenced;

    //-- Rail A Sequence
    public List<SeqItem> userSequenceRailA = Sequencer.CreateNewSeqList(AutoFenceCreator.kMaxNumSeqSteps);

    public int numUserSeqStepsRailA = 5;
    public List<SeqItem> optimalSequenceRailA = Sequencer.CreateNewSeqList(16);
    //========  Rails B  =========
    public string railBName = "ABasicConcrete_Panel_Rail";

    public bool useRailsB = false;
    public int numStackedRailsB = 1;
    public float railBSpread = 0.6f;
    public int spreadModeRailB = 0;
    public Vector3 railBPositionOffset = Vector3.zero, railBSize = Vector3.one, railBRotation = Vector3.zero;
    public bool railBKeepGrounded = false;
    public bool rotateFromBaseRailB = false;
    public SlopeMode slopeModeRailB;
    public JointStyle jointStyleRailB = JointStyle.overlap;

    //-- Rails B Randomization
    public int railBRandomScope = 2;

    public bool allowRailBHeightVariation = false;
    public float minRailBHeightVar = 0.95f, maxRailBHeightVar = 1.05f;
    public bool allowRandRailBRotationVariation = false;
    public Vector3 smallRotationAmountRailB = Vector3.zero;
    public float chanceOfMissingRailB = 0;
    public float quantizeRotAngleRailB = 90;
    public bool allowQuantizedRandomRailBRotation = false;
    public int quantizeRotAxisRailB = 1;
    public float quantizeRotProbRailB = 0;

    //-- Rails B Variation
    public bool useRailBVariations = false;

    public bool scaleVariationHeightToMainHeightB = false;
    public bool allowIndependentSubmeshVariationB = false;
    public VariationMode variationModeRailB = VariationMode.sequenced;
    public List<SourceVariant> railBVariants = new List<SourceVariant>();
    public List<SeqItem> userSequenceRailB = Sequencer.CreateNewSeqList(AutoFenceCreator.kMaxNumSeqSteps);
    public int numUserSeqStepsRailB = 5;
    public List<SeqItem> optimalSequenceRailB = Sequencer.CreateNewSeqList(16);
    //public RandomRecords railBRandRec;
    public bool useRailBSequencer = true;

    //=====  Rails Common A & B  =====
    public bool[] useRailSequencer = { false, false };

    public int[] numRailVariants = { 2, 2 }; //added 29/3/23
    public bool[] allowRailRandomization = { false, false };
    public bool[] extendRailEnds = { false, false };
    public float[] endExtensionLength = { 0.2f, 0.2f };
    public bool[] allowMirroring_X_Rail = { false, false };

    //=========  Subposts  =========
    public string subpostName = "ConcreteOldSquare_Post";

    public bool useSubposts = false;
    public int subsFixedOrProportionalSpacing = 0;
    public float subSpacing;
    public Vector3 subPositionOffset = Vector3.zero;
    public Vector3 subSize = Vector3.one;
    public Vector3 subRotation = Vector3.zero;
    public bool forceSubsToGroundContour;
    public bool keepSubsAboveGround;
    public bool useWave, useSubJoiners;
    public float frequency;
    public float amplitude;
    public float wavePosition;
    public float subPostSpread = 0;

    //-- SubPosts Randomization
    public bool allowSubPostHeightVariation = false;

    public float minSubPostHeightVar = 0.95f, maxSubPostHeightVar = 1.05f;
    public bool allowRandSubPostRotationVariation = false;
    public float chanceOfMissingSubPost = 0;
    public Vector3 smallRotationAmountSubpost = Vector3.zero;
    public bool allowQuantizedRandomSubPostRotation = false;
    public float quantizeRotAngleSubpost = 90;
    public bool allowSubpostRandomization = true;
    public float quantizeRotProbSubpost = 0;

    //-- Subposts Variation
    public bool useSubpostVariations = false;

    public List<SourceVariant> subpostVariants = new List<SourceVariant>();
    public List<SeqItem> userSequenceSubpost = Sequencer.CreateNewSeqList(AutoFenceCreator.kMaxNumSeqSteps);
    public int numUserSeqStepsSubpost = 5;
    //public RandomRecords subpostRandRec;

    //=========  Extras  =========
    public bool useExtras = false;

    public string extraName = "ConcreteOldSquare_Post";
    public bool relativeMovement;
    public bool relativeScaling;
    public Vector3 extraPositionOffset, extraSize = Vector3.one, extraRotation;
    public int extraFrequency; //****
    public bool makeMultiArray;
    public int numExtras;
    public float extrasGap;
    public bool raiseExtraByPostHeight;
    public bool extrasFollowIncline = false;

    public int extrasMode = 0;
    public float extraSpawnAreaWidth = 2;
    public int numScatterColumns = 3;
    public int numScatterRows = 4;
    public Vector3 scatterExtraRandPos = Vector3.zero;
    public Vector3 scatterExtraRandScale = Vector3.zero;
    public Vector3 scatterExtraRandRot = Vector3.zero;
    public float scatterRandomStrength = 1.0f;
    public int randomScaleMode;
    public float flipXProb = 0, flipYProb = 0, flipZProb = 0;
    public FlipMode extraRandomFlipMode = FlipMode.NoFlip; //0 = 90, 1 = 180
    public int extraFrequencyMode = 1;
    public bool excludeExtraVarXZRotations = true;
    public ExtraVarsStruct extraVarsStruct; // contains a List of ExtraVars
    public bool enableChoosePrefabs = false;
    public int finalPostMode = 0; //0= no change,  1 = force off,   2 = force on
    public PivotPosition pivotPosition = PivotPosition.Center;//0 = base, 1 = center

    //=========  Globals  ========
    //public float globalHeight = 1;
    public float postHeightOffset = 0;

    public bool interpolate = true;
    public float interPostDist = 3;
    public bool smooth = false;
    public float tension = 0.0f;
    public float roundingDistance = 1.5f;
    public float removeIfLessThanAngle = 7, stripTooClose = 0.75f;

    public Vector3 globalScale = Vector3.one;
    public bool scaleInterpolationAlso = true;
    public bool snapMainPosts = false;
    public float snapSize = 1;
    public bool hideInterpolated = false;
    public bool lerpPostRotationAtCorners = true;
    public float postSpacingVariation = 0;
    public ColliderType railAColliderMode = 0;
    public ColliderType postColliderMode = 0;

    public string notes = "";

    //------------------------------------------------
    // Used when Loading
    private static List<SourceVariant> UpdateSourceVariantList(List<SourceVariant> sourceList, LayerSet layer, AutoFenceCreator af, ScriptablePresetAFWB preset)
    {
        if (sourceList == null)
            return null;
        List<SourceVariant> destinationVariants = af.GetSourceVariantsForLayer(layer);
        if (destinationVariants == null)
            return null;

        GameObject mainGoForPreset = PresetCheckFixEd.GetMainPrefabForLayerForPreset(layer, af, preset);
        // Possibility with old or broken presetsEd

        int numFixes = PresetCheckFixEd.CheckAndRepairSourceVariantsList(sourceList, layer, preset, af, mainGoForPreset);
        if (numFixes > 0)
        {
            Debug.Log($"Fixed {numFixes}  SourceVariants in preset:   {preset.categoryName}/ {preset.name}    for sourceLayerList:   {layer.ToString()} \n");
            preset.minPostHeightVar = 0.7777f;
            string filePath = ScriptablePresetAFWB.CreateSaveString(af, preset.name, preset.categoryName);
            //bool saved = ScriptablePresetAFWB.SaveScriptablePreset(af, preset, filePath, false, true);
            Debug.Log($"Saved preset to  {filePath}\n");
        }

        for (int i = 0; i < sourceList.Count; i++)
        {
            SourceVariant sourceVariant = sourceList[i];
            if (sourceVariant == null)
                continue;
            destinationVariants[i].UpdateSourceVariant(sourceVariant, af, layer);
        }
        return destinationVariants;
    }

    //------------------------------------------------
    // Used when saving
    private static List<SourceVariant> CopySourceVariantList(List<SourceVariant> sourceList, LayerSet layer, AutoFenceCreator af, ScriptablePresetAFWB preset)
    {
        if (sourceList == null)
            return null;

        GameObject mainForPreset = PresetCheckFixEd.GetMainPrefabForLayerForPreset(layer, af, preset);
        // Possibility with old or broken presetsEd
        PresetCheckFixEd.CheckAndRepairSourceVariantsList(sourceList, layer, preset, af, mainForPreset);
        List<SourceVariant> copyList = SourceVariant.CreateInitialisedSourceVariantList();
        for (int i = 0; i < sourceList.Count; i++)
        {
            SourceVariant thisVariant = sourceList[i];
            if (thisVariant == null)
            {
                continue;
            }

            // we expect it will be null when loading as the go hasn't been assigned yet. will find go by presetName if needed
            SourceVariant copy = new SourceVariant(thisVariant, copyGo: true, af, layer);
            copyList[i] = copy;
        }
        return copyList;
    }


    //-----------------------

    private static List<SeqItem> CopySequenceList(List<SeqItem> sourceList, bool copyGo, int enforceMinimumCount)
    {
        if (sourceList == null)
            return null;
        int count = sourceList.Count;
        List<SeqItem> newList = Sequencer.CreateNewSeqList(count);
        for (int i = 0; i < sourceList.Count; i++)
        {
            SeqItem thisSeqStep = sourceList[i];
            if (thisSeqStep == null)
            {
                continue;
            }
            //if (thisSeqStep.go != null)
            //thisSeqStep.searchName = thisSeqStep.go.presetName; // we save the go as a presetName, rather than a reference to the object
            SeqItem copy = new SeqItem(thisSeqStep);

            /*if (copyGo == false)
                copy.go = null;*/
            newList[i] = copy;
        }
        // Pad with initialized if nedded
        if (count < enforceMinimumCount)
        {
            int padCount = enforceMinimumCount - count;
            for (int i = 0; i < padCount; i++)
            {
                newList.Add(new SeqItem());
            }
        }
        return newList;
    }

    //================================================================
    //        Create Preset (Save)
    //================================================================
    public static ScriptablePresetAFWB CreatePresetFromCurrentSettings(string name, string categoryName, AutoFenceCreator af)
    {
        // (used 'preset.' for some easy copy/replace/paste job during preset sysyem conversion)
        //==  Globals  ==
        //preset.globalHeight = af.globalHeight;

        //keepInterpolatedPostsGrounded

        ScriptablePresetAFWB preset = ScriptableObject.CreateInstance<ScriptablePresetAFWB>();
        preset.name = name;
        preset.categoryName = categoryName;

        //preset.presetSeedsList = af.presetSeedsList;

        preset.globalScale = af.globalScale;
        preset.interpolate = af.interpolate;
        preset.interPostDist = af.interPostDist;
        preset.smooth = af.smooth;
        preset.tension = af.tension;
        preset.roundingDistance = af.roundingDistance;
        preset.removeIfLessThanAngle = af.removeIfLessThanAngle;
        preset.stripTooClose = af.stripTooClose;
        preset.overlapAtCorners = af.overlapAtCorners;
        preset.autoHideBuriedRails = af.autoHideBuriedRails;
        preset.slopeModeRailA = af.slopeMode[AutoFenceCreator.kRailALayerInt];
        preset.slopeModeRailB = af.slopeMode[AutoFenceCreator.kRailBLayerInt];
        preset.scaleInterpolationAlso = af.scaleInterpolationAlso;
        preset.snapMainPosts = af.snapMainPosts;
        preset.snapSize = af.snapSize;
        preset.hideInterpolated = af.hideInterpolated;
        preset.lerpPostRotationAtCorners = af.lerpPostRotationAtCorners;
        preset.postSpacingVariation = af.postSpacingVariation;
        preset.railAColliderMode = (ColliderType)af.railAColliderMode; // cast for compatibilkity with old presetsEd
        preset.postColliderMode = (ColliderType)af.postColliderMode;
        preset.allFollowPostRaiseLower = af.allFollowPostRaiseLower;

        //==  Posts  ==
        preset.usePosts = af.usePostsLayer;
        preset.postHeightOffset = af.postHeightOffset;
        preset.postName = af.postPrefabs[af.currentPost_PrefabIndex].name;
        preset.postSize = af.postScale;
        preset.postRotation = af.postRotation;
        preset.mainPostsSizeBoost = af.mainPostsSizeBoost;
        preset.endPostsSizeBoost = af.endPostsSizeBoost;
        //-- Posts Randomization
        preset.allowPostHeightVariation = af.allowHeightVariationPost;
        preset.minPostHeightVar = af.minRandHeightPost;
        preset.maxPostHeightVar = af.maxRandHeightPost;
        preset.allowRandPostRotationVariation = af.allowRandPostSmallRotationVariation;
        preset.smallRotationAmountRailA = af.smallRotationAmountRailA;
        preset.quantizeRotAnglePost = af.quantizeRotAnglePost;
        preset.allowQuantizedRandomPostRotation = af.allowQuantizedRandomPostRotation;
        preset.chanceOfMissingPost = af.chanceOfMissingPost;
        preset.allowPostXZShift = af.allowPostXZShift; //added 31/12/21
        preset.minPostXZShift = af.minShiftXZPost;
        preset.maxPostXZShift = af.maxShiftXZPost;
        preset.allowPostSizeVariation = af.allowPostSizeVariation;
        preset.minPostSizeVar = af.minSizeXZPost;
        preset.maxPostSizeVar = af.maxSizeXZPost;
        preset.allowPostRandomization = af.allowPostRandomization;
        preset.postQuantizeRotAxis = af.quantizeRotAxisPost;
        preset.postQuantizeRotProb = af.quantizeRotProbPost;
        preset.postSurfaceNormalAmount = af.postSurfaceNormalAmount;
        preset.stretchPostWidthAtMitreJoint = af.stretchPostWidthAtMitreJoint;
        preset.postAndGlobalSeeds = af.postAndGlobalSeeds;

        //-- Posts Variation
        preset.usePostVariations = af.usePostVariations;

        preset.allowNodePostsPrefabOverride = af.allowNodePostsPrefabOverride;
        preset.nodePostsOverridePrefabIndex = af.nodePostsOverridePrefabIndex;

        preset.allowEndPostsPrefabOverride = af.allowEndPostsPrefabOverride;
        preset.endPostsOverridePrefabIndex = af.endPostsOverridePrefabIndex;

        preset.userSequencePost = CopySequenceList(af.postSequencer.seqList, false, AutoFenceCreator.kMaxNumSeqSteps);
        preset.numUserSeqStepsPost = af.postSequencer.Length();
        //preset.postRandRec = af.postRandRec;

        //==  Rails A  ==
        preset.useRailsA = af.useRailLayer[0];
        preset.railAName = af.railPrefabs[af.currentRail_PrefabIndex[0]].name;
        preset.numStackedRailsA = (int)af.numStackedRails[AutoFenceCreator.kRailALayerInt];
        preset.railASpread = af.railSpread[AutoFenceCreator.kRailALayerInt];
        preset.spreadModeRailA = (int)af.railSpreadMode[AutoFenceCreator.kRailALayerInt];
        preset.railAPositionOffset = af.railAPositionOffset;
        preset.railASize = af.railAScale;
        preset.railARotation = af.railARotation;
        //preset.railAKeepGrounded = af.keepRailGrounded[AutoFenceCreator.kRailALayerInt];
        preset.railAKeepGrounded = false;
        preset.rotateFromBaseRailA = af.rotateFromBaseRailA;
        preset.slopeModeRailA = af.slopeMode[AutoFenceCreator.kRailALayerInt];
        preset.jointStyleRailA = af.railJointStyle[AutoFenceCreator.kRailALayerInt];

        //==  Rails B  ==
        preset.useRailsB = af.useRailLayer[1];
        preset.railBName = af.railPrefabs[af.currentRail_PrefabIndex[1]].name;
        preset.numStackedRailsB = (int)af.numStackedRails[AutoFenceCreator.kRailBLayerInt];
        preset.railBSpread = af.railSpread[AutoFenceCreator.kRailBLayerInt];
        preset.spreadModeRailB = (int)af.railSpreadMode[AutoFenceCreator.kRailBLayerInt];
        preset.railBPositionOffset = af.railBPositionOffset;
        preset.railBSize = af.railBScale;
        preset.railBRotation = af.railBRotation;
        //preset.railBKeepGrounded = af.keepRailGrounded[AutoFenceCreator.kRailBLayerInt];
        preset.railBKeepGrounded = false;
        preset.rotateFromBaseRailB = af.rotateFromBaseRailB;
        preset.slopeModeRailB = af.slopeMode[AutoFenceCreator.kRailBLayerInt];
        preset.jointStyleRailB = af.railJointStyle[AutoFenceCreator.kRailBLayerInt];

        //-- Rail A Randomization
        preset.railARandomScope = (int)af.railARandomScope;
        preset.allowRailAHeightVariation = af.allowHeightVariationRailA;
        preset.minRailAHeightVar = af.minRandHeightRailA;
        preset.maxRailAHeightVar = af.maxRandHeightRailA;
        preset.smallRotationAmountRailA = af.smallRotationAmountRailA;
        preset.chanceOfMissingRailA = af.chanceOfMissingRailA;
        preset.allowQuantizedRandomRailARotation = af.allowQuantizedRandomRailARotation;
        preset.quantizeRotAngleRailA = af.quantizeRotAngleRailA;
        preset.quantizeRotAxisRailA = af.quantizeRotAxisRailA;
        preset.quantizeRotProbRailA = af.quantizeRotProbRailA;
        preset.railASeeds = af.railASeeds;

        //-- Rail B Randomization
        preset.railBRandomScope = (int)af.railBRandomScope;
        preset.allowRailBHeightVariation = af.allowHeightVariationRailB;
        preset.minRailBHeightVar = af.minRandHeightRailB;
        preset.maxRailBHeightVar = af.maxRandHeightRailB;
        preset.smallRotationAmountRailB = af.smallRotationAmountRailB;
        preset.chanceOfMissingRailB = af.chanceOfMissingRailB;
        preset.allowRailRandomization[kRailBLayerInt] = af.allowRailRandomization[kRailBLayerInt];
        preset.allowQuantizedRandomRailBRotation = af.allowQuantizedRandomRailBRotation;
        preset.quantizeRotAngleRailB = af.quantizeRotAngleRailB;
        preset.quantizeRotAxisRailB = af.quantizeRotAxisRailB;
        preset.quantizeRotProbRailB = af.quantizeRotProbRailB;
        preset.railBSeeds = af.railBSeeds;

        //-- Rail A Variation
        preset.scaleVariationHeightToMainHeightA = af.scaleVariationHeightToMainHeightA;
        preset.allowIndependentSubmeshVariationA = af.allowIndependentSubmeshVariationA;
        preset.useRailAVariations = af.useRailVariations[kRailALayerInt];
        preset.variationModeRailA = af.variationModeRailA;
        preset.numRailVariants[kRailALayerInt] = af.numRailVariantsInUse[kRailALayerInt];
        preset.allowRailRandomization[kRailALayerInt] = af.allowRailRandomization[kRailALayerInt];

        //-- Rail B Variation
        preset.scaleVariationHeightToMainHeightB = af.scaleVariationHeightToMainHeightB;
        preset.allowIndependentSubmeshVariationB = af.allowIndependentSubmeshVariationB;
        preset.useRailBVariations = af.useRailVariations[kRailBLayerInt];
        preset.variationModeRailB = af.variationModeRailB;

        preset.numRailVariants[kRailBLayerInt] = af.numRailVariantsInUse[kRailBLayerInt];
        preset.allowRailRandomization[kRailBLayerInt] = af.allowRailRandomization[kRailBLayerInt];

        //-- Rail A Sequencer
        preset.useRailSequencer[kRailALayerInt] = af.GetUseSequencerForLayer(LayerSet.railALayer);
        preset.userSequenceRailA = CopySequenceList(af.railASequencer.seqList, false, AutoFenceCreator.kMaxNumSeqSteps);
        preset.numUserSeqStepsRailA = af.railASequencer.Length();
        preset.optimalSequenceRailA = CopySequenceList(af.optimalSequenceRailA, false, 1);
        //-- Rail B Sequencer
        preset.useRailSequencer[kRailBLayerInt] = af.GetUseSequencerForLayer(LayerSet.railBLayer);
        preset.userSequenceRailB = CopySequenceList(af.railBSequencer.seqList, false, AutoFenceCreator.kMaxNumSeqSteps);
        preset.numUserSeqStepsRailB = af.railBSequencer.Length();
        preset.optimalSequenceRailB = CopySequenceList(af.optimalSequenceRailB, false, 1);

        preset.extendRailEnds[kRailALayerInt] = af.extendRailEnds[kRailALayerInt];
        preset.extendRailEnds[kRailBLayerInt] = af.extendRailEnds[kRailBLayerInt];
        preset.endExtensionLength[kRailALayerInt] = af.endExtensionLength[kRailALayerInt];
        preset.endExtensionLength[kRailBLayerInt] = af.endExtensionLength[kRailBLayerInt];

        preset.allowMirroring_X_Rail[kRailALayerInt] = af.allowMirroring_X_Rail[kRailALayerInt];
        preset.allowMirroring_X_Rail[kRailBLayerInt] = af.allowMirroring_X_Rail[kRailBLayerInt];

        //==  Extras  ==
        preset.useExtras = af.useExtrasLayer;
        preset.extraName = af.extraPrefabs[af.currentExtra_PrefabIndex].name;
        preset.relativeMovement = af.ex.relativeMovement;
        preset.relativeScaling = af.ex.relativeScaling;
        preset.extraPositionOffset = af.ex.extraTransformPositionOffset;
        preset.extraSize = af.ex.extraTransformScale;
        preset.extraRotation = af.ex.extraTransformRotation;
        preset.extraFrequency = af.ex.extraFreq; //****
        preset.makeMultiArray = af.ex.makeMultiArray;
        preset.numExtras = af.ex.numExtras;
        preset.extrasGap = af.ex.extrasGap;
        preset.raiseExtraByPostHeight = af.ex.raiseExtraByPostHeight;
        preset.extrasFollowIncline = af.ex.extrasFollowIncline;

        preset.extrasMode = (int)af.ex.extrasMode;
        preset.extraSpawnAreaWidth = af.ex.gridWidth;
        preset.numScatterColumns = (int)af.ex.numGridX;
        preset.numScatterRows = (int)af.ex.numGridZ;
        preset.scatterExtraRandPos = af.ex.scatterExtraRandPosRange;
        preset.scatterExtraRandScale = af.ex.scatterExtraRandScaleRange;
        preset.scatterExtraRandRot = af.ex.scatterExtraRandRotRange;
        preset.scatterRandomStrength = af.ex.scatterRandomStrength;
        preset.extraSeeds = af.extraSeeds;
        preset.randomScaleMode = af.ex.randomScaleMode;
        preset.flipXProb = af.ex.flipXProb;
        preset.flipYProb = af.ex.flipYProb;
        preset.flipZProb = af.ex.flipZProb;
        preset.extraRandomFlipMode = af.ex.flipMode;
        preset.extraFrequencyMode = (int)af.ex.extraFreqMode;
        preset.excludeExtraVarXZRotations = af.ex.excludeExtraVarXZRotations;
        preset.extraVarsStruct = af.ex.extraVarsStruct;
        preset.enableChoosePrefabs = af.ex.enablePrefabVars;
        preset.finalPostMode = af.ex.finalPostMode;
        preset.pivotPosition = af.ex.pivotPosition;

        //==  SubPosts  ==
        preset.useSubposts = af.useSubpostsLayer;
        preset.subpostName = af.postPrefabs[af.currentSubpost_PrefabIndex].name;
        preset.subsFixedOrProportionalSpacing = (int)af.subsSpacingMode;
        preset.subSpacing = af.subSpacing;
        preset.subPositionOffset = af.subpostPositionOffset;
        preset.subSize = af.subpostScale;
        preset.subRotation = af.subpostRotation;
        preset.forceSubsToGroundContour = af.forceSubsToGroundContour;
        preset.keepSubsAboveGround = af.keepSubsAboveGround;
        preset.useWave = af.useSubWave;
        preset.useSubJoiners = af.useSubJoiners;
        preset.frequency = af.subWaveFreq;
        preset.amplitude = af.subWaveAmp;
        preset.wavePosition = af.subWavePosition;
        preset.subPostSpread = af.subPostSpread;
        //-- Subposts Variation
        preset.useSubpostVariations = af.useSubpostVariations;
        preset.userSequenceSubpost = CopySequenceList(af.userSequenceSubpost, false, AutoFenceCreator.kMaxNumSeqSteps);
        //preset.numUserSeqStepsSubpost = af.subseqNumSteps[kPostLayerInt];
        //preset.subpostRandRec = af.subpostRandRec;

        //-- SubPosts Randomization
        preset.allowSubPostHeightVariation = af.allowHeightVariationSubpost;
        preset.minSubPostHeightVar = af.minRandHeightSubpost;
        preset.allowRandSubPostRotationVariation = af.allowRandSubpostSmallRotationVariation;
        preset.smallRotationAmountSubpost = af.smallRotationAmountSubpost;
        preset.chanceOfMissingSubPost = af.chanceOfMissingSubpost;
        preset.allowQuantizedRandomSubPostRotation = af.allowQuantizedRandomSubpostRotation;
        preset.quantizeRotAngleSubpost = af.quantizeRotAngleSubpost;
        preset.allowSubpostRandomization = af.allowSubpostRandomization;
        preset.quantizeRotProbSubpost = af.quantizeRotProbSubpost;
        preset.subpostSeeds = af.subpostSeeds;

        //===  Copy the SourceVariants List to the Preset  ===
        // Do this at end to make sure the rest of the preset is correctly configured
        preset.postVariants = CopySourceVariantList(af.postSourceVariants, LayerSet.postLayer, af, preset);// false means don't copy go, copy searchName only
        preset.railAVariants = CopySourceVariantList(af.railSourceVariants[kRailALayerInt], LayerSet.railALayer, af, preset);
        preset.railBVariants = CopySourceVariantList(af.railSourceVariants[kRailBLayerInt], LayerSet.railBLayer, af, preset);
        preset.subpostVariants = CopySourceVariantList(af.subpostSourceVariants, LayerSet.postLayer, af, preset);// false means don't copy go, copy searchName only
        preset.notes = af.presetNotes;
        return preset;
    }

    //===========================================
    //Called from: SetupPreset()      CreateCurrentFromFinished()
    public void BuildFromPreset(AutoFenceCreator af)
    {
        //Set defaults to new feature parameters to maintain compatibility with old presetsEd

        //==  Globals  ==
        af.globalScale = globalScale;
        af.postHeightOffset = postHeightOffset;
        af.interpolate = interpolate;
        af.interPostDist = interPostDist;
        af.smooth = smooth;
        af.tension = tension;
        af.roundingDistance = roundingDistance;
        af.removeIfLessThanAngle = removeIfLessThanAngle;
        af.stripTooClose = stripTooClose;
        af.overlapAtCorners = overlapAtCorners;
        af.autoHideBuriedRails = autoHideBuriedRails;
        af.slopeMode[AutoFenceCreator.kRailALayerInt] = slopeModeRailA;
        af.slopeMode[AutoFenceCreator.kRailBLayerInt] = slopeModeRailB;
        af.scaleInterpolationAlso = scaleInterpolationAlso;
        af.snapMainPosts = snapMainPosts;
        af.snapSize = snapSize;
        af.hideInterpolated = hideInterpolated;
        af.lerpPostRotationAtCorners = lerpPostRotationAtCorners;
        af.postSpacingVariation = postSpacingVariation;
        af.postSurfaceNormalAmount = postSurfaceNormalAmount;
        if (postSurfaceNormalAmount == 0)
            af.adaptPostToSurfaceDirection = false;
        else
            af.adaptPostToSurfaceDirection = true;
        af.railAColliderMode = railAColliderMode;
        af.postColliderMode = postColliderMode;
        af.allFollowPostRaiseLower = allFollowPostRaiseLower;

        //==  Posts  ==
        af.currentPost_PrefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, postName, $"ScriptablePreset.BuildFromPreset({categoryName}/{name})  postName ");
        af.usePostsLayer = usePosts;
        af.postHeightOffset = postHeightOffset;
        af.postScale = postSize;
        af.postRotation = postRotation;
        af.mainPostsSizeBoost = mainPostsSizeBoost;
        af.endPostsSizeBoost = endPostsSizeBoost;
        //-- Posts Randomization
        af.allowHeightVariationPost = allowPostHeightVariation;
        af.minRandHeightPost = minPostHeightVar;
        af.maxRandHeightPost = maxPostHeightVar;
        af.allowRandPostSmallRotationVariation = allowRandPostRotationVariation;
        af.smallRotationAmountRailA = smallRotationAmountRailA;
        af.quantizeRotAnglePost = quantizeRotAnglePost;
        af.allowQuantizedRandomPostRotation = allowQuantizedRandomPostRotation;
        af.chanceOfMissingPost = chanceOfMissingPost;
        af.allowPostXZShift = allowPostXZShift; //added 31/12/21
        af.minShiftXZPost = minPostXZShift;
        af.maxShiftXZPost = maxPostXZShift;
        af.allowPostSizeVariation = allowPostSizeVariation; //added 31/12/21
        af.minSizeXZPost = minPostSizeVar;
        af.maxSizeXZPost = maxPostSizeVar;
        af.allowPostRandomization = allowPostRandomization;
        af.quantizeRotAxisPost = postQuantizeRotAxis;
        af.quantizeRotProbPost = postQuantizeRotProb;
        af.SetSeededValuesForLayer(LayerSet.postLayer, postAndGlobalSeeds);
        af.postRandomScope = RandomScope.all;

        //-- Posts Variation
        af.usePostVariations = usePostVariations;
        af.postSequencer.seqList = CopySequenceList(userSequencePost, false, AutoFenceCreator.kMaxNumSeqSteps); ;
        //af.seqNumSteps[kPostLayerInt] = numUserSeqStepsPost;
        af.postSurfaceNormalAmount = postSurfaceNormalAmount;
        af.stretchPostWidthAtMitreJoint = stretchPostWidthAtMitreJoint;
        af.allowNodePostsPrefabOverride = allowNodePostsPrefabOverride;
        af.nodePostsOverridePrefabIndex = nodePostsOverridePrefabIndex;



        //==  Rails A  ==
        af.currentRail_PrefabIndex[0] = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.railPrefab, railAName, $"ScriptablePreset.BuildFromPreset({categoryName}//{name})  railA Name ");
        af.useRailLayer[0] = useRailsA;
        af.SetRailPrefab(af.currentRail_PrefabIndex[0], LayerSet.railALayer, false, false);
        af.numStackedRails[kRailALayerInt] = numStackedRailsA;
        af.railSpread[kRailALayerInt] = railASpread;
        af.railSpreadMode[kRailALayerInt] = (RailSpreadMode)spreadModeRailA;
        af.railAPositionOffset = railAPositionOffset;
        af.railAScale = railASize;
        af.railARotation = railARotation;
        // af.keepRailGrounded[AutoFenceCreator.kRailALayerInt] = railAKeepGrounded;
        af.keepRailGrounded[AutoFenceCreator.kRailALayerInt] = false;
        af.slopeMode[AutoFenceCreator.kRailALayerInt] = slopeModeRailA;
        af.railJointStyle[AutoFenceCreator.kRailALayerInt] = jointStyleRailA;

        //-- Rail A Randomization
        af.railARandomScope = RandomScope.all;
        af.allowHeightVariationRailA = allowRailAHeightVariation;
        af.minRandHeightRailA = minRailAHeightVar;
        if (af.minRandHeightRailA < af.minRailHeightLimit)
            af.minRandHeightRailA = 0.97f;
        af.maxRandHeightRailA = maxRailAHeightVar;
        if (af.maxRandHeightRailA <= af.minRailHeightLimit)
            af.maxRandHeightRailA = 1.03f;
        af.smallRotationAmountRailA = smallRotationAmountRailA;
        af.chanceOfMissingRailA = chanceOfMissingRailA;
        af.quantizeRotAngleRailA = quantizeRotAngleRailA;
        af.allowQuantizedRandomRailARotation = allowQuantizedRandomRailARotation;
        af.allowRailRandomization[kRailALayerInt] = allowRailRandomization[kRailALayerInt];
        af.quantizeRotAxisRailA = quantizeRotAxisRailA;
        af.quantizeRotProbRailA = quantizeRotProbRailA;
        af.allowMirroring_X_Rail[kRailALayerInt] = allowMirroring_X_Rail[kRailALayerInt];
        af.allowMirroring_X_Rail[kRailBLayerInt] = allowMirroring_X_Rail[kRailBLayerInt];

        af.SetSeededValuesForLayer(LayerSet.railALayer, railASeeds);

        //-- Rail A Variation
        af.useRailVariations[kRailALayerInt] = useRailAVariations;
        af.scaleVariationHeightToMainHeightA = scaleVariationHeightToMainHeightA;
        af.allowIndependentSubmeshVariationA = allowIndependentSubmeshVariationA;
        af.variationModeRailA = variationModeRailA;
        //-- Rail A Sequencer
        af.SetUseSequencerForLayer(LayerSet.railALayer, useRailSequencer[kRailALayerInt]);
        af.railASequencer.seqList = CopySequenceList(userSequenceRailA, false, AutoFenceCreator.kMaxNumSeqSteps);
        //af.seqNumSteps[kRailALayerInt] = numUserSeqStepsRailA;
        af.optimalSequenceRailA = CopySequenceList(optimalSequenceRailA, false, 1);
        //af.railARandRec = railARandRec;

        //==  Rails B  ==
        af.currentRail_PrefabIndex[1] = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.railPrefab, railBName, $"ScriptablePreset.BuildFromPreset({categoryName}/{name})  railB Name ");
        af.useRailLayer[1] = useRailsB;
        af.SetRailPrefab(af.currentRail_PrefabIndex[1], LayerSet.railBLayer, false, false);
        af.numStackedRails[kRailBLayerInt] = numStackedRailsB;
        af.railSpread[kRailBLayerInt] = railBSpread;
        af.railSpreadMode[kRailBLayerInt] = (RailSpreadMode)spreadModeRailB;

        af.railBPositionOffset = railBPositionOffset;
        af.railBScale = railBSize;
        af.railBRotation = railBRotation;
        //af.keepRailGrounded[AutoFenceCreator.kRailBLayerInt] = railBKeepGrounded;
        af.keepRailGrounded[AutoFenceCreator.kRailBLayerInt] = false;
        af.slopeMode[AutoFenceCreator.kRailBLayerInt] = slopeModeRailB;
        af.railJointStyle[AutoFenceCreator.kRailBLayerInt] = jointStyleRailB;

        //-- Rail B Randomization
        //af.railBRandomScope = (RandomScope)railBRandomScope;
        af.railBRandomScope = RandomScope.all;
        af.allowHeightVariationRailB = allowRailBHeightVariation;
        af.minRandHeightRailB = minRailBHeightVar;
        if (af.minRandHeightRailB < af.minRailHeightLimit)
            af.minRandHeightRailB = 0.97f;
        af.maxRandHeightRailB = maxRailBHeightVar;
        if (af.maxRandHeightRailB <= af.minRailHeightLimit)
            af.maxRandHeightRailB = 1.03f;
        af.chanceOfMissingRailB = chanceOfMissingRailB;
        af.quantizeRotAngleRailB = quantizeRotAngleRailB;
        af.allowQuantizedRandomRailBRotation = allowQuantizedRandomRailBRotation;
        af.quantizeRotAxisRailB = quantizeRotAxisRailB;
        af.quantizeRotProbRailB = quantizeRotProbRailB;

        af.allowRailRandomization[kRailBLayerInt] = allowRailRandomization[kRailBLayerInt];
        af.SetSeededValuesForLayer(LayerSet.railBLayer, railBSeeds);

        //-- Rail B Variation
        af.useRailVariations[kRailBLayerInt] = useRailBVariations;
        af.scaleVariationHeightToMainHeightB = scaleVariationHeightToMainHeightB;
        af.allowIndependentSubmeshVariationB = allowIndependentSubmeshVariationB;
        af.variationModeRailB = variationModeRailB;
        af.railBSequencer.seqList = CopySequenceList(userSequenceRailB, false, AutoFenceCreator.kMaxNumSeqSteps);
        af.SetUseSequencerForLayer(LayerSet.railBLayer, useRailSequencer[kRailBLayerInt]);
        //af.seqNumSteps[kRailBLayerInt] = numUserSeqStepsRailB;
        af.optimalSequenceRailB = CopySequenceList(optimalSequenceRailB, false, 1);
        //af.railBRandRec = railBRandRec;

        af.extendRailEnds[kRailALayerInt] = extendRailEnds[kRailALayerInt];
        af.extendRailEnds[kRailBLayerInt] = extendRailEnds[kRailBLayerInt];
        af.endExtensionLength[kRailALayerInt] = endExtensionLength[kRailALayerInt];
        af.endExtensionLength[kRailBLayerInt] = endExtensionLength[kRailBLayerInt];

        //==  Extras  ==
        af.useExtrasLayer = useExtras;
        af.currentExtra_PrefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.extraPrefab, extraName, $"ScriptablePreset.BuildFromPreset({categoryName}/{name})  Extra Name ");
        af.ex.relativeMovement = relativeMovement;
        af.ex.relativeScaling = relativeScaling;
        af.ex.extraTransformPositionOffset = extraPositionOffset;
        af.ex.extraTransformScale = extraSize;
        af.ex.extraTransformRotation = extraRotation;
        af.ex.extraFreq = extraFrequency; //****
        af.ex.makeMultiArray = makeMultiArray;
        af.ex.numExtras = numExtras;
        af.ex.extrasGap = extrasGap;
        af.ex.raiseExtraByPostHeight = raiseExtraByPostHeight;
        af.ex.extrasFollowIncline = extrasFollowIncline;

        af.ex.extrasMode = (ExtrasMode)extrasMode;
        af.ex.gridWidth = extraSpawnAreaWidth;
        af.ex.numGridX = numScatterColumns;
        af.ex.numGridZ = numScatterRows;
        af.ex.scatterExtraRandPosRange = scatterExtraRandPos;
        af.ex.scatterExtraRandScaleRange = scatterExtraRandScale;
        af.ex.scatterExtraRandRotRange = scatterExtraRandRot;
        af.ex.scatterRandomStrength = scatterRandomStrength;
        af.ex.randomScaleMode = randomScaleMode;
        af.ex.flipXProb = flipXProb;
        af.ex.flipYProb = flipYProb;
        af.ex.flipZProb = flipZProb;
        af.ex.flipMode = extraRandomFlipMode;
        af.ex.extraFreqMode = (ExtraPlacementMode)extraFrequencyMode;
        af.ex.excludeExtraVarXZRotations = excludeExtraVarXZRotations;
        af.ex.extraVarsStruct = extraVarsStruct;
        af.ex.enablePrefabVars = enableChoosePrefabs;
        af.ex.finalPostMode = finalPostMode;
        af.ex.pivotPosition = pivotPosition;
        af.SetSeededValuesForLayer(LayerSet.extraLayer, extraSeeds);

        //==  SubPosts  ==
        af.currentSubpost_PrefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, subpostName, $"ScriptablePreset.BuildFromPreset({categoryName}/{name})  subpostName ");
        af.useSubpostsLayer = useSubposts;
        af.subsSpacingMode = (SubSpacingMode)subsFixedOrProportionalSpacing;
        af.subSpacing = subSpacing;
        af.subpostPositionOffset = subPositionOffset;
        af.subpostScale = subSize;
        af.subpostRotation = subRotation;
        af.forceSubsToGroundContour = forceSubsToGroundContour;
        af.keepSubsAboveGround = keepSubsAboveGround;
        af.useSubWave = useWave;
        af.useSubJoiners = useSubJoiners;
        af.subWaveFreq = frequency;
        af.subWaveAmp = amplitude;
        af.subWavePosition = wavePosition;
        af.subPostSpread = subPostSpread;
        //-- Subposts Variation
        af.useSubpostVariations = useSubpostVariations;
        af.userSequenceSubpost = CopySequenceList(userSequenceSubpost, false, AutoFenceCreator.kMaxNumSeqSteps); ;
        //af.subseqNumSteps[kPostLayerInt] = numUserSeqStepsSubpost;
        //af.subpostRandRec = subpostRandRec;
        //-- SubPosts Randomization
        af.allowHeightVariationSubpost = allowSubPostHeightVariation;
        af.minRandHeightSubpost = minSubPostHeightVar;
        af.allowRandSubpostSmallRotationVariation = allowRandSubPostRotationVariation;
        af.smallRotationAmountSubpost = smallRotationAmountSubpost;
        af.quantizeRotAngleSubpost = quantizeRotAngleSubpost;
        af.allowQuantizedRandomSubpostRotation = allowQuantizedRandomSubPostRotation;
        af.chanceOfMissingSubpost = chanceOfMissingSubPost;
        af.allowSubpostRandomization = allowSubpostRandomization;
        af.SetSeededValuesForLayer(LayerSet.subpostLayer, subpostSeeds);

        //===  Copy the SourceVariants List to the Preset  ===
        // Do this at end to make sure the rest of the preset is correctly configured
        af.postSourceVariants = UpdateSourceVariantList(postVariants, LayerSet.postLayer, af, this);// true means copy go, or reinstate from presetName
        af.railSourceVariants[kRailALayerInt] = UpdateSourceVariantList(railAVariants, LayerSet.railALayer, af, this);// true means copy go, or reinstate from presetName
        af.railSourceVariants[kRailBLayerInt] = UpdateSourceVariantList(railBVariants, LayerSet.railBLayer, af, this);
        af.subpostSourceVariants = UpdateSourceVariantList(subpostVariants, LayerSet.subpostLayer, af, this);// true means copy go, or reinstate from presetName

        af.presetNotes = notes;

        //== Random Seeds ==
        //af.rsPostSpacing = rsPostSpacing;
        //af.rsRailARand = rsRailARand;
    }

    //---------------------
    public static string FindCategoryForPreset(ScriptablePresetAFWB preset, string presetName, string menuCategorySetting, AutoFenceCreator af)
    {
        string categoryFolderName = "";
        int dirPositionInString = presetName.IndexOf('/');

        // If the preset has a category assigned
        if (preset.categoryName != "")
        {
            // If there's a '/', just take presetName part and strip the rest
            if (dirPositionInString != -1)
                preset.name = presetName.Substring(dirPositionInString + 1);
 
        }
        // If there' no category assigned
        else if (preset.categoryName == "")
        {
            //Does the presetName have a preset prefix eg. "Brick/MyWall"
            if (dirPositionInString != -1)
            {
                categoryFolderName = presetName.Remove(dirPositionInString);
                preset.categoryName = categoryFolderName = categoryFolderName.Trim();
                preset.name = presetName.Substring(dirPositionInString + 1);
            }
            else if (menuCategorySetting == "Auto")
            //If not try to auto-assign
            {
                categoryFolderName = af.AssignPresetOrPrefabCategoryByName(presetName, "");
                preset.categoryName = categoryFolderName = categoryFolderName.Trim();
            }
            else if (menuCategorySetting != "Auto")
            {
                if (menuCategorySetting != "")
                    preset.categoryName = categoryFolderName = menuCategorySetting;
                else
                    preset.categoryName = categoryFolderName = af.AssignPresetOrPrefabCategoryByName(presetName, "");
                //-- If somehow it's still failed, assign to 'Other'
                if (preset.categoryName == "")
                    preset.categoryName = "Other";
                preset.categoryName = categoryFolderName = categoryFolderName.Trim();
            }
        }
        return preset.categoryName;
    }
    //--------------
    public static bool SaveScriptablePreset(AutoFenceCreator af, ScriptablePresetAFWB preset, string savePath, bool overwrite, bool saveUserAlso = true)
    {

        //    Save New Preset
        //======================================
        if (!File.Exists(savePath))
        {
            try
            {
                AssetDatabase.CreateAsset(preset, savePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Problem in SaveScriptablePreset() " + e.ToString() + " \n");
                return false;
            }
        }
        //    Overwrite Existing
        //============================
        else
        {
            if (overwrite)
            {
                AssetDatabase.CreateAsset(preset, savePath);
            }
            else
            {
                return false;
            }
        }
        Debug.Log("AFWB Saved " + preset.categoryName + "/" + preset.name + " to " + savePath + "\n");


        //      Save to UserAssets_AFWB Folder. Create Folder if needed
        //================================================================
        //if (saveUserAlso)
        {
            var userClone = Instantiate(preset);
            string userFolderPath = af.currAutoFenceBuilderDir + "/AFWB_Presets/User Recently Saved";
            bool folderExists = AssetDatabase.IsValidFolder(userFolderPath);
            if (folderExists == false)
            {
                string guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir + "/AFWB_Presets", "User Recently Saved");
                userFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            }
            string userSavePath = userFolderPath + "/" + preset.name + ".asset";
            AssetDatabase.CreateAsset(userClone, userSavePath); 
        }

        AssetDatabase.SaveAssets();
        return true;
    }

    //-----------
    public static string CreateSaveString(AutoFenceCreator af, string name, string categoryFolderName)
    {
        if (categoryFolderName == "")
        {
            Debug.LogWarning("Empty Folder Name for " + name + "   Not saving");
            return "";
        }
        string presetsFilePath = af.currAutoFenceBuilderDir + "/AFWB_Presets";

        string presetName = name;

        if (presetName.StartsWith("_"))
            presetName = presetName.Substring(1);

        name = presetName;
        bool folderExists = false;
        //Check if a folder exists for this category
        string guid, folderPath = presetsFilePath + "/" + categoryFolderName;
        folderExists = AssetDatabase.IsValidFolder(folderPath);
        if (folderExists == false)
        {
            guid = AssetDatabase.CreateFolder(presetsFilePath, categoryFolderName);
            folderPath = AssetDatabase.GUIDToAssetPath(guid);
        }
        string savePath = folderPath + "/" + presetName + ".asset";

        return savePath;
    }

    //-----------
    public static string CreateSaveStringForFinished(AutoFenceCreator af, string path, string name, string categoryFolderName)
    {
        if (categoryFolderName == "")
        {
            Debug.LogWarning("Empty Folder Name for " + name + "   Not saving");
            return "";
        }
        string guid, folderPath;
        /*string presetsForFinishedFilePath = path;
        if (AssetDatabase.IsValidFolder(presetsForFinishedFilePath) == false)
        {
            guid = AssetDatabase.CreateFolder(af.currAutoFenceBuilderDir +  , "PresetsForFinishedFences");
            folderPath = AssetDatabase.GUIDToAssetPath(guid);
        }*/

        string presetName = name;

        if (presetName.StartsWith("_"))
            presetName = presetName.Substring(1);

        name = presetName;
        bool folderExists = false;
        //Check if a folder exists for this category
        //folderPath = presetsForFinishedFilePath + "/" + categoryFolderName;
        folderExists = AssetDatabase.IsValidFolder(path);
        if (folderExists == false)
        {
            guid = AssetDatabase.CreateFolder(path, categoryFolderName);
            folderPath = AssetDatabase.GUIDToAssetPath(guid);
        }
        string savePath = path + "/" + presetName + ".asset";

        return savePath;
    }
    //----------------------------------------
    /// <summary>Update the presetName of the main prefab for this sourceLayerList</summary>
    /// <returns>true if updated, false if wasn't necessary</returns>
    //-- It's a bit of a mess because there could be unknown edge-case naming errors in presets
    public bool ReplaceMainGoNameForLayer(string searchName, string newName, LayerSet layer, AutoFenceCreator af)
    {
        string mainPresetPrefabName = GetMainPresetGoNameForLayer(layer);

        if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
        {

            //-- All this is because we don't know the rail suffixes, so we have to check for them
            string searchNameSuffix = "";
            if (searchName.EndsWith("_Rail"))
                searchNameSuffix = "_Rail";
            else if (searchName.EndsWith("_Panel"))
                searchNameSuffix = "_Panel";

            string presetNameSuffix = "";
            if (mainPresetPrefabName.EndsWith("_Rail"))
                presetNameSuffix = "_Rail";
            else if (mainPresetPrefabName.EndsWith("_Panel"))
                presetNameSuffix = "_Panel";

            string newNameSuffix = "";
            if (newName.EndsWith("_Rail"))
                newNameSuffix = "_Rail";
            else if (newName.EndsWith("_Panel"))
                newNameSuffix = "_Panel";

            if (searchNameSuffix != "" && presetNameSuffix != "" && newNameSuffix != "")
            {
                string presetNameStripped = AutoFenceCreator.StripLayerTypeFromNameStatic(mainPresetPrefabName);
                string searchNameStripped = AutoFenceCreator.StripLayerTypeFromNameStatic(searchName);

                if (presetNameStripped == searchNameStripped)
                {
                    string newNameStripped = AutoFenceCreator.StripLayerTypeFromNameStatic(newName);
                    string newNameFixed = newNameStripped + newNameSuffix;
                    SetMainPresetGoNameForLayer(newNameFixed, layer);
                    return true;
                }
            }
            //-- Fix if a post presetName has been saved into a rail presetName
            /*else if(searchName.EndsWith("_Post") && mainPresetPrefabName.EndsWith("_Post"))
            {
                string presetNameStripped = AutoFenceCreator.StripLayerTypeFromNameStatic(mainPresetPrefabName);
                string searchNameStripped = AutoFenceCreator.StripLayerTypeFromNameStatic(searchName);
                if (presetNameStripped == searchNameStripped)
                {
                    string newNameStripped = AutoFenceCreator.StripLayerTypeFromNameStatic(newName);
                    string newNameFixed = newNameStripped + "_Panel";
                    SetMainPresetGoNameForLayer(newNameFixed, sourceLayerList);
                    return true;
                }

            }*/
        }

        else if (layer == LayerSet.postLayer && mainPresetPrefabName == searchName)
        {
            if (searchName.EndsWith("_Post") && mainPresetPrefabName.EndsWith("_Post"))
                SetMainPresetGoNameForLayer(newName, LayerSet.postLayer);
            return true;
        }
        else if (layer == LayerSet.extraLayer && mainPresetPrefabName == searchName)
        {
            SetMainPresetGoNameForLayer(newName, LayerSet.extraLayer);
            return true;
        }
        return false;
    }
    //----------------------------------------
    public string GetMainPresetGoNameForLayer(LayerSet layer)
    {
        if (layer == LayerSet.railALayer)
            return railAName;
        else if (layer == LayerSet.railBLayer)
            return railBName;
        else if (layer == LayerSet.postLayer)
            return postName;
        else if (layer == LayerSet.subpostLayer)
            return subpostName;
        else if (layer == LayerSet.extraLayer)
            return extraName;
        return "";
    }
    //----------------------------------------
    public void SetMainPresetGoNameForLayer(string name, LayerSet layer)
    {
        if (layer == LayerSet.railALayer)
            railAName = name;
        else if (layer == LayerSet.railBLayer)
            railBName = name;
        else if (layer == LayerSet.postLayer)
            postName = name;
        else if (layer == LayerSet.subpostLayer)
            subpostName = name;
        else if (layer == LayerSet.extraLayer)
            extraName = name;

    }
    /// <summary>
    /// Replaces GameObjects that match 'searchName' in the source variants for this sourceLayerList.
    /// </summary>
    /// <param presetName="oldName">The presetName of the GameObject to be replaced.</param>
    /// <param presetName="newGo">The new GameObject to replace with.</param>
    /// <returns>True if any replacements were made, false if no replacements were made. A list of which ones were replaced or not</returns>

    public (bool isDirty, List<bool> replaced) ReplaceSourceVariantGosByNameForLayer(string oldName, GameObject newGo, LayerSet layer, AutoFenceCreator af)
    {
        List<SourceVariant> sourceVariants = GetPresetSourceVariantsForLayer(layer);
        List<bool> replaced = new List<bool>();
        bool isDirty = false;

        for (int i = 0; i < sourceVariants.Count; i++)
        {
            SourceVariant sv = sourceVariants[i];
            SourceVariant.CheckSourceVariant(sv, layer, i, true, af);

            if (sv.Go.name == oldName)
            {
                Debug.Log($"Found {oldName} in SourceVariants for sourceLayerList {layer} at index {i}");
                sv.Go = newGo;
                replaced.Add(true);
                isDirty = true;
            }
            else
            {
                replaced.Add(false);
            }
        }

        // ABasicConcrete_Post
        //-- Extras use a different system, so update here also
        if (layer == LayerSet.extraLayer)
        {
            if (newGo != null)
            {
                isDirty = ReplaceExtraVarsStructByName(oldName, newGo.name);
            }
        }



        return (isDirty, replaced);
    }

    public bool ReplaceExtraVarsStructByName(string oldName, string newName)
    {
        bool isDirty = false;

        int numVars = extraVarsStruct.varNames.Count;

        for (int i = 0; i < numVars; i++)
        {
            if (extraVarsStruct.varNames[i] == oldName)
            {
                extraVarsStruct.varNames[i] = newName;
                isDirty = true;
            }
        }
        return isDirty;
    }

    //----------------------------------------
    public List<SourceVariant> GetPresetSourceVariantsForLayer(LayerSet layer)
    {
        List<SourceVariant> sourceVariants = new List<SourceVariant>();


        if (layer == LayerSet.railALayer)
            sourceVariants = railAVariants;
        else if (layer == LayerSet.railBLayer)
            sourceVariants = railBVariants;
        else if (layer == LayerSet.postLayer)
            sourceVariants = postVariants;
        else if (layer == LayerSet.subpostLayer)
            sourceVariants = subpostVariants;

        return sourceVariants;

    }


    //---------------------------
    //-- Loads all Presets from the Assets folder
    public static List<ScriptablePresetAFWB> ReadAllPresetFiles(AutoFenceCreator af, AutoFenceEditor ed, bool clearOld = true)
    {
        List<ScriptablePresetAFWB> presetList = new List<ScriptablePresetAFWB>();
        string presetFilePath = af.currPresetsDir;
        //ScriptablePresetAFWB defaultPreset = null; //-- This is the preset used for new fences

        bool mainPresetFolderExists = AssetDatabase.IsValidFolder(presetFilePath);
        if (mainPresetFolderExists == false)
        {
            Debug.LogWarning("Main AFWB Presets Folder Missing, Can't load Presets.");
            return null;
        }

        string[] directoryPaths = null, filePaths = null, userDirectoryPaths = null;

        //-- We're only expecting directories at the top level, but mop up any loose presetsEd also
        string[] firstLevelPaths = Directory.GetDirectories(presetFilePath).Concat(Directory.GetFiles(presetFilePath)).ToArray();
        foreach (string path in firstLevelPaths)
        {
            if (path.Contains("Preset") && path.EndsWith(".asset"))
            {
                ScriptablePresetAFWB preset = AssetDatabase.LoadAssetAtPath<ScriptablePresetAFWB>(path);
                if (preset != null)
                {
                    preset.categoryName = "Other";
                    //presetsEd.CheckAndRepairPreset();
                    if (preset.name == "Default Preset")
                        ed.defaultPreset = preset;
                    else
                        presetList.Add(preset);
                }
            }
        }
        // Now loop through the category subdirectories
        try
        {
            directoryPaths = Directory.GetDirectories(presetFilePath);
        }
        catch (System.Exception e)
        {
            Debug.Log("Missing Preset Category Folder." + e.ToString());
            return null;
        }
        // Now check user folder for uniques, and add the directories to the main array
        userDirectoryPaths = Directory.GetDirectories(af.currAutoFenceBuilderDir + "/UserAssets_AFWB/User_Presets");
        Array.Resize(ref directoryPaths, directoryPaths.Length + userDirectoryPaths.Length);
        Array.Copy(userDirectoryPaths, 0, directoryPaths, directoryPaths.Length - userDirectoryPaths.Length, userDirectoryPaths.Length);

        foreach (string dirPath in directoryPaths)
        {
            //Debug.Log(dirPath);
            filePaths = Directory.GetFiles(dirPath);
            foreach (string filePath in filePaths)
            {
                if (filePath.Contains("Preset") && filePath.EndsWith(".asset"))
                {
                    ScriptablePresetAFWB preset = AssetDatabase.LoadAssetAtPath(filePath, typeof(ScriptablePresetAFWB)) as ScriptablePresetAFWB;
                    if (preset == null)
                        continue;
                    ReCategoriseBasedOnDirectory(dirPath, preset);
                    FixPresetDebug(preset);

                    //-- check for bad Size vectors from obsolete presetsEd
                    for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
                    {
                        if (preset.userSequenceRailA[i].size == Vector3.zero)
                            preset.userSequenceRailA[i].size = Vector3.one;
                        if (preset.userSequenceRailB[i].size == Vector3.zero)
                            preset.userSequenceRailB[i].size = Vector3.one;
                    }
                    if (filePath.Contains("PresetsForFinishedFences"))
                    {
                        preset.categoryName = "Used in Finished Fences";
                    }
                    PresetCheckFixEd.CheckAndRepairSourceVariantsListsAllLayerForPreset(preset, af);
                    presetList.Add(preset);
                    // add a copy in a seperate [User] category
                    if (dirPath.Contains("User_Presets"))
                    {
                        PresetCheckFixEd.CheckAndRepairSourceVariantsListsAllLayerForPreset(preset, af);
                        SourceVariant.stopComplainingAboutNullGos = true;
                        ScriptablePresetAFWB userCopy = Instantiate(preset);
                        SourceVariant.stopComplainingAboutNullGos = false;
                        userCopy.categoryName = "[User]";
                        presetList.Add(userCopy);
                    }
                }
            }
        }
        return presetList;
    }

    //-------------------
    private static void ReCategoriseBasedOnDirectory(string dirPath, ScriptablePresetAFWB preset)
    {
        if (preset == null)
            return;

        var dirName = new DirectoryInfo(dirPath).Name;
        if (preset.categoryName != dirName)
            preset.categoryName = dirName;
        //Debug.Log(preset.categoryName + "  " + dirName);
    }

    //-------------------
    private static void FixPresetDebug(ScriptablePresetAFWB preset)
    {

    }

    //---------------------------
    public static List<ScriptablePresetAFWB> ReadZeroContentPresetFiles(AutoFenceCreator af)
    {
        List<ScriptablePresetAFWB> presetList = new List<ScriptablePresetAFWB>();
        string presetFolderPath = af.currAutoFenceBuilderDir + "/ZeroPrefabContentVersion";

        bool presetFolderExists = AssetDatabase.IsValidFolder(presetFolderPath);
        if (presetFolderExists == false)
        {
            Debug.LogWarning("AFWB ZeroContent Presets Folder Missing, Can't load Presets.\n");
            return null;
        }

        string presetFilePath = presetFolderPath + "/DefaultZeroContentPreset.asset";

        ScriptablePresetAFWB preset = AssetDatabase.LoadAssetAtPath(presetFilePath, typeof(ScriptablePresetAFWB)) as ScriptablePresetAFWB;
        if (preset != null)
            presetList.Add(preset);
        else
            Debug.LogWarning("DefaultZeroContentPreset missing\n");

        return presetList;
    }
    public static (List<ScriptablePresetAFWB> matchingPresets, List<string[]> matchingComponents) FindPresetsContainingSubstring(List<ScriptablePresetAFWB> presets,
        string mySubString, bool includeCategoryName = true)
    {
        string searchString = mySubString.ToLower();

        var matchingPresets = new List<ScriptablePresetAFWB>();
        var matchingComponents = new List<string[]>();

        foreach (var preset in presets)
        {
            var componentNames = new List<string>();

            if (preset.usePosts && preset.postName.ToLower().Contains(searchString))
                componentNames.Add("Post");
            if (preset.useRailsA && preset.railAName.ToLower().Contains(searchString))
                componentNames.Add("RailA");
            if (preset.useRailsB && preset.railBName.ToLower().Contains(searchString))
                componentNames.Add("RailB");
            if (preset.useSubposts && preset.subpostName.ToLower().Contains(searchString))
                componentNames.Add("Subpost");
            if (preset.useExtras && preset.extraName.ToLower().Contains(searchString))
                componentNames.Add("Extra");

            if (includeCategoryName && preset.categoryName.ToLower().Contains(searchString))
                componentNames.Add("Category");

            if (componentNames.Count > 0)
            {
                matchingPresets.Add(preset);
                matchingComponents.Add(componentNames.ToArray());
            }
        }

        return (matchingPresets, matchingComponents);
    }


}