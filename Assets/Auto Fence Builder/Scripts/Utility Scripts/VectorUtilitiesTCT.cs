using AFWB;
using System;
using UnityEngine;

public static class VectorTCTxtensions
{

    /// <summary>Uses the x and z components of the Vector3 to create a new Vector2 (x, y) </summary>
    public static Vector2 To2D(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    /// <summary>
    /// Gets a Vector3 with the assumption that the Vector2 y value represents the z value,
    // as it does when utilising Vector2s on the XZ plane. Then sets the new y value to 0
    /// </summary>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static Vector3 To3D(this Vector2 v2)
    {
        return new Vector3(v2.x, 0, v2.y);
    }
    //-------------
    /// <summary> Gets a Vector3 by copying  3D.x= 2D.x,      3D.y=0,      3D.z = 2D.y </summary>
    /// <param name="v2"> The Vector being modified</param>
    /// <returns></returns>
    public static Vector3 To3DY0(this Vector2 v2)
    {
        return new Vector3(v2.x, 0, v2.y);
    }
    /// <summary>
    /// Converts to #D and sets y to new Y
    /// </summary>

    public static Vector3 To3D(this Vector2 v2, float newY)
    {
        return new Vector3(v2.x, newY, v2.y);
    }

    public static Vector3 ToY0(this Vector3 v3)
    {
        return new Vector3(v3.x, 0, v3.z);
    }

    // DistanceTCT from this vector to destination vector, ignoring the y difference
    public static float HorizDistance(this Vector3 v3, Vector3 destVec)
    {
        float distance = Vector2.Distance(v3.To2D(), destVec.To2D());
        return distance;
    }

    // world heading to go from this vector to the destination vector
    public static float HeadingTo(this Vector3 v3, Vector3 destVec)
    {
        Vector3 direction = Quaternion.LookRotation(destVec - v3).eulerAngles;
        float heading = direction.y;
        return heading;
    }
    /// <summary>
    /// check if any component of the vector is greater than the value. Defaults to .001f (1mm)
    /// </summary>

    public static bool ComponentGreaterThan(this Vector3 vector, float value = .001f)
    {
        return Mathf.Abs(vector.x) > value || Mathf.Abs(vector.y) > value || Mathf.Abs(vector.z) > value;
    }
    /// <summary>
    /// check if X or Z component of the vector is greater than the value. Defaults to .001f (1mm)
    /// </summary>

    public static bool XZGreaterThan(this Vector3 vector, float value = .001f)
    {
        return Mathf.Abs(vector.x) > value || Mathf.Abs(vector.z) > value;
    }
}

//==================================================================================================
//                                  VectorUtilitiesTCT
//==================================================================================================

public enum LeftRightTCT
{
    left = -1,
    right = 1,
    center = 0
}

public class VectorUtilitiesTCT
{
    /* A lot of these are redundant as Unity has them built in, and I might have defined similar functions elsewhere
     * but they're here to remind me in a particular context how they work.
     * For example, character rotations can often be simplified to the XZ plane with up = y (like a fence on flat ground)
     * and can be visualised with a single angle around the Y axis.
     * But an aircraft might need to rotate in all 3 dimensions (like a fence going up a hill at some arbitrary direction and slope)
     * In this case we stick with Quaternions, and at a pinch, the  Euler angles of the Quaternion. (q.eulerAngles) taking care of order of rotation.
     * An example of this below is GetDirectionToNextPost3D() which returns a Quaternion,
     * and GetDirectionToNextPost2D() which returns a Vector3 (the XZ plane) and the angle around Y.
     * In short, just a convenience and memory aid.
     */

    //==================================================================================================
    /// <summary>
    /// Applies vector to the objects local space.
    /// e.g. if vector = (1,0,0) (right) and the objectForward is default forward (0,0,1), then inVector will remain unchanged
    /// <para>But if the object's forward is actuall heading south in world space (0,0,-1), then  inVector will become (-1,0,0)</para>
    /// <para> In other turning right, when the object is heading south in world space, will result in turning left in world space</para>
    /// </summary>
    /// <param name="objectForward"></param>
    /// <param name="vector"></param>
    /// <returns></returns>

    public static Vector3 ConvertVectorToLocalObjectForwardSpace(Vector3 objectForward, Vector3 inVector)
    {
        Quaternion rotation = Quaternion.LookRotation(objectForward, Vector3.up);
        Vector3 localObjectVec = rotation * inVector;
        return localObjectVec;
    }


    /// <summary>
    /// Gets the Quaternion angles necessary to turn from the current position 
    /// </summary>
    /// <param name="currPosition"></param>
    /// <param name="nextPosition"></param>
    /// <param name="currForward"></param>
    /// <returns></returns>& direction towards nextPosition
    /// <remarks> This version assumes currPosition's forward is currForward, and returns a Quaternion that will rotate currPosition to look at nextPosition
    /// This is for a fence on a slope</remarks>
    public static Quaternion GetDirectionToNextPost3D(Vector3 currPosition, Vector3 nextPosition, Vector3 currForward)
    {
        Vector3 dir = nextPosition - currPosition;
        Quaternion currentRotation = Quaternion.LookRotation(currForward);
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        Quaternion q = Quaternion.Inverse(currentRotation) * targetRotation;
        return q;
    }

    /// <summary>
    /// Make a version for the above function that takes two gameobjects instead of two vectors and gets the forward vector from the transform of the first
    //  Assumes that the first object is facing the right way, ie. its forward vector is the direction it came from the previous Post
    /// </summary>
    /// <param name="currPosition"></param>
    /// <param name="nextPosition"></param>
    /// <returns></returns>


    public static Quaternion GetDirectionToNextPost3D(GameObject currPosition, GameObject nextPosition)
    {
        Vector3 dir = nextPosition.transform.position - currPosition.transform.position;
        Quaternion currentRotation = currPosition.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        Quaternion q = Quaternion.Inverse(currentRotation) * targetRotation;
        return q;
    }

    /* A version similar to the above but doesn't assume currPosition has had it's rotations already set correctly
     * and instead explicitly states the forward vector of currPosition
     */

    public static Quaternion GetDirectionToNextPost3D(GameObject currentPost, GameObject nextPost, Vector3 currForwardVector)
    {
        if (currentPost == null || nextPost == null)
        {
            throw new ArgumentNullException("GameObject parameters cannot be null in GetDirectionToNextPost3D() \n");
        }
        Vector3 currentPos = currentPost.transform.position;
        Vector3 targetPos = nextPost.transform.position;

        Vector3 dir = targetPos - currentPos;

        // Check if the direction vector is not zero
        if (dir == Vector3.zero)
        {
            return Quaternion.identity; // No rotation needed if the positions are the same
        }
        dir.Normalize(); // Normalize to ensure consistent behavior
        Quaternion currentRotation = Quaternion.LookRotation(currForwardVector.normalized);
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        Quaternion q = Quaternion.Inverse(currentRotation) * targetRotation;
        return q;
    }

    /* This wrapper includes previousPost as input to automatically calculate currForwardVector
     */

    public static Quaternion GetDirectionToNextPost3D(GameObject previousPost, GameObject currentPost, GameObject nextPost)
    {
        Vector3 currForward = currentPost.transform.position - previousPost.transform.position;
        Quaternion q = GetDirectionToNextPost3D(currentPost, nextPost, currForward);
        return q;
    }

    /* Same again using Previous Post, but only Vector3 positions
     * Had to rename because of clash with first version
     */

    public static Quaternion GetDirectionToNextPost3DFromPositions(Vector3 prevPostPos, Vector3 currPostPos, Vector3 nextPostPos)
    {
        Vector3 currForward = currPostPos - prevPostPos;
        Quaternion q = GetDirectionToNextPost3D(currPostPos, nextPostPos, currForward);
        return q;
    }

    /*   Based On World Forward : Vector3.forward (0, 0, 1)
     *
    *   This version assumes currPosition's forward is world forward, Vector3.forward = (0,0,1)
    *   and returns a Quaternion that will rotate currPosition to look at nextPosition
    *   For example when currPosition is the first Post that has no implicit existing forward vector
    */

    public static Quaternion GetDirectionToNextPost3D(Vector3 currPosition, Vector3 nextPosition)
    {
        Quaternion q = Quaternion.LookRotation(nextPosition - currPosition);
        return q;
    }

    //=================================================================================================
    //
    //                  2D Flat & Level Versions. Y is assumed either 0, or to be the same in both
    //
    //=================================================================================================

    // Use Vector2s when working on the assunption we're on the flat level XZ plane

    //-- Assumes up is Vector3.up (0,1,0)
    public static Vector3 GetRightFromForward(Vector3 forward, bool normalize = true)
    {
        Vector3 right;
        //If y0, making it effectively 2D we can do it quicker
        if (forward.y == 0)
            right = new Vector3(forward.z, 0, -forward.x);
        else
            right = Vector3.Cross(Vector3.up, forward);

        if (normalize)
            right.Normalize();
        return right;
    }

    //-- Same but for 2D
    public static Vector2 GetRightFromForward2D(Vector2 forward)
    {
        return new Vector2(forward.y, -forward.x).normalized;
    }

    // A wrapper that deals with the edge case of A = B
    // and returns eulers instead of Quaternion
    public static Vector3 GetRotationAnglesFromDirection(Vector3 A, Vector3 B)
    {
        if (B - A == Vector3.zero)
        {
            //Debug.Log("Same Position in CalculateDirection()");
            B.x += .00001f;
        }
        Quaternion q2 = Quaternion.LookRotation(B - A);
        Vector3 euler = q2.eulerAngles;
        return euler;
    }
    //-- Heading going clockwise from (0,0,-1)
    public static Vector3 GetRotationAnglesFromDirection(Vector3 dir)
    {
        Quaternion q = Quaternion.LookRotation(dir);
        Vector3 euler = q.eulerAngles;
        return euler;
    }

    // Based on A's forward vector being world forward, calculate a quaternion rotation needed to look towards B
    // In other words a world angle from (0,0,1) to B
    // Just a wrapper for Quaternion.LookRotation(B - A) so I'll remember why I used it in 6 months
    public static Quaternion GetRotationQFromDirection(Vector3 A, Vector3 B)
    {
        //-- If they're the same position, add a tiny amount to B to prevent divide-by-zero errors
        if (B - A == Vector3.zero)
            B.x += .00001f;

        // Calculate the quaternion for rotation.
        // Quaternion.LookRotation creates a rotation that looks along the forward vector (B - A),
        // aligning the object's Z-axis with the direction from A to B.
        // This means the object at point A will be rotated to face towards point B.
        Quaternion q2 = Quaternion.LookRotation(B - A);
        return q2;
    }

    // Just a wrapper for consistency
    public static Quaternion GetRotationQFromDirection(Vector3 dir)
    {
        Quaternion q2 = Quaternion.LookRotation(dir);
        return q2;
    }

    public static void DrawRotationVector(Vector3 pos, Quaternion q)
    {
        Vector3 vec = q * Vector3.forward;
        Debug.DrawLine(pos, pos + vec);
    }

    public static void DrawDirectionVector(Vector3 pos, Vector3 v)
    {
        Debug.DrawLine(pos, pos + v.normalized);
    }

    public static float GetMaxAbsVector3Element(Vector3 v3)
    {
        float max = Mathf.Max(Mathf.Max(Mathf.Abs(v3.x), Mathf.Abs(v3.y)), Mathf.Abs(v3.z));
        return max;
    }

    //-- This version is from the world reference forward of (0,1) and returns the angle around Y
    //-- The same as heading
    public static float GetClockwiseAngleFromWorldForward2D(Vector2 to)
    {
        Vector2 worldForward = new Vector2(0, 1);
        float angle = -Vector2.SignedAngle(worldForward, to);
        if (angle < 0)
        {
            angle += 360; // Convert the angle to a range of 0 to 360
        }
        return angle;
    }

    public static float GetClockwiseAngle(Vector2 from, Vector2 to)
    {
        float angle = -Vector2.SignedAngle(from, to);
        if (angle < 0)
        {
            angle += 360; // Convert the angle to a range of 0 to 360
        }
        return angle;
    }

    //-- The same as heading
    public static float GetClockwiseAngleFromWorldForward(Vector3 to)
    {
        Vector2 worldForward = new Vector2(0, 1);
        float angle = -Vector2.SignedAngle(worldForward, to.To2D());
        if (angle < 0)
        {
            angle += 360; // Convert the angle to a range of 0 to 360
        }
        return angle;
    }
    /// <summary>
    /// Calculates the clockwise angle between two vectors.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The ending vector.</param>
    /// <returns>The clockwise angle in degrees between the two vectors.</returns>
    public static float GetClockwiseAngle(Vector3 from, Vector3 to, bool includeNegativeAngles  = false)
    {
        float angle = -Vector2.SignedAngle(from.To2D(), to.To2D());
        if (angle < 0)
        {
            angle += 360; // Convert the angle to a range of 0 to 360
        }

        if(includeNegativeAngles && angle > 180)
            angle -= 360;

        return angle;
    }

    //-----------------------
    public static bool IsVector3Zero(Vector3 v3, float tolerance = .0001f)
    {
        if (Mathf.Abs(v3.x) > tolerance || Mathf.Abs(v3.y) > tolerance || Mathf.Abs(v3.z) > tolerance)
            return false;

        return true;
    }

    //-- Only use when referenced by Bob, which should be never
    public static bool AreEqualBob(Vector3 a, Vector3 b, float epsilon = .0001f)
    {
        if (Vector3.SqrMagnitude(a - b) < epsilon)
            return true;
        return false;
    }

    //-----------------------
    public static Vector3 LocalToWorld(Transform trans, Vector3 point)
    {
        point = trans.TransformPoint(point);

        return point;
    }

    //-----------------------
    // Gets distance between 2 points ignoring height difference
    public static float GetFlatDistance(Vector3 a, Vector3 b)
    {
        Vector3 flatA = GetFlatVector(a);
        Vector3 flatB = GetFlatVector(b);
        float distance = Vector3.Distance(flatA, flatB);
        return distance;
    }
    /// <summary>Offsets an array of Vector2 by the given Vector2 offset.</summary>
    /// <param name="array">The array of Vector2 to offset.</param>
    /// <param name="offset">The Vector2 offset to apply.</param>
    /// <returns>The offset array of Vector2.</returns>
    public static Vector2[] OffsetVector2Array(Vector2[] array, Vector2 offset)
    {
        for (int i = 0; i < array.Length; i++) array[i] += offset;
        return array;
    }

    /// <summary>Throws an exception if called with a Vector3 offset.</summary>
    /// <param name="array">The array of Vector2.</param>
    /// <param name="offset">The Vector3 offset (incorrect type).</param>
    /// <returns>Throws an ArgumentException.</returns>
    public static Vector2[] OffsetVector2Array(Vector2[] array, Vector3 offset)
    {
        Debug.LogError("Did you mean to use a Vector3 as offset instead of Vector2?.\n");
        array = OffsetVector2Array(array, offset.To2D());
        return array;
    }


    //-----------------------
    // Gets copy of vector with y=0. Use ToXZ() Instead
    public static Vector3 GetFlatVector(Vector3 v)
    {
        Vector3 flatVector = new Vector3(v.x, 0, v.z);
        return flatVector;
    }

    //-- angle to turn relative to continuing forward
    public static float GetCornerAngle(Vector3 pt1, Vector3 pt2, Vector3 pt3)
    {
        if (pt3 == pt2)
            pt3 = -pt1;

        float heading1 = GetHeading(pt1, pt2);
        float heading2 = GetHeading(pt2, pt3);
        float angle = heading2 - heading1;

        if (angle < 0)
            angle = 360 + angle;

        return angle;
    }

    // Gets the positive 0-360 angle between two vectors' headings
    public static float GetCornerAngle(Vector3 directionIn, Vector3 directionOut)
    {
        float heading1 = Quaternion.LookRotation(directionIn).eulerAngles.y;
        float heading2 = Quaternion.LookRotation(directionOut).eulerAngles.y;
        float angle = heading2 - heading1;
        if (angle < 0)
            angle = 360 + angle;

        return angle;
    }

    public static float GetCornerAngle(float headingIn, float headingOut)
    {
        float angle = headingOut - headingIn;
        if (angle < 0)
            angle = 360 + angle;

        return angle;
    }

    //-- Just a reminder! Note this always 0 - 180
    public static float GetAngleXZ(Vector3 vecA, Vector3 vecB)
    {
        // zero out the y components
        vecA = vecA.ToY0();
        vecB = vecB.ToY0();
        float angle = Vector3.Angle(vecA, vecB);
        return angle;
    }

    //----------------
    public static float GetLengthOfArcOnCircle(float radius, float angleInDegrees)
    {
        // Convert angle from degrees to radians
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        // Calculate and return the arc length
        return angleInRadians * radius;
    }

    //----------------
    public static Vector3 GetDirection(Vector3 pt1, Vector3 pt2)
    {
        Vector3 currDirection = Quaternion.LookRotation(pt2 - pt1).eulerAngles;
        return currDirection;
    }

    //----------------
    public static float GetHeading(Vector3 pt1, Vector3 pt2)
    {
        Vector3 normVec = (pt2 - pt1).normalized;
        Vector3 currDirection = Quaternion.LookRotation(pt2 - pt1).eulerAngles;
        return currDirection.y;
    }
    public static float GetSignedHeading(Vector3 pt1, Vector3 pt2)
    {
        Vector3 normVec = (pt2 - pt1).normalized;
        Vector3 currDirection = Quaternion.LookRotation(pt2 - pt1).eulerAngles;
        float heading = currDirection.y;
        if (heading > 180)
            heading -= 360;
        
        return heading;
    }

    //----------------------
    public static float GetAngleFromZero(float angle)
    {
        if (angle <= 180 && angle >= 0)
            return angle;
        if (angle > 180)
            return 360 - angle;
        if (angle < -180)
            return 360 + angle;

        return -angle;
    }

    //-----------------
    public static string GetCompassStringFromHeading(float heading)
    {
        if (heading < 0)
            heading += 360;
        if (heading > 360)
            heading -= 360;

        string compassString = "";
        if (heading >= 337.5 || heading <= 22.5)
            compassString = "North  +Z";
        else if (heading > 22.5 && heading <= 67.5)
            compassString = "North East  +X +Z";
        else if (heading >= 67.5 && heading <= 112.5)
            compassString = "East  +X";
        else if (heading > 112.5 && heading < 157.5)
            compassString = "South East  +X -Z";
        else if (heading >= 157.5 && heading <= 202.5)
            compassString = "South -Z";
        else if (heading > 202.5 && heading < 247.5)
            compassString = "South West  -X -Z";
        else if (heading >= 247.5 && heading <= 292.5)
            compassString = "West  -X";
        else if (heading > 292.5 && heading < 337.5)
            compassString = "North West  -X +Z";

        return compassString;
    }

    public static float GetWidthAtElbow(Vector3 inPt, Vector3 elbowPt, Vector3 outPt, float width)
    {
        //outPt = new Vector3(-1, 0, 1);
        float halfWidth = width * 0.5f;
        float cornerWidth = 0;
        float angle = GetCornerAngle(inPt, elbowPt, outPt);
        float miterAngle = angle / 2 * Mathf.Deg2Rad;

        Vector3 pathCornerPtL = elbowPt + new Vector3(halfWidth, 0, 0);
        Vector3 pathCornerPtR = elbowPt + new Vector3(-halfWidth, 0, 0);

        float opp = Mathf.Tan(miterAngle) * (halfWidth);

        Vector3 newCornerPtL = pathCornerPtL + new Vector3(0, 0, opp);
        Vector3 newCornerPtR = pathCornerPtR + new Vector3(0, 0, -opp);

        cornerWidth = (newCornerPtR - newCornerPtL).magnitude;

        return cornerWidth;
    }

    //-----------------
    public static Vector3[] GetMiterPoints(Vector3 inPt, Vector3 elbowPt, Vector3 outPt, float width)
    {
        //outPt = new Vector3(-1, 0, 1);
        float halfWidth = width * 0.5f;
        float angle = GetCornerAngle(inPt, elbowPt, outPt);
        float miterAngle = angle / 2 * Mathf.Deg2Rad;

        Vector3 pathCornerPtL = elbowPt + new Vector3(halfWidth, 0, 0);
        Vector3 pathCornerPtR = elbowPt + new Vector3(-halfWidth, 0, 0);

        float opp = Mathf.Tan(miterAngle) * (halfWidth);

        Vector3 newCornerPtL = pathCornerPtL + new Vector3(0, 0, opp);
        Vector3 newCornerPtR = pathCornerPtR + new Vector3(0, 0, -opp);

        Vector3[] miterPoints = new Vector3[2];
        miterPoints[0] = newCornerPtL;
        miterPoints[1] = newCornerPtR;

        return miterPoints;
    }
    public static bool IsPointInTriangle(Vector2 pt, Vector2 triV0, Vector2 triV1, Vector2 triV2)
    {
        var d1 = Sign(pt, triV0, triV1);
        var d2 = Sign(pt, triV1, triV2);
        var d3 = Sign(pt, triV2, triV0);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }
    public static bool IsPointInPolygon(Vector3 point3D, Quadrilateral2D quad)
    {
        // Convert Vector3 to Vector2 (dropping the y component)
        Vector2 point = new Vector2(point3D.x, point3D.z);

        Vector2[] pts = quad.v;
        int j = pts.Length - 1;
        bool inside = false;

        for (int i = 0; i < pts.Length; j = i++)
        {
            if (((pts[i].y <= point.y && point.y < pts[j].y) ||
                 (pts[j].y <= point.y && point.y < pts[i].y)) &&
                (point.x < (pts[j].x - pts[i].x) * (point.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
    public static bool IsPointInPolygon2D(Vector2 point2D, Vector2[] pts)
    {
        int j = pts.Length - 1;
        bool inside = false;

        for (int i = 0; i < pts.Length; j = i++)
        {
            if (((pts[i].y <= point2D.y && point2D.y < pts[j].y) ||
                 (pts[j].y <= point2D.y && point2D.y < pts[i].y)) &&
                (point2D.x < (pts[j].x - pts[i].x) * (point2D.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
    public static bool IsPointInPolygon2D(Vector2 point2D, Quadrilateral2D quad)
    {
        Vector2[] pts = quad.v;
        int j = pts.Length - 1;
        bool inside = false;

        for (int i = 0; i < pts.Length; j = i++)
        {
            if (((pts[i].y <= point2D.y && point2D.y < pts[j].y) ||
                 (pts[j].y <= point2D.y && point2D.y < pts[i].y)) &&
                (point2D.x < (pts[j].x - pts[i].x) * (point2D.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    public static bool IsPointInPentagon(Vector3 point3D, Pentagon2D pent)
    {
        // Convert Vector3 to Vector2 (dropping the y component)
        Vector2 point = new Vector2(point3D.x, point3D.z);

        Vector2[] pts = pent.v;
        int j = pts.Length - 1;
        bool inside = false;

        for (int i = 0; i < pts.Length; j = i++)
        {
            if (((pts[i].y <= point.y && point.y < pts[j].y) ||
                 (pts[j].y <= point.y && point.y < pts[i].y)) &&
                (point.x < (pts[j].x - pts[i].x) * (point.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    public static bool IsPointInQuad(Vector2 p, Vector2[] quad)
    {
        // Assuming quad is an array of four points (quadrilateral)
        return IsPointInTriangle(p, quad[0], quad[1], quad[2]) ||
               IsPointInTriangle(p, quad[2], quad[3], quad[0]);
    }



    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}