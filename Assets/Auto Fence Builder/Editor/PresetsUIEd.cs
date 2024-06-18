using AFWB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// PresetsUIEd.cs
/// Part of AutoFenceEditor handling Preset UI functionality.
/// </summary>
public partial class AutoFenceEditor
{
    private string presetFilterString = ""; // to filter the preset list
    private List<string> filteredPresetNames = new List<string>(); //-- the allPresetMenuNames after filtering with presetFilterString
    List<string> displayMenuNames = new List<string>();
    int realPresetMenuIndexForLayer = 0;
    public void ShowPresetsUI()
    {
        //============================================================================================================================
        //
        //                         Presets:      Choose or Save Scriptable Main Fence/Wall Preset
        //
        //============================================================================================================================
        if (allPresetMenuNames != null && allPresetMenuNames.Count > 0 && mainPresetList != null && mainPresetList.Count > 0)
        {
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
            List<string> fullPresetMenuNames = allPresetMenuNames;
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
            //af.currPresetIndex = EditorGUILayout.Popup(af.currPresetIndex, allPresetMenuNames.ToArray(), greenPopupStyle, GUILayout.Width(338));
            af.presetDisplayMenuIndex = EditorGUILayout.Popup(af.presetDisplayMenuIndex, displayMenuNames.ToArray(), greenPopupStyle, GUILayout.Width(338));


            //-- Finf the preset withis name from the filtered list
            string scriptablePresetName = displayMenuNames[af.presetDisplayMenuIndex];
            scriptablePresetName = GetPresetNameWithoutCategory(scriptablePresetName);
            //ScriptablePresetAFWB currPreset = mainPresetList.Find(p => p.name == scriptablePresetName);

            af.currPresetIndex = mainPresetList.FindIndex(p => p.name == scriptablePresetName);
            //ScriptablePresetAFWB currPreset = presetIndex != -1 ? mainPresetList[presetIndex] : null;



            GUI.backgroundColor = Color.white;
            if (af.currPresetIndex < allPresetMenuNames.Count)
                scriptablePresetName = allPresetMenuNames[af.currPresetIndex];
            GUILayout.Space(7);
            GUI.backgroundColor = moduleBgColor;

       
            //-- Favourite. Resave a copy in Favorites folder
            if (GUILayout.Button("Fave", GUILayout.Width(40)))
            {
                if (scriptablePresetName.Contains("/"))
                    scriptablePresetName = scriptablePresetName.Replace("/", "-");
                af.scrPresetSaveName = "Favorite/" + af.scrPresetSaveName;
                presetsEd.SavePreset(true);
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
            if(af.presetNotes != "")
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
            //      Setup Loaded Preset
            //======================================
            if (EditorGUI.EndChangeCheck())
            {
                af.currPresetName = scriptablePresetName;
                presetsEd.SetupPreset(af.currPresetIndex, forceRebuild: false);
                af.ForceRebuildFromClickPoints();
            }
            GUILayout.Space(8);

            //===========================================================================
            //                             Save Preset
            //===========================================================================
            bool saved = DisplaySavePresetControls();
            if (saved)
            {
                af.ForceRebuildFromClickPoints();
            }

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

        //      Name Preset 
        //==============================
        saveLabelColor = bgColor;
        saveLabelColor.r += 0.95f;
        saveLabelColor.g += 0.79f;
        saveLabelColor.b += 0.69f;
        saveLabelColor.a = 0.1f;
        GUI.backgroundColor = saveLabelColor;
        EditorGUILayout.LabelField("  Name Preset: ", GUILayout.Width(85));

        GUIStyle greenLabelStyle = new GUIStyle(EditorStyles.textField);
        greenLabelStyle.normal.textColor = new Color(0.77f, .81f, .73f);
        GUI.backgroundColor = bgColor;
        af.scrPresetSaveName = EditorGUILayout.TextField(af.scrPresetSaveName, greenLabelStyle, GUILayout.Width(215));


        //      New Preset
        //==============================
        GUILayout.Space(55);
        if (GUILayout.Button(new GUIContent("New Preset", "Sets Everything to Default Values."), GUILayout.Width(87)))
        {
            CreateNewPreset();
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
        foreach (string s in allPresetMenuNames)
        {
            if (s.ToLower().Contains(filterString.ToLower()))
                filteredList.Add(s);
        }
        if (filteredList.Count == 0)
        {
            //Debug.Log("No presets found with filter: " + filterString);
            // return allPresetMenuNames;
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
        af.presetMenuIndexInDisplayList = allPresetMenuNames.Count - 1;
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
        af.presetMenuIndexInDisplayList = allPresetMenuNames.Count - 1;
        bool savedNew = presetsEd.SavePreset(false);
    }

    //----------------
    private List<string> GetFilteredPresetsList(string filterString)
    {

        af.ClearConsole();


        List<string> filteredList = new List<string>();
        /*foreach (string s in allPresetMenuNames)
        {
            if (s.ToLower().Contains(filterString.ToLower()))
                filteredList.Add(s);

        }*/


        // Assuming prefabs is a List<Prefab> and you have a method StripPrefabTypeFromNameForType
        string targetPresetName = filterString; // Already stripped
        List<string> presetNames = allPresetMenuNames;
        List<string> presetMenuNamesWithoutCategory = allPresetMenuNames.Select(name => name.Substring(name.IndexOf('/') + 1)).ToList();


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
        if (slashIndex != -1 && slashIndex + 1 < input.Length)
        {
            return input.Substring(slashIndex + 1);
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
        window.position = new Rect(mousePosition.x + 100, mousePosition.y-50, 600, 400); // Set initial position at the mouse position
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

