using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;

public static class LogFileUtility
{
    private const int MaxRetryCount = 5;
    private const int RetryDelayMilliseconds = 200;

    [MenuItem("Assets/Open Editor Log Location", false, 200)]
    public static void OpenEditorLogLocation()
    {
        string logFilePath = GetEditorLogPath();
        if (File.Exists(logFilePath))
        {
            Process.Start("explorer.exe", $"/select,\"{logFilePath}\"");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not find the editor log file.", "OK");
        }
    }

    [MenuItem("Assets/Backup, Clear, and Copy Console to Text File", false, 201)]
    public static void BackupClearAndCopyConsoleToTextFile()
    {
        string logFilePath = GetEditorLogPath();
        if (File.Exists(logFilePath))
        {
            string backupPath = GetBackupFilePath(logFilePath);
            if (BackupLogFile(logFilePath, backupPath))
            {
                string consoleOutput = ReadLogFile(logFilePath); // Read the log file first
                string separator = GenerateSeparator();
                WriteConsoleOutputToFile(separator + consoleOutput); // Write the console output to a text file after reading
                ClearLogFile(logFilePath);
                RestoreLogFile(logFilePath, backupPath); // Restore the original log file from the backup
                EditorUtility.DisplayDialog("Log File Processed", $"The editor log file has been backed up to:\n{backupPath}\nand the console output has been copied to ConsoleOutput.txt", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not find the editor log file.", "OK");
        }
    }

    private static bool BackupLogFile(string logFilePath, string backupPath)
    {
        return RetryFileOperation(() =>
        {
            using (var sourceStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var destinationStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                sourceStream.CopyTo(destinationStream);
            }
        });
    }

    private static string ReadLogFile(string logFilePath)
    {
        StringBuilder stringBuilder = new StringBuilder();
        RetryFileOperation(() =>
        {
            using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Filter out non-printable characters
                    stringBuilder.AppendLine(FilterNonPrintableCharacters(line));
                }
            }
        });
        return stringBuilder.ToString();
    }

    private static string FilterNonPrintableCharacters(string input)
    {
        var output = new StringBuilder();
        foreach (char c in input)
        {
            if (!char.IsControl(c) || c == '\r' || c == '\n' || c == '\t')
            {
                output.Append(c);
            }
        }
        return output.ToString();
    }

    private static void ClearLogFile(string logFilePath)
    {
        RetryFileOperation(() =>
        {
            using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                fileStream.SetLength(0);
            }
        });
    }

    private static void WriteConsoleOutputToFile(string consoleOutput)
    {
        string outputPath = "Assets/ConsoleOutput.txt";
        consoleOutput = NormalizeLineEndings(consoleOutput);
        RetryFileOperation(() =>
        {
            File.AppendAllText(outputPath, consoleOutput);
            AssetDatabase.Refresh();
        });
    }

    private static void RestoreLogFile(string logFilePath, string backupPath)
    {
        RetryFileOperation(() =>
        {
            using (var sourceStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var destinationStream = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                sourceStream.CopyTo(destinationStream);
            }
        });
    }

    private static bool RetryFileOperation(System.Action fileOperation)
    {
        int retryCount = 0;
        int delay = RetryDelayMilliseconds;
        while (true)
        {
            try
            {
                fileOperation();
                return true;
            }
            catch (IOException ex)
            {
                if (++retryCount >= MaxRetryCount)
                {
                    UnityEngine.Debug.LogError($"File operation failed after {MaxRetryCount} retries: {ex.Message}");
                    return false;
                }
                Thread.Sleep(delay);
                delay *= 2; // Exponential backoff
            }
        }
    }

    private static string NormalizeLineEndings(string input)
    {
        return input.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static string GetBackupFilePath(string logFilePath)
    {
        string directory = Path.GetDirectoryName(logFilePath);
        string fileName = Path.GetFileNameWithoutExtension(logFilePath);
        string extension = Path.GetExtension(logFilePath);
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(directory, $"{fileName}_{timestamp}{extension}");
    }

    private static string GetEditorLogPath()
    {
        string logPath = "";
#if UNITY_EDITOR_WIN
        logPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Unity", "Editor", "Editor.log");
#elif UNITY_EDITOR_OSX
        logPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Library", "Logs", "Unity", "Editor.log");
#elif UNITY_EDITOR_LINUX
        logPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ".config", "unity3d", "Editor.log");
#endif
        return logPath;
    }

    private static string GenerateSeparator()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return $"\n//==========================================\n//                      Next ({timestamp})\n//==========================================\n";
    }
}
