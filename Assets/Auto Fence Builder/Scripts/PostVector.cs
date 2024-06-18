using AFWB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

/// <summary>
/// This contains final build mods after randomization, variation etc.
/// Each Post vector may have one of these, but always check validity before using.
/// as posts that haven't been built yet may not contain it </summary>


public class PostBuildInfo
{
    /// <summary>
    /// Extra scaling from build parameters such as randomization and variation. This is to be applied on top of the transform and mesh scaling
    /// </summary>
    public Vector3 varSizeScaling; //scaling from use of variations and sequencer
    public Vector3 varRotation, varPositionOffset;
    public int postIndex;
    public int prefabIndex;
    public PostVector postVector;

    public PostBuildInfo(int postIndex = 0, Vector3 positionOffset = default, Vector3 sizeScaling = default, Vector3 rotation = default,
        int prefabIndex = -1, PostVector postVector = null)
    {
        this.postIndex = postIndex;
        this.varPositionOffset = positionOffset == default ? Vector3.zero : positionOffset;
        this.varSizeScaling = sizeScaling == default ? Vector3.one : sizeScaling;
        this.varRotation = rotation == default ? Vector3.zero : rotation;
        this.prefabIndex = prefabIndex;
        this.postVector = postVector;
    }
}


/// <summary> 
/// Contains full Vector info for each Post. For the main use, they are a member of a "List<PostVector> parentList" that runs parallel to the allPostPositions List 
/// <para>Position, Forward and DirRight vectors (and their averages), IndexIsClickPointNode, CornerAngle, Width</para>
/// <para><b>Position:</b> The position of the post.</para>
/// <para><b>Forward:</b> The Forward direction vector of the post towards the next post.</para>
/// <para><b>DirRight:</b> The direction vector perpendicular to the right of the post.</para>
/// <para><b>DirAvg:</b> The average of the direction vectors of the previous and current post. This will be the same as Forward if the post is not a corner node, or the posts are linear.</para>
/// <para><b>DirAvgRight:</b> The average of the direction vectors perpendicular to the right of the previous and current post. These will be pointing at the inner and outer elbows.</para>
/// <para><b>IndexIsClickPointNode:</b> Indicates whether this is a click point node.</para>
/// <para><b>CornerAngle:</b> The corner angle at the post. Only applies at nodes, and is relative to straight forward.</para>
/// <para><b>Width:</b> Optional: The corner radius of the post, for if we're working with widths or elbows.</para>
/// <para><b>af:</b> Reference to the AutoFenceCreator instance.</para>
/// </summary>
public class PostVector
{
    /// <summary>The position of the post.</summary>
    public Vector3 Position { get; private set; }
    /// <summary>The Forward direction vector of the post towards the next post.</summary>
    public Vector3 Forward { get; private set; }
    /// <summary>The direction vector perpendicular to the right of the post.</summary>
    public Vector3 DirRight { get; private set; }
    /// <summary>The average of the direction vectors of the previous and current post. This will be same as Forward if the post is not a corner node, or the posts are linear.</summary>
    public Vector3 DirAvg { get; private set; }
    /// <summary>The average of the direction vectors perpendicular to the right of the previous and current post. These will be pointing at the inner and outer elbows.</summary>
    public Vector3 DirAvgRight { get; private set; }
    /// <summary>Indicates whether this is a click point node.</summary>
    public bool IsClickPointNode { get; set; }

    /// <summary>The corner angle at the post. Only applies at nodes, and is relative to straight forward. Is Signed 0 to 180 or -0 to -180 </summary>
    public float CornerAngle { get; private set; }

    /// <summary>Optional: The corner radius of the post, for if we're working with widths or elbows.</summary>
    public float Width { get; set; }


    public Vector3 OuterElbowOffset { get; private set; } = Vector3.zero;
    public Vector3 InnerElbowOffset { get; private set; } = Vector3.zero;



    /// <summary>Contains the final build data for the post.</summary>
    public PostBuildInfo postBuildInfo;


    /// <summary>Reference to the AutoFenceCreator instance.</summary>
    public AutoFenceCreator af;

    /// <summary>The static parent list containing all PostVector instances.
    /// This needs to be static, and as such we can only manage one List<PostVector> in AutoFenceCreator</summary>
    private static List<PostVector> parentList;



    //========================  Constructor   ==================================
    /// <summary>Initializes a new instance of the PostVector class and sets af. 
    /// Has to be done here as the other methods are static and can't access af</summary>
    public PostVector(AutoFenceCreator autoFenceCreator)
    {
        af = autoFenceCreator; // Store the reference to the AutoFenceCreator instance
        postBuildInfo = new PostBuildInfo();
    }
    /// <summary>Links the parent list for all PostVector instances.</summary>
    public static void LinkParentList(List<PostVector> list)
    {
        parentList = list;
    }

    /// <summary> Adds a new PostVector to the parent list and calculates all associated data. </summary>
    /// <param name="startPosition">The start position of the new PostVector. (current Post)</param>
    /// <param name="endPosition">The end position of the new PostVector (next Post).</param>
    /// <remarks> This is generally called in a loop from where the next post is available to define the endPosition.
    /// The central use is to be called from af.CalculateAllPostDirectionVectors(), and so converts allPostPositions to PostVectors.
    /// Width isn't always used but can represent the width of a wall to track its inner and outer elbow points at corners.
    ///</remarks>
    public static void Add(Vector3 startPosition, Vector3 endPosition, AutoFenceCreator af, float Width = 2)
    {
        Init(startPosition, endPosition, af, Width);
    }

    //========================================================
    //              Main List Constructor For Posts
    //========================================================
    /// <summary>
    /// Create the full list of PostVectors from a list of post positions.. usually from allPostPositions
    /// </summary>
    /// <param name="postPositions"></param>
    /// <param name="af"></param>
    /// <param name="Width"></param>
    /// <remarks> Called from AutoFenceCreator.CalculateAllPostDirectionVectors()
    /// 
    public static void AddList(List<Vector3> postPositions, AutoFenceCreator af, float Width = 2)
    {
        parentList.Clear();
        int postCount = postPositions.Count;

        //--If there's only one post set it to world forward, it's arbitrary but V3.zero will trigger alerts unnecessarily
        if (postCount == 1)
        {
            AddFirstAndOnlySinglePost(postPositions[0], af);
            return;
        }

        //   Add All Except the last one
        //--------------------------------
        for (int i = 0; i < postCount - 1; i++)
        {
            Add(postPositions[i], postPositions[i + 1], af);
        }

        //   Add the Last One
        //------------------------------
        PostVector.AddFinalPost(postPositions.Last(), af);
    }

    //-----------------------------------
    /// <summary>Same as Add() but for when there's only one Post in the scene. No prev or next, so most parameters have default values</summary>


    /// <summary>Same as Add() but for when twe know this is the final Post allPostPosition, vectors are set to be the same as the 
    /// penultimate post which govern the direction of the final section</summary>
    public static void AddFinalPost(Vector3 startPosition, AutoFenceCreator af)
    {
        PostVector newPostVector = Init(startPosition, startPosition, af);

        //-- As this is the last post Copy the direction vectors from the penultimate post
        PostVector penultimateVector = newPostVector.GetPrevious();
        newPostVector.Forward = penultimateVector.Forward;
        newPostVector.DirRight = penultimateVector.DirRight;
        newPostVector.DirAvg = penultimateVector.DirAvg;
        newPostVector.DirAvgRight = penultimateVector.DirAvgRight;
        newPostVector.IsClickPointNode = true;
    }
    //--------------------------------
    static PostVector Init(Vector3 startPosition, Vector3 endPosition, AutoFenceCreator af, float Width = 2)
    {
        CheckNullAndLinkAF(af);
        PostVector newPostVector = new PostVector(af);
        parentList.Add(newPostVector);
        newPostVector.Position = startPosition;
        newPostVector.postBuildInfo.postIndex = parentList.Count - 1;
        newPostVector.postBuildInfo.postVector = newPostVector;
        newPostVector.CalculateDirVector(startPosition, endPosition);
        newPostVector.CalculateDirRight();
        newPostVector.UpdatePrevious();
        newPostVector.SetCornerAngle();
        newPostVector.SetIsClickPointNode();
        newPostVector.SetElbowOffsets();

        if (parentList.Count > 1)
            newPostVector.CalculateAverageVectors();

        return newPostVector;
    }
    public static void AddFirstAndOnlySinglePost(Vector3 startPosition, AutoFenceCreator af)
    {
        PostVector newPostVector = Init(startPosition, startPosition, af);

        /*CheckNullAndLinkAF(af);
        PostVector newPostVector = new PostVector(af);
        newPostVector.Position = startPosition;*/
        newPostVector.Forward = new Vector3(0, 0, 1);
        newPostVector.DirRight = new Vector3(1, 0, 0);
        newPostVector.DirAvg = new Vector3(0, 0, 1);
        newPostVector.DirAvgRight = new Vector3(1, 0, 0);
        newPostVector.CornerAngle = 0;
        newPostVector.IsClickPointNode = true;
        //parentList.Add(newPostVector);
    }
    /// <summary>
    /// If we've added a post, we need to update the previous one as this one affects its direction and CornerAngle etc,
    /// </summary>
    public void UpdatePrevious()
    {
        PostVector prev = GetPrevious();
        if (prev != null)
        {
            prev.Forward = (this.Position - prev.Position).normalized;
            prev.CalculateDirRight();
            prev.CalculateAverageVectors();
            prev.SetCornerAngle();
        }
    }
    //-----------------------------------
    /// <summary>Gets the previous PostVector in the parent list.</summary>
    /// <returns>The previous PostVector if it exists; otherwise, null.</returns>
    public PostVector GetPrevious()
    {
        PostVector prevPostVector = null;
        int index = parentList.IndexOf(this);
        int prevIndex = index - 1;
        if (prevIndex >= 0)
        {
            prevPostVector = parentList[prevIndex];
            return prevPostVector;
        }
        return null;
    }

    /// <summary>Gets the next PostVector in the parent list.</summary>
    /// <returns>The next PostVector if it exists; otherwise, null.</returns>
    public PostVector GetNext()
    {
        int index = parentList.IndexOf(this);
        return index < parentList.Count - 1 ? parentList[index + 1] : null;
    }
    
    //--------
    /// <summary>Calculate the corner angle at the post, relative to the previous Posts direction. Only applies at nodes. Is Signed 0 to 180 or 0 to -180 </summary>
    void SetCornerAngle()
    {
        PostVector prev = GetPrevious();
        if (prev != null)
        {
            CornerAngle = VectorUtilitiesTCT.GetClockwiseAngle(prev.Forward, Forward);
            if (CornerAngle > 180)
                CornerAngle = -(360 - CornerAngle);
        }
        else
            CornerAngle = 0;
    }
    //------------------
    /// <summary>
    /// Important these are offsets relative to Position. NOT absolute world positions.
    /// Use GetInnerElbowWorldPosition() to get the world position or GetInnerElbowOffset() to get the offset
    /// </summary>
    void SetElbowOffsets()
    {
        PostVector prev = GetPrevious();
        if (prev != null && Width > 0)
        {
            OuterElbowOffset = CalculateOuterElbowOffset2D(Width);
            InnerElbowOffset = CalculateInnerElbowOffset2D(Width);
        }
        else
            CornerAngle = 0;
    }
    /// <summary>
    /// Calculates the inner elbow position in 2D space for a given radius.
    /// </summary>
    /// <param name="radius">The radius of the inner elbow.</param>
    /// <returns>The calculated inner elbow position as a Vector2.</returns>
    public Vector2 CalculateOuterElbowOffset2D(float radius)
    {
        // Define the world forward direction in 2D space (north)
        Vector2 worldForward2D = new Vector2(0, 1);
        // Get the forward direction of the previous post and convert it to 2D
        Vector2 incomingLocalForwardVector = GetPrevious().Forward.To2D();
        // Calculate the signed angle between the world forward direction and the incoming forward direction
        float forwardHeading = Vector2.SignedAngle(worldForward2D, incomingLocalForwardVector);
        // Calculate the cosine and sine of the forward heading angle (converted to radians)
        float cos = Mathf.Cos(forwardHeading * Mathf.Deg2Rad);
        float sin = Mathf.Sin(forwardHeading * Mathf.Deg2Rad);
        // Get the corner angle at the current post
        float cornerAngle = CornerAngle;

        //--TODO Find a better way to deal with accute angles and infinitely extending elbows
        if (cornerAngle > 135)
            cornerAngle = 135;
        if (cornerAngle < -135)
            cornerAngle = -135;

        float halfAngle = cornerAngle / 2;
        float rad = halfAngle * Mathf.Deg2Rad;
        // Calculate the tangent of the half angle in radians
        float tanRad = Mathf.Tan(rad);
        // Calculate the length to the intersection point
        float lengthToIntersection = radius * tanRad;
        // Define the intersection point using the radius and the length to the intersection point
        Vector2 intersectionPt = new Vector2(radius, -lengthToIntersection);
        // Rotate the intersection point using the cosine and sine values to get the inner elbow position
        Vector2 outerElbow = -new Vector2(intersectionPt.x * cos - intersectionPt.y * sin, intersectionPt.x * sin + intersectionPt.y * cos);
        // Flip the inner elbow position for negative radius values to ensure correct placement
        if (CornerAngle > 180 || CornerAngle < 0)
            outerElbow = -outerElbow;
        // Return the calculated inner elbow position
        return outerElbow;
    }
    public Vector2 CalculateInnerElbowOffset2D(float radius)
    {
        Vector2 innerElbow = -CalculateOuterElbowOffset2D(radius);
        //if (CornerAngle > 180 || CornerAngle < 0)
        //innerElbow = -innerElbow;
        return innerElbow;
    }
    //---------------------------
    public Vector3 GetInnerElbowWorldPosition()
    {
        Vector3 innerElbowWorldPos = Position + InnerElbowOffset;
        return innerElbowWorldPos;
    }
    public Vector3 GetOuterElbowWorldPosition()
    {
        Vector3 outerElbowWorldPos = Position + OuterElbowOffset;
        return outerElbowWorldPos;
    }
    //----------------------------
    /// <summary>Simple wrapper around IndexOf(this) to find the index within postVectors of this postVector</summary>
    public int Index()
    {
        int index = parentList.IndexOf(this);
        return index;
    }
    //-----------------------------------
    public bool IsLastPost()
    {
        return parentList.IndexOf(this) == parentList.Count - 1;
    }
    public bool IsFirstPost()
    {
        return parentList.IndexOf(this) == 0;
    }
    public bool IsFirstOrLastPost()
    {
        if (parentList.IndexOf(this) == 0 || parentList.IndexOf(this) == parentList.Count - 1)
            return true;
        return false;
    }
    public Vector3 GetPreviousPosition()
    {
        if (IsFirstPost())
            return Vector3.zero;
        return GetPrevious().Position;
    }
    public Vector3 GetNextPosition()
    {
        if (IsLastPost())
            return Vector3.zero;
        return GetNext().Position;
    }
    public int GetParentCount()
    {
        return parentList.Count;
    }
    public PostVector GetNextNodePost()
    {
        int currIndex = parentList.IndexOf(this);
        int numPostVectors = parentList.Count;

        for (int i = currIndex+1; i < numPostVectors; i++)
        {
            if (parentList[i].IsClickPointNode)
                return parentList[i];
        }
        return null;
    }

    /// <summary>Get the heading angle in world space of the post towards the next post, where 0 degrees is north and equivalent to a forward vector of (0,0,1)</summary>
    public float GetHeading()
    {
        PostVector next = GetNext();
        if (next == null) return 0;

        Vector3 directionToNext = next.Position - this.Position;
        float headingAngle = Quaternion.LookRotation(directionToNext).eulerAngles.y;

        //float heading = Mathf.RoundToInt(headingAngle);

        return headingAngle;
    }
    /// <summary>Get the heading angle in world space of the post towards the next post, where 0 degrees is north and equivalent to a forward vector of (0,0,1)</summary>
    public float GetSignedHeading()
    {
        float heading = GetHeading();
        if (heading > 180) heading -= 360;
        return heading;

        return heading;
    }
    /// <summary>Get the heading angle in world space of the post towards the next post, where 0 degrees is north and equivalent to a forward vector of (0,0,1)</summary>
    public float GetPreviousHeading()
    {
        PostVector prev = GetPrevious();
        if (prev == null) 
            return 0;
        PostVector next = this;
        Vector3 directionFromPrevToThis = Position - prev.Position;
        float prevHeadingAngle = Quaternion.LookRotation(directionFromPrevToThis).eulerAngles.y;
        //int prevHeading = Mathf.RoundToInt(prevHeadingAngle);
        return prevHeadingAngle;
    }
    //-----------------------------
    /// <summary>Get the heading angle in world space of the post towards the next-next post from the next post's perspective</summary>
    public int GetNextHeading()
    {
        PostVector next = GetNext();
        if (next == null) return 0;
        PostVector nextNext = next.GetNext();
        if (nextNext == null) return 0;
        Vector3 directionToNextNext = nextNext.Position - next.Position;
        float nextHeadingAngle = Quaternion.LookRotation(directionToNextNext).eulerAngles.y;
        int nextHeading = Mathf.RoundToInt(nextHeadingAngle);
        return nextHeading;
    }
    //------------------------
    /// <summary>
    /// Returns the signed heading angle of the DirAvg vector in degrees,
    /// where (0,0,1) is 0 degrees, (1,0,0) is 90 degrees, (0,0,-1) is 180 degrees, and (-1,0,0) is -90 degrees.
    /// </summary>
    /// <returns>The signed heading angle in degrees.</returns>
    public float GetDirAvgHeading()
    {
        PostVector prev = GetPrevious();
        if (prev == null)
        {
            float heading = GetHeading();
            if (heading > 180) heading -= 360;
            return heading;
        }

        // Get the forward vectors of the current and previous posts
        Vector3 currentForward = Forward;
        Vector3 previousForward = prev.Forward;

        // Normalize the vectors
        currentForward.Normalize();
        previousForward.Normalize();

        // Add the vectors
        Vector3 averageDirection = currentForward + previousForward;
        averageDirection.Normalize();

        // Convert the average direction vector to a signed heading angle
        //float avgHeading = GetSignedHeadingAngleFromDirection(averageDirection);

        float avgHeading = Mathf.Atan2(averageDirection.x, averageDirection.z) * Mathf.Rad2Deg;
        // Ensure the angle is between -180 and 180 degrees
        if (avgHeading > 180) avgHeading -= 360;

        return avgHeading;
    }

    /// <summary>Get the average heading angle in world space  prev->current &  current->next 
    /// Warning, gives non-intuitive result. Use GetDirAvgHeading() instead</summary>
    public float GetAverageHeading()
    {
        float heading = GetHeading();
        float prevHeading = GetPreviousHeading();

        float avgHeading = (heading + prevHeading) / 2;
        return avgHeading;
    }

    public GameObject GetPrefab()
    {
        int index = parentList.IndexOf(this);
        GameObject prefab = af.postsPool[index].gameObject;
        return prefab;
    }
    //-----------------------------------
    /// <summary>
    /// Returns the Left/Right corner orientation relative to Post's local Forward.
    /// </summary>
    /// <returns></returns>
    public CornerOrientation GetCornerOrientation()
    {
        CornerOrientation cornerOrientation = CornerOrientation.none;
        if (CornerAngle > 0 && CornerAngle < 180)
            cornerOrientation = CornerOrientation.cornerRight;
        else if ((CornerAngle < 0 && CornerAngle > -180) || (CornerAngle > 180 && CornerAngle < 360))
            cornerOrientation = CornerOrientation.cornerLeft;

        return cornerOrientation;
    }
    //-----------------------------------
    /// <summary>Calculates the direction vector of the post towards the next post.</summary>
    /// <param name="start">The start position of the post.</param>
    /// <param name="end">The end position of the post.</param>
    private void CalculateDirVector(Vector3 start, Vector3 end)
    {
        Forward = (end - start).normalized;
    }
    //-----------------------------------
    /// <summary>Calculates the direction vector perpendicular to the right of the post.</summary>
    private void CalculateDirRight()
    {
        DirRight = -Vector3.Cross(Forward, Vector3.up).normalized;
    }


    /// <summary>Calculates the average direction vectors based on the previous post vector.</summary>
    private void CalculateAverageVectors()
    {
        PostVector prev = GetPrevious();

        if (prev == null || parentList.Count < 2)
        {
            DirAvg = Forward;
            DirAvgRight = DirRight;
            return;
        }

        DirAvg = ((prev.Forward + Forward) / 2).normalized; // average of previous and current direction vectors
        DirAvgRight = ((prev.DirRight + DirRight) / 2).normalized; // average of previous and current perpendicular direction vectors
    }
    //-----------------------------
    /// <summary>Checks if the parentList and af for nulls and links bothe.</summary>
    static bool CheckNullAndLinkAF(AutoFenceCreator af)
    {
        bool isOK = true;
        if (parentList == null)
        {
            Debug.LogWarning("PostVector parentList is null. Creating new.\n");

            if (af == null)
            {
                isOK = false;
                af = GameObject.FindObjectOfType<AutoFenceCreator>();
                if (af != null)
                {
                    parentList = new List<PostVector>();
                    af.postVectors = parentList;
                }
            }
            else
            {
                parentList = new List<PostVector>();
                af.postVectors = parentList;
            }
        }
        return isOK;
    }
    //--------
    public float GetDistanceToNextPost()
    {
        PostVector next = GetNext();
        if (next == null)
            return 0;
        return Vector3.Distance(Position, next.Position);
    }
    //--------
    /// <summary>
    /// If there's another Node Post, get the distance to it. Otherwise, return 0
    /// </summary>
    /// <returns></returns>
    public float GetDistanceToNextNode()
    {
        PostVector nextNodePost = GetNextNodePost();
        if (nextNodePost == null)
            return 0;
        return Vector3.Distance(Position, nextNodePost.Position);
    }
    //--------
    void SetIsClickPointNode()
    {
        IsClickPointNode = false;
        int indexOfThis = parentList.IndexOf(this);
        if(indexOfThis == 0)
            IsClickPointNode = true;

        if(IsClickPointNode = af.IsCloseClickPoint(Position) != -1)
        {
            IsClickPointNode = true;
            //Debug.Log($"PostVector {indexOfThis} is a Click Point Node\n");
        }
    }
    //-------------
    public int GetIndex()
    {
        int index = parentList.IndexOf(this);
        return index;
    }
    //--------
    /*public int GetParentListCount()
    {
        int parentListCount = parentList.Count;
        return parentListCount;
    }*/

    
    //-----------------------------------
    /// <summary>Gets the PostVector two positions after the current one in the parent list.</summary>
    /// <returns>The PostVector two positions ahead if it exists; otherwise, null.</returns>
    public PostVector GetNextNext()
    {
        int index = parentList.IndexOf(this);
        return index < parentList.Count - 2 ? parentList[index + 2] : null;
    }

    /// <summary>
    /// Calculates the position to the right of the current post at a given distance.
    /// </summary>
    /// <param name="distance">The distance to the right of the current post.</param>
    /// <param name="relativePosOnly">Optional. If set to true, the method will return the local relative position only, i.e. the offset from the Post 
    /// If false, it will return the absolute world position. Defaults to false.</param>
    /// <returns>The position to the right of the current post at the specified distance.</returns>
    public Vector3 CalculatePositionToRightAtDistance(float distance, bool relativePosOnly = false)
    {
        Vector3 rightPos = DirRight * distance;
        if (relativePosOnly == false)
            rightPos += Position;
        return rightPos;
    }
    public Vector3 ConvertVectorToPostForwardSpace(Vector3 inVector)
    {
        Quaternion rotation = Quaternion.LookRotation(Forward, Vector3.up);
        Vector3 localObjectVec = rotation * inVector;
        return localObjectVec;
    }
    /*public Vector2 CalculateOuterElbowOffset2D(float radius)
    {
        //Vector2 outerElbowOffset = -CalculateInnerElbowOffset2D(radius);
        //Vector2 outerElbowOffset = -CalculateInnerElbow2(radius);
        Vector2 outerElbow = CalculateOuterElbowOffset2D(radius);
        return outerElbow;
    }*/
    //-----------------------------------



    /// <summary>
    /// Calculates the position to the right of the current post at a given distance.
    /// </summary>
    /// <param name="distance">The distance to the right of the current post.</param>
    /// <param name="relativePosOnly">Optional. If set to true, the method will return the local relative position only, i.e. the offset from the Post 
    /// If false, it will return the absolute world position. Defaults to false.</param>
    /// <returns>The position to the right of the current post at the specified distance in world space. Or local offset if relativePosOnly == true</returns>
    public Vector3 CalculatePositionToLeftAtDistance(float distance, bool relativePosOnly = false)
    {
        Vector3 leftPos = -DirRight * distance;
        if (relativePosOnly == false)
            leftPos += Position;
        return leftPos;
        //Vector2 incomingLocalForwardVector = dirVectors[postIndex - 1].ToVector2XZ().normalized;
    }

    public Vector3 CalculateInnerElbowPosition(float radius)
    {
        Vector2 offset2D = CalculateInnerElbowOffset2D(radius);
        Vector2 worldPos2D = Position.To2D() + offset2D;
        Vector3 worldPos = new Vector3(worldPos2D.x, Position.y, worldPos2D.y);
        return worldPos;
    }
    /// <summary>Returns the outer elbow position in 2D space for a given radius. Wiill be OuterAtTopLeft for a right-hand turn between 0-180, OuterAtBottomRight 
    /// Trivial, but sometimes easier to grok than thinking of angles and signs
    /// </summary>
    public ElbowOrientation GetElbowMode()
    {
        if (CornerAngle >= 0 && CornerAngle < 180)
            return ElbowOrientation.OuterAtTopLeft;

        return ElbowOrientation.OuterAtBottomRight;
    }
    /// <summary>
    /// Gets the index of this post as a click point node in the parent list.
    /// </summary>
    /// <returns>The index of this post as a click point node if found; otherwise, -1.</returns>
    /// <summary>
    /// Gets the index of this post as a click point node in the parent list.
    /// </summary>
    /// <returns>The index of this post as a click point node if found; otherwise, -1.</returns>
    public int GetClickNodeIndex()
    {
        if (!IsClickPointNode)
        {
            //Debug.Log($"This post is not a click point node.\n");
            return -1;
        }

        int clickPointNodeCount = 0;
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].IsClickPointNode)
            {
                if (parentList[i] == this)
                {
                    //Debug.Log($"This post is the [{clickPointNodeCount}] click point node in the parent list.\n");
                    return clickPointNodeCount;
                }
                clickPointNodeCount++;
            }
        }

        //Debug.Log($"This post is not found in the parent list.\n");
        return -1; // Return -1 if this post is not found in the parent list
    }
    //---------------------
    public LeftRightTCT LeftOrRightCorner()
    {
        if (CornerAngle < 0 || CornerAngle > 180)
            return LeftRightTCT.left;

        return LeftRightTCT.right;
    }

    // Private helper method for rotating points
    private void RotatePoints(Vector2[] points, Vector2 centerOfRotation, Vector2 forward)
    {
        // Normalize forward to ensure it's a unit vector
        forward.Normalize();

        // Calculate the angle between the world forward direction (0, 1) and the provided forward vector
        float angle = Mathf.Atan2(forward.y, forward.x) - Mathf.Atan2(1, 0);

        // Create the rotation matrix
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        // Rotation matrix components
        float[,] rotationMatrix = new float[,]
        {
            { cosAngle, -sinAngle },
            { sinAngle, cosAngle }
        };

        // Apply the rotation to each point
        for (int i = 0; i < points.Length; i++)
        {
            // Translate point to the origin
            Vector2 translatedPoint = points[i] - centerOfRotation;

            // Apply rotation
            float x = translatedPoint.x * rotationMatrix[0, 0] + translatedPoint.y * rotationMatrix[0, 1];
            float y = translatedPoint.x * rotationMatrix[1, 0] + translatedPoint.y * rotationMatrix[1, 1];

            // Translate point back to its original position
            points[i] = new Vector2(x, y) + centerOfRotation;
        }
    }

    /// <summary>Rotates a set of points to the local forward direction of this PostVector's instance variable v.</summary>
    public void RotatePointsToPostForward(Vector2[] points)
    {
        RotatePoints(points, this.Position.To2D(), this.Forward.To2D());
    }

    /// <summary>Rotates a set of points to the forward direction of the previous PostVector of this instance.</summary>
    public void RotatePointsToPreviousPostForward(Vector2[] points)
    {
        PostVector previous = this.GetPrevious();
        if (previous != null)
            RotatePoints(points, this.Position.To2D(), previous.Forward.To2D());
    }

    /// <summary>Rotates a set of points to the forward direction of the next PostVector of this instance.</summary>
    public void RotatePointsToNextPostForward(Vector2[] points)
    {
        PostVector next = this.GetNext();
        if (next != null)
            RotatePoints(points, this.Position.To2D(), next.Forward.To2D());
    }

    //=========================================================================
    //                                Statics
    //=========================================================================


    /// <summary>
    /// Calculates the position to the right of the current post at a given distance.
    /// </summary>
    /// <param name="distance">The distance to the right of the current post.</param>
    /// <param name="relativePosOnly">Optional. If set to true, the method will return the local relative position only, i.e. the offset from the Post 
    /// If false, it will return the absolute world position. Defaults to false.</param>
    /// <returns>The position to the right of the current post at the specified distance in world space. Or local offset if relativePosOnly == true .</returns>
    public static Vector3 CalculatePositionToRight(int postIndex, float distance, bool relativePosOnly = false)
    {
        Vector3 rightVec = parentList[postIndex].DirRight;
        Vector3 rightPos = rightVec * distance;
        if (relativePosOnly == false)
            rightPos += parentList[postIndex].Position;
        return rightPos;
    }

    /// <summary>
    /// Calculates the position to the right of the current post at a given distance.
    /// </summary>
    /// <param name="distance">The distance to the right of the current post.</param>
    /// <param name="relativePosOnly">Optional. If set to true, the method will return the local relative position only, i.e. the offset from the Post 
    /// If false, it will return the absolute world position. Defaults to false.</param>
    /// <returns>The position to the right of the current post at the specified distance.</returns>
    public Vector3 CalculatePositionToLeft(int postIndex, float distance, bool relativePosOnly = false)
    {
        Vector3 leftVec = -parentList[postIndex].DirRight;
        Vector3 leftPos = leftVec * distance;
        if (relativePosOnly == false)
            leftPos += parentList[postIndex].Position;
        return leftPos;
    }

    //---------------------------------------
    /// <summary>
    /// Find the nth PostVector where IndexIsClickPointNode == true, and get its index into af.postVectors
    /// </summary>
    /// <param name="clickPointIndex"></param>
    /// <returns>postIndex</returns>
    public static int GetPostIndexFromClickpointIndex(int clickPointIndex)
    {
        int postIndex = 0;
        int clickPointIndexCounter = 0;
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].IsClickPointNode)
            {
                if (clickPointIndex == clickPointIndexCounter)
                {
                    postIndex = i;
                    return postIndex;
                }
                clickPointIndexCounter++;
            }
        }
        return -1;
    }
    public static int GetClickpointIndexFromPostIndex(int postIndex)
    {
        int clickPointIndex = 0;
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].IsClickPointNode)
            {
                if (i == postIndex)
                    return clickPointIndex;
                clickPointIndex++;
            }
        }
        return -1;
    }

    public static bool IndexIsClickPointNode(int postIndex)
    {
        PostVector postVector = GetPostVectorAtIndex(postIndex);
        if (postVector == null)
            return false;
        return postVector.IsClickPointNode;
    }
    //--------
    public static int GetParentListCount()
    {
        int parentListCount = parentList.Count;
        return parentListCount;
    }

    public static void PrintPostVectors()
    {
        Debug.Log("PostVectors: \n");

        string angleStr = "", headingStr = "";
        int postVectorsCount = parentList.Count;
        for (int i = 0; i < postVectorsCount; i++)
        {
            PostVector postVector = parentList[i];


            float worldHeading = Quaternion.LookRotation(postVector.Forward).eulerAngles.y;
            headingStr = $"  Heading:  {worldHeading.ToString("F1")}";
            headingStr += $"      ( {VectorUtilitiesTCT.GetCompassStringFromHeading(worldHeading)} )";

            if (i == postVectorsCount - 1)
            {
                angleStr = "";
                headingStr = "";
            }
            if (postVector.IsClickPointNode)
            {
                angleStr = $"  Angle: {postVector.CornerAngle.ToString("F1")}";
                Debug.Log($"<color=#99FF99>{i}   Direction: {postVector.Forward.ToString("F2")}   " +
                    $"{angleStr}        {headingStr}</color>\n");
            }
            else
            {
                Debug.Log($"{i}   Direction: {postVector.Forward.ToString("F2")}                                   " +
                    $"{headingStr} \n");
            }
        }
    }
    /// <summary>
    /// Safely gets the PostVector at the specified index if it exists, otherwise returns null.
    /// </summary>
    /// <param name="index">The index of the PostVector to get.</param>
    /// <returns>The PostVector at the specified index, or null if the index is out of bounds.</returns>
    public static PostVector GetPostVectorAtIndex(int postIndex)
    {
        if (postIndex >= 0 && postIndex < parentList.Count)
            return parentList[postIndex];
        Debug.LogWarning($"Index {postIndex} is out of bounds in GetPostVectorAtIndex(). Valid range is 0 to {parentList.Count - 1}.");
        return null;
    }
    /// <summary>
    /// Finds the index of a post given its position.
    /// </summary>
    /// <param name="position">The position of the post to find.</param>
    /// <param name="tolerance">The tolerance to account for floating-point precision errors.</param>
    /// <returns>The index of the post if found; otherwise, -1.</returns>
    /// <remarks>Given that the PostVector.Position was copied from allPostPositions[i] in the first place, 
    /// there shouldn't be any float innacuracy, but include a chack anyway for future proofing.
    /// The check won't happen at all if a telerance of 0 is used. </remarks>
    /// .</remarks>
    public static int FindIndexByPosition(Vector3 position, float tol = 0.001f)
    {
        if (tol == 0)
        {
            for (int i = 0; i < parentList.Count; i++)
            {
                if (parentList[i].Position == position)
                    return i;
            }
        }
        else
        {
            for (int i = 0; i < parentList.Count; i++)
            {
                Vector3 pos = parentList[i].Position;
                if (Mathf.Abs(pos.x - position.x) <= tol && Mathf.Abs(pos.y - position.y) <= tol && Mathf.Abs(pos.z - position.z) <= tol)
                    return i;
            }
        }
        return -1; // Return -1 if no post with the given position is found
    }
    public static int FindIndexByPosition(GameObject go, float tol = 0.001f)
    {
        int index = FindIndexByPosition(go.transform.position, tol);
        return index;
    }
}
