using UnityEditor;
using UnityEngine;

public class ImageTooltipAttribute : PropertyAttribute
{
    public readonly string imagePath;

    public ImageTooltipAttribute(string imagePath)
    {
        this.imagePath = imagePath;
    }
}

[CustomPropertyDrawer(typeof(ImageTooltipAttribute))]
public class ImageTooltipDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var attribute = new ImageTooltipAttribute("Assets/Auto Fence Builder/Editor/Images/KeepRowLevelOn.jpg");
        if (attribute != null)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(attribute.imagePath);
            if (texture != null)
            {
                var tooltipRect = new Rect(position.x + position.width + 10, position.y, 50, 50);
                EditorGUI.LabelField(tooltipRect, GUIContent.none, new GUIStyle { normal = new GUIStyleState { background = texture } });
            }
        }

        EditorGUI.PropertyField(position, property, GUIContent.none);

        EditorGUI.EndProperty();
    }
}