using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TCT.PrintUtils
{
    public static class PrintUtilities
    {
        /*public static void PrintList<T>(List<T> list, string header, bool removeUnityType = true, string itemPrefix = "", 
            bool allInOneLine = true, string hint = "", int printFirstN = 0)
        {
            if (header != "")
                Debug.Log($"\n{header} [{list.Count}] :\n\n");
            else
                Debug.Log($"\n");

            if (allInOneLine)
            {
                string itemString = itemPrefix;
                foreach (T item in list)
                {
                    itemString += $"{item.ToString()},  ";
                }
                if (removeUnityType == true)
                    itemString = itemString.Replace("(UnityEngine.GameObject)", "");
                Debug.Log($"{itemString} \n");
            }
            else
            {
                foreach (T item in list)
                {
                    string itemString = $"    {item.ToString()},  ";
                    if (removeUnityType == true)
                        itemString = itemString.Replace("(UnityEngine.GameObject)", "");
                    Debug.Log($"{itemPrefix} {itemString} \n");
                }
            }
        }*/
        public static void PrintList<T>(List<T> list, string header = "", bool removeUnityType = true, string itemPrefix = "", 
            bool allInOneLine = true, string hint = "", int printFirstN = 0)
        {
            if (!string.IsNullOrEmpty(header))
            {
                Debug.Log($"\n{header} [{list.Count}] :\n\n");
            }

            int maxItems = (printFirstN > 0 && printFirstN < list.Count) ? printFirstN : list.Count;
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < maxItems; i++)
            {
                string itemString = itemPrefix;
                if (itemPrefix == "index")
                    itemString = $"[{i}]";

                itemString += $" {list[i]}"; // Use list[i] directly

                if (removeUnityType)
                    itemString = itemString.Replace("(UnityEngine.GameObject)", "");

                if (allInOneLine)
                    stringBuilder.Append(itemString).Append(", ");
                else
                    Debug.Log(itemString + "\n");
            }

            if (allInOneLine)
                Debug.Log(stringBuilder.ToString().TrimEnd(',', ' ')+ "\n"); // Trim trailing comma and space
        }

        //--------------------------------------------------------------------
        public static void PrettyPrintRaycastInfo(List<RaycastHitInfo> hitInfos)
        {
            for (int i = 0; i < hitInfos.Count; i++)
            {
                var hitInfo = hitInfos[i];
                string message = $"Index {i}: Object '{hitInfo.ObjectName}', DistanceTCT: {hitInfo.Distance:F2}, Colliders: {hitInfo.ColliderTypes}";

                if (hitInfo.ObjectBeneath != null)
                {
                    message += $"\n\tBeneath: '{hitInfo.ObjectBeneath.ObjectName}', DistanceTCT: {hitInfo.ObjectBeneath.Distance:F2}, Colliders: {hitInfo.ObjectBeneath.ColliderTypes}";
                }

                Debug.Log(message);
            }
        }
    }
}