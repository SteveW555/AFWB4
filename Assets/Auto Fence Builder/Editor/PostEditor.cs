//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 //3.4
using AFWB;
using UnityEditor;
using UnityEngine;
using static AFWB.AutoFenceCreator;

public class PostEditor
{
    private AutoFenceCreator af;
    private AutoFenceEditor ed;
    private ExtrasAFWB ex;
    private PrefabAssignEditor helper;
    private SerializedProperty exProp;
    private string postsStr = "Posts", postsHelpStr = "Click to Disable Posts";

    public PostEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor, PrefabAssignEditor help)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        helper = help;
    }

    //===================================
    //serializedObject here refers to the AutoFEnceEditor's SO (which is AutoFenceCreator)
    public void ShowPostEditor(SerializedObject serializedObject)
    {
        //      Enable/Disable
        //========================
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = ed.switchGreen;
        if (af.usePostsLayer == false)
            GUI.backgroundColor = ed.switchRed;
        if (GUILayout.Button(new GUIContent(postsStr, "Enable/Disable Posts \n When Disabled they still act as node positions for the layout."), GUILayout.Width(70)))
        {
            af.usePostsLayer = !af.usePostsLayer;
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.usePostsLayer)
                af.componentToolbar = ComponentToolbar.posts;
        }
        GUI.backgroundColor = Color.white;

        //======================== 
        //      Reset
        //========================
        GUILayout.Space(2);
        if (GUILayout.Button(new GUIContent("Reset", "Resets all Post Parameters\nUseful when troubleshooting" +
            "\nControl-click to optionally set the Prefab to a basic Brick Post"), GUILayout.Width(80)))
        {
            ResetAllPostParameters(Event.current.control);
        }
        GUILayout.Space(20);
        //      Copy / Paste
        //========================
        if (GUILayout.Button(new GUIContent("Copy", "Copy All Post Parameters"), EditorStyles.miniButton, GUILayout.Width(42)))
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[af.currPresetIndex];
            CopyPasteAFWB.CopyComponentParametersFromScriptablePreset(preset, AFWB.LayerSet.postLayerSet);
        }
        if (GUILayout.Button(new GUIContent("Paste", "Paste All Paste Parameters"), EditorStyles.miniButton, GUILayout.Width(44)))
        {
            CopyPasteAFWB.PasteExtraParametersFromScriptablePreset(af);
        }

        //      All Posts Disabled Warning
        //=====================================
        if (af.usePostVariations == true)
        {
            bool allPostsDisabled = SequenceEditor.AreAllSeqItemsDisabledForLayer(af.postSequencer, af.GetNumSectionsBuiltForLayer(LayerSet.postLayerSet));
            if (allPostsDisabled == true && af.usePostVariations == true)
                EditorGUILayout.LabelField(" [All steps are disabled!  Re-enable in Variations]", ed.warningStyle, GUILayout.Width(320));
        }
        //      Show Asset Folder Links
        //=====================================
        ed.assetFolderLinks.Show_Prefab_Mesh_Material_InAssetFolder(LayerSet.postLayerSet, 204);

        GUILayout.EndHorizontal();
        GUILayout.Space(1);

        //===================================================================================
        //
        //                       Choose Post Prefab and Aletrnatives
        //
        //===================================================================================
        GUILayout.BeginVertical("Box");
        helper.ChooseMainPrefab(LayerSet.postLayerSet);
        GUILayout.EndVertical();

        //==============================================================================
        //           Post Main Parameters - Raise, Size, Position, Rotation
        //==============================================================================

        //  Post Raise/Lower
        //===========================
        GUILayout.Space(7);
        int minButtonWidth = 20;
        //GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
        {
            EditorGUILayout.LabelField(new GUIContent("Post Raise/Lower",
                    "Use this to move the post up and down visually. \n\n" +
                    "This can also be useful to sink elements in the ground a little. " +
                    "When Building on a steep slope, for example, one side of a large post may be above the ground compared to the other side.\n\n" +
                    "You can choose if the other components follow this offset with 'Rails Follow'.\n\n" +
                    "There isn't a Position Offset X/Y/Z/ as the Post's X & Z position are determined entirely by the click-point nodes." +
                    "\n\n A similar effect can be achieved in Globals/Scale & Raise/Lower.\n\nDefault setting is 0"),
                GUILayout.Width(225));

            //==  Slider  ==
            EditorGUILayout.PropertyField(ed.postHeightOffset, new GUIContent(""), GUILayout.Width(218));

            //    Rails Follow Height
            //===========================
            GUILayout.Space(16);
            EditorGUILayout.LabelField(new GUIContent("Rails Follow", "With this enabled, Rails will be raised" +
                " or lowered by the same amount as 'Post Raise/Lower'.\nDefault setting is 'Off'"), GUILayout.Width(82));
            EditorGUILayout.PropertyField(ed.serializedObject.FindProperty("allFollowPostRaiseLower"), new GUIContent(""), GUILayout.Width(20));

            //    Reset
            //===================
            if (GUILayout.Button(new GUIContent("R", "Reset Post height offset to default zero"), EditorStyles.miniButton, GUILayout.Width(minButtonWidth)))
            {
                ed.postHeightOffset.floatValue = af.postHeightOffset = 0;
                af.allFollowPostRaiseLower = false;
            }
            GUILayout.Space(2);
        }
        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            if (af.allFollowPostRaiseLower)
                af.keepRailGrounded[kRailALayerInt] = af.keepRailGrounded[kRailBLayerInt] = false;

            af.ForceRebuildFromClickPoints();
        }

        //===========================
        //  Transforms
        //===========================
        GUILayout.Space(2);
        EditorGUI.BeginChangeCheck();
        EditorShowTransforms.ShowTransformEditor(LayerSet.postLayerSet, ed);

        GUILayout.Space(10);

        if (EditorGUI.EndChangeCheck())
        {
            ed.postSizeProp.vector3Value = ed.EnforceVectorMinimums(ed.postSizeProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.mainPostsSizeBoostProp.vector3Value = ed.EnforceVectorMinimums(ed.mainPostsSizeBoostProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.endPostsSizeBoostProp.vector3Value = ed.EnforceVectorMinimums(ed.endPostsSizeBoostProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }

        //========================
        //     Inbetween Posts
        //========================
        GUILayout.Space(4);
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        EditorStyles.label.wordWrap = false;
        int miniButtonWidth = 20, toggleWidth = 18, endOfLineSpace = 2, titleWidth = 195;
        string helpStr = "Add interpolated postsPool between the main click points." +
            "\nAFWB will position them as evenly as possible given your Click-points.\n You can adjust this by moving your Click-point nodes " +
            "or Inserting extra ones using Control-Shift-Click. \n\n* Note that using Smoothing will also add extra necessary postsPool not disabled here." +
            "\n\n You will nearly always want this to be on as they determine the position and spacing of the panels. " +
            "\n Use 'Hide In-between to just hide them visually." +
            "\n\nThe Default setting is 'On'.";

        EditorGUILayout.LabelField(new GUIContent("In-Between Posts", helpStr), GUILayout.Width(titleWidth));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("interpolate"), new GUIContent(""), GUILayout.Width(toggleWidth));

        EditorGUI.BeginDisabledGroup(serializedObject.FindProperty("interpolate").boolValue == false);

        //  Keep Grounded Switch
        //===========================

        GUILayout.Space(10); //horiz space before "Keep Grounded"
        EditorGUILayout.LabelField(new GUIContent("   Keep Grounded", "Ensure in-between postsPool are forced to the ground. Default On." +
            "\n\nIf using a wall to span between two high points, bridge-style, or creating a ramp between two poins, it may be useful to disable this"), GUILayout.Width(105));
        EditorGUILayout.PropertyField(ed.keepInterpolatedPostsGrounded, new GUIContent(""), GUILayout.Width(toggleWidth));

        // Hide Interpolated Posts
        //===========================
        GUILayout.Space(12);
        EditorGUILayout.LabelField(new GUIContent("Hide In-between", "Posts will only display at your Main ClickPoint Post Positions, and not the inbetween postsPool. " +
            "\nThe hidden postsPool will still determine the length and position of the rails that span them"),
            GUILayout.Width(100));
        EditorGUILayout.PropertyField(ed.hideInterpolated, new GUIContent(""), GUILayout.Width(toggleWidth));

        //      Reset
        //=================
        if (GUILayout.Button(new GUIContent("R", "Set defaults for Inbetween post settings"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
        {
            serializedObject.FindProperty("interpolate").boolValue = true;
            ed.keepInterpolatedPostsGrounded.boolValue = true;
            ed.hideInterpolated.boolValue = false;
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(endOfLineSpace);
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        //==========================
        //  Rotate Corner Posts
        //==========================
        EditorGUI.BeginChangeCheck();
        //-- Rotate Corner Posts to Match Direction
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Rotate Post to corner direction", "Posts orient to the average " +
            "of the incoming and outgoing section directions. \nDefault is On. \nWhen off, Post will match outgoing direction."), GUILayout.Width(titleWidth));
        EditorGUILayout.PropertyField(ed.lerpPostRotationAtCorners, new GUIContent(""), GUILayout.Width(toggleWidth));

        //-- Reset
        /* GUILayout.Space(16);
         if (GUILayout.Button(new GUIContent("R", "Use default settings"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
         {
             ed.lerpPostRotationAtCorners.boolValue = true;
             ed.lerpPostRotationAtCornersInters.boolValue = true;
         }
         GUILayout.Space(endOfLineSpace);
         GUILayout.EndHorizontal();
         if (EditorGUI.EndChangeCheck())
         {
             serializedObject.ApplyModifiedProperties();
             af.ResetPoolForLayer(LayerSet.postLayerSet);
             af.ForceRebuildFromClickPoints();
         }*/
        //============================
        //  Mitre Width  & Surface Direction
        //============================
        //EditorGUI.BeginChangeCheck();
        //-- Mitre Stretch
        // GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Stretch Post Width At Mitre Joint", "The width of a wall across a corner is greater owing to the " +
            "angle of the end piece (the hypotenuse). \nThis will stretch the post to fit. " +
            "This is usually only needed when the wall/fence is quite thick and you want to match the thickness"), GUILayout.Width(titleWidth));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stretchPostWidthAtMitreJoint"), new GUIContent(""), GUILayout.Width(toggleWidth));

        //-- Adapt to Surface for v4.0
        /*GUILayout.Space(19);
        SerializedProperty surfaceNormProp = serializedObject.FindProperty("adaptPostToSurfaceDirection");
        EditorGUILayout.LabelField(new GUIContent("Adapt to Surface Direction", "Posts usually point straight up, but can adapt to the direction of the surface by " +
            "increasing this amount. This is only noticeable across a slope. \n\nThe Default setting is 'Off'."), GUILayout.Width(163));
        EditorGUILayout.PropertyField(surfaceNormProp, new GUIContent(""), GUILayout.Width(toggleWidth));
        if (surfaceNormProp.boolValue == true)
        {
            af.postSurfaceNormalAmount = EditorGUILayout.Slider(af.postSurfaceNormalAmount, 0, 1,GUILayout.Width(147));
            serializedObject.FindProperty("postSurfaceNormalAmount").floatValue = af.postSurfaceNormalAmount;
        }
        //-- Reset Surface Normal
        GUILayout.Space(2);
        if (GUILayout.Button(new GUIContent("R", "Use default settings"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
        {
            serializedObject.FindProperty("stretchPostWidthAtMitreJoint").boolValue = false;
            surfaceNormProp.boolValue = false;
        }*/
        //GUILayout.Space(endOfLineSpace);
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(LayerSet.postLayerSet);
            af.ForceRebuildFromClickPoints();
        }


        //EditorGUI.EndDisabledGroup();
    }

    private void ResetAllPostParameters(bool controlKey)
    {
        af.ResetPostTransforms(true);
        af.endPostsSizeBoost = Vector3.one;
        af.mainPostsSizeBoost = Vector3.one;
        af.allowEndPostsPrefabOverride = false;
        af.allowNodePostsPrefabOverride = false;
        ed.postHeightOffset.floatValue = af.postHeightOffset = 0;
        af.allFollowPostRaiseLower = false;

        af.interpolate = true;
        af.hideInterpolated = false;
        af.keepInterpolatedPostsGrounded = true;
        af.lerpPostRotationAtCorners = true;
        af.stretchPostWidthAtMitreJoint = false;
        af.allowPostRandomization = false;
        af.usePostVariations = false;


        if (controlKey)
        {
            int prefabIndex = af.GetPrefabIndexForLayerByName(LayerSet.postLayerSet, "Brick_Square1_Post");
            if (prefabIndex == -1)
                prefabIndex = af.GetPrefabIndexForLayerByName(LayerSet.postLayerSet, "ABasicConcrete_Post");
            if (prefabIndex == -1)
                prefabIndex = 0;
            af.SetCurrentPrefabIndexForLayer(LayerSet.postLayerSet, prefabIndex);
            af.SetMenuIndexFromPrefabIndexForLayer(prefabIndex, LayerSet.postLayerSet);

        }

    }

    //==================================================================
    //                   Show Rails Variation Header
    //==================================================================
    public void ShowSetupVariationsSources(SerializedObject serializedObject)
    {
        //===================================================================
        //                          Posts Variation
        //====================================================================

        //====  Enable Variations  ====
        GUILayout.Space(10);

        GUILayout.BeginVertical("helpbox");

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUILayout.Space(9);

        ed.showPostVariations.boolValue = EditorGUILayout.Foldout(ed.showPostVariations.boolValue, "");

        GUILayout.Space(104);

        EditorGUILayout.LabelField(new GUIContent("Use Post Variations "), ed.cyanBoldStyle, GUILayout.Width(150));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("usePostVariations"), new GUIContent(""), GUILayout.Width(20));

        GUILayout.Space(30);
        Sequencer sequencer = af.postSequencer;
        if (af.usePostVariations == true && af.GetUseSequencerForLayer(LayerSet.postLayerSet) == true)
            EditorGUILayout.LabelField(new GUIContent("[ Seq In Use ]"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(80));


        GUILayout.Space(290);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            /*if (af.nonNullPostVariants.Count == 1)
            {
                af.postSourceVariants[1].Go = af.postPrefabs[af.currentPost_PrefabIndex];
            }*/
            af.ResetPoolForLayer(LayerSet.postLayerSet);
            af.ForceRebuildFromClickPoints();
        }
        if (ed.showPostVariations.boolValue)
        {
            using (new EditorGUI.DisabledScope(af.usePostVariations == false))
            {
                //===============  Setup Sources  ==========================
                ed.varEd.SetupSourcePrefabs(LayerSet.postLayerSet);
                ed.seqEd.SetupStepSeqVariations(LayerSet.postLayerSet);

                ed.DrawUILine(ed.UILineGrey, 6, 10, 2, 10);
            }
        }
        GUILayout.EndVertical();
    }
}