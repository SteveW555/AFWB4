using AFWB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableClickPoints : ScriptableObject
{
    public List<Vector3> clickPoints;
    public List<Vector3> gapPoints;

    public static ScriptableClickPoints CreateData(GameObject sourceFence, AutoFenceCreator af)
    {
        ScriptableClickPoints scriptableClickPoints = ScriptableObject.CreateInstance<ScriptableClickPoints>();

        FenceCloner fenceCloner = new FenceCloner();

        if (sourceFence != null)
        {
            scriptableClickPoints.clickPoints = fenceCloner.GetClickPointsFromFence(sourceFence);
            scriptableClickPoints.gapPoints = fenceCloner.GetGapPointsFromFence(sourceFence);
        }
        /*if (copiedClickPoints != null)
        {
            af.CopyLayoutFromScriptableClickPoints(copiedClickPoints, copiedGapPoints);
        }*/

        return scriptableClickPoints;
    }

    //------------------
    public static ScriptableClickPoints LoadScriptableClickPoints(string name, AutoFenceCreator af)
    {
        ScriptableClickPoints scriptableClickPoints = null;
        string loadPath = af.currAutoFenceBuilderDir + "/FinishedData/" + name;

        string[] filePaths = Directory.GetFiles(loadPath);
        foreach (string filePath in filePaths)
        {
            //string filename = "ClickPoints-" + name;
            string endStr = name + ".asset";
            if (filePath.EndsWith(endStr))
            {
                scriptableClickPoints = AssetDatabase.LoadAssetAtPath(filePath, typeof(ScriptableClickPoints)) as ScriptableClickPoints;
                if (scriptableClickPoints != null)
                    break;
            }
        }
        return scriptableClickPoints;
    }

    //--------------
    public static bool SaveScriptableClickPoints(AutoFenceCreator af, ScriptableClickPoints clickPoints, string path, string name)
    {
        string savePath = path + "/" + name + ".asset";

        if (File.Exists(savePath))
        {
            AssetDatabase.CreateAsset(clickPoints, savePath);
        }
        else
        {
            try
            {
                AssetDatabase.CreateAsset(clickPoints, savePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Problem in  SaveScriptableClickPoints() " + e.ToString() + " \n");
                return false;
            }
        }
        AssetDatabase.SaveAssets();
        return true;
    }
}