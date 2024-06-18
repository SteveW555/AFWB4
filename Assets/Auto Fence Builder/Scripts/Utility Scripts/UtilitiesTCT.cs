using AFWB;
using System;
using System.Collections.Generic;
using TCT.PrintUtils;

using UnityEngine;

public class RaycastHitInfo
{
    public string ObjectName { get; set; }
    public string ColliderTypes { get; set; }
    public float Distance { get; set; }
    public RaycastHitInfo ObjectBeneath { get; set; }
}

public static class UtilitiesTCT
{
    //-----------------------------------------
    // Debug. Raycast from all post and looks for problems with hit objects, multiple colliders, and sneaky AFWB objects beneath
    public static void AnalyzeHitsAndColliders(AutoFenceCreator af)
    {
        List<float> hitDistances = new List<float>();
        List<RaycastHitInfo> hitInfos = GetHitObjectNamesAndDistances(af.postsPool);
        PrintUtilities.PrettyPrintRaycastInfo(hitInfos);

        List<RaycastHitInfo> problems = GetFilteredRaycastHitInfos(hitInfos);
        Debug.Log("Problems:  ------------------------ \n");
        PrintUtilities.PrettyPrintRaycastInfo(problems);
    }

    //--------------------------------------------------------------------
    public static List<RaycastHitInfo> GetHitObjectNamesAndDistances(List<Transform> transforms)
    {
        List<RaycastHitInfo> hitInfos = new List<RaycastHitInfo>();

        foreach (var transform in transforms)
        {
            if (!transform.gameObject.activeSelf)
            {
                continue;
            }

            RaycastHit hit;
            Vector3 pos = transform.position + new Vector3(0f, 0.2f, 0f);
            if (Physics.Raycast(pos, Vector3.down, out hit))
            {
                var hitInfo = new RaycastHitInfo
                {
                    ObjectName = hit.collider.gameObject.name,
                    ColliderTypes = string.Join(", ", Array.ConvertAll(hit.collider.gameObject.GetComponents<Collider>(), c => "-" + c.GetType().Name)),
                    Distance = hit.distance,
                    ObjectBeneath = null // To be set after the second raycast
                };

                // Perform a secondary raycast from just below the first hit point
                Vector3 secondRayStart = hit.point - Vector3.up * 0.1f; // Slightly below the hit point
                RaycastHit hitBeneath;
                if (Physics.Raycast(secondRayStart, Vector3.down, out hitBeneath))
                {
                    hitInfo.ObjectBeneath = new RaycastHitInfo
                    {
                        ObjectName = hitBeneath.collider.gameObject.name,
                        ColliderTypes = string.Join(", ", Array.ConvertAll(hitBeneath.collider.gameObject.GetComponents<Collider>(), c => "-" + c.GetType().Name)),
                        Distance = hitBeneath.distance
                    };
                }

                hitInfos.Add(hitInfo);
            }
        }

        return hitInfos;
    }

    public static List<RaycastHitInfo> GetFilteredRaycastHitInfos(List<RaycastHitInfo> hitInfos)
    {
        List<RaycastHitInfo> filteredHitInfos = new List<RaycastHitInfo>();
        string[] keywords = { "post", "rail", "panel", "sub", "extra" }; // List of keywords

        foreach (var hitInfo in hitInfos)
        {
            if (hitInfo.ObjectBeneath != null)
            {
                // Check if both hit object and beneath object contain any of the keywords
                if (ContainsAnyKeyword(hitInfo.ObjectName, keywords) || ContainsAnyKeyword(hitInfo.ObjectBeneath.ObjectName, keywords))
                {
                    // Add to the filtered list
                    filteredHitInfos.Add(hitInfo);
                }
            }
        }
        return filteredHitInfos;
    }

    public static bool ContainsAnyKeyword(string name, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }
}// --end class