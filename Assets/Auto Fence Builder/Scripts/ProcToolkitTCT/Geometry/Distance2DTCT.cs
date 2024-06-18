using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Collection of distance calculation algorithms
    /// </summary>
    public static partial class DistanceTCT
    {
        #region Point-Line

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the line
        /// </summary>
        public static float PointLine(Vector2 point, Line2TCT line)
        {
            return Vector2.Distance(point, ClosestTCT.PointLine(point, line));
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the line
        /// </summary>
        public static float PointLine(Vector2 point, Vector2 lineOrigin, Vector2 lineDirection)
        {
            return Vector2.Distance(point, ClosestTCT.PointLine(point, lineOrigin, lineDirection));
        }

        #endregion Point-Line

        #region Point-Ray

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the ray
        /// </summary>
        public static float PointRay(Vector2 point, Ray2D ray)
        {
            return Vector2.Distance(point, ClosestTCT.PointRay(point, ray));
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the ray
        /// </summary>
        /// <param name="rayDirection">Normalized direction of the ray</param>
        public static float PointRay(Vector2 point, Vector2 rayOrigin, Vector2 rayDirection)
        {
            return Vector2.Distance(point, ClosestTCT.PointRay(point, rayOrigin, rayDirection));
        }

        #endregion Point-Ray

        #region Point-Segment

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the segment
        /// </summary>
        public static float PointSegment(Vector2 point, Segment2TCT segment)
        {
            return Vector2.Distance(point, ClosestTCT.PointSegment(point, segment));
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the segment
        /// </summary>
        public static float PointSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB)
        {
            return Vector2.Distance(point, ClosestTCT.PointSegment(point, segmentA, segmentB));
        }

        private static float PointSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB, Vector2 segmentDirection, float segmentLength)
        {
            float pointProjection = Vector2.Dot(segmentDirection, point - segmentA);
            if (pointProjection < -GeometryTCT.Epsilon)
            {
                return Vector2.Distance(point, segmentA);
            }
            if (pointProjection > segmentLength + GeometryTCT.Epsilon)
            {
                return Vector2.Distance(point, segmentB);
            }
            return Vector2.Distance(point, segmentA + segmentDirection * pointProjection);
        }

        #endregion Point-Segment

        #region Point-Circle

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the circle
        /// </summary>
        /// <returns>Positive value if the point is outside, negative otherwise</returns>
        public static float PointCircle(Vector2 point, Circle2TCT circle)
        {
            return PointCircle(point, circle.center, circle.radius);
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the circle
        /// </summary>
        /// <returns>Positive value if the point is outside, negative otherwise</returns>
        public static float PointCircle(Vector2 point, Vector2 circleCenter, float circleRadius)
        {
            return (circleCenter - point).magnitude - circleRadius;
        }

        #endregion Point-Circle

        #region Line-Line

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the lines
        /// </summary>
        public static float LineLine(Line2TCT lineA, Line2TCT lineB)
        {
            return LineLine(lineA.origin, lineA.direction, lineB.origin, lineB.direction);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the lines
        /// </summary>
        public static float LineLine(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            if (Mathf.Abs(VectorETCT.PerpDot(directionA, directionB)) < GeometryTCT.Epsilon)
            {
                // Parallel
                Vector2 originBToA = originA - originB;
                if (Mathf.Abs(VectorETCT.PerpDot(directionA, originBToA)) > GeometryTCT.Epsilon ||
                    Mathf.Abs(VectorETCT.PerpDot(directionB, originBToA)) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    float originBProjection = Vector2.Dot(directionA, originBToA);
                    float distanceSqr = originBToA.sqrMagnitude - originBProjection * originBProjection;
                    // distanceSqr can be negative
                    return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                }

                // Collinear
                return 0;
            }

            // Not parallel
            return 0;
        }

        #endregion Line-Line

        #region Line-Ray

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the ray
        /// </summary>
        public static float LineRay(Line2TCT line, Ray2D ray)
        {
            return LineRay(line.origin, line.direction, ray.origin, ray.direction);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the ray
        /// </summary>
        public static float LineRay(Vector2 lineOrigin, Vector2 lineDirection, Vector2 rayOrigin, Vector2 rayDirection)
        {
            Vector2 rayOriginToLineOrigin = lineOrigin - rayOrigin;
            float denominator = VectorETCT.PerpDot(lineDirection, rayDirection);
            float perpDotA = VectorETCT.PerpDot(lineDirection, rayOriginToLineOrigin);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                float perpDotB = VectorETCT.PerpDot(rayDirection, rayOriginToLineOrigin);
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    float rayOriginProjection = Vector2.Dot(lineDirection, rayOriginToLineOrigin);
                    float distanceSqr = rayOriginToLineOrigin.sqrMagnitude - rayOriginProjection * rayOriginProjection;
                    // distanceSqr can be negative
                    return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                }
                // Collinear
                return 0;
            }

            // Not parallel
            float rayDistance = perpDotA / denominator;
            if (rayDistance < -GeometryTCT.Epsilon)
            {
                // No intersection
                float rayOriginProjection = Vector2.Dot(lineDirection, rayOriginToLineOrigin);
                Vector2 linePoint = lineOrigin - lineDirection * rayOriginProjection;
                return Vector2.Distance(linePoint, rayOrigin);
            }
            // Point intersection
            return 0;
        }

        #endregion Line-Ray

        #region Line-Segment

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the segment
        /// </summary>
        public static float LineSegment(Line2TCT line, Segment2TCT segment)
        {
            return LineSegment(line.origin, line.direction, segment.a, segment.b);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the segment
        /// </summary>
        public static float LineSegment(Vector2 lineOrigin, Vector2 lineDirection, Vector2 segmentA, Vector2 segmentB)
        {
            Vector2 segmentAToOrigin = lineOrigin - segmentA;
            Vector2 segmentDirection = segmentB - segmentA;
            float denominator = VectorETCT.PerpDot(lineDirection, segmentDirection);
            float perpDotA = VectorETCT.PerpDot(lineDirection, segmentAToOrigin);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                // Normalized direction gives more stable results
                float perpDotB = VectorETCT.PerpDot(segmentDirection.normalized, segmentAToOrigin);
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    float segmentAProjection = Vector2.Dot(lineDirection, segmentAToOrigin);
                    float distanceSqr = segmentAToOrigin.sqrMagnitude - segmentAProjection * segmentAProjection;
                    // distanceSqr can be negative
                    return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                }
                // Collinear
                return 0;
            }

            // Not parallel
            float segmentDistance = perpDotA / denominator;
            if (segmentDistance < -GeometryTCT.Epsilon || segmentDistance > 1 + GeometryTCT.Epsilon)
            {
                // No intersection
                Vector2 segmentPoint = segmentA + segmentDirection * Mathf.Clamp01(segmentDistance);
                float segmentPointProjection = Vector2.Dot(lineDirection, segmentPoint - lineOrigin);
                Vector2 linePoint = lineOrigin + lineDirection * segmentPointProjection;
                return Vector2.Distance(linePoint, segmentPoint);
            }
            // Point intersection
            return 0;
        }

        #endregion Line-Segment

        #region Line-Circle

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the circle
        /// </summary>
        public static float LineCircle(Line2TCT line, Circle2TCT circle)
        {
            return LineCircle(line.origin, line.direction, circle.center, circle.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the circle
        /// </summary>
        public static float LineCircle(Vector2 lineOrigin, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
        {
            Vector2 originToCenter = circleCenter - lineOrigin;
            float centerProjection = Vector2.Dot(lineDirection, originToCenter);
            float sqrDistanceToLine = originToCenter.sqrMagnitude - centerProjection * centerProjection;
            float sqrDistanceToIntersection = circleRadius * circleRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                // No intersection
                return Mathf.Sqrt(sqrDistanceToLine) - circleRadius;
            }
            return 0;
        }

        #endregion Line-Circle

        #region Ray-Ray

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the rays
        /// </summary>
        public static float RayRay(Ray2D rayA, Ray2D rayB)
        {
            return RayRay(rayA.origin, rayA.direction, rayB.origin, rayB.direction);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the rays
        /// </summary>
        public static float RayRay(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            Vector2 originBToA = originA - originB;
            float denominator = VectorETCT.PerpDot(directionA, directionB);
            float perpDotA = VectorETCT.PerpDot(directionA, originBToA);
            float perpDotB = VectorETCT.PerpDot(directionB, originBToA);

            bool codirected = Vector2.Dot(directionA, directionB) > 0;
            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                float originBProjection = -Vector2.Dot(directionA, originBToA);
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    if (!codirected && originBProjection < GeometryTCT.Epsilon)
                    {
                        return Vector2.Distance(originA, originB);
                    }
                    float distanceSqr = originBToA.sqrMagnitude - originBProjection * originBProjection;
                    // distanceSqr can be negative
                    return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                }
                // Collinear

                if (codirected)
                {
                    // Ray intersection
                    return 0;
                }
                else
                {
                    if (originBProjection < GeometryTCT.Epsilon)
                    {
                        // No intersection
                        return Vector2.Distance(originA, originB);
                    }
                    else
                    {
                        // Segment intersection
                        return 0;
                    }
                }
            }

            // Not parallel
            float distanceA = perpDotB / denominator;
            float distanceB = perpDotA / denominator;
            if (distanceA < -GeometryTCT.Epsilon || distanceB < -GeometryTCT.Epsilon)
            {
                // No intersection
                if (codirected)
                {
                    float originAProjection = Vector2.Dot(directionB, originBToA);
                    if (originAProjection > -GeometryTCT.Epsilon)
                    {
                        Vector2 rayPointA = originA;
                        Vector2 rayPointB = originB + directionB * originAProjection;
                        return Vector2.Distance(rayPointA, rayPointB);
                    }
                    float originBProjection = -Vector2.Dot(directionA, originBToA);
                    if (originBProjection > -GeometryTCT.Epsilon)
                    {
                        Vector2 rayPointA = originA + directionA * originBProjection;
                        Vector2 rayPointB = originB;
                        return Vector2.Distance(rayPointA, rayPointB);
                    }
                    return Vector2.Distance(originA, originB);
                }
                else
                {
                    if (distanceA > -GeometryTCT.Epsilon)
                    {
                        float originBProjection = -Vector2.Dot(directionA, originBToA);
                        if (originBProjection > -GeometryTCT.Epsilon)
                        {
                            Vector2 rayPointA = originA + directionA * originBProjection;
                            Vector2 rayPointB = originB;
                            return Vector2.Distance(rayPointA, rayPointB);
                        }
                    }
                    else if (distanceB > -GeometryTCT.Epsilon)
                    {
                        float originAProjection = Vector2.Dot(directionB, originBToA);
                        if (originAProjection > -GeometryTCT.Epsilon)
                        {
                            Vector2 rayPointA = originA;
                            Vector2 rayPointB = originB + directionB * originAProjection;
                            return Vector2.Distance(rayPointA, rayPointB);
                        }
                    }
                    return Vector2.Distance(originA, originB);
                }
            }
            // Point intersection
            return 0;
        }

        #endregion Ray-Ray

        #region Ray-Segment

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the ray and the segment
        /// </summary>
        public static float RaySegment(Ray2D ray, Segment2TCT segment)
        {
            return RaySegment(ray.origin, ray.direction, segment.a, segment.b);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the ray and the segment
        /// </summary>
        public static float RaySegment(Vector2 rayOrigin, Vector2 rayDirection, Vector2 segmentA, Vector2 segmentB)
        {
            Vector2 segmentAToOrigin = rayOrigin - segmentA;
            Vector2 segmentDirection = segmentB - segmentA;
            float denominator = VectorETCT.PerpDot(rayDirection, segmentDirection);
            float perpDotA = VectorETCT.PerpDot(rayDirection, segmentAToOrigin);
            // Normalized direction gives more stable results
            float perpDotB = VectorETCT.PerpDot(segmentDirection.normalized, segmentAToOrigin);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                float segmentAProjection = -Vector2.Dot(rayDirection, segmentAToOrigin);
                Vector2 originToSegmentB = segmentB - rayOrigin;
                float segmentBProjection = Vector2.Dot(rayDirection, originToSegmentB);
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    if (segmentAProjection > -GeometryTCT.Epsilon)
                    {
                        float distanceSqr = segmentAToOrigin.sqrMagnitude - segmentAProjection * segmentAProjection;
                        // distanceSqr can be negative
                        return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                    }
                    if (segmentBProjection > -GeometryTCT.Epsilon)
                    {
                        float distanceSqr = originToSegmentB.sqrMagnitude - segmentBProjection * segmentBProjection;
                        // distanceSqr can be negative
                        return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                    }

                    if (segmentAProjection > segmentBProjection)
                    {
                        return Vector2.Distance(rayOrigin, segmentA);
                    }
                    return Vector2.Distance(rayOrigin, segmentB);
                }
                // Collinear
                if (segmentAProjection > -GeometryTCT.Epsilon || segmentBProjection > -GeometryTCT.Epsilon)
                {
                    // Point or segment intersection
                    return 0;
                }
                // No intersection
                return segmentAProjection > segmentBProjection ? -segmentAProjection : -segmentBProjection;
            }

            // Not parallel
            float rayDistance = perpDotB / denominator;
            float segmentDistance = perpDotA / denominator;
            if (rayDistance < -GeometryTCT.Epsilon ||
                segmentDistance < -GeometryTCT.Epsilon || segmentDistance > 1 + GeometryTCT.Epsilon)
            {
                // No intersection
                bool codirected = Vector2.Dot(rayDirection, segmentDirection) > 0;
                Vector2 segmentBToOrigin;
                if (!codirected)
                {
                    PTUtilsTCT.Swap(ref segmentA, ref segmentB);
                    segmentDirection = -segmentDirection;
                    segmentBToOrigin = segmentAToOrigin;
                    segmentAToOrigin = rayOrigin - segmentA;
                    segmentDistance = 1 - segmentDistance;
                }
                else
                {
                    segmentBToOrigin = rayOrigin - segmentB;
                }

                float segmentAProjection = -Vector2.Dot(rayDirection, segmentAToOrigin);
                float segmentBProjection = -Vector2.Dot(rayDirection, segmentBToOrigin);
                bool segmentAOnRay = segmentAProjection > -GeometryTCT.Epsilon;
                bool segmentBOnRay = segmentBProjection > -GeometryTCT.Epsilon;
                if (segmentAOnRay && segmentBOnRay)
                {
                    if (segmentDistance < 0)
                    {
                        Vector2 rayPoint = rayOrigin + rayDirection * segmentAProjection;
                        Vector2 segmentPoint = segmentA;
                        return Vector2.Distance(rayPoint, segmentPoint);
                    }
                    else
                    {
                        Vector2 rayPoint = rayOrigin + rayDirection * segmentBProjection;
                        Vector2 segmentPoint = segmentB;
                        return Vector2.Distance(rayPoint, segmentPoint);
                    }
                }
                else if (!segmentAOnRay && segmentBOnRay)
                {
                    if (segmentDistance < 0)
                    {
                        Vector2 rayPoint = rayOrigin;
                        Vector2 segmentPoint = segmentA;
                        return Vector2.Distance(rayPoint, segmentPoint);
                    }
                    else if (segmentDistance > 1 + GeometryTCT.Epsilon)
                    {
                        Vector2 rayPoint = rayOrigin + rayDirection * segmentBProjection;
                        Vector2 segmentPoint = segmentB;
                        return Vector2.Distance(rayPoint, segmentPoint);
                    }
                    else
                    {
                        Vector2 rayPoint = rayOrigin;
                        float originProjection = Vector2.Dot(segmentDirection, segmentAToOrigin);
                        Vector2 segmentPoint = segmentA + segmentDirection * originProjection / segmentDirection.sqrMagnitude;
                        return Vector2.Distance(rayPoint, segmentPoint);
                    }
                }
                else
                {
                    // Not on ray
                    Vector2 rayPoint = rayOrigin;
                    float originProjection = Vector2.Dot(segmentDirection, segmentAToOrigin);
                    float sqrSegmentLength = segmentDirection.sqrMagnitude;
                    if (originProjection < 0)
                    {
                        return Vector2.Distance(rayPoint, segmentA);
                    }
                    else if (originProjection > sqrSegmentLength)
                    {
                        return Vector2.Distance(rayPoint, segmentB);
                    }
                    else
                    {
                        Vector2 segmentPoint = segmentA + segmentDirection * originProjection / sqrSegmentLength;
                        return Vector2.Distance(rayPoint, segmentPoint);
                    }
                }
            }
            // Point intersection
            return 0;
        }

        #endregion Ray-Segment

        #region Ray-Circle

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the ray and the circle
        /// </summary>
        public static float RayCircle(Ray2D ray, Circle2TCT circle)
        {
            return RayCircle(ray.origin, ray.direction, circle.center, circle.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the ray and the circle
        /// </summary>
        public static float RayCircle(Vector2 rayOrigin, Vector2 rayDirection, Vector2 circleCenter, float circleRadius)
        {
            Vector2 originToCenter = circleCenter - rayOrigin;
            float centerProjection = Vector2.Dot(rayDirection, originToCenter);
            if (centerProjection + circleRadius < -GeometryTCT.Epsilon)
            {
                // No intersection
                return Mathf.Sqrt(originToCenter.sqrMagnitude) - circleRadius;
            }

            float sqrDistanceToOrigin = originToCenter.sqrMagnitude;
            float sqrDistanceToLine = sqrDistanceToOrigin - centerProjection * centerProjection;
            float sqrDistanceToIntersection = circleRadius * circleRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                // No intersection
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    return Mathf.Sqrt(sqrDistanceToOrigin) - circleRadius;
                }
                return Mathf.Sqrt(sqrDistanceToLine) - circleRadius;
            }
            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    // No intersection
                    return Mathf.Sqrt(sqrDistanceToOrigin) - circleRadius;
                }
                // Point intersection
                return 0;
            }

            // Line intersection
            float distanceToIntersection = Mathf.Sqrt(sqrDistanceToIntersection);
            float distanceA = centerProjection - distanceToIntersection;
            float distanceB = centerProjection + distanceToIntersection;

            if (distanceA < -GeometryTCT.Epsilon)
            {
                if (distanceB < -GeometryTCT.Epsilon)
                {
                    // No intersection
                    return Mathf.Sqrt(sqrDistanceToOrigin) - circleRadius;
                }

                // Point intersection;
                return 0;
            }

            // Two points intersection;
            return 0;
        }

        #endregion Ray-Circle

        #region Segment-Segment

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the segments
        /// </summary>
        public static float SegmentSegment(Segment2TCT segment1, Segment2TCT Segment2TCT)
        {
            return SegmentSegment(segment1.a, segment1.b, Segment2TCT.a, Segment2TCT.b);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the segments
        /// </summary>
        public static float SegmentSegment(Vector2 segment1A, Vector2 segment1B, Vector2 Segment2TCTA, Vector2 Segment2TCTB)
        {
            Vector2 from2ATo1A = segment1A - Segment2TCTA;
            Vector2 direction1 = segment1B - segment1A;
            Vector2 direction2 = Segment2TCTB - Segment2TCTA;
            float segment1Length = direction1.magnitude;
            float Segment2TCTLength = direction2.magnitude;

            bool segment1IsAPoint = segment1Length < GeometryTCT.Epsilon;
            bool Segment2TCTIsAPoint = Segment2TCTLength < GeometryTCT.Epsilon;
            if (segment1IsAPoint && Segment2TCTIsAPoint)
            {
                return Vector2.Distance(segment1A, Segment2TCTA);
            }
            if (segment1IsAPoint)
            {
                direction2.Normalize();
                return PointSegment(segment1A, Segment2TCTA, Segment2TCTB, direction2, Segment2TCTLength);
            }
            if (Segment2TCTIsAPoint)
            {
                direction1.Normalize();
                return PointSegment(Segment2TCTA, segment1A, segment1B, direction1, segment1Length);
            }

            direction1.Normalize();
            direction2.Normalize();
            float denominator = VectorETCT.PerpDot(direction1, direction2);
            float perpDot1 = VectorETCT.PerpDot(direction1, from2ATo1A);
            float perpDot2 = VectorETCT.PerpDot(direction2, from2ATo1A);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                if (Mathf.Abs(perpDot1) > GeometryTCT.Epsilon || Mathf.Abs(perpDot2) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    float Segment2TCTAProjection = -Vector2.Dot(direction1, from2ATo1A);
                    if (Segment2TCTAProjection > -GeometryTCT.Epsilon &&
                        Segment2TCTAProjection < segment1Length + GeometryTCT.Epsilon)
                    {
                        float distanceSqr = from2ATo1A.sqrMagnitude - Segment2TCTAProjection * Segment2TCTAProjection;
                        // distanceSqr can be negative
                        return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                    }

                    Vector2 from1ATo2B = Segment2TCTB - segment1A;
                    float Segment2TCTBProjection = Vector2.Dot(direction1, from1ATo2B);
                    if (Segment2TCTBProjection > -GeometryTCT.Epsilon &&
                        Segment2TCTBProjection < segment1Length + GeometryTCT.Epsilon)
                    {
                        float distanceSqr = from1ATo2B.sqrMagnitude - Segment2TCTBProjection * Segment2TCTBProjection;
                        // distanceSqr can be negative
                        return distanceSqr <= 0 ? 0 : Mathf.Sqrt(distanceSqr);
                    }

                    if (Segment2TCTAProjection < 0 && Segment2TCTBProjection < 0)
                    {
                        if (Segment2TCTAProjection > Segment2TCTBProjection)
                        {
                            return Vector2.Distance(segment1A, Segment2TCTA);
                        }
                        return Vector2.Distance(segment1A, Segment2TCTB);
                    }
                    if (Segment2TCTAProjection > 0 && Segment2TCTBProjection > 0)
                    {
                        if (Segment2TCTAProjection < Segment2TCTBProjection)
                        {
                            return Vector2.Distance(segment1B, Segment2TCTA);
                        }
                        return Vector2.Distance(segment1B, Segment2TCTB);
                    }
                    float segment1AProjection = Vector2.Dot(direction2, from2ATo1A);
                    Vector2 Segment2TCTPoint = Segment2TCTA + direction2 * segment1AProjection;
                    return Vector2.Distance(segment1A, Segment2TCTPoint);
                }
                // Collinear

                bool codirected = Vector2.Dot(direction1, direction2) > 0;
                if (codirected)
                {
                    // Codirected
                    float Segment2TCTAProjection = -Vector2.Dot(direction1, from2ATo1A);
                    if (Segment2TCTAProjection > -GeometryTCT.Epsilon)
                    {
                        // 1A------1B
                        //     2A------2B
                        return SegmentSegmentCollinear(segment1A, segment1B, Segment2TCTA);
                    }
                    else
                    {
                        //     1A------1B
                        // 2A------2B
                        return SegmentSegmentCollinear(Segment2TCTA, Segment2TCTB, segment1A);
                    }
                }
                else
                {
                    // Contradirected
                    float Segment2TCTBProjection = Vector2.Dot(direction1, Segment2TCTB - segment1A);
                    if (Segment2TCTBProjection > -GeometryTCT.Epsilon)
                    {
                        // 1A------1B
                        //     2B------2A
                        return SegmentSegmentCollinear(segment1A, segment1B, Segment2TCTB);
                    }
                    else
                    {
                        //     1A------1B
                        // 2B------2A
                        return SegmentSegmentCollinear(Segment2TCTB, Segment2TCTA, segment1A);
                    }
                }
            }

            // Not parallel
            float distance1 = perpDot2 / denominator;
            float distance2 = perpDot1 / denominator;
            if (distance1 < -GeometryTCT.Epsilon || distance1 > segment1Length + GeometryTCT.Epsilon ||
                distance2 < -GeometryTCT.Epsilon || distance2 > Segment2TCTLength + GeometryTCT.Epsilon)
            {
                // No intersection
                bool codirected = Vector2.Dot(direction1, direction2) > 0;
                Vector2 from1ATo2B;
                if (!codirected)
                {
                    PTUtilsTCT.Swap(ref Segment2TCTA, ref Segment2TCTB);
                    direction2 = -direction2;
                    from1ATo2B = -from2ATo1A;
                    from2ATo1A = segment1A - Segment2TCTA;
                    distance2 = Segment2TCTLength - distance2;
                }
                else
                {
                    from1ATo2B = Segment2TCTB - segment1A;
                }
                Vector2 segment1Point;
                Vector2 Segment2TCTPoint;

                float Segment2TCTAProjection = -Vector2.Dot(direction1, from2ATo1A);
                float Segment2TCTBProjection = Vector2.Dot(direction1, from1ATo2B);

                bool Segment2TCTAIsAfter1A = Segment2TCTAProjection > -GeometryTCT.Epsilon;
                bool Segment2TCTBIsBefore1B = Segment2TCTBProjection < segment1Length + GeometryTCT.Epsilon;
                bool Segment2TCTAOnSegment1 = Segment2TCTAIsAfter1A && Segment2TCTAProjection < segment1Length + GeometryTCT.Epsilon;
                bool Segment2TCTBOnSegment1 = Segment2TCTBProjection > -GeometryTCT.Epsilon && Segment2TCTBIsBefore1B;
                if (Segment2TCTAOnSegment1 && Segment2TCTBOnSegment1)
                {
                    if (distance2 < -GeometryTCT.Epsilon)
                    {
                        segment1Point = segment1A + direction1 * Segment2TCTAProjection;
                        Segment2TCTPoint = Segment2TCTA;
                    }
                    else
                    {
                        segment1Point = segment1A + direction1 * Segment2TCTBProjection;
                        Segment2TCTPoint = Segment2TCTB;
                    }
                }
                else if (!Segment2TCTAOnSegment1 && !Segment2TCTBOnSegment1)
                {
                    if (!Segment2TCTAIsAfter1A && !Segment2TCTBIsBefore1B)
                    {
                        segment1Point = distance1 < -GeometryTCT.Epsilon ? segment1A : segment1B;
                    }
                    else
                    {
                        // Not on segment
                        segment1Point = Segment2TCTAIsAfter1A ? segment1B : segment1A;
                    }
                    float segment1PointProjection = Vector2.Dot(direction2, segment1Point - Segment2TCTA);
                    segment1PointProjection = Mathf.Clamp(segment1PointProjection, 0, Segment2TCTLength);
                    Segment2TCTPoint = Segment2TCTA + direction2 * segment1PointProjection;
                }
                else if (Segment2TCTAOnSegment1)
                {
                    if (distance2 < -GeometryTCT.Epsilon)
                    {
                        segment1Point = segment1A + direction1 * Segment2TCTAProjection;
                        Segment2TCTPoint = Segment2TCTA;
                    }
                    else
                    {
                        segment1Point = segment1B;
                        float segment1PointProjection = Vector2.Dot(direction2, segment1Point - Segment2TCTA);
                        segment1PointProjection = Mathf.Clamp(segment1PointProjection, 0, Segment2TCTLength);
                        Segment2TCTPoint = Segment2TCTA + direction2 * segment1PointProjection;
                    }
                }
                else
                {
                    if (distance2 > Segment2TCTLength + GeometryTCT.Epsilon)
                    {
                        segment1Point = segment1A + direction1 * Segment2TCTBProjection;
                        Segment2TCTPoint = Segment2TCTB;
                    }
                    else
                    {
                        segment1Point = segment1A;
                        float segment1PointProjection = Vector2.Dot(direction2, segment1Point - Segment2TCTA);
                        segment1PointProjection = Mathf.Clamp(segment1PointProjection, 0, Segment2TCTLength);
                        Segment2TCTPoint = Segment2TCTA + direction2 * segment1PointProjection;
                    }
                }
                return Vector2.Distance(segment1Point, Segment2TCTPoint);
            }

            // Point intersection
            return 0;
        }

        private static float SegmentSegmentCollinear(Vector2 leftA, Vector2 leftB, Vector2 rightA)
        {
            Vector2 leftDirection = leftB - leftA;
            float rightAProjection = Vector2.Dot(leftDirection.normalized, rightA - leftB);
            if (Mathf.Abs(rightAProjection) < GeometryTCT.Epsilon)
            {
                // LB == RA
                // LA------LB
                //         RA------RB

                // Point intersection
                return 0;
            }
            if (rightAProjection < 0)
            {
                // LB > RA
                // LA------LB
                //     RARB
                //     RA--RB
                //     RA------RB

                // Segment intersection
                return 0;
            }
            // LB < RA
            // LA------LB
            //             RA------RB

            // No intersection
            return rightAProjection;
        }

        #endregion Segment-Segment

        #region Segment-Circle

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the segment and the circle
        /// </summary>
        public static float SegmentCircle(Segment2TCT segment, Circle2TCT circle)
        {
            return SegmentCircle(segment.a, segment.b, circle.center, circle.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the segment and the circle
        /// </summary>
        public static float SegmentCircle(Vector2 segmentA, Vector2 segmentB, Vector2 circleCenter, float circleRadius)
        {
            Vector2 segmentAToCenter = circleCenter - segmentA;
            Vector2 fromAtoB = segmentB - segmentA;
            float segmentLength = fromAtoB.magnitude;
            if (segmentLength < GeometryTCT.Epsilon)
            {
                return segmentAToCenter.magnitude - circleRadius;
            }

            Vector2 segmentDirection = fromAtoB.normalized;
            float centerProjection = Vector2.Dot(segmentDirection, segmentAToCenter);
            if (centerProjection + circleRadius < -GeometryTCT.Epsilon ||
                centerProjection - circleRadius > segmentLength + GeometryTCT.Epsilon)
            {
                // No intersection
                if (centerProjection < 0)
                {
                    return segmentAToCenter.magnitude - circleRadius;
                }
                return (circleCenter - segmentB).magnitude - circleRadius;
            }

            float sqrDistanceToA = segmentAToCenter.sqrMagnitude;
            float sqrDistanceToLine = sqrDistanceToA - centerProjection * centerProjection;
            float sqrDistanceToIntersection = circleRadius * circleRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                // No intersection
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    return Mathf.Sqrt(sqrDistanceToA) - circleRadius;
                }
                if (centerProjection > segmentLength + GeometryTCT.Epsilon)
                {
                    return (circleCenter - segmentB).magnitude - circleRadius;
                }
                return Mathf.Sqrt(sqrDistanceToLine) - circleRadius;
            }

            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    // No intersection
                    return Mathf.Sqrt(sqrDistanceToA) - circleRadius;
                }
                if (centerProjection > segmentLength + GeometryTCT.Epsilon)
                {
                    // No intersection
                    return (circleCenter - segmentB).magnitude - circleRadius;
                }
                // Point intersection
                return 0;
            }

            // Line intersection
            float distanceToIntersection = Mathf.Sqrt(sqrDistanceToIntersection);
            float distanceA = centerProjection - distanceToIntersection;
            float distanceB = centerProjection + distanceToIntersection;

            bool pointAIsAfterSegmentA = distanceA > -GeometryTCT.Epsilon;
            bool pointBIsBeforeSegmentB = distanceB < segmentLength + GeometryTCT.Epsilon;

            if (pointAIsAfterSegmentA && pointBIsBeforeSegmentB)
            {
                // Two points intersection
                return 0;
            }
            if (!pointAIsAfterSegmentA && !pointBIsBeforeSegmentB)
            {
                // The segment is inside, but no intersection
                distanceB = -(distanceB - segmentLength);
                return distanceA > distanceB ? distanceA : distanceB;
            }

            bool pointAIsBeforeSegmentB = distanceA < segmentLength + GeometryTCT.Epsilon;
            if (pointAIsAfterSegmentA && pointAIsBeforeSegmentB)
            {
                // Point A intersection
                return 0;
            }
            bool pointBIsAfterSegmentA = distanceB > -GeometryTCT.Epsilon;
            if (pointBIsAfterSegmentA && pointBIsBeforeSegmentB)
            {
                // Point B intersection
                return 0;
            }

            // No intersection
            if (centerProjection < 0)
            {
                return Mathf.Sqrt(sqrDistanceToA) - circleRadius;
            }
            return (circleCenter - segmentB).magnitude - circleRadius;
        }

        #endregion Segment-Circle

        #region Circle-Circle

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the circles
        /// </summary>
        /// <returns>
        /// Positive value if the circles do not intersect, negative otherwise.
        /// Negative value can be interpreted as depth of penetration.
        /// </returns>
        public static float CircleCircle(Circle2TCT circleA, Circle2TCT circleB)
        {
            return CircleCircle(circleA.center, circleA.radius, circleB.center, circleB.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the circles
        /// </summary>
        /// <returns>
        /// Positive value if the circles do not intersect, negative otherwise.
        /// Negative value can be interpreted as depth of penetration.
        /// </returns>
        public static float CircleCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            return Vector2.Distance(centerA, centerB) - radiusA - radiusB;
        }

        #endregion Circle-Circle
    }
}