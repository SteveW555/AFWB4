using UnityEditor;
using UnityEngine;

public static class GUILayoutExtensions
{
    public static bool ButtonAutoWidth(string text, string helpText = "", GUIStyle style = null)
    {
        // Use the provided style or default to the current skin's button style
        style = style ?? GUI.skin.button;

        GUIContent buttonText = new GUIContent(text);
        float width = style.CalcSize(buttonText).x;
        return GUILayout.Button(new GUIContent(text, helpText), style, GUILayout.Width(width));
    }
}

public static class SeriaPropExt
{
    /// <summary> Sets the value of a SerializedProperty identified by its name, to a value of any supported type</summary>
    /// <param name="serializedObject">The SerializedObject containing the property. Probably the target of an Editor</param>
    /// <param name="variableName">The name of the property to set.</param>
    /// <param name="value">The value to set the property to. The type of the value must match the property type.</param>
    /// <param name="applyModifiedProperties">Whether to apply the modified properties to the serialized object.</param>
    /// <exception cref="ArgumentException">Thrown when the property type is unsupported.</exception>
    /// <example>
    /// // Example usage
    ///         SeriaPropExt.SetPropToWithValue(serializedObject, "myIntProperty", 42, true);
    ///         SeriaPropExt.SetPropToWithValue(serializedObject, "myStringProperty", "Hello, World!", true);

    /// </example>
    public static void SetPropToValue(SerializedObject serializedObject, string variableName, object value, bool applyModifiedProperties = true)
    {
        SerializedProperty menu = serializedObject.FindProperty("nodePostsOverrideMenuIndex");
        SerializedProperty prefab = serializedObject.FindProperty("nodePostsOverridePrefabIndex");

        SerializedProperty sp = serializedObject.FindProperty(variableName);
        if (sp == null)
        {
            Debug.LogError($"Property '{variableName}' not found");
            return;
        }
        if (value is int intValue)
            sp.intValue = intValue;
        else if (value is float floatValue)
            sp.floatValue = floatValue;
        else if (value is bool boolValue)
            sp.boolValue = boolValue;
        else if (value is string stringValue)
            sp.stringValue = stringValue;
        else if (value is Color colorValue)
            sp.colorValue = colorValue;
        else if (value is AnimationCurve curveValue)
            sp.animationCurveValue = curveValue;
        else if (value is Object objectValue)
            sp.objectReferenceValue = objectValue;
        else if (value is Vector2 vector2Value)
            sp.vector2Value = vector2Value;
        else if (value is Vector3 vector3Value)
            sp.vector3Value = vector3Value;
        else if (value is Vector4 vector4Value)
            sp.vector4Value = vector4Value;
        else if (value is Quaternion quaternionValue)
            sp.quaternionValue = quaternionValue;
        else
        {
            Debug.LogError($"Unsupported data type: {value.GetType()}");
            return;
        }
        if (applyModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }
}