using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GuiItemsUtilities : MonoBehaviour
{
    public static GUIStyle CreateLabelStyleWithBorder(int width, Color? bgCol = null, Color? borderCol = null)
    {
        GUIStyle customLabelStyle = new GUIStyle(EditorStyles.label);
        // Set the font to match the popup style
        customLabelStyle.font = EditorStyles.popup.font;

        if (bgCol == null)
            bgCol = new Color(0.31f, 0.31f, 0.31f, 1f);
        if (borderCol == null)
            borderCol = new Color(0.26f, 0.26f, 0.26f, 1f);

        int textureWidth = width;
        int textureHeight = (int)EditorStyles.label.CalcSize(new GUIContent("A")).y;

        // Create a new Texture2D object
        Texture2D labelBackground = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Fill the texture with the label background color
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgCol.Value;
        }
        labelBackground.SetPixels(pixels);

        // Add a black border around the texture
        for (int x = 0; x < textureWidth; x++)
        {
            labelBackground.SetPixel(x, 0, borderCol.Value);
            labelBackground.SetPixel(x, textureHeight - 1, borderCol.Value);
        }
        for (int y = 0; y < textureHeight; y++)
        {
            labelBackground.SetPixel(0, y, borderCol.Value);
            labelBackground.SetPixel(textureWidth - 1, y, borderCol.Value);
        }
        labelBackground.Apply();

        customLabelStyle.normal.background = labelBackground;

        //set text color
        customLabelStyle.normal.textColor = new Color(0.88f, 0.88f, 0.88f);

        return customLabelStyle;
    }
}
