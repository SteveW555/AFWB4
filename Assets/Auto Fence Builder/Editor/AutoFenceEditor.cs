/* Auto Fence & Wall Builder v3.5 twoclicktools@gmail.com February 2024  */
////#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
////#pragma warning disable 0414

using AFWB;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

using UnityEngine;


//--------------------------
[CustomEditor(typeof(AutoFenceCreator))]
public partial class AutoFenceEditor : Editor
{
    LayerSet kRailALayer = LayerSet.railALayer; // to save a lot of typing
    LayerSet kRailBLayer = LayerSet.railBLayer;
    LayerSet kPostLayer = LayerSet.postLayer;
    LayerSet kExtraLayer = LayerSet.extraLayer;
    LayerSet kSubpostLayer = LayerSet.subpostLayer;
    LayerSet kAllLayer = LayerSet.allLayer;
    public AutoFenceCreator af; // the main AutoFence script
    public SerializedProperty[] userPrefabRailProp = { null, null };
    public SerializedProperty userPrefabPostProp, userPrefabExtraProp, userPrefabProp, userPrefabPlaceholderProp;
    public SerializedProperty railACustomColliderMeshProp, railBCustomColliderMeshProp, postCustomColliderMeshProp, showOptionalPostPrefabsProp;
    public SerializedProperty extraCustomColliderMeshProp, subpostCustomColliderMeshProp;
    public SerializedProperty postImportScaleModeProp, railAImportScaleModeProp, railBImportScaleModeProp, extraImportScaleModeProp, importScaleModeProp;
    public SerializedProperty railAColliderModeProp, railBColliderModeProp, postColliderModeProp, extraColliderModeProp, subpostColliderModeProp;
    private SerializedProperty railASequencerProp, railBSequencerProp, postSequencerProp;
    private SerializedProperty railASeqListProp, railBSeqListProp, postSeqListProp;
    public SerializedProperty interPostDistProp, fenceHeight, fenceWidth, postSizeProp, postRotationProp, postHeightOffset, mainPostsSizeBoostProp, endPostsSizeBoostProp;
    public SerializedProperty exProp, extraSizeProp, extraPositionOffsetProp, extraRotationProp, extraFreq;
    public SerializedProperty numStackedRailsA, numStackedRailsB, railASizeProp, railBSizeProp, railAPositionOffsetProp, railARotationProp;
    public SerializedProperty railBPositionOffsetProp, railBRotationProp;
    private SerializedProperty useMainRails, useSecondaryRails, railASpread, railBSpread;
    public SerializedProperty subSpacing, useSubPosts, subpostScaleProp, subpostPositionOffsetProp, subpostRotationProp, useSubJoiners;
    private SerializedProperty roundingDistance;
    private SerializedProperty showControlsProp, closeLoopProp, switchControlsAlso;
    private SerializedProperty obj, globalLiftLower, extraGameObject;
    private SerializedProperty gs, scaleInterpolationAlso; //global scale
    public SerializedProperty keepInterpolatedPostsGrounded;
    public SerializedProperty lerpPostRotationAtCorners, lerpPostRotationAtCornersInters, hideInterpolated, snapMainPostsProp, snapSizeProp;
    private SerializedProperty jitterAmount;

    //===== Post Variaton Parameters ========
    private SerializedProperty postSpacingVariation, allowVertical180Invert_Post, allowMirroring_X_Post, allowMirroring_Z_Post;
    private SerializedProperty jitterPostVerts, mirrorXPostProbability, mirrorZPostProbability, verticalInvertPostProbability;

    //===== Rail Variaton Parameters ========
    private SerializedProperty allowMirroring_X_Rail, allowMirroring_Z_Rail, useRailASeq;
    private SerializedProperty jitterRailVerts, mirrorXRailProbability, mirrorZRailProbability, verticalInvertRailProbability;
    private SerializedProperty railVariation1, railVariation2, railVariation3, railADisplayVariationGOs;
    private SerializedProperty minRailHeightLimit, maxRailHeightLimit;
    private SerializedProperty minRailAHeightVar, maxRailAHeightVar, minRailBHeightVar, maxRailBHeightVar;
    public SerializedProperty railAProbArray, varRailAPositionOffset, varRailASize, varRailARotation;
    public SerializedProperty railBProbArray, varRailBPositionOffset, varRailBSize, varRailBRotation;
    private SerializedProperty varRailABackToFront, varRailAMirrorZ, varRailAInvert, varRailBBackToFront, varRailBMirrorZ, varRailBInvert;
    private SerializedProperty varRailABackToFrontBools, varRailAMirrorZBools, varRailAInvertBools;
    private SerializedProperty varRailBBackToFrontBools, varRailBMirrorZBools, varRailBInvertBools;
    public SerializedProperty railSingleVariantsProp, postSinglesList;
    private SerializedProperty railVariantsProp;
    public SerializedProperty quantizeRotIndexRailAProp, quantizeRotIndexRailBProp, quantizeRotIndexPostProp, quantizeRotIndexSubpostProp;
    public SerializedProperty quantizeRotProbPost, quantizeRotProbRailA, quantizeRotProbRailB, quantizeRotProbSubpost;
    public SerializedProperty allowPostRandomization, allowSubpostRandomization;
    public SerializedProperty autoRotateImports, autoScaleImports, componentToolbarProp;
    public SerializedProperty showRailAVariations, showRailBVariations, showPostVariations, showSubpostVariations, showGlobals;
    public SerializedProperty randRotAxisPost, randRotAxisSubpost, quantizeRotAxisRailA, quantizeRotAxisRailB;

    //===== Subpost SerializedProperty ========
    public SerializedProperty subpostDuplicateModeProp;

    private bool oldCloseLoop = false;
    protected bool prefabLoadOK = true, userUnloadedAssets = false;
    public string presetName = "Fence Preset_001";
    public string scriptablePresetName = "scriptablePresetName";
    public bool undone = false;
    public bool addedPostNow = false, deletedPostNow = false;



    public GUIStyle warningStyle, warningStyle2, warningStyleLarge, mildWarningStyle, infoStyle, infoStyleSmall, italicStyle, italicStyle2, lightGrayStyle, darkGrayStyle10, darkGrayStyle11;
    public GUIStyle mediumPopup, redWarningStyle, defaultButtonStyle, smallPopupStyle, tinyButtonStyle, grayHelpStyle;
    public GUIStyle smallLabel, small10Style, smallButtonStyle7, smallButtonStyle9, smallToolbarStyle, lilacUnityStyle, lilacPopupStyle;
    public GUIStyle label9Style, label10Style, label11Style, label12Style, label13Style, label14Style, smallOrangeItalicLabelStyle;
    public GUIStyle popup11Style, popup12Style, popup13Style, smallModuleLabelStyle;
    public GUIStyle greyStyle, lightGreyStyle2, italicHintStyle, italicHintStyleLight, moduleHeaderLabelStyle, popupLabelStyle, orangePopupStyle;
    public GUIStyle extrasNormalModeLabelStyle, extrasScatterModeLabelStyle;
    public GUIStyle unityBoldLabel, unityBoldLabelLarge, miniBold, smallBoldBlack, cyanBoldStyle, greenStyle2, cyanNonBoldStyle, cyanBoldStyleBigger, sty2, sty3;
    private GUIStyle boxUIStyle, boxUIGreenStyle, boxUIDarkCyanStyle, boxUIStyleNoBorder, boxUIStyleNoBorderLight, boxUIStyleNoBorderDark, boxUIStyleDark;
    public GUIStyle fatButt, biggerPopupStyle, borderedLabelStyle, tooltipStyle, greenPopupStyle;
    public GUIStyle smallToolbarButtonStyle;

    bool isDark = true;
    bool showBatchingHelp = false, showRefreshAndUnloadHelp = false;
    public bool displayRailsA = true, displayRailsB = false;
    int selGridInt = 0;
    string[] variationModeToolbarStrings = { "Quick Optimal Variation", "Random Variation", "Sequenced" };
    string[] railSetToolbarStrings = { "Rails Main Layer A", "Rails Secondary Layer B" };
    static string railATooltipString = "Show settings for main Rails sourceLayerList. \n" +
        "Layer A can be toggled on/off while using Layer B, by Control-Clicking the Toolbar Button";
    static string railBTooltipString = "Use this when you want to add a secondary Rail design in conjunction with Rail Layer A. \n" +
        "Layer B can be toggled on/off while using Layer A, by Control-Clicking the Toolbar Button";
    GUIContent[] railsetToolbarGUIContent = {new GUIContent("Rails Main Layer A", "This is the default Rail that is used where only one Rail design is required"),
        new GUIContent("Rails Secondary Layer B", railBTooltipString)};

    GUIContent[] globalsToolbarRow1_GUIContent = {  new GUIContent("Scale & Raise/Lower", "Scale everything. Raise or lower everything"),
        new GUIContent("Smoothing", "Round corners"),
        new GUIContent("Snap, Close, Reverse", "Snap to Unity World Grid, Close loop of fence"),
        new GUIContent("Layers", "Set Ignore Colliders Layer")

        };
    GUIContent[] globalsToolbarRow2_GUIContent = {  new GUIContent("Cloning", "Clone other fence"), new GUIContent("Combine", "Combine Finished Fence Meshes"),
                                                    new GUIContent("Resources", "Reload or offload prefabs & prefabs, Save Meshes. Show Triangle Counts"),
                                                    new GUIContent("Settings", "Parenting, Layer Numbers, Gaps, LOD"),
                                                    new GUIContent("Colliders", "Set Colliders") ,
                                                    new GUIContent("Debug", ""),
                                                    new GUIContent("Dev", "Housekeeping")
    };
    GUIContent[] componentToobarContent = {  new GUIContent("Posts Controls", "Show Posts Controls"),
                                                new GUIContent("Rails A Controls", "Show Rails A Controls"),
                                                new GUIContent("Rails B Controls", "Show Rails B Controls"),
                                                new GUIContent("Subpost Controls", "Show Subposts Controls"),
                                                     new GUIContent("Extras Controls", "Show Extras Controls"),

    };
    //-------------------------------

    int varASeqPopup = 0, varBSeqPopup = 0;
    int randomizeUserSeq = 0, optimaToUserSeq = 0;
    int prefabToRenamePrefabIndex = 0, prefabToRenameMenuIndex = 0;
    int variationDisplayChoice = 0;

    public int launchPresetIndex = -1, frameCount = 0;
    public int currSeqPostStepIndex = 1, varPostSeqPopup = 0;
    public int[] currSeqRailStepIndex = { 1, 1 };
    //-------------------------------

    string postsStr = "Posts", postsHelpStr = "Click to Disable Posts";
    string randomizeUserSeqString = "Randomize All Steps", optimalToUserSeqString = "Replace All Steps With Optimal";
    string randomToUserSeqString = "Replace From Random Mode";
    string oldNameForPrefab = "OldName", newNameForPrefab = "NewName";
    string subpostStr = "Subposts", subpostHelpStr = "Click to Disable Subposts";
    string extrasStr = "Extra", extrasHelpStr = "Click to Disable Extra";
    string[] variationDisplayAmountStrings = { "Show All Variations", "Show Active Only", "Show None" };
    static string sp = "Show parameters for this Step";

    public string railAStr = "Rail A", railAHelpStr = "Click to Disable Rail A";
    public string railBStr = "Rail B", railBHelpStr = "Click to Disable Rail B";
    public string prefixForFinished = "[AFWB]";
    public string[] prefixStrings = { "[]", "[AFWB]", "[Wall]", "[Fence]", "[Done]", "[Finished]", "Assign..." };
    public string[] quantizeRotStrings = { "15", "30", "45", "60", "90", "120", "180", "Fix 90", "Fix 180", "Consecutive 90" };
    public string presetSaveRename = "", userFenceName = "";
    public string[] randomScopeStrings = { "Main Only", "Variations Only", "Main&Variations" };
    public List<string> presetMenuNames = new List<string>();
    //-------------------------------

    bool respanAfterGlobalScale = true;
    private bool showNodeInfo = true;
    private bool openFinishControls = false;
    internal bool showSinglesHelp;

    public bool foundEnabledRailA = true, foundEnabledRailB = true, foundEnabledPost = true;
    public bool showSinglesRailA = true, showSinglesRailB = true, showSinglesPost = true;
    public bool showVarHelp = false, showSeqHelp = false;
    public bool removingAssets = false;
    public bool useBreakpoint = false;
    public bool rotationsWindowIsOpen = false;
    public bool showingUnlockMouseFromAFButton = false;

    //------   Colors   ---------
    //-- Assigned in EditorUtilitiesAf.SetupColors()
    public Color darkGrey, grey20percent_80alpha;
    private Color grey50alpha;
    public Color UILineGrey, UILineLightGrey, UILineDarkGrey;
    public Color switchGreen, switchRed, seedGreen, seedGreenTextCol;
    public Color switchGreenSceneView;
    public Color sceneViewBoxColor, sceneViewBoxColorMoreOpaque, sceneViewBoxBorderColor;
    public Color lineColor;
    public Color darkCyan, darkMagenta;
    public Color darkRed, darkerRed;
    public Color transRed;
    public Color panelBg, panelBorder, panelGreenBg, panelGreenBorder, panelDarkCyanBorder;
    public Color uiLineGreyCol, uiLineGreyCol2, uiLineGreyCol3;
    public Color defaultBackgroundColor;
    public Color lightBlueBackgroundColor;
    public Color lightYellowBackgroundColor;
    public Color lightRedBackgroundColor;
    public Color midGrayColor;
    public Color lightGrayColor;
    public Color bgTrans2;
    public Color topScreenButtonsCol;
    //-------------------------------

    private Texture2D tooltipImage;
    private LayerSet currViewedLayer;
    public List<ScriptablePresetAFWB> mainPresetList = new List<ScriptablePresetAFWB>();
    public static List<ScriptablePresetAFWB> mainPresetListStaticCopy = new List<ScriptablePresetAFWB>();
    public Texture2D bgBoxTex, bgBoxSmallTex, bgBoxLargeTex, bgGreenBoxTex, bgDarkCyanBoxTex, shapesTex, saveTex;
    public ScriptablePresetAFWB currPreset = null;
    public SeqItem currSeqAStepVariant, currSeqBStepVariant, currSeqPostStepVariant;
    public PrefabLoader prefabLoader;
    public GUIContent[] seqToolbarGUIContent = new GUIContent[2];
    Timer unlockMouseButtonTimer = new Timer("Unlock Mouse");
    public ScriptablePresetAFWB defaultPreset = null;

    //-------------------------------


    //      Scene View
    //=======================
    private bool showNothingWorking;
    private bool showSectionIndices = false;

    Vector2 dblClickScreenPoint = new Vector2(0, 0);

    //-- These are used to check if the Prefabs folder is changed
    private HashSet<string> cachedExistingPrefabPaths = new HashSet<string>();
    private bool isCacheInitialized = false;

    //      Editors
    //=======================
    //   These are linked in SetupLinkedEditorClasses 
    public PostEditor postEd;
    public RailEditor railEd;
    public ExtraEditor exEd;
    public SubpostEditor subEd;
    public PrefabAssignEditor prefabAssignEd;
    public ResourceUtilities resEd;
    public VariationsEditor varEd;
    public SequenceEditor seqEd;
    public RandomizationEditor randEd;
    public SinglesEditor singlesEd;

    //   Other Linked Helper Classes
    //==================================
    public PresetFiles presetsEd;
    private SceneViewDebugDisplay sceneDebug;
    private SceneViewContextMethods sceneViewContextMethods;
    public LinksToAssetFolder assetFolderLinks;
    private GUIStyle boxUIDarkYellowStyle;
    private Color panelDarkYellowBorder;
    private Texture2D bgDarkYellowBoxTex;
    private int modPrefabTypeIndex = 0;
    private string renameSourceSubtring = "Hatstand";
    private string renameNewSubstring = "Blimp";
    private string presetPrefabName;
    private float minPostToPostDistance;
    private string meshName;
    private bool drawEditingGizmo = false;
    private Vector3 editingGizmoPos = Vector3.zero;
    private bool isLeftButtonOn = true, isRightButtonOn = false;
    private bool showSwitches = false;
    private Vector2 switchPosition;
    private bool isUseGameObjectMenuVisible = false;

    //private static GizmoDrawManager instance;

    //====================================================
    //Useful to be able to re-check or call while debugging
    void LinkTargetAF()
    {
        if (af == null)
            af = (AutoFenceCreator)target;
        af = (AutoFenceCreator)target;
        af.afwbActive = true;

    }

    //--------------------------
    void Awake()
    {
        //StackLog(this.GetType().Name);
        //-- Order on new AutoFence:               af.Awake, af.Reset, ed.Awake, af.SetupAutoFenceDEpendencies, af.ResetAutoFence ed.OnEnable [->SetupEditor]
        //-- Order on Re-Selecting in Hierarchy:   ed.Awake,  af.SetupAutoFenceDependencies,  ed.OnEnable [-> SetupEditor -> ForceRebuildFromClickPoints]
        //-- Order on re-compile                   ed.OnEnable [->SetupEditor]

        LinkTargetAF();
        //__ Ensure the Current Fences folder and component folders exist first, some setup relies on it
        af.CheckFoldersBuilt();
        //__ Check, and keep a record of their location paths, which can be changed if the user has moved the Master AFWB folder
        CheckFolderLocations(false);
        SetupStyles();
        LoadGUITextures(); // Loads an image file as a Texture2D. needs to know folder locations so call after checking folders
        //explain how I can itegrate it into my existing project which has
    }

    //---------------------------------------
    ///-- Called on recompile, Re-selection in Hierarchy and and on first launch
    void OnEnable()
    {
        //EditorApplication.update += OnCustomUpdate; // -- add a cusom method that will be called every frame

        //-- Enure the postVectors have a reference to their master parents List
        PostVector.LinkPostVectorParentList(af.postVectors);
        InitializeGizmoDrawManager();

        //StackLog(this.GetType().Name);
        //Due to Unity's sequence of initialization, particularly with regard to Editor GUI stuff being ready
        //it's good to do a sanity check here. Prefer this to EditorApplication.delayCall for now. SetupStyles() is originally called in Awake()
        if (unityBoldLabel == null)
        {
            Debug.Log("unityBoldLabel was null in OnEnable() so reinitializing the GUIStyles\n");
            SetupStyles();
        }

        //-- Do the Setup in its own method so we can forcibly call it if ever needed
        SetupEditor();
        af.PrintPoolDebugInfo("End of SetupEditor", false, noPrintOnSuccess: false);

        af.CheckPostDirectionVectors(logMissing: true);
        af.ForceRebuildFromClickPoints();

        //af.PrintSourceVariantGOsForLayer(LayerSet.railALayer);
        // This shouldn't be necessary if the List<SourceVariant> is serializable, but for some reason
        // after a re-compile, it can forget. It seems others who have used an array of a List of custom classes containing
        // GameObjects have seen this. Anyway for .001ms it takes, here we are

        //EditorApplication.update += CheckContextMenuVisibility;
        //EditorApplication.update += DrawSwitches;
        
    }
    private void OnDisable()
    {
        //EditorApplication.update -= OnCustomUpdate;
        af.afwbActive = false;
        //EditorApplication.update -= CheckContextMenuVisibility;
        //EditorApplication.update -= DrawSwitches;
    }
    //-------------------------------------------
    //Called by OnEnable()
    void SetupEditor()
    {
        if (userUnloadedAssets == true || prefabLoadOK == false) // AFWB will not function until the user reloads it.
        {
            Debug.Log("Auto Fence Builder:  Unloading Assets. Please reload Auto Fence Builder in the Hierarchy");
            return;
        }
        af = (AutoFenceCreator)target;
        af.afwbActive = true;

        //Debug.Log(af.currPresetIndex);

        //-- Ensure Folders exist, and their locations are stored and correct
        ManageFolders();

        //-- Important to deal with folders before this as it needs them to load Textures

        //Finds and Caches SeriealizedProperties for the most used properties and fields
        SetupSerializedProperties();

        //-- Setup the other Editors for the Sequencer, Randomization, Singles, etc
        SetupLinkedEditorClasses();

        //-- At this point we don't care about  Preset/Variations choices, just that Prefabs and SourceVariants are all valid and non-null
        bool prefabsChanged = CheckPrefabsAndOptionallyReload(reload: true, warn: false);

        bool presetsChanged = CheckPresetsAndOptionReload(reload: true, warn: false);

        bool layerPrefabsChanged = CheckAllLayerPrefabsExist();
        bool sourceVariantsChanged = CheckSourceVariantsExist();

        //CheckPostsExist(1); // Debug.Don't delete

        //-- Don't do anything in AutoFenceCreator until we know everything is setup and loaded
        af.SetupAutoFenceDependencies(rebuildFolders: false); // -> init layerSeeds

        AutoFenceCreator seedsAF = af.railASeeds.af;


        SetAndSyncInitialPresetIndices();

        //-- Make sure SourceVariation GOs are assigned
        if (af.clickPoints.Count > 0)
            af.LowerPostsToGround(af.clickPoints);
        tooltipImage = EditorGUIUtility.Load("Assets/Auto Fence Builder/Editor/Images/KeepRowLevelOn.jpg") as Texture2D;


        if (af.initialReset == false && prefabLoadOK == true)
            af.ResetAutoFence(resetFenceParameters: false);

        af.ValidateAndUpdatePools();

        List<Transform> postPool = af.GetPoolForLayer(kPostLayer);

        af.PrintPoolDebugInfo("End of SetupEditor", false, noPrintOnSuccess: false);


        //-- Dealy the Seed init until after presets have loaded in case an old preset without them is loaded
        af.InitializeAllSeededValues();

        af.ValidateAllSeeds();
        CheckForNewPrefabs();

        af.userPrefabPlaceholder = PrefabLoader.LoadPrefabNamed("Drag GameObj From Hrchy", af.systemFilesDir);
        ResetUserPrefabPlaceholder();

    }

    public SerializedProperty ResetUserPrefabPlaceholder()
    {
        userPrefabPlaceholderProp = serializedObject.FindProperty("userPrefabPlaceholder");
        return userPrefabPlaceholderProp;
    }

    //-------------------------------------------
    void ManageFolders()
    {
        // make sure the Current Fences folder and component folders exist
        af.CheckFoldersBuilt();
        //-- Check, and keep a record of their location paths, which can be changed if the user has moved the Master AFWB folder
        CheckFolderLocations(false);
    }
    //---------------------------------------
    //-- Link all the aux Editor classes to this Editor
    protected void SetupLinkedEditorClasses()
    {
        prefabAssignEd = new PrefabAssignEditor(af, this);
        postEd = new PostEditor(af, this, prefabAssignEd);
        railEd = new RailEditor(af, this, prefabAssignEd);
        subEd = new SubpostEditor(af, this, prefabAssignEd, serializedObject);
        exEd = new ExtraEditor(af, this, prefabAssignEd, af.ex, exProp);
        resEd = new ResourceUtilities(af, this);
        varEd = new VariationsEditor(af, this);
        seqEd = new SequenceEditor(af, this);
        assetFolderLinks = new LinksToAssetFolder(af, this);
        exProp = serializedObject.FindProperty("ex");
        randEd = new RandomizationEditor(af, this);
        singlesEd = new SinglesEditor(af, this, LayerSet.railALayer);

        presetsEd = new PresetFiles(af, this);
        sceneDebug = new SceneViewDebugDisplay(af, this);
        sceneViewContextMethods = new SceneViewContextMethods(af, this);

        Housekeeping.LinkAutoFenceCreatorToHousekeeping(af, this);
    }
    //---------------------------------------
    // Called From:
    // EditorUtilitiesAF.CheckPrefabsAndOptionallyReload()
    // AutoFenceEditor.OnInspectorAssetsCheck()
    // AutoFenceEditor.ReloadPrefabsAndPresets()
    public void LoadPrefabs()
    { //Debug.Log("LoadPrefabs()\n");

        af.ClearAllPrefabs();

        if (prefabLoader == null)
            prefabLoader = new PrefabLoader(af);

        prefabLoadOK = prefabLoader.LoadAllPrefabLayers(this);


        af.needsReloading = false;
        userUnloadedAssets = false;
        if (prefabLoadOK)
        {
            af.BackupPrefabMeshes(af.railPrefabs, af.origRailPrefabMeshes);
            af.BackupPrefabMeshes(af.postPrefabs, af.origPostPrefabMeshes);

            //=============================================================
            //      Create The Prefab Menu Names with Categories
            //=============================================================

            af.CreatePrefabMenuNames();
        }
        else
            Debug.LogWarning("Auto Fence Builder:  Prefabs folder not found. Please reload Auto Fence Builder in the Hierarchy");
    }

    //=============================================================================
    public bool ShowFinishPanel()
    {
        GUI.backgroundColor = new Color(0.92f, 1.0f, .92f, 1);
        //GUILayout.BeginVertical("Box");
        bool didFinish = false;
        using (var verticalScope = new GUILayout.VerticalScope("box"))
        {
            GUILayout.Space(10);
            EditorStyles.label.wordWrap = true;
            string finishStr = "Finish & Start New";
            EditorGUILayout.LabelField(finishStr, moduleHeaderLabelStyle);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("If you want the Finished Fence folder to be parented to an object in the Hierarchy," +
                "drag the parent object here, else it will be placed in the top level of the Hierarchy", GUILayout.Width(400), GUILayout.Height(50));
            EditorStyles.label.wordWrap = false;
            af.finishedFoldersParent = EditorGUILayout.ObjectField(af.finishedFoldersParent, typeof(Transform), true, GUILayout.Width(200)) as Transform;
            GUILayout.Space(15);

            EditorGUI.BeginChangeCheck();

            //======================================
            //          Finish Merged 
            //======================================
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("finishMerged"), new GUIContent(""), GUILayout.Width(20));
            EditorGUILayout.LabelField(new GUIContent("Merge Meshes in Finished Fence", "Choose to merge the meshes or keep them as seperate items in" +
                " the Finished build"));
            GUILayout.EndHorizontal();

            if (serializedObject.FindProperty("finishMerged").boolValue == true)
            {
                if (af.usePostsLayer)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("createMergedPostsCollider"),
                        new GUIContent("               Add Merged Posts Mesh Collider", "Create Mesh Collider"));
                if (af.useRailLayer[0])
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("createMergedRailACollider"),
                        new GUIContent("               Add Merged Rails A Mesh Collider", "Create Mesh Collider"));
                if (af.useRailLayer[1])
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("createMergedRailBCollider"),
                        new GUIContent("               Add Merged Rails B Mesh Collider", "Create Mesh Collider"));
                if (af.useSubpostsLayer && af.subpostsPool.Count > 0)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("createMergedSubpostsCollider"),
                        new GUIContent("               Add Merged Subposts Mesh Collider", "Create Mesh Collider"));
                if (af.useExtrasLayer && af.ex.extrasPool.Count > 0)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("createMergedExtrasCollider"),
                        new GUIContent("               Add Merged Extras Mesh Collider", "Create Mesh Collider"));
                GUILayout.Space(10);
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                af.SetClickMarkersActiveStatus(showControlsProp.boolValue);
            }

            //====   Duplicate Checkbox  =====
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("finishDuplicate"), new GUIContent(""), GUILayout.Width(20));
            EditorGUILayout.LabelField(new GUIContent("Duplicate & Keep Live Fence Active", "This will create a Finished Build, while also keeping the" +
                " current live session. As they are in the same position, the Finished version is disabled. You can choose to move the position" +
                "of the live fence, or the Finished"));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                af.SetClickMarkersActiveStatus(showControlsProp.boolValue);
            }

            string defaultFenceName = "[AF] " + af.presetSaveName, fenceName = "";
            if (openFinishControls == false)
            {
                userFenceName = defaultFenceName;
            }
            EditorGUILayout.LabelField("Fence Name:", GUILayout.Width(75));
            userFenceName = EditorGUILayout.TextField(userFenceName, GUILayout.Width(300));
            EditorGUI.FocusTextInControl("NameTextField");
            GUILayout.Space(10);

            openFinishControls = true;

            GUI.backgroundColor = Color.white;

            GUILayout.BeginHorizontal();

            //======================================
            //          Create Finished
            //======================================
            if (GUILayout.Button("OK", GUILayout.Width(110)))
            {
                FinishedFenceUtilitiesEditor.CreateFinishedFromCurrent(af, userFenceName);

                didFinish = true;
                openFinishControls = false;

                //TODO Possible bug with Progress bar
                if (af.finishMerged == false)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20);
                    GUIUtility.ExitGUI();
                }
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                openFinishControls = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }
        GUILayout.Space(20);
        EditorStyles.label.wordWrap = false;

        return didFinish;
    }
    //=============================================================================
    // This resets and reloads everything, except the clickpoint Nodes. So should look identical after a rebuild
    // There's an issue possibly related to invisible Marker objects which can get multiple colliders compounded
    // after a crash of Unity, or an unfortunate AFWB SNAFU. It's similar to removing and re-adding AFWB, but keeps your layout data.
    // It's on the TODO list, although having this has proved generally useful
    // as a sanity check, so we can rule out AFWB as the cause of any problem.
    public void CleanAndRebuild()
    {
        //Debug.Log("CleanAndRebuild():  \n");
        DestroyImmediate(af.currentFencesFolder);
        af.SetupFolders();
        ReloadPrefabs(rebuild: false);
        ReloadPresets(rebuild: false);
        af.ResetAllPools();
        SetupEditor();
        af.ForceRebuildFromClickPoints();

        SceneView.RepaintAll();
    }

    //========================================================================================================================
    //========================================================================================================================
    //
    //                                  OnInspectorGUI()
    //
    //=======================================================================================================================
    //========================================================================================================================
    public override void OnInspectorGUI()
    {
        af.onInspectorGuiTime = 0;
        Timer t = new Timer("OnInspectorGUI");

        //StackLog(this.GetType().Name);

        //-- Useful for testing things that only need checking periodically, eg. every frameFreq frames
        frameCount++;
        CheckPeriodicallyFromOnInspectorGUI(frameFreq: 300);

        //==============================================
        //      TestButton - Comment Out for Release
        //==============================================
        /*if (GUILayout.Button("Test"))
        {
            //ProcGenAFWB.Test(.4f, 5, 4f);
            af.CheckNodePositions();
        }*/

        //====================================
        //      Test 4 - Comment Out for Release
        //======================================
        /*if (GUILayout.Button("Sim Reset"))
        {
            SimulateFirstEnable();
        }*/

        // Completely block use, if user has chosen to unload assets to optimize buildSize, or if FencePrefabs folder is missing
        if (OnInspectorAssetsCheck() == false)
            return;
        //Debug.Log("frameCount  " + frameCount);

        defaultButtonStyle = new GUIStyle(GUI.skin.button);
        Color defaultBackgroundColor = GUI.backgroundColor; //???

        smallPopupStyle = new GUIStyle(EditorStyles.popup);
        mediumPopup = new GUIStyle(EditorStyles.popup);
        serializedObject.Update(); // updates serialized ed from the main script

        tinyButtonStyle = new GUIStyle();
        tinyButtonStyle.fontSize = 10;

        if (varEd == null)
            SetupLinkedEditorClasses();


        if (Event.current.keyCode == KeyCode.Escape)// cancels a ClearAll
            af.clearAllFencesWarning = 0;


        TidyUp();

        //==================================
        //      Finish And Start New
        //==================================
        GUILayout.BeginVertical("box");
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Space(160);
        if (((openFinishControls == true) || GUILayout.Button(new GUIContent("Finish & Start New", "This will complete the building by converting and copying the fence into a regular set of " +
            "Game Objects in the Hierarchy, and then clearing AFWB to start with a new construction. The Finished fence can be reloaded and edited again later if required."),
            /*sty3,*/ GUILayout.Width(260))) && af.clickPoints.Count > 0)
        {
            ShowFinishPanel();
        }

        GUILayout.Space(10);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        //GUILayout.EndVertical();

        //==================================
        //          Button Row
        //==================================
        if (openFinishControls == false)
        {
            GUILayout.Space(10);
            int buttSpacing = 15;
            GUILayout.BeginHorizontal();
            GUILayout.Space(buttSpacing);
            EditorStyles.label.wordWrap = false;
            //============================
            //   Clean & Rebuild
            //============================
            if (GUILayout.Button(new GUIContent("Clean & Rebuild", "Clears all sections, reloads, and rebuilds. " +
                "\nCan help with hidden or orphaned assets, or rogue colliders \nUseful if things get a bit too interesting" +
                "\n\nSafe to do this anytime, no build details will be affected."), sty2, GUILayout.Width(103)))
            {
                CleanAndRebuild();
            }
            GUILayout.Space(buttSpacing);

            //====================================
            //      Reverse
            //======================================
            if (GUILayout.Button(new GUIContent("Reverse Fence",
                "Reverses the order of your click-point nodes. This will also make all objects face 180 the other way."), sty2, GUILayout.Width(100)))
            {
                showControlsProp.boolValue = af.showControls = false;
                ReverseClickPoints();
            }
            GUILayout.Space(buttSpacing);

            //============================================
            //      Set Placement from Clickpoints file
            //============================================
            //====  For version 4.1  =======
            /*if (GUILayout.Button(new GUIContent("Reverse Fence",
                "Drag & Drop a Clickpoints file here to set the layout of your fence"), sty2, GUILayout.Width(100)))
            {
                ScriptableClickPoints clickpointsFile = null;
                EditorGUILayout.PropertyField(clickpointsFile, new GUIContent(""), GUILayout.Width(150));
            }
            GUILayout.Space(buttSpacing);*/

            //============================
            //      Snapping
            //============================
            string snapHelpStr = "OFF";
            string snapButtStr = "Enable Snap";
            int snapButtWidth = 80;
            if (af.snapMainPosts)
            {
                snapButtStr = "Snap";
                snapHelpStr = "ON";
                GUI.backgroundColor = new Color(.65f, .65f, .65f);
                snapButtStr += " (" + af.snapSize.ToString("F1") + ")";
                snapButtWidth = 80;
            }
            if (GUILayout.Button(new GUIContent(snapButtStr, "Snap is " + snapHelpStr + ".\nSnaps the position of main click-point node postsPool. " +
                "Settings can be found in Globals.\n\nControl-click Snap button to quickly set to 1.0m \n\n If you need to snap intermediate postsPool," +
                " convert them into Click-points by control-right-clicking on them, or insert a new Click-point in that position " +
                "with control-shift-click"), sty2, GUILayout.Width(snapButtWidth)))
            {

                if (Event.current.control == true)
                {
                    af.snapSize = 1.0f;
                    af.snapMainPosts = snapMainPostsProp.boolValue = true;
                }
                else
                {
                    snapMainPostsProp.boolValue = !snapMainPostsProp.boolValue;
                    af.snapMainPosts = snapMainPostsProp.boolValue;
                    af.ForceRebuildFromClickPoints();
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(buttSpacing);

            //============================
            //      Close Loop
            //============================
            string loopStr = "OFF";
            if (af.closeLoop)
            {
                loopStr = "ON";
                GUI.backgroundColor = new Color(.65f, .65f, .65f);
            }
            if (GUILayout.Button(new GUIContent("Close Loop", "Close Loop is " + loopStr + ".\nBuilds sections to complete a loop back to the starting point"), sty2, GUILayout.Width(75)))
            {
                closeLoopProp.boolValue = !closeLoopProp.boolValue;
                af.closeLoop = closeLoopProp.boolValue;
                af.ManageCloseLoop(af.closeLoop);
                af.ForceRebuildFromClickPoints();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(buttSpacing);

            //========================
            //      Clear All
            //========================
            if (GUILayout.Button(new GUIContent("Clear All", "Clears the current build. Settings are not affected"), sty2, GUILayout.Width(68)) && af.clickPoints.Count > 0)
            {
                if (af.clearAllFencesWarning == 1)
                {
                    af.ClearAllFences();
                    af.clearAllFencesWarning = 0;
                }
                else
                    af.clearAllFencesWarning = 1;
            }
            if (af.clearAllFencesWarning == 1)
            {
                GUILayout.EndHorizontal();
                EditorGUILayout.LabelField("   ** This will clear all the fence parts currently being built. (Design parameters are not affected)", warningStyle);
                EditorGUILayout.LabelField("      Press [Clear All] again to continue or Escape Key to cancel **", warningStyle);
                af.clearAllFencesWarning = 1;
            }
            else
                GUILayout.EndHorizontal();
        }
        GUILayout.Space(4);
        GUILayout.EndVertical();
        GUI.backgroundColor = Color.white;

        //================================
        //      Describe Help Tooltips
        //================================
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(new GUIContent("   Comprehensive Tooltip Help is available for every Label and Control"
            , "The pdf manual has been removed as both Unity and AFWB are changing rapidly. Hover over any Label or Control with the mouse pointer. Please contact support or leave a message in the Auto Fence forum if " +
            "you need more details about a particular control or workflow. Also check 'Auto Fence Builder' on YouTube as more videos are being added. Thanks "), GUILayout.Width(500));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //==============================================================================================================================
        //
        //                                              Master Settings
        //
        //==============================================================================================================================
        GUILayout.Space(3);
        EditorGUIUtility.labelWidth = 0;
        //GUILayout.Space(10);
        if (isDark)
        {
            boxUIStyle.normal.background = bgBoxTex;
            GUILayout.BeginVertical(boxUIStyle);
            GUILayout.BeginVertical();
        }
        else
        {
            GUILayout.BeginVertical();
            GUILayout.BeginVertical("box");
        }

        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
        {
            EditorGUILayout.LabelField("", GUILayout.Width(112));
            EditorGUILayout.LabelField("------    Master Settings    ------  ", cyanBoldStyle, GUILayout.Width(210));
            AFWB_HelpText.ShowMasterHelp(horizontalScope, cyanBoldStyle, 50);
            GUILayout.Space(85);
        }
        GUILayout.Space(3);
        EditorGUI.BeginChangeCheck();

        //===================================
        //      Show Control Nodes 
        //===================================
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(showControlsProp, new GUIContent(""), GUILayout.Width(20));
        EditorGUILayout.LabelField(new GUIContent("Show Control Post Handles in Scene", "Display dragable handles in Scene View to adjust Click-point Post positions"));
        GUILayout.EndHorizontal();

        //-- Click Guide
        EditorGUILayout.LabelField("    [ ADD Post: shift-click       INSERT: ctrl-shift-click       GAP: Shift-Right-Click       DELETE:Ctrl-Left-Click node ]", italicHintStyle);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.SetClickMarkersActiveStatus(showControlsProp.boolValue);
        }
        GUILayout.Space(10); GUILayout.Space(10);

        //====================================================
        //      Post-Rail Spacing : Inter-section Distance 
        //====================================================

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(new GUIContent("Post-Rail Spacing:", "The distance between each interpolated post/rail. \n\n" +
            "This is a target Size. E.g. if your fence is 10m long and you request a distance of 3m, " +
            "the nearest value to give a whole number of sections would be 3.333 (x 3 sections = 10m). This calculated value is shown in parentheses." +
            "\n\nThe default value is 3m" +
            "\n\nNote: Rails are automatically scaled to span the correct 'Post-Rail Spacing' between Clickpoint Nodes, but be aware that if you have placed" +
                    " two Nodes say, 2m apart, then the Rail (and other elements) will be scaled down to fit in that span."), GUILayout.Width(126));

        //-- Section Length Slider
        EditorGUILayout.PropertyField(interPostDistProp, new GUIContent(""), GUILayout.Width(100));

        if (interPostDistProp.floatValue < 0.2f)
            interPostDistProp.floatValue = 0.2f;

        string actualDistStr = "(" + af.actualInterPostDistance.ToString("F1") + ")";
        EditorGUILayout.LabelField(new GUIContent(actualDistStr, actualDistStr + " is the closest useable value to " + af.interPostDist
                                     + " needed to create a whole number of sections"), lightGrayStyle, GUILayout.Width(30));

        //-- Section Length Reset
        if (GUILayout.Button(new GUIContent("R", "Reset Interpost distance to 3.0"), EditorStyles.miniButton, GUILayout.Width(20)))
        {
            af.baseInterPostDistance = 3.0f;
            af.interPostDist = af.baseInterPostDistance * af.globalHeight;
            af.interpolate = true;
            af.keepInterpolatedPostsGrounded = true;
            af.postSpacingVariation = postSpacingVariation.floatValue = 0;
            af.ForceRebuildFromClickPoints();
        }

        //===================================
        //      Random Spacing 
        //===================================

        EditorGUI.BeginChangeCheck();
        GUILayout.Space(15);
        EditorGUILayout.LabelField(new GUIContent("Random Spacing", "Recommended to leave this at 0 until the final step of design " +
            "as it greatly impacts all other settings. \n\n " +
            "Randomizes the length of each inter - post section, does not affect your clicked points. \n\n Default 0"), GUILayout.Width(100));

        //==  Slider  ==
        EditorGUILayout.PropertyField(postSpacingVariation, new GUIContent(""), GUILayout.Width(115));

        //==  Seed Random Spacing  ==
        if (GUILayout.Button(new GUIContent("S", "Re-randomize the spacing with a new Seed"), EditorStyles.miniButton, GUILayout.Width(20)))
        {
            af.postAndGlobalSeeds.SeedGlobalSpacing();
        }

        //==  Reset Random Spacing  ==
        if (GUILayout.Button(new GUIContent("R", "Reset Random Spacing Variation to 0"), EditorStyles.miniButton, GUILayout.Width(20)))
        {
            af.postSpacingVariation = postSpacingVariation.floatValue = 0;
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(2);
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        #region Smooth
        //==================================================
        //                 Smooth
        //===================================================
        GUILayout.BeginVertical("box");
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();//+
        GUIStyle subHeadingStyle = new GUIStyle(EditorStyles.label);// { alignment = TextAnchor.LowerCenter };
        subHeadingStyle.fontSize = 13;
        EditorGUILayout.LabelField(new GUIContent("Smooth",
            "Creates smooth curves along the path of the fence by adding inbetween posts" +
            "\n\n To start, try pressing [Default] which will Reset all values to reasonable defaults for this fence"), subHeadingStyle, GUILayout.Width(60));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("smooth"), new GUIContent(""), GUILayout.Width(30));
        EditorGUILayout.LabelField("", GUILayout.Width(2));
        if (serializedObject.FindProperty("smooth").boolValue == true)
        {
            //      Rounding DistanceTCT
            //===========================
            EditorGUIUtility.labelWidth = 115;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("roundingDistance"), new GUIContent("Rounding DistanceTCT",
                "Reduce this to increase smoothing - more smoothed interpolated sections are added." +
                "\n\nA good starting point is the Post-Rail Spacing -1 : The Smoothing needs to be able to place more closely-spaced posts, " +
                "but could look inconsistent or cramped if they're very close together" +
                "\n\nIf too dense, try increasing the Cull controls to thin out the interpolated GameObjects, " +
                "or if necessary increase Post-Rail Spacing. " +
                "\n\n If it's difficult to find perfect settings, insert an extra Clickpoint-Post in the curve which you can poosition precisely. " +

                "\n\n Default Rounding DistanceTCT is 2"), GUILayout.Width(263));
            //--Tension
            GUILayout.Space(14);
            EditorGUIUtility.labelWidth = 52;
            //      Tension
            //===========================
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tension"), new GUIContent("Tension",
                "This is an interpolation between Smoothed and Unsmoothed " +
                "Default value is 0 - fully smoothed."), GUILayout.Width(209));
            //GUILayout.FlexibleSpace();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();//+

            //-------------------------------
            GUILayout.Space(2);
            using (var h = new EditorGUILayout.HorizontalScope())
            {
                //  Reset
                if (GUILayout.Button(new GUIContent("Default", "Reset All Smoothing Controls to values suitable for this fence"), EditorStyles.miniButton, GUILayout.Width(55)))
                {
                    serializedObject.FindProperty("roundingDistance").floatValue = 2;
                    af.stripTooClose = serializedObject.FindProperty("tension").floatValue = 0;
                    af.stripTooClose = serializedObject.FindProperty("removeIfLessThanAngle").floatValue = 7;
                    float closeValue = af.interPostDist * 0.4f;
                    af.stripTooClose = serializedObject.FindProperty("stripTooClose").floatValue = closeValue;
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.Space(43);

                //      Cull Angle
                //===========================
                EditorGUILayout.LabelField("Cull Angle", GUILayout.Width(114));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("removeIfLessThanAngle"), new GUIContent("",
                    "Remove unnecessary postsPool where there is little curvature"), GUILayout.Width(146));
                GUILayout.Space(14);
                //      Cull Close
                //===========================
                EditorGUILayout.LabelField("Cull Close", GUILayout.Width(68));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stripTooClose"), new GUIContent("",
                    "Remove any sections that are closer together than this value. " +
                    "\n\nYou can think of this as a mimimum length of each section." +
                    "\n\nA good starting point is approximately half the Post-Rail Spacing value." +
                    "\n\nIncrease Rounding DistanceTCT if the density is too high."), GUILayout.Width(138));
            }
        }
        else
            GUILayout.EndHorizontal();//+
        EditorGUIUtility.labelWidth = 0;
        GUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            if (serializedObject.FindProperty("roundingDistance").floatValue > interPostDistProp.floatValue * 2)
                serializedObject.FindProperty("roundingDistance").floatValue = interPostDistProp.floatValue * 2;
            if (serializedObject.FindProperty("stripTooClose").floatValue > interPostDistProp.floatValue)
                serializedObject.FindProperty("stripTooClose").floatValue = interPostDistProp.floatValue;
            serializedObject.ApplyModifiedProperties();
            af.ForceRebuildFromClickPoints();
        }
        af.baseInterPostDistance = af.interPostDist / af.globalHeight;

        GUILayout.EndVertical();

        if (af.smooth == false)
            GUILayout.Space(2); //was 10
        GUILayout.EndVertical();
        #endregion


        int vertSpace = 16;
        if (af.smooth == false)
            vertSpace = 36;
        GUILayout.Space(vertSpace);

        //============================================================================================================================
        //
        //                         Presets:      Choose or Save Scriptable Main Fence/Wall Preset
        //
        //============================================================================================================================
        ShowPresetsUI(this);

        GUILayout.Space(28);

        GUI.backgroundColor = defaultBackgroundColor;

        //================================================================================================================
        //
        //                                        Component Part Switches
        //
        //================================================================================================================
        if (isDark)
        {
            //boxUIStyle.normal.background = bgBoxLargeTex;
            boxUIDarkCyanStyle.normal.background = bgDarkCyanBoxTex;
            GUILayout.BeginVertical(boxUIDarkCyanStyle);
            GUILayout.BeginVertical();
        }
        else
        {
            GUILayout.BeginVertical();
            GUILayout.BeginVertical("box");
        }
        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
        {
            EditorGUILayout.LabelField("", GUILayout.Width(160), GUILayout.Height(22));
            EditorGUILayout.LabelField("Components", cyanBoldStyle, GUILayout.Width(90));
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawPreviewTexture(new Rect(rect.x - 50, rect.y, 21, 18), shapesTex);
            GUILayout.Space(10);
            //GUI.backgroundColor = switchRed;
            AFWB_HelpText.ShowComponentsHelp(horizontalScope, cyanBoldStyle, 110);
            GUILayout.Space(88);
        }
        GUILayout.Space(12);
        GUILayout.BeginHorizontal();
        int buttonWidth = 108;
        EditorGUILayout.LabelField("Enable:", GUILayout.Width(49));

        postsStr = "Posts"; postsHelpStr = "Click to Disable Posts";

        if (af.usePostsLayer == false)
        {
            postsStr = "Posts (off)";
            postsHelpStr = "Click to Enable Posts";
            GUI.backgroundColor = switchRed;
        }
        else
            GUI.backgroundColor = switchGreen;
        if (GUILayout.Button(new GUIContent(postsStr, postsHelpStr), EditorStyles.miniButton, GUILayout.Width(buttonWidth)))
        {
            af.usePostsLayer = !af.usePostsLayer;
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.usePostsLayer)
                af.componentToolbar = ComponentToolbar.posts;
        }
        //----------------
        railAStr = "Rail A"; railAHelpStr = "Click to Disable Rail A";
        if (af.useRailLayer[0] == false)
        {
            railAStr = "Rail A (off)";
            railAHelpStr = "Click to Enable Rail A";
            GUI.backgroundColor = switchRed;
        }
        else
            GUI.backgroundColor = switchGreen;
        if (GUILayout.Button(new GUIContent(railAStr, railAHelpStr), EditorStyles.miniButton, GUILayout.Width(buttonWidth)))
        {
            af.useRailLayer[0] = !af.useRailLayer[0];
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.useRailLayer[0])
                af.componentToolbar = ComponentToolbar.railsA;
        }
        //----------------
        railBStr = "Rail B"; railBHelpStr = "Click to Disable Rail B";
        if (af.useRailLayer[1] == false)
        {
            railBStr = "Rail B (off)";
            railBHelpStr = "Click to Enable Rail B";
            GUI.backgroundColor = switchRed;
        }
        else
            GUI.backgroundColor = switchGreen;
        if (GUILayout.Button(new GUIContent(railBStr, railBHelpStr), EditorStyles.miniButton, GUILayout.Width(buttonWidth)))
        {
            af.useRailLayer[1] = !af.useRailLayer[1];
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.useRailLayer[1])
                af.componentToolbar = ComponentToolbar.railsB;
        }
        //---------------------------
        subpostStr = "Subposts"; subpostHelpStr = "Click to Disable Subposts";
        if (af.useSubpostsLayer == false)
        {
            subpostStr = "Subpost (off)";
            subpostHelpStr = "Click to Enable Subpost";
            GUI.backgroundColor = switchRed;
        }
        else
            GUI.backgroundColor = switchGreen;
        if (GUILayout.Button(new GUIContent(subpostStr, subpostHelpStr), EditorStyles.miniButton, GUILayout.Width(buttonWidth)))
        {
            af.useSubpostsLayer = !af.useSubpostsLayer;
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso)
                af.componentToolbar = ComponentToolbar.subposts;
        }
        //----------------
        extrasStr = "Extra"; extrasHelpStr = "Click to Disable Extra";
        if (af.useExtrasLayer == false)
        {
            extrasStr = "Extra (off)";
            extrasHelpStr = "Click to Enable Extra";
            GUI.backgroundColor = switchRed;
        }
        else
            GUI.backgroundColor = switchGreen;

        if (GUILayout.Button(new GUIContent(extrasStr, extrasHelpStr), EditorStyles.miniButton, GUILayout.Width(buttonWidth)))
        {
            af.useExtrasLayer = !af.useExtrasLayer;
            af.ForceRebuildFromClickPoints();
            if (af.switchControlsAlso && af.useExtrasLayer)
                af.componentToolbar = ComponentToolbar.extras;
        }
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);

        //========================================================================
        //                      "Show" Layers Toolbar
        //========================================================================
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Show:", GUILayout.Width(48));
        af.componentToolbar = (ComponentToolbar)GUILayout.Toolbar((int)af.componentToolbar, componentToobarContent,
            GUILayout.Width(548));
        componentToolbarProp.enumValueIndex = (int)af.componentToolbar;
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            currViewedLayer = GetLayerSetFromToolbarChoice();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(7);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 2), uiLineGreyCol);

        //=================================================================
        //
        //                     Posts
        //
        //=================================================================
        if (af.componentToolbar == ComponentToolbar.posts)
        {
            postEd.ShowPostEditor(serializedObject);
            EditorGUI.BeginDisabledGroup(af.usePostsLayer == false);

            //        Posts Randomization 
            //-------------------------------------
            GUILayout.Space(7);
            bool randEnabled = randEd.SetupRandomization(LayerSet.postLayer);

            //         Posts Variation  
            //-------------------------------------
            postEd.ShowSetupVariationsSources(serializedObject);
        }

        //=================================================================
        //                                                      
        //                        Rails A                     
        //                                                             
        //=================================================================
        if (af.componentToolbar == ComponentToolbar.railsA)
        {
            railEd.ShowRailEditor(serializedObject, LayerSet.railALayer);

            //        Rail A Randomization 
            //-------------------------------------   
            GUILayout.Space(7);
            bool randEnabled = randEd.SetupRandomization(kRailALayer);

            //         Rails A Variation  
            //-------------------------------------
            railEd.ShowSetupVariationsSources(serializedObject, LayerSet.railALayer);
        }

        //=================================================================
        //                                                      
        //                        Rails B                    
        //                                                             
        //=================================================================
        if (af.componentToolbar == ComponentToolbar.railsB)
        {
            railEd.ShowRailEditor(serializedObject, LayerSet.railBLayer);

            //        Rail B Randomization 
            //-------------------------------------   
            GUILayout.Space(7);
            bool randEnabled = randEd.SetupRandomization(kRailBLayer);

            //         Rails B Variation  
            //-------------------------------------
            railEd.ShowSetupVariationsSources(serializedObject, LayerSet.railBLayer);
        }

        //=================================================================
        //
        //                        Subpost Options
        //
        //=================================================================
        if (af.componentToolbar == ComponentToolbar.subposts)
        {
            subEd.ShowSubpostEditor();
        }

        //=================================================================
        //
        //                        Extra Game Object Options
        //
        //=================================================================
        if (af.componentToolbar == ComponentToolbar.extras)
        {
            //======================================================
            //              Main Extras Editor  
            //======================================================
            exEd.SetupExtras(serializedObject);
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        GUI.backgroundColor = defaultBackgroundColor;
        EditorGUI.EndDisabledGroup();

        //====================================================================================================
        //
        //                                          Globals
        //
        //====================================================================================================
        GUILayout.Space(20);

        ManageGlobals();

        af.onInspectorGuiTime = t.End(print: false);

    }

    //====================================================================================
    //                              End of OnInspectorGUI()
    //====================================================================================
    // Note: Don't call this for OnEnable as Unity has a specific sequence in which it reinitializes the ed environment
    // after a script compilation,a nd the GUI styles may not be available when OnEnable is invoked.

    //------------------------------------------
    //      Check Periodically From OnInspectorGUI
    public bool OnInspectorAssetsCheck()
    {
        if (userUnloadedAssets == true || prefabLoadOK == false)
        {
            GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
            if (prefabLoadOK == false)
            {
                EditorGUILayout.LabelField("Missing FencePrefabs Folder. It must be at Assets/Auto Fence Builder/FencePrefabs/");
                EditorGUILayout.LabelField("Please relocate this folder or re-import Auto Fence & Wall Builder");
                if (GUILayout.Button("Retry", GUILayout.Width(200)))
                {
                    prefabLoadOK = true; // assume it's true before retrying
                    LoadPrefabs();
                    Debug.Log("AFWB Loaded Prefabs \n");
                }
            }
            else
            {
                EditorGUILayout.LabelField("You have Unloaded all AFWB Assets to optimize Build Size.", warningStyle);
                EditorGUILayout.LabelField("To continue using AFWB, press Reload below.", warningStyle);
                if (GUILayout.Button("Reload Auto Fence & Wall Builder", GUILayout.Width(200)))
                {
                    ReloadPrefabs();
                    ReloadPresets();
                    userUnloadedAssets = false;
                }
            }
            GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
            return false;
        }
        return true;
    }


    private Vector3 DrawSmallerPositionHandle(Vector3 position, Quaternion? rotation = null)
    {
        // Use the provided rotation or default to Quaternion.identity
        Quaternion effectiveRotation = rotation ?? Quaternion.identity;

        // Define the scale factor and handle color
        float scaleFactor = 0.5f;
        Color handleColor = new Color(0.5f, 0.5f, 0.5f); // Dark gray

        // Save the original handle matrix and color
        Matrix4x4 originalMatrix = Handles.matrix;
        Color originalColor = Handles.color;

        // Set the new handle matrix and color
        Handles.matrix = Matrix4x4.TRS(position, effectiveRotation, Vector3.one * scaleFactor);
        Handles.color = handleColor;

        // Draw the position handle
        Vector3 newPosition = Handles.PositionHandle(Vector3.zero, Quaternion.identity);

        // Restore the original handle matrix and color
        Handles.matrix = originalMatrix;
        Handles.color = originalColor;

        // Return the adjusted position
        return position + newPosition;
    }
    /*private void DrawSwitches()
    {
        Debug.Log("DrawSwitches\n");
        if(showSwitches == false)
            return;

        Handles.BeginGUI();
        float buttonWidth = 80;
        Rect leftButtonRect = new Rect(switchPosition.x, switchPosition.y, buttonWidth, 30);
        Rect rightButtonRect = new Rect(switchPosition.x + buttonWidth, switchPosition.y, buttonWidth, 30);

        if (GUI.Button(leftButtonRect, isLeftButtonOn ? "Left On" : "Left"))
        {
            isLeftButtonOn = true;
            isRightButtonOn = false;
        }

        if (GUI.Button(rightButtonRect, isRightButtonOn ? "Right On" : "Right"))
        {
            isRightButtonOn = true;
            isLeftButtonOn = false;
        }

        Handles.EndGUI();
    }*/

    //===========================================================================================================
    //
    //                                          OnSceneGUI                   
    //
    //===========================================================================================================
    // Display a GUI control with a Color depending on a bool value
    public void SetControlBackgroundColorFromBool(bool on, Color col, Color bg = default)
    {
        if (on == true)
            GUI.backgroundColor = col;
        else
            GUI.backgroundColor = bg;
    }
    #region OnSceneGUI
    void OnSceneGUI()
    {
        Event currEvent = Event.current;
        if (af != null && af.gameObject != null)
            Selection.activeGameObject = af.gameObject; // Sanity check: keep Auto Fence Builder object selected so we can see the inspector. TODO: Unlock this if we need to select other objects




        //return;     // It's not for us!


        //-- Completely block use, if user has chosen to unload assets to optimize build Size
        if (userUnloadedAssets == true)
        {
            GUI.Label(new Rect(200, Screen.height - 197, 450, 20), "" + "Assets Are Unloaded!", unityBoldLabelLarge);
            return;
        }
        Timer t = new Timer("OnSceneGUI");
        //StackLog(this.GetType().Name, true);

        Color originalUnityBgColor = GUI.backgroundColor;
        af.mouseHoveringOverIgnoreLayer = false;

        /* if (currEvent.control == true && currEvent.alt == false && currEvent.type == EventType.MouseDown && currEvent.button == 1)
         {
             showSwitches = true;
             switchPosition = currEvent.mousePosition;
             //go = UseGameObjectAsAutoFence(currEvent, rayPosition, go);
         }
         if (showSwitches)
         {
             DrawSwitches(switchPosition);
         }*/

        //===================================================================================
        //                  Show Buttons, Controls, Gizmos, and  Info
        //===================================================================================
        Handles.BeginGUI(); //Begin a 2D GUI block
        EditorGUI.BeginChangeCheck();

        //=====================================
        //     Show Gizmo Controls & Markers
        //=====================================
        int helpBoxHeight, helpBoxWidth, x, y, boxYPos, boxTextYPos;
        ShowGizmoControls(out helpBoxHeight, out helpBoxWidth, out x, out y);

        //===============================
        //  Left Buttons Column
        //===============================
        GUILayout.Space(Screen.height - 850);
        if (af.showDebugInfo == true)
            originalUnityBgColor = LeftButtonsColumn();

        //===============================
        //  Top Screen Buttons
        //===============================
        TopScreenButtons();
        GUI.backgroundColor = new Color(.5f, .5f, .5f);
        GUI.backgroundColor = originalUnityBgColor;



        //===========================================
        //     Show Debug Info And Bottom Buttons 
        //===========================================
        float toolsBoxWidth = helpBoxWidth;
        if (af.showDebugInfo)
            toolsBoxWidth = 880;
        boxYPos = Screen.height - 70 - 30;
        boxTextYPos = Screen.height - 98;
        Handles.DrawSolidRectangleWithOutline(new Rect(x, y - 28, toolsBoxWidth, helpBoxHeight), sceneViewBoxColor, sceneViewBoxBorderColor);
        af.showDebugInfo = GUI.Toggle(new Rect(6, (boxYPos + 2), 120, 20), af.showDebugInfo,
            new GUIContent("Print Debug Details", "Show Utilities to use in Scene View. Only available when a Fence has started to be built."));
        if (af.showDebugInfo == true)
        {
            originalUnityBgColor = ShowLowerDebugToolbar(boxTextYPos, firstButtonXPos: 102);

            sceneDebug.ShowSceneViewDebugInfoPanel(LayerSet.railALayer);
            sceneDebug.ShowFenceLabels();
            sceneDebug.ShowNodeDistances();
            sceneDebug.ShowStepNumbers();
            sceneDebug.ShowPostPositions();
        }

        if (EditorGUI.EndChangeCheck())
        {
            af.ForceRebuildFromClickPoints();
        }

        //  Draw the 'Empty Fence' Background Box
        //==========================================
        ShowEmptyFenceInfoBox();

        //  Draw the Log Comments Background Box
        //==========================================
        ShowLogComments();




        Handles.EndGUI();


        af.CheckFoldersBuilt();

        Vector3 clickPoint = Vector3.zero;
        int shiftRightClickAddGap = 0; // use 0 instead of a boolean so we can store int flags in clickPointFlags


        GameObject go = null;
        bool isClickPoint;
        int[] sectionIndexForLayers = { -1, -1, -1 }; //[0] = index of railA, [1] = index of railB, [2] = index of post. (in LayerSet enum order)
        LayerSet hoveredLayer = LayerSet.None;
        RaycastHit hit;

        //    The Master Ray Position used by all other methods of OnSceneGUI
        //=========================================================================
        Ray rayPosition = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);

        bool isMouseInSceneView;
        if (currEvent.mousePosition.x > 0 && currEvent.mousePosition.x < Screen.width && currEvent.mousePosition.y > 0 && currEvent.mousePosition.y < Screen.height)
            isMouseInSceneView = true;
        else
            return;

        //=====================================
        //      Handle dragging & controls
        //=====================================
        //-- Moved 9/6/24 to ensure gizmos drawn behind text for better visibility
        HandleDragAndControls(currEvent);
        AssignStepIndexForVariations(currEvent, sectionIndexForLayers, hoveredLayer, go);






        //=======================================================
        //          Mouse Hover 
        //=======================================================
        go = MouseHover(helpBoxHeight, rayPosition, out hoveredLayer, out isClickPoint);
        bool overFenceLayer = hoveredLayer.IsFence();
        bool variationsEnabledForLayer = af.GetUseVariationsForLayer(hoveredLayer);

        if (overFenceLayer && go == null)
            af.mouseHoveringOverIgnoreLayer = true;

        if (overFenceLayer && currEvent.alt)
        {
            //Debug.Log("Alt key pressed\n");
            //currEvent.Use();
        }
        if (currEvent.alt == true && currEvent.control == false && currEvent.type == EventType.MouseDown && currEvent.clickCount == 2 && currEvent.button == 0)
        {
            Debug.Log("Double-clicked on Fence to Enable Scene Editing");
            drawEditingGizmo = true;
            //ToggleNodeGizmos();
            // consume the event so that it doesn't get passed on to other methods
            currEvent.Use();
        }

        if (overFenceLayer && go != null)
        {
            editingGizmoPos = go.transform.position;
            editingGizmoPos.y = af.GetNodeMarkerPosition(0).y;
        }

        if (drawEditingGizmo)
            DrawSmallerPositionHandle(editingGizmoPos);

        //============================================
        //          Double-click shortcuts
        //============================================
        ShowHideControlsViaDoubleClick(currEvent, hoveredLayer);
        UnlockMouse(currEvent, hoveredLayer);
        GoToPrefabInFolderFromSceneView(currEvent, hoveredLayer);

        //EnableSceneEditing(currEvent, hoveredLayer);

        //============================================
        //          Add Post/ClickPoint
        //============================================
        int layerMask = AddClickPoint(currEvent, rayPosition, ref clickPoint, shiftRightClickAddGap);
        if (Event.current.type == EventType.MouseMove)
            SceneView.RepaintAll();

        //=======================================================
        //          Delete Click Point
        //=======================================================
        DeleteClickPoint(currEvent, rayPosition);

        //=================================================================
        //          Toggle Gap Status of Post (control-right-click) 
        //=================================================================
        bool togglingGaps = ToggleGapStatusOfPost(currEvent);

        //=======================================================
        //          Add Gap (Shift-Right-Click
        //=======================================================
        //some redundant checking, but need to make this extra visible for maintainence, as control-click has two very different effects. 
        if (togglingGaps == false && currEvent.button == 1 && currEvent.shift && !currEvent.control && currEvent.type == EventType.MouseDown)
        {
            shiftRightClickAddGap = 1;// we're inserting a new clickPoint, but as a break/gap.  TODO ?? recheckCloseLoop = true;
        }

        //=====================================
        //          Create Vectors for Preview Lines
        //=====================================
        CreateVectorsForPreviewLines(currEvent, rayPosition, ref clickPoint, shiftRightClickAddGap, layerMask);

        //=====================================
        //          Insert Post
        //=====================================
        InsertPost(currEvent, isMouseInSceneView);

        //=====================================
        //          Switch Toolbar Component View
        //=====================================
        SwitchToolbarComponentViewOnClick(currEvent, hoveredLayer);

        //===================================================================
        //                      Handle Right-Click :
        //===================================================================

        //      Menus Etc.
        //===================
        hit = HandleRightClick(currEvent, rayPosition, ref go, isClickPoint, sectionIndexForLayers, hoveredLayer);

        //    Control Right-Click  :  Get GameObject to Use as Post
        //===================================================================

        /*if (showSwitches == true)
        {
            DrawSwitches();
        }*/

        if (currEvent.control == true && currEvent.alt == false && currEvent.type == EventType.MouseDown && currEvent.button == 1)
        {
            showSwitches = true;
            isUseGameObjectMenuVisible = true;
            switchPosition = currEvent.mousePosition;
            go = UseGameObjectAsAutoFence(this, currEvent, rayPosition, go);
        }

        //=======================================================================



        /*Timer ti = new Timer("OnSceneGUI - Raycast");
        GameObject hitObject = RaycastForGameObject(rayPosition);
        ti.End(print: true);
        if (hitObject != null)
        {
            Debug.Log($"Hit Object: {hitObject.name}\n");
            // You can display more info about the GameObject here
        }*/


        float sceneViewTime = t.End(print: false);
        Handles.BeginGUI();
        if (af.showDebugInfo)
            GUILayout.Label($"SceneView Time: {sceneViewTime.ToString("F2")} ms", GUILayout.Width(150));
        //GUI.Label(new Rect(0, 400, 300, 40), $"Extra Build Time: {sceneViewTime.ToString("F2")} ms");
        Handles.EndGUI();
    }
    /*private void CheckContextMenuVisibility()
    {
        // Reset the context menu visibility flag if the context menu loses focus
        if (isUseGameObjectMenuVisible && (EditorWindow.focusedWindow == null || EditorWindow.focusedWindow.GetType() != typeof(GenericMenu)))
        {
            isUseGameObjectMenuVisible = false;
            Debug.Log($"isUseGameObjectMenuVisible = {isUseGameObjectMenuVisible}\n");
            showSwitches = false;
        }
    }*/
    //-------------------
    private static GameObject RaycastForGameObject(Ray worldRay)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                if (bounds.IntersectRay(worldRay))
                {
                    return obj;
                }
            }
        }
        return null;
    }
    //----------------------------------
    /// <summary>Shows gizmo controls and adjusts help box dimensions and position.</summary>
    /// <remarks>af.showControls will set ther status of SetClickMarkersActiveStatus() which in turn will
    /// decide if the yellow markers are later drawn.</remarks>
    private void ShowGizmoControls(out int helpBoxHeight, out int helpBoxWidth, out int x, out int y)
    {
        helpBoxHeight = 24;
        helpBoxWidth = 150;
        x = 1;
        y = Screen.height - 72;
        int boxYPos = Screen.height - 72, boxTextYPos = Screen.height - 68;
        int helpButtWidth = 72, helpButtPosX = 154;

        Handles.DrawSolidRectangleWithOutline(new Rect(x, y, helpBoxWidth, helpBoxHeight), sceneViewBoxColor, sceneViewBoxBorderColor);
        af.showControls = GUI.Toggle(new Rect(6, (boxYPos + 2), 160, 20), af.showControls, new GUIContent("Show Control Nodes",
            "Show Moveable Gizmos at your Clickpoint Nodes. Use to reposition the nodes. " +
            "\nRight-click on a non-node post to create a Post there, or Shift-Control-Click to Insert a new node"));

        string helpStr = "Show Help";
        if (af.showHelp == true && af.showControls == true)
        {
            helpStr = "Hide Help";
            helpBoxWidth = 1085;
            GUI.Label(new Rect(helpButtPosX + helpButtWidth + 20, (boxTextYPos), 853, 20),
            " ADD POST:  shift-click   |   INSERT:  ctrl-shift-click   |   GAP:  shift-right-click   |   DELETE:  ctrl-click Post    | " +
            "  OPTIONS:  ctrl-right-click Post/Rail");
        }

        //-- Show the gizmo keyboard shortcuts guide
        if (af.showControls == true && GUI.Button(new Rect(helpButtPosX, boxTextYPos, helpButtWidth, 20), helpStr))
            af.showHelp = !af.showHelp;
    }
    //---------------------
    private void ShowLogComments()
    {
        af.showLogComments = true;
        //af.logComment = "Testing the log comment box";
        if (af.showLogComments)
        {
            float w = 500, posX = Screen.width - (w + 2);
            int h = 24, posY = Screen.height - (h);
            posX = 5; posY = Screen.height - 150;
            Handles.DrawSolidRectangleWithOutline(new Rect(posX, posY, w, h), new Color(.2f, .2f, .2f, .7f), sceneViewBoxBorderColor);
            GUI.Label(new Rect(posX + 2, posY, w - 4, h - 2), af.logComment);
        }
    }


    private void ShowEmptyFenceInfoBox()
    {
        GUI.backgroundColor = Color.black;
        int w = 520, posX = (Screen.width - w) / 2;
        if (af.allPostPositions.Count == 0) // show as empty if nothing built yet
        {
            Handles.DrawSolidRectangleWithOutline(new Rect(posX, Screen.height - 200, 510, 42), grey20percent_80alpha, sceneViewBoxBorderColor);
            GUI.Label(new Rect(posX + 35, Screen.height - 198, 450, 20), "" +
                " Empty Fence  -  Shift-Click on Terrain or on Surface with Collider", unityBoldLabelLarge);
            GUI.Label(new Rect(posX + 130, Screen.height - 180, 450, 20), "" +
                "[ Ensure Unity Gizmos are Enabled ]");
        }
    }
    //-----------------------------------------------------------
    private void TopScreenButtons()
    {
        //      Reverse
        //========================
        float buttonWidth = 80;
        float buttonHeight = 17f;
        float buttonPadding = 10f;
        Rect buttonRect = new Rect(Screen.width - buttonWidth - buttonPadding - 280, buttonPadding, buttonWidth, buttonHeight);
        // DrawTCT the button
        GUI.backgroundColor = topScreenButtonsCol;
        if (GUI.Button(buttonRect, new GUIContent("Reverse", "Reverses the order of the Clickpoint Nodes, this has the effect of turning the wall back to front." +
            "\n\nNote:  If the objects have a symetrical design, there may be no apparent visual difference.")))
        {
            //showControlsProp.boolValue = af.showControlsProp = false;
            ReverseClickPoints();
            af.ForceRebuildFromClickPoints();
        }

        //      Snap
        //========================
        string snapStr = "Snap is Off";
        buttonRect = new Rect(Screen.width - buttonWidth - buttonPadding - 190, buttonPadding, buttonWidth, buttonHeight);
        // DrawTCT the button
        GUI.backgroundColor = topScreenButtonsCol;
        if (af.snapMainPosts == true)
        {
            snapStr = "Snap is On";
            GUI.backgroundColor = new Color(.1f, .9f, .1f, 0.5f);
        }
        if (GUI.Button(buttonRect, new GUIContent(snapStr, "Snap is " + af.snapMainPosts.ToString() + ".\nSnaps the position of main click-point node postsPool. " +
                "Settings can be found in Globals.\n\nControl-click Snap button to quickly set to 1.0m \n\n If you need to snap intermediate postsPool," +
                " convert them into Click-points by control-right-clicking on them, or insert a new Click-point in that position " +
                "with control-shift-click")))
        {
            if (Event.current.control == true)
            {
                af.snapSize = 1.0f;
                af.snapMainPosts = snapMainPostsProp.boolValue = true;
            }
            else
            {
                snapMainPostsProp.boolValue = !snapMainPostsProp.boolValue;
                af.snapMainPosts = snapMainPostsProp.boolValue;
                af.ForceRebuildFromClickPoints();
            }
        }

        //      Close Loop
        //========================
        GUI.backgroundColor = topScreenButtonsCol;
        buttonRect = new Rect(Screen.width - buttonWidth - buttonPadding - 100, buttonPadding, buttonWidth, buttonHeight);
        string closeStr = "Close Loop";
        if (af.closeLoop)
        {
            GUI.backgroundColor = new Color(.1f, .9f, .1f, 0.5f);
            closeStr = "Loop Closed";
        }
        // DrawTCT the button
        if (GUI.Button(buttonRect, new GUIContent(closeStr, "Adds an extra Clickpoint Post at the end to close a loop on your fence layout")))
        {
            closeLoopProp.boolValue = !closeLoopProp.boolValue;
            af.closeLoop = closeLoopProp.boolValue;
            af.ManageCloseLoop(af.closeLoop);
            af.ForceRebuildFromClickPoints();
        }

        GUI.backgroundColor = topScreenButtonsCol;

        //       Clear All  
        //========================
        EditorGUI.BeginDisabledGroup(af.clickPoints.Count < 1);
        if (Event.current.keyCode == KeyCode.Escape)// cancels a ClearAll
            af.clearAllFencesWarning = 0;

        if (af.clickPoints.Count > 0)
        {
            buttonRect.x = buttonRect.x - 380;
            buttonRect.width = 90;
            if (GUI.Button(buttonRect, new GUIContent("Clear All", "Clears the current build layout. Settings are not affected")))
            {
                if (af.clearAllFencesWarning == 1)
                {
                    af.ClearAllFences();
                    af.clearAllFencesWarning = 0;
                }
                else
                    af.clearAllFencesWarning = 1;
            }
            if (af.clearAllFencesWarning == 1)
            {
                float rectY = 100;
                Handles.DrawSolidRectangleWithOutline(new Rect(100, rectY, 750, 45), new Color(0f, 0f, 0f, 0.65f), new Color(0f, 0f, 0f, 0.8f));
                GUI.Label(new Rect(100, rectY, 750, 20), "     This will clear all the fence parts currently being built. (Design parameters are not affected)", warningStyleLarge);
                GUI.Label(new Rect(100, rectY + 20, 600, 20), "     Press [Clear All] again to continue or Escape Key to cancel ", warningStyleLarge);
                af.clearAllFencesWarning = 1;
            }
        }

        EditorGUI.EndDisabledGroup();
    }

    //--------------------------------------------------
    private Color LeftButtonsColumn()
    {
        Color originalUnityBgColor = Color.white;
        if (GUILayoutExtensions.ButtonAutoWidth("Rebuild"))
        {
            af.ForceRebuildFromClickPoints();
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Clean & Rebuild", "Clears all sections, reloads, and rebuilds. " +
                "\nCan help with hidden or orphaned assets, or rogue colliders \nUseful if things get a bit too interesting" +
                "\n\nSafe to do this anytime, no build details will be affected."))
        {
            af.ClearConsole();
            CleanAndRebuild();
        }
        GUILayout.Space(8);

        //if ( GUILayoutExtensions.ButtonAutoWidth("Print Post Dir Vectors",  GUILayout.Width(  GUI.skin.button.CalcSize(new GUIContent("Print Post Dir Vectors")).x ) ) )
        if (GUILayoutExtensions.ButtonAutoWidth("Print Post Dir Vectors"))
        {
            af.ClearConsole();
            PostVector.PrintPostVectors();
        }

        if (GUILayoutExtensions.ButtonAutoWidth("Print Pools"))
        {
            af.ClearConsole();
            af.PrintPoolInfo();
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Print Source Variants"))
        {
            af.ClearConsole();
            af.PrintSourceVariantGOsForLayer(currViewedLayer, activeOnly: false);
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Print Variant Menus"))
        {
            af.ClearConsole();
            af.PrintSourceVariantMenuGos(currViewedLayer, activeOnly: true);
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Print Seq Steps"))
        {
            af.ClearConsole();
            af.PrintSeqStepGOs(currViewedLayer, activeOnly: true);
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Print Singles"))
        {
            af.ClearConsole();
            af.singlesContainer.PrintSinglesForLayer(currViewedLayer, inUseOnly: false);
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Toggle Use Variations"))
        {
            // Switch off all seq and singles in case there's a problem
            // and rebuild so we can get the inspector back
            af.ToggleUseVariationsForLayer(LayerSet.railALayer);
            af.ToggleUseVariationsForLayer(LayerSet.railBLayer);
            af.ToggleUseVariationsForLayer(LayerSet.postLayer);
            af.ResetAllPools();
            af.ForceRebuildFromClickPoints();
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Toggle Use Sequencer"))
        {
            af.ToggleUseSequencerForLayer(LayerSet.railALayer);
            af.ToggleUseVariationsForLayer(LayerSet.railBLayer);
            af.ToggleUseVariationsForLayer(LayerSet.postLayer);
            af.ResetAllPools();
            af.ForceRebuildFromClickPoints();
        }
        if (GUILayoutExtensions.ButtonAutoWidth("Toggle Use Singles"))
        {
            af.ToggleUseSinglesForLayer(LayerSet.railALayer);
            af.ToggleUseSinglesForLayer(LayerSet.railBLayer);
            af.ToggleUseSinglesForLayer(LayerSet.postLayer);
            af.ResetAllPools();
            af.ForceRebuildFromClickPoints();
        }

        //======  Show Section Index  ======
        if (GUILayoutExtensions.ButtonAutoWidth("Show Section Index"))
        {
            showSectionIndices = !showSectionIndices;
        }
        if (showSectionIndices)
            sceneDebug.ShowSectionIndices();
        originalUnityBgColor = GUI.backgroundColor;


        if (GUILayoutExtensions.ButtonAutoWidth("Post Markers"))
        {
            //af.db.showPostVizMarkers = !af.db.showPostVizMarkers;
        }

        //SetControlBackgroundColorFromBool(af.db.showExtraMarkers, Color.green);
        if (GUILayoutExtensions.ButtonAutoWidth("Extra Markers"))
        {
            //af.db.showExtraMarkers = !af.db.showExtraMarkers;
        }
        //===============================
        //  Extra Polyrects
        //===============================
        //SetControlBackgroundColorFromBool(af.db.showExtraPolyrects, Color.green);
        if (GUILayoutExtensions.ButtonAutoWidth("Extra Grid Rects"))
        {
            //af.db.showExtraPolyrects = !af.db.showExtraPolyrects;
        }

        //===============================
        //  Direction Vectors
        //===============================
        GUILayout.Space(10);

        SetControlBackgroundColorFromBool(af.showOverlapZones, switchGreenSceneView, bgTrans2);
        if (GUILayout.Button("Show Overlap Zones", GUILayout.Width(140)))
        {
            af.showOverlapZones = !af.showOverlapZones;
        }

        SetControlBackgroundColorFromBool(af.showFillZones, switchGreenSceneView, bgTrans2);
        if (GUILayout.Button("Show Fill Zones", GUILayout.Width(140)))
        {
            af.showFillZones = !af.showFillZones;
        }

        SetControlBackgroundColorFromBool(af.showOtherMarkers, switchGreenSceneView, bgTrans2);
        if (GUILayout.Button("Show Other Markers", GUILayout.Width(140)))
        {
            af.showOtherMarkers = !af.showOtherMarkers;
        }

        SetControlBackgroundColorFromBool(af.showPostDirectionVectors, switchGreenSceneView, bgTrans2);
        if (GUILayout.Button("Post Direction Vectors", GUILayout.Width(140)))
        {
            af.showPostDirectionVectors = !af.showPostDirectionVectors;
        }

        SetControlBackgroundColorFromBool(af.showPostDirectionVectorsRight, switchGreenSceneView, bgTrans2);
        if (GUILayout.Button("Post Direction Vectors Right", GUILayout.Width(170)))
        {
            af.showPostDirectionVectorsRight = !af.showPostDirectionVectorsRight;
        }
        GUILayout.Space(10);

        GUI.backgroundColor = Color.white;

        //===============================
        //        Timers
        //===============================


        GUILayout.Label("OnInspectorGui Time: " + af.onInspectorGuiTime.ToString("F2") + "ms", GUILayout.Width(160));
        GUILayout.Label("Build Time: " + af.buildTime.ToString("F2") + "ms", GUILayout.Width(120)); //-- This is ForceRebuildFromClickPoints time
        GUILayout.Label("Post Build Time: " + af.postBuildTime.ToString("F2") + "ms", GUILayout.Width(140));
        GUILayout.Label("Rail Build Time: " + af.railBuildTime.ToString("F2") + "ms", GUILayout.Width(140));
        GUILayout.Label("Extra Build Time: " + af.extraBuildTime.ToString("F2") + "ms", GUILayout.Width(150));
        GUILayout.Label($"Extras Built:  {af.ex.extrasBuiltCount},    Extras Pool:  {af.ex.extrasPool.Count}", GUILayout.Width(220));
        return originalUnityBgColor;
    }

    private Color ShowLowerDebugToolbar(int boxTextYPos, int firstButtonXPos)
    {
        //======  Show Debug Info Panel ======
        Color originalUnityBgColor = GUI.backgroundColor;
        GUI.backgroundColor = af.showSceneDebugInfoPanel ? switchGreenSceneView : originalUnityBgColor;
        if (GUI.Button(new Rect(firstButtonXPos + 90, boxTextYPos, 110, 20), new GUIContent("Show Debug Info", "Shows info on all the fence parts and variations")))
        {
            af.showSceneDebugInfoPanel = !af.showSceneDebugInfoPanel;
        }
        GUI.backgroundColor = originalUnityBgColor;

        //======  Show Fence Labels  ======
        GUI.backgroundColor = af.showSceneFenceLabels ? switchGreenSceneView : originalUnityBgColor;
        if (GUI.Button(new Rect(firstButtonXPos + 90 + 117, boxTextYPos, 120, 20), new GUIContent("Show Fence Labels", "..")))
        {
            af.showSceneFenceLabels = !af.showSceneFenceLabels;
        }
        GUI.backgroundColor = originalUnityBgColor;

        //======  Show Post Distances  ======
        GUI.backgroundColor = af.showNodeDistances ? switchGreenSceneView : originalUnityBgColor;
        if (GUI.Button(new Rect(firstButtonXPos + 90 + 242, boxTextYPos, 140, 20), new GUIContent("Show Post Distances", "Show distance from previous node")))
        {
            af.showNodeDistances = !af.showNodeDistances;
        }
        GUI.backgroundColor = originalUnityBgColor;

        //======  Show Step Numbers ======
        GUI.backgroundColor = af.showSceneStepNums ? switchGreenSceneView : originalUnityBgColor;
        if (GUI.Button(new Rect(firstButtonXPos + 90 + 400, boxTextYPos, 130, 20), new GUIContent("Show SeqStep Nums", "..")))
        {
            af.showSceneStepNums = !af.showSceneStepNums;
        }
        GUI.backgroundColor = originalUnityBgColor;

        //======  Show Post Positions  ======
        GUI.backgroundColor = af.showPostPositions ? switchGreenSceneView : originalUnityBgColor;
        if (GUI.Button(new Rect(firstButtonXPos + 90 + 550, boxTextYPos, 128, 20), new GUIContent("Show Post Positions", "..")))
        {
            af.showPostPositions = !af.showPostPositions;
        }
        GUI.backgroundColor = originalUnityBgColor;
        GUI.backgroundColor = originalUnityBgColor;

        //      Show Debug Info Panel
        //=================================
        //if (af.showSceneDebugInfoPanel && af.showDebugInfo == true)
        /*{
            //foreach (LayerSet lyr in Enum.GetValues(typeof(LayerSet)))
            {
            }
        }*/

        EditorGUI.EndDisabledGroup();
        return originalUnityBgColor;
    }
    /// <summary>
    /// Alt-double-click on Fence Prefab to Show in Assets Folder
    /// </summary>
    /// <param name="layer"></param>
    private void GoToPrefabInFolderFromSceneView(Event currEvent, LayerSet layer)
    {
        /*if (currEvent.type == EventType.MouseDown && currEvent.clickCount == 2 && currEvent.button == 0 && currEvent.alt == true)
        {
            if (layer < LayerSet.markerLayer)
            {
                assetFolderLinks.ShowPrefabInAssetsFolder(layer);
                //Selection.activeObject = selectedObj;
                return;
            }
        }*/
    }
    /// <summary>
    /// Double-Click on Prefab or Markers to Show/Hide Controls
    /// </summary>
    /// <param name="layer"></param>
    void ShowHideControlsViaDoubleClick(Event currEvent, LayerSet layer)
    {
        if (currEvent.type == EventType.MouseDown && currEvent.clickCount == 2 && currEvent.button == 0)
        {
            if (layer <= LayerSet.markerLayer)
            {
                ToggleNodeGizmos();
                af.ForceRebuildFromClickPoints();
                return;
            }
            //Also disable the editing gizmo
            drawEditingGizmo = false;
        }
    }
    /// <summary>
    /// Enabbles/Disable the ability to Offset / Scale / Rotate Fence parts
    /// </summary>
    /// <param name="layer"></param>
    void EnableSceneEditing(Event currEvent, LayerSet layer)
    {
        if (currEvent.alt == true && currEvent.type == EventType.MouseDown && currEvent.clickCount == 2 && currEvent.button == 0)
        {
            Debug.Log("Double-clicked on Fence to Enable Scene Editing");
            // consume the event so that it doesn't get passed on to other methods
            currEvent.Use();


        }
    }
    /// <summary>
    /// Enabbles/Disable the Node Markers & Gizmos
    /// </summary>
    private void ToggleNodeGizmos()
    {
        showControlsProp.boolValue = !showControlsProp.boolValue;
        af.showControls = !af.showControls;
        af.SetClickMarkersActiveStatus(showControlsProp.boolValue);
    }

    //-------------------------------------------------
    /// <summary>  Return Mouse Control to Unity by Double-clicking Away from fence  </summary>
    private void UnlockMouse(Event currEvent, LayerSet layer)
    {
        //-- Return early if over fence, and cancel if already shown
        if (layer != LayerSet.None)
        {
            showingUnlockMouseFromAFButton = false;
            dblClickScreenPoint = Vector2.zero;
            return;
        }

        //  Double-click to show mouse unlock button
        //Vector2 offsetScreenPoint = Vector2.zero;
        //============================================
        if (currEvent.type == EventType.MouseDown && currEvent.clickCount == 2 && currEvent.button == 0
            && currEvent.control == false && currEvent.alt == false)
        {
            showingUnlockMouseFromAFButton = true;
            Vector2 buttonPosition = currEvent.mousePosition;
            buttonPosition.y = SceneView.currentDrawingSceneView.position.height - buttonPosition.y;
            //buttonPosition.x -= 40;
            dblClickScreenPoint = HandleUtility.GUIPointToScreenPixelCoordinate(buttonPosition);
            //offsetScreenPoint = dblClickScreenPoint + new Vector2(1, 1);
            unlockMouseButtonTimer.Reset();
            //consume the click so that it doesn't get passed on to GUI.Button()
            currEvent.Use();
            EditorApplication.update += OnCustomUpdate;
        }
        //   Show the Release Mouse button and get click on it
        //=========================================================
        if (showingUnlockMouseFromAFButton && dblClickScreenPoint != Vector2.zero)
        {
            Handles.BeginGUI();
            GUI.backgroundColor = new Color(.9f, .9f, .9f, .4f); ;
            if (GUI.Button(new Rect(dblClickScreenPoint.x - 90, dblClickScreenPoint.y + 10, 190, 38),
               new GUIContent("Release Mouse  \n[Dbl-Clk Fence to Reselect]")))
            {
                showingUnlockMouseFromAFButton = false;
                dblClickScreenPoint = Vector2.zero;
                RaycastHit rayHit;
                Ray rayPos = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);
                Physics.Raycast(rayPos, out rayHit, 2000.0f);
                Selection.activeObject = Selection.activeGameObject = rayHit.collider.gameObject;
                //-- If the Node Gizmos are shown, disable them
                if (af.showControls == true)
                    ToggleNodeGizmos();
            }
            Handles.EndGUI();


            //  Remove if mouse moved away
            //=========================================================
            Vector2 newButtonPosition = currEvent.mousePosition;
            // we have to replicate the offset from the orginal button position
            newButtonPosition.x -= 40; newButtonPosition.y = SceneView.currentDrawingSceneView.position.height - newButtonPosition.y - 2;
            var newScreenPoint = HandleUtility.GUIPointToScreenPixelCoordinate(newButtonPosition);
            float distance = Vector2.Distance(dblClickScreenPoint, newScreenPoint);
            if (distance > 100)
            {
                showingUnlockMouseFromAFButton = false;
                dblClickScreenPoint = Vector2.zero;
            }
        }
    }

    private static void DebugEvents(Event currentEvent, bool mouseDownOnly = false)
    {
        //EventDebugging.LogEvent(currEvent, false);

        // Detect and log mouse button activities
        EventType buttonEventType = EventDebugging.MouseButtonActivityDetected(currentEvent, mouseDownOnly);
        if (buttonEventType != EventType.Ignore)
            Debug.Log($"MouseButtonActivityDetected Detected Event: {buttonEventType} \n");

        // Detect and log other mouse activities
        //EventType mouseEventType = EventDebugging.MouseOtherActivityDetected(currEvent);
        //if (mouseEventType != EventType.Ignore)
        //    Debug.Log($"MouseOtherActivityDetected Detected Event: {mouseEventType} \n");
    }

    //========================================================================================
    //                              End of OnSceneGUI()
    //========================================================================================
    /// <summary>
    /// Logs the type of the current event to the console. If a specific event type is specified,
    /// it only logs the matching type, otherwise it logs ant event type.
    /// </summary>
    /// <param name="currentEvent">The current event to be checked.</param>
    /// <param name="eventTypeToDetect">The specific type of event to detect. If null, all event types will be logged.</param>
    private static void CurrentEventDebug(Event currentEvent, EventType? eventTypeToDetect = null)
    {
        bool detectAll = !eventTypeToDetect.HasValue;

        if (detectAll || currentEvent.type == eventTypeToDetect)
        {
            if (currentEvent.type == EventType.ScrollWheel)
                Debug.Log("Scroll Wheel");
            else if (currentEvent.type == EventType.MouseMove)
                Debug.Log("Mouse Move");
            else if (currentEvent.type == EventType.MouseDown)
                Debug.Log("Mouse Down");
            else if (currentEvent.type == EventType.MouseUp)
                Debug.Log("Mouse Up");
            else if (currentEvent.type == EventType.Used)
                Debug.Log("Used");
            else
                Debug.Log($"{currentEvent.type}");
        }
    }
    //-----------------------------------------------------------
    /// <summary>
    /// Gets all info about what we're hovering over, using InferLayerFromGoName() including the sourceLayerList type, and if it's a clickNode.
    /// <para>Also displays a help box with info about the object.</para>
    /// </summary>
    /// <param name="helpBoxHeight"></param>
    /// <param name="hoveredLayer"></param>
    /// <param name="isClickNode"></param>
    /// <returns></returns>
    private GameObject MouseHover(int helpBoxHeight, Ray rayPosition, out LayerSet hoveredLayer, out bool isClickNode)
    {
        Event currentEvent = Event.current;

        RaycastHit hit;
        //Ray rayPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        //--debug only
        bool shift = currentEvent.shift;
        bool control = currentEvent.control;

        //CurrentEventDebug(currEvent, EventType.MouseDown);

        int boxHeight = 75, boxWidth = 250;
        hoveredLayer = LayerSet.postLayer;
        bool isPostPoint = true;
        isClickNode = false;
        GameObject go = null;
        string layerNameString;
        bool didHit = Physics.Raycast(rayPosition, out hit, 1000.0f);
        if (currentEvent.shift == false && didHit)
        {
            go = hit.transform.gameObject;
            hoveredLayer = af.InferLayerFromGoName(go);

            if (hoveredLayer != LayerSet.None && go != null)
            {
                //====================
                //      Post
                //====================
                if (hoveredLayer == LayerSet.postLayer)
                {
                    //      Determine Name, Position, Index, Post Status
                    //=========================================================
                    layerNameString = af.GetLayerNameAsString(hoveredLayer);
                    string postString = "Post  ";

                    //     Is Post Point
                    //=======================
                    if (af.IsCloseClickPoint(go.transform.position) != -1)
                    {
                        postString = "Post [NODE]  ";
                        isClickNode = true;
                        boxWidth = 296;
                    }
                    else if (af.IsPostPoint(go.transform.position) == -1)
                    {
                        // Maybe we're hovering over a finished fence, so it is not part of this live fence
                        isPostPoint = false;
                        boxHeight = 35;
                    }
                    string layerStr = "Post:  ", seqString = go.name.Substring(go.name.Length - 2);
                    bool isInt = int.TryParse(seqString, out currSeqPostStepIndex);
                    //string postIndex = Regex.Match(go.name, @"\d+").Value;
                    string postIndex = PostVector.FindIndexByPosition(go).ToString();
                    Vector3 mousePosition = Event.current.mousePosition;
                    int boxPosX = (int)mousePosition.x, boxPosY = (int)mousePosition.y;
                    if (boxPosX < 55)
                        boxPosX = 55;
                    if (boxPosY > Screen.height - helpBoxHeight)
                        boxPosY = Screen.height - helpBoxHeight;
                    Handles.BeginGUI();
                    //int postnamePos = go.name.IndexOf("_Post");

                    //  DrawTCT the Background Box
                    //===========================
                    Handles.DrawSolidRectangleWithOutline(new Rect(boxPosX - 50, mousePosition.y + 15, boxWidth + 15, boxHeight), sceneViewBoxColor, sceneViewBoxBorderColor);
                    string name = "";
                    if (go.name.Contains("_Post"))
                        name = go.name.Substring(0, go.name.IndexOf("_Post"));
                    else if (go.name.Contains("_Extra"))
                        name = go.name.Substring(0, go.name.IndexOf("_Extra"));


                    //  Display Post label with position and index
                    //===============================================
                    string variationStr = "";
                    if (af.usePostVariations)
                        variationStr = "Step " + (currSeqPostStepIndex + 1) + " Variation. ";
                    string posString = af.Vector3ToStringNeat(go.transform.localPosition, 1);
                    GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 16, boxWidth + 15 - 2, 20), postString + name + " [" + postIndex + "]   " + posString);

                    //     Is Post Point
                    //=======================
                    if (isPostPoint)
                    {
                        GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 50, 250, 20), "Right Click for Options ");
                        if (af.usePostVariations)
                            GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 68, 250, 20), "Click to view step in Variations Seq ");
                        else
                            GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 68, 250, 20), "Click to view Posts in Inspector");
                    }
                    else
                        GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 32, 250, 20), "(This a non-live Finished Post. Or something went wrong.)");
                    Handles.EndGUI();
                    Repaint();
                }

                //      Hovering over Rail
                //===============================
                if (hoveredLayer == LayerSet.railALayer || hoveredLayer == LayerSet.railBLayer)
                {
                    string layerStr = "";
                    boxHeight = 85;
                    bool isRailPoint = true;
                    if (af.IsRailPoint(go) == (-1, -1))
                    {
                        // Maybe we're hovering over a finished fence, so it is not part of this live fence
                        isRailPoint = false;
                        boxHeight = 30;
                    }

                    bool isStep = false;
                    string seqString = go.name.Substring(go.name.Length - 2);
                    if (hoveredLayer == LayerSet.railALayer)
                    {
                        layerStr = "Rail A:  ";
                        isStep = int.TryParse(seqString, out currSeqRailStepIndex[0]);

                    }
                    else if (hoveredLayer == LayerSet.railBLayer)
                    {
                        layerStr = "Rail B:  ";
                        isStep = int.TryParse(seqString, out currSeqRailStepIndex[1]);
                    }

                    //- Infers the section index from the namw
                    string railIndexStr = Regex.Match(go.name, @"\[(\d+)").Groups[1].Value;
                    //float sectionLength = af.GetSectionLength(int.Parse(railIndexStr));
                    //Debug.Log("Section Length: " + sectionLength);

                    Vector3 mousePosition = Event.current.mousePosition;
                    int boxPosX = (int)mousePosition.x, boxPosY = (int)mousePosition.y;
                    if (boxPosX < 55)
                        boxPosX = 55;
                    if (boxPosY > Screen.height - helpBoxHeight)
                        boxPosY = Screen.height - helpBoxHeight;
                    Handles.BeginGUI();
                    float hoverBoxWidth = 290;
                    Handles.DrawSolidRectangleWithOutline(new Rect(boxPosX - 50, mousePosition.y + 15, hoverBoxWidth, boxHeight), sceneViewBoxColor, sceneViewBoxBorderColor);
                    int startIndex = go.name.IndexOf("_Rail");
                    if (startIndex == -1)
                        startIndex = go.name.IndexOf("_Panel");
                    string name = go.name.Substring(0, startIndex);
                    string variationStr = "";
                    bool usingVariations = false;
                    if (hoveredLayer == LayerSet.railALayer && af.useRailVariations[0])
                    {
                        variationStr = "Step " + (currSeqRailStepIndex[0] + 1) + " Variation. ";
                        usingVariations = true;
                    }
                    else if (hoveredLayer == LayerSet.railALayer && af.useRailVariations[1])
                    {
                        variationStr = "Step " + (currSeqRailStepIndex[1] + 1) + " Variation. ";
                        usingVariations = true;
                    }

                    GUIStyle testStyle = new GUIStyle(GUI.skin.label);
                    Color testCol = testStyle.normal.textColor;

                    GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 16, 240, 20), layerStr + name + " [" + railIndexStr + "]", testStyle);
                    if (isRailPoint)
                    {
                        GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 38, 220, 20), "Right Click for Options ", testStyle);
                        if (usingVariations)
                            GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 60, 220, 30), "Uses Step " + (currSeqRailStepIndex[0] + 1) + " in Variation Sequencer \n");
                        else
                            GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 60, 280, 20), "Click to Set " + layerStr + "in Components Toolbar");

                        GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 82, 220, 20), "Control-Shift-Click to Insert Node", testStyle);
                    }
                    Handles.EndGUI();
                    Repaint();
                }
            }
            if (go != null && go.name.Contains("_Extra"))
            {
                Vector3 mousePosition = Event.current.mousePosition;
                int boxPosX = (int)mousePosition.x, boxPosY = (int)mousePosition.y;
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(new Rect(boxPosX - 50, mousePosition.y + 15, 300, 40),
                    sceneViewBoxColorMoreOpaque, sceneViewBoxBorderColor);
                if (showNodeInfo == true)
                {
                    GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 36, 240, 20), go.transform.localPosition.ToString());
                }
                Handles.EndGUI();
            }
            if (go != null && go.name.Contains("Marker"))
            {
                Vector3 mousePosition = Event.current.mousePosition;
                int boxPosX = (int)mousePosition.x, boxPosY = (int)mousePosition.y;
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(new Rect(boxPosX - 50, mousePosition.y + 15, 300, 40),
                    sceneViewBoxColorMoreOpaque, sceneViewBoxBorderColor);
                GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 16, 300, 20), "Control-Click to Remove Node");
                //GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 34, 240, 20), "[ Can not remove first or last ]");
                if (showNodeInfo == true)
                {
                    GUI.Label(new Rect(boxPosX - 45, mousePosition.y + 36, 240, 20), go.transform.localPosition.ToString());
                }
                Handles.EndGUI();
            }
            //Debug.Log(go.name);
        }
        else
            hoveredLayer = LayerSet.None;

        return go;
    }
    //-----------------------------------------------------------------------------------------------------
    private int AddClickPoint(Event currentEvent, Ray rayPosition, ref Vector3 clickPoint, int shiftRightClickAddGap)
    {
        //--debug only
        bool shift = currentEvent.shift;
        bool control = currentEvent.control;
        bool mouseDown = currentEvent.type == EventType.MouseDown;
        RaycastHit hit;
        //Ray rayPosition;

        rayPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        Physics.Raycast(rayPosition, out hit, 2000.0f);


        // Create a sourceLayerList mask of layers to ignore, and Invert it (~)
        int layerMask = ~LayerMask.GetMask("AF Test Layer", "AF Test Layer 2");

        if ((!currentEvent.control && currentEvent.shift && currentEvent.type == EventType.MouseDown && Event.current.button != 1) || shiftRightClickAddGap == 1)
        {

            // Test if we were trying to click on a sourceLayerList that's set to be ignored, note re-inversion of mask with ~
            string warnIgnoreString = "";
            if (Physics.Raycast(rayPosition, out hit, 2000.0f, ~layerMask))
            {
                warnIgnoreString = hit.transform.gameObject.name + " was ignored, as it is set to be ignored by Auto Fence (see Globals->Layers) \n" +
                    "The click point was instead placed on the nextPos available object with a collider: ";
            }

            //bool placedClickPoint = false;
            if (Physics.Raycast(rayPosition, out hit, 2000.0f/*, ~layerMask*/))
            {
                //int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

                af.currMousePosition = hit.point;
                //Debug.Log(af.currMousePosition + "\n");
                if (currentEvent.button == 0 || (shiftRightClickAddGap == 1 && af.allowGaps))
                {
                    Undo.RecordObject(af, "Add ClickPoint");
                    af.addIntersEndPoint = Handles.PositionHandle(af.addIntersEndPoint, Quaternion.identity);
                    af.addIntersEndPoint = hit.point;
                    clickPoint = hit.point - new Vector3(0, 0.00f, 0); //TODO bury it in ground as little
                    if (af.snapMainPosts)
                        clickPoint = SnapHandles(clickPoint, af.snapSize);
                    oldCloseLoop = af.closeLoop = false;
                    if (Mathf.Abs(clickPoint.y) < 0.0001f)
                        clickPoint.y = 0.0f;

                    RepositionFolderHandles(clickPoint);
                    af.clickPoints.Add(clickPoint);
                    af.clickPointFlags.Add(shiftRightClickAddGap); // 0 if normal, 1 if break
                    af.keyPoints.Add(clickPoint);
                    //Timer t = new Timer("ForceRebuild");

                    //-- This willl ensure the pool is rebuilt with the new click points set to the override Mains prefab
                    if (af.allowNodePostsPrefabOverride)
                        af.DestroyPoolForLayer(LayerSet.postLayer);
                    af.ForceRebuildFromClickPoints();
                    //if (af.rotateY)
                    //af.ForceRebuildFromClickPoints();
                    //t.End();
                    //-- copy click points to handle points
                    af.handles.Clear();
                    for (int i = 0; i < af.clickPoints.Count; i++)
                    {
                        af.handles.Add(af.clickPoints[i]);
                    }
                    if (warnIgnoreString != "")
                        Debug.Log(warnIgnoreString + hit.transform.gameObject.name);
                }
            }
        }
        return layerMask;
    }
    //------------------------------------------
    private void HandleDragAndControls(Event currentEvent)
    {
        //=====================================
        //      Handle dragging & controls
        //=====================================
        if (af.showControls && af.clickPoints.Count > 0)
        {
            bool wasDragged = false;
            List<Vector3> clickPointsCopy = new List<Vector3>(); // used when dragging all handles with control-drag
                                                                 // CreateMergedPrefabs handles at every click point
            if (currentEvent.type == EventType.MouseDrag || af.handles.Count != af.clickPoints.Count)
            {
                af.handles.Clear();
                af.handles.AddRange(af.clickPoints); // copy them to the handles
                wasDragged = true;
                Undo.RecordObject(af, "Move Post");
                if (currentEvent.control)
                {
                    clickPointsCopy.AddRange(af.clickPoints);
                }
            }

            for (int i = 0; i < af.handles.Count; i++)
            {
                if (af.closeLoop && i == af.handles.Count - 1)// don't make a handle for the last point if it's a closed loop
                    continue;
                af.handles[i] = Handles.PositionHandle(af.handles[i], Quaternion.identity); //allows movement of the handles
                if (af.snapMainPosts)
                    af.handles[i] = SnapHandles(af.handles[i], af.snapSize);
                af.clickPoints[i] = af.handles[i];// set new clickPoint position
            }
            if (wasDragged == true)
            {
                af.Ground(af.clickPoints);
                //-- Set the handles at the grounded position
                for (int i = 0; i < af.handles.Count; i++)
                {
                    af.handles[i] = new Vector3(af.handles[i].x, af.clickPoints[i].y, af.handles[i].z); // set the y position back to the clickpoint (grounded)
                }
            }

            // If we want to drag all with control-drag, figure out which one was dragged
            if (currentEvent.control && clickPointsCopy.Count == af.clickPoints.Count)
            {
                int movedclickPointIndex = -1;
                Vector3 moveVector = Vector3.zero;
                for (int c = 0; c < af.clickPoints.Count; c++)
                {
                    if (af.clickPoints[c] != clickPointsCopy[c])
                    {
                        movedclickPointIndex = c;
                        //Debug.Log("moved  " + movedclickPointIndex);
                        moveVector = af.clickPoints[c] - clickPointsCopy[c];
                        break;
                    }
                }
                if (movedclickPointIndex != -1 && moveVector != Vector3.zero)
                {
                    for (int j = 0; j < af.clickPoints.Count; j++)
                    {
                        if (j != movedclickPointIndex)
                        {
                            af.clickPoints[j] += moveVector;

                        }
                    }
                    af.handles.Clear();
                    af.handles.AddRange(af.clickPoints);
                }
            }

            if (wasDragged)
            {
                af.UpdateCloseLoop(af.closeLoop);
                af.CheckClickPointsForIgnoreLayers();
                af.CheckNodePositions();
                af.ForceRebuildFromClickPoints();
            }

        }
    }

    private void InsertPost(Event currentEvent, bool isMouseInSceneView)
    {
        if (isMouseInSceneView == false)
            return;

        //=====================================
        //      Insert Post
        //=====================================
        if (currentEvent.shift && currentEvent.control && currentEvent.type == EventType.MouseDown)
        {
            RaycastHit hit;
            Ray rayPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            if (Physics.Raycast(rayPosition, out hit, 2000.0f))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                {
                    af.mouseHoveringOverIgnoreLayer = true;
                    return;
                }

                af.currMousePosition = hit.point;
                Undo.RecordObject(af, "Insert Post");
                af.InsertPost(hit.point);
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(new Rect(currentEvent.mousePosition.x, currentEvent.mousePosition.y + 18, 130, 20), sceneViewBoxColor, sceneViewBoxBorderColor);
                GUI.Label(new Rect(currentEvent.mousePosition.x, currentEvent.mousePosition.y + 20, 128, 20), "Insert");
                Handles.EndGUI();
            }
        }
    }

    private void CreateVectorsForPreviewLines(Event currentEvent, Ray rayPosition, ref Vector3 clickPoint, int shiftRightClickAddGap, int layerMask)
    {
        //=====================================
        //    Create Vectors for Preview Lines
        //=====================================
        //Ray rayPosition;
        RaycastHit hit;
        if (currentEvent.shift)
        {
            //rayPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

            string firstNonIgnoredObject = "";
            if (Physics.Raycast(rayPosition, out hit, 2000.0f/*, layerMask*/))
            {
                Vector3 pt = hit.point;
                firstNonIgnoredObject = hit.transform.gameObject.name;
                if (af.snapMainPosts)
                    pt = SnapHandles(pt, af.snapSize);
                af.currMousePosition = pt;
                if (af.clickPoints.Count > 0 && currentEvent.button == 0 || (shiftRightClickAddGap == 1 && af.allowGaps))
                {
                    clickPoint = hit.point;
                    if (af.snapMainPosts)
                        clickPoint = SnapHandles(clickPoint, af.snapSize);
                    Vector3 lastPt = af.clickPoints[af.clickPoints.Count - 1];
                    af.previewPoints[0] = lastPt;
                    af.previewPoints[1] = clickPoint;
                }
                af.mouseHoveringOverIgnoreLayer = true;

                if (currentEvent.control)
                {
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(currentEvent.mousePosition.x, currentEvent.mousePosition.y + 18, 100, 20), sceneViewBoxColor, sceneViewBoxBorderColor);
                    GUI.Label(new Rect(currentEvent.mousePosition.x, currentEvent.mousePosition.y + 20, 100, 18), "Insert");
                    Handles.EndGUI();
                }
            }
            if (Physics.Raycast(rayPosition, out hit, 2000.0f, ~layerMask))
            {
                string layerName = LayerMask.LayerToName(hit.transform.gameObject.layer);
                af.mouseHoveringOverIgnoreLayer = true;
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(new Rect(currentEvent.mousePosition.x, currentEvent.mousePosition.y + 18, 330, 36), sceneViewBoxColor, sceneViewBoxBorderColor);
                GUI.Label(new Rect(currentEvent.mousePosition.x, currentEvent.mousePosition.y + 20, 400, 60),
                    $"' {hit.transform.gameObject.name} ' " +
                    $"\nis assigned to an Ignore Layer: {layerName} " +
                    $"\n\nUsing {firstNonIgnoredObject} below instead");


                Handles.EndGUI();
            }
        }
    }
    //--------------------------------------------------------------------------------

    private bool ToggleGapStatusOfPost(Event currentEvent)
    {
        //=======================================================
        //     Toggle Gap Status of Post (control-right-click) 
        //=======================================================
        bool togglingGaps = false;
        RaycastHit hit;
        if (af.showControls && currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 1)// showControlsProp + control-right-click
        {
            Ray rayPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            if (Physics.Raycast(rayPosition, out hit, 2000.0f))
            {
                string name = hit.collider.gameObject.name;
                if (name.StartsWith("FenceManagerMarker_"))
                {
                    Undo.RecordObject(af, "Toggle Gap Status Of Post");
                    string indexStr = name.Remove(0, 19);
                    int index = Convert.ToInt32(indexStr);
                    int oldStatus = af.clickPointFlags[index];
                    af.clickPointFlags[index] = 1 - oldStatus; // svInvert 0/1
                    af.ForceRebuildFromClickPoints();
                    togglingGaps = true;
                }
            }
        }

        return togglingGaps;
    }

    private void DeleteClickPoint(Event currentEvent, Ray rayPosition)
    {
        //=======================================================
        //         Delete ClickPoint (Control-Click)
        //=======================================================
        // Normal Control-normalClick  - No shift
        RaycastHit hit;
        //Ray rayPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        if (af.showControls && currentEvent.control && currentEvent.shift == false && currentEvent.type == EventType.MouseDown && currentEvent.button == 0) //showControlsProp + control-left-click
        {
            if (Physics.Raycast(rayPosition, out hit, 2000.0f))
            {
                string name = hit.collider.gameObject.name;
                if (name.StartsWith("FenceManagerMarker_"))
                {
                    Undo.RecordObject(af, "Delete ClickPoint");
                    string indexStr = name.Remove(0, 19);
                    int index = Convert.ToInt32(indexStr);
                    af.DeleteClickPoint(index);
                    //deletedPostNow = true;
                }
            }
        }
    }

    //-------------------------------------------
    /// <summary>
    /// Gets the correct seq Step Num that this section will use
    /// </summary>
    /// <param name="currentEvent"></param>
    /// <param name="sectionIndexForLayers"></param>
    /// <param name="layer"></param>
    /// <param name="go"></param>
    private void AssignStepIndexForVariations(Event currentEvent, int[] sectionIndexForLayers, LayerSet layer, GameObject go)
    {
        //if (af.usePostVariations == false)
        //return;

        if (af.IsRailLayer(layer))
        {
            int railLayer = (int)layer;
            string seqString = go.name.Substring(go.name.Length - 2);
            if (currentEvent.type == EventType.MouseDown)
            {
                sectionIndexForLayers[railLayer] = GetSectionIndexFromName(go);
            }
            Repaint();
        }

        //    Post Layer
        //===================
        else if (layer == LayerSet.postLayer)
        {
            int postLayer = (int)layer;
            if (go == null)
            {
                Debug.Log("GameObject is null");
            }
            string seqString = go.name.Substring(go.name.Length - 2);
            bool isInt = int.TryParse(seqString, out currSeqPostStepIndex);
            if (currentEvent.type == EventType.MouseDown) //
            {
                sectionIndexForLayers[postLayer] = GetSectionIndexFromName(go);
            }
            //string resultString = Regex.Match(go.name, @"\d+").Value;

            Repaint();
        }
    }

    #endregion //end of SceneViewGUI
}
