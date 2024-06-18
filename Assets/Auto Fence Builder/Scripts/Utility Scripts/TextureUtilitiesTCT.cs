using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class TextureUtilitiesTCT : MonoBehaviour
{
    public static Texture2D CreateTex2D(int width, int height, Color color)
    {
        var texture = new Texture2D(width, height);
        Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
