using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

public static class StackTraceLogger
{
    public static void LogCallStack(bool clearConsole = false)
    {
        //return;
        if (clearConsole)
            ClearConsole();
        StackTrace stackTrace = new StackTrace(true); // 'true' to capture file names and line numbers
        string traceString = stackTrace.ToString();

        //UnityEngine.Debug.Log(traceString);

        List<string> traceList = ParseCallStack(traceString);

        PrettyPrint(traceList);
    }

    public static List<string> ParseCallStack(string stackTrace)
    {
        var parsedLines = new List<string>();
        var lines = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        string pattern = @"at\s+((?:[A-Za-z0-9]+\.)*)([A-Za-z0-9._]+)\.([A-Za-z0-9._]+)\s*\((.*)\)\s*[\[\]0-9a-fx]+";
        int alignmentPosition = 50; // Position at which the function name should start
        string methodNameToOmit = "LogCallStack";
        //string previousClassName = null;
        string lastSeenClassName = null;

        foreach (var line in lines)
        {
            var match = Regex.Match(line, pattern);
            if (match.Success)
            {
                string namespaceName = match.Groups[1].Value;
                string className = match.Groups[2].Value;
                string methodName = match.Groups[3].Value;
                string parameters = match.Groups[4].Value;

                // Skip adding the line if it's the call to LogCallStack itself
                if (methodName != methodNameToOmit)
                {
                    // Remove namespace if present
                    className = namespaceName.Length > 0 ? className : namespaceName + className;
                    lastSeenClassName = className;

                    string formattedClassName = className.PadRight(alignmentPosition - 1, ' ');
                    string formattedLine = formattedClassName + "." + methodName + "(" + parameters + ")";
                    parsedLines.Add(formattedLine);
                }
            }
        }

        // Reverse the list to show the flow from origin to current point
        parsedLines.Reverse();

        // Correct the class name for the first entry after reversal
        for (int i = 0; i < parsedLines.Count; i++)
        {
            if (i > 0 && parsedLines[i].Contains(lastSeenClassName))
            {
                parsedLines[i] = parsedLines[i].Replace(lastSeenClassName.PadRight(alignmentPosition - 1, ' '), new string(' ', alignmentPosition));
            }
            else
            {
                break; // Once we've corrected the first occurrence, we can stop the loop.
            }
        }

        return parsedLines;
    }

    public static void PrettyPrint(List<string> parsedStack)
    {
        foreach (var line in parsedStack)
        {
            UnityEngine.Debug.Log($"{line}  \n");
        }
    }

    public static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}