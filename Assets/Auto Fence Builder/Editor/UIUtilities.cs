using AFWB;

using UnityEditor;

using UnityEngine;

#nullable enable // Now nullable annotations are respected in this file.

namespace TCT.UIUtilities
{
    public class UIUtilities
    {
        private AutoFenceCreator af;
        private AutoFenceEditor ed;
        private SerializedObject so;

        public static Color panelDarkSkinBg = new Color(0.2784f, 0.2784f, 0.2902f);  // 71, 71, 74, in hex: #47474A
        public static Color unityDarkSkinBg = new Color(0.22f, 0.22f, 0.22f); // 56, 56, 56, in hex: #383838
        public static Color unityDarkSkinDarkBg = new Color(0.1765f, 0.1765f, 0.1765f); // 45, 45, 45, in hex: #2D2D2D
        public static Color panelBg = new Color(.28f, .28f, .29f);// 71, 71, 74, in hex: 47474A

        public UIUtilities(AutoFenceCreator autoFenceCreator, AutoFenceEditor autoFenceEditor)
        {
            af = autoFenceCreator;
            ed = autoFenceEditor;
            so = ed.serializedObject;
        }

        public static Color ModifyColor(Color inCol, float r, float g, float b, float a = 1)
        {
            Color newColor = new Color(inCol.r + r, inCol.g + g, inCol.b + b, inCol.a + a);
            return newColor;
        }

        public static Color ModifyColor(Color inCol, float val)
        {
            Color newColor = new Color(inCol.r + val, inCol.g + val, inCol.b + val, 1);
            return newColor;
        }

        //-- This nonly requires 1 color and 1 pixel set
        public static Texture2D MakeTexSolid(Color col)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, col);
            texture.Apply();
            return texture;
        }

        // make a new GUIStyle with a solid color
        public static GUIStyle MakeStyle(Color col)
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = MakeTexSolid(col);
            return style;
        }

        //-------------------------------------------
        // Function to duplicate a specified GUIStyle
        // If no styleName is provided, it returns a default GUIStyle
        // An optional background color can be specified
        // Style name examples: "button", "label", "box", "textField", "window", "toggle", "scrollView".
        // "toggle", "scrollView", "horizontalSlider", "verticalSlider", "sliderThumb", "horizontalScrollbar", "verticalScrollbar",
        // "scrollbarThumb", "toolbar", "toolbarButton", "toolbarTextField", "toolbarDropDown", "menuItem", "menuBar", "area".
        //-- Useage: DuplicateGUIStyle("button");
        public static GUIStyle DuplicateGUIStyle(string? styleName = null, Color? backgroundColor = null)
        {
            //If n o style name is provided, return a default GUIStyle
            GUIStyle originalStyle = string.IsNullOrEmpty(styleName) ? new GUIStyle() : GUI.skin.FindStyle(styleName) ?? new GUIStyle();

            GUIStyle newStyle = new GUIStyle(originalStyle);

            // If a background color is provided, set it
            if (backgroundColor.HasValue)
            {
                newStyle.normal.background = MakeTexSolid(backgroundColor.Value);
            }

            return newStyle;
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}