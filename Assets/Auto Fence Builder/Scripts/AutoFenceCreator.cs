/* Auto Fence & Wall Builder v3.5 twoclicktools@gmail.com May 2023*/
//#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
//#pragma warning disable 0414 // same for private fields

using MeshUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices; //Needed only for Debug Stack Trace
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace AFWB
{
    /// <summary> 
    /// Defines the modes for creating variation in different sections of the fence.
    /// <para> 'optimal' is the optimal mode for variation, whereauto-calculated to try to givem variation</para>
    /// <para> 'random' is the random mode for variation, the amount of randomness can be set for each parameter </para>
    /// <para> 'sequenced' is the sequenced mode for variation, where each section is assigned a step whose variations can be set in the inspector </para>
    /// </summary>
    public enum VariationMode
    {
        optimal = 0, random, sequenced, none
    };
    /// <summary> 
    /// Defines the styles for joints in the fence where each section meets.
    /// <para> 'simple' is the simple joint style, where the fence sections just meet at the posts.</para>
    /// <para> 'overlap' is the overlap joint style, where the fence sections overlap at the posts. </para>
    /// <para> 'mitre' is the mitre joint style, where the fence sections are cut at an angle to meet at the posts. </para>
    /// </summary>
    public enum JointStyle
    {
        simple = 0, overlap, mitre
    };
    public enum CornerRotation { none = 0, incoming, outgoing, average };

    /// <summary> 
    /// Defines the types of prefabs in the Auto Fence Builder.
    /// <para> 'postPrefab' is the prefab type for fence posts and subposts as they share the same source prefabs.</para>
    /// <para> 'railPrefab' is the prefab type for fence rails A or B as they share the same source prefabs. </para>
    /// <para> 'extraPrefab' is the prefab type for extra components or decorations on the fence. </para>
    /// <para> 'allPrefab' & 'nonePrefab' are used internally to process all or none of the above. </para>
    /// </summary>
    public enum PrefabTypeAFWB
    {
        postPrefab = 0, railPrefab, extraPrefab, allPrefab, nonePrefab
    };

    /// <summary> 
    /// Defines the types of layer sets in the Auto Fence Builder.
    /// <para> 'railALayerSet' is the layer set type for fence rail A.</para>
    /// <para> 'railBLayerSet' is the layer set type for fence rail B. </para>
    /// <para> 'postLayerSet' is the layer set type for fence posts. </para>
    /// <para> 'extraLayerSet' is the layer set type for extra components or decorations on the fence. </para>
    /// <para> 'subpostLayerSet' is the layer set type for sub-posts on the fence. </para>
    /// <para> 'markerLayerSet' is a special type representing layers such as "Markers" not defined above</para>
    /// <para> 'allLayerSet' is a special type representing all the above types. </para>
    /// <para> 'noneLayerSet' is a special type representing no layer set. </para>
    /// </summary>
    public enum LayerSet
    {
        railALayerSet = 0, railBLayerSet = 1, postLayerSet = 2, extraLayerSet = 3, subpostLayerSet = 4, markerLayerSet = 5, allLayerSet = 6, noneLayerSet = 7
    };
    public enum Seed { railAFlip = 0, railAHeight };
    public enum RotationType { none = 0, x90, x180, y90, y180, z90, z180 };

    /// <summary> 
    /// Defines the types of components in the Auto Fence Builder toolbar.
    /// <para> 'posts' represents the post component in the toolbar.</para>
    /// <para> 'railsA' represents rail A component in the toolbar. </para>
    /// <para> 'railsB' represents  rail B component in the toolbar. </para>
    /// <para> 'subposts' represents the subpost component in the toolbar. </para>
    /// <para> 'extras' represents the extra components or decorations in the toolbar. </para>
    /// </summary>
    public enum ComponentToolbar { posts = 0, railsA = 1, railsB, subposts, extras };
    public enum SlopeMode { slope = 0, step, shear };
    public enum AxisChoice3D { X = 0, Y, Z, XY, XZ, YZ, XYZ };
    public enum UseIncline { IncludeIncline, IgnoreIncline };
    public enum CornerOrientation { cornerRight, cornerLeft, none } // Corner Right is Clockwise (0  180), Corner Left is Counter-Clockwise (0 to -180 or 180 to 360)
    public enum CornerDirectionMode { currentDirection, averageDirection };
    //0 = single box, 1 = keep original (user), 2 = no colliders
    public enum ColliderType { boxCollider = 0, originalCollider = 1, noCollider = 2, meshCollider = 3, capsuleCollider = 4, customCollider = 5, allColliders = 6 };

    public enum DuplicateMode { single = 0, dual, dualWithCentre };

    /// <summary>
    /// When the Rail is build, determines:
    /// Basic: the Rail is simply translated according to its position offset which can create gaps
    /// Joined: behaves as if the Posts were in the translated positions, then moves & scales to form a continuous rail
    /// </summary>
    public enum RailOffsetMode { basic = 0, joined = 1 };

    public enum RandomScope { main = 0, variations = 1, all=2};

    /// <summary>A basic upright tall box to act as a marker in debugging</summary>

    //-----------------------
    public struct Quadrilateral2D
    {
        public Vector2[] v;

        /// <summary>Constructor with four points.</summary>
        public Quadrilateral2D(Vector2 pt0, Vector2 pt1, Vector2 pt2, Vector2 pt3)
        {
            v = new Vector2[4];
            v[0] = pt0;
            v[1] = pt1;
            v[2] = pt2;
            v[3] = pt3;
        }

        /// <summary>Constructor with an array of points. Optionally offsets each vertex by a given Vector2 offset.</summary>
        /// <param name="pts">Array of points to initialize the quadrilateral. Only the first 4 points are used if the array is longer.</param>
        /// <param name="optionalOffset">Optional offset to apply to each vertex. Default is Vector2.zero.</param>
        public Quadrilateral2D(Vector2[] pts, Vector2 optionalOffset = default)
        {
            v = new Vector2[4];

            if (pts.Length > 4)
                Debug.Log($"Input array has more than 4 elements. Only the first 4 will be used. Array size: {pts.Length}\n");

            for (int i = 0; i < Math.Min(4, pts.Length); i++)
                v[i] = pts[i] + optionalOffset;

            if (pts.Length < 4)
                Debug.Log($"Input array has less than 4 elements. Missing elements will be set to Vector2.zero. Array size: {pts.Length}\n");

            for (int i = pts.Length; i < 4; i++)
                v[i] = Vector2.zero + optionalOffset;
        }

        /// <summary>Wrapper constructor with an array of points. Optionally offsets each vertex by a given Vector3 offset.</summary>
        /// <param name="pts">Array of points to initialize the quadrilateral. Only the first 4 points are used if the array is longer.</param>
        /// <param name="optionalOffset">Optional Vector3 offset to apply to each vertex. Default is Vector3.zero.</param>
        /*public Quadrilateral2D(Vector2[] pts, Vector3 optionalOffset = default) : this(pts, new Vector2(optionalOffset.x, optionalOffset.z))
        {
        }*/

        /// <summary>Offsets each vertex of the quadrilateral by a given Vector2 offset. This is typically used to position the quadrilateral in world space.</summary>
        /// <param name="offset">The Vector2 offset to apply to each vertex.</param>
        public void OffsetQuadrilateral(Vector2 offset)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] += offset;
        }

        /// <summary>Converts the quadrilateral vertices to a Vector3 array with the y value set to a fixed value.</summary>
        /// <param name="yValue">The y value to use for the Vector3 array.</param>
        /// <returns>An array of Vector3 where each Vector3 is a vertex of the quadrilateral with the y value set to the specified value.</returns>
        public Vector3[] ToVector3Array(float yValue = 0)
        {
            Vector3[] vector3s = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
                vector3s[i] = new Vector3(v[i].x, yValue, v[i].y);

            return vector3s;
        }
    }

    public struct Triangle2D
    {
        public Vector2[] v;

        // You can add constructors, methods, or properties as needed
        public Triangle2D(Vector2 pt0, Vector2 pt1, Vector2 pt2)
        {
            v = new Vector2[3];
            v[0] = pt0;
            v[1] = pt1;
            v[2] = pt2;
        }
        //-- Get as Vector3s with the y set to 0 or some other fixed value
        public Vector3[] ToVector3Array(float yValue = 0)
        {
            Vector3[] vector3s = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vector3s[i] = new Vector3(v[i].x, yValue, v[i].y);
            }
            return vector3s;
        }
    }
    //---------------------------------
    //This can be any arbitrary five-sided poly, doesn't have to be a regular pentagon
    public class Pentagon2D
    {
        public Vector2[] v;

        /// <summary>Constructor with four points.</summary>
        public Pentagon2D(Vector2 pt0, Vector2 pt1, Vector2 pt2, Vector2 pt3, Vector2 pt4)
        {
            v = new Vector2[5];
            v[0] = pt0;
            v[1] = pt1;
            v[2] = pt2;
            v[3] = pt3;
            v[4] = pt4;
        }

        /// <summary>Constructor with an array of points. Optionally offsets each vertex by a given Vector2 offset.</summary>
        /// <param name="pts">Array of points to initialize the pentagon. Only the first 5 points are used if the array is longer.</param>
        /// <param name="optionalOffset">Optional offset to apply to each vertex. Default is Vector2.zero.</param>
        public Pentagon2D(Vector2[] pts, Vector2 optionalOffset = default)
        {
            v = new Vector2[5];

            if (pts.Length > 5)
                Debug.Log($"Input array has more than 5 elements. Only the first 5 will be used. Array size: {pts.Length}\n");

            for (int i = 0; i < Math.Min(5, pts.Length); i++)
                v[i] = pts[i] + optionalOffset;

            if (pts.Length < 5)
                Debug.Log($"Input array has less than 5 elements. Missing elements will be set to Vector2.zero. Array size: {pts.Length}\n");

            for (int i = pts.Length; i < 5; i++)
                v[i] = Vector2.zero + optionalOffset;
        }

        /// <summary>Wrapper for Pentagon2D constructor using constructor chaining</summary>
        public Pentagon2D(Vector2[] pts, Vector3 optionalOffset)
            : this(pts, new Vector2(optionalOffset.x, optionalOffset.z)) { }

        /// <summary>Offsets each vertex of the pentagon by a given Vector2 offset. This is typically used to position the pentagon in world space.</summary>
        /// <param name="offset">The Vector2 offset to apply to each vertex.</param>
        public void OffsetPentagon(Vector2 offset)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] += offset;
        }
        /// <summary>Offsets the vertices of the pentagon by the given x and yz (y or z depending if using v2 or v3).</summary>
        public void OffsetPentagon(float x, float yz)
        {
            OffsetPentagon(new Vector2(x, yz));
        }
        //--------
        public void OffsetPentagon(Vector3 offset)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i].x += offset.x;
                v[i].y += offset.z;
            }
        }
        //--------------------------------------------------------
        /// <summary>Converts the pentagon vertices to a Vector3 array with the y value set to a fixed value.</summary>
        /// <param name="yValue">The y value to use for the Vector3 array.</param>
        /// <returns>An array of Vector3 where each Vector3 is a vertex of the pentagon with the y value set to the specified value.</returns>
        public Vector3[] ToVector3Array(float yValue = 0)
        {
            Vector3[] vector3s = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
                vector3s[i] = new Vector3(v[i].x, yValue, v[i].y);

            return vector3s;
        }
        public void Print()
        {
            Debug.Log("\n");
            for (int i = 0; i < v.Length; i++)
                Debug.Log($"Vertex {i}: {v[i]}\n\n");
        }

        /// <summary>Expands the pentagon by moving each vertex away from the center by the given scale factor.</summary>
        /// <param name="scaleFactor">The factor by which to scale the distance of each vertex from the center.</param>
        public void ExpandPentagon(float scaleFactor)
        {
            Vector2 center = CalculateCenter();

            for (int i = 0; i < v.Length; i++)
            {
                Vector2 direction = (v[i] - center).normalized;
                v[i] = center + direction * Vector2.Distance(center, v[i]) * scaleFactor;
            }
        }
        public void ExpandPentagonByDistance(float extra)
        {
            Vector2 center = CalculateCenter();

            for (int i = 0; i < v.Length; i++)
            {
                Vector2 direction = (v[i] - center).normalized;
                v[i] = center + direction * (Vector2.Distance(center, v[i]) + extra);
            }
        }

        /// <summary>Calculates the center of the pentagon based on its vertices.</summary>
        /// <returns>The center point of the pentagon.</returns>
        private Vector2 CalculateCenter()
        {
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i];
            }
            return sum / v.Length;
        }


    }


    //-----------------------------------------------------------

    public struct ParallelNode
    {
        public Vector3 left, center, right;
        public Vector3 avgPosLeft, avgPosRight;
        public NodeInfo nodeDirection;
        public float width;
        public CornerDirectionMode elbowMode;
        public bool isInsert;
        public bool excludeLeft, excludeRight; // for when we've interpolated outer elbow at acute angle joints
    }

    public struct NodeInfo
    {
        public Vector3 position;
        public Vector3 dirLeft, dirRight;
        public Vector3 forward; // the direction to the Next node
        public Vector3 avgForward; // the average direction between the previous and nextPos
        public Vector3 prevForward; // the direction to the Previous node
        public Vector3 avgRight, avgLeft; //perpendicular to nodeAvgForward
        public CornerOrientation cornerOrientation; // if a corner between prevPos/nextPos, is it turning left or right
        public float angle; //the angle between prevPos-this and this-nextPos
    }

    [System.Serializable]
    /*public class RandomRecords
    {
        public int sequenceShuffle;
        public int heightVariation, smallRotVariation, quantRotVariation;

    }*/

    public class PrefabDetails
    {
        public bool isFavorite = false;
        public bool isProtected = false;
        public string parentFolderName;
        public PrefabDetails(bool fav, bool prot, string parentDirName)
        {
            isFavorite = fav;
            isProtected = prot;
            parentFolderName = parentDirName;
        }
        public PrefabDetails(string parentDirName)
        {
            isFavorite = false;
            isProtected = false;
            parentFolderName = parentDirName;
        }
        public static List<PrefabDetails> GetPrefabDetailsForLayer(LayerSet layer, AutoFenceCreator af)
        {
            List<PrefabDetails> prefabDetails = null;

            if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
                prefabDetails = af.railPrefabDetails;
            else if (layer == LayerSet.postLayerSet || layer == LayerSet.subpostLayerSet)
                prefabDetails = af.postPrefabDetails;
            else if (layer == LayerSet.extraLayerSet)
                prefabDetails = af.extraPrefabDetails;


            if (prefabDetails == null)
            {
                Debug.LogWarning($"prefabDetails was null for layer {layer.String()} in GetPrefabDetailsForPrefabType().  Making new List \n");
                prefabDetails = new List<PrefabDetails>();
            }
            return prefabDetails;
        }
    }

    //=====================================================================================================
    /* We also create postDirectionVectorsRight at the same time.
    * Useage:  PostVectorList postDirectionVectors = new PostVectorList();
    * Vector3 v = postDirectionVectors[0];  and   int count = postDirectionVectors.Count;  are fine
    * Not allowed direct write: postDirectionVectors[0] = myVector3 ; Use AddPostDirectionVector() or ModifyPostDirectionVector() (defined in af) instead
    * for postDirectionVectors.Add() use AddPostDirectionVector() instead
    **/

    //=====================================================================================================

    [ExecuteInEditMode]
    //------------------------------------
    [System.Serializable]
    public partial class AutoFenceCreator : MonoBehaviour, ISourceVariantObserver
    {
        // Create an string array for the first 5LayerSet names. GetNames returns a string array, Take(5) gets first 5 as IEnumerable, Select removes "Layer".
        /// <summary>Names of the layer sets</summary>
        private static readonly string[] _layerSetNames = Enum.GetNames(typeof(LayerSet))
                                                              .Take(5)
                                                              .Select(name => name.Replace("LayerSet", ""))
                                                              .ToArray();
        public string[] LayerSetNames => _layerSetNames;

        readonly LayerSet kRailALayer = LayerSet.railALayerSet; // to save a lot of typing
        readonly LayerSet kRailBLayer = LayerSet.railBLayerSet;
        LayerSet kPostLayer = LayerSet.postLayerSet;
        LayerSet kExtraLayer = LayerSet.extraLayerSet;
        LayerSet kSubpostLayer = LayerSet.subpostLayerSet;
        LayerSet kAllLayer = LayerSet.allLayerSet;
        public enum RailLayerIndex { railLayerIndexA = 0, railLayerIndexB };
        public const int kRailALayerInt = 0, kRailBLayerInt = 1, kPostLayerInt = 2, kSubpostLayerInt = 3, kExtraLayerInt = 4; // the index of these layers in LayerSet
        public const int kMaxNumSourceVariants = 9, kMaxNumSeqSteps = 20, kMaxNumSingles = 100;

        public RandomSeededValuesAF postAndGlobalSeeds, railASeeds, railBSeeds, subpostSeeds, extraSeeds;
        //public List<RandomSeededValuesAF> presetSeedsList;
        public ComponentToolbar componentToolbar = ComponentToolbar.railsA;
        public Vector3 Vecc3Ignore = new Vector3(-10000, -10000, -10000);
        public List<string> categoryNames = new List<string>(new string[] { "Auto" });
        public enum SplineFillMode { fixedNumPerSpan = 0, equiDistant, angleDependent };
        public enum VariationSwapOutMode { linearCycle = 0, randomProb, sequenced }; // we're referring to either the Main rails, or the Secondary rails
        public enum ItemVariationMode { optimalVariation = 0, random, randomNoRepeat, shuffled, sequenced };
        //public Dictionary<string, PrefabDetailsStruct> railPrefabDetails = new Dictionary<string, PrefabDetailsStruct>(); // name(key), favorite, protected
        public List<PrefabDetails> railPrefabDetails = new List<PrefabDetails>();
        public List<PrefabDetails> postPrefabDetails = new List<PrefabDetails>();
        public List<PrefabDetails> extraPrefabDetails = new List<PrefabDetails>();
        public List<PrefabDetails> subpostPrefabDetails = new List<PrefabDetails>();



        public int autoFenceInstanceNum = 0; // 1-based indexing
        public int objectsPerFolder = 150; // lower this number if using high-poly meshes. Only 65k can be combined, so objectsPerFolder * [number of verts/tris in mesh] must be less than 65,000
        public const float DEFAULT_RAIL_LENGTH = 3.0f;
        [Range(0.1f, 10.0f)]
        public float gs = 1.0f; //global adjustedNativeScale, avoided long name as it occurs so often and takes up space!
        public Vector3 globalScale = Vector3.one;
        public bool scaleInterpolationAlso = true; // this can be annoying if you want your postsPool to stay where they are.
        public Vector3 addIntersStartPoint = Vector3.zero;
        public Vector3 addIntersEndPoint = Vector3.zero;
        List<Vector3> gaps = new List<Vector3>(); // stores the location of gap start & ends: {start0, end0, start1, end1} etc.
        [Tooltip(AFBTooltipsText.allowGaps)]
        public bool allowGaps = true, showDebugGapLine = true; // draws a blue line to fill gaps, only in Editor

        public int defaultPoolSize = 30;

        public GameObject currentFencesFolder, postsFolder, railsFolder, subpostsFolder, extrasFolder;
        public List<GameObject> folderList = new List<GameObject>();

        public int railsATotalTriCount = 0, railsBTotalTriCount = 0, postsTotalTriCount = 0, subPostsTotalTriCount = 0, extrasTotalTriCount = 0;


        public List<Transform> nodeMarkersPool = new List<Transform>(); // the yellow spheres that denote moveable clickpoint node positions
        private List<Vector3> interPostPositions = new List<Vector3>(); // temp for calculating linear interps
        public List<Vector3> clickPoints = new List<Vector3>(); // the points the user clicked, pure.
        public List<int> clickPointFlags = new List<int>(); //to hold potential extra info about the click points. Not used in v1.1 and below
        public List<Vector3> keyPoints = new List<Vector3>(); // the clickPoints + some added primary curve-fitting points

        public List<Vector3> handles = new List<Vector3>(); // the positions of the transform handles for dragging clickNodes

        public int categoryIndex = 0;


        //===== Fence height ======
        [Range(0.2f, 10.0f)]
        public float globalHeight = 1f, globalWidth = 1f;
        public bool keepGlobalScaleHeightWidthLinked = true;


        public bool showGlobals = false;


        //===== Interpolate =========
        [Tooltip(AFBTooltipsText.subsGroundBurial)]
        public bool interpolate = true;

        //[Range(0.25f, 30.0f)]

        public const float minInterPostDist = 0.25f;
        /// <summary>
        /// Specifies the distance between posts, which governs the length of each section
        /// <para>Must be within the range specified by LogRange, defaults to (.3, 10, 60)</para>
        /// </summary>

        [LogRange(minInterPostDist, 10, 60)]
        public float interPostDist = 3.0f;
        public float baseInterPostDistance = 3.0f;

        /// <summary>
        /// If true, keeps interpolated posts grounded, else they will be positioned linearly between the start and end heights, 
        /// which can be useful when bridging gaps or creating ramps
        /// </summary>
        public bool keepInterpolatedPostsGrounded = true;
        //===== Snapping =========
        public bool snapMainPosts = false;
        public float snapSize = 1.0f;

        //===== Smoothing =========
        public bool smooth = false;
        [Range(0.0f, 0.5f)]
        public float tension = 0.0f;
        [Range(0.5f, 10)]
        public float roundingDistance = 1.5f;
        [Range(0, 45)]
        public float removeIfLessThanAngle = 7f;
        [Range(0.2f, 10)]
        public float stripTooClose = 0.75f;

        public bool closeLoop = false;
        [Range(0.0f, 0.5f)]
        //public float randomPostHeight = 0.1f;
        Vector3 preCloseEndPost;
        public bool showControls = false, showHelp = true, showDebugInfo = true;

        public int currPresetIndex = 0;
        public string currPresetName = "";
        public Vector3 lastDeletedPoint = Vector3.zero;
        public int lastDeletedIndex = 0;

        public SlopeMode[] slopeMode = { SlopeMode.shear, SlopeMode.shear };
        public int clearAllFencesWarning = 0;


        //public bool weld = true;
        public LayerMask groundLayers;
        public int ignoreControlNodesLayerNum = 8;
        //-- These will be used as defaults if there is a problem with a given Prefab. markerPost is also used as a marker during Build-clicling in SceneView
        public GameObject markerPost, fallbackPost, fallbackRail, fallbackExtra, nodeMarkerObj;

        //---------- Cloning & Copying ----------
        public GameObject fenceToCopyFrom = null;
        FenceCloner fenceCloner = null;
        [Tooltip(AFBTooltipsText.globalLift)]
        [Range(-2.0f, 30.0f)]
        public float globalLift = 0.0f; //-- This lifts the whole fence off the ground. Used for stacking different fences, should be 0 for normal use

        public bool addCombineScripts = false;
        public bool usingStaticBatching = false;
        public int batchingMode = 1; //0=unity static batching, 1=use a combine script, 2 = none

        List<GameObject> tempMarkers = new List<GameObject>();

        Vector3 newPivotPoint = Vector3.zero;
        List<Vector3> overlapPostErrors = new List<Vector3>();
        public List<float> userSubMeshRailOffsets = new List<float>(); //-- if the user'stackIdx custom singleVarGO contains submeshes, these are their offsets

        public Vector3 autoRotationResults = Vector3.zero;

        public bool needsReloading = true;
        public bool initialReset = false;
        public Transform finishedFoldersParent = null; // optionally an object that all Finished fences will be parented to.
        public bool listsAndArraysCreated = false;
        public bool launchPresetAssigned = false;
        GameObject guideFirstPostMarker = null;
        public bool switchControlsAlso = false;

        public int optimiseRandomiseToolbarValueA = 0, optimiseRandomiseToolbarValueB = 0;
        //bool useMeshRotations = false;
        public bool addLODGroup = false;
        public bool showPreviewLines = true, showBuldHints = true;
        public List<Vector3> previewPoints = new List<Vector3>() { Vector3.zero, Vector3.zero };
        public string prefabsDefaultDir = "Assets/Auto Fence Builder/AFWB_Prefabs";
        public string userAssetsDefaultDir = "Assets/Auto Fence Builder/UserAssets_AFWB";
        public string extrasDefaultDir = "Assets/Auto Fence Builder/AFWB_Prefabs/_Extras_AFWB";//Initial default locations of prefab folders
        public string postsDefaultDir = "Assets/Auto Fence Builder/AFWB_Prefabs/_Posts_AFWB";
        public string railsDefaultDir = "Assets/Auto Fence Builder/AFWB_Prefabs/_Rails_AFWB";
        public string meshesDefaultDir = "Assets/Auto Fence Builder/AFWB_Meshes";
        public string presetsDefaultFilePath = "Assets/Auto Fence Builder/AFWB_Presets";
        public string autoFenceBuilderDefaultDir = "Assets/Auto Fence Builder";
        public string currPrefabsDir, currExtraPrefabsDir, currPostPrefabsDir, currRailPrefabsDir, currMeshesDir, currPresetsDir;
        public string currAutoFenceBuilderDir, currTexturesDir, currMaterialsDir;
        public string scrPresetSaveName = "New Fence Preset_001";

        public bool autoScaleImports = true, autoRotateImports = true;
        public bool addScalingToSizeYAfterUserObjectImport = true;//???
        public bool allowContentFreeUse = false, usingMinimalVersion = false;
        /// <summary>
        /// Current globals toolbar row 1 index.
        /// </summary>
        /// <summary>
        /// Current globals toolbar row 2 index.
        /// </summary>
        public int currGlobalsToolbarRow1 = 1, currGlobalsToolbarRow2 = -1;

        public int infoStyleSmallSize = 10, greyStyleSize = 11;
        public Color infoStyleSmallColor = new Color(0, .3f, .6f); //Dark Cyan
        /// <summary>The color used for grey style.</summary>
        public Color greyStyleColor = Color.grey;


        public bool afwbActive = false;
        public Mesh markerPostMesh;
        public Mesh markerPostMeshLarge; //used in preview lines drawing
        public Vector3 currMousePosition; //this is constantly updated from editor.OnSceneGUI.
        public bool allFollowPostRaiseLower = false; // determines of rails/subpostsPool etc are raised/lowered too
        public bool globalLevelling = false; //Keeps the whole construction at the same height as the first post
        [Range(-2.0f, 10.0f)]
        public float globalLevellingOffset = 1.0f;
        public bool deleteRailAssets = true, deletePostAssets = true, deleteExtraAssets = true, keepFavorites = true;
        public bool scaleMeshOnImport = true; // for user objects
        public bool finishMerged = false, finishDuplicate = false;
        public bool createMergedRailACollider = true, createMergedRailBCollider = false, createMergedPostsCollider = false;
        public bool createMergedSubpostsCollider = false, createMergedExtrasCollider = false;
        public string ignoreRaycastsLayerName = "", useGapLayerName = "";

        public int ignoreRaycastsLayerIndex;
        public bool mouseHoveringOverIgnoreLayer = false;
        public bool obeyUnityIgnoreRaycastLayer = false;
        public bool limitDisplayedSteps = true; //limit the number of seq variation steps to the number of sections built
        public Texture2D keepRowLevelImage;
        [ImageTooltipAttribute("Assets/Auto Fence Builder/Editor/Images/KeepRowLevelOn.jpg")]
        public string myString;
        public bool showSceneDebugInfoPanel = false, showSceneFenceLabels = false, showPostPositions = false, showNodeDistances = false, showSceneStepNums = false;

        public bool enable = true;
        public AutoFenceCreator af;
        //public float heightOffset = 0;
        public float scale = 1;
        float directionVectorLength = 2.0f;
        public Color color = Color.grey;
        public string shapeStr = "0 = SphereTCT, 1 = Post";
        public int shape = 0;
        [HideInInspector]
        public List<Vector3> avgForward = new List<Vector3>();
        public List<Vector3> avgRight = new List<Vector3>();
        //-- Includes all mesh scaling and transforms Rail A    Rail B      Post         Subpost       Extra
        public Vector3[] prefabMeshWithTransformsSize = { Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one };

        //public PostVectorList postDirectionVectors = null;

        public float buildTime = 0, railBuildTime = 0, postBuildTime = 0, subpostBuildTime = 0, extraBuildTime = 0;
        public bool showStackTrace = true;
        public bool useDB = false, utilsDebugObjectAvailable = false;
        public List<Transform> highlightedParts = new List<Transform>();

        public bool showPostVizMarkers = true, showExtraMarkers = true, showExtraPolyrects = true, showNodeDirectionVectors = true;
        public bool showPostDirectionVectors = true, showPostDirectionVectorsRight = true, showAvgRight = true, showRects = true;
        public bool showNodeDirectionVectorsRight = true, showDirectionVectorsPerp = true, showAvgForward = true;
        public int postsBuiltCount = 0; //These are the actual number built in the Scene, which is different to the number in the pool (postsPool.Count)
        public int railABuiltCount = 0; //Number built in the scene (different to pool count), but this might be much higher than postsBuiltCount if there are stacked rails
        public int railBBuiltCount = 0;
        public int subpostsBuiltCount = 0;
        public int subJoinersBuiltCount = 0;
        public bool showOverlapZones = false, showFillZones = false, showOtherMarkers = false;
        public bool countTriangles = false;

        //-------------------------------------
        public int NumPostVariantsInUse
        {
            get
            {
                return _numPostVariantsInUse;
            }
            set
            {
                Debug.LogWarning("NumPostVariantsInUse changed");
                if (_numPostVariantsInUse != value)
                {
                    Debug.LogWarning($"NumPostVariantsInUse changed from {_numPostVariantsInUse} to {value}");
                    _numPostVariantsInUse = value;
                }
            }
        }

        //=====================================
        //          Colliders
        //=====================================
        [Tooltip(AFBTooltipsText.addColliders)]
        public ColliderType postColliderMode = ColliderType.boxCollider; //0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
        public ColliderType railAColliderMode = ColliderType.boxCollider, railBColliderMode = ColliderType.boxCollider; //0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
        public ColliderType extraColliderMode = ColliderType.noCollider, subpostColliderMode = ColliderType.noCollider;//0 = single box, 1 = keep original (user), 2 = no colliders,  3 = meshCollider
        public bool stackedRailsAreTriggerColliders = true; // all rails in stack except first one are triggers

        public float railABoxColliderHeightScale = 1.0f, railBBoxColliderHeightScale = 1.0f, postBoxColliderHeightScale = 1.0f;
        public float extraBoxColliderHeightScale = 1.0f, subpostBoxColliderHeightScale = 1.0f; // customizeable BoxColliders
        public float railABoxColliderHeightOffset = 0.0f, railBBoxColliderHeightOffset = 0.0f, postBoxColliderHeightOffset = 0.0f;
        public float extraBoxColliderHeightOffset = 0.0f, subpostBoxColliderHeightOffset = 0.0f;

        //=====================================
        //          Sequencer
        //=====================================
        //public int[] seqNumSteps = { 2, 2, 2, 1, 1 }; // RailA, RailB, Post, Subpost, Extra. Subpost & Extra not currently used in seq, 1 ensures no error


        //=====================================================
        //                  Posts
        //=====================================================
        public List<GameObject> postPrefabs = new List<GameObject>(); // the prefabs from the Assets folder
        public List<Transform> postsPool = new List<Transform>();
        public List<Transform> postsAndSubpostsCombined = new List<Transform>(); //Used by Extras
        public List<Vector3> allPostPositions = new List<Vector3>();
        public List<Vector3> allPostsPositionsUnrandomized = new List<Vector3>(); //the 'true' positions before random spacing is added
        public List<List<Mesh>> origPostPrefabMeshes = new List<List<Mesh>>();

        public int rsPostSpacing = 0; //-- rsRailARand=1;
        public int quantizeRotAxisPost = 1;
        public int currentPost_PrefabMenuIndex = 1;


        //A list of strings with a one-to-one correspondence to the main Prefab Lists
        public List<string> postMenuNames = new List<string>();
        public bool usePostsLayer = true;
        public GameObject userPrefabPost = null, oldUserPostObject = null;
        public Vector3 postScale = Vector3.one;
        //[Tooltip(AFBTooltipsText.mainPostsSizeBoost)]
        public Vector3 mainPostsSizeBoost = Vector3.one; // Boosts the svSize of the main (user click-point) postsPool, not the interpolated postsPool. Good for extra variation
        public Vector3 endPostsSizeBoost = Vector3.one;
        [Range(-1.0f, 4.0f)]
        public float postHeightOffset = 0;
        public Vector3 postRotation = Vector3.zero;
        [Tooltip(AFBTooltipsText.lerpPostRotationAtCorners)]
        public bool lerpPostRotationAtCorners = true, lerpPostRotationAtCornersInters = true; // should we rotate the corner postsPool so they are the average direction of the rails.
        public bool hideInterpolated = false;
        public Vector3 nativePostScale = Vector3.one;
        public bool adaptPostToSurfaceDirection = false, adaptSubpostToSurfaceDirection = false;
        [Range(0.0f, 1.0f)]
        public float postSurfaceNormalAmount = 0.0f, subpostSurfaceNormalAmount = 0.0f;
        public Vector3 postUserMeshBakeRotations = Vector3.zero;
        public int postBakeRotationMode = 1; // 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
        public int quantizeRotIndexPost = 7;
        public bool stretchPostWidthAtMitreJoint = false;

        public int currentPost_PrefabIndex = 1, currentPostMenuIndex = 0;
        public int mirrorXFreqPost = 5, mirrorYFreqPost = 3, mirrorZFreqPost = 7;

        //============================ Post Variations =================================
        [Range(0.0f, 1.0f)]
        public float postSpacingVariation = 0.05f;
        public float actualInterPostDistance = 3.0f; // This is the final interpost distanceToNextPost (closest to user'stackIdx request) that gives while number of sections 
        public bool allowRotations_Y_Post = false, allowVertical180Invert_Post = false;
        public bool allowMirroring_X_Post = false, allowMirroring_Y_Post = false, allowMirroring_Z_Post = false, jitterPostVerts = false;
        public Vector3 jitterAmountPost = new Vector3(0.03f, 0.03f, 0.03f);
        public int variationRotationQuantize_Y_Post = 0;
        [Range(0.0f, 1.0f)]
        public float mirrorXPostProbability = 0, mirrorZPostProbability = 0;
        public GameObject postVariation1, postVariation2, postVariation3; //Only used for the editor boxes placeholders

        List<GameObject> nonNullPostGOs = new List<GameObject>(new GameObject[] { null, null, null, null });
        public bool usePostVariations = false;
        public VariationMode variationModePost = VariationMode.sequenced;

        //public List<SourceVariant> nonNullPostVariants = new List<SourceVariant>();
        public Vector3[] varPostPositionOffset = new Vector3[kMaxNumSourceVariants];
        public Vector3[] varPostSize = new Vector3[kMaxNumSourceVariants];
        public Vector3[] varPostRotation = new Vector3[kMaxNumSourceVariants];
        public float[] varPostProbs = new float[kMaxNumSourceVariants];
        public List<SeqItem> optimalSequencePost = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);

        //public bool[] seqPostX = new bool[kMaxNumSeqSteps], seqPostZ = new bool[kMaxNumSeqSteps], seqPostInvert180 = new bool[kMaxNumSeqSteps]; // the sequence orientations
        //public Vector3[] seqPostSize = new Vector3[kMaxNumSeqSteps], seqPostOffset = new Vector3[kMaxNumSeqSteps], seqPostRotate = new Vector3[kMaxNumSeqSteps];

        public int[] seqPostSourceVarIndex = new int[kMaxNumSeqSteps];//
        public bool[] usePostVar = new bool[kMaxNumSourceVariants];
        public bool[] seqPostStepEnabled = new bool[kMaxNumSeqSteps];

        public List<SourceVariant> postSourceVariants = new List<SourceVariant>(new SourceVariant[kMaxNumSourceVariants]);
        public int _numPostVariantsInUse = 2;
        public bool showPostVariations = false;
        public List<int> postSourceVariant_PrefabIndices = new List<int>(new int[kMaxNumSourceVariants]);
        public List<int> postSourceVariant_MenuIndices = new List<int>(new int[kMaxNumSourceVariants]);

        public bool allowNodePostsPrefabOverride = true, allowEndPostsPrefabOverride = true;

        public int nodePostsOverridePrefabIndex = 30, nodePostsOverrideMenuIndex = 31; // the prefab index of the override post

        public int endPostsOverridePrefabIndex = 40, endPostsOverrideMenuIndex = 41;//the menu index of the override post dropdown
        //===================   Post & Subpost Randomization   ======================
        public float minPostHeightLimit = 0.25f, maxPostHeightLimit = 2f, minSubpostHeightLimit = 0.5f, maxSubpostHeightLimit = 1.5f;
        public float minRandHeightPost = 0.96f, maxRandHeightPost = 1.04f, minRandHeightSubpost = 0.96f, maxRandHeightSubpost = 1.04f;
        public float chanceOfMissingPost = 0, chanceOfMissingSubpost = 0;
        public float minShiftXZPost = -0.1f, maxShiftXZPost = 0.1f, minPostXZShiftLimit = -1.5f, maxPostXZShiftLimit = 1.5f;
        public float minSizeXZPost = 0.8f, maxSizeXZPost = 1.2f, minPostSizeLimit = 0.25f, maxPostSizeLimit = 2f;
        [Range(0.0f, 1.0f)]
        public float quantizeRotAnglePost = 90, quantizeRotAngleSubpost = 90;
        [Range(0.0f, 1.0f)]
        public float quantizeRotProbPost = 0.1f, quantizeRotProbSubpost = 0;
        public RandomScope postRandomScope = RandomScope.all, subpostRandomScope = RandomScope.all;
        public Vector3 smallRotationAmountPost = new Vector3(0.03f, 0.03f, 0.03f), smallRotationAmountSubpost = new Vector3(0.03f, 0.03f, 0.03f);

        public bool allowRandPostSmallRotationVariation = false, allowRandSubpostSmallRotationVariation = false, allowHeightVariationPost = false, allowHeightVariationSubpost = false;
        public bool allowQuantizedRandomRailARotation = false, allowQuantizedRandomRailBRotation = false, allowQuantizedRandomPostRotation = false, allowQuantizedRandomSubpostRotation = false;
        public bool allowPostSizeVariation = false, allowPostXZShift = false; //added 31/12/21
        public bool allowPostRandomization = true;

        // Sequencer
        public bool showPostSequencer = false;

        public Sequencer postSequencer = new Sequencer(LayerSet.postLayerSet);

        /*[SerializeField] public List<SeqItem> _SeqItemListPost = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);
        public List<SeqItem> SeqItemListPost
        {
            get { return _SeqItemListPost; }
            set
            {
                //Debug.LogWarning("_SeqItemListPost changed");
                if (_SeqItemListPost != value)
                {
                    //Debug.LogWarning($"_SeqItemListPost changed from {_SeqItemListPost} to {value}");
                    _SeqItemListPost = value;
                }
            }
        }*/


        //===================== Singles ========================
        public bool usePostSingles = false, showPostSinglesEditor = false;


        //=====================================================
        //                  Rails  A & B
        //=====================================================
        public List<string> railMenuNames = new List<string>();
        public List<Transform> railsAPool = new List<Transform>();
        public List<Transform> railsBPool = new List<Transform>();
        public List<GameObject> railPrefabs = new List<GameObject>();

        public List<List<Mesh>> origRailPrefabMeshes = new List<List<Mesh>>();
        public int quantizeRotAxisRailA = 1, quantizeRotAxisRailB = 1;
        [Range(0.0f, 1.0f)]
        public float quantizeRotAngleRailA = 90, quantizeRotAngleRailB = 90;
        public float quantizeRotProbRailA = 0, quantizeRotProbRailB = 0;

        //-- Have to use two seoerate Lists for A and B as Unity doesn't serialize multidimensional arrays and the workaround is a pain
        public List<int> railASourceVariant_MenuIndices = new List<int>(new int[kMaxNumSourceVariants]);
        public List<int> railBSourceVariant_MenuIndices = new List<int>(new int[kMaxNumSourceVariants]);

        public int railSetToolbarChoice = 0; // 0 = Rails currPost, 1 = Rails nextPost
        public RandomScope railARandomScope = RandomScope.all, railBRandomScope = RandomScope.all;
        public int[] currentRail_PrefabIndex = { 1, 1 }; // RailA, RailB
        public int[] currentRail_PrefabMenuIndex = { 0, 0 }; // RailA, RailB
        public int[] railSpreadMode = { 0, 0 }; // 0: DistanceTCT is the total spread, 1: distanceToNextPost per singleVarGO
        [Range(1, 12)]
        public float[] numStackedRails = { 1, 1 }; //float because slider value box isn't automatically typable with int

        public bool rotateFromBaseRailA = false, rotateFromBaseRailB = false;
        public bool useCustomPost = false, useCustomExtra = false;
        public bool centralizeRails = false;
        public bool overlapAtCorners = true;
        public bool autoHideBuriedRails = true;
        public bool rotateY = false;// used in repetition disguise variations
        public bool railAAdaptToSurface = false, railBAdaptToSurface = false;
        public bool[] useCustomRail = { false, false };
        public bool[] keepRailGrounded = { true, true };
        public bool[] useRailLayer = { true, true };
        public bool[] extendRailEnds = { false, false };
        public float[] endExtensionLength = { 0.2f, 0.2f };

        public float minGap = 0.1f, maxGap = 1.5f;
        public float railASurfaceNormalAmount = 1.0f, railBSurfaceNormalAmount = 1.0f;
        [Range(0.02f, 20.0f)]
        public float[] railSpread = { 1.5f, 1.5f };
        [Range(0.0f, 1.0f)]
        public float mirrorXRailProbability = 0, mirrorZRailProbability = 0, verticalInvertRailProbability = 0;
        [Range(-1.0f, 1.0f)]
        public float chanceOfMissingRailA = 0.0f;
        [Range(-1.0f, 1.0f)]
        public float chanceOfMissingRailB = 0.0f;

        public string userBackupPathPost, userBackupPathExtra;
        public string[] userBackupPathRail = { "", "" };

        //-- AFWB prefabs always have their scale tranforms at Vector3.one, custom user prefabs might not
        public Vector3 nativeRailAScale = Vector3.one, nativeRailBScale = Vector3.one;
        public Vector3 railAPositionOffset = Vector3.zero, railBPositionOffset = Vector3.zero; //the user settings
        public Vector3 railAScale = Vector3.one, railBScale = Vector3.one;//the user settings
        public Vector3 railARotation = Vector3.zero, railBRotation = Vector3.zero;//the user settings

        public GameObject[] userPrefabRail = { null, null };

        [Tooltip(AFBTooltipsText.overlapAtCorners)]
        public JointStyle[] railJointStyle = { JointStyle.overlap, JointStyle.overlap };
        [Tooltip(AFBTooltipsText.rotateY)]

        public List<SourceVariant>[] railSourceVariants = { new List<SourceVariant>(new SourceVariant[kMaxNumSourceVariants]),
            new List<SourceVariant>(new SourceVariant[kMaxNumSourceVariants]) };

        public bool[] showRailVariations = { false, false };
        public bool[] useRailVariations = { false, false };

        public int[] numRailVariantsInUse = { 2, 2 }; // 2 for RailA and 2 for RailB. This is JUST the extra variations, add 1 to include Main

        public List<List<Mesh>> railAPreparedMeshVariants = new List<List<Mesh>>(new List<Mesh>[kMaxNumSourceVariants]); //prepared mesh for each variations
        public List<List<Mesh>> railBPreparedMeshVariants = new List<List<Mesh>>(new List<Mesh>[kMaxNumSourceVariants]);
        //public List<SourceVariant>[] nonNullRailSourceVariants = { new List<SourceVariant>(), new List<SourceVariant>() };
        public bool[] useRailVarA = new bool[kMaxNumSourceVariants], useRailVarB = new bool[kMaxNumSourceVariants];
        public Vector3[] varRailAPositionOffset = new Vector3[kMaxNumSourceVariants], varRailBPositionOffset = new Vector3[kMaxNumSourceVariants];
        public Vector3[] varRailASize = new Vector3[kMaxNumSourceVariants], varRailBSize = new Vector3[kMaxNumSourceVariants];
        public Vector3[] varRailARotation = new Vector3[kMaxNumSourceVariants], varRailBRotation = new Vector3[kMaxNumSourceVariants];
        [Range(0.01f, 1.0f)]
        public float[] varRailAProbs = new float[kMaxNumSourceVariants], varRailBProbs = new float[kMaxNumSourceVariants];
        [Range(0.0f, 1.0f)]

        public bool[] varRailABackToFront = new bool[kMaxNumSourceVariants], varRailAMirrorZ = new bool[kMaxNumSourceVariants],
            varRailAInvert = new bool[kMaxNumSourceVariants];
        [Range(0.0f, 1.0f)]
        public bool[] varRailBBackToFront = new bool[kMaxNumSourceVariants], varRailBMirrorZ = new bool[kMaxNumSourceVariants],
            varRailBInvert = new bool[kMaxNumSourceVariants];

        public bool allowIndependentSubmeshVariationA = true, allowIndependentSubmeshVariationB = true;
        public bool scaleVariationHeightToMainHeightA = true, scaleVariationHeightToMainHeightB = true; //if height of variation object differs, then match main
        public VariationMode variationModeRailA = VariationMode.sequenced, variationModeRailB = VariationMode.sequenced;

        public Vector3 railUserMeshBakeRotations = Vector3.zero;
        public int railBakeRotationMode = 1, extraBakeRotationMode = 1; // 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
        public bool autoHideRailAVar = true, autoHideRailBVar = true;
        public int quantizeRotIndexRailA = 7, quantizeRotIndexRailB = 7;

        //=====================================
        //        Rail Randomization
        //=====================================
        public bool allowRandRailARotationVariation = true, allowRandRailBRotationVariation = true;
        public bool[] allowRailRandomization = { false, false };
        public bool[] allowMirroring_X_Rail = { false, false }, allowMirroring_Y_Rail = { false, false }, allowMirroring_Z_Rail = { false, false };
        public int[] mirrorXFreqRail = { 2, 2 }, mirrorYFreqRail = { 3, 3 }, mirrorZFreqRail = { 5, 5 };
        public bool[] allowVertical180Invert_Rail = { false, false };
        public bool allowHeightVariationRailA = false, allowHeightVariationRailB = false;
        public bool jitterRailAVerts = false, jitterRailBVerts = false;
        public bool[] varRailABackToFrontBools = new bool[kMaxNumSourceVariants], varRailAMirrorZBools = new bool[kMaxNumSourceVariants],
                varRailAInvertBools = new bool[kMaxNumSourceVariants];
        public bool[] varRailBBackToFrontBools = new bool[kMaxNumSourceVariants], varRailBMirrorZBools = new bool[kMaxNumSourceVariants],
            varRailBInvertBools = new bool[kMaxNumSourceVariants];

        public float minRailHeightLimit = 0.5f, maxRailHeightLimit = 1.5f;
        /// <summary> These are the current ranges set bu user with slider </summary>
        public float minRandHeightRailA = 0.97f, maxRandHeightRailA = 1.03f, minRandHeightRailB = 0.9f, maxRandHeightRailB = 1.1f;

        public Vector3 jitterAmountRail = new Vector3(0.03f, 0.03f, 0.03f);
        public Vector3 smallRotationAmountRailA = new Vector3(0.03f, 0.03f, 0.03f), smallRotationAmountRailB = new Vector3(0.03f, 0.03f, 0.03f);
        public int[] shuffledRailAIndices = new int[1], shuffledRailBIndices = new int[1];
        public Vector3[] seqRailASize = new Vector3[kMaxNumSeqSteps], seqRailAOffset = new Vector3[kMaxNumSeqSteps], seqRailARotate = new Vector3[kMaxNumSeqSteps];
        public int[] seqRailASourceVarIndex = new int[kMaxNumSeqSteps];
        public bool[] seqRailAStepEnabled = new bool[kMaxNumSeqSteps], seqRailBStepEnabled = new bool[kMaxNumSeqSteps];
        public Vector3[] seqRailBSize = new Vector3[kMaxNumSeqSteps], seqRailBOffset = new Vector3[kMaxNumSeqSteps], seqRailBRotate = new Vector3[kMaxNumSeqSteps];
        public int[] seqRailBSourceVarIndex = new int[kMaxNumSeqSteps];

        //public RandomRecords railARandRec, railBRandRec, postRandRec;
        public bool allowBackToFrontRailA = true, allowMirrorZRailA = true, allowInvertRailA = true;
        public bool allowBackToFrontRailB = true, allowMirrorZRailB = true, allowInvertRailB = true;
        /// <summary>Shows instant comments in the lower right of the screen view. New Comments overwrite the old one </summary>
        public bool showLogComments;
        /// <summary>the instant temporary comment to show in the lower right of the screen view </summary>
        public string logComment = "";
        public int postImportScaleMode = 0, railAImportScaleMode = 0, railBImportScaleMode = 0, extraImportScaleMode = 0;
        public Vector3 postCurrBakeRot = Vector3.zero, railACurrBakeRot = Vector3.zero, railBCurrBakeRot = Vector3.zero, extraCurrBakeRot = Vector3.zero;


        //==================   Sequencer   =====================
        public Sequencer railASequencer = new Sequencer(LayerSet.railALayerSet);
        public Sequencer railBSequencer = new Sequencer(LayerSet.railBLayerSet);

        public bool[] showRailSequencer = { false, false };
        /*[SerializeField] public List<SeqItem> _SeqItemListRailA = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);
        public List<SeqItem> SeqItemListRailA
        {
            get { return _SeqItemListRailA; }
            set
            {
                Debug.LogWarning("_SeqItemListRailA changed");
                if (_SeqItemListRailA != value)
                {
                    Debug.LogWarning($"_SeqItemListRailA changed from {_SeqItemListRailA} to {value}");
                    _SeqItemListRailA = value;
                }
            }
        }*/
        /*[SerializeField] public List<SeqItem> _SeqItemListRailB = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);
        public List<SeqItem> SeqItemListRailB
        {
            get { return _SeqItemListRailB; }
            set
            {
                //Debug.LogWarning("_SeqItemListRailB changed");
                if (_SeqItemListRailB != value)
                {
                    //Debug.LogWarning($"_SeqItemListRailB changed from {_SeqItemListRailB} to {value}");
                    _SeqItemListRailB = value;
                }
            }
        }*/
        public List<SeqItem> optimalSequenceRailA = new List<SeqItem>(new SeqItem[16]);
        public List<SeqItem> optimalSequenceRailB = new List<SeqItem>(new SeqItem[16]);

        //===================== Singles ========================
        public bool[] useRailSingles = { false, false }, showRailSinglesEditor = { false, false };


        //=====================================================
        //                  Extras
        //=====================================================
        public List<GameObject> extraPrefabs = new List<GameObject>();
        public ExtrasAFWB ex;
        public bool useExtrasLayer = true;
        public GameObject userPrefabExtra = null, oldExtraGameObject = null;
        public Vector3 nativeExtraScale = Vector3.one;
        //public int extraMenuIndex = 0;
        public List<GameObject> extraDisplayVariationGOs = new List<GameObject>(new GameObject[kMaxNumSourceVariants]);
        public int currentExtra_PrefabIndex = 1, currentExtra_PrefabMenuIndex = 0;
        public float minRandHeightExtra, maxRandHeightExtra;

        //=====================================================
        //                  Singles
        //=====================================================
        public SinglesContainer[] railSinglesContainer = {null, null};
        public SinglesContainer postSinglesContainer = null;
        public SinglesContainer singlesContainer = null; // this is an empty container, for convenience in calling static-like functions using af
        public bool[] railSinglesEnabled = { true, true };
        public bool postSinglesEnabled = true;
        public const int kSingleSkipIndex = 9999; // denotes a 'single' replacement should be skipped (similar to a gap)
        public List<string> extraMenuNames = new List<string>();

        //=====================================================
        //                  Subposts
        //=====================================================

        public List<Transform> subpostsPool = new List<Transform>();
        private List<Transform> subJoiners = new List<Transform>();
        public List<GameObject> subPrefabs = new List<GameObject>();
        public List<GameObject> subJoinerPrefabs = new List<GameObject>();
        public List<string> subMenuNames = new List<string>();
        public int subJoinersBuiltSoFarCount = 0;
        //public int currentSubpost_PrefabIndex = 1, currentSubpost_PrefabMenuIndex = 0;
        public int currentSubJoinerType = 0;
        //public int currentSubpost_PrefabMenuIndex = 1;
        public int currentSubpost_PrefabIndex = 1, currentSubpost_PrefabMenuIndex = 0;
        public bool useSubpostsLayer = false;
        public int numSubpostsPerSection = 1;
        [Range(0.1f, 20)]
        public float subSpacing = 0.5f;
        [Range(-2.0f, 2.0f)]
        public float subPostSpread = 1;
        public Vector3 subpostPositionOffset = Vector3.zero;
        public Vector3 subpostScale = Vector3.one;
        public Vector3 subpostRotation = Vector3.zero;
        public bool forceSubsToGroundContour = false, keepSubsAboveGround = true;
        public bool addSubpostAtPostPointAlso = false;
        [Tooltip(AFBTooltipsText.subsGroundBurial)]
        [Range(-2.0f, 0.0f)]
        public float subsGroundBurial = 0.0f;
        //List<Material> originalSubMaterials = new List<Material>();
        [Range(0.0f, 1.0f)]
        public Vector3 nativeSubScale = Vector3.one;

        public bool useSubWave = false;
        [Range(0.01f, 10.0f)]
        public float subWaveFreq = 1;
        [Range(0.0f, 2.0f)]
        public float subWaveAmp = 0.25f;
        [Range(-Mathf.PI * 4, Mathf.PI * 4)]
        public float subWavePosition = Mathf.PI / 2;
        public bool useSubJoiners = false;
        public bool useBinaryFBX = true; // The export format, hacked FBXExpoter

        //public List<SourceVariant> nonNullSubpostVariants = new List<SourceVariant>();
        public bool[] seqSubpostStepEnabled = new bool[kMaxNumSeqSteps];
        public bool[] seqSubpostX = new bool[kMaxNumSeqSteps], seqSubpostZ = new bool[kMaxNumSeqSteps], seqSubpostInvert180 = new bool[kMaxNumSeqSteps]; // the sequence orientations
        public int[] seqSubpostSourceVarIndex = new int[kMaxNumSeqSteps];
        public Vector3[] varSubpostPositionOffset = new Vector3[kMaxNumSourceVariants];
        public Vector3[] varSubpostSize = new Vector3[kMaxNumSourceVariants];
        public Vector3[] varSubpostRotation = new Vector3[kMaxNumSourceVariants];
        public Vector3[] seqSubpostSize = new Vector3[kMaxNumSeqSteps], seqSubpostOffset = new Vector3[kMaxNumSeqSteps], seqSubpostRotate = new Vector3[kMaxNumSeqSteps];
        public bool[] useSubpostVar = new bool[kMaxNumSourceVariants];
        public List<int> varPrefabIndexSubpost = new List<int>(new int[kMaxNumSourceVariants]);
        public List<int> varMenuIndexSubpost = new List<int>(new int[kMaxNumSourceVariants]);
        public bool useSubpostVariations = true;
        public bool useSubpostSeq = false;
        public List<SourceVariant> subpostSourceVariants = new List<SourceVariant>(new SourceVariant[kMaxNumSourceVariants]);
        public List<SeqItem> optimalSequenceSubpost = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);
        public List<SeqItem> userSequenceSubpost = new List<SeqItem>(new SeqItem[kMaxNumSeqSteps]);
        public bool allowSubpostRandomization = true;
        public int quantizeRotIndexSubpost = 7;
        public SourceVariant sourceVariant;// only to link observers
        public int defaultPostPrefabIndex = 27, defaultRailPrefabIndex = 27, defaultExtraPrefabIndex = 27;
        //This keeps track of the sequential order of posts and subposts
        //Useful when needing to consider them sequentially, e.g. when placing extras at their positions. List is build in BuildSubposts()
        public List<string> postAndSubpostStringList = new List<string>();
        public int quantizeRotAxisSubpost = 1;
        public DuplicateMode subpostDuplicateMode = DuplicateMode.single;
        public Vector3 resizedPrefabSize = Vector3.zero;
        public int currPresetMenuIndex = 7, presetMenuIndexInDisplayList = 0; // because the List we're displaying nmight not be the full List
        public Mesh railACustomColliderMesh, railBCustomColliderMesh, postCustomColliderMesh, extraCustomColliderMesh, subpostCustomColliderMesh;
        public RailOffsetMode[] railOffsetMode = { RailOffsetMode.basic, RailOffsetMode.basic };
        public bool showOptionalPostPrefabsProp = false;

        //public GizmoDrawManager gizmoManager = null;

        /// <summary>
        /// The Master List of all postVectors. Each one contains all possible parameters about a post, and its neighbours
        /// </summary>
        public List<PostVector> postVectors = new List<PostVector>();
        public List<UprightMarker> uprightMarkers = new List<UprightMarker>();
        /// <summary>Depending on if the prefab menu has been filtered, this shows the available prefabs </summary>
        public int componentDisplayMenuIndex = 0;// filteredMenuIndex = 0;
        public int presetDisplayMenuIndex = 0;
        public string presetNotes = "This is a preset note";
        public GameObject userPrefabPlaceholder = null; //a dummy prefab called "Drag GameObj From Hrchy" to show in the drag custom box

        //====================================================
        // Prints the name of the Class and the Method that called it
        // There is also a version of this in AutoFenceEditor, it's just convenient to have it here too to avoid extra referencing
        // Add:  StackLog(this.GetType().Name); to any Method you want to include in the log
        public void StackLog(string classType, bool verbose = true, [CallerMemberName] string caller = null)
        {
            bool show = false;
            if (af != null)
                show = af.showStackTrace;
            if (caller == "Awake") //because the first time it's called, af hasn't been initialized yet
                show = true;
            if (show)
            {
                if (verbose == true)
                    Debug.Log($"                         _____   [{classType}]  :  {caller}( )   _____\n");
                else
                    Debug.Log($"{caller}( )\n");
            }
        }
        //-------------------------------------
        private void OnDisable()
        {
            if (sourceVariant != null)
                sourceVariant.Unsubscribe(this);
        }
        void OnEnable()
        {
            // Ensure the parent list is set when the object is enabled
            PostVector.LinkParentList(postVectors);
            // TODO - duplicate some other inits here.
        }
        //==============================
        //          Awake
        //==============================
        void Awake()
        {
            //StackLog(this.GetType().Name);
            //ClearConsole();
            if (ex == null)
            {
                ex = new ExtrasAFWB(this);
                if (ex == null)
                    Debug.LogWarning("new ExtrasAFWB() was null in AutoFenceCreator Awake() \n");
            }
            else
                ex.af = this;
            // Ensure GizmoDrawManager is initialized
            //EditorApplication.delayCall += InitializeGizmoDrawManager;
            //-- This is not a good place to initialise stuff as it's not called during a recompile, or re-selection
        }
        //-------------------------------------
        void Reset()
        {
            //DebugUtilitiesTCT.LogStackTrace();
            //CreateAllObjectsLists();

            ResetDefaultDirectories();
            ResetAutoFence(true);

            List<GameObject> p = postPrefabs;
            List<GameObject> r = railPrefabs;
        }
        //-- Called from SetupEditor()
        public void ResetAutoFence(bool resetFenceParameters = false)
        {
            StackLog(this.GetType().Name);
            keyPoints.Clear();
            postsBuiltCount = railABuiltCount = railBBuiltCount = subpostsBuiltCount = ex.extrasBuiltCount = subJoinersBuiltCount = 0;
            globalScale.y = 1.0f;
            globalLift = 0.0f;
            //InitializeSequencesAndSingles();;//???
            if (resetFenceParameters)
            {
                numStackedRails[kRailALayerInt] = numStackedRails[kRailBLayerInt] = 1;
                railSpread[kRailALayerInt] = 0.5f;
                railAPositionOffset.y = 0.36f;
                subpostScale.y = 0.36f;
                roundingDistance = 6;
                centralizeRails = false;
                slopeMode[kRailALayerInt] = SlopeMode.shear;
                interpolate = true;
                interPostDist = 3;
                autoHideBuriedRails = false;
                useRailLayer[1] = false;
                railABoxColliderHeightScale = 1.0f;
                railABoxColliderHeightOffset = 0.0f;
                ex.extraTransformPositionOffset.y = globalScale.y * postScale.y;  //initialize so the extra is visible at the maxGoTop of the post
            }
            initialReset = true;
        }


        public void CreateAllObjectsLists()
        {
            //-- Prefab Lists
            postPrefabs = new List<GameObject>();
            railPrefabs = new List<GameObject>();
            subJoinerPrefabs = new List<GameObject>();
            extraPrefabs = new List<GameObject>();
            
            //-- Pool Lists
            postsPool = new List<Transform>();
            railsAPool = new List<Transform>();
            railsBPool = new List<Transform>();
            ex.extrasPool = new List<Transform>();
            subpostsPool = new List<Transform>();
        }


        /*private void InitializeGizmoDrawManager()
        {
            if (GizmoDrawManager.gizmoSingletonInstance == null)
            {
                GameObject gizmoManager = GameObject.Find("GizmoDrawManager");
                if (gizmoManager == null)
                {
                    gizmoManager = new GameObject("GizmoDrawManager");
                    gizmoManager.AddComponent<GizmoDrawManager>();
                    Debug.Log("GizmoDrawManager created in the scene from AutoFenceCreator.");
                }
                else
                {
                    Debug.Log("GizmoDrawManager already exists in the scene from AutoFenceCreator.");
                }
            }
        }*/


        //==============================

        /// <summary>Sets up the dependencies for the AutoFenceCreator.</summary>
        /// <param name="rebuildFolders">Indicates whether the folders should be rebuilt. The default value is false.</param>
        public void SetupAutoFenceDependencies(bool rebuildFolders = false)
        {
            //StackLog(this.GetType().Name);

            //Ensure everything is updated when a sourceVariant is changed
            //sourceVariant.Subscribe(this);
            if (postVectors == null)
                postVectors = new List<PostVector>();


            CreateSinglesContainers();

            //TODO
            if (ex == null || ex.extrasPool == null)
                ex = new ExtrasAFWB(this);
            else
                ex.af = this;

            // Set defaults for now, will be checked periodically bt AutoFenceEditor
            currPrefabsDir = prefabsDefaultDir;
            currExtraPrefabsDir = extrasDefaultDir;
            currPostPrefabsDir = postsDefaultDir;
            currRailPrefabsDir = railsDefaultDir;
            currMeshesDir = meshesDefaultDir;

            afwbActive = true;
            GameObject existingFolder = GameObject.Find("Current Fences Folder");
            if (existingFolder != null)
            {
                if (Application.isEditor)
                {
                    if (rebuildFolders)
                    {
                        currentFencesFolder = existingFolder;
                        DestroyImmediate(existingFolder);
                        SetupFolders();
                    }

                    SetClickMarkersActiveStatus(showControls);
                }
                else if (Application.isPlaying)
                {
                    SetClickMarkersActiveStatus(false); //???
                }
            }
            postsBuiltCount = railABuiltCount = railBBuiltCount = subpostsBuiltCount = ex.extrasBuiltCount = subJoinersBuiltCount = 0;

            needsReloading = true;//???
            groundLayers = ~((1 << LayerMask.NameToLayer("IgnoreRaycast")));
            keepRowLevelImage = EditorGUIUtility.Load("Assets/Auto Fence Builder/Editor/Images/KeepRowLevelOn.jpg") as Texture2D;
        }


        /// <summary>
        /// Initializes the containers for single sections of rails and posts. These are individual modification done to a single section of a fence.
        /// They will override any other assignment to that section
        /// Called by AutoFenceCreator.SetupAutoFenceDependencies()
        /// </summary>
        void CreateSinglesContainers()
        {
            railSinglesContainer = new SinglesContainer[2];
            railSinglesContainer[0] = new SinglesContainer(this);
            railSinglesContainer[1] = new SinglesContainer(this);
            postSinglesContainer = new SinglesContainer(this);
            singlesContainer = new SinglesContainer(this); // this is an empty container, for convenience in calling static-like functions using af
        }


        /// <summary>
        /// Sets the GameObject for a source variant at a specific index for a given layer.
        /// </summary>
        /// <param name="sourceVariantIndex">The index of the source variant.</param>
        /// <param name="prefabIndex">The index of the prefab in the main prefab list for this layer.</param>
        /// <param name="layer">The layer for which the source variant GameObject is to be set.</param>
        public void SetSourceVariantGoAtIndexForLayer(int sourceVariantIndex, int prefabIndex, LayerSet layer)
        {
            string layerName = GetLayerNameAsString(layer);
            int prefabCount = GetPrefabsForLayer(layer).Count;

            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);

            GameObject prefab = GetPrefabAtIndexForLayer(prefabIndex, layer);
            if (prefab == null)
            {
                int mainIndex = GetCurrentPrefabIndexForLayer(layer);
                Debug.LogWarning($"SetSourceVariantGoAtIndexForLayer()  {layerName} prefab was null. " +
                    $"Using Main prefab {GetPrefabAtIndexForLayer(mainIndex, layer).name} instead\n");
                return;
            }

            sourceVariants[sourceVariantIndex].Go = prefab;
            //print the name of the prefab
            //debug.Log($"SetSourceVariantGoAtIndexForLayer()  {layerName} sourceVariantIndex: {sourceVariantIndex}  prefabIndex: {prefabIndex}  prefab: {prefab.name}\n");
        }


        //-------------------------------------
        public bool GetShowSequencerForLayer(LayerSet layer)
        {
            if (IsRailLayer(layer))
            {
                if (layer == LayerSet.railALayerSet)
                    return showRailSequencer[kRailALayerInt];
                else if (layer == LayerSet.railBLayerSet)
                    return showRailSequencer[kRailBLayerInt];
            }
            else
                 if (layer == LayerSet.postLayerSet)
                return showPostSequencer;

            return false;
        }
        public void ShowVariationsForLayer(LayerSet layer, bool showVariations)
        {
            if (layer == LayerSet.railALayerSet)
                showRailVariations[0] = showVariations;
            else if (layer == LayerSet.railBLayerSet)
                showRailVariations[1] = showVariations;
            else if (layer == LayerSet.postLayerSet)
                showPostVariations = showVariations;
        }
        //-------------------------------------
        public bool GetUseSequencerForLayer(LayerSet layer)
        {
            Sequencer sequencer = GetSequencerForLayer(layer);
            if (sequencer == null)
                return false;
            bool useSeq = sequencer.GetUseSeq();
            return useSeq;
        }
        //-------------------------------------
        public void SetUseSequencerForLayer(LayerSet layer, bool useSeq)
        {
            Sequencer sequencer = GetSequencerForLayer(layer);
            if (sequencer == null)
            {
                Debug.LogWarning($"SetUseSequencerForLayer()  sequencer was null for {layer.String()} layer\n");
                return;
            };
            sequencer.SetUseSeq(useSeq);
        }
        //--------------------
        public void ToggleUseSequencerForLayer(LayerSet layer)
        {
            Sequencer sequencer = GetSequencerForLayer(layer);
            sequencer.ToggleUseSeq();
        }
        //---------------------------
        public GameObject GetCurrentPrefabForLayer(LayerSet layer)
        {
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            if (prefabs == null)
                return null;

            int index = GetCurrentPrefabIndexForLayer(layer);
            if (index == -1)
                return prefabs[0];
            GameObject prefab = GetPrefabAtIndexForLayer(index, layer);
            return prefab;
        }
        public string GetCurrentPrefabNameForLayer(LayerSet layer)
        {
            string layerPrefabName = $"Null {layer.String()} Prefab";
            GameObject prefab = GetCurrentPrefabForLayer(layer);
            if (prefab != null)
                layerPrefabName = prefab.name;
            return layerPrefabName;
        }
        //------------------
        public GameObject GetMainPostOverridePrefabForLayer(LayerSet layer)
        {
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            if (prefabs == null)
                return null;
            if (nodePostsOverridePrefabIndex == -1)
                return prefabs[0];
            GameObject prefab = GetPrefabAtIndexForLayer(nodePostsOverridePrefabIndex, layer);
            return prefab;
        }
        //----------------------

        public GameObject GetEndPostsOverridePrefabForLayer(LayerSet layer)
        {
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            if (prefabs == null)
                return null;

            if (endPostsOverridePrefabIndex == -1)
                return prefabs[0];

            GameObject prefab = GetPrefabAtIndexForLayer(endPostsOverridePrefabIndex, layer);
            return prefab;
        }
        //--------------------------
        // Ensures all necessary housekeeping is done
        public void DestroyCurrentFencesFolder()
        {
            DestroyImmediate(currentFencesFolder);
            railsAPool.Clear();
            railsBPool.Clear();
            postsPool.Clear();
            subpostsPool.Clear();
            ex.extrasPool.Clear();
        }
        /// <summary>
        /// Finishes the current fence and starts a new one.
        /// </summary>
        /// <remarks>
        /// This method first removes unwanted colliders and destroys unused objects. Then it clears the references to the old parts. 
        /// If the duplicate parameter is set to false, it also clears the clickPoints and clickPointFlags. 
        /// It then sets up the folders and creates all pools. If the duplicate parameter is set to true, it forces a rebuild from the click points. 
        /// Finally, it returns the GameObject representing the new fence.
        /// </remarks>
        /// <param name="duplicate">If set to true, the new fence will be a duplicate of the current one.</param>
        /// <param name="merge">If set to true, the new fence will be merged with the current one.</param>
        /// <param name="hourMinSecStr">A string representing the time at which the new fence is started.</param>
        /// <param name="fencename">The name of the new fence. The default value is "Finished".</param>
        /// <returns>Returns the GameObject representing the new fence.</returns>
        public GameObject FinishAndStartNew(bool duplicate, bool merge, string hourMinSecStr, string fencename = "Finished")
        {
            RemoveUnwantedColliders();
            DestroyUnused();

            //-- Clear the references to the old parts ---
            if (duplicate == false)
            {
                clickPoints.Clear();
                clickPointFlags.Clear();
            }
            keyPoints.Clear();
            postsPool.Clear();
            railsAPool.Clear();
            railsBPool.Clear();
            subpostsPool.Clear();
            ex.extrasPool.Clear();
            subJoiners.Clear();
            closeLoop = false;
            gaps.Clear();
            allPostPositions.Clear();
            railsATotalTriCount = railsBTotalTriCount = postsTotalTriCount = extrasTotalTriCount = subPostsTotalTriCount = 0;

            globalLift = 0.0f;
            railABoxColliderHeightScale = 1.0f;
            railABoxColliderHeightOffset = 0.0f;
            currentFencesFolder = null; // break the reference to the old folder

            SetupFolders();
            CreateAllPools();

            if (duplicate == true)
                ForceRebuildFromClickPoints();

            return currentFencesFolder;
        }
        //---------------
        public void AddLODGroup(GameObject folder)
        {
            if (addLODGroup == true)
            {
                LODGroup lodGroup = folder.AddComponent<LODGroup>();
                LOD[] lodArray = new LOD[1];
                Transform[] allChildren = folder.GetComponentsInChildren<Transform>();
                List<Renderer> renderersList = new List<Renderer>();
                for (int i = 0; i < allChildren.Length; i++)
                {
                    Renderer childRenderer = allChildren[i].gameObject.GetComponent<Renderer>();
                    if (childRenderer != null)
                        renderersList.Add(childRenderer);
                }
                lodArray[0] = new LOD(0.08f, renderersList.ToArray());
                lodGroup.SetLODs(lodArray);
            }
        }
        //--------------------------
        public void ClearAllFences()
        {
            clickPoints.Clear(); clickPointFlags.Clear();
            keyPoints.Clear();
            allPostPositions.Clear();
            railsATotalTriCount = railsBTotalTriCount = postsTotalTriCount = extrasTotalTriCount = subPostsTotalTriCount = 0;
            ResetAllPools();
            DestroyNodeMarkers();
            postVectors.Clear();
            overlapExtrasZone.Clear();
            fillExtrasZone.Clear();
            postSinglesContainer.ClearAllSingles(this);
            closeLoop = false;
            globalLift = 0.0f;
        }
        //=====================================================
        /// <summary>
        /// Checks the post direction vectors for sufficient count and non-null values.
        /// </summary>
        /// <param name="logMissing">Optional parameter that indicates whether to log missing post direction vectors. Default is true.</param>
        /// <param name="caller">Optional parameter that indicates the caller of this method. Default is null.</param>
        /// <remarks>
        /// This method first checks if there are sufficient post direction vectors for the number of posts. 
        /// If not, it calculates all of them again from all post positions.
        /// Then, it checks that the post direction vectors are not null or zero.
        /// </remarks>
        /// <para> called by ed.OnEnable() and ex.BuildExtras()</para>
        public void CheckPostDirectionVectors(bool logMissing = true, [CallerMemberName] string caller = null)
        {
            //postVectors.checkForCountExceptions = false; //TODO Temp, will be re-enabled in Count. We don't want it OnEnable()
            int numPosts = allPostPositions.Count, numPostDirVectors = postVectors.Count;

            //-- Check we have sufficient postDirectionVectors for the number of postsPool
            if (numPostDirVectors != numPosts)
            {
                //if (logMissing == true)
                Debug.Log($"postDirectionVectors.Count ({numPostDirVectors}) !=  allPostPositions.Count() ({numPosts}   in CheckPostDirectionVectors().   {caller}\n");
                //-- If not, just Calculate all of them again from allPostPositions.Count
                CalculateAllPostDirectionVectors();
            }

            //-- Check that they're not null or zero
            for (int i = 0; i < allPostPositions.Count; i++)
            {
                if (postVectors[i] == null)
                    Debug.LogWarning($"postVectors[ {i} ] was null in CheckPostDirectionVectors().  {caller}\n");

                //Complain if the postDirectionVector is v3.zero, but only if there is more than one post
                if (allPostPositions.Count > 1 && postVectors[i].Position == Vector3.zero)
                    Debug.LogWarning($"postDirectionVectors[ {i} ] was zero in CheckPostDirectionVectors().   {caller}\n");

            }
        }
        //=========================================================================

        // This should be the only place where the ALL post direction vectors are calculated
        // -RebuildFromFinalList() In essence this is the ONLY source of creation, the rest is just checking
        // -From CheckPostDirectionVectors

        private void CalculateAllPostDirectionVectors([CallerMemberName] string caller = null)
        {
            if (allPostPositions.Count() == 0)
                return;
            if (postVectors == null)
            {
                Debug.LogError($"postVectors was null in CalculateAllPostDirectionVectors(). Recalculating them. \n");
                postVectors = new List<PostVector>();
            }
            PostVector.AddList(allPostPositions, this);
        }

        //--------------------------------------------------------------------------------

        /*private void CalculateAllPostDirectionVectors([CallerMemberName] string caller = null)
        {
            //Debug.Log($"CalculateAllPostDirectionVectors()  {caller}\n");

            if (allPostPositions.Count() == 0)
                return;
            if (postDirectionVectors == null)
            {
                Debug.LogError($"postDirectionVectors was null in CalculateAllPostDirectionVectors(). Recalculating them. \n");
                postDirectionVectors = new PostVectorList(af);
            }

            //-- Add All the postsPool in one go from RebuildFromFinalList(), any issues originate there.
            //-- We don't Add single postsPool as this risks the List becoming out of sync with af.allPostPositions
            postDirectionVectors.Clear();
            int postCount = allPostPositions.Count;

            //If there's only one post set it to world forward, it's arbitrary but V3.zero will trigger alerts unnecessarily
            if (postCount == 1)
            {
                postDirectionVectors.AddPostDirectionVectorToList(allPostPositions[0], allPostPositions[0] + Vector3.forward);
                //Debug.Log("Created 1 post in CalculateAllPostDirectionVectors()\n");
                return;
            }
            else
            {
                for (int sectionIndex = 0; sectionIndex < postCount - 1; sectionIndex++)
                {
                    postDirectionVectors.AddPostDirectionVectorToList(allPostPositions[sectionIndex], allPostPositions[sectionIndex + 1]);
                }
                // For the last one, we reuse the direction of the penultimate one
                AddLastPostDirectionVector(allPostPositions.Last());
                //Debug.Log($"Created {postDirectionVectors.Count} PostDirection vectors in CalculateAllPostDirectionVectors().     allPostPositions = {postCount}\n");
            }
        }
        private void AddLastPostDirectionVector(Vector3 position)
        {
            //-- this is the only time we set a vector explicitly, by reusing the previous one but at the new position
            //-- Maybe doesn't need to be a separate function, but waiting to see if there are more special considerations for the last one
            postDirectionVectors.AddLastPostDirectionVectorToList(position);

        }*/

        //=================================================
        //              CreateMergedPrefabs/Destroy Folders
        //=================================================
        public void SetupFolders()
        {
            currentFencesFolder = GameObject.Find("Current Fences Folder");
            // Make the Current Fences folder 
            if (currentFencesFolder == null)
            {
                currentFencesFolder = new GameObject("Current Fences Folder");
                //currentFencesFolder.transform.parent = GameObject.Find("Auto Fence Builder").transform;

                folderList.Add(currentFencesFolder);
                //?Selection.activeGameObject = this.gameObject;
            }
            if (currentFencesFolder != null)
            { // if it'stackIdx already there, destroy sub-folders before making new ones
                int numChildren = currentFencesFolder.transform.childCount;
                for (int i = numChildren - 1; i >= 0; i--)
                {
                    GameObject subFolder = currentFencesFolder.transform.GetChild(i).gameObject;
                    int grandChilds = subFolder.transform.childCount;
                    for (int j = grandChilds - 1; j >= 0; j--)
                    {
                        GameObject.DestroyImmediate(subFolder.transform.GetChild(j).gameObject);
                    }
                    DestroyImmediate(subFolder);
                }
            }
            extrasFolder = new GameObject("Extras");
            extrasFolder.transform.parent = currentFencesFolder.transform;
            postsFolder = new GameObject("Posts");
            postsFolder.transform.parent = currentFencesFolder.transform;
            railsFolder = new GameObject("Rails");
            railsFolder.transform.parent = currentFencesFolder.transform;
            subpostsFolder = new GameObject("Subs");
            subpostsFolder.transform.parent = currentFencesFolder.transform;
        }
        //--------------------------
        //Do this when necessary to check the user hasn't deleted the current working folder
        public void CheckFoldersBuilt()
        {
            if (currentFencesFolder == null)
            {
                SetupFolders();
                if (allPostPositions.Count > 0)
                    ClearAllFences();
            }
            else
            {

                if (postsFolder == null)
                {
                    postsFolder = new GameObject("Posts");
                    postsFolder.transform.parent = currentFencesFolder.transform;
                    ResetPoolForLayer(LayerSet.postLayerSet);
                }
                if (railsFolder == null)
                {
                    railsFolder = new GameObject("Rails");
                    railsFolder.transform.parent = currentFencesFolder.transform;
                    ResetPoolForLayer(LayerSet.railALayerSet);
                    ResetPoolForLayer(LayerSet.railBLayerSet);
                }
                if (subpostsFolder == null)
                {
                    subpostsFolder = new GameObject("Subs");
                    subpostsFolder.transform.parent = currentFencesFolder.transform;
                    ResetPoolForLayer(LayerSet.subpostLayerSet);
                }
                if (extrasFolder == null)
                {
                    extrasFolder = new GameObject("Extras");
                    extrasFolder.transform.parent = currentFencesFolder.transform;
                    ResetPoolForLayer(LayerSet.extraLayerSet);
                }
            }
        }

        //---------------------
        public void RebuildPoolWithNewUserPrefab(GameObject newUserPrefab, LayerSet layer)
        {
            int indexOfNewUserPost = FindPrefabIndexByName(layer, newUserPrefab.name);

            SetCurrentPrefabIndexForLayer(layer, indexOfNewUserPost);

            SetFirstSourceVariantToMainForLayer(layer);
            DestroyPoolForLayer(layer);
            CreatePoolForLayer(layer);
            //CreatePostsPool(allPostPositions.Count);

            ForceRebuildFromClickPoints();
        }
        //--------------------
        // If there are multiple contiguous gaps found, merge them in to 1 gap by deleting the previous point
        void MergeClickPointGaps()
        {
            for (int i = 2; i < clickPointFlags.Count(); i++)
            {
                if (clickPointFlags[i] == 1 && clickPointFlags[i - 1] == 1) // tow together so keep the last one, deactivate the first one
                    DeleteClickPoint(i - 1, false);
            }
        }
        //-----------------------------------------------
        // Where the use asked for a GAP, we remove all inter/spline postsPool between the break-clickPoint and the previous clickPoint
        void RemoveAllInbetweenPostPositionsFromGaps()
        {
            if (allowGaps == false || clickPoints.Count < 3) return;

            //Vector3 previousValidClickPoint = clickPoints[2];
            int clickPointIndex = 0, breakPointIndex = -1, previousValidIndex = 1;

            List<int> removePostsIndices = new List<int>();

            for (int i = 2; i < allPostPositions.Count; i++)
            { // the first two can not be break points, as they are the minimum 1 single section of fence
                Vector3 thisPostPos = allPostPositions[i];
                clickPointIndex = clickPoints.IndexOf(thisPostPos);
                if (clickPointIndex != -1)
                { // it'stackIdx a clickPoint!
                    if (clickPointFlags[clickPointIndex] == 1)
                    { // it'stackIdx a break point!
                        breakPointIndex = i; // we will remove all the post between this and previousValidIndex
                        for (int r = previousValidIndex + 1; r < breakPointIndex; r++)
                        {
                            if (removePostsIndices.Contains(r) == false)
                                removePostsIndices.Add(r);
                        }
                    }
                    else
                        previousValidIndex = i;
                }
            }

            for (int i = removePostsIndices.Count - 1; i >= 0; i--)
            { // comment this out to disable breakPoints
                allPostPositions.RemoveAt(removePostsIndices[i]);
            }
        }
        //------------------------
        bool IsBreakPoint(Vector3 pos)
        {

            int clickPointIndex = clickPoints.IndexOf(pos);
            if (clickPointIndex != -1)
            { // it'stackIdx a clickPoint!
                if (clickPointFlags[clickPointIndex] == 1)
                { // it'stackIdx a break point!
                    return true;
                }
            }
            return false;
        }

        //------------
        // Usually(?) coming after OnSceneGUI
        void OnDrawGizmos()
        {
            //StackLog(this.GetType().Name);

            /*if (gizmoManager == null)
            {
                gizmoManager = new GizmoDrawManager(this);
                Debug.Log("GizmoDrawManager was null in OnDrawGizmos\n");
            }
            gizmoManager.DrawGizmoManager();*/


            DrawVisualDebug();

            //==================    Fence part highlighting   ==================
            //if (showPartHighlighting == true)

            if (highlightedParts != null)
            {
                for (int i = 0; i < highlightedParts.Count; i++)
                {
                    Transform highlightedPartTrans = highlightedParts[i];
                    if (highlightedPartTrans == null)
                        continue;


                    Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.5f);
                    Vector3 pos = highlightedPartTrans.transform.position;
                    Debug.Log(pos + " transform's world position  \n");

                    //Gizmos.DrawCube(highlightedPart.transform.position, highlightedPart.transform.localScale);

                    Bounds bounds_ws = highlightedPartTrans.gameObject.GetComponent<Renderer>().bounds;
                    Vector3 boundsCenter_ws = bounds_ws.center;
                    Debug.Log(boundsCenter_ws + " boundsCenter_ws \n");
                    Vector3 extents = bounds_ws.extents;

                    //Gizmos.DrawCube(meshCenter, highlightedPart.transform.localScale);

                    //Debug.Log(boundsCenter_ws + " boundsCenter_ws \n");

                    //-- Calculate the  bounds_ws corners 
                    Vector3[] worldCorners = new Vector3[8];
                    Vector3 pt_corner;

                    pt_corner = boundsCenter_ws + new Vector3(-extents.x, extents.y, extents.z);
                    worldCorners[0] = pt_corner;
                    //Debug.Log(pt_corner + "pt_corner  \n");

                    pt_corner = boundsCenter_ws + new Vector3(extents.x, extents.y, extents.z);
                    worldCorners[1] = pt_corner;

                    pt_corner = boundsCenter_ws + new Vector3(extents.x, -extents.y, extents.z);
                    worldCorners[2] = pt_corner;


                    pt_corner = boundsCenter_ws + new Vector3(-extents.x, -extents.y, extents.z);
                    worldCorners[3] = pt_corner;


                    pt_corner = boundsCenter_ws + new Vector3(-extents.x, extents.y, -extents.z);
                    worldCorners[4] = pt_corner;



                    pt_corner = boundsCenter_ws + new Vector3(extents.x, extents.y, -extents.z);
                    worldCorners[5] = pt_corner;


                    pt_corner = boundsCenter_ws + new Vector3(extents.x, -extents.y, -extents.z);
                    worldCorners[6] = pt_corner;


                    pt_corner = boundsCenter_ws + new Vector3(-extents.x, -extents.y, -extents.z);
                    worldCorners[7] = pt_corner;


                    // Adjust each vert by the object's position
                    for (int j = 0; j < worldCorners.Length; j++)
                    {
                        //worldCorners[j] += meshCenter;

                        //worldCorners[j].x += meshCenter.x;
                    }


                    // DrawTCT lines between the corners to form the box
                    Gizmos.color = new Color(1, 0.6f, 0.2f); // Set Gizmo color

                    // Bottom square
                    Gizmos.DrawLine(worldCorners[0], worldCorners[1]);
                    Gizmos.DrawLine(worldCorners[1], worldCorners[2]);
                    Gizmos.DrawLine(worldCorners[2], worldCorners[3]);
                    Gizmos.DrawLine(worldCorners[3], worldCorners[0]);

                    // Top square
                    Gizmos.DrawLine(worldCorners[4], worldCorners[5]);
                    Gizmos.DrawLine(worldCorners[5], worldCorners[6]);
                    Gizmos.DrawLine(worldCorners[6], worldCorners[7]);
                    Gizmos.DrawLine(worldCorners[7], worldCorners[4]);

                    // Sides
                    Gizmos.DrawLine(worldCorners[0], worldCorners[4]);
                    Gizmos.DrawLine(worldCorners[1], worldCorners[5]);
                    Gizmos.DrawLine(worldCorners[2], worldCorners[6]);
                    Gizmos.DrawLine(worldCorners[3], worldCorners[7]);

                    //Gizmos.color = color;
                    for (int k = 0; k < 8; k++)
                    {
                        Gizmos.DrawCube(worldCorners[k], Vector3.one * 0.1f);


                    }
                    Color transBoundBoxColor = new Color(1, 1.0f, 0.3f, 0.15f); // Set Gizmo color
                    Gizmos.color = transBoundBoxColor;
                    Gizmos.DrawCube(boundsCenter_ws, bounds_ws.size);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(pos, Vector3.one * 0.1f);

                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(boundsCenter_ws, Vector3.one * 0.1f);

                }
            }

            //==================================================================



            //=============debug utils
            //=====  DrawTCT Markers  =====
            /* for (int stackIdx = 0; stackIdx < debugMarkers.Count; stackIdx++)
             {
                 Vector3 pos = debugMarkers[stackIdx];
                 pos.y += heightOffset;
                 if (shape == 0 || mesh == null)
                     Gizmos.DrawSphere(pos, adjustedNativeScale);
                 else if (shape == 1 && mesh != null)
                     Gizmos.DrawMesh(mesh, pos);
             }*/
            //=================================


            //======  Show Gap Lines  ======
            Color lineColor = new Color(.1f, .1f, 1.0f, 0.4f);
            Gizmos.color = lineColor;
            Vector3 a = Vector3.zero, b = Vector3.zero;
            if (showDebugGapLine && allowGaps)
            {
                for (int i = 0; i < gaps.Count(); i += 2)
                {
                    a = gaps[i]; a.y += 0.8f;
                    b = gaps[i + 1]; b.y += 0.8f;
                    Gizmos.DrawLine(a, b); // draw a line to show user gaps
                    a.y += 0.3f;
                    b.y += 0.3f;
                    Gizmos.DrawLine(a, b);
                    a.y += 0.3f;
                    b.y += 0.3f;
                    Gizmos.DrawLine(a, b);
                }
            }
            //======  Show Preview Lines  ======
            if (afwbActive == true)
            {
                Vector3 p0 = previewPoints[0], p1 = previewPoints[1];
                Event e = Event.current;
                // Lines
                if (e.shift && clickPoints.Count > 0 && showPreviewLines)
                {
                    //Debug.Log("shift \n");
                    Gizmos.color = new Color(.2f, .7f, .2f, 0.75f);

                    p0.y += 0.1f;
                    p1.y += 0.1f;
                    if (!e.control) // we're not inserting
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            Gizmos.DrawLine(p0, p1);
                            p0.y += 0.3f;
                            p1.y += 0.3f;
                        }
                    }
                    Gizmos.DrawLine(p1, p1 - new Vector3(0, 2.5f, 0));
                }
                // Show Preview Post
                if (e.shift /*&& clickPoints.Count == 0*/)
                {
                    Gizmos.color = new Color(.2f, .85f, .2f, 0.55f);
                    float dist = Vector3.Distance(p0, p1);
                    dist = dist % interPostDist;
                    Mesh mesh = markerPostMeshLarge;
                    if (snapMainPosts == true && (dist < -0.01f || dist > 0.01f))
                    {
                        mesh = markerPostMesh;
                        Gizmos.color = new Color(.7f, .1f, .1f, 0.55f);
                    }
                    if (!e.control)
                    {
                        //Gizmos.color = new Color(.2f, .7f, .2f, 0.55f);
                        Gizmos.DrawMesh(mesh, currMousePosition);
                    }
                    else
                    {
                        Gizmos.color = new Color(.75f, .7f, .2f, 0.6f);
                        Gizmos.DrawMesh(mesh, currMousePosition);
                    }
                }
            }
        }


        //-----------------------
        // Get the original transform.localScale from the current object.
        // AFWB build-in prefabs are always scaled (1,1,1), but users' might differ
        void ResetNativePrefabScales()
        {
            nativeRailAScale = GetCurrentPrefabForLayer(LayerSet.railALayerSet).transform.localScale;
            nativeRailBScale = GetCurrentPrefabForLayer(LayerSet.railBLayerSet).transform.localScale;
            nativeSubScale = GetCurrentPrefabForLayer(LayerSet.postLayerSet).transform.localScale;
            nativeExtraScale = GetCurrentPrefabForLayer(LayerSet.extraLayerSet).transform.localScale;
            nativePostScale = GetCurrentPrefabForLayer(LayerSet.postLayerSet).transform.localScale;


        }
        //------------------------
        public Mesh GetMainMeshFromGO(GameObject go)
        {
            List<Mesh> meshes = MeshUtilitiesAFWB.GetAllMeshesFromGameObject(go);
            if (meshes != null && meshes.Count > 0)
                return meshes[0];
            else
                return null;
        }
        //--------------------
        //-- Randomly shift the post positions in the XZ (ground) plane, not in height.
        //-- This is done before the building, just on the positions list: allPostPositions[], which are then read during SetupPost()
        //-- The random shift is done in the direction of the previous post, with constraints on angle (60deg) and distance (0.5 -1.5 orginal).
        //-- Called from ForceRebuildFromClickPoints()
        void RandomisePostPositionsXZShift(List<Vector3> postPosList)
        {
            //ClearConsole();
            int postCount = postPosList.Count;

            postAndGlobalSeeds.CheckSeedValues(); //needed for the random positioning
            //postAndGlobalSeeds.rPostShiftX

            for (int i = 1; i < postCount; i++) // Start from 1 since we need a previous post
            {
                Vector3 prevPostPos = postPosList[i - 1];
                Vector3 postPos = postPosList[i];
                PostVector postVector = PostVector.GetPostVectorAtIndex(i);

                //if (!IsClickPoint(postPos))
                if (!postVector.IsClickPointNode)
                {
                    //check if the previous post or the nextPos post is a clickPoint
                    bool prevIsClickPoint = postVector.GetPrevious().IsClickPointNode;
                    bool nextIsClickPoint = false;
                    if (i < postCount - 1)
                        nextIsClickPoint = postVector.GetPrevious().IsClickPointNode;

                    // If the previous or nextPos is a clickPoint, reduce min max and angle deviation. This is because at a Post
                    // there could already be a sharp angle and we can't risk bending that too far which will cause mitering issues
                    float maxAngleDeviation = 60f;
                    float minDist = 0.5f, maxDist = 1.5f;
                    if (prevIsClickPoint || nextIsClickPoint)
                    {
                        maxAngleDeviation = 20f;
                        minDist = 0.7f;
                        maxDist = 1.3f;
                    }

                    Vector3 dirVec = (postPos - prevPostPos).normalized;
                    float d = Vector3.Distance(prevPostPos, postPos);
                    float halfD = d / 2; // Compute once outside the loop

                    bool positionValid = false;
                    int attempt = 0;
                    for (attempt = 0; attempt < 3 && !positionValid; attempt++)
                    {
                        float randX, randZ;
                        if (attempt == 0) // Use initial randomization for the first attempt
                        {
                            randX = postAndGlobalSeeds.rPostShiftX[i];
                            randZ = postAndGlobalSeeds.rPostShiftZ[i];
                        }
                        else // Subsequent attempts use new random values within half the distance
                        {
                            randX = UnityEngine.Random.Range(-halfD, halfD);
                            randZ = UnityEngine.Random.Range(-halfD, halfD);
                        }

                        Vector3 newPostPos = new Vector3(postPos.x + randX, postPos.y, postPos.z + randZ);
                        Vector3 newDirVec = (newPostPos - prevPostPos).normalized;
                        float newD = Vector3.Distance(prevPostPos, newPostPos);

                        // Check angle condition, limit to maxAngleDeviation degrees variance form original direction
                        bool angleCondition = Vector3.Angle(dirVec, newDirVec) <= maxAngleDeviation;

                        // Check distance condition to prevent posts from being too close or too far apart
                        bool distanceCondition = newD >= minDist * d && newD <= maxDist * d;

                        if (angleCondition && distanceCondition)
                        {
                            postPosList[i] = newPostPos;
                            positionValid = true;
                        }
                    }

                    if (!positionValid)
                    {
                        // Leave the final position unmodified if conditions are not met after 3 attempts
                        postPosList[i] = postPos;
                        //Debug.Log($" Gave Up!!      Attempted {attempt} times to randomize post position {stackIdx}\n");
                    }
                    //else
                    //Debug.Log($"Attempted {attempt} times to randomize post position {stackIdx}\n");
                }
            }
        }
        //--------------------
        //Random Spacing along the linear direction
        void RandomisePostSpacing(List<Vector3> allKeyPoints)
        {
            int numKeyPoints = allKeyPoints.Count;

            if (postSpacingVariation > 0)
            {
                UnityEngine.Random.InitState(postAndGlobalSeeds.globalSpacingSeed);

                List<Vector3> randKeyPoints = new List<Vector3>();
                randKeyPoints.Add(allKeyPoints[0]);
                Vector3 prev = keyPoints[0], curr = allKeyPoints[0], next = allKeyPoints[0];
                for (int i = 1; i < numKeyPoints - 1; i++)
                {
                    curr = allKeyPoints[i];
                    next = allKeyPoints[i + 1];

                    float x, dxPrev = (curr.x - prev.x); //distanceToNextPost between  postsPool
                    float y, dyPrev = (curr.y - prev.y);
                    float z, dzPrev = (curr.z - prev.z);
                    float dxNext = (next.x - curr.x); //distanceToNextPost between  postsPool
                    float dyNext = (next.y - curr.y);
                    float dzNext = (next.z - curr.z);

                    float rangeScalar = 0.95f;//TODO so the display can read 0-1 neatly
                    float r = UnityEngine.Random.Range(-rangeScalar * postSpacingVariation, rangeScalar * postSpacingVariation);

                    if (r <= 0)
                    {
                        x = curr.x - (dxPrev * r);
                        y = curr.y - (dyPrev * r);
                        z = curr.z - (dzPrev * r);
                    }
                    else
                    {
                        x = curr.x + (dxNext * r);
                        y = curr.y + (dyNext * r);
                        z = curr.z + (dzNext * r);
                    }

                    Vector3 randKeyPoint = new Vector3(x, y, z);
                    randKeyPoints.Add(randKeyPoint);
                    prev = curr;
                }
                randKeyPoints.Add(allKeyPoints[numKeyPoints - 1]);
                for (int i = 1; i < numKeyPoints; i++)
                {
                    if (i >= keyPoints.Count || i >= randKeyPoints.Count)
                        Debug.Log("");

                    if (IsCloseClickPoint(allKeyPoints[i]) == -1)
                    {
                        allKeyPoints[i] = randKeyPoints[i];
                    }
                }
            }
        }
        //-------------------
        // Smooth
        List<Vector3> MakeSmoothSplineFromClickPoints()
        {
            // SplineFillMode {fixedNumPerSpan = 0, equiDistant, angleDependent};
            if (smooth == false || roundingDistance == 0 || clickPoints.Count < 3)
                return keyPoints; //abort
                                  //-- Add 2 at each end before interpolating
            List<Vector3> splinedList = new List<Vector3>();
            Vector3 dirFirstTwo = (clickPoints[1] - clickPoints[0]).normalized;
            Vector3 dirLastTwo = (clickPoints[clickPoints.Count - 1] - clickPoints[clickPoints.Count - 2]).normalized;

            if (closeLoop)
            {
                splinedList.Add(clickPoints[clickPoints.Count - 3]);
                splinedList.Add(clickPoints[clickPoints.Count - 2]);
            }
            else
            {
                splinedList.Add(clickPoints[0] - (2 * dirFirstTwo));
                splinedList.Add(clickPoints[0] - (1 * dirFirstTwo));
            }

            splinedList.AddRange(clickPoints);
            if (closeLoop)
            {
                splinedList.Add(clickPoints[1]);
                splinedList.Add(clickPoints[2]);
            }
            else
            {
                splinedList.Add(clickPoints[clickPoints.Count - 1] + (2 * dirLastTwo));
                splinedList.Add(clickPoints[clickPoints.Count - 1] + (1 * dirLastTwo));
            }
            //int points = 51 - roundingDistance;
            splinedList = CreateCubicSpline3D(splinedList, roundingDistance, SplineFillMode.equiDistant, tension);
            ThinByAngle(splinedList);
            ThinByDistance(splinedList);
            //---------------------------
            keyPoints.Clear();
            keyPoints.AddRange(splinedList);

            if (keepInterpolatedPostsGrounded)
                LowerPostsToGround(keyPoints);
            else
                RaiseUpToGround(keyPoints);

            return keyPoints;
        }
        //--------------
        // Every single singleVarGO gizmoSingletonInstance has to have it'stackIdx own unique mesh, because they become re-shaped
        // to fit the slope of the land. 
        List<Mesh> CreatePreparedRailMesh(LayerSet railSet, List<Mesh> railMeshGroup)
        {
            //Check that we've made backups so we don't break the original
            if (origRailPrefabMeshes == null || origRailPrefabMeshes.Count < currentRail_PrefabIndex[0] + 1 || origRailPrefabMeshes[currentRail_PrefabIndex[0]] == null)
            {
                BackupPrefabMeshes(railPrefabs, origRailPrefabMeshes); // build update
            }
            List<Mesh> preparedMeshSet = new List<Mesh>();
            // For the object and its children
            for (int i = 0; i < railMeshGroup.Count; i++)
            {
                Mesh origMesh = railMeshGroup[i];
                Mesh dupMesh = MeshUtilitiesAFWB.DuplicateMesh(origMesh);
                preparedMeshSet.Add(dupMesh);
            }
            return preparedMeshSet;
        }
        //------------------------
        //CreateMergedPrefabs one of each singleVarGO types meshes/submeshes. These will be used as a source for cloning each section
        public void CreateAllPreparedMeshVariations(LayerSet layer)
        { //Debug.Log("CreateAllPreparedMeshVariations()\n");
            float mainMeshHeight = 1, variantMeshHeight = 0;
            int layerIndex = (int)layer;

            List<SourceVariant> sourceVariants = GetSourceVariantsForLayer(layer);
            int numVariations = sourceVariants.Count; //This is icluding main, so maximum is 1 + 8 = 9
            if (GetUseLayerVariations(layer) == false)
                numVariations = 1;
            if (sourceVariants == null || sourceVariants.Count == 0 || sourceVariants[0] == null)
                Debug.Log("sourceVariants missing /numRequired");

            Mesh mainMesh = null;
            if (sourceVariants[0].Go != null)
                mainMesh = GetMainMeshFromGO(sourceVariants[0].Go);
            else
            {
                Debug.LogWarning($"SourceVariant[0] was Null in CreateAllPreparedMeshVariations()  {GetLayerNameAsString(layer)} " +
                    $"   PreparedMeshVariations were not created   \n");
                return;
                //PrintSourceVariantGOsForLayer(layer, activeOnly: true);

            }
            mainMeshHeight = mainMesh.bounds.size.y;
            for (int i = 0; i < numVariations; i++)
            {
                SourceVariant thisVariant = sourceVariants[i];
                GameObject thisGO = thisVariant.Go;
                if (thisGO == null)
                    Debug.Log($" CreateAllPreparedMeshVariations  {GetLayerNameAsString(layer)}   sourceVariants is null");
                if (layer == LayerSet.railALayerSet)
                {
                    //-- This INCLUDES the main  singleVarGO.
                    railAPreparedMeshVariants[i] = CreatePreparedRailMesh(LayerSet.railALayerSet, MeshUtilitiesAFWB.GetAllMeshesFromGameObject(thisGO));
                    //-- If it'stackIdx a variation and not the main singleVarGO, make sure it'stackIdx height adjustedNativeScale matches th main
                    if (scaleVariationHeightToMainHeightA == true && i > 0)
                        variantMeshHeight = railAPreparedMeshVariants[i][0].bounds.size.y;
                }
                if (layer == LayerSet.railBLayerSet)
                {
                    railBPreparedMeshVariants[i] = CreatePreparedRailMesh(LayerSet.railBLayerSet, MeshUtilitiesAFWB.GetAllMeshesFromGameObject(thisGO));
                    if (scaleVariationHeightToMainHeightB == true && i > 0)
                        variantMeshHeight = railBPreparedMeshVariants[i][0].bounds.size.y;
                }
            }
        }
        //-----------------
        public List<List<Mesh>> GetPreparedMeshVariantsForLayer(LayerSet layer)
        {
            List<List<Mesh>> preparedMeshVariants = railAPreparedMeshVariants;
            if (layer == LayerSet.railBLayerSet)
                preparedMeshVariants = railBPreparedMeshVariants;

            if (preparedMeshVariants == null)
                Debug.LogWarning($"preparedMeshVariants is null  for {GetLayerNameAsString(layer)}\n");
            else if (preparedMeshVariants.Count == 0)
                Debug.LogWarning($"preparedMeshVariants.Count is 0  for {GetLayerNameAsString(layer)}\n");
            else if (preparedMeshVariants[0] == null)
                Debug.LogWarning($"preparedMeshVariants[0] is null  for {GetLayerNameAsString(layer)}\n");

            return preparedMeshVariants;
        }
        //---------------------------------------------
        public void SetGlobalLevelling()
        {
            if (globalLevelling == false)
                return;

            Vector3 firstPostPos = allPostPositions[0];
            float firstPostHeight = firstPostPos.y;

            for (int i = 1; i < allPostPositions.Count; i++)
            {
                Vector3 postPos = allPostPositions[i];
                postPos.y = firstPostHeight;
                allPostPositions[i] = postPos;
            }
        }
        //--------------------
        // lower things to ground level
        public void LowerPostsToGround(List<Vector3> vec3List)
        {
            RaycastHit hit;
            Vector3 pos, highPos;
            float extraHeight = 15;
            SetIgnorePartsColliders(true);

            LayerMask layerMask = GetIgnoreLayerMask();
            int val = layerMask.value;

            for (int i = 0; i < vec3List.Count; i++)
            {
                highPos = pos = vec3List[i];
                highPos.y += extraHeight;
                if (Physics.Raycast(highPos, Vector3.down, out hit, 500/*, ~layerMask*/)) // First check from above, looking down
                                                                                          //if (Physics.Raycast(highPos, Vector3.down, out hit, 500, groundLayers)) // First check from above, looking down
                {
                    //Debug.Log(hit.collider.gameObject.name + "  hit.collider.gameObject.name \n");
                    if (hit.collider.gameObject != null)
                    {
                        pos += new Vector3(0, -(hit.distance - extraHeight), 0);
                    }
                }
                else if (Physics.Raycast(pos, Vector3.up, out hit, 500/*, ~layerMask*/)) // maybe we've gone below... check upwards
                {
                    if (hit.collider.gameObject != null)
                    {
                        pos += new Vector3(0, +hit.distance, 0);
                    }
                }
                vec3List[i] = pos;
            }
            SetIgnorePartsColliders(false);
        }
        //--------------------
        // Ensure things aren't below ground level
        public void RaiseUpToGround(List<Vector3> vec3List)
        {
            RaycastHit hit;
            Vector3 pos;
            SetIgnorePartsColliders(true);

            for (int i = 0; i < vec3List.Count; i++)
            {
                pos = vec3List[i];
                Vector3 currPos = vec3List[i];
                float currY = currPos.y;
                float rayStartHeight = 50;
                currPos.y += rayStartHeight;
                if (Physics.Raycast(currPos, Vector3.down, out hit, 500))
                {
                    if (hit.collider.gameObject != null)
                    {
                        float distToGround = hit.distance + 0.0f;
                        if (hit.distance < rayStartHeight)
                        {
                            //thisSub.transform.Translate(0, -(distToGround - rayStartHeight), 0);
                            pos.y -= (distToGround - rayStartHeight);
                            vec3List[i] = pos;
                        }
                        //Debug.Log(hit.distanceToNextPost + "  " + rayStartHeight + "    " + currY + "\n");
                    }

                }
            }
            SetIgnorePartsColliders(false);
        }
        //--------------------
        public void GlobalLevellingLowerPostsToGround(List<Vector3> vec3List)
        {
            if (globalLevelling == false)
                return;

            Vector3 firstPostPos = allPostPositions[0];
            float firstPostElevation = firstPostPos.y;

            float trueMeshHeightPost = postsPool[0].gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
            float scaledPostHeight = trueMeshHeightPost * postsPool[0].transform.localScale.y;

            //-- From the position and true height, we now now the position of the maxGoTop which is our levelling level.
            //float levellingHeight = firstPostElevation + scaledPostHeight;

            RaycastHit hit;
            Vector3 newPos = Vector3.zero, raycastPos, newScale;
            Vector3 currPostPos = Vector3.zero;
            float origPostPosY, hitPosY, raycastElevation = 50, newScaledHeight;
            float newScaleY = 1, postBasePosDelta = 0, currTopOfPost;

            float mostBuriedPostTop = 0; //keep a record of the max a post-maxGoTop is buriied so we can auto raise all to be just visible

            SetIgnorePartsColliders(true);

            for (int i = 1; i < postsBuiltCount; i++)
            {
                origPostPosY = postsPool[i].transform.position.y;
                newPos = currPostPos = raycastPos = postsPool[i].transform.position;
                raycastPos.y += raycastElevation;

                currTopOfPost = currPostPos.y + scaledPostHeight;

                if (Physics.Raycast(raycastPos, Vector3.down, out hit, 500)) // First check from above, looking down
                {
                    if (hit.collider.gameObject != null)
                    {
                        //-- Position
                        float delta = hit.distance - raycastElevation;
                        hitPosY = raycastPos.y - hit.distance;
                        //newPos.y = (origPostPosY - delta);
                        newPos.y = hit.point.y;

                        // Does the post base need to be lower, if so, move down and increase length of post so maxGoTop remains level
                        // If the post is lower than the hitPoint  (stackIdx.e post is buried, set its position to be 1m under and adjustedNativeScale accordingly
                        postBasePosDelta = origPostPosY - hitPosY;


                        if (currPostPos.y > hit.point.y) // the post needs to be lowered
                        {
                            postsPool[i].transform.position = newPos;
                            //-- Scaling
                            newScaledHeight = scaledPostHeight + delta;
                            newScaleY = newScaledHeight / trueMeshHeightPost;
                            newScale = postsPool[i].transform.localScale;
                            newScale.y = newScaleY;
                            postsPool[i].transform.localScale = newScale;
                        }
                        else if (currPostPos.y <= hit.point.y) // It'stackIdx already lower so set to consistent1m below
                        {
                            //postsPool[stackIdx].transform.position = new Vector3(currPostPos.x, hit.point.y - 1, currPostPos.z);
                        }

                        if (currTopOfPost <= hit.point.y)
                        {
                            float buried = -(hit.point.y - currTopOfPost);
                            if (buried < mostBuriedPostTop)
                                mostBuriedPostTop = buried;
                        }
                    }
                }
            }

            float defaultExtraHeight = 0.0f;
            float extraheightRequired = (mostBuriedPostTop * -1) + defaultExtraHeight + globalLevellingOffset;
            float tallestPost = 0, lowestPostPosY = 100000;
            for (int i = 0; i < postsBuiltCount; i++)
            {
                float currScaledPostHeight = trueMeshHeightPost * postsPool[i].transform.localScale.y;
                newScaledHeight = currScaledPostHeight + extraheightRequired;
                newScaleY = newScaledHeight / trueMeshHeightPost;
                postsPool[i].transform.localScale = new Vector3(postsPool[i].transform.localScale.x, newScaleY, postsPool[i].transform.localScale.z);
                if (newScaledHeight > tallestPost)
                    tallestPost = newScaledHeight;
                if (postsPool[i].transform.localPosition.y < lowestPostPosY)
                    lowestPostPosY = postsPool[i].transform.localPosition.y;

                /*if (newScaleY / postScale.y > maxExtraPostScaling)
                    maxExtraPostScaling = newScaleY / postScale.y;*/
            }
            //float maxExtraPostScaling = tallestPost / trueMeshHeightPost;
            //float maxPostHeightIncrease = tallestPost - trueMeshHeightPost;
            //Debug.Log(maxExtraPostScaling + "\n");


            float trueMeshHeightRail = 1.0f, initialRailAPosY = 0;
            if (useRailLayer[0] == true && railsAPool.Count > 0)
            {
                trueMeshHeightRail = railsAPool[0].gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
                //initialRailAPosY = railsAPool[0].transform.localPosition.y;
            }
            float scaledRailHeight = trueMeshHeightRail * railAScale.y;
            //float newScaledRailHeight = 0;
            // float heightDeltaRailPost = scaledPostHeight - scaledRailHeight;

            float initialPostPosY = 0;
            if (usePostsLayer == true && postsPool.Count > 0)
            {
                initialPostPosY = postsPool[0].transform.localPosition.y;
            }
            //float initialDelta = initialRailAPosY - initialPostPosY;
            //float postPosY = 0;

            float railBaseRelativeToPostBase = railAPositionOffset.y;
            //float topOfRailRelativeToPostBase = railBaseRelativeToPostBase + scaledRailHeight;
            //float railTopRelativeToPostTop = scaledPostHeight - topOfRailRelativeToPostBase;
            //float railPostHeightDifference = scaledPostHeight - scaledRailHeight;
            Vector3 currRailPos = Vector3.zero;
            //============  Rails currPost =============
            if (useRailLayer[0])
            {
                for (int i = 0; i < railABuiltCount; i++)
                {
                    float currPostheight = tallestPost;
                    float requiredRailHeight = scaledRailHeight;
                    float newRailScalingY = requiredRailHeight / trueMeshHeightRail;

                    railsAPool[i].transform.localScale = new Vector3(railsAPool[i].transform.localScale.x, newRailScalingY, railsAPool[i].transform.localScale.z);

                    currRailPos = railsAPool[i].transform.localPosition;
                    currRailPos.y = lowestPostPosY + (requiredRailHeight / 2);
                    railsAPool[i].transform.localPosition = currRailPos;

                    int stackNumber = 0;
                    if (numStackedRails[kRailALayerInt] > 1)
                    {
                        stackNumber = i % (int)numStackedRails[kRailALayerInt];
                        railsAPool[i].transform.localPosition += new Vector3(0, stackNumber * railSpread[kRailALayerInt], 0);
                    }
                }
            }
            //============  Rails nextPost =============

            if (useRailLayer[1])
            {
                trueMeshHeightRail = railsBPool[0].gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
                scaledRailHeight = trueMeshHeightRail * railBScale.y;
                //newScaledRailHeight = 0;
                //heightDeltaRailPost = scaledPostHeight - scaledRailHeight;

                initialRailAPosY = railsBPool[0].transform.localPosition.y;
                initialPostPosY = postsPool[0].transform.localPosition.y;
                //initialDelta = initialRailAPosY - initialPostPosY;
                //postPosY = 0;

                railBaseRelativeToPostBase = railBPositionOffset.y;
                //topOfRailRelativeToPostBase = railBaseRelativeToPostBase + scaledRailHeight;
                //railTopRelativeToPostTop = scaledPostHeight - topOfRailRelativeToPostBase;
                //railPostHeightDifference = scaledPostHeight - scaledRailHeight;
                currRailPos = Vector3.zero;

                for (int i = 0; i < railBBuiltCount; i++)
                {
                    //float currPostheight = tallestPost;
                    float requiredRailHeight = scaledRailHeight;
                    float newRailScalingY = requiredRailHeight / trueMeshHeightRail;

                    railsBPool[i].transform.localScale = new Vector3(railsBPool[i].transform.localScale.x, newRailScalingY, railsBPool[i].transform.localScale.z);

                    currRailPos = railsBPool[i].transform.localPosition;
                    currRailPos.y = lowestPostPosY + (requiredRailHeight / 2);
                    railsBPool[i].transform.localPosition = currRailPos;

                    int stackNumber = 0;
                    if (numStackedRails[kRailBLayerInt] > 1)
                    {
                        stackNumber = i % (int)numStackedRails[kRailBLayerInt];
                        railsBPool[i].transform.localPosition += new Vector3(0, stackNumber * railSpread[kRailBLayerInt], 0);
                    }
                }
            }

            SetIgnorePartsColliders(false);
        }
        //-----------------------
        public void ResizeSingleVariantsLists(int sectionCount)
        {
            if (useRailLayer[kRailALayerInt] == true && useRailSingles[kRailALayerInt] == true && sectionCount > singlesContainer.GetSingleVariantsForLayer(LayerSet.railALayerSet, af, warning: false).Count)
                ResizeSingleVariantsListForLayer(LayerSet.railALayerSet, sectionCount);
            else if (useRailLayer[kRailBLayerInt] == true && useRailSingles[kRailBLayerInt] && sectionCount > singlesContainer.GetSingleVariantsForLayer(LayerSet.railBLayerSet, af, warning: false).Count)
                ResizeSingleVariantsListForLayer(LayerSet.railBLayerSet, sectionCount);
            else if (usePostsLayer == true && usePostSingles == true && sectionCount > singlesContainer.GetSingleVariantsForLayer(LayerSet.postLayerSet, af, warning: false).Count)
                ResizeSingleVariantsListForLayer(LayerSet.postLayerSet, sectionCount);
        }
        //-----------------------------------
        //-- Will expand the List to be of railScale count
        void ResizeSingleVariantsListForLayer(LayerSet layer, int newCount)
        {
            List<SinglesItem> SinglesItems = singlesContainer.GetSingleVariantsForLayer(layer, af);
            int oldCount = SinglesItems.Count;
            if (SinglesItems.Count < newCount)
            {
                int extraRequired = newCount - SinglesItems.Count;
                SinglesItem newSinglesItem = new SinglesItem();
                SinglesItems.AddRange(Enumerable.Repeat(newSinglesItem, extraRequired));
            }
            Debug.Log($"Resizing{GetLayerNameAsString(layer)} SingleVariants \n" + $"  oldCount {oldCount}    new count: {SinglesItems.Count} \n");

            //Reduce
            /*else if (count > SinglesItems.Count + 50)
            {
                //resize the SinglesItems List to be of railScale count
                //SinglesItems.Capacity = count;
            }*/
        }
        //-----------------------------------------
        //Includes Mesh and Transforms
        public void CalculateAllFinalPrefabSizes(bool print = false)
        {
            prefabMeshWithTransformsSize[(int)LayerSet.railALayerSet] = CalculateTotalSizeOfCurrentLayerPrefab(LayerSet.railALayerSet);
            prefabMeshWithTransformsSize[(int)LayerSet.railBLayerSet] = CalculateTotalSizeOfCurrentLayerPrefab(LayerSet.railBLayerSet);
            prefabMeshWithTransformsSize[(int)LayerSet.postLayerSet] = CalculateTotalSizeOfCurrentLayerPrefab(LayerSet.postLayerSet);

            if (print)
            {
                if (useRailLayer[0])
                    Debug.Log("Rail A Size = " + prefabMeshWithTransformsSize[(int)LayerSet.railALayerSet] + "\n");
                if (useRailLayer[1])
                    Debug.Log("Rail B Size = " + prefabMeshWithTransformsSize[(int)LayerSet.railBLayerSet] + "\n");
                if (usePostsLayer)
                    Debug.Log("Post Size = " + prefabMeshWithTransformsSize[(int)LayerSet.postLayerSet] + "\n");
            }
        }
        //-----------------------------------------
        // This will include all meshes and all transforms
        public Vector3 CalculateTotalSizeOfCurrentLayerPrefab(LayerSet layer)
        {
            if (IsLayerEnabled(layer) == false)
                return Vector3.zero;

            GameObject go = GetMainPrefabForLayer(layer);
            if (go == null)
            {
                Debug.Log("CalculateTotalSizeOfCurrentLayerPrefab()  singleVarGO is null");
                return Vector3.zero;
            }
            Vector3 size = MeshUtilitiesAFWB.GetWorldSizeOfGameObject(go, layer, this);
            return size;
        }
        //---------------------
        // Finds the position of the inbetween postsPool between clickPoints
        // Called from a loop of clicked array points [Rebuild()] or from a Click in OnSceneGui
        public void AddNextPostAndInters(Vector3 keyPoint, Vector3 nextKeyPoint, bool interpThisPost = true, bool doRebuild = true)
        {
            interPostPositions.Clear();
            float distance = Vector3.Distance(keyPoint, nextKeyPoint);
            float flatDistance = VectorUtilitiesTCT.GetFlatDistance(keyPoint, nextKeyPoint);
            float interDist = interPostDist;

            if (interpolate == true && distance > interDist && interpThisPost)
            {
                //Debug.Log("Created Inter \n");
                int numSpans = (int)Mathf.Round(distance / interDist);
                float fraction = 1.0f / numSpans;
                float x, dx = (nextKeyPoint.x - keyPoint.x) * fraction;
                float y, dy = (nextKeyPoint.y - keyPoint.y) * fraction;
                float z, dz = (nextKeyPoint.z - keyPoint.z) * fraction;
                actualInterPostDistance = new Vector3(dx, dy, dz).magnitude;

                for (int i = 0; i < numSpans - 1; i++)
                {
                    x = keyPoint.x + (dx * (i + 1));
                    y = keyPoint.y + (dy * (i + 1));
                    z = keyPoint.z + (dz * (i + 1));
                    Vector3 interPostPos = new Vector3(x, y, z);
                    interPostPositions.Add(interPostPos);
                }
                if (keepInterpolatedPostsGrounded)
                    LowerPostsToGround(interPostPositions);
                else
                    RaiseUpToGround(interPostPositions);
                allPostPositions.AddRange(interPostPositions);
            }
            allPostPositions.Add(nextKeyPoint);
            if (doRebuild)
                RebuildFromFinalList();
        }
        void AddToAllPostPositions(Vector3 pos, bool updateUnrandomizedAlso = true)
        {
            allPostPositions.Add(pos);
            if (updateUnrandomizedAlso)
                allPostsPositionsUnrandomized.Add(pos);
        }
        //--------------------
        // Called when the layout is required to change
        // Creates, the interpolated postsPool, the smoothing curve and
        // then calls RebuildFromFinalList where the fence gets put together
        public void ForceRebuildFromClickPoints(LayerSet layerSet = LayerSet.allLayerSet, [CallerMemberName] string caller = null)
        {
            if (clickPoints.Count == 0)
                return;

            //StackTraceLogger.LogCallStack();
            Timer t = new Timer("ForceRebuildFromClickPoints");

            //-- so we don't raycast hit the click markers
            SetIgnoreClickMarkers(true);

            // This creates a list of post positions by interpolating the user clicked points (posts), along with some other prepartion
            // It the calls RebuildFromFinalList() which invokes the building of Posts, Rails, Subposts and Extras

            buildTime = postBuildTime = railBuildTime = subpostBuildTime = extraBuildTime = 0;

            if (clickPoints.Count == 1) // the first post doesn't need anything else
            {
                allPostPositions.Clear();
                allPostsPositionsUnrandomized.Clear();
                keyPoints.Clear();
                keyPoints.AddRange(clickPoints);
                allPostPositions.Add(keyPoints[0]);
                CalculateAllPostDirectionVectors();
                RebuildFromFinalList();
                return;
            }
            DeactivateEntirePoolForLayer(layerSet); // Switch off, but don't delete
            MergeClickPointGaps();
            allPostPositions.Clear();
            keyPoints.Clear();
            keyPoints.AddRange(clickPoints);

            //-- Keypoints are used as a buffer to process and add to allPostPositions, while keeping a copy.

            //-- Add Smoothing Posts to KeyPoints
            if (smooth == true && roundingDistance > 0 && clickPoints.Count > 2)
                keyPoints = MakeSmoothSplineFromClickPoints(); // 

            //-- Add Interpolated allPostPositions from smoothed keyPoints
            addIntersStartPoint = keyPoints[0];
            AddNextPostAndInters(addIntersStartPoint, keyPoints[0], false, false);
            for (int i = 0; i < keyPoints.Count - 1; i++)
            {
                AddNextPostAndInters(keyPoints[i], keyPoints[i + 1], true, false);
            }
            //-- allPostPositions now has all post positions including smoothed and interoplated
            //-- Copy them back to the KeyPoints list ready for randomization
            keyPoints.Clear();
            keyPoints.AddRange(allPostPositions);
            //-- Randomise the Forward/Back Post Spacing of the keyPoints
            RandomisePostSpacing(keyPoints);



            RemoveAllInbetweenPostPositionsFromGaps();

            //   This is the default place where the PostVectors are created
            //====================================================================
            CalculateAllPostDirectionVectors();

            //Store the true positions before any random shifting is added to allPostPositions
            allPostsPositionsUnrandomized = new List<Vector3>(allPostPositions);

            if (allowPostXZShift && allowPostRandomization)
            {
                RandomisePostPositionsXZShift(allPostPositions);
            }

            //ValidateAndUpdatePools();

            if (allPostsPositionsUnrandomized.Count != allPostPositions.Count)
                Debug.Log("allPostsPositionsUnrandomized.Count != allPostPositions.Count  " + allPostsPositionsUnrandomized.Count + "   " + allPostPositions.Count);
            if (allPostsPositionsUnrandomized[0] != allPostPositions[0])
                Debug.Log("(allPostsPositionsUnrandomized[0] != allPostPositions[0]");

            //Check we have enough SingleVariants, one for each post.
            ResizeSingleVariantsLists(allPostPositions.Count);

            RebuildFromFinalList(layerSet);
            centralizeRails = false;

            //RemoveAllCollidersForAllLayers(); ?//??
            //AddBoxCollidersToPostAndRailsForSceneVieweDetection(); // ???
            //CheckCollidersAfterBuild();
            SetIgnoreClickMarkers(false);
            buildTime = t.End(print: false);


        }
        //--------------------------
        public GameObject GetMarkerPost()
        {
            if (markerPost == null)
                markerPost = FindPrefabByNameAndType(PrefabTypeAFWB.postPrefab, "Marker_Post");
            if (markerPost == null)
                markerPost = GetPrefabAtIndexForLayer(0, kPostLayer);
            return markerPost;
        }
        //------------------------
        // This is where the gameobjects actually get built
        // The final list differs from the main clickPoints list in that has now added extra postsPool for interpolating and smoothing
        public void RebuildFromFinalList(LayerSet layer = LayerSet.allLayerSet)
        {
            Timer t = new Timer("RebuildFromFinalList"); t.enabled = false;// Uncomment to time build
            railsATotalTriCount = railsBTotalTriCount = postsTotalTriCount = subPostsTotalTriCount = extrasTotalTriCount = 0;

            ex.extrasBuiltCount = 0;
            ResetNativePrefabScales();//Get the original transform.localScale for current post, singleVarGO etc.

            Vector3 A = Vector3.zero, B, C = Vector3.zero, prevPostPos = Vector3.positiveInfinity; //To test if it's ever set, cant use V3.zero as could be valid position
                                                                                                   //Check if we need to increase the pool svSize before we do any building

            //-- At this point we know the rail pool will be smaller than required for the new num posts, so no need to warn
            ValidateAndUpdatePools(warn: false, update: true);
            SetClickMarkersActiveStatus(showControls);
            gaps.Clear();
            tempMarkers.Clear();
            overlapPostErrors.Clear();
            postsBuiltCount = railABuiltCount = railBBuiltCount = subpostsBuiltCount = subJoinersBuiltCount = 0;

            t.Lap();

            // Crete a new single clean fixed Rail mesh which can be duplicated to make the individual sections.
            // These cannot be pre-packed in to the game objects, otherwise the mesh modifications would compound on every rebuild.
            if (useRailLayer[kRailALayerInt])
                CreateAllPreparedMeshVariations(LayerSet.railALayerSet);
            if (useRailLayer[kRailBLayerInt])
                CreateAllPreparedMeshVariations(LayerSet.railBLayerSet);

            variationModeRailA = VariationMode.sequenced;
            variationModeRailB = VariationMode.sequenced;

            //CheckSinglesLengths(); // Single elements that have had a unique property applied
            ValidateAllSeeds();
            postAndSubpostStringList.Clear();
            postsAndSubpostsCombined.Clear();

            SetGlobalLevelling();

            CalculateAllFinalPrefabSizes(print: false);

            CopyPostSeqInfoToPostBuildInfo();

            //ResetAllPools(); //rebuilds the pools

            //gizmoManager.ClearAll();
            uprightMarkers.Clear();//-- Clear DEbug Markers


            t.Lap();

            //guideFirstPostMarker = null;
            markerPost = GetMarkerPost();
            GameObject railGo = null;
            for (int sectionIndex = 0; sectionIndex < allPostPositions.Count; sectionIndex++)
            {
                PostVector postVector = PostVector.GetPostVectorAtIndex(sectionIndex);
                bool isClickPoint = postVector.IsClickPointNode;


                //=====================================================
                //      CreateMergedPrefabs POSTS (and click-postMarkers if enabled)
                //=====================================================
                A = allPostPositions[sectionIndex];
                SetupPost(sectionIndex, A);  // Build Post nextPost(stackIdx) at position stackIdx. We need to build them even if not used as reference for everything else
                //====== Set Up Yellow Click Markers =======
                if (isClickPoint)
                    SetupClickMarker(postVector);

                //- Make a temporary nodeMarker if postsPool aren't being used so that the user can see the position of the first click
                if (usePostsLayer == false && allPostPositions.Count == 1 && sectionIndex == 0)
                {
                    if (markerPost) guideFirstPostMarker = Instantiate(markerPost, A, Quaternion.identity) as GameObject;
                    guideFirstPostMarker.transform.parent = postsFolder.transform;
                    guideFirstPostMarker.name = "Marker Post - can be deleted";
                }
                //=====================================================
                //      CreateMergedPrefabs Rails, Extras and Subs (check validity & gaps)
                //=====================================================
                if (sectionIndex > 0)
                    prevPostPos = allPostPositions[sectionIndex - 1];
                else
                    prevPostPos = Vector3.zero;
                if (sectionIndex < allPostPositions.Count - 1) //Do the last post separately as it has no nextPos post
                {
                    B = allPostPositions[sectionIndex + 1];
                    if (sectionIndex < allPostPositions.Count - 2)
                        C = allPostPositions[sectionIndex + 2];
                    else
                        C = B;
                    if (A == B)
                    {
                        print("Warning: Posts currPost & nextPost are in identical positions. Enable [Show Move Controls] and delete or move one of them " + sectionIndex + "  " + (sectionIndex + 1));
                        allPostPositions[sectionIndex + 1] += new Vector3(0.1f, 0, 0.01f);
                    }
                    else if (IsBreakPoint(allPostPositions[sectionIndex + 1]) == false || allowGaps == false)
                    {
                        if (useRailLayer[kRailALayerInt] == true && (layer == LayerSet.railALayerSet || layer == LayerSet.allLayerSet))
                            railGo = BuildRailsForSection(prevPostPos, A, B, C, sectionIndex, LayerSet.railALayerSet, GetPreparedMeshVariantsForLayer(LayerSet.railALayerSet));  //---- CreateMergedPrefabs Main Rails ----
                                                                                                                                                                                 //railGo = BuildRailsForSection(prevPostPos, A, B, C, sectionIndex, LayerSet.railALayerSet, GetPreparedMeshVariantsForLayer(LayerSet.railALayerSet));  //---- CreateMergedPrefabs Main Rails ----

                        if (useRailLayer[kRailBLayerInt] == true && (layer == LayerSet.railBLayerSet || layer == LayerSet.allLayerSet))
                            railGo = BuildRailsForSection(prevPostPos, A, B, C, sectionIndex, LayerSet.railBLayerSet, GetPreparedMeshVariantsForLayer(LayerSet.railBLayerSet)); //---- CreateMergedPrefabs Seconday Rails ----
                                                                                                                                                                                //railGo = BuildRailsForSection(prevPostPos, A, B, C, sectionIndex, LayerSet.railBLayerSet, GetPreparedMeshVariantsForLayer(LayerSet.railBLayerSet)); //---- CreateMergedPrefabs Seconday Rails ----

                        if (useSubpostsLayer == true && (layer == LayerSet.subpostLayerSet || layer == LayerSet.allLayerSet))
                            BuildSubposts(A, B, sectionIndex);
                        //-- if the subpostsPool are replicating the postsPool, need to build the last subpost at the last Post
                        if (sectionIndex == allPostPositions.Count - 2 && useSubpostsLayer == true && (subsSpacingMode == SubSpacingMode.postPositionsOnly || subsSpacingMode == SubSpacingMode.nodePositionsOnly || addSubpostAtPostPointAlso == true) &&
                            (layer == LayerSet.subpostLayerSet || layer == LayerSet.allLayerSet))
                            BuildSubposts(B, A, sectionIndex + 1, true);
                    }
                    else
                    {
                        gaps.Add(A);
                        gaps.Add(B);
                    }

                }
                postsBuiltCount++;
            }

            List<Transform> pool = GetPoolForLayer(kRailALayer);

            postAndSubpostStringList.Add($"{postAndSubpostStringList.Count.ToString()} Post[{(allPostPositions.Count - 1).ToString()}]"); //add the last post to the string list)
            postsAndSubpostsCombined.Add(postsPool[allPostPositions.Count - 1]);
            t.Lap("Build Main");
            // Delete the guide nodeMarker once we've got going
            if (guideFirstPostMarker != null && allPostPositions.Count > 1)
            {
                DestroyImmediate(guideFirstPostMarker);
                guideFirstPostMarker = null;
            }
            SetUpClickMarkers();
            RotateAndModifyPostsFinal(); //rotate each post to correctly follow the fence direction, best to do at the end when all directions have been calc'd
                                         //Debug.Log("postVizMarkers.Count = " + db.postVizMarkers.Count);
            t.Lap("RotateAndModifyPostsFinal");
            GlobalLevellingLowerPostsToGround(allPostPositions);
            ex.BuildExtras();// Build the extrasPool last



            //=====  Global Lift. lifts everything off the ground. this should only be used for cloning/layering =======
            if (globalLift > 0.05f || globalLift < -0.05f)
            {
                Transform[] allParts = currentFencesFolder.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allParts)
                {
                    string name = child.gameObject.name;
                    if (name.Contains("PostsGroupedFolder") || name.Contains("RailsAGroupedFolder") || name.Contains("RailsBGroupedFolder") || name.Contains("SubsGroupedFolder") || name.Contains("ExtrasGroupedFolder"))
                    {
                        child.Translate(0, globalLift, 0);
                    }
                }
            }
            t.End(); // Un-comment to time editor rebuild
                     //PrintUtilitiesTCT.PrintList(postAndSubpostStringList, "postAndSubpostStringList", allInOneLine:false);

        }
        //------------------------------------------
#pragma warning disable 0219
#pragma warning disable 0414
        // This the real meat of the thing where the fence rails get assembled. Input positions of two postsPool currPost nextPost, and build rails/walls between them 
        // sectionIndex is the post-to-post section we are currently building (effectively, the same as the post number)
        // **Note**  In sheared mode, rails[x]'stackIdx mesh may be null at this point, as it was chnaged to a buffer mesh, which may have been cleared ready for rebuilding
        // Nope. No plans to refactor this one any further, it's where 90% of work is done and I'm fed up jumping around in the source from Method to Method.
        // And anyway, there hasn't been an issue here since 1983. But if you're a dev trying to mod it, I'll gladly send you a refactored version.
        public GameObject BuildRailsForSection(Vector3 prevPostPos, Vector3 posA, Vector3 posB, Vector3 posC, int sectionIndex, LayerSet layer, List<List<Mesh>> preparedMeshes)
        {//Debug.Log("BuildRailsForSection() \n");
            Timer railTimer = new Timer("BuildRailsForSection");
            int layerIndex = (int)layer;

            float distanceToNextPost = 0, halfRailThickness;
            float alternateHeightDelta = 0;
            float thisRailSpread = railSpread[layerIndex], gap = globalScale.y * gs; // gs = Global Scale
            int spreadMode = railSpreadMode[layerIndex];
            Vector3 nativeScale = nativeRailAScale, railPositionOffset = railAPositionOffset, railRotation = railARotation, P, Q;
            //float railThickness = 0, railMeshLength = 0, railMeshHeight = 0;
            Bounds bounds;
            //float newChangeInHeading = 0;

            Vector3 railScale = railAScale;// the adjustedNativeScale transform of the rail layer

            bool allowRailRand = allowRailRandomization[kRailALayerInt];
            bool allowIndependentSubmeshVariation = false, allowRailHeightVariation = false, allowRandRailRotationVariation = false;
            bool jitterRailVerts = false, rotateFromBase = false;
            List<SeqItem> currSeq = null;
            SourceVariant currVariantForSeqStep = null;
            float minRailHeightVar = 0, maxRailHeightVar = 0;
            VariationMode variationMode = variationModeRailA;
            RandomScope randomScope = RandomScope.all;
            //RandomRecords randRec = railARandRec;
            //List<SourceVariant> nonNullRailVariants = nonNullRailSourceVariants[0];
            List<SourceVariant> rail_Variants = railSourceVariants[0];
            //List<int> railSinglesIdx = railSinglesIndices[0];
            SlopeMode railSlopeMode = slopeMode[kRailALayerInt];
            JointStyle jointStyle = railJointStyle[kRailALayerInt];
            float quantRotProb = quantizeRotProbRailA;
            float quantRotAngle = quantizeRotAngleRailA;
            int quantRotAxis = quantizeRotAxisRailA;
            bool allowQuantRot = allowQuantizedRandomRailARotation;
            RandomSeededValuesAF seeds = railASeeds;
            float chanceOfMissing = chanceOfMissingRailA;
            bool useSingles = useRailSingles[0];
            SinglesContainer singlesContainer = railSinglesContainer[layerIndex];
            int numRailVariants = GetNumSourceVariantsInUseForLayer(layer, incMain: true), numSeqSteps = 0;

            bool allowMirrorX = allowMirroring_X_Rail[layerIndex];
            bool allowMirrorY = allowMirroring_Y_Rail[layerIndex];
            bool allowMirrorZ = allowMirroring_Z_Rail[layerIndex];


            int railPrefabIndex = currentRail_PrefabIndex[layerIndex];
            bool railKeepGrounded = keepRailGrounded[layerIndex];
            int numStackedRailsInThisSet = (int)numStackedRails[layerIndex];
            int numSections = allPostPositions.Count - 1;
            int railBuiltCount = railABuiltCount;
            int numRailsInPool = GetPoolCountForLayer(layer);
            if (numRailsInPool < numSections)
            {
                Debug.Log($"numRailsInPool {numRailsInPool}   <  numSections  {numSections} \n");
                return null;
            }




            useSingles = useRailSingles[layerIndex];
            //nonNullRailVariants = nonNullRailSourceVariants[layerIndex]; //???
            rail_Variants = railSourceVariants[layerIndex];
            railSlopeMode = slopeMode[layerIndex];
            jointStyle = railJointStyle[layerIndex];
            RailOffsetMode offsetMode = railOffsetMode[layerIndex];
            //Vector3 currentVectorDir;

            if (layer == LayerSet.railALayerSet)
            {
                currSeq = optimalSequenceRailA;
                variationMode = variationModeRailA;
                /*if (variationModeRailA == VariationMode.sequenced)
                    currSeq = _SeqItemListRailA;*/
                minRailHeightVar = minRandHeightRailA; maxRailHeightVar = maxRandHeightRailA;
                allowIndependentSubmeshVariation = allowIndependentSubmeshVariationA;
                allowRailHeightVariation = allowHeightVariationRailA;
                allowRandRailRotationVariation = allowRandRailARotationVariation;
                jitterRailVerts = jitterRailAVerts;
                randomScope = railARandomScope;
                rotateFromBase = rotateFromBaseRailA;
            }
            else if (layer == LayerSet.railBLayerSet)
            {
                railRotation = railBRotation;
                railPositionOffset = railBPositionOffset;
                nativeScale = nativeRailBScale;
                railScale = railBScale;
                currSeq = optimalSequenceRailB;
                variationMode = variationModeRailB;

                /*if (variationModeRailB == VariationMode.sequenced)
                    currSeq = SeqItemListRailB;*/

                minRailHeightVar = minRandHeightRailB; maxRailHeightVar = maxRandHeightRailB;
                allowIndependentSubmeshVariation = allowIndependentSubmeshVariationB;
                allowRailHeightVariation = allowHeightVariationRailB;
                allowRandRailRotationVariation = allowRandRailBRotationVariation;
                jitterRailVerts = jitterRailBVerts;
                randomScope = railBRandomScope;
                //randRec = railBRandRec;
                rotateFromBase = rotateFromBaseRailB;
                quantRotProb = quantizeRotProbRailB;
                quantRotAngle = quantizeRotAngleRailB;
                quantRotAxis = quantizeRotAxisRailB;
                allowQuantRot = allowQuantizedRandomRailBRotation;
                seeds = railBSeeds;

                chanceOfMissing = chanceOfMissingRailB;
                railBuiltCount = railBBuiltCount;
            }

            Sequencer sequencer = GetSequencerForLayer(layer);
            bool useRailSeq = sequencer.GetUseSeq();
            numSeqSteps = sequencer.numSteps;
            currSeq = sequencer.seqList;


            if (numStackedRailsInThisSet > 1)
            {
                float spread = 0;
                Vector3 totalSize = prefabMeshWithTransformsSize[layerIndex];
                if (spreadMode == 0)//total mode
                    gap = thisRailSpread / (numStackedRailsInThisSet - 1);
                else
                    gap = thisRailSpread;
            }
            else
                gap = 0;
            gap *= globalScale.y * gs;


            P = posA;
            Q = posB;
            Vector3 currDirectionEuler = VectorUtilitiesTCT.GetRotationAnglesFromDirection(posB, posA);
            Vector3 currDirectionVector = (posB - posA).normalized;
            //currentVectorDir = (posB - posA);
            Vector3 prevDirection = Vector3.zero;
            float currHeading = currDirectionEuler.y, prevHeading = prevDirection.y;
            float horizDistance = Vector3.Distance(P, Q);

            //if (dual)
            //railPositionOffset.z = -railPositionOffset.z;
            bool lastSection = false;
            offsetMode = RailOffsetMode.joined;

            if (offsetMode == RailOffsetMode.joined)
            {
                //   Calculate new positions for Posts A B C based on the railPositionOffset
                Vector3 offsetPosA = posA;
                Vector3 offsetPosB = posB;
                Vector3 offsetPosC = posC;
                Vector3 offsetPrevPost = prevPostPos;

                //if(postVectors.Count <= sectionIndex + 2)
                //Debug.Log($"Not enough PostVectors in BuildRailsForSection()   {postVectors.Count} / {sectionIndex + 2}");
                PostVector postVectorA = postVectors[sectionIndex];
                PostVector postVectorB = postVectorA.GetNext();
                PostVector postVectorC = postVectorB.GetNext();

                PostVector postVectorPrev = postVectorA.GetPrevious();

                //Get dirVectors for A B C
                Vector3 dirAFwd = postVectorA.Forward;
                Vector3 shiftFwd = Vector3.Scale(dirAFwd, railPositionOffset);

                Vector3 dirARight = postVectorA.DirRight;
                Vector3 shiftRight = dirARight * railPositionOffset.z;
                offsetPosA = posA + shiftRight;

                if (sectionIndex + 1 < allPostPositions.Count - 1)
                {
                    Vector3 dirBRight = postVectorB.DirRight;
                    shiftRight = dirBRight * railPositionOffset.z;
                    offsetPosB = posB + shiftRight;
                }
                else
                {
                    lastSection = true;
                    shiftRight = dirARight * railPositionOffset.z;
                    offsetPosB = posB + shiftRight;
                }

                if (sectionIndex + 2 < allPostPositions.Count - 1)
                {
                    Vector3 dirCRight = postVectorC.DirRight;
                    shiftRight = dirCRight * railPositionOffset.z;
                    offsetPosC = posC + shiftRight;
                }
                if (sectionIndex > 0)
                {
                    Vector3 dirPrevRight = postVectorPrev.DirRight;
                    shiftRight = dirPrevRight * railPositionOffset.z;
                    offsetPrevPost = prevPostPos + shiftRight;
                }

                //-- Check if the Offset is non-zero, if so use these as the post positions (but ignoring Y offset)
                if (railPositionOffset.XZGreaterThan(.001f))
                {
                    offsetPosA.y = posA.y; offsetPosB.y = posB.y; offsetPosC.y = posC.y;
                    bool isNode = postVectorA.IsClickPointNode, isNextNode = postVectorB.IsClickPointNode;
                    float currShiftLength = shiftRight.magnitude, newShiftLength = 0;
                    Vector3 newElbowPos = Vector3.zero;
                    if (isNode && sectionIndex > 0)
                    {
                        //Get the extended elbow width
                        Vector2 elbowOffset = postVectorA.CalculateOuterElbowOffset2D(railPositionOffset.z);
                        offsetPosA.x = posA.x + elbowOffset.x;
                        offsetPosA.z = posA.z + elbowOffset.y;

                    }
                    if (isNextNode && lastSection == false)
                    {
                        Vector2 elbowOffset = postVectorB.CalculateOuterElbowOffset2D(railPositionOffset.z);
                        offsetPosB.x = posB.x + elbowOffset.x;
                        offsetPosB.z = posB.z + elbowOffset.y;
                    }
                    posA = offsetPosA;
                    posB = offsetPosB;
                    posC = offsetPosC;
                    prevPostPos = offsetPrevPost;
                    P.y = Q.y = 0;
                }

                P = posA;
                Q = posB;
                horizDistance = Vector3.Distance(P, Q);
                distanceToNextPost = Vector3.Distance(posA, posB);
                // currentVectorDir = (posB - posA);
                //currDirectionVector = (posB - posA).normalized;
                currDirectionEuler = VectorUtilitiesTCT.GetRotationAnglesFromDirection(posB, posA);
                currDirectionVector = (posB - posA).normalized;
                prevDirection = Vector3.zero;
                currHeading = currDirectionEuler.y;
                prevHeading = prevDirection.y;

                /*if (gizmoManager == null)
                {
                    gizmoManager = new GizmoDrawManager(this);
                    Debug.LogWarning("GizmoDrawManager was null in BuildRailsForSection).");
                }*/

                Color col = Color.red;
                if (sectionIndex == 0)
                    col = Color.green;

                //gizmoManager.DrawPost(posA, size: new Vector3(0.3f, 4f, 0.3f), color: col);
                //if (lastSection == true) gizmoManager.DrawPost(posB, size: new Vector3(0.3f, 4f, 0.3f), color: Color.green); 
            }

            //==

            //==========================================================================
            //            Start looping through for each stacked Rail in the section, 
            //==========================================================================
            GameObject thisRail = null;
            for (int stackIdx = 0; stackIdx < numStackedRailsInThisSet; stackIdx++)
            {
                bool omit = false;
                if (SkipChanceOfMissing(sectionIndex, layer, seeds, chanceOfMissing, stackIdx))
                    continue;
                numRailsInPool = CheckAndRepairInvalidRailPool(layer);
                Mesh thisMesh;

                //==========================================
                //       Get Rail GameObject from pool 
                //==========================================
                thisRail = GetFromPoolAndInit(layer, railBuiltCount, stackIdx);
                if (thisRail == null)
                    continue;

                //====================================================================================
                //                                   Variations
                //====================================================================================

                int currVariantIndex = -1, meshIndex = 0, currSeqStepNum = -1;  //currSingleVariantIndex = -1;
                bool isSingle = false, singlesInUse = singlesContainer.GetUseSinglesForLayer(layer, this) && singlesContainer.numInUse > 0;
                SourceVariant sourceVariantForSingle = null;
                SourceVariant currSourceVariant = new SourceVariant(railPrefabs[railPrefabIndex]);//initialise with current singleVarGO in cast something goes wrong with the sourceVariant
                SinglesItem currSingleItem = new SinglesItem();
                SeqItem currSeqStepItem = new SeqItem();


                if (GetUseVariationsForLayer(layer) == true)
                {
                    //==========================================
                    //       Assign Prefab from Singles
                    //==========================================
                    //-- If we're using Singles and there are at least 1 of them , and there is a Single at this singleVarGO section
                    if (singlesInUse)
                    {
                        //return GetSinglesInfo(sectionIndex, layer, ref currVariantForSeqStep, rail_Variants, singlesContainer, ref thisRail, ref currVariantIndex, ref isSingle, ref sourceVariantForSingle, ref meshIndex, ref currSourceVariant, out currSingleItem);

                        currSingleItem = singlesContainer.GetSingleItemForLayer(layer, sectionIndex, af);
                        if (currSingleItem != null)
                        {
                            sourceVariantForSingle = GetSinglesSourceVariantForLayerWithSectionIndex(layer, sectionIndex); // exists so they have independent adjustedNativeScale/position/svRotation
                            if (sourceVariantForSingle != null)
                            {
                                currVariantIndex = currSingleItem.sourceVariantIndex;
                                // Check that this sourceVariant is still in the source list
                                // User may have reset the source list after setting the single, if not, cancel this single
                                /*if (FindFirstUseofGoInVariants(layer, singleVariant.singleVarGO) == -1)
                                {
                                    RemoveSingleVariantFromList(layer, sectionIndex);
                                }*/

                                if (currSingleItem != null && currSingleItem.enabled == true)
                                {
                                    //Hide this singleVarGO
                                    if (currSingleItem.enabled == true)
                                    {
                                        if (layer == LayerSet.railALayerSet) railABuiltCount++;
                                        else if (layer == LayerSet.railBLayerSet) railBBuiltCount++;
                                        thisRail.hideFlags = HideFlags.HideInHierarchy; thisRail.SetActive(false);
                                        return null;
                                    }
                                    // First destroy the existing Rail that we're about to replace with a Single Variant
                                    if (layer == LayerSet.railALayerSet)
                                        DestroyImmediate(railsAPool[railABuiltCount].gameObject);
                                    if (layer == LayerSet.railBLayerSet)
                                        DestroyImmediate(railsBPool[railBBuiltCount].gameObject);

                                    thisRail = Instantiate(rail_Variants[currVariantIndex].Go, Vector3.zero, Quaternion.identity) as GameObject;
                                    FormatInstantiatedGONameWithSectionAndVariation(thisRail, sectionIndex, currVariantIndex);
                                    if (layer == LayerSet.railALayerSet)
                                        railsAPool[railABuiltCount] = thisRail.transform;
                                    if (layer == LayerSet.railBLayerSet)
                                        railsBPool[railBBuiltCount] = thisRail.transform;
                                    isSingle = true;
                                    // Override the sequence for this section
                                    currVariantForSeqStep = sourceVariantForSingle;
                                    currSourceVariant = sourceVariantForSingle;
                                    meshIndex = currVariantIndex;
                                }
                            }
                            else
                                Debug.Log("BuildRailsForSection(): sourceVariantForSingle was null");
                        }
                        else
                            Debug.Log("BuildRailsForSection(): singleVariant was null");
                    }

                    //==========================================
                    //       Assign Prefab from Sequencer
                    //==========================================
                    currSeqStepNum = sectionIndex % numSeqSteps;
                    if (GetUseSequencerForLayer(layer) && isSingle == false)// Find the variation Index by parsing singleVarGO.name
                    {
                        currSeqStepItem = GetSeqItemAtSectionIndexForLayer(sectionIndex, layer);
                        currVariantIndex = currSeqStepItem.sourceVariantIndex;

                        if (variationMode == VariationMode.sequenced)
                        {
                            currSeqStepItem = GetSeqItemAtSectionIndexForLayer(currSeqStepNum, layer);
                            currVariantIndex = currSeqStepItem.sourceVariantIndex;

                            currVariantForSeqStep = rail_Variants[currVariantIndex];

                        }
                        if (currVariantForSeqStep == null || currVariantForSeqStep.Go == null)
                            Debug.Log(("BuildRailsForSection(): currSeqStepItem was null"));
                        else
                            currSourceVariant = currVariantForSeqStep;

                        //-- Name the seq step if needed --
                        if (variationMode == VariationMode.sequenced && thisRail.name.EndsWith("]")) //ending with ']' means we haven't named the seq yet 
                        {
                            string seqIndexString = "";
                            seqIndexString = "_sq" + currSeqStepNum.ToString("00");
                            thisRail.name += seqIndexString;
                        }

                        currVariantForSeqStep.enabled = currSeqStepItem.stepEnabled;
                    }
                }

                //======================================
                //              Meshes
                //======================================
                if (useRailVariations[layerIndex] && useRailSeq == true)
                {
                    meshIndex = FindFirstUseofGoInVariants(layer, currVariantForSeqStep.Go);
                    if (meshIndex == -1)
                    {
                        Debug.LogWarning($"meshIndex = -1  Couldn't find {currVariantForSeqStep.Go.name} in sourceVariants\n");
                        PrintSourceVariantGOsForLayer(layer, activeOnly: true);
                        meshIndex = 0;
                    }
                }
                List<MeshFilter> mfList = MeshUtilitiesAFWB.GetAllMeshFiltersFromGameObject(thisRail);
                Mesh thisModdedMesh = null;
                int meshCount = mfList.Count;
                List<Mesh> preparedMeshGroup = preparedMeshes[meshIndex];
                //-- Duplicate all the meshes in this group with copies from [preparedMeshGroup], ready for any editing
                int triCount = 0;
                for (int m = 0; m < meshCount; m++)
                {
                    MeshUtilitiesAFWB.ReplaceSharedMeshWithDuplicateOfMesh(mfList[m], preparedMeshGroup[m], preparedMeshGroup[m].name);
                    //MeshUtilitiesAFWB.RecalculateNormalsAndTangents(mfList[m].sharedMesh);
                    Mesh mesh = mfList[m].sharedMesh;
                    triCount += mfList[m].sharedMesh.triangles.Length;
                }
                bounds = mfList[0].sharedMesh.bounds;
                float railThickness = bounds.size.z * railScale.z;
                float railMeshLength = bounds.size.x;
                float railMeshHeight = bounds.size.y;

                //====  Set the svSize and offsets for the variations  ====
                Vector3 varPosOffset = Vector3.zero;
                Vector3 varSizeMultiplier = Vector3.one;
                Vector3 varRotation = Vector3.zero;
                if (useRailVariations[layerIndex] && variationMode == VariationMode.sequenced && useRailSeq == true)
                {
                    varPosOffset = currSeq[currSeqStepNum].pos;
                    varSizeMultiplier = currSeq[currSeqStepNum].size;
                    //varSizeMultiplier = currSourceVariant.svSize;
                    varRotation = currSeq[currSeqStepNum].rot;
                }
                if (useRailVariations[layerIndex])
                {
                    // If this has been modified as a single, apply it here
                    if (singlesInUse == true && isSingle && sourceVariantForSingle != null)
                    {
                        varSizeMultiplier = Vector3.Scale(varSizeMultiplier, currSingleItem.size);
                        varPosOffset += currSingleItem.pos;
                        varRotation += currSingleItem.rot;
                    }
                    railRotation = railRotation + varRotation;
                }

                //==== Skip Section for Zero Sized Variation  ====
                if (useRailVariations[layerIndex] == true && useRailSeq == true && (currVariantIndex > 0 || variationMode == VariationMode.sequenced) &&
                (currSourceVariant != null && currSourceVariant.enabled == false || varSizeMultiplier.x == 0 || varSizeMultiplier.y == 0 || varSizeMultiplier.z == 0))
                {
                    if (layer == LayerSet.railALayerSet) railABuiltCount++;
                    else if (layer == LayerSet.railBLayerSet) railBBuiltCount++;
                    thisRail.hideFlags = HideFlags.HideInHierarchy; thisRail.SetActive(false);
                    return null;
                }

                if (layer == LayerSet.railALayerSet)
                    railsATotalTriCount += triCount / 3;
                if (layer == LayerSet.railBLayerSet)
                    railsBTotalTriCount += triCount / 3;

                //=====================================
                //      Rail Rotation From Direction
                //======================================
                GameObject prevRail = null;
                if (layer == LayerSet.railALayerSet && railABuiltCount > numStackedRailsInThisSet)
                    prevRail = railsAPool[railABuiltCount - numStackedRailsInThisSet].gameObject;
                else if (layer == LayerSet.railBLayerSet && railBBuiltCount > numStackedRailsInThisSet)
                    prevRail = railsBPool[railBBuiltCount - numStackedRailsInThisSet].gameObject;

                //====================================
                //      Simple Non-Overlap Version
                //====================================
                if (sectionIndex == 0 || jointStyle != JointStyle.overlap || prevRail == null)
                {
                    thisRail.transform.Rotate(new Vector3(0, -90, 0));// because we want the length side to be considered 'forward'
                    thisRail.transform.Rotate(new Vector3(0, currDirectionEuler.y, 0));
                    //Debug.Log("simple forward " + thisGO.transform.svRotation.eulerAngles + "\n");

                    //-- Position basically in the world
                    thisRail.transform.position = posA + new Vector3(0, (gap * stackIdx) + alternateHeightDelta, 0);


                    //====================================
                    //      Transform Position Offset 
                    //====================================
                    if (offsetMode == RailOffsetMode.basic)
                        thisRail.transform.Translate(railPositionOffset.x, 0, railPositionOffset.z);
                    else if (offsetMode == RailOffsetMode.joined)
                    {
                        // Do nothing for now, as the new virtual post positions (offsetPosA B C) already account for this position offset
                    }
                }
                else
                {
                    prevDirection = VectorUtilitiesTCT.GetRotationAnglesFromDirection(posA, prevPostPos);
                    prevHeading = prevDirection.y;
                    halfRailThickness = railThickness * 0.5f;
                    currDirectionEuler = VectorUtilitiesTCT.GetRotationAnglesFromDirection(posB, posA);
                    currHeading = currDirectionEuler.y;
                    float newChangeInHeading = currHeading - prevHeading;
                    newPivotPoint = posA; // set initially to the primary Post point
                    Vector3 prevVectorDir = (posA - prevPostPos).normalized;
                    Vector3 orthogonalPreviousDirection = Vector3.zero;
                    if (Mathf.Abs(newChangeInHeading) < 1f) { }//to do
                    else
                    {
                        orthogonalPreviousDirection = (-1 * (Quaternion.AngleAxis(90, Vector3.up) * prevVectorDir)).normalized;
                        float sine = Mathf.Sin((90 - newChangeInHeading) * Mathf.Deg2Rad);
                        float orthoScale = halfRailThickness - (sine * halfRailThickness);
                        Vector3 currExtraVector = orthogonalPreviousDirection * orthoScale;

                        if ((newChangeInHeading >= 0 && newChangeInHeading < 90) || (newChangeInHeading <= -270 && newChangeInHeading > -360))
                        {
                            newPivotPoint += currExtraVector;
                        }
                        else if ((newChangeInHeading >= 90 && newChangeInHeading < 180) || (newChangeInHeading <= -180 && newChangeInHeading > -270))
                        {
                            newPivotPoint += currExtraVector;
                            newPivotPoint -= orthogonalPreviousDirection * railThickness;
                        }
                        else if ((newChangeInHeading >= 180 && newChangeInHeading < 270) || (newChangeInHeading <= -90 && newChangeInHeading > -180))
                        {
                            orthogonalPreviousDirection *= -1;
                            sine *= -1;
                            orthoScale = halfRailThickness - (sine * halfRailThickness);
                            currExtraVector = orthogonalPreviousDirection * orthoScale;
                            newPivotPoint -= currExtraVector;
                        }
                        else if ((newChangeInHeading > 270 && newChangeInHeading < 360) || (newChangeInHeading < 0 && newChangeInHeading > -90))
                        {
                            orthogonalPreviousDirection *= -1;
                            currExtraVector = orthogonalPreviousDirection * orthoScale;
                            newPivotPoint += currExtraVector;
                        }
                        //Scale the previous singleVarGO to match the new calculation
                        float cosine = Mathf.Cos((90 - newChangeInHeading) * Mathf.Deg2Rad);
                        float adjacentSize = cosine * halfRailThickness;
                        Vector3 adjacentExtraVector = -prevRail.transform.right * adjacentSize;
                        float prevRailRealLength = railMeshLength * prevRail.transform.localScale.x;
                        float newPrevRailLength = prevRailRealLength + adjacentExtraVector.magnitude;
                        float prevRailLengthScalar = newPrevRailLength / prevRailRealLength;
                        Vector3 newPrevRailScale = Vector3.Scale(prevRail.transform.localScale, new Vector3(prevRailLengthScalar, 1, 1));
                        prevRail.transform.localScale = newPrevRailScale;
                    }

                    thisRail.transform.Rotate(new Vector3(0, -90, 0));// because we want the length side to be considered 'forward'
                    Vector3 newEulerDirection = VectorUtilitiesTCT.GetRotationAnglesFromDirection(posB, newPivotPoint);
                    thisRail.transform.RotateAround(newPivotPoint, Vector3.up, newEulerDirection.y);

                    distanceToNextPost = Vector3.Distance(newPivotPoint, posB);

                    //-- Position - Use Translate for x & z to keep it local relative
                    thisRail.transform.position = newPivotPoint + new Vector3(0, (gap * stackIdx) + alternateHeightDelta, 0);
                    if (offsetMode == RailOffsetMode.basic)
                        thisRail.transform.Translate(railPositionOffset.x, 0, railPositionOffset.z);
                    else if (offsetMode == RailOffsetMode.joined)
                    {
                        // Do nothing for now, as the new virtual post positions (offsetPosA B C) already account for this position offset
                    }
                }
                float sectionInclineAngle = -currDirectionEuler.x;


                //===========================================================
                //         Rotate For Slope Incline
                //===========================================================
                // Remember. transform.Rotate() rotates around pivot, not rCentre
                // Also, in Shear mode, there is NO incline svRotation as the mesh is just sheared upwards, its direction remains (x, 0, z)
                if (railSlopeMode == SlopeMode.slope && sectionInclineAngle != 0)
                {
                    thisRail.transform.Rotate(new Vector3(0, 0, sectionInclineAngle)); //Incline. (z and x are swapped because we consider the length of the fence to be 'forward')
                }

                //=================================================
                //     Rail Height Variation changed v3.5 3/2/22
                //=================================================
                float randHeightScale = 1;

                bool randomScopeIsValid = true;
                if (useRailVariations[layerIndex] == true)
                {
                    randomScopeIsValid = false;
                    if (randomScope == 0 && currVariantIndex == 0) // main only && is main
                        randomScopeIsValid = true;
                    else if (randomScope == RandomScope.variations && currVariantIndex > 0)
                        randomScopeIsValid = true;//variations only and is variation
                    else if (randomScope == RandomScope.all)
                        randomScopeIsValid = true;//main and variations, so is always true
                }

                if (allowRailRand && allowRailHeightVariation && randomScopeIsValid)
                {
                    randHeightScale = UnityEngine.Random.Range(minRailHeightVar, maxRailHeightVar);
                    randHeightScale = seeds.rHeight[sectionIndex];
                    //Debug.Log(railASeeds.height + "   " + randHeightScale + "\n");
                }
                float cumulativeHeightScaling = globalScale.y * railScale.y * varSizeMultiplier.y * randHeightScale;

                if (jointStyle == JointStyle.overlap)
                {
                    cumulativeHeightScaling += sectionIndex * 0.0001f; // prevents z-fighting on overlaps
                }

                Vector3 adjustedNativeScale = nativeScale;
                if (railSlopeMode != SlopeMode.shear)//don't adjustedNativeScale raillSize.y if sheared, as the vertices are explicitly set instead
                    adjustedNativeScale.y *= cumulativeHeightScaling;
                //-- If it'sectionIndex a panel type but NOT sheared, adjustedNativeScale it with the fence
                else if ((railPrefabs[railPrefabIndex].name.EndsWith("_Panel_Rail") || railPrefabs[railPrefabIndex].name.EndsWith("_Panel")) && railSlopeMode != SlopeMode.shear)
                    adjustedNativeScale.y *= cumulativeHeightScaling;
                //-- It'sectionIndex a regular sheared
                else if (railSlopeMode == SlopeMode.shear)
                    adjustedNativeScale.y *= cumulativeHeightScaling;

                //-- User transform height
                float cumulativeHeightOffset = railMeshHeight * globalScale.y * varSizeMultiplier.y * (railPositionOffset.y + varPosOffset.y);
                if (allFollowPostRaiseLower)
                    cumulativeHeightOffset += postHeightOffset;

                if (railKeepGrounded == true)
                {
                    cumulativeHeightOffset = cumulativeHeightScaling / 2;
                }
                else if (railKeepGrounded == false)
                {
                    cumulativeHeightOffset = cumulativeHeightScaling / 2;
                    cumulativeHeightOffset += railPositionOffset.y;
                    if (allFollowPostRaiseLower)
                        cumulativeHeightOffset += postHeightOffset;
                }
                if (useRailVariations[layerIndex] == true)
                {
                    if ((variationMode == VariationMode.optimal || variationMode == VariationMode.random) && currVariantIndex > 0)
                        cumulativeHeightOffset += varPosOffset.y;
                    else if (variationMode == VariationMode.sequenced)
                        cumulativeHeightOffset += varPosOffset.y;
                }

                thisRail.transform.Translate(0, cumulativeHeightOffset, 0);
                float heightAdjustmentForNonNormalizedMeshes = cumulativeHeightScaling * (railMeshHeight - 1.0f) / 2.0f;
                thisRail.transform.Translate(0, heightAdjustmentForNonNormalizedMeshes, 0);
                if (railSlopeMode == SlopeMode.step)
                {
                    if (posB.y > posA.y)
                        thisRail.transform.position += new Vector3(0, posB.y - posA.y, 0);
                }

                //===========================================
                //              Extend Ends
                //============================================

                if (extendRailEnds[layerIndex] == true && (sectionIndex == 0 || sectionIndex == allPostPositions.Count - 2))
                {
                    float oldLength = distanceToNextPost;
                    float desiredLength = distanceToNextPost + endExtensionLength[layerIndex];
                    float xScaling = (desiredLength / oldLength) - 1;

                    //calculate the direction of this section
                    Vector3 thisSectionDir = (posB - posA).normalized;
                    //create a vecor3 that is the same length as the extra length, and in the same direction as this section
                    Vector3 extraLengthVector = thisSectionDir * endExtensionLength[layerIndex];
                    //move this section along by the extra length
                    if (sectionIndex == 0)
                        thisRail.transform.position -= extraLengthVector;
                    thisRail.transform.localScale += new Vector3(xScaling, 0, 0);
                }


                //===========================================
                //              Rail Scale
                //============================================
                bool useMeshDeformation = true; //TODO. currPost little pointless in v3.5, but preparing for v4.1
                                                //--- X ---
                float gainInLength = 0;
                if (railSlopeMode == SlopeMode.slope)// real length, sectionIndex.e. hypotenuse
                {
                    adjustedNativeScale.x *= (distanceToNextPost / 3.0f) * railScale.x * varSizeMultiplier.x;
                    gainInLength = (distanceToNextPost * railScale.x) - distanceToNextPost;
                }
                else if (railSlopeMode == SlopeMode.shear)
                {
                    if ((jointStyle == JointStyle.mitre) || VectorUtilitiesTCT.GetAngleFromZero(sectionInclineAngle) > 16) // this can be tuned, tradeoff for smooth overlaps on level ground, or smooth joints on steep slopes
                    {
                        adjustedNativeScale.x *= (horizDistance / 3.0f) * railScale.x * varSizeMultiplier.x; //prevRailLengthScalar
                        gainInLength = (horizDistance * railScale.x) - horizDistance;
                    }
                    else
                    {

                        adjustedNativeScale.x *= (distanceToNextPost / 3.0f) * railScale.x * varSizeMultiplier.x;
                        gainInLength = (distanceToNextPost * railScale.x) - distanceToNextPost;
                    }
                }
                else if (railSlopeMode == SlopeMode.step)
                {
                    adjustedNativeScale.x *= (horizDistance / 3.0f) * railScale.x * varSizeMultiplier.x; // distanceToNextPost along ground sectionIndex.e. adjacent
                    gainInLength = (horizDistance * railScale.x) - horizDistance;
                }
                if (railScale.x != 1.0f)
                    thisRail.transform.Translate(gainInLength / 2, 0, 0);
                //--- Z ---
                adjustedNativeScale.z *= railScale.z * globalScale.z * varSizeMultiplier.z;

                //adjustedNativeScale.x *= newScale;

                //-- Apply scaling ----
                if (useMeshDeformation == false)
                {
                    thisRail.transform.localScale = adjustedNativeScale;

                }
                else
                    MeshUtilitiesAFWB.ScaleAllMeshesInGO(thisRail, adjustedNativeScale);

                //=============   Calculate Centre   =================
                float totalQuantRotAmount = 0;
                Vector3 axisRight = currDirectionVector;
                Vector3 f = thisRail.transform.forward;
                Vector3 railUp = thisRail.transform.up;
                Vector3 g = thisRail.transform.TransformDirection(thisRail.transform.forward);
                Vector3 railFwd = new Vector3(-f.z, f.y, f.x); //swap x & z
                Vector3 r = thisRail.transform.right;
                Vector3 railRight = new Vector3(r.z, r.y, -r.x);
                Vector3 railMeshSize = MeshUtilitiesAFWB.GetMeshSize(thisRail);
                Vector3 railPos = thisRail.transform.localPosition;
                Vector3 rCentre = railPos;
                rCentre.x += railFwd.x * railMeshSize.x / 2f * thisRail.transform.localScale.x;
                rCentre.y += (-thisRail.transform.right.y) * railMeshSize.x / 2f * thisRail.transform.localScale.y;

                //rCentre.y = (thisGO.transform.localScale.y * railMeshSize.y) / 2f;
                rCentre.z += (-thisRail.transform.right.z) * (railMeshSize.x / 2f) * thisRail.transform.localScale.x;
                //Debug.Log("pos " + railPos + "   fwd " + railFwd + "   up " + railUp + "   right " + thisGO.transform.right + "   rCentre " + rCentre + "   meshSize " + railMeshSize + "\n");

                // rCentre between postsPool
                Vector3 postsCentre = posA + ((posB - posA)) / 2;
                rCentre = new Vector3(postsCentre.x, rCentre.y, postsCentre.z); ;

                //===========================================
                //              Rail Position
                //============================================
                if (jointStyle != JointStyle.mitre) // to stop possible z-fighting on overlaps 
                    alternateHeightDelta = (sectionIndex % 2) * 0.001f;


                //============== Move for Var if needed  ==============
                if (useRailVariations[layerIndex])
                    thisRail.transform.Translate(new Vector3(varPosOffset.x * gs, 0, varPosOffset.z * gs));

                Vector3 railCentre = CalculateCentreOfRail(thisRail);



                //=====================================================
                //           Quantized & Small Random Rotations
                //=====================================================
                Vector3 totalQuantRot = Vector3.zero; // only so we can calculate the total rotations later if needed or for debug
                                                      //Vector3 totalSmallRandRot = Vector3.zero, smallRandRot = Vector3.zero;
                Vector3 totalUserModdedRotation = Vector3.zero;
                if (allowRailRand)
                    totalQuantRot = RandomRailRotations(sectionIndex, allowRailRand, allowRandRailRotationVariation, quantRotProb, quantRotAngle,
                    quantRotAxis, allowQuantRot, seeds, stackIdx, thisRail, randomScopeIsValid, totalQuantRotAmount, rCentre, railCentre, totalQuantRot);
                totalUserModdedRotation += totalQuantRot;

                //=====================================================
                //        Rail Rotation from User Settings
                //=====================================================
                // we have to invert some axis if the rail has been flipped, mirrored or turned 180
                Vector3 adjustedUserRotForFlipsAnd180 = railRotation;
                if (totalQuantRot.y == 180)
                {
                    adjustedUserRotForFlipsAnd180.x *= -1;
                    adjustedUserRotForFlipsAnd180.z *= -1;
                }
                if (railRotation.x != 0)
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.right, adjustedUserRotForFlipsAnd180.x);
                if (railRotation.y != 0)
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.up, adjustedUserRotForFlipsAnd180.y);
                if (railRotation.z != 0)
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.forward, adjustedUserRotForFlipsAnd180.z);

                totalUserModdedRotation += adjustedUserRotForFlipsAnd180;

                if (useMeshDeformation)//TODO. currPost little reduntant in v4, but preparing for Version 4.1
                {
                    //unity order is zxy
                    // undo all user rots
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.forward, -totalUserModdedRotation.z);
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.up, -totalUserModdedRotation.y);
                    if (totalQuantRot.y != 180)
                        thisRail.transform.RotateAround(rCentre, thisRail.transform.right, -totalUserModdedRotation.x);
                    else
                        thisRail.transform.RotateAround(rCentre, thisRail.transform.right, totalUserModdedRotation.x);

                    for (int m = 0; m < meshCount; m++)
                    {
                        thisModdedMesh = mfList[m].sharedMesh;


                        if (VectorUtilitiesTCT.GetMaxAbsVector3Element(totalUserModdedRotation) >= 0.1f)
                        {
                            //Pivot must be at centre for normal rotations to work
                            Vector3 shift = MeshUtilitiesAFWB.RecentreMesh(thisModdedMesh);

                            MeshUtilitiesAFWB.RotateMeshAndNormals(thisModdedMesh, new Vector3(0, 0, totalUserModdedRotation.z), recentre: false);
                            MeshUtilitiesAFWB.RotateMeshAndNormals(thisModdedMesh, new Vector3(totalUserModdedRotation.x, 0, 0), recentre: false);
                            MeshUtilitiesAFWB.RotateMeshAndNormals(thisModdedMesh, new Vector3(0, totalUserModdedRotation.y, 0), recentre: false);

                            // Put the pivot back
                            MeshUtilitiesAFWB.TranslateMesh(thisModdedMesh, -shift);

                            thisModdedMesh.RecalculateTangents();
                        }
                    }
                }

                //=====================================
                //      Mirror & Invert Variations
                //=====================================
                bool backToFront = false, mirrorZ = false, invert = false;
                if (useRailVariations[layerIndex] && currSourceVariant != null && (useRailSeq || useSingles))
                {
                    if (variationMode == VariationMode.sequenced)
                    {
                        backToFront = currSeqStepItem.backToFront;
                        mirrorZ = currSeqStepItem.mirrorZ;
                        invert = currSeqStepItem.invert;
                    }
                    for (int m = 0; m < meshCount; m++)
                    {
                        thisModdedMesh = mfList[m].sharedMesh;
                        if (m > 0 && allowIndependentSubmeshVariation)
                        {
                            backToFront = (currSeqStepItem.backToFront ? 1f : 0f) > UnityEngine.Random.value;
                            mirrorZ = (currSeqStepItem.mirrorZ ? 1f : 0f) > UnityEngine.Random.value;
                            invert = (currSeqStepItem.invert ? 1f : 0f) > UnityEngine.Random.value;
                        }
                        /*if (mirrorZ)
                        {
                            thisModdedMesh = MeshUtilitiesAFWB.ScaleMesh(thisModdedMesh, new Vector3(-1, 1, 1), adjustForPivot: true);
                            thisModdedMesh = MeshUtilitiesAFWB.ReverseNormals(thisModdedMesh);
                            thisModdedMesh.RecalculateNormals();
                            thisModdedMesh.RecalculateTangents();
                        }
                        if (invert)
                        {
                            thisModdedMesh = MeshUtilitiesAFWB.ScaleMesh(thisModdedMesh, new Vector3(1, -1, 1));
                            thisModdedMesh = MeshUtilitiesAFWB.ReverseNormals(thisModdedMesh);
                            thisModdedMesh.RecalculateNormals();
                            thisModdedMesh.RecalculateTangents();
                        }
                        if (backToFront)
                        {
                            thisModdedMesh = MeshUtilitiesAFWB.ScaleMesh(thisModdedMesh, new Vector3(1, 1, -1));
                            thisModdedMesh = MeshUtilitiesAFWB.ReverseNormals(thisModdedMesh);
                            thisModdedMesh.RecalculateNormals();
                            thisModdedMesh.RecalculateTangents();
                        }*/
                    }
                }
                if (allowMirrorX && (sectionIndex + 1) % mirrorXFreqRail[layerIndex] == 0)
                {
                    thisModdedMesh = MeshUtilitiesAFWB.ScaleMesh2(thisModdedMesh, new Vector3(-1, 1, 1));
                    //-- This is necessary otherwise the mesh will be to the left, and hidden by the previous rail
                    thisModdedMesh = MeshUtilitiesAFWB.SetRailMeshPivotToLeftCentre(thisModdedMesh);
                }
                if (allowMirrorY && (sectionIndex + 1) % mirrorYFreqRail[layerIndex] == 0)
                {
                    thisModdedMesh = MeshUtilitiesAFWB.ScaleMesh2(thisModdedMesh, new Vector3(1, -1, 1));
                }
                if (allowMirrorZ && (sectionIndex + 1) % mirrorZFreqRail[layerIndex] == 0)
                {
                    thisModdedMesh = MeshUtilitiesAFWB.ScaleMesh2(thisModdedMesh, new Vector3(1, 1, -1));
                }

                //-- Omit rails that would intersect with ground/other objects(Hide Colliding Rails) 
                omit = OmitBuriedRails(currDirectionVector, distanceToNextPost, railScale, omit, thisRail);
                if (omit == true)
                    return null;

                //===================================
                //         Shear Mesh
                //===================================
                float absRotX = Mathf.Abs(totalUserModdedRotation.x);
                float absRotY = Mathf.Abs(totalUserModdedRotation.y);
                bool yRotLessThan30 = (absRotY <= 30 || absRotY >= 120 && absRotY <= 210 || absRotY >= 330);
                // we don't want to shear for extreme rotations, so just set the height
                if (railSlopeMode == SlopeMode.shear && yRotLessThan30 == true)
                    thisModdedMesh = ShearRailMesh(thisModdedMesh, posA, posB, nativeScale, railScale, thisRail, mfList, adjustedNativeScale,
                        useMeshDeformation);

                //===================================
                //         Mitre Mesh
                //===================================
                // Don't attempt to mitre panels that are rotated more than 10 on the Y axis
                if (jointStyle == JointStyle.mitre && Mathf.Abs(railRotation.y) <= 7) //TODO
                    MiterRailMesh(ref thisModdedMesh, prevPostPos, posA, posB, posC, sectionIndex, railScale, horizDistance, adjustedNativeScale, useMeshDeformation);
                //MiterRailMeshOld(prevPostPos, posA, posB, posC, sectionIndex, railScale, horizDistance, ref thisModdedMesh, adjustedNativeScale, useMeshDeformation);

                //==================================================================================
                //  Colliders   Make/adjustedNativeScale x on first singleVarGO in the stack & remove on others
                //==================================================================================
                Vector3 centreColliderPos = (posA + posB) / 2;
                Collider collider = CreateColliderForLayer(thisRail, centreColliderPos, layer);

                thisRail.isStatic = usingStaticBatching;
                railBuiltCount++;
                if (layer == LayerSet.railALayerSet)
                    railABuiltCount++;
                else if (layer == LayerSet.railBLayerSet)
                    railBBuiltCount++;

                //====== Organize into subfolders so we can combine for drawcalls, but don't hit the mesh combine limit of 65k ==========
                int numRailsAFolders = (railABuiltCount / objectsPerFolder) + 1;
                int numRailsBFolders = (railBBuiltCount / objectsPerFolder) + 1;

                string railsDividedFolderName = "";
                if (layer == LayerSet.railALayerSet)
                    railsDividedFolderName = "RailsAGroupedFolder" + (numRailsAFolders - 1);
                else if (layer == LayerSet.railBLayerSet)
                    railsDividedFolderName = "RailsBGroupedFolder" + (numRailsBFolders - 1);


                GameObject railsDividedFolder = GameObject.Find("Current Fences Folder/Rails/" + railsDividedFolderName);
                if (railsDividedFolder == null)
                {
                    railsDividedFolder = new GameObject(railsDividedFolderName);
                    railsDividedFolder.transform.parent = railsFolder.transform;
                    railsDividedFolder.transform.localPosition = Vector3.zero;
                    if (addCombineScripts)
                    {
                        CombineChildrenPlus combineChildren = railsDividedFolder.AddComponent<CombineChildrenPlus>();
                        if (combineChildren != null)
                            combineChildren.combineAtStart = true;
                    }
                }
                thisRail.transform.parent = railsDividedFolder.transform;
            }
            railBuildTime += railTimer.End(print: false);
            return thisRail;
        }

        /*private GameObject GetSinglesInfo(int sectionIndex, LayerSet layer, ref SourceVariant currVariantForSeqStep, List<SourceVariant> rail_Variants, SinglesContainer singlesContainer, ref GameObject thisRail, ref int currVariantIndex, ref bool isSingle, ref SourceVariant sourceVariantForSingle, ref int meshIndex, ref SourceVariant currSourceVariant, out SinglesItem currSingleItem)
        {
            currSingleItem = singlesContainer.GetSingleItemForLayer(layer, sectionIndex, af);
            if (currSingleItem != null)
            {
                sourceVariantForSingle = GetSinglesSourceVariantForLayerWithSectionIndex(layer, sectionIndex); // exists so they have independent adjustedNativeScale/position/svRotation
                if (sourceVariantForSingle != null)
                {
                    currVariantIndex = currSingleItem.sourceVariantIndex;
     
                    if (currSingleItem != null && currSingleItem.enabled == true)
                    {
                        //Hide this singleVarGO
                        if (currSingleItem.enabled == true)
                        {
                            if (layer == LayerSet.railALayerSet) railABuiltCount++;
                            else if (layer == LayerSet.railBLayerSet) railBBuiltCount++;
                            thisRail.hideFlags = HideFlags.HideInHierarchy; thisRail.SetActive(false);
                            return null;
                        }
                        // First destroy the existing Rail that we're about to replace with a Single Variant
                        if (layer == LayerSet.railALayerSet)
                            DestroyImmediate(railsAPool[railABuiltCount].gameObject);
                        if (layer == LayerSet.railBLayerSet)
                            DestroyImmediate(railsBPool[railBBuiltCount].gameObject);

                        thisRail = Instantiate(rail_Variants[currVariantIndex].Go, Vector3.zero, Quaternion.identity) as GameObject;
                        FormatInstantiatedGONameWithSectionAndVariation(thisRail, sectionIndex, currVariantIndex);
                        if (layer == LayerSet.railALayerSet)
                            railsAPool[railABuiltCount] = thisRail.transform;
                        if (layer == LayerSet.railBLayerSet)
                            railsBPool[railBBuiltCount] = thisRail.transform;
                        isSingle = true;
                        // Override the sequence for this section
                        currVariantForSeqStep = sourceVariantForSingle;
                        currSourceVariant = sourceVariantForSingle;
                        meshIndex = currVariantIndex;
                    }
                }
                else
                    Debug.Log("BuildRailsForSection(): sourceVariantForSingle was null");
            }
            else
                Debug.Log("BuildRailsForSection(): singleVariant was null");
        }*/

        //------------------------------------------
        private GameObject GetFromPoolAndInit(LayerSet layer, int railBuiltCount, int sectionIndex)
        {
            GameObject railGo = GetGOFromPoolAtIndexForLayer(railBuiltCount, layer);
            Mesh thisRailMesh = MeshUtilitiesAFWB.GetFirstMeshInGameObject(railGo, layer);
            if (thisRailMesh == null)
            {
                Debug.LogWarning($"BuildRailsForSection(): sectionIndex: {sectionIndex}   thisRailMesh was null for layer {GetLayerNameAsString(layer)} \n");
                return null;
            }
            if (railGo == null)
            {
                Debug.LogWarning($"Missing Rail sectionIndex: {sectionIndex} was null  for {GetLayerNameAsString(layer)} Have you deleted one?");
                return null;
            }

            railGo.hideFlags = HideFlags.None;
            railGo.SetActive(true);
            railGo.transform.rotation = Quaternion.identity; // make sure it'sectionIndex reset before moving position
            railGo.transform.position = Vector3.zero;
            railGo.transform.localScale = Vector3.one;
            return railGo;
        }
        /*private bool AssignAndSetupGameObjectFromPool(LayerSet layer, int railBuiltCount, int stackIdx, out GameObject railGo, out Mesh mesh)
         {
             //==========================================
             //          Get Rail GameObject from pool 
             //==========================================
             railGo = GetGOFromPoolAtIndexForLayer(railBuiltCount, layer);
             mesh = MeshUtilitiesAFWB.GetFirstMeshInGameObject(railGo, layer);
             if (mesh == null)
             {
                 Debug.LogWarning($"BuildRailsForSection(): sectionIndex: {stackIdx}   thisRailMesh was null for layer {GetLayerNameAsString(layer)} \n");
                 return false;
             }
             if (railGo == null)
             {
                 Debug.LogWarning($"Missing Rail sectionIndex: {stackIdx} was null  for {GetLayerNameAsString(layer)} Have you deleted one?");
                 return false;
             }
             railGo.hideFlags = HideFlags.None;
             railGo.SetActive(true);
             railGo.transform.rotation = Quaternion.identity; // make sure it'stackIdx reset before moving position
             railGo.transform.position = Vector3.zero;
             railGo.transform.localScale = Vector3.one;
             return true;
         }*/
        //==================================================================================
        private void MiterRailMeshOld(Vector3 prevPostPos, Vector3 posA, Vector3 posB, Vector3 posC, int sectionIndex, Vector3 railScale,
            float horizDistance, ref Mesh thisModdedMesh, Vector3 adjustedNativeScale, bool useMeshDeformation)
        {
            bool isFirstSection = sectionIndex == 0;
            bool isLastSection = false;
            if (closeLoop == false && sectionIndex == allPostPositions.Count - 2)
                isLastSection = true;
            if (closeLoop == true && sectionIndex == allPostPositions.Count - 2)
                isLastSection = true;

            Vector3 currDirection = Quaternion.LookRotation(posB - posA).eulerAngles;
            float currHeading = currDirection.y;

            Vector3 prevDirection = Quaternion.LookRotation(posA - prevPostPos).eulerAngles;
            if (isFirstSection)
                prevDirection = currDirection;
            float prevHeading = prevDirection.y;

            Vector3 nextDirection = Vector3.zero;
            float nextHeading = 0;
            if (isLastSection == false)
            {
                nextDirection = Quaternion.LookRotation(posC - posB).eulerAngles;
                nextHeading = nextDirection.y;
            }

            float incomingAngle = currHeading - prevHeading;
            if (incomingAngle < 0)
                incomingAngle += 360;

            float outgoingAngle = nextHeading - currHeading;
            if (outgoingAngle < 0)
                outgoingAngle += 360;
            if (isLastSection) //this is the last section
                outgoingAngle = 0;

            if (closeLoop == true) // there is a next panel, or we're looping back to the first
            {
                if (isFirstSection == true)
                {
                    // We need to set posC to the first post to loop
                    Vector3 penultimatePostPos = allPostPositions[allPostPositions.Count - 2];
                    Vector3 lastPostPos = allPostPositions[0]; // as we know the last is same as for first
                    prevDirection = Quaternion.LookRotation(lastPostPos - penultimatePostPos).eulerAngles;
                    prevHeading = prevDirection.y;
                }
                else if (isLastSection == true)
                {
                    Vector3 postPos0 = allPostPositions[0];
                    Vector3 postPos1 = allPostPositions[1];
                    nextDirection = Quaternion.LookRotation(postPos1 - postPos0).eulerAngles;
                    nextHeading = nextDirection.y;
                }
                incomingAngle = currHeading - prevHeading;
                outgoingAngle = nextHeading - currHeading;

                if (incomingAngle < 0)
                    incomingAngle += 360;
                if (outgoingAngle < 0)
                    outgoingAngle += 360;
            }

            float widthScaling = 1;
            if (useMeshDeformation == false)
                widthScaling = adjustedNativeScale.z;

            Vector3 meshSize = thisModdedMesh.bounds.size;
            float meshLength = meshSize.x;
            float meshWidth = meshSize.z;
            float actualLength = meshLength * (1 / 1) * railScale.x;
            if (useMeshDeformation == false)
                actualLength = meshLength * (horizDistance / DEFAULT_RAIL_LENGTH) * railScale.x;

            float meshLengthScaling = actualLength / meshLength;
            float halfW = meshWidth / 2.0f;
            if (useMeshDeformation == false)
                halfW = meshWidth * widthScaling / 2.0f;

            // Bounding box points (anti-clockwise from bottom right
            Vector3 p0 = new Vector3(0, 0, halfW);
            Vector3 p1 = new Vector3(meshLength, 0, halfW);
            Vector3 p2 = new Vector3(meshLength, 0, -halfW);
            Vector3 p3 = new Vector3(0, 0, -halfW);

            float inMiterAngle = incomingAngle / 2 * Mathf.Deg2Rad;
            float outMiterAngle = outgoingAngle / 2 * Mathf.Deg2Rad;

            float oppIn = Mathf.Tan(inMiterAngle) * halfW; // this is the shift in x position (was * w)
            float oppOut = Mathf.Tan(outMiterAngle) * halfW;
            oppIn /= meshLengthScaling;
            oppOut /= meshLengthScaling;

            p0 += new Vector3(oppIn, 0, 0);
            p1 += new Vector3(-oppOut, 0, 0);
            p2 += new Vector3(oppOut, 0, 0);
            p3 += new Vector3(-oppIn, 0, 0);

            float len0 = p1.x - p0.x; // the length of side 0 (the side that contains p0)
            float len1 = p2.x - p3.x;
            float scale0 = len0 / meshLength; //the adjustedNativeScale of side 0
            float scale1 = len1 / meshLength; //the adjustedNativeScale of side 1
            float maxScale = scale0 > scale1 ? scale0 : scale1;

            Vector3 meshCenter = thisModdedMesh.bounds.center;
            float zPos = 0, zRatio = 0, centerZ = meshCenter.z;
            float deltaX = 0, vx, newBoundBoxX = 0;
            float halfScaledDist = 1 / halfW;
            if (useMeshDeformation == false)
                halfScaledDist = widthScaling / halfW;
            Vector3[] verts = thisModdedMesh.vertices;
            for (int v = 0; v < verts.Length; v++)
            {
                //float uvx = uvs[sv].x;
                vx = verts[v].x;
                zRatio = Mathf.Abs(verts[v].z - centerZ) * halfScaledDist;

                if (verts[v].z > 0)
                {
                    newBoundBoxX = (vx * scale0) - oppIn;
                    deltaX = newBoundBoxX - vx;
                    verts[v].x = vx + (deltaX * zRatio);

                    float dx = verts[v].x - vx;
                    float dxRatio = dx / meshLength;
                    //uvs[sv].x = uvx - (dxRatio / 2);
                }
                else if (verts[v].z < 0)
                {
                    newBoundBoxX = (vx * scale1) + oppIn;
                    deltaX = newBoundBoxX - vx;
                    verts[v].x = vx + (deltaX * zRatio);

                    float dx = verts[v].x - vx;
                    float dxRatio = dx / meshLength;
                }
            }
            thisModdedMesh.vertices = verts;
        }
        //------------
        // currPosA = current post. We need both B and C to make the far end miter we need to know the direction of the next section
        // adjustedNativeScale will be (1,1,1) unless we're dealing with a non-standard User GameObject
        private void MiterRailMesh(ref Mesh thisModdedMesh, Vector3 prevPostPos, Vector3 currPosA, Vector3 posB, Vector3 posC, int sectionIndex, Vector3 railScale,
            float horizDistance, Vector3 adjustedNativeScale, bool useMeshDeformation)
        {
            // First calculate all the necessary parameters
            bool isFirstSection = sectionIndex == 0;
            bool isLastSection = false;
            if (sectionIndex == allPostPositions.Count - 2)
                isLastSection = true;

            float currHeading = currPosA.HeadingTo(posB);
            float prevHeading = prevPostPos.HeadingTo(currPosA);
            if (isFirstSection)
                prevHeading = currHeading;

            float nextHeading = 0;
            if (isLastSection == false)
                nextHeading = posB.HeadingTo(posC);

            if (closeLoop == true) // there is a nextPos panel, or we're looping back to the first
            {
                if (isFirstSection == true)
                {
                    // We need to set posC to the first post to loop
                    Vector3 penultimatePostPos = allPostPositions[allPostPositions.Count - 2];
                    Vector3 lastPostPos = allPostPositions[0]; // as we know the last is same as for first
                    prevHeading = penultimatePostPos.HeadingTo(lastPostPos);
                }
                else if (isLastSection == true)
                {
                    Vector3 postPos0 = allPostPositions[0];
                    Vector3 postPos1 = allPostPositions[1];
                    nextHeading = postPos0.HeadingTo(postPos1);
                }
            }
            float incomingAngle = VectorUtilitiesTCT.GetCornerAngle(prevHeading, currHeading);
            float outgoingAngle = VectorUtilitiesTCT.GetCornerAngle(currHeading, nextHeading);
            if (isLastSection && closeLoop == false) //this is the last section
                outgoingAngle = 0;

            float widthScaling = 1;
            if (useMeshDeformation == false)
                widthScaling = adjustedNativeScale.z;

            Vector3 meshSize = thisModdedMesh.bounds.size;
            float meshLength = meshSize.x;
            float meshWidth = meshSize.z;
            float actualLength = meshLength * (1 / 1) * railScale.x;
            if (useMeshDeformation == false)
                actualLength = meshLength * (horizDistance / DEFAULT_RAIL_LENGTH) * railScale.x;

            float meshLengthScaling = actualLength / meshLength;
            float halfW = meshWidth / 2.0f;
            if (useMeshDeformation == false)
                halfW = meshWidth * widthScaling / 2.0f;

            //-------------------------------------------------------------

            // Bounding box points (anti-clockwise from bottom right, looking from above the rail)
            Vector3 p0 = new Vector3(0, 0, halfW);
            Vector3 p1 = new Vector3(meshLength, 0, halfW);
            Vector3 p2 = new Vector3(meshLength, 0, -halfW);
            Vector3 p3 = new Vector3(0, 0, -halfW);

            float inMiterAngle = incomingAngle / 2 * Mathf.Deg2Rad;
            float outMiterAngle = outgoingAngle / 2 * Mathf.Deg2Rad;

            float oppIn = Mathf.Tan(inMiterAngle) * halfW; // this is the shift in x position 
            float oppOut = Mathf.Tan(outMiterAngle) * halfW;
            oppIn /= meshLengthScaling;
            oppOut /= meshLengthScaling;

            p0 += new Vector3(oppIn, 0, 0);
            p1 += new Vector3(-oppOut, 0, 0);
            p2 += new Vector3(oppOut, 0, 0);
            p3 += new Vector3(-oppIn, 0, 0);

            float len0 = p1.x - p0.x; // the length of side 0 (the side that contains p0)
            float len1 = p2.x - p3.x;
            float scale0 = len0 / meshLength; //the adjustedNativeScale of side 0
            float scale1 = len1 / meshLength; //the adjustedNativeScale of side 1

            Vector3 meshCenter = thisModdedMesh.bounds.center;
            float zRatio = 0, centerZ = meshCenter.z;
            float deltaX = 0, vx, newBoundBoxX = 0;
            float halfScaledDist = 1 / halfW;
            if (useMeshDeformation == false)
                halfScaledDist = widthScaling / halfW;
            Vector3[] verts = thisModdedMesh.vertices;
            for (int v = 0; v < verts.Length; v++)
            {
                //float uvx = uvs[sv].x;
                vx = verts[v].x;
                zRatio = Mathf.Abs(verts[v].z - centerZ) * halfScaledDist;

                if (verts[v].z > 0)
                {
                    newBoundBoxX = (vx * scale0) - oppIn;
                    deltaX = newBoundBoxX - vx;
                    verts[v].x = vx + (deltaX * zRatio);

                    float dx = verts[v].x - vx;
                    float dxRatio = dx / meshLength;
                    //uvs[sv].x = uvx - (dxRatio / 2);
                }
                else if (verts[v].z < 0)
                {
                    newBoundBoxX = (vx * scale1) + oppIn;
                    deltaX = newBoundBoxX - vx;
                    verts[v].x = vx + (deltaX * zRatio);

                    float dx = verts[v].x - vx;
                    float dxRatio = dx / meshLength;
                }
            }
            thisModdedMesh.vertices = verts;
        }



        private bool ScaleRail(int sectionIndex, int layerIndex, float distanceToNextPost, Vector3 railScale, SlopeMode railSlopeMode, JointStyle jointStyle, float horizDistance, Vector3 currDirectionEuler, GameObject thisRail, Vector3 varSizeMultiplier, float sectionInclineAngle, ref Vector3 adjustedNativeScale)
        {
            bool useMeshDeformation = true; //TODO. currPost little pointless in v3.5, but preparing for v4.1
                                            //--- X ---
            float gainInLength = 0;
            if (railSlopeMode == SlopeMode.slope)// real length, stackIdx.e. hypotenuse
            {
                adjustedNativeScale.x *= (distanceToNextPost / 3.0f) * railScale.x * varSizeMultiplier.x;
                gainInLength = (distanceToNextPost * railScale.x) - distanceToNextPost;
            }
            else if (railSlopeMode == SlopeMode.shear)
            {
                if ((jointStyle == JointStyle.mitre) || VectorUtilitiesTCT.GetAngleFromZero(sectionInclineAngle) > 16) // this can be tuned, tradeoff for smooth overlaps on level ground, or smooth joints on steep slopes
                {
                    adjustedNativeScale.x *= (horizDistance / 3.0f) * railScale.x * varSizeMultiplier.x; //prevRailLengthScalar
                    gainInLength = (horizDistance * railScale.x) - horizDistance;
                }
                else
                {
                    adjustedNativeScale.x *= (distanceToNextPost / 3.0f) * railScale.x * varSizeMultiplier.x;
                    gainInLength = (distanceToNextPost * railScale.x) - distanceToNextPost;
                }
            }
            else if (railSlopeMode == SlopeMode.step)
            {
                adjustedNativeScale.x *= (horizDistance / 3.0f) * railScale.x * varSizeMultiplier.x; // distanceToNextPost along ground stackIdx.e. adjacent
                gainInLength = (horizDistance * railScale.x) - horizDistance;
            }
            if (railScale.x != 1.0f)
                thisRail.transform.Translate(gainInLength / 2, 0, 0);
            //--- Z ---
            adjustedNativeScale.z *= railScale.z * globalScale.z * varSizeMultiplier.z;


            if (extendRailEnds[layerIndex] == true && (sectionIndex == 0 || sectionIndex == allPostPositions.Count - 2))
            {
                float oldLength = distanceToNextPost;
                float desiredLength = distanceToNextPost + endExtensionLength[layerIndex];
                float xScaling = (desiredLength / oldLength) - 1;

                //calculate the direction of this section
                //create a vecor3 that is the same length as the extra length, and in the same direction as this section
                Vector3 extraLengthVector = currDirectionEuler * endExtensionLength[layerIndex];
                //move this section along by the extra length
                if (sectionIndex == 0)
                    thisRail.transform.position -= extraLengthVector;
                thisRail.transform.localScale += new Vector3(xScaling, 0, 0);
            }

            //-- Apply scaling ----
            if (useMeshDeformation == false)
                thisRail.transform.localScale = adjustedNativeScale;
            else
                MeshUtilitiesAFWB.ScaleAllMeshesInGO(thisRail, adjustedNativeScale);
            return useMeshDeformation;
        }

        //------------------------------------------------
        private void AddColliderToRail(Vector3 posA, Vector3 posB, LayerSet layer, GameObject thisRail)
        {
            List<Transform> pool = GetPoolForLayer(layer);
            Vector3 centreColliderPos = (posA + posB) / 2;
            Collider collider = CreateColliderForLayer(thisRail, centreColliderPos, layer);
            //if (stackIdx > 0 && stackedRailsAreTriggerColliders && collider != null)
            collider.isTrigger = false; ;
        }


        private int CheckAndRepairInvalidRailPool(LayerSet layer)
        {

            if (ValidatePoolForLayer(layer) == false)
            {
                Debug.LogWarning($"Pool for {GetLayerNameAsString(layer)} was null.  \n");
                ResetPoolForLayer(layer);

            }
            List<Transform> pool = GetPoolForLayer(layer);
            return pool.Count;
        }

        private static bool SkipChanceOfMissing(int sectionIndex, LayerSet layer, RandomSeededValuesAF seeds, float chanceOfMissing, int stackIndex)
        {
            //==========================================
            //       Chance of Missing
            //==========================================
            float rMissing = seeds.GetChanceOfMissingAtSectionIndex(sectionIndex, layer);

            //-- To ensure stacked aren't using the same value as the base, we'll modulo in to the seeds list to get a different value.
            int numSeeds = seeds.rChanceMissing.Count, seedIndex = 0;
            if (stackIndex > 0)
                seedIndex = numSeeds % ((sectionIndex * 3) + 7);
            rMissing = seeds.GetChanceOfMissingAtSectionIndex(seedIndex, layer);


            //if (allowRailRand && chanceOfMissing > rMissing)
            //{
            //    continue;
            //}
            //========= Chance of Missing Post ==============
            bool skipRail = false;
            if ((chanceOfMissing == -1 && sectionIndex % 2 == 1))
                skipRail = true;
            else if (chanceOfMissing > 0 && chanceOfMissing > rMissing)
                skipRail = true;
            return skipRail;
        }
        //=======================================================================================================
        private Vector3 RandomRailRotations(int sectionIndex, bool allowRailRand, bool allowRandRailRotationVariation, float quantRotProb,
            float quantRotAngle, int quantRotAxis, bool allowQuantRot, RandomSeededValuesAF seeds, int stackIdx, GameObject thisRail, bool randomScopeIsValid,
             float totalQuantRotAmount, Vector3 rCentre, Vector3 railCentre, Vector3 totalQuantRot)
        {
            if (allowQuantRot)
            {
                bool forceAlternate = (quantRotProb == 0);

                int numChunks = (int)(360 / quantRotAngle) - 1;//eg, for 90 will return 3 (90,180,270)
                int multipleRots = 1; // the number of times we should rotate by the quantRotAngle, so totalQuantRotAmount = multipleRots * quantRotAngle

                if (quantRotAngle > 0) // -90, or -180 denote that we should do exactly 1 of that angle, not multiples
                                       //multipleRots = UnityEngine.Random.Range(1, numChunks + 1);//+1 because Range max is exclusive
                    multipleRots = (int)(seeds.rQuantRot[sectionIndex] * (numChunks + 1));
                else
                    quantRotAngle *= -1;

                totalQuantRotAmount = multipleRots * quantRotAngle;
                // If we're forcing alternate rotations, and the RNG has given us 0, we'll force a rotation of 1 * quantRotAngle, other wise it looks like we're not rotating at all
                if (forceAlternate && totalQuantRotAmount == 0)
                    totalQuantRotAmount = quantRotAngle;
                else if (forceAlternate && quantRotAngle != 180 && Mathf.Abs(totalQuantRotAmount) == 180)
                    totalQuantRotAmount = quantRotAngle;

                bool rot = false, isOddSectionIndex = (sectionIndex % 2 == 1);

                if (forceAlternate && isOddSectionIndex)
                    rot = true;
                else if (quantRotProb > 0 && seeds.rQuantRot[sectionIndex] <= quantRotProb)
                    rot = true;
                if (quantRotAngle == 1) // -1 denotes consecutive 90 deg rotations, and negative values have already been flipped
                {
                    totalQuantRotAmount = sectionIndex % 4 * 90;
                    rot = true;
                }

                totalQuantRotAmount += stackIdx * quantRotAngle; //???

                if (rot == true)
                {
                    if (quantRotAxis == 0) //x
                    {
                        thisRail.transform.RotateAround(rCentre, thisRail.transform.right, totalQuantRotAmount);
                        totalQuantRot = new Vector3(totalQuantRotAmount, 0, 0);//for debug
                    }
                    if (quantRotAxis == 1)//y
                    {
                        thisRail.transform.RotateAround(rCentre, thisRail.transform.up, totalQuantRotAmount);
                        totalQuantRot = new Vector3(0, totalQuantRotAmount, 0);
                    }
                    else if (quantRotAxis == 2)//z
                    {
                        thisRail.transform.RotateAround(rCentre, thisRail.transform.forward, totalQuantRotAmount);
                        totalQuantRot = new Vector3(0, 0, totalQuantRotAmount);
                    }
                }
            }
            //==============================================================
            //     Small random rotations
            //==============================================================
            Vector3 totalSmallRandRot = Vector3.zero;
            Vector3 smallRandRot = seeds.rSmallRot[sectionIndex];
            if (allowRandRailRotationVariation && Vector3.SqrMagnitude(smallRandRot) > 0.01f /*&& randomScopeIsValid*/)
            {
                //UnityEngine.Random.InitState(railASeeds.smallRotSeed);
                /*if (useMeshRotations == true)
                {
                    for (int m = 0; m < meshCount; m++)
                    {
                        thisModdedMesh = mfList[m].sharedMesh;
                        // If first mesh, or the object has multiple meshes and we allow them to vary indepenently, get new random values
                        if (m == 0 || allowIndependentSubmeshVariation)
                            smallRandRot = seeds.rSmallRot[sectionIndex];
                        totalSmallRandRot = smallRandRot;
                        thisModdedMesh = MeshUtilitiesAFWB.RotateMesh(thisModdedMesh, smallRandRot, true, default(Vector3));
                    }
                }
                else*/
                {
                    railCentre = CalculateCentreOfRail(thisRail);


                    //-- From bottom row (0), to top
                    if (stackIdx == 0)
                    {
                        // Leave unchanged
                    }
                    if (stackIdx % 2 == 1)
                    {
                        smallRandRot = -smallRandRot * 0.5f;
                        //smallRandRot = smallRandRot = new Vector3(-smallRandRot.z*.7f, smallRandRot.x*1.2f, smallRandRot.y*.9f);
                        //smallRandRot = Vector3.zero;
                    }
                    else if (stackIdx % 2 == 2)
                    {
                        //smallRandRot = new Vector3(smallRandRot.z, smallRandRot.x, -smallRandRot.y);
                        //smallRandRot = -smallRandRot;
                        smallRandRot = smallRandRot = new Vector3(-smallRandRot.z * .7f, smallRandRot.x * 1.2f, smallRandRot.y * .9f);
                    }
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.right, smallRandRot.x);
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.up, smallRandRot.y);
                    thisRail.transform.RotateAround(rCentre, thisRail.transform.forward, smallRandRot.z);
                    totalSmallRandRot = smallRandRot;
                    totalSmallRandRot += stackIdx * smallRandRot; //???
                }
            }

            Vector3 totalUserModdedRotation = totalQuantRot + totalSmallRandRot;
            return totalUserModdedRotation;
        }

        //-----------------------------
        // Shear the rail mesh to fit the slope - all vertices are gradually moved up or down between the start and end elevations
        private Mesh ShearRailMesh(Mesh thisModdedMesh, Vector3 posA, Vector3 posB, Vector3 nativeScale, Vector3 size, GameObject thisRail,
            List<MeshFilter> mfList, Vector3 scale, bool useMeshDeformation)
        {
            float heightDeltaAB = posA.y - posB.y;
            int meshCount = mfList.Count;
            if (true /* && dontShearPerpendicular*/) //v4.1
            {
                List<GameObject> goList = MeshUtilitiesAFWB.GetAllMeshGameObjectsFromGameObject(thisRail);
                float relativeDistance = 0, heightChangeFromSlope = 0;
                for (int m = 0; m < meshCount; m++)
                {
                    GameObject thisGO = goList[m];
                    thisModdedMesh = mfList[m].sharedMesh;
                    Vector3[] origVerts = thisModdedMesh.vertices;
                    Vector3[] vertices = thisModdedMesh.vertices;
                    Vector3 vert;
                    int n = vertices.Length;
                    for (int v = 0; v < n; v++)
                    {
                        vert = vertices[v];
                        float regularScaledY = origVerts[v].y;
                        if (useMeshDeformation == false)
                        {
                            relativeDistance = (Mathf.Abs(vert.x)) / DEFAULT_RAIL_LENGTH; // the distanceToNextPost of each vertex from the end
                            relativeDistance *= -size.x;
                            heightChangeFromSlope = relativeDistance * heightDeltaAB * (nativeScale.x / scale.y);
                            if (meshCount > 1)
                                heightChangeFromSlope *= thisGO.transform.localScale.x;
                        }
                        else if (useMeshDeformation == true)
                        {
                            relativeDistance = (Mathf.Abs(vert.x)) / thisModdedMesh.bounds.size.x;
                            relativeDistance *= -size.x;
                            heightChangeFromSlope = relativeDistance * heightDeltaAB * nativeScale.x;
                        }
                        vertices[v].y = regularScaledY + heightChangeFromSlope;
                    }
                    thisModdedMesh.vertices = vertices;
                    thisModdedMesh.RecalculateBounds();
                    mfList[m].sharedMesh = thisModdedMesh;
                }
            }
            return thisModdedMesh;
        }

        //=================================================================
        private void SetUpClickMarkers()
        {
            DestroyNodeMarkers();
            if (showControls)
                ResetNodeMarkerPool();
            else
                return;

            Vector3 markerScale = new Vector3(0.3f, 0.3f, 0.3f);

            //-- Variation might need to know the size of the Mesh
            GameObject go = GetCurrentPrefabForLayer(LayerSet.postLayerSet);
            float postMeshHeight = MeshUtilitiesAFWB.GetCombinedSizeOfAllMeshesInGameObject(go).y;

            Vector3 variantScaling = Vector3.one, variantOffset = Vector3.zero;

            int numSeqSteps = GetNumSeqStepsForLayer(LayerSet.postLayerSet);


            //SeqItem currSeqItem = new SeqItem();
            int numClickPoints = clickPoints.Count;

            for (int i = 0; i < numClickPoints; i++)
            {
                int postIndex = PostVector.GetPostIndexFromClickpointIndex(i);

                GameObject marker = nodeMarkersPool[i].gameObject;
                marker.SetActive(true);
                marker.hideFlags = HideFlags.HideInHierarchy;
                Vector3 clickPointPos = clickPoints[i];
                Vector3 markerPos = clickPoints[i];

                //float h = (globalScale.y * postScale.y * mainPostsSizeBoost.y * variantScaling.y) + postHeightOffset + variantOffset.y + globalLift;
                //if (h < 1) h = 1;
                //float markerHeightBoost = 1.1f;
                //markerPos.y += (h * markerHeightBoost);
                //float postTop = markerPos.y;

                //float groundY = postPos.y, padding = 0.35f, min = 0.5f;
                float groundY = clickPointPos.y, padding = 0.35f, min = 0.5f;
                (float highestPointRailA, float yOffsetRailA) = MeshUtilitiesAFWB.CalculateHighestPointOfLayer(this, LayerSet.railALayerSet);
                (float highestPointRailB, float yOffsetRailB) = MeshUtilitiesAFWB.CalculateHighestPointOfLayer(this, LayerSet.railBLayerSet);
                (float highestPointPost, float yOffsetPost) = MeshUtilitiesAFWB.CalculateHighestPointOfLayer(this, LayerSet.postLayerSet);

                float maxRailTop = Mathf.Max(highestPointRailA + yOffsetRailA, highestPointRailB + yOffsetRailB);
                float maxGoTop = Mathf.Max(maxRailTop, highestPointPost + yOffsetPost);
                float topPos = maxGoTop + padding;
                if (topPos < min)
                    topPos = min;

                float extraHeightOfVariant = 0;
                if (usePostVariations)
                {
                    int seqStep = postIndex % numSeqSteps;
                    SeqItem currSeqItem = GetSeqItemAtStepForLayer(seqStep, LayerSet.postLayerSet);
                    variantScaling = currSeqItem.size;
                    variantOffset = currSeqItem.pos;
                    extraHeightOfVariant = (globalScale.y * postScale.y * mainPostsSizeBoost.y * (variantScaling.y - 1) * postMeshHeight) + variantOffset.y;
                }

                markerPos.y = groundY + topPos;
                markerPos.y += extraHeightOfVariant;
                marker.transform.position = markerPos;
                marker.transform.localScale = markerScale;
                marker.name = "FenceManagerMarker_" + i.ToString();
            }
        }
        //-------------------------------------------------
        //-- called from SetupPost
        private void SetupClickMarker(PostVector postVector)
        {
            Vector3 postPos = postVector.Position;
            int postIndex = postVector.Index();
            int clickpointIndex = postVector.GetClickNodeIndex();
            if (clickpointIndex == -1)
            {
                Debug.LogWarning($"No clickpointIndex for postVector {postVector.Index()} in SetupClickMarker()");
                return;
            }
            if (ValidateNodeMarkers() == false)
                return;

            //-- Is it a sequencer step or a Single?
            Vector3 variantScaling = Vector3.one, variantOffset = Vector3.zero;
            if (GetUseSequencerForLayer(LayerSet.postLayerSet) == true /* || using single*/)
            {
                SeqItem currSeqItem = postSequencer.GetSeqItemForSectionIndex(postIndex);
                variantScaling = postSequencer.GetSeqScaleForSectionIndex(postIndex);
                variantOffset = postSequencer.GetSeqOffsetForSectionIndex(postIndex);
            }

            GameObject marker = nodeMarkersPool[clickpointIndex].gameObject;
            marker.SetActive(true);
            marker.hideFlags = HideFlags.HideInHierarchy;

            Vector3 markerPos = postPos;
            float h = (globalScale.y * postScale.y * mainPostsSizeBoost.y * variantScaling.y) + postHeightOffset + variantOffset.y + globalLift;
            if (h < 1) h = 1;
            float markerHeightBoost = 1.1f;

            markerPos.y += (h * markerHeightBoost);
            float postTop = markerPos.y;

            float groundY = postPos.y, padding = 0.35f, min = 0.5f;
            (float highestPointRailA, float yOffsetRailA) = MeshUtilitiesAFWB.CalculateHighestPointOfLayer(this, LayerSet.railALayerSet);
            (float highestPointRailB, float yOffsetRailB) = MeshUtilitiesAFWB.CalculateHighestPointOfLayer(this, LayerSet.railBLayerSet);
            (float highestPointPost, float yOffsetPost) = MeshUtilitiesAFWB.CalculateHighestPointOfLayer(this, LayerSet.postLayerSet);

            float maxRailTop = Mathf.Max(highestPointRailA + yOffsetRailA, highestPointRailB + yOffsetRailB);
            float maxGoTop = Mathf.Max(maxRailTop, highestPointPost + yOffsetPost);
            float topPos = maxGoTop + padding;
            if (topPos < min)
                topPos = min;
            markerPos.y = groundY + topPos;
            marker.transform.position = markerPos;
            marker.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            marker.name = "FenceManagerMarker_" + clickpointIndex.ToString();
        }

        //-----------
        //sets parts to be either on a regular layer, or on a special layer that ignores raycasts
        //useful to switch these on and off when we want to do a raycast, but IGNORE existing fence objects
        public void SetIgnorePartsColliders(bool inIgnore)
        {
            int layer = 0; //default layer
            if (inIgnore)
                layer = 2;// 'Ignore Raycast' layer
            for (int i = 0; i < postsPool.Count; i++)
            {
                if (postsPool[i] != null)
                    postsPool[i].gameObject.layer = layer;
            }
            for (int i = 0; i < railsAPool.Count; i++)
            {
                if (railsAPool[i] != null)
                    railsAPool[i].gameObject.layer = layer;
            }
            for (int i = 0; i < railsBPool.Count; i++)
            {
                if (railsBPool[i] != null)
                    railsBPool[i].gameObject.layer = layer;
            }
            for (int i = 0; i < ex.extrasPool.Count; i++)
            {
                if (ex.extrasPool[i] != null)
                    ex.extrasPool[i].gameObject.layer = layer;
            }
            SetIgnoreClickMarkers(inIgnore);
        }
        //------------
        // we sometimes need to disable these when raycasting postsPool to the ground
        // but we need them back on when control-click-deleting them
        public void SetIgnoreClickMarkers(bool inIgnore)
        {
            int layer = 0; //default layer
            if (inIgnore)
                layer = 2;// 'Ignore Raycast' layer

            if (ValidateNodeMarkers() == false)
                return;

            for (int i = 0; i < clickPoints.Count; i++)
            {
                if (nodeMarkersPool[i] != null)
                    nodeMarkersPool[i].gameObject.layer = layer;
            }
        }
        /// <summary> Validate and returns List<Transform> nodeMarkersPool </summary>
        /// <returns>List<Transform> nodeMarkersPool </returns>
        /// <remarks> Use clickPoints.Count to access the correct num in use from the pool</remarks>
        public List<Transform> GetNodeMarkers()
        {
            ValidateNodeMarkers();
            return nodeMarkersPool;
        }
        //----------------
        // Set the visibilty of the nodeMarkersPool. Depending on 'showControlNodes', the active number of posts .
        // we want to avoid raycasting on them when building, but do when scene-view editing
        // set on each rebuild
        public void SetClickMarkersActiveStatus(bool newState)
        {
            if (ValidateNodeMarkers() == false)
                return;

            for (int i = 0; i < clickPoints.Count; i++)
            {
                if (nodeMarkersPool[i] != null)
                {
                    nodeMarkersPool[i].GetComponent<Renderer>().enabled = newState;
                    nodeMarkersPool[i].gameObject.SetActive(newState);
                    if (newState == true)
                        nodeMarkersPool[i].hideFlags = HideFlags.None;
                    else
                        nodeMarkersPool[i].hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }
        //------------------------
        /// <summary>Checks the status of node markers and ensures they are correctly initialized and populated.</summary>
        /// <returns>True if node markers are valid; otherwise, false.</returns>
        /// <remarks> nodeMarkerObj is the main prefab for the node markers.
        /// Logs & Warnings are optional, but critical Errors will always be shown.</remarks>
        private bool ValidateNodeMarkers(bool fix = true, bool warn = true)
        {
            //-- Check if nodeMarkersPool is null
            if (nodeMarkersPool == null)
            {
                Debug.LogError("nodeMarkersPool is null\n");
                if (fix)
                    ResetNodeMarkerPool();
                else
                    return false;
            }

            //-- Check if nodeMarkerObj prefab is null
            if (nodeMarkerObj == null)
            {
                Debug.LogError("ValidateNodeMarkers(): nodeMarkerObj is null. ClickMarkerObj is missing from AFWBPrefabs folder, please find and replace, or reload AFWB.\n");
                return false;
            }
            //-- Check if nodeMarkersPool is empty
            if (nodeMarkersPool.Count < clickPoints.Count() || nodeMarkersPool.Count == 0)
            {
                //-- Reset pool if there are clickPoints but nodeMarkersPool,Count is 0 ot too few

                //Debug.LogWarning($"nodeMarkersPool.Count is {nodeMarkersPool.Count}, but there are {clickPoints.Count()} clickPoints\n");
                if (fix)
                    ResetNodeMarkerPool();
                else
                    return false;

            }
            //-- Check if the first element in nodeMarkersPool is null
            if (nodeMarkersPool.Count > 0 && nodeMarkersPool[0] == null)
            {
                Debug.LogError("nodeMarkersPool[0] is null\n");
                if (fix)
                    ResetNodeMarkerPool();
                else
                    return false;
            }
            //-- All checks passed
            return true;
        }

        //================================================================
        Vector3 CalculateCentreOfRail(GameObject rail)
        {
            Vector3 center = rail.transform.position;
            Vector3 newCenter = center;
            Vector3 f = rail.transform.forward;
            Vector3 fwd = new Vector3(f.x, f.y, f.z);

            fwd = new Vector3(f.z, f.y, -f.x); //swap x & z
            fwd *= rail.transform.localScale.x * 3 / 2; // adjustedNativeScale by length of fence (native length = 3m, then divide by two for centre)
            newCenter = center - fwd;

            return newCenter;
        }
        //-------------
        // Find the index of the input GO in the full sourceVariant list
        // If it'stackIdx in one or more sourceVariant slots, return the first index, else return -1
        public int FindFirstUseofGoInVariants(LayerSet layer, GameObject go)
        {
            int index = -1, numVariants = GetNumSourceVariantsInUseForLayer(layer, incMain: true);
            List<SourceVariant> variants = GetSourceVariantsForLayer(layer);
            if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
            {
                for (int i = 0; i < numVariants; i++)
                {
                    if (variants[i].Go == go)
                    {
                        index = i;
                        return index;
                    }
                }
            }

            return index;
        }
        //-----------------------------
        void FormatInstantiatedGONameWithSectionAndVariation(GameObject rail, int sectionIndex, int variantIndex)
        {
            string idStr = "[" + sectionIndex + " sv" + variantIndex + "]";
            string name = rail.name.Replace("(Clone)", idStr);
            rail.name = name;
        }

        //------------------
        public void ManageCloseLoop(bool loop)
        {
            if (loop)
                CloseLoop();
            else
                OpenLoop();
        }
        //------------------
        // After moving the first post check that the last is still positioned at the first
        public void UpdateCloseLoop(bool loop)
        {
            if (loop)
            {
                clickPoints[clickPoints.Count - 1] = clickPoints[0];
            }
        }
        //------------------
        public void CloseLoop()
        {
            if (clickPoints.Count < 3)
            {// prevent user from closing if less than 3 points
                closeLoop = false;
            }
            if (clickPoints.Count >= 3 && clickPoints[clickPoints.Count - 1] != clickPoints[0])
            {
                clickPoints.Add(clickPoints[0]); // copy the first clickPoint
                clickPointFlags.Add(clickPointFlags[0]);
                ForceRebuildFromClickPoints();
                //?SceneView.RepaintAll();
            }
        }
        //------------------
        public void OpenLoop()
        {
            if (clickPoints.Count >= 3)
            {
                clickPoints.RemoveAt(clickPoints.Count - 1); // remove the last clickPoint (the closer)
                ForceRebuildFromClickPoints();
                //?SceneView.RepaintAll();
            }
        }
        //---------------
        public void DeleteClickPoint(int index, bool rebuild = true)
        {
            if (clickPoints.Count > 0 && index < clickPoints.Count)
            {
                lastDeletedIndex = index;
                lastDeletedPoint = clickPoints[index];
                clickPoints.RemoveAt(index); clickPointFlags.RemoveAt(index);
                handles.RemoveAt(index);
                ForceRebuildFromClickPoints();
            }
        }
        //---------------------
        public void InsertPost(Vector3 clickPosition)
        {
            // Find the nearest post and connecting lines to the click position
            float nearest = 1000000;
            int insertPosition = -1;
            for (int i = 0; i < clickPoints.Count - 1; i++)
            {
                float distToLine = CalcDistanceToLine(clickPoints[i], clickPoints[i + 1], clickPosition);
                if (distToLine < nearest)
                {
                    nearest = distToLine;
                    insertPosition = i + 1;
                }
            }
            if (insertPosition != -1)
            {
                clickPoints.Insert(insertPosition, clickPosition);
                clickPointFlags.Insert(insertPosition, clickPointFlags[0]);

                ForceRebuildFromClickPoints();
                //-- Update handles ----
                handles.Clear();
                for (int i = 0; i < clickPoints.Count; i++)
                {
                    handles.Add(clickPoints[i]);
                }
            }
        }

        /// <summary>
        /// Calculates the unsigned angle (0-360) between Posts A, B, relative to A's current forward vector.
        /// </summary>
        /// <param name="postA">The transform of the first post.</param>
        /// <param name="postB">The transform of the second post, to which the angle is measured.</param>
        /// <returns>The unsigned angle from postA to postB in degrees. The angle is normalized to be within the range of 0 to 360 degrees.</returns>
        float GetUnsignedAngleToNextPost(Transform postA, Transform postB)
        {
            //-- This is the posts current forward vector dictated by its current rotation
            Vector3 referenceForward = postA.forward;

            Vector3 newDirection = postB.position - postA.position;
            float angle = Vector3.Angle(newDirection, referenceForward);
            float sign = (Vector3.Dot(newDirection, postA.right) > 0.0f) ? 1.0f : -1.0f;
            float finalAngle = sign * angle;
            if (finalAngle < 0) finalAngle = finalAngle + 360;
            return finalAngle;

            //-- Alternatively:
            //float signedAngle = Vector3.SignedAngle(newDirection, referenceForward, Vector3.up);
            //float finalAngle = signedAngle < 0 ? signedAngle + 360 : signedAngle;
        }


        //-------------------
        // we have to do this recursively one at a time because removing one will alter the angles of the others
        void ThinByAngle(List<Vector3> posList)
        {
            if (removeIfLessThanAngle < 0.01f) return;

            float minAngle = 180;
            int minAngleIndex = -1;
            for (int i = 1; i < posList.Count - 1; i++)
            {
                Vector3 vecA = posList[i] - posList[i - 1];
                Vector3 vecB = posList[i + 1] - posList[i];
                float angle = Vector3.Angle(vecA, vecB);
                if (!clickPoints.Contains(posList[i]) && angle < minAngle)
                {
                    minAngle = angle;
                    minAngleIndex = i;
                }
            }
            if (minAngleIndex != -1 && minAngle < removeIfLessThanAngle) // we found one
            {
                posList.RemoveAt(minAngleIndex);
                ThinByAngle(posList);
            }
        }
        //-------------------
        // we have to do this recursively one at a time because removing one will alter the distances of the others
        void ThinByDistance(List<Vector3> posList)
        {
            float minDist = 10000;
            int minDistIndex = -1;
            float distToPre, distToNext, distToNextNext;
            for (int i = 1; i < posList.Count - 1; i++)
            {
                if (IsCloseClickPoint(posList[i]) != -1)
                {
                    distToNext = Vector3.Distance(posList[i], posList[i + 1]);
                    if (distToNext < stripTooClose)
                    {
                        // close to neighbour, do we strip this one or the neighbour? Strip the one that has the other closest neighbour
                        // but only if it is not a clickpoint
                        if (IsCloseClickPoint(posList[i + 1]) != -1)
                        {
                            distToPre = Vector3.Distance(posList[i], posList[i - 1]);
                            distToNextNext = Vector3.Distance(posList[i + 1], posList[i + 2]);

                            if (distToPre < distToNextNext)
                            {
                                minDist = distToNext;
                                minDistIndex = i;
                            }
                            else
                            {
                                minDist = distToNext;
                                minDistIndex = i + 1;
                            }
                        }
                        else
                        {
                            minDist = distToNext;
                            minDistIndex = i;
                        }
                    }
                }
            }
            if (minDistIndex != -1 && minDist < stripTooClose) // we found one
            {
                posList.RemoveAt(minDistIndex);
                ThinByDistance(posList);
            }
        }

        //-------------------
        /*public bool IsClickPoint(int postIndex)
        {
            bool isClickPoint = PostVector.GetPostVectorAtIndex(postIndex).IsClickPointNode;
            return isClickPoint;
        }*/
        //----------
        //returns index or -1 if fail
        public int IsCloseClickPoint(Vector3 pos)
        {
            //if (postHeightOffset != 0)
            //-- Compensate for possible height offset
            float y = pos.y + (postHeightOffset * globalScale.y);


            for (int i = 0; i < clickPoints.Count; i++)
            {
                float cY = clickPoints[i].y;
                //Debug.Log($"{i}  pos.y = {pos.y}       clickPoints[i] = {cY}\n");
                
                float distSqr = Vector3.Magnitude(pos - clickPoints[i]);
                if (distSqr < .01f)
                    return i;
                //else Debug.Log(Vector3.Magnitude(pos - clickPoints[stackIdx]) + "\n");
            }
            return -1;
        }
        //-------------------
        // return the index of the  clickpoint of the given pos (usually a post position, or -1 if fail
        public int GetClickPointIndex(Vector3 pos)
        {
            int index = clickPoints.IndexOf(pos);
            return index;
        }
        //----------
        // Is the given pos a valid post position?
        //returns index or -1 if fail
        public int IsPostPoint(Vector3 pos)
        {
            //-- Compensate for possible height offset
            pos.y -= postHeightOffset * globalScale.y;

            for (int i = 0; i < allPostPositions.Count; i++)
            {
                if (Vector3.Magnitude(pos - allPostPositions[i]) < .001f)
                    return i;
            }
            return -1;
        }
        //----------------------
        public int GetNumBuiltForLayer(LayerSet layer)
        {
            if (layer == LayerSet.postLayerSet)
                return postsBuiltCount;
            else if (layer == LayerSet.railALayerSet)
                return railABuiltCount;
            else if (layer == LayerSet.railBLayerSet)
                return railBBuiltCount;
            else if (layer == LayerSet.subpostLayerSet)
                return subpostsBuiltCount;
            else if (layer == LayerSet.extraLayerSet)
                return ex.extrasBuiltCount;

            return 0;
        }
        //---------------------------
        //Gets the number of postsPool from the current click point index (inclusive) to the nextPos click point index (non-inclusive)
        public int GetNumPostsInSectionNodeToNode(int clickPointIndex)
        {
            int numPosts = 0;

            int startPostIndex = GetPostIndexOfClickPoint(clickPointIndex);
            int nextPostIndex = GetPostIndexOfClickPoint(clickPointIndex + 1);

            if (startPostIndex != -1 && nextPostIndex != -1)
            {
                numPosts = nextPostIndex - startPostIndex;
            }
            return numPosts;
        }
        //---------------------------
        public int GetPostIndexOfClickPoint(int clickPointIndex)
        {
            for (int i = 0; i < allPostPositions.Count; i++)
            {
                int thisClickPointIndex = GetClickPointIndex(allPostPositions[i]);
                if (thisClickPointIndex == clickPointIndex)
                    return i;
            }
            return -1;
        }
        // Is the given pos a valid RailA position?
        public int IsRailAPoint(Vector3 pos)
        {
            for (int i = 0; i < railsAPool.Count; i++)
            {
                if (Vector3.Magnitude(pos - railsAPool[i].position) < .001f)
                    return i;
            }
            return -1;
        }
        public int IsRailBPoint(Vector3 pos)
        {
            for (int i = 0; i < railsBPool.Count; i++)
            {
                if (Vector3.Magnitude(pos - railsBPool[i].position) < .001f)
                    return i;
            }
            return -1;
        }
        // returns (a, b) where 1 = index of singleVarGO, and b = 0 if railA or 1 if railB 
        // will return (-1, -1) if not found
        public (int, int) IsRailPoint(GameObject go)
        {
            LayerSet layer = GetRailLayerOfRail(go);

            int railAIndex = IsRailAPoint(go.transform.localPosition);
            if (layer == LayerSet.railALayerSet && railAIndex != -1)
                return (railAIndex, 0);
            int railBIndex = IsRailBPoint(go.transform.localPosition);
            if (layer == LayerSet.railALayerSet && railBIndex != -1)
                return (railBIndex, 1);

            return (-1, -1);
        }

        //---------------
        // check the go to see if it is in railA or railB Layer
        public LayerSet GetRailLayerOfRail(GameObject go)
        {
            if (go == null)
            {
                Debug.LogWarning("GetRailLayerOfRail: go is null");
                return LayerSet.noneLayerSet;
            }
            if (go.transform.parent == null)
            {
                Debug.LogWarning("GetRailLayerOfRail: go.transform.parent is null");
                return LayerSet.noneLayerSet;
            }

            if (go.transform.parent.name.Contains("RailsAGroupedFolder"))
                return LayerSet.railALayerSet;
            else if (go.transform.parent.name.Contains("RailsBGroupedFolder"))
                return LayerSet.railBLayerSet;
            return LayerSet.noneLayerSet;
        }
        //---------------
        public LayerSet InferLayerFromGoName(GameObject go)
        {
            LayerSet layer = LayerSet.noneLayerSet;
            if (go == null || go.transform.parent == null)
            {
                //Debug.LogWarning("InferLayerFromGoName: singleVarGO is null");
                return layer;
            }
            if (go.transform.parent.name.Contains("RailsAGroupedFolder"))
                layer = LayerSet.railALayerSet;
            else if (go.transform.parent.name.Contains("RailsBGroupedFolder"))
                layer = LayerSet.railBLayerSet;
            else if (go.transform.parent.name.Contains("PostsGroupedFolder0"))
                layer = LayerSet.postLayerSet;
            else if (go.transform.parent.name.Contains("ExtrasGroupedFolder"))
                layer = LayerSet.extraLayerSet;
            else if (go.transform.parent.name.Contains("SubsGroupedFolder"))
                layer = LayerSet.subpostLayerSet;
            else if (go.transform.name.Contains("Marker"))
                layer = LayerSet.markerLayerSet;

            else if (go.transform.name.Contains("_Post"))
                layer = LayerSet.postLayerSet;

            //-- The following doesn't work because the prefab names dont have the designator A or B. Left here as it's not obvious
            /*else if (go.transform.name.Contains("_RailA"))
                    layer = LayerSet.railALayerSet;
            else if (go.transform.name.Contains("_RailB"))
                layer = LayerSet.railBLayerSet;*/

            else if (go.transform.name.Contains("_Extra"))
                layer = LayerSet.extraLayerSet;
            else if (go.transform.name.Contains("_Sub"))
                layer = LayerSet.subpostLayerSet;

            else
                layer = LayerSet.noneLayerSet;



            return layer;
        }
        //---------------
        public PrefabTypeAFWB InferPrefabTypeFromGoName(GameObject go)
        {
            PrefabTypeAFWB prefabType = PrefabTypeAFWB.nonePrefab;
            if (go == null || go.transform.parent == null)
                return prefabType;

            prefabType = InferLayerFromGoName(go).ToPrefabType();
            return prefabType;
        }
        public PrefabTypeAFWB GetPrefabTypeFromName(string goName)
        {
            string[] suffixes = { "_Panel_Rail", "_Rail", "_Panel" };
            foreach (var suffix in suffixes)
            {
                if (goName.Contains(suffix))
                    return PrefabTypeAFWB.railPrefab;
            }
            if (goName.Contains("_Post"))
                return PrefabTypeAFWB.postPrefab;
            if (goName.Contains("_Extra"))
                return PrefabTypeAFWB.extraPrefab;

            return PrefabTypeAFWB.nonePrefab;
        }

        //---------------------------
        /// <summary>
        /// Full name including layer suffix
        /// </summary>
        /// <returns>int Prefab Index</returns>
        public int FindPrefabIndexByNameForLayer(PrefabTypeAFWB prefabType, string prefabName, bool warnMissing = true, bool replaceMissingWithDefault = true)
        {
            List<GameObject> prefabs = GetPrefabsForPrefabType(prefabType);
            int prefabsCount = prefabs.Count;

            for (int i = 0; i < prefabsCount; i++)
            {
                bool prefabOK = CheckPrefabAtIndexForLayer(i, prefabType.ToLayer());
                if (prefabOK == false)
                    continue;

                string name = prefabs[i].name;
                if (name == prefabName)
                    return i;
            }
            //-- If a Post wasn't found, maybe the Post is using an Extra
            if (prefabType == PrefabTypeAFWB.postPrefab)
            {
                prefabs = GetPrefabsForLayer(LayerSet.extraLayerSet);
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] == null)
                        continue;
                    string name = prefabs[i].name;
                    if (name == prefabName)
                        return i;
                }
            }

            //-- If an Extra wasn't found, maybe the Extra is using a Post
            if (prefabType == PrefabTypeAFWB.extraPrefab)
            {
                prefabs = GetPrefabsForLayer(LayerSet.postLayerSet);
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] == null)
                        continue;
                    string name = prefabs[i].name;
                    if (name == prefabName)
                        return i;
                }
            }

            if ((warnMissing || replaceMissingWithDefault) && prefabName != "-")
            {
                string warningStr = $"FindPrefabIndexByNameForLayer():   Couldn't find prefab with name: {prefabName} . " +
                    $"Is it a User Object that's been deleted or re-named?  ( {prefabType} )" +
                    $"\n Unable to use presets that rely on this prefab.\n";
                if (replaceMissingWithDefault && prefabName != "-")
                    warningStr += $"Replacing with default {prefabType.ToString()} prefab\n";
                else
                    warningStr += $"Null Prefab was returned\n";

                Debug.LogWarning($"{warningStr}");

                if (replaceMissingWithDefault)
                {
                    int defaultPrefabIndex = GetDefaultPrefabIndexForLayer(GetLayerFromPrefabType(prefabType));
                    return defaultPrefabIndex;
                }
            }
            return -1;
        }
        //-------------------------
        public int GetDefaultPrefabIndexForLayer(LayerSet layer)
        {
            int defaultPrefabIndex = 0;

            if (layer == LayerSet.postLayerSet)
                defaultPrefabIndex = FindPrefabIndexByName(layer, "BrickSquare_Post", false, false);
            else if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
                defaultPrefabIndex = FindPrefabIndexByName(layer, "Brick_WallHigh_Panel", false, false);
            else if (layer == LayerSet.subpostLayerSet)
                defaultPrefabIndex = FindPrefabIndexByName(layer, "BrickSquare_Post", false, false);
            else if (layer == LayerSet.extraLayerSet)
                defaultPrefabIndex = FindPrefabIndexByName(layer, "ConcreteBlock_Extra", false, false);

            if (GetPrefabAtIndexForLayer(defaultPrefabIndex, layer) == null)
            {
                Debug.LogWarning($"The default {GetLayerNameAsString(layer)} prefab index {defaultPrefabIndex} was also null. Using prefab at index 0 instead");
                return 0;
            }
            return defaultPrefabIndex;
        }

        //-------------------------------
        public void SetPostPrefab(int postPrefabIndex, bool doRebuild, bool resetRailPool = true)
        {
            currentPost_PrefabIndex = postPrefabIndex;
            if (currentPost_PrefabIndex == -1)
            {
                Debug.Log("Problem finding Post prefab, Post has been set to the default Prefab");
                currentPost_PrefabIndex = 1;
                currentPost_PrefabMenuIndex = ConvertPostPrefabIndexToMenuIndex(currentPost_PrefabIndex);
            }

            if (postPrefabs[currentPost_PrefabIndex].name.StartsWith("[User]"))
                useCustomPost = true;
            else
                useCustomPost = false;
            DeactivateEntirePoolForLayer(kPostLayer);
            ResetPoolForLayer(kPostLayer);

            if (doRebuild)
                ForceRebuildFromClickPoints(kPostLayer);
        }
        //-------------------------------
        public void SetExtraPrefab(int extraPrefabIndex, bool doRebuild, bool resetRailPool = true)
        {
            currentExtra_PrefabIndex = extraPrefabIndex;
            if (currentExtra_PrefabIndex == -1)
            {
                Debug.Log("Problem finding Extra prefab, Post has been set to the default Prefab");
                currentExtra_PrefabIndex = 0;
            }
            if (extraPrefabs[currentExtra_PrefabIndex].name.StartsWith("[User]"))
                useCustomExtra = true;
            else
                useCustomExtra = false;
            DeactivateEntirePoolForLayer(kExtraLayer);
            ResetPoolForLayer(kExtraLayer);

            if (doRebuild)
                ForceRebuildFromClickPoints(kExtraLayer);
        }
        //--------------------------------------------
        public GameObject SetRailPrefab(int railPrefabIndex, LayerSet layer, bool doRebuild, bool resetRailPool = true)
        {
            int layerIndex = (int)layer;
            if (railPrefabIndex == -1)
            {
                Debug.Log($"Couldn't find this {layer.String()} prefab -1. Is it a custom one that has been deleted?");
                railPrefabIndex = 0;
            }
            currentRail_PrefabIndex[layerIndex] = railPrefabIndex;
            GameObject prefab = railPrefabs[railPrefabIndex];
            if (prefab.name.StartsWith("[User]"))
                useCustomRail[layerIndex] = true;
            else
                useCustomRail[layerIndex] = false;

            if (prefab.name.EndsWith("Panel_Rail") == true || prefab.name.EndsWith("Panel") == true)
            { // always change to 'shear' for panel fences
                slopeMode[layerIndex] = SlopeMode.shear;
            }
            if (resetRailPool)
            {
                DeactivateEntirePoolForLayer(layer);
                ResetPoolForLayer(layer);
            }
            if (doRebuild)
                ForceRebuildFromClickPoints();

            return prefab;
        }
        //-----------------
        public void SetSubpostPrefab(int subpostPrefabIndex, bool doRebuild, bool resetSubpostPool = true)
        {
            if (subpostPrefabIndex == -1)
            {
                Debug.Log("Couldn't find this current subpost prefab. Is it a custom one that has been deleted?");
                subpostPrefabIndex = 0;
            }

            currentSubpost_PrefabIndex = subpostPrefabIndex;
            if (resetSubpostPool)
            {
                DeactivateEntirePoolForLayer(kSubpostLayer);
                ResetPoolForLayer(kSubpostLayer);
            }
            if (doRebuild)
                ForceRebuildFromClickPoints(kSubpostLayer);
        }
        //-------------------
        void SaveCustomRailMeshAndAddToPrefabList(GameObject customRail)
        {
            if (railPrefabs.Count == 0)
                return;
            railPrefabs.Insert(0, customRail);
        }
        //---------------------------
        public void SeedRandom(bool rebuild = true)
        {
            if (rebuild == true)
                ForceRebuildFromClickPoints();
        }
        //---------------------------
        public int GetNewRandomSeed()
        {
            int newSeed = (int)System.DateTime.Now.Ticks;
            return newSeed;
        }
        //-----------------------------
        float GetMeshHeight(GameObject go)
        {
            float height = 0;
            Mesh mesh = null;
            MeshFilter mf = (MeshFilter)go.GetComponent<MeshFilter>();
            if (mf != null)
                mesh = mf.sharedMesh;
            if (mesh != null)
                height = mesh.bounds.size.y;
            return height;
        }
        //-----------------------------
        Vector3 GetMeshMin(GameObject go)
        {
            Bounds bounds = MeshUtilitiesAFWB.GetCombinedBoundsOfAllMeshesInGameObject(go);
            return bounds.min;
        }
        //--------------------
        // lower all vec3List to ground level
        public void Ground(List<Vector3> vec3List)
        {
            //Debug.Log("Ground\n");
            //return;
            RaycastHit hit;
            Vector3 pos, posMax = Vector3.zero;
            Vector3 extraHeight = new Vector3(0, 10, 0);
            float maxY = -1000000;
            SetIgnorePartsColliders(true);
            float deltaY = .0f;
            for (int i = 0; i < vec3List.Count; i++)
            {
                pos = vec3List[i];
                if (Physics.Raycast(pos + extraHeight, Vector3.down, out hit, 500)) // First check from above, looking down
                {
                    if (hit.collider.gameObject != null)
                    {
                        if (hit.point.y > maxY)
                        {
                            deltaY = -(hit.distance - extraHeight.y);
                            pos += new Vector3(0, deltaY, 0);
                            //posMax = pos + new Vector3(0, deltaY, 0);
                        }
                    }
                }
                vec3List[i] = pos;
            }
            SetIgnorePartsColliders(false);
        }
        //---------------------------
        // Set the bottom singleVarGO/wall to be flush with ground
        public void GroundRails(LayerSet railSet, bool rebuild = true)
        {

            GameObject rail = null;
            float userHeightScale = 0;
            if (railSet == LayerSet.railALayerSet)
            {
                rail = railPrefabs[currentRail_PrefabIndex[0]];
                userHeightScale = railAScale.y;
            }
            else if (railSet == LayerSet.railBLayerSet)
            {
                rail = railPrefabs[currentRail_PrefabIndex[1]];
                userHeightScale = railBScale.y;
            }

            //TODO
            float meshHeight = GetMeshHeight(rail);
            float finalRailHeight = meshHeight * userHeightScale * globalScale.y;
            float bottom = -finalRailHeight / 2;

            if (railSet == LayerSet.railALayerSet)
                railAPositionOffset = new Vector3(railAPositionOffset.x, 0, railAPositionOffset.z);
            else if (railSet == LayerSet.railBLayerSet)
                railBPositionOffset = new Vector3(railBPositionOffset.x, 0, railBPositionOffset.z);

            ForceRebuildFromClickPoints();
        }
        //---------------------------
        // Set the bottom singleVarGO/wall to be flush with ground
        public void CentralizeRails(LayerSet railSet, bool rebuild = true)
        {

            GameObject rail = null;
            int numRails = 1;
            if (railSet == LayerSet.railALayerSet)
            {
                rail = railPrefabs[currentRail_PrefabIndex[0]];
                numRails = (int)numStackedRails[kRailALayerInt];
            }
            else if (railSet == LayerSet.railBLayerSet)
            {
                rail = railPrefabs[currentRail_PrefabIndex[1]];
                numRails = (int)numStackedRails[kRailBLayerInt];
            }

            float startHeight = 0, totalHeight = gs * postScale.y;
            float singleGapSize = totalHeight / ((numRails - 1) + 2); // +2 because we have a gap at maxGoTop and bottom

            if (numStackedRails[kRailALayerInt] > 1)
            {
                railSpread[kRailALayerInt] = singleGapSize * ((int)numStackedRails[kRailALayerInt] - 1);
                startHeight = (totalHeight / 2) - (railSpread[kRailALayerInt] / 2);
            }
            else
            {
                railSpread[kRailALayerInt] = 0.5f;
                startHeight = totalHeight / 2;
            }
            if (numStackedRails[kRailBLayerInt] > 1)
            {
                railSpread[kRailALayerInt] = singleGapSize * ((int)numStackedRails[kRailBLayerInt] - 1);
                startHeight = (totalHeight / 2) - (railSpread[kRailALayerInt] / 2);
            }
            else
            {
                railSpread[kRailALayerInt] = 0.5f;
                startHeight = totalHeight / 2;
            }

            railAPositionOffset = new Vector3(railAPositionOffset.x, startHeight, railAPositionOffset.z);
            railBPositionOffset = new Vector3(railBPositionOffset.x, startHeight, railBPositionOffset.z);
            ForceRebuildFromClickPoints();
        }

        public void ResetRail(LayerSet layer, bool controlKey, bool rebuild = true)
        {
            if (layer == LayerSet.railALayerSet)
            {
                allowHeightVariationRailA = false;
                ResetRailATransforms(rebuild: false);
            }
            else if (layer == LayerSet.railBLayerSet)
            {

                allowHeightVariationRailB = false;
                ResetRailBTransforms(rebuild: false);
            }

            allowRailRandomization[layer.Int()] = false;
            useRailVariations[layer.Int()] = false;
            slopeMode[layer.Int()] = SlopeMode.shear;
            railJointStyle[layer.Int()] = JointStyle.mitre;
            autoHideBuriedRails = false;
            numStackedRails[layer.Int()] = 1;
            railSpread[layer.Int()] = 0.5f;
            extendRailEnds[layer.Int()] = false;

            if (controlKey)
            {
                int prefabIndex = GetPrefabIndexForLayerByName(LayerSet.railALayerSet, "_A_DefaultBrickWall_Panel");
                if (prefabIndex == -1)
                    prefabIndex = GetPrefabIndexForLayerByName(LayerSet.railALayerSet, "ABasicConcrete_Panel");
                if (prefabIndex == -1)
                    prefabIndex = 0;
                SetCurrentPrefabIndexForLayer(layer, prefabIndex);
                SetMenuIndexFromPrefabIndexForLayer(prefabIndex, layer);
                ResetPoolForLayer(layer);
                ForceRebuildFromClickPoints();

            }


        }
        //---------------------------
        public void ResetRailATransforms(bool rebuild = true)
        {
            numStackedRails[kRailBLayerInt] = 1;
            railSpread[kRailALayerInt] = 0.5f;
            railAPositionOffset = new Vector3(0, 0.25f, 0);
            railAScale = Vector3.one;
            railARotation = Vector3.zero;
            overlapAtCorners = true;
            autoHideBuriedRails = false;
            slopeMode[kRailALayerInt] = SlopeMode.shear;
            GroundRails(LayerSet.railALayerSet);
            if (rebuild == true)
                ForceRebuildFromClickPoints();
        }
        //---------------------------
        public void ResetRailBTransforms(bool rebuild = true)
        {
            numStackedRails[kRailBLayerInt] = 1;
            railSpread[kRailBLayerInt] = 0.5f;
            railBPositionOffset = new Vector3(0, 0.25f, 0);
            railBScale = Vector3.one;
            railBRotation = Vector3.zero;
            overlapAtCorners = true;
            autoHideBuriedRails = false;
            slopeMode[kRailBLayerInt] = SlopeMode.shear;
            GroundRails(LayerSet.railBLayerSet);
            if (rebuild == true)
                ForceRebuildFromClickPoints();
        }
        //---------------------------
        public void ResetPostTransforms(bool rebuild = true)
        {
            postHeightOffset = 0;
            postScale = Vector3.one;
            mainPostsSizeBoost = Vector3.one;
            postRotation = Vector3.zero;
            usePostsLayer = true;
            hideInterpolated = false;

            if (rebuild == true)
                ForceRebuildFromClickPoints();
        }
        //---------------------------
        public void ResetSubpostTransforms(bool rebuild = true)
        {

            postHeightOffset = 0;

            subpostPositionOffset = Vector3.zero;
            subpostScale = Vector3.one;
            subpostRotation = Vector3.zero;

            useSubpostsLayer = true;
            //if (currentSubpost_PrefabIndex == 0)
            // currentSubpost_PrefabIndex = FindPrefabIndexByNameForLayer(PrefabTypeAFWB.postPrefab, "BrickSquare_Post");

            subSpacing = 3.0f;

            adaptSubpostToSurfaceDirection = false;
            useSubWave = false;

            if (rebuild == true)
                ForceRebuildFromClickPoints();
        }


        //---------------------------
        //- If the name starts with [User] then it's a custom prefab so give it the 'User category
        public void CheckIfSaveCopyInUser(string menuName, PrefabTypeAFWB type)
        {
            string category = "";
            if (menuName.StartsWith("[User]") || menuName.StartsWith("[U]"))
                category = "User";
            else
                return;

            menuName = category + "/" + menuName;
            if (type == PrefabTypeAFWB.railPrefab)
                railMenuNames.Add(menuName);
            else if (type == PrefabTypeAFWB.postPrefab)
                postMenuNames.Add(menuName);
            else if (type == PrefabTypeAFWB.extraPrefab)
                extraMenuNames.Add(menuName);
        }
        //----------------------------------------
        public void CreatePrefabMenuNamesForLayer(LayerSet layer)
        {
            List<string> prefabMenuNames = GetPrefabMenuNamesForLayer(layer);
            prefabMenuNames.Clear();
            List<GameObject> prefabs = GetPrefabsForLayer(layer);
            List<PrefabDetails> prefabDetailsList = PrefabDetails.GetPrefabDetailsForLayer(layer, this);
            PrefabDetails prefabDetails = null;

            int numPrefabs = prefabs.Count;
            string menuName = "", category = "", parentFolderName = "";

            for (int i = 0; i < numPrefabs; i++)
            {
                GameObject prefab = prefabs[i];
                if (CheckShouldSkip(prefab, layer))
                    continue;
                
                
                menuName = prefab.name;
                CheckIfSaveCopyInUser(menuName, PrefabTypeAFWB.postPrefab);

                if (i < prefabDetailsList.Count)
                    prefabDetails = prefabDetailsList[i];
                else
                    Debug.LogWarning($"PrefabDetailsList is shorter than the number of prefabs in the list for layer {layer.String()}");

                parentFolderName = prefabDetails.parentFolderName;
                category = AssignPresetOrPrefabCategoryByName(menuName, parentFolderName);
                if (category != "")
                {
                    menuName = category + "/" + menuName;
                }
                else
                    menuName = "Other" + "/" + menuName;
                prefabMenuNames.Add(menuName);
            }
            prefabMenuNames.Sort();
        }
        /// <summary>
        /// We've added Extras to Posts, and Posts & Rails to Extras. We will add the to the menus in a separate submenu at the end, 
        /// so don't include them while building the main menu for the layer.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        private bool CheckShouldSkip(GameObject prefab, LayerSet layer)
        {
            if ((layer == LayerSet.postLayerSet || layer == LayerSet.subpostLayerSet )&& prefab.name.Contains("_Extra"))
                return true;
            
            if (layer == LayerSet.extraLayerSet && prefab.name.Contains("_Post"))
                return true;

            if (layer == LayerSet.extraLayerSet && (prefab.name.Contains("_Rail") || prefab.name.Contains("_Panel")))
                return true;
            return false;
        }

        //----------------------------------------
        public void CreatePrefabMenuNames()
        {
            CreatePrefabMenuNamesForLayer(LayerSet.railALayerSet);
            CreatePrefabMenuNamesForLayer(LayerSet.postLayerSet);
            CreatePrefabMenuNamesForLayer(LayerSet.extraLayerSet);
            CreatePrefabMenuNamesForLayer(LayerSet.subpostLayerSet);

            AddExtrasToPostMenus();
            AddExtrasToSubpostMenus();

            AddPostsToExtrasMenu();
            AddRailsToExtraMenus();


        }
        /// <summary>
        /// // Add the extra menu items to the post menu
        /// </summary>
        private void AddExtrasToPostMenus()
        {
            List<string> extraMenuNames = GetPrefabMenuNamesForLayer(LayerSet.extraLayerSet);
            List<string> postMenuNames = GetPrefabMenuNamesForLayer(LayerSet.postLayerSet);

            for (int i = 0; i < extraMenuNames.Count; i++)
            {
                postMenuNames.Add($"Extra/{extraMenuNames[i]}");
            }
        }
        private void AddExtrasToSubpostMenus()
        {
            List<string> extraMenuNames = GetPrefabMenuNamesForLayer(LayerSet.extraLayerSet);
            List<string> subpostMenuNames = GetPrefabMenuNamesForLayer(LayerSet.subpostLayerSet);

            for (int i = 0; i < extraMenuNames.Count; i++)
            {
                subpostMenuNames.Add($"Extra/{extraMenuNames[i]}");
            }
        }
        private void AddPostsToExtrasMenu()
        {
            List<string> extraMenuNames = GetPrefabMenuNamesForLayer(LayerSet.extraLayerSet);
            List<string> postMenuNames = GetPrefabMenuNamesForLayer(LayerSet.postLayerSet);

            for (int i = 0; i < postMenuNames.Count; i++)
            {
                if(postMenuNames[i].Contains("_Extra") == false)
                    extraMenuNames.Add($" - Posts/{postMenuNames[i]}");
            }
        }
        private void AddRailsToExtraMenus()
        {
            List<string> extraMenuNames = GetPrefabMenuNamesForLayer(LayerSet.extraLayerSet);
            List<string> railMenuNames = GetPrefabMenuNamesForLayer(LayerSet.railALayerSet);

            for (int i = 0; i < railMenuNames.Count; i++)
            {
                extraMenuNames.Add($" - Rails/{railMenuNames[i]}");
            }
        }
        //---------------------------
        public string GetUserPrefabBackupPath(LayerSet layer)
        {
            int layerIndex = (int)layer;
            string path = userBackupPathPost;
            if (layer == LayerSet.railALayerSet || layer == LayerSet.railBLayerSet)
                path = userBackupPathRail[layerIndex];
            if (layer == LayerSet.extraLayerSet)
                path = userBackupPathExtra;
            return path;
        }
        //---------------------------
        public string GetCurrentPrefabPathForLayer(LayerSet layer)
        {
            GameObject go = GetMainPrefabForLayer(layer);
            //Find the location of the prefab with singleVarGO.name in the Assets folder, and create a path string
            string path = AssetDatabase.GetAssetPath(go);
            return path;
        }
        //---------------------------
        // Relaces the prefab at the current index with in go
        public void SetCurrentPrefabForLayer(GameObject go, LayerSet layer)
        {
            if (layer == LayerSet.postLayerSet)
                postPrefabs[currentPost_PrefabIndex] = go;
            else if (layer == LayerSet.railALayerSet)
                railPrefabs[currentRail_PrefabIndex[0]] = go;
            else if (layer == LayerSet.railBLayerSet)
                railPrefabs[currentRail_PrefabIndex[1]] = go;
            else if (layer == LayerSet.extraLayerSet)
                extraPrefabs[currentExtra_PrefabIndex] = go;

        }
        public GameObject GetUserObjectForLayer(LayerSet layerSet)
        {
            GameObject customObject = null;
            if (layerSet == LayerSet.postLayerSet)
                customObject = userPrefabPost;
            else if (layerSet == LayerSet.railALayerSet)
                customObject = userPrefabRail[kRailALayerInt];
            else if (layerSet == LayerSet.railBLayerSet)
                customObject = userPrefabRail[kRailBLayerInt];
            else if (layerSet == LayerSet.extraLayerSet)
                customObject = userPrefabExtra;

            return customObject;
        }

        

        public LayerMask GetIgnoreLayerMask()
        {
            LayerMask ignoreLayersMask = 0;

            // Add the defauly Unity one
            if (obeyUnityIgnoreRaycastLayer)
                ignoreLayersMask |= (1 << LayerMask.NameToLayer("Ignore Raycast"));

            // If the user has not specified 'None'
            if (ignoreRaycastsLayerName != "None")
            {
                ignoreLayersMask |= (1 << LayerMask.NameToLayer(ignoreRaycastsLayerName));
            }

            return ignoreLayersMask;
        }

    }
}


