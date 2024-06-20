using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

using System.Runtime.CompilerServices;
using System.Linq;
using System.ComponentModel;
using static UnityEditor.Experimental.GraphView.GraphView;
using System;
using System.Reflection;

/*
 * For Prefabs & Source Variants, and all the menus that interact with them
 */

namespace AFWB
{
    public partial class AutoFenceCreator
    {
        //==================================================================================
        //                                  Main Prefabs
        //==================================================================================

        //-- These have a one-to-one correspondence with the main prefab lists and are set during Load or a refresh
        public List<string> GetPrefabNamesForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                return railMenuNames;
            else if (layer == LayerSet.postLayer || layer == LayerSet.subpostLayer)
                return postMenuNames;
            else if (layer == LayerSet.extraLayer)
                return extraMenuNames;
            return null;
        }

        //-----------------------
        public int GetMainPrefabMenuIndexForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return currentRail_PrefabMenuIndex[kRailALayerInt];
            if (layer == LayerSet.railBLayer)
                return currentRail_PrefabMenuIndex[kRailBLayerInt];
            else if ((layer == LayerSet.postLayer))
                return currentPost_PrefabMenuIndex;
            else if ((layer == LayerSet.subpostLayer))
                return currentSubpost_PrefabMenuIndex;
            else if (layer == LayerSet.extraLayer)
                return currentExtra_PrefabMenuIndex;
            return 0;
        }
        public void SetMainPrefabMenuIndexForLayer(LayerSet layer, int index)
        {
            if (layer == LayerSet.railALayer)
                currentRail_PrefabMenuIndex[kRailALayerInt] = index;
            if (layer == LayerSet.railBLayer)
                currentRail_PrefabMenuIndex[kRailBLayerInt] = index;
            else if ((layer == LayerSet.postLayer))
                currentPost_PrefabMenuIndex = index;
            else if ((layer == LayerSet.subpostLayer))
                currentSubpost_PrefabMenuIndex = index;
            else if (layer == LayerSet.extraLayer)
                currentExtra_PrefabMenuIndex = index;
        }

        /// <summary>Assumes the full name with _Post suffix etc.</summary>
        public PrefabTypeAFWB GetPrefabTypeByName(string name)
        {
            if (name.EndsWith("_Post"))
                return PrefabTypeAFWB.postPrefab;
            else if (name.EndsWith("_Rail"))
                return PrefabTypeAFWB.railPrefab;
            else if (name.EndsWith("_Extra"))
                return PrefabTypeAFWB.extraPrefab;

            return PrefabTypeAFWB.postPrefab;
        }

        //------------------------------------------
        public GameObject GetPrefabByName(string name)
        {
            PrefabTypeAFWB prefabTypeAFWB = GetPrefabTypeByName(name);
            int prefabIndex = FindPrefabIndexByNameForLayer(prefabTypeAFWB, name, "GetPrefabByName()");
            if (prefabIndex == -1)
                return null;

            if (prefabTypeAFWB == PrefabTypeAFWB.postPrefab)
                return postPrefabs[prefabIndex];
            else if (prefabTypeAFWB == PrefabTypeAFWB.railPrefab)
                return railPrefabs[prefabIndex];
            else if (prefabTypeAFWB == PrefabTypeAFWB.extraPrefab)
                return extraPrefabs[prefabIndex];
            return null;
        }
        //------------------------------------------
        public int GetPrefabIndexForLayerByName(LayerSet layer, string name)
        {
            PrefabTypeAFWB prefabTypeAFWB = layer.ToPrefabType();
            int prefabIndex = FindPrefabIndexByNameForLayer(prefabTypeAFWB, name, "GetPrefabIndexForLayerByName()");
            return prefabIndex;
        }

        //------------------------------------
        //return the index in the PREFAB list of the current prefab
        public int GetCurrentPrefabIndexForLayer(LayerSet layer)
        {
            int currentPrefabIndex = 0;
            string layerString = GetLayerNameAsString(layer);
            bool valid = false;
            string msg = "currentPrefabIndex";

            if (layer == LayerSet.railALayer)
            {
                currentPrefabIndex = currentRail_PrefabIndex[kRailALayerInt];
                valid = CheckPrefabAtIndexForLayer(currentPrefabIndex, LayerSet.railALayer, false, msg);
            }
            else if (layer == LayerSet.railBLayer)
            {
                currentPrefabIndex = currentRail_PrefabIndex[kRailBLayerInt];
                valid = CheckPrefabAtIndexForLayer(currentPrefabIndex, LayerSet.railBLayer, false, msg);
            }
            else if (layer == LayerSet.postLayer)
            {
                currentPrefabIndex = currentPost_PrefabIndex;
                valid = CheckPrefabAtIndexForLayer(currentPrefabIndex, LayerSet.postLayer, false, msg);
            }
            else if (layer == LayerSet.subpostLayer)
            {
                currentPrefabIndex = currentSubpost_PrefabIndex;
                valid = CheckPrefabAtIndexForLayer(currentPrefabIndex, LayerSet.subpostLayer, false, msg);
            }
            else if (layer == LayerSet.extraLayer)
            {
                currentPrefabIndex = currentExtra_PrefabIndex;
                valid = CheckPrefabAtIndexForLayer(currentPrefabIndex, LayerSet.extraLayer, false, msg);
            }

            return currentPrefabIndex;
        }

        /// <summary>
        /// Sets a new prefab index for the current layer. For Posts & Rails we also need to update the SourceVariants
        /// Checks the validity of the index and sets to 0 if bad
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        /// <returns> The index in case we had to correct it </returns>
        public int SetCurrentPrefabIndexForLayer(int index, LayerSet layer)
        {
            //List<SourceVariant> sv = GetSourceVariantsForLayer(layer);

            if (CheckPrefabAtIndexForLayer(index, layer) == false)
                index = 0;

            if (layer == LayerSet.railALayer)
            {
                currentRail_PrefabIndex[kRailALayerInt] = index;
                SetFirstSourceVariantToMainForLayer(layer);

            }
            else if (layer == LayerSet.railBLayer)
            {
                currentRail_PrefabIndex[kRailBLayerInt] = index;
                SetFirstSourceVariantToMainForLayer(layer);
            }
            else if (layer == LayerSet.postLayer)
            {
                currentPost_PrefabIndex = index;
                SetFirstSourceVariantToMainForLayer(layer);
            }
            else if (layer == LayerSet.subpostLayer)
                currentSubpost_PrefabIndex = index;
            else if (layer == LayerSet.extraLayer)
                currentExtra_PrefabIndex = index;

            GameObject prefab = GetPrefabAtIndexForLayer(index, layer);

            SetIsUserPrefab(prefab, layer);

            if (prefab.name.EndsWith("Panel_Rail") == true || prefab.name.EndsWith("Panel") == true)
            {
                slopeMode[layer.Int()] = SlopeMode.shear;//-- Always change to 'shear' for panel fences
            }
            return index;
        }
        public bool SetIsUserPrefab(GameObject prefab, LayerSet layer)
        {
            bool isUserPrefab = false;
            if (prefab.name.StartsWith("[User]") || prefab.name.StartsWith("[U]"))
                isUserPrefab = true;

            if (layer == LayerSet.railALayer)
                useCustomRail[0] = isUserPrefab;
            else if (layer == LayerSet.railALayer)
                useCustomRail[1] = isUserPrefab;
            else if (layer == LayerSet.postLayer)
                useCustomPost = isUserPrefab;
            else if (layer == LayerSet.extraLayer)
                useCustomExtra = isUserPrefab;

            return isUserPrefab;
        }

        //-----------------------
        /// <summary>Checks if the prefabIndex is within the bounds of the prefab list for this layer
        /// Checks if the prefab at that index is null</summary>
        /// <returns> true if index and prefabis ok </returns>
        /// <remarks>Logs a warning if the index is out of bounds. Optionally sets a bad index to zero if resetBad is true.</remarks>
        public bool CheckPrefabAtIndexForLayer(int prefabIndex, LayerSet layer, bool resetBad = false, string msg = "")
        {
            string layerString = GetLayerNameAsString(layer);
            int prefabCount = GetNumPrefabsForLayer(layer);
            string errorStr = "";

            if (prefabIndex > prefabCount)
                errorStr = $"prefabIndex {prefabIndex} for {layer.String()} is out of bounds {prefabCount}. {msg}\n";
            if (prefabIndex == -1)
                errorStr = $"prefabIndex for {layer.String()} is -1. {msg}\n";
            if ((errorStr != "" && resetBad))
                SetCurrentPrefabIndexForLayer(0, layer);
            if (errorStr != "")
            {
                Debug.LogWarning(errorStr);
                return false;
            }

            if (GetPrefabAtIndexForLayer(prefabIndex, layer) == null)
            {
                Debug.LogWarning($"CheckPrefabAtIndexForLayer()  {layerString} prefab at index {prefabIndex} was null\n");
                return false;
            }

            return true;
        }
        //----------------------------
        /// <summary>
        /// Checks the prefab index for all layers and returns true if all checks pass, otherwise false.
        /// </summary>
        /// <returns>True if all layers have valid prefab indexes, false otherwise.</returns>
        public bool CheckPrefabIndexForAllLayers()
        {
            //-- Although this looks strange checking the already assigned index, 
            //-- it it hasn't been accessed yet, such as after applying a preset this is the opportunity to fix it.
            bool allLayersValid = CheckPrefabAtIndexForLayer(currentRail_PrefabIndex[0], LayerSet.railALayer, resetBad: true) &&
                                  CheckPrefabAtIndexForLayer(currentRail_PrefabIndex[1], LayerSet.railALayer, resetBad: true) &&
                                  CheckPrefabAtIndexForLayer(currentPost_PrefabIndex, LayerSet.postLayer, resetBad: true) &&
                                  CheckPrefabAtIndexForLayer(currentSubpost_PrefabIndex, LayerSet.subpostLayer, resetBad: true) &&
                                  CheckPrefabAtIndexForLayer(currentExtra_PrefabIndex, LayerSet.extraLayer, resetBad: true);

            return allLayersValid;
        }


        //-----------------------
        public GameObject GetMainPrefabForLayer(LayerSet layer)
        {
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            int currPrefabIndexForLayer = GetCurrentPrefabIndexForLayer(layer);
            GameObject currPrefab = GetPrefabAtIndexForLayer(currPrefabIndexForLayer, layer);

            //-Validate the prefab
            if (currPrefab == null)
            {
                GameObject firstNonNull = GetPrefabsForLayer(layer).FirstOrDefault(go => go != null);
                currPrefab = firstNonNull;
                if (currPrefab == null)
                {
                    Debug.LogWarning($"No prefab for {layer} in the prefabs List.  Cannot fix SourceVariants\n");
                    return null;
                }
            }

            return currPrefab;
        }

        //-----------------------
        public string GetMainPrefabNameForLayer(LayerSet layer, bool strip = true)
        {
            GameObject prefab = GetMainPrefabForLayer(layer);
            string name = prefab.name;
            if (strip)
                name = StripLayerTypeFromNameStatic(layer, prefab.name);
            return name;
        }
        //-----------------------
        public Material GetMainMaterialForLayer(LayerSet layer)
        {
            GameObject prefab = GetMainPrefabForLayer(layer);
            Material mat = null;
            if (prefab != null)
            {
                Renderer renderer = GetMainRendererForLayer(layer);
                if (renderer != null)
                {
                    mat = renderer.sharedMaterial;
                    if (mat == null)
                        Debug.LogWarning($"GetMainMaterialForLayer()  {layer.String()} prefab {prefab.name} had no material\n");
                }

            }
            return mat;
        }
        //-----------------------
        public Renderer GetMainRendererForLayer(LayerSet layer)
        {
            GameObject prefab = GetMainPrefabForLayer(layer);
            Renderer renderer = null;
            renderer = prefab.GetComponent<Renderer>();
            if (renderer == null)
                Debug.LogWarning($"GetMainRendererForLayer()  {layer.String()} prefab {prefab.name} had no renderer\n");
            return renderer;
        }

        //-----------------------
        /// <summary>
        /// -- Gets the whole prefab list for this layer e.g. railPrefabs, postPrefabs. 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="warn"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        /// <remarks>Logs a warning if the prefab list is null or empty. Optionally logs a warning if the first prefab is null.
        /// When loading prefabs, we expect the list to be empty initially, so don't warn</remarks>
        public List<GameObject> GetPrefabsForLayer(LayerSet layer, bool warn = true, [CallerMemberName] string caller = null)
        {
            //if (warn) DebugUtilitiesTCT.LogStackTrace();


            List<GameObject> prefabsForLayer = null;
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                prefabsForLayer = railPrefabs;
            else if (layer == LayerSet.postLayer || layer == LayerSet.subpostLayer)
                prefabsForLayer = postPrefabs;
            else if (layer == LayerSet.extraLayer)
                prefabsForLayer = extraPrefabs;
            else if (layer == LayerSet.allLayer)
                prefabsForLayer = GetAllPrefabs();

            //-- Always warn for null
            if (prefabsForLayer == null)
            {
                Debug.LogWarning($"GetPrefabsForLayer()  {layer.String()} Prefab List was null.  Caller: {caller}\n");
                return null;
            }
            if (prefabsForLayer.Count > 0 && prefabsForLayer[0] == null)
                Debug.LogWarning($"GetPrefabsForLayer() {layer.String()} prefab at index [0] was null.  Count = {prefabsForLayer.Count} \n");
            else if (prefabsForLayer.Count == 0 && warn)
                Debug.LogWarning($"GetPrefabsForLayer()  {layer.String()} prefabs.Count = 0.  Caller: {caller}\n");

            return prefabsForLayer;
        }
        //----------------------
        public void SetPrefabsForLayer(LayerSet layer, List<GameObject> prefabsForLayer)
        {
            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                railPrefabs = prefabsForLayer;
            else if (layer == LayerSet.postLayer || layer == LayerSet.subpostLayer)
                postPrefabs = prefabsForLayer;
            else if (layer == LayerSet.extraLayer)
                extraPrefabs = prefabsForLayer;
        }
        public GameObject SetPrefabForLayerByName(LayerSet layer, string prefabname)
        {
            // get the prefabs for layer
            List<GameObject> prefabsForLayer = GetPrefabsForLayer(layer);
            // find the index of the prefab with prefabName
            int prefabIndex = FindPrefabIndexByName(layer, prefabname);


            // set the current prefab index for layer
            SetCurrentPrefabIndexForLayer(prefabIndex, layer);
            //sync the prefab menu index
            SetMenuIndexFromPrefabIndexForLayer(prefabIndex, layer);

            return GetPrefabAtIndexForLayer(prefabIndex, layer);
        }

        public List<GameObject> GetAllPrefabs()
        {
            List<GameObject> allPrefabs = new List<GameObject>();
            allPrefabs.AddRange(GetPrefabsForLayer(LayerSet.railALayer));
            allPrefabs.AddRange(GetPrefabsForLayer(LayerSet.postLayer));
            allPrefabs.AddRange(GetPrefabsForLayer(LayerSet.extraLayer));
            return allPrefabs;
        }

        //-- Gets the whole prebab list for this layer e.g. railPrefabs, postPrefabs. Remember A & B share the same Rails prefab list
        public List<GameObject> GetPrefabsForPrefabType(PrefabTypeAFWB prefabType)
        {
            if (prefabType == PrefabTypeAFWB.railPrefab)
                return GetPrefabsForLayer(LayerSet.railALayer); // it doesn't matter which Rail Set, the prefabs are the same
            else if (prefabType == PrefabTypeAFWB.postPrefab)
                return GetPrefabsForLayer(LayerSet.postLayer);
            else if (prefabType == PrefabTypeAFWB.extraPrefab)
                return GetPrefabsForLayer(LayerSet.extraLayer);
            return null;
        }

        //------------------------
        //Return the menus for each later
        // these are a list of prefab names and we can't guarantee they'll be in the same order as the prefab Lists
        // so we have to convert when using them in context
        public List<string> GetPrefabMenuNamesForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return railMenuNames;
            else if (layer == LayerSet.railBLayer)
                return railMenuNames;
            else if (layer == LayerSet.postLayer)
                return postMenuNames;
            else if (layer == LayerSet.subpostLayer)
                return postMenuNames;
            else if (layer == LayerSet.extraLayer)
                return extraMenuNames;

            return null;
        }

        public List<string> GetShortPrefabMenuNamesForLayer(LayerSet layer, bool stripCategory = false)
        {
            List<string> layerPrefabMenuNames = GetPrefabMenuNamesForLayer(layer);
            List<string> shortPrefabMenuNames = new List<string>();
            string shortprefabName = "";
            for (int i = 0; i < layerPrefabMenuNames.Count; i++)
            {
                if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                    shortprefabName = StripPanelRailFromName(layerPrefabMenuNames[i]);
                else if (layer == LayerSet.postLayer || layer == LayerSet.subpostLayer)
                    shortprefabName = StripPostFromName(layerPrefabMenuNames[i]);
                else if (layer == LayerSet.extraLayer)
                    shortprefabName = StripExtraFromName(layerPrefabMenuNames[i]);

                if (stripCategory == true)
                    shortprefabName = StripCategoryFromName(shortprefabName);

                shortPrefabMenuNames.Add(shortprefabName);
                //-- Add a space after the Category divider for clarity
                shortPrefabMenuNames[i] = shortPrefabMenuNames[i].Replace("/", "/  ");
            }
            return shortPrefabMenuNames;
        }

        //------------------------
        //When a component selection is changed programatically, rather than using a menu
        // the menu selection displayed will be out of sync.
        public void SetMenuIndexFromPrefabIndexForLayer(int prefabIndex, LayerSet layer)
        {
            int menuIndex = ConvertPrefabIndexToMenuIndexForLayer(prefabIndex, layer);
            List<string> layerMenuNames = GetPrefabMenuNamesForLayer(layer);
            int menuCount = layerMenuNames.Count;

            if (menuIndex >= menuCount)
            {
                Debug.LogWarning($"SetMenuPrefabIndexForLayer()  {GetLayerNameAsString(layer)} menuIndex {menuIndex} out of range. " +
                                       $"True menu count is {menuCount}.  Menu selection not set \n");
                return;
            }
            if (layer == LayerSet.postLayer)
                currentPost_PrefabMenuIndex = menuIndex;
            else if (layer == LayerSet.railALayer)
                currentRail_PrefabMenuIndex[kRailALayerInt] = menuIndex;
            else if (layer == LayerSet.railBLayer)
                currentRail_PrefabMenuIndex[kRailBLayerInt] = menuIndex;
            else if (layer == LayerSet.subpostLayer)
                currentSubpost_PrefabMenuIndex = menuIndex;
            else if (layer == LayerSet.extraLayer)
                currentExtra_PrefabMenuIndex = menuIndex;
        }

        //-----------------------
        public GameObject GetPrefabAtIndexForLayer(int index, LayerSet layer)
        {
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            string layerString = GetLayerNameAsString(layer);

            if (prefabs == null)
            {
                Debug.LogWarning($"GetPrefabAtIndexForLayer()  {layerString} prefabs were null\n");
                return null;
            }
            else if (index < 0 || index >= prefabs.Count)
            {
                Debug.LogWarning($"GetPrefabAtIndexForLayer()  {layerString} index {index} out of range. " +
                                       $"True prefab count is {prefabs.Count}\n");
                return null;
            }
            GameObject prefab = prefabs[index].gameObject;
            if (prefab == null)
                Debug.LogWarning($"GetPrefabAtIndexForLayer()  {layerString} prefab at index {index} was null\n");

            return prefab;
        }

        //-----------------------
        public GameObject GetPrefabAtMenuIndexForLayer(int index, LayerSet layer)
        {
            //Convert the menu index to the prefab index
            int prefabIndex = ConvertMenuIndexToPrefabIndexForLayer(index, GetPrefabTypeFromLayer(layer));
            GameObject prefab = GetPrefabAtIndexForLayer(prefabIndex, layer);
            return prefab;
        }
        //-----------------------
        public int GetNumPrefabsForLayer(LayerSet layer)
        {
            int numPrefabs = 0;

            if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
                numPrefabs = railPrefabs.Count;
            else if (layer == LayerSet.postLayer || layer == LayerSet.subpostLayer)
                numPrefabs = postPrefabs.Count;
            else if (layer == LayerSet.extraLayer)
                numPrefabs = extraPrefabs.Count;

            return numPrefabs;
        }

        //--------------------------------
        // IsLayerEnabled
        internal bool IsLayerEnabled(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return useRailLayer[kRailALayerInt];
            else if (layer == LayerSet.railBLayer)
                return useRailLayer[kRailBLayerInt];
            else if (layer == LayerSet.postLayer)
                return usePostsLayer;
            else if (layer == LayerSet.subpostLayer)
                return useSubpostsLayer;
            else if (layer == LayerSet.extraLayer)
                return useExtrasLayer;
            return false;
        }

        public Transform GetBuiltTransformAtSectionIndexForLayer(int sectionIndex, LayerSet layer)
        {
            List<Transform> builtLayerTransforms = GetBuiltLayerTransforms(layer);

            if (builtLayerTransforms == null)
                return null;

            if (sectionIndex < 0 || sectionIndex >= builtLayerTransforms.Count)
            {
                Debug.LogWarning($"GetBuiltGOAtSectionIndexForLayer()  {GetLayerNameAsString(layer)} sectionIndex {sectionIndex} out of range. " +
                    $"True section count is {builtLayerTransforms.Count}\n");
                return null;
            }
            Transform transform = builtLayerTransforms[sectionIndex];
            if (transform == null)
                Debug.LogWarning($"GetBuiltGOAtSectionIndexForLayer()  {GetLayerNameAsString(layer)} transform at sectionIndex {sectionIndex} was null\n");
            return transform;
        }

        // Gets the list of built transforms for this layer, which are the pools of built objects, e.g. railsAPool, postsPool
        private List<Transform> GetBuiltLayerTransforms(LayerSet layer)
        {
            List<Transform> builtLayer = null;
            if (layer == LayerSet.railALayer)
                builtLayer = railsAPool;
            else if (layer == LayerSet.railBLayer)
                builtLayer = railsBPool;
            else if (layer == LayerSet.postLayer)
                builtLayer = postsPool;
            else if (layer == LayerSet.subpostLayer)
                builtLayer = subpostsPool;
            else if (layer == LayerSet.extraLayer)
                builtLayer = ex.extrasPool;

            if (builtLayer == null)
                Debug.LogWarning($"GetBuiltLayer()  {GetLayerNameAsString(layer)} builtLayer was null\n");

            return builtLayer;
        }

        //-------------
        public int ConvertMenuIndexToPrefabIndexForLayer(int menuIndex, PrefabTypeAFWB PrefabTypeAFWB)
        {
            if (PrefabTypeAFWB == PrefabTypeAFWB.postPrefab)
                return ConvertPostMenuIndexToPrefabIndex(menuIndex);
            if (PrefabTypeAFWB == PrefabTypeAFWB.railPrefab)
                return ConvertRailMenuIndexToPrefabIndex(menuIndex);
            if (PrefabTypeAFWB == PrefabTypeAFWB.extraPrefab)
                return ConvertExtraMenuIndexToPrefabIndex(menuIndex);
            return 0;
        }

        //-------------
        public int ConvertPrefabIndexToMenuIndexForLayer(int prefabIndex, LayerSet layer)
        {
            GameObject prefab = GetPrefabAtIndexForLayer(prefabIndex, layer);

            string prefabName = prefab.name;
            int menuIndex = FindPrefabIndexInMenuNamesList(layer, prefabName, warnMissing: false);
            if (menuIndex == -1)
            {
                if (layer == LayerSet.extraLayer)
                {
                    menuIndex = FindPrefabIndexInMenuNamesList(LayerSet.postLayer, prefabName, warnMissing: false);
                    if (menuIndex == -1)
                        menuIndex = FindPrefabIndexInMenuNamesList(LayerSet.railALayer, prefabName, warnMissing: false);
                }
                if (menuIndex == -1)
                    Debug.LogWarning($"menuIndex was -1 for layer  {GetLayerNameAsString(layer)} in ConvertRailPrefabIndexToMenuIndex()");
            }

            return menuIndex;
        }

        //-------------
        // Because they the prefabs are in a different order in the menu compared to
        // how they are in the prefabs list, due to Category and Alphabet sorting
        public int ConvertRailMenuIndexToPrefabIndex(int railmenuIndex)
        {
            string prefabName = railMenuNames[railmenuIndex]; // name including category
            prefabName = prefabName.Remove(0, prefabName.IndexOf("/") + 1); // remove category name
            int prefabIndex = FindPrefabIndexByNameForLayer(PrefabTypeAFWB.railPrefab, prefabName, "ConvertRailMenuIndexToPrefabIndex");
            return prefabIndex;
        }

        //-------------
        // Because they the prefabs are in a different order in the menu compared to
        // how they are in the prebas list, due to Category and Alphabet sorting
        public int ConvertRailPrefabIndexToMenuIndex(int prefabIndex)
        {
            if (prefabIndex == -1)
                Debug.LogWarning($"prefabIndex was -1 in ConvertRailPrefabIndexToMenuIndex()");
            LayerSet layer = LayerSet.railALayer;
            GameObject prefab = GetPrefabAtIndexForLayer(prefabIndex, layer);
            string prefabName = prefab.name;
            int menuIndex = FindPrefabIndexInMenuNamesList(GetPrefabTypeFromLayer(layer), prefabName);
            if (menuIndex == -1)
                Debug.LogWarning($"menuIndex was -1 in ConvertRailPrefabIndexToMenuIndex()");
            return menuIndex;

            /*string prefabName = railPrefabs[prefabIndex].name; // name including category
            int menuIndex = FindPrefabIndexInMenuNamesList(PrefabTypeAFWB.railPrefab, prefabName);
            if(menuIndex == -1)
                Debug.LogWarning($"menuIndex was -1 in ConvertRailPrefabIndexToMenuIndex()");
            return menuIndex;*/
        }

        //-------------
        public int ConvertPostMenuIndexToPrefabIndex(int postMenuIndex)
        {
            if (postMenuIndex >= postMenuNames.Count)
                Debug.LogWarning("ConvertPostMenuIndexToPrefabIndex: postMenuIndex out of range \n");

            string prefabName = postMenuNames[postMenuIndex]; // name including category

            bool usingExtra = false;
            if (prefabName.StartsWith("Extra/"))
                usingExtra = true;


            prefabName = prefabName.Remove(0, prefabName.IndexOf("/") + 1); // remove category name

            //--If there's a subcategory, remove that too
            if (prefabName.Contains("/"))
                prefabName = prefabName.Remove(0, prefabName.IndexOf("/") + 1); // remove subcategory name

            int prefabIndex = FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, prefabName, "ConvertPostMenuIndexToPrefabIndex");

            return prefabIndex;
        }

        //-------------
        public int ConvertPostPrefabIndexToMenuIndex(int postPrefabIndex)
        {
            string prefabName = postPrefabs[postPrefabIndex].name; // name including category
            int menuIndex = FindPrefabIndexInMenuNamesList(PrefabTypeAFWB.postPrefab, prefabName);
            return menuIndex;
        }

        //-------------
        public int ConvertExtraMenuIndexToPrefabIndex(int extraMenuIndex)
        {
            string prefabName = extraMenuNames[extraMenuIndex]; // name including category
            prefabName = prefabName.Remove(0, prefabName.IndexOf("/") + 1); // remove category name
                                                                            //--If there'stackIdx a subcategory, remove that too
            if (prefabName.Contains("/"))
                prefabName = prefabName.Remove(0, prefabName.IndexOf("/") + 1); // remove subcategory name
            int prefabIndex = FindPrefabIndexByNameForLayer(PrefabTypeAFWB.extraPrefab, prefabName, "ConvertExtraMenuIndexToPrefabIndex");
            return prefabIndex;
        }

        //-------------
        public int ConvertExtraPrefabIndexToMenuIndex(int extraPrefabIndex)
        {
            string prefabName = extraPrefabs[extraPrefabIndex].name; // name including category
            int menuIndex = FindPrefabIndexInMenuNamesList(PrefabTypeAFWB.extraPrefab, prefabName);
            return menuIndex;
        }

        //==================================================================================
        //                              SourceVariants
        //==================================================================================
        public void OnSourceVariantGoChanged(GameObject go)
        {
            UpdateSourceVariantMenuIndicesForLayerFromPrefabIndicies(LayerSet.railALayer);
            UpdateSourceVariantMenuIndicesForLayerFromPrefabIndicies(LayerSet.railBLayer);
            UpdateSourceVariantMenuIndicesForLayerFromPrefabIndicies(LayerSet.postLayer);
        }

        // List<int> sourceVariantMenuIndices : This is the list that holds kMaxNumSourceVariants (usually Main + 8 = 9) menu indices for each of the source sourceVariants
        // Each index is an index into the Main prefab list for that layer
        public void UpdateSourceVariantMenuIndicesForLayerFromPrefabIndicies(LayerSet layer)
        {
            List<int> sourceVariantMenuIndices = GetSourceVariantMenuListForLayer(layer);
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);

            for (int i = 0; i < sourceVariants.Count; i++)
            {
                GameObject go = sourceVariants[i].Go;
                //  Set the menu index for this source variant to be the index of the prefab go it's using
                UpdateSourceVariantMenuIndexForLayerFromPrefabIndex(go, i, layer);
            }
        }

        // As above but for a single index
        public void UpdateSourceVariantMenuIndexForLayerFromPrefabIndex(GameObject go, int sourceVariantIndex, LayerSet layer)
        {
            List<int> sourceVariantMenuIndices = GetSourceVariantMenuListForLayer(layer);
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            int mainPrefabIndex = GetCurrentPrefabIndexForLayer(layer);
            int prefabIndex = 0, menuIndex = 0;

            if (go != null)
            {
                prefabIndex = FindPrefabIndexByName(layer, go.name);
                if (prefabIndex == -1)
                {
                    Debug.LogWarning($"UpdateSourceVariantMenuIndicesForLayerFromPrefabIndicies ( {layer.ToString()} , sourceVariants[ {sourceVariantIndex} ] " +
                                                                                                        $"was null. Setting to Main \n");
                    prefabIndex = mainPrefabIndex;
                }
            }
            else
            {
                Debug.LogWarning($"SetSourceVariantMenuIndicesListForLayer ( {layer.ToString()} , The GameObject on sourceVariants[ {sourceVariantIndex} ] " +
                                                                             $"was null. Setting to Main \n");
            }
            menuIndex = ConvertPrefabIndexToMenuIndexForLayer(prefabIndex, layer);
            sourceVariantMenuIndices[sourceVariantIndex] = menuIndex;
        }

        public List<int> GetSourceVariantMenuListForLayer(LayerSet layer)
        {
            List<int> sourceVariantMenuIndices = new List<int>();
            if (layer == LayerSet.railALayer)
                sourceVariantMenuIndices = railASourceVariant_MenuIndices;
            else if (layer == LayerSet.railBLayer)
                sourceVariantMenuIndices = railBSourceVariant_MenuIndices;
            else if (layer == LayerSet.postLayer)
                sourceVariantMenuIndices = postSourceVariant_MenuIndices;

            return sourceVariantMenuIndices;
        }

        //-----------------------------------------------------
        //Sets  source variant GO
        // Also syncs the SourceVariant Menus
        public void SetSourceVariantGOForLayer(int svIndex, int prefabIndex, LayerSet layer)
        {
            SourceVariant sourceVariant = GetSourceVariantAtIndexForLayer(svIndex, layer);
            GameObject prefab = GetPrefabAtIndexForLayer(prefabIndex, layer);
            sourceVariant.Go = prefab;
        }

        public void SetSourceVariantGoAtIndexForLayer(int svIndex, GameObject go, LayerSet layer)
        {
            SourceVariant sourceVariant = GetSourceVariantAtIndexForLayer(svIndex, layer);
            sourceVariant.Go = go;
        }

        public void SetSourceVariantGO(int svIndex, GameObject go, LayerSet layer)
        {
            SourceVariant sourceVariant = GetSourceVariantAtIndexForLayer(svIndex, layer);
            sourceVariant.Go = go;
        }

        /// <summary>Also updates the SV Menu indices, and checks the validity of the SourceVariant List for the layer</summary>
        public void SetFirstSourceVariantToMainForLayer(LayerSet layer)
        {
            SourceVariant sourceVariant = GetSourceVariantAtIndexForLayer(0, layer);
            GameObject go = GetMainPrefabForLayer(layer);
            sourceVariant.Go = go;

            //Sync the SourceVariant Menus
            UpdateSourceVariantMenuIndexForLayerFromPrefabIndex(go, 0, layer);
            CheckSourceVariantGosForLayerAreValid(layer);
        }

        //-----------------------------------------------------
        //Sets all source variant GOs to be the same as the main current prefab for that layer
        // Also syncs the SourceVariant Menus
        public void SetAllSourceVariantsToMainForLayer(LayerSet layer)
        {
            //Ignore Extras and Subposts for now
            if (layer == LayerSet.extraLayer || layer == LayerSet.subpostLayer || layer == LayerSet.allLayer || layer == LayerSet.None)
                return;
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            GameObject mainPrefab = GetMainPrefabForLayer(layer);
            for (int i = 0; i < kMaxNumSourceVariants; i++)
            {
                sourceVariants[i].Go = mainPrefab;
            }
        }

        //-----------------------------------------------------
        //Sets  sourceVariant[index] GOs to be the same as the main current prefab for that layer
        // Also syncs the SourceVariant Menus
        public void SetSourceVariantAtIndexToMainForLayer(int index, LayerSet layer)
        {
            //Ignore Extras and Subposts for now
            if (layer == LayerSet.extraLayer || layer == LayerSet.subpostLayer || layer == LayerSet.allLayer || layer == LayerSet.None)
                return;

            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            GameObject mainPrefab = GetMainPrefabForLayer(layer);
            sourceVariants[index].Go = mainPrefab;
        }

        //---------------------------
        // This is the list that holds kMaxNumSourceVariants (usually Main + 8 = 9)  menu indices for each of the source sourceVariants
        public List<int> GetSourceVariantMenuIndicesListForLayer(LayerSet layer)
        {
            List<int> sourceVariantMenuIndices = new List<int>();

            if (layer == LayerSet.railALayer)
            {
                List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer, warn: false);
                for (int i = 0; i < sourceVariants.Count; i++)
                {
                    GameObject go = sourceVariants[i].Go;
                    int prefabIndex = FindPrefabIndexByName(layer, go.name);
                    int menuIndex = ConvertPrefabIndexToMenuIndexForLayer(prefabIndex, layer);
                    railASourceVariant_MenuIndices[i] = menuIndex;
                    //sourceVariantMenuIndices.Add(menuIndex);
                }
                return railASourceVariant_MenuIndices;
            }

            if (layer == LayerSet.railALayer)
                sourceVariantMenuIndices = railASourceVariant_MenuIndices;
            else if (layer == LayerSet.railBLayer)
                sourceVariantMenuIndices = railBSourceVariant_MenuIndices;
            else if (layer == LayerSet.postLayer)
                sourceVariantMenuIndices = postSourceVariant_MenuIndices;

            if (sourceVariantMenuIndices == null)
            {
                Debug.LogWarning($"GetSourceVariantMenuIndicesListForLayer ( {layer.ToString()} , {sourceVariantMenuIndices.Count} " +
                                                          $"was null. Setting all {kMaxNumSourceVariants}menuIndices to 0 \n");
                return new List<int>(new int[kMaxNumSourceVariants]);
            }

            if (sourceVariantMenuIndices.Count != kMaxNumSourceVariants)
            {
                Debug.LogWarning($"GetSourceVariantMenuIndicesListForLayer ( {layer.ToString()} , {sourceVariantMenuIndices.Count} " +
                                       $"was not equal to {kMaxNumSourceVariants}. Setting all {kMaxNumSourceVariants}menuIndices to 0 \n");
                return new List<int>(new int[kMaxNumSourceVariants]);
            }

            int numPrefabsAvailable = GetPrefabsForLayer(layer).Count;
            //Check that the menu indices are valid
            for (int i = 0; i < sourceVariantMenuIndices.Count; i++)
            {
                int index = sourceVariantMenuIndices[i];
                if (index < 0 || index >= numPrefabsAvailable)
                {
                    //if index is -1, it just means they haven't been chosen yet, or are from an old preset
                    // so just set it to the Main prefab
                    // unless i = [0] in which case it's the main prefab, and should be valid
                    if (i > 0 && index == -1)
                        sourceVariantMenuIndices[i] = GetCurrentPrefabIndexForLayer(layer);
                    else if (i == 0 && index == -1)
                        Debug.LogWarning($"GetSourceVariantMenuIndicesListForLayer ( {layer.ToString()} , sourceVariantMenuIndices[ {i} ] " +
                                                              $"was invalid  [ {index} ] . Setting to 0 \n");
                }
            }
            return sourceVariantMenuIndices;
        }

        public List<GameObject> GetSourceVariantPrefabsForLayer(LayerSet layer)
        {
            //for (int i = 0; i<kMaxNumSourceVariants

            //Get the source variants prefabs for this layer
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            List<GameObject> svPrefabs = new List<GameObject>();
            for (int i = 0; i < sourceVariants.Count; i++)
            {
                SourceVariant sourceVariant = sourceVariants[i];
                GameObject prefab = sourceVariant.Go;
                svPrefabs.Add(prefab);
            }
            return svPrefabs;
        }
        //---------------------------------------
        //-- Check that This SourceVariants List for this Layer has kMaxNumSourceVariants SourceVariants
        //-- Does NOT check that the SourceVariants GOs are not null, that's done in CheckSourceVariantGOs()
        public bool CheckSourceVariantsForLayerAreValid(LayerSet layer)
        {
            bool neededCreating = false;
            List<SourceVariant> sourceVariantsList = GetSourceVariantsForLayer(layer, warn: false);
            if (sourceVariantsList == null || sourceVariantsList[0] == null)
            {
                GameObject go = GetMainPrefabForLayer(layer);
                sourceVariantsList = SourceVariant.CreateInitialisedSourceVariantList(go);
                neededCreating = true;
            }

            int svCount = sourceVariantsList.Count;
            SourceVariant firstSV = sourceVariantsList[0];

            //-- First check there's enough, if not, Clear and rebuild them
            if (svCount < AutoFenceCreator.kMaxNumSourceVariants)
            {
                bool t = allowMirroring_X_Rail[0];

                sourceVariantsList.Clear();
                sourceVariantsList = SourceVariant.CreateInitialisedSourceVariantList();
                sourceVariantsList[0] = firstSV;
                neededCreating = true;
            }

            //-- Now create a SourceVariant for each index
            for (int i = 0; i < AutoFenceCreator.kMaxNumSourceVariants; i++)
            {
                if (sourceVariantsList[i] == null)
                {
                    sourceVariantsList[i] = new SourceVariant(firstSV.Go);
                    neededCreating = true;
                }
            }
            return neededCreating;
        }

        public bool CheckSourceVariantGosForAllLayersAreValid()
        {
            bool neededFixing = CheckSourceVariantGosForLayerAreValid(LayerSet.railALayer);
            neededFixing = CheckSourceVariantGosForLayerAreValid(LayerSet.railBLayer);
            neededFixing = CheckSourceVariantGosForLayerAreValid(LayerSet.postLayer);
            return neededFixing;
        }

        //---------------------------------------
        /// <summary>
        /// -- Check that each SourceVariants GOs are not null
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="warn"></param>
        /// <returns>true if needed fixing</returns>
        //-- Any Nulls are replaced by the current Main Prefab
        public bool CheckSourceVariantGosForLayerAreValid(LayerSet layer, bool warn = true)
        {
            bool neededFixing = false;
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            GameObject currPrefabForLayer = GetMainPrefabForLayer(layer);
            int added = 0;
            //Check that all sourcevariants in this Layer's List has vaild GOs
            for (int i = 0; i < AutoFenceCreator.kMaxNumSourceVariants; i++)
            {
                if (sourceVariants[i] == null)
                {
                    sourceVariants[i] = new SourceVariant(currPrefabForLayer);
                }
                if (sourceVariants[i].Go == null)
                {
                    sourceVariants[i].Go = currPrefabForLayer;
                    neededFixing = true;
                    added += 1;
                }
            }
            //if (warn)
            //Debug.Log($"CheckSourceVariantGosForLayerAreValid()  {layer}  replaced {added} GOs \n");
            return neededFixing;
        }
        //---------------------------
        // This generates a  list (of 9),  of the indices in to the MAIN PREFAB LIST for each (9) SourveVariants layer type
        //-- We DON'T cache them as that would mean maintaining that list up to dat. Instead we generate it on the fly from the SourceVariant List
        /*public List<int> CreateSourceVariantPrefabIndicesListForLayer(LayerSet layer)
        {
            List<int> indices = GetSourceVariantMenuIndicesListForLayer(layer);
            List<int> prefabIndices = new List<int>(new int[indices.Count]); ; //do it this way so at least we get all 0s if something goes wrong

            for (int i = 0; i < indices.Count; i++)
            {
                int menuIndex = indices[i];
                int prefabIndex = ConvertMenuIndexToPrefabIndexForLayer(menuIndex, GetPrefabTypeFromLayer(layer));
                prefabIndices[i] = prefabIndex;
            }
            return prefabIndices;
        }*/
        //------------------
        /* public List<int> GetSourceVariantPrefabIndicesForLayer(LayerSet layer)
        {
             List<int> prefabIndices = new List<int>();

             if (layer == LayerSet.railALayer)
                 prefabIndices = railASourceVariant_PrefabIndices;
             else if (layer == LayerSet.railBLayer)
                 prefabIndices = railBSourceVariant_PrefabIndices;
             else if (layer == LayerSet.postLayer)
                 prefabIndices = postSourceVariant_PrefabIndices;

             return prefabIndices;
        }*/

        //-----------------------
        // Get the SourceVariant for the single at the specified section index
        public SourceVariant GetSinglesSourceVariantForLayerWithSectionIndex(LayerSet layer, int inSingleSectionIndex, [CallerMemberName] string caller = null)
        {
            //-- This is List(one per fence section) of SinglesItems that each contain an index into
            //-- 'sourceVariantsList' which is a List of kMaxNumSourceVariants (usually 9) SourceVariants which define the GO used.
            List<SinglesItem> singleVariants = singlesContainer.GetSingleVariantsForLayer(layer, af); //Must be the total num of Posts or Rails along the Fence
            List<SourceVariant> sourceVariantsList = GetSourceVariantsForLayer(layer);// Should be 9

            //    Check Counts
            //====================
            int numFenceSections = GetNumSectionsBuiltForLayer(layer);
            if (singleVariants.Count < numFenceSections)
            {
                Debug.LogWarning($"[ GetSinglesSourceVariantGONameForLayerWithSectionIndex (  {GetLayerNameAsString(layer)}  )   called by  {caller}()  ]\n" +
                    $"singleVariants.Count: {singleVariants.Count}  was less than  GetNumSectionsBuiltForLayer: {numFenceSections} \n");
                return null;
            }

            if (sourceVariantsList.Count < kMaxNumSourceVariants)
            {
                Debug.LogWarning($"[ GetSinglesSourceVariantGONameForLayerWithSectionIndex ( {GetLayerNameAsString(layer)} )    called by  {caller}()  ] \n" +
                                       $"sourceVariantsList.Count: {sourceVariantsList.Count}  was less than  kMaxNumSourceVariants: {kMaxNumSourceVariants} \n");
                return null;
            }

            //     Get The Single for this section index
            //================================================
            SinglesItem singleVariant = singleVariants[inSingleSectionIndex];

            //     Get the index into the sourceVariantsList for this single
            //================================================================
            int sourceVariantIndex = singleVariant.sourceVariantIndex;

            //     Get the SourceVariant for this single from the 9 SourceVariants defined for this layer
            //================================================================================================
            string nameStr = "";
            SourceVariant sourceVariant = sourceVariantsList[sourceVariantIndex];
            if (sourceVariant == null)
            {
                Debug.LogWarning($"GetSinglesSourceVariantGONameForLayerWithSectionIndex ( {GetLayerNameAsString(layer)} , " +
                                       $"sourceVariantsList[sourceVariantIndex] was null for section {inSingleSectionIndex}\n");
                return null;
            }

            return sourceVariant;
        }

        //-----------------------
        // Get the SourceVariant Name for the single at the specified section index
        public string GetSinglesSourceVariantGONameForLayerWithSectionIndex(LayerSet layer, int inSingleSectionIndex, [CallerMemberName] string caller = null)
        {
            SourceVariant sourceVariant = GetSinglesSourceVariantForLayerWithSectionIndex(layer, inSingleSectionIndex);
            if (sourceVariant == null)
            {
                return ""; //-- We've already warned in GetSinglesSourceVariantForLayerWithSectionIndex()
            }
            GameObject go = sourceVariant.Go;
            if (go != null)
                return go.name;
            else
                Debug.LogWarning($"GetSinglesSourceVariantGONameForLayerWithSectionIndex ( {GetLayerNameAsString(layer)} , " +
                                       $"sourceVariantsList[sourceVariantIndex].go was null for section {inSingleSectionIndex}\n");
            return "";
        }

        //------------------------------------------------------------
        public GameObject GetSingleSourceVariantGameObjectForLayerAtSectionIndex(LayerSet layer, int inSingleSectionIndex)
        {
            SourceVariant variant = GetSinglesSourceVariantForLayerWithSectionIndex(layer, inSingleSectionIndex);
            GameObject go = variant.Go;
            return go;
        }

        //---------------------------------------
        // Will give warnings if issues
        public void CheckAllPrefabsLists()
        {
            GetPrefabsForLayer(LayerSet.railALayer);
            GetPrefabsForLayer(LayerSet.railBLayer);
            GetPrefabsForLayer(LayerSet.postLayer);
            GetPrefabsForLayer(LayerSet.subpostLayer);
            GetPrefabsForLayer(LayerSet.extraLayer);
        }

        //-----------------------
        // Called by SetupSingle from the Scene View Context Menus
        // This should be the only route through which singles can be added to the singles list
        public void AddSinglesSourceVariantForLayerWithSectionIndex(LayerSet layer, int singleSectionIndex, int variantIndex)
        {
            if (variantIndex >= kMaxNumSourceVariants)
            {
                variantIndex = 0;
                Debug.LogWarning($"AddSinglesSourceVariantForLayerWithSectionIndex ( {layer.ToString()} , {variantIndex} " +
                    $"was greater than kMaxNumSourceVariants. Setting sourceVariantIndex to 0 \n");
            }
            SinglesContainer layerSinglesContainer = singlesContainer.GetSinglesForLayer(layer, af);
            layerSinglesContainer.SetSingleSourceVariantAtSectionIndex(singleSectionIndex, variantIndex);
        }

        //-----------------------
        /*public GameObject GetSingleGoForSingleVariantsWithSectionIndex(List<SinglesItem> singleVariants, int singleSectionIndex)
        {
            //List < SinglesItem > singleVariants = GetSinglesSourceVariantForLayerWithSectionIndex(layer, singleSectionIndex);
            //List<GameObject> prefabs = GetPrefabsForLayer(layer);

            SinglesItem sourceVariantForSingle = singleVariants[singleSectionIndex];
            SourceVariant fenceVarian = Get
            GameObject singleVarGO = sourceVariant.singleVarGO;
            return singleVarGO;
        }*/

        //-----------------------
        // Get the SourceVariant for the single at the specified section index
        /*public SourceVariant GetSinglesSourceVariantForLayerWithSectionIndex(LayerSet layer, int inSingleSectionIndex)
        {
            SourceVariant sourceVariant = null;
            List<SinglesItem> singleVariants = GetSingleVariantsForLayer(layer);
            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            int count = singleVariants.Count;
            if (inSingleSectionIndex > count)
            {
                Debug.LogWarning($"GetSinglesSourceVariantForLayerWithSectionIndex ( {GetLayerNameAsString(layer)} , {inSingleSectionIndex} " +
                                    $"was greater than singleVariants.Count. Returning Variant[0] \n");
                return sourceVariants[0];
            }

            SinglesItem SinglesItem = singleVariants[inSingleSectionIndex];
            int variantIndex = SinglesItem.sourceVariantIndex;

            if (variantIndex >= kMaxNumSourceVariants)
            {
                variantIndex = 0;
                Debug.LogWarning($"GetSingleSourceVariantWithSectionIndex ( {GetLayerNameAsString(layer)} , {variantIndex} " +
                    $"was greater than kMaxNumSourceVariants. Setting sourceVariantIndex to 0 \n");
            }
            sourceVariant = sourceVariants[variantIndex];
            return sourceVariant;
        }*/

        //-----------------------
        public float GetRandomQuantRotProbForLayer(LayerSet layer)
        {
            if (layer == LayerSet.railALayer)
                return quantizeRotProbRailA;
            else if (layer == LayerSet.railBLayer)
                return quantizeRotProbRailB;
            else if (layer == LayerSet.postLayer)
                return quantizeRotProbPost;
            else if (layer == LayerSet.subpostLayer)
                return quantizeRotProbSubpost;

            return 0;
        }

        public void SetRandomQuantRotProbForLayer(LayerSet layer, float quantizeRotProb)
        {
            if (layer == LayerSet.railALayer)
                quantizeRotProbRailA = quantizeRotProb;
            else if (layer == LayerSet.railBLayer)
                quantizeRotProbRailB = quantizeRotProb;
            else if (layer == LayerSet.postLayer)
                quantizeRotProbPost = quantizeRotProb;
            else if (layer == LayerSet.subpostLayer)
                quantizeRotProbSubpost = quantizeRotProb;
        }

        public void EnforceRangeOfRandomQuantRotProbForLayer(LayerSet layer)
        {
            float quantizeRotProb = GetRandomQuantRotProbForLayer(layer);
            SetRandomQuantRotProbForLayer(layer, Mathf.Clamp(quantizeRotProb, 0.0f, 1.0f));
        }

        //======================================================================
        public string GetLayerNameAsString(LayerSet layer, bool useCamel = false)
        {
            if (layer == LayerSet.railALayer)
            {
                if (useCamel)
                    return "railA";
                else
                    return "RailA";
            }
            else if (layer == LayerSet.railBLayer)
            {
                if (useCamel)
                    return "railB";
                else
                    return "RailB";
            }
            else if (layer == LayerSet.postLayer)
            {
                if (useCamel)
                    return "post";
                else
                    return "Post";
            }
            else if (layer == LayerSet.subpostLayer)
            {
                if (useCamel)
                    return "subpost";
                else
                    return "Subpost";
            }
            else if (layer == LayerSet.extraLayer)
            {
                if (useCamel)
                    return "extra";
                else
                    return "Extra";
            }
            return "";
        }
    }
}