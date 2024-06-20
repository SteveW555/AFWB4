//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using AFWB;
using static AFWB.AutoFenceCreator;

public class RandomizationEditor
{
    LayerSet kRailALayer = LayerSet.railALayer;
    LayerSet kRailBLayer = LayerSet.railBLayer;
    LayerSet kPostLayer = LayerSet.postLayer;
    LayerSet kSubpostLayer = LayerSet.subpostLayer;
    LayerSet kExtraLayer = LayerSet.extraLayer;

    AutoFenceCreator af;
    AutoFenceEditor ed;
    LayerSet layer;
    string layerWord = "";
    RandomSeededValuesAF layerSeeds;
    RandomScope randomScope = RandomScope.main;
    bool allowRandSmallRotationVariation;

    SerializedProperty allowRandProp, allowHeightProp, usingVariations, allowQuantizedProp, randRotAxisProp, quantizeRotIndexProp, quantizeRotProbProp;
    SerializedProperty minRandHeightProp, maxRandHeightProp, minSizeXZProp, maxSizeXZProp;
    SerializedProperty allowMirroring_X_Prop, allowMirroring_Y_Prop, allowMirroring_Z_Prop, mirrorXFreqRailProp, mirrorYFreqRailProp, mirrorZFreqRailProp;
    private SerializedProperty allowRandPostSmallRotationVariationProp, allowRandRailARotationVariationProp, allowRandRailBRotationVariationProp,
        allowRandSubpostRotationVariationProp, allowRandSmallRotationVariationProp;
    private SerializedProperty smallRotationAmountPostProp, smallRotationAmountRailAProp, smallRotationAmountRailBProp, smallRotationAmountSubpostProp, smallRotationAmountProp;
    private SerializedProperty chanceOfMissingProp;
    private SerializedProperty postMinRandHeightProp, postMaxRandHeightProp, railAMinHeightProp, railAMaxHeightProp, railBMinHeightProp, railBMaxHeightProp, subpostMinHeightProp, subpostMaxHeightProp;
    private SerializedProperty postXZSizeMinProp, postXZSizeMaxProp, postXZRandPosMinProp, postXZRandPosMaxProp;

    //-------------------------
    public RandomizationEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        SetupSerializedProperties();
    }

    //-----------------
    public bool SetupRandomization(LayerSet layer)
    {
        layerWord = SetupSerializedPropertiesByLayer(layer);

        this.layer = layer;
        bool rebuildPosts = false;
        bool rebuildAll = false;
        bool reseedSmallRot = false, reseedQuantRot = false;
        bool reseedHeight = false, reseedChanceMissing = false;
        bool reseedPostSizeXZ = false, reseedPostShiftXZ = false;

        EditorGUI.BeginChangeCheck();

        if (allowRandProp.boolValue == true)
        {
            GUILayout.Space(2);
            GUILayout.BeginVertical("helpbox");
        }
        else
            GUILayout.Space(6);

        if (layer == kPostLayer)
        {
            allowRandSmallRotationVariation = af.allowRandPostSmallRotationVariation;
            allowRandSmallRotationVariationProp = allowRandPostSmallRotationVariationProp;
            smallRotationAmountProp = smallRotationAmountPostProp;
        }
        else if (layer == kRailALayer)
        {
            allowRandSmallRotationVariation = af.allowRandRailARotationVariation;
            allowRandSmallRotationVariationProp = allowRandRailARotationVariationProp;
            smallRotationAmountProp = smallRotationAmountRailAProp;
        }
        else if (layer == kRailBLayer)
        {
            allowRandSmallRotationVariation = af.allowRandRailBRotationVariation;
            allowRandSmallRotationVariationProp = allowRandRailBRotationVariationProp;
            smallRotationAmountProp = smallRotationAmountRailBProp;
        }
        else if (layer == kSubpostLayer)
        {
            allowRandSmallRotationVariation = af.allowRandSubpostSmallRotationVariation;
            allowRandSmallRotationVariationProp = allowRandSubpostRotationVariationProp;
            smallRotationAmountProp = smallRotationAmountSubpostProp;
        }

        //==================================
        //-- Enable All Randomization
        //==================================
        int sliderWidth = 241, valBoxWidth = 43, titleWidth = 196, toggleWidth = 24, miniButtonWidth = 20, endOfLineSpace = 2;
        GUILayout.BeginHorizontal();
        ed.cyanBoldStyle.wordWrap = false; //TODO find where this isn't being reset to default false
        EditorGUILayout.LabelField(new GUIContent(layerWord + " Randomization", "Enable/Disable all randomization of this sourceLayerList"),
            ed.moduleHeaderLabelStyle, GUILayout.Width(145));
        EditorGUILayout.PropertyField(allowRandProp, new GUIContent("", ""), GUILayout.Width(toggleWidth));

        //-- Random Scope Toolbar
        //if (allowRandProp.boolValue == true && usingVariations.boolValue == true)
        //{
        //    GUILayout.Space(25);
        //    //-- Random Scope
        //    EditorGUILayout.LabelField(new GUIContent("Apply", "This enables you to only affect some of the components. " +
        //    "If Variations aren't enabled it will deafult to randomizing all"), GUILayout.Width(37));
        //    randomScope = GUILayout.Toolbar(randomScope, ed.randomScopeStrings, ed.smallButtonStyle7, GUILayout.Width(310));
        //}
        //GUILayout.Space(2);

        GUILayout.EndHorizontal();

        af.railARandomScope = randomScope;
        if (this.layer == kRailBLayer)
            af.railBRandomScope = randomScope;
        if (this.layer == kPostLayer)
            af.postRandomScope = randomScope;

        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }

        // Randomization is off for this sourceLayerList,  Nothing to do here!
        if (allowRandProp.boolValue == false)
        {
            return false;
        }

        GUILayout.Space(6);
        //===================================================
        //              Height Randomization 
        //===================================================

        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        string helpStr = "Randomly scale each " + layerWord + " between the min & max values.  Does not affect Rail Panels";
        string thisCaseHelpStr = "";

        //   Allow Height Randomization 
        //==============================
        EditorGUILayout.LabelField(new GUIContent(layerWord + "Height Variation", helpStr), GUILayout.Width(titleWidth));
        EditorGUILayout.PropertyField(allowHeightProp, new GUIContent("", "Randomize " + layerWord + " Height"), GUILayout.Width(toggleWidth));
        GUILayout.Space(10); //horiz space before slider

        //   Min/Max Height  
        //==============================
        if (this.layer == kPostLayer)
        {
            EditorGUILayout.LabelField("", af.minRandHeightPost.ToString("F2"), GUILayout.Width(valBoxWidth));
            EditorGUILayout.MinMaxSlider(ref af.minRandHeightPost, ref af.maxRandHeightPost, af.minPostHeightLimit, af.maxPostHeightLimit, GUILayout.Width(sliderWidth));
            EditorGUILayout.LabelField("", af.maxRandHeightPost.ToString("F2"), GUILayout.Width(valBoxWidth));
            minRandHeightProp.floatValue = af.minRandHeightPost;
            maxRandHeightProp.floatValue = af.maxRandHeightPost;
        }
        if (this.layer == kRailALayer)
        {
            EditorGUILayout.LabelField("", af.minRandHeightRailA.ToString("F2"), GUILayout.Width(valBoxWidth));
            EditorGUILayout.MinMaxSlider(ref af.minRandHeightRailA, ref af.maxRandHeightRailA, af.minRailHeightLimit, af.maxRailHeightLimit, GUILayout.Width(sliderWidth));
            EditorGUILayout.LabelField("", af.maxRandHeightRailA.ToString("F2"), GUILayout.Width(valBoxWidth));
            railAMinHeightProp.floatValue = af.minRandHeightRailA;
            railAMaxHeightProp.floatValue = af.maxRandHeightRailA;
        }
        if (this.layer == kRailBLayer)
        {
            EditorGUILayout.LabelField("", af.minRandHeightRailB.ToString("F2"), GUILayout.Width(valBoxWidth));
            EditorGUILayout.MinMaxSlider(ref af.minRandHeightRailB, ref af.maxRandHeightRailB, af.minRailHeightLimit, af.maxRailHeightLimit, GUILayout.Width(sliderWidth));
            EditorGUILayout.LabelField("", af.maxRandHeightRailB.ToString("F2"), GUILayout.Width(valBoxWidth));
            railBMinHeightProp.floatValue = af.minRandHeightRailB;
            railBMaxHeightProp.floatValue = af.maxRandHeightRailB;
        }
        if (this.layer == kSubpostLayer)
        {
            EditorGUILayout.LabelField("", af.minRandHeightSubpost.ToString("F2"), GUILayout.Width(valBoxWidth));
            EditorGUILayout.MinMaxSlider(ref af.minRandHeightSubpost, ref af.maxRandHeightSubpost, af.minSubpostHeightLimit, af.maxSubpostHeightLimit, GUILayout.Width(sliderWidth));
            EditorGUILayout.LabelField("", af.maxRandHeightPost.ToString("F2"), GUILayout.Width(valBoxWidth));
            subpostMinHeightProp.floatValue = af.minRandHeightPost;
            subpostMaxHeightProp.floatValue = af.maxRandHeightPost;
        }
        //      Re-Seed
        //=====================
        ed.SetContentAndBGColorForControlOnCondition(af.allowHeightVariationPost, ed.seedGreenTextCol, ed.seedGreen);
        if (GUILayout.Button(new GUIContent("S", "Re-Seed random height values"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
        {
            reseedHeight = true;
            rebuildAll = true;
        }
        ed.RestoreDefaultContentAndBGColorForControl();

        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            ed.serializedObject.Update();//TODO Find out why this is needed for MinMaxSlider to work, related to need for ref
            af.postAndGlobalSeeds.GenerateRandomHeightValues();
            rebuildAll = true;
        }
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (this.layer == kPostLayer)
        {
            //========================================================================
            //              Post Size (XZ) Randomization 
            //========================================================================= 

            //      Enable SizeXZ
            //========================
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Post Size XZ Variation", "Randomize Post X & Z Size - Thickness & Width  (excludes height which can be varied independently)"), GUILayout.Width(titleWidth));
            EditorGUILayout.PropertyField(ed.serializedObject.FindProperty("allowPostSizeVariation"), new GUIContent("", "Randomize Post Size excluding height"),
                GUILayout.Width(toggleWidth));
            //-- Slider & Value boxes
            GUILayout.Space(10);
            EditorGUILayout.LabelField("", af.minSizeXZPost.ToString("F2"), GUILayout.Width(valBoxWidth));

            EditorGUILayout.MinMaxSlider(ref af.minSizeXZPost, ref af.maxSizeXZPost, af.minPostSizeLimit, af.maxPostSizeLimit, GUILayout.Width(sliderWidth));

            EditorGUILayout.LabelField("", af.maxSizeXZPost.ToString("F2"), GUILayout.Width(valBoxWidth));
            postXZSizeMinProp.floatValue = af.minSizeXZPost;
            postXZSizeMaxProp.floatValue = af.maxSizeXZPost;
            if (EditorGUI.EndChangeCheck())
            {
                ed.serializedObject.ApplyModifiedProperties();
                af.postAndGlobalSeeds.GenerateRandomPostSizeXZValues();
                rebuildAll = true;
            }

            //      Re-Seed
            //=====================
            ed.SetContentAndBGColorForControlOnCondition(af.allowPostSizeVariation, ed.seedGreenTextCol, ed.seedGreen);
            if (GUILayout.Button(new GUIContent("S", "Re-Seed random Size XZ values"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
            {
                reseedPostSizeXZ = true;
                rebuildAll = true;
            }
            ed.RestoreDefaultContentAndBGColorForControl();

            GUILayout.EndHorizontal();

            //========================================================================
            //   Post XZ Random Position Shift 
            //======================================================================== 
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(new GUIContent("Post XZ Random Position Shift", "Randomly shift Position away from linear." +
                "\n\nTip: Look in the Scene View along the Fence from ground level, even an insignificant wobble from above can appear intense along its length" +
                "\n\nNote: This will also visually shift the Rail Panels as these span from Post-to-Post. " +
                "\n(Contact support if you think it would be useful to have separate controls)" +
                "\n\nDoes not affect Click-Point Post Posts, as these determine the layout of your fence, and the other components."), GUILayout.Width(titleWidth));
            //-- Switch
            EditorGUILayout.PropertyField(ed.serializedObject.FindProperty("allowPostXZShift"),
                new GUIContent("", "Randomly shift position away from linear. Does not affect Click-Point Nodes"), GUILayout.Width(toggleWidth));

            //-- Slider & Value boxes
            GUILayout.Space(10);
            EditorGUILayout.LabelField("", af.minShiftXZPost.ToString("F2"), GUILayout.Width(valBoxWidth + 4));
            EditorGUILayout.MinMaxSlider(ref af.minShiftXZPost, ref af.maxShiftXZPost, af.minPostXZShiftLimit, af.maxPostXZShiftLimit, GUILayout.Width(sliderWidth - 29));
            EditorGUILayout.LabelField("", af.maxShiftXZPost.ToString("F2"), GUILayout.Width(valBoxWidth + 3));

            if (EditorGUI.EndChangeCheck())
            {
                postXZRandPosMinProp.floatValue = af.minShiftXZPost;
                postXZRandPosMaxProp.floatValue = af.maxShiftXZPost;
                ed.serializedObject.ApplyModifiedProperties();
                af.postAndGlobalSeeds.GenerateRandomPostShiftXZ();
                af.ForceRebuildFromClickPoints();
            }

            GUILayout.Space(22);
            //      Re-Seed
            //=====================
            ed.SetContentAndBGColorForControlOnCondition(af.allowPostXZShift, ed.seedGreenTextCol, ed.seedGreen);
            if (GUILayout.Button(new GUIContent("S", "Re-Seed random Shift XZ values"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
            {
                reseedPostShiftXZ = true;
                rebuildAll = true;
            }
            ed.RestoreDefaultContentAndBGColorForControl();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        //===========================================================================
        //                      Small Random Rotations  
        //===========================================================================
        int rotTransBoxWidth = 320;
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField(new GUIContent("Small Random Rotations", "Make small random rotations. Intended as small imperfections up to 15 degrees. " +
            "For larger random rotations use Quantized Random Roations below." +
            "\n\n Values in the range 0.5 to 2 can often be enough to give subtle variation, especially for straight objects wwhere misalignment is clear."), GUILayout.Width(titleWidth));

        //-- Allow Rotation
        EditorGUILayout.PropertyField(allowRandSmallRotationVariationProp, new GUIContent(""), GUILayout.Width(24));
        GUILayout.Space(12);
        //-- Rotation Amount
        EditorGUILayout.PropertyField(smallRotationAmountProp, new GUIContent(""), GUILayout.Width(rotTransBoxWidth));


        //    Reseed Samll Rotations
        //==============================
        GUILayout.Space(12);
        ed.SetContentAndBGColorForControlOnCondition(allowRandSmallRotationVariation, ed.seedGreenTextCol, ed.seedGreen);
        if (GUILayout.Button(new GUIContent("S", "Re-Seed random Rotation values"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
        {
            reseedSmallRot = true;
            rebuildAll = true;
        }
        ed.RestoreDefaultContentAndBGColorForControl();
        GUILayout.Space(1);

        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            af.GenerateRandomSmallRotValuesForLayer(layer);

           //af.seed

            if (this.layer == kPostLayer)
                af.smallRotationAmountPost = ed.EnforceVectorMinMax(af.smallRotationAmountPost, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(15.0f, 15.0f, 15.0f));
            if (this.layer == kRailALayer)
                af.smallRotationAmountRailA = ed.EnforceVectorMinMax(af.smallRotationAmountRailA, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(15.0f, 15.0f, 15.0f));
            if (this.layer == kRailBLayer)
                af.smallRotationAmountRailB = ed.EnforceVectorMinMax(af.smallRotationAmountRailB, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(15.0f, 15.0f, 15.0f));
            if (this.layer == kSubpostLayer)
                af.smallRotationAmountSubpost = ed.EnforceVectorMinMax(af.smallRotationAmountSubpost, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(15.0f, 15.0f, 15.0f));

            rebuildAll = true;
        }
        GUILayout.Space(2);
        GUILayout.EndHorizontal();

        //===============================
        //   QUANTIZED Rotation 
        //===============================
        float quantRotProb = af.GetRandomQuantRotProbForLayer(this.layer);
        helpStr = "Rotates the piece by a random multiple of these values.\nE.g. It can be useful to lock a square post to 90 degree rotations, " +
            "for example, which would result in random rotations of either 90, 180, or 270. \nLikewise, choosing 15 could result in 15, 30, 45, 60 etc. " +
            "\n\nFixed 90 and Fixed 180 will restrict the rotations to exactly those values, rather than multiples. " +
                "\n\nConsecutive 90 will rotate in the sequence: 0, 90, 180, 270. In this mode probability is ignored. \n\nThe start and end posts are not rotated.\n\n" +
                "When the probabilty is set to zero, alternate items will be rotated. For true zero, disable Quantized Rotations";

        thisCaseHelpStr = $"Will Rotate the piece by a random multiple of {ed.quantizeRotStrings[quantizeRotIndexProp.intValue]} degrees around the {GetRotationAxisAsString(randRotAxisProp.intValue)} Axis.";

        if (quantRotProb > 0)
            thisCaseHelpStr += $"\n\nFrom the nextPos Probability setting, there is a {quantRotProb} that it will be rotated.";
        else
            thisCaseHelpStr += $"\n\nAs the nextPos Probability setting is set to Alternate (0), then alternate {af.GetLayerNameAsString(this.layer)}s will be rotated.";

        string[] axisStr = { "x", "y", "z" };

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        //-- Allow
        EditorGUILayout.LabelField(new GUIContent("Quantized Random Rotations", helpStr), GUILayout.Width(titleWidth));
        EditorGUILayout.PropertyField(allowQuantizedProp, new GUIContent("", thisCaseHelpStr), GUILayout.Width(24));

        //-- Axis
        GUILayout.Space(12);
        EditorGUILayout.LabelField(new GUIContent("Axis:", thisCaseHelpStr), GUILayout.Width(40)); //the only way to get a tooltip with a narrow popup width
        randRotAxisProp.intValue = EditorGUILayout.Popup(new GUIContent("", thisCaseHelpStr), randRotAxisProp.intValue, axisStr, GUILayout.Width(40));

        //-- Angle
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("Angle:", thisCaseHelpStr), GUILayout.Width(40)); //the only way to get a tooltip with a narrow popup width
        quantizeRotIndexProp.intValue = EditorGUILayout.Popup(quantizeRotIndexProp.intValue, ed.quantizeRotStrings, GUILayout.Width(65), GUILayout.ExpandWidth(false));

        //-- Probability
        EditorGUI.BeginDisabledGroup(quantizeRotIndexProp.intValue == 9); // Consecutive 90
        GUILayout.Space(12);
        string probStr = "Prob.";
        if (quantizeRotProbProp.floatValue == 0)
            probStr = "[Alternate]";
        //EditorStyles.label.wordWrap = true; // dunno, if this is false slider doesn't show
        EditorGUILayout.LabelField(new GUIContent(probStr, "Probability of Rotation (0 - 1.0) \n\nWhen the probabilty is set to 0, alternate items will be rotated." +
            $"\n\nIf set higher than 0 then there is a corresponding chance that the {af.GetLayerNameAsString(this.layer)} will be rotated or not." +
            $" At 1, all will be rotated." +
            "\n\nThis makes it easy to break up repetitions. \nWorks well at 'Fix 180' as most elements remain visually consistent at this angle. " +
            "For true zero, disable Quantized Rotations"), GUILayout.Width(60));
        EditorGUILayout.PropertyField(quantizeRotProbProp, new GUIContent("", "Probability of this quantized svRotation being applied." +
            "\n\nAt zero it will apply them to alternate sections."), GUILayout.Width(38));

        //    Reseed Quantized Rotations
        //===================================
        GUILayout.Space(12);
        ed.SetContentAndBGColorForControlOnCondition(allowQuantizedProp.boolValue, ed.seedGreenTextCol, ed.seedGreen);
        if (quantizeRotProbProp.floatValue > 0)
        {
            if (GUILayout.Button(new GUIContent("S", "Re-Seed random Rotation values"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
            {
                reseedQuantRot = true;
                rebuildAll = true;
            } 
        }
        ed.RestoreDefaultContentAndBGColorForControl();


        EditorGUI.EndDisabledGroup();

        GUILayout.Space(2);
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            GetRotationAnglesFromMenuIndex();
            af.EnforceRangeOfRandomQuantRotProbForLayer(this.layer);
            rebuildAll = true;
        }

        EditorGUI.BeginChangeCheck();
        //===========================
        //      Mirror
        //===========================
        if (layer.ToPrefabType() == PrefabTypeAFWB.railPrefab)
        {
            helpStr = "Visually flip the appearance along a given axis every N sections.";
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Mirror on Axis", helpStr), GUILayout.Width(177));

            EditorGUILayout.LabelField(new GUIContent("X:"), GUILayout.Width(16));
            EditorGUILayout.PropertyField(allowMirroring_X_Prop, new GUIContent(""), GUILayout.Width(16));
            EditorGUILayout.LabelField(new GUIContent("N:"), GUILayout.Width(16));
            EditorGUILayout.PropertyField(mirrorXFreqRailProp, new GUIContent(""), GUILayout.Width(30));


            GUILayout.Space(35);

            EditorGUILayout.LabelField(new GUIContent("Y:"), GUILayout.Width(16));
            EditorGUILayout.PropertyField(allowMirroring_Y_Prop, new GUIContent(""), GUILayout.Width(16));
            EditorGUILayout.LabelField(new GUIContent("N:"), GUILayout.Width(16));
            EditorGUILayout.PropertyField(mirrorYFreqRailProp, new GUIContent(""), GUILayout.Width(30));

            GUILayout.Space(35);
            EditorGUILayout.LabelField(new GUIContent("Z:"), GUILayout.Width(16));
            EditorGUILayout.PropertyField(allowMirroring_Z_Prop, new GUIContent(""), GUILayout.Width(16));
            EditorGUILayout.LabelField(new GUIContent("N:"), GUILayout.Width(16));
            EditorGUILayout.PropertyField(mirrorZFreqRailProp, new GUIContent(""), GUILayout.Width(30));
            GUILayout.Space(2);
            GUILayout.EndHorizontal();
        }

        //===========================
        //   Chance Of Missing 
        //===========================
        GUILayout.Space(7);
        sliderWidth = 200; miniButtonWidth = 20; endOfLineSpace = 2;

        GUILayout.BeginHorizontal();

        if (layer == kPostLayer)
            EditorGUILayout.LabelField(new GUIContent("Chance of Missing Posts", "Posts will be randomly omitted (except first/last post).  " +
                "\n\nSet to -1 to omit alternate Posts"), GUILayout.Width(titleWidth));
        else if (layer == kRailALayer || layer == kRailBLayer)
            EditorGUILayout.LabelField(new GUIContent("Chance of Missing Rail", "Rail will be randomly omitted.  " +
                "\n\nSet to -1 to omit alternate Rail"), GUILayout.Width(titleWidth));
        else if (layer == kSubpostLayer)
            EditorGUILayout.LabelField(new GUIContent("Chance of Missing Subposts", "Subposts will be randomly omitted.  " +
                "\n\nSet to -1 to omit alternate Subposts"), GUILayout.Width(titleWidth));

        EditorGUILayout.PropertyField(chanceOfMissingProp, new GUIContent("", "Items will be randomly omitted (except first/last post).  " +
            "\n\nSet to -1 to omit alternate items"), GUILayout.Width(50));

        if (chanceOfMissingProp.floatValue > 1)
            chanceOfMissingProp.floatValue = 1;


        ed.SetContentAndBGColorForControlOnCondition(chanceOfMissingProp.floatValue > 0, ed.seedGreenTextCol, ed.seedGreen);
        if (GUILayout.Button(new GUIContent("S", "Re-Seed Chance of Missing values"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
        {
            reseedChanceMissing = true;
            rebuildAll = true;
        }
        ed.RestoreDefaultContentAndBGColorForControl();


        GUILayout.Space(endOfLineSpace);
        GUILayout.EndHorizontal();

        if (allowRandProp.boolValue == true)
        {
            GUILayout.Space(4);
            GUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            mirrorXFreqRailProp.intValue = mirrorXFreqRailProp.intValue < 1 ? 1 : mirrorXFreqRailProp.intValue;
            mirrorYFreqRailProp.intValue = mirrorYFreqRailProp.intValue < 1 ? 1 : mirrorYFreqRailProp.intValue;
            mirrorZFreqRailProp.intValue = mirrorZFreqRailProp.intValue < 1 ? 1 : mirrorZFreqRailProp.intValue;

            ed.serializedObject.ApplyModifiedProperties();
            rebuildAll = true;
        }

        if (reseedHeight)
            layerSeeds.ReseedHeight();
        if (reseedPostShiftXZ)
            layerSeeds.ReseedPostShiftXZ();
        if (reseedPostSizeXZ)
            layerSeeds.ReseedPostSizeXZ();
        if (reseedSmallRot)
            layerSeeds.ReseedSmallRotations();
        if (reseedSmallRot)
            layerSeeds.ReseedQuantizedRotations();
        if (reseedChanceMissing)
            layerSeeds.ReseedQuantizedRotations();

        if (rebuildAll)
        {
            layerSeeds.CheckSeedValues();
            af.ForceRebuildFromClickPoints();
        }

        GUILayout.Space(1);
        return allowRandProp.boolValue; //so we know if it was enabled
    }
    //----------------------------------------------------------------
    //Sets the rotation angles for each sourceLayerList from the menu index
    public void GetRotationAnglesFromMenuIndex()
    {
        string angStr = ed.quantizeRotStrings[quantizeRotIndexProp.intValue];
        if (angStr == "Fix 90")
        {
            if (layer == kPostLayer)
                af.quantizeRotAnglePost = -90;
            if (layer == kRailALayer)
                af.quantizeRotAngleRailA = -90;
            if (layer == kRailBLayer)
                af.quantizeRotAngleRailB = -90;
            if (layer == kSubpostLayer)
                af.quantizeRotAngleSubpost = -90;

        }
        else if (angStr == "Fix 180")
        {
            if (layer == kPostLayer)
                af.quantizeRotAnglePost = -180;
            if (layer == kRailALayer)
                af.quantizeRotAngleRailA = -180;
            if (layer == kRailBLayer)
                af.quantizeRotAngleRailB = -180;
            if (layer == kSubpostLayer)
                af.quantizeRotAngleSubpost = -180;
        }
        else if (angStr == "Consecutive 90")
        {
            if (layer == kPostLayer)
                af.quantizeRotAnglePost = -1;
            if (layer == kRailALayer)
                af.quantizeRotAngleRailA = -1;
            if (layer == kRailBLayer)
                af.quantizeRotAngleRailB = -1;
            if (layer == kSubpostLayer)
                af.quantizeRotAngleSubpost = -1;
        }
        else
        {
            if (layer == kPostLayer)
                af.quantizeRotAnglePost = float.Parse(ed.quantizeRotStrings[ed.quantizeRotIndexPostProp.intValue]);//parses the number from the menu string;
            if (layer == kRailALayer)
                af.quantizeRotAngleRailA = float.Parse(ed.quantizeRotStrings[ed.quantizeRotIndexRailAProp.intValue]);
            if (layer == kRailBLayer)
                af.quantizeRotAngleRailB = float.Parse(ed.quantizeRotStrings[ed.quantizeRotIndexRailBProp.intValue]);
            if (layer == kSubpostLayer)
                af.quantizeRotAngleSubpost = float.Parse(ed.quantizeRotStrings[ed.quantizeRotIndexSubpostProp.intValue]);
        }
    }
    public string GetRotationAxisAsString(int index)
    {
        if (index == 0)
            return "x";
        if (index == 1)
            return "y";
        if (index == 2)
            return "z";

        else return "x";

    }
    //--------------------------
    // Mainly needed for the Rails which are dependent on which sourceLayerList is being edited
    private string SetupSerializedPropertiesByLayer(LayerSet layer)
    {
        allowRandProp = ed.allowPostRandomization;
        layerWord = "Post ";

        //SetupSerializedProperties();

        if (layer == kPostLayer)
        {
            randomScope = af.postRandomScope;
            randRotAxisProp = ed.randRotAxisPost;
            quantizeRotIndexProp = ed.quantizeRotIndexPostProp;
            quantizeRotProbProp = ed.quantizeRotProbPost;
            layerSeeds = af.postAndGlobalSeeds;

            allowRandProp = ed.allowPostRandomization;
            allowHeightProp = ed.serializedObject.FindProperty("allowHeightVariationPost");
            minRandHeightProp = ed.serializedObject.FindProperty("minRandHeightPost");
            maxRandHeightProp = ed.serializedObject.FindProperty("maxRandHeightPost");
            usingVariations = ed.serializedObject.FindProperty("usePostVariations");
            allowQuantizedProp = ed.serializedObject.FindProperty("allowQuantizedRandomPostRotation");
            allowRandPostSmallRotationVariationProp = ed.serializedObject.FindProperty("allowRandPostSmallRotationVariation");
            smallRotationAmountPostProp = ed.serializedObject.FindProperty("smallRotationAmountPost");
            minSizeXZProp = ed.serializedObject.FindProperty("minSizeXZPost");
            maxSizeXZProp = ed.serializedObject.FindProperty("maxSizeXZPost");
            chanceOfMissingProp = ed.serializedObject.FindProperty("chanceOfMissingPost");

            postXZSizeMinProp = ed.serializedObject.FindProperty("minSizeXZPost");
            postXZSizeMaxProp = ed.serializedObject.FindProperty("maxSizeXZPost");
            postXZRandPosMinProp = ed.serializedObject.FindProperty("minShiftXZPost");
            postXZRandPosMaxProp = ed.serializedObject.FindProperty("maxShiftXZPost");

            allowMirroring_X_Prop = ed.serializedObject.FindProperty("allowMirroring_X_Post");
            allowMirroring_Y_Prop = ed.serializedObject.FindProperty("allowMirroring_Y_Post");
            allowMirroring_Z_Prop = ed.serializedObject.FindProperty("allowMirroring_Z_Post");
            mirrorXFreqRailProp = ed.serializedObject.FindProperty("mirrorXFreqPost");
            mirrorYFreqRailProp = ed.serializedObject.FindProperty("mirrorYFreqPost");
            mirrorZFreqRailProp = ed.serializedObject.FindProperty("mirrorZFreqPost");
        }

        if (layer == kRailALayer)
        {
            layerWord = "Rail A ";
            allowRandProp = ed.serializedObject.FindProperty("allowRailRandomization").GetArrayElementAtIndex(kRailALayerInt);
            randomScope = af.railARandomScope;

            allowHeightProp = ed.serializedObject.FindProperty("allowHeightVariationRailA");
            minRandHeightProp = ed.serializedObject.FindProperty("minRandHeightRailA");
            maxRandHeightProp = ed.serializedObject.FindProperty("maxRandHeightRailA");
            usingVariations = ed.serializedObject.FindProperty("useRailAVariations");
            allowQuantizedProp = ed.serializedObject.FindProperty("allowQuantizedRandomRailARotation");
            randRotAxisProp = ed.quantizeRotAxisRailA;
            quantizeRotIndexProp = ed.quantizeRotIndexRailAProp;
            quantizeRotProbProp = ed.quantizeRotProbRailA;
            layerSeeds = af.railASeeds;

            allowMirroring_X_Prop = ed.serializedObject.FindProperty("allowMirroring_X_Rail").GetArrayElementAtIndex(kRailALayerInt);
            allowMirroring_Y_Prop = ed.serializedObject.FindProperty("allowMirroring_Y_Rail").GetArrayElementAtIndex(kRailALayerInt);
            allowMirroring_Z_Prop = ed.serializedObject.FindProperty("allowMirroring_Z_Rail").GetArrayElementAtIndex(kRailALayerInt);
            chanceOfMissingProp = ed.serializedObject.FindProperty("chanceOfMissingRailA");

            mirrorXFreqRailProp = ed.serializedObject.FindProperty("mirrorXFreqRail").GetArrayElementAtIndex(kRailALayerInt);
            mirrorYFreqRailProp = ed.serializedObject.FindProperty("mirrorYFreqRail").GetArrayElementAtIndex(kRailALayerInt);
            mirrorZFreqRailProp = ed.serializedObject.FindProperty("mirrorZFreqRail").GetArrayElementAtIndex(kRailALayerInt);
        }
        if (layer == kRailBLayer)
        {
            layerWord = "Rail B ";
            allowRandProp = ed.serializedObject.FindProperty("allowRailRandomization").GetArrayElementAtIndex(kRailBLayerInt);
            randomScope = af.railBRandomScope;
            allowHeightProp = ed.serializedObject.FindProperty("allowHeightVariationRailB");
            minRandHeightProp = ed.serializedObject.FindProperty("minRandHeightRailB");
            maxRandHeightProp = ed.serializedObject.FindProperty("maxRandHeightRailB");
            usingVariations = ed.serializedObject.FindProperty("useRailBVariations");
            allowQuantizedProp = ed.serializedObject.FindProperty("allowQuantizedRandomRailBRotation");
            randRotAxisProp = ed.quantizeRotAxisRailB;
            quantizeRotIndexProp = ed.quantizeRotIndexRailBProp;
            quantizeRotProbProp = ed.quantizeRotProbRailB;
            layerSeeds = af.railBSeeds;

            allowMirroring_X_Prop = ed.serializedObject.FindProperty("allowMirroring_X_Rail").GetArrayElementAtIndex(kRailBLayerInt);
            allowMirroring_Y_Prop = ed.serializedObject.FindProperty("allowMirroring_Y_Rail").GetArrayElementAtIndex(kRailBLayerInt);
            allowMirroring_Z_Prop = ed.serializedObject.FindProperty("allowMirroring_Z_Rail").GetArrayElementAtIndex(kRailBLayerInt);

            mirrorXFreqRailProp = ed.serializedObject.FindProperty("mirrorXFreqRail").GetArrayElementAtIndex(kRailBLayerInt);
            mirrorYFreqRailProp = ed.serializedObject.FindProperty("mirrorYFreqRail").GetArrayElementAtIndex(kRailBLayerInt);
            mirrorZFreqRailProp = ed.serializedObject.FindProperty("mirrorZFreqRail").GetArrayElementAtIndex(kRailBLayerInt);
            chanceOfMissingProp = ed.serializedObject.FindProperty("chanceOfMissingRailB");
        }
        else if (layer == kSubpostLayer)
        {
            layerWord = "Subpost ";
            randomScope = af.subpostRandomScope;
            randRotAxisProp = ed.randRotAxisSubpost;
            quantizeRotIndexProp = ed.quantizeRotIndexSubpostProp;
            quantizeRotProbProp = ed.quantizeRotProbSubpost;
            layerSeeds = af.subpostSeeds;

            allowRandProp = ed.allowSubpostRandomization;
            allowHeightProp = ed.serializedObject.FindProperty("allowHeightVariationSubpost");
            minRandHeightProp = ed.serializedObject.FindProperty("minRandHeightSubpost");
            maxRandHeightProp = ed.serializedObject.FindProperty("maxRandHeightSubpost");
            allowQuantizedProp = ed.serializedObject.FindProperty("allowQuantizedRandomSubpostRotation");
            smallRotationAmountSubpostProp = ed.serializedObject.FindProperty("smallRotationAmountSubpost");

            //allowRandPostSmallRotationVariationProp = ed.serializedObject.FindProperty("allowRandSubpostSmallRotationVariation");
            //usingVariations = ed.serializedObject.FindProperty("useSubpostVariations");
            //minSizeXZProp = ed.serializedObject.FindProperty("minSizeXZSubpost");
            //maxSizeXZProp = ed.serializedObject.FindProperty("maxSizeXZSubpost");
            chanceOfMissingProp = ed.serializedObject.FindProperty("chanceOfMissingSubpost");

            randRotAxisProp = ed.serializedObject.FindProperty("quantizeRotAxisSubpost");
        }
        else if (layer == kExtraLayer)
        {
            layerWord = "Extra ";
        }
        return layerWord;
    }
    private void SetupSerializedProperties()
    {

        allowRandRailARotationVariationProp = ed.serializedObject.FindProperty("allowRandRailARotationVariation");
        smallRotationAmountRailAProp = ed.serializedObject.FindProperty("smallRotationAmountRailA");
        allowRandRailBRotationVariationProp = ed.serializedObject.FindProperty("allowRandRailBRotationVariation");
        smallRotationAmountRailBProp = ed.serializedObject.FindProperty("smallRotationAmountRailB");
        allowRandSubpostRotationVariationProp = ed.serializedObject.FindProperty("allowRandSubpostSmallRotationVariation");
        smallRotationAmountSubpostProp = ed.serializedObject.FindProperty("smallRotationAmountSubpost");

        //minRandHeightPostProp = ed.serializedObject.FindProperty("minRandHeightPost");
        //maxRandHeightPostProp = ed.serializedObject.FindProperty("maxRandHeightPost");
        railAMinHeightProp = ed.serializedObject.FindProperty("minRandHeightRailA");
        railAMaxHeightProp = ed.serializedObject.FindProperty("maxRandHeightRailA");
        railBMinHeightProp = ed.serializedObject.FindProperty("minRandHeightRailB");
        railBMaxHeightProp = ed.serializedObject.FindProperty("maxRandHeightRailB");
        subpostMinHeightProp = ed.serializedObject.FindProperty("minRandHeightSubpost");
        subpostMaxHeightProp = ed.serializedObject.FindProperty("maxRandHeightSubpost");

        layerWord = "Subpost ";
        allowRandProp = ed.serializedObject.FindProperty("allowSubpostRandomization");
        allowHeightProp = ed.serializedObject.FindProperty("allowHeightVariationSubpost");
        minRandHeightProp = ed.serializedObject.FindProperty("minRandHeightSubpost");
        maxRandHeightProp = ed.serializedObject.FindProperty("maxRandHeightSubpost");
        allowQuantizedProp = ed.serializedObject.FindProperty("allowQuantizedRandomSubpostRotation");
    }

}