////#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
////#pragma warning disable 0414 //3.4
using AFWB;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//using UnityEditor;
using static AFWB.AutoFenceCreator;

//using AFWB;

public class RailEditor
{
    private const LayerSet kRailALayer = LayerSet.railALayerSet;
    private const LayerSet kRailBLayer = LayerSet.railBLayerSet;
    private AutoFenceCreator af;
    private AutoFenceEditor ed;
    private ExtrasAFWB ex;
    private PrefabAssignEditor choosePrefabEd;
    private SerializedProperty exProp;

    //GUIStyle unityBoldLabel = new GUIStyle(EditorStyles.label);
    private string postsStr = "Posts", postsHelpStr = "Click to Disable Posts";

    private string[] spreadModeStr;

    public RailEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor, PrefabAssignEditor help)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        choosePrefabEd = help;
        //ex = extrasPool;
    }

    //===================================
    //serializedObject here refers to the AutoFEnceEditor's SO (which is AutoFenceCreator)
    public void ShowRailEditor(SerializedObject serializedObject, LayerSet layer)
    {
        spreadModeStr = new string[] { "Total", "Per Rail", "Gap" };
        //=====  Set up the variables depending on which layer it is  =====

        int railLayerIndex = kRailALayerInt;
        if (layer == LayerSet.railBLayerSet)
            railLayerIndex = kRailBLayerInt;
        string layerName = af.GetLayerNameAsString(layer);

        string railStr = "Rail A";
        LayerSet railLayer = kRailALayer;
        SerializedProperty keepGroundedProp = serializedObject.FindProperty("keepRailGrounded").GetArrayElementAtIndex(railLayerIndex);
        SerializedProperty jointStyleProp = serializedObject.FindProperty("railJointStyle").GetArrayElementAtIndex(railLayerIndex);
        SerializedProperty numStackedRailsProp = serializedObject.FindProperty("numStackedRails").GetArrayElementAtIndex(railLayerIndex);
        SerializedProperty railSpreadProp = serializedObject.FindProperty("railSpread").GetArrayElementAtIndex(railLayerIndex);
        SerializedProperty railSpreadModeProp = serializedObject.FindProperty("railSpreadMode").GetArrayElementAtIndex(railLayerIndex);

        if (layer == LayerSet.railBLayerSet)
        {
            railLayerIndex = AutoFenceCreator.kRailBLayerInt;
            railStr = "Rail B";
            railLayer = kRailBLayer;
        }

        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = af.useRailLayer[railLayerIndex]? ed.switchGreen : ed.switchRed;

        //====================
        //   Enable/Disable
        //====================
        if (GUILayout.Button(new GUIContent(railStr, "Enable/Disable" + railStr), GUILayout.Width(70)))
        {
            af.useRailLayer[railLayerIndex] = !af.useRailLayer[railLayerIndex];
            af.ForceRebuildFromClickPoints();
        }

        GUI.backgroundColor = Color.white;
        GUI.contentColor = new Color(.9f, .9f, .999f); ;
        //GUI.backgroundColor = new Color(.7f, .7f, .8f);

        //     Reset
        //=================
        if (GUILayout.Button(new GUIContent("Reset All", "Reset all Rail Parameters\n Optionally hold Control to also rest prefab to default wall."),
            EditorStyles.miniButton, GUILayout.Width(87)))
        {

            af.ResetRail(railLayer, Event.current.control, true);
        }

        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;

        //    Copy / Paste  
        //=====================
        GUILayout.Space(20);
        if (GUILayout.Button(new GUIContent("Copy", "Copy All Post Parameters"), EditorStyles.miniButton, GUILayout.Width(42)))
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[af.currPresetIndex];
            CopyPasteAFWB.CopyComponentParametersFromScriptablePreset(preset, layer);
        }
        if (GUILayout.Button(new GUIContent("Paste", "Paste All Paste Parameters"), EditorStyles.miniButton, GUILayout.Width(44)))
        {
            CopyPasteAFWB.PasteExtraParametersFromScriptablePreset(af);
        }

        //========================
        //   Show Linked Asset
        //========================
        ed.assetFolderLinks.Show_Prefab_Mesh_Material_InAssetFolder(layer, horizSpaceOffset:196);

        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        EditorGUI.BeginDisabledGroup(af.useRailLayer[railLayerIndex] == false);
        GUILayout.Space(1);

        //===========================================================================================================================
        //                                              Setup Choose Prefab
        //===========================================================================================================================
        GUILayout.BeginVertical("Box");
        MeshCollider userMeshCol = choosePrefabEd.ChooseMainPrefab(railLayer);
        GUILayout.EndVertical();

        //==========================================
        //      Rail Reset Buttons
        //===========================================
        /*GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        //-- Centralise Y
        if (GUILayout.Button(new GUIContent("Central Y", "Centralise " + railStr), EditorStyles.miniButton,
            GUILayout.Width(70)))
            af.CentralizeRails(railLayer);
        //-- Ground
        if (GUILayout.Button(new GUIContent("Ground", "Place lowest " + railStr + "flush with ground.\nTo lock there, use [Lock To Ground]"),
            EditorStyles.miniButton, GUILayout.Width(60)))
            af.GroundRails(railLayer);*/
       

        //======================
        //   Lock To Ground
        //======================
       /* GUILayout.Space(215);
        string lockStr = " Lock to Ground ";
        if (af.keepRailGrounded[railLayerIndex])
        {
            lockStr = "Locked to Ground";
            GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f, 0.5f);
        }
        else
            GUI.backgroundColor = Color.clear;
        //GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

        EditorGUI.BeginDisabledGroup(af.globalLevelling == true);
        string lockHelpStr = "Locks the rail to ground level.\nIf you just want to set it to ground level without locking " +
                "use the [Ground] button to the left, or set Position Offset Y to 0 below. \n\nThis option is disabled when using Global Levelling";
        if (GUILayout.Button(new GUIContent(lockStr, lockHelpStr), EditorStyles.miniButtonMid, GUILayout.Width(101)))
        {
            keepGroundedProp.boolValue = !(keepGroundedProp.boolValue);
        }
        GUI.backgroundColor = Color.white;

        if (af.keepRailGrounded[railLayerIndex] == false && af.allFollowPostRaiseLower && af.postHeightOffset != 0)
        {
            EditorGUILayout.LabelField(new GUIContent("(Y Pos follows Posts)", "In the Post controls, the option to have Rails follow their" +
                " elevation offset is enabled. \nDisable 'Rails Follow' in Posts Controls, or click X: "), ed.lightGrayStyle, GUILayout.Width(105));
            if (GUILayout.Button(new GUIContent("x"), EditorStyles.miniButtonMid, GUILayout.Width(18), GUILayout.Height(14)))
            {
                serializedObject.FindProperty("allFollowPostRaiseLower").boolValue = af.allFollowPostRaiseLower = false;
            }
        }
        EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(3);
        GUILayout.EndHorizontal();*/
        GUILayout.Space(6);

        //===============================================================
        //      Main Parameters - Transform Box Raise, Size, Position, Rotation
        //===============================================================

        bool rebuild = false;

        EditorGUI.BeginChangeCheck();
        //  Transform Box
        //==================
        EditorShowTransforms.ShowTransformEditor(railLayer, ed);
        if (EditorGUI.EndChangeCheck())
            rebuild = true;

        if (rebuild)
        {
            ed.railASizeProp.vector3Value = ed.EnforceVectorMinimums(ed.railASizeProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            ed.railBSizeProp.vector3Value = ed.EnforceVectorMinimums(ed.railBSizeProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));

            //If we changed the scale, Resize the Colliders to fit the new svSize
            List<Transform> rails = ed.af.railsAPool;
            if (railLayer == LayerSet.railBLayerSet)
                rails = ed.af.railsBPool;

            for (int i = 0; i < rails.Count; i++)
            {
                if (rails[i] != null)
                {
                    BoxCollider bc = rails[i].GetComponent<BoxCollider>();
                    if (bc != null)
                    {
                        bc.size = ed.railASizeProp.vector3Value;
                    }
                }
            }
            ed.serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        //}

        //=================================================
        //           JointStyle, Slope & Hide
        //=================================================
        GUILayout.Space(10); GUILayout.Space(10);
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            GUILayout.BeginHorizontal();
            //=====  Slope Mode  =====
            SlopeMode oldSlopeMode = af.slopeMode[railLayerIndex];
            string[] slopeModeNames = { "Normal Slope", "Stepped", "Sheared" };
            EditorGUILayout.LabelField(new GUIContent("Slope Mode", "Default is shear"), GUILayout.Width(70));
            af.slopeMode[railLayerIndex] = (SlopeMode)EditorGUILayout.Popup("", (int)af.slopeMode[railLayerIndex], slopeModeNames, GUILayout.Width(95));
            serializedObject.FindProperty("slopeMode").GetArrayElementAtIndex(railLayerIndex).intValue = (int)af.slopeMode[railLayerIndex];

            //=====  Joint Style  =====
            EditorGUILayout.LabelField(new GUIContent("     Joint Style", "Simple:  Sections are unmodified at corners." +
                "\n\nOverlap:  Sections are increased in length to close the gap at corners" +
                "\n\nMitre:  The outside edges of corner section are stretched, and the inner edges shortened to meet without overlap"), GUILayout.Width(77));
            af.railJointStyle[railLayerIndex] = (JointStyle)EditorGUILayout.EnumPopup(af.railJointStyle[railLayerIndex], GUILayout.Width(65));
            jointStyleProp.enumValueIndex = (int)af.railJointStyle[railLayerIndex]; //in case af.jointStyleRailA has been changed by a preset change

            //ShowAdjustUVs();//-- For v4.0

            //=====  AutoHide  =====
            EditorGUILayout.LabelField(new GUIContent("     AutoHide Collided", "Hide if rail goes through ground or other objects"),
                GUILayout.Width(120));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoHideBuriedRails"), new GUIContent(""),
                GUILayout.Width(30));

            //=====  Rotate Pivot  =====
            af.rotateFromBaseRailA = EditorGUILayout.ToggleLeft(new GUIContent("Base Rotation", "Default off, = Rotate rails from centre. " +
                "\nOn = Rotate from base "),
            af.rotateFromBaseRailA);
            GUILayout.EndHorizontal();
            //ShowAdaptToSurfaceRotation();//-- For v4.0
            if (check.changed)
            {
                serializedObject.ApplyModifiedProperties();
                af.ForceRebuildFromClickPoints();
            }
            if (af.railJointStyle[railLayerIndex] == JointStyle.mitre && Mathf.Abs(af.railARotation.y) > 7)
            {
                EditorGUILayout.HelpBox("Not possible to Mitre unaligned Rails. Disable Mitre Joints or set Rail Rotation.y to 0", MessageType.Warning);
            }
        }

        //=================================================
        //           Stacked Rails
        //=================================================
        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();

        // Num Stacked Rails Slider
        //===========================

        EditorGUILayout.LabelField(new GUIContent("Num Stacked Rails:", ""), GUILayout.Width(126));
        EditorGUILayout.PropertyField(numStackedRailsProp, new GUIContent(""), GUILayout.Width(140));

        if ((int)numStackedRailsProp.floatValue > 1)
        {
            // Spread Slider
            //==================
            GUILayout.Space(15);
            EditorGUILayout.LabelField(new GUIContent(af.GetLayerNameAsString(layer) + " Spread: ",
            "This is the space between each vertically stacked Rail Panel.\n\nIf set To Total this will be the total height that all of them occupy" +
            "\n\nPer Rail will set the distance between each Rail Panel."), GUILayout.Width(90));
            EditorGUILayout.PropertyField(railSpreadProp, new GUIContent(""), GUILayout.Width(140));

            // Spread Mode Popup
            //=====================
            GUILayout.Space(5);
            railSpreadModeProp.intValue = EditorGUILayout.Popup(railSpreadModeProp.intValue, spreadModeStr, GUILayout.Width(66));

            // Stack Perfect
            //=====================
            // TODO:
        }
        GUILayout.EndHorizontal();

        //===============================
        //         Extend Ends
        //===============================
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        // Switch
        //==============
        EditorGUILayout.LabelField(new GUIContent("Extend Ends", "Extends the length of the first and last section by the specified amount" +
            "\n\nUseful, for example, if a wall should envelop or overlap the first/last Post,  rather than starting at the Post's centre position."), GUILayout.Width(85));

        SerializedProperty extendEndsProp = serializedObject.FindProperty("extendRailEnds").GetArrayElementAtIndex(railLayerIndex);
        EditorGUILayout.PropertyField(extendEndsProp, new GUIContent(""), GUILayout.Width(35));

        //  Length
        //==============
        if (af.extendRailEnds[railLayerIndex] == true)
        {
            EditorGUILayout.LabelField(new GUIContent("Extension Length", "Extends the length of the first and last section by the specified amount" +
                "\n\nUseful, for example, if a wall should envelop the first Post rather than starting at the Post's centre position."), GUILayout.Width(103));

            SerializedProperty endExtensionLengthProp = serializedObject.FindProperty("endExtensionLength").GetArrayElementAtIndex(railLayerIndex);
            EditorGUILayout.PropertyField(endExtensionLengthProp, new GUIContent(""), GUILayout.Width(35));
        }
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            //Debug.Log(" numStackedRailsProp.floatValue = " + numStackedRailsProp.floatValue + "\n");
            numStackedRailsProp.floatValue = (int)numStackedRailsProp.floatValue;
            serializedObject.ApplyModifiedProperties();

            af.ForceRebuildFromClickPoints();
            // Calculate the stacked gap
            if (af.numStackedRails[railLayerIndex] > 1)
            {
                float gap = 0;
                if (af.railSpreadMode[railLayerIndex] == 0)//total
                    gap = af.railSpread[railLayerIndex] / (af.numStackedRails[railLayerIndex] - 1);
                else
                    gap = af.railSpread[railLayerIndex];
            }
            //Debug.Log("prefabMeshWithTransformsSize = " + af.prefabMeshWithTransformsSize[railLayerIndex] + "\n");
        }
    }

    //==================================================================
    //                   Show Rails Variation Header
    //==================================================================
    public void ShowSetupVariationsSources(SerializedObject serializedObject, LayerSet layer)
    {
        int railLayerIndex = ((int)layer);
        SerializedProperty showRailVariationsProp = serializedObject.FindProperty("showRailVariations").GetArrayElementAtIndex(railLayerIndex);
        //====  Enable Variations  ====
        GUILayout.Space(14);
        if (af.showRailVariations[railLayerIndex] == true)
            GUILayout.BeginVertical("helpbox");
        else
            GUILayout.BeginVertical("box");

        if (showRailVariationsProp.boolValue)
            GUILayout.Space(6);
        else
            GUILayout.Space(2);
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();

        GUILayout.Space(9); //puts foldout triangle inside box
        //      Foldout
        //=====================
        showRailVariationsProp.boolValue = EditorGUILayout.Foldout(showRailVariationsProp.boolValue, "Show");

        //GUILayout.Button("Show", ed.tinyButtonStyle);

        GUILayout.Space(80);
        EditorGUILayout.LabelField(new GUIContent("Use " + layer.String() + " Variations ",
            "Choose a set of prefabs that will be available for variations. \n" +
            "These can then be used by either right-clicking on a section in Scene View,\n" +
            "or assigning them to a step in the Variation Sequencer"), ed.greenStyle2, GUILayout.Width(130));

        //==================================================
        //             Use Rail Variations
        //==================================================
        bool useSeq = af.GetUseSequencerForLayer(layer);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useRailVariations").GetArrayElementAtIndex(railLayerIndex),
            new GUIContent(""), GUILayout.Width(20));
        GUILayout.Space(10);
        if(af.useRailVariations[railLayerIndex] == true && useSeq == true)
            EditorGUILayout.LabelField(new GUIContent("[ Seq In Use ]"), ed.smallOrangeItalicLabelStyle, GUILayout.Width(80));
        GUILayout.Space(5);
        EditorGUILayout.LabelField(new GUIContent("Choose Prefabs for Variations:"), ed.italicStyle2, GUILayout.Width(320));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
        }
        bool variantsInUse = true;

        if (useSeq == false && (af.useRailSingles[railLayerIndex] == false || af.railSinglesContainer[0].numInUse == 0))
            variantsInUse = false;

        if (showRailVariationsProp.boolValue == true)
            GUILayout.Space(2);
        else
            GUILayout.Space(2);
        if (variantsInUse == false && serializedObject.FindProperty("useRailVariations").GetArrayElementAtIndex(railLayerIndex).boolValue == true)
        {
            EditorGUILayout.LabelField(new GUIContent($"      (No Sequencer or Singles are being used.  Enable {af.GetLayerNameAsString(layer)} Variation Sequencer, or Singles below)", "" +
                "Rail Variations are only used when you have assigned their use, either with the Variation Sequencer, or by assigning individual single replacements " +
                "by right clicking on a Panel in Scene View. \n\n At the moment nothing is using these variants so they have no effect."), ed.warningStyle2);
            GUILayout.Space(4);
        }
        else
            GUILayout.Space(10);

        if (showRailVariationsProp.boolValue && af.useRailVariations[railLayerIndex])
        {
            using (new EditorGUI.DisabledScope(af.useRailVariations[railLayerIndex] == false))
            {
                //===================================
                //          Setup Sources
                //===================================
                ed.varEd.SetupSourcePrefabs(layer);
                if (variantsInUse == true)
                    GUILayout.Space(10);
            }
        }
        //using (new EditorGUI.DisabledScope(af.useRailVariations[layerIndex] == false))
        {
            if (variantsInUse == true)
                GUILayout.Space(10);

            //========================
            //      Setup Sequencer
            //========================
            ed.seqEd.SetupStepSeqVariations(layer);

            //========================
            //      Setup Singles
            //========================
            ed.singlesEd.SetupSingles(layer);
        }

        GUILayout.Space(4);

        GUILayout.EndVertical();
        GUILayout.Space(2);
    }

    //---------------------------------------------------------
    //For v4.0 Don't remove
    private void ShowAdaptToSurfaceRotation()
    {
        //============= Adapt To Surface Rotation AFWB for v4.0 =============
        /*GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Adapt to Surface Slope", "Extras usually point straight up or to the nextPos Post. This can adapt them to the exact " +
        "slope of the surface by increasing this amount. This is only noticeable across a slope. \nNote" +
        " this might give uneven results if the ground is bumpy beneath the Extra's position \n\nThe Default setting is 'Off'."), GUILayout.Width(150));

        SerializedProperty railAAdapToSurfaceProp = serializedObject.FindProperty("railAAdaptToSurface");
        EditorGUILayout.PropertyField(railAAdapToSurfaceProp, new GUIContent(""), GUILayout.Width(50));
        if (railAAdapToSurfaceProp.boolValue == true)
        {
            GUILayout.Space(2);
            af.railASurfaceNormalAmount = EditorGUILayout.Slider(af.railASurfaceNormalAmount, 0, 1, GUILayout.Width(150));
            serializedObject.FindProperty("railASurfaceNormalAmount").floatValue = af.railASurfaceNormalAmount;
        }
        GUILayout.EndHorizontal();*/
    }

    //--------------
    private void ShowAdjustUVs()
    {
        //if (af.railJointStyle[layerIndex] == JointStyle.mitre)
        //{
        //    EditorGUILayout.LabelField(new GUIContent("     Stretch UVs", "Mitring can distort UVs, try this to compensate"), GUILayout.Width(90));
        //    EditorGUILayout.PropertyField(serializedObject.FindProperty("stretchUVs"), new GUIContent(""), GUILayout.Width(27));
        //}
    }
}