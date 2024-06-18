using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Selection Count")]
internal class OverlayA : Overlay
{
    private Label m_Label;

    public override VisualElement CreatePanelContent()
    {
        m_Label = new Label($"Selection Count {Selection.count}");
        Selection.selectionChanged += UpdateLabel;
        return m_Label;
    }

    private void UpdateLabel()
    {
        m_Label.text = $"Selection Count {Selection.count}";
    }
}