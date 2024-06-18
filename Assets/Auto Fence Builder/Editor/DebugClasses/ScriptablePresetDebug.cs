using AFWB;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * Used in Development only
 */

public struct GameObjectFields
{
    public string PostName;
    public string RailAName;
    public string RailBName;
    public string SubPostName;
    public string ExtraName;
    public List<string> PostSourceVariants;
    public List<string> RailASourceVariants;
    public List<string> RailBSourceVariants;
    //public List<string> SubpostSourceVariants; //v4.1
}

public struct ProblemPreset
{
    public string PresetName;
    public string CategoryName;
    public string MissingGameObjectName;
    public string MissingMeshOnGameObject;
    public string FieldName;
    public string ProblemDescriptionMessage;
    public string ProblemGameObject; // eg. "Post", "RailA", "RailB", "SubPost", "Extra", "PostVariant", "RailAVariant", "RailBVariant", "SubPostVariant", "ExtraVariant"
    public int ProblemVariantIndex;
    public int VariantIndex; // only if applicable
}

public class PresetChecker
{
    public static void TryRepairPreset(AutoFenceCreator af, List<ProblemPreset> problemPresetList)
    {
        // Look for file _PrefabMeshNames.txt in the Assets folder
        string[] files = System.IO.Directory.GetFiles(Application.dataPath, "_PrefabMeshNames.txt", System.IO.SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            Debug.Log("No _PrefabMeshNames.txt file found in Assets folder");
            return;
        }
        string filePath = files[0];
        string[] lines = System.IO.File.ReadAllLines(filePath);

        // create a dictionary where each key is the first word in a line, and the value is a list of the rest of the words in the line
        Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        foreach (string line in lines)
        {
            string[] words = line.Split(',');
            string key = words[0];
            List<string> values = new List<string>();
            for (int i = 1; i < words.Length; i++)
            {
                values.Add(words[i]);
            }
            dict.Add(key, values);
        }

        // for each ProblemPreset, try to find a prefab that matches the missing name
        foreach (ProblemPreset problemPreset in problemPresetList)
        {
            string missingName = problemPreset.MissingGameObjectName;
            string fieldName = problemPreset.FieldName;
            int variantIndex = problemPreset.VariantIndex;
            string presetName = problemPreset.PresetName;

            // if the missing name is a key in the dictionary, print the 2nd word in the line
            if (dict.ContainsKey(missingName))
            {
                List<string> values = dict[missingName];
                if (values.Count > 1)
                {
                    string meshName = values[0];
                    string meshFileName = values[1];
                    //print the preset name, the missingName, the meshName, and the meshFileName
                    Debug.Log(presetName + " " + fieldName + " " + missingName + " " + meshName + " " + meshFileName);
                }
            }
        }
    }

    public static List<ProblemPreset> PrintPresetsWithMissingPrefabs(AutoFenceCreator af, List<ScriptablePresetAFWB> presetList)
    {
        List<GameObject> allPrefabs = PrefabsDebug.CombinePrefabsLists(af.railPrefabs, af.postPrefabs, af.extraPrefabs);

        List<GameObjectFields> golist = PresetChecker.ExtractGameObjectFieldsFromPresetList(presetList);
        List<string> missingPrefabNames = PresetChecker.GetMissingGameObjectNames(golist, allPrefabs);

        List<ProblemPreset> problemPresets = FindPresetsWithMissingPrefabNames(missingPrefabNames, presetList);

        foreach (ProblemPreset problemPreset in problemPresets)
        {
            string problemStr = "Preset " + " [" + problemPreset.CategoryName + "/" + problemPreset.PresetName + "]  is missing Prefab:   [" + problemPreset.MissingGameObjectName + "]  in  [" + problemPreset.FieldName + "]";
            if (problemPreset.FieldName.Contains("Variant"))
                problemStr += problemPreset.VariantIndex;
            Debug.Log(problemStr + "\n");
        }
        return problemPresets;
    }

    //-------------------------------------------------
    //Finds all presetsEd that contain a substring and changes their category name
    public static List<ScriptablePresetAFWB> ChangePresetCategoryName(AutoFenceCreator af, List<ScriptablePresetAFWB> presetList, string presetSubtring, string oldCatName, string newCatName)
    {
        List<ScriptablePresetAFWB> changedPresets = new List<ScriptablePresetAFWB>();

        // For each Preset in the list, if the category name matches the old category name, change it to the new category name
        foreach (ScriptablePresetAFWB preset in presetList)
        {
            if (preset.name.Contains(presetSubtring) && preset.categoryName == oldCatName)
            {
                preset.categoryName = newCatName;
                changedPresets.Add(preset);

                //if a directory with the new category name does not exist in af.currPresetsDir, create it
                string newCatPath = af.currPresetsDir + "/" + newCatName;
                if (!System.IO.Directory.Exists(newCatPath))
                {
                    System.IO.Directory.CreateDirectory(newCatPath);
                }

                //Move the preset to the new category directory using FileUtil.MoveFileOrDirectory
                string oldCatPath = af.currPresetsDir + "/" + oldCatName;
                string presetPath = oldCatPath + "/" + preset.name + ".asset";
                string newPresetPath = newCatPath + "/" + preset.name + ".asset";
                FileUtil.MoveFileOrDirectory(presetPath, newPresetPath);
            }
        }
        AssetDatabase.Refresh();

        return changedPresets;
    }

    //-------------------------------------------------
    //Get a list of all GameObject or GameObject.names in a ScriptablePresetAFWB> presetsList)
    // GameObjectFields is all the Main and SourceVaraint names in a ScriptablePresetAFWB
    public static List<GameObjectFields> ExtractGameObjectFieldsFromPresetList(List<ScriptablePresetAFWB> presetsList)
    {
        List<GameObjectFields> gameObjectFieldsList = new List<GameObjectFields>();

        foreach (ScriptablePresetAFWB preset in presetsList)
        {
            GameObjectFields gameObjectFields = ExtractGameObjectFieldsFromPreset(preset);
            gameObjectFieldsList.Add(gameObjectFields);
        }
        return gameObjectFieldsList;
    }

    //-------------------------------------------------
    // Get the names of all GameObjects associated with this preset
    public static GameObjectFields ExtractGameObjectFieldsFromPreset(ScriptablePresetAFWB preset)
    {
        GameObjectFields gameObjectFields = new GameObjectFields
        {
            PostName = preset.postName,
            RailAName = preset.railAName,
            RailBName = preset.railBName,
            SubPostName = preset.subpostName,
            ExtraName = preset.extraName,
            PostSourceVariants = ExtractGameObjectNamesFromSourceVariants(preset.postVariants),
            //RailASourceVariants = ExtractGameObjectNamesFromSourceVariants(preset.railAVariants),
            RailBSourceVariants = ExtractGameObjectNamesFromSourceVariants(preset.railBVariants),
            //SubpostSourceVariants = ExtractGameObjectNamesFromSourceVariants(preset.subpostVariants),
        };

        return gameObjectFields;
    }

    //-------------------------------------------------
    // Get the names of all GameObjects in a list of SourceVariants
    private static List<string> ExtractGameObjectNamesFromSourceVariants(List<SourceVariant> SourceVariants)
    {
        List<string> gameObjectNames = new List<string>();

        foreach (SourceVariant sourceVariant in SourceVariants)
        {
            if (sourceVariant.Go != null)
            {
                gameObjectNames.Add(sourceVariant.Go.name);
            }
        }
        return gameObjectNames;
    }

    //-------------------------------------------------
    // Look for missing prefabs referenced by ScriptablePresetAFWB either directly or by goName
    public static List<string> GetMissingGameObjectNames(List<GameObjectFields> gameObjectFieldsList, List<GameObject> goList)
    {
        HashSet<string> goNames = new HashSet<string>();
        List<string> missingNames = new List<string>();

        foreach (GameObject go in goList)
        {
            goNames.Add(go.name);
        }

        foreach (GameObjectFields goFields in gameObjectFieldsList)
        {
            CheckAndAddMissingName(goNames, missingNames, goFields.PostName);
            CheckAndAddMissingName(goNames, missingNames, goFields.RailAName);
            CheckAndAddMissingName(goNames, missingNames, goFields.RailBName);
            CheckAndAddMissingName(goNames, missingNames, goFields.SubPostName);
            CheckAndAddMissingName(goNames, missingNames, goFields.ExtraName);

            foreach (string postVariant in goFields.PostSourceVariants)
            {
                CheckAndAddMissingName(goNames, missingNames, postVariant);
            }

            foreach (string railAVariant in goFields.RailASourceVariants)
            {
                CheckAndAddMissingName(goNames, missingNames, railAVariant);
            }

            foreach (string railBVariant in goFields.RailBSourceVariants)
            {
                CheckAndAddMissingName(goNames, missingNames, railBVariant);
            }

            //V 4.1
            //    foreach (string subPostVariant in goFields.SubpostSourceVariants)
            //    {
            //        CheckAndAddMissingName(goNames, missingNames, subPostVariant);
            //    }
        }

        return missingNames;
    }

    public static void CheckAndAddMissingName(HashSet<string> goNames, List<string> missingNames, string name)
    {
        if (!goNames.Contains(name) && !missingNames.Contains(name))
        {
            missingNames.Add(name);
        }
    }

    public static List<ProblemPreset> FindPresetsWithMissingPrefabNames(List<string> missingNames, List<ScriptablePresetAFWB> presets)
    {
        List<ProblemPreset> problemPresets = new List<ProblemPreset>();

        // For each Preset
        for (int i = 0; i < presets.Count; i++)
        {
            ScriptablePresetAFWB preset = presets[i];
            GameObjectFields gameObjectFields = ExtractGameObjectFieldsFromPreset(preset);
            List<string> currentPresetMissingNames = new List<string>();

            // Check each missing goName
            foreach (string missingGoName in missingNames)
            {
                (string fieldName, int variantIndex) = CheckGameObjectFieldsForName(gameObjectFields, missingGoName);

                if (fieldName != "")
                {
                    problemPresets.Add(new ProblemPreset { PresetName = preset.name, MissingGameObjectName = missingGoName, CategoryName = preset.categoryName, FieldName = fieldName, VariantIndex = variantIndex });
                }
            }
        }
        return problemPresets;
    }

    //checks if any of the GameObjects or GameObject names contain goName
    // return the field name, and if applicable, the variant index
    public static (string, int) CheckGameObjectFieldsForName(GameObjectFields fields, string goName)
    {
        if (fields.PostName == goName)
        {
            return ("PostName", -1);
        }
        else if (fields.RailAName == goName)
        {
            return ("RailAName", -1);
        }
        else if (fields.RailBName == goName)
        {
            return ("RailBName", -1);
        }
        else if (fields.SubPostName == goName)
        {
            return ("SubPostName", -1);
        }
        else if (fields.ExtraName == goName)
        {
            return ("ExtraName", -1);
        }
        else if (fields.PostSourceVariants.Contains(goName))
        {
            int index = fields.PostSourceVariants.IndexOf(goName);
            return ("PostSourceVariants", index);
        }
        else if (fields.RailASourceVariants.Contains(goName))
        {
            int index = fields.RailASourceVariants.IndexOf(goName);
            return ("RailASourceVariants", index);
        }
        else if (fields.RailBSourceVariants.Contains(goName))
        {
            int index = fields.RailBSourceVariants.IndexOf(goName);
            return ("RailBSourceVariants", index);
        }
        // v4.1
        //else if (fields.SubpostSourceVariants.Contains(goName))
        //{
        //    int index = fields.SubpostSourceVariants.IndexOf(goName);
        //    return ("SubpostSourceVariants", index);
        //}

        return ("", -1);
    }
}