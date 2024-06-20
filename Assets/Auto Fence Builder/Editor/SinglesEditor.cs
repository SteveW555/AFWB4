////#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
////#pragma warning disable 0414

using AFWB;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Called from AutoFenceEditor, builds controls for the Variations Sequence Blocks
public class SinglesEditor
{
    private LayerSet kRailALayer = LayerSet.railALayer;
    private LayerSet kRailBLayer = LayerSet.railBLayer;
    private LayerSet kPostLayer = LayerSet.postLayer;
    private LayerSet kSubpostLayer = LayerSet.subpostLayer;

    private AutoFenceCreator af;
    private AutoFenceEditor ed;

    private LayerSet currLayerSet;

    private SerializedProperty numUserSeqStepsProperty;
    private SerializedProperty seqOffset, seqSize, seqRot;
    private SerializedProperty seqInfoProperty;
    private SerializedProperty SeqItemListProperty;

    private SeqItem currSeqStepVariant = null;
    private List<SeqItem> SeqItemList = null; // the list of SeqItem for all seq steps

    //private List<SeqItem> optimalSeq = null;
    private List<SourceVariant> sourceVariants = null; // the 5 prefabs that have been assigned as possible variants

    private List<GameObject> displayVariationGOs = null;
    public List<GameObject> mainPrefabs = null;

    private bool foundEnabledObject = false;
    private bool showSourcePrefabs = true;
    private bool needsRebuild = false;
    private bool[] prefabButtonSwitch = new bool[5];

    //private SeqInfo seqInfo = null;
    private PrefabTypeAFWB PrefabTypeAFWB;

    private LayerSet layerSet;
    private int maxNumVariations = 0;
    private int currentMainPrefabTypeAFWB = 0;
    private int mainMenuIndex = 0;
    private List<int> varPrefabIndex = null;
    private List<int> varMenuIndex = null;
    private string displayVariationGOsStr = "";
    private string layerWord = "post";
    private string randomizeUserSeqString = "Randomize All Steps", optimalToUserSeqString = "Quick-Fill Optimal";
    private string randomToUserSeqString = "Replace From Random Mode";
    private List<string> prefabNames = null;

    private string[] autoAssignStrings = new[] { "Consecutive", "Same category", "Any", "All Main" };
    private int autoAssignMenuIndex = 0;
    private bool updateSequenceAlso = false;
    private int autoAssignToolbarValue = 0;
    private int layerIndex = 0; // 0 = railA, 1 = railB 2 = post
    private int numBuilt = 0, numSingles = 0;
    private bool showSingles = false, useSingles = false;

    private Color lineColor = new Color(0.94f, 0.94f, 0.94f);

    public SinglesEditor(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor, LayerSet inLayerSet)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
        currLayerSet = inLayerSet;
        //GrabVariablesForSet();
    }

    //------------------
    // To avoid lots of case switching in the ed code, set the variables here
    private void GrabVariablesForSet()
    {
        maxNumVariations = AutoFenceCreator.kMaxNumSourceVariants;
        if (currLayerSet == kPostLayer)
        {
            /*seqInfoProperty = ed.serializedObject.FindProperty("postSeqInfo");
            seqList = af.SeqItemListPost;
            seqListProperty = ed.serializedObject.FindProperty("SeqItemListPost");
            layerNameString = "post";
            seqOffset = ed.serializedObject.FindProperty("seqPostOffset");
            seqSize = ed.serializedObject.FindProperty("seqPostSize");
            seqRot = ed.serializedObject.FindProperty("seqPostRotate");
            seqInfo = af.postSeqInfo;
            prefabType = prefabType.postPrefab;
            sourceVariants = af.postVariants;
            displayVariationGOsStr = "postDisplayVariationGOs";
            displayVariationGOs = af.postDisplayVariationGOs;
            ///varPrefabChoiceIndex = varPrefabChoiceIndexForLayers[0];
            sourceVariantPrefabIndices = af.varPrefabIndexPost;
            variantGlobalPrefabMENUIndices9 = af.postVarMenuIndex;
            layerPrefabMenuNames = af.postNames;
            mainPrefabs = af.postPrefabs;
            currentMainPrefabTypeAFWB = af.currentPost_PrefabIndex;
            mainPrefab_MenuIndex = af.currentPost_PrefabMenuIndex;
            numSectionsBuiltForLayer = af.postsBuiltCount;*/

            layerIndex = 2;
        }
        else if (currLayerSet == kRailALayer)
        {
            layerWord = "railA";
            PrefabTypeAFWB = PrefabTypeAFWB.railPrefab;
            sourceVariants = af.railSourceVariants[0];
            //displayVariationGOsStr = "railADisplayVariationGOs";
            //displayVariationGOs = af.railADisplayVariationGOs;
            //varPrefabIndex = af.railASourceVariant_PrefabIndices;
            varMenuIndex = af.railASourceVariant_MenuIndices;
            prefabNames = af.railMenuNames;
            mainPrefabs = af.railPrefabs;
            currentMainPrefabTypeAFWB = af.currentRail_PrefabIndex[0];
            mainMenuIndex = af.currentRail_PrefabMenuIndex[0];
            layerIndex = 0;
            numBuilt = af.railABuiltCount;
            showSingles = ed.showSinglesRailA;
            //useSingles = af.useRailSingles[0];
            if (af.railSinglesContainer[0] == null )
                Debug.Log("railSinglesContainer[0] is null \n");
            else
                numSingles = af.railSinglesContainer[0].numInUse;
        }
        else if (currLayerSet == kRailBLayer)
        {
            layerWord = "railB";
            PrefabTypeAFWB = PrefabTypeAFWB.railPrefab;
            sourceVariants = af.railSourceVariants[1];
            //displayVariationGOsStr = "railBDisplayVariationGOs";
            //displayVariationGOs = af.railBDisplayVariationGOs;
            //varPrefabIndex = af.railBSourceVariant_PrefabIndices;
            varMenuIndex = af.railBSourceVariant_MenuIndices;
            prefabNames = af.railMenuNames;
            mainPrefabs = af.railPrefabs;
            currentMainPrefabTypeAFWB = af.currentRail_PrefabIndex[1];
            mainMenuIndex = af.currentRail_PrefabMenuIndex[1];
            layerIndex = 1;
            numBuilt = af.railBBuiltCount;
            showSingles = ed.showSinglesRailB;
            //useSingles = af.useRailSingles[1];
            if (af.railSinglesContainer[1] == null)
                Debug.Log("railSinglesContainer[1] is null \n");
            else
                numSingles = af.railSinglesContainer[0].numInUse;
        }
    }

    //------------------
    public void SetupSingles(LayerSet layer)
    {
        //return;
        currLayerSet = layer;
        GrabVariablesForSet();
        needsRebuild = false;
        ed.DrawUILine(ed.UILineGrey, 6, 10, 2, 10);
        ed.cyanBoldStyle.wordWrap = false;

        if (af.singlesContainer.GetUseSinglesForLayer(layer, af) == false)
            return;

        //===================      Get Properties     ===================================
        SerializedProperty singlesContainerArrayProp = null, layerSinglesContainerProp = null;
        if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
        {
            singlesContainerArrayProp = ed.serializedObject.FindProperty("railSinglesContainer");
            layerSinglesContainerProp = singlesContainerArrayProp.GetArrayElementAtIndex((int)layer);
        }
        else
        {
            layerSinglesContainerProp = ed.serializedObject.FindProperty("postSinglesContainer");
        }
        if (layerSinglesContainerProp == null)
            Debug.Log("singlesContainerArrayProp is null \n");

        SerializedProperty variantsListProp = layerSinglesContainerProp.FindPropertyRelative("singleVariants");
        SerializedProperty numInUseProp = layerSinglesContainerProp.FindPropertyRelative("numInUse");
        SerializedProperty maxIndexInUseProp = layerSinglesContainerProp.FindPropertyRelative("maxIndexInUse");
        //================================================================================================

        List<SinglesItem> SinglesItemsList = af.singlesContainer.GetSingleVariantsForLayer(layer, af);
        if (SinglesItemsList == null)
            Debug.Log($"SinglesItemsList  {layer}  is null \n");

        int numSinglesItems = SinglesItemsList.Count;
        int numSourceVariants = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true);
        SinglesContainer layerSingles = af.singlesContainer.GetSinglesForLayer(layer, af);

        //==============================
        //      Singles Header
        //==============================
        {
            GUILayout.BeginHorizontal();

            //      Foldout
            //======================
            GUILayout.Space(9); //puts foldout triangle inside box
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                af.showRailSinglesEditor[layerIndex] = EditorGUILayout.Foldout(af.showRailSinglesEditor[layerIndex], "");
            else
                af.showPostSinglesEditor = EditorGUILayout.Foldout(af.showPostSinglesEditor, "");

            //
            //     Use Singles Checkbox
            //==============================
            //GUILayout.Space(3);
            EditorGUILayout.LabelField(new GUIContent(af.GetLayerNameAsString(layer) + " Singles Editor", "This will contains a list" +
                "of all single-section modifications. \nThese are created by right-clicking on a fence part in the Scene View" +
                "\nThese will override the Sequence assignment for this gizmoSingletonInstance"), ed.greenStyle2, GUILayout.Width(190));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ed.serializedObject.FindProperty("useRailSingles").GetArrayElementAtIndex(layerIndex), new GUIContent(""), GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                ed.serializedObject.ApplyModifiedProperties();
                af.ResetPoolForLayer(currLayerSet);
                af.ForceRebuildFromClickPoints();
            }
            GUILayout.Space(10);
            GUIStyle helpButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            helpButtonStyle.fontStyle = FontStyle.Bold;
            helpButtonStyle.normal.textColor = new Color(0.2f, .50f, .78f);
            string helpStr = "?";
            int helpButtW = 24;
            if (ed.showSinglesHelp)
            {
                helpStr = "Close";
                helpButtW = 45;
            }
            //     Singles Help
            //=====================
            if (GUILayout.Button(new GUIContent(helpStr, "Show Help for Singles Editor"), helpButtonStyle, GUILayout.Width(helpButtW)))
            {
                ed.showSinglesHelp = !ed.showSinglesHelp;
            }
            //GUILayout.FlexibleSpace();
            if (ed.showSinglesHelp)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical("box");
                ed.varEd.ShowSinglesHelp();
                if (GUILayout.Button("Close", GUILayout.Width(45)))
                {
                    ed.showSinglesHelp = false;
                }
                GUILayout.Space(3);
                GUILayout.EndVertical();
            }
            else
            {
                //     Num Singles In Use
                //============================
                int numInUse = af.singlesContainer.GetNumberOfSinglesInUseForLayer(currLayerSet, af);
                // EditorGUILayout.LabelField(new GUIContent($"(In Use =  {numInUse})", $"There are {numInUse} in use"), ed.label12Style, GUILayout.Width(175));
                GUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
        }

        //      No Singles assigned
        //==============================
        if (useSingles && numSingles == 0)
            EditorGUILayout.LabelField("No singleVariants have been set. To assign single variations, right-click on any Post or Rail in Scene View", ed.warningStyle2);

        GUILayout.Space(5);

        //    Clear and Disable Buttons
        //==================================
        if (showSingles)
        {
            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(numSingles == 0);

            //    Clear Button
            //====================
            if (GUILayout.Button(
                new GUIContent("Clear All Singles", "Sets all sections assigned with a unique variation back to their default." +
                "\n\n This will Clear the entire list as there no longer any Individual Single sections"), GUILayout.Width(120)))
            {
                af.singlesContainer.ResetAllSinglesForLayer(currLayerSet, af);
                if (currLayerSet == LayerSet.railALayer)
                {
                    af.railSinglesEnabled[0] = false;
                    af.singlesContainer.ToggleAllSingleVariants(kRailALayer, af.railSinglesEnabled[0], af);
                }
                if (currLayerSet == LayerSet.railBLayer)
                {
                    af.railSinglesEnabled[1] = false;
                    af.singlesContainer.ToggleAllSingleVariants(kRailBLayer, af.railSinglesEnabled[1], af);
                }
                if (currLayerSet == LayerSet.postLayer)
                {
                    af.postSinglesEnabled = false;
                    af.singlesContainer.ToggleAllSingleVariants(kPostLayer, af.postSinglesEnabled, af);
                }
                af.ResetPoolForLayer(currLayerSet);
                af.ForceRebuildFromClickPoints();
                return;
            }

            string disableString = "Disable All Singles";
            if (af.railSinglesEnabled[0] == false)
                disableString = "Enable All Singles";

            //    Disable Button
            //=====================
            if (GUILayout.Button(new GUIContent(disableString, "Toggle that disables all single section modifications"), GUILayout.Width(130)))
            {
                if (currLayerSet == LayerSet.railALayer)
                {
                    af.railSinglesEnabled[0] = !af.railSinglesEnabled[0];
                    af.singlesContainer.ToggleAllSingleVariants(kRailALayer, af.railSinglesEnabled[0], af);
                    if (af.railSinglesEnabled[0] == false && numSingles > 0)
                        EditorGUILayout.LabelField(new GUIContent("All Singles are disabled, 'Enable All Singles' to show"), ed.warningStyle, GUILayout.Width(300));
                }
                if (currLayerSet == LayerSet.railBLayer)
                {
                    af.railSinglesEnabled[1] = !af.railSinglesEnabled[1];
                    af.singlesContainer.ToggleAllSingleVariants(kRailBLayer, af.railSinglesEnabled[1], af);
                    if (af.railSinglesEnabled[0] == false && numSingles > 0)
                        EditorGUILayout.LabelField(new GUIContent("All Singles are disabled, 'Enable All Singles' to show"), ed.warningStyle, GUILayout.Width(300));
                }
                if (currLayerSet == LayerSet.postLayer)
                {
                    af.postSinglesEnabled = !af.postSinglesEnabled;
                    af.singlesContainer.ToggleAllSingleVariants(kPostLayer, af.postSinglesEnabled, af);
                    if (af.postSinglesEnabled == false && ed.postSinglesList.arraySize > 0)
                        EditorGUILayout.LabelField(new GUIContent("All Singles are disabled, 'Enable All Singles' to show"), ed.warningStyle, GUILayout.Width(300));
                }
                af.ResetPoolForLayer(currLayerSet);
                af.ForceRebuildFromClickPoints();
            }

            EditorGUI.EndDisabledGroup();

            //    Randomize Button
            //==================================

            //    Randomize Button
            //=======================
            if (GUILayout.Button(new GUIContent("Randomize", "Sets a random selection of sections" +
                " to a random choice of the Source Prefabs that are set in 'Use Rail/Post Variations' "),
                GUILayout.Width(120)))
            {
                af.singlesContainer.ClearAllSinglesForLayer(layer, af);

                int numSections = af.GetNumSectionsBuiltForLayer(layer) - 1;

                for (int i = 0; i < numSections; i++)
                {
                    //get a random int between 0 and numSourceVariantsInUseProp
                    int randomSourceVariantIndex = Random.Range(0, numSourceVariants - 1);
                    // now set approx half of them to be unchanged by giving them a n index of -1
                    if (Random.value > -0.5f)
                        layerSingles.SetSingleSourceVariantAtSectionIndex(i, randomSourceVariantIndex);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            //===================================================
            //      Main Loop : Singles Variants List Editor
            //===================================================
            //List<SinglesItem> SinglesItemsList = af.GetSingleVariantsForLayer(sourceLayerList);

            if (af.railSinglesEnabled[0] && numSinglesItems > 0)
            {
                GUIStyle smallGreyStyle = new GUIStyle(EditorStyles.label);
                smallGreyStyle.normal.textColor = new Color(0.2f, 0.2f, 0.2f);
                smallGreyStyle.fontSize = 9;
                //EditorGUILayout.Space();
                for (int i = 0; i < numSinglesItems; i++)
                {
                    SerializedProperty singleVariantProp = variantsListProp.GetArrayElementAtIndex(i);
                    SerializedProperty posProp = singleVariantProp.FindPropertyRelative("pos");
                    SerializedProperty sizeProp = singleVariantProp.FindPropertyRelative("size");
                    SerializedProperty rotPriop = singleVariantProp.FindPropertyRelative("rot");

                    /*if(i%2 == 1)
                        GUI.backgroundColor = new Color(0.99f, 0.09f, 0.09f);
                    else
                        GUI.backgroundColor = Color.white;*/

                    if (i % 2 == 1)
                    {
                        Rect rect = EditorGUILayout.BeginVertical();
                        EditorGUI.DrawRect(rect, new Color(0.27f, 0.27f, 0.27f));
                    }
                    else
                    {
                        Rect rect = EditorGUILayout.BeginVertical();
                        EditorGUI.DrawRect(rect, new Color(0.24f, 0.24f, 0.24f));
                    }

                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();

                    //  GO Name
                    //=====================
                    int sourceVarIndex = singleVariantProp.FindPropertyRelative("sourceVariantIndex").intValue;
                    string shortprefabName = af.GetSinglesSourceVariantGONameForLayerWithSectionIndex(layer, sourceVarIndex);
                    string indexStr = i + ":  ";
                    if (shortprefabName == "")
                        return;
                    shortprefabName = shortprefabName.Substring(0, shortprefabName.Length - 5);
                    indexStr += shortprefabName;
                    EditorGUILayout.LabelField(indexStr, ed.label11Style, GUILayout.Width(180));

                    //  Section Index
                    //=====================
                    indexStr = "    Section ";
                    indexStr += sourceVarIndex;
                    EditorGUILayout.LabelField(indexStr, ed.label11Style);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    //===================================
                    //      Position Offset
                    //===================================
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.LabelField(new GUIContent("Pos:", "Offset the position of this variation. Default=0."), ed.small10Style, GUILayout.Width(38));
                    EditorGUILayout.PropertyField(posProp, new GUIContent(""), GUILayout.Width(127));
                    if (GUILayout.Button(new GUIContent("X", "Set Position Offset values to default 0"), GUILayout.Width(8)))
                    {
                        posProp.vector3Value = Vector3.zero;
                    }
                    //===================================
                    //      Size
                    //===================================
                    EditorGUILayout.LabelField(new GUIContent("  Size:", "Multiply Size of this variation. Default=1/1/1."), ed.small10Style, GUILayout.Width(48));
                    EditorGUILayout.PropertyField(sizeProp, new GUIContent(""), GUILayout.Width(127));
                    if (GUILayout.Button(new GUIContent("X", "Set Size values to default 1"), GUILayout.Width(8)))
                    {
                        sizeProp.vector3Value = Vector3.one;
                    }
                    //===================================
                    //      Rotation
                    //===================================
                    EditorGUILayout.LabelField(new GUIContent("   Rot:", "Add Rotation to this variation . Default=0."), ed.small10Style, GUILayout.Width(48));
                    EditorGUILayout.PropertyField(rotPriop, new GUIContent(""), GUILayout.Width(127));
                    if (GUILayout.Button(new GUIContent("X", "Set Rotation values to default 0"), GUILayout.Width(8)))
                    {
                        rotPriop.vector3Value = Vector3.zero;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        ed.serializedObject.ApplyModifiedProperties();
                        af.ResetPoolForLayer(layer);
                        af.ForceRebuildFromClickPoints();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                    EditorGUILayout.EndVertical();
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }

    //---------------------------------
    private void UpdateSinglesAfterSourceVariantsChanged(LayerSet layer)
    {
        int numSourceVariants = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true);

        //===============================================================================
        //      Update Singles to reflect the new Variants
        //===============================================================================
        //--At this point, we've only changed the menu properties, so we need to update the real af menu lists and set the variants

        //-- This should update the REAL af menu lists (railASourceVariant_MenuIndices etc.) from the sourceVariant_MenuIndex_List properties
        ed.serializedObject.ApplyModifiedProperties();

        //=== Now convert to Prefab indices, check them,  and set the sourceVariant choices
        List<int> sourceVariant_PrefabIndices = af.GetSourceVariantMenuIndicesListForLayer(layer);
        for (int i = 1; i < numSourceVariants + 1; i++)
        {
            int prefabIndex = sourceVariant_PrefabIndices[i];
            if (prefabIndex >= mainPrefabs.Count)
                Debug.Log(prefabIndex + mainPrefabs.Count + "\n");

            GameObject go = mainPrefabs[prefabIndex];

            sourceVariants[i].Go = go;

            for (int j = 0; j < af.GetSequencerForLayer(layer).Length(); j++)
            {
                if (layer == kPostLayer)
                {
                    //af.seqPostSourceVarIndex[i] = seqList[i].sourceVariantIndex;
                    //af.seqPostSize[i] = ed.EnforceVectorMinMax(af.seqPostSize[i], -9.99f, 9.99f);
                }

                //currSeqStepVariant = seqList[i];
                //ed.varEd.SetSequenceVariantFromDisplaySettings(this.sourceLayerList, ref currSeqStepVariant, i);
            }
        }
    }
}