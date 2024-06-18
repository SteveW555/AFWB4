using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//These are here more for reference, and a quick copy paste inline.

public class TransformUtilsAFB : MonoBehaviour
{
    static public string GetTransformAsString(GameObject go)
    {
        string str = GetTransformAsString(go.transform);
        return str;
    }
    static public string GetTransformAsString(Transform t)
    {
        string str = "   P:" + t.localPosition + "::" + t.position;
        str += "      R:" + t.localEulerAngles + "::" + t.eulerAngles;
        str += "      S:" + t.localScale + "::" + t.lossyScale;
        return str;
    }
    static public int GetDepth(Transform t)
    {
        int depth = 0;
        while (t.parent != null)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }
    static public int GetDepth(GameObject go)
    {
        return GetDepth(go.transform);
    }

}
