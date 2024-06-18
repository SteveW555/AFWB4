using AFWB;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AFWBCustomDemo : MonoBehaviour
{
    private List<Vector3> newClickPoints = new List<Vector3>();
    private List<int> newClickPointFlags = new List<int>(); // defines if the point is a break/gap or not
    private AutoFenceCreator autoFence;

    //------------------------------
    private void Awake()
    {
        autoFence = GameObject.Find("Auto Fence Builder").GetComponent<AutoFenceCreator>();
    }

    //------------------------------
    public void TestDemo()
    {
        if (autoFence == null)
            autoFence = GameObject.Find("Auto Fence Builder").GetComponent<AutoFenceCreator>();
        if (autoFence != null)
        {
            CreateClickPointsAndSendToAFWB(); // CreateMergedPrefabs the positions

            // Example 1, manually design the fence
            /*
			EditAFWBParameters(); // Change the design of the fence
			autoFence.ForceRebuildFromClickPoints(); // build the fence
			*/

            // or Example 2, use a fence preset
            autoFence.currPresetIndex = 46;
            //autoFence.RedesignFenceFromPreset(autoFence.currentPresetIndex); // this also calls ForceRebuildFromClickPoints()
            // If you want to create multiple fences, you might need to call:
            //autoFence.FinishAndStartNew();
        }
        else
            Debug.Log("Couldn't find Auto Fence Builder, are you sure it's imported and in the scene?");
    }

    //------------------------------
    private void CreateClickPointsAndSendToAFWB()
    {
        newClickPoints.Clear();
        autoFence.clickPointFlags.Clear(); // the flags tell AFWB if there is a gap/break in the fence at this point. 0=normal, 1 = gap/break

        //CreateMergedPrefabs some clickPoints
        // It is usually best to set y=0, and let AFWB find the correct Ground level
        AddClickPoint(new Vector3(5, 0, 5));
        AddClickPoint(new Vector3(20, 0, 5));
        AddClickPoint(new Vector3(20, 0, 20));
        AddClickPoint(new Vector3(5, 0, 20));

        autoFence.LowerPostsToGround(newClickPoints);

        /*  Of course, you can also add points in a loop from your own node list instead  */

        // Assign them to AFWB
        autoFence.clickPoints = newClickPoints;
        autoFence.clickPointFlags = newClickPointFlags;
    }

    //------------------------------
    private void AddClickPoint(Vector3 point, int type = 0) //type 0= normal clickPoint, 1 = gap/break
    {
        newClickPoints.Add(point);
        newClickPointFlags.Add(type);
    }

    //------------------------------
    //Test Change the design of the fence
    //-- All of the public parameters, e.g. globalHeight can be found in AutoFenceCreator.cs
    private void EditAFWBParameters()
    {
        //Example 1:
        autoFence.globalHeight = 3.0f;
        autoFence.SetPostPrefab(5, false);
        autoFence.SetRailPrefab(43, LayerSet.railALayerSet, false);
        autoFence.railAPositionOffset = new Vector3(0, 0.5f, 0);
        autoFence.interpolate = true;
        autoFence.interPostDist = 4.0f;
    }
}