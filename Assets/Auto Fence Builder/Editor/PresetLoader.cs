using AFWB;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AFWB
{
    public class PresetFiles
    {
        private AutoFenceCreator af;
        private AutoFenceEditor ed;
        private const int kRailAIndex = 0, kRailBIndex = 1;
        private LayerSet kRailALayer = LayerSet.railALayerSet, kRailBLayer = LayerSet.railBLayerSet, kPostLayer = LayerSet.postLayerSet;
        private LayerSet kExtraLayer = LayerSet.extraLayerSet, kSubpostLayer = LayerSet.subpostLayerSet;

        public PresetFiles(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
        {
            af = autoFenceCreator;
            ed = autoFenceEditor;
        }

        //=================== Load All Presets ===================
        
        // SourceVariants and their menus are setup automatically by an observer on the SourceVariants list
        //---------------
        /// <summary>
        /// Loads all scriptable presets via  ScriptablePresetAFWB.ReadAllPresetFiles()
        /// <para>Creates the Menu names for them</para><para >Checks for missing categories and assigns them</para>
        /// </summary>
        /// <param name="zeroContentVersion">If set to <c>true</c>, handles zero content version.</param>
        /// <returns>A list of all loaded scriptable presets.</returns>
        
        public List<ScriptablePresetAFWB> LoadAllScriptablePresets(bool zeroContentVersion)
        {
            
            //========================================================
            //      Load All Presets via ScriptablePresetAFWB
            //========================================================

            if (zeroContentVersion == false)
                ed.mainPresetList = ScriptablePresetAFWB.ReadAllPresetFiles(af, ed);
            else if (zeroContentVersion == true)
                HandleZeroContentPreset();


            //     Check if the Presets List has issues ot other details
            //==============================================================
           if (ed.mainPresetList == null || ed.mainPresetList.Count == 0)
            {
                Debug.LogWarning("Presets missing from Main AFWB Presets. No presetsEd available\n");
                Debug.LogWarning("Presets should be in Auto Fence Builder/AFWB_Prests\n");
                ed.mainPresetList = new List<ScriptablePresetAFWB>();
                for (int i = 0; i < 2; i++)
                {
                    ScriptablePresetAFWB defaultMissingPreset = ScriptableObject.CreateInstance<ScriptablePresetAFWB>();
                    defaultMissingPreset.name = "default preset (it looks like your preset folder is empty " + i;
                    defaultMissingPreset.categoryName = "default";

                    ed.mainPresetList.Add(defaultMissingPreset);
                    ed.presetMenuNames.Add(defaultMissingPreset.name);
                }
            }

            //   Check Bad Presets
            //=======================
            for (int i = 0; i < ed.mainPresetList.Count; i++)
            {
                ScriptablePresetAFWB preset = ed.mainPresetList[i];
                //-- Check for missing Category name
                if (preset.categoryName == "")
                {
                    preset.categoryName = ScriptablePresetAFWB.FindCategoryForPreset(preset, ed.af.presetSaveName, "", af);
                    Debug.Log("categoryName missing for " + preset.name + ".  Assigned to: " + preset.categoryName);
                }
                if (af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, preset.postName, $"LoadAllScriptablePresets() Preset: {preset.name} postName") == -1)
                {
                    Debug.LogWarning("Missing Post [" + preset.postName + "] is -1 in Preset: " + preset.categoryName + "/" + preset.name + "\n");
                }
                //--Check there's correct number of post sourceVariants
                if (preset.postVariants.Count < AutoFenceCreator.kMaxNumSourceVariants)
                {
                    for (int j = preset.postVariants.Count; j < AutoFenceCreator.kMaxNumSourceVariants; j++)
                    {
                        if (af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, preset.postName, $"LoadAllScriptablePresets() PostVariants Preset: {preset.name} postName") != -1)
                            preset.postVariants.Add(new SourceVariant(preset.postName));
                        else
                            preset.postVariants.Add(new SourceVariant("ABasicConcrete_Post"));
                    }
                }
            }
            ed.CreateScriptablePresetStringsForMenus(ed.mainPresetList);
            ed.GetCategoryNamesFromLoadedPresetList();
            return ed.mainPresetList;
        }

        //=================== Save Preset ===================

        /// <summary> main Preset Save: Saves the current settings as a preset with the name af.presetSaveName.</summary>
        /// <param name="forcedSave">If set to <c>true</c>, the preset will be saved even if a file with the same name already exists.</param>
        /// <param name="reloadAll">If set to <c>true</c>, all scriptable presets will be reloaded after the save.</param>
        /// <returns><c>true</c> if the preset was saved successfully; otherwise, <c>false</c>.</returns>
        /// <remarks> If control and shift are held down, the preset will be saved without asking for confirmation.
        /// <remarks> Called by DisplaySavePresetControls()  ResaveAllScriptablePresets()  GUILayout.Button("Fave")  CreateNewPreset</remarks>
        public bool SavePreset(bool forcedSave = false)
        {
            if (Event.current.control && Event.current.shift)
                forcedSave = true;

            //    Setup & Name Preset Ready to Save
            //============================================
            ScriptablePresetAFWB preset = ScriptablePresetAFWB.CreatePresetFromCurrentSettings(ed.af.presetSaveName, "", af);
            PresetCheckFixEd.CheckAndRepairSourceVariantsListsAllLayerForPreset(preset, af, warn: true);

            preset.categoryName = ScriptablePresetAFWB.FindCategoryForPreset(preset, ed.af.presetSaveName, af.categoryNames[af.categoryIndex], af);
            string filePath = ScriptablePresetAFWB.CreateSaveString(af, preset.name, preset.categoryName);
            if (filePath == "")
            {
                Debug.LogWarning("filePath was zero. Not saving");
                return false;
            }
            bool fileExists = File.Exists(filePath);

            //    Normal Save
            //========================
            if (!fileExists || forcedSave)
            {
                ScriptablePresetAFWB.SaveScriptablePreset(af, preset, filePath, true, forcedSave);
            }
            else
            {
                //    Force Overwrite
                //========================
                ed.presetSaveRename = "";
                SavePresetWindow saveWindow = ScriptableObject.CreateInstance(typeof(SavePresetWindow)) as SavePresetWindow;
                saveWindow.Init(ed, ed.af.presetSaveName, preset);
                saveWindow.minSize = new Vector2(475, 190); saveWindow.maxSize = new Vector2(475, 190);
                saveWindow.ShowUtility();
            }
            return true;
        }
        //--------------------------------
        /// <summary>
        /// Saves the current settings as a preset with the given name and folder path, for a Finished Fence
        /// </summary>
        /// <param name="finishedName"></param>
        /// <param name="finishedFolderPath"></param>
        /// <param name="afc"></param>
        /// <returns></returns>
        public static ScriptablePresetAFWB SaveFinishedPreset(string finishedName, string finishedFolderPath, AutoFenceCreator afc)
        {
            ScriptablePresetAFWB preset = ScriptablePresetAFWB.CreatePresetFromCurrentSettings(finishedName, "", afc);
            preset.categoryName = ScriptablePresetAFWB.FindCategoryForPreset(preset, afc.presetSaveName, afc.categoryNames[afc.categoryIndex], afc);
            string filePath = finishedFolderPath + "/" + "Preset-" + preset.name + ".asset";
            if (filePath == "")
            {
                Debug.LogWarning("filePath was zero. Not saving");
                return null;
            }
            AssetDatabase.CreateAsset(preset, filePath);
            return preset;
        }
        //-------------
        public void ValidateSeqStepValues()
        {
            for (int i = 0; i < AutoFenceCreator.kMaxNumSeqSteps; i++)
            {
                List<SeqItem> railASeqItems = af.railASequencer.seqList;
                SeqItem railASeqItem = railASeqItems[i];
                List<SeqItem> railBSeqItems = af.railBSequencer.seqList;
                SeqItem railBSeqItem = railBSeqItems[i];
                List<SeqItem> postSeqItems = af.postSequencer.seqList;
                SeqItem postSeqItem = postSeqItems[i];

                //Fix bad sequence with all zero svSize
                if (railASeqItem.size.x == 0 && railASeqItem.size.y == 0 && railASeqItem.size.z == 0)
                    railASeqItem.size = Vector3.one;
                if (railBSeqItem.size.x == 0 && railBSeqItem.size.y == 0 && railBSeqItem.size.z == 0)
                    railBSeqItem.size = Vector3.one;
                if (postSeqItem.size.x == 0 && postSeqItem.size.y == 0 && postSeqItem.size.z == 0)
                    postSeqItem.size = Vector3.one;
            }
            ed.seqEd.CreateOptimalSequenceForLayer(LayerSet.railALayerSet);
            ed.seqEd.CreateOptimalSequenceForLayer(LayerSet.railBLayerSet);
            ed.seqEd.CreateOptimalSequenceForLayer(LayerSet.postLayerSet);
        }

        //-------------
        // First sets each layer prefab with current****_PrefabIndex
        // Then sets the menu index for each layer to match the prefab index. They are not necessarily the same due to Unity & List sorting etc.
        public void SyncPrefabMenus()
        {
            af.SetMenuIndexFromPrefabIndexForLayer(af.currentRail_PrefabIndex[kRailAIndex], LayerSet.railALayerSet);
            af.SetMenuIndexFromPrefabIndexForLayer(af.currentRail_PrefabIndex[kRailBIndex], LayerSet.railBLayerSet);
            af.SetMenuIndexFromPrefabIndexForLayer(af.currentPost_PrefabIndex, LayerSet.postLayerSet);
            af.SetMenuIndexFromPrefabIndexForLayer(af.currentExtra_PrefabIndex, LayerSet.extraLayerSet);
            af.SetMenuIndexFromPrefabIndexForLayer(af.currentSubpost_PrefabIndex, LayerSet.subpostLayerSet);
        }
        //-------------------------------------------
        /// <summary> Loads the presets if needed, If they're already loaded and healthy, returns.
        /// Optionally force a reload even if they are healthy (Default false) </summary>
        /// <returns><c>true</c> if any changes were made to the presets; otherwise, <c>false</c>.</returns>
        public bool LoadPresets(bool force = false)
        {
            bool changed = false;

            // Check the main preset list and load all scriptable presets if necessary.
            // If the current preset index is out of range, adjust it and mark that changes were made.
            if (af.allowContentFreeUse == false)
            {
                if (force == true || ed.mainPresetList == null || ed.mainPresetList.Count < 1 || ed.mainPresetList[0] == null)
                {
                    LoadAllScriptablePresets(false);
                    changed = true;
                }
                if (af.currPresetIndex >= ed.mainPresetList.Count)
                {
                    changed = true;
                    af.currPresetIndex = ed.mainPresetList.Count - 1;
                }
                af.presetSaveName = ed.mainPresetList[af.currPresetIndex].name;
                return changed;
            }

            // If content free use is allowed, load all scriptable presets and reset all indices.
            // Then, disable content free use and mark that we're using the minimal version.
            else if (af.allowContentFreeUse == true)
            {
                if (ed.mainPresetList == null || ed.mainPresetList.Count < 1)
                    LoadAllScriptablePresets(true);

                ed.launchPresetIndex = 0;
                af.currPresetIndex = 0;
                af.currentPost_PrefabIndex = 0;
                af.currentRail_PrefabIndex[0] = 0;
                af.currentRail_PrefabIndex[1] = 0;
                af.currentSubpost_PrefabIndex = 0;
                af.currentExtra_PrefabIndex = 0;

                af.allowContentFreeUse = false; //the new directories have been created, so we can access them directly now
                af.usingMinimalVersion = true;
            }
            return changed;
        }


        //-------------------------
        private void HandleZeroContentPreset()
        {
            ed.mainPresetList = ScriptablePresetAFWB.ReadZeroContentPresetFiles(af);
            // CreateMergedPrefabs new presetsEd folders if necessary
            string mainPresetsFolderPath = ed.af.currAutoFenceBuilderDir + "/AFWB_Presets";
            bool folderExists = AssetDatabase.IsValidFolder(mainPresetsFolderPath);
            if (folderExists == false)
            {
                string guid = AssetDatabase.CreateFolder(ed.af.currAutoFenceBuilderDir, "AFWB_Presets");
                mainPresetsFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                if (mainPresetsFolderPath == "")
                {
                    Debug.LogWarning("Couldn't create AFWB_Presets folder \n");
                }
                else
                {
                    folderExists = true;
                }
            }

            if (folderExists && ed.mainPresetList != null && ed.mainPresetList.Count > 0)
            {
                bool saved = SaveZeroContentPreset(ed.mainPresetList[0], ed.af);
                if (saved == false)
                    Debug.LogWarning("Problem saving Zero Content Preset \n");
            }
            else
            {
                if (folderExists == false)
                    Debug.LogWarning("Couldn't create AFWB_Presets folder \n");
                if (ed.mainPresetList == null)
                    Debug.LogWarning("ed.scriptablePresetList was null \n");
                if (ed.mainPresetList.Count == 0)
                    Debug.LogWarning("ed.scriptablePresetList.Count was 0 \n");
            }
        }

        //------------------
        public void ResaveAllScriptablePresets()
        {
            for (int i = 0; i < ed.mainPresetList.Count; i++)
            {
                ScriptablePresetAFWB preset = ed.mainPresetList[i];

                ed.af.presetSaveName = preset.name;

                af.currPresetIndex = i;

                SetupPreset(af.currPresetIndex);

                bool resave = false;
                if (preset.railAName.Contains("_Panel_Rail") && preset.slopeModeRailA == SlopeMode.slope)
                {
                    af.slopeMode[AutoFenceCreator.kRailALayerInt] = SlopeMode.shear;
                    resave = true;
                }
                if (preset.railBName.Contains("_Panel_Rail") && preset.slopeModeRailB == SlopeMode.slope)
                {
                    af.slopeMode[AutoFenceCreator.kRailBLayerInt] = SlopeMode.shear;
                    resave = true;
                }

                if (resave)
                {
                    Debug.Log(preset.name + "\n");
                    SavePreset(true);
                }
            }
        }

        //------------------
        /*public void ResaveScriptablePreset()
        {
            SavePreset(ScriptablePresetAFWB preset, AutoFenceCreator af, bool forcedSave, bool reloadAll)
        }*/

        //--------------------
        private bool CheckCurrentSettingsPrefabsAssignment()
        {
            if (af.currentPost_PrefabIndex == -1)
            {
                Debug.LogWarning("currentPost_PrefabIndex is -1  " + ed.currPreset.name + "\n");
                return false;
            }
            if (af.currentRail_PrefabIndex[0] == -1)
            {
                Debug.LogWarning("currentRail_PrefabIndex[0] is -1 " + ed.currPreset.name + "\n");
                return false;
            }
            if (af.currentRail_PrefabIndex[1] == -1)
            {
                Debug.LogWarning("currentRail_PrefabIndex[1] is -1 " + ed.currPreset.name + "\n");
                return false;
            }
            if (af.currentSubpost_PrefabIndex == -1)
            {
                Debug.LogWarning("currentSubpost_PrefabIndex is -1  " + ed.currPreset.name + "\n");
                return false;
            }
            if (af.currentExtra_PrefabIndex == -1)
            {
                Debug.LogWarning("currentExtra_PrefabIndex is -1  in " + ed.currPreset.name + "\n");
                return false;
            }

            //Check there's correct number of post sourceVariants
            if (af.postSourceVariants.Count < AutoFenceCreator.kMaxNumSourceVariants)
            {
                for (int j = af.postSourceVariants.Count; j < AutoFenceCreator.kMaxNumSourceVariants; j++)
                {
                    if (af.currentPost_PrefabIndex != -1)
                    {
                        af.postSourceVariants.Add(new SourceVariant(af.postPrefabs[af.currentPost_PrefabIndex].name));
                        Debug.Log("Fixed current postVariants");
                    }
                    else
                        af.postSourceVariants.Add(new SourceVariant("ABasicConcrete_Post"));
                }
            }
            return true;
        }

        //-------------
        /// <summary>
        /// Checks the current preset for any bad values, nulls, or obsolete presets missing required parameters.
        /// If any invalid prefab indices are found, they are reset to 0.
        /// </summary>

        public void CheckForBadValuesInPreset()
        {
            //-- These were broken in old presets
            af.railASeeds.layer = LayerSet.railALayerSet;
            af.railBSeeds.layer = LayerSet.railBLayerSet;
            af.postAndGlobalSeeds.layer = LayerSet.postLayerSet;
            af.extraSeeds.layer = LayerSet.extraLayerSet;
            af.subpostSeeds.layer = LayerSet.subpostLayerSet;

            if (af.numStackedRails[0] < 1 )
                af.numStackedRails[0] = 1;
            if (af.numStackedRails[1] < 1)
                af.numStackedRails[1] = 1;

            //-- Check for Seeds problems from old presets
            af.ValidateAllSeeds();
            ValidateSeqStepValues();

            // Probably not necessary as they were checked durinmg the preset load, but leave in case of changes
            PresetCheckFixEd.CheckAndRepairSourceVariantsListsAllLayers(ed.currPreset);
        }

        //---------------
        /// <summary>
        /// Sets up the preset with the given index. The parameters are set by calling  ScriptablePreserAFWB.BuildFromPreset()
        /// </summary>
        /// <param name="presetIndex">The index of the preset to set up.</param>
        /// <param name="forceRebuild">(Optional) If set to <c>true</c>, forces a rebuild of the preset.</param>

        public ScriptablePresetAFWB SetupPreset(ScriptablePresetAFWB preset, bool forceRebuild = false)
        {


            //--  This copies all the parameters from the preset into the af current settings. No checks or modifications
            //seedsAF = af.railASeeds.af;
            ed.currPreset = preset;
            ed.currPreset.BuildFromPreset(af);

            af.CheckPrefabIndexForAllLayers();
            //-- Deal with presets with bad, values, nulls, or obsolete presets missing required parameters
            CheckForBadValuesInPreset();

            //-- Sets the main prefab for each layer and syncs the prefab menu iondex for that layer
            SyncPrefabMenus();
            ed.af.presetSaveName = ed.currPreset.name;
            af.ex.UpdateExtrasFromExtraVariantsStruct(ed.currPreset.extraVarsStruct);
            af.singlesContainer.ResetAllRailSingles(af);
            af.ResetAllPools();
            SetupParametersAfterPresetSelect();
            ed.af.userPrefabPost = null;
            ed.af.userPrefabRail[kRailAIndex] = null;
            ed.af.userPrefabExtra = null;

            ed.useBreakpoint = true;

            return ed.currPreset;
        }
        
       
        public ScriptablePresetAFWB SetupPreset(int presetIndex, bool forceRebuild = false)
        {
            if (presetIndex >= ed.mainPresetList.Count)
            {
                Debug.LogWarning("Trying to access presetIndex beyond count. " + presetIndex + " Aborting");
                return null;
            }
            af.currPresetIndex = presetIndex; //in case it's called from code
            ed.currPreset = GetPresetFromPresetsList(af.currPresetIndex);
            SetupPreset(ed.currPreset, forceRebuild);

            return ed.currPreset;
        }

        //---------------
        /// <summary>
        /// Get the preset from the main preset list at the given index.
        /// </summary>
        /// <param name="presetIndex"></param>
        /// <returns>ScriptablePresetAFWB</returns>
        public ScriptablePresetAFWB GetPresetFromPresetsList(int presetIndex)
        {
            ScriptablePresetAFWB preset = ed.mainPresetList[presetIndex];
            return preset;
        }
        //---------------
        /// <summary>
        /// Gets the preset by name from the main preset list.
        /// </summary>
        /// <param name="presetName">The name of the preset to find.</param>
        /// <returns>The ScriptablePresetAFWB with the specified name, or null if not found.</returns>
        public ScriptablePresetAFWB GetPresetByName(string presetName)
        {
            // Find preset in ed.mainPresetList with presetName
            ScriptablePresetAFWB preset = ed.mainPresetList.Find(x => x.name == presetName);

            // Warn if preset not found
            if (preset == null)
                Debug.LogWarning($"Preset with name '{presetName}' not found\n");

            return preset;
        }


        //---------------
        /// <summary>
        /// Called from SetupPreset() to set up the parameters after a preset has been selected.
        /// </summary>
        /// <remarks>
        /// 1. Maps parameters that don't directly map to an integer index, such as strings or non-linear values.
        /// 2. Populates SeqItems with the correct GameObjects (GOs) using the sourceVariantIndex.
        /// 3. Handles different Auto Fence & Wall Builder (AFWB) version features saved in presets, including adjusting the number of sourceVariants.
        /// </remarks>
        public void SetupParametersAfterPresetSelect()
        {
            // Map parameters that don't directly map to an integer index, e.g., strings or non-linear values
            af.currentExtra_PrefabMenuIndex = af.ConvertExtraPrefabIndexToMenuIndex(af.currentExtra_PrefabIndex);
            af.categoryIndex = af.categoryNames.IndexOf(ed.currPreset.categoryName);
            af.quantizeRotIndexRailA = af.GetMenuIndexFromQuantizedRotAngle(af.quantizeRotAngleRailA);
            af.quantizeRotIndexRailB = af.GetMenuIndexFromQuantizedRotAngle(af.quantizeRotAngleRailB);
            af.quantizeRotIndexPost = af.GetMenuIndexFromQuantizedRotAngle(af.quantizeRotAnglePost);
            af.quantizeRotIndexSubpost = af.GetMenuIndexFromQuantizedRotAngle(af.quantizeRotAngleSubpost);

            //-- Populate the SeqItems with the correct GOs by using the sourceVariantIndex into the list of Variants
            //-- This is needed because the SeqItems are saved in the preset, but the GOs are not
            List<SourceVariant> railAVariants = af.GetSourceVariantsForLayer(kRailALayer);
            int numRailASeqSteps = af.GetNumSeqStepsForLayer(kRailALayer);
            List<SeqItem> railASeq = af.GetSequenceForLayer(kRailALayer);
            for (int i = 0; i < numRailASeqSteps; i++)
            {
                SeqItem seqVar = railASeq[i];
                int goIndex = seqVar.sourceVariantIndex;
            }

            //-- Deal with different AFWB version features saved in presetsEd
            //-- Do we need to increase/decrease the number of sourceVariants, as it used to be fixed at 1 + 4
            int maxIndex = -1;
            for (int i = 0; i < numRailASeqSteps; i++)
            {
                SeqItem seqVar = railASeq[i];
                int variantIndex = seqVar.sourceVariantIndex;
                if (variantIndex > maxIndex)
                    maxIndex = variantIndex;
            }
            //Debug.Log($"maxIndex {maxIndex} numRailASeqSteps {numRailASeqSteps}\n");
            //-- Set the num variations to the maxIndex, ensuring it's at least
            af.SetNumVariationsInUseForLayer(kRailALayer, (maxIndex > 2) ? maxIndex : 2);
        }


        //--------------------------------
        //this saves from an existing ScriptablePresetAFWB
        public bool SaveZeroContentPreset(ScriptablePresetAFWB preset, AutoFenceCreator afc)
        {
            ScriptablePresetAFWB presetCopy = ScriptablePresetAFWB.CreatePresetFromCurrentSettings(preset.name, preset.categoryName, afc);
            string filePath = ScriptablePresetAFWB.CreateSaveString(af, presetCopy.name, presetCopy.categoryName);
            if (filePath == "")
            {
                Debug.LogWarning("filePath was zero. Not saving");
                return false;
            }
            ScriptablePresetAFWB.SaveScriptablePreset(af, presetCopy, filePath, true, true);
            return true;
        }

        public void PrintAllPresets()
        {
            //create a copy of ed.mainPresetList sorted alphabetically
            List<ScriptablePresetAFWB> sortedList = new List<ScriptablePresetAFWB>(ed.mainPresetList);
            sortedList.Sort((x, y) => string.Compare(x.name, y.name));

            for (int i = 0; i < ed.mainPresetList.Count; i++)
            {
                ScriptablePresetAFWB preset = sortedList[i];
                Debug.Log($"{preset.name} / {preset.categoryName} \n");
            }
        }
    }
}