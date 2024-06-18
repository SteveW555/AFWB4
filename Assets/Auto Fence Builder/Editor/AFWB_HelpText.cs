using UnityEditor;
using UnityEngine;

public static class AFWB_HelpText
{
    private static bool showMasterHelp = false, showPresetsHelp = false, showComponentsHelp = false, showGlobalsHelp = false;

    public static GUIStyle helpBoldItalicStyle = new GUIStyle(EditorStyles.label);
    public static GUIStyle helpBoldStyle = new GUIStyle(EditorStyles.label);
    public static GUIStyle helpButtonStyle = new GUIStyle(EditorStyles.toolbarButton);

    //=================================
    //         Master
    //=================================
    public static void ShowMasterHelp(GUILayout.HorizontalScope horiz, GUIStyle style, int space)
    {
        SetupStyles(style);

        GUILayout.Space(space);

        if (GUILayout.Button(new GUIContent("?", "Show Help for Master Settings"), helpButtonStyle, GUILayout.Width(25)))
        {
            showMasterHelp = !showMasterHelp;
        }
        if (showMasterHelp)
        {
            if (horiz != null)
                horiz.Dispose();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Hover over parameter names for tooltip help", helpBoldStyle);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Inter-section distance", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Inbetween postsPool & rails are added between your click-points, at these distance intervals.");
            EditorGUILayout.LabelField("AFWB will choose the nearest value to this that will give a whole number of sections.");
            EditorGUILayout.LabelField("E.g. if your fence is 10m long and you request a distance of 3m, the nearest value to");
            EditorGUILayout.LabelField("give a whole number of sections would be 3.333 (3 sections * 3.333 = 10m).");
            EditorGUILayout.LabelField("The default target value is = 3m");
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Random Spacing", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Randomize the spacing of the inbetween sections");
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Inbetween Posts", helpBoldItalicStyle);
            EditorGUILayout.LabelField("You can enable/disable the inbetween post positions");
            EditorGUILayout.LabelField("To use inbetween Rails without postsPool, keep Inbetween Posts On & use [Hide Interpolated] in the Posts Controls");
            //EditorGUILayout.LabelField("Hide Interpolated in the Posts Controls");
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Smooth", helpBoldItalicStyle);
            EditorGUILayout.LabelField("This will create a smoother curve along your fence by adding & moving postsPool.");
            EditorGUILayout.LabelField("Spacing will determine how many, and how close the extra points are.");
            EditorGUILayout.LabelField("More detailed Smooth settings can be found under the Globas/Smoothing tab at the bottom");
            GUILayout.Space(10);

            if (GUILayout.Button("Close Help", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                showMasterHelp = false;
            }
            GUILayout.Space(10);
            //DrawUILine(new Color(0.8f,0.8f,0.8f,1.0f),0,1);
            GUILayout.EndVertical();
        }
    }

    //=================================
    //         Presets
    //=================================
    public static void ShowPresetsHelp(GUILayout.HorizontalScope horiz, GUIStyle style, int space)
    {
        SetupStyles(style);
        GUILayout.Space(space);
        string helpStr = "?";
        int helpButtW = 24;
        if (showPresetsHelp)
        {
            helpStr = "Close";
            helpButtW = 45;
        }
        if (GUILayout.Button(new GUIContent(helpStr, "Show Help for Presets"), helpButtonStyle, GUILayout.Width(helpButtW)))
        {
            showPresetsHelp = !showPresetsHelp;
        }
        if (showPresetsHelp)
        {
            if (horiz != null)
                horiz.Dispose();
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Hover over parameter names for tooltip help", helpBoldStyle);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Save Preset", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Enter a name in the 'Name Preset' field.");
            EditorGUILayout.LabelField("Choose the desired Category for the preset from the dropdown Category menu");
            EditorGUILayout.LabelField("or to create a new category add a category + '/' to the beginning of the name, for example");
            EditorGUILayout.LabelField("saving as 'Rustic/Old Wooden Fence' would create a new category for 'Rustic'");
            EditorGUILayout.LabelField("and save the preset 'Old Wooden Fence' in that category");
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Fave", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Copies the preset in to the Favourites folder");
            GUILayout.Space(10);

            if (GUILayout.Button("Close Help", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                showPresetsHelp = false;
            }
            GUILayout.Space(10);
            //DrawUILine(new Color(0.8f, 0.8f, 0.8f, 1.0f), 0, 1);
            GUILayout.EndVertical();
        }
    }

    //=================================
    //         Cmponents
    //=================================
    public static void ShowComponentsHelp(GUILayout.HorizontalScope horiz, GUIStyle style, int space)
    {
        SetupStyles(style);
        GUILayout.Space(space);
        string helpStr = "?";
        int helpButtW = 24;
        if (showComponentsHelp)
        {
            helpStr = "Close";
            helpButtW = 45;
        }
        if (GUILayout.Button(new GUIContent(helpStr, "Show Help for Components"), helpButtonStyle, GUILayout.Width(helpButtW)))
        {
            showComponentsHelp = !showComponentsHelp;
        }
        EditorStyles.label.wordWrap = true;
        if (showComponentsHelp)
        {
            if (horiz != null)
                horiz.Dispose();
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("*Hover over parameter names for tooltip help", helpBoldStyle);
            EditorGUILayout.LabelField("*Press [R] to set Default values for each parameter, or [S] to Seed Randomize", helpBoldStyle);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Components", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Fences & Walls are made up of five main components:");
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Posts", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Posts are the keypoints that determine the layout of your fence. \nTheir display can be disabled, but they are still there" +
                " behind the scenes providing positional \ninformation to the other components. \nUnlike the other components, they can't be repositioned via the Inspector" +
                ", \ninstead you should enable Show Controls in the lower left of the Scene View, \nwhere they can be repositioned with the Move tool.");
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Rails", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Rails are the sections that are built beween the postsPool, for example, \n a brick wall, or wooden rails.");
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Subposts", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Subposts are decorative - usually smaller - postsPool built beween the main postsPool. \nThey don't affect the layout" +
                " of the fence, and as such can be duplicated or positioned more freely." +
                "\nIf you can't see the Subposts even when enabled, they can be hidden within a Post or Rail - " +
                "\ntry using Subpost Scale to make them large enough to see.");
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Extras", helpBoldItalicStyle);
            EditorGUILayout.LabelField("These are multi-purpose accessory prefabs that can be added anywhere to add detail." +
                "\nExtras can also be used in Scatter Mode, where multiple gizmoSingletonInstance can be scattered " +
                "\nin any pattern around other elements of the fence." +
                "\nIf you can't see the Extras even when enabled, they can be hidden within a Post or Rail - " +
                "\ntry using Extra Scale to make them large enough to see.");

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Protected", helpBoldItalicStyle);
            // For v4.1
            //EditorGUILayout.LabelField("The 'P' switch nextPos to a component keeps that component Protected from deletionn" +
            //    "If you use the 'Remove Unused Assets From Project Folder (Globals/Resources) to delete all unused assets, " +
            //    "Protected components will *not* be removed even if they are not in use");

            GUILayout.Space(10);

            if (GUILayout.Button("Close Help", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                showComponentsHelp = false;
            }
            GUILayout.Space(10);
            //DrawUILine(new Color(0.8f, 0.8f, 0.8f, 1.0f), 0, 1);
            GUILayout.EndVertical();
        }
        EditorStyles.label.wordWrap = false;
    }

    //=================================
    //         Globals
    //=================================
    public static void ShowGlobalsHelp(GUILayout.HorizontalScope horiz, GUIStyle style, int space)
    {
        SetupStyles(style);
        GUILayout.Space(space);
        if (GUILayout.Button(new GUIContent("?", "Show Help for Globals"), helpButtonStyle, GUILayout.Width(25)))
        {
            showGlobalsHelp = !showGlobalsHelp;
        }
        EditorStyles.label.wordWrap = true;
        if (showGlobalsHelp)
        {
            if (horiz != null)
                horiz.Dispose();
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("*Hover over Tab names for tooltip help", helpBoldStyle);

            /*GUILayout.Space(10);
            EditorGUILayout.LabelField("Components", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Fences & Walls are made up of five main components:");

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Posts", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Posts are the keypoints that determine the layout of your fence. \nTheir display can be disabled, but they are still there" +
                " behind the scenes providing positional \ninformation to the other components. \nUnlike the other components, they can't be repositioned via the Inspector" +
                ", \ninstead you should enable Show Controls in the lower left of the Scene View, \nwhere they can be repositioned with the Move tool.");

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Rails", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Rails are the sections that are built beween the postsPool, for example, \n a brick wall, or wooden rails.");

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Subposts", helpBoldItalicStyle);
            EditorGUILayout.LabelField("Subposts are decoraive - usually smaller - postsPool built beween the main postsPool. \nThey don't affect the layout" +
                " of the fence, and as such can be duplicated or positioned more freely." +
                "\nIf you can't see the Subposts even when enabled, they can be hidden within a Post or Rail - " +
                "\ntry using Subpost Scale to make them large enough to see.");

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Extras", helpBoldItalicStyle);
            EditorGUILayout.LabelField("These are multi-purpose accessory prefabs that can be added anywhere to add detail." +
                "\nExtras can also be used in Scatter Mode, where multiple gizmoSingletonInstance can be scattered " +
                "\nin any pattern around other elements of the fence." +
                "\nIf you can't see the Extras even when enabled, they can be hidden within a Post or Rail - " +
                "\ntry using Extra Scale to make them large enough to see.");
            GUILayout.Space(10);*/

            if (GUILayout.Button("Close Help", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                showGlobalsHelp = false;
            }
            GUILayout.Space(10);
            //DrawUILine(new Color(0.8f, 0.8f, 0.8f, 1.0f), 0, 1);
            GUILayout.EndVertical();
        }
        EditorStyles.label.wordWrap = false;
    }

    private static void SetupStyles(GUIStyle style)
    {
        helpBoldItalicStyle.fontStyle = FontStyle.BoldAndItalic;
        helpBoldItalicStyle.fontSize = 13;

        helpBoldStyle.fontStyle = FontStyle.Bold;
        helpBoldStyle.fontSize = 13;

        helpButtonStyle.fontStyle = FontStyle.Bold;
        helpButtonStyle.normal.textColor = style.normal.textColor;
    }

    //----------------
    public static void DrawUILine(Color color, int widthEndPadding, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        //r.width = 500;
        r.width -= widthEndPadding;
        EditorGUI.DrawRect(r, color);
    }
}