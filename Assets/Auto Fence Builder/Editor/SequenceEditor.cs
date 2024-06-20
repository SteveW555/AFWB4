//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

/*===================================================================
 * This class handles the sequencing of prefab variations
 * It also selects desired variation prefabs
 * ==================================================================*/

using AFWB;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum SeqShortcuts
{ First, Second, Third, Fourth }

public class SequenceEditor
{
    private LayerSet kRailALayer = LayerSet.railALayer;
    private LayerSet kRailBLayer = LayerSet.railBLayer;
    private LayerSet kPostLayer = LayerSet.postLayer;
    private LayerSet kSubpostLayer = LayerSet.subpostLayer;

    private AutoFenceCreator af;
    private AutoFenceEditor ed;

    private SerializedProperty numSourceVariantsInUseProp, sequencerProp, useSequencerProp;
    private SerializedProperty seqOffset, seqSize, seqRot;
    private SerializedProperty seqListProperty; // the list of seqItems for this sourceLayerList

    private SeqItem currSeqStepVariant = null;
    private List<SeqItem> seqList = null; // the list of SeqItem for all seq steps
    private List<SourceVariant> sourceVariants = null; // the 8+1 prefabs that have been assigned as possible sourceVariants
    public List<GameObject> mainPrefabs = null;

    private bool showSourcePrefabs = true, showOptimiseRandomise = false;
    private bool needsRebuild = false;
    private bool[] prefabButtonSwitch = new bool[AutoFenceCreator.kMaxNumSeqSteps];
    private PrefabTypeAFWB prefabType;
    private LayerSet layer;
    private int layerIndex = 0; // 0 = railA, 1 = railB
    private int maxNumVariations = 0;
    private int mainPrefab_MenuIndex = 0, mainPrefab_PrefabIndex = 0, numSectionsBuiltForLayer = 0;
    private List<int> variantPrefabIndex = null;

    //List<int> variantGlobalPrefabMENUIndices9 = null;
    private string displayVariationGOsStr = "";

    private string layerNameString = "post";
    private string randomToUserSeqString = "Quick-Fill Random", optimalToUserSeqString = "Quick-Fill Optimal";
    private List<string> globalPrefabNames = null;

    private int autoAssignMenuIndex = 0, autoAssignToolbarValue = 0;
    private string[] optimiseRandomiseToolbarStrings = new[] { "Optimise Sequence...", "Randomise Sequence..." };
    private int numSourceVariantsInUse, numPostVariations;
    public bool allPostsDisabled = false, allRailADisabled = false, allRailBDisabled = false, allExtrasDisabled = false; //
    private Vector2 scrollPos = Vector2.zero;
    public bool[] allDisabled = { false, false, false, false, false, false, false, false }; //8, one for each possible variant
    //private int seqNumSteps = 2;
    public SeqItem copySeqStepVariant = null; // a convenient place to store a copy ready for pasting
    Sequencer sequencer;
    public SequenceEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor/*, LayerSet sourceLayerList*/)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        SerializedObject so = ed.serializedObject;
    }

    //------------------
    // To avoid lots of case switching in the ed code, set the variables here
    private void GrabVariablesForSet()
    {
        //      SerializedProperties
        //===============================
        seqListProperty = ed.GetSequencerListForLayerProp(layer);
        //seqNumStepsProp = ed.GetSequencerNumStepsForLayerProp(sourceLayerList);
        numSourceVariantsInUseProp = ed.GetNumSourceVariantsInUseForLayerProp(layer);

        //      AF and sourceLayerList Variables
        //===============================
        seqList = GetSequencerListForLayer(layer);
        //seqNumSteps = seqNumStepsProp.intValue;
        numSourceVariantsInUse = numSourceVariantsInUseProp.intValue;

        maxNumVariations = AutoFenceCreator.kMaxNumSourceVariants;
        sourceVariants = af.GetSourceVariantsForLayer(layer);
        globalPrefabNames = af.GetPrefabNamesForLayer(layer);
        mainPrefabs = af.GetPrefabsForLayer(layer);
        mainPrefab_MenuIndex = af.GetMainPrefabMenuIndexForLayer(layer);
        mainPrefab_PrefabIndex = af.GetCurrentPrefabIndexForLayer(layer);
        prefabType = af.GetPrefabTypeFromLayer(layer);
        layerIndex = (int)layer;
        numSectionsBuiltForLayer = af.GetNumSectionsBuiltForLayer(layer);
        layerNameString = af.GetLayerNameAsString(layer, useCamel: true);
        sequencer = af.GetSequencerForLayer(layer);
        
    }

    private void GetSequencerPropForLayer()
    {
        if (layer == LayerSet.postLayer)
            sequencerProp = ed.serializedObject.FindProperty("postSequencer"); //-- Replace "sequencer" with the actual name of your Sequencer field
        if (layer == LayerSet.railALayer)
            sequencerProp = ed.serializedObject.FindProperty("railASequencer");
        else if (layer == LayerSet.railBLayer)
            sequencerProp = ed.serializedObject.FindProperty("railBSequencer");
        useSequencerProp = sequencerProp.FindPropertyRelative("useSeq");
    }

    //--------------------------------
    /// <summary>
    /// Sets up the Variation Sequencer interface for the given LayerSet. The interface allows users to
    /// configure and control the variations of each section in the sequence for the specified sourceLayerList.
    /// </summary>
    /// <param name="layer">The LayerSet for which the Variation Sequencer interface is being set up.</param>
    public void SetupStepSeqVariations(LayerSet layer)
    {
        ed.CheckPrefabsExistForLayer(layer); //debug only

        //bool useSequencer = GetUseSequencerForLayer(sourceLayerList);

        this.layer = layer;
        GetSequencerPropForLayer();
        needsRebuild = false;
        ed.DrawUILine(ed.UILineGrey, 6, 10, 2, 10);
        GUILayout.BeginHorizontal();
        ed.cyanBoldStyle.wordWrap = false;
        GUILayout.Space(9); //puts foldout triangle inside box

        //==============================
        //      Sequencer Header
        //==============================
        //{
        if (af.IsRailLayer(layer))
            af.showRailSequencer[layerIndex] = EditorGUILayout.Foldout(af.showRailSequencer[layerIndex], "");
        else
            af.showPostSequencer = EditorGUILayout.Foldout(af.showPostSequencer, "");
        GUILayout.Space(78);

        //      Use Variation Step Sequencer
        //======================================
        EditorGUILayout.LabelField(new GUIContent(af.GetLayerNameAsString(layer) + " Variation Sequencer",
            "Use a sequence of steps to modify each section"), ed.greenStyle2, GUILayout.Width(175));
        EditorGUI.BeginChangeCheck();
            
        EditorGUILayout.PropertyField(useSequencerProp,new GUIContent(""), GUILayout.Width(20));

        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(this.layer);
            af.ForceRebuildFromClickPoints();
        }
        if(useSequencerProp.boolValue == false)
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return;
        }
        GrabVariablesForSet();

        //      Step Sequencer Help
        //================================
        GUILayout.FlexibleSpace();
        //GUILayout.Space(10);
        GUIStyle helpButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        helpButtonStyle.fontStyle = FontStyle.Bold;
        helpButtonStyle.normal.textColor = new Color(0.2f, .50f, .78f);
        string helpStr = "?";
        int helpButtW = 24;
        if (ed.showSeqHelp)
        {
            helpStr = "Close";
            helpButtW = 45;
        }
        if (GUILayout.Button(new GUIContent(helpStr, "Show Help for Variation Sequencer"), helpButtonStyle, GUILayout.Width(helpButtW)))
        {
            ed.showSeqHelp = !ed.showSeqHelp;
        }

        //      Needs Variations Warning
        //===================================
        GUILayout.Space(10);
        if (af.GetUseVariationsForLayer(layer) == false)
            EditorGUILayout.LabelField(new GUIContent("[Needs Rail Variations Enabled]",
            "Use a sequence of steps to modify each section"), ed.label11Style, GUILayout.Width(175));

        GUILayout.EndHorizontal();
        if (ed.showSeqHelp)
        {
            GUILayout.BeginVertical("box");
            ed.varEd.ShowSequencerHelp();
            if (GUILayout.Button("Close", GUILayout.Width(45)))
            {
                ed.showSeqHelp = false;
            }
            GUILayout.Space(3);
            GUILayout.EndVertical();
        }
        //}

        //================================
        //  Set Up Step Sequencer
        //================================
        if (af.GetUseSequencerForLayer(layer) == false)
            return;

        bool showSeq = af.GetShowSequencerForLayer(layer);

        using (new EditorGUI.DisabledScope(af.GetUseSequencerForLayer(layer) == false))
        {
            //      Setup Sequencer Headers
            //===================================
            if (showSeq)
            {
                GUILayout.Space(6);
                GUILayout.BeginVertical();////
                GUILayout.Space(6);

                int indentSpace = 10;
                
                //========================================
                //          Set Number Of Steps
                //========================================
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                EditorGUILayout.LabelField("Set Number Of Seq Steps: ", ed.greenStyle2, GUILayout.Width(170));

                
                //      Int Text Field
                //=========================
                int currentCount = seqListProperty.arraySize;
                int newCount = EditorGUILayout.IntField("", seqListProperty.arraySize);
                if (newCount != currentCount)
                    seqListProperty.arraySize = newCount;

                //      Remove Button
                //=========================
                GUILayout.Space(8);
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(17)) && currentCount > 2)
                {
                    sequencer.RemoveLastNSeqItems(1);
                }
                GUILayout.Space(1);
                
                //      Add Button
                //=========================
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(17)) && currentCount < AutoFenceCreator.kMaxNumSeqSteps)
                {
                    sequencer.AddSeqItems(1);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    ed.serializedObject.ApplyModifiedProperties();
                    allDisabled[(int)this.layer] = AreAllSeqItemsDisabledForLayer(af.GetSequencerForLayer(layer), af.GetNumSectionsBuiltForLayer(this.layer));
                    SetAllLayersDisabledStatus(allDisabled[(int)this.layer], this.layer);
                    af.ResetPoolForLayer(this.layer);
                    af.ForceRebuildFromClickPoints();
                }

                //=================================
                //    Enable All Button
                //=================================
                if (allDisabled[(int)this.layer] == true)
                    EditorGUILayout.LabelField("All steps are disabled!", ed.warningStyle, GUILayout.Width(140));

                GUILayout.Space(8);

                EditorGUI.BeginChangeCheck();
                bool allEnableStateChanged = false, allEnableState = true;

                if (allDisabled[(int)this.layer])
                    GUILayout.Space(2);
                else
                    GUILayout.Space(100);
                if (GUILayout.Button(new GUIContent("Enable All", "Enable All Sequencer Steps"), EditorStyles.miniButton, GUILayout.Width(72)))
                {
                    allEnableState = true; allEnableStateChanged = true;
                    for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
                    {
                        seqList[i].stepEnabled = true;
                    }
                    af.ClearConsole();
                    // af.PrintSeqStepGOs(LayerSet.railALayer, false);
                    for (int i = 0; i < seqList.Count; i++)
                    {
                        List<int> sectionsUsingStep = GetSectionsUsingSeqStep(i, layer);
                        //PrintUtilitiesTCT.PrintList(sectionsUsingStep, "", $"Step [{i}]  is used in Sections:    ");
                    }
                }
                SeqItem sv = new SeqItem();
                sv.sourceVariantIndex = 0;

                GUILayout.Space(10);
                GUILayout.EndHorizontal();
                //============================
                //     Limit No. Steps
                //============================
                //if (af.GetSequenceForLayer(this.sourceLayerList).Count > numSectionsBuiltForLayer)
                if (af.GetSequenceForLayer(this.layer).Count > numSectionsBuiltForLayer)
                {
                    GUILayout.BeginHorizontal();
                    //add checkbox
                    EditorGUILayout.LabelField(new GUIContent("  Only Show Sufficient For Built Sections (" + numSectionsBuiltForLayer + ")", "Limit displayed steps to number of sections built"), GUILayout.Width(252));
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(ed.serializedObject.FindProperty("limitDisplayedSteps"), new GUIContent(""), GUILayout.Width(20));
                    if (EditorGUI.EndChangeCheck())
                    {
                        ed.serializedObject.ApplyModifiedProperties();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(2);

                if (EditorGUI.EndChangeCheck())
                {
                    needsRebuild = true;
                    ed.serializedObject.ApplyModifiedProperties();
                    allDisabled[(int)this.layer] = AreAllSeqItemsDisabledForLayer(af.GetSequencerForLayer(this.layer), af.GetNumSectionsBuiltForLayer(this.layer));
                    SetAllLayersDisabledStatus(allDisabled[(int)this.layer], this.layer);
                    af.ForceRebuildFromClickPoints(this.layer);
                }
                //-------------------------------------
                GUILayout.Space(5); // vert space before toolbar

                string[] fullPrefabNames = GetSourceVariantNamesForLayer(this.layer);

                Color switchRed2 = new Color(0.7f, 0.6f, 0.6f);
                Color switchGreen2 = new Color(0.3f, 0.85f, 0.3f);
                GUIStyle stepButtonsStyle = new GUIStyle(EditorStyles.miniButton);

                EditorGUI.BeginChangeCheck();

                //==========================================================================================
                //                              Step Loops
                //
                //    Main loop dispaying each Step's parameters. UI = two Horizontal rows per step
                //==========================================================================================

                //    Ensure the Popup names are up to date with the SourceVariants
                GUIContent[] seqPopupStrings = GetSeqPopupStrings(this.layer);
                GUIContent[] seqPopupStringsOnlyOnesNeeded = seqPopupStrings.Take(numSourceVariantsInUse + 1).ToArray();

                int numStepsToShow = (af.GetSequenceForLayer(this.layer).Count);
                if (af.limitDisplayedSteps && numStepsToShow > numSectionsBuiltForLayer)
                    numStepsToShow = numSectionsBuiltForLayer;
                int numBeforeScrolling = 9, cellFeight = 64;
                float scrollBarHeight = (cellFeight * numStepsToShow) > (cellFeight * numBeforeScrolling) ? (cellFeight * numBeforeScrolling) : (cellFeight * numStepsToShow);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(600), GUILayout.Height(scrollBarHeight));
                bool goWasChanged = false;
                bool isMouseOverButton = false;
                SerializedProperty stepProp = null, stepEnabledProp = null;

                for (int i = 0; i < numStepsToShow; i++)
                {
                    GUILayout.BeginVertical("box");

                    // start a disabled section
                    //using (new EditorGUI.DisabledScope(seqList[i].stepEnabled == false))
                    using (new EditorGUI.DisabledScope(i >= numSectionsBuiltForLayer))
                    {
                        //      SeqItem For This Step
                        //===================================
                        stepProp = seqListProperty.GetArrayElementAtIndex(i);

                        //========================
                        //      Enable Button
                        //========================
                        string stepStr = "Step " + (i + 1);
                        helpStr = "Change the Display (on/Off) of  Step " + i + "\nWhen Off the section will not be visible";
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(4);
                        using (new EditorGUI.DisabledScope(i > numSectionsBuiltForLayer == true))
                        {
                            if (seqList[i].stepEnabled == true)
                            {
                                stepButtonsStyle.normal.textColor = switchGreen2;
                                stepButtonsStyle.hover.textColor = switchGreen2;
                            }
                            else
                            {
                                stepButtonsStyle.normal.textColor = switchRed2;
                                stepButtonsStyle.hover.textColor = switchRed2;
                            }
                            if (GUILayout.Button(new GUIContent(stepStr, helpStr), stepButtonsStyle, GUILayout.Width(50)))
                            {
                                stepEnabledProp = stepProp.FindPropertyRelative("stepEnabled");
                                //-- Toggle the bool both on the Property and real
                                stepEnabledProp.boolValue = !stepEnabledProp.boolValue;
                                seqList[i].stepEnabled = stepEnabledProp.boolValue;
                            }
                            Rect buttonRect = GUILayoutUtility.GetLastRect();
                            // Check if the mouse is over the button
                            if (buttonRect.Contains(Event.current.mousePosition))
                            {
                                af.highlightedParts.Clear();
                                if (!isMouseOverButton)
                                {
                                    // Mouse just started hovering over the button

                                    List<int> sectionsUsingStep = GetSectionsUsingSeqStep(i, layer);
                                    int numSectionsUsingStep = sectionsUsingStep.Count;
                                    // for each section using this step, get the position of the prefab at that section

                                    for (int s = 0; s < numSectionsUsingStep; s++)
                                    {
                                        int sectionIndex = sectionsUsingStep[s];
                                        Transform transformForSection = af.GetBuiltTransformAtSectionIndexForLayer(sectionIndex, layer);
                                        af.highlightedParts.Add(transformForSection);
                                    }

                                    //PrintUtilitiesTCT.PrintList(sectionsUsingStep, "", $"Step [{i}]  is used in Sections:    ");

                                    isMouseOverButton = true;
                                    //af.highlightedPart =
                                    //OnMouseEnterButton();
                                }
                            }
                            else
                            {
                                if (isMouseOverButton)
                                {
                                    // Mouse just stopped hovering over the button
                                    isMouseOverButton = false;
                                    //OnMouseExitButton();
                                }
                            }
                        }
                        //     Set Color Of StepEnabled Button
                        //========================================
                        if (seqList[i].stepEnabled == true)
                        {
                            stepButtonsStyle.normal.textColor = EditorStyles.miniButton.normal.textColor;
                            stepButtonsStyle.hover.textColor = EditorStyles.miniButton.normal.textColor;
                        }
                        using (new EditorGUI.DisabledScope(seqList[i].stepEnabled == false))
                        {
                            //=======================================
                            //        Popup Prefab Choice
                            //=======================================

                            //      The SourceVariant for this step
                            //=======================================
                            SerializedProperty sourceVariantIndexForStepProp = ed.GetSequencerSourceVariantAtStepIndexProp(stepProp, layer, i);
                            int sourceVariantIndex = sourceVariantIndexForStepProp.intValue;

                            Rect popupRect = GUILayoutUtility.GetRect(new GUIContent(fullPrefabNames[seqList[i].sourceVariantIndex]), EditorStyles.popup, GUILayout.Width(185));
                            try
                            {
                                GUIStyle popStyle = EditorStyles.popup;
                                GUIContent[] seqPopupStrs = seqPopupStringsOnlyOnesNeeded;
                                int requestedIndex = af.GetVariantIndexForSequencerStep(i, this.layer);
                                if (requestedIndex > numSourceVariantsInUse)
                                {
                                    popStyle = ed.orangePopupStyle;
                                    //seqPopupStrs = seqPopupStrs;
                                }
                                /*              Important
                                 *              The railASequencer.seqList isn't getting set manually
                                 *              It's just updated by the serializedObject, when ApplyModifiedProperties() is called
                                 *              and actually chosen and instantiated in CreateRailPool()
                                 */

                                //      Popup
                                //==================
                                sourceVariantIndexForStepProp.intValue = EditorGUI.Popup(popupRect, sourceVariantIndexForStepProp.intValue, seqPopupStrs, popStyle);
                            }
                            catch (System.Exception)
                            {
                                Debug.LogError("err\n");
                                throw;
                            }

                            if (sourceVariantIndexForStepProp.intValue != sourceVariantIndex)
                                goWasChanged = true;

                            //=================================
                            //        Orientation Switches
                            //=================================
                            GUILayout.Space(4);
                            HandleSeqOrientationSwitches(this.layer, i, indentSpace);

                            //=================================
                            //        Copy/Paste
                            //=================================
                            if (GUILayout.Button(new GUIContent("C", "Copy all Step Parameters"), EditorStyles.miniButton, GUILayout.Width(21)))
                            {
                                copySeqStepVariant = seqList[i];
                            }
                            if (GUILayout.Button(new GUIContent("P", "Paste Step Parameters"), EditorStyles.miniButton, GUILayout.Width(20)))
                            {
                                currSeqStepVariant = new SeqItem(copySeqStepVariant);
                                seqList[i] = currSeqStepVariant;
                                ed.seqEd.SyncSequencerControlsDisplayFromSeqItem(this.layer, currSeqStepVariant, i);
                                SetPrefabButtonSwitchesState(seqList[i].sourceVariantIndex, this.layer);
                                af.ResetPoolForLayer(this.layer);
                                af.ForceRebuildFromClickPoints();
                            }
                            //=================================
                            //        Reset Step
                            //=================================
                            SerializedProperty posProp = stepProp.FindPropertyRelative("pos");
                            SerializedProperty thisSize1 = stepProp.FindPropertyRelative("size");
                            SerializedProperty thisRotate1 = stepProp.FindPropertyRelative("rot");
                            if (GUILayout.Button(new GUIContent("R", "Reset Step, will assign the Main prefab, Enable the step, and set all other parameters to default"), EditorStyles.miniButton, GUILayout.Width(20)))
                            {
                                seqList[i].stepEnabled = true;
                                seqList[i].sourceVariantIndex = 0;

                                posProp.vector3Value = Vector3.zero;
                                //thisSize1.vector3Value = Vector3.one;
                                //thisRotate1.vector3Value = Vector3.zero;

                                //af.ResetPoolForLayer(sourceLayerList);
                                //af.ForceRebuildFromClickPoints();
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.Space(3);
                            //===================================
                            //      Sequence Position Offset
                            //===================================
                            int transformsWidth = 133;
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(indentSpace);
                            EditorGUILayout.LabelField(new GUIContent("Pos:", "Offset the position of this variation step. Default=0."), GUILayout.Width(30));
                            EditorGUILayout.PropertyField(posProp, new GUIContent(""), GUILayout.Width(transformsWidth));
                            //af.seqPostPos[p] = thisOffset.vector3Value;
                            if (GUILayout.Button(new GUIContent("X", "Set Position Offset values to default 0"), GUILayout.Width(10)))
                            {
                                posProp.vector3Value = Vector3.zero;
                            }
                            //===================================
                            //      Sequence Size
                            //===================================
                            EditorGUILayout.LabelField(new GUIContent("    Size:", "Multiply Size of this variation step. Default=1|1|1."), GUILayout.Width(42));
                            EditorGUILayout.PropertyField(thisSize1, new GUIContent(""), GUILayout.Width(transformsWidth));
                            if (GUILayout.Button(new GUIContent("X", "Set Size values to default 1"), GUILayout.Width(10)))
                            {
                                //thisSize1.vector3Value = Vector3.one;
                            }
                            //===================================
                            //      Sequence Rotation
                            //===================================
                            EditorGUILayout.LabelField(new GUIContent("     Rot:", "Add Rotation to this variation step. Default=0."), GUILayout.Width(40));
                            EditorGUILayout.PropertyField(thisRotate1, new GUIContent(""), GUILayout.Width(transformsWidth));
                            if (GUILayout.Button(new GUIContent("X", "Set Rotation values to default 0"), GUILayout.Width(10)))
                            {
                                //thisRotate1.vector3Value = Vector3.zero;
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(1);
                        }// end disabled for StepEnabled = false
                    }// end disabled for numSectionsBuiltForLayer
                    GUILayout.EndVertical();
                    ed.DrawUILine(ed.UILineLightGrey, 6, 10, 1, 5);
                }
                EditorGUILayout.EndScrollView();

                if (EditorGUI.EndChangeCheck())
                {
                    /*if (goWasChanged == true)
                    {
                        List<SeqItem> seq = af.GetSequenceForLayer(sourceLayerList);
                        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(sourceLayerList);
                    }*/

                    ed.serializedObject.ApplyModifiedProperties();

                    //-- check the reals are upo to date with the Properties
                    if (this.seqList == null)
                    {
                        Debug.LogError($"seqList is null for sourceLayerList {layer} \n");
                    }
                    for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
                    {
                        /*this.seqList[i].size = seqSize.GetArrayElementAtIndex(i).vector3Value;
                        this.seqList[i].pos = seqOffset.GetArrayElementAtIndex(i).vector3Value;
                        this.seqList[i].rot = seqRot.GetArrayElementAtIndex(i).vector3Value;*/
                    }

                    //We have to manually correct this as the properties update will have put the invalid step variant back in
                    /*for (int i = 0; i < numStepsToShow; i++)
                    {
                        if (i > numSourceVariantsInUseProp)
                            seqList[i].sourceVariantIndex = 0;
                    }*/

                    allDisabled[(int)this.layer] = AreAllSeqItemsDisabledForLayer(af.GetSequencerForLayer(this.layer), af.GetNumSectionsBuiltForLayer(this.layer));
                    needsRebuild = true;

                    List<SeqItem> seqList = GetSequencerListForLayer(layer);
                    for (int i = 0; i < af.GetSequenceForLayer(this.layer).Count; i++)
                    {
                        SeqItem seqVar = seqList[i];

                        if (this.layer == kPostLayer)
                        {
                            //af.seqPostSize[i] = ed.EnforceVectorMinMax(af.seqPostSize[i], -9.99f, 9.99f);
                            seqVar.size = ed.EnforceVectorMinMax(seqVar.size, -9.99f, 9.99f);
                        }
                        else if (this.layer == kRailALayer)
                            af.seqRailASize[i] = ed.EnforceVectorMinMax(af.seqRailASize[i], -9.99f, 9.99f);
                        else if (this.layer == kRailBLayer)
                            af.seqRailBSize[i] = ed.EnforceVectorMinMax(af.seqRailBSize[i], -9.99f, 9.99f);
                    }
                    af.ResetPoolForLayer(this.layer);
                    af.ForceRebuildFromClickPoints(this.layer);
                }

                //-- Ensure seqList[i].sourceVariantIndex and seqList[i].go are updated. Can be neccesary after a recompile etc.
                /*for (int i = 0; i < af.GetSequencerForLayer(sourceLayerList).Length(); i++)
                {
                    if (sourceLayerList == kPostLayer)
                    {
                        seqList[i].sourceVariantIndex = af.seqPostSourceVarIndex[i];
                        seqList[i].go = sourceVariants[seqList[i].sourceVariantIndex].go;
                    }
                    else if (sourceLayerList == kRailALayer)
                    {
                        seqList[i].sourceVariantIndex = af.seqRailASourceVarIndex[i];
                        seqList[i].go = sourceVariants[seqList[i].sourceVariantIndex].go;
                    }
                    else if (sourceLayerList == kRailBLayer)
                    {
                        seqList[i].sourceVariantIndex = af.seqRailBSourceVarIndex[i];
                        seqList[i].go = sourceVariants[seqList[i].sourceVariantIndex].go;
                    }
                }*/

                //===================================
                //      Enable/Disable STEP
                //===================================
                GUILayout.Space(2);
                GUILayout.BeginVertical("box");
                GUILayout.Space(6);

                EditorGUI.BeginChangeCheck();

                //===============================================================================================================
                //
                //                                         AllGlobal Buttons
                //
                //===============================================================================================================
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                // Copy Step
                /*if (GUILayout.Button(new GUIContent("Copy Step", "This will copy the current seq step for pasting in to another step. "),
                EditorStyles.miniButton, GUILayout.Width(75)))
                {
                    ed.copySeqStepVariant = currSeqStepVariant;
                }
                EditorGUI.BeginDisabledGroup(ed.currSeqPostStepVariant == null);
                // Paste Step
                if (GUILayout.Button(new GUIContent("Paste Step", "This will paste the copied step in to the current step. "),
                EditorStyles.miniButton, GUILayout.Width(75)))
                {
                    currSeqStepVariant = new SeqItem(ed.copySeqStepVariant);
                    seqList[currSelectedStepIndex] = currSeqStepVariant;
                    ed.varEd.SyncSequencerControlsDisplayFromSeqItem(sourceLayerList, currSeqStepVariant, currSelectedStepIndex);
                    SetPrefabButtonSwitchesState(seqList[currSelectedStepIndex].sourceVariantIndex);
                    af.ResetPoolForLayer(sourceLayerList);
                    af.ForceRebuildFromClickPoints();
                }
                EditorGUI.EndDisabledGroup();*/

                // Reset Step
                /*if (GUILayout.Button(new GUIContent("Reset Step", "This will replace the step with the main Game Object and settings. "),
                EditorStyles.miniButton, GUILayout.Width(75)))
                {
                    currSeqStepVariant.InitWithBaseVariant(sourceVariants);
                    ed.varEd.SyncSequencerControlsDisplayFromSeqItem(sourceLayerList, currSeqStepVariant, currSelectedStepIndex);
                    SetPrefabButtonSwitchesState(0);
                    af.ResetPoolForLayer(sourceLayerList);
                    af.ForceRebuildFromClickPoints();
                }*/
                //=======================
                //  Reset All Steps
                //=======================
                if (GUILayout.Button(new GUIContent("Reset All Steps", "This will replace the all steps with the main Game Object and settings."),
                EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    for (int s = 0; s < af.GetSequenceForLayer(this.layer).Count; s++)
                    {
                        seqList[s].InitWithBaseVariant(sourceVariants);
                        ed.seqEd.SyncSequencerControlsDisplayFromSeqItem(this.layer, seqList[s], s);
                    }
                    //PrintSourceVariantIndicesForAllStepsForLayer(sourceLayerList);
                    SetPrefabButtonSwitchesState(0, layer);
                    //af.ResetPoolForLayer(sourceLayerList);
                    //af.ForceRebuildFromClickPoints();
                }
                //=======================
                //  Assign All Different
                //=======================
                if (GUILayout.Button(new GUIContent("All Different", "This will assign a different prefab for each Step (in order, drawn from the Main and the " +
                    numSourceVariantsInUseProp.intValue + "sourceVariants. \nWill not affect position/scale/svRotation or orientation settings."), EditorStyles.miniButton, GUILayout.Width(82)))
                {
                    af.ReSeed();
                    // Assigns to seqList(  e.g.  af.railASequencer.seqList )
                    SeqItem.AssignAllDifferentObjectIndicesInSequence(af, this.layer, seqList);

                    //af.ResetPoolForLayer(sourceLayerList);
                    //af.ForceRebuildFromClickPoints(sourceLayerList);
                }
                //=======================
                //  Shuffle
                //=======================
                if (GUILayout.Button(new GUIContent("Shuffle All Variants", "This will replace jsut the prefabs on all steps with one of the " + numSourceVariantsInUseProp.intValue +
                    " sourceVariants you have assigned. \nIt will keep the position/Size settings. Press 'Reset All' first if you want these reset before randomizing"),
                    EditorStyles.miniButton, GUILayout.Width(165)))
                {
                    bool randStepParameterAlso = false;
                    Event currentEvent = Event.current;
                    if (currentEvent.control)
                        randStepParameterAlso = true;
                    af.ReSeed();
                    SeqItem.ShuffleObjectIndicesInSequence(seqList, af.GetSequenceForLayer(this.layer).Count, sourceVariants, randStepParameterAlso, this.layer);
                    af.ResetPoolForLayer(this.layer);
                    af.ForceRebuildFromClickPoints();
                }
                GUILayout.EndHorizontal();
                //GUILayout.EndVertical();

                //==============================
                //      Optimise Randomise
                //==============================
                //EditorGUILayout.Space();
                //DrawUILine(ed.lineColor, 2, 3);
                GUILayout.Space(12);

                using (new EditorGUI.IndentLevelScope())
                {
                    GUILayout.BeginHorizontal();
                    showOptimiseRandomise = EditorGUILayout.Foldout(showOptimiseRandomise,
                        new GUIContent("Show Optimise/Randomise Options ", "Options to Optimise or Randomise the sequence"));
                    GUILayout.EndHorizontal();

                    if (showOptimiseRandomise)
                    {
                        if (this.layer == kRailALayer)
                        {
                            af.optimiseRandomiseToolbarValueA = GUILayout.Toolbar(af.optimiseRandomiseToolbarValueA,
                                optimiseRandomiseToolbarStrings, ed.smallButtonStyle7);
                            /*if (af.optimiseRandomiseToolbarValueA == 0)
                                SetOptimise(sourceLayerList, currSelectedStepIndex);
                            else if (af.optimiseRandomiseToolbarValueA == 1)
                                SetRandomise(sourceLayerList, currSelectedStepIndex);*/
                        }
                        if (this.layer == kRailBLayer)
                        {
                            af.optimiseRandomiseToolbarValueB = GUILayout.Toolbar(af.optimiseRandomiseToolbarValueB,
                                optimiseRandomiseToolbarStrings, ed.smallButtonStyle7);
                            /*if (af.optimiseRandomiseToolbarValueB == 0)
                                SetOptimise(sourceLayerList, currSelectedStepIndex);
                            else if (af.optimiseRandomiseToolbarValueB == 1)
                                SetRandomise(sourceLayerList, currSelectedStepIndex);*/
                        }
                    }
                    //DrawUILine(ed.lineColor, 2, 3);
                }
                EditorGUILayout.Space(4);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    //update menus adter changing the step prefab assignments programatically with the buttonss
                    //List<int> seq_ShowPrebChoice_MenuIndices = af.GetSourceVariantMenuIndicesForLayer(sourceLayerList);
                    //for (int i = 0; i < numSourceVariantsInUse; i++)
                    //{
                    //    seq_ShowPrebChoice_MenuIndices[i] = seqList[i].sourceVariantIndex;
                    //}
                    PrintSourceVariantIndicesForAllStepsForLayer(layer);
                    UpdateSequenceAfterSourceVariantsChanged(layer);

                    allDisabled[(int)this.layer] = AreAllSeqItemsDisabledForLayer(af.GetSequencerForLayer(this.layer), af.GetNumSectionsBuiltForLayer(this.layer));
                    af.ResetPoolForLayer(this.layer);
                    af.ForceRebuildFromClickPoints(this.layer);
                }
                GUILayout.EndVertical();
            }
        }
        //EditorGUI.indentLevel--;
    }

    private void OnMouseEnterButton()
    {
        // Code to execute when mouse enters the button area
        Debug.Log("Mouse entered button area");
    }

    private void OnMouseExitButton()
    {
        // Code to execute when mouse exits the button area
        Debug.Log("Mouse exited button area");
    }

    public void PrintSourceVariantIndicesForAllStepsForLayer(LayerSet layer)
    {
        //ed.StackLog(this.GetType().Name);
        List<SeqItem> seq = af.GetSequenceForLayer(layer);
        int numSteps = af.GetSequencerForLayer(layer).Length();
        for (int i = 0; i < numSteps; i++)
        {
            Debug.Log($"Step {i} : {seq[i].sourceVariantIndex} \n");
        }
    }

    //---------------------------------
    public void UpdateSequenceAfterSourceVariantsChanged(LayerSet layer)
    {
        //ed.StackLog(this.GetType().Name);
        int numSteps = af.GetSequencerForLayer(layer).Length();
        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
        int numSourceVariants = sourceVariants.Count;
        List<SeqItem> seq = af.GetSequenceForLayer(layer);

        //print  go for sourceVariants 1
        Debug.Log($"SourceVariant 1 : {sourceVariants[1].Go.name} \n");

        //-- First Update each step - using its sourceVariantIndex, to reflect the new sourceVariants
        for (int i = 0; i < numSteps; i++)
        {
            //print the sourceVariantIndex for each step
            //Debug.Log($"Step {i} : {seq[i].sourceVariantIndex} \n");

            //int sourceVariantIndexForStep = seqList[i].sourceVariantIndex;
            //SourceVariant sourceVar = af.GetSourceVariantAtIndexForLayer(sourceLayerList, sourceVariantIndexForStep);
        }

        //-- Now update the popup menu that is avaialable for each seq step with real prefab names
        //-- We first need to get the list of source sourceVariants, and the real prefabs they point to

        //-- 1. Get the real prefab indices:
        List<int> sourceVariantPrefabIndices = af.GetSourceVariantMenuIndicesListForLayer(layer);
        //-- 2. Give these indices to the seq sourceVariant popups so they can display the correct prefab names by looking them up in the main prebab lists
        List<int> seq_ShowPrebChoice_MenuIndices = af.GetSourceVariantMenuIndicesForLayer(layer);

        int numSourceVariantsInUse = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true);

        for (int i = 1; i < numSourceVariants; i++)
        {
            seq_ShowPrebChoice_MenuIndices[i] = sourceVariantPrefabIndices[i];
        }

        //-- These are the kMaxNumSourceVariants menu indices of the SourceVariants
        /*List<int> sourceVariant_MenuIndices = af.GetSourceVariantMenuIndicesForLayer(sourceLayerList);

    // the Indices in to the MAIN PREFAB LIST for each(9) SourveVariants sourceLayerList type
    List<int> sourceVariantPrefabIndices = af.CreateSourceVariantPrefabIndicesListForLayer(sourceLayerList);

    for (int i = 1; i < numSourceVariantsInUse + 1; i++)
    {
        //Copy from Property to Real List
        //These are the main Prefab Menu Indices
        sourceVariant_MenuIndices[i] = svMenuListProp.GetArrayElementAtIndex(i).intValue;
        //Debug.Log($"{i} : {sourceVariant_MenuIndices[i]} \n");

        //Convert them to indices into the actual Prefab List
        int prefabIndex = sourceVariantPrefabIndices[i] = af.ConvertMenuIndexToPrefabIndexForLayer(sourceVariant_MenuIndices[i], prefabType);

        //Update the sourceVariants list with the new prefabs
        //SourceVariant sourceVariant = sourceVariants[i];
        //sourceVariant.Go = af.GetPrefabAtIndexForLayer(sourceLayerList, prefabIndex);
    }*/

        /*SerializedProperty seqProp = edUtils.GetSequencerListForLayerProp(sourceLayerList);

        for (int i = 0; i < seqNumSteps; i++)
        {
            SerializedProperty seqStepProp = edUtils.GetSeqItemAtStepIndexProp(seqProp, sourceLayerList, i);
        }

            // Update the Seq Menu indices so they display the correct prefabs
        for (int i = 0; i < seqNumSteps; i++)
        {
            SeqItem currSeqStepVariant = af.GetSequenceForLayer(sourceLayerList)[i];

            int sourceVariantIndexForStep = seqList[i].sourceVariantIndex;
            SourceVariant sourceVar = af.GetSourceVariantAtIndexForLayer(sourceLayerList, sourceVariantIndexForStep);

            List<SeqItem> seq = af.GetSequenceForLayer(sourceLayerList);
        }

        List<int> sourceVariant_MenuIndices = af.GetSourceVariantMenuIndicesForLayer(sourceLayerList);
        SerializedProperty svMenuListProp = ed.edUtils.GetSourceVariantMenuListForLayer(sourceLayerList);

        for (int i = 1; i < numSourceVariantsInUseProp + 1; i++)
        {
            //Copy from Property to Real List
            //These are the main Prefab Menu Indices
            sourceVariant_MenuIndices[i] = svMenuListProp.GetArrayElementAtIndex(i).intValue;
            //Debug.Log($"{i} : {sourceVariant_MenuIndices[i]} \n");

            //Convert them to indices into the actual Prefab List
            int prefabIndex = sourceVariantPrefabIndices[i] = af.ConvertMenuIndexToPrefabIndexForLayer(sourceVariant_MenuIndices[i], prefabType);

            //Update the sourceVariants list with the new prefabs
            SourceVariant sourceVariant = sourceVariants[i];
            sourceVariant.Go = af.GetPrefabAtIndexForLayer(sourceLayerList, prefabIndex);
        }*/

        /*//===============================================================================
        //      Update Sequence and Singles to reflect the new Variants
        //===============================================================================
        //--At this point, we've only changed the menu properties, so we need to update the real af menu lists and set the sourceVariants

        //-- This should update the REAL af menu lists (railASourceVariant_MenuIndices etc.) from the sourceVariant_MenuIndex_List properties
        ed.serializedObject.ApplyModifiedProperties();

        //=== Now convert to Prefab indices, check them,  and set the sourceVariant choices
        List<int> sourceVariant_PrefabIndices = af.CreateSourceVariantPrefabIndicesListForLayer(sourceLayerList);
        for (int i = 1; i < numSourceVariantsInUseProp + 1; i++)
        {
            int prefabIndex = sourceVariant_PrefabIndices[i];
            if (prefabIndex >= mainPrefabs.Count)
                Debug.Log(prefabIndex + mainPrefabs.Count + "\n");

            GameObject go = mainPrefabs[prefabIndex];

            sourceVariants[i].Go = go;

            for (int j = 0; j < af.GetSequencerForLayer(sourceLayerList).Length(); j++)
            {
                {
                    af.seqPostSourceVarIndex[j] = seqList[i].sourceVariantIndex;
                    af.seqPostSize[j] = ed.EnforceVectorMinMax(af.seqPostSize[i], -9.99f, 9.99f);

                currSeqStepVariant = seqList[i];
                //ed.varEd.SetSequenceVariantFromDisplaySettings(this.sourceLayerList, ref currSeqStepVariant, i);
            }
        }*/
    }

    //-- Remember that although these are 0-based internally, both section and seq steps are 1-based in the UI
    //-- Although this calculation should be trivial it's a bit of a brainfeck when juggling munSteps, numSources, numSections etc.
    private List<int> GetSectionsUsingSeqStep(int stepNum, LayerSet layer)
    {
        int numSections = af.GetNumSectionsBuiltForLayer(layer);
        int numSteps = af.GetNumSeqStepsForLayer(layer);
        List<int> sectionIndices = new List<int>();
        int maxNumLoops = (numSections / numSteps) + 1;

        //sectionIndices = 0, numSteps * 1, numSteps * 2, numSteps * 3 etc until numSteps * n > numSections, then add an offset of stepNum to each
        for (int i = 0; i < maxNumLoops; i++)
        {
            int sectionIndex = (i * numSteps) + stepNum;
            if (sectionIndex < numSections)
                sectionIndices.Add(sectionIndex);
            //sectionIndices.Add((i * numSteps) + stepNum);
        }
        return sectionIndices;
    }

    //------------------------
    private List<SeqItem> GetSequencerListForLayer(LayerSet layer)
    {
        return af.GetSequenceForLayer(layer);
    }

    //--------------
    // Build a List of not only the sourceVariants in use, but also the ones beyond that referenced by steps
    // and temporarily replace thos with Main Variant
    public GUIContent[] GetSeqPopupStrings(LayerSet layer)
    {
        string[] sourceVariantNames = GetSourceVariantNamesForLayer(layer);
        int numVariantsInUse = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true);

        List<SeqItem> seqItems = af.GetSequenceForLayer(layer);

        //First make a list of indices referenced by each of kMaxNumSteps, then add relacements for all  steps that reference sourceVariants beyond numVariantsInUse

        //-- First 0 to numVariantsInUse
        List<int> indicesNeeded = Enumerable.Range(0, numVariantsInUse).ToList();

        //Now check all the steps for others outside this list and add those too
        
        for (int stepNum = 0; stepNum < seqItems.Count; stepNum++)
        {
            int variantIndexForStep = seqItems[stepNum].sourceVariantIndex;
            //if we need it and it's not already been added to the list
            if (variantIndexForStep >= numVariantsInUse && indicesNeeded.Contains(variantIndexForStep) == false)
                indicesNeeded.Add(variantIndexForStep);
        }

        // Now finally create the array of popup contents that we need for each index, and replace the ones not avaialable
        int numPopupsNeeded = indicesNeeded.Count;
        GUIContent[] seqPopupStrings = new GUIContent[numPopupsNeeded];
        for (int i = 0; i < numPopupsNeeded; i++)
        {
            int index = indicesNeeded[i];
            seqPopupStrings[i] = new GUIContent(sourceVariantNames[index], "Choose one of the prefabs defined in Assign Sources above");

            if (index > numSourceVariantsInUse)
            {
                string replacementPopupString = GetVariantNameAtIndexForLayer(0, layer);
                string originalPopupString = GetVariantNameAtIndexForLayer(index, layer);
                string msgStr = $"Using  {replacementPopupString}   as variant   '{originalPopupString}'   was removed From the Sources list above";
                seqPopupStrings[i] = new GUIContent(msgStr, $"Variant '{originalPopupString}' Removed. \nUsing {replacementPopupString} instead");
                //GUIStyle popStyle = ed.orangePopupStyle;
                //**     If there was a replacemnet, we assign varaint[0] in the af.CreateRailPool() code   **
            }
        }

        return seqPopupStrings;
    }

    //-----------
    // If the indices have been changed, make sure the GO is chosen
    public void SetSeqGameObjects(LayerSet layer)
    {
        SetSeqGoFromSeqGoIndex(layer);
        af.ClearConsole();
        af.PrintSeqGOs(layer);
        PrintSeqGoMenus(layer);

        // PrintSeqGoMenus(sourceLayerList);
    }

    //------------------
    // If the indices of been changed, make sure the GO is chosen
    public void SetSeqGoFromSeqGoIndex(LayerSet layer)
    {
        List<SeqItem> seq = af.GetSequenceForLayer(layer);
        List<SourceVariant> variants = af.GetSourceVariantsForLayer(layer);
        for (int s = 0; s < AutoFenceCreator.kMaxNumSeqSteps; s++)
        {
            if (seq[s].sourceVariantIndex >= 0)
            {
                int goIndex = seq[s].sourceVariantIndex;
                //seq[s].go = sourceVariants[goIndex].go;
            }
        }
    }

    //------------------
    // switch the prefab choice buttins onIndex on, the rest off
    private void SetPrefabButtonSwitchesState(int onIndex, LayerSet layerSet)
    {
        if (onIndex == -1)
            onIndex = 0;

        if (layerSet == LayerSet.railALayer || layerSet == LayerSet.railBLayer)

            for (int i = 0; i < seqList.Count; i++)
            {
                prefabButtonSwitch[i] = false;
            }
        prefabButtonSwitch[onIndex] = true;
    }

    //---------
    public void PrintSeqGoMenus(LayerSet layer)
    {
        int numSteps = af.GetNumSeqStepsForLayer(layer);
        SerializedProperty seqVarListProp = ed.GetSequencerListForLayerProp(layer); // e.g. af.railASequencer.seqList Property
        for (int i = 0; i < numSteps; i++)
        {
            SerializedProperty goIndexProp = seqVarListProp.GetArrayElementAtIndex(i).FindPropertyRelative("sourceVariantIndex");// e.g.af.railASequencer.seqList[i].sourceVariantIndex Property
            //Debug.Log(goIndexProp.intValue + "\n");
        }
    }

    //---------
    public SerializedProperty GetSeqGoIndexProperty(LayerSet layer, int i)
    {
        SerializedProperty thisSeqVarListProperty = ed.GetSequencerListForLayerProp(layer);
        SerializedProperty goIndexProp = thisSeqVarListProperty.GetArrayElementAtIndex(i).FindPropertyRelative("sourceVariantIndex");
        return goIndexProp;
    }

    //--------------
    // return a list of kMaxNumSourceVariants for the Variant names of this sourceLayerList
    public string[] GetSourceVariantNamesForLayer(LayerSet layer)
    {
        string shortprefabName = "";
        string[] fullPrefabNames = new string[AutoFenceCreator.kMaxNumSourceVariants];

        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);

        for (int i = 0; i < AutoFenceCreator.kMaxNumSourceVariants; i++)
        {
            if (sourceVariants[i].Go == null)
                sourceVariants[i].Go = sourceVariants[0].Go;
            string baseName = af.StripPanelRailFromName(sourceVariants[i].Go.name);
            shortprefabName = i + ": " + baseName;
            fullPrefabNames[i] = "Var " + i + ":  " + baseName;
            if (i == 0)
            {
                shortprefabName = baseName;
                fullPrefabNames[i] = "Main:   " + baseName;
            }
            if (shortprefabName.Length > 12)
                shortprefabName = shortprefabName.Substring(0, 12);
        }

        return fullPrefabNames;
    }

    //--------------
    // return a list of kMaxNumSourceVariants for the Variant names of this sourceLayerList
    public string GetVariantNameAtIndexForLayer(int i, LayerSet layer)
    {
        string shortprefabName = "";
        string fullName = "";

        List<SourceVariant> variants = af.GetSourceVariantsForLayer(layer);

        if (variants[i].Go == null)
            variants[i].Go = variants[0].Go;
        string baseName = af.StripPanelRailFromName(variants[i].Go.name);
        shortprefabName = i + ": " + baseName;
        fullName = "Var " + i + ":  " + baseName;
        if (i == 0)
        {
            shortprefabName = baseName;
            fullName = "Main:   " + baseName;
        }
        if (shortprefabName.Length > 12)
            shortprefabName = shortprefabName.Substring(0, 12);

        return fullName;
    }

    private void SetAllLayersDisabledStatus(bool allDisabled, LayerSet layerSet)
    {
        if (allDisabled == true)
        {
            if (layerSet == LayerSet.postLayer)
                allPostsDisabled = true;
            if (layerSet == LayerSet.railALayer)
                allRailADisabled = true;
            if (layerSet == LayerSet.railBLayer)
                allRailBDisabled = true;
            if (layerSet == LayerSet.extraLayer)
                allExtrasDisabled = true;
        }
        else
        {
            if (layerSet == LayerSet.postLayer)
                allPostsDisabled = false;
            if (layerSet == LayerSet.railALayer)
                allRailADisabled = false;
            if (layerSet == LayerSet.railBLayer)
                allRailBDisabled = false;
            if (layerSet == LayerSet.extraLayer)
                allExtrasDisabled = false;
        }
    }

    public bool AreAllLayerSequencesDisabled(LayerSet layerSet)
    {
        if (layerSet == LayerSet.postLayer && allPostsDisabled == true)
            return true;
        if (layerSet == LayerSet.railALayer && allRailADisabled == true)
            return true;
        if (layerSet == LayerSet.railBLayer && allRailBDisabled == true)
            return true;
        if (layerSet == LayerSet.extraLayer && allExtrasDisabled == true)
            return true;
        return false;
    }

    //-----------------------------
    private bool AllSeqItemsEnabled(List<SeqItem> seqVarList, int numSteps)
    {
        for (int i = 0; i < numSteps; i++)
        {
            if (seqVarList[i].stepEnabled == false)
            {
                return false;
            }
        }
        return true;
    }

    public static bool AreAllSeqItemsDisabledForLayer(Sequencer sequencer, int numSectionsBuilt)
    {

        List<SeqItem> seqItemList = sequencer.seqList;
        int numSeqSteps = seqItemList.Count;
        int numStepsInUse = numSectionsBuilt;

        if(numStepsInUse > numSeqSteps)
            numStepsInUse = numSeqSteps;

        //for (int i = 0; i < seqNumSteps; i++) // Changed 2/4/23
        for (int i = 0; i < numStepsInUse; i++)
        {
            if (seqItemList[i].stepEnabled == true)
            {
                return false;
            }
        }
        return true;
    }

    //-------------------
    public void SetRandomise(LayerSet layerSet, int currStepIndex)
    {
        //EditorGUILayout.Space();
        GUIStyle redWarningStyle = new GUIStyle(GUI.skin.button);
        redWarningStyle.normal.textColor = new Color(0.75f, .0f, .0f); //dark red
        GUIStyle buttonStyle = ed.defaultButtonStyle;
        if (optimalToUserSeqString == "Sure? This will replace all steps")
            buttonStyle = redWarningStyle;

        GUILayout.BeginVertical();

        if (GUILayout.Button(new GUIContent("Randomize", "Randomize the order of the variation prefabs, and their orientation flips"), EditorStyles.miniButton, GUILayout.Width(100)))
        {
            af.SeedRandom(false);
            af.shuffledRailAIndices = SeqItem.CreateShuffledIndices(sourceVariants, af.allPostPositions.Count - 1);

            //Debug.Log(af.shuffledRailAIndices.Length);
            //Debug.Log(seqInfo.seqNumSteps);
            for (int i = 0; i < af.GetSequenceForLayer(layerSet).Count; i++)
            {
                int a = i % af.shuffledRailAIndices.Length;
                //Debug.Log(af.shuffledRailAIndices[a]);
                seqList[i].sourceVariantIndex = af.shuffledRailAIndices[a];
                //seqList[i].go = sourceVariants[seqList[i].sourceVariantIndex].go;

                if (af.allowBackToFrontRailA)
                {
                    seqList[i].backToFront = System.Convert.ToBoolean(UnityEngine.Random.Range(0, 2));
                }
                if (af.allowMirrorZRailA)
                {
                    seqList[i].mirrorZ = System.Convert.ToBoolean(UnityEngine.Random.Range(0, 2));
                }
                if (af.allowInvertRailA)
                {
                    seqList[i].invert = System.Convert.ToBoolean(UnityEngine.Random.Range(0, 2));
                }
            }
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }

        //====  Main Rail Probability ( can not be 0 )  ====
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("Probabilities:",
            "The amount of times this variation will appear, relative to the others. If all set to the same value, they will appear equally."));

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("[Main]  " + sourceVariants[0].Go.name + "    ",
             "The amount of times this variation will appear, relative to the others. If all set to the same value, they will appear equally."),
                 GUILayout.Width(260));

        EditorGUI.BeginChangeCheck();
        var mainProb = ed.railAProbArray.GetArrayElementAtIndex(0);
        EditorGUILayout.PropertyField(mainProb, new GUIContent(""), GUILayout.Width(150));
        if (EditorGUI.EndChangeCheck())
        {
            sourceVariants[0].probability = mainProb.floatValue;

            ed.serializedObject.ApplyModifiedProperties();
            if (af.varRailAProbs[0] < 0.01f)
                af.varRailAProbs[0] = 0.01f;
            af.railSourceVariants[0][0].probability = mainProb.floatValue;
            //af.shuffledRailAIndices = SourceVariations.CreateShuffledIndices(af.nonNullRailSourceVariants[0], af.allPostPositions.Count - 1);
            //changedGameObjects = true;
        }
        EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(40));

        GUILayout.EndHorizontal();

        //====  Probability  ====

        for (int i = 1; i < 5; i++)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("[" + i + "]       " + sourceVariants[i].Go.name + "    ",
                    "The amount of times this variation will appear, relative to the others. If all set to the same value, they will appear equally."),
                GUILayout.Width(260));
            EditorGUI.BeginChangeCheck();
            var varProb = ed.railAProbArray.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(varProb, new GUIContent(""), GUILayout.Width(150));
            if (EditorGUI.EndChangeCheck())
            {
                ed.serializedObject.ApplyModifiedProperties();
                sourceVariants[i].probability = varProb.floatValue;
                //af.shuffledRailAIndices = SourceVariations.CreateShuffledIndices(af.nonNullRailSourceVariants[0], af.allPostPositions.Count - 1);
            }
            GUILayout.EndHorizontal();
        }

        //EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Allow Randomise to use:    ", ed.label11Style, GUILayout.Width(150));
        if (layerSet == kRailALayer)
        {
            EditorGUILayout.LabelField("Back to Front", ed.smallLabel, GUILayout.Width(80));
            af.allowBackToFrontRailA = EditorGUILayout.Toggle(af.allowBackToFrontRailA, GUILayout.Width(35));

            EditorGUILayout.LabelField("Mirror Z", ed.smallLabel, GUILayout.Width(60));
            af.allowMirrorZRailA = EditorGUILayout.Toggle(af.allowMirrorZRailA, GUILayout.Width(35));

            EditorGUILayout.LabelField("Invert Vertical", ed.smallLabel, GUILayout.Width(90));
            af.allowInvertRailA = EditorGUILayout.Toggle(af.allowInvertRailA, GUILayout.Width(35));
        }
        else if (layerSet == kRailBLayer)
        {
            EditorGUILayout.LabelField("Back to Front", ed.smallLabel, GUILayout.Width(80));
            af.allowBackToFrontRailB = EditorGUILayout.Toggle(af.allowBackToFrontRailB, GUILayout.Width(35));

            EditorGUILayout.LabelField("Mirror Z", ed.smallLabel, GUILayout.Width(60));
            af.allowMirrorZRailB = EditorGUILayout.Toggle(af.allowMirrorZRailB, GUILayout.Width(35));

            EditorGUILayout.LabelField("Invert Vertical", ed.smallLabel, GUILayout.Width(90));
            af.allowInvertRailB = EditorGUILayout.Toggle(af.allowInvertRailB, GUILayout.Width(35));
        }

        if (EditorGUI.EndChangeCheck())
        {
            for (int s = 0; s < af.GetSequenceForLayer(layerSet).Count; s++)
            {
                if (layerSet == kRailALayer)
                {
                    if (seqList[s].backToFront == true && af.allowBackToFrontRailA == false)
                        seqList[s].backToFront = false;
                    if (seqList[s].mirrorZ == true && af.allowMirrorZRailA == false)
                        seqList[s].mirrorZ = false;
                    if (seqList[s].invert == true && af.allowInvertRailA == false)
                        seqList[s].invert = false;
                }
                if (layerSet == kRailBLayer)
                {
                    if (seqList[s].backToFront == true && af.allowBackToFrontRailB == false)
                        seqList[s].backToFront = false;
                    if (seqList[s].mirrorZ == true && af.allowMirrorZRailB == false)
                        seqList[s].mirrorZ = false;
                    if (seqList[s].invert == true && af.allowInvertRailB == false)
                        seqList[s].invert = false;
                }
            }
            af.ForceRebuildFromClickPoints(layerSet);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        //EditorGUILayout.Space();
    }

    //----------------------
    /*public bool GetUseSequencerForLayer(LayerSet sourceLayerList)
    {
        if (sourceLayerList == LayerSet.railALayer || sourceLayerList == LayerSet.railBLayer)
            return af.useRailSequencer[(int)sourceLayerList];
        else if (sourceLayerList == LayerSet.postLayer)
            return af.usePostSequencer;
        return false;
    }*/

    //-------------------
    // Put the variantList indices in to the seq list
    private void GetMasterVariantListIndices(List<SourceVariant> mainVariants, List<SeqItem> seqList)
    {
        int index = -1;
        for (int i = 0; i < seqList.Count; i++)
        {
            SeqItem seqStepVariant = seqList[i];
            //GameObject go = seqStepVariant.GetSourceVariantGO(af, sourceLayerList);

            index = af.FindFirstInVariants(mainVariants, seqStepVariant.GetSourceVariantGO(af, layer), layer);
            seqList[i].sourceVariantIndex = index;
        }
    }

    //-------------------
    public void SetOptimise(LayerSet layerSet, int currStepIndex)
    {
        //EditorGUILayout.Space();

        GUIStyle redWarningStyle = new GUIStyle(GUI.skin.button);
        redWarningStyle.normal.textColor = new Color(0.75f, .0f, .0f); //dark red
        GUIStyle buttonStyle = ed.defaultButtonStyle;
        if (optimalToUserSeqString == "Sure? This will replace all steps")
            buttonStyle = redWarningStyle;

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(optimalToUserSeqString, "Using this, you can quickly fill all steps and then modify specific steps.\n\n" +
                "It will use the best combination of the different (if any) source prefabs you have assigned, along with the orientation changes you allow.\n\n" +
                "To use only the Main prefab, first choose 'All Main' in the 'Choose Source Prefabs' options."), EditorStyles.miniButton, GUILayout.Width(110)))
        {
            List<SourceVariant> uniqueList = CreateUniqueVariantList(sourceVariants);
            //SourceVariations.PrintSourceVariantList(uniqueList);

            List<SeqItem> optimalSeq = null;
            if (layerSet == kRailALayer)
                optimalSeq = SeqItem.CreateOptimalSequence(af, uniqueList, layer);
            //else if (layerSet == kRailBLayer)
            //optimalSeq = SourceVariations.CreateOptimalSequence(uniqueList, af.allowBackToFrontRailB, af.allowMirrorZRailB, af.allowInvertRailB);

            GetMasterVariantListIndices(sourceVariants, optimalSeq);
            //SourceVariations.PrintSeqItemList(optimalSeq);

            ed.seqEd.CopySequence(optimalSeq, seqList);
            af.GetSequencerForLayer(layerSet).EnforceSeqMinLength(optimalSeq.Count);
            af.GetSequencerForLayer(layerSet).EnforceSeqListBounds();
            //SourceVariations.PrintSeqItemList(seqList);

            ed.seqEd.SyncSequencerControlsDisplayAllSteps(layerSet);

            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }

        GUILayout.EndHorizontal();
        //EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Allow Optimal to use:    ", ed.label11Style, GUILayout.Width(150));
        if (layerSet == kRailALayer)
        {
            EditorGUILayout.LabelField("Back to Front", ed.smallLabel, GUILayout.Width(80));
            af.allowBackToFrontRailA = EditorGUILayout.Toggle(af.allowBackToFrontRailA, GUILayout.Width(35));

            EditorGUILayout.LabelField("Mirror Z", ed.smallLabel, GUILayout.Width(60));
            af.allowMirrorZRailA = EditorGUILayout.Toggle(af.allowMirrorZRailA, GUILayout.Width(35));

            EditorGUILayout.LabelField("Invert Vertical", ed.smallLabel, GUILayout.Width(90));
            af.allowInvertRailA = EditorGUILayout.Toggle(af.allowInvertRailA, GUILayout.Width(35));
        }
        else if (layerSet == kRailBLayer)
        {
            EditorGUILayout.LabelField("Back to Front", ed.smallLabel, GUILayout.Width(80));
            af.allowBackToFrontRailB = EditorGUILayout.Toggle(af.allowBackToFrontRailB, GUILayout.Width(35));

            EditorGUILayout.LabelField("Mirror Z", ed.smallLabel, GUILayout.Width(60));
            af.allowMirrorZRailB = EditorGUILayout.Toggle(af.allowMirrorZRailB, GUILayout.Width(35));

            EditorGUILayout.LabelField("Invert Vertical", ed.smallLabel, GUILayout.Width(90));
            af.allowInvertRailB = EditorGUILayout.Toggle(af.allowInvertRailB, GUILayout.Width(35));
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        //EditorGUILayout.Space();
    }

    //--------------------
    public List<SourceVariant> CreateUniqueVariantList(List<SourceVariant> sourceVariantList)
    {
        List<SourceVariant> uniqueList = new List<SourceVariant>();

        GameObject mainGO = sourceVariantList[0].Go;

        foreach (var source in sourceVariantList)
        {
            bool found = false;
            foreach (var dest in uniqueList)
            {
                if (source.Go == dest.Go)
                {
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                uniqueList.Add(source);
            }
        }
        return uniqueList;
    }

    //-------------------
    public void HandleSeqOrientationSwitches(LayerSet layerSet, int currStepIndex, int indentSpace)
    {
        var backToFrontProperty = seqListProperty.GetArrayElementAtIndex(currStepIndex).FindPropertyRelative("backToFront");
        var mirrorZProperty = seqListProperty.GetArrayElementAtIndex(currStepIndex).FindPropertyRelative("mirrorZ");
        var invertProperty = seqListProperty.GetArrayElementAtIndex(currStepIndex).FindPropertyRelative("invert");

        GUILayout.BeginHorizontal();
        GUILayout.Space(indentSpace);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField(new GUIContent("Back to Front", "The front and back faces of the wall will be swapped (equivalent to a 180 Y axis svRotation)" +
            "\n Try with the Fence Prefab 'TestVariationTest1' to visualize the effect"), ed.label11Style, GUILayout.Width(73));
        EditorGUILayout.PropertyField(backToFrontProperty, new GUIContent(""), GUILayout.Width(26));
        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints(layerSet);
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField(new GUIContent("Mirror Z", "The left-to-right direction is reversed (equivalent to the texture running in the opposite direction)" +
            "\n Try with the Fence Prefab 'TestVariationTest1' to visualize the effect"), ed.label11Style, GUILayout.Width(45));
        EditorGUILayout.PropertyField(mirrorZProperty, new GUIContent(""), GUILayout.Width(26));
        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints(layerSet);
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField(new GUIContent("Invert", "The top and bottom are reversed (equivalent to a 180 Z axis svRotation)" +
            "\n Try with the Fence Prefab 'TestVariationTest1' to visualize the effect"), ed.label11Style, GUILayout.Width(40));
        EditorGUILayout.PropertyField(invertProperty, new GUIContent(""), GUILayout.Width(26));
        if (EditorGUI.EndChangeCheck())
        {
            ed.serializedObject.ApplyModifiedProperties();
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints(layerSet);
        }

        GUILayout.EndHorizontal();
    }

    //------------------------------------
    // Necessary after switching on/off variant objects to ensure the sequence is not referencing an obsolete go
    public void CheckSequencerHasValidObjects(LayerSet layer)
    {
        int numSeqSteps = af.railASequencer.Length(), currSeqStep = ed.currSeqRailStepIndex[0];
        List<SeqItem> userSequence = af.GetSequenceForLayer(layer);

        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);

        int[] seqVarIndex = af.seqRailASourceVarIndex;
        SeqItem currStepSeqItem = af.railASequencer.seqList[currSeqStep];

        if (layer == kRailBLayer)
        {
            userSequence = af.railBSequencer.seqList;
            numSeqSteps = userSequence.Count;
            currSeqStep = ed.currSeqRailStepIndex[1];
            sourceVariants = af.railSourceVariants[1];
            //nonNullCount = af.nonNullRailSourceVariants[1].Count;
            seqVarIndex = af.seqRailBSourceVarIndex;
            currStepSeqItem = af.railBSequencer.seqList[currSeqStep];
        }

        for (int i = 0; i < numSeqSteps; i++)
        {
            SeqItem thisStep = userSequence[i];
            int seqStepObjIndex = thisStep.sourceVariantIndex;

            if (seqStepObjIndex >= sourceVariants.Count || seqVarIndex[i] >= sourceVariants.Count)
            {
                thisStep.sourceVariantIndex = 0;
                userSequence[i] = thisStep;
                seqVarIndex[i] = 0;
            }
        }

        if (layer == kRailALayer)
            af.seqRailASourceVarIndex = seqVarIndex;
        else if (layer == kRailBLayer)
            af.seqRailBSourceVarIndex = seqVarIndex;

        currStepSeqItem.sourceVariantIndex = seqVarIndex[ed.currSeqRailStepIndex[0]];
        //currStepSeqItem.go = sourceVariants[seqVarIndex[currSeqStep]].go;
        userSequence[currSeqStep] = currStepSeqItem;
    }

    //-----------
    public void InitializeUserSequence(LayerSet layer)
    {
        if (layer == kRailALayer)
        {
            //Initialize the first 1-5 with any available RailVariants
            for (int i = 0; i < af.railSourceVariants[0].Count; i++)
            {
                af.railASequencer.seqList[i] = new SeqItem(i, af.railSourceVariants[(int)kRailALayer][i], layer);
            }
            // then initialize the rest with the base
            for (int i = af.railSourceVariants[0].Count; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                af.railASequencer.seqList[i] = new SeqItem(0, af.railSourceVariants[(int)kRailALayer][0], layer);
            }
            SyncSequencerControlsDisplayFromSeqItem(layer, af.railASequencer.seqList[0], 0);
        }

        if (layer == kRailBLayer)
        {
            for (int i = 0; i < af.railSourceVariants[1].Count; i++)
            {
                af.railBSequencer.seqList[i] = new SeqItem(i, af.railSourceVariants[(int)kRailBLayer][i], layer);
            }
            for (int i = af.railSourceVariants[1].Count; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                af.railBSequencer.seqList[i] = new SeqItem(0, af.railSourceVariants[(int)kRailBLayer][0], layer);
            }
            SyncSequencerControlsDisplayFromSeqItem(layer, af.railBSequencer.seqList[0], 0);
        }

        if (layer == kPostLayer)
        {
            for (int i = 0; i < af.postSourceVariants.Count; i++)
            {
                af.postSequencer.seqList[i] = new SeqItem(i, af.postSourceVariants[i], layer);
            }
            for (int i = af.postSourceVariants.Count; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                af.postSequencer.seqList[i] = new SeqItem(0, af.postSourceVariants[0], layer);
            }
            SyncSequencerControlsDisplayFromSeqItem(layer, af.postSequencer.seqList[0], 0);
        }
    }

    //-------------------------
    // If the sequence step is changed programmatically, we need to update the interface contrlols
    public void SyncSequencerControlsDisplayAllSteps(LayerSet layerSet)
    {
        if (layerSet == kRailALayer)
        {
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                SyncSequencerControlsDisplayFromSeqItem(layerSet, af.railASequencer.seqList[i], i);
            }
        }
        if (layerSet == kRailBLayer)
        {
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                SyncSequencerControlsDisplayFromSeqItem(layerSet, af.railBSequencer.seqList[i], i);
            }
        }
        if (layerSet == kPostLayer)
        {
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                SyncSequencerControlsDisplayFromSeqItem(layerSet, af.postSequencer.seqList[i], i);
            }
        }
        /*if (layerSet == kSubpostLayer)
        {
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                SyncSequencerControlsDisplayFromSeqItem(layerSet, af.userSequenceSubpost[i], i);
            }
        }*/
    }

    //-------------------------
    // If the sequence step is changed programmatically, we need to update the interface contrlol
    public void SyncSequencerControlsDisplayFromSeqItem(LayerSet layerSet, SeqItem SeqItem, int stepIndex)
    {
        if (layerSet == kRailALayer)
        {
            af.seqRailAStepEnabled[stepIndex] = SeqItem.stepEnabled;
            /*af.seqAX[stepIndex] = SeqItem.svBackToFront;
            af.seqAZ[stepIndex] = SeqItem.svMirrorZ;
            af.seqAInvert180[stepIndex] = SeqItem.svInvert;*/

            /*af.seqRailASourceVarIndex[stepIndex] = SeqItem.sourceVariantIndex;

            af.seqRailASize[stepIndex] = SeqItem.size;
            af.seqRailAOffset[stepIndex] = SeqItem.pos;
            af.seqRailARotate[stepIndex] = SeqItem.rot;*/
        }
        else if (layerSet == kRailBLayer)
        {
            af.seqRailBStepEnabled[stepIndex] = SeqItem.stepEnabled;
            /*af.seqBX[stepIndex] = SeqItem.svBackToFront;
            af.seqBZ[stepIndex] = SeqItem.svMirrorZ;
            af.seqBInvert180[stepIndex] = SeqItem.svInvert;*/

            /*af.seqRailBSourceVarIndex[stepIndex] = SeqItem.sourceVariantIndex;
            af.seqRailBSize[stepIndex] = SeqItem.size;
            af.seqRailBOffset[stepIndex] = SeqItem.pos;
            af.seqRailBRotate[stepIndex] = SeqItem.rot;*/
        }
        else if (layerSet == kPostLayer)
        {
            af.seqPostStepEnabled[stepIndex] = SeqItem.stepEnabled;
            /*af.seqPostX[stepIndex] = SeqItem.backToFront;
            af.seqPostZ[stepIndex] = SeqItem.mirrorZ;
            af.seqPostInvert180[stepIndex] = SeqItem.invert;

            af.seqPostSourceVarIndex[stepIndex] = SeqItem.sourceVariantIndex;
            af.seqPostSize[stepIndex] = SeqItem.size;
            af.seqPostOffset[stepIndex] = SeqItem.pos;
            af.seqPostRotate[stepIndex] = SeqItem.rot;*/
        }
        else if (layerSet == kSubpostLayer)
        {
            /*af.seqSubpostStepEnabled[stepIndex] = SeqItem.stepEnabled;
            af.seqSubpostX[stepIndex] = SeqItem.backToFront;
            af.seqSubpostZ[stepIndex] = SeqItem.mirrorZ;
            af.seqSubpostInvert180[stepIndex] = SeqItem.invert;

            af.seqSubpostSourceVarIndex[stepIndex] = SeqItem.sourceVariantIndex;
            af.seqSubpostSize[stepIndex] = SeqItem.size;
            af.seqSubpostOffset[stepIndex] = SeqItem.pos;
            af.seqSubpostRotate[stepIndex] = SeqItem.rot;*/
        }
    }

    //-----------
    public SeqItem SetSequenceVariantFromDisplaySettings2(LayerSet layerSet, ref SeqItem seqStepVariant, int currSeqStep)
    {
        if (layerSet == kPostLayer)
        {
            /*seqStepVariant.pos = af.seqPostOffset[currSeqStep];
            seqStepVariant.size = af.seqPostSize[currSeqStep];
            seqStepVariant.rot = af.seqPostRotate[currSeqStep];*/

            seqStepVariant.sourceVariantIndex = af.seqPostSourceVarIndex[currSeqStep];
            af.postSequencer.seqList[currSeqStep] = seqStepVariant;
        }
        else if (layerSet == kSubpostLayer)
        {
            /*seqStepVariant.pos = af.seqSubpostOffset[currSeqStep];
            seqStepVariant.size = af.seqSubpostSize[currSeqStep];
            seqStepVariant.rot = af.seqSubpostRotate[currSeqStep];*/
            seqStepVariant.sourceVariantIndex = af.seqSubpostSourceVarIndex[currSeqStep];
            af.userSequenceSubpost[currSeqStep] = seqStepVariant;
        }
        else if (layerSet == kRailALayer)
        {
            /*seqStepVariant.pos = af.seqRailAOffset[currSeqStep];
            seqStepVariant.size = af.seqRailASize[currSeqStep];
            seqStepVariant.rot = af.seqRailARotate[currSeqStep];*/
            seqStepVariant.sourceVariantIndex = af.seqRailASourceVarIndex[currSeqStep];
            /*seqStepVariant.svBackToFront = af.seqAX[currSeqStep];
            seqStepVariant.svMirrorZ = af.seqAZ[currSeqStep];
            seqStepVariant.svInvert = af.seqAInvert180[currSeqStep];*/

            af.railASequencer.seqList[currSeqStep] = seqStepVariant;
        }
        else if (layerSet == kRailBLayer)
        {
            /*seqStepVariant.pos = af.seqRailBOffset[currSeqStep];
            seqStepVariant.size = af.seqRailBSize[currSeqStep];
            seqStepVariant.rot = af.seqRailBRotate[currSeqStep];*/

            seqStepVariant.sourceVariantIndex = af.seqRailBSourceVarIndex[currSeqStep];
            /*seqStepVariant.svBackToFront = af.seqBX[currSeqStep];
            seqStepVariant.svMirrorZ = af.seqBZ[currSeqStep];
            seqStepVariant.svInvert = af.seqBInvert180[currSeqStep];*/

            af.railBSequencer.seqList[currSeqStep] = seqStepVariant;
        }

        return seqStepVariant;
    }

    //-----------
    public SeqItem SetSequenceVariantFromDisplaySettings(LayerSet layerSet, ref SeqItem seqStepVariant, int currSeqStep)
    {
        if (layerSet == kPostLayer)
        {
            /*seqStepVariant.pos = af.seqPostOffset[currSeqStep];
            seqStepVariant.size = af.seqPostSize[currSeqStep];
            seqStepVariant.rot = af.seqPostRotate[currSeqStep];*/
            //seqStepVariant.go = af.postVariants[seqStepVariant.sourceVariantIndex].go;
            af.postSequencer.seqList[currSeqStep] = seqStepVariant;
        }
        else if (layerSet == kSubpostLayer)
        {
            /*seqStepVariant.pos = af.seqSubpostOffset[currSeqStep];
            seqStepVariant.size = af.seqSubpostSize[currSeqStep];
            seqStepVariant.rot = af.seqSubpostRotate[currSeqStep];*/
            //seqStepVariant.go = af.subpostVariants[seqStepVariant.sourceVariantIndex].go;
            af.userSequenceSubpost[currSeqStep] = seqStepVariant;
        }
        else if (layerSet == kRailALayer)
        {
            /*seqStepVariant.pos = af.seqRailAOffset[currSeqStep];
            seqStepVariant.size = af.seqRailASize[currSeqStep];
            seqStepVariant.rot = af.seqRailARotate[currSeqStep];

            seqStepVariant.sourceVariantIndex = af.seqRailASourceVarIndex[currSeqStep];*/
            //seqStepVariant.go = af.railSourceVariants[0][seqStepVariant.sourceVariantIndex].go;

            /*seqStepVariant.svBackToFront = af.seqAX[currSeqStep];
            seqStepVariant.svMirrorZ = af.seqAZ[currSeqStep];
            seqStepVariant.svInvert = af.seqAInvert180[currSeqStep];*/

            af.railASequencer.seqList[currSeqStep] = seqStepVariant;
        }
        else if (layerSet == kRailBLayer)
        {
            /*seqStepVariant.pos = af.seqRailBOffset[currSeqStep];
            seqStepVariant.size = af.seqRailBSize[currSeqStep];
            seqStepVariant.rot = af.seqRailBRotate[currSeqStep];

            seqStepVariant.sourceVariantIndex = af.seqRailBSourceVarIndex[currSeqStep];*/
            //seqStepVariant.go = af.railSourceVariants[1][seqStepVariant.sourceVariantIndex].go;

            /*seqStepVariant.svBackToFront = af.seqBX[currSeqStep];
            seqStepVariant.svMirrorZ = af.seqBZ[currSeqStep];
            seqStepVariant.svInvert = af.seqBInvert180[currSeqStep];*/

            af.railBSequencer.seqList[currSeqStep] = seqStepVariant;
        }

        return seqStepVariant;
    }

    //-----------
    public SeqItem SetPostSequenceData(ref SeqItem currSeqStepVariant, int currSeqStep)
    {
        /*currSeqStepVariant.pos = af.seqPostOffset[currSeqStep];
        currSeqStepVariant.size = af.seqPostSize[currSeqStep];
        currSeqStepVariant.rot = af.seqPostRotate[currSeqStep];*
        currSeqStepVariant.sourceVariantIndex = af.seqPostSourceVarIndex[currSeqStep];*/
        //currSeqStepVariant.go = af.postVariants[currSeqStepVariant.sourceVariantIndex].go;
        af.railASequencer.seqList[currSeqStep] = currSeqStepVariant;
        return currSeqStepVariant;
    }

    //-------------
    /*public void CopyOptimalToUserSequence(LayerSet railSet, bool createAlso = true)
    {
        int optCount = af.optimalSequenceRailA.Count;
        if (railSet == kRailALayer)
        {
            if(createAlso)
                CreateOptimalSequenceForLayer();
            if (optCount > AutoFenceCreator.kMaxNumSeqSteps)
                optCount = AutoFenceCreator.kMaxNumSeqSteps;
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                int optIndex = i % optCount;
                af.railASequencer.seqList[i] = new SeqItem(af.optimalSequenceRailA[optIndex]);
            }
        }
        else if (railSet == kRailBLayer)
        {
            if(createAlso)
                CreateOptimalSequenceB();
            optCount = af.optimalSequenceRailB.Count;
            if (optCount > AutoFenceCreator.kMaxNumSeqSteps)
                optCount = AutoFenceCreator.kMaxNumSeqSteps;
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                int optIndex = i % optCount;
                af.railBSequencer.seqList[i] = new SeqItem(af.optimalSequenceRailB[optIndex]);
            }
        }
    }*/

    //-------------
    // will loop source if fewer than dest
    public void CopySequence(List<SeqItem> source, List<SeqItem> dest)
    {
        int sourceCount = source.Count;

        int length = AutoFenceCreator.kMaxNumSeqSteps;

        if (dest.Count < AutoFenceCreator.kMaxNumSeqSteps)
        {
            Debug.LogWarning("dest.Count < kMaxNumSeqSteps in CopySequence()");
        }

        if (sourceCount > length)
            sourceCount = length;
        for (int i = 0; i < length; i++)
        {
            int loopIndex = i % sourceCount;
            dest[i] = new SeqItem(source[loopIndex]);
        }
    }

    //-------------
    public List<SeqItem> CreateOptimalSequenceForLayer(LayerSet layer)
    {
        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
        List<SeqItem> optimalSeq = SeqItem.CreateOptimalSequence(af, sourceVariants, layer);

        return optimalSeq;
    }

    /*public List<SeqItem> CreateOptimalPost()
    {
        af.optimalSequencePost = SourceVariations.CreateOptimalSequence(af.nonNullPostVariants,
            System.Convert.ToBoolean(af.varRailBBackToFront[0]),
            System.Convert.ToBoolean(af.varRailBMirrorZ[0]),
            System.Convert.ToBoolean(af.varRailBInvert[0]));
        return af.optimalSequencePost;
    }
    public List<SeqItem> CreateOptimalSubpost()
    {
        af.optimalSequenceSubpost = SourceVariations.CreateOptimalSequence(af.nonNullSubpostVariants,
            System.Convert.ToBoolean(af.varRailBBackToFront[0]),
            System.Convert.ToBoolean(af.varRailBMirrorZ[0]),
            System.Convert.ToBoolean(af.varRailBInvert[0]));
        return af.optimalSequenceSubpost;
    }*/

    //-------------
    // Fuill the user sequence with random values
    public void RandomizeSequence(LayerSet railSet, int startStep = 0, int endStep = AutoFenceCreator.kMaxNumSeqSteps)
    {
        SeqItem thisSeqStep = new SeqItem();
        //List<SourceVariant> variantList = af.nonNullRailSourceVariants[0];
        //List<SeqItem> seq = af.railASequencer.seqList;
        //int numGos = 0;
        /*if(railSet == kRailALayer) {
            seq = af.railASequencer.seqList;
            variantList = af.nonNullRailSourceVariants[0];
         }*/
        if (railSet == kRailBLayer)
        {
            //seq = af.railBSequencer.seqList;
            //variantList = af.nonNullRailSourceVariants[1];
        }

        for (int i = startStep; i < endStep; i++)
        {
            //int goIndex = UnityEngine.Random.Range(0, variantList.Count);
            //thisSeqStep.go = variantList[sourceVariantIndex].go;
            //thisSeqStep.sourceVariantIndex = goIndex;
            thisSeqStep.backToFront = (UnityEngine.Random.value > 0.5f);
            thisSeqStep.mirrorZ = (UnityEngine.Random.value > 0.5f);
            thisSeqStep.invert = (UnityEngine.Random.value > 0.5f);

            thisSeqStep.size = Vector3.one;
            //thisSeqStep.svSize.x = UnityEngine.Random.Range(0.5f, 1.5f);

            if (railSet == kRailALayer)
                af.railASequencer.seqList[i] = new SeqItem(thisSeqStep);
            else if (railSet == kRailBLayer)
                af.railBSequencer.seqList[i] = new SeqItem(thisSeqStep);
        }
    }
}