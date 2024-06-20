//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 //3.4

using AFWB;

using TCT.UIUtilities;

using UnityEditor;

using UnityEngine;

public class SubpostEditor
{
    private AutoFenceCreator af;
    private AutoFenceEditor ed;
    private PrefabAssignEditor helperEd;
    private string subpostsStr = "Subposts", subpostsHelpStr = "Click to Disable Subposts";
    private LayerSet kSubpostLayer = LayerSet.subpostLayer;

    private SerializedProperty subWaveFreqProp;
    private SerializedProperty subWaveAmpProp;
    private SerializedProperty subWavePositionProp;
    private SerializedProperty useSubWaveProp;
    private SerializedProperty useSubJoinersProp;
    private SerializedProperty subpostScaleProp;
    private SerializedProperty subpostDuplicateModeProp;
    private SerializedObject serializedObject;

    public SubpostEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor, PrefabAssignEditor help, SerializedObject serializedObj)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        helperEd = help;
        serializedObject = serializedObj;
        subWaveFreqProp = ed.serializedObject.FindProperty("subWaveFreq");
        subWaveAmpProp = serializedObject.FindProperty("subWaveAmp");
        subWavePositionProp = serializedObject.FindProperty("subWavePosition");
        useSubWaveProp = serializedObject.FindProperty("useSubWave");
        useSubJoinersProp = serializedObject.FindProperty("useSubJoiners");
        useSubWaveProp = serializedObject.FindProperty("useSubWave");
        subpostScaleProp = serializedObject.FindProperty("subpostScaleProp");
        subpostDuplicateModeProp = serializedObject.FindProperty("subpostDuplicateMode");
    }

    public void ShowSubpostEditor()
    {
        Color uiLineGreyCol = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), uiLineGreyCol);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = ed.switchGreen;
        if (af.useSubpostsLayer == false)
            GUI.backgroundColor = ed.switchRed;
        if (GUILayout.Button(new GUIContent(subpostsStr, subpostsHelpStr), GUILayout.Width(90)))
        {
            af.useSubpostsLayer = !af.useSubpostsLayer;
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.useSubpostsLayer)
                af.componentToolbar = ComponentToolbar.subposts;
        }

        GUI.backgroundColor = Color.white;
        EditorGUI.BeginDisabledGroup(af.useSubpostsLayer == false);

        //-- Enable/Disable

        if (GUILayout.Button(new GUIContent("Reset", "Reset all Subpost Scaling/Offsets/Rotations"), GUILayout.Width(44)))
        {
            af.ResetSubpostTransforms(true);
        }

        //====== Copy / Paste   ======
        if (GUILayout.Button(new GUIContent("Copy", "Copy All Post Parameters"), EditorStyles.miniButton, GUILayout.Width(42)))
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[af.currPresetIndex];
            CopyPasteAFWB.CopyComponentParametersFromScriptablePreset(preset, AFWB.LayerSet.subpostLayer);
        }
        if (GUILayout.Button(new GUIContent("Paste", "Paste All Paste Parameters"), EditorStyles.miniButton, GUILayout.Width(44)))
        {
            CopyPasteAFWB.PasteExtraParametersFromScriptablePreset(af);
        }

        ed.assetFolderLinks.Show_Prefab_Mesh_Material_InAssetFolder(LayerSet.subpostLayer, 242);

        GUILayout.EndHorizontal();

        //======  Choose Post Preset or Add Custom Prefab  ======
        helperEd.ChooseMainPrefab(kSubpostLayer);

        //if(problem)
        //GUI.backgroundColor = Color.red;

        Color slightlyDarkerPanelCol = UIUtilities.ModifyColor(UIUtilities.panelBg, -0.027f, -0.026f, -0.022f);
        GUIStyle panelStyle = UIUtilities.MakeStyle(slightlyDarkerPanelCol);

        Color myColor = new Color(0.22f, 0.22f, 0.22f);
        //GUIStyle testStyle = UIUtilities.DuplicateGUIStyle("button", backgroundColor: myColor);
        GUIStyle testStyle = UIUtilities.DuplicateGUIStyle("button", slightlyDarkerPanelCol);

        //testStyle.border = new RectOffset(-15, -15, -15, -15);
        //testStyle.padding = new RectOffset(-10, -10, -10, -10);

        GUILayout.BeginVertical(testStyle);
        //GUILayout.BeginVertical("Box");
        GUILayout.Space(3);
        
        //SubPost Spacing Mode
        //==================================

        // -- First Setup the strings depending on Mode
        string[] subModeNames = { "Fixed Number Between Posts", "Depends on Section Length", "Duplicate Post Positions Only", "Duplicate Node Positions Only" };
        int[] subModeNums = { 0, 1, 2, 3 };
        SerializedProperty subSpacingModeProp = serializedObject.FindProperty("subsSpacingMode");
        string spacingStr = "SubPosts Spacing";
        string spacingHelpStr = "For each post, equally space this number of subposts";
        if (subSpacingModeProp.intValue == 0) //-- Fixed Number Between Posts
            spacingStr = "Num Per Post Section";
        if (subSpacingModeProp.intValue == 1) //-- DistanceTCT depends
        {
            spacingHelpStr = "For each post, start by equally spacing this number of subposts, but add or remove if the distance between two posts is longer or shorter." +
                "\n\n For example, if you change the 'Post-Rail Spacing' (in Master Settings) from 3m to 6m, the approx double the amount of subposts will be created.";
        }
        if (subSpacingModeProp.intValue == 2) //-- Posts Position
            spacingHelpStr = "Place a Subpost at the same position as each post. \n\nCan be useful to add additional detail objects to each Post.";
        if (subSpacingModeProp.intValue == 3) //-- Post Positions
            spacingHelpStr = "Place a Subpost at the same position as each Clickpoint Post. \n\nCan be useful to add additional detail objects to Main Posts.";

        EditorGUI.BeginChangeCheck();

        //-- Spacing Mode
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("SubpostStr Spacing Mode", spacingHelpStr), GUILayout.Width(150));
        subSpacingModeProp.intValue = EditorGUILayout.IntPopup("", subSpacingModeProp.intValue, subModeNames, subModeNums);
        GUILayout.EndHorizontal();

        //-- Spacing Size
        GUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(subSpacingModeProp.intValue == 2);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(spacingStr, spacingHelpStr), GUILayout.Width(150));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("subSpacing"), new GUIContent("", spacingHelpStr));
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        //======  Add Subpost at Post point ========
        GUILayout.Space(4);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("addSubpostAtPostPointAlso"),
                    new GUIContent("Add Subpost at Post position also",
                        "Duplicates subpost at incoming post position. Useful when setting up a pattern of subposts and you don't need posts. Default = off"));

        GUILayout.Space(1);
        GUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            if (af.subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts)
            {// Mode 0 = Fixed, so round the number.
                af.subSpacing = Mathf.Round(af.subSpacing);
                if (af.subSpacing < 1) { af.subSpacing = 1; }
                //Debug.Log("subSpacing = " + af.subSpacing + "\n");
            }
            af.ForceRebuildFromClickPoints();
        }

        GUILayout.Space(7);
        //===================================================
        //  Set Main - Subpost Position - Size - Rotation
        //===================================================
        SetSubpostMainParameters();

        EditorGUI.BeginChangeCheck();
        //======  Force Ground,Bury ========
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        //      Follow Ground Height
        //================================
        EditorGUILayout.LabelField(new GUIContent("Follow Ground Height",
            "Subposts will will follow changes in ground height. When off, the height position will span linearly between the Posts." +
            " Useful to have off if section spans 2 high points."), GUILayout.Width(140));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("forceSubsToGroundContour"), new GUIContent("",
            "Subposts will will follow changes in ground height. When off, the height position will span linearly between the Posts. " +
            "Useful to have off if section spans 2 high points."), GUILayout.Width(30));

        //      Keep Subs Above Ground
        //================================
        if (af.forceSubsToGroundContour == false)
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField(new GUIContent("Keep Subs Above Ground",
                " When not following the ground contour, ensures that Subposts are at least visible above ground."), GUILayout.Width(150));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keepSubsAboveGround"), new GUIContent("",
                " When not following the ground contour, ensures that Subposts are at least visible above ground."), GUILayout.Width(30));
        }
        else
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField(new GUIContent("Bury Subposts in Ground", "Useful to appear sunk in"), GUILayout.Width(150));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subsGroundBurial"),
                new GUIContent("", "Useful to appear sunk in"), GUILayout.Width(30));
            GUILayout.Space(10);
        }

        //     Adapt to Surfcae slope & Tilt
        //=======================================
        SerializedProperty subpostSurfaceNormProp = serializedObject.FindProperty("adaptSubpostToSurfaceDirection");
        EditorGUILayout.LabelField(new GUIContent("Adapt to Surface Direction", "Subposts usually point straight up, but can adapt to the direction of the surface by " +
            "increasing this amount. This is only noticeable across a slope. \n\nThe Default setting is 'Off'."), GUILayout.Width(155));
        EditorGUILayout.PropertyField(subpostSurfaceNormProp, new GUIContent(""), GUILayout.Width(20));
        if (subpostSurfaceNormProp.boolValue == true)
        {
            GUILayout.Space(2);
            af.subpostSurfaceNormalAmount = EditorGUILayout.Slider(af.subpostSurfaceNormalAmount, 0, 1, GUILayout.Width(150));
            serializedObject.FindProperty("subpostSurfaceNormalAmount").floatValue = af.subpostSurfaceNormalAmount;

            //-- Reset Surface Normal
            GUILayout.Space(12);
            if (GUILayout.Button(new GUIContent("R", "Use default settings"), EditorStyles.miniButton, GUILayout.Width(20)))
            {
                subpostSurfaceNormProp.boolValue = false;
            }
        }
        GUILayout.EndHorizontal();

        //======= Sub Wave ==========
        EditorGUILayout.LabelField(new GUIContent("Use Wave Shape", "Use a Sine wave to give a pattern to the height of subposts.\n" +
            "This is often used in older decorative railing where the array of posts would form an arc or other pattern." +
            "It usually requires many Subposts to see a effect. Try Fixed Number and 12 " +
            "\n\nCheck the preset: Metal Railings Wave for an example"), GUILayout.Width(155));
        EditorGUILayout.PropertyField(useSubWaveProp);
        af.useSubWave = useSubWaveProp.boolValue;
        if (af.useSubWave)
        {
            EditorGUILayout.PropertyField(subWaveFreqProp);
            EditorGUILayout.PropertyField(subWaveAmpProp);
            EditorGUILayout.PropertyField(subWavePositionProp);
            EditorGUILayout.PropertyField(useSubJoinersProp);
        }
        //===================================================================
        //                      Duplicate Mode
        //====================================================================
        //af.subpostDuplicateMode = (DuplicateMode)EditorGUILayout.EnumPopup(af.subpostDuplicateMode, GUILayout.Width(170));
        //subpostDuplicateModeProp.intValue = (int)af.subpostDuplicateMode;

        //===================================================================
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            subpostScaleProp.vector3Value = ed.EnforceVectorMinimums(subpostScaleProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            af.ForceRebuildFromClickPoints();
        }
        //                          Subposts Randomization
        //====================================================================
        GUILayout.Space(7);
        //GUILayout.BeginVertical("Box");
        //DrawUILine(UILineDarkGrey, 0, 1, 0);
        bool randEnabled = ed.randEd.SetupRandomization(kSubpostLayer);
        //DrawUILine(UILineDarkGrey, 0, 1, 0);
        //GUILayout.EndVertical();
        //===================================================================
        //                          Subposts Variation
        //====================================================================
        /*GUILayout.Space(10);
        //GUILayout.BeginVertical("box"); // begin VERTICAL Post  Variations
        //====  Enable Variations  ====
        //GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useSubpostVariations"), GUILayout.Width(400));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ResetSubpostPool();
            af.ForceRebuildFromClickPoints();
        }
        if (af.useSubpostVariations == true)
        {
            subpostSeqEd.SetupSourcePrefabs(kSubpostLayer);
            subpostSeqEd.SetupStepSeqVariations(kSubpostLayer);
        }
        else
            //EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();*/
        GUILayout.Space(10);
    }
    //------------------------------------
    public void SetSubpostMainParameters()
    {
        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();
        EditorShowTransforms.ShowTransformEditor(LayerSet.subpostLayer, ed);

        if (EditorGUI.EndChangeCheck())
        {
            ed.subpostScaleProp.vector3Value = ed.EnforceVectorMinimums(ed.subpostScaleProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
    }
}