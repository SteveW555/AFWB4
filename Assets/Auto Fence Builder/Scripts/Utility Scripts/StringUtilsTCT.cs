using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Globalization;


/// <summary>
/// Two methods and a combo to assess the similarity between two strings
/// </summary>
public enum SimilarityMetric
{
    Levenshtein,
    Jaccard,
    Both
}

public static class StringUtilsTCT
{
    /// <summary>
    /// Filters a list of strings, returning only those that end with the specified substring, with an option for case sensitivity.
    /// </summary>
    /// <param name="inputList">The list of strings to filter.</param>
    /// <param name="endingSubstring">The substring that each string must end with to be included in the returned list.</param>
    /// <param name="caseSensitive">Determines whether the search should be case-sensitive. Default is true.</param>
    /// <returns>A List<string> containing only the strings from the input list that end with the specified substring.</returns>
    public static List<string> FilterByEnding(List<string> inputList, string endingSubstring, bool caseSensitive = false)
    {
        if (string.IsNullOrEmpty(endingSubstring))
            return new List<string>();  // Return an empty list if the ending substring is null or empty
        List<string> filteredList;
        if (caseSensitive)
        {
            filteredList = inputList
                .Where(item => item.EndsWith(endingSubstring))
                .ToList();
        }
        else
        {
            filteredList = inputList
                .Where(item => item.EndsWith(endingSubstring, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        return filteredList;
    }
    //-----------
    // Performs a case-insensitive search and replaces all occurrences of 'search' with 'replacement' in the 'input' string.

    public static string ReplaceCaseInsensitive(string input, string search, string replacement)
    {
        // Check for null or empty input to avoid errors
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search))
            return input;

        string lowerInput = input.ToLower(CultureInfo.InvariantCulture);
        string lowerSearch = search.ToLower(CultureInfo.InvariantCulture);
        int index = 0;

        // Loop to find all occurrences of the search string
        while ((index = lowerInput.IndexOf(lowerSearch, index, StringComparison.InvariantCulture)) != -1)
        {
            // Remove the found string and insert the replacement
            input = input.Substring(0, index) + replacement + input.Substring(index + search.Length);
            lowerInput = lowerInput.Substring(0, index) + replacement.ToLower(CultureInfo.InvariantCulture) + lowerInput.Substring(index + search.Length);

            // Move index forward to continue search
            index += replacement.Length;
        }

        return input;
    }

    static public string GetNumberAtEndOfString(string str)
    {
        string result = string.Concat(str.ToArray().Reverse().TakeWhile(char.IsNumber).Reverse());
        return result;
    }

    // will append or increment a number at end of string
    static public string IncrementString(string str, bool addNumberifNonePresent = true)
    {
        //does it already end with a string

        string numStr = string.Concat(str.ToArray().Reverse().TakeWhile(char.IsNumber).Reverse()); // num at end of string
        if (numStr == "" && addNumberifNonePresent)
            str += "1";
        else
        {
            int numLen = numStr.Length;

            int num = int.Parse(numStr);
            string incStr = (num + 1).ToString();

            str = str.Remove(str.Length - numLen);
            str += incStr;

        }
        return str;
    }
    static public string GetPrefabTypeString(AFWB.PrefabTypeAFWB prefabType, bool includeUnderscore = true)
    {
        string layerStr = "";
        if (includeUnderscore)
            layerStr = "_";

        if (prefabType == AFWB.PrefabTypeAFWB.postPrefab)
            layerStr += "Post";
        if (prefabType == AFWB.PrefabTypeAFWB.railPrefab)
            layerStr += "Rail";
        if (prefabType == AFWB.PrefabTypeAFWB.extraPrefab)
            layerStr += "Extra";

        return layerStr;
    }
    static public string RemoveSubstring(string sourceString, string substring)
    {
        int index = sourceString.IndexOf(substring);
        string resultStr = (index < 0) ? sourceString : sourceString.Remove(index, substring.Length);
        return resultStr;
    }
    public static bool StringContainsAutoFencePart(string name)
    {
        return name.Contains("_Panel") || name.Contains("_Rail") ||
               name.Contains("_Post") ||
               name.Contains("_Extra") ||
               name.Contains("_Sub");
    }
    //---------------------------------
    // Useful for creating abbreviated Names
    public static string RemoveVowels(string input, bool leaveFiirstLetterUnmodified)
    {
        StringBuilder result = new StringBuilder();
        char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (i == 0 || Array.IndexOf(vowels, c) == -1) // Keep the first character or if the character is not a vowel
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
    //Converts a camel-case enum value and converts to a pretty string e.g . "myEnumValue" -> "My Enum Value"
    public static string EnumToReadableString(this Enum enumValue)
    {
        // Handle underscore-separated names first
        string readableString = Regex.Replace(enumValue.ToString(), "_", " ");

        // Add a space before each uppercase letter, except for the first character
        readableString = Regex.Replace(readableString, "(?<!^)([A-Z])", " $1");

        // Capitalize each word
        readableString = string.Join(" ", readableString.Split(' ').Select(word =>
        {
            if (word.Length > 1)
                return char.ToUpper(word[0]) + word.Substring(1).ToLower();
            return word.ToUpper(); // Ensure single-letter words are capitalized too
        }));

        return readableString;
    }

    /// <summary>
    /// Calculates the closest match using the Levenshtein distance between two strings.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <returns>The Levenshtein distance between the two strings.</returns>
    /// <remarks> The Levenshtein distance measures the number of single-character edits (insertions, deletions, or substitutions) 
    /// required to change one word into another. The lower the distance, the more similar the strings are.</remarks>
    public static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return string.IsNullOrEmpty(b) ? 0 : b.Length;
        if (string.IsNullOrEmpty(b)) return a.Length;

        int[,] costs = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            costs[i, 0] = i;
        for (int j = 0; j <= b.Length; j++)
            costs[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                costs[i, j] = Math.Min(Math.Min(costs[i - 1, j] + 1, costs[i, j - 1] + 1), costs[i - 1, j - 1] + cost);
            }
        }

        return costs[a.Length, b.Length];
    }
    /// <summary>
    /// Calculates the Levenshtein distances between the given name and a list of names.
    /// </summary>
    /// <param name="names">The list of names to compare against.</param>
    /// <param name="targetName">The name to compare.</param>
    /// <returns>A list of tuples containing the index and distance for each name.</returns>
    public static List<(int Index, int Distance)> CalculateDistances(List<string> names, string targetName)
    {
        // Create a list to hold the distances and their corresponding indices
        return names.Select((name, index) => (Index: index, Distance: LevenshteinDistance(name, targetName))).ToList();
    }


    /// <summary>
    /// Finds the indices and distances of the closest names based on Levenshtein distance to the given target name.
    /// </summary>
    /// <param name="names">The list of names to search.</param>
    /// <param name="targetName">The name to find the closest matches to.</param>
    /// <param name="numToFind">The number of closest matches to find.</param>
    /// <returns>
    /// A list of tuples containing the indices and distances of the closest matched names.
    /// If <paramref name="numToFind"/> is greater than the number of elements in <paramref name="names"/>, 
    /// all elements are returned.
    /// </returns>
    public static List<(int Index, int Distance)> FindTopClosestStringMatches(List<string> names, string targetName, int numToFind)
    {
        // Calculate the distances
        var distances = CalculateDistances(names, targetName);

        // Order by distance and take the top closest matches
        return distances.OrderBy(x => x.Distance).Take(numToFind).ToList();
    }

    //================================================================================================


    /// <summary>
    /// Normalizes the Levenshtein distance to a similarity score between 0 and 1.
    /// </summary>
    /// <param name="levenshteinDistance">The Levenshtein distance between two strings.</param>
    /// <param name="maxLength">The length of the longer of the two strings.</param>
    /// <returns>The normalized Levenshtein similarity score.</returns>
    public static double NormalizeLevenshtein(int levenshteinDistance, int maxLength)
    {
        return 1.0 - ((double)levenshteinDistance / maxLength);
    }


    /// <summary>
    /// Calculates both Levenshtein distances and Jaccard indices between the given name and a list of names, and normalizes them.
    /// </summary>
    /// <param name="names">The list of names to compare against.</param>
    /// <param name="targetName">The name to compare.</param>
    /// <param name="n">The length of the jaccardNGramLength-grams for Jaccard index.</param>
    /// <returns>A list of tuples containing the index, normalized Levenshtein similarity, and Jaccard index for each name.</returns>
    public static List<(int Index, double NormalizedLevenshtein, double JaccardIndex)> CalculateDistancesIncludingJaccard(List<string> names, string targetName, int n)
    {
        int maxLength = Math.Max(targetName.Length, names.Max(name => name.Length));

        // Create a list to hold the distances, indices, and Jaccard indices
        return names.Select((name, index) => (
            Index: index,
            NormalizedLevenshtein: NormalizeLevenshtein(LevenshteinDistance(name, targetName), maxLength),
            JaccardIndex: JaccardIndex(name, targetName, n)
        )).ToList();
    }

    //=========================     Jaccard     ================================
    
    // <summary>
    /// Calculates the Jaccard indices between the given name and a list of names using jaccardNGramLength-grams.
    /// </summary>
    /// <param name="names">The list of names to compare against.</param>
    /// <param name="targetName">The name to compare.</param>
    /// <param name="jaccardNGramLength">The length of the jaccardNGramLength-grams for Jaccard index.</param>
    /// <returns>A list of tuples containing the index and Jaccard index for each name.</returns>
    public static List<(int Index, double JaccardIndex)> CalculateJaccardIndices(List<string> names, string targetName, int jaccardNGramLength)
    {

        List<(int Index, double JaccardIndex)> topJaccardList = new List<(int Index, double JaccardIndex)>();

        for (int index = 0; index < names.Count; index++)
        {
            string name = names[index];
            double jaccardIndex = JaccardIndex(name, targetName, jaccardNGramLength);
            topJaccardList.Add((Index: index, JaccardIndex: jaccardIndex));
        }

        return topJaccardList;
    }
    //---------------------------------------------------

    /// <summary>
    /// Generates a set of n-grams for a given string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="n">The length of the n-grams.</param>
    /// <returns>A set of n-grams.</returns>
    public static HashSet<string> GetNGrams(string input, int n)
    {
        var nGrams = new HashSet<string>();

        for (int i = 0; i <= input.Length - n; i++)
        {
            nGrams.Add(input.Substring(i, n));
        }

        return nGrams;
    }

    /// <summary>
    /// Calculates the Jaccard index between two strings using n-grams.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <param name="n">The length of the n-grams.</param>
    /// <returns>The Jaccard index between the two strings.</returns>
    public static double JaccardIndex(string a, string b, int n)
    {
        var nGramsA = GetNGrams(a, n);
        var nGramsB = GetNGrams(b, n);

        var intersection = new HashSet<string>(nGramsA);
        intersection.IntersectWith(nGramsB);

        var union = new HashSet<string>(nGramsA);
        union.UnionWith(nGramsB);

        return (double)intersection.Count / union.Count;
    }


    /*/// <summary>
    /// Generates a set of jaccardNGramLength-grams for a given string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="jaccardNGramLength">The length of the jaccardNGramLength-grams.</param>
    /// <returns>A set of jaccardNGramLength-grams.</returns>
    public static HashSet<string> GetNGrams(string input, int jaccardNGramLength)
    {
        var nGrams = new HashSet<string>();
        for (int i = 0; i <= input.Length - jaccardNGramLength; i++)
        {
            nGrams.Add(input.Substring(i, jaccardNGramLength));
        }
        return nGrams;
    }

    /// <summary>
    /// Calculates the Jaccard index between two strings using jaccardNGramLength-grams.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <param name="jaccardNGramLength">The length of the jaccardNGramLength-grams.</param>
    /// <returns>The Jaccard index between the two strings.</returns>
    public static double JaccardIndex(string a, string b, int jaccardNGramLength)
    {
        var nGramsA = GetNGrams(a, jaccardNGramLength);
        var nGramsB = GetNGrams(b, jaccardNGramLength);

        var intersection = new HashSet<string>(nGramsA);
        intersection.IntersectWith(nGramsB);

        var union = new HashSet<string>(nGramsA);
        union.UnionWith(nGramsB);

        double result = (double)intersection.Count / union.Count;

        //Debug.Log($"Jaccard Index between '{a}' and '{b}' with jaccardNGramLength = {jaccardNGramLength}:        {result}");

        return result;
    }*/

    //================================================================================================


    /// <summary>
    /// Calculates the Levenshtein distances between the given name and a list of names.
    /// </summary>
    /// <param name="names">The list of names to compare against.</param>
    /// <param name="targetName">The name to compare.</param>
    /// <returns>A list of tuples containing the index and distance for each name.</returns>
    public static List<(int Index, int Distance)> CalculateLevenshteinDistances(List<string> names, string targetName)
    {
        return names.Select((name, index) => (Index: index, Distance: LevenshteinDistance(name, targetName))).ToList();
    }


    /// <summary>
    /// Finds the indices and distances of the closest names based on the chosen similarity metric.
    /// </summary>
    /// <param name="names">The list of names to search.</param>
    /// <param name="targetName">The name to find the closest matches to.</param>
    /// <param name="numToFind">The number of closest matches to find.</param>
    /// <param name="metric">The similarity metric to use.</param>
    /// <param name="n">The length of the n-grams for Jaccard index (optional, used only if metric is Jaccard or Both).</param>
    /// <returns>A list of tuples containing the indices and distances based on the chosen similarity metric.</returns>
    public static List<(int Index, double Score)> FindTopClosestStringMatches(List<string> names, string targetName, int numToFind, SimilarityMetric metric, int n = 3)
    {
        List<(int Index, double Score)> results;

        switch (metric)
        {
            case SimilarityMetric.Levenshtein:
                results = CalculateLevenshteinDistances(names, targetName)
                    .Select(x => (x.Index, Score: (double)x.Distance))
                    .OrderBy(x => x.Score)
                    .Take(numToFind)
                    .ToList();
                break;

            case SimilarityMetric.Jaccard:
                results = CalculateJaccardIndices(names, targetName, n)
                    .Select(x => (x.Index, Score: x.JaccardIndex)) // Jaccard similarity, higher is better
                    .OrderByDescending(x => x.Score)
                    .Take(numToFind)
                    .ToList();
                break;

            case SimilarityMetric.Both:
                results = CalculateDistancesIncludingJaccard(names, targetName, n)
                    .Select(x => (x.Index, Score: (x.NormalizedLevenshtein + x.JaccardIndex) / 2.0))
                    .OrderByDescending(x => x.Score)
                    .Take(numToFind)
                    .ToList();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(metric), metric, null);
        }

        return results;
    }













}
