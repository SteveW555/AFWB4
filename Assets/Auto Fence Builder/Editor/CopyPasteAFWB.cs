//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using AFWB;
using UnityEditor;
using UnityEngine;

public class CopyPasteAFWB
{
    private AutoFenceCreator af;
    private AutoFenceEditor ed;
    private ExtrasAFWB ex;
    private PrefabAssignEditor helper;

    public static void CopyComponentParametersFromScriptablePreset(ScriptablePresetAFWB sourcePreset, AFWB.LayerSet layer)
    {
        string componentCopyName = "presetCopy_RailA";
        if (layer == AFWB.LayerSet.railBLayerSet)
            componentCopyName = "presetCopy_RailB";
        if (layer == AFWB.LayerSet.postLayerSet)
            componentCopyName = "presetCopy_Post";
        if (layer == AFWB.LayerSet.subpostLayerSet)
            componentCopyName = "presetCopy_Subpost";
        if (layer == AFWB.LayerSet.extraLayerSet)
            componentCopyName = "presetCopy_Extra";

        string pathBase = "Assets/Auto Fence Builder/UserAssets_AFWB/PresetCopies/";
        string path = pathBase + componentCopyName + ".asset";

        //If Assets/UserAssets_AFWB/PresetCopies folder doesn't exist, create it
        if (!AssetDatabase.IsValidFolder("Assets/Auto Fence Builder/UserAssets_AFWB/"))
        {
            AssetDatabase.CreateFolder("Assets/Auto Fence Builder/UserAssets_AFWB", "PresetCopies");
        }
        AssetDatabase.Refresh();
        //make a unique copy of the preset
        ScriptablePresetAFWB componentCopyNameSO = ScriptableObject.CreateInstance<ScriptablePresetAFWB>();

        EditorUtility.CopySerialized(sourcePreset, componentCopyNameSO);

        AssetDatabase.Refresh();

        // Save the copy as a new asset
        AssetDatabase.CreateAsset(componentCopyNameSO, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void PasteExtraParametersFromScriptablePreset(AutoFenceCreator af)
    {
        ScriptablePresetAFWB presetCopy = (ScriptablePresetAFWB)AssetDatabase.LoadAssetAtPath("Assets/Auto Fence Builder/UserAssets_AFWB/PresetCopies/presetCopy.asset", typeof(ScriptablePresetAFWB));

        //==  Extras  ==
        af.useExtrasLayer = presetCopy.useExtras;
        af.currentExtra_PrefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.extraPrefab, presetCopy.extraName);
        af.ex.relativeMovement = presetCopy.relativeMovement;
        af.ex.relativeScaling = presetCopy.relativeScaling;
        af.ex.extraTransformPositionOffset = presetCopy.extraPositionOffset;
        af.ex.extraTransformScale = presetCopy.extraSize;
        af.ex.extraTransformRotation = presetCopy.extraRotation;
        af.ex.extraFreq = presetCopy.extraFrequency; //****
        af.ex.makeMultiArray = presetCopy.makeMultiArray;
        af.ex.numExtras = presetCopy.numExtras;
        af.ex.extrasGap = presetCopy.extrasGap;
        af.ex.raiseExtraByPostHeight = presetCopy.raiseExtraByPostHeight;
        af.ex.extrasFollowIncline = presetCopy.extrasFollowIncline;

        af.ex.extrasMode = (ExtrasMode)presetCopy.extrasMode;
        af.ex.gridWidth = presetCopy.extraSpawnAreaWidth;
        af.ex.numGridX = presetCopy.numScatterColumns;
        af.ex.numGridZ = presetCopy.numScatterRows;
        af.ex.scatterExtraRandPosRange = presetCopy.scatterExtraRandPos;
        af.ex.scatterExtraRandScaleRange = presetCopy.scatterExtraRandScale;
        af.ex.scatterExtraRandRotRange = presetCopy.scatterExtraRandRot;
        af.ex.scatterRandomStrength = presetCopy.scatterRandomStrength;
        af.ex.randomScaleMode = presetCopy.randomScaleMode;
        af.ex.flipXProb = presetCopy.flipXProb;
        af.ex.flipYProb = presetCopy.flipYProb;
        af.ex.flipZProb = presetCopy.flipZProb;
        af.ex.flipMode = (FlipMode)presetCopy.extraRandomFlipMode;
        af.ex.extraFreqMode = (ExtraPlacementMode)presetCopy.extraFrequencyMode;
        af.ex.excludeExtraVarXZRotations = presetCopy.excludeExtraVarXZRotations;
        af.ex.extraVarsStruct = presetCopy.extraVarsStruct;
        af.ex.enablePrefabVars = presetCopy.enableChoosePrefabs;
        af.ex.finalPostMode = presetCopy.finalPostMode;
        af.ex.pivotPosition = presetCopy.pivotPosition;
    }
}