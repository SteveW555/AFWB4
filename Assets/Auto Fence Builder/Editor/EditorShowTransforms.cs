using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AFWB
{
    public class EditorShowTransforms
    {
        public static void ShowTransformEditor(LayerSet layer, AutoFenceEditor ed, int inControlWidth = 344)
        {
            int labelWidth = 220, controlWidth = inControlWidth, endOfLineSpace = 2, toggleWidth = 20, miniButtonWidth = 20, preResetSpace = 8;
            SerializedProperty posProp = EdUtils.GetLayerPositionProperty(layer, ed);
            SerializedProperty scaleProp = EdUtils.GetLayerScaleProperty(layer, ed);
            SerializedProperty rotProp = EdUtils.GetLayerRotationProperty(layer, ed);
            string layerStr = EdUtils.GetLayerString(layer);

            //      Position
            //=========================
            EditorGUILayout.BeginHorizontal();
            Vector3 oldYPos = Vector3.zero;
            string helpStr = "Offset the relative postion of all " + layerStr;
            if (layer != LayerSet.postLayerSet)//because there is no Post offset property
            {
                oldYPos = posProp.vector3Value;
                EditorGUILayout.LabelField(new GUIContent(layerStr + " Position Offset", helpStr), GUILayout.Width(labelWidth));
                EditorGUILayout.PropertyField(posProp, new GUIContent("", "Offset the relative postion of all " + layerStr), GUILayout.Width(controlWidth));
                GUILayout.Space(preResetSpace);

                //-- Reset
                if (GUILayout.Button(new GUIContent("R", "Set Position values to default 0"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
                {
                    posProp.vector3Value = Vector3.zero;
                }
                if (posProp.vector3Value.y != oldYPos.y && (layer == LayerSet.railALayerSet && ed.af.keepRailGrounded[AutoFenceCreator.kRailALayerInt] == true ||
                    layer == LayerSet.railBLayerSet && ed.af.keepRailGrounded[AutoFenceCreator.kRailBLayerInt] == true))
                {
                    posProp.vector3Value = oldYPos;
                    Debug.LogWarning("The Rail vertical position is locked to the ground height. Select [Locked to Ground] to enable editing \n");
                }
            }
            

            GUILayout.Space(endOfLineSpace);
            EditorGUILayout.EndHorizontal();

            //      Scale
            //=========================
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            helpStr = "Scale the individual dimensions of all " + layerStr;
            if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
                helpStr += "\n\n Note: X will scale the visual length of a Rail but does not affect their spacing or the layout of other fence elements." +
                    "\nIf you want to scale a Rail from e.g. 3m to 5m, and want the layout to change, you need to also " +
                    "change the 'Post-Rail Spacing' to 5m in the Master Settings above.\n\n However you can create the effect of overlaps or gaps by scaling Rail X alone " +
                    "if that is the desired effect" +
                    "\n\n Rails are automatically scaled to span the correct 'Post-Rail Spacing' between Clickpoint Nodes, but be aware that if you have placed" +
                    "\two Nodes say, 2m apart, then the Rail has to be scaled down to fit in that span.";
            EditorGUILayout.LabelField(new GUIContent(layerStr + " Scale", helpStr), GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(scaleProp, new GUIContent("", "Offset the relative postion of all " + layerStr), GUILayout.Width(controlWidth));
            GUILayout.Space(preResetSpace);
            if (GUILayout.Button(new GUIContent("R", "Set Scale values to default 1"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
            {
                scaleProp.vector3Value = Vector3.one;
            }
            GUILayout.Space(endOfLineSpace);
            EditorGUILayout.EndHorizontal();

            //      Rotation
            //=========================
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(layerStr + " Rotation", GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(rotProp, new GUIContent("", "Rotate all " + layerStr), GUILayout.Width(controlWidth));
            GUILayout.Space(preResetSpace);
            if (GUILayout.Button(new GUIContent("R", "Set all Rotation values to default 0"), EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
            {
                rotProp.vector3Value = Vector3.zero;
            }
            GUILayout.Space(endOfLineSpace);
            EditorGUILayout.EndHorizontal();

            //================================================
            //          Main & End  Post Scale Boost
            //================================================      
            if (layer == LayerSet.postLayerSet)
            {
                GUILayout.Space(4);
                //if (ed.af.allowNodePostsPrefabOverride)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    MainPostsScaleBoost(ed, labelWidth, controlWidth, toggleWidth, preResetSpace); //Posts have a unique Scale Boost transform that the others don't have
                    EditorGUILayout.EndHorizontal(); 
                }

                //if (ed.af.allowEndPostsPrefabOverride)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    EndPostsScaleBoost(ed, labelWidth, controlWidth, toggleWidth, preResetSpace); //Posts have a unique Scale Boost transform that the others don't have
                    EditorGUILayout.EndHorizontal(); 
                }
            }

            scaleProp.vector3Value = ed.EnforceVectorMinimums(scaleProp.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
            rotProp.vector3Value = ed.EnforceRange360(rotProp.vector3Value);
        }

        //--------------------------------
        public static void MainPostsScaleBoost(AutoFenceEditor ed, int labelWidth, int controlWidth, int toggleWidth, int preResetSpace)
        {
            SerializedProperty scaleProp = EdUtils.GetLayerScaleProperty(LayerSet.postLayerSet, ed);
            EditorGUILayout.LabelField(new GUIContent(" - Node Posts Boost Scale", "Boost the relative scale of the Main (ClickPoint Nodes) Posts"), 
                ed.lilacUnityStyle, GUILayout.Width(labelWidth-2));
            EditorGUILayout.PropertyField(ed.mainPostsSizeBoostProp, new GUIContent("", "Boost the relative scale of the Main (ClickPoint Nodes) Posts"), 
                GUILayout.Width(controlWidth));
            GUILayout.Space(preResetSpace);
            if (GUILayout.Button(new GUIContent("R", "Set Scale values to default 1"), EditorStyles.miniButton, GUILayout.Width(toggleWidth)))
            {
                ed.mainPostsSizeBoostProp.vector3Value = Vector3.one;
            }
        }

        public static void EndPostsScaleBoost(AutoFenceEditor ed, int labelWidth, int controlWidth, int toggleWidth, int preResetSpace)
        {
            SerializedProperty scaleProp = EdUtils.GetLayerScaleProperty(LayerSet.postLayerSet, ed);
            EditorGUILayout.LabelField(new GUIContent(" - Ends Post Boost Scale", "Boost the relative scale of the First and Last Posts"),
                ed.lilacUnityStyle, GUILayout.Width(labelWidth-2));
            EditorGUILayout.PropertyField(ed.endPostsSizeBoostProp, new GUIContent("", "Boost the relative scale of the Main (ClickPoint Nodes) Posts"),
                GUILayout.Width(controlWidth));
            GUILayout.Space(preResetSpace);
            if (GUILayout.Button(new GUIContent("R", "Set Scale values to default 1"), EditorStyles.miniButton, GUILayout.Width(toggleWidth)))
            {
                ed.endPostsSizeBoostProp.vector3Value = Vector3.one;
            }
        }
    }

    //------------------------------------------

    public class EdUtils
    {
        public static SerializedProperty GetLayerPositionProperty(LayerSet layer, AutoFenceEditor ed)
        {
            if (layer == LayerSet.postLayerSet)
                return null;
            else if (layer == LayerSet.railALayerSet)
                return ed.railAPositionOffsetProp;
            else if (layer == LayerSet.railBLayerSet)
                return ed.railBPositionOffsetProp;
            else if (layer == LayerSet.subpostLayerSet)
                return ed.subpostPositionOffsetProp;
            else if (layer == LayerSet.extraLayerSet)
                return ed.extraPositionOffsetProp;
            return null;
        }

        public static SerializedProperty GetLayerScaleProperty(LayerSet layer, AutoFenceEditor ed)
        {
            if (layer == LayerSet.postLayerSet)
                return ed.postSizeProp;
            else if (layer == LayerSet.railALayerSet)
                return ed.railASizeProp;
            else if (layer == LayerSet.railBLayerSet)
                return ed.railBSizeProp;
            else if (layer == LayerSet.subpostLayerSet)
                return ed.subpostScaleProp;
            else if (layer == LayerSet.extraLayerSet)
                return ed.extraSizeProp;
            return null;
        }

        public static SerializedProperty GetLayerRotationProperty(LayerSet layer, AutoFenceEditor ed)
        {
            if (layer == LayerSet.postLayerSet)
                return ed.postRotationProp;
            else if (layer == LayerSet.railALayerSet)
                return ed.railARotationProp;
            else if (layer == LayerSet.railBLayerSet)
                return ed.railBRotationProp;
            else if (layer == LayerSet.subpostLayerSet)
                return ed.subpostRotationProp;
            else if (layer == LayerSet.extraLayerSet)
                return ed.extraRotationProp;
            return null;
        }

        public static string GetLayerString(LayerSet layer, bool noSpace = false, bool lowerCaseFirst = false)
        {
            string layerStr = "layer";

            if (layer == LayerSet.postLayerSet)
                layerStr = "Post";
            else if (layer == LayerSet.railALayerSet)
                layerStr = "Rail A";
            else if (layer == LayerSet.railBLayerSet)
                layerStr = "Rail B";
            else if (layer == LayerSet.subpostLayerSet)
                layerStr = "Subposts";
            else if (layer == LayerSet.extraLayerSet)
                layerStr = "Extras";

            if (noSpace == true)
                layerStr = String.Concat(layerStr.Where(c => !Char.IsWhiteSpace(c)));
            if (lowerCaseFirst == true)
            {
                StringBuilder sb = new StringBuilder(layerStr);
                sb[0] = Char.ToLower(layerStr[0]);
                layerStr = sb.ToString();
            }
            return layerStr;
        }

        //------------------------------
        public static string StrToCamel(String str)
        {
            str = String.Concat(str.Where(c => !Char.IsWhiteSpace(c)));
            StringBuilder sb = new StringBuilder(str);
            sb[0] = Char.ToLower(str[0]);
            str = sb.ToString();
            return str;
        }

        public static string StrNoSpace(String str)
        {
            str = String.Concat(str.Where(c => !Char.IsWhiteSpace(c)));
            return str;
        }

        //------------------------------
        public static string LayerToCamel(LayerSet layer)
        {
            string str = GetLayerString(layer);
            str = StrToCamel(str);
            return str;
        }

        public static string LayerNoSpace(LayerSet layer)
        {
            string str = GetLayerString(layer);
            str = StrNoSpace(str);
            return str;
        }

        //--------------------------------
    }
}