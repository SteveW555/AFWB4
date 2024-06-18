//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414

using AFWB;
using MeshUtils;
using UnityEditor;
using UnityEngine;

public class TestWindow : EditorWindow
{
}

public class BakeRotationsWindow : EditorWindow
{
    //AutoFenceEditor ed = null;
    public AutoFenceCreator af = null;

    private AutoFenceEditor ed = null;
    private bool isDirty = false;
    private Color darkGrey = new Color(.15f, .15f, .15f);
    private Color darkCyan = new Color(0, .5f, .75f);
    private GUIStyle infoStyle, headingStyle;
    private bool x90 = false, y90 = false, z90 = false;
    private bool x90minus = false, y90minus = false, z90minus = false;
    private Vector3 tempRailUserMeshBakeRotations = Vector3.zero;
    private Vector3 tempPostUserMeshBakeRotations = Vector3.zero;
    public LayerSet layerSet;
    public PrefabTypeAFWB prefabType;
    public int selctionMode = 0;// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh

    //public string[] selStrings = new string[] { "Use Above Rotations", "Auto", "Don't Rotate" };
    public string[] selStrings = new string[] { "Rotate X", "Rotate Y", "Rotate Z" };

    private GameObject customObject = null;

    public BakeRotationsWindow()
    {
    }

    public void Init(AutoFenceEditor ed, LayerSet inLayerSet)
    {
        this.ed = ed;
        af = ed.af;
        layerSet = inLayerSet;
        prefabType = af.GetPrefabTypeFromLayer(layerSet);
        customObject = af.GetUserObjectForLayer(layerSet);
    }

    //--------------------------
    private void SetValuesInAFB()
    {
    }

    //--------------------------
    private void SetSelectionMode(int inSelectionMode)
    {
    }

    //-------------------
    private void OnGUI()
    {
        AutoFenceEditor editor = null;
        AutoFenceEditor[] editors = Resources.FindObjectsOfTypeAll<AutoFenceEditor>();
        if (editors != null && editors.Length > 0)
            editor = editors[0];
        if (editor != null)
            editor.rotationsWindowIsOpen = true;

        headingStyle = new GUIStyle(EditorStyles.label);
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.normal.textColor = darkCyan;
        if (EditorGUIUtility.isProSkin)
        {
            infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.fontStyle = FontStyle.Normal;
            infoStyle.fontSize = 12;
            //infoStyle.normal.textColor = darkGrey;
        }

        if (customObject == null)
        {
            //infoStyle.normal.textColor = Color.red;
            GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
            EditorGUILayout.LabelField("You need to import a GameObject first. Drag & Drop in to the 'Custom Object Import' box. Then re-open this dialog", infoStyle);
            GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10); GUILayout.Space(10);
            if (GUILayout.Button("OK"))
            {
                Close();
                if (editor != null)
                    editor.rotationsWindowIsOpen = false;
                if (isDirty)
                {
                    SetValuesInAFB();
                }
                GUIUtility.ExitGUI();
            }
            return;
        }

        GUILayout.Space(10);
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Imported Mesh Rotation Baking", headingStyle);
        GUILayout.Space(20);
        string layerStr = EdUtils.GetLayerString(layerSet);
        EditorGUILayout.LabelField(layerStr, headingStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Note: These are not Unity rotations, instead the mesh vertices are being rotated, in the order Z, X, Y)\n", infoStyle, GUILayout.Height(21));
        EditorGUILayout.LabelField("Although Rotations could also be corrected in the Rotation section, it becomes very counter-intuitive to then apply \n", infoStyle, GUILayout.Height(21));
        EditorGUILayout.LabelField("creative rotations on top of the corrective ones\n", infoStyle, GUILayout.Height(20));

        //======== X Y Z Buttons ============
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate X 90", GUILayout.Width(120)))
        {
            RotateX();
        }
        if (GUILayout.Button("Rotate Y 90", GUILayout.Width(120)))
        {
            RotateY();
        }
        if (GUILayout.Button("Rotate Z 90", GUILayout.Width(120)))
        {
            RotateZ();
        }

        //=====  Reset =====
        GUILayout.Space(15);
        if (GUILayout.Button("Reset", GUILayout.Width(120)))
        {
            ResetAll();
        }

        //=====  Auto =====
        GUILayout.Space(15);
        if (GUILayout.Button("Auto", GUILayout.Width(120)))
        {
            AutoRotate();
        }
        GUILayout.EndHorizontal();

        //============== Rescale ==============
        //
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("If the svRotation looks correct, but the dimensions are not suitable for the part you can try rescaling." +
            "\nNote: This does not need to be perfect as you can set the sizes accurately in the controls section." +
            "\nAs with svRotation, you just need to start with something approximately Post or Wall/Rail shaped " +
            "\nto avoid using extreme values later", infoStyle, GUILayout.Width(580), GUILayout.Height(65));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto Rescale", GUILayout.Width(120)))
        {
            UserCustomPrefab.ScaleCustomObjectSuitableForType(customObject, prefabType);
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("+X", GUILayout.Width(30)))
        {
            MeshUtilitiesAFWB.ScaleMesh(customObject, new Vector3(1.25f, 1, 1));
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }
        if (GUILayout.Button("-X", GUILayout.Width(30)))
        {
            MeshUtilitiesAFWB.ScaleMesh(customObject, new Vector3(0.8f, 1, 1));
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("+Y", GUILayout.Width(30)))
        {
            MeshUtilitiesAFWB.ScaleMesh(customObject, new Vector3(1, 1.25f, 1));
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }
        if (GUILayout.Button("-Y", GUILayout.Width(30)))
        {
            MeshUtilitiesAFWB.ScaleMesh(customObject, new Vector3(1, 0.8f, 1));
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("+Z", GUILayout.Width(30)))
        {
            MeshUtilitiesAFWB.ScaleMesh(customObject, new Vector3(1, 1, 1.25f));
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }
        if (GUILayout.Button("-Z", GUILayout.Width(30)))
        {
            MeshUtilitiesAFWB.ScaleMesh(customObject, new Vector3(1, 1, 0.8f));
            af.ResetPoolForLayer(layerSet);
            af.ForceRebuildFromClickPoints();
        }

        GUILayout.EndHorizontal();
        //--------------------------------------

        // Force-close in the event of a Unity glitch (control-right-click)
        Event currentEvent = Event.current;
        if (currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
        {
            Close();
            editor.rotationsWindowIsOpen = false;
            GUIUtility.ExitGUI();
        }

        GUILayout.Space(15);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("OK"))
        {
            Close();
            if (editor != null)
                editor.rotationsWindowIsOpen = false;
            if (isDirty)
            {
                SetValuesInAFB();
            }
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10); GUILayout.Space(10);
        EditorGUILayout.LabelField("Imported Models need to be in the appropriate orientation for use as as a wall or post. For example, a cylinder\n", infoStyle, GUILayout.Height(20));
        EditorGUILayout.LabelField("on its side would need to be rotated upright to be a useful post. Something used as a rail/wall is usually longer \n", infoStyle, GUILayout.Height(20));
        EditorGUILayout.LabelField("than it is wide/tall. In this case, you would rotate something that looks like a post on to its side to become a rail.\n", infoStyle, GUILayout.Height(20));
        EditorGUILayout.LabelField("If the mesh is not rotated correctly there will be stretching or flattening, e.g. if a post was stretched to a 3m wide wall.\n", infoStyle, GUILayout.Height(20));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Auto", headingStyle);
        EditorGUILayout.LabelField("Most of the time 'Auto' will correctly guess based on the relative dimensions, so try this first.\n", infoStyle, GUILayout.Height(20));
        EditorGUILayout.LabelField("However, Game Objects with unusual shapes, or complex parent/child transforms might give unexpected results.\n", infoStyle, GUILayout.Height(20));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Use the three Rotae Buttons if you want to set them manually.\n", infoStyle, GUILayout.Height(20));
        GUILayout.Space(10);
        /*EditorGUILayout.LabelField("Custom", headingStyle);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Press preview, or re-import to apply these changes\n", infoStyle, GUILayout.Height(20));*/
    }

    //-------------------------------
    private void RotateX()
    {
        MeshUtilitiesAFWB.RotateX(customObject, 90);
        if (prefabType == PrefabTypeAFWB.railPrefab)
            MeshUtilitiesAFWB.SetRailPivotToLeftCentre(customObject);
        if (prefabType == PrefabTypeAFWB.postPrefab)
            MeshUtilitiesAFWB.SetPivotToCentreBase(customObject);

        af.ResetPoolForLayer(layerSet);
        af.ForceRebuildFromClickPoints();
    }

    //-------------------------------
    private void RotateY()
    {
        MeshUtilitiesAFWB.RotateY(customObject, 90);

        if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            UserCustomPrefab.ScaleRailToDefaultX(customObject);
            MeshUtilitiesAFWB.SetRailPivotToLeftCentre(customObject);
        }
        if (prefabType == PrefabTypeAFWB.postPrefab)
            MeshUtilitiesAFWB.SetPivotToCentreBase(customObject);

        af.ResetPoolForLayer(layerSet);
        af.ForceRebuildFromClickPoints();
    }

    //-------------------------------
    private void RotateZ()
    {
        MeshUtilitiesAFWB.RotateZ(customObject, 90);
        if (prefabType == PrefabTypeAFWB.railPrefab)
            MeshUtilitiesAFWB.SetRailPivotToLeftCentre(customObject);

        af.ResetPoolForLayer(layerSet);
        af.ForceRebuildFromClickPoints();
    }

    //-------------------------------
    private void AutoRotate()
    {
        UserCustomPrefab.AutoRotate(customObject, af, layerSet);

        af.ResetPoolForLayer(layerSet);
        af.ForceRebuildFromClickPoints();
    }

    //-------------------------------
    private void ResetAll()
    {
        UserCustomPrefab.ResetMeshOnPrefab(customObject, af, layerSet);
        if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            UserCustomPrefab.ScaleRailToDefaultX(customObject);
            MeshUtilitiesAFWB.SetRailPivotToLeftCentre(customObject);
        }
        if (prefabType == PrefabTypeAFWB.postPrefab)
        {
            MeshUtilitiesAFWB.SetPivotToCentreBase(customObject);
        }

        af.ResetPoolForLayer(layerSet);
        af.ForceRebuildFromClickPoints();
    }

    //-------------------------------
}