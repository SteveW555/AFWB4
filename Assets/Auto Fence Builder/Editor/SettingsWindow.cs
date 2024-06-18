using AFWB;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SettingsWindow : EditorWindow
{
    private AutoFenceCreator afb = null;
    private bool isDirty = false;
    private Color darkGrey = new Color(.2f, .2f, .3f);
    private Color darkCyan = new Color(0, .5f, .75f);
    private GUIStyle infoStyle, headingStyle;
    private ColliderType tempPostColliderMode = ColliderType.originalCollider, tempRailColliderMode = ColliderType.originalCollider, tempExtraColliderMode = ColliderType.originalCollider;
    private bool tempAllowGaps = true, tempShowDebugLines = true;
    private Transform parent = null;

    public void Init(AutoFenceCreator inAFB)
    {
        afb = inAFB;
        tempPostColliderMode = afb.postColliderMode;
        tempRailColliderMode = afb.railAColliderMode;
        tempExtraColliderMode = afb.extraColliderMode;

        tempAllowGaps = afb.allowGaps;
        tempShowDebugLines = afb.showDebugGapLine;
        parent = inAFB.finishedFoldersParent;
    }

    private void OnGUI()
    {
        headingStyle = new GUIStyle(EditorStyles.label);
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.normal.textColor = darkCyan;

        infoStyle = new GUIStyle(EditorStyles.label);
        infoStyle.fontStyle = FontStyle.Normal;
        infoStyle.normal.textColor = darkGrey;

        GUILayout.Space(10);
        GUILayout.Space(10);

        //=================================
        //	 Parent Folder for Finished
        //=================================
        GUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Optional Parent for Finished Folders", headingStyle);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("If you want your Finished Fence folders to be parented to an object in your hierarchy", infoStyle);
        EditorGUILayout.LabelField("drag the parent object here\n", infoStyle);

        EditorGUI.BeginChangeCheck();
        parent = EditorGUILayout.ObjectField(parent, typeof(Transform), true) as Transform;
        if (EditorGUI.EndChangeCheck())
        {
            afb.finishedFoldersParent = parent;
        }

        GUILayout.Space(10);
        GUILayout.EndVertical();
        GUILayout.Space(10); GUILayout.Space(10);

        //=================================
        //			Colliders
        //=================================

        GUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Colliders", headingStyle);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("By default, a single BoxCollider will be placed on the rails/walls, set to the height of the postsPool.\n", infoStyle);
        EditorGUILayout.LabelField("For most purposes this gives the expected collision on the fence.\n", infoStyle);
        EditorGUILayout.LabelField("It's not usually necessary to have colliders on the postsPool.\n", infoStyle);
        EditorGUILayout.LabelField("You can change this if, for example, the postsPool & rails are radically different thicknesses,\n", infoStyle);
        EditorGUILayout.LabelField("or if you have postsPool but no rails.", infoStyle);
        EditorGUILayout.LabelField("For best performance, use Single or None where possible. Using 'Keep Original' on", infoStyle);
        EditorGUILayout.LabelField("custom objects which have MeshColliders, or multiple small colliders is not recommended.", infoStyle);

        GUILayout.Space(10);
        GUILayout.Space(10);
        GUILayout.Space(10);
        //=========== Defaults ============
        if (GUILayout.Button("Set Defaults", GUILayout.Width(100)))
        {
            afb.postColliderMode = ColliderType.originalCollider;
            afb.railAColliderMode = ColliderType.originalCollider;
            afb.extraColliderMode = ColliderType.originalCollider;
            afb.railABoxColliderHeightScale = 1.0f;
            afb.railABoxColliderHeightOffset = 0.0f;
            isDirty = true;
        }
        GUILayout.Space(10);
        GUILayout.Space(10);
        GUILayout.Space(10);

        //Collider Modes: 0 = single box, 1 = keep original (user), 2 = no colliders
        string[] subModeNames = { "Use Single Box Collider", "Keep Original Colliders (Custom Objects Only)", "No Colliders", "Mesh Colliders" };
        int[] subModeNums = { 0, 1, 2, 3 };
        EditorGUI.BeginChangeCheck();
        //afb.railAColliderMode = (int)EditorGUILayout.IntPopup("Rail A Colliders: ", (int)afb.railAColliderMode, subModeNames, subModeNums);
        afb.railAColliderMode = (ColliderType)EditorGUILayout.EnumPopup("Rail A Colliders: ", afb.railAColliderMode);
        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
        }
        EditorGUI.BeginChangeCheck();
        //afb.postColliderMode = EditorGUILayout.IntPopup("Post Colliders: ", afb.postColliderMode, subModeNames, subModeNums);
        afb.postColliderMode = (ColliderType)EditorGUILayout.EnumPopup("Post Colliders: ", afb.postColliderMode);
        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
        }
        EditorGUI.BeginChangeCheck();
        //afb.extraColliderMode = EditorGUILayout.IntPopup("Extras Colliders: ", afb.extraColliderMode, subModeNames, subModeNums);
        afb.extraColliderMode = (ColliderType)EditorGUILayout.EnumPopup("Extra Colliders: ", afb.extraColliderMode);
        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Use these to modify the height and vertical postion of the Rail/Wall's Box Collider:", infoStyle);
        EditorGUI.BeginChangeCheck();
        afb.railABoxColliderHeightScale = EditorGUILayout.FloatField("Rail BoxCollider Y Scale", afb.railABoxColliderHeightScale);
        if (afb.railABoxColliderHeightScale < 0.01f)
            afb.railABoxColliderHeightScale = 0.01f;
        if (afb.railABoxColliderHeightScale > 10f)
            afb.railABoxColliderHeightScale = 10.0f;
        GUILayout.Space(10);
        afb.railABoxColliderHeightOffset = EditorGUILayout.FloatField("Rail BoxCollider Y Offset", afb.railABoxColliderHeightOffset);
        if (afb.railABoxColliderHeightOffset < -10.0f)
            afb.railABoxColliderHeightOffset = -10.0f;
        if (afb.railABoxColliderHeightOffset > 10f)
            afb.railABoxColliderHeightOffset = 10.0f;
        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("(On long or complex fences, selecting 'No Colliders' will improve performance", infoStyle);
        EditorGUILayout.LabelField("while designing in the Editor. Add them when you're ready to finish.)", infoStyle);
        GUILayout.Space(10);
        GUILayout.Space(10);
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.Space(10);
        GUILayout.Space(10);

        //=================================
        //			Gaps
        //=================================
        GUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Gaps", headingStyle);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Control-Right-Click to create gaps in the fence.", infoStyle);

        GUILayout.Space(10);
        afb.allowGaps = EditorGUILayout.Toggle("Allow Gaps", afb.allowGaps);
        afb.showDebugGapLine = EditorGUILayout.Toggle("Show Gap Lines", afb.showDebugGapLine);
        GUILayout.Space(10);
        GUILayout.EndVertical();

        EditorGUI.BeginChangeCheck();
        afb.ignoreControlNodesLayerNum = EditorGUILayout.IntField("ignoreControlsLayerNum", afb.ignoreControlNodesLayerNum);
        if (EditorGUI.EndChangeCheck()) isDirty = true;

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            Close();

            if (isDirty)
            {
                List<Transform> posts = afb.postsPool;
                for (int p = 0; p < afb.allPostPositions.Count - 1; p++)
                {
                    if (posts[p] != null)
                        posts[p].gameObject.layer = 0;
                }
                afb.ForceRebuildFromClickPoints();
            }
            if (afb.railAColliderMode < ColliderType.originalCollider || afb.postColliderMode < ColliderType.originalCollider || afb.extraColliderMode < ColliderType.originalCollider)
            {
                Debug.Log("Colliders are being used. It's recommended to leave colliders off until ready to Finish the Fence. " +
                    "(They have to be recalculated every time there's a change, this can slow down responsiveness in the Editor.)\n");
            }

            GUIUtility.ExitGUI();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
            afb.postColliderMode = tempPostColliderMode;
            afb.railAColliderMode = tempRailColliderMode;
            afb.extraColliderMode = tempExtraColliderMode;

            afb.allowGaps = tempAllowGaps;
            afb.showDebugGapLine = tempShowDebugLines;

            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }
}