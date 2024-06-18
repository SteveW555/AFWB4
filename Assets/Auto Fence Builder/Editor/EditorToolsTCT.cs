using UnityEditor;
using UnityEngine;

public class EditorToolsTCT
{
    public enum EditorThemeTCT
    { yellow = 0, darkCyan };

    public static EditorThemeTCT edTheme = EditorThemeTCT.yellow;

    private static GUIStyle moduleBoxStyle = new GUIStyle();
    private static Color boxBg = new Color(.65f, .25f, .25f);
    private static Color boxBorder = new Color(.17f, .17f, .17f);

    private static GUIStyle headingBoxStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
    private static Color headingBg = new Color(.85f, .71f, .33f);
    private static Color headingBorder = new Color(.17f, .17f, .17f);
    private static GUIStyle headingTextStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter };

    public static GUILayout.VerticalScope CreateModuleWithHeader(string name, GUIStyle style, bool showHelpButton = false)
    {
        headingBoxStyle.normal.background = MakeEditorTexWithBorder(600, 100, headingBg, new RectOffset(1, 1, 1, 1), headingBorder);

        edTheme = EditorThemeTCT.darkCyan;
        ChooseTheme();
        GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope("box");
        using (verticalScope)
        {
            //Rect rect = verticalScope.rect;
            //EditorGUILayout.HelpBox("WARNING!\n\nBaking is NOT the same as SkeletonAnimator!\nDoes not support the fot realtig is to export Spine ", MessageType.Warning, true);

#if UNITY_2019_1_OR_NEWER

            EditorGUILayout.Space(2);

#elif UNITY_2018_4_OR_NEWER
            GUILayout.Space(10);
#endif

            using (var horizontalScope = new GUILayout.HorizontalScope(headingBoxStyle))
            {
                EditorGUILayout.LabelField(name, headingTextStyle, GUILayout.ExpandWidth(true), GUILayout.Height(23));
            }
            GUILayout.Space(10); GUILayout.Space(10);
        }
        return verticalScope;
    }

    public static void ChooseTheme()
    {
        if (edTheme == EditorThemeTCT.yellow)
        {
            headingBg = new Color(.85f, .71f, .33f);
            headingBorder = new Color(.17f, .17f, .17f);

            headingTextStyle.normal.textColor = new Color(.25f, .25f, .25f, 0.83f);
            headingTextStyle.fontStyle = FontStyle.Bold;
            headingTextStyle.fontSize = 13;
        }
        else if (edTheme == EditorThemeTCT.darkCyan)
        {
            headingBg = new Color(.3f, .3f, .3f);
            headingBorder = new Color(.17f, .17f, .17f);

            //cyanBoldStyle.normal.textColor = new Color(0.28f, .53f, .78f);
            headingTextStyle.normal.textColor = new Color(0.22f, .5f, .8f);
            headingTextStyle.fontStyle = FontStyle.Bold;
            headingTextStyle.fontSize = 13;
        }
    }

    public static Texture2D MakeEditorTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public static Texture2D MakeEditorTexWithBorder(int width, int height, Color textureColor, RectOffset border, Color bordercolor)
    {
        int widthInner = width;
        width += border.left;
        width += border.right;
        Color[] pix = new Color[width * (height + border.top + border.bottom)];
        for (int i = 0; i < pix.Length; i++)
        {
            if (i < (border.bottom * width))
                pix[i] = bordercolor;
            else if (i >= ((border.bottom * width) + (height * width)))  //Border Top
                pix[i] = bordercolor;
            else
            { //Center of Texture
                if ((i % width) < border.left) // Border left
                    pix[i] = bordercolor;
                else if ((i % width) >= (border.left + widthInner)) //Border right
                    pix[i] = bordercolor;
                else
                    pix[i] = textureColor;    //Color texture
            }
        }
        Texture2D result = new Texture2D(width, height + border.top + border.bottom);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public static void CheckBoxPropertyTCT(string title, SerializedProperty property, int width = 80, string helpStr = "")
    {
        EditorGUILayout.LabelField(new GUIContent(title, helpStr), GUILayout.ExpandWidth(false), GUILayout.Width(50));
        EditorGUILayout.PropertyField(property, new GUIContent(""), GUILayout.ExpandWidth(false));
    }
}