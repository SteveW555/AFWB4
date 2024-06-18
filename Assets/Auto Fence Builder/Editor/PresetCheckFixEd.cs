using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AFWB
{
    /// <summary> Utilities for fixing bad presets  </summary>
    /// <remarks> Needs access to ScriptablePresetAFWB so needs to be in the Editor folder 
    /// <para> See also PresetUtilitiesAF in Scripts folder for access by AutoFenceCreator </para>
    /// </remarks>
    public static class PresetCheckFixEd
    {
        //---------------------------
        /// <summary>Checks all aspects of a SourceVariant list for a Preset's Layer.  Where there are issues a new SourceVariant
        /// is created, or any null Gos are replaced with the main prefab. </summary>
        /// <param name="sourceVariants">The list of SourceVariants to check and repair.</param>
        /// <param name="layer">The LayerSet that the SourceVariants belong to.</param>
        /// <param name="preset">The ScriptablePresetAFWB that the SourceVariants are part of.</param>
        /// <param name="af">The AutoFenceCreator gizmoSingletonInstance that is currently in use.</param>
        /// <param name="mainGo">Optional. The main GameObject to use if a SourceVariant needs to be created or replaced. If not provided, the method will use the currently assigned Main prefab for the layer.</param>
        /// <param name="warn">Optional. If set to true, the method will log warnings when it encounters issues. Defaults to true.</param>
        /// <remarks>
        /// Optionally pass in a main go. By default it will use the currently assigned Main prefab for the layer,
        /// but in cases where the main prefab is not yet assigned, (during loading of a preset) you can pass in a go to use instead.
        /// </remarks>
        /// <returns>Returns the number of sourceVariants fixed.</returns>
        public static int CheckAndRepairSourceVariantsList(List<SourceVariant> sourceVariants, LayerSet layer, ScriptablePresetAFWB preset, AutoFenceCreator af, GameObject mainGo = null, bool warn = true)
        {
            if (mainGo == null)
            {
                mainGo = af.GetMainPrefabForLayer(layer);
                if (mainGo == null)
                {
                    GameObject firstNonNull = af.GetPrefabsForLayer(layer).FirstOrDefault(go => go != null);
                    mainGo = firstNonNull;
                    if (mainGo == null)
                    {
                        if (warn)
                            Debug.LogWarning($"No prefab for {layer} in the prefabs List.  Cannot fix SourceVariants\n");
                        return 0;
                    }
                }
            }
            string mainGoName = mainGo.name;
            int numFixed = 0;

            // Ensure the list is initialized
            if (sourceVariants == null || sourceVariants.Count < 1)
            {
                if (warn)
                {
                    string message = sourceVariants == null ? "is null" : "was Empty";
                    Debug.LogWarning($"SourceVariant list for {layer} {message}.  Fixing\n");
                }
                sourceVariants = SourceVariant.CreateInitialisedSourceVariantList(mainGo, AutoFenceCreator.kMaxNumSourceVariants);
                numFixed = AutoFenceCreator.kMaxNumSourceVariants;
            }

            // Check and fix the first sourceVariant
            if (sourceVariants[0] == null || sourceVariants[0].Go != mainGo)
            {
                numFixed += FixFirstSourceVariantForLayer(layer, sourceVariants, preset, mainGo, mainGoName, warn);
            }
            // Check and fix any nulls in the list
            numFixed += FixNullSourceVariantsForLayer(sourceVariants, preset, mainGo, layer, warn);
            if (numFixed > 0)
                Debug.Log($"{layer.String(false)}  -  {preset.name}    numFixed SourceVariants = {numFixed}  ---------------------------------------------------------\n");

            if (numFixed > 0)
            {
                EditorUtility.SetDirty(preset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return numFixed;
        }

        public static int FixNullSourceVariantsForLayer(List<SourceVariant> sourceVariants, ScriptablePresetAFWB preset, GameObject mainGo, LayerSet layer, bool warn)
        {
            int fixes = 0;
            for (int i = 0; i < sourceVariants.Count; i++)
            {
                if (sourceVariants[i] == null || sourceVariants[i].Go == null)
                {
                    sourceVariants[i] = new SourceVariant(mainGo);
                    if (warn)
                    {
                        Debug.LogWarning($"{preset.name}:   SourceVariant [{i}] for {layer} was null. Setting To Main\n");
                    }
                    fixes++;
                }
            }
            return fixes;
        }

        //----------------
        // A wrapper for above that checks the existing List for that layer if none is passed in
        public static int CheckAndRepairSourceVariantsListForLayer(LayerSet layer, ScriptablePresetAFWB preset, bool warn = true)
        {
            //  We don't want to warn here as we are already chececking for probelms
            //List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer, warn: false);
            //int numFixed = CheckAndRepairSourceVariantsList(sourceVariants, layer,  preset, warn);
            //return numFixed;

            return 0;
        }

        //------------------
        public static int CheckAndRepairSourceVariantsListsAllLayers(ScriptablePresetAFWB preset, bool warn = true)
        {
            int numFixed = 0;
            numFixed += CheckAndRepairSourceVariantsListForLayer(LayerSet.postLayerSet, preset, warn);
            numFixed += CheckAndRepairSourceVariantsListForLayer(LayerSet.railALayerSet, preset, warn);
            numFixed += CheckAndRepairSourceVariantsListForLayer(LayerSet.railBLayerSet, preset, warn);
            //numFixed += CheckAndRepairSourceVariantsListForLayer(LayerSet.subpostLayerSet, warn); //for v4.1
            return numFixed;
        }
        //---------------------------------
        public static int CheckAndRepairSourceVariantsListsAllLayerForPreset(ScriptablePresetAFWB preset, AutoFenceCreator af, bool warn = true)
        {
            //af.ClearConsole();
            //Debug.Log($"  ------  Preset:     {preset.name}         CheckAndRepairSourceVariantsListsAllLayerForPreset  -------\n");
            int numFixed = 0;

            //     Post
            //===============
            List<SourceVariant> postSourceVariants = preset.postVariants;
            GameObject mainPostGo = GetMainPrefabForLayerForPreset(LayerSet.postLayerSet, af, preset);
            numFixed += PresetCheckFixEd.CheckAndRepairSourceVariantsList(postSourceVariants, LayerSet.postLayerSet, preset, af, mainPostGo, warn);

            //     Rail A
            //===============
            List<SourceVariant> railASourceVariants = preset.railAVariants;
            GameObject mainRailAGo = GetMainPrefabForLayerForPreset(LayerSet.railALayerSet, af, preset);
            numFixed += PresetCheckFixEd.CheckAndRepairSourceVariantsList(railASourceVariants, LayerSet.railALayerSet, preset, af, mainRailAGo, warn);

            //     Rail B
            //===============
            List<SourceVariant> railBSourceVariants = preset.railBVariants;
            GameObject mainRailBGo = GetMainPrefabForLayerForPreset(LayerSet.railBLayerSet, af, preset);
            numFixed += PresetCheckFixEd.CheckAndRepairSourceVariantsList(railBSourceVariants, LayerSet.railBLayerSet, preset, af, mainRailBGo, warn);

            // for v4.1
            /* List<SourceVariant> subpostSourceVariants = preset.subpostVariants;
            GameObject mainSubpostGo = GetMainPrefabForLayerForPreset(LayerSet.subpostLayerSet, af, preset);
            af.CheckAndRepairSourceVariantsListForLayer(subpostSourceVariants, LayerSet.subpostLayerSet, true, mainSubpostGo);*/
            return numFixed;
        }
        public static GameObject GetMainPrefabForLayerForPreset(LayerSet layer, AutoFenceCreator af, ScriptablePresetAFWB preset)
        {
            GameObject mainForPreset = null;
            string msg = preset.categoryName + "/" + preset.name;
            if (layer == LayerSet.postLayerSet)
                mainForPreset = af.FindPrefabByName(layer, preset.postName, true, true, msg);
            else if (layer == LayerSet.railALayerSet)
                mainForPreset = af.FindPrefabByName(layer, preset.railAName, true, true, msg);
            else if (layer == LayerSet.railBLayerSet)
                mainForPreset = af.FindPrefabByName(layer, preset.railBName, true, true, msg);
            else if (layer == LayerSet.subpostLayerSet)
                mainForPreset = af.FindPrefabByName(layer, preset.subpostName, true, true, msg);
            else if (layer == LayerSet.extraLayerSet)
                mainForPreset = af.FindPrefabByName(layer, preset.extraName, true, true, msg);

            if (mainForPreset == null)
            {
                //Debug.LogWarning("Couldn't find prefab for layer: " + preset.postName + "   " + layer.ToString() + "  in preset: " + msg + "\n");
                af.FixAndWarnBadPrefab(layer, -1);
            }

            return mainForPreset;
        }
        //-------------------
        /// <summary>
        /// If the first source variant is null, it is created with the main GameObject.
        /// If it's not null but set to the wrong variant, it is set to the main GameObject.
        /// If it's not null, but the Go is null or different, it is set to the main GameObject.
        ///
        /// <param name="layer">The layer set being processed.</param>
        /// <param name="sourceVariants">The list of source variants.</param>
        /// <param name="preset">The scriptable preset associated with the layer.</param>
        /// <param name="mainGo">The main GameObject to ensure as the first source variant.</param>
        /// <param name="mainGoName">The name of the main GameObject.</param>
        /// <param name="warn">Whether to log warnings if the first source variant is null or does not match the main GameObject.</param>
        /// <returns>The number of fixes applied.</returns>
        public static int FixFirstSourceVariantForLayer(LayerSet layer, List<SourceVariant> sourceVariants, ScriptablePresetAFWB preset, GameObject mainGo, string mainGoName, bool warn)
        {
            int fixes = 0;
            SourceVariant firstSourceVariant = sourceVariants[0];

            if (firstSourceVariant == null)
            {
                sourceVariants[0] = new SourceVariant(mainGo);
                if (warn)
                {
                    Debug.LogWarning($"{layer.String()}: First sourceVariant was null. Setting to Main {mainGoName}    Preset: {preset.name} \n");
                }
                fixes++;
            }
            else if (firstSourceVariant.Go == null)
            {
                firstSourceVariant.Go = mainGo;
                firstSourceVariant.Go.name = firstSourceVariant.Go.name;
                if (warn)
                {
                    Debug.LogWarning($"{layer.String()}: First sourceVariant is null. Setting to Main {mainGoName}    Preset: {preset.name}\n");
                }
                fixes++;
            }
            else if (firstSourceVariant.Go != null && firstSourceVariant.Go.name != mainGo.name)
            {
                firstSourceVariant.Go = mainGo;
                if (warn)
                {
                    Debug.LogWarning($"{layer.String()}: First sourceVariant: {firstSourceVariant.Go.name} " +
                        $"is not the same as the main prefab: {mainGoName}. Setting to Main     Preset: {preset.name}\n");

                    //DebugUtilitiesTCT.LogStackTrace();
                }
                firstSourceVariant.Go.name = firstSourceVariant.Go.name;
                fixes++;
            }

            if (mainGo.name != mainGoName)
            {
                Debug.LogWarning($"Conflict in SourceVariant GO and name.   " +
                    $"FixFirstSourceVariantForLayer: mainGo.name = {mainGo.name}  mainGoName = {mainGoName}     Preset: {preset.name}\n");
            }
            return fixes;
        }
    }
}