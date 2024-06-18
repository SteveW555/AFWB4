using System.Collections.Generic;
using UnityEngine;

public class FenceCloner
{
    public List<Vector3> GetClickPointsFromFence(GameObject fenceToCopyFrom)
    {
        List<Vector3> clickPoints = new List<Vector3>();
        Transform[] allChildren = fenceToCopyFrom.GetComponentsInChildren<Transform>(true);
        //int count = allChildren.Length;
        foreach (Transform child in allChildren)
        {
            string name = child.gameObject.name;
            if (name.Contains("_click"))
            {
                //print(name);
                clickPoints.Add(child.position);
            }
        }
        return clickPoints;
    }

    public List<Vector3> GetGapPointsFromFence(GameObject fenceToCopyFrom)
    {
        List<Vector3> gapPoints = new List<Vector3>();
        Transform[] allChildren = fenceToCopyFrom.GetComponentsInChildren<Transform>(true);
        //int count = allChildren.Length;
        foreach (Transform child in allChildren)
        {
            string name = child.gameObject.name;
            if (name.Contains("_gap"))
            {
                //print(name);
                gapPoints.Add(child.position);
            }
        }
        return gapPoints;
    }
}