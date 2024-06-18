using AFWB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Used in Development only
 */

public class PrefabsDebug
{
    //TODO

    /*private static void GetAllPrefabs(AutoFenceCreator  af, List<int> list1, List<int> list2, List<int> list3)
    {
        List<int> combinedList = list1.Concat(list2).Concat(list3).ToList();

        foreach (int item in combinedList)
        {
            Console.WriteLine(item);
        }
    }*/

    // Call:e.g. List<GameObject> allPrefabs = PrefabsDebug.CombinePrefabsLists(af.railPrefabs, af.postPrefabs, af.extraPrefabs);
    public static List<GameObject> CombinePrefabsLists(params List<GameObject>[] lists)
    {
        return lists.SelectMany(list => list).ToList();
    }

}
