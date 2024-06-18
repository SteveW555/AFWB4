using UnityEngine;

public class MaterialUtilities : MonoBehaviour
{
    public static Material CreateTransparentMaterial(float r, float g, float b, float a)
    {
        // Create a new material with a transparent shader
        Material transparentMat = new Material(Shader.Find("Standard"));

        // Set the color to green with alpha for transparency
        transparentMat.color = new Color(r, g, b, a); // RGBA, A is the alpha for transparency

        // Enable transparency on the material
        transparentMat.SetFloat("_Mode", 3); // Sets the material to Transparent mode
        transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMat.SetInt("_ZWrite", 0);
        transparentMat.DisableKeyword("_ALPHATEST_ON");
        transparentMat.EnableKeyword("_ALPHABLEND_ON");
        transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMat.renderQueue = 3000;

        return transparentMat;
    }
}