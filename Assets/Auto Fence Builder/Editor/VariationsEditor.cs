//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using AFWB;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AFWB.AutoFenceCreator;

public class VariationsEditor
{
    private AutoFenceCreator af;
    private AutoFenceEditor ed;

    private bool showSourcePrefabs = true;
    private LayerSet kRailALayer = LayerSet.railALayer; // to save a lot of typing
    private LayerSet kRailBLayer = LayerSet.railBLayer;
    private LayerSet kPostLayer = LayerSet.postLayer;
    private LayerSet kSubpostLayer = LayerSet.subpostLayer;
    private LayerSet kExtraLayer = LayerSet.postLayer;

    private LayerSet layer;
    private string layerNameString = "";
    private List<string> layerPrefabMenuNames;
    private PrefabTypeAFWB prefabType;
    private List<SourceVariant> sourceVariants;
    private List<int> sourceVariantPrefabIndices, sourceVariantMenuIndices;
    private List<GameObject> mainPrefabs;
    private int layerIndex;
    private int maxNumVariations = AutoFenceCreator.kMaxNumSourceVariants;
    private int mainPrefabMenuIndex, mainPrefabIndex, numBuilt;
    private int numSourceVariantsInUse;
    private int autoAssignMenuIndex = 0, autoAssignToolbarValue = 0;

    private SerializedProperty numSourceVariantsInUseProp, SeqItemListProperty, seqNumStepsProp;
    private SerializedProperty sourceVariant_MenuIndicesProp;

    private GUIContent[] autoAssignStrings = {
        new GUIContent("All Main", "All Variations will be the same prefab as the Main"),

        new GUIContent("Consecutive", "These will be the 4 prefabs following the Main prefab.\n\n" +
            "If you add your custom Prefabs using a consecutive naming convention, they will automatically be assigned in order.\n" +
            "e.g. My Wall 1, My Wall 2 etc \n" +
            "For an example see the Preset 'Variation Test Consecutive'."),

        new GUIContent("Same category", "Random prefabs in the same categeory, e.g. All Castle, or all Wire etc."),

        new GUIContent("Random", "Random prefabs in any category"),

        new GUIContent("Similar", "Chooses Random prefabs that are somewhat similar. Hmmm.")};

    //------------------------------------
    // To avoid lots of case switching in the ed code, set the variables here
    private void GrabVariablesForSet()
    {
        layerNameString = af.GetLayerNameAsString(layer, useCamel: true);
        prefabType = af.GetPrefabTypeFromLayer(layer);
        sourceVariants = af.GetSourceVariantsForLayer(layer);
        mainPrefabs = af.GetPrefabsForLayer(layer);
        sourceVariantPrefabIndices = af.GetSourceVariantMenuIndicesListForLayer(layer);
        //List<int> indices = af.railASourceVariant_MenuIndices;

        //-- These are the menu indices for the sourceVariants of this sourceLayerList
        /*SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp0 = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(0);
        SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp1 = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(1);
        SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp2 = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(2);

        int a = currSourceVariant_PrefabMenuIndex_ForRowProp0.intValue;
        int b = currSourceVariant_PrefabMenuIndex_ForRowProp1.intValue;
        int c = currSourceVariant_PrefabMenuIndex_ForRowProp2.intValue;*/

        //int prefabIndex = af.GetCurrentPrefabIndexForLayer(sourceLayerList);

        layerPrefabMenuNames = af.GetPrefabMenuNamesForLayer(layer);

        if (layer == kPostLayer)
        {
            sourceVariant_MenuIndicesProp = ed.serializedObject.FindProperty("postSourceVariant_MenuIndices");

            //layerPrefabMenuNames = af.postMenuNames;
            mainPrefabMenuIndex = af.currentPost_PrefabMenuIndex;
            mainPrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(mainPrefabMenuIndex, prefabType);
            layerIndex = 2;
            numBuilt = af.postsBuiltCount;
            numSourceVariantsInUseProp = ed.serializedObject.FindProperty("_numPostVariantsInUse");
            numSourceVariantsInUse = numSourceVariantsInUseProp.intValue;
        }
        else if (layer == kRailALayer)
        {
            sourceVariant_MenuIndicesProp = ed.serializedObject.FindProperty("railASourceVariant_MenuIndices");

            // layerPrefabMenuNames = af.railMenuNames;
            mainPrefabMenuIndex = af.currentRail_PrefabMenuIndex[kRailALayerInt];
            mainPrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(mainPrefabMenuIndex, prefabType);
            layerIndex = 0;
            numBuilt = af.railABuiltCount;
            numSourceVariantsInUseProp = ed.serializedObject.FindProperty("numRailVariantsInUse").GetArrayElementAtIndex(kRailALayerInt);
            numSourceVariantsInUse = numSourceVariantsInUseProp.intValue;
        }
        else if (layer == kRailBLayer)
        {
            sourceVariant_MenuIndicesProp = ed.serializedObject.FindProperty("railBSourceVariant_MenuIndices");

            //layerPrefabMenuNames = af.railMenuNames;
            mainPrefabMenuIndex = af.currentRail_PrefabMenuIndex[kRailBLayerInt];
            mainPrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(mainPrefabMenuIndex, prefabType);
            layerIndex = 1;
            numBuilt = af.railBBuiltCount;
            numSourceVariantsInUseProp = ed.serializedObject.FindProperty("numRailVariantsInUse").GetArrayElementAtIndex(kRailBLayerInt);
            numSourceVariantsInUse = numSourceVariantsInUseProp.intValue;
        }

        //seqListProperty = ed.GetSequencerListForLayerProp(sourceLayerList);
        //seqNumStepsProp = ed.serializedObject.FindProperty("seqNumSteps").GetArrayElementAtIndex((int)sourceLayerList);

        layerNameString = af.GetLayerNameAsString(layer);

        //--  Note: we have to do this for A and B seperately as Unity can't handle multi-dimensional arrays
        //List<int> indices = af.railASourceVariant_MenuIndices();
    }

    public VariationsEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    //---------------
    public void CheckPresetValidateVariants(ScriptablePresetAFWB preset)
    {
        /*if (preset.railAVariants.Count > AutoFenceCreator.kMaxNumSourceVariants || preset.railBVariants.Count > AutoFenceCreator.kMaxNumSourceVariants)
        {
            Debug.Log("Too many rail sourceVariants in " + preset.name + "   A: " + preset.railAVariants.Count + "   B: " + preset.railBVariants.Count);
            int endIndex = AutoFenceCreator.kMaxNumSourceVariants;
            preset.railAVariants.RemoveRange(endIndex, preset.railAVariants.Count - endIndex);
            preset.railBVariants.RemoveRange(endIndex, preset.railBVariants.Count - endIndex);
        }*/
    }

    //----------------------
    public void ShowVariationSourcesHelp()
    {
        if (GUILayout.Button("?", GUILayout.Width(25)))
        {
            ed.showVarHelp = !ed.showVarHelp;
        }
        if (ed.showVarHelp)
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("• Use the 4 slots in 'Choose Source Prefabs' as possible variation prefabs");
            EditorGUILayout.LabelField("• Use the 'Assign All Sources' shortcut buttons to quickly fill all 4 slots");
            EditorGUILayout.LabelField("• Each of these 4 prefabs will now be available to assign to any step in the sequence");
            EditorGUILayout.LabelField("• 'Auto Update Sequence' will automatically assign these to seq steps when changed");
            EditorGUILayout.LabelField("• Note: If you have less than 5 steps in your sequence, you will only see the first 1, 2, or 3 variations");
            GUILayout.Space(10);
            if (GUILayout.Button("Close Help", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                ed.showVarHelp = false;
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }

    //----------------------
    public void ShowSequencerHelp()
    {
        EditorGUILayout.LabelField("  Here you can set up a sequence to modify each section of the fence");
        EditorGUILayout.LabelField("  Each 'step' is a collection of settings that will modify a particular section of fence");
        EditorGUILayout.LabelField("  The steps will Loop, according to how many steps you choose");
        EditorGUILayout.LabelField("  For example: If you had 3 steps, and your fence was 8 sections long, then the sequence would be:");
        EditorGUILayout.LabelField("  [Step1, Step2, Step3]    [Step1, Step2, Step3]    [Step1, Step2]");
        EditorGUILayout.LabelField("  (See the example preset: Variation Templates/3 Step Sequence Example)");
        EditorGUILayout.LabelField("  Each step can be assigned a prefab using the buttons; a transform, and a choice of orientation flips");
        EditorGUILayout.LabelField("  In the example preset, Step 1 = VariationTest1      Step 2 = VariationTest2      Step 1 = ABasicConcrete");
        EditorGUILayout.LabelField("  Additionally, Step2 is Inverted, and Step 3 is taller with  its Size.Y set to 1.5");
        EditorGUILayout.LabelField("  ");
        EditorGUILayout.LabelField("  You can use the 'Optimise' and 'Randomise' Options to quickly fill all steps");
        EditorGUILayout.LabelField("  ");
        EditorGUILayout.LabelField("  Prefab choices for each fence section can also be set by Control-Right-Clicking on them in Scene View");
        EditorGUILayout.LabelField("  Here you have a choice of:'Free': ");
        EditorGUILayout.LabelField("  • These will be single independent changes and appear in the 'Singles' list below");
        EditorGUILayout.LabelField("    They will override the sequence at that position. This is useful if you want to modify only one section");
        EditorGUILayout.LabelField("  Or 'Put in Sequence Step': ");
        EditorGUILayout.LabelField("  • These will be placed at the correct sequence step, and looped with the sequence");
        EditorGUILayout.LabelField("  • In both cases, the ONLY available prefabs are the ones assigned in 'Choose Source Prefabs' ");
    }

    //----------------------
    public void ShowSinglesHelp()
    {
        EditorGUILayout.LabelField("  Single Panels or Posts can be replaced with different prefabs by right-clicking on them in the Scene");
        EditorGUILayout.LabelField("  In the Singles Editor, each of these can be further modified");
        EditorGUILayout.LabelField("  These will override the Sequence Step assignment for this gizmoSingletonInstance");
    }

    //====================================================================================================
    //                                Setup Source Prefabs
    //====================================================================================================

    public void SetupSourcePrefabs(LayerSet layer)
    {
        ed.CheckPrefabsExistForLayer(layer); //debug only
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        this.layer = layer;
        layerIndex = (int)layer;
        GrabVariablesForSet();
        bool changedVariantChoices = false;

        //return;

        //=======================================
        //        Auto Assign Toolbar
        //=======================================
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Assign Sources: ", ed.unityBoldLabel, GUILayout.Width(120));
        autoAssignToolbarValue = GUILayout.Toolbar(autoAssignToolbarValue, autoAssignStrings, ed.smallButtonStyle7, GUILayout.Width(460));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
        int varMenuIdx = -1, varPrefabIdx = -1;
        string catName = GetCategoryNameFromMenuName(layerPrefabMenuNames[mainPrefabMenuIndex]);
        if (EditorGUI.EndChangeCheck())
        {
            //-- Get Menu Indices for all sourceVariants;
            List<int> sv = af.GetSourceVariantMenuIndicesForLayer(layer);

            List<int> categoryList = ed.prefabAssignEd.GetListOfPrefabMenuIndicesWithCategory(prefabType, catName);
            for (int i = 1; i < maxNumVariations; i++)
            {
                SerializedProperty thisMenuIndexProp = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i);
                //===============================
                //   All the same as Main
                //===============================
                if (autoAssignToolbarValue == 0)
                {
                    // Set ll the variant menu indices to be the same as used in the Main Choose Prefab
                    thisMenuIndexProp.intValue = mainPrefabMenuIndex;
                }
                //===============================
                //   Consecutive
                //===============================
                if (autoAssignToolbarValue == 1)
                    thisMenuIndexProp.intValue = mainPrefabMenuIndex + i;
                //===============================
                //   Category
                //===============================
                if (autoAssignToolbarValue == 2 && categoryList.Count > 0)
                {
                    //af.SeedRandom(false);
                    int r = UnityEngine.Random.Range(0, categoryList.Count - 1);
                    thisMenuIndexProp.intValue = categoryList[r];
                }
                //===============================
                //   Random
                //===============================
                if (autoAssignToolbarValue == 3)
                {
                    thisMenuIndexProp.intValue = UnityEngine.Random.Range(1, mainPrefabs.Count - 1);
                }
            }
            //if (updateSequenceAlso)
            //{
            //    SeqItem.AssignAllDifferentObjectIndicesInSequence(seqList, af.GetSequencerForLayer(sourceLayerList).Length(), sourceVariants);
            //}
            changedVariantChoices = true;
        }

        //=========================================
        //      Set Number of Variants In Use
        //=========================================
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 139; // Replace this with any width
        EditorGUILayout.PropertyField(numSourceVariantsInUseProp,
            new GUIContent("Num Variations:   Main + ", "The number of alternative prefabs that can be used"), GUILayout.Width(162));
        EditorGUIUtility.labelWidth = 0; //reset default
        GUILayout.Space(6);
        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(17)) && numSourceVariantsInUseProp.intValue > 1)
        {
            numSourceVariantsInUseProp.intValue -= 1;
        }
        GUILayout.Space(1);
        if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(17)) && numSourceVariantsInUseProp.intValue < 9)
        {
            numSourceVariantsInUseProp.intValue += 1;
        }
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            if (numSourceVariantsInUseProp.intValue < 1)
                numSourceVariantsInUseProp.intValue = 1;
            if (numSourceVariantsInUseProp.intValue > AutoFenceCreator.kMaxNumSourceVariants - 1)
                numSourceVariantsInUseProp.intValue = AutoFenceCreator.kMaxNumSourceVariants - 1;
            ed.serializedObject.ApplyModifiedProperties();
            changedVariantChoices = true;
        }

        SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp0 = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(0);
        SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp1 = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(1);
        SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp2 = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(2);

        int a = currSourceVariant_PrefabMenuIndex_ForRowProp0.intValue;
        int b = currSourceVariant_PrefabMenuIndex_ForRowProp1.intValue;
        int c = currSourceVariant_PrefabMenuIndex_ForRowProp2.intValue;
        //=======================================

        //===============================================
        //       Show the name of the Main prefab
        //===============================================
        EditorGUILayout.Space(1);
        GUILayout.BeginHorizontal();
        //string catName = af.categoryNames[af.categoryIndex];
        EditorGUILayout.LabelField(new GUIContent("Main", "This is the prefab you have specified as the main " +
            $"prefab for {layerNameString}. \nCan be changed in 'Choose Prefab Type' above"), GUILayout.Width(80));

        EditorGUILayout.LabelField(new GUIContent(catName + "/  " + sourceVariants[0].Go.name, "This is the prefab you have specified as the main " +
            $"prefab for {layerNameString}. \nCan be changed in 'Choose Prefab Type' above"), ed.popupLabelStyle, GUILayout.Width(300));
        GUILayout.EndHorizontal();

        //===============================
        //        Choose Variants
        //===============================
        EditorGUILayout.Space(1);
        //-- make a copy of the layerPrefabMenuNames list using short names
        List<string> shortPrefabMenuNames = af.GetShortPrefabMenuNamesForLayer(layer);
        /*layerPrefabMenuNames = af.GetPrefabMenuNamesForLayer(sourceLayerList);
        List<string> shortPrefabMenuNames = new List<string>();
        for (int i = 0; i < layerPrefabMenuNames.Count; i++)
        {
            string shortprefabName = af.StripPanelRailFromName(layerPrefabMenuNames[i]); ;
            shortPrefabMenuNames.Add(shortprefabName);
            //-- Add a space after the Category divider for clarity
            shortPrefabMenuNames[i] = shortPrefabMenuNames[i].Replace("/", "/  ");
        }*/

        // We don't actually need or use this, it's just for sanity cross-checking while debugging, easier than looking at properties
        ///List<int> sourceVariant_MenuIndices = af.GetSourceVariantMenuIndicesListForLayer(layer);

        //=======================================================================================================
        //                              Main Loop to Show Each Row Of SourceVariants
        //=======================================================================================================
        for (int i = 1; i < numSourceVariantsInUse + 1; i++)
        {
            //changedVariantChoices = false;
            bool print = false;
            EditorGUI.BeginChangeCheck();
            string varName = "";
            GUILayout.BeginHorizontal();

            //===========================================================
            //      GameObject: Choose From Menu or < > Buttons
            //===========================================================
            GUILayout.Space(2);
            EditorGUI.BeginChangeCheck();
            GUILayout.Label(new GUIContent("Var " + i + ": ", "Choose a Variation that will be available to swap in certain sections"), GUILayout.Width(36));

            //       For Row i, get the menu Index from the List of 9 Menu Indices
            //int currMenuIndexForRow = af.railASourceVariant_MenuIndices[i];

            //      Get this  Menu index from the property (int)
            //============================================================
            SerializedProperty currSourceVariant_PrefabMenuIndex_ForRowProp = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i);
            int sourceMenuIndex = currSourceVariant_PrefabMenuIndex_ForRowProp.intValue;
            //Debug.Log($"sourceMenuIndex {i} :  {sourceMenuIndex} \n");

            //=====================
            //     < > Buttons
            //=====================
            if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(17)) && sourceMenuIndex > 0)
            {
                currSourceVariant_PrefabMenuIndex_ForRowProp.intValue -= 1; // A List 9 menu items that contain the index in to the global Prefabs Menus
                changedVariantChoices = true;
            }
            if (GUILayout.Button(">", EditorStyles.miniButton, GUILayout.Width(17)) && sourceMenuIndex < layerPrefabMenuNames.Count - 1)
            {
                currSourceVariant_PrefabMenuIndex_ForRowProp.intValue += 1;
                sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i).intValue += 1;
                //Debug.Log($"Setting{i}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i).intValue}");
                changedVariantChoices = true;
                //print = true;
            }

            //==================
            //      Popup
            //==================
            currSourceVariant_PrefabMenuIndex_ForRowProp.intValue = EditorGUILayout.Popup("", currSourceVariant_PrefabMenuIndex_ForRowProp.intValue, shortPrefabMenuNames.ToArray(), GUILayout.Width(300));
            GUILayout.Space(1);
            int prefabMenuIndex = currSourceVariant_PrefabMenuIndex_ForRowProp.intValue;

            /*string prefabMenuName = shortPrefabMenuNames[componentDisplayMenuIndex];
            Debug.Log($"Setting{i}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i).intValue}");
            if (print)
                Debug.Log($"Setting{i}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i).intValue}");*/

            //===================================
            //      Button - Set As Main
            //===================================
            //-- Do this last so that menus don't override it
            if (GUILayout.Button(new GUIContent("Use Main", "Use the same prefab as the default base Object: " +
             mainPrefabs[(int)prefabType].name), EditorStyles.miniButton, GUILayout.Width(70)))
            {
                // Setup the correct menu and prefab indices
                currSourceVariant_PrefabMenuIndex_ForRowProp.intValue = mainPrefabMenuIndex;
                //sourceVariantPrefabIndices[i] = mainPrefab_PrefabIndex;
                changedVariantChoices = true;
            }
            //None have been chosen since starting, so assign the main prefab
            if (currSourceVariant_PrefabMenuIndex_ForRowProp.intValue == 0)
            {
                currSourceVariant_PrefabMenuIndex_ForRowProp.intValue = mainPrefabMenuIndex;
            }
            GUILayout.Space(1);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                int newMenuIndex = currSourceVariant_PrefabMenuIndex_ForRowProp.intValue; // the new menu index chosen in the popup etc,
                int newPrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(newMenuIndex, prefabType);// the index into the main prebas Lists
                GameObject newGo = af.GetPrefabAtIndexForLayer(newPrefabIndex, layer);

                changedVariantChoices = true;
                string debugString = "";
                /*for (int j = 0; j < numSourceVariantsInUse; j++)
                {
                    int index = sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(j).intValue;
                    debugString += $"{j}:{index},  {af.GetPrefabAtIndexForLayer(index, sourceLayerList).name}     ";
                }
                Debug.Log($"{debugString} \n");*/

                //-- Update the sourceVariant with the new prefab
                af.SetSourceVariantGoAtIndexForLayer(i, newPrefabIndex, layer);
                //-- Update the menu index for the new sourceVariant
                af.UpdateSourceVariantMenuIndexForLayerFromPrefabIndex(newGo, i, layer);

                //-- Update Sequencer step menus to show the new SourceVariant GOs
                //ed.seqEd.UpdateSequenceAfterSourceVariantsChanged(sourceLayerList);

                List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
                //Debug.Log($"EndChangeCheck SourceVariant {i}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i).intValue}  {sourceVariants[i].Go.name} \n");
                sourceVariants = af.GetSourceVariantsForLayer(layer);
                Debug.Log($"Changed SourceVariant {i}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(i).intValue}  {sourceVariants[i].Go.name} \n");
                //Debug.Log($"Changed SourceVariant {2}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(2).intValue}  {sourceVariants[2].Go.name} \n");
            }
        }

        //=================================
        //      Commit Changes
        //=================================
        if (changedVariantChoices)
        {
            //List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(sourceLayerList);
            //Debug.Log($"Commit SourceVariant {1}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(1).intValue}  {sourceVariants[1].Go.name} \n");
            //Debug.Log($"Commit SourceVariant {2}: {sourceVariant_MenuIndicesProp.GetArrayElementAtIndex(2).intValue}  {sourceVariants[2].Go.name} \n");

            //ed.serializedObject.ApplyModifiedProperties();

            //-- Update SourceVariant GOs from the new menu choices
            //af.UpdateSourceVariantMenuIndicesForLayerFromPrefabIndicies(sourceLayerList);
            //sourceVariants = af.GetSourceVariantsForLayer(sourceLayerList);

            //-- Update Sequencer step menus to show the new SourceVariant GOs
            //ed.seqEd.UpdateSequenceAfterSourceVariantsChanged(sourceLayerList);

            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();

            //af.PrintSourceVariantGOsForLayer(sourceLayerList, activeOnly:true);
        }
        GUILayout.Space(2);
    }

    //===================================================================================
    //-- In the loop above, we have chosen the menu indices, so now we need to update the
    //-- sourceVariant List with the prefabs that correspond to those menu choices
    /*private void SyncSourceVariantsFromMenuIndices(LayerSet sourceLayerList)
    {
        List<int> sourceVariant_MenuIndices = af.GetSourceVariantMenuIndicesForLayer(sourceLayerList);
        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(sourceLayerList);
        SerializedProperty svMenuListProp = ed.edUtils.GetSourceVariantMenuListForLayer(sourceLayerList);

        for (int i = 1; i < numSourceVariantsInUse + 1; i++)
        {
            //Copy from Property to Real List
            //These are the main Prefab Menu Indices
            sourceVariant_MenuIndices[i] = svMenuListProp.GetArrayElementAtIndex(i).intValue;
            //Debug.Log($"{i} : {sourceVariant_MenuIndices[i]} \n");

            //Convert them to indices into the actual Prefab List
            int prefabIndex = sourceVariantPrefabIndices[i] = af.ConvertMenuIndexToPrefabIndexForLayer(sourceVariant_MenuIndices[i], prefabType);

            //Update the sourceVariants list with the new prefabs
            SourceVariant sourceVariant = sourceVariants[i];
            sourceVariant.Go = af.GetPrefabAtIndexForLayer(prefabIndex, sourceLayerList);
        }
    }*/

    //--------------------
    private string GetCategoryNameFromMenuName(string menuName)
    {
        string catName = "";
        int dirPositionInString = menuName.IndexOf('/');

        // If there's a '/', just take name part and strip the rest
        if (dirPositionInString != -1)
            catName = menuName.Substring(0, dirPositionInString);

        return catName;
    }

    //------------------
    /*void ReApplyModifiedEditorParameters()
    {
        if (sourceLayerList == kPostLayer)
        {
            //currSelectedStepIndex = currSeqStepIndex;
        }
        else if (sourceLayerList == kRailALayer)
        {
            //ed.currSeqAStepIndex = currSeqStepIndex;
        }
        else if (sourceLayerList == kRailBLayer)
        {
            //ed.currSeqAStepIndex = currSeqStepIndex;
        }
    }*/
    //-------------------------
    /*public void SyncControlsDisplayFromVariant(SourceVariant sourceVariant, int variantIndex, LayerSet sourceLayerList, bool fillNullsWithMain = true)
    {
        int i = variantIndex;

        if (i == 0)
        {
            af.currentRail_PrefabMenuIndex[0] = af.ConvertRailPrefabIndexToMenuIndex(af.currentRail_PrefabIndex[0]);
        }
        List<int> sourceVariant_PrefabIndices_ForLayer = af.CreateSourceVariantPrefabIndicesListForLayer(sourceLayerList);
        if (sourceLayerList == kRailALayer)
        {
            if (i == 0)
            {
                af.currentRail_PrefabIndex[0] = af.FindRailPrefabIndexByName(sourceVariant.Go.name);
                af.currentRail_PrefabMenuIndex[0] = af.ConvertRailPrefabIndexToMenuIndex(af.currentRail_PrefabIndex[0]);
            }
            if (i > 0 && sourceVariant.Go == null && fillNullsWithMain)
                sourceVariant.Go = af.railSourceVariants[0][0].Go;

            af.railASourceVariant_MenuIndices[i] = af.ConvertRailPrefabIndexToMenuIndex(sourceVariant_PrefabIndices_ForLayer[i]);

            af.varRailAPositionOffset[i] = sourceVariant.svPositionOffset;
            af.varRailARotation[i] = sourceVariant.svRotation;
            af.varRailASize[i] = sourceVariant.svSize;
            af.useRailVarA[i] = sourceVariant.enabled;
            af.varRailABackToFront[i] = sourceVariant.svBackToFront;
            af.varRailABackToFrontBools[i] = System.Convert.ToBoolean(sourceVariant.svBackToFront);
            af.varRailAMirrorZ[i] = sourceVariant.svMirrorZ;
            af.varRailAMirrorZBools[i] = System.Convert.ToBoolean(sourceVariant.svMirrorZ);
            af.varRailAInvert[i] = sourceVariant.svInvert;
            af.varRailAInvertBools[i] = System.Convert.ToBoolean(sourceVariant.svInvert);
            af.varRailAProbs[i] = sourceVariant.probability;
        }
        else if (sourceLayerList == kRailBLayer)
        {
            if (i == 0)
            {
                af.currentRail_PrefabMenuIndex[1] = af.ConvertRailPrefabIndexToMenuIndex(af.currentRail_PrefabIndex[1]);
            }
            if (i > 0 && sourceVariant.Go == null && fillNullsWithMain)
                sourceVariant.Go = af.railSourceVariants[1][0].Go;

            //af.railBSourceVariant_PrefabIndices[i] = af.FindRailPrefabIndexByName(sourceVariant.Go.name);
            af.railBSourceVariant_MenuIndices[i] = af.ConvertRailPrefabIndexToMenuIndex(sourceVariant_PrefabIndices_ForLayer[i]);

            af.varRailBPositionOffset[i] = sourceVariant.svPositionOffset;
            af.varRailBRotation[i] = sourceVariant.svRotation;
            af.varRailBSize[i] = sourceVariant.svSize;
            af.useRailVarB[i] = sourceVariant.enabled;
            af.varRailBBackToFront[i] = sourceVariant.svBackToFront;
            af.varRailBBackToFrontBools[i] = System.Convert.ToBoolean(sourceVariant.svBackToFront);
            af.varRailBMirrorZ[i] = sourceVariant.svMirrorZ;
            af.varRailBMirrorZBools[i] = System.Convert.ToBoolean(sourceVariant.svMirrorZ);
            af.varRailBInvert[i] = sourceVariant.svInvert;
            af.varRailBInvertBools[i] = System.Convert.ToBoolean(sourceVariant.svInvert);
            af.varRailBProbs[i] = sourceVariant.probability;
        }
        else if (sourceLayerList == kPostLayer)
        {
            if (i == 0)
            {
                af.currentPost_PrefabMenuIndex = af.ConvertPostPrefabIndexToMenuIndex(af.currentPost_PrefabIndex);
            }
            if (i > 0 && sourceVariant.Go == null && fillNullsWithMain)
                sourceVariant.Go = af.postVariants[0].Go;

            //af.varPrefabIndexPost[i] = af.FindPostPrefabIndexByName(sourceVariant.Go.name);
            //af.postVarMenuIndex[i] = af.ConvertPostPrefabIndexToMenuIndex(af.varPrefabIndexPost[i]);

            af.varPostPositionOffset[i] = sourceVariant.svPositionOffset;
            af.varPostRotation[i] = sourceVariant.svRotation;
            af.varPostSize[i] = sourceVariant.svSize;
            af.usePostVar[i] = sourceVariant.enabled;
            af.varPostProbs[i] = sourceVariant.probability;
        }
    }*/
    //-------------------------
    // a Main rail/post/extra prefab was changed from the selction menus
    /*public void SyncControlsAfterComponentChange()
    {
        af.railSourceVariants[0][0].Go = af.railPrefabs[af.currentRail_PrefabIndex[0]];
        af.railSourceVariants[1][0].Go = af.railPrefabs[af.currentRail_PrefabIndex[1]];
        af.postVariants[0].Go = af.postPrefabs[af.currentPost_PrefabIndex];
    }*/

    //------------------
    /*public void FillEmptyVariantsWithMain()
    {
        string goName = af.GetSourceVariantGONameAtIndexForLayer(0, sourceLayerList);

        int prefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.railPrefab, goName);
        int mainMenuIndexA = af.ConvertRailPrefabIndexToMenuIndex(prefabIndex);

        prefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.railPrefab, af.railSourceVariants[1][0].Go.name);
        int mainMenuIndexB = af.ConvertRailPrefabIndexToMenuIndex(prefabIndex);

        prefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, af.postVariants[0].Go.name);
        int mainMenuIndexPost = af.ConvertPostPrefabIndexToMenuIndex(prefabIndex);

        //prefabIndex = af.FindPrefabIndexByNameForLayer(prefabType.postPrefab, af.subpostVariants[0].go.name);
        int mainMenuIndexSubpost = af.ConvertPostPrefabIndexToMenuIndex(prefabIndex);

        for (int i = 1; i < AutoFenceCreator.kMaxNumSourceVariants; i++)
        {
            if (af.railSourceVariants[0][i].Go == null)
            {
                af.railSourceVariants[0][i].Go = af.railSourceVariants[0][0].Go;
                af.railASourceVariant_MenuIndices[i] = mainMenuIndexA;
            }
            if (af.railSourceVariants[1][i].Go == null)
            {
                af.railSourceVariants[1][i].Go = af.railSourceVariants[1][0].Go;
                af.railASourceVariant_MenuIndices[i] = mainMenuIndexB;
            }
        }
        for (int i = 1; i < AutoFenceCreator.kMaxNumSourceVariants; i++)
        {
            if (af.postVariants[i].Go == null)
            {
                af.postVariants[i].Go = af.postVariants[0].Go;
                //af.postVarMenuIndex[i] = mainMenuIndexPost;
            }
        }
    }*/

    //-----------------------------------------
    public void CheckValidPrefabPresetIndices()
    {
        if (af.currentRail_PrefabIndex[0] >= af.railPrefabs.Count)
            af.currentRail_PrefabIndex[0] = 0;
        if (af.currentRail_PrefabIndex[1] >= af.railPrefabs.Count)
            af.currentRail_PrefabIndex[1] = 0;
        if (af.currentPost_PrefabIndex >= af.postPrefabs.Count)
            af.currentPost_PrefabIndex = 0;
        if (af.currentSubpost_PrefabIndex >= af.postPrefabs.Count)
            af.currentSubpost_PrefabIndex = 0;
        if (af.currentExtra_PrefabIndex >= af.extraPrefabs.Count)
            af.currentExtra_PrefabIndex = 0;
    }

    //-----------------------------------------
    // Whenever a base part changes, ensure that it's added in the sourceVariants list
    /* public void SetMainSourceVariantObjects()
     {
         //if (af.railSourceVariants[0].Count > AutoFenceCreator.kNumRailVariations || af.nonNullRailSourceVariants[0].Count > AutoFenceCreator.kNumRailVariations)
         //Debug.Log("Too Many Rail Variants in SetMainSourceVariantObjects() 1   " + af.railSourceVariants[0].Count + "   " + af.nonNullRailSourceVariants[0].Count);

         CheckValidPrefabPresetIndices();

         //if (af.railSourceVariants[0].Count > AutoFenceCreator.kNumRailVariations || af.nonNullRailSourceVariants[0].Count > AutoFenceCreator.kNumRailVariations)
         //Debug.Log("Too Many Rail Variants in SetMainSourceVariantObjects() 2   " + af.railSourceVariants[0].Count + "   " + af.nonNullRailSourceVariants[0].Count);

         //-- always set the [0] to be the main rail
         af.railSourceVariants[0][0].Go = af.railPrefabs[af.currentRail_PrefabIndex[0]];

         if (af.nonNullRailSourceVariants[0].Count == 0)
             af.nonNullRailSourceVariants[0].Add(af.railSourceVariants[0][0]);
         else if (af.nonNullRailSourceVariants[0][0] == null)
             af.nonNullRailSourceVariants[0][0] = af.railSourceVariants[0][0];

         af.railSourceVariants[1][0].Go = af.railPrefabs[af.currentRail_PrefabIndex[1]];
         if (af.nonNullRailSourceVariants[1].Count == 0)
             af.nonNullRailSourceVariants[1].Add(af.railSourceVariants[1][0]);
         else if (af.nonNullRailSourceVariants[1][0] == null)
             af.nonNullRailSourceVariants[1][0] = af.railSourceVariants[1][0];

         af.postVariants[0].Go = af.postPrefabs[af.currentPost_PrefabIndex];
     }*/

    //------------------------
    //-- Ensure that all the  railSourceVariants[0] are populated
    /*public void CheckVariationGOs()
    {
        //---- Rails A ----
        if (af.railSourceVariants[0].Count < AutoFenceCreator.kMaxNumSourceVariants)
        {
            af.railSourceVariants[0].Clear();
            af.railSourceVariants[0].AddRange(new SourceVariant[AutoFenceCreator.kMaxNumSourceVariants]);
            af.railSourceVariants[0][0] = new SourceVariant(af.railPrefabs[af.currentRail_PrefabIndex[0]]);
        }
        else
        {
            for (int i = 0; i < af.railSourceVariants[0].Count; i++)
            {
                if (af.railSourceVariants[0][i] == null)
                {
                    af.railSourceVariants[0][i] = new SourceVariant();
                    af.railSourceVariants[0][i].Go = af.railSourceVariants[0][0].Go;
                    af.railSourceVariants[0][i].enabled = af.useRailVarA[i];
                    af.railSourceVariants[0][i].probability = af.varRailAProbs[i];
                    af.railSourceVariants[0][i].svPositionOffset = af.varRailAPositionOffset[i];
                    af.railSourceVariants[0][i].svSize = af.varRailASize[i];
                    af.railSourceVariants[0][i].svRotation = af.varRailARotation[i];
                }
                else if (af.railSourceVariants[0][i].Go == null)
                {
                    af.railSourceVariants[0][i].Go = af.railSourceVariants[0][0].Go;
                }
                //int afMenuIndex = af.railASourceVariant_MenuIndices[i];
                //int realPrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(afMenuIndex, PrefabTypeAFWB.railPrefab);
                //if (realPrefabIndex < 1 || realPrefabIndex >= af.railPrefabs.Count)
                //    Debug.LogWarning(realPrefabIndex + af.railPrefabs.Count + "\n");
                //GameObject go = af.railPrefabs[realPrefabIndex];
                //af.railSourceVariants[0][i].Go = go;
            }
        }
        //af.nonNullRailSourceVariants[0] = af.CreateUsedVariantsList(kRailALayer);
        for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
        {
            if (af.railASequencer.seqList[i] == null)
            {
                SeqItem seqVarA = new SeqItem();
                seqVarA.pos = af.seqRailAOffset[i];
                seqVarA.size = af.seqRailASize[i];
                seqVarA.rot = af.seqRailARotate[i];
                //seqVarA.svMirrorZ = af.seqAZ[i];
                //seqVarA.svBackToFront = af.seqAX[i];
                //seqVarA.svInvert = af.seqAInvert180[i];
                //seqVarA.sourceVariantIndex = af.seqRailASourceVarIndex[i];
                //af.railASequencer.seqList[i] = seqVarA;
            }
        }

        //---- Rails B ----
        if (af.railSourceVariants[1].Count < AutoFenceCreator.kMaxNumSourceVariants)
        {
            af.railSourceVariants[1].Clear();
            af.railSourceVariants[1].AddRange(new SourceVariant[AutoFenceCreator.kMaxNumSourceVariants]);
            af.railSourceVariants[1][0] = new SourceVariant(af.railPrefabs[af.currentRail_PrefabIndex[1]]);
        }
        else
        {
            for (int i = 0; i < af.railSourceVariants[1].Count; i++)
            {
                if (af.railSourceVariants[1][i] == null)
                {
                    af.railSourceVariants[1][i] = new SourceVariant();
                    af.railSourceVariants[1][i].Go = af.railSourceVariants[1][0].Go;
                    af.railSourceVariants[1][i].enabled = af.useRailVarB[i];
                    af.railSourceVariants[1][i].probability = af.varRailBProbs[i];
                    af.railSourceVariants[1][i].svPositionOffset = af.varRailBPositionOffset[i];
                    af.railSourceVariants[1][i].svSize = af.varRailBSize[i];
                    af.railSourceVariants[1][i].svRotation = af.varRailBRotation[i];
                }
                else if (af.railSourceVariants[1][i].Go == null)
                {
                    af.railSourceVariants[1][i].Go = af.railSourceVariants[1][0].Go;
                }
            }
        }
        //af.nonNullRailSourceVariants[1] = af.CreateUsedVariantsList(kRailBLayer);
        for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
        {
            if (af.railBSequencer.seqList[i] == null)
            {
                SeqItem seqVarB = new SeqItem();
                seqVarB.pos = af.seqRailBOffset[i];
                seqVarB.size = af.seqRailBSize[i];
                seqVarB.rot = af.seqRailBRotate[i];
                //seqVarB.svMirrorZ = af.seqBZ[i];
                //seqVarB.svBackToFront = af.seqBX[i];
                //seqVarB.svInvert = af.seqBInvert180[i];
                seqVarB.sourceVariantIndex = af.seqRailBSourceVarIndex[i];
                af.railBSequencer.seqList[i] = seqVarB;
            }
        }

        //---- Posts ----
        if (af.postVariants.Count < AutoFenceCreator.kMaxNumSourceVariants)
        {
            af.postVariants.Clear();
            af.postVariants.AddRange(new SourceVariant[AutoFenceCreator.kMaxNumSourceVariants]);
            af.postVariants[0] = new SourceVariant(af.postPrefabs[af.currentPost_PrefabIndex]);
        }
        else
        {
            if (af.postVariants[0].Go == null)
                af.postVariants[0].Go = af.postPrefabs[af.currentPost_PrefabIndex];
            for (int i = 0; i < af.postVariants.Count; i++)
            {
                if (af.postVariants[i] == null)
                //{
                //    af.postVariants[i] = new SourceVariant();
                //    af.postSourcevariants[i].Go = af.postVariants[0].Go;
                //    af.postVariants[i].enabled = af.usePostVar[i];
                //    af.postVariants[i].probability = af.varPostProbs[i];
                //    af.postVariants[i].svPositionOffset = af.varPostPositionOffset[i];
                //    af.postVariants[i].svSize = af.varPostSize[i];
                //    af.postVariants[i].svRotation = af.varPostRotation[i];
                }
                else if (af.postVariants[i].Go == null)
                {
                    af.postVariants[i].Go = af.postVariants[0].Go;
                }
            }
        }
        //af.nonNullPostVariants = af.CreateUsedVariantsList(kPostLayer);
        for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
        {
            if (af.postSequencer.seqList[i] == null)
            {
                SeqItem seqVarPost = new SeqItem();
                seqVarPost.pos = af.seqPostOffset[i];
                seqVarPost.size = af.seqPostSize[i];
                seqVarPost.rot = af.seqPostRotate[i];
                seqVarPost.sourceVariantIndex = af.seqPostSourceVarIndex[i];
                af.postSequencer.seqList[i] = seqVarPost;
            }
        }
        //---- Subosts ----
        if (af.subpostVariants.Count < AutoFenceCreator.kMaxNumSourceVariants)
        {
            af.subpostVariants.Clear();
            af.subpostVariants.AddRange(new SourceVariant[AutoFenceCreator.kMaxNumSourceVariants]);
            af.subpostVariants[0] = new SourceVariant(af.postPrefabs[af.currentSubpost_PrefabIndex]);
        }
        else
        {
            if (af.subpostVariants[0].go == null)
                af.subpostVariants[0].go = af.postPrefabs[af.currentPost_PrefabIndex];
            for (int i = 0; i < af.subpostVariants.Count; i++)
            {
                if (af.subpostVariants[i] == null)
                {
                    af.subpostVariants[i] = new SourceVariant(af.subpostDisplayVariationGOs[i]);
                    af.subpostvariants[i].Go = af.subpostVariants[0].go;
                    af.subpostVariants[i].enabled = af.useSubpostVar[i];
                    //af.subpostVariants[i].probability = af.varSubpostProbs[i];
                    af.subpostVariants[i].svPositionOffset = af.varSubpostPositionOffset[i];
                    af.subpostVariants[i].svSize = af.varSubpostSize[i];
                    af.subpostVariants[i].svRotation = af.varSubpostRotation[i];
                }
                else if (af.subpostvariants[i].Go == null)
                {
                    af.subpostvariants[i].Go = af.subpostVariants[0].go;
                }
            }
        }
        //af.nonNullPostVariants = af.CreateUsedVariantsList(kPostLayer);
        for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
        {
            if (af.userSequenceSubpost[i] == null)
            {
                SeqItem seqVarSubpost = new SeqItem();
                seqVarSubpost.pos = af.seqSubpostOffset[i];
                seqVarSubpost.svSize = af.seqSubpostSize[i];
                seqVarSubpost.rot = af.seqSubpostRotate[i];
                seqVarSubpost.sourceVariantIndex = af.seqSubpostSourceVarIndex[i];
                af.userSequenceSubpost[i] = seqVarSubpost;
            }
        }
    }*/
}