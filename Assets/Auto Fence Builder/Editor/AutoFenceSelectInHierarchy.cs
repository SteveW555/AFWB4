using AFWB;
using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// This class is used to re-enable the Auto Fence Builder in the Unity scene view if
/// alt-a key, or double-click on fence component is detected 
/// </summary>
[InitializeOnLoad]
public static class AutoFenceSelectInHierarchy
{
    /// <summary>Static constructor that subscribes the OnSceneGUI method to the duringSceneGui event. </summary>
    static AutoFenceSelectInHierarchy()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    /// <summary>
    /// This method is called during the Scene GUI event. It checks for alt-a or double-click and 
    /// re-enablesAuto Fence Builder in hierarchy. </summary>
    private static void OnSceneGUI(SceneView sceneView)
    {
        Event currEvent = Event.current;

        // Ensure the Scene View has focus
        if (sceneView != SceneView.lastActiveSceneView)
            return;

        // Check if no text field or input field is focused
        if (GUI.GetNameOfFocusedControl() != string.Empty)
            return;

        // Check for Alt-A
        if (currEvent.type == EventType.KeyDown && currEvent.shift && currEvent.keyCode == KeyCode.A)
        {
            ReenableAF();
            currEvent.Use(); // Mark the event as used to prevent further processing
        }

        // Check for double-click on a Fence component
        if (currEvent.type == EventType.MouseDown && currEvent.clickCount == 2 && currEvent.button == 0
            && currEvent.alt == false && currEvent.alt == false)
        {
            // Perform a raycast to detect the clicked GameObject
            Ray ray = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                
                if (IsFencePart(clickedObject))
                {
                    currEvent.Use(); // Mark the event as used to prevent further processing
                    GameObject autoFenceGo = ReenableAF();
                    AutoFenceCreator af = autoFenceGo.GetComponent<AutoFenceCreator>();
                    LayerSet layer =  af.InferLayerFromGoName(clickedObject);
                    af.SwitchToolbarComponentView(layer);
                }
            }
        }
    }

    /// <summary>
    /// Finds  Auto Fence Builder GameObject in the scene and sets it as the active selection.
    /// </summary>
    private static GameObject ReenableAF()
    {
        GameObject autoFenceGo = GameObject.Find("Auto Fence Builder");
        if (autoFenceGo != null)
        {
            Selection.activeGameObject = autoFenceGo;
            Debug.Log("Auto Fence Builder re-enabled");
        }
        else
            Debug.LogWarning("Auto Fence Builder not found in ReenableAF()");
        return autoFenceGo;
    }
    private static bool IsFencePart(GameObject go)
    {
        if(go == null)
            return false;
        if (StringUtilsTCT.StringContainsAutoFencePart(go.name))
            return true;
        return false;
    }
}

