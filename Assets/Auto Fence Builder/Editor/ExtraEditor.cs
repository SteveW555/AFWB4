//#pragma warning disable 0219 // disable unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using AFWB;
using UnityEditor;
using UnityEngine;

// Simple script that creates a new non-dockable window
public class ExtraHelpWindow : EditorWindow
{
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        //load an image
        Texture2D myTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Auto Fence Builder/Editor/Images/ExtrasHelpSmaller.png", typeof(Texture2D));

        //get image svSize
        float imageWidth = myTexture.width;
        float imageHeight = myTexture.height;
        this.minSize = new Vector2(imageWidth - 20, 300);
        this.maxSize = new Vector2(imageWidth + 50, imageHeight + 20);

        float windowWidth = this.position.width;
        float windowHeight = this.position.height;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(windowWidth - 20), GUILayout.Height(windowHeight - 20));

        GUILayout.Label(myTexture);
        EditorGUILayout.EndScrollView();
    }
}

public class ExtraEditor
{
    private enum QuickStartTemplate
    {
        none, grid4x4PerPost, grid4x4PerNode, scatterRandom
    }

    private AutoFenceCreator af;
    private AutoFenceEditor ed;
    private ExtrasAFWB ex;
    private PrefabAssignEditor helper;
    private SerializedProperty extraProp, rotateToFenceDirectionProp, numGridXProp, numGridZProp, gridWidthProp, quickStartProp;
    private SerializedProperty usePostToPostInclineProp, extrasFollowGroundHeightProp, extraSurfaceNormalAmountProp;
    private SerializedProperty averageCornerDirectionProp, avgHeightPositionForRowProp, raiseExtraByPostHeightProp;
    private SerializedProperty scatterExtraRandPosProp, scatterExtraRandScaleProp, scatterExtraRandRotProp;
    private SerializedProperty extraFreqProp, extraFreqModeProp, finalPostModeProp, randomScatterProp, pivotPositionProp;

    private QuickStartTemplate quickStartTemplate = 0;
    private string[] quickStartStrings = { "--", "Simple 4 x 4 Grid Per Post", "Simples 4 x 4 Grid Per Post", "Scatter Random", "1 Per Post-Top" };

    public ExtraEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor, PrefabAssignEditor help, ExtrasAFWB extras, SerializedProperty exProp)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        helper = help;
        ex = extras;
        extraProp = exProp;
        rotateToFenceDirectionProp = extraProp.FindPropertyRelative("rotateToFenceDirection");

        numGridXProp = extraProp.FindPropertyRelative("numGridX");
        numGridZProp = extraProp.FindPropertyRelative("numGridZ");
        gridWidthProp = extraProp.FindPropertyRelative("gridWidth");

        scatterExtraRandPosProp = extraProp.FindPropertyRelative("scatterExtraRandPosRange");
        scatterExtraRandScaleProp = extraProp.FindPropertyRelative("scatterExtraRandScaleRange");
        scatterExtraRandRotProp = extraProp.FindPropertyRelative("scatterExtraRandRotRange");

        extraFreqProp = extraProp.FindPropertyRelative("extraFreq");
        extraFreqModeProp = extraProp.FindPropertyRelative("extraFreqMode");
        finalPostModeProp = extraProp.FindPropertyRelative("finalPostMode");
        randomScatterProp = extraProp.FindPropertyRelative("useRandomScatter");

        usePostToPostInclineProp = extraProp.FindPropertyRelative("usePostToPostIncline");
        averageCornerDirectionProp = extraProp.FindPropertyRelative("averageCornerDirection");

        extrasFollowGroundHeightProp = extraProp.FindPropertyRelative("extrasFollowGroundHeight");
        avgHeightPositionForRowProp = extraProp.FindPropertyRelative("avgHeightPositionForRow");
        extraSurfaceNormalAmountProp = extraProp.FindPropertyRelative("extraSurfaceNormalAmount");
        pivotPositionProp = extraProp.FindPropertyRelative("pivotPosition");
        raiseExtraByPostHeightProp = extraProp.FindPropertyRelative("raiseExtraByPostHeight");
    }

    private void PropertyChecks()
    {
        if (extraProp == null)
        {
            extraProp = ed.serializedObject.FindProperty("ex");
            Debug.Log("extraProp is null in ExtraEditor.PropertyChecks() \n");
            if (extraProp == null)
                Debug.Log(" and it couldn't be relinked with  ed.serializedObject.FindProperty(\"ex\") \n");
        }
        if (rotateToFenceDirectionProp == null)
        {
            rotateToFenceDirectionProp = extraProp.FindPropertyRelative("rotateToFenceDirection");
            if (rotateToFenceDirectionProp == null)
                Debug.Log("rotateToFenceDirectionProp is null in ExtraEditor.PropertyChecks() \n");
        }
        if (numGridXProp == null)
        {
            numGridXProp = extraProp.FindPropertyRelative("numGridX");
            if (numGridXProp == null)
                Debug.Log("extraProp is null in ExtraEditor.PropertyChecks() \n");
        }
        if (numGridZProp == null)
        {
            numGridZProp = extraProp.FindPropertyRelative("numGridZ");
            if (numGridZProp == null)
                Debug.Log("numGridZProp is null in ExtraEditor.PropertyChecks() \n");
        }
        if (gridWidthProp == null)
        {
            gridWidthProp = extraProp.FindPropertyRelative("gridWidth");
            if (gridWidthProp == null)
                Debug.Log("gridWidthProp is null in ExtraEditor.PropertyChecks() \n");
        }
    }

    private void PreflightChecks()
    {
        PropertyChecks();
    }

    //===================================
    public void SetupExtras(SerializedObject serializedObject)
    {
        PreflightChecks();

        SerializedProperty extraSize = extraProp.FindPropertyRelative("extraTransformScale");
        Color greyLineCol = new Color(0.34f, 0.34f, 0.4f, 0.6f);
        // EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), greyLineCol);
        GUILayout.Space(5);
        string extrasStr = "Extra", extrasHelpStr = "Click to Disable Extra";

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = ed.switchGreen;
        if (af.useExtrasLayer == false)
            GUI.backgroundColor = ed.switchRed;
        //EditorGUI.BeginChangeCheck();
        if (GUILayout.Button(new GUIContent(extrasStr, extrasHelpStr), GUILayout.Width(70)))
        {
            af.useExtrasLayer = !af.useExtrasLayer;
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.useExtrasLayer)
                af.componentToolbar = ComponentToolbar.extras;
        }
        GUI.backgroundColor = Color.white;

        EditorGUI.BeginDisabledGroup(af.useExtrasLayer == false);

        //      Reset 
        //=================
        if (GUILayout.Button(new GUIContent("Reset All", "Reset all Extra Scaling/Offsets/Rotations temorarily disable Scatter randomization"),
             GUILayout.Width(87)))
        {
            af.ex.ResetExtraTransforms();
        }
        GUILayout.Space(10);

        //      Copy Paste 
        //=================
        if (GUILayout.Button(new GUIContent("Copy", "Copy All Extras Parameters"), EditorStyles.miniButton, GUILayout.Width(42)))
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[af.currPresetIndex];
            CopyPasteAFWB.CopyComponentParametersFromScriptablePreset(preset, AFWB.LayerSet.extraLayerSet);
        }
        if (GUILayout.Button(new GUIContent("Paste", "Paste All Extras Parameters"), EditorStyles.miniButton, GUILayout.Width(44)))
        {
            CopyPasteAFWB.PasteExtraParametersFromScriptablePreset(af);
        }
        //====== Show Prefab Material Mesh   ======
        ed.assetFolderLinks.Show_Prefab_Mesh_Material_InAssetFolder(LayerSet.extraLayerSet, 208);

        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(af.useExtrasLayer == false);

        //==============================================================================
        //                          Choose Extra Prefab
        //==============================================================================

        helper.ChooseMainPrefab(LayerSet.extraLayerSet);


        //==============================================================================
        //                          Transforms
        //==============================================================================
        helper.SetExtraMainParameters();


        //=================================================================
        //      Template
        //=================================================================
        /*GUILayout.Space(4);
        EditorGUILayout.BeginVertical("box");
        EditorGUI.BeginChangeCheck();
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();

        string templateHelpString = "Set all parameters below to values suitable for a typical use case.\n\n" +
            "For example, 'Ornament' is suitable for single models that decorate the fence, such as a lamp on every post.\n\n" +
            "Use 'Rail' when you want the model to mimic the behaviour of rails and orient themselves from one post position to the next.";

        EditorGUILayout.LabelField(new GUIContent("Shortcut:", templateHelpString), ed.moduleHeaderLabelStyle, GUILayout.Width(65));

        //--  'Set' Button
        if (GUILayout.Button(new GUIContent("Set", templateHelpString), EditorStyles.miniButton, GUILayout.Width(50)))
        {
            if (af.ex.template == ExtraTemplate.MimicRail)
            {
                rotateToFenceDirectionProp.boolValue = true;
                usePostToPostInclineProp.boolValue = true;
                averageCornerDirectionProp.boolValue = false;
                extrasFollowGroundHeightProp.boolValue = true;
                avgHeightPositionForRowProp.boolValue = false;
                raiseExtraByPostHeightProp.boolValue = false;
                af.ex.extraSurfaceNormalAmount = 0;
                af.ex.pivotPosition = 0;
            }
            else if (af.ex.template == ExtraTemplate.PostOrnament)
            {
                rotateToFenceDirectionProp.boolValue = false;
                usePostToPostInclineProp.boolValue = false;
                averageCornerDirectionProp.boolValue = true;
                extrasFollowGroundHeightProp.boolValue = false;
                avgHeightPositionForRowProp.boolValue = true;
                raiseExtraByPostHeightProp.boolValue = false;

                // af.currentExtra_PrefabIndex].extraTransformPositionOffset.y = 0;
                af.SetPrefabForLayerByName(LayerSet.extraLayerSet, "Concrete Wall");

                af.ex.extraSurfaceNormalAmount = 0;
                af.ex.pivotPosition = 0;
            }
            else if (af.ex.template == ExtraTemplate.GroundOrnament)
            {
                rotateToFenceDirectionProp.boolValue = false;
                usePostToPostInclineProp.boolValue = false;
                averageCornerDirectionProp.boolValue = true;
                extrasFollowGroundHeightProp.boolValue = false;
                avgHeightPositionForRowProp.boolValue = true;
                raiseExtraByPostHeightProp.boolValue = false;
                af.ex.extraSurfaceNormalAmount = 0;
                af.ex.pivotPosition = 0;
            }
            else if (af.ex.template == ExtraTemplate.FixedDirection)
            {
                rotateToFenceDirectionProp.boolValue = false;
                usePostToPostInclineProp.boolValue = false;
                averageCornerDirectionProp.boolValue = true;
                extrasFollowGroundHeightProp.boolValue = false;
                avgHeightPositionForRowProp.boolValue = true;
                raiseExtraByPostHeightProp.boolValue = false;
                af.ex.extraSurfaceNormalAmount = 0;
                af.ex.pivotPosition = 0;
            }

            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(5);

        //-- Popup
        af.ex.template = (ExtraTemplate)EditorGUILayout.EnumPopup(af.ex.template, GUILayout.Width(100));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("Use Example Prefab", "Use an Example Prefab suitable for the Shortcut.\n\n" +
            "Useful for quick visualization. Choosing the 'Rail' Shortcut while trying to use a 'Lamp' Extra would " +
            "look very distorted. Similarly, so would using a long Rail as a decorator ornament.\n\n" +
            "Disable this if you've already ckosen a suitable prefab,"), GUILayout.Width(120));

        //-- Use Example Prefab
        EditorGUI.BeginChangeCheck();
        var useExamplePrefabProp = extraProp.FindPropertyRelative("useExamplePrefab");
        useExamplePrefabProp.boolValue = EditorGUILayout.Toggle(new GUIContent(""), useExamplePrefabProp.boolValue, GUILayout.Width(20));
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(3);
        EditorGUILayout.EndVertical();*/
        //---------------------------------------------------------------

        GUILayout.Space(8);
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();

        //EditorGUILayout.LabelField(new GUIContent("Rotate To Fence Direction", "Will rotate the Extra to match the direction of the section.\n " +
        //    "At corners this will be the average of the incoming and outgoing sections. \n" +
        //    "With it off Extras will face the World Z direction \n\n" +
        //    "This needs to be on for 'Incline to nextPos Post' and 'Average Corner' to function as they require the direction vector." +
        //    "\n\nThe default is for it to be On"), GUILayout.Width(153));

        //  Rotate To Fence Direction
        //===============================
        EditorGUILayout.LabelField(new GUIContent("Rotate To Fence Direction", "Rotates Extras so that they're facing in the direction of the Fence" +
            "\nWith this off, they will all face the same way dictated by the model itself."), GUILayout.Width(153));
        EditorGUILayout.PropertyField(rotateToFenceDirectionProp, new GUIContent(""), GUILayout.Width(20));

        //EditorGUI.BeginDisabledGroup(extraProp.FindPropertyRelative("rotateToFenceDirection").boolValue == false);
        {
            //  Incline to nextPos Post
            //===============================
            GUILayout.Space(11);
            EditorGUILayout.LabelField(new GUIContent("Incline to nextPos Post", "If the nextPos Post is higher or lower, the extra will also point up or down towards it." +
            "\nThe default is for it to be On"), GUILayout.Width(115));
            EditorGUILayout.PropertyField(usePostToPostInclineProp, new GUIContent(""), GUILayout.Width(20));

            //  Average Corner Direction
            //===============================
            GUILayout.Space(11);
            EditorGUILayout.LabelField(new GUIContent("Average Corner Direction", "At corners, the direction of the Extra will be the average of the incoming" +
                " and outgoing directions. \n The default is for it to be On"), GUILayout.Width(153));
            EditorGUILayout.PropertyField(averageCornerDirectionProp, new GUIContent(""), GUILayout.Width(20));
        }

        //add a button
        GUILayout.Space(10);
        if (GUILayout.Button(new GUIContent("R", "Use default settings"), EditorStyles.miniButton, GUILayout.Width(19)))
        {
            rotateToFenceDirectionProp.boolValue = true;
            usePostToPostInclineProp.boolValue = true;
            averageCornerDirectionProp.boolValue = true;
        }

        //-- add a button to open a window
        GUILayout.Space(8);
        if (GUILayout.Button(new GUIContent("Help", "Show guide images for these parameters"), EditorStyles.miniButton, GUILayout.Width(40)))
        {
            ExtraHelpWindow win = EditorWindow.GetWindow<ExtraHelpWindow>();
            //ExtraHelpWindow.Initialize();
        }
        GUILayout.Space(10);

        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        //  Follow Ground Height
        //===============================
        SerializedProperty extarSurfaceNormProp = extraProp.FindPropertyRelative("adaptExtraToSurfaceDirection");
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Follow Ground Height", "The Extra's height will be set relative to varying ground height between postsPool." +
            "\n Ensure Position Offset Y is positive to keep them above ground level"), GUILayout.Width(126));
        EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("extrasFollowGroundHeight"), new GUIContent(""), GUILayout.Width(23));

        //  Keep Row Level
        //===============================
        //GUILayout.Space(2);
        EditorGUILayout.LabelField(new GUIContent("Keep Row Level", "The height level of each Extra across the width of the row will be the same, " +
            "regardless of differences in ground height."), GUILayout.Width(96));
        EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("avgHeightPositionForRow"), new GUIContent(""), GUILayout.Width(23));

        //  Adapt to Surface Tilt
        //===============================
        EditorGUILayout.LabelField(new GUIContent("Adapt to Surface Tilt", "Extras usually face straight up or to the nextPos Post. This can adapt them to the exact " +
            "slope of the surface by increasing this amount. This is only noticeable across a slope. \nNote" +
            " this might give uneven results if the ground is bumpy beneath the Extra's position \n\nThe Default setting is 'Off'."), GUILayout.Width(120));
        EditorGUILayout.PropertyField(extarSurfaceNormProp, new GUIContent(""), GUILayout.Width(22));
        if (extarSurfaceNormProp.boolValue == true)
        {
            GUILayout.Space(2);
            af.ex.extraSurfaceNormalAmount = EditorGUILayout.Slider(af.ex.extraSurfaceNormalAmount, 0, 1, GUILayout.Width(10));
            extraProp.FindPropertyRelative("extraSurfaceNormalAmount").floatValue = af.ex.extraSurfaceNormalAmount;
            //-- Reset Surface Normal
            GUILayout.Space(8);
            if (GUILayout.Button(new GUIContent("R", "Use default settings"), EditorStyles.miniButton, GUILayout.Width(20)))
            {
                extarSurfaceNormProp.boolValue = false;
            }
        }
        //      Pivot Position 
        //============================
        //0 = base, 1 = center
        GUILayout.Space(3);
        string[] pivotNames = System.Enum.GetNames(typeof(PivotPosition));


        EditorGUILayout.LabelField(new GUIContent("Pivot Pos", "Extra's postion is set at the Pivot Point, which also determines the point of Rotation.\n\n " +
            "For objects on the ground, set to Center_Base so their base aligns with the ground. \n\n" +
            "For objects such as Gates, set to LeftX_CentreY so they rotate from their edge"), GUILayout.Width(60));

        //SerializedProperty pivotProp = extraProp.FindPropertyRelative("pivotPosition");
        pivotPositionProp.intValue = EditorGUILayout.Popup("", (int)pivotPositionProp.intValue, pivotNames, GUILayout.Width(95));

        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }


        //============================================================================================
        //                                      Mode Toolbar
        //============================================================================================

        if (af.ex.extrasMode == ExtrasMode.scatter)
        {
            GUI.backgroundColor = new Color(1f, .94f, .88f, 1);
            GUI.contentColor = new Color(1f, .94f, .88f, 1); ;
        }
        GUILayout.Space(7);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        GUIContent[] extraModeToolbarContent = { new GUIContent("Normal Mode", "Extras are based around positions of certain Posts. "),
                new GUIContent("Scatter Mode", "Extras are scattered in a grid between each Post position. ") };

        string[] toolbarStrings = { "Extras: Normal Mode", "Extras: Random Scatter Mode" };

        Color bgColor = GUI.backgroundColor;

        float idealInspectorWidth = 604;
        float inset = 8;
        float toolbarWidth = idealInspectorWidth - (inset * 2);
        GUILayout.Space(inset);

        //    Show Toolbar
        //=======================
        GUI.backgroundColor = new Color(.82f, .88f, 1.0f, 1);

        extraProp.FindPropertyRelative("extrasMode").intValue = GUILayout.Toolbar((int)extraProp.FindPropertyRelative("extrasMode").intValue,
            extraModeToolbarContent,
            GUILayout.Width(toolbarWidth),
            GUILayout.Height(28)
        );

        GUI.backgroundColor = bgColor;
        GUILayout.Space(10); GUILayout.Space(10);
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(LayerSet.extraLayerSet);
            af.ForceRebuildFromClickPoints();
        }

        //      Mode Description
        //============================
        if (af.ex.extrasMode == ExtrasMode.normal)
        {
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Space(70);
            EditorGUILayout.LabelField("Normal Mode:   Single or stacked prefabs based on post positions.", ed.extrasNormalModeLabelStyle);




            GUILayout.EndHorizontal();
            GUILayout.Space(8);
        }

        //      Placement Mode
        //=========================
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        string extraFreqModeHelpString = "", extraFreqModeHelpString2 = "";

        extraFreqModeHelpString = "Extras placement is based upon Post positions. " +
            "\n\nYou can filter which postsPool you want to use for the placement of Extras." +
            "\n\n ---------------------------- \n\n If Extra Frequence Mode is set to 'All Post Positions'  then there will be  " + af.ex.numGridZ + "  Rows between each Post." +
            "\n\n If Extra Frequence Mode is set to 'Main Posts Only'  then there will be  " + af.ex.numGridZ + "  Rows between each ClickPoint Post." +
            "\n\n If Extra Frequence Mode is set to 'Ends Only'  then there will be  " + af.ex.numGridZ + "  Rows between the very first Post and the very last Post.";

        if (af.ex.extrasMode == ExtrasMode.normal)
        {
            extraFreqModeHelpString = "Extras placement is based upon Post positions. " +
            "\n\nYou can filter which Post positions you want to use for the placement of Extras." +
            "\n\n\n 'All Post Positions':  Places Extra at each Post position." +
            "\n\n'Main Posts Only':  Extra at each Main ClickPoint Post." +
            "\n\n 'Ends Only':  Extra placed only at the very first Post and the very last Post." +
            "\n\n 'All Except Main':  Extra at each Post position except at the Main CklickPoint Nodes." +
            "\n\n'Every 1/n':  Extra at every n'th post, set by entered value" +
            "\n\n'Every 1/n + Subposts':  Same as above but will also use Subpost postions" +
            "\n\n'N for each Main ClickPoint':  Places n Extras between each Main ClickPoint";

            EditorGUILayout.LabelField(new GUIContent("Placement:", extraFreqModeHelpString), GUILayout.Width(150), GUILayout.ExpandWidth(false));
            af.ex.extraFreqMode = (ExtraPlacementMode)EditorGUILayout.EnumPopup(af.ex.extraFreqMode, GUILayout.Width(170));
            extraFreqModeProp.intValue = (int)af.ex.extraFreqMode;
            // Define N
            if (extraFreqModeProp.intValue == (int)ExtraPlacementMode.everyNthPost || extraFreqModeProp.intValue == (int)ExtraPlacementMode.everyNthPostAndSubposts) //Every 1/n
            {
                EditorGUILayout.LabelField(new GUIContent("    1 \u2215 "), GUILayout.Width(40));
                EditorGUILayout.PropertyField(extraFreqProp, new GUIContent(""), GUILayout.Width(30));
                if (extraFreqProp.intValue < 1)
                    extraFreqProp.intValue = 1;
            }

            GUILayout.Space(2);

            //      Final Post Mode
            //===========================
            string[] finalUsageStrings = { "Regular Final", "Omit Final", "Enforce Final On" };
            EditorGUILayout.LabelField(new GUIContent("Final Post Mode", "You can override the Frequency Mode to specify if the final Post is used when " +
                "determining Extras placement"), GUILayout.Width(94), GUILayout.ExpandWidth(false));
            finalPostModeProp.intValue = EditorGUILayout.Popup(finalPostModeProp.intValue, finalUsageStrings, GUILayout.Width(125));
        }
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }

        //===========================================
        //
        //          Extras Normal Mode
        //
        //===========================================

        float dx = af.ex.scatterExtraRandScaleRange.x, dy = af.ex.scatterExtraRandScaleRange.y, dz = af.ex.scatterExtraRandScaleRange.z;
        if (af.ex.extrasMode == ExtrasMode.normal)
        {
            GUILayout.Space(4);
            EditorGUI.BeginChangeCheck();




            GUILayout.BeginHorizontal();

            //    Raise by Post Height
            //============================

            EditorGUILayout.LabelField(new GUIContent("Raise by post-height",
            "Will raise all Extras by the height of the post. \nAny Position Offset you set will be added to the post height. \n\nSet 'Extras Position Offset' Y " +
            " to zero if you want them to be at exactly post height. \n\n Useful when the height of the Post is varaible and you need to " +
            "set them relative to each Post's height."), GUILayout.Width(150));
            EditorGUILayout.PropertyField(raiseExtraByPostHeightProp, new GUIContent(""), GUILayout.Width(20));

            if (ex.extraTransformPositionOffset.y != 0)
            {
                string s = " (A height offset of " + ex.extraTransformPositionOffset.y + " is also set in 'Extras Position Offset' )";
                EditorGUILayout.LabelField(new GUIContent(s));
                GUILayout.Space(2);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Incline with slopes"), GUILayout.Width(150));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("extrasFollowIncline"), new GUIContent(""));
            GUILayout.EndHorizontal();

            //      Stack 
            //=====================
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Make Stack", "Stack multiple Extras vertically"), GUILayout.Width(150));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("makeMultiArray"), new GUIContent("", "Stack multiple Extras vertically"));
            GUILayout.EndHorizontal();
            if (af.ex.makeMultiArray)
            {
                EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("numExtras"), new GUIContent("Num Extras"));
                EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("extrasGap"), new GUIContent("Extras Gap"));
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ed.EnforceVectorMinimums(extraSize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
                af.ex.multiArraySize.y = af.ex.numExtras;
                af.ex.extraFreq = extraFreqProp.intValue;
                af.ForceRebuildFromClickPoints();
            }
            EditorGUILayout.EndVertical();
        }

        //===========================================
        //
        //          Extras Scatter Mode
        //
        //===========================================

        if (af.ex.extrasMode == ExtrasMode.scatter)
        {
            //GUILayout.Space(3);
            EditorGUI.BeginChangeCheck();
            bool rebuild = false;
            //==========================
            //       Grid Layout
            //==========================
            //EditorGUILayout.BeginVertical("box");
            //GUILayout.Space(1);
            GUILayout.BeginHorizontal();
            GUILayout.Space(70);
            //      Mode Description
            //============================  
            EditorGUILayout.LabelField("Scatter Mode:   Multiple prefabs scattered within a grid area, aligned or randomised." /*, ed.extrasScatterModeLabelStyle*/);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Grid Layout: ", "Set the width of the grid area, and the number of" +
                " Extra prefabs that will be scattered in that area. The length of the area is the length of the section between main postsPool."), EditorStyles.boldLabel);

            //      Quick Start
            //=========================
            //Noy using Proprty as i's just an Editor variable
            QuickStartTemplate oldQuickStart = quickStartTemplate;
            EditorGUILayout.LabelField(new GUIContent("Quick Start: ", "Templates that set all parameters for typical uses."), ed.label11Style, GUILayout.Width(65));
            quickStartTemplate = (QuickStartTemplate)EditorGUILayout.Popup((int)quickStartTemplate, quickStartStrings, ed.popup11Style, GUILayout.Width(170), GUILayout.Height(13));
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            //     Frequency Mode
            //=========================
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Grid Spans Across:", extraFreqModeHelpString), GUILayout.Width(150), GUILayout.ExpandWidth(false));
            af.ex.extraFreqMode = (ExtraPlacementMode)EditorGUILayout.EnumPopup(af.ex.extraFreqMode, GUILayout.Width(170));
            extraFreqModeProp.intValue = (int)af.ex.extraFreqMode;
            // Define N
            if (extraFreqModeProp.intValue == (int)ExtraPlacementMode.everyNthPost || extraFreqModeProp.intValue == (int)ExtraPlacementMode.everyNthPostAndSubposts) //Every 1/n
            {
                EditorGUILayout.LabelField(new GUIContent("    1 \u2215 "), GUILayout.Width(40));
                EditorGUILayout.PropertyField(extraFreqProp, new GUIContent(""), GUILayout.Width(30));
                if (extraFreqProp.intValue < 1)
                    extraFreqProp.intValue = 1;
            }

            GUILayout.Space(10);
            //-- Final Post Mode
            string[] finalUsageStrings = { "Regular Final", "Omit Final", "Enforce Final On" };
            EditorGUILayout.LabelField(new GUIContent("Final Post Mode", "You can override the Frequency Mode to specify if the final Post is used when " +
                "determining Extras placement"), GUILayout.Width(100), GUILayout.ExpandWidth(false));
            finalPostModeProp.intValue = EditorGUILayout.Popup(finalPostModeProp.intValue, finalUsageStrings, GUILayout.Width(130));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ed.EnforceVectorMinimums(extraSize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));

                //If something changed but it weasn't QuickStartTemplate, set Quick start to none "--"
                if (quickStartTemplate == oldQuickStart)
                    quickStartTemplate = QuickStartTemplate.none;

                SetupQuickStartScatter();

                af.ex.multiArraySize.y = af.ex.numExtras;
                af.ex.extraFreq = extraFreqProp.intValue;
                //af.ForceRebuildFromClickPoints();
                rebuild = true;
            }
            GUILayout.EndHorizontal();

            //EditorGUI.EndDisabledGroup(); return;

            EditorGUI.BeginChangeCheck();
            GUILayout.Space(7);

            //=========================
            //     Extras Per Row
            //=========================
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Extras per Row: ", "How many extrasPool across width, perpendicular to the fence direction"), GUILayout.Width(95));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("numGridX"), new GUIContent(""), GUILayout.Width(120));
            extraProp.FindPropertyRelative("numGridX").floatValue = (int)extraProp.FindPropertyRelative("numGridX").floatValue;
            EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(15));

            //=======================================
            //     Contect-Dependent Labels & Help
            //=======================================
            //-- All Post Positions
            string numRowsString = "Num Rows Between Each Post:";
            string numRowsHelpString = "How many rows of Extras between each Post Position (set in 'Extra Frequency Mode' above)";

            //-- All Post Positions
            if (extraFreqModeProp.intValue == 0)
            {
                numRowsString = "Num Rows Between Each Post:";
                numRowsHelpString += "\n\nAs it's currently set to 'All Post Positions', there will be  " + af.ex.numGridZ + "  Rows between each Post";
            }
            //-- Main Posts Only
            if (extraFreqModeProp.intValue == 1)
            {
                numRowsString = "Num Rows Between Each Click Point:";
                numRowsHelpString += "\n\nAs it's currently set to 'Main Posts Only', there will be " + af.ex.numGridZ + "  Rows between each main ClickPoint Post";
            }
            if (extraFreqModeProp.intValue == 2)
            {
                numRowsString = "Num Total Rows Start to End:";
                numRowsHelpString += "\n\nAs it's currently set to 'Ends Only', there will be " + af.ex.numGridZ + "  Rows between the very first Post and the very last Post";
            }

            numRowsHelpString += "\n\n ---------------------------- \n\n If Extra Frequence Mode is set to 'All Post Positions'  then there will be  " + af.ex.numGridZ + "  Rows between each Post." +
                "\n\n If Extra Frequence Mode is set to 'Main Posts Only'  then there will be  " + af.ex.numGridZ + "  Rows between each ClickPoint Post." +
                "\n\n If Extra Frequence Mode is set to 'Ends Only'  then there will be  " + af.ex.numGridZ + "  Rows between the very first Post and the very last Post.";

            //========================
            //     Number of Rows
            //=========================
            EditorGUILayout.LabelField(new GUIContent(numRowsString, numRowsHelpString), GUILayout.Width(215));
            EditorGUILayout.PropertyField(numGridZProp, new GUIContent(""), GUILayout.Width(120));
            ex.numGridZ = (int)numGridZProp.floatValue;
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            //        Width
            //=========================
            EditorGUI.BeginDisabledGroup(af.ex.numGridX < 2);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Width: ", "How far the grid and randomness will spread out to the side of the wall. Doesn't apply when Extras per Grid-Width = 1"), GUILayout.Width(50));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("gridWidth"), new GUIContent(""), GUILayout.Width(125));

            //   Stretch Width At Corners
            //=========================
            //__ For v4.1
            /*GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Stretch Width At Corners", "Will Stretch the width spacing to fit a Mitered corner"), GUILayout.Width(155));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("stretchWidthAtCorners"), new GUIContent(""), GUILayout.Width(20));
            //     Corner Mode
            //=========================
            //string[] cornerNames = { "None", "Corner Angle", "Corner Angle Outer Only", "Auto" };
            string[] cornerNames = { "Basic", "Angled Corner" };
            EditorGUILayout.LabelField(new GUIContent("Corner Mode", "Determines how additional Extras are placed on Post Corners"), GUILayout.Width(90));
            af.ex.cornerMode = EditorGUILayout.Popup("", (int)af.ex.cornerMode, cornerNames, GUILayout.Width(100));
            extraProp.FindPropertyRelative("cornerMode").intValue = (int)af.ex.cornerMode;*/

            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Show Grid Density Calculations", "Get Info about the number of rows and extrasPool that will be created"), GUILayout.Width(185));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("showLayoutCalc"), new GUIContent(""), GUILayout.Width(20));

            //-- Corner Mode
            GUILayout.Space(100);
            EditorGUILayout.LabelField(new GUIContent("Corner Fill Mode",
                "None: The Extras or their Grid are placed linearly with no adaption for the corner. There could be some overlap, or gaps depending on the Grid size and density" +
                "\n\nGrid: The Grids are still Linearly connected, but overlaps and gaps will be removed or filled"), GUILayout.Width(185));

            SerializedProperty cornerFillModeProp = extraProp.FindPropertyRelative("cornerFillMode");
            cornerFillModeProp.intValue = (int)(CornerFillMode)EditorGUILayout.EnumPopup((CornerFillMode)cornerFillModeProp.intValue, GUILayout.Width(100));

            GUILayout.Space(50);



            GUILayout.EndHorizontal();

            //      Show Grid Summary Debug
            //================================
            if (af.ex.showLayoutCalc == true)
            {
                float spacing = af.interPostDist / af.ex.numGridZ;
                string spacingStr = "there will be a row every " + spacing.ToString("0.0") + " meters";
                int totalNumRows = (int)af.ex.numGridZ * (af.postsBuiltCount - 1), totalNumExtras = (totalNumRows + 1) * (int)af.ex.numGridX;
                totalNumExtras = ex.extrasBuiltCount;
                if (spacing < 1.0f)
                    spacingStr = "there will be " + (1.0f / spacing).ToString("0.0") + " rows per meter ( " + spacing.ToString("0.00") + "m apart ) ";

                spacingStr += "\nThere are " + (af.postsBuiltCount - 1) + " Posts (excluding end), making a total of ( " + af.ex.numGridZ + "x" + (af.postsBuiltCount - 1) + " ) = "
                        + totalNumRows + "rows.    There is also 1 extra row at the final Post, " +
                        "\nand possibly 1 extra row to fill each corner, depending on the corner mode chosen.   Total extrasPool =  " + totalNumExtras;

                string summaryString = "You are Using ' " + af.ex.extraFreqMode.EnumToReadableString() + " ' ";
                if (extraFreqModeProp.intValue == 0) //All Post Positions
                    summaryString += "so there will be  " + af.ex.numGridZ + " x [Num Posts between each Post].  " +
                        "\nWith your current Post-Rail Spacing setting of " + af.interPostDist + "m, " + spacingStr;

                GUIStyle wrapStyle = new GUIStyle(EditorStyles.label);
                wrapStyle.wordWrap = true;
                wrapStyle.fontSize = 11;
                EditorGUILayout.LabelField(new GUIContent(summaryString, summaryString), wrapStyle, GUILayout.Width(800));
            }

            GUILayout.Space(4);
            //EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(4);

            //-- Do checks outside of EndChangeCheck() in case they got screwed elsewhere
            if (af.ex.numGridX < 1)
                extraProp.FindPropertyRelative("numGridX").floatValue = 1;
            if (af.ex.numGridZ < 1)
                extraProp.FindPropertyRelative("numGridZ").floatValue = 1;
            if (af.ex.gridWidth < 0.2f)
                extraProp.FindPropertyRelative("gridWidth").floatValue = 0.2f;

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                af.ForceRebuildFromClickPoints();
                //If something changed but it weasn't QuickStartTemplate, set Quick start to none "--"
                if (quickStartTemplate == oldQuickStart)
                    quickStartTemplate = QuickStartTemplate.none;
                rebuild = true;
            }

            EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), greyLineCol);
            
            //==============================================
            //                 Randomization
            //==============================================
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Scatter Randomization", "With all random parameters set to 0, the elements are arranged evenly " +
                "within a grid. Randomize their Position, Scale and Rotation"), EditorStyles.boldLabel, GUILayout.Width(145));

            EditorGUI.BeginChangeCheck();
            //      Use Random
            //========================
            SerializedProperty useRandomScatterProp = extraProp.FindPropertyRelative("useRandomScatter");
            EditorGUILayout.PropertyField(useRandomScatterProp, new GUIContent(""), GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
               serializedObject.ApplyModifiedProperties();
                rebuild = true;
            }

            if (useRandomScatterProp.boolValue == true)
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.EndHorizontal();

                //EditorGUI.BeginDisabledGroup(extraProp.FindPropertyRelative("useRandomScatter").boolValue == false);

                EditorGUILayout.BeginVertical("box");

                //      Randomization Strength 
                //==========================================
                GUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                SerializedProperty scatterRandomStrengthProp = extraProp.FindPropertyRelative("scatterRandomStrength");
                EditorGUILayout.LabelField(new GUIContent("Random Strength", "Scale the overall effect of all random parametrs. " +
                    "Default is 1. \n\n Also a convenient way to temporarily zero all Randomness"), EditorStyles.boldLabel, GUILayout.Width(214));
                
                EditorGUILayout.Slider(scatterRandomStrengthProp, 0.0f, 2.0f, new GUIContent(""), GUILayout.Width(295));

                GUILayout.Space(50);
                if (GUILayout.Button(new GUIContent("R", "Set Rotation values to default 0"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    extraProp.FindPropertyRelative("scatterRandomStrength").floatValue = 1.0f;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                //--------------------------------------------------
                bool rebuildRandomScaleValues = false, reseedRandomPositionValues = false, reseedRandomRotationValues = false;

                //===================================================
                //                  Random Position
                //===================================================
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Random Position Offset", "Randomize Extra Position "), GUILayout.Width(214));

                //     Transform Box
                //=====================
                EditorGUILayout.PropertyField(scatterExtraRandPosProp, new GUIContent("", "Randomize Extra Position between -value and +value"),
                    GUILayout.Width(295));

                GUILayout.Space(24);
                //      Re-Seed
                //=====================
                if (GUILayout.Button(new GUIContent("S", "Re-Seed random Position"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    reseedRandomPositionValues = true;
                    
                }

                GUILayout.Space(5);
                //      Reset
                //=====================
                if (GUILayout.Button(new GUIContent("R", "Set Position values to default 0"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    scatterExtraRandPosProp.vector3Value = Vector3.zero;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(1);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                   if(reseedRandomPositionValues)
                        af.extraSeeds.ReseedScatterPos();
                    af.extraSeeds.GenerateRandomExtraScatterPos();

                    rebuild = true;
                }



                //===================================================
                //                  Random Scale
                //===================================================
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                
                //     Label
                //=====================
                EditorGUILayout.LabelField(new GUIContent("Random Scale", "Will scale each axis between the value chosen, and '1' " +
                    " as a multiple of the scale set in 'Extras Scale' above " +
                    "\n\n A value of 1 represents no scaling. Values less than 1 will decrease the scale, and above 1 will increase the scale" +
                    "\n\n For example if 'Extras Scale' = 0.8, and Random Scale = 0.5, an object of size 1m will be scaled to between 0.8m and 0.4m  [(1 x 0.8) to (0.5 x 0.8)]" +
                    "\n\n If 'Extras Scale' = 4, and Random Scale = 1.5, an object of size 1m will be scaled to between 4m and 6m  [(1 x 4) to 1.5 x 4)]" +

                    "\n\n[X]  [Y]  [Z]  \nThis will allow x,y,z scaling to be done independently. " +
                    "\n\n[X+Z]  [Y] will lock X+Z together \nthis is useful when you want to scale the height independently of the overall Size & shape. " +
                    "\n\n[X+Y+Z] will lock all axes \nUse when you want to scale all keeping the original shape"), GUILayout.Width(100));

                string[] randomScaleModeStrings = { "[X]  [Y]  [Z]", "[X+Z]  [Y]", "[X+Y+Z]" };

                //     [X] [Y] [Z]
                //=====================
                extraProp.FindPropertyRelative("randomScaleMode").intValue =
                    EditorGUILayout.Popup(extraProp.FindPropertyRelative("randomScaleMode").intValue, randomScaleModeStrings, GUILayout.Width(80));

                //     Transform Box
                //=====================
                GUILayout.Space(30);
                EditorGUILayout.PropertyField(scatterExtraRandScaleProp, new GUIContent("", "Randomize Scale"), GUILayout.Width(295));
                GUILayout.Space(24);
                
                //      Re-Seed
                //=====================

                if (GUILayout.Button(new GUIContent("S", "Re-Seed random Scale"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.extraSeeds.ReseedScatterScale();
                    rebuildRandomScaleValues = true;
                }
                GUILayout.Space(5);
                //      Reset
                //=====================
                if (GUILayout.Button(new GUIContent("R", "Set Scale values to default 1"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    scatterExtraRandScaleProp.vector3Value = Vector3.one;
                    rebuildRandomScaleValues = true;
                }
                GUILayout.Space(1);
                EditorGUILayout.EndHorizontal();

                //-- Enforce Scale > 0.01   and  < 4
                scatterExtraRandScaleProp.vector3Value = ed.EnforceVectorMinMax(scatterExtraRandScaleProp.vector3Value,
                    new Vector3(0.01f, 0.01f, 0.01f), new Vector3(10, 10, 10));


                if (EditorGUI.EndChangeCheck() || rebuildRandomScaleValues)
                {
                    serializedObject.ApplyModifiedProperties();
                    //-- This will set class variables for the minRandScatterScale and maxRandScatterScale
                    af.ex.CalculateMinMaxScatterScaling();
                    af.extraSeeds.ReseedScatterScale();
                    af.extraSeeds.GenerateRandomExtraScatterScale(af.ex.minRandScatterScale, af.ex.maxRandScatterScale);

                    //Debug.Log($"{af.extraSeeds.scatterScaleSeed}\n");
                    //Debug.Log($"{af.extraSeeds.rScatterScale[0]}\n");

                    if (extraProp.FindPropertyRelative("randomScaleMode").intValue == 1) //[x+z], y
                    {
                        float x = af.ex.scatterExtraRandScaleRange.x;
                        float y = af.ex.scatterExtraRandScaleRange.y;
                        float z = af.ex.scatterExtraRandScaleRange.z;

                        if (x != dx)
                            af.ex.scatterExtraRandScaleRange = new Vector3(x, y, x);
                        if (z != dz)
                            af.ex.scatterExtraRandScaleRange = new Vector3(z, y, z);
                    }
                    if (extraProp.FindPropertyRelative("randomScaleMode").intValue == 2) //x+y+z
                    {
                        float x = af.ex.scatterExtraRandScaleRange.x;
                        float y = af.ex.scatterExtraRandScaleRange.y;
                        float z = af.ex.scatterExtraRandScaleRange.z;

                        if (x != dx)
                            af.ex.scatterExtraRandScaleRange = new Vector3(x, x, x);
                        if (y != dy)
                            af.ex.scatterExtraRandScaleRange = new Vector3(y, y, y);
                        if (z != dz)
                            af.ex.scatterExtraRandScaleRange = new Vector3(z, z, z);
                    }
                    rebuild = true;
                }



                //===================================================
                //                  Random Rotation
                //===================================================
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Random Rotation", "Randomize Rotation between +/- value." +
                    "\n Use the main Extras Rotation settings above to offset this." +
                    "\n For example setting 30 here and then 90 in Extras Rotation will yield a range of 60 to 120 (90-30  to 90+30"), GUILayout.Width(184));

                //     Transform Box
                //=====================
                GUILayout.Space(30);
                EditorGUILayout.PropertyField(scatterExtraRandRotProp, new GUIContent(""),  GUILayout.Width(295));

                //      Re-Seed
                //=====================
                GUILayout.Space(24);
                if (GUILayout.Button(new GUIContent("S", "Re-Seed random Rotation"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                   // af.extraSeeds.ReseedScatterRot();
                    reseedRandomRotationValues = true;
                    rebuild = true;
                }
                //      Reset
                //=====================
                GUILayout.Space(5);
                if (GUILayout.Button(new GUIContent("R", "Set Rotation values to default 0"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    scatterExtraRandRotProp.vector3Value = Vector3.zero;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                ///scatterExtraRandPosProp.vector3Value = ed.EnforceVectorMinMax(scatterExtraRandPosProp.vector3Value, Vector3.zero, new Vector3(1, 1, 1));
                scatterExtraRandRotProp.vector3Value = ed.EnforceVectorMinMax(scatterExtraRandRotProp.vector3Value, new Vector3(-360, -360, -360), new Vector3(360, 360, 360));


                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if(reseedRandomRotationValues)
                        af.extraSeeds.ReseedScatterRot();
                    af.extraSeeds.GenerateRandomExtraScatterRotation();
                    rebuild = true;
                }
            }
            else
            {
                GUILayout.EndHorizontal();
            }
            //------------------------------------------------------------

            
            if (rebuild)
            {
                af.ForceRebuildFromClickPoints();
            }

            EditorGUI.EndDisabledGroup();//end randomization block





            //======================================================
            //              Scatter Variations
            //======================================================
            GUILayout.Space(5);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), greyLineCol);
            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Add Prefab Alternatives", "You can choose up to 3 different prefabs to be scattered." +
                "\nTheir relative quantity can be assigned with the the Frequency slider." +
                "\nIf you only need 1 prefab, disable 'Add Prefab Alternatives' " +
                "\n(or set its Probability to 1 and the others to 0, or press the [R] Reset button)"), EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.PropertyField(extraProp.FindPropertyRelative("enablePrefabVars"), new GUIContent(""), GUILayout.Width(20));

            if (extraProp.FindPropertyRelative("enablePrefabVars").boolValue == true)
            {
                if (GUILayout.Button("R", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.ex.extraScatterVarFreq[0] = 1;
                    for (int i = 1; i < 4; i++)
                    {
                        af.ex.extraScatterVarFreq[i] = 0;
                    }
                }
                if (GUILayout.Button("S", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    af.extraSeeds.SeedExtraVar();
                }
            }

            //----  Exclude XZ Rotations ----
            GUILayout.Space(240);
            string excludeStr = "Exclude XZ Rotations";
            if (af.ex.excludeExtraVarXZRotations)
            {
                excludeStr = "XZ Rotations Excluded";
                GUI.backgroundColor = new Color(.45f, .45f, .45f);
            }
            if (GUILayout.Button(new GUIContent(excludeStr, "Prevent any of the variation prefabs from being rotated on " +
                "the X or Z axis. This keeps them from tipping into the ground, but the direction can still be randomised"), GUILayout.Width(140)))
            {
                af.ex.excludeExtraVarXZRotations = !af.ex.excludeExtraVarXZRotations;

                //snapMainPostsProp.boolValue = !snapMainPostsProp.boolValue;
                //af.snapMainPostsProp = snapMainPostsProp.boolValue;
                //af.ForceRebuildFromClickPoints();
                //Debug.Log(af.snapMainPostsProp);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                af.ResetPoolForLayer(LayerSet.extraLayerSet);
                af.ForceRebuildFromClickPoints();
            }

            EditorGUILayout.BeginVertical("box");
            GUILayout.Space(4);

            //---------Main ------------------
            if (extraProp.FindPropertyRelative("enablePrefabVars").boolValue == true)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"0:   Main:    {af.extraPrefabs[af.currentExtra_PrefabIndex].name}", EditorStyles.boldLabel, GUILayout.Width(250));
                EditorGUILayout.LabelField("0:   Main:    " + af.extraPrefabs[af.currentExtra_PrefabIndex].name, EditorStyles.boldLabel, GUILayout.Width(250));
                GUILayout.Space(17);
                EditorGUILayout.LabelField(new GUIContent("Freq.", ""), GUILayout.Width(32));
                af.ex.extraScatterVarFreq[0] = EditorGUILayout.Slider(af.ex.extraScatterVarFreq[0], 0, 1, GUILayout.Width(105));
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    af.ResetPoolForLayer(LayerSet.extraLayerSet);
                    af.ForceRebuildFromClickPoints();
                }
            }

            //--------------
            GUILayout.Space(4);
            SerializedProperty extraVarScaleProp = extraProp.FindPropertyRelative("extraVarScale");
            SerializedProperty enableChoosePrefabsProp = extraProp.FindPropertyRelative("enablePrefabVars");
            int numVariations = enableChoosePrefabsProp.boolValue ? af.ex.numExtraVars : 1;
            string[] extraNamesArray = af.extraMenuNames.ToArray();

            for (int i = 1; i < numVariations; i++)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();

                SerializedProperty scaleProp = extraVarScaleProp.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField(new GUIContent($"{i}: "), GUILayout.Width(16));

                if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(17)) && af.ex.extraScatterVarMenuIndex[i] > 0)
                {
                    af.ex.extraScatterVarMenuIndex[i] -= 1;
                }
                if (GUILayout.Button(">", EditorStyles.miniButton, GUILayout.Width(17)) && af.ex.extraScatterVarMenuIndex[i] < af.extraMenuNames.Count - 1)
                {
                    af.ex.extraScatterVarMenuIndex[i] += 1;
                }
                af.ex.extraScatterVarMenuIndex[i] = EditorGUILayout.Popup("", af.ex.extraScatterVarMenuIndex[i], extraNamesArray, GUILayout.Width(200));

                GUILayout.Space(7);
                EditorGUILayout.LabelField(new GUIContent("Freq.", ""), GUILayout.Width(30));
                af.ex.extraScatterVarFreq[i] = EditorGUILayout.Slider(af.ex.extraScatterVarFreq[i], 0, 1, GUILayout.Width(105));

                GUILayout.Space(6);
                EditorGUILayout.LabelField(new GUIContent("Scale"), GUILayout.Width(35));
                EditorGUILayout.PropertyField(scaleProp, new GUIContent(""), GUILayout.Width(130));

                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    af.ex.extraScatterVarIndex[i] = af.ConvertExtraMenuIndexToPrefabIndex(af.ex.extraScatterVarMenuIndex[i]);
                    scaleProp.vector3Value = ed.EnforceVectorMinMax(scaleProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f), new Vector3(100, 100, 100));
                    serializedObject.ApplyModifiedProperties();
                    af.ex.extraScatterVarIndex[i] = af.ConvertExtraMenuIndexToPrefabIndex(af.ex.extraScatterVarMenuIndex[i]);

                    af.ex.prefabVars[i] = af.extraPrefabs[af.ex.extraScatterVarIndex[i]];
                    af.ResetPoolForLayer(LayerSet.extraLayerSet);
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.Space(7);
            }
            EditorGUILayout.EndVertical();

            /*GUILayout.Space(4);
            SerializedProperty extraVarScaleProp = extraProp.FindPropertyRelative("extraVarScale");
            int numVariations = af.ex.numExtraVars;
            if (extraProp.FindPropertyRelative("enablePrefabVars").boolValue == false)
                numVariations = 1;
            for (int i = 1; i < numVariations; i++)
            {
                string varName = "";
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();

                //====  GameObject: Choose From Menu  ====
                SerializedProperty scaleProp = extraVarScaleProp.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField(new GUIContent(i + ": "), GUILayout.Width(16));

                if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(17)) && af.ex.extraScatterVarMenuIndex[i] > 0)
                {
                    af.ex.extraScatterVarMenuIndex[i] -= 1;
                }
                if (GUILayout.Button(">", EditorStyles.miniButton, GUILayout.Width(17)) && af.ex.extraScatterVarMenuIndex[i] < af.extraMenuNames.Count - 1)
                {
                    af.ex.extraScatterVarMenuIndex[i] += 1;
                }
                af.ex.extraScatterVarMenuIndex[i] = EditorGUILayout.Popup("", af.ex.extraScatterVarMenuIndex[i], af.extraMenuNames.ToArray(), GUILayout.Width(200));

                //-- Extra Var Fequency
                GUILayout.Space(7);
                EditorGUILayout.LabelField(new GUIContent("Freq.", ""), GUILayout.Width(30));
                af.ex.extraScatterVarFreq[i] = EditorGUILayout.Slider(af.ex.extraScatterVarFreq[i], 0, 1, GUILayout.Width(105));

                GUILayout.Space(6);
                EditorGUILayout.LabelField(new GUIContent("Scale"), GUILayout.Width(35));
                EditorGUILayout.PropertyField(scaleProp, new GUIContent(""), GUILayout.Width(130));

                ////GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    af.ex.extraScatterVarIndex[i] = af.ConvertExtraMenuIndexToPrefabIndex(af.ex.extraScatterVarMenuIndex[i]);
                    scaleProp.vector3Value = ed.EnforceVectorMinMax(scaleProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f), new Vector3(100, 100, 100));
                    serializedObject.ApplyModifiedProperties();
                    af.ex.extraScatterVarIndex[i] = af.ConvertExtraMenuIndexToPrefabIndex(af.ex.extraScatterVarMenuIndex[i]);

                    af.ex.prefabVars[i] = af.extraPrefabs[af.ex.extraScatterVarIndex[i]];
                    af.RebuildExtraPool();
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.Space(7);
            }
            EditorGUILayout.EndVertical();*/

            EditorGUILayout.EndVertical();
        }

        EditorGUI.EndDisabledGroup();
    }//end Setup Extras */

    private void SetupQuickStartScatter()
    {
        if (quickStartTemplate == QuickStartTemplate.grid4x4PerPost)
        {
            ex.numGridX = numGridXProp.floatValue = 4;
            ex.numGridZ = numGridZProp.floatValue = 4;
            ex.gridWidth = gridWidthProp.floatValue = 2f;
            ex.extraFreqMode = ExtraPlacementMode.allPostPositions;
            extraFreqModeProp.intValue = (int)ex.extraFreqMode;
            ex.useRandomScatter = randomScatterProp.boolValue = false;
        }
        else if (quickStartTemplate == QuickStartTemplate.grid4x4PerNode)
        {
            ex.extraFreqMode = ExtraPlacementMode.nodePostPositions;
            extraFreqModeProp.intValue = (int)ex.extraFreqMode;
            ex.numGridX = numGridXProp.floatValue = 4;
            ex.numGridZ = numGridZProp.floatValue = 4;
            ex.gridWidth = gridWidthProp.floatValue = 2f;
            ex.useRandomScatter = randomScatterProp.boolValue = false;
        }
        else if (quickStartTemplate == QuickStartTemplate.scatterRandom)
        {
            ex.extraFreqMode = ExtraPlacementMode.nodePostPositions;
            extraFreqModeProp.intValue = (int)ex.extraFreqMode;
            ex.numGridX = numGridXProp.floatValue = 4;
            ex.numGridZ = numGridZProp.floatValue = 4;
            ex.gridWidth = gridWidthProp.floatValue = 2f;
            ex.useRandomScatter = randomScatterProp.boolValue = true;
        }
    }
}