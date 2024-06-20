using AFWB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static AFWB.AutoFenceCreator;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SceneViewDebugDisplay
{
    public enum SceneViewCorner
    {
        BottomRight,
        BottomLeft,
        TopRight,
        TopLeft
    }

    private AutoFenceCreator af;
    private AutoFenceEditor ed;
    private bool showPostLabels, showRailLabels, showExtraLabels, showSubpostLabels, showMarkerIndices;
    public const int kRailAIndex = 0, kRailBIndex = 1, kPostIndex = 2;

    public SceneViewDebugDisplay(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
    {
        af = autoFenceCreator;
        ed = autoFenceEditor;
    }

    public void ShowFenceLabels()
    {
        showPostLabels = false;
        showRailLabels = false;
        showExtraLabels = false;
        showSubpostLabels = false;
        showMarkerIndices = true;

        if (af.clickPoints.Count > 0 && af.showSceneFenceLabels == true)
        {
            Vector3 labelPosition = af.allPostPositions[0];
            GUIStyle postLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(.75f, .85f, 1f, .99f) },
            };
            GUIStyle railLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(.65f, .999f, .65f, .99f) },
            };
            

            if (af.usePostsLayer && showPostLabels)
            {
                Color boxlColor = new Color(0f, 0f, .1f, 0.48f);
                Vector3 prevClickPointPos = af.clickPoints[0];
                for (int i = 0; i < af.allPostPositions.Count; i++)
                {
                    string goName = i.ToString() + " " + GetGoNameForSection(LayerSet.postLayer, i, strip: true) + " [Post]";
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));

                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.postLayer, i);
                    screenPosition.x -= labelSize.x / 3;

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, postLabelStyle);
                    Handles.EndGUI();
                }
            }
            if (af.useRailLayer[0] && showRailLabels)
            {
                Color boxlColor = new Color(.0f, 0.0f, .0f, 0.666f);

                for (int i = 0; i < af.railABuiltCount; i++)
                {
                    string goName = i.ToString();
                    goName += " " + GetGoNameForSection(LayerSet.railALayer, i, strip: true) + " [RailA]";
                    Vector2 labelSize = railLabelStyle.CalcSize(new GUIContent(goName));

                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.railALayer, i);
                    screenPosition.x -= labelSize.x / 2;

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, railLabelStyle);
                    Handles.EndGUI();
                }
            }
            if (af.useRailLayer[1] && showRailLabels)
            {
                Color boxlColor = new Color(.0f, 0.05f, .0f, 0.48f);
                for (int i = 0; i < af.railsBPool.Count; i++)
                {
                    labelPosition = af.railsBPool[i].localPosition;
                    labelPosition.x += af.actualInterPostDistance / 4;
                    labelPosition.y += 0.4f;
                    labelPosition.y += (i % 2) * 0.35f; //offset every other for clarity
                    string goName = i.ToString() + " " + GetGoNameForSection(LayerSet.railBLayer, i, strip: true) + " [RailB]";
                    Vector2 screenPosition = HandleUtility.WorldToGUIPoint(labelPosition);
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    Handles.EndGUI();
                    Handles.Label(labelPosition, goName, railLabelStyle);
                }
            }
            if (af.useExtrasLayer && showExtraLabels)
            {
                Color boxlColor = new Color(.0f, 0.05f, .0f, 0.48f);
                for (int i = 0; i < af.ex.extrasPool.Count; i++)
                {
                    labelPosition = af.ex.extrasPool[i].localPosition;
                    labelPosition.x += af.actualInterPostDistance / 4;
                    labelPosition.y += 0.4f;
                    labelPosition.y += (i % 2) * 0.35f; //offset every other for clarity
                    string goName = i.ToString() + " " + GetGoNameForSection(LayerSet.extraLayer, i, strip: true) + " [Extra]";
                    Vector2 screenPosition = HandleUtility.WorldToGUIPoint(labelPosition);
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    Handles.EndGUI();
                    Handles.Label(labelPosition, goName, railLabelStyle);
                }
            }
            if (af.useSubpostsLayer && showSubpostLabels)
            {
                Color boxlColor = new Color(0f, 0f, .3f, 0.48f);
                Vector3 prevClickPointPos = af.clickPoints[0];
                int numSubposts = af.subpostsBuiltCount;
                for (int i = 0; i < numSubposts; i++)
                {
                    string goName = i.ToString() + " " + GetGoNameForSection(LayerSet.subpostLayer, i, strip: true) + " [Subpost]";
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));

                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.subpostLayer, i);
                    screenPosition.x -= labelSize.x / 3;

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, postLabelStyle);
                    Handles.EndGUI();
                }
            }

            //-- Markers --
            if (af.clickPoints.Count > 0 && showMarkerIndices == true)
            {
                List<Transform> nodeMarkers = af.GetNodeMarkers();
                GUIStyle markerLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = new Color(1f, 1f, 1f, 1f) },
                };
                Color boxCol = new Color(.28f, .23f, .01f, 0.55f);
                //Color boxlColor = new Color(0f, 0f, .3f, 0.48f);
                    List<Transform> markers = af.GetNodeMarkers();
                for (int i = 0; i < af.clickPoints.Count; i++)
                {
                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.markerLayer, i);
                    screenPosition.x -= 20;
                    Vector2 markerPos2D =  markers[i].localPosition.To2D();
                    string markerStr = $"{i.ToString()}   {markerPos2D.ToString("F1")}";

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 23, 94, 17), boxCol, boxCol);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 24, 92, 16), markerStr, markerLabelStyle);
                    Handles.EndGUI();
                }
                //GUI.Label(new Rect(6, Screen.height - 50, 300, 20), "Unlock Mouse:  Shift-Alt-Right-Click on a Post");
            }
        }
    }

    public void ShowPostPositions()
    {
        if (af.clickPoints.Count > 0 && af.showPostPositions == true)
        {
            Vector3 labelPosition = af.allPostPositions[0];
            GUIStyle postLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(.75f, .85f, 1f, .99f) },
            };
            GUIStyle subpostLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(.75f, .85f, 1f, .99f) },
            };

            if (af.usePostsLayer)
            {
                Color boxlColor = new Color(0f, 0f, .1f, 0.48f);
                Vector3 prevClickPointPos = af.clickPoints[0];
                for (int i = 0; i < af.allPostPositions.Count; i++)
                {
                    string goName = i.ToString() + " " + GetGoNameForSection(LayerSet.postLayer, i, strip: true) + " [Post]  " + af.allPostPositions[i];
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));

                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.postLayer, i);
                    screenPosition.x -= labelSize.x / 3;

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, postLabelStyle);
                    Handles.EndGUI();
                }
            }
            if (af.useSubpostsLayer)
            {
                Color boxlColor = new Color(0f, 0f, .3f, 0.48f);
                Vector3 prevClickPointPos = af.clickPoints[0];
                int numSubposts = af.subpostsBuiltCount;
                /*for (int i = 0; i < numSubposts; i++)
                {
                    string goName = i.ToString() + " " + GetGoNameForSection(LayerSet.subpostLayer, i, strip: true) + " [Subpost]  " + af.subpostsPool[i].localPosition;
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));

                    Vector2 screenPosition = ed.GetCentreScreenPositionForGO(LayerSet.subpostLayer, i);
                    screenPosition.x -= labelSize.x / 3;

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, subpostLabelStyle);
                    Handles.EndGUI();
                }*/
            }
        }
    }

    public void ShowSectionIndices()
    {
        if (af.clickPoints.Count > 0)
        {
            Vector3 labelPosition = af.allPostPositions[0];
            GUIStyle postLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(.75f, .85f, 1f, .99f) },
            };
            GUIStyle railLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(.55f, .95f, .85f, .99f) },
            };

            if (af.useRailLayer[0])
            {
                Color boxlColor = new Color(.0f, 0.0f, .0f, 0.55f);

                for (int i = 0; i < af.allPostPositions.Count - 1; i++)
                {
                    string goName = "Sec " + i.ToString();
                    Vector2 labelSize = railLabelStyle.CalcSize(new GUIContent(goName));
                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.railALayer, i);
                    screenPosition.x -= labelSize.x / 2;
                    screenPosition.y -= 35;
                    //Handles.BeginGUI(); //should already be on
                    //
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, railLabelStyle);
                    //Handles.EndGUI();
                }
            }
        }
    }
    //------------------------------------------------------------
    public void ShowNodeDistances()
    {
        if (af.showControls == false)
            return;

        //af.showNodeDistances = true;

        Color boxCol = new Color(0f, 0f, .1f, 0.60f);

        if (af.clickPoints.Count > 0 && af.showNodeDistances == true && af.usePostsLayer)
        {
            Vector3 labelPosition = af.allPostPositions[0];
            labelPosition.y += 1.5f;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(.95f, .95f, .75f, .99f) },
            };

            Color boxlColor = new Color(0f, 0f, .1f, 0.40f);
            Vector3 prevNodePos = af.nodeMarkersPool[0].localPosition;
            int screenPosXOffset = 50, screenPosYOffset = 50;
            Vector2 prevScreenPos = HandleUtility.WorldToGUIPoint(prevNodePos);
            prevScreenPos.x -= screenPosXOffset;
            prevScreenPos.y -= screenPosYOffset;
            for (int i = 1; i < af.clickPoints.Count; i++)
            {
                if (i > af.clickPoints.Count - 1)
                    break;

                //-- Position Attempt 2
                Vector3 currNodePos = currNodePos = af.nodeMarkersPool[i].localPosition;
                currNodePos.x += 0.1f;
                string posString = currNodePos.ToString("F1");
                posString = $"[ {currNodePos.x:F1},  {currNodePos.y:F1},  {currNodePos.z:F1} ]";

                //Get node Screen position
                Vector2 screenPosition = GetScreenPositionOfNode(i);

                //-- Position label
                Vector2 labelSize = labelStyle.CalcSize(new GUIContent(posString));
                labelSize = labelStyle.CalcSize(new GUIContent(posString));
                labelSize.x -= 1;

                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - screenPosXOffset, screenPosition.y - screenPosYOffset,
                    labelSize.x + 4, labelSize.y + 2), boxCol, boxlColor);
                GUI.Label(new Rect(screenPosition.x - screenPosXOffset, screenPosition.y - screenPosYOffset,
                    labelSize.x, labelSize.y), posString, labelStyle);

                //-- Distance
                float distance = Vector3.Distance(currNodePos, prevNodePos);
                Vector3 distVec = currNodePos - prevNodePos;
                string distString = distance.ToString("F2");

                //-- Distance Label
                labelSize = labelStyle.CalcSize(new GUIContent(distString));
                labelSize.x -= 1;
                prevScreenPos = HandleUtility.WorldToGUIPoint(prevNodePos);
                //calculate the midpoint between screenPosition and screenPositionPrev
                Vector2 screenPosDist = (screenPosition + prevScreenPos) / 2;
                Handles.DrawSolidRectangleWithOutline(new Rect(screenPosDist.x, screenPosDist.y - 15, labelSize.x + 4, labelSize.y + 2), boxCol, boxlColor);
                GUI.Label(new Rect(screenPosDist.x, screenPosDist.y - 15, labelSize.x, labelSize.y), distString, labelStyle);

                DrawDistanceLine(screenPosition, prevScreenPos, 10);

                Handles.EndGUI();
                prevScreenPos = screenPosition;
                prevNodePos = currNodePos;
            }
        }
    }

    //--------------------------------
    private void DrawDistanceLine(Vector2 a, Vector2 b, float yOffset)
    {
        // Calculate the midpoint
        Vector2 midpoint = (a + b) / 2;

        // Calculate the direction from a to b
        Vector2 direction = (b - a).normalized;

        // Calculate 60% of the distance
        float distance = Vector2.Distance(a, b);
        float gapSize = 40;

        // Calculate the start and end points of the line
        Vector2 startPoint = midpoint - (direction * (distance / 2));
        Vector2 endPoint = midpoint + (direction * (distance / 2));

        startPoint = a; endPoint = b;
        startPoint = a + (direction * gapSize); endPoint = b - (direction * gapSize);

        // Draw the line
        Handles.color = new Color(0.5f, 0.5f, 0.99f, 0.9f);
        Handles.DrawLine(startPoint, endPoint); // Draw a line between the start and end points
    }

    //---------------
    private Vector2 GetCentreScreenPositionForGO(Vector3 goPosition)
    {
        Vector2 screenPosition = HandleUtility.WorldToGUIPoint(goPosition);
        return screenPosition;
    }

    private Vector2 GetScreenPositionOfNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= af.nodeMarkersPool.Count)
        {
            Debug.Log($"GetScreenPositionOfNode: nodeIndex out of range  {nodeIndex} / {af.nodeMarkersPool.Count}");
            return new Vector2(0, 0);
        }

        Vector3 nodePos = af.nodeMarkersPool[nodeIndex].localPosition;
        Vector2 screenPosition = HandleUtility.WorldToGUIPoint(nodePos);
        return screenPosition;
    }

    public void ShowStepNumbers()
    {
        if (af.clickPoints.Count > 0 && af.showSceneStepNums == true)
        {
            Vector3 labelPosition = af.allPostPositions[0];
            GUIStyle postLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(.75f, .85f, 1f, .99f) },
            };
            GUIStyle railLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(.65f, .999f, .6f, .99f) },
            };
            /*if (af.usePostsLayer)
            {
                Color boxlColor = new Color(0f, 0f, .1f, 0.48f);
                for (int i = 0; i < af.allPostPositions.Count; i++)
                {
                    string goName = i.ToString() + " " + sceneDebug.GetGoNameForSection(LayerSet.postLayer, i, strip: false);
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));
                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.postLayer, i);
                    screenPosition.x -= labelSize.x / 3;
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), goName, postLabelStyle);
                    Handles.EndGUI();
                }
            }*/
            if (af.useRailLayer[0])
            {
                Color boxlColor = new Color(.0f, 0.03f, .0f, 0.68f);
                for (int i = 0; i < af.allPostPositions.Count - 1; i++)
                {
                    int stepNum = i % af.GetNumSeqStepsForLayer(LayerSet.railALayer);
                    string stepNumStr = "Step " + stepNum.ToString();
                    Vector2 labelSize = new Vector2(38, 16);
                    Vector2 screenPosition = GetCentreScreenPositionForGO(LayerSet.railALayer, i);
                    screenPosition.x -= labelSize.x / 2;
                    screenPosition.y -= 18;
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y), boxlColor, boxlColor);
                    GUI.Label(new Rect(screenPosition.x, screenPosition.y - 4, labelSize.x, labelSize.y), stepNumStr, railLabelStyle);
                    Handles.EndGUI();
                }
            }
            /*if (af.useRailLayer[1])
            {
                Color boxlColor = new Color(.0f, 0.05f, .0f, 0.48f);
                for (int i = 0; i < af.railsAPool.Count; i++)
                {
                    labelPosition = af.railsAPool[i].localPosition;
                    labelPosition.x += af.actualInterPostDistance / 4;
                    labelPosition.y += 0.4f;
                    labelPosition.y += (i % 2) * 0.35f; //offset every other for clarity
                    string goName = i.ToString() + " " + sceneDebug.GetGoNameForSection(LayerSet.railBLayer, i, strip: true);
                    Vector2 screenPosition = HandleUtility.WorldToGUIPoint(labelPosition);
                    Vector2 labelSize = postLabelStyle.CalcSize(new GUIContent(goName));
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(new Rect(screenPosition.x - 2, screenPosition.y - 3, labelSize.x + 4, labelSize.y + 2), boxlColor, boxlColor);
                    Handles.EndGUI();
                    Handles.Label(labelPosition, goName, railLabelStyle);
                }
            }*/
        }
    }

    private int DisplayCurrentPrefabsInfo(int panelYPos, GUIStyle style, int xpt, int rowHeight)
    {
        //iterate over a LayerSet enum
        for (int i = 0; i < Enum.GetNames(typeof(LayerSet)).Length; i++)
        {
            LayerSet layer = (LayerSet)i;
            if ((layer >= LayerSet.markerLayer)) //-- Non-Fence layers
                continue;
            int currPrefabIndex = af.GetCurrentPrefabIndexForLayer(layer);
            GameObject go = af.GetPrefabAtIndexForLayer(currPrefabIndex, layer);
            GUI.Label(new Rect(xpt, panelYPos, 270, 20), $"{layer.String()}  Prefab Index = {currPrefabIndex}  {go.name}", style);
            panelYPos += rowHeight;
            af.ValidatePoolForLayer(layer);
        }
        return panelYPos;
    }
    private int DisplayPoolsInfo(int panelYPos, GUIStyle style, int xpt, int rowHeight)
    {
        //iterate over a LayerSet enum
        for (int i = 0; i < Enum.GetNames(typeof(LayerSet)).Length; i++)
        {
            LayerSet layer = (LayerSet)i;
            if ((layer >= LayerSet.markerLayer)) //-- Non-Fence layers
                continue;

            int poolCount = af.GetPoolCountForLayer(layer);
            GUI.Label(new Rect(xpt, panelYPos, 270, 20), $"{layer.String()}  Pool Size = {poolCount}", style);
            panelYPos += rowHeight;
            //af.ValidatePoolForLayer(sourceLayerList);
        }
        return panelYPos;
    }
    private void DisplayFenceStatsInfo(int panelYPos, GUIStyle style, int xpt, int rowHeight)
    {
        GUI.Label(new Rect(xpt, panelYPos, 270, 20), $"Num Posts = {af.allPostPositions.Count}     Num Nodes = {af.clickPoints.Count}", style);
        GUI.Label(new Rect(xpt, panelYPos + rowHeight, 270, 20), $"Num Rails A = {af.railABuiltCount}     Num Rails B = {af.railBBuiltCount} ", style);
    }

    private void DisplayCurrentPresetInfo(int panelYPos, GUIStyle style, int xpt, int rowHeight)
    {
        int currPresetIndex = af.currPresetIndex;
        List<ScriptablePresetAFWB> presetList = ed.mainPresetList;
        ScriptablePresetAFWB currPreset = ed.currPreset;

        GUI.Label(new Rect(xpt, panelYPos, 270, 20), $"Current Preset Index = {currPresetIndex}       {currPreset.name}", style);
    }

    public void ShowSceneViewDebugInfoPanel(LayerSet layer)
    {
        //return;

        if (af.showSceneDebugInfoPanel == false)
            return;

        if ((layer >= LayerSet.markerLayer)) //-- Non-Fence layers
            return;

        int colWidth = 215, rowHeight = 16;
        float numCols = 3.5f;
        int panelHeight = 400, panelWidth = (int)(colWidth * numCols);
        int panelYPos = Screen.height - panelHeight - 44, panelXPos = Screen.width - panelWidth - 10;

        Rect rectDebug = new Rect(panelXPos, panelYPos, panelWidth + 100, panelHeight);
        Color colorDebugPanel = new Color(0f, 0f, 0f, 0.48f);
        Handles.DrawSolidRectangleWithOutline(rectDebug, colorDebugPanel, ed.sceneViewBoxBorderColor);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 11;
        style.normal.textColor = new Color(1, 1, 1, 0.75f);

        string nameStr = "", labelStr = "", layerName = af.GetLayerNameAsString(layer);
        int xpt = panelXPos + 2, numVariants = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true), numSeqSteps = af.GetNumSeqStepsForLayer(layer);
        List<SourceVariant> variants = af.GetSourceVariantsForLayer(layer);

        //=====  Basic Info  =====
        int newYPos = DisplayCurrentPrefabsInfo(panelYPos, style, xpt, rowHeight);
        DisplayCurrentPresetInfo(newYPos + 8, style, xpt, rowHeight);

        DisplayPoolsInfo(panelYPos + 120, style, xpt, rowHeight);

        //DisplayFenceStatsInfo(panelYPos + 220, style, xpt, rowHeight);



        if (af.GetUseLayerVariations(layer) == true)
        {
            //=====  Variants  =====

            GUI.Label(new Rect(xpt, panelYPos, colWidth, 20), $"- {layerName} Variants -", style);
            List<string> tempNames = SourceVariant.GetGoNamesFromSourceVariantList(variants);
            for (int i = 0; i < numVariants; i++)
            {
                GUI.Label(new Rect(xpt, 2 + panelYPos + ((i + 1) * rowHeight), colWidth, 20), i.ToString() + " " + tempNames[i], style);
            }

            //=====  Sequence  =====

            if (af.GetUseSequencerForLayer(layer) == true)
            {
                xpt = panelXPos + colWidth * 1;
                GUI.Label(new Rect(xpt, panelYPos, colWidth, 20), "- {layerName} Variants in Sequence Steps -", style);
                tempNames = SourceVariant.GetGoNamesFromSourceVariantList(variants);
                List<SeqItem> sequenceVariants = af.GetSequenceForLayer(layer);
                for (int i = 0; i < numSeqSteps; i++)
                {
                    SeqItem seqVar = sequenceVariants[i];
                    GameObject seqVarGO = seqVar.GetSourceVariantGO(af, layer);
                    if (seqVar == null)
                        Debug.Log($"seqVar {i} of {layerName} is null");
                    else if (seqVarGO == null)
                        Debug.Log($"seqVar,go {i} of {layerName} is null");
                    if (seqVar == null || seqVarGO == null)
                        break;

                    int idx = seqVar.sourceVariantIndex;
                    nameStr = StripLayerTypeFromNameStatic(seqVarGO.name);

                    idx = i % (numVariants + 1);
                    nameStr = tempNames[idx];
                    labelStr = i.ToString() + " [V" + idx + "] " + nameStr;

                    SerializedProperty seqVarListProp = ed.GetSequencerListForLayerProp(layer);
                    SerializedProperty goIndexProp = seqVarListProp.GetArrayElementAtIndex(i).FindPropertyRelative("sourceVariantIndex");
                    labelStr += " M" + goIndexProp.intValue.ToString();

                    GUI.Label(new Rect(xpt, 2 + panelYPos + ((i + 1) * 16), colWidth, 20), labelStr, style);
                }
            }
        }
        //=====  Panels in Use  =====

        xpt = panelXPos + colWidth * 2;
        GUI.Label(new Rect(xpt, panelYPos, colWidth, 20), "- {layerName} Variants  in Scene -", style);
        int seqLoopCount = 0, numBuilt = af.GetNumSectionsBuiltForLayer(layer);
        int maxLength = numBuilt.ToString().Length;// to pad the strings so they line up
        for (int i = 0; i < numBuilt; i++)
        {
            string goName = GetGoNameForSection(layer, i, strip: true);

            nameStr = i.ToString().PadLeft(maxLength);
            nameStr += string.Concat(Enumerable.Repeat(" +", seqLoopCount));
            nameStr += " " + goName;
            GUI.Label(new Rect(xpt, (seqLoopCount * 5) + 2 + panelYPos + ((i + 1) * 16), colWidth, 20), nameStr, style);
        }

        //===  Close Button  ===
        if (GUI.Button(new Rect(panelXPos + panelWidth - 20, 2 + panelYPos + 2, 20, 20), new GUIContent("X", "Close this Debug Info window")))
        {
            af.showSceneDebugInfoPanel = false;
        }
    }



    //---------------------------------------------------------
    // Uses the sequence and loop to get the name of the GO for a given section
    public string GetGoNameForSection(LayerSet layer, int sectionNum, bool strip = true, bool shorten = false)
    {
        string goName = "null";
        int variantIndex = 0;
        GameObject thisGo = null;
        if (layer != LayerSet.extraLayer && layer != LayerSet.subpostLayer)
        {
            int numSeqSteps = af.GetSequencerForLayer(layer).Length();
            int seqStep = sectionNum % numSeqSteps;
            int seqLoopCount = sectionNum / numSeqSteps; //how many times have we moduloed past seqNumSteps
            List<SeqItem> SeqItems = af.GetSequenceForLayer(layer);
            variantIndex = SeqItems[seqStep].sourceVariantIndex;
            int numVariants = af.GetNumSourceVariantsInUseForLayer(layer, incMain: true);
            if (variantIndex > numVariants)
                variantIndex = SeqItems[0].sourceVariantIndex;
            List<SourceVariant> variants = af.GetSourceVariantsForLayer(layer);
            thisGo = variants[variantIndex].Go;
        }
        else
            thisGo = af.GetMainPrefabForLayer(layer);

        goName = thisGo.name;
        if (strip)
            goName = StripLayerTypeFromNameStatic(goName);

        if (shorten && goName.Length > 11)
            goName = StringUtilsTCT.RemoveVowels(goName, leaveFiirstLetterUnmodified: false);

        return goName;
    }

    //---------------------------------------------------------

    public static void DrawSemiTransparentRectangle(SceneViewCorner corner = SceneViewCorner.BottomRight,
        Color? rectangleColor = null, Color? borderColor = null, int borderThickness = 1, float offsetX = 0, float offsetY = 0)
    {
        // Set default colors if not provided
        rectangleColor = rectangleColor ?? new Color(0.0f, 1.0f, 0.0f, 0.5f); // Semi-transparent green color
        borderColor = borderColor ?? Color.black;

        SceneView sceneView = SceneView.currentDrawingSceneView;
        if (sceneView == null)
        {
            return;
        }

        // Calculate the rectangle position based on the selected corner
        float xPos, yPos;
        switch (corner)
        {
            case SceneViewCorner.TopLeft:
                xPos = 0 + offsetX;
                yPos = 0 + offsetY;
                break;

            case SceneViewCorner.TopRight:
                xPos = sceneView.position.width - 600 - offsetX;
                yPos = 0 + offsetY;
                break;

            case SceneViewCorner.BottomLeft:
                xPos = 0 + offsetX;
                yPos = sceneView.position.height - 300 - offsetY;
                break;

            case SceneViewCorner.BottomRight:
            default:
                xPos = sceneView.position.width - 600 - offsetX;
                yPos = sceneView.position.height - 300 - offsetY;
                break;
        }
        Rect rectangleRect = new Rect(xPos, yPos, 600, 300);

        // Create a texture for the rectangle
        Texture2D rectangleTexture = new Texture2D(1, 1);
        rectangleTexture.SetPixel(0, 0, rectangleColor.Value);
        rectangleTexture.Apply();

        // Create a texture for the border
        Texture2D borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, borderColor.Value);
        borderTexture.Apply();

        Handles.BeginGUI();
        Color storeColor = GUI.color;

        // DrawTCT the border
        GUI.color = borderColor.Value;
        // Top border
        GUI.DrawTexture(new Rect(rectangleRect.x - borderThickness, rectangleRect.y - borderThickness, rectangleRect.width + borderThickness * 2, borderThickness), borderTexture);
        // Bottom border
        GUI.DrawTexture(new Rect(rectangleRect.x - borderThickness, rectangleRect.y + rectangleRect.height, rectangleRect.width + borderThickness * 2, borderThickness), borderTexture);
        // Left border
        GUI.DrawTexture(new Rect(rectangleRect.x - borderThickness, rectangleRect.y, borderThickness, rectangleRect.height), borderTexture);
        // Right border
        GUI.DrawTexture(new Rect(rectangleRect.x + rectangleRect.width, rectangleRect.y, borderThickness, rectangleRect.height), borderTexture);

        // DrawTCT the semi-transparent rectangle
        GUI.color = rectangleColor.Value;
        GUI.DrawTexture(rectangleRect, rectangleTexture);

        //restore color
        GUI.color = storeColor;

        Handles.EndGUI();
    }

    //---------------------------------
    public Vector2 GetCentreScreenPositionForGO(LayerSet layer, int sectionNum)
    {
        int adjustedSectionNum = sectionNum;

        if (layer == LayerSet.railALayer && af.numStackedRails[0] > 1)
        {
            adjustedSectionNum = (int)((float)sectionNum / af.numStackedRails[0]);
        }
        else if (layer == LayerSet.railBLayer && af.numStackedRails[1] > 1)
        {
            adjustedSectionNum = (int)((float)sectionNum / af.numStackedRails[1]);
        }

        Vector3 endX = Vector3.zero, midpoint = Vector3.zero, startX = af.allPostPositions[adjustedSectionNum];
        if (layer == LayerSet.railALayer || layer == LayerSet.railBLayer)
        {
            if (sectionNum < af.allPostPositions.Count - 1)
                endX = af.allPostPositions[adjustedSectionNum + 1]; // use instead of goPositiions to get a better center point and deal with problem of last section
            else
                endX = startX;
            midpoint = (startX + endX) / 2;
        }
        else if (layer == LayerSet.postLayer)
            midpoint = startX;
        else if (layer == LayerSet.subpostLayer)
        {
            startX = af.subpostsPool[sectionNum].localPosition;
            midpoint = startX;
            midpoint.y -= 0.3f; // lower the subpost labels a little to avoid the post labels
        }

        if (layer == LayerSet.railALayer)
        {
            midpoint.y = af.railsAPool[adjustedSectionNum].localPosition.y;
            if (af.numStackedRails[0] > 1 && sectionNum >= af.allPostPositions.Count)
            {
                midpoint.y += 20;
            }
        }
        else if (layer == LayerSet.railBLayer)
        {
            midpoint.y = af.railsBPool[sectionNum].localPosition.y;
            if (af.numStackedRails[1] > 1 && sectionNum >= af.allPostPositions.Count)
            {
                midpoint.y += 20;
            }
        }
        else if (layer == LayerSet.markerLayer)
        {
            List<Transform> markers = af.GetNodeMarkers();
            midpoint = markers[sectionNum].localPosition;
        }

        Vector2 screenPosition = HandleUtility.WorldToGUIPoint(midpoint);
        Vector3 labelPosition = midpoint;
        return screenPosition;
    }
}