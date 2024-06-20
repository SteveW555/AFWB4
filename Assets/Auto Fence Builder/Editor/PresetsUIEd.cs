using AFWB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// PresetsUIEd.cs
/// Part of AutoFenceEditor handling Preset UI functionality.
/// </summary>
public partial class AutoFenceEditor
{
    private string presetFilterString = ""; // to filter the preset list
    private List<string> filteredPresetNames = new List<string>(); //-- the presetMenuNames after filtering with presetFilterString
    List<string> displayMenuNames = new List<string>();
    int realPresetMenuIndexForLayer = 0;
    //AutoFenceEditor ed = null;
    public void ShowPresetsUI(AutoFenceEditor ed)
    {
        //this.ed = ed;
        //============================================================================================================================
        //
        //                         Presets:      Choose or Save Scriptable Main Fence/Wall Preset
        //
        //============================================================================================================================
        if (presetMenuNames != null && presetMenuNames.Count > 0 && mainPresetList != null && mainPresetList.Count > 0)
        {
            //-- Sanity check as tif this gets currupted it wont get reset until a new preset is changed which might be impossible if Inspector is bugged
            //Find index oSf af.currPresetName in  presetMenuNames
            af.currPresetMenuIndex = presetMenuNames.IndexOf(af.currPresetName); //-- Menu names include the Category
            if (af.currPresetMenuIndex == -1)
            {
                af.currPresetMenuIndex = 0;
                Debug.LogWarning("currPresetMenuIndex was -1. Setting to 0\n");
                
                af.currPresetIndex = GetPresetIndexByNameFromMainPresets(af.currPresetName);
                if (af.currPresetIndex == -1)
                    Debug.LogWarning("currPresetIndex was -1. Setting to 0\n");
            }
            else
                af.currPresetIndex = GetPresetIndexByNameFromMainPresets(af.currPresetName);




            GUILayout.Space(3);
            if (isDark)
            {
                boxUIGreenStyle.normal.background = bgGreenBoxTex;
                GUILayout.BeginVertical(boxUIGreenStyle);
                GUILayout.BeginVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                GUILayout.BeginVertical("box");
            }
            Color moduleHeaderStyleColor = new Color(0.58f, .73f, .5f);
            Color moduleBgColor = new Color(0.99f, 1f, .97f);
            //Color moduleBgColor2 = new Color(0.8f, 1f, .7f);
            using (var horizontalScope = new GUILayout.HorizontalScope("box"))
            {
                GUI.backgroundColor = moduleBgColor;

                //GUILayout.Space(15);
                //cyanBoldStyle
                EditorGUILayout.LabelField("", GUILayout.Width(160), GUILayout.Height(22));
                EditorGUILayout.LabelField("Presets", moduleHeaderLabelStyle, GUILayout.Width(60));
                Rect rect = GUILayoutUtility.GetLastRect();
                if (saveTex == null)
                    Debug.LogWarning("saveTex is null \n");
                else
                    EditorGUI.DrawPreviewTexture(new Rect(rect.x - 50, rect.y, 18, 17), saveTex);
                GUILayout.Space(10);
                AFWB_HelpText.ShowPresetsHelp(horizontalScope, moduleHeaderLabelStyle, 140);
                GUILayout.Space(15);
                GUIStyle sty = new GUIStyle(EditorStyles.toolbarButton);
                sty.normal.textColor = new Color(0.25f, 0.55f, 0.89f);
                sty.normal.textColor = moduleHeaderStyleColor;
                //sty.normal.textColor = cyanBoldStyle.normal.textColor;
                //======   Refresh   =====
                if (GUILayout.Button(new GUIContent("Refresh", "Reloads all presetsEd & prefabs, including your custom ones. " +
                    "Necessary if you've manually added presetsEd or prefabs into the AFWB directories"), sty, GUILayout.Width(53)))
                {
                    ReloadPrefabsAndPresets();
                }
                GUILayout.Space(20);
            }

            GUI.backgroundColor = moduleBgColor;


            //      Filter Calculation
            //==============================
            realPresetMenuIndexForLayer = af.currPresetMenuIndex;
            List<string> fullPresetMenuNames = presetMenuNames;
            List<string> filteredMenuNames = new List<string>();
            for (int i = 0; i < fullPresetMenuNames.Count; i++)
            {
                if (fullPresetMenuNames[i].ToLower().Contains(presetFilterString.ToLower()))
                    filteredMenuNames.Add(fullPresetMenuNames[i]);
            }

            //--  Choose what menu names to display, all or filtered
            if (filteredMenuNames.Count > 0)
                displayMenuNames = filteredMenuNames;
            else
                displayMenuNames = fullPresetMenuNames;

            //-- Calculate the menu index to use
            if (filteredMenuNames.Count > 0)
            {
                if (af.presetDisplayMenuIndex >= filteredMenuNames.Count)
                    af.presetDisplayMenuIndex = 0;
            }
            else
                af.presetDisplayMenuIndex = realPresetMenuIndexForLayer;

            if (af.presetDisplayMenuIndex == -1)
            {
                af.presetDisplayMenuIndex = 0;
                Debug.LogWarning($"presetDisplayMenuIndex was -1\n");
            }


            //===============================================================
            //                  Choose Preset
            //================================================================
            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.LabelField(new GUIContent("Choose Preset:", "This is the master Preset it will change every visual element of your weall/fence" +
                "\n\nIt will not affect the layout, though the length of individual section-panels may change if the preset's 'Post-Rail Spacing is very different" +
                "\n\n If you only want to change the visual style of only the panels, or postsPool etc. then 'Choose Prefab Type' " +
                "under the relevant toolvar tab, for example 'Rails A Control'"), moduleHeaderLabelStyle, GUILayout.Width(110));

            //    Back/Forward < >
            //=======================
            if (GUILayout.Button(new GUIContent("<", "Choose Previous Preset"), EditorStyles.miniButton, GUILayout.Width(17)) && af.currPresetIndex > 0)
            {
                //af.currPresetIndex -= 1;
                af.presetDisplayMenuIndex -= 1;
            }
            GUILayout.Space(2);
            if (GUILayout.Button(new GUIContent(">", "Choose Next Preset"), EditorStyles.miniButton, GUILayout.Width(18)) && af.currPresetIndex < mainPresetList.Count - 1)
            {
                //af.currPresetIndex += 1;
                af.presetDisplayMenuIndex += 1;
            }

            GUI.backgroundColor = moduleBgColor;

            //      Popup
            //================
            GUILayout.Space(2);
            if (isDark)
                GUI.backgroundColor = new Color(.67f, .67f, .67f);
            //af.currPresetIndex = EditorGUILayout.Popup(af.currPresetIndex, presetMenuNames.ToArray(), greenPopupStyle, GUILayout.Width(338));
            af.presetDisplayMenuIndex = EditorGUILayout.Popup(af.presetDisplayMenuIndex, displayMenuNames.ToArray(), greenPopupStyle, GUILayout.Width(338));


            //-- Find the preset with this name from the filtered Menu Names List
            string presetMenuName = displayMenuNames[af.presetDisplayMenuIndex];
            //-- Remove the category name from the preset name
            string presetMenuNameNoCat = GetPresetNameWithoutCategory(presetMenuName);
            
            //-- Get the index of the preset with this name from mainPresetList. This is our main presetIndex into the main preset List
            af.currPresetIndex = GetPresetIndexByNameFromMainPresets(presetMenuName);
            af.currPresetMenuIndex = GetPresetMenuIndexByNameFromMainPresetMenu(presetMenuName);
            af.currPresetName = presetMenuName;

            //-- We now have the:
            //-- 1. presetMenuName - the local name of the preset, the same in both the Global and Display menus
            //-- 2. presetDisplayMenuIndex - the index of the preset in the Display menu
            //-- 3. af.currPresetIndex - the index of the preset in the MainPresetList
            //-- 4. af.currPresetMenuIndex - the index of the preset in the Global presetMenuNames menu
            //-- 5. af.currPresetName - the af name of the preset in the MainPresetList
            //-- 6. presetMenuNameNoCat - the name of the preset without the category name
            //-- Note: The preset ARE with Categories, onviously to display them sub-menu style


            GUI.backgroundColor = Color.white;
            //if (af.currPresetMenuIndex < presetMenuNames.Count)
                //presetMenuName = presetMenuNames[af.currPresetMenuIndex];
            GUILayout.Space(7);
            GUI.backgroundColor = moduleBgColor;


            //-- Favourite. Resave a copy in Favorites folder
            bool savedFave = false;
            if (GUILayout.Button("Fave", GUILayout.Width(40)))
            {
                if (presetMenuName.Contains("/"))
                    presetMenuName = presetMenuName.Replace("/", "-");
                af.presetSaveName = "Favorite/" + af.presetSaveName;
                savedFave = presetsEd.SavePreset(true);
            }
            GUILayout.Space(1);

            //      Delete
            //====================
            if (GUILayout.Button("Delete", GUILayout.Width(49)))
            {
                DeletePresetWindow deleteWindow = ScriptableObject.CreateInstance(typeof(DeletePresetWindow)) as DeletePresetWindow;
                deleteWindow.Init(this, currPreset);
                deleteWindow.minSize = new Vector2(430, 190); deleteWindow.maxSize = new Vector2(430, 190);
                deleteWindow.ShowUtility();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();



            //===============================================================
            //                      Filter Display Box
            //===============================================================

            GUILayout.Space(163);

            EditorGUILayout.LabelField("Search: ", GUILayout.Width(50));
            GUILayout.Space(2);
            presetFilterString = EditorGUILayout.TextField(presetFilterString, GUILayout.Width(195));
            if (presetFilterString != "")
            {
                if (GUILayout.Button(new GUIContent("X", "Clear the search filter"), EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    presetFilterString = "";
                }
            }
            if (presetFilterString != "" && filteredMenuNames.Count == 0)
                EditorGUILayout.LabelField("No presets found with filter:  " + presetFilterString, warningStyle2, GUILayout.Width(133));
            else if (presetFilterString != "")
                EditorGUILayout.LabelField($"{filteredMenuNames.Count} Matches for  '{presetFilterString}'", GUILayout.Width(133));
            else
                GUILayout.Space(135);


            //      Edit Notes
            //========================
            GUILayout.Space(2);
            if (af.presetNotes != "")
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            else
                GUI.backgroundColor = new Color(.95f, .94f, 1f); ;

            if (GUILayout.Button(new GUIContent(" Notes ", "Read guides for some presets, or enter your own notes."),
                smallToolbarButtonStyle, GUILayout.Width(47)))
            {
                Vector2 mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                PresetNotesWindow.ShowWindow(af, mousePosition);
            }
            GUI.backgroundColor = Color.white;


            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1, 1, 1), uiLineGreyCol3);


            //======================================
            //      EndChangeCheck - Setup Loaded Preset
            //======================================
            if (EditorGUI.EndChangeCheck())
            {
                //af.currPresetName = presetMenuName;
                presetsEd.SetupPreset(af.currPresetIndex, forceRebuild: false);

                //find the index of af.currPresetName in the presetMenuNames list

                //af.currPresetMenuIndex = presetMenuNames.IndexOf(presetMenuName);
                af.currPresetMenuIndex = GetPresetMenuIndexByNameFromMainPresetMenu(presetMenuName);
                af.currPresetIndex = GetPresetIndexByNameFromMainPresets(presetMenuName);

                //af.currPresetMenuIndex = af.presetMen

                af.ForceRebuildFromClickPoints();
            }
            GUILayout.Space(8);

            //===========================================================================
            //                             Save Preset
            //===========================================================================
            bool saved = DisplaySavePresetControls();

            if (saved || savedFave)
            {
                //-- Reload the Presets Including the new one. This will also invoke the menu renaming
                presetsEd.LoadAllScriptablePresets(af.allowContentFreeUse);

                string savedName = af.presetSaveName;
                string savedCategoryName = af.categoryNames[af.categoryIndex];
                //-- This is the menu name of the new preset
                string savedMenuName = savedCategoryName + "/" + savedName;
                //-- This is the new Menu index of the new preset
                int savedMenuIndex = ed.presetMenuNames.IndexOf(savedMenuName);
                af.currPresetMenuIndex  = savedMenuIndex;
                if (savedMenuIndex != -1)
                {
                    af.currPresetIndex = GetPresetIndexByNameFromMainPresets(savedName);
                    presetsEd.SetupPreset(af.currPresetIndex);
                    //af.ForceRebuildFromClickPoints();
                }

                af.presetDisplayMenuIndex = af.currPresetIndex;
                af.ForceRebuildFromClickPoints();
            }
            //else
            //{
                
            //}

            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }
    //--------------------------------
    private bool DisplaySavePresetControls()
    {
        bool saved = false;
        //GUILayout.Space(0);

        //      Save Preset Label
        //==============================
        string helpStr = "Save the current settings as a Preset. Enter a name for the preset.\n\n To make a new category: \nAdd 'Category_Name/' to the beginning of the preset name" +
            "\ne.g.  Garden/Wooden White Garden Fence";
        Color bgColor = GUI.backgroundColor;
        EditorGUILayout.LabelField(new GUIContent(" Save Preset:", helpStr), moduleHeaderLabelStyle, GUILayout.Width(80));
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();

        Color saveLabelColor = bgColor;
        saveLabelColor.r = 0.9f;
        saveLabelColor.g = 1.01f;
        saveLabelColor.b = 0.8f;
        saveLabelColor.a = 0.55f;
        GUI.backgroundColor = saveLabelColor;

        //      Save Preset Button
        //==============================
        if (GUILayout.Button(new GUIContent("Save Preset", helpStr), GUILayout.Width(105)))
        {
            saved = presetsEd.SavePreset(false);
        }

        //      Name Preset Label
        //==============================
        saveLabelColor = bgColor;
        saveLabelColor.r += 0.95f;
        saveLabelColor.g += 0.79f;
        saveLabelColor.b += 0.69f;
        saveLabelColor.a = 0.1f;
        GUI.backgroundColor = saveLabelColor;
        EditorGUILayout.LabelField("  Name Preset: ", GUILayout.Width(85));

        //      Name Preset TextField - Save on Return/Enter
        //====================================================
        GUIStyle greenLabelStyle = new GUIStyle(EditorStyles.textField);
        greenLabelStyle.normal.textColor = new Color(0.77f, .81f, .73f);
        GUI.backgroundColor = bgColor;

        //-- Set the control name before the TextField
        GUI.SetNextControlName("PresetNameTextField");

        //    TextField
        //=================
        //af.presetSaveName = EditorGUILayout.TextField(af.presetSaveName, greenLabelStyle, GUILayout.Width(215));
        af.presetSaveName = EditorGUILayout.TextField(af.presetSaveName, greenLabelStyle, GUILayout.Width(215));

        Event e = Event.current;
        // Check if the TextField is focused and the Return/Enter key is pressed
        if (GUI.GetNameOfFocusedControl() == "PresetNameTextField" && e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            saved = presetsEd.SavePreset(false);
            GUI.FocusControl(null); // Unfocus the TextField after saving
            e.Use(); // Mark the event as used to prevent further processing
        }

        //      New Preset
        //==============================
        GUILayout.Space(55);
        if (GUILayout.Button(new GUIContent("New Preset", "Sets Everything to Default Values."), GUILayout.Width(87)))
        {
            CreateNewPreset();
            saved = true;
        }
        //     Random Preset
        //==============================
        /*if (GUILayout.Button(new GUIContent("Random Preset", helpStr), GUILayout.Width(90)))
        {
            CreateRandomPreset();
        }*/


        GUILayout.EndHorizontal();
        GUILayout.Space(5);


        //     In Category
        GUI.backgroundColor = bgColor;
        //============================
        GUILayout.BeginHorizontal();
        GUILayout.Space(3);
        smallPopupStyle.fontSize = 10;
        EditorGUILayout.LabelField(new GUIContent(" In Category:", "To make a new category add 'My Category Name/' " +
            "to the beginning of the preset name \n\n e.g. Plastic/My Fence"), GUILayout.Width(110));


        //-- Category Popup
        af.categoryIndex = EditorGUILayout.Popup(af.categoryIndex, af.categoryNames.ToArray(), smallPopupStyle, GUILayout.Width(105));

        EditorGUILayout.LabelField(new GUIContent("   Add \" My Category Name/ \"  at beginning to create New Category\"", "To make a new category add 'My Category Name/' " +
            "to the beginning of the preset name \n\n e.g. Plastic/My Fence"), italicHintStyle);
        GUILayout.EndHorizontal();

        return saved;
    }
    //----------------
    List<string> GetFilteredPresetsListOld(string filterString)
    {
        List<string> filteredList = new List<string>();
        foreach (string s in presetMenuNames)
        {
            if (s.ToLower().Contains(filterString.ToLower()))
                filteredList.Add(s);
        }
        if (filteredList.Count == 0)
        {
            //Debug.Log("No presets found with filter: " + filterString);
            // return presetMenuNames;
        }
        return filteredList;
    }
    //-----------------------
    private void CreateNewPreset()
    {
        if (this.defaultPreset == null)
        {
            presetsEd.LoadAllScriptablePresets(false);
            if (this.defaultPreset != null)
                Debug.LogWarning("Needed to Load Default Preset.\n");
        }
        if (this.defaultPreset == null)
        {
            Debug.LogWarning("Couldn't Find or Load Default Preset.\n");
            this.defaultPreset = mainPresetList[0];
        }

        ScriptablePresetAFWB newPreset = presetsEd.SetupPreset(this.defaultPreset, forceRebuild: false);
        string dateTimeStr = DateTime.Now.ToString("MM-dd-HH-mm-ss");

        newPreset.name = "New Preset " + dateTimeStr;
        newPreset.categoryName = "New";
        af.currPresetName = "newPreset.name ";

        //-- Add the Ne Preset to mainPresets List
        mainPresetList.Add(newPreset);
        //-- The curr currPresetIndex will now be the last one we added
        af.currPresetIndex = mainPresetList.Count - 1;

        //-- Add the name of the new preset to the menu string list
        AddSinglePresetStringForPresetMenu(newPreset);
        //-- The  presetMenuIndexInDisplayList will now be the last one we added
        af.presetMenuIndexInDisplayList = presetMenuNames.Count - 1;
        bool savedNew = presetsEd.SavePreset(false);
    }

    //-----------------------
    private void CreateRandomPreset()
    {
        if (this.defaultPreset == null)
        {
            presetsEd.LoadAllScriptablePresets(false);
            if (this.defaultPreset != null)
                Debug.LogWarning("Needed to Load Default Preset.\n");
        }
        if (this.defaultPreset == null)
        {
            Debug.LogWarning("Couldn't Find or Load Default Preset.\n");
            this.defaultPreset = mainPresetList[0];
        }

        ScriptablePresetAFWB newPreset = presetsEd.SetupPreset(this.defaultPreset, forceRebuild: false);
        string dateTimeStr = DateTime.Now.ToString("MM-dd-HH-mm-ss");

        newPreset.name = "New Preset " + dateTimeStr;
        newPreset.categoryName = "New";
        af.currPresetName = "newPreset.name ";

        //-- Random Rail AB Prefab
        int numRailPrefabs = af.GetNumPrefabsForLayer(kRailALayer);
        af.currentRail_PrefabIndex[0] = UnityEngine.Random.Range(0, numRailPrefabs);
        af.currentRail_PrefabIndex[1] = UnityEngine.Random.Range(0, numRailPrefabs);

        //-- Random Post Prefab
        int numPostPrefabs = af.GetNumPrefabsForLayer(kPostLayer);
        af.currentPost_PrefabIndex = UnityEngine.Random.Range(0, numPostPrefabs);

        //-- Random Post Prefab
        int numExtrasPrefabs = af.GetNumPrefabsForLayer(kExtraLayer);
        af.currentExtra_PrefabIndex = UnityEngine.Random.Range(0, numExtrasPrefabs);



        //-- Add the New Preset to mainPresets List
        mainPresetList.Add(newPreset);
        //-- The curr currPresetIndex will now be the last one we added
        af.currPresetIndex = mainPresetList.Count - 1;

        //-- Add the name of the new preset to the menu string list
        AddSinglePresetStringForPresetMenu(newPreset);
        //-- The  presetMenuIndexInDisplayList will now be the last one we added
        af.presetMenuIndexInDisplayList = presetMenuNames.Count - 1;
        bool savedNew = presetsEd.SavePreset(false);
    }

    //----------------
    private List<string> GetFilteredPresetsList(string filterString)
    {

        af.ClearConsole();


        List<string> filteredList = new List<string>();
        /*foreach (string s in presetMenuNames)
        {
            if (s.ToLower().Contains(filterString.ToLower()))
                filteredList.Add(s);

        }*/


        // Assuming prefabs is a List<Prefab> and you have a method StripPrefabTypeFromNameForType
        string targetPresetName = filterString; // Already stripped
        List<string> presetNames = presetMenuNames;
        List<string> presetMenuNamesWithoutCategory = presetMenuNames.Select(name => name.Substring(name.IndexOf('/') + 1)).ToList();


        //List<string> levList = Levenshtein(filterString, presetNames, 20);

        //List<string> jaccardList = Jaccard(filterString, presetNames, 20);

        List<string> matchingComponentList = FilterByMatchingPresetComponentSubstrings(filterString, presetNames, presetMenuNamesWithoutCategory);
        filteredList.AddRange(matchingComponentList);


        // Example usage: Find the top 20 closest prefab names using both metrics
        /*List<(int Index, double Score)> top20ClosestPrefabsBoth = StringUtilsTCT.FindTopClosestStringMatches(presetNames, targetPresetName, 10, SimilarityMetric.Both, 3);
        // Display the closest matches using both metrics (for debugging purposes)
        Debug.Log("Top 20 closest matches using both Levenshtein and Jaccard:\n---------------------");
        foreach (var match in top20ClosestPrefabsBoth)

        }
        if (filteredList.Count == 0)
        {
            Debug.Log($"{presetNames[match.Index]}, Index: {match.Index}, Combined Score: {match.Score}\n");
        }*/

        return filteredList;
    }

    private static List<string> Levenshtein(string targetPresetName, List<string> presetNames, int numToFind = 20, bool print = false)
    {

        List<string> levenshteinList = new List<string>();
        List<(int Index, double Score)> topClosestPrefabsLevenshtein = StringUtilsTCT.FindTopClosestStringMatches(presetNames, targetPresetName, 10, SimilarityMetric.Levenshtein);
        // Display the closest matches using Levenshtein (for debugging purposes)
        Debug.Log("Top 20 closest matches using Levenshtein:\n----------------");
        if (print)
        {
            foreach (var match in topClosestPrefabsLevenshtein)
            {
                Debug.Log($"{presetNames[match.Index]}, Index: {match.Index}, Levenshtein Distance: {match.Score}\n");
            }
        }
        return levenshteinList;
    }

    //---------------------------------------
    private List<string> Jaccard(string targetPresetName, List<string> presetNames, int numToFind = 20, bool print = false)
    {
        // Example usage: Find the top 20 closest prefab names using Jaccard index
        List<string> jaccardList = new List<string>();

        List<(int Index, double Score)> topClosestPrefabsJaccard = StringUtilsTCT.FindTopClosestStringMatches(presetNames, targetPresetName, numToFind, SimilarityMetric.Jaccard, 3);
        af.ClearConsole();
        Debug.Log($"Top 20 {numToFind} matches using Jaccard:\n---------------------");

        if (print)
        {
            foreach (var match in topClosestPrefabsJaccard)
            {
                Debug.Log($"{presetNames[match.Index]}, Index: {match.Index}, Jaccard Index: {1 - match.Score}\n");
                jaccardList.Add(presetNames[match.Index]);
            }
        }
        return jaccardList;
    }


    //------------------------------------------------------------------------
    private List<string> FilterByMatchingPresetComponentSubstrings(string filterString, List<string> presetNames, List<string> presetMenuNamesWithoutCategory, bool print = false)
    {
        (List<ScriptablePresetAFWB> presetsWithComponentMatch, List<string[]> matchingComponents) = ScriptablePresetAFWB.FindPresetsContainingSubstring(mainPresetList, filterString);

        //-- Create matchingsring names using LINQ
        List<string> presetWithComponentNames = presetsWithComponentMatch.Select(preset => preset.name).ToList();

        //-- Create matching Indices using LINQ
        List<int> matchingIndices = presetsWithComponentMatch.Select(preset => presetMenuNamesWithoutCategory.IndexOf(preset.name))
            .Where(index => index != -1).ToList();


        SortPresetsByComponentCount(ref presetsWithComponentMatch, ref matchingComponents);


        List<string> presetCompNames = new List<string>();

        foreach (ScriptablePresetAFWB preset in presetsWithComponentMatch)
        {
            string presetName = preset.name;
            //presetNames.Add(presetName);

            int menuNameIndex = presetMenuNamesWithoutCategory.IndexOf(presetName);

            if (menuNameIndex != -1)
            {
                //Debug.Log($"{presetName}' matching '{filterString}' index {menuNameIndex}\n");

                // Find the corresponding components array in matchingComponents
                int presetIndex = presetsWithComponentMatch.IndexOf(preset); // Find index of the preset in the original list
                string[] components = matchingComponents[presetIndex]; // Get components from the tuple

                // Use StringBuilder for formatted output

                if (print)
                {
                    StringBuilder componentStringBuilder = new StringBuilder();
                    foreach (string component in components)
                    {
                        componentStringBuilder.Append($" [ {component} ] ");
                    }
                    Debug.Log($"{presetName}'   :   '{filterString}'        Matching components:{componentStringBuilder}\n");
                }
            }
        }
        Debug.Log($"Found  {presetWithComponentNames.Count}  '{filterString}'   FilterByMatchingPresetComponentSubstrings\n");

        return presetWithComponentNames;
    }
    //------------------
    /// <summary>
    /// Find the preset name in mainPresetList and return the index
    /// </summary>
    /// <param name="presetMenuName"></param>
    /// <returns>int index of the preset with presetName</returns>
    /// <remarks    >presetName has the categories (as menus have them)
    /// so we have to strip the category</remarks>
    public int GetPresetIndexByNameFromMainPresets(string presetName)
    {
        int presetIndex = 0;

        //-- TODO Figure out why this fails but a simple loop succeeds
        //presetIndex = mainPresetList.FindIndex(p => p.name == presetName);

        string presetNameNoCat = GetPresetNameWithoutCategory(presetName);

        for (int i = 0; i < mainPresetList.Count; i++)
        {
            string mainListName = mainPresetList[i].name;
            if (presetNameNoCat == mainListName)
            {
                presetIndex = i;
                return presetIndex;
            }
        }
        return presetIndex;
    }
    public int GetPresetMenuIndexByNameFromMainPresetMenu(string presetName)
    {
        int presetIndex = 0;
        for (int i = 0; i < presetMenuNames.Count; i++)
        {
            string menuListName = presetMenuNames[i];
            if (presetName == menuListName)
            {
                presetIndex = i;
                return presetIndex;
            }
        }
        return presetIndex;
    }
    //------------------------------------------------------------------------
    // Helper method to sort the presets and components by component count (descending)
    static void SortPresetsByComponentCount(ref List<ScriptablePresetAFWB> presets, ref List<string[]> components)
    {
        var presetComponentPairs = presets.Zip(components, (preset, component) => (preset, component)).ToList();
        presetComponentPairs.Sort((pair1, pair2) => pair2.component.Length.CompareTo(pair1.component.Length));

        presets = presetComponentPairs.Select(pair => pair.preset).ToList();
        components = presetComponentPairs.Select(pair => pair.component).ToList();
    }
    /// <summary>
    /// Returns the substring after the first '/' character.
    /// If the '/' character is not found, returns the original string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The substring after the first '/' character or the original string if '/' is not found.</returns>
    public static string GetPresetNameWithoutCategory(string input)
    {
        int slashIndex = input.IndexOf('/');
        string strippedName = input;
        if (slashIndex != -1 && slashIndex + 1 < input.Length)
        {
            input =  input.Substring(slashIndex + 1);
        }
        //-- Do it twice as there may be dual nested Categories. Dont bother with recursion for just two.
        if (slashIndex != -1 && slashIndex + 1 < input.Length)
        {
            input = input.Substring(slashIndex + 1);
        }
        return input;
    }
}

public class PresetNotesWindow : EditorWindow
{
    private AutoFenceCreator af;
    private string notes;

    public static void ShowWindow(AutoFenceCreator af, Vector2 mousePosition)
    {
        PresetNotesWindow window = GetWindow<PresetNotesWindow>("Preset Notes");
        window.position = new Rect(mousePosition.x + 100, mousePosition.y - 50, 600, 400); // Set initial position at the mouse position
        window.minSize = new Vector2(300, 200); // Set minimum size
        window.af = af;
        window.notes = af.presetNotes;
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter your notes:", EditorStyles.boldLabel);
        notes = EditorGUILayout.TextArea(notes, GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Save"))
        {
            af.presetNotes = notes;
            Close();
        }
    }

    private void OnDestroy()
    {
        af.presetNotes = notes;
    }
}

