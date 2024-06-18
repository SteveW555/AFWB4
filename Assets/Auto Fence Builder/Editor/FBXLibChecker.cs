using System;

public static class FBXExporterChecker
{
    private static bool? _isFBXExporterAvailable;

    /// <summary>Checks if the FBX Exporter library is available.</summary>
    public static bool IsFBXExporterAvailable()
    {
        if (_isFBXExporterAvailable.HasValue)
            return _isFBXExporterAvailable.Value;

        try
        {
            // Try to get a type from the FBX Exporter library
            Type fbxExporterType = Type.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter, Unity.Formats.Fbx.Editor");
            _isFBXExporterAvailable = fbxExporterType != null;
        }
        catch
        {
            _isFBXExporterAvailable = false;
        }

        return _isFBXExporterAvailable.Value;
    }
}



