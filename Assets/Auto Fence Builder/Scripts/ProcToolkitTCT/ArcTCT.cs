using MeshUtils;
using System.Collections.Generic;
using UnityEngine;

public class ArcTCT
{
    private Vector2 arcOrigin = Vector2.zero;
    private Vector2 arcStartPoint = new Vector2(0, 1); //12 oclock forward as default
    private Vector2 arcEndPoint = new Vector2(1, 0); //right, 3 oclock as default
    private float arcRadius = 1;
    private bool pointsEquidistant = true;

    //-- Three ways to define the placemnet of points on the arc:
    private float arcSegmentLength = 0.1f; // Defined by a segment length. This may adapt slightly if equidistance is needed

    private float arcSegmentAngle = 5.0f; // Or by an angle
    private int arcSegmentCount = 0; // Or, the number of segments to use between start and end points

    public void Init(Vector2 origin, Vector2 start, Vector2 end, float radius, float arcSegLength = 0.2f, float arcSegAngle = 5.0f)
    {
        arcOrigin = origin;
        arcStartPoint = start;
        arcEndPoint = end;
        arcRadius = radius;
        arcSegmentLength = arcSegLength;
        arcSegmentAngle = arcSegAngle;

        // For AFWB use, the points will lie on the same arc, but warn anyway if they're way off
        float originToStartLength = Vector2.Distance(origin, start);
        float originToEndLength = Vector2.Distance(origin, end);
        if (Mathf.Abs(originToStartLength - originToEndLength) > 0.1f)
        {
            Debug.LogWarning("ArcTCT: The start and end points are not with 0.1m the same arc. This is not supported by AFWB\n");
        }
    }

    //-- segmentCount is the number of segments to use between start and end points
    //-- origin is the poivot or Post point
    //-- start and end are the points on the arc
    public static List<Vector2> CreateArcWithSegmentCount(Vector2 origin, Vector2 startVecWorld, Vector2 endVecWorld, float radius, int segmentCount)
    {
        //As the angle is relative from the startVecWorld, we also consider this the local forward
        //float angle = Vector2.Angle(startVecWorld, endVecWorld);

        Vector2 startVecLocal = startVecWorld - origin;
        Vector2 endVecLocal = endVecWorld - origin;
        float angle = VectorUtilitiesTCT.GetClockwiseAngle(startVecLocal, endVecLocal);
        float signedAngle = -Vector2.SignedAngle(startVecLocal, endVecLocal);

        float segmentAngle = signedAngle / segmentCount;
        int numInterPoints = segmentCount - 1;
        float cumulativeAngle = 0;
        List<Vector2> segmentPoints = new List<Vector2>();

        //segmentPoints.Add(radiusLengthStart);
        for (int i = 0; i <= numInterPoints + 1; i++)
        {
            Vector3 pt = MeshUtilitiesAFWB.RotatePointAroundPivotYFromLocalForwardNoQ(startVecWorld.To3D(), origin.To3D(), cumulativeAngle, radius);
            Vector2 pt2D = new Vector2(pt.x, pt.z);
            segmentPoints.Add(pt2D);
            cumulativeAngle += segmentAngle;
        }
        return segmentPoints;
    }

    // SegmentLength is the distance between points on the arc, in order to have equidistant points on the arc the segmentLength may be adjusted slightly
    // Note: This includes the start and end points. You can remove them with: segmentPoints.RemoveAt(0) and segmentPoints.RemoveAt(segmentPoints.Count - 1)
    public static List<Vector2> CreateArcWithSegmentDistance(Vector2 origin, Vector2 startVecWorld, Vector2 endVecWorld, float radius, float targetSegmentLength)
    {
        //To get the angle we need local pos, so subtract the origin from the start and end points
        Vector2 startVecLocal = startVecWorld - origin;
        Vector2 endVecLocal = endVecWorld - origin;
        float angle = Vector2.Angle(startVecLocal, endVecLocal);
        float signedAngle = -Vector2.SignedAngle(startVecLocal, endVecLocal);

        float arcLength = VectorUtilitiesTCT.GetLengthOfArcOnCircle(radius, signedAngle);
        arcLength = Mathf.Abs(arcLength);

        int numSegments = (int)Mathf.Ceil(arcLength / targetSegmentLength);

        List<Vector2> arcSegmentPoints = CreateArcWithSegmentCount(origin, startVecWorld, endVecWorld, radius, numSegments);
        return arcSegmentPoints;
    }

    //-- segmentAngle is the angle between points on the arc, in order to have equidistant points on the arc the angle may be adjusted slightly
    public static List<Vector2> CreateArcWithSegmentAngles(Vector2 origin, Vector2 startVecWorld, Vector2 endVecWorld, float radius, float targetAngle)
    {
        float angle = Vector2.Angle(startVecWorld, endVecWorld);
        int numSegments = (int)Mathf.Round(angle / targetAngle);
        float realSegmentAngle = angle / numSegments;
        List<Vector2> arcSegmentPoints = CreateArcWithSegmentCount(origin, startVecWorld, endVecWorld, radius, numSegments);
        return arcSegmentPoints;
    }
}