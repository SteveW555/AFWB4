using System.Collections.Generic;
using UnityEngine;
using AFWB;
using MeshUtils;
using System.Runtime.CompilerServices;
using System;

[System.Serializable]
public struct ExtraVarsStruct
{
    public List<string> varNames;
    public List<float> varProbs;
    public List<Vector3> varScales;
}
public enum ExtrasMode
{
    normal, scatter
}
public enum ExtraPlacementMode
{
    allPostPositions = 0, nodePostPositions, endsOnly, allExceptMain, everyNthPost, everyNthPostAndSubposts, allSubposts, allPostsAndSubposts
}
public enum CornerFillMode
{
    none, arcs, linearGrid, miterLine, simple
}
/// <summary>
/// Looking down at a corner angle, aligned with the vector forward going in, A righ-hand corner will have its outer elbow
/// positioned at the top left, and a left-hand corner will have its outer elbow positioned at the bottom right
/// Its most often the outer elboow that's needed for things like Miters and Fills. The  
/// </summary>
public enum ElbowOrientation
{
    OuterAtTopLeft, OuterAtBottomRight
}
public enum ExtraTemplate
{
    PostOrnament = 0, GroundOrnament, MimicRail, MimicPost, FixedDirection
}

public enum PivotPosition
{
    Center, Center_Base = 0, Left
}

public enum FlipMode
{
    NoFlip = 0, Flip90, Flip270, Flip180
}



[System.Serializable]
public class ExtrasAFWB
{
    public AutoFenceCreator af;
    public List<Transform> extrasPool = new List<Transform>();

    public bool makeMultiArray = false, keepArrayCentral = true;
    public bool currentExtraIsPreset = true;
    public Vector3 extraTransformPositionOffset = Vector3.zero;
    public Vector3 extraTransformScale = Vector3.one;
    public Vector3 extraTransformRotation = Vector3.zero;
    public Vector3 extraGameObjectOriginalScale = Vector3.one;
    public Vector3 multiArraySize = new Vector3(1, 1, 1), multiArraySpacing = new Vector3(1, 1, 1);
    [Tooltip(AFBTooltipsText.relativeScaling)]
    public bool relativeScaling = true;
    [Tooltip(AFBTooltipsText.relativeMovement)]
    public bool relativeMovement = false;
    //[Tooltip(AFBTooltipsText.rotateToFenceDirection)]
    public bool rotateToFenceDirection = true;
    public ExtraPlacementMode extraFreqMode = ExtraPlacementMode.allPostPositions;
    public int extraFreq = 2;
    [Range(1, 12)]
    public int numExtras = 2;
    [Range(0.02f, 20f)]
    public float extrasGap = 1;
    [Tooltip(AFBTooltipsText.raiseExtraByPostHeight)]
    public bool raiseExtraByPostHeight = true;
    public bool extrasFollowIncline = true;
    [Range(0.0f, 1.0f)]
    public float chanceOfMissingExtra = 0.0f;
    public ExtrasMode extrasMode = ExtrasMode.normal; // 0= normal mode, 1 = random scatter mode
    [Range(1, 20)]
    public int numExtraSpawn = 2;
    public bool DRAWDEBUG = true, showLayoutCalc = false;
    //public int extraScatterPatternMode = 2;
    [Range(0.3f, 20f)]
    public float gridWidth = 2;
    [Range(1, 16)]
    public float numGridX = 3; // float because can't enter digits in editor box with float??
    [Range(1, 16)]
    public float numGridZ = 4;
    //[Range(0, 1)]
    //public float randomShiftWidth = 0, randomShiftLength = 0;
    public Vector3 scatterExtraRandPosRange = Vector3.zero;
    public Vector3 scatterExtraRandScaleRange = Vector3.zero;
    public Vector3 scatterExtraRandRotRange = Vector3.zero;
    [Range(0.0f, 2)]
    public float scatterRandomStrength = 1.0f;
    public int randomScaleMode;
    [Range(0f, 1f)]
    public float flipXProb = 0, flipYProb = 0, flipZProb = 0;
    public FlipMode flipMode = FlipMode.NoFlip;
    public int numExtraVars = 3;
    public int[] extraScatterVarIndex;
    public int[] extraScatterVarMenuIndex;
    [Range(0f, 1f)]
    public float[] extraScatterVarFreq, extraScatterVarShare;
    public bool useRandomScatter = true;
    public bool enablePrefabVars = false;
    public GameObject[] prefabVars;
    public Vector3[] extraVarScale;

    //List<Transform> standardPostsList = new List<Transform>();// Posts Pool i.e. not Subposts
    List<Transform> postsList = new List<Transform>();// Will be Posts or (Posts & Suposts) depending on the ExtraPlacementMode
    int numBuiltPosts = 0;
    //int numBuiltPosts = 0;//This is either numBuiltPosts or numBuiltPosts + numBuiltSubposts (postsList), depending on the ExtraPlacementMode
    //public bool usingCombinedPostsMode = false; // Will be true if ExtraPlacementMode is using both Posts and Subposts

    public bool excludeExtraVarXZRotations = true;
    //public bool omitFinal = false; //useful when offseting x to position them in middle of postsPool
    public ExtraVarsStruct extraVarsStruct = new ExtraVarsStruct();
    public float extraSurfaceNormalAmount = 0.0f;
    public bool adaptExtraToSurfaceDirection = false;
    float miterWidthScaling = 1; //the relative overlapGridWidth at miter joint elbows 
    public PivotPosition pivotPosition = PivotPosition.Center;
    public int finalPostMode = 0; //0= no change, 1 = force off, 2 = force on
    public AxisChoice3D followDirectionAxes = AxisChoice3D.XZ;
    public bool usePostToPostIncline = true;
    public bool stretchWidthAtCorners = true;
    public int cornerMode = 0; //0 = none, 1 = average, 2 = mitre
    public bool averageCornerDirection = true;
    public bool extrasFollowGroundHeight = true;
    public bool avgHeightPositionForRow = true;
    public bool alternatePostPositions = false;
    private Vector3 extraPos0 = Vector3.zero, extraPos1 = Vector3.zero, extraPos2 = Vector3.zero, extraPos3 = Vector3.zero;
    private Rect prevGroupRect = new Rect(0, 0, 0, 0);
    public int extrasBuiltCount = 0;
    public CornerFillMode cornerFillMode = CornerFillMode.none;
    public ExtraTemplate template = ExtraTemplate.MimicRail;
    public bool useExamplePrefab = true;
    public Vector3 minRandScatterScale = Vector3.one, maxRandScatterScale = Vector3.one;
    private bool scaleByDistanceToNextPost = false; //useful when emulating rails or panels

    public ExtrasAFWB(AutoFenceCreator af)
    {
        this.af = af;
        extraScatterVarIndex = new int[numExtraVars];
        extraScatterVarMenuIndex = new int[numExtraVars];
        extraScatterVarFreq = new float[numExtraVars];
        extraScatterVarShare = new float[numExtraVars];
        extraVarScale = new Vector3[numExtraVars];
        prefabVars = new GameObject[numExtraVars];
        for (int i = 0; i < numExtraVars; i++)
        {
            extraVarScale[i] = Vector3.one;
        }
        extrasPool = new List<Transform>();
        extraVarsStruct = new ExtraVarsStruct();
    }
    public void ResetExtraTransforms(bool rebuild = true)
    {

        extraTransformPositionOffset = new Vector3(0, 0, 0);
        extraTransformScale = Vector3.one;
        //mainPostsSizeBoost = Vector3.one;
        extraTransformRotation = Vector3.zero;

        rotateToFenceDirection = true;
        relativeMovement = relativeScaling = false;
        extrasFollowIncline = false;
        raiseExtraByPostHeight = true;

        makeMultiArray = false;
        numExtras = 2;
        extrasGap = 0.75f;

        useRandomScatter = false;

        if (rebuild == true)
            af.ForceRebuildFromClickPoints();
    }
    //---------------------------------
    // Prepares the struct for saving
    public void UpdateExtraVarsStruct()
    {
        extraVarsStruct.varNames = new List<string>();
        extraVarsStruct.varProbs = new List<float>();
        extraVarsStruct.varScales = new List<Vector3>();
        for (int i = 0; i < numExtraVars; i++)
        {
            extraVarsStruct.varNames.Add(GetPrefabVarNameAtIndex(i));
            extraVarsStruct.varProbs.Add(extraScatterVarFreq[i]);
            extraVarsStruct.varScales.Add(extraVarScale[i]);
        }
    }
    public string GetPrefabVarNameAtIndex(int index)
    {
        if (index < prefabVars.Length && prefabVars[index] != null)
            return prefabVars[index].name;
        return "";
    }
    //---------------------------------
    // puts the contents of a loaded struct in the current extrasPool
    public void UpdateExtrasFromExtraVariantsStruct(ExtraVarsStruct evs)
    {
        // Must have been a preset pre-scatter mode
        if (evs.varNames == null || evs.varProbs == null || evs.varScales == null ||
            evs.varNames.Count < numExtraVars || evs.varProbs.Count < numExtraVars || evs.varScales.Count < numExtraVars)
            return;

        extraVarsStruct = evs;
        for (int i = 0; i < numExtraVars; i++)
        {
            int prefabIndex = af.FindPrefabIndexByNameForLayer(PrefabTypeAFWB.extraPrefab, extraVarsStruct.varNames[i], "UpdateExtrasFromExtraVariantsStruct()");
            prefabVars[i] = af.extraPrefabs[prefabIndex];
            extraScatterVarFreq[i] = extraVarsStruct.varProbs[i];
            extraVarScale[i] = extraVarsStruct.varScales[i];

            af.ex.extraScatterVarMenuIndex[i] = af.ConvertExtraPrefabIndexToMenuIndex(prefabIndex);
        }
    }
    //---------------------------------
    public void CheckExtraPrefabVariants()
    {

        af.CheckPrefabAtIndexForLayer(af.currentExtra_PrefabIndex, LayerSet.extraLayerSet, false, "CheckExtraPrefabVariants()");
        GameObject mainExtraPrefab = af.extraPrefabs[af.currentExtra_PrefabIndex];

        bool rebuildVars = false;
        if (prefabVars == null)
        {
            Debug.Log("prefabVars is null in CheckExtraPrefabVariants(). Setting all to Main");
            rebuildVars = true;
        }
        else if (prefabVars.Length == 0)
        {
            Debug.Log("prefabVars.Length is 0 in CheckExtraPrefabVariants(). Setting all to Main");
            rebuildVars = true;
        }
        else if (prefabVars[0] == null)
        {
            Debug.Log("prefabVars[0] is null in CheckExtraPrefabVariants(). Setting all to Main");
            rebuildVars = true;
        }
        if (rebuildVars == true)
        {
            Debug.Log("Rebuilding prefabVars \n");
            prefabVars = new GameObject[] { mainExtraPrefab, mainExtraPrefab, mainExtraPrefab, mainExtraPrefab };
        }


        prefabVars[0] = mainExtraPrefab;
        for (int i = 1; i < numExtraVars; i++)
        {
            if (prefabVars[i] == null)
            {
                if (extraScatterVarMenuIndex[i] < af.extraPrefabs.Count)
                    prefabVars[i] = af.extraPrefabs[extraScatterVarMenuIndex[i]];
            }
        }
    }
    //---------------------------------
    float CalcVariantDistribution()
    {
        float total = 0;

        for (int i = 0; i < numExtraVars; i++)
        {
            total += extraScatterVarFreq[i];
        }
        for (int i = 0; i < numExtraVars; i++)
        {
            extraScatterVarShare[i] = extraScatterVarFreq[i] / total;
        }

        return total;
    }
    /// <summary>Destroys all direct children of the given transform.</summary>
    /// <param name="parent">The parent transform whose children will be destroyed.</param>
    private void DestroyAllExtraSubFolders(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
    //---------------------------------
    //-- This MUST be called after RotatePostsFinal(), it's dependent on the postsPool' data --
    public void BuildExtras()
    {
        af.CheckPostDirectionVectors();// REMOVE FOR RELEASE!
        af.arcPointsLists.Clear();

        Timer t = new Timer("BuildExtras");
        af.extrasTotalTriCount = 0;

        af.overlapExtrasZone.Clear();
        af.overlapExtrasPentagonZone.Clear();
        af.triangleZones.Clear();
        af.fillExtrasZone.Clear();

        if (af.useExtrasLayer == false || af.allPostPositions.Count == 0)
            return;
        CheckExtraPrefabVariants();
        CalcVariantDistribution();
        UpdateExtraVarsStruct();
        DestroyAllExtraSubFolders(af.extrasFolder.transform);

        //========================================================================================================
        //  For ExtraPlacementMode.subposts, Using Posts & Subposts Combined to calculate Extra Position so create a list of built Posts & Subposts
        //=========================================================================================================
        //usingCombinedPostsMode = extraFreqMode.ToString().Contains("subpost", StringComparison.OrdinalIgnoreCase);
        //if (af.useSubpostsLayer == false || af.ex.extrasMode == ExtrasMode.normal == false)
        //usingCombinedPostsMode = false;

        //standardPostsList = af.postsPool;
        numBuiltPosts = af.GetNumBuiltForLayer(LayerSet.postLayerSet);
        //numBuiltSubposts = af.GetNumBuiltForLayer(LayerSet.subpostLayerSet);

        /*if (usingCombinedPostsMode)
        {
            postsList = af.postsAndSubpostsCombined;
            //numBuiltPosts = postsList.Count;
        }
        else*/

        postsList = af.postsPool;

        //return;
        //=========================================================================================================
        //              Create Pool   (do every build as the meshes get rotated and will compound)
        //=========================================================================================================
        int numToBuild = CalculateNumberOfExtrasToBuild(af.allPostPositions.Count);
        CreateExtrasPool(numToBuild);
        /*else if (extrasMode == ExtrasMode.scatter && (flipXProb > 0 || flipYProb > 0 || flipZProb > 0)) //Scatter mode
        {
            // Destroy and rebuild Extras as they have had their meshes modified
            af.ResetPoolForLayer(LayerSet.extraLayerSet);
            Debug.Log("Rebuilding Extras Due To Flip");
        }*/

        //return;

        Transform postTransform = null;
        GameObject postGO = null;
        float midPointHeightDelta = 0;


        float maxAxisXZ = scatterExtraRandScaleRange.x;
        float maxAxisXYZ = maxAxisXZ;
        //float minScaleX = 1, maxScaleX = 1, minScaleY = 1, maxScaleY = 1, minScaleZ = 1, maxScaleZ = 1;

        if (scatterExtraRandScaleRange.z > scatterExtraRandScaleRange.x)
            maxAxisXZ = scatterExtraRandScaleRange.z;
        if (scatterExtraRandScaleRange.y > maxAxisXZ)
            maxAxisXYZ = scatterExtraRandScaleRange.y;

        Vector3 totalWorldExtraSize;
        float halfMeshHeight;
        GetRealWorldSizeAndMeshHeight(out totalWorldExtraSize, out halfMeshHeight);

        //--  Keep track of the pos, scale, rot of each Extra with a list of each of the different operations
        //--  such as basic Transfors, Random, corner fill mose etc. The sum and apply them at the end    
        //=========================================================
        /*List<Vector3> cumulativePositionsList = new List<Vector3>();
        List<Vector3> cumulativeScalesList = new List<Vector3>();
        List<Vector3> cumulativeRotationsList = new List<Vector3>();*/


        int interpostNum = 0; //this is the number of the post from the last clickpoint node, 0=clickpoint
        //int postNum = -1; //this is the number of non-subposts. Start at -1 so we can increment at the start of the loop in parallel with postNum

        // Define temporary variables for base transformations
        Vector3 basePosition = Vector3.zero;
        Quaternion baseRotation = Quaternion.identity;
        Vector3 baseScale = Vector3.one;

        //return;
        //=======================================================================================
        //
        //                                  Main Posts Loop
        //
        //=======================================================================================
        //Debug.Log($"{numBuiltPosts}  numBuiltPosts\n");


        //-- Create a block of extra between this post and the next post
        for (int postNum = 0; postNum < numBuiltPosts; postNum++) //numBuiltPosts will be numBuiltPosts or (numBuiltPosts + numBuiltSubposts), depending on ExtraPlacementMode 
        {
            //Debug.Log($"{postNum}  postNum\n");

            PostVector postVector = PostVector.GetPostVectorAtIndex(postNum);

            int clickPointNodeIndex = -1;
            bool isLastPost = false, isNextPostLastPost = false, isFirstPost = postNum == 0;
            if (postNum == numBuiltPosts - 1)
                isLastPost = true;
            if (postNum == numBuiltPosts - 2)
                isNextPostLastPost = true;

            midPointHeightDelta = 0;
            postTransform = postsList[postNum]; //Either Post or Post&Subpost depending on ExtraPlacementMode

            postGO = postTransform.gameObject;

            // if postNum is the last post
            if (finalPostMode == 1 && isLastPost) //1 = Omit final
                continue;


            //     is ClickPoint
            //=======================
            bool isClickPointNode = false, isNextPostNode = false;

            isClickPointNode = af.postVectors[postNum].IsClickPointNode;

            if (isLastPost == false)
                isNextPostNode = af.postVectors[postNum + 1].IsClickPointNode;

            clickPointNodeIndex = postVector.GetClickNodeIndex();

            //===========   Calculate number of postsPool in section (inc.current, exc.nextPos)   ============
            int numPostsInSectionNodeToNode = 0;
            if (postNum < numBuiltPosts - 1)
            {
                if (clickPointNodeIndex != -1)
                {
                    numPostsInSectionNodeToNode = af.GetNumPostsInSectionNodeToNode(clickPointNodeIndex);
                }
            }

            Vector3 autoDirEulers = Vector3.zero;
            GameObject thisExtra = null;
            float distanceToNextNodePost = 3, distanceToNextTargetPost = 3, distToPrevStdPost = 3;
            miterWidthScaling = 1;


            //==========================================
            //  Check Validity of Extra in this mode
            //==========================================
            bool addExtra = IsPostIndexUsed(postNum, numBuiltPosts, isClickPointNode);
            if (addExtra == false)
                continue;
            if (extrasMode == ExtrasMode.normal)
            {
                if (extrasPool.Count < postNum + 1 || extrasPool.Count <= extrasBuiltCount || extrasPool[postNum] == null) //Not enough extrasPool in the pool
                    continue;
                if (postNum == numBuiltPosts - 1 && relativeMovement == true && extraTransformPositionOffset.z > 0.25f) // Don't need the last post if it's been pushed past the end
                    continue;
                if (extraFreqMode == ExtraPlacementMode.allSubposts && af.useSubpostsLayer == false)
                    continue;
            }
            if (chanceOfMissingExtra > 0 && UnityEngine.Random.value <= chanceOfMissingExtra)
                continue;

            //============================================================================================
            //     Calculate all the necessary Positions and Vectors for Current, Previous and Next Posts
            //============================================================================================

            Vector3 postPos = postVector.Position;
            //Vector3 postPos = postTransform.position;



            postPos.y -= af.postHeightOffset; // make sure we're using the natural grounded position


            //      Find the Next (Target) post. i.e the one that will be used
            //      for this span of grid points from postPos to nextNodePostPos
            //===========================================================================
            //int nextNodePostIndex = 0;
            int indexOfNextTargetPost = 0, indexofNextNodePost = 0;
            Vector3 nextNodePostPos = Vector3.zero, nextTargetPostPos = Vector3.zero, prevStdPostPos = Vector3.zero;
            if (postNum < numBuiltPosts - 1)
            {
                //-- Find Next Post Post
                (nextNodePostPos, indexofNextNodePost) = FindNextNodePostPosition(postNum, postsList);
                nextNodePostPos.y -= af.postHeightOffset; // make sure we're using the natural grounded position

                if (indexofNextNodePost == -1)
                    indexofNextNodePost = postNum + 1;
                if (nextNodePostPos == Vector3.zero)
                    continue;
                distanceToNextNodePost = Vector3.Distance(postPos, nextNodePostPos);
                midPointHeightDelta = nextNodePostPos.y - postPos.y;

                //-- Find Next any useable Post
                // Target post can be a post NodePost ot SubPost depending on the ExtraPlacementMode
                indexOfNextTargetPost = FindIndexOfNextTargetPost(postNum, postsList);
                if (indexOfNextTargetPost != -1)
                {
                    nextTargetPostPos = postsList[indexOfNextTargetPost].position;
                    nextTargetPostPos.y -= af.postHeightOffset; // make sure we're using the natural grounded position
                    if (nextTargetPostPos == Vector3.zero)
                        continue;
                    distanceToNextTargetPost = Vector3.Distance(postPos, nextTargetPostPos);
                    midPointHeightDelta = nextTargetPostPos.y - postPos.y;
                }
            }

            //      Find the Previous Post.
            //===========================================================================
            //if (isStandardPost && postNum > 0)
            if (postNum > 0)
                prevStdPostPos = postsList[postNum - 1].position;

            //else if (isStandardPost == false && postNum > 0)
            //prevStdPostPos = standardPostsList[postNum].position;

            prevStdPostPos.y -= af.postHeightOffset; // make sure we're using the natural grounded position
            distToPrevStdPost = Vector3.Distance(prevStdPostPos, postPos);

            Vector3 forward = af.postVectors[postNum].Forward;//== Important - This INCLUDES the slope between the two post positions. For level vector, use flatLevelForward
            Vector3 forward2D = forward.To2D().normalized; // the forward2D direction ignoring any height level changes

            Vector3 right = af.postVectors[postNum].DirRight;
            Vector3 right2D = right.To2D();

            float headingAngle = VectorUtilitiesTCT.GetClockwiseAngle(Vector3.forward, forward);

            Vector3 postPos2D = postPos.ToY0();
            Vector3 nextPostPos2D = nextNodePostPos.ToY0();
            Vector3 prevPostPos2D = prevStdPostPos.ToY0();

            Vector3 prevForward2D = Vector3.zero;
            if (postNum > 0 && postNum < numBuiltPosts - 1)
            {
                prevForward2D = af.postVectors[postNum - 1].Forward;
                if (stretchWidthAtCorners && averageCornerDirection == true)
                    miterWidthScaling = VectorUtilitiesTCT.GetWidthAtElbow(inPt: prevPostPos2D, elbowPt: postPos2D, outPt: nextPostPos2D, width: 1.0f);
            }

            Vector2 postPos_2D = postPos.To2D();
            Vector2 nextPostPos_2D = nextNodePostPos.To2D();
            Vector2 prevPostPos_2D = prevStdPostPos.To2D();

            Vector2 forward_2D = (nextPostPos_2D - postPos_2D).normalized;
            if (isLastPost == true)
                forward_2D = (postPos_2D - prevPostPos_2D).normalized;

            Vector2 previousForward_2D = (postPos_2D - prevPostPos_2D).normalized;
            if (postNum > 0 && postNum < numBuiltPosts - 1)
                previousForward_2D = (postPos_2D - prevPostPos_2D).normalized;

            Vector2 right_2D = VectorUtilitiesTCT.GetRightFromForward2D(forward_2D);

            //=======================================================================================================
            //
            //                           Normal Mode 
            //
            //=======================================================================================================
            //Remove for release ???
            if (extrasPool.Count < numBuiltPosts)
                Debug.LogWarning("extrasPool.Count < numBuiltPosts");
            // Much of the two modes are redundantly repeated, but easier for now while debugging initial release
            if (extrasMode == ExtrasMode.normal)
            {
                thisExtra = BuildNormalMode(ref midPointHeightDelta, postNum, isLastPost, isClickPointNode,
                    ref distanceToNextNodePost, ref distToPrevStdPost, postVector, ref nextNodePostPos, ref prevStdPostPos);
            }
            //=======================================================================================================
            //
            //                          Scatter Mode 
            //
            // A Row is first drawn at the new Post/Post position, and then subsequent rows are created depending
            // on the spacing settings, up to, but not including, the Next Post/Post position. So if the direction
            // of the nextPos section continues straight forward2D, there is no overlap, i.e at the join, the new node 
            // will be responsible for drawing that row, the outgoing grid does not draw it.
            // However, at an angled corner node (Elbow), ther will be a gap on the outside of the corner.
            // If the fwdHeadingAngle is great than [theta] degrees the outgoing grid will draw the row, but ONLY the outside
            // half of the grid width.
            //
            //=======================================================================================================

            else if (extrasMode == ExtrasMode.scatter)
            {
                //return;
                // Define temporary variables for base transformations
                Vector3 tempBasePosition = Vector3.zero, tempBaseRotation = Vector3.zero;
                Vector3 tempBaseScale = Vector3.one;


                //------  Find Non-useable cases and return  ---------
                if (extrasPool.Count < postNum + 1 || extrasPool.Count <= extrasBuiltCount || extrasPool[postNum] == null)
                    continue;
                if (af.useExtrasLayer == false)
                    continue;
                if (postNum == numBuiltPosts - 1 && relativeMovement == true && extraTransformPositionOffset.z > 0.25f) // we don't need the last post if it's been pushed past the end
                    continue;
                GameObject extrasGroupedFolder = null;
                float widthCentreOffset = 0;
                int numRows = 1;

                // This is used as the base starting point, it's set to the Post position and will have the main Extra Transforms applied.
                // It's also translated right to be at the start of the row
                //GameObject tempGO = new GameObject(); //It's convenient to have a temp Transform to use for calculations. Destroy at end of function
                //Transform baseExtraTransform = tempGO.transform;

                //==============================================
                //         Scale By Transform
                //==============================================
                //baseExtraTransform.localScale = Vector3.Scale(af.nativeExtraScale, extraTransformScale);

                //==============================================
                //         Rotation By GameObject's Transform
                //==============================================

                baseRotation = Quaternion.identity;
                //baseExtraTransform.transform.rotation = Quaternion.identity;
                if (postNum == numBuiltPosts - 1 && extrasBuiltCount > 1)
                    baseRotation = extrasPool[extrasBuiltCount - 2].gameObject.transform.rotation; //for the last, copy the penultimate

                //=================================================
                //        Position By GameObject's Transform
                //=================================================
                basePosition = postPos;
                //Vector3 offsetRight = Vector3.zero;
                //Vector3 offsetForward = Vector3.zero;
                basePosition += new Vector3(0, extraTransformPositionOffset.y, 0);
                //    Translate Left to Center the Row Width
                //-------------------------------------------
                widthCentreOffset = 0;
                if (numGridX > 1)
                    widthCentreOffset = gridWidth / 2;
                basePosition -= (right * widthCentreOffset);
                if (pivotPosition == PivotPosition.Center)
                    basePosition -= new Vector3(0, halfMeshHeight, 0);


                //=======================================
                //     Calculate Main Post Height Boost
                //=======================================
                float postTopHeight = 0;
                if (raiseExtraByPostHeight == true)
                {
                    //if (isStandardPost)
                    postTopHeight = af.globalScale.y * af.postScale.y;
                    //else
                    //postTopHeight = af.globalScale.y * af.subpostScale.y;

                    if (isClickPointNode)
                        postTopHeight *= af.mainPostsSizeBoost.y;
                    postTopHeight += af.postHeightOffset;
                }
                numRows = (int)numGridZ;
                if (numGridZ == 0)
                    numRows = numPostsInSectionNodeToNode;

                if (postNum == numBuiltPosts - 1)
                    numRows = 1;

                float xSpacing = gridWidth / 2; // If there';s ony 1 per row, set it at overlapGridWidth/2 so it's in the middle
                if (numGridX > 1)
                    xSpacing = gridWidth / (float)(numGridX - 1);

                float zSpacing = distanceToNextTargetPost / (float)numRows; // The distance between each row
                float nextCornerAngle = 0, cornerAngle = af.postVectors[postNum].CornerAngle;


                Pentagon2D fillZone = null, overlapZone = null, nextOverlapZone = null;
                //=======================================================================================================
                //      Calculate Fill Zones at each Corner node - which will be the corner's Outer Elbow
                //=======================================================================================================
                fillZone = CalculateFillZones(postVector, cornerFillMode, zSpacing);

                //=======================================================================================================
                //      Calculate Overlap Zones before each Corner node    &    Fill Zones in the outer corner elbow
                //=======================================================================================================
                /*if (cornerFillMode != CornerFillMode.none && isClickPointNode && isFirstPost == false)
                {
                    overlapZone = CalculateOverlapZones2(postVector, totalWorldExtraSize, xSpacing, zSpacing);
                }*/
                if (cornerFillMode != CornerFillMode.none)
                {
                    nextOverlapZone = CalculateNextOverlapZone(postVector, totalWorldExtraSize, xSpacing, zSpacing);
                }


                //===================================
                //        Main Scatter Loop 
                //===================================

                int startRowNum = 0;
                if (isClickPointNode)
                    startRowNum = 0;
                else
                    startRowNum = 0;
                bool isFirstRowInBlock = false;
                bool isLastRowInBlock = false;

                if (isLastPost == false)
                    nextCornerAngle = af.postVectors[postNum + 1].CornerAngle;


                // If the fwdHeadingAngle is between 0 and 180, then it's a right turn meaning the outer elbow is on the left
                // and this where we potentially need to fill. In this case we're filling the
                // FIRST HALF of a row width (0  to  numGridX/2), as row items are build left to right (in fence's local space)
                //
                // If the fwdHeadingAngle is between 180 and 360, then it's a left turn meaning the outer elbow is on the right, we;'re filling the
                // SECOND HALF of a row. (numGridX/2  to  numGridX)
                bool fillLastRowLeft, fillLastRowRight;
                if (nextCornerAngle > 0 && nextCornerAngle < 180)
                {
                    fillLastRowLeft = true;
                    fillLastRowRight = false;
                }
                else
                {
                    fillLastRowLeft = false;
                    fillLastRowRight = true;
                }

                if (cornerFillMode == CornerFillMode.arcs && isNextPostNode == true && isLastPost == false)
                {
                    numRows += 1;
                }
                else if (cornerFillMode == CornerFillMode.linearGrid && isNextPostNode == true && isLastPost == false)
                {
                    // start by assuming we have to fill in another section of post-to-post rows, but we only need the right or left side of those
                    // rows depending on the corner direction
                    //numRows += numRows;
                }

                //  Prepare a folder to store each post-to-post (or node-to-node) block
                //===============================================
                GameObject folderForBlock = new GameObject("ExtrasGroupedFolder" + postNum);
                extrasGroupedFolder = PrepareFoldersExtraMode(folderForBlock);
                //=============================================================================================================================
                //                                     Loop over all of the Rows
                //============================================================================================================================
                isFirstRowInBlock = false; isLastRowInBlock = false;
                bool valid = false;
                for (float rowNum = startRowNum; rowNum < numRows; rowNum++)
                {
                    if (rowNum == startRowNum)
                        isFirstRowInBlock = true;
                    else
                        isFirstRowInBlock = false;
                    if (rowNum == numRows - 1)
                        isLastRowInBlock = true;
                    else
                        isLastRowInBlock = false;

                    //==========================================================================================================================
                    //                                Loop through Each Item in Row
                    //==========================================================================================================================
                    GameObject finalGo = null;
                    int numX = (int)numGridX, startX = 0;
                    for (float itemNum = startX; itemNum < numX; itemNum++)
                    {
                        Vector3 clonePos, cloneScale, cloneRot, pos;
                        valid = SetupItem(basePosition, baseScale, postNum, postVector, nextTargetPostPos, forward, right, numRows, xSpacing, zSpacing,
                            rowNum, itemNum, fillZone: null, out clonePos, out cloneScale, out cloneRot);
                        if (valid)
                            finalGo = FinaliseExtra(clonePos, cloneScale, cloneRot, extrasGroupedFolder);

                        //-- Remove if in overlap zone
                        if (nextOverlapZone != null && finalGo != null && VectorUtilitiesTCT.IsPointInPolygon2D(clonePos.To2D(), nextOverlapZone.v) == true)
                        {
                            finalGo.SetActive(false);
                            finalGo.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }
                }
                //=========================================
                //      Create The Fill Zone Items
                //=========================================
                if (isClickPointNode && clickPointNodeIndex > 0 && cornerFillMode == CornerFillMode.linearGrid /*&& fillZone != null*/ && isLastPost == false)
                {
                    GameObject finalGo = null;
                    numRows = (int)numGridZ;
                    int startX = 0, numX = 0;
                    int outsideFill = 0, numFills = 0;
                    for (float rowNum = 0; rowNum < numRows; rowNum++)
                    {
                        LeftRightTCT cornerDirection = postVector.LeftOrRightCorner();
                        if (cornerDirection == LeftRightTCT.right)
                        {
                            startX = 0;
                            numX = (int)numGridX / 2;
                        }
                        else
                        {
                            startX = (int)numGridX / 2;
                            numX = (int)numGridX;
                        }

                        Vector3 prevRight = postVector.GetPrevious().DirRight;
                        basePosition = postVector.Position;
                        basePosition += new Vector3(0, extraTransformPositionOffset.y, 0);
                        widthCentreOffset = gridWidth / 2;
                        basePosition -= (prevRight * widthCentreOffset);
                        //basePosition += new Vector3(0, halfMeshHeight, 0);

                        Vector3 fillForward = postVector.GetPrevious().Forward;
                        Vector3 fillRight = prevRight;

                        //baseScale.y += 1;

                        for (float itemNum = startX; itemNum < numX; itemNum++)
                        {
                            Vector3 clonePos, cloneScale, cloneRot, pos;
                            valid = SetupItem(basePosition, baseScale, postNum, postVector, nextTargetPostPos, fillForward, fillRight, numRows, xSpacing, zSpacing,
                                rowNum, itemNum, fillZone, out clonePos, out cloneScale, out cloneRot);
                            if (valid)
                            {
                                finalGo = FinaliseExtra(clonePos, cloneScale, cloneRot, extrasGroupedFolder, name: "**GapFill");
                                numFills++;
                                if (finalGo != null)
                                {
                                    //-- Remove if in overlap zone
                                    /*if (nextOverlapZone != null && VectorUtilitiesTCT.IsPointInPolygon2D(clonePos.To2D(), nextOverlapZone.v) == true)
                                    {
                                        finalGo.SetActive(false);
                                        finalGo.hideFlags = HideFlags.HideInHierarchy;
                                    }*/
                                    //-- Remove if outside of fill zone
                                    if (VectorUtilitiesTCT.IsPointInPolygon2D(clonePos.To2D(), fillZone.v) == false)
                                    {
                                        //finalGo.transform.localScale = new Vector3(finalGo.transform.localScale.x / 2.0f, finalGo.transform.localScale.y, finalGo.transform.localScale.z / 2.0f);
                                        //outsideFill++;
                                        finalGo.SetActive(false);
                                        finalGo.hideFlags = HideFlags.HideInHierarchy;
                                    }
                                }
                            }
                        }
                    }
                    //-- TODO place a single one at the outer poivot point
                    if (postVector.CornerAngle > 90 || postVector.CornerAngle < -90)
                    {
                        Vector3 outerElbowPos = postVector.Position + postVector.CalculateOuterElbowOffset2D(gridWidth / 2.0f).To3DY0();
                        Vector3 avgForwardRot = postVector.DirAvg;
                        Vector3 transBoxRot = extraTransformRotation;
                        Vector3 elbowRot = avgForwardRot + transBoxRot;
                        Vector3 elbowScale = extraTransformScale;
                        Vector3 avgHeading = new Vector3(0, postVector.GetAverageHeading(), 0);
                        finalGo = FinaliseExtra(outerElbowPos, elbowScale, avgHeading, extrasGroupedFolder, name: "**** Elbow");
                        //Debug.Log($"outsideFill: {outsideFill}    numFills: {numFills}\n");
                    }
                }
            }
            interpostNum++;
        }
        //GameObject.DestroyImmediate(tempGO);
        af.extraBuildTime += t.End(print: false);
        //Debug.Log($"extrasPool.Count:  { extrasPool.Count}       extrasBuiltCount:  { extrasBuiltCount}");

    }
    //------------------------
    private bool SetupItem(Vector3 basePosition, Vector3 baseScale, int postNum, PostVector postVector, Vector3 nextTargetPostPos, Vector3 forward, Vector3 right,
        int numRows, float xSpacing, float zSpacing, float rowNum, float itemNum, Pentagon2D fillZone, out Vector3 clonePos, out Vector3 cloneScale, out Vector3 cloneRot)
    {
        clonePos = Vector3.zero;
        cloneScale = Vector3.one;
        cloneRot = Vector3.zero;
        //==========================================================================
        //          Position  -  Set the position based on the grid spacing
        //==========================================================================

        //Vector3 forward = postVector.Forward;
        //Vector3 right = postVector.DirRight;

        clonePos = basePosition; //Start of row
                                 // Move forward2D by the spacing between each row
        clonePos += forward * zSpacing * rowNum;
        //Move Right based on the position within the row
        clonePos += right * xSpacing * itemNum;

        //=============================
        //      Transform Position 
        //=============================
        Vector3 extraPositionOffsetObjectSpace = VectorUtilitiesTCT.ConvertVectorToLocalObjectForwardSpace(forward, extraTransformPositionOffset);
        clonePos += extraPositionOffsetObjectSpace;

        //      Arc Fill
        //=======================
        bool madeArc = false;
        List<Vector2> arcPointsList = new List<Vector2>(), arcPointsDirVec = new List<Vector2>();
        //HandleArcMode(cumulativePositionsList, cumulativeScalesList, cumulativeRotationsList, postNum, isLastPost,
        //isNextPostLastPost, isNextPostNode, postPos, forward, xSpacing, zSpacing, isLastRowInBlock, fillLastRowLeft, fillLastRowRight, itemNum,
        //clonePos, baseScale, ref madeArc, ref arcPointsList, ref arcPointsDirVec);

        //  Is Valid for FillZone
        if (fillZone != null)
        {
            //if( VectorUtilitiesTCT.IsPointInPolygon2D(clonePos, fillZone) == false) return false;
        }


        //==========================
        //      Random Position 
        //==========================
        if (useRandomScatter)
        {
            Vector3 randPosOffset = GetRandomExtraPositionOffset(extrasBuiltCount);
            Vector3 randPosOffsetLocal = VectorUtilitiesTCT.ConvertVectorToLocalObjectForwardSpace(forward, randPosOffset);
            clonePos += randPosOffsetLocal;
        }

        //===========================
        //      Cull Inner Elbow 
        //===========================
        if (cornerFillMode == CornerFillMode.none)
        {
            bool isInTriangle = IsExtraInInnerElbowTriangle(postVector.Position, nextTargetPostPos, postNum, clonePos);
            if (isInTriangle && numRows > 2)
            {
                //Debug.Log($" rownNum: {rowNum}    itemNum: {itemNum}       isInTriangle: {isInTriangle} \n");
                return false;
            }
        }

        //===========================
        //     Cull Overlap Zone
        //===========================
        if (cornerFillMode == CornerFillMode.none)
        {
            bool inOverlapZone = false;
            if (postVector.GetClickNodeIndex() > 0)
                inOverlapZone = IsExtraInOverlapPentagon(clonePos);
            if (inOverlapZone && numRows > 1)// If in overlap zone, don't create the extra
                return false;
        }

        //===================================================================================
        //                                  All Scaling 
        //===================================================================================
        Vector3 scale = Vector3.one;
        baseScale = Vector3.Scale(af.nativeExtraScale, extraTransformScale);
        {
            //     Transform Scale 
            //=======================
            cloneScale = baseScale;
            //      Random Scale 
            //=======================
            if (useRandomScatter)
            {
                Vector3 randomExtraScale = GetRandomExtraScale(extrasBuiltCount);
                cloneScale = Vector3.Scale(cloneScale, randomExtraScale);
            }

            /*if (fillZone != null)
                cloneScale.y += 1;*/

        }

        //===================================================================================
        //                                  All Rotations 
        //===================================================================================
        //--  At this point List will be empty OR have rotations from Arc Fills

        //      Initialise with Transform Box Rotate 
        //================================================
        cloneRot += extraTransformRotation;

        //     Rotate To Fence Direction
        //=================================
        Vector3 autoUp = Vector3.zero;
        if (rotateToFenceDirection == true) // this should always be on except for single object placement
        {
            Vector3 anglesForward = VectorUtilitiesTCT.GetRotationAnglesFromDirection(forward);
            cloneRot += anglesForward;
        }

        //       Rotate Random
        //=================================
        if (useRandomScatter)
        {
            Vector3 randomRotations = Vector3.zero;
            randomRotations = GetRandomScatterRotations(extrasBuiltCount);
            cloneRot += randomRotations;
        }

        return true;
    }

    //------------------------------------
    private GameObject FinaliseExtra(Vector3 pos, Vector3 scale, Vector3 rotation, GameObject extrasGroupedFolder, string name = "")
    {
        //    Finalise EXtra
        //==========================
        GameObject newCloneExtra = GetExtraFromPool(extrasBuiltCount);
        newCloneExtra.transform.localRotation = Quaternion.Euler(rotation);
        if (newCloneExtra == null)
        {
            Debug.LogError($"newCloneExtra is null accessing {extrasBuiltCount} of {af.ex.extrasPool.Count - 1}");
            return null;
        }
        newCloneExtra.transform.position = pos;
        newCloneExtra.transform.localScale = scale;
        newCloneExtra.SetActive(true);
        newCloneExtra.isStatic = af.usingStaticBatching;
        newCloneExtra.hideFlags = HideFlags.None;
        newCloneExtra.name += name;
        newCloneExtra.transform.parent = extrasGroupedFolder.transform;
        extrasBuiltCount++;
        return newCloneExtra;
    }
    //--------------
    private void Other(List<Vector3> cumulativePositionsList, List<Vector3> cumulativeScalesList, List<Vector3> cumulativeRotationsList, int postNum, bool isLastPost, Vector3 autoDirEulers, Vector3 postPos, Vector3 nextNodePostPos, Vector3 prevStdPostPos, GameObject extrasGroupedFolder, float rowNum, Vector3 clonePos, Vector3 cloneScale)
    {
        /*cumulativePositionsList.Add(clonePos);
        cumulativeScalesList.Add(cloneScale);
        cumulativeRotationsList.Add(cloneRotate);*/
        //=====================================
        //    Extras Adapt to Ground Height
        //=====================================
        //AdaptToGroundHeight(cloneExtra, centerRowPosition);
        //=====================================
        //      Rotate Final
        //=====================================
        //FinalRotateExtras(isLastPost, postNum, autoDirEulers, nextNodePostPos, prevStdPostPos, postPos, cloneExtra, extrasGroupedFolder, rowNum, rotX, rotY, rotZ, rotPoint);


        //======================================
        //        Set up Colliders
        //======================================

        // af.CreateColliderForLayer(newCloneExtra, newCloneExtra.transform.localPosition, LayerSet.extraLayerSet);//do somewger else outside critical loop

        /*newCloneExtra.isStatic = af.usingStaticBatching;
        newCloneExtra.hideFlags = HideFlags.None;
        newCloneExtra.transform.parent = extrasGroupedFolder.transform;*/

        //af.extrasTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(cloneExtra); //do somewger else outside critical loop
    }



    private void HandleArcMode(List<Vector3> cumulativePositionsList, List<Vector3> cumulativeScalesList, List<Vector3> cumulativeRotationsList,
        int postNum, bool isLastPost, bool isNextPostLastPost, bool isNextPostNode, Vector3 postPos, Vector3 forward, float xSpacing, float zSpacing,
        bool isLastRowInBlock, bool fillLastRowLeft, bool fillLastRowRight, float itemNum, Vector3 clonePos, Vector3 baseScale,
        ref bool madeArc, ref List<Vector2> arcPointsList, ref List<Vector2> arcPointsDirVec)
    {
        if (isLastRowInBlock && isLastPost == false && isNextPostLastPost == false)
        {
            if (cornerFillMode == CornerFillMode.arcs && isNextPostNode == true && isLastPost == false)
            {
                //__ Is the item to the left or right of centre line. Can't just use 1st half as the extraTransformPositionOffset might have moved it
                LeftRightTCT leftOrRight = IsLeftOrRightOfVectorForward(postPos, forward, clonePos);
                //Debug.Log($"itemNum: {itemNum}      {leftOrRight} \n");


                //Debug.Log($"postNum {postNum}     rowNum {rowNum} \n");
                //if (isLastRowInBlock && fillLastRowLeft && itemNum < numX / 2)
                if (fillLastRowLeft && leftOrRight == LeftRightTCT.left)
                {
                    (arcPointsList, arcPointsDirVec) = CreateArcFill(clonePos, (int)itemNum, postNum, xSpacing, zSpacing, leftOrRight);
                }
                //else if (isLastRowInBlock && fillLastRowRight && itemNum >= numX / 2)
                else if (fillLastRowRight && leftOrRight == LeftRightTCT.right)
                {
                    (arcPointsList, arcPointsDirVec) = CreateArcFill(clonePos, (int)itemNum, postNum, xSpacing, zSpacing, LeftRightTCT.right);
                }
                int numArcPoints = arcPointsList.Count;
                for (int a = 0; a < numArcPoints; a++)
                {
                    Vector3 arcPos = arcPointsList[a].To3D();
                    cumulativePositionsList.Add(arcPos);
                    cumulativeScalesList.Add(baseScale);
                    //cumulativeRotationsList.Add(Vector3.zero);
                    float heading = VectorUtilitiesTCT.GetClockwiseAngleFromWorldForward2D(arcPointsDirVec[a]);
                    Vector3 arcRot = new Vector3(0, heading, 0);
                    cumulativeRotationsList.Add(arcRot);
                }
                if (numArcPoints > 0)
                    madeArc = true;
            }
        }
    }

    private void GetRealWorldSizeAndMeshHeight(out Vector3 totalWorldExtraSize, out float halfMeshHeight)
    {
        GameObject exampleExtra = GetExtraFromPool(0);
        totalWorldExtraSize = MeshUtilitiesAFWB.GetWorldSizeOfGameObject(exampleExtra, LayerSet.extraLayerSet, af);
        Mesh exampleMesh = MeshUtilitiesAFWB.GetMeshFromGameObject(exampleExtra);
        if (exampleMesh == null)
        {
            Debug.LogWarning("exampleMesh is null in GetRealWorldSizeAndMeshHeight()");
            halfMeshHeight = 1;
        }
        else
            halfMeshHeight = exampleMesh.bounds.size.y / 2;
    }

    //-----------------------------------------------------------------------------------------------------------------
    // Given a point P and an object-space Vector forward2D for P, test if another point Q is to the left or right of that Vector
    LeftRightTCT IsLeftOrRightOfVectorForward(Vector3 P, Vector3 forward, Vector3 Q)
    {
        Vector3 PQ = Q - P; // Vector from P to Q
        Vector3 crossProduct = Vector3.Cross(forward, PQ);

        if (crossProduct.y > 0)
            return LeftRightTCT.right; //right

        else if (crossProduct.y < 0)
            return LeftRightTCT.left; //left
        else
            return LeftRightTCT.center; // Q is neither left nor right, but inline with forward2D

    }
    private GameObject BuildNormalMode(ref float midPointHeightDelta, int postNum, bool isLastPost, bool isClickPoint,
        ref float distanceToNextNodePost, ref float distanceToPrevPost,
        PostVector postVector, ref Vector3 nextNodePostPos, ref Vector3 prevPostPos)
    {
        //----- Setup the initial object --------
        GameObject thisExtra = GetInitializedGameObjectFromPool();

        //         Setup and Scaling
        //===============================
        ScalingNormalMode(ref midPointHeightDelta, postNum, ref distanceToNextNodePost,
             postVector, ref nextNodePostPos, ref prevPostPos, thisExtra);

        //         Transform Position
        //===============================
        PositionNormalMode(isClickPoint, thisExtra, postVector, nextNodePostPos, isLastPost);

        //         Rotation
        //===============================
        RotationNormalMode(thisExtra, postVector);


        //        Set up Colliders
        //===============================
        af.CreateColliderForLayer(thisExtra, thisExtra.transform.localPosition, LayerSet.extraLayerSet);

        //         Put In Folders
        //===============================
        GameObject extrasGroupedFolder = PutInFoldersNormalMode(thisExtra);

        //         Stack
        //===============================
        StackNormalMode(thisExtra, extrasGroupedFolder);
        return thisExtra;
    }
    private GameObject GetInitializedGameObjectFromPool()
    {
        GameObject thisExtra = GetExtraFromPool(extrasBuiltCount++);
        thisExtra.SetActive(true);
        thisExtra.hideFlags = HideFlags.None;
        thisExtra.layer = 8;
        thisExtra.transform.localScale = Vector3.Scale(af.nativeExtraScale, extraTransformScale);

        //Vector3 pivotPos2D = MeshUtilitiesAFWB.GetPivotBasedOnBounds(MeshUtilitiesAFWB.GetMeshFromGameObject(thisExtra));

        //-- Although this is an Extra, it might be a Post or Rail being Repurposed
        PrefabTypeAFWB prefabType = af.InferPrefabTypeFromGoName(thisExtra);
        if (prefabType == PrefabTypeAFWB.railPrefab)
        {
            //-- As Rail meshes run in the poitive X direction instead of Z, for use as an extra, rotate mesh -90 to face positive Z
            Vector3 pivot = MeshUtilitiesAFWB.GetMeshFromGameObject(thisExtra).bounds.center + new Vector3(1.5f, 0, 0);
            MeshUtilitiesAFWB.RotateMesh(thisExtra, new Vector3(0, 90, 0), recalcBounds: true, recalcNormals: false, centre: pivot);
        }
        return thisExtra;
    }
    //------------------------------------
    private void ScalingNormalMode(ref float midPointHeightDelta, int postNum, ref float distanceToNextNodePost,
        PostVector postVector, ref Vector3 nextNodePostPos, ref Vector3 prevPostPos, GameObject thisExtra)
    {
        float postPosY = postVector.Position.y - af.postHeightOffset; // make sure we're using the natural grounded position

        float distanceToNextPost = postVector.GetDistanceToNextPost();
        PostVector nextPostVector = postVector.GetNext();

        //----- Calculate values needed if the Extra is aligned with the rail (midwy between posts ---
        if (postNum < numBuiltPosts - 1)
        {
            nextNodePostPos.y -= af.postHeightOffset; // make sure we're using the natural grounded position
            midPointHeightDelta = nextNodePostPos.y - postPosY;
        }
        if (postNum > 0)
            prevPostPos.y -= af.postHeightOffset;

        //-- Scale by Post to Post distance (like rails do)
        if (scaleByDistanceToNextPost == true && distanceToNextPost > 0)
        {
            float xScalingToNextPost = distanceToNextPost / 3.0f;
            MeshUtilitiesAFWB.ScaleMesh(thisExtra, new Vector3(1, 1, xScalingToNextPost));
        }

        Vector3 nextPostPos = postVector.Position;
        if (postVector.GetNext() != null)
        {
            nextPostPos = postVector.GetNext().Position;
        }
        Vector3 nextNextPostPos = nextPostPos;
        if (postVector.GetNextNext() != null)
        {
            nextNextPostPos = postVector.GetNextNext().Position;
        }


        Vector3 railScale = new Vector3(1, 1, 1);
        Vector3 adjustedNativeScale = new Vector3(1, 1, 1);

        Mesh mesh = MeshUtilitiesAFWB.GetMeshFromGameObject(thisExtra);


        bool nextPostIsClickPoint = false;
        if (nextPostVector != null)
            nextPostIsClickPoint = nextPostVector.IsClickPointNode;

        // Miter for v4.1
        /*if (postVector.IsClickPointNode || nextPostIsClickPoint)
        {
            af.MiterRailMesh(ref mesh, prevPostPos, postVector.Position, nextPostPos, nextNextPostPos, postNum, railScale, distanceToNextPost,
            adjustedNativeScale, useMeshDeformation: true);

            thisExtra.GetComponent<MeshFilter>().mesh = mesh;

            MeshUtilitiesAFWB.ReverseNormals(thisExtra);
        }*/

    }
    private Vector3 GetRandomScatterRotations(int i)
    {
        int randCount = af.extraSeeds.rScatterRot.Count;

        if (randCount <= i)
        {
            af.extraSeeds.GenerateRandomExtraScatterRotation();
            Debug.LogWarning($"Scatter Rotation Count {randCount} was less than the number of extras being built {i}. Building more.\n");

        }

        if (i >= randCount)
            Debug.LogWarning($"Extra Scatter Rotation Seed Count {randCount} is less than the number of extras being built {i}\n");

        af.ValidateSeedsForLayer(LayerSet.extraLayerSet);

        Vector3 randRot;
        try
        {
            randRot = af.extraSeeds.rScatterRot[i];
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError($"Index {i} is out of range for random scatter rotations {randCount}. Exception: {e.Message}\n");
            randRot = Vector3.zero; // Default to zero rotation in case of error
        }

        return randRot;
    }

    //-----------------------------------------------------------------------------------------------------------------
    private bool IsExtraInOverlapZone(Vector3 extraPos)
    {
        if (af.overlapExtrasZone.Count == 0)
            return false;
        int numQuads = af.overlapExtrasZone.Count;
        int quadNum = numQuads - 1;
        Quadrilateral2D thisQuad = af.overlapExtrasZone[quadNum];

        //-- check if cloneExtra.transform.position is inside the previous group's rect
        //Vector3 pt = new Vector3(cloneExtra.transform.position.x, 0.1f, cloneExtra.transform.position.z);
        extraPos.y = 0;
        Vector3 pt = extraPos;
        if (VectorUtilitiesTCT.IsPointInPolygon(pt, thisQuad))
        {
            if (pt.x > thisQuad.v[0].x && pt.x > thisQuad.v[1].x && pt.x > thisQuad.v[2].x && pt.x > thisQuad.v[3].x)
                Debug.Log("");
            return true;
        }
        return false;
    }
    //-----------------------------------------------------------------------------------------------------------------
    private bool IsExtraInOverlapPentagon(Vector3 extraPos)
    {
        if (af.overlapExtrasPentagonZone.Count == 0)
            return false;
        int numPents = af.overlapExtrasPentagonZone.Count;
        int pentNum = numPents - 1;
        Pentagon2D thisPent = af.overlapExtrasPentagonZone[pentNum];

        //-- check if cloneExtra.transform.position is inside the previous group's rect
        //Vector3 pt = new Vector3(cloneExtra.transform.position.x, 0.1f, cloneExtra.transform.position.z);
        extraPos.y = 0;
        Vector3 pt = extraPos;
        if (VectorUtilitiesTCT.IsPointInPentagon(pt, thisPent))
        {
            //if (pt.x > thisPent.v[0].x && pt.x > thisPent.v[1].x && pt.x > thisPent.v[2].x && pt.x > thisPent.v[3].x)
            //Debug.Log("");
            return true;
        }
        return false;
    }
    private void CalculateArcOfFill(Vector3 postPos, Vector2 forward2D, int clickpointIndex)
    {
        // print the clickpointIndex
        //Debug.Log("clickpointIndex  " + clickpointIndex + "\postNum");

        if (clickpointIndex != 1)
            return;

        Vector2 pivot = postPos.To2D();
        Vector2[] v = new Vector2[4];
        float halfWidth = gridWidth / 2;

        //clockwise from pos, pos is bottom right of the fill area
        v[0] = pivot; //bottom right
        v[1] = pivot + new Vector2(-halfWidth, 0);//bottom left
        v[2] = pivot + new Vector2(-halfWidth, -halfWidth);//top left
        v[3] = pivot + new Vector2(0, -halfWidth);//top right


        Vector2 arcOrigin = v[0];
        Vector2 arcStartPoint = v[3];
        Vector2 arcEndPoint = v[1];
        float arcRadius = halfWidth;


    }

    private (List<Vector2>, List<Vector2>) CreateArcFill(Vector3 itemPos, int itemNum, int postIndex, float xSpacing, float zSpacing, LeftRightTCT leftOrRight)
    {
        List<Vector2> arcPointsList = new List<Vector2>();
        if (postIndex >= af.postVectors.Count - 1)
            return (arcPointsList, null); //better than returning null so we can get a count of zero

        // itemPos is the position of this extra of the last row of the current block.
        // We need to calculate where the matching itemNum is on the first row of the nextPos block
        int nextPostIndex = postIndex + 1;
        Vector3 arcStartPoint = Vector3.zero, arcEndPoint = Vector3.zero;
        Vector3 nextForward = af.postVectors[nextPostIndex].Forward;
        Vector3 nextPostPos = af.postVectors[nextPostIndex].Position;
        Vector3 forward = af.postVectors[postIndex].Forward;
        Vector2 forward2D = forward.To2D();
        Vector2 worldForward2D = new Vector2(0, 1);
        float heading = VectorUtilitiesTCT.GetClockwiseAngleFromWorldForward2D(forward2D);

        Vector3 arcOrigin = nextPostPos;
        float arcRadius = 0;

        //Is item left of center 
        //if (itemNum < numGridX / 2)
        if (leftOrRight == LeftRightTCT.left)
        {
            //Calculate the position of the matching itemNum on the first row of the nextPos block
            float distanceFromNode = (gridWidth / 2) + (itemNum * xSpacing);


            Vector3 nextBlockItemPos = af.postVectors[nextPostIndex].CalculatePositionToLeftAtDistance(distanceFromNode);
            arcStartPoint = itemPos;
            arcEndPoint = nextBlockItemPos;
            arcRadius = Vector3.Distance(arcOrigin, arcStartPoint);
        }
        else if (leftOrRight == LeftRightTCT.right)
        {
            float distanceFromStartOfRow = (itemNum * xSpacing);
            float distanceFromNode = distanceFromStartOfRow + (gridWidth / 2);
            Vector3 nextBlockItemPos = af.postVectors[nextPostIndex].CalculatePositionToRightAtDistance(distanceFromNode);
            arcStartPoint = itemPos;
            arcEndPoint = nextBlockItemPos;
            arcRadius = Vector3.Distance(arcOrigin, arcStartPoint);
        }

        arcPointsList = ArcTCT.CreateArcWithSegmentDistance(arcOrigin.To2D(), arcStartPoint.To2D(), arcEndPoint.To2D(), arcRadius, zSpacing);
        //Remove the first and last as they already exist
        if (arcPointsList.Count > 0)
            arcPointsList.RemoveAt(0);
        if (arcPointsList.Count > 0)
            arcPointsList.RemoveAt(arcPointsList.Count - 1);

        //Optional - Get the direction of the arc point

        List<Vector2> arcPointsDirVectors = new List<Vector2>();
        for (int i = 0; i < arcPointsList.Count; i++)
        {
            // Vector going from the Pivot to the arc point
            Vector2 arcPointDirVecObj2D;
            Vector2 arcPointVecFromOrigin = (arcPointsList[i] - arcOrigin.To2D()).normalized; // vector from arcOrigin to arcStartPoint

            // Rotate this by the heading angle
            // Use Quaternions:
            /*Quaternion rotation = Quaternion.Euler(0, heading, 0);
            Vector3 arcPointDirVecObj3D = rotation * arcPointVecFromOrigin.To3D().normalized;
            Vector2 arcPointDirVecObj2D = arcPointDirVecObj3D.To2D();*/

            // Or use Trig:
            float cosTheta = Mathf.Cos((heading + 90) * Mathf.Deg2Rad);
            float sinTheta = Mathf.Sin((heading + 90) * Mathf.Deg2Rad);
            float x = arcPointVecFromOrigin.x * cosTheta - arcPointVecFromOrigin.y * sinTheta;
            float y = arcPointVecFromOrigin.x * sinTheta + arcPointVecFromOrigin.y * cosTheta;
            arcPointDirVecObj2D = new Vector2(x, y);

            //Vector3(v.y, 0, -v.x);
            /*if (leftOrRight == LeftRightTCT.right)
                arcPointDirVec2D = -arcPointDirVec2D;*/

            if (leftOrRight == LeftRightTCT.left)
                arcPointDirVecObj2D = -arcPointDirVecObj2D;

            //arcPointsDirVectors.Add(arcPointDirVec2D);
            arcPointsDirVectors.Add(arcPointDirVecObj2D);
        }

        //af.arcPointsLists.Add(arcPointsList);

        return (arcPointsList, arcPointsDirVectors);
    }
    //-----------------------------------------------------------------------------------------------------------------
    private bool IsExtraInInnerElbowTriangle(Vector3 postPos, Vector3 nextPostPos, int postIndex, Vector3 extraPos)
    {
        if (postIndex >= numBuiltPosts - 1)
            return false;

        PostVector postVector = af.postVectors[postIndex];
        PostVector nextPostVector = postVector.GetNext();

        float widthPadding = 0.1f;
        float halfWidth = (gridWidth / 2) + widthPadding;
        float halfWidthSigned = halfWidth;

        if (af.postVectors[postIndex + 1].CornerAngle < 0 || af.postVectors[postIndex + 1].CornerAngle > 180)
            halfWidthSigned = -halfWidthSigned;


        Vector2 pivot = nextPostPos.To2D();
        Vector2 rightUpperCorner = new Vector2(halfWidthSigned, 0);


        Vector2 rightUpperCornerObjSpace = pivot + postVector.ConvertVectorToPostForwardSpace(rightUpperCorner.To3D()).To2D();

        Vector2 innerElbow = nextPostVector.CalculateInnerElbowOffset2D(halfWidth);
        innerElbow += pivot;

        Vector2[] pts = new Vector2[3];
        Vector2 forward2D = postVector.Forward.To2D().normalized;

        pts[0] = pivot + (forward2D * .5f);
        pts[1] = rightUpperCornerObjSpace;
        pts[2] = innerElbow;

        float cornerAngle = nextPostVector.CornerAngle;
        LeftRightTCT leftOrRightCorner = nextPostVector.LeftOrRightCorner();

        //Debug.Log($"leftOrRightCorner:    {leftOrRightCorner} \n");

        // Shift the zone to the left or right depending on the extraTransformPositionOffset.x transform settings
        float sign = 1;
        if (cornerAngle < 0 || cornerAngle > 180)
            sign = -1;


        bool cullIsRedundant = false;
        float xOffset = extraTransformPositionOffset.x;
        //Move the culling triangle if the extraTransformPositionOffset.x is causing an inner overlap
        if (extraTransformPositionOffset.x > 0 && leftOrRightCorner == LeftRightTCT.right ||
            extraTransformPositionOffset.x < 0 && leftOrRightCorner == LeftRightTCT.left)

        {
            for (int i = 0; i < 3; i++)
            {
                pts[i] += new Vector2(extraTransformPositionOffset.x, 0);
            }
            /*if (Mathf.Abs(xOffset) > gridWidth*.75f)
                return false;*/
        }
        //--if the extraTransformPositionOffset.x is causing an inner overlap greater than the width, just remove this culling zone


        bool isInTriangle = VectorUtilitiesTCT.IsPointInTriangle(extraPos.To2D(), pts[0], pts[1], pts[2]);

        Triangle2D tri = new Triangle2D(pts[0], pts[1], pts[2]);
        af.triangleZones.Add(tri);

        return isInTriangle;
    }
    //------------------
    /*public static Vector2[] WorldToLocalTransformPoints(Transform transform, Vector2[] worldPoints)
    {
        Vector2[] localPoints = new Vector2[worldPoints.Length];

        for (int i = 0; i < worldPoints.Length; i++)
        {
            // Convert Vector2 to Vector3, assuming Y=0
            Vector3 worldPoint3D = new Vector3(worldPoints[i].x, 0, worldPoints[i].y);
            // Convert world space point to local space
            Vector3 localPoint3D = transform.InverseTransformPoint(worldPoint3D);
            // Convert back to Vector2
            localPoints[i] = new Vector2(localPoint3D.x, localPoint3D.z);
        }
        return localPoints;
    }*/
    /* public static Vector2[] WorldToLocalVectorForwardPoints2(Vector3 forward, Vector2[] worldPoints)
     {
         Vector2[] localPoints = new Vector2[worldPoints.Length];
         //-- Calculate the rotation needed to align with the forward direction
         Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
         Quaternion inverseRotation = Quaternion.Inverse(rotation);
         for (int i = 0; i < worldPoints.Length; i++)
         {
             //-- Convert Vector2 to Vector3, assuming Y=0
             Vector3 worldPoint3D = new Vector3(worldPoints[i].x, 0, worldPoints[i].y);
             //-- Apply the inverse rotation to transform the point to the local space
             Vector3 localPoint3D = inverseRotation * worldPoint3D;
             //-- Convert back to Vector2
             localPoints[i] = new Vector2(localPoint3D.x, localPoint3D.z);
         }
         return localPoints;
     }*/
    public static Vector2[] WorldToLocalVectorForwardPoints(Vector3 forward, Vector2[] worldPoints)
    {
        Vector2[] localPoints = new Vector2[worldPoints.Length];

        // Calculate the angle between the forward direction and the world forward (0, 0, 1)
        float angle = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
        angle -= 90; // Rotate 90 degrees to align with the Z axis
        Quaternion rotation = Quaternion.Euler(0, -angle, 0); // Use negative angle for inverse rotation

        for (int i = 0; i < worldPoints.Length; i++)
        {
            // Convert Vector2 to Vector3, assuming Y=0
            Vector3 worldPoint3D = new Vector3(worldPoints[i].x, 0, worldPoints[i].y);
            // Apply the rotation to transform the point to the local space
            Vector3 localPoint3D = rotation * worldPoint3D;
            // Convert back to Vector2
            localPoints[i] = new Vector2(localPoint3D.x, localPoint3D.z);
        }

        return localPoints;
    }


    //---------------------------------------------------------------------
    private Pentagon2D CalculateFillZones(PostVector postVector, CornerFillMode cornerFillMode, float zSpacing)
    {
        //--  Manage early exit conditions  --
        if (postVector == null)
        { Debug.Log("postVector is null in CalculateFillZones()"); return null; }
        if (cornerFillMode == CornerFillMode.none) return null;
        if (postVector.IsFirstOrLastPost()) return null;
        int clickNodeIndex = postVector.GetClickNodeIndex();
        if (clickNodeIndex < 1)
            return null;

        //-- Define some useful points
        Vector2[] v = new Vector2[4];
        float halfWidth = gridWidth / 2;
        Vector3 postPos = postVector.Position;
        Vector2 pivotPos2D = postPos.To2D();

        Vector2 outerElbow = postVector.CalculateOuterElbowOffset2D(halfWidth);
        Vector2 innerElbow = postVector.CalculateInnerElbowOffset2D(halfWidth);

        // The area we need to fill is between:
        // -the item on the last row (left or right depending on the corner)
        // -the outer elbow
        // -the item on the first row of the next block (left or right)
        // -the pivot
        // -the center position of the last row


        PostVector prevPostVector = postVector.GetPrevious();
        //-- Get the position of the item on the left of the first last ougoing row
        Vector2 dirBackwardPrev = -prevPostVector.Forward.To2D();
        Vector2 dirRightPrev = prevPostVector.DirRight.To2D();
        Vector2 dirLeftPrev = -dirRightPrev;
        Vector2 centerOfLastRow = dirBackwardPrev * zSpacing;
        Vector2 dirRightNext = postVector.DirRight.To2D();
        Vector2 dirLeftNext = -dirRightNext;


        ElbowOrientation elbowOrient = postVector.GetElbowMode();
        Vector2 itemOnLastRowPos = Vector2.zero, itemOnNextRowPos = Vector2.zero;
        Vector2[] fillV = new Vector2[5];
        if (elbowOrient == ElbowOrientation.OuterAtTopLeft) // For a right hand turn
        {
            itemOnLastRowPos = centerOfLastRow + (dirLeftPrev * (halfWidth + .01f)); // +.01f to include the outer edge
            itemOnNextRowPos = (dirLeftNext * halfWidth);
        }
        if (elbowOrient == ElbowOrientation.OuterAtBottomRight) // For a right hand turn
        {
            itemOnLastRowPos = centerOfLastRow + (dirRightPrev * (halfWidth + .01f));
            itemOnNextRowPos = (dirRightNext * halfWidth);
        }

        //-- All points are relaive to the pivotPost
        fillV[0] = itemOnLastRowPos * 1.0f;
        fillV[1] = outerElbow * 1.0f;
        fillV[2] = itemOnNextRowPos;
        fillV[3] = Vector2.zero; //the pivot position that everything is relative to
        fillV[4] = centerOfLastRow;
        fillV = VectorUtilitiesTCT.OffsetVector2Array(fillV, pivotPos2D);

        Pentagon2D fillZone = new Pentagon2D(fillV);
        af.fillExtrasZone.Add(fillZone);


        //-- A 4 point polygon which will define the fill zone.
        //-- Positions are in world space forward (0,0,1) relative to the Post Pivot, will be offset to world space later
        /*if (cornerFillMode == CornerFillMode.linearGrid)
        {
            if (elbowOrient == ElbowOrientation.OuterAtTopLeft) // For a right hand turn
            {
                v[0] = new Vector2(-halfWidth, +halfWidth); ; //Fwd,  Left
                v[1] = new Vector2(0, +halfWidth);//Fwd, inline with Pivot
                v[2] = new Vector2(0, 0);// Pivot.x, Pivot.y
                v[3] = new Vector2(-halfWidth, 0);//level with pivot,  Left}
            }
        }

        //-- Offset the points to the post's position
        v = VectorUtilitiesTCT.OffsetVector2Array(v, pivotPos2D);

        //-- Rotate the points to the *Previous* post's forward direction, this will extend them in the continued direction of Previous -> Current
        postVector.RotatePointsToPreviousPostForward(v);



        Quadrilateral2D fillQuad = new Quadrilateral2D(v);
        af.fillExtrasZone.Add(fillQuad);*/

        //if (clickNodeIndex < 3)
        {
            outerElbow += pivotPos2D;
            innerElbow += pivotPos2D;
            UprightMarker outerElbowMarker = new UprightMarker(outerElbow.To3D(), color: Color.green);
            UprightMarker innerElbowmarker = new UprightMarker(innerElbow.To3D(), color: Color.blue);
            af.uprightMarkers.Add(outerElbowMarker);
            af.uprightMarkers.Add(innerElbowmarker);
        }

        return fillZone;
    }
    //--------------------------------------------------------------------
    //zSpacing or totalWorldExtraSize aren't crucial but help pad the overlap zones suitably for the settings
    private Pentagon2D CalculateOverlapZones2(PostVector postVector, Vector3 totalExtraSize, float xSpacing, float zSpacing)
    {
        if (postVector == null)
        {
            Debug.Log("postVector is null in CalculateOverlapZones()");
            //af.overlapExtrasPentagonZone.Add(new Pentagon2D());
            return null;
        }
        if (postVector.IsFirstOrLastPost())
            return null;

        //-- Get positions to use in building the zone
        Vector3 postPos = postVector.Position;
        Vector2 pivot2D = postPos.To2D();
        PostVector nextPostVector = postVector.GetNext(), prevPostVector = postVector.GetPrevious();
        Vector2 prevPostPos2D = prevPostVector.Position.To2D();
        int postIndex = postVector.Index();
        int clickNodeIndex = postVector.GetClickNodeIndex();
        bool isLastPost = postIndex == af.allPostPositions.Count - 1;
        Vector2 forward2D = postVector.Forward.To2D();

        //if (postIndex != 2) return;

        //-- A 5 point polygon which will define the overlap zone
        Vector2[] polygonPts = new Vector2[5];

        //-- Some size to define the dimensions of the overlap zone
        float widthPadding = xSpacing + 0.1f; //xSpaving is the distance between each item in a row. TODO include Extra Size
        float overlapGridWidth = gridWidth + widthPadding;
        float halfWidth = overlapGridWidth / 2;
        float postToPostDistance = Vector2.Distance(pivot2D, prevPostPos2D);
        float overlapGridLength = postToPostDistance * 2f;
        float halfLength = overlapGridLength / 2;
        float extendIncomingLastRow = (zSpacing + totalExtraSize.z) / 2; // extends the overlap rect a little into the nextPos zone to prevent close overlaps
        Vector2 innerElbowOffset = postVector.CalculateInnerElbowOffset2D(halfWidth);
        // Vector2 innerElbowPos2D = pivot2D + InnerElbowOffset;
        float cornerAngle = postVector.CornerAngle;
        CornerOrientation cornerDir = postVector.GetCornerOrientation();

        //     First Find Points to define the protected area of the Group between the Previous and Current Post. Pts are relative offsets from the Post Pivot
        //------------------------------------------------------------------------------------------------------------

        //-- Point[0] is initially Forward & Left of the Post Pivot. May become inner elbow if Corner is going Left. R (color in the Debug Draw)
        polygonPts[0] = new Vector2(-halfWidth, halfLength + extendIncomingLastRow);
        //-- Point[1] is Centrally Forward of Post Pivot. G
        polygonPts[1] = new Vector2(0, halfLength + extendIncomingLastRow);
        //-- Point[2] is Initiall Forward & Left, bt may becaome an inner elbow if this is a Corner angle Right (clockwise) . B
        polygonPts[2] = new Vector2(halfWidth, halfLength + extendIncomingLastRow);  //inner elbow
        //-- Point[3] is Behind & Right of  Post Pivot. C 
        polygonPts[3] = new Vector2(halfWidth, -halfLength);
        //-- Point[4] is Behind & Left. M
        polygonPts[4] = new Vector2(-halfWidth, -halfLength);

        //-- The point used as the elbow will differ depending on the left/right corner angle
        if (cornerDir == CornerOrientation.cornerRight && isLastPost == false)
            polygonPts[2] = innerElbowOffset;
        else if (cornerDir == CornerOrientation.cornerLeft && isLastPost == false)
            polygonPts[0] = innerElbowOffset;


        float distanceToNextPost = postVector.GetDistanceToNextPost();

        if (cornerFillMode == CornerFillMode.linearGrid)
        {
            halfWidth = gridWidth / 2;

            //polygonPts[0] = new Vector2(-halfWidth, halfWidth); //Fwd Left
            //polygonPts[1] = new Vector2(halfWidth, halfWidth); //Fwd Right
            polygonPts[0] = new Vector2(-halfWidth, distanceToNextPost); //Fwd Left
            polygonPts[1] = new Vector2(halfWidth, distanceToNextPost); //Fwd Right



            polygonPts[2] = new Vector2(halfWidth, 0); //level, Right
            polygonPts[3] = new Vector2(0, 0); // the pivot point itself. We could skip it but for consitence always use a 5-point zone
            polygonPts[4] = new Vector2(-halfWidth, 0); //Level, Left

            polygonPts = WorldToLocalVectorForwardPoints(postVector.Forward, polygonPts);

        }


        //-- Transform the points to the local Post;s forward direction
        //polygonPts = WorldToLocalVectorForwardPoints(-postVector.Forward, polygonPts);

        /*if (cornerAngle > 90)
            polygonPts[2] = polygonPts[3];*/

        Pentagon2D pentagon = new Pentagon2D(polygonPts);
        pentagon.OffsetPentagon(postPos);

        // Shift the zone depending on the Extra Transform Position Offset
        //pentagon.OffsetPentagon(extraTransformPositionOffset.x, 0);

        af.overlapExtrasPentagonZone.Add(pentagon);

        postVector.GetClickNodeIndex();
        //pentagon.Print();
        return pentagon;
    }
    private Pentagon2D CalculateNextOverlapZone(PostVector postVector, Vector3 totalExtraSize, float xSpacing, float zSpacing)
    {
        if (postVector == null)
        {
            Debug.Log("postVector is null in CalculateOverlapZones()");
            return null;
        }
        if (postVector.IsLastPost())
            return null;

        PostVector nextPostVector = postVector.GetNext();

        //-- allPostPositions
        if (extraFreqMode == ExtraPlacementMode.allPostPositions && postVector.GetNext().IsClickPointNode == false)
            nextPostVector = postVector.GetNextNodePost();



        if (extraFreqMode == ExtraPlacementMode.nodePostPositions)
            nextPostVector = postVector.GetNextNodePost();


        //-- Get positions to use in building the zone
        Vector3 postPos = nextPostVector.Position;
        Vector2 pivot2D = postPos.To2D();
        PostVector prevPostVector = nextPostVector.GetPrevious();
        Vector2 prevPostPos2D = prevPostVector.Position.To2D();
        int postIndex = nextPostVector.Index();
        int clickNodeIndex = nextPostVector.GetClickNodeIndex();
        bool isLastPost = postIndex == af.allPostPositions.Count - 1;
        Vector2 forward2D = nextPostVector.Forward.To2D();

        //if (postIndex != 2) return;

        //-- A 5 point polygon which will define the overlap zone
        Vector2[] polygonPts = new Vector2[5];

        //-- Some size to define the dimensions of the overlap zone
        float widthPadding = xSpacing + 0.1f; //xSpaving is the distance between each item in a row. TODO include Extra Size
        float overlapGridWidth = gridWidth + widthPadding;
        float halfWidth = overlapGridWidth / 2;
        float postToPostDistance = Vector2.Distance(pivot2D, prevPostPos2D);
        float overlapGridLength = postToPostDistance * 2f;
        float halfLength = overlapGridLength / 2;
        float extendIncomingLastRow = (zSpacing + totalExtraSize.z) / 2; // extends the overlap rect a little into the nextPos zone to prevent close overlaps
        Vector2 innerElbowOffset = nextPostVector.CalculateInnerElbowOffset2D(halfWidth);
        // Vector2 innerElbowPos2D = pivot2D + InnerElbowOffset;
        float cornerAngle = nextPostVector.CornerAngle;
        CornerOrientation cornerDir = nextPostVector.GetCornerOrientation();

        //     First Find Points to define the protected area of the Group between the Previous and Current Post. Pts are relative offsets from the Post Pivot
        //------------------------------------------------------------------------------------------------------------

        //-- Point[0] is initially Forward & Left of the Post Pivot. May become inner elbow if Corner is going Left. R (color in the Debug Draw)
        polygonPts[0] = new Vector2(-halfWidth, halfLength + extendIncomingLastRow);
        //-- Point[1] is Centrally Forward of Post Pivot. G
        polygonPts[1] = new Vector2(0, halfLength + extendIncomingLastRow);
        //-- Point[2] is Initiall Forward & Left, bt may becaome an inner elbow if this is a Corner angle Right (clockwise) . B
        polygonPts[2] = new Vector2(halfWidth, halfLength + extendIncomingLastRow);  //inner elbow
        //-- Point[3] is Behind & Right of  Post Pivot. C 
        polygonPts[3] = new Vector2(halfWidth, -halfLength);
        //-- Point[4] is Behind & Left. M
        polygonPts[4] = new Vector2(-halfWidth, -halfLength);

        //-- The point used as the elbow will differ depending on the left/right corner angle
        if (cornerDir == CornerOrientation.cornerRight && isLastPost == false)
            polygonPts[2] = innerElbowOffset;
        else if (cornerDir == CornerOrientation.cornerLeft && isLastPost == false)
            polygonPts[0] = innerElbowOffset;


        //float distanceToNextPost = nextPostVector.GetDistanceToNextPost();
        // Get the Next Clickpoint Node Post so we can extend the overlap zone that fat
        //PostVector nextNextNodePost = nextPostVector.GetNextNodePost();
        float distanceToNextNode = nextPostVector.GetDistanceToNextNode();
        if (distanceToNextNode == 0)
            distanceToNextNode = nextPostVector.GetDistanceToNextPost();

        if (cornerFillMode == CornerFillMode.linearGrid)
        {
            halfWidth = gridWidth / 2;

            polygonPts[0] = new Vector2(-halfWidth, distanceToNextNode); //Fwd Left
            polygonPts[1] = new Vector2(halfWidth, distanceToNextNode); //Fwd Right

            polygonPts[2] = new Vector2(halfWidth, 0); //level, Right
            polygonPts[3] = new Vector2(0, 0); // the pivot point itself. We could skip it but for consitence always use a 5-point zone
            polygonPts[4] = new Vector2(-halfWidth, 0); //Level, Left

            polygonPts = WorldToLocalVectorForwardPoints(nextPostVector.Forward, polygonPts);
        }
        Pentagon2D pentagon = new Pentagon2D(polygonPts);
        pentagon.OffsetPentagon(postPos);
        pentagon.ExpandPentagon(1.01f);

        af.overlapExtrasPentagonZone.Add(pentagon);

        //nextPostVector.GetClickNodeIndex();
        //pentagon.Print();
        return pentagon;


    }
    //--------------------------------------------------------------------
    //zSpacing or totalWorldExtraSize aren't crucial but help pad the overlap zones suitably for the settings
    private void CalculateOverlapZones(Vector3 postPos, int postIndex, Vector3 totalExtraSize, float xSpacing, float zSpacing, float cornerAngle)
    {
        if (af.allPostPositions.Count < 2 || postIndex == 0 /*|| postNum == af.postVectors.Count-1*/)
            return;

        //=====================================================================
        //         Calculate the Overlap Area of the Previous Post Group
        //=====================================================================
        //-- This is the rectangle near the end of the incoming group that 
        //-- the start of incoming group can overlap, depending on the corner fwdHeadingAngle
        PostVector postVector = af.postVectors[postIndex];
        PostVector nextPostVector = postVector.GetNext(), prevPostVector = postVector.GetPrevious();

        Vector2 pivot = postPos.To2D();
        Vector2 prevPostPos2D = prevPostVector.Position.To2D();// we already returned if postNum == 0
        float postToPostDistance = Vector2.Distance(pivot, prevPostPos2D);
        Vector2 incomingLocalForward = prevPostVector.Forward.To2D();
        Vector2 worldForward2D = new Vector2(0, 1);

        float widthPadding = xSpacing + 0.1f; //TODO include Extra Size
        float overlapGridWidth = gridWidth + widthPadding;
        //float overlapGridLength = postToPostDistance > overlapGridWidth ? postToPostDistance : overlapGridWidth; // arbitrary at the moment, but covers most cases for now
        float overlapGridLength = postToPostDistance * 2f;


        float halfWidth = overlapGridWidth / 2;
        float halfLength = overlapGridLength / 2;
        // Ignore position initiall and just get the relative pos correct, makes it easier to align to local forward2D
        // Go clockwise from top left
        Vector2[] pts = new Vector2[4];
        Vector2[] pentPts = new Vector2[5];


        float extendIncomingLastRow = (zSpacing + totalExtraSize.z) / 2; // extends the overlap rect a little into the nextPos zone to prevent close overlaps
                                                                         //extendIncomingLastRow = 0.1f;

        float halfWidthSigned = halfWidth;
        Vector2 innerElbowPos2D = Vector2.zero;


        Vector2 center = Vector2.zero;

        float fwdHeadingAngle = Vector2.SignedAngle(worldForward2D, incomingLocalForward);
        float cos = Mathf.Cos(fwdHeadingAngle * Mathf.Deg2Rad);
        float sin = Mathf.Sin(fwdHeadingAngle * Mathf.Deg2Rad);
        Vector2 dir;

        pentPts[0] = new Vector2(-halfWidth, halfLength + extendIncomingLastRow); // to the left of the pos
        pentPts[1] = new Vector2(0, halfLength + extendIncomingLastRow);       //pos
        pentPts[2] = new Vector2(halfWidth, halfLength + extendIncomingLastRow);  //inner elbow
        pentPts[3] = new Vector2(halfWidth, -halfLength); //
        pentPts[4] = new Vector2(-halfWidth, -halfLength);

        if (cornerAngle > 90)
            pentPts[2] = pentPts[3];

        if (postVector.CornerAngle >= 0 && postVector.CornerAngle <= 180)
        {
            if (postIndex < numBuiltPosts - 1)
            {
                innerElbowPos2D = postVector.Position.To2D() + postVector.CalculateInnerElbowOffset2D(halfWidth);
                pentPts[2] = innerElbowPos2D + new Vector2(0, halfLength);
            }
        }
        else
        {
            innerElbowPos2D = postVector.Position.To2D() + postVector.CalculateInnerElbowOffset2D(halfWidth);
            pentPts[0] = innerElbowPos2D + new Vector2(0, halfLength);
        }

        //If extraTransformPositionOffset.x is causing an inner overlap, move the adjust overlap zone




        // Shift the zone to the left or right depending on the extraTransformPositionOffset.x transform settings
        for (int i = 0; i < 5; i++)
        {
            pentPts[i] += new Vector2(extraTransformPositionOffset.x, 0);
        }

        for (int i = 0; i < 5; i++)
        {
            dir = (pentPts[i] - center);
            pentPts[i] = center + new Vector2(
                dir.x * cos - dir.y * sin,
                dir.x * sin + dir.y * cos
            );
        }
        for (int i = 0; i < 5; i++)
        {
            pentPts[i] += pivot;
            pentPts[i] -= (incomingLocalForward * halfLength);
        }
        Pentagon2D pentagon = new Pentagon2D(pentPts[0], pentPts[1], pentPts[2], pentPts[3], pentPts[4]);
        af.overlapExtrasPentagonZone.Add(pentagon);


        //  Calculate th point of intersection between the two blocks edges at the inside of the corner
        /*float halfA = cornerAngle / 2;
        float rad = halfA * Mathf.Deg2Rad;
        float tanRad = Mathf.Tan(rad);
        float lengthToIntersection = halfWidth * tanRad;
        Vector2 PI = new Vector2(halfWidth, -lengthToIntersection); //point of intersection
        dir = PI;
        Vector2 p = center + new Vector2(
                dir.x * cos - dir.y * sin,
                dir.x * sin + dir.y * cos
            );*/

        innerElbowPos2D = postVector.Position.To2D() + postVector.CalculateInnerElbowOffset2D(halfWidth);
        Vector2 outerElbow2D = postVector.Position.To2D() + postVector.CalculateOuterElbowOffset2D(halfWidth);

        if (postIndex == 1)
        {
            innerElbowPos2D += pivot;
            af.marker1 = innerElbowPos2D.To3D();
            outerElbow2D += pivot;
            af.marker2 = outerElbow2D.To3D();
        }

    }
    //--------------------------------------------------------------------
    //zSpacing or totalWorldExtraSize aren't crucial but help pad the overlap zones suitably for the settings
    private void CalculateOverlapZonesQuad(Vector3 postPos, int postIndex, Vector3 totalExtraSize, float zSpacing = 0.2f)
    {
        if (af.allPostPositions.Count < 2 || postIndex == 0 /*|| postNum == af.postVectors.Count-1*/)
            return;

        //=====================================================================
        //         Calculate the Overlap Area of the Previous Post Group
        //=====================================================================
        //-- This is the rectangle near the end of the incoming group that 
        //-- the start of incoming group can overlap, depending on the corner fwdHeadingAngle

        PostVector postVector = af.postVectors[postIndex];
        PostVector nextPostVector = postVector.GetNext(), prevPostVector = postVector.GetPrevious();

        Vector2 pivot = postPos.To2D();
        Vector2 prevPostPos = prevPostVector.Position.To2D();// we already returned if postNum == 0
        float postToPostDistance = Vector2.Distance(pivot, prevPostPos);
        Vector2 incomingLocalForward = prevPostVector.Forward.To2D();
        Vector2 worldForward2D = new Vector2(0, 1);

        float widthPadding = 0.6f; //TODO base it on Extra Size
        float overlapGridWidth = gridWidth + widthPadding;
        //float overlapGridLength = postToPostDistance > overlapGridWidth ? postToPostDistance : overlapGridWidth; // arbitrary at the moment, but covers most cases for now
        float overlapGridLength = postToPostDistance * 2f;


        float halfWidth = overlapGridWidth / 2;
        float halfLength = overlapGridLength / 2;
        // Ignore position initiall and just get the relative pos correct, makes it easier to align to local forward2D
        // Go clockwise from top left
        Vector2[] pts = new Vector2[4];
        Vector2[] pentPts = new Vector2[5];


        float extendIncomingLastRow = (zSpacing + totalExtraSize.z) / 2; // extends the overlap rect a little into the nextPos zone to prevent close overlaps
                                                                         //extendIncomingLastRow = 0.1f;
        pts[0] = new Vector2(-halfWidth, halfLength + extendIncomingLastRow);
        pts[1] = new Vector2(halfWidth, halfLength + extendIncomingLastRow);
        pts[2] = new Vector2(halfWidth, -halfLength);
        pts[3] = new Vector2(-halfWidth, -halfLength);


        float halfWidthSigned = halfWidth;
        Vector2 innerElbowPos2D = Vector2.zero;
        if (postVector.CornerAngle >= 0 && postVector.CornerAngle <= 180)
        {
            if (postIndex < numBuiltPosts - 1)
            {
                innerElbowPos2D = postVector.Position.To2D() + postVector.CalculateInnerElbowOffset2D(halfWidth);
                pts[1] = innerElbowPos2D + new Vector2(0, halfLength);
            }
        }
        else
        {
            innerElbowPos2D = postVector.Position.To2D() + postVector.CalculateInnerElbowOffset2D(halfWidth);
            pts[0] = innerElbowPos2D + new Vector2(0, halfLength);
        }

        Vector2 center = Vector2.zero;

        float forwardAngle = Vector2.SignedAngle(worldForward2D, incomingLocalForward);
        float cos = Mathf.Cos(forwardAngle * Mathf.Deg2Rad);
        float sin = Mathf.Sin(forwardAngle * Mathf.Deg2Rad);
        Vector2 dir;


        // rotate the points by the fwdHeadingAngle
        for (int i = 0; i < 4; i++)
        {
            dir = (pts[i] - center);
            pts[i] = center + new Vector2(
                dir.x * cos - dir.y * sin,
                dir.x * sin + dir.y * cos
            );
        }
        for (int i = 0; i < 4; i++)
        {
            pts[i] += pivot;
            pts[i] -= (incomingLocalForward * halfLength);
        }
        Quadrilateral2D quad = new Quadrilateral2D(pts[0], pts[1], pts[2], pts[3]);
        af.overlapExtrasZone.Add(quad);


        innerElbowPos2D = postVector.Position.To2D() + postVector.CalculateInnerElbowOffset2D(halfWidth);
        Vector2 outerElbow = postVector.Position.To2D() + postVector.CalculateOuterElbowOffset2D(halfWidth);

        if (postIndex == 1)
        {
            innerElbowPos2D += pivot;
            af.marker1 = innerElbowPos2D.To3D();
            outerElbow += pivot;
            af.marker2 = outerElbow.To3D();
        }
    }

    private GameObject GetExtraFromPool(int n)
    {
        GameObject ex = null;
        if (n >= extrasPool.Count)
        {
            Debug.LogWarning($"GetExtraFromPool()    Index ( {n} ) exceeded extrasPool in Pool list ( {extrasPool.Count} ).\n");
            return null;
        }
        ex = extrasPool[n].gameObject;
        if (ex == null)
            Debug.LogWarning($"GetExtraFromPool()    GameObject at index ( {n} ) was null.\n");
        return ex;
    }

    /// <summary>
    /// Randomly flips the orientation by Rotating 90, 180, 270. Can be seen as a quantized roation in 90 increments
    /// </summary>
    /// <param name="isVar">A boolean indicating whether the extra object is a variant. We may want to refine the flip based on this</param>
    /// <param name="rotX">The rotation of the extra object around the X-axis. This value may be modified by this method.</param>
    /// <param name="rotY">The rotation of the extra object around the Y-axis. This value may be modified by this method.</param>
    /// <param name="rotZ">The rotation of the extra object around the Z-axis. This value may be modified by this method.</param>
    /// <returns>A Vector3 representing the flipped rotation of the extra object.</returns>
    private Vector3 GetRandomFlipRotations(bool isVar)
    {
        int flipAngle = 0;
        Vector3 flipRotations = Vector3.zero;

        if (flipMode == FlipMode.NoFlip)
            return Vector3.zero;
        else if (flipMode == FlipMode.Flip90)
            flipAngle = -90;
        else if (flipMode == FlipMode.Flip180)
            flipAngle = 180;
        else if (flipMode == FlipMode.Flip270)
            flipAngle = -270;

        if (flipMode != FlipMode.NoFlip)
        {
            if (UnityEngine.Random.value < flipXProb && (isVar == false || excludeExtraVarXZRotations == false))
                flipRotations.x += flipAngle;
            if (UnityEngine.Random.value < flipYProb)
                flipRotations.y += flipAngle;
            if (UnityEngine.Random.value < flipZProb)
                flipRotations.z += flipAngle;
        }

        return flipRotations;
    }

    private void RandomizePosition(Vector3 forward, Vector3 right, GameObject cloneExtra, float rowSpacing, bool isVar, float colSpacing, ref float rotX, out float rotY, ref float rotZ, ref Vector3 centreOffset, out Vector3 rotPoint)
    {
        float rx = UnityEngine.Random.Range(-1f, 1f) * scatterExtraRandPosRange.x * scatterRandomStrength;
        cloneExtra.transform.position += right * colSpacing * rx;

        float ry = UnityEngine.Random.Range(-1f, 1f) * scatterExtraRandPosRange.y * scatterRandomStrength;
        cloneExtra.transform.position += Vector3.up * ry;

        float rz = UnityEngine.Random.Range(-1f, 1f) * scatterExtraRandPosRange.z * scatterRandomStrength;
        cloneExtra.transform.position -= forward * rowSpacing * rz;


        //-- Randomize Rotation. Possibly exclude XZ

        rotY = UnityEngine.Random.Range(-scatterExtraRandRotRange.y * scatterRandomStrength, scatterExtraRandRotRange.y * scatterRandomStrength);
        if (useRandomScatter == true && (isVar == false || excludeExtraVarXZRotations == false))
        {
            rotX = UnityEngine.Random.Range(-scatterExtraRandRotRange.x * scatterRandomStrength, scatterExtraRandRotRange.x * scatterRandomStrength);
            rotZ = UnityEngine.Random.Range(-scatterExtraRandRotRange.z * scatterRandomStrength, scatterExtraRandRotRange.z * scatterRandomStrength);
        }

        centreOffset = Vector3.Scale(centreOffset, cloneExtra.transform.localScale);
        rotPoint = cloneExtra.transform.position + centreOffset;
    }
    /// <summary>
    /// Generates a random position offset for an extra object.
    /// The offset is based on the scatterExtraRandPosRange field and the scatterRandomStrength field.
    /// </summary>
    /// <returns>A Vector3 representing the random position offset.</returns>
    private Vector3 GetRandomExtraPositionOffset(int index)
    {
        if (scatterExtraRandPosRange.sqrMagnitude < .001f)
            return Vector3.zero;

        if (af.extraSeeds.rScatterPos.Count < extrasPool.Count)
        {
            af.extraSeeds.GenerateRandomExtraScatterPos();
            Debug.Log("Generated New Values in GenerateRandomExtraScatterPos()");
        }

        Vector3 randPosOffset = af.extraSeeds.rScatterPos[index];
        return randPosOffset;
    }
    public (Vector3 min, Vector3 max) CalculateMinMaxScatterScaling()
    {
        float x = scatterExtraRandScaleRange.x;
        float adjustment = (x - 1) * scatterRandomStrength;
        float scaleX = 1 + adjustment;
        float minX = Mathf.Min(scaleX, 1);
        float maxX = Mathf.Max(scaleX, 1);

        //float rxScale = UnityEngine.Random.Range(minX, maxX);

        float y = scatterExtraRandScaleRange.y;
        adjustment = (y - 1) * scatterRandomStrength;
        float scaleY = 1 + adjustment;
        float minY = Mathf.Min(scaleY, 1);
        float maxY = Mathf.Max(scaleY, 1);

        //float ryScale = UnityEngine.Random.Range(minY, maxY);

        float z = scatterExtraRandScaleRange.z;
        adjustment = (z - 1) * scatterRandomStrength;
        float scaleZ = 1 + adjustment;
        float minZ = Mathf.Min(scaleZ, 1);
        float maxZ = Mathf.Max(scaleZ, 1);

        //-- class variables
        minRandScatterScale = new Vector3(minX, minY, minZ);
        maxRandScatterScale = new Vector3(maxX, maxY, maxZ);

        //Vector3 randScale = new Vector3(rxScale, ryScale, rzScale );
        return (minRandScatterScale, maxRandScatterScale);
    }
    private Vector3 GetRandomExtraScale(int extraIndex)
    {
        if (af.extraSeeds.rScatterScale.Count < extrasPool.Count)
        {
            CalculateMinMaxScatterScaling();
            af.extraSeeds.GenerateRandomExtraScatterScale(minRandScatterScale, maxRandScatterScale);
        }

        Vector3 randScale = af.extraSeeds.rScatterScale[extraIndex];
        //Debug.Log($"randScale   {randScale}\n");
        return randScale;
    }

    private void ScaleByRandom(Transform thisExtraTrans, ref float minScaleX, float maxScaleX, ref float minScaleY, float maxScaleY, ref float minScaleZ, float maxScaleZ, GameObject cloneExtra, float halfHeight)
    {
        if (useRandomScatter)
        {
            //The transform Rotate uses object space svRotation, so to scale in world direction, we need to calculate the world space svRotation
            Quaternion localRotation = cloneExtra.transform.rotation;
            // Convert the local svRotation to world space
            Vector3 extraWorldRotation = cloneExtra.transform.TransformDirection(localRotation.eulerAngles);
            //Debug.Log("extraWorldRotation  " + extraWorldRotation + "\postNum");

            minScaleX = minScaleY = minScaleZ = 1;

            //--Randomized Scale
            float rsx = UnityEngine.Random.Range(minScaleX, maxScaleX);
            float rsy = UnityEngine.Random.Range(minScaleY, maxScaleY);
            float rsz = UnityEngine.Random.Range(minScaleZ, maxScaleZ);
            if (randomScaleMode == 1)//x+z, y
                rsx = rsz;
            if (randomScaleMode == 2)//x+y+z
                rsx = rsy = rsz;
            if (rsx != 0 || rsy != 0 || rsz != 0)
            {
                //MeshUtilitiesAFWB.SetPivotAtBase(cloneExtra);
                cloneExtra.transform.localScale = Vector3.Scale(cloneExtra.transform.localScale, new Vector3(rsx, rsy, rsz));
                cloneExtra.transform.position = thisExtraTrans.transform.position + new Vector3(0, cloneExtra.transform.localScale.y * halfHeight, 0);
            }
        }
    }
    /*private Vector3 GetRandomExtraScale(float min, float max, float halfHeight)
    {
        Vector3 addedScaling = Vector3.one;
        // Avoid doing the random if we don't need to
        if (useRandomScatter && scatterRandomStrength != 1 && (scatterExtraRandScaleRange.x != ! || scatterExtraRandScaleRange.y != 1 || scatterExtraRandScaleRange.z != 1))
        {
            float rsy = 1, rsx = 1, rsz = 1;
            float scaledMin = min;
            float scaledMax = max, minPercent = 1, maxPercent = 1, randomValue = 1;

            if (scatterRandomStrength <= 1)
            {
                float a = 1f, b = min;
                scaledMin = Mathf.Lerp(a, b, scatterRandomStrength);
                a = 1f; b = max;
                scaledMax = Mathf.Lerp(a, b, scatterRandomStrength);
            }
            else if (scatterRandomStrength > 1)
            {
                float a = min, b = min / 2;
                scaledMin = Mathf.Lerp(a, b, scatterRandomStrength - 1);
                a = max; b = max * 2;
                scaledMax = Mathf.Lerp(a, b, scatterRandomStrength);
            }


            if (scatterExtraRandScaleRange.x > 0)
            {
                minPercent = 1 - ((1 - scaledMin) * scatterExtraRandScaleRange.x);
                maxPercent = 1 + ((scaledMax - 1) * scatterExtraRandScaleRange.x);
                randomValue = UnityEngine.Random.value;
                if (randomValue > 0.5f)
                    rsx = 1 + (randomValue - 0.5f) * 2 * (maxPercent - 1);
                else
                    rsx = minPercent + randomValue * (2 - (2 * minPercent)); // 1.6f is the range size (1 - 0.2)
            }

            if (scatterExtraRandScaleRange.y > 0)
            {
                minPercent = 1 - ((1 - scaledMin) * scatterExtraRandScaleRange.y);
                maxPercent = 1 + ((scaledMax - 1) * scatterExtraRandScaleRange.y);
                randomValue = UnityEngine.Random.value;
                if (randomValue > 0.5f)
                    rsy = 1 + (randomValue - 0.5f) * 2 * (maxPercent - 1);
                else
                    rsy = minPercent + randomValue * (2 - (2 * minPercent)); // 1.6f is the range size (1 - 0.2)
            }

            if (scatterExtraRandScaleRange.z > 0)
            {
                minPercent = 1 - ((1 - scaledMin) * scatterExtraRandScaleRange.z);
                maxPercent = 1 + ((scaledMax - 1) * scatterExtraRandScaleRange.z);
                randomValue = UnityEngine.Random.value;
                if (randomValue > 0.5f)
                    rsz = 1 + (randomValue - 0.5f) * 2 * (maxPercent - 1);
                else
                    rsz = minPercent + randomValue * (2 - (2 * minPercent)); // 1.6f is the range size (1 - 0.2)  
            }



            if (randomScaleMode == 1)//x+z, y
                rsx = rsz;
            if (randomScaleMode == 2)//x+y+z
                rsx = rsy = rsz;
            if (rsx != 0 || rsy != 0 || rsz != 0)
            {
                addedScaling = new Vector3(rsx, rsy, rsz);
            }
        }
        return addedScaling;
    }*/

    private Vector3 FinalRotateExtras(bool lastPost, int n, Vector3 autoDirEulers, Vector3 nextPostPos, Vector3 prevPostPos, Vector3 currPostPos, GameObject cloneExtra, GameObject extrasGroupedFolder, float rowNum, float rotX, float rotY, float rotZ, Vector3 rotPoint)
    {
        //===== Rotation  =====
        Vector3 autoUp = Vector3.zero;
        if (rotateToFenceDirection == true) // this should always be on except for single object placement
        {
            Vector3 nextPost_0y = nextPostPos; // the nextPos post position with the no y delta, i.e flat and level
            Vector3 prevPost_0y = prevPostPos;
            if (usePostToPostIncline == false)
            {
                // set the nex and prevPos postsPool to the same height as the current post, so they're flat and level on the XZ plane
                nextPost_0y.y = currPostPos.y;
                prevPost_0y.y = currPostPos.y;
            }

            currPostPos = new Vector3(2, 0, 1);
            nextPost_0y = new Vector3(3, 0, -1);
            cloneExtra.transform.rotation = VectorUtilitiesTCT.GetRotationQFromDirection(currPostPos, nextPost_0y);

            Quaternion a = VectorUtilitiesTCT.GetRotationQFromDirection(prevPost_0y, currPostPos);
            Quaternion b = VectorUtilitiesTCT.GetRotationQFromDirection(currPostPos, nextPost_0y);
            // It's not the first post, and it's not the last post
            if (n > 0 && n < numBuiltPosts - 1)
            {
                cloneExtra.transform.rotation = b;
                if (rowNum == 0 && averageCornerDirection == true)
                    cloneExtra.transform.rotation = Quaternion.Lerp(a, b, .5f);
                autoDirEulers = cloneExtra.transform.rotation.eulerAngles;
            }
            else if (n == 0)
            {
                cloneExtra.transform.rotation = b;
                autoDirEulers = cloneExtra.transform.rotation.eulerAngles;
            }
            else if (lastPost)
            {
                cloneExtra.transform.rotation = a;
                autoDirEulers = cloneExtra.transform.rotation.eulerAngles;
            }
            autoUp = cloneExtra.transform.up;
        }

        if (adaptExtraToSurfaceDirection)
        {
            Vector3 normLerp = af.GetLerpedSurfaceNormal(cloneExtra.transform.position, extraSurfaceNormalAmount);
            if (usePostToPostIncline)
                cloneExtra.transform.rotation = Quaternion.LookRotation(cloneExtra.transform.forward, normLerp);
            else
            {
                cloneExtra.transform.rotation = Quaternion.FromToRotation(Vector3.up, normLerp);
                autoDirEulers.x = autoDirEulers.z = 0;
                if (rotateToFenceDirection == true)
                    cloneExtra.transform.Rotate(autoDirEulers);
            }
        }
        cloneExtra.transform.RotateAround(rotPoint, cloneExtra.transform.forward, rotZ);
        cloneExtra.transform.RotateAround(rotPoint, cloneExtra.transform.right, rotX);
        cloneExtra.transform.RotateAround(rotPoint, cloneExtra.transform.up, rotY);




        //=============================================
        //       Rotate by Extra Transform amount 
        //=============================================
        if (VectorUtilitiesTCT.IsVector3Zero(extraTransformRotation, .01f) == false)
            cloneExtra.transform.Rotate(extraTransformRotation.x, extraTransformRotation.y, extraTransformRotation.z);

        //if (postRotateScale == true)
        //{
        //    cloneExtra.transform.localScale = baseExtraTransform.transform.localScale;
        //}
        //Debug.Log(postNum + "  " + rowNum + " " + itemNum);;
        return autoDirEulers;
    }
    private void SeupExtraColliders()
    {
        for (int n = 0; n < extrasBuiltCount; n++)
        {
            GameObject thisExtra = extrasPool[n].gameObject;


            if (af.extraColliderMode == 0)//single box
            {
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(thisExtra, false);
                BoxCollider extraBoxCollider = (BoxCollider)thisExtra.AddComponent<BoxCollider>();
                if (extraBoxCollider != null)
                {
                    extraBoxCollider.enabled = true;
                }
            }
            else if (af.extraColliderMode == ColliderType.originalCollider)// all original
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(thisExtra, true);
            else if (af.extraColliderMode == ColliderType.noCollider)//none
                MeshUtilitiesAFWB.RemoveAllColliders(thisExtra);
            else if (af.extraColliderMode == ColliderType.meshCollider)//exampleMesh
            {
                MeshUtilitiesAFWB.SetEnabledStatusAllColliders(thisExtra, false);
                MeshCollider extraMeshCollider = thisExtra.GetComponent<MeshCollider>();
                if (extraMeshCollider == null)
                {
                    extraMeshCollider = (MeshCollider)thisExtra.AddComponent<MeshCollider>();
                    if (extraMeshCollider != null)
                    {
                        extraMeshCollider.enabled = true;
                    }
                }
                else
                    extraMeshCollider.enabled = true;
            }
        }
    }

    private void AdaptToGroundHeight(GameObject cloneExtra, Vector3 centerRowPosition)
    {
        if (extrasFollowGroundHeight)
        {
            af.SetIgnorePartsColliders(true); // temporarily ignore other fence colliders to find distance to ground
            Vector3 currPos = cloneExtra.transform.position;
            if (avgHeightPositionForRow == true)
                currPos = centerRowPosition;
            float rayStartHeight = 20.0f;
            currPos.y += rayStartHeight;
            RaycastHit hit;
            if (Physics.Raycast(currPos, Vector3.down, out hit, 500))
            {
                if (hit.collider.gameObject != null)
                {
                    float distToGround = hit.distance; //in the ground a little
                    cloneExtra.transform.Translate(0, -(distToGround - rayStartHeight), 0);
                    Vector3 newPos = cloneExtra.transform.position;
                    newPos.y = hit.point.y + extraTransformPositionOffset.y;
                    cloneExtra.transform.position = newPos;
                }
            }
            af.SetIgnorePartsColliders(false);
        }
    }
    private void SetPivot(GameObject cloneExtra, out float halfHeight)
    {
        //This is now done on the source exampleMesh during Extra Pool creation to avoid the extra processing of doing it to every clone
        //=== Recentre for the rotations. But we need the base y for it's height position still
        Mesh mesh = cloneExtra.GetComponent<MeshFilter>().sharedMesh;
        halfHeight = mesh.bounds.size.y / 2;

        /*if (pivotPosition == 0) //Base
            MeshUtilitiesAFWB.SetPivotAtBase(cloneExtra);
        else if (pivotPosition == 1) //Center
            MeshUtilitiesAFWB.RecentreMesh(cloneExtra);*/
    }

    private bool SkipItemInRow(float cornerAngle, bool evenNum, float r, float c)
    {
        bool skipItemInRow = false;
        // Skip Inner Corner  if Corner Mode == Corner Angle Outer Only(2)
        if (cornerMode == 2 && r == 0 && cornerAngle >= 30) //Corner Angle Outer Only
        {
            if (evenNum && c >= ((int)numGridX / 2 + 1))
                skipItemInRow = true;
            if (c > ((int)numGridX / 2))
                skipItemInRow = true; ;
        }
        if (cornerMode == 3 && r == 0) //Auto
        {
            if (cornerAngle >= 40 && cornerAngle < 55)
            {
                if (evenNum && c >= (int)(numGridX - 1))
                    skipItemInRow = true; ;
                if (c > (int)(numGridX - 1))
                    skipItemInRow = true; ;
            }
            else if (cornerAngle >= 55 && cornerAngle < 65)
            {
                if (evenNum && c >= (int)(numGridX - 2))
                    skipItemInRow = true; ;
                if (c > (int)(numGridX - 2))
                    skipItemInRow = true; ;
            }
            else if (cornerAngle >= 65 && cornerAngle < 75)
            {
                if (evenNum && c >= (int)(numGridX - 3))
                    skipItemInRow = true; ;
                if (c > (int)(numGridX - 3))
                    skipItemInRow = true; ;
            }
            else if (cornerAngle >= 75 && cornerAngle < 135)
            {
                if (evenNum && c >= ((int)numGridX / 2 + 1))
                    skipItemInRow = true; ;
                if (c > ((int)numGridX / 2))
                    skipItemInRow = true; ;
            }
        }
        return skipItemInRow;
    }



    private GameObject PrepareFoldersExtraMode(GameObject extrasGroupedFolder)
    {
        if (extrasGroupedFolder == null)
        {
            Debug.LogWarning($"Folder: {extrasGroupedFolder.name} is null in PrepareFoldersExtraMode()\n");
            //extrasGroupedFolder = new GameObject(extrasGroupedFolder.name);
        }
        extrasGroupedFolder.transform.parent = af.extrasFolder.transform;
        if (af.addCombineScripts)
        {
            CombineChildrenPlus combineChildren = extrasGroupedFolder.AddComponent<CombineChildrenPlus>();
            if (combineChildren != null)
                combineChildren.combineAtStart = true;
        }
        return extrasGroupedFolder;
    }


    private GameObject PutInFoldersNormalMode(GameObject thisExtra)
    {
        int numExtrasFolders = (extrasBuiltCount / af.objectsPerFolder) + 1;
        string extrasGroupFolderName = "ExtrasGroupedFolder" + (numExtrasFolders - 1);
        GameObject extrasGroupedFolder = GameObject.Find("Current Fences Folder/Extras/" + extrasGroupFolderName);
        if (extrasGroupedFolder == null)
        {
            extrasGroupedFolder = new GameObject(extrasGroupFolderName);
            extrasGroupedFolder.transform.parent = af.extrasFolder.transform;
            if (af.addCombineScripts)
            {
                CombineChildrenPlus combineChildren = extrasGroupedFolder.AddComponent<CombineChildrenPlus>();
                if (combineChildren != null)
                    combineChildren.combineAtStart = true;
            }
        }
        thisExtra.transform.parent = extrasGroupedFolder.transform;
        af.extrasTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(thisExtra);
        return extrasGroupedFolder;
    }

    private void PositionNormalMode(bool isClickPoint, GameObject thisExtra, PostVector postVector, Vector3 nextPostPos, bool isLastPost)
    {
        Vector3 offsetRight = Vector3.zero, offsetForward = Vector3.zero, offsetY = Vector3.zero;
        Vector3 postPos = postVector.Position;
        thisExtra.transform.position = postPos;


        PostBuildInfo postBuildInfo = postVector.postBuildInfo;

        // --- Calculate effect  Main Post Height Boost ------
        float postTopHeight = 0;
        if (raiseExtraByPostHeight == true)
        {
            //if (isStandardPost)
            //{
            //-- This is the height of the post after build randomization and variation has been applied
            float postBuildHeightScaling = postBuildInfo.varSizeScaling.y;
            postTopHeight = af.globalScale.y * postBuildHeightScaling * af.postScale.y;

            //}
            //else
            //postTopHeight = af.globalScale.y * af.subpostScale.y;

            if (isClickPoint)
                postTopHeight *= af.mainPostsSizeBoost.y;

            postTopHeight += af.postHeightOffset;
        }

        Vector3 nextPos = nextPostPos;
        if (isLastPost == true)
            nextPos = postPos + thisExtra.transform.forward;
        Quaternion origAveragedRot = thisExtra.transform.rotation;
        Quaternion trueSectionDirection = VectorUtilitiesTCT.GetRotationQFromDirection(postPos, nextPos); ;


        //Translate only on z first so it's linear in the fence direction using thisExtra.transform  
        thisExtra.transform.rotation = trueSectionDirection;
        thisExtra.transform.Translate(0, extraTransformPositionOffset.y + postTopHeight, extraTransformPositionOffset.z);

        //Now translate on x using the averaged rotation at the corners
        thisExtra.transform.rotation = origAveragedRot;
        thisExtra.transform.Translate(extraTransformPositionOffset.x, 0, 0);

        // If it's offset along a corner in z, stop using avg and use true direction
        if (Mathf.Abs(extraTransformPositionOffset.z) > 0.1f)
            thisExtra.transform.rotation = trueSectionDirection;
    }

    private void RotationNormalMode(GameObject thisExtra, PostVector postVector)
    {
        if (postVector.GetParentCount() < 2)
            return;

        thisExtra.transform.rotation = Quaternion.identity;
        PostVector prevPost = postVector.GetPrevious();
        PostVector nextPost = postVector.GetNext();
        Vector3 postPos = postVector.Position; //-- as we bail early when only a single post, there should always be a previous available
        Vector3 prevPostPos = postVector.GetPreviousPosition();
        Vector3 nextPostPos = postVector.GetNextPosition();
        int postNum = postVector.Index();


        bool isFirstPost = postVector.IsFirstPost();
        bool isLastPost = postVector.IsLastPost();

        Vector3 nonAveragedRot = Vector3.zero; //to later move the extra along true z direction of fence, not the averaged direction at corners
        if (rotateToFenceDirection == true) // this should always be on except for single object placement
        {

            //===============================
            //     Incline To Next Post
            //===============================

            //If we don't want to incline towards the next, we remove its height delta before rotating
            Vector3 prevPostAdjustedPos = prevPostPos;
            Vector3 nextPostAdjustedPos = nextPostPos;

            if (usePostToPostIncline == false)
            {
                nextPostAdjustedPos.y = postPos.y; //because we don't want to rotate height differences
                prevPostAdjustedPos.y = postPos.y;
            }

            thisExtra.transform.rotation = VectorUtilitiesTCT.GetRotationQFromDirection(postPos, nextPostAdjustedPos);
            nonAveragedRot = thisExtra.transform.rotation.eulerAngles;

            if (averageCornerDirection && postNum > 0 && isLastPost == false)
            {
                Quaternion a = VectorUtilitiesTCT.GetRotationQFromDirection(prevPostAdjustedPos, postPos); //-- Prev -> Curr
                Quaternion b = VectorUtilitiesTCT.GetRotationQFromDirection(postPos, nextPostAdjustedPos); //-- Curr -> Next
                if (isFirstPost == false && isLastPost == false)
                    thisExtra.transform.rotation = Quaternion.Lerp(a, b, .5f);
            }
            else if (isLastPost)
                thisExtra.transform.rotation = VectorUtilitiesTCT.GetRotationQFromDirection(prevPostAdjustedPos, postPos);
        }

        //TODO
        if (adaptExtraToSurfaceDirection)
        {
            Vector3 groundNormLerp = af.GetLerpedSurfaceNormal(thisExtra.transform.position, 0.4f);

            //normLerp.x *= -1;
            //groundNormLerp.y *= -1;
            //groundNormLerp.z *= -1;

            Quaternion surfaceTiltRotation = Quaternion.FromToRotation(Vector3.up, groundNormLerp);

            thisExtra.transform.rotation = surfaceTiltRotation * Quaternion.LookRotation(thisExtra.transform.forward, groundNormLerp);

            //thisExtra.transform.rotation = Quaternion.FromToRotation(Vector3.up, normLerp);
            /*autoDirEulers.x = autoDirEulers.z = 0;
            if (rotateToFenceDirection == true)
                thisExtra.transform.Rotate(autoDirEulers);*/
        }

        if (extrasFollowIncline == true && isLastPost == false)
        {
            //float sectionInclineAngle = VectorUtilitiesTCT.GetRotationAnglesFromDirection(standardPostsList[postNum + 1].position, standardPostsList[postNum].position).x;
            //thisExtra.transform.Rotate(sectionInclineAngle, 0, 0);
        }
        //=============================================
        //       Rotate by Extra Transform amount 
        //=============================================
        if (VectorUtilitiesTCT.IsVector3Zero(extraTransformRotation, .01f) == false)
            thisExtra.transform.Rotate(extraTransformRotation.x, extraTransformRotation.y, extraTransformRotation.z); //???

        /*if (relativeMovement == true)
            thisExtra.transform.Translate(0, midPointHeightDelta, 0);*/

        /*// -- Final Rotation - have to apply this after the translation, so that the forward2D direction is not confused
        /*thisExtra.transform.Rotate(extraTransformRotation.x, extraTransformRotation.y, extraTransformRotation.z);
        thisExtra.isStatic = af.usingStaticBatching;*/
        //return autoDirEulers;
    }
    //-------------------------------------------------------------------

    private void StackNormalMode(GameObject thisExtra, GameObject extrasGroupedFolder)
    {
        if (makeMultiArray == false)
            return;

        int sizeY = (int)multiArraySize.y;
        GameObject cloneExtra = null;
        int z = 0;
        for (int y = 0; y < sizeY; y++)
        {
            if (y == 0)
                continue; //we don't clone the one in the root position
            cloneExtra = extrasPool[extrasBuiltCount++].gameObject;
            cloneExtra.transform.position = thisExtra.transform.position;
            cloneExtra.transform.rotation = thisExtra.transform.rotation;
            cloneExtra.transform.localScale = thisExtra.transform.localScale;
            cloneExtra.SetActive(true);
            cloneExtra.hideFlags = HideFlags.None;
            //cloneExtra.transform.Translate(x * multiArraySpacing.x,  y * multiArraySpacing.y, z * multiArraySpacing.z);
            cloneExtra.transform.Translate(0, y * extrasGap, z);

            cloneExtra.transform.parent = extrasGroupedFolder.transform;
            af.extrasTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(cloneExtra);
        }
    }
    //-------------
    //-- We're calculating a grid from one post to the nextPos, however 'nextPos' could be nextPos ordinary post, or nextPos clickpoint Post
    //-- depending on the Extra Frequency Mode. This is our target post, or the end position of the grids we're calculating
    //-- Find this index in the postList (which is main List of build Posts, af.postsPool). (TODO, adapt af.postDirectionList for this, so we can use V3s instead of transforms))
    int FindIndexOfNextTargetPost(int currPostIndex, List<Transform> postList)
    {
        int nextPostIndex = -1;
        bool nextPostIsLastPost = false;

        //-- The nextPos Post is the last Post
        if (currPostIndex == numBuiltPosts - 2)
            return numBuiltPosts - 1;

        //--we're already on the last post
        if (currPostIndex == numBuiltPosts - 1)
            return -1;

        if (extraFreqMode == ExtraPlacementMode.allPostPositions)//All
            return currPostIndex + 1;
        else if (extraFreqMode == ExtraPlacementMode.nodePostPositions)//Main
        {
            nextPostIndex = currPostIndex + 1;
            while (nextPostIndex < numBuiltPosts)
            {
                if (postList[nextPostIndex].name.Contains("_click"))
                {
                    return nextPostIndex;
                }
                nextPostIndex++;
            }
        }
        else if (extraFreqMode == ExtraPlacementMode.endsOnly) //ends
            return numBuiltPosts - 1;


        else if (extraFreqMode == ExtraPlacementMode.allExceptMain)//Not Main
        {
            nextPostIndex = currPostIndex + 1;
            while (nextPostIndex < numBuiltPosts)
            {
                if (postList[nextPostIndex].name.Contains("_click") == false)
                {
                    return nextPostIndex;
                }
                nextPostIndex++;
            }
            return -1; //-- we didn't find a non-main
        }

        // For v4.1
        if (extraFreqMode == ExtraPlacementMode.everyNthPost) //Every 1/postNum
        {
            nextPostIndex = currPostIndex + 1;
            while (nextPostIndex < numBuiltPosts)
            {
                if (nextPostIndex % extraFreq == 0)
                {
                    return nextPostIndex;
                }
                nextPostIndex++;
            }
            return -1;
        }

        if (extraFreqMode == ExtraPlacementMode.everyNthPostAndSubposts) //Every Subpost
        {
            nextPostIndex = currPostIndex + 1;
            while (nextPostIndex < numBuiltPosts)
            {
                if (nextPostIndex % extraFreq == 0)
                {
                    return nextPostIndex;
                }
                nextPostIndex++;
            }
            return -1;//TODO
        }
        if (extraFreqMode == ExtraPlacementMode.allSubposts) //Every Subpost
        {
            nextPostIndex = currPostIndex + 1;
            return nextPostIndex;
        }
        if (extraFreqMode == ExtraPlacementMode.allPostsAndSubposts) //Every Subpost
        {
            nextPostIndex = currPostIndex + 1;
            return nextPostIndex;
        }

        return -1;
    }
    //--------------
    bool IsPostIndexUsed(int currPostIndex, int postListCounter, bool isClickPoint)
    {
        bool usePostIndex = false;
        if (extraFreqMode == ExtraPlacementMode.allPostPositions)//All
            usePostIndex = true;
        else if (extraFreqMode == ExtraPlacementMode.nodePostPositions && isClickPoint == true)//Main
            usePostIndex = true;
        else if (extraFreqMode == ExtraPlacementMode.endsOnly && (currPostIndex == 0 || currPostIndex == postListCounter - 1)) //Ends Only
            usePostIndex = true;
        else if (extraFreqMode == ExtraPlacementMode.allExceptMain && isClickPoint == false) //Not Main
            usePostIndex = true;
        // For v4.1
        else if (extraFreqMode == ExtraPlacementMode.everyNthPost) //Every 1/postNum
        {
            usePostIndex = false;
            if (currPostIndex % extraFreq == 0)
                usePostIndex = true;
        }
        else if (extraFreqMode == ExtraPlacementMode.everyNthPostAndSubposts) //Every Subpost
        {
            usePostIndex = false;
            if (currPostIndex % extraFreq == 0)
                usePostIndex = true;
        }

        else if (extraFreqMode == ExtraPlacementMode.allSubposts && af.useSubpostsLayer == false) //Every Subpost
            usePostIndex = true;
        else if (extraFreqMode == ExtraPlacementMode.allSubposts && af.postAndSubpostStringList[currPostIndex].Contains("Sub")) //Every Subpost
            usePostIndex = true;
        else if (extraFreqMode == ExtraPlacementMode.allPostsAndSubposts) //Every Post and Subpost
            usePostIndex = true;

        //Check if we're forcing the last position On or Off. 0= no change,  1 = force off,   2 = force on
        if (finalPostMode == 1 && currPostIndex == postListCounter - 1)
            usePostIndex = false;

        return usePostIndex;
    }
    //-------------
    (Vector3, int) FindNextNodePostPosition(int currPostIndex, List<Transform> postList)
    {
        Vector3 nextPostPos = Vector3.zero;
        int numPosts = postList.Count;
        int nextPostIndex = currPostIndex + 1;

        while (nextPostIndex < numPosts)
        {
            if (postList[nextPostIndex].name.Contains("_click"))
            {
                nextPostPos = postList[nextPostIndex].position;
                return (nextPostPos, nextPostIndex);
            }
            nextPostIndex++;
        }

        // we didn't find one
        nextPostIndex = -1;
        return (nextPostPos, nextPostIndex);
    }
    //-------------
    //BuildExtras(), RebuildPoolWithNewUserPrefab()
    public List<Transform> CreateExtrasPool(int numToBuild, bool append = false, bool isDirty = false, [CallerMemberName] string caller = null)
    {
        //As there can be hundreds we want to avoid instantiating extrasPool as much as possible.
        //on the other hand, if they've had mesh changes, we need to reload them to prevent compounded changes
        //TODO - more aggresive isDirty check.

        if (af.useExtrasLayer == false || af.allPostPositions.Count == 0)
        {
            // This could be null if called at the start before anything has been built
            if (extrasPool == null)
                extrasPool = new List<Transform>();
            return extrasPool;
        }
        int multiArrayMultiplier = 1;
        if(makeMultiArray)
            multiArrayMultiplier = (int)multiArraySize.x * (int)multiArraySize.y * (int)multiArraySize.z;


        //-- Do one last safety check
        if (extrasMode == ExtrasMode.normal && numToBuild < af.allPostPositions.Count * multiArrayMultiplier)
            Debug.LogWarning("CreateExtrasPool(): numToBuild ExtrasMode.normal is less than the number of posts in the fence. This will result in missing extras");
        else if (extrasMode == ExtrasMode.scatter && numToBuild < numGridX * (numGridZ + 1) * (af.allPostPositions.Count + 1))
            Debug.LogWarning("CreateExtrasPool(): numToBuild ExtrasMode.scatter is not enough");


        Timer poolTimer = new Timer("Create Extra Pool");

        int currCount = extrasPool.Count;
        //CalculateNumberOfExtrasToBuild(n, ref numToBuild);

        // Always build all to avoid compounding mesh rotations and scales
        //extrasPool.Clear();
        af.DestroyPoolForLayer(LayerSet.extraLayerSet);

        int currExtraType = af.currentExtra_PrefabIndex;
        CheckExtraPrefabVariants(); //check all have non-null prefabs assigned
                                    //af.extraSeeds.CheckSeedValues(af);
        float totalVarDistribution = CalcVariantDistribution();

        // Make sure the post type is valid
        if (currExtraType == -1 || currExtraType >= af.extraPrefabs.Count || af.extraPrefabs[currExtraType] == null)
        {
            currExtraType = 0;
        }

        if (currExtraType > af.extraPrefabs.Count - 1)
            Debug.Log("CreateExtrasPool(): Extra prefab beyond loaded index");
        if (af.extraPrefabs[currExtraType] == null)
            Debug.Log("CreateExtrasPool(): Extra was null");
        // Figure out how many to make
        if (numToBuild == 0)
            numToBuild = af.defaultPoolSize;
        int start = 0;
        if (append)
        {
            start = extrasPool.Count;
        }
        // Add postNum new ones to the  List<>
        GameObject extra = null;

        //TODO 
        Mesh[] varMesh = null;
        Mesh mainMesh = null;
        if (enablePrefabVars)
        {
            varMesh = new Mesh[numExtraVars];
            for (int i = 0; i < numExtraVars; i++)
            {
                if (prefabVars[i] != null)
                    varMesh[i] = CreateCloneMeshSourceCopy(prefabVars[i]);

                //change the exampleMesh pos her so we don't have to do it in the build extrasPool loop which is very expensive
                if (af.ex.pivotPosition == PivotPosition.Center_Base)
                    MeshUtilitiesAFWB.SetPivotAtBase(varMesh[i]);
                else if (af.ex.pivotPosition == PivotPosition.Center)
                    MeshUtilitiesAFWB.RecentreMesh(varMesh[i]);
            }
        }
        else
        {
            //change the exampleMesh pos her so we don't have to do it in the build extrasPool loop which is very expensive
            /*mainMesh = CreateCloneMeshSourceCopy(af.extraPrefabs[af.currentExtra_PrefabIndex]);
            if (af.ex.pivotPosition == PivotPosition.Center_Base)
                MeshUtilitiesAFWB.SetPivotAtBase(mainMesh);
            else if (af.ex.pivotPosition == PivotPosition.Center)
                MeshUtilitiesAFWB.RecentreMesh(mainMesh);*/
        }
        af.ValidateSeedsForLayer(LayerSet.extraLayerSet);

        int varIndex = 0, numCreated = 0, extraType = 0;

        for (int i = start; i < start + numToBuild; i++)
        {
            bool var = false;
            extraType = currExtraType;
            if (enablePrefabVars == false || extrasMode == ExtrasMode.normal)
            {
                extra = GameObject.Instantiate(af.ex.prefabVars[varIndex], Vector3.zero, Quaternion.identity) as GameObject;
                //extra.gameObject.GetComponent<MeshFilter>().sharedMesh = MeshUtilitiesAFWB.DuplicateMesh(mainMesh); TODO too slow ???
                var = false;
            }
            else
            {
                varIndex = GetRandomVarIndex(totalVarDistribution, i);
                extra = GameObject.Instantiate(af.ex.prefabVars[varIndex], Vector3.zero, Quaternion.identity) as GameObject;
                extra.gameObject.GetComponent<MeshFilter>().sharedMesh = varMesh[varIndex];
                var = true;
            }

            extra.name = extra.name.Replace("(Clone)", "");
            if (extra.name.EndsWith("_Extra") == false)
                extra.name += "_Ex";
            string varIdxStr = "";
            if (var == true)
                varIdxStr += "_v" + varIndex;
            extra.name += "[" + i + varIdxStr + "]";

            extra.hideFlags = HideFlags.HideInHierarchy;
            extrasPool.Add(extra.transform);
            extra.transform.parent = af.extrasFolder.transform;
        }
        numCreated = extrasPool.Count - currCount;
        //Debug.Log($"Created   {numCreated}   extrasPool  for {caller} . Needed {numNeeded}\postNum");
        //Debug.Log("Num Extras = " + extrasPool.Count + "    Just numCreated " + numCreated + "    Required   " + numNeeded);
        poolTimer.End(print: false);

        return extrasPool;
    }

    private int CalculateNumberOfExtrasToBuild(int guideNumToBuild)
    {
        int numNeeded = 0, numToBuild = 0;
        if (extrasMode == ExtrasMode.normal)
        {
            if (guideNumToBuild < af.allPostPositions.Count)
                guideNumToBuild = af.allPostPositions.Count;

            if (makeMultiArray)
            {
                numNeeded = ((int)multiArraySize.x * (int)multiArraySize.y * (int)multiArraySize.z);
                //numClones = numNeeded; // -1 because we don't clone the root postion one in the array
                //numClones = ((int)ex.multiArraySize.y * (int)ex.multiArraySize.y * (int)ex.multiArraySize.y);
                numToBuild = (guideNumToBuild * numNeeded);
            }
            else
                numToBuild = guideNumToBuild;
        }
        else if (extrasMode == ExtrasMode.scatter)
        {
            int numSectionsToBuild = af.allPostPositions.Count + 1;
            if (extraFreqMode == ExtraPlacementMode.nodePostPositions)
                numSectionsToBuild = af.clickPoints.Count + 2;
            int numRowsToBuild = (int)numGridZ + 1; // an extra row for safety
            if (numRowsToBuild == 1)
                numRowsToBuild = 2;

            numNeeded = (int)(numGridX * numRowsToBuild * numSectionsToBuild);
            numToBuild = numNeeded;
        }
        return numToBuild;
    }

    //------------------------------------
    int GetRandomVarIndex(float total, int n)
    {
        if (total == 0)
            return 0;

        if (n > af.extraSeeds.rExtraVarIndex.Count)
            Debug.Log("dsg");

        float r = af.extraSeeds.rExtraVarIndex[n] * total;
        //Debug.Log(rowNum);
        if (r < extraScatterVarFreq[0])
            return 0;
        else if (r < extraScatterVarFreq[0] + extraScatterVarFreq[1])
            return 1;
        else if (r < extraScatterVarFreq[0] + extraScatterVarFreq[1] + extraScatterVarFreq[2])
            return 2;
        else
            return 3;
    }
    //----------------------------------
    // base the Extra clones on a copy of the prefab/exampleMesh so that we don't disturb the original
    Mesh CreateCloneMeshSourceCopy(GameObject go)
    {
        Mesh meshDup = null;
        Mesh mainMesh = MeshUtilitiesAFWB.GetMeshFromGameObject(go);
        if (mainMesh != null)
        {
            float mainMeshHeight = mainMesh.bounds.size.y;
            meshDup = MeshUtilitiesAFWB.DuplicateMesh(mainMesh);
        }
        else
            Debug.LogWarning("Mesh was null in " + go.name + "\n");
        return meshDup;
    }
}
