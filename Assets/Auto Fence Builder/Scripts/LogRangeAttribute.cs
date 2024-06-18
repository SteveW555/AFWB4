using System;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LogRangeAttribute : PropertyAttribute
{
    public float min = 0.3f;
    public float center = 10;
    public float max = 100;
    public LogRangeAttribute(float min, float center, float max)
    {
        this.min = min;
        this.center = center;
        this.max = max;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LogRangeAttribute))]
public class LogRangePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LogRangeAttribute logRangeAttribute = (LogRangeAttribute)attribute;
        LogRangeConverter rangeConverter = new LogRangeConverter(logRangeAttribute.min, logRangeAttribute.center, logRangeAttribute.max);

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        property.floatValue = EditorGUILayout.FloatField(float.Parse(property.floatValue.ToString("F1")), GUILayout.Width(30));

        float value = rangeConverter.ToNormalized(property.floatValue);
        value = GUI.HorizontalSlider(position, value, 0, 1);
        //value = GUILayout.HorizontalSlider(value, 0, 1, GUILayout.Width(80));

        
        property.floatValue = rangeConverter.ToRange(value);

        if (property.floatValue == 2.0f)
            property.floatValue = 2.1f;

        //Debug.Log("logval " + property.floatValue + "\n");
        
        EditorGUI.EndProperty();
    }
}
#endif
