using System.Linq;
using UnityEngine;

//namespace GameObjectUtilsTCT
//{
public class GameObjectUtilitiesTCT
{
    //Finds a deeply nested child gameobject by name
    public static GameObject FindChildByName(GameObject parent, string childName)
    {
        var children = parent.transform.GetComponentsInChildren<Transform>(true); // true to include inactive children
        var foundChild = children.FirstOrDefault(t => t.name == childName);
        return foundChild?.gameObject;
    }
}

//}