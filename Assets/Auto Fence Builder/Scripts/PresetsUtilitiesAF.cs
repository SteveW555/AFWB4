using System.Collections.Generic;
using System.Linq;

namespace AFWB
{
    /// <summary>
    /// Utilities for working with Presets
    /// See also PresetRepairsEd that needs to be in the Editor Folder to access ScriptablePresetsAFWB    
    /// </summary>
    public partial class AutoFenceCreator
    {
        /// <summary>
        /// Assigns a category to a Preset or Prefab that lacks one, based on its prefabName or substring, or paren Folder prefabName
        /// </summary>
        /// <param prefabName="prefabName"></param>
        /// <param prefabName="parentFolderName"></param>
        /// <returns></returns>
        public string AssignPresetOrPrefabCategoryByName(string prefabName, string parentFolderName)
        {
            
            //prefabName = inName.Trim();

            //-- Find if there's already a prefix category in the prefabName of a Preset. e.g. "Wood/Wooden_Post" returns "Wood"
            //-- Usually not, but may change with updates
            string category = GetPresetCategoryFromName(prefabName);


            /*if (prefabName.EndsWith("_Extra"))
                category = "Extra";
            else if (category != "" && category != "Extra")
                return category;*/
            if (prefabName.StartsWith("_Template") || prefabName.StartsWith("Template"))
                category = " Basic Templates";
            else if (prefabName.StartsWith("Demo"))
                category = " Demo Usage";
            else if (prefabName.Contains("Wood") || prefabName.Contains("Fortress"))
                category = " Wood";
            else if (new[] { "Tree", "Bush", "Veg", "Plant", "Birch", "Shrub", "Grass", "Flower", "Pine" }.Any(prefabName.Contains))
                category = " Vegetation";
            else if (prefabName.Contains("Concrete") && prefabName.Contains("Wood"))
                category = " Concrete & Wood";
            else if (prefabName.Contains("Brick") || prefabName.Contains("CinderBlock") || prefabName.Contains("Cinderblock"))
                category = " Brick";
            else if (prefabName.Contains("Cable"))
                category = " Cable";
            else if (prefabName.Contains("Concrete"))
                category = " Concrete";
            else if (prefabName.Contains("Metal") || prefabName.Contains("Steel") || prefabName.Contains("Rusty") || prefabName.Contains("Girder")
                     || prefabName.Contains("Chrome") || prefabName.Contains("Iron") || prefabName.Contains("Aluminium") || prefabName.Contains("Rebar")
                || prefabName.Contains("Gold") || prefabName.Contains("Silver") || prefabName.Contains("Galv"))
                category = " Metal";
            else if (prefabName.Contains("Railings"))
                category = " Railings";
            else if (prefabName.Contains("Rustic"))
                category = " Rustic";
            else if (prefabName.Contains("Stone") || prefabName.Contains("Rock") || prefabName.Contains("Boulder") || prefabName.Contains("Drywall"))
                category = " Rock & Stone ";
            else if (prefabName.Contains("Castle"))
                category = " Castle";
            else if (prefabName.Contains("Fort"))
                category = " Fort";
            else if (prefabName.Contains("Test"))
                category = " Test";
            else if (prefabName.Contains("Goose"))
                category = " Goose";
            else if (prefabName.Contains("Wire"))
                category = " Wire";
            else if (prefabName.Contains("SciFi"))
                category = " SciFi";
            else if (prefabName.Contains("Industrial"))
                category = " Industrial";
            else if (new[] { "Military", "Sandbag", "Cheval De Frise", "CzechHedgehog" }.Any(prefabName.Contains))
                category = " Military";
            else if (prefabName.Contains("Urban"))
                category = " Urban";
            else if (prefabName.Contains("Residential"))
                category = " Residential";

            else if (parentFolderName != "" && parentFolderName.Contains("_AFWB") == false) //-- There is a category directory, not just the top level prefabs dir
                category = $" {parentFolderName}";
            else
                category = " Other";

            return category;
        }
        //---------------------------------
        public string GetSubCategoryOfExtras(string name)
        {
            //string subcat = "";

            if (name.Contains("Rock") == true || name.Contains("Stone") == true)
                name = "Rocks & Stone" + "/" + name;
            else if (name.Contains("Veg") || name.Contains("Tree") || name.Contains("Bush"))
                name = "Vegetation" + "/" + name;
            else if (name.Contains("Block") || name.Contains("SphereTCT") || name.Contains("Box")
               || name.Contains("Dome") || name.Contains("Spire") || name.Contains("Disc"))
                name = "Blocks & Spheres" + "/" + name;
            return name;
        }
        //------------------------------------------------
        /// <summary>
        /// Finds if there's a prefix category in the prefabName of a Preset. e.g. "Wood/Wooden_Post" returns "Wood"
        /// </summary>
        /// <param prefabName="presetName"></param>
        /// <returns></returns>
        public string GetPresetCategoryFromName(string presetName)
        {
            string category = "";

            if (presetName.Contains("/"))
            {
                int index = presetName.IndexOf("/");
                category = presetName.Substring(0, index);
            }
            return category;
        }

        private void SetAllPopupsToShowCorrectPrefabsAfterPresetChange()
        {
            //      First set the Primary Component Selections
            //======================================================
            SetPrefabMenuForLayer(af.currentRail_PrefabIndex[kRailALayerInt], kRailALayer);
            SetPrefabMenuForLayer(af.currentRail_PrefabIndex[kRailBLayerInt], kRailBLayer);
            SetPrefabMenuForLayer(af.currentPost_PrefabIndex, kPostLayer);
            SetPrefabMenuForLayer(af.currentSubpost_PrefabIndex, kSubpostLayer);
            SetPrefabMenuForLayer(af.currentExtra_PrefabIndex, kExtraLayer);

            List<SourceVariant> railAVariants = af.GetSourceVariantsForLayer(kRailALayer);
        }
        /*public void GetPresetNameForIndex()
        {
            string selectedPresetName = scriptablePresetMenuNames[af.presetMenuIndexInDisplayList];
        }*/

    }
}