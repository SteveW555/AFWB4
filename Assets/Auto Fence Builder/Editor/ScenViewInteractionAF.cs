using AFWB;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public partial class AutoFenceEditor
{
    private RaycastHit HandleRightClick(Event currentEvent, Ray rayPosition, ref GameObject go, bool isClickPoint, int[] sectionIndexForLayers,
        LayerSet layer)
    {
        RaycastHit hit;
        if (Physics.Raycast(rayPosition, out hit, 1000.0f))
            go = hit.transform.gameObject;

        //      Right-Click, No Shift
        //=================================
        if (currentEvent.button == 1 && currentEvent.type == EventType.MouseDown && currentEvent.shift == false)
        {
            string railName = "", matchStr = "", layerName = af.GetLayerNameAsString(layer);
            VariationMode variationMode = VariationMode.none;
            int layerNum = layer.Int();
            int sectionIndex = layerNum <= 5 ? sectionIndexForLayers[layerNum] : 0;

            EditorContextInfo contextInfo = new EditorContextInfo(this);
            contextInfo.sectionIndex = sectionIndex;
            contextInfo.transform = go.transform;
            contextInfo.layer = layer;

            string allowRandString = "", variationsString = "", enableStr = "Enable", disableStr = "Disable";
            string variationString = $"Enable {layer.String()} Variations";
            bool allowRand = af.GetAllowRandomizationForLayer(layer);
            string randString = $"{(allowRand == false ? enableStr : disableStr)} {layerName} Randomization";

            //=========================================
            //           Posts Context Menus
            //=========================================
            if (layer == LayerSet.postLayer)
            {
                List<string> varNames = new List<string>();
                GenericMenu menu = new GenericMenu();

                //variationsEnabled = false;

                contextInfo.seqStepNum = currSeqPostStepIndex;
                contextInfo.layer = LayerSet.postLayer;

                //--------------------------
                //   Convert To ClickPoint
                //--------------------------
                if (isClickPoint == false)
                {
                    menu.AddItem(new GUIContent("Convert to Moveable ClickPoint Node"), false, sceneViewContextMethods.ConvertToClickPoint, contextInfo);
                    menu.AddSeparator("");
                }
                else if (isClickPoint == true)
                {
                    menu.AddItem(new GUIContent("Remove Node Status"), false, sceneViewContextMethods.RemoveClickPoint, contextInfo);
                    menu.AddSeparator("");
                }

                //=====  Allow Post Randomization  =====
                //sets the string to "Enable/Disable LayerName Randomization"
                allowRandString = $"{(allowRand == false ? enableStr : disableStr)} {layerName} Randomization";
                menu.AddItem(new GUIContent(allowRandString), false, sceneViewContextMethods.SetRandomizationStatus, contextInfo);
                menu.AddSeparator("");

                //=====   Use Post Variations    =====
                bool useVariations = af.GetUseLayerVariations(layer);
                //sets the string to "Enable/Disable LayerName Randomization"
                variationsString = $"{(useVariations == false ? enableStr : disableStr)} {layerName} Variations";
                menu.AddItem(new GUIContent(variationsString), false, sceneViewContextMethods.SetVariationStatus, contextInfo);
                menu.AddSeparator("");

                if (useVariations)
                {
                    //======================
                    //   Single For Posts
                    //======================

                    //=====  Info Labels  =====

                    contextInfo.menuIndex = 301;
                    variationMode = af.GetVariationModeForLayer(layer);
                    varNames = af.GetSourceVariantGoNamesForLayer(layer);

                    if (variationMode != VariationMode.sequenced)
                        contextInfo.flag = 2;

                    //AddEmptyLine(menu);
                    AddTextLine(menu, $"\n------            Individual Item Assignment  :  {layerName}            ------");
                    AddTextLine(menu, "Note: Applies only to this single Post, and willOverride the Seq assignment) \n");
                    AddTextLine(menu, $"To change availble prefab choices, open {layerName} - Use  {layerName} Variations\n");
                    AddTextLine(menu, "\n");

                    //=====  Assign Singles  =====
                    for (int variantIndex = 0; variantIndex < varNames.Count; variantIndex++)
                    {
                        //this.sceneViewMenuIndex = sourceVariantIndex;
                        contextInfo.variantIndex = variantIndex;
                        contextInfo.isSingle = true;

                        string prefixStr = $"For Fence Section [{sectionIndex}]  use:    (Main)  ";

                        if (variantIndex > 0)
                            prefixStr = $"                                                  (Var {variantIndex.ToString()})   ";

                        string varName = prefixStr + varNames[variantIndex];
                        string simpleVarName = af.StripPostFromName(varName);
                        //      Add the Post Single
                        //==============================
                        menu.AddItem(new GUIContent(prefixStr + varNames[variantIndex]), false, sceneViewContextMethods.SetupSingle, contextInfo);
                    }
                    //=====================
                    //   Sequence For Posts
                    //========================
                    //==================================
                    //    Enable / Disable Sequence
                    //==================================

                    //-----  Add a Menu option to Enable the Sequencer  -----
                    menu.AddSeparator("");

                    /*if (isLayerA)
                    {
                        contextInfo.menuIndex = 500;
                        if (af.GetUseSequencerForLayer(sourceLayerList) == false)
                            menu.AddItem(new GUIContent("Enable Rail A Sequencer"), false, ContextMenuCallback, contextInfo);
                        else
                            menu.AddItem(new GUIContent("Disable Rail B Sequencer"), false, ContextMenuCallback, contextInfo);
                    }
                    menu.AddSeparator("");

                    if (af.GetUseSequencerForLayer(sourceLayerList) == true && variationMode == VariationMode.sequenced)
                    {
                        //=====  Info Labels  =====
                        if (isLayerA)
                        {
                            menu.AddItem(new GUIContent("\n---- Sequencer Step " + currSeqRailAStepIndex + " Rail A ----"), false, null, null);
                            menu.AddItem(new GUIContent("Will apply to every Step " + currSeqRailAStepIndex +
                                " of " + af.seqNumSteps[kRailALayerInt] + " in sequence "), false, null, null);
                        }
                        if (isLayerB)
                        {
                            menu.AddItem(new GUIContent("---- Sequencer Step " + currSeqRailAStepIndex + " Rail B ----"), false, null, null);
                            menu.AddItem(new GUIContent("Will apply to every Step " + currSeqRailBStepIndex +
                                " of " + af.seqNumSteps[kRailBLayerInt] + " in sequence "), false, null, null);
                        }

                        for (int v = varNames.Count; v < (varNames.Count + varNames.Count); v++)
                        {
                            this.sceneViewMenuIndex = v;
                            contextInfo.menuIndex = v - varNames.Count;
                            contextInfo.isSingle = false; // it's for the sequencer
                            string varName = varNames[v - varNames.Count];
                            string simpleVarName = af.StripPanelRailFromName(varName);
                            string menuNumStr = "Assign   ( Var " + (v - (varNames.Count)).ToString() + ") " + simpleVarName + "   to  Step " + (currSeqRailAStepIndex + 1);
                            if (v == varNames.Count)
                                menuNumStr = "Assign   (Main) " + simpleVarName + "   to  Step " + (currSeqRailAStepIndex + 1);
                            //string displayName = menuNumStr + " " + simpleVarName;

                            menu.AddItem(new GUIContent(menuNumStr), false, ContextMenuCallback, contextInfo);
                        }
                    }*/
                }
                //===========================
                //   Show in Assets Folder
                //============================
                AddEmptyLine(menu);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Show Post in Prefabs Folder"), false, sceneViewContextMethods.ShowInPrefabsFolder, contextInfo);

                /*menu.AddItem(new GUIContent(""), false, ContextMenuCallback, contextInfo);
                contextInfo.resetSection = true;
                contextInfo.menuIndex = 100;

                if (variationsEnabled)
                {
                    menu.AddItem(new GUIContent("Reset Section Variation Settings"), false, ContextMenuCallback, contextInfo);
                    menu.AddItem(new GUIContent("(Change choice of prefabs in Rail A or B 'Variations')"), false, null, null);
                }*/

                menu.ShowAsContext();
                Repaint();
            }

            //=========================================
            //        Rails Context Menus
            //=========================================
            else if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
            {
                railName = af.StripPanelRailFromName(go.name);

                List<string> varNames = new List<string>();
                GenericMenu menu = new GenericMenu();

                if (layer == LayerSet.railALayer)
                {
                    contextInfo.seqStepNum = currSeqRailStepIndex[0];
                }
                else
                {
                    contextInfo.seqStepNum = currSeqRailStepIndex[0];
                }
                contextInfo.layer = layer;

                //=====  Allow Rail Randomization  =====

                menu.AddSeparator("");

                //=====   Use Rail Variations    =====

                bool useVariations = af.GetUseLayerVariations(layer);
                //sets the string to "Enable/Disable LayerName Randomization"
                variationsString = $"{(useVariations == false ? enableStr : disableStr)} {layerName} Variations";
                menu.AddItem(new GUIContent(variationsString), false, sceneViewContextMethods.SetVariationStatus, contextInfo);
                menu.AddSeparator("");

                //=====   Use Rail Sequencer    =====
                contextInfo.layer = 0; //RailA
                menu.AddSeparator("");
                //if (sourceLayerList == LayerSet.railALayer)
                {
                    bool useSeq = af.GetUseSequencerForLayer(layer);
                    string seqString = $"{(useSeq == false ? enableStr : disableStr)} {layerName} Sequencer";
                    menu.AddItem(new GUIContent(seqString), false, sceneViewContextMethods.SetSequencerStatus, contextInfo);
                }
                menu.AddSeparator("");

                if (useVariations)
                {
                    //======================
                    //   Single For Rails
                    //======================

                    //=====  Info Labels  =====

                    contextInfo.menuIndex = 301;
                    variationMode = af.GetVariationModeForLayer(layer);
                    varNames = af.GetSourceVariantGoNamesForLayer(layer);
                    if (variationMode != VariationMode.sequenced)
                        contextInfo.flag = 2;

                    AddTextLine(menu, $"\n------            Individual Item Assignment  :  {layerName}            ------");
                    AddTextLine(menu, "Note: Applies only to this single Rail section, and willOverride the Seq assignment) \n");
                    AddTextLine(menu, $"To change availble prefab choices, open {layerName} - Use  {layerName} Variations\n");
                    AddEmptyLine(menu);

                    //=====  Assign Singles  =====
                    for (int variantIndex = 0; variantIndex < varNames.Count; variantIndex++)
                    {
                        //this.sceneViewMenuIndex = sourceVariantIndex;
                        contextInfo.variantIndex = variantIndex;
                        contextInfo.isSingle = true;

                        string prefixStr = $"Section [{sectionIndex}]  use:    (Main)  ";

                        if (variantIndex > 0)
                            prefixStr = $"                                (Var {variantIndex.ToString()})   ";

                        string varName = prefixStr + varNames[variantIndex];
                        string simpleVarName = af.StripPanelRailFromName(varName);
                        //      Add the Rail Single
                        //==============================
                        menu.AddItem(new GUIContent(prefixStr + varNames[variantIndex]), false, sceneViewContextMethods.SetupSingle, contextInfo);
                    }
                    //======================
                    //   Sequence For Rails
                    //=======================

                    //    Enable / Disable Sequence
                    //==================================

                    /*if (af.GetUseSequencerForLayer(sourceLayerList) == true && variationMode == VariationMode.sequenced)
                    {
                        //=====  Info Labels  =====
                        if (isLayerA)
                        {
                            menu.AddItem(new GUIContent("\n---- Sequencer Step " + currSeqRailAStepIndex + " Rail A ----"), false, null, null);
                            menu.AddItem(new GUIContent("Will apply to every Step " + currSeqRailAStepIndex +
                                " of " + af.seqNumSteps[kRailALayerInt] + " in sequence "), false, null, null);
                        }
                        if (isLayerB)
                        {
                            menu.AddItem(new GUIContent("---- Sequencer Step " + currSeqRailAStepIndex + " Rail B ----"), false, null, null);
                            menu.AddItem(new GUIContent("Will apply to every Step " + currSeqRailBStepIndex +
                                " of " + af.seqNumSteps[kRailBLayerInt] + " in sequence "), false, null, null);
                        }

                        for (int v = varNames.Count; v < (varNames.Count + varNames.Count); v++)
                        {
                            this.sceneViewMenuIndex = v;
                            contextInfo.menuIndex = v - varNames.Count;
                            contextInfo.isSingle = false; // it's for the sequencer
                            string varName = varNames[v - varNames.Count];
                            string simpleVarName = af.StripPanelRailFromName(varName);
                            string menuNumStr = "Assign   ( Var " + (v - (varNames.Count)).ToString() + ") " + simpleVarName + "   to  Step " + (currSeqRailAStepIndex + 1);
                            if (v == varNames.Count)
                                menuNumStr = "Assign   (Main) " + simpleVarName + "   to  Step " + (currSeqRailAStepIndex + 1);
                            //string displayName = menuNumStr + " " + simpleVarName;

                            menu.AddItem(new GUIContent(menuNumStr), false, ContextMenuCallback, contextInfo);
                        }
                    }*/
                }
                AddEmptyLine(menu);
                menu.AddSeparator("");
                AddTextLine(menu, $"Enable Posts\n");

                AddEmptyLine(menu);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Show Rail in Prefabs Folder"), false, sceneViewContextMethods.ShowInPrefabsFolder, contextInfo);

                /*menu.AddItem(new GUIContent(""), false, ContextMenuCallback, contextInfo);
                contextInfo.resetSection = true;
                contextInfo.menuIndex = 100;

                if (variationsEnabled)
                {
                    menu.AddItem(new GUIContent("Reset Section Variation Settings"), false, ContextMenuCallback, contextInfo);
                    menu.AddItem(new GUIContent("(Change choice of prefabs in Rail A or B 'Variations')"), false, null, null);
                }*/
                menu.ShowAsContext();
                Repaint();
            }
            else
            {
                //Debug.Log("Right Mouse Button Clicked");

                //Vector2 mousePosition = currentEvent.mousePosition;
                //CreateButton(mousePosition);
                //currentEvent.Use(); // Consume the event so it doesn't propagate further

            }
        }


        return hit;
    }
    private GameObject UseGameObjectAsAutoFence(AutoFenceEditor ed, Event currentEvent, Ray rayPosition, GameObject go)
    {
        if (currentEvent.button == 1 && currentEvent.type == EventType.MouseDown && currentEvent.control && currentEvent.shift == false)
        {
            //-- If go is null, it's possible that the mouse is over a valid object that doesn't have a collider.
            //-- If so, do one full Scene scan to find the object nearest under the mouse
            if (go == null || go.name.Contains("errain"))
            {
                go = RaycastForGameObject(rayPosition);
                if (go != null)
                {
                    Debug.Log($"Hit Object: {go.name}\n");
                }
            }

            GenericMenu menu = new GenericMenu();
            if (go != null && go.name.Contains("errain") == false && go.GetComponent<Terrain>() == null)
            {
                //af.logComment = $"  {go.name} ";
                UseGameObjectAsLayer(menu, go, LayerSet.postLayer);
                UseGameObjectAsLayer(menu, go, LayerSet.railALayer);
                UseGameObjectAsLayer(menu, go, LayerSet.railBLayer);
                UseGameObjectAsLayer(menu, go, LayerSet.extraLayer);

                //EditorContextInfo contextInfo = new EditorContextInfo();

                menu.AddItem(
                new GUIContent(
                "(Hold Ctrl to retain layer's 'Scale' setting. Else defaults to new object's Scale",
                "By default, the transform-scaling on the new object will be used. " +
                "If you wish to retain the scaling you have set in the layer's Scale control, hold the control key while adding."),
                 false, null);
                //menu.AddSeparator("");


            }
            menu.ShowAsContext();
            // Set the context menu visibility flag
            isUseGameObjectMenuVisible = true;

            // Store the current mouse position
            //switchPosition = currentEvent.mousePosition;
            //showSwitches = true;

        }
        currentEvent.Use();
        return go;

    }
    private void UseGameObjectAsLayer(GenericMenu menu, GameObject go, LayerSet layer)
    {
        EditorContextInfo contextInfo = new EditorContextInfo(go, layer);
        contextInfo.control = Event.current.control;
        contextInfo.shift = Event.current.shift;

        menu.AddItem(new GUIContent($"Use as AutoFence  {layer.String()} "), false, sceneViewContextMethods.UseAsLayer, contextInfo);
        menu.AddSeparator("");

    }
    //-------------------------------------------
    private void CreateButton(Vector2 position)
    {
        Handles.BeginGUI();

        Rect buttonRect = new Rect(position.x, position.y, 100, 20);
        if (GUI.Button(buttonRect, "Click Me"))
        {
            Debug.Log("Got a click");
        }

        Handles.EndGUI();
    }
    private void AddTextLine(GenericMenu menu, string text)
    {
        menu.AddItem(new GUIContent(text), false, sceneViewContextMethods.Dummy, null);
    }

    private void AddEmptyLine(GenericMenu menu)
    {
        AddTextLine(menu, "\n");
        //menu.AddItem(new GUIContent(" \n"), false, sceneViewContextMethods.Dummy, null);
    }
}