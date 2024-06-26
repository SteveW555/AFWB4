﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AFWB;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;
using System;

/// <summary>
/// Used to display context menus in the SceneView, and act as a link for callbacks to invoke related methods
/// </summary>
public class EditorContextInfo
{
    public AutoFenceEditor editor;
    public int menuIndex;
    public int flag;
    public int sectionIndex;
    public LayerSet layer;
    public bool isSingle, isSequence;
    public bool resetSection;
    public Transform transform;
    public GameObject prefab;
    public int seqStepNum;
    public bool useRandomization, useVariations, useSingles;
    internal int variantIndex;
    public bool control = false, shift = false; // was the control or shift key pressed when the context menu was invoked


    private EditorContextInfo()
    {
    }
    public EditorContextInfo(GameObject go, LayerSet layer)
    {
        prefab = go;
        transform = go.transform;
        this.layer = layer;
    }

    /// <summary>Initializes a new instance of the <see cref="EditorContextInfo"/> class.</summary>
    public EditorContextInfo(AutoFenceEditor editor)
    {
        this.editor = editor;
        flag = 0;
        layer = LayerSet.None;
        variantIndex = 0;
        resetSection = false;
    }
}
//-----------------------------------------------------
namespace AFWB
{
    public class SceneViewContextMethods
    {
        AutoFenceCreator af;
        AutoFenceEditor ed;
        public const int kRailAIndex = 0, kRailBIndex = 1, kPostIndex = 2;

        public SceneViewContextMethods(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
        {
            af = autoFenceCreator;
            ed = autoFenceEditor;
        }

        public void UseAsLayer(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            GameObject prefab = info.prefab;
            LayerSet layer = info.layer;
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab is null in UseAsLayer()\n");
                return;
            }
            else
                Debug.LogWarning($"Added Prefab: {prefab.name}  as  {layer.String()}: \n");


            //-- By default (control not pressed), use the localScale of the incoming prefab
            //if(info.control == false)
            //af.SetScaleTransformForLayer(prefab.transform.localScale, layer);


            bool mainPrefabChanged = false, importAttempted = true;
            int layerIndex = 0;
            GameObject savedPrefab = ed.prefabAssignEd.AssignUserPrefab(layer, out mainPrefabChanged, prefab);
            int indexOfNewPrefab = PrefabAssignEditor.IntegrateUserPrefab(ed, layer, savedPrefab);




            /*if (layer == LayerSet.postLayer)
            {
                int prefabIndex = af.currentPost_PrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentPost_PrefabMenuIndex, PrefabTypeAFWB.postPrefab);
                af.SetPostPrefab(prefabIndex, false);
                af.SetSourceVariantGoAtIndexForLayer(0, prefabIndex, layer);
            }
            else if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
            {
                int prefabIndex = af.currentRail_PrefabIndex[layerIndex] = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentRail_PrefabMenuIndex[layerIndex], PrefabTypeAFWB.railPrefab);
                af.SetRailPrefab(prefabIndex, layer, false);
                af.SetSourceVariantGoAtIndexForLayer(0, prefabIndex, layer);
            }
            else if (layer == LayerSet.subpostLayer)
            {
                af.currentSubpost_PrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentSubpost_PrefabMenuIndex, PrefabTypeAFWB.postPrefab);
                af.SetSubpostPrefab(af.currentSubpost_PrefabIndex, false);
            }
            else if (layer == LayerSet.extraLayer)
            {
                af.currentExtra_PrefabIndex = af.ConvertMenuIndexToPrefabIndexForLayer(af.currentExtra_PrefabMenuIndex, PrefabTypeAFWB.extraPrefab);
                af.SetExtraPrefab(af.currentExtra_PrefabIndex, false);
            }*/

            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
        }

        public void ConvertToClickPoint(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            LayerSet layer = info.layer;
            if (layer == LayerSet.postLayer)
            {
                AutoFenceEditor.CreateClickPointAtPostPosition(info.transform.localPosition, af);
            }
        }
        public void RemoveClickPoint(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            LayerSet layer = info.layer;
            if (layer == LayerSet.postLayer)
            {
                af.DeleteClickPoint(info.sectionIndex);
            }
        }

        public void SetRandomizationStatus(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            LayerSet layer = info.layer;
            af.ToggleRandomizationForLayer(layer);
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
            return;
        }

        public void SetVariationStatus(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            LayerSet layer = info.layer;
            af.ToggleUseVariationsForLayer(layer);

            if (af.GetUseVariationsForLayer(layer) == true)
            {
                af.ShowVariationsForLayer(layer, true);
                //- Variations have been turned on, so if singles are off, we at least need to enable the sequencer so something happens
                if (af.singlesContainer.GetUseSinglesForLayer(layer, af) == false)
                {
                    af.SetUseSequencerForLayer(layer, true);
                }
            }
            else
                af.ShowVariationsForLayer(layer, false);

            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
            return;
        }
        public void SetSequencerStatus(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            LayerSet layer = info.layer;
            af.ToggleUseSequencerForLayer(layer);
            /*if(af.GetUseSequencerForLayer(sourceLayerList) == true)
            {
                if(af.GetUseSinglesForLayer(sourceLayerList) == false)
                {
                    af.ToggleUseSinglesForLayer(sourceLayerList);
                }
            }*/


            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
            return;
        }
        public void Dummy(object obj)
        {
            //Enables the menu item without actually doing anything. Maybe there's a better way?
        }

        //-----------------------------
        // This should be the only route through which a Single variant can be added to the singles list
        public void SetupSingle(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            int sectionIndex = info.sectionIndex;
            int variantIndex = info.variantIndex;
            LayerSet layer = info.layer;
            af.AddSinglesSourceVariantForLayerWithSectionIndex(layer, sectionIndex, variantIndex);
            af.ResetPoolForLayer(layer);
            af.ForceRebuildFromClickPoints();
        }

        //===================================================
        public void ContextMenuCallback(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            AutoFenceEditor ed = info.editor;
            int menuIndex = info.menuIndex;
            LayerSet layer = info.layer;

            if (menuIndex == -1)
                return;

            //LayerSet layerA = LayerSet.railALayer;
            LayerSet layerB = LayerSet.railBLayer;
            LayerSet layerPost = LayerSet.postLayer;

            //===========================================
            //       Convert Post to ClickPoint
            //===========================================
            if (layer == LayerSet.postLayer)
            {
                if (menuIndex == 200)
                {
                    AutoFenceEditor.CreateClickPointAtPostPosition(info.transform.localPosition, ed.af);
                }
                else if (menuIndex == 201)
                {
                    int index = ed.af.GetClickPointIndex(info.transform.localPosition);
                    if (index != -1)
                    {
                        ed.af.DeleteClickPoint(index);
                    }
                    //ed.af.ResetPoolForLayer(LayerSet.postLayer)();
                    ed.af.ForceRebuildFromClickPoints();
                }

            }

            //===========================================
            //      Reset and clear the assigned single 
            //===========================================
            if (menuIndex == 100 && info.resetSection == true)
            {
                if (layer == LayerSet.railALayer) // Rail A
                {
                    if (info.sectionIndex != -1)
                    {
                        // SourceVariant singleVariant = ed.af.FindSingleVariantWithSectionIndex(layerA, info.sectionIndex);
                        //if (singleVariant != null)
                        //ed.af.railSingleVariants[0].Remove(singleVariant);
                    }
                    if (info.flag == 2) // Not in Sequencer Mode
                    {
                        //ed.af.railSinglesIndices[0][info.sectionIndex] = -1; // -1 means ignore as single
                    }
                    else // In Sequencer Mode
                    {
                        //if (info.sectionIndex != -1)
                        //ed.af.railSinglesIndices[0][info.sectionIndex] = -1; // -1 means ignore as single
                        ed.af.seqRailASourceVarIndex[ed.currSeqRailStepIndex[0]] = 0;
                        //ed.varEd.SetSequenceVariantFromDisplaySettings1(layerA, ref ed.currSeqAStepVariant, ed.currSeqRailAStepIndex);
                        //seqStepVariant.go = af.railSourceVariants[0][seqStepVariant.sourceVariantIndex].go;
                        Debug.Log("Resetting Section");
                    }
                    ed.af.ResetPoolForLayer(layer);
                }
                if (layer == LayerSet.railBLayer)//Rail B
                {
                    if (info.sectionIndex != -1)
                    {
                        /*SourceVariant singleVariant = ed.af.FindSingleVariantWithSectionIndex(layerB, info.sectionIndex);
                        if (singleVariant != null)
                            ed.af.railSingleVariants[1].Remove(singleVariant);*/
                    }
                    if (info.flag == 2) // Not in Sequencer Mode
                    {
                        //if (info.sectionIndex != -1)
                        //ed.af.railSinglesIndices[1][info.sectionIndex] = -1; // -1 means ignore as single
                    }
                    else // In Sequencer Mode
                    {
                        //if (info.sectionIndex != -1)
                        // ed.af.railSinglesIndices[1][info.sectionIndex] = -1; // -1 means ignore as single
                        //ed.af.seqRailBSourceVarIndex[ed.currSeqRailBStepIndex] = 0;
                        //ed.varEd.SetSequenceVariantFromDisplaySettings(layerB, ref ed.currSeqBStepVariant, ed.currSeqRailBStepIndex);
                        Debug.Log("Resetting Section");
                    }
                    ed.af.ResetPoolForLayer(layer);
                }
                if (layer == LayerSet.postLayer) // Post
                {
                    if (info.sectionIndex != -1)
                    {
                        //TODO
                        /*SourceVariant singleVariant = ed.af.FindSingleVariantWithSectionIndex(layerPost, info.sectionIndex);
                        if (singleVariant != null)
                            ed.af.postSingleVariants.Remove(singleVariant);*/
                    }
                    if (info.flag == 2) // Not in Sequencer Mode
                    {
                        //if (info.sectionIndex != -1)
                        //ed.af.postSinglesIndices[info.sectionIndex] = -1; // -1 means ignore as single
                    }
                    else // In Sequencer Mode
                    {
                        //if (info.sectionIndex != -1)
                        //ed.af.postSinglesIndices[info.sectionIndex] = -1; // -1 means ignore as single
                        ed.af.seqPostSourceVarIndex[ed.currSeqPostStepIndex] = 0;
                        //ed.varEd.SetSequenceVariantFromDisplaySettings(layerPost, ref ed.currSeqPostStepVariant, ed.currSeqPostStepIndex);
                        Debug.Log("Resetting Section");
                    }
                    ed.af.ResetPoolForLayer(LayerSet.postLayer);
                }
                ed.af.ForceRebuildFromClickPoints();
                ed.Repaint();
                return;
            }

            // It's a RailA and we're using RailA Variations
            if (layer == LayerSet.railALayer && ed.af.useRailVariations[kRailAIndex] == true && menuIndex < 100)
            {
                int variantIndex = menuIndex;
                if (info.flag == 2) // Not in Sequencer Mode
                {
                    if (info.sectionIndex != -1)
                    {
                        //ed.af.railSinglesIndices[0][info.sectionIndex] = menuIndex;
                        //SourceVariant.AddVariantToSingles(layerA, info.sectionIndex, menuIndex, ed.af);
                    }
                }
                else // In Sequencer Mode
                {

                    // it's for the sequencer
                    if (info.isSingle == false)
                    {
                        //First reset any free singleVariants
                        //if (info.sectionIndex != -1)
                        //ed.af.railSinglesIndices[0][info.sectionIndex] = -1; // -1 means ignore as single
                        int railAVariantIndex = menuIndex;

                        ed.af.railASequencer.seqList[info.seqStepNum].sourceVariantIndex = variantIndex;

                    }
                }
                ed.af.ResetPoolForLayer(layer);
                ed.af.ForceRebuildFromClickPoints();
            }
            if (layer == LayerSet.railBLayer && ed.af.useRailVariations[kRailBIndex] == true && menuIndex < 100)//Rail B
            {
                if (info.flag == 2) // Not in Sequencer Mode
                {
                    if (info.sectionIndex != -1)
                    {
                        //ed.af.railSinglesIndices[1][info.sectionIndex] = menuIndex;
                        //SourceVariant.AddVariantToSingles(layerB, info.sectionIndex, menuIndex, ed.af);
                    }

                }
                else // In Sequencer Mode
                {
                    if (info.isSingle == false) // it's for the sequencer
                    {
                        //First reset any free singleVariants
                        if (info.sectionIndex != -1)
                            //ed.af.railSinglesIndices[1][info.sectionIndex] = -1; // -1 means ignore as single
                            //-----
                            //ed.af.seqRailBSourceVarIndex[ed.af.railASeqInfo.currStepIndex] = menuIndex;
                            ed.currSeqBStepVariant.sourceVariantIndex = menuIndex;
                        ed.seqEd.SetSequenceVariantFromDisplaySettings(layerB, ref ed.currSeqBStepVariant, ed.currSeqRailStepIndex[0]);
                    }
                    else// it's free
                    {
                        if (info.sectionIndex != -1)
                        {
                            //ed.af.railSinglesIndices[1][info.sectionIndex] = menuIndex;
                            //SourceVariant.AddVariantToSingles(layerB, info.sectionIndex, menuIndex, ed.af);
                        }
                    }
                }
                ed.af.ResetPoolForLayer(layer);
                ed.af.ForceRebuildFromClickPoints();
            }
            //-- Post
            if (layer == LayerSet.postLayer && ed.af.usePostVariations == true && menuIndex < 100)
            {
                if (info.flag == 2) // Not in Sequencer Mode
                {
                    if (info.sectionIndex != -1)
                    {
                        //ed.af.postSinglesIndices[info.sectionIndex] = menuIndex;
                        //SourceVariant.AddVariantToSingles(layerPost, info.sectionIndex, menuIndex, ed.af);
                    }
                }
                else // In Sequencer Mode
                {

                    // it's for the sequencer
                    if (info.isSingle == false)
                    {
                        //First reset any free singleVariants
                        //if (info.sectionIndex != -1)
                        // ed.af.postSinglesIndices[info.sectionIndex] = -1; // -1 means ignore as single
                        //-----
                        //ed.af.seqPostSourceVarIndex[ed.af.postSeqInfo.currStepIndex] = menuIndex;
                        ed.currSeqPostStepVariant.sourceVariantIndex = menuIndex;
                        ed.seqEd.SetSequenceVariantFromDisplaySettings(layerPost, ref ed.currSeqPostStepVariant, ed.currSeqPostStepIndex);
                    }
                    else // it's free
                    {
                        if (info.sectionIndex != -1)
                        {
                            //ed.af.postSinglesIndices[info.sectionIndex] = menuIndex;
                            //SourceVariant.AddVariantToSingles(layerPost, info.sectionIndex, menuIndex, ed.af);
                        }
                    }
                }
                ed.af.ResetPoolForLayer(LayerSet.postLayer);
                ed.af.ForceRebuildFromClickPoints();
            }

            ed.Repaint();
        }

        internal void ShowInPrefabsFolder(object obj)
        {
            EditorContextInfo info = (EditorContextInfo)obj;
            ed.assetFolderLinks.ShowPrefabInAssetsFolder(info.layer);
        }
    }
}
