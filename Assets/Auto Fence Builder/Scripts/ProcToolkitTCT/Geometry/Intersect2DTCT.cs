using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Collection of intersection algorithms
    /// </summary>
    public static partial class IntersectTCT
    {
        #region Point-Line

        /// <summary>
        /// Tests if the point lies on the line
        /// </summary>
        public static bool PointLine(Vector2 point, Line2TCT line)
        {
            return PointLine(point, line.origin, line.direction);
        }

        /// <summary>
        /// Tests if the point lies on the line
        /// </summary>
        /// <param name="side">
        /// -1 if the point is to the left of the line,
        /// 0 if it is on the line,
        /// 1 if it is to the right of the line
        /// </param>
        public static bool PointLine(Vector2 point, Line2TCT line, out int side)
        {
            return PointLine(point, line.origin, line.direction, out side);
        }

        /// <summary>
        /// Tests if the point lies on the line
        /// </summary>
        public static bool PointLine(Vector2 point, Vector2 lineOrigin, Vector2 lineDirection)
        {
            float perpDot = VectorETCT.PerpDot(point - lineOrigin, lineDirection);
            return -GeometryTCT.Epsilon < perpDot && perpDot < GeometryTCT.Epsilon;
        }

        /// <summary>
        /// Tests if the point lies on the line
        /// </summary>
        /// <param name="side">
        /// -1 if the point is to the left of the line,
        /// 0 if it is on the line,
        /// 1 if it is to the right of the line
        /// </param>
        public static bool PointLine(Vector2 point, Vector2 lineOrigin, Vector2 lineDirection, out int side)
        {
            float perpDot = VectorETCT.PerpDot(point - lineOrigin, lineDirection);
            if (perpDot < -GeometryTCT.Epsilon)
            {
                side = -1;
                return false;
            }
            if (perpDot > GeometryTCT.Epsilon)
            {
                side = 1;
                return false;
            }
            side = 0;
            return true;
        }

        #endregion Point-Line

        #region Point-Ray

        /// <summary>
        /// Tests if the point lies on the ray
        /// </summary>
        public static bool PointRay(Vector2 point, Ray2D ray)
        {
            return PointRay(point, ray.origin, ray.direction);
        }

        /// <summary>
        /// Tests if the point lies on the ray
        /// </summary>
        /// <param name="side">
        /// -1 if the point is to the left of the ray,
        /// 0 if it is on the line,
        /// 1 if it is to the right of the ray
        /// </param>
        public static bool PointRay(Vector2 point, Ray2D ray, out int side)
        {
            return PointRay(point, ray.origin, ray.direction, out side);
        }

        /// <summary>
        /// Tests if the point lies on the ray
        /// </summary>
        public static bool PointRay(Vector2 point, Vector2 rayOrigin, Vector2 rayDirection)
        {
            Vector2 toPoint = point - rayOrigin;
            float perpDot = VectorETCT.PerpDot(toPoint, rayDirection);
            return -GeometryTCT.Epsilon < perpDot && perpDot < GeometryTCT.Epsilon &&
                   Vector2.Dot(rayDirection, toPoint) > -GeometryTCT.Epsilon;
        }

        /// <summary>
        /// Tests if the point lies on the ray
        /// </summary>
        /// <param name="side">
        /// -1 if the point is to the left of the ray,
        /// 0 if it is on the line,
        /// 1 if it is to the right of the ray
        /// </param>
        public static bool PointRay(Vector2 point, Vector2 rayOrigin, Vector2 rayDirection, out int side)
        {
            Vector2 toPoint = point - rayOrigin;
            float perpDot = VectorETCT.PerpDot(toPoint, rayDirection);
            if (perpDot < -GeometryTCT.Epsilon)
            {
                side = -1;
                return false;
            }
            if (perpDot > GeometryTCT.Epsilon)
            {
                side = 1;
                return false;
            }
            side = 0;
            return Vector2.Dot(rayDirection, toPoint) > -GeometryTCT.Epsilon;
        }

        #endregion Point-Ray

        #region Point-Segment

        /// <summary>
        /// Tests if the point lies on the segment
        /// </summary>
        public static bool PointSegment(Vector2 point, Segment2TCT segment)
        {
            return PointSegment(point, segment.a, segment.b);
        }

        /// <summary>
        /// Tests if the point lies on the segment
        /// </summary>
        /// <param name="side">
        /// -1 if the point is to the left of the segment,
        /// 0 if it is on the line,
        /// 1 if it is to the right of the segment
        /// </param>
        public static bool PointSegment(Vector2 point, Segment2TCT segment, out int side)
        {
            return PointSegment(point, segment.a, segment.b, out side);
        }

        /// <summary>
        /// Tests if the point lies on the segment
        /// </summary>
        public static bool PointSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB)
        {
            Vector2 fromAToB = segmentB - segmentA;
            float sqrSegmentLength = fromAToB.sqrMagnitude;
            if (sqrSegmentLength < GeometryTCT.Epsilon)
            {
                // The segment is a point
                return point == segmentA;
            }
            // Normalized direction gives more stable results
            Vector2 segmentDirection = fromAToB.normalized;
            Vector2 toPoint = point - segmentA;
            float perpDot = VectorETCT.PerpDot(toPoint, segmentDirection);
            if (-GeometryTCT.Epsilon < perpDot && perpDot < GeometryTCT.Epsilon)
            {
                float pointProjection = Vector2.Dot(segmentDirection, toPoint);
                return pointProjection > -GeometryTCT.Epsilon &&
                       pointProjection < Mathf.Sqrt(sqrSegmentLength) + GeometryTCT.Epsilon;
            }
            return false;
        }

        /// <summary>
        /// Tests if the point lies on the segment
        /// </summary>
        /// <param name="side">
        /// -1 if the point is to the left of the segment,
        /// 0 if it is on the line,
        /// 1 if it is to the right of the segment
        /// </param>
        public static bool PointSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB, out int side)
        {
            Vector2 fromAToB = segmentB - segmentA;
            float sqrSegmentLength = fromAToB.sqrMagnitude;
            if (sqrSegmentLength < GeometryTCT.Epsilon)
            {
                // The segment is a point
                side = 0;
                return point == segmentA;
            }
            // Normalized direction gives more stable results
            Vector2 segmentDirection = fromAToB.normalized;
            Vector2 toPoint = point - segmentA;
            float perpDot = VectorETCT.PerpDot(toPoint, segmentDirection);
            if (perpDot < -GeometryTCT.Epsilon)
            {
                side = -1;
                return false;
            }
            if (perpDot > GeometryTCT.Epsilon)
            {
                side = 1;
                return false;
            }
            side = 0;
            float pointProjection = Vector2.Dot(segmentDirection, toPoint);
            return pointProjection > -GeometryTCT.Epsilon &&
                   pointProjection < Mathf.Sqrt(sqrSegmentLength) + GeometryTCT.Epsilon;
        }

        private static bool PointSegment(Vector2 point, Vector2 segmentA, Vector2 segmentDirection, float sqrSegmentLength)
        {
            float segmentLength = Mathf.Sqrt(sqrSegmentLength);
            segmentDirection /= segmentLength;
            Vector2 toPoint = point - segmentA;
            float perpDot = VectorETCT.PerpDot(toPoint, segmentDirection);
            if (-GeometryTCT.Epsilon < perpDot && perpDot < GeometryTCT.Epsilon)
            {
                float pointProjection = Vector2.Dot(segmentDirection, toPoint);
                return pointProjection > -GeometryTCT.Epsilon &&
                       pointProjection < segmentLength + GeometryTCT.Epsilon;
            }
            return false;
        }

        public static bool PointSegmentCollinear(Vector2 segmentA, Vector2 segmentB, Vector2 point)
        {
            if (Mathf.Abs(segmentA.x - segmentB.x) < GeometryTCT.Epsilon)
            {
                // Vertical
                if (segmentA.y <= point.y && point.y <= segmentB.y)
                {
                    return true;
                }
                if (segmentA.y >= point.y && point.y >= segmentB.y)
                {
                    return true;
                }
            }
            else
            {
                // Not vertical
                if (segmentA.x <= point.x && point.x <= segmentB.x)
                {
                    return true;
                }
                if (segmentA.x >= point.x && point.x >= segmentB.x)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion Point-Segment

        #region Point-Circle

        /// <summary>
        /// Tests if the point is inside the circle
        /// </summary>
        public static bool PointCircle(Vector2 point, Circle2TCT circle)
        {
            return PointCircle(point, circle.center, circle.radius);
        }

        /// <summary>
        /// Tests if the point is inside the circle
        /// </summary>
        public static bool PointCircle(Vector2 point, Vector2 circleCenter, float circleRadius)
        {
            // For points on the circle's edge magnitude is more stable than sqrMagnitude
            return (point - circleCenter).magnitude < circleRadius + GeometryTCT.Epsilon;
        }

        #endregion Point-Circle

        #region Line-Line

        /// <summary>
        /// Computes an intersection of the lines
        /// </summary>
        public static bool LineLine(Line2TCT lineA, Line2TCT lineB)
        {
            return LineLine(lineA.origin, lineA.direction, lineB.origin, lineB.direction, out IntersectionLineLine2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the lines
        /// </summary>
        public static bool LineLine(Line2TCT lineA, Line2TCT lineB, out IntersectionLineLine2TCT intersection)
        {
            return LineLine(lineA.origin, lineA.direction, lineB.origin, lineB.direction, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the lines
        /// </summary>
        public static bool LineLine(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            return LineLine(originA, directionA, originB, directionB, out IntersectionLineLine2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the lines
        /// </summary>
        public static bool LineLine(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB,
            out IntersectionLineLine2TCT intersection)
        {
            Vector2 originBToA = originA - originB;
            float denominator = VectorETCT.PerpDot(directionA, directionB);
            float perpDotB = VectorETCT.PerpDot(directionB, originBToA);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                float perpDotA = VectorETCT.PerpDot(directionA, originBToA);
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    intersection = IntersectionLineLine2TCT.None();
                    return false;
                }
                // Collinear
                intersection = IntersectionLineLine2TCT.Line(originA);
                return true;
            }

            // Not parallel
            intersection = IntersectionLineLine2TCT.Point(originA + directionA * (perpDotB / denominator));
            return true;
        }

        #endregion Line-Line

        #region Line-Ray

        /// <summary>
        /// Computes an intersection of the line and the ray
        /// </summary>
        public static bool LineRay(Line2TCT line, Ray2D ray)
        {
            return LineRay(line.origin, line.direction, ray.origin, ray.direction, out IntersectionLineRay2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the ray
        /// </summary>
        public static bool LineRay(Line2TCT line, Ray2D ray, out IntersectionLineRay2TCT intersection)
        {
            return LineRay(line.origin, line.direction, ray.origin, ray.direction, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the ray
        /// </summary>
        public static bool LineRay(Vector2 lineOrigin, Vector2 lineDirection, Vector2 rayOrigin, Vector2 rayDirection)
        {
            return LineRay(lineOrigin, lineDirection, rayOrigin, rayDirection, out IntersectionLineRay2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the ray
        /// </summary>
        public static bool LineRay(Vector2 lineOrigin, Vector2 lineDirection, Vector2 rayOrigin, Vector2 rayDirection,
            out IntersectionLineRay2TCT intersection)
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
                    intersection = IntersectionLineRay2TCT.None();
                    return false;
                }
                // Collinear
                intersection = IntersectionLineRay2TCT.Ray(rayOrigin);
                return true;
            }

            // Not parallel
            float rayDistance = perpDotA / denominator;
            if (rayDistance > -GeometryTCT.Epsilon)
            {
                intersection = IntersectionLineRay2TCT.Point(rayOrigin + rayDirection * rayDistance);
                return true;
            }
            intersection = IntersectionLineRay2TCT.None();
            return false;
        }

        #endregion Line-Ray

        #region Line-Segment

        /// <summary>
        /// Computes an intersection of the line and the segment
        /// </summary>
        public static bool LineSegment(Line2TCT line, Segment2TCT segment)
        {
            return LineSegment(line.origin, line.direction, segment.a, segment.b, out IntersectionLineSegment2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the segment
        /// </summary>
        public static bool LineSegment(Line2TCT line, Segment2TCT segment, out IntersectionLineSegment2TCT intersection)
        {
            return LineSegment(line.origin, line.direction, segment.a, segment.b, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the segment
        /// </summary>
        public static bool LineSegment(Vector2 lineOrigin, Vector2 lineDirection, Vector2 segmentA, Vector2 segmentB)
        {
            return LineSegment(lineOrigin, lineDirection, segmentA, segmentB, out IntersectionLineSegment2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the segment
        /// </summary>
        public static bool LineSegment(Vector2 lineOrigin, Vector2 lineDirection, Vector2 segmentA, Vector2 segmentB,
            out IntersectionLineSegment2TCT intersection)
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
                    intersection = IntersectionLineSegment2TCT.None();
                    return false;
                }
                // Collinear
                bool segmentIsAPoint = segmentDirection.sqrMagnitude < GeometryTCT.Epsilon;
                if (segmentIsAPoint)
                {
                    intersection = IntersectionLineSegment2TCT.Point(segmentA);
                    return true;
                }

                bool codirected = Vector2.Dot(lineDirection, segmentDirection) > 0;
                if (codirected)
                {
                    intersection = IntersectionLineSegment2TCT.Segment(segmentA, segmentB);
                }
                else
                {
                    intersection = IntersectionLineSegment2TCT.Segment(segmentB, segmentA);
                }
                return true;
            }

            // Not parallel
            float segmentDistance = perpDotA / denominator;
            if (segmentDistance > -GeometryTCT.Epsilon && segmentDistance < 1 + GeometryTCT.Epsilon)
            {
                intersection = IntersectionLineSegment2TCT.Point(segmentA + segmentDirection * segmentDistance);
                return true;
            }
            intersection = IntersectionLineSegment2TCT.None();
            return false;
        }

        #endregion Line-Segment

        #region Line-Circle

        /// <summary>
        /// Computes an intersection of the line and the circle
        /// </summary>
        public static bool LineCircle(Line2TCT line, Circle2TCT circle)
        {
            return LineCircle(line.origin, line.direction, circle.center, circle.radius, out IntersectionLineCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the circle
        /// </summary>
        public static bool LineCircle(Line2TCT line, Circle2TCT circle, out IntersectionLineCircleTCT intersection)
        {
            return LineCircle(line.origin, line.direction, circle.center, circle.radius, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the circle
        /// </summary>
        public static bool LineCircle(Vector2 lineOrigin, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
        {
            return LineCircle(lineOrigin, lineDirection, circleCenter, circleRadius, out IntersectionLineCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the line and the circle
        /// </summary>
        public static bool LineCircle(Vector2 lineOrigin, Vector2 lineDirection, Vector2 circleCenter, float circleRadius,
            out IntersectionLineCircleTCT intersection)
        {
            Vector2 originToCenter = circleCenter - lineOrigin;
            float centerProjection = Vector2.Dot(lineDirection, originToCenter);
            float sqrDistanceToLine = originToCenter.sqrMagnitude - centerProjection * centerProjection;

            float sqrDistanceToIntersection = circleRadius * circleRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                intersection = IntersectionLineCircleTCT.None();
                return false;
            }
            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                intersection = IntersectionLineCircleTCT.Point(lineOrigin + lineDirection * centerProjection);
                return true;
            }

            float distanceToIntersection = Mathf.Sqrt(sqrDistanceToIntersection);
            float distanceA = centerProjection - distanceToIntersection;
            float distanceB = centerProjection + distanceToIntersection;

            Vector2 pointA = lineOrigin + lineDirection * distanceA;
            Vector2 pointB = lineOrigin + lineDirection * distanceB;
            intersection = IntersectionLineCircleTCT.TwoPoints(pointA, pointB);
            return true;
        }

        #endregion Line-Circle

        #region Ray-Ray

        /// <summary>
        /// Computes an intersection of the rays
        /// </summary>
        public static bool RayRay(Ray2D rayA, Ray2D rayB)
        {
            return RayRay(rayA.origin, rayA.direction, rayB.origin, rayB.direction, out IntersectionRayRay2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the rays
        /// </summary>
        public static bool RayRay(Ray2D rayA, Ray2D rayB, out IntersectionRayRay2TCT intersection)
        {
            return RayRay(rayA.origin, rayA.direction, rayB.origin, rayB.direction, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the rays
        /// </summary>
        public static bool RayRay(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            return RayRay(originA, directionA, originB, directionB, out IntersectionRayRay2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the rays
        /// </summary>
        public static bool RayRay(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB,
            out IntersectionRayRay2TCT intersection)
        {
            Vector2 originBToA = originA - originB;
            float denominator = VectorETCT.PerpDot(directionA, directionB);
            float perpDotA = VectorETCT.PerpDot(directionA, originBToA);
            float perpDotB = VectorETCT.PerpDot(directionB, originBToA);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    intersection = IntersectionRayRay2TCT.None();
                    return false;
                }
                // Collinear

                bool codirected = Vector2.Dot(directionA, directionB) > 0;
                float originBProjection = -Vector2.Dot(directionA, originBToA);
                if (codirected)
                {
                    intersection = IntersectionRayRay2TCT.Ray(originBProjection > 0 ? originB : originA, directionA);
                    return true;
                }
                else
                {
                    if (originBProjection < -GeometryTCT.Epsilon)
                    {
                        intersection = IntersectionRayRay2TCT.None();
                        return false;
                    }
                    if (originBProjection < GeometryTCT.Epsilon)
                    {
                        intersection = IntersectionRayRay2TCT.Point(originA);
                        return true;
                    }
                    intersection = IntersectionRayRay2TCT.Segment(originA, originB);
                    return true;
                }
            }

            // Not parallel
            float distanceA = perpDotB / denominator;
            if (distanceA < -GeometryTCT.Epsilon)
            {
                intersection = IntersectionRayRay2TCT.None();
                return false;
            }

            float distanceB = perpDotA / denominator;
            if (distanceB < -GeometryTCT.Epsilon)
            {
                intersection = IntersectionRayRay2TCT.None();
                return false;
            }

            intersection = IntersectionRayRay2TCT.Point(originA + directionA * distanceA);
            return true;
        }

        #endregion Ray-Ray

        #region Ray-Segment

        /// <summary>
        /// Computes an intersection of the ray and the segment
        /// </summary>
        public static bool RaySegment(Ray2D ray, Segment2TCT segment)
        {
            return RaySegment(ray.origin, ray.direction, segment.a, segment.b, out IntersectionRaySegment2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the ray and the segment
        /// </summary>
        public static bool RaySegment(Ray2D ray, Segment2TCT segment, out IntersectionRaySegment2TCT intersection)
        {
            return RaySegment(ray.origin, ray.direction, segment.a, segment.b, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the ray and the segment
        /// </summary>
        public static bool RaySegment(Vector2 rayOrigin, Vector2 rayDirection, Vector2 segmentA, Vector2 segmentB)
        {
            return RaySegment(rayOrigin, rayDirection, segmentA, segmentB, out IntersectionRaySegment2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the ray and the segment
        /// </summary>
        public static bool RaySegment(Vector2 rayOrigin, Vector2 rayDirection, Vector2 segmentA, Vector2 segmentB,
            out IntersectionRaySegment2TCT intersection)
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
                if (Mathf.Abs(perpDotA) > GeometryTCT.Epsilon || Mathf.Abs(perpDotB) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    intersection = IntersectionRaySegment2TCT.None();
                    return false;
                }
                // Collinear

                bool segmentIsAPoint = segmentDirection.sqrMagnitude < GeometryTCT.Epsilon;
                float segmentAProjection = Vector2.Dot(rayDirection, segmentA - rayOrigin);
                if (segmentIsAPoint)
                {
                    if (segmentAProjection > -GeometryTCT.Epsilon)
                    {
                        intersection = IntersectionRaySegment2TCT.Point(segmentA);
                        return true;
                    }
                    intersection = IntersectionRaySegment2TCT.None();
                    return false;
                }

                float segmentBProjection = Vector2.Dot(rayDirection, segmentB - rayOrigin);
                if (segmentAProjection > -GeometryTCT.Epsilon)
                {
                    if (segmentBProjection > -GeometryTCT.Epsilon)
                    {
                        if (segmentBProjection > segmentAProjection)
                        {
                            intersection = IntersectionRaySegment2TCT.Segment(segmentA, segmentB);
                        }
                        else
                        {
                            intersection = IntersectionRaySegment2TCT.Segment(segmentB, segmentA);
                        }
                    }
                    else
                    {
                        if (segmentAProjection > GeometryTCT.Epsilon)
                        {
                            intersection = IntersectionRaySegment2TCT.Segment(rayOrigin, segmentA);
                        }
                        else
                        {
                            intersection = IntersectionRaySegment2TCT.Point(rayOrigin);
                        }
                    }
                    return true;
                }
                if (segmentBProjection > -GeometryTCT.Epsilon)
                {
                    if (segmentBProjection > GeometryTCT.Epsilon)
                    {
                        intersection = IntersectionRaySegment2TCT.Segment(rayOrigin, segmentB);
                    }
                    else
                    {
                        intersection = IntersectionRaySegment2TCT.Point(rayOrigin);
                    }
                    return true;
                }
                intersection = IntersectionRaySegment2TCT.None();
                return false;
            }

            // Not parallel
            float rayDistance = perpDotB / denominator;
            float segmentDistance = perpDotA / denominator;
            if (rayDistance > -GeometryTCT.Epsilon &&
                segmentDistance > -GeometryTCT.Epsilon && segmentDistance < 1 + GeometryTCT.Epsilon)
            {
                intersection = IntersectionRaySegment2TCT.Point(segmentA + segmentDirection * segmentDistance);
                return true;
            }
            intersection = IntersectionRaySegment2TCT.None();
            return false;
        }

        #endregion Ray-Segment

        #region Ray-Circle

        /// <summary>
        /// Computes an intersection of the ray and the circle
        /// </summary>
        public static bool RayCircle(Ray2D ray, Circle2TCT circle)
        {
            return RayCircle(ray.origin, ray.direction, circle.center, circle.radius, out IntersectionRayCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the ray and the circle
        /// </summary>
        public static bool RayCircle(Ray2D ray, Circle2TCT circle, out IntersectionRayCircleTCT intersection)
        {
            return RayCircle(ray.origin, ray.direction, circle.center, circle.radius, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the ray and the circle
        /// </summary>
        public static bool RayCircle(Vector2 rayOrigin, Vector2 rayDirection, Vector2 circleCenter, float circleRadius)
        {
            return RayCircle(rayOrigin, rayDirection, circleCenter, circleRadius, out IntersectionRayCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the ray and the circle
        /// </summary>
        public static bool RayCircle(Vector2 rayOrigin, Vector2 rayDirection, Vector2 circleCenter, float circleRadius,
            out IntersectionRayCircleTCT intersection)
        {
            Vector2 originToCenter = circleCenter - rayOrigin;
            float centerProjection = Vector2.Dot(rayDirection, originToCenter);
            if (centerProjection + circleRadius < -GeometryTCT.Epsilon)
            {
                intersection = IntersectionRayCircleTCT.None();
                return false;
            }

            float sqrDistanceToLine = originToCenter.sqrMagnitude - centerProjection * centerProjection;
            float sqrDistanceToIntersection = circleRadius * circleRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                intersection = IntersectionRayCircleTCT.None();
                return false;
            }
            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    intersection = IntersectionRayCircleTCT.None();
                    return false;
                }
                intersection = IntersectionRayCircleTCT.Point(rayOrigin + rayDirection * centerProjection);
                return true;
            }

            // Line intersection
            float distanceToIntersection = Mathf.Sqrt(sqrDistanceToIntersection);
            float distanceA = centerProjection - distanceToIntersection;
            float distanceB = centerProjection + distanceToIntersection;

            if (distanceA < -GeometryTCT.Epsilon)
            {
                if (distanceB < -GeometryTCT.Epsilon)
                {
                    intersection = IntersectionRayCircleTCT.None();
                    return false;
                }
                intersection = IntersectionRayCircleTCT.Point(rayOrigin + rayDirection * distanceB);
                return true;
            }

            Vector2 pointA = rayOrigin + rayDirection * distanceA;
            Vector2 pointB = rayOrigin + rayDirection * distanceB;
            intersection = IntersectionRayCircleTCT.TwoPoints(pointA, pointB);
            return true;
        }

        #endregion Ray-Circle

        #region Segment-Segment

        /// <summary>
        /// Computes an intersection of the segments
        /// </summary>
        public static bool SegmentSegment(Segment2TCT segment1, Segment2TCT Segment2TCT)
        {
            return SegmentSegment(segment1.a, segment1.b, Segment2TCT.a, Segment2TCT.b, out IntersectionSegmentSegment2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the segments
        /// </summary>
        public static bool SegmentSegment(Segment2TCT segment1, Segment2TCT Segment2TCT, out IntersectionSegmentSegment2TCT intersection)
        {
            return SegmentSegment(segment1.a, segment1.b, Segment2TCT.a, Segment2TCT.b, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the segments
        /// </summary>
        public static bool SegmentSegment(Vector2 segment1A, Vector2 segment1B, Vector2 Segment2TCTA, Vector2 Segment2TCTB)
        {
            return SegmentSegment(segment1A, segment1B, Segment2TCTA, Segment2TCTB, out IntersectionSegmentSegment2TCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the segments
        /// </summary>
        public static bool SegmentSegment(Vector2 segment1A, Vector2 segment1B, Vector2 Segment2TCTA, Vector2 Segment2TCTB,
            out IntersectionSegmentSegment2TCT intersection)
        {
            Vector2 from2ATo1A = segment1A - Segment2TCTA;
            Vector2 direction1 = segment1B - segment1A;
            Vector2 direction2 = Segment2TCTB - Segment2TCTA;

            float sqrSegment1Length = direction1.sqrMagnitude;
            float sqrSegment2TCTLength = direction2.sqrMagnitude;
            bool segment1IsAPoint = sqrSegment1Length < GeometryTCT.Epsilon;
            bool Segment2TCTIsAPoint = sqrSegment2TCTLength < GeometryTCT.Epsilon;
            if (segment1IsAPoint && Segment2TCTIsAPoint)
            {
                if (segment1A == Segment2TCTA)
                {
                    intersection = IntersectionSegmentSegment2TCT.Point(segment1A);
                    return true;
                }
                intersection = IntersectionSegmentSegment2TCT.None();
                return false;
            }
            if (segment1IsAPoint)
            {
                if (PointSegment(segment1A, Segment2TCTA, direction2, sqrSegment2TCTLength))
                {
                    intersection = IntersectionSegmentSegment2TCT.Point(segment1A);
                    return true;
                }
                intersection = IntersectionSegmentSegment2TCT.None();
                return false;
            }
            if (Segment2TCTIsAPoint)
            {
                if (PointSegment(Segment2TCTA, segment1A, direction1, sqrSegment1Length))
                {
                    intersection = IntersectionSegmentSegment2TCT.Point(Segment2TCTA);
                    return true;
                }
                intersection = IntersectionSegmentSegment2TCT.None();
                return false;
            }

            float denominator = VectorETCT.PerpDot(direction1, direction2);
            float perpDot1 = VectorETCT.PerpDot(direction1, from2ATo1A);
            float perpDot2 = VectorETCT.PerpDot(direction2, from2ATo1A);

            if (Mathf.Abs(denominator) < GeometryTCT.Epsilon)
            {
                // Parallel
                if (Mathf.Abs(perpDot1) > GeometryTCT.Epsilon || Mathf.Abs(perpDot2) > GeometryTCT.Epsilon)
                {
                    // Not collinear
                    intersection = IntersectionSegmentSegment2TCT.None();
                    return false;
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
                        return SegmentSegmentCollinear(segment1A, segment1B, sqrSegment1Length, Segment2TCTA, Segment2TCTB, out intersection);
                    }
                    else
                    {
                        //     1A------1B
                        // 2A------2B
                        return SegmentSegmentCollinear(Segment2TCTA, Segment2TCTB, sqrSegment2TCTLength, segment1A, segment1B, out intersection);
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
                        return SegmentSegmentCollinear(segment1A, segment1B, sqrSegment1Length, Segment2TCTB, Segment2TCTA, out intersection);
                    }
                    else
                    {
                        //     1A------1B
                        // 2B------2A
                        return SegmentSegmentCollinear(Segment2TCTB, Segment2TCTA, sqrSegment2TCTLength, segment1A, segment1B, out intersection);
                    }
                }
            }

            // Not parallel
            float distance1 = perpDot2 / denominator;
            if (distance1 < -GeometryTCT.Epsilon || distance1 > 1 + GeometryTCT.Epsilon)
            {
                intersection = IntersectionSegmentSegment2TCT.None();
                return false;
            }

            float distance2 = perpDot1 / denominator;
            if (distance2 < -GeometryTCT.Epsilon || distance2 > 1 + GeometryTCT.Epsilon)
            {
                intersection = IntersectionSegmentSegment2TCT.None();
                return false;
            }

            intersection = IntersectionSegmentSegment2TCT.Point(segment1A + direction1 * distance1);
            return true;
        }

        private static bool SegmentSegmentCollinear(Vector2 leftA, Vector2 leftB, float sqrLeftLength, Vector2 rightA, Vector2 rightB,
            out IntersectionSegmentSegment2TCT intersection)
        {
            Vector2 leftDirection = leftB - leftA;
            float rightAProjection = Vector2.Dot(leftDirection, rightA - leftB);
            if (Mathf.Abs(rightAProjection) < GeometryTCT.Epsilon)
            {
                // LB == RA
                // LA------LB
                //         RA------RB
                intersection = IntersectionSegmentSegment2TCT.Point(leftB);
                return true;
            }
            if (rightAProjection < 0)
            {
                // LB > RA
                // LA------LB
                //     RARB
                //     RA--RB
                //     RA------RB
                Vector2 pointB;
                float rightBProjection = Vector2.Dot(leftDirection, rightB - leftA);
                if (rightBProjection > sqrLeftLength)
                {
                    pointB = leftB;
                }
                else
                {
                    pointB = rightB;
                }
                intersection = IntersectionSegmentSegment2TCT.Segment(rightA, pointB);
                return true;
            }
            // LB < RA
            // LA------LB
            //             RA------RB
            intersection = IntersectionSegmentSegment2TCT.None();
            return false;
        }

        #endregion Segment-Segment

        #region Segment-Circle

        /// <summary>
        /// Computes an intersection of the segment and the circle
        /// </summary>
        public static bool SegmentCircle(Segment2TCT segment, Circle2TCT circle)
        {
            return SegmentCircle(segment.a, segment.b, circle.center, circle.radius, out IntersectionSegmentCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the segment and the circle
        /// </summary>
        public static bool SegmentCircle(Segment2TCT segment, Circle2TCT circle, out IntersectionSegmentCircleTCT intersection)
        {
            return SegmentCircle(segment.a, segment.b, circle.center, circle.radius, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the segment and the circle
        /// </summary>
        public static bool SegmentCircle(Vector2 segmentA, Vector2 segmentB, Vector2 circleCenter, float circleRadius)
        {
            return SegmentCircle(segmentA, segmentB, circleCenter, circleRadius, out IntersectionSegmentCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the segment and the circle
        /// </summary>
        public static bool SegmentCircle(Vector2 segmentA, Vector2 segmentB, Vector2 circleCenter, float circleRadius,
            out IntersectionSegmentCircleTCT intersection)
        {
            Vector2 segmentAToCenter = circleCenter - segmentA;
            Vector2 fromAtoB = segmentB - segmentA;
            float segmentLength = fromAtoB.magnitude;
            if (segmentLength < GeometryTCT.Epsilon)
            {
                float distanceToPoint = segmentAToCenter.magnitude;
                if (distanceToPoint < circleRadius + GeometryTCT.Epsilon)
                {
                    if (distanceToPoint > circleRadius - GeometryTCT.Epsilon)
                    {
                        intersection = IntersectionSegmentCircleTCT.Point(segmentA);
                        return true;
                    }
                    intersection = IntersectionSegmentCircleTCT.None();
                    return true;
                }
                intersection = IntersectionSegmentCircleTCT.None();
                return false;
            }

            Vector2 segmentDirection = fromAtoB.normalized;
            float centerProjection = Vector2.Dot(segmentDirection, segmentAToCenter);
            if (centerProjection + circleRadius < -GeometryTCT.Epsilon ||
                centerProjection - circleRadius > segmentLength + GeometryTCT.Epsilon)
            {
                intersection = IntersectionSegmentCircleTCT.None();
                return false;
            }

            float sqrDistanceToLine = segmentAToCenter.sqrMagnitude - centerProjection * centerProjection;
            float sqrDistanceToIntersection = circleRadius * circleRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                intersection = IntersectionSegmentCircleTCT.None();
                return false;
            }

            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                if (centerProjection < -GeometryTCT.Epsilon ||
                    centerProjection > segmentLength + GeometryTCT.Epsilon)
                {
                    intersection = IntersectionSegmentCircleTCT.None();
                    return false;
                }
                intersection = IntersectionSegmentCircleTCT.Point(segmentA + segmentDirection * centerProjection);
                return true;
            }

            // Line intersection
            float distanceToIntersection = Mathf.Sqrt(sqrDistanceToIntersection);
            float distanceA = centerProjection - distanceToIntersection;
            float distanceB = centerProjection + distanceToIntersection;

            bool pointAIsAfterSegmentA = distanceA > -GeometryTCT.Epsilon;
            bool pointBIsBeforeSegmentB = distanceB < segmentLength + GeometryTCT.Epsilon;

            if (pointAIsAfterSegmentA && pointBIsBeforeSegmentB)
            {
                Vector2 pointA = segmentA + segmentDirection * distanceA;
                Vector2 pointB = segmentA + segmentDirection * distanceB;
                intersection = IntersectionSegmentCircleTCT.TwoPoints(pointA, pointB);
                return true;
            }
            if (!pointAIsAfterSegmentA && !pointBIsBeforeSegmentB)
            {
                // The segment is inside, but no intersection
                intersection = IntersectionSegmentCircleTCT.None();
                return true;
            }

            bool pointAIsBeforeSegmentB = distanceA < segmentLength + GeometryTCT.Epsilon;
            if (pointAIsAfterSegmentA && pointAIsBeforeSegmentB)
            {
                // Point A intersection
                intersection = IntersectionSegmentCircleTCT.Point(segmentA + segmentDirection * distanceA);
                return true;
            }
            bool pointBIsAfterSegmentA = distanceB > -GeometryTCT.Epsilon;
            if (pointBIsAfterSegmentA && pointBIsBeforeSegmentB)
            {
                // Point B intersection
                intersection = IntersectionSegmentCircleTCT.Point(segmentA + segmentDirection * distanceB);
                return true;
            }

            intersection = IntersectionSegmentCircleTCT.None();
            return false;
        }

        #endregion Segment-Circle

        #region Circle-Circle

        /// <summary>
        /// Computes an intersection of the circles
        /// </summary>
        /// <returns>True if the circles intersect or one circle is contained within the other</returns>
        public static bool CircleCircle(Circle2TCT circleA, Circle2TCT circleB)
        {
            return CircleCircle(circleA.center, circleA.radius, circleB.center, circleB.radius, out IntersectionCircleCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the circles
        /// </summary>
        /// <returns>True if the circles intersect or one circle is contained within the other</returns>
        public static bool CircleCircle(Circle2TCT circleA, Circle2TCT circleB, out IntersectionCircleCircleTCT intersection)
        {
            return CircleCircle(circleA.center, circleA.radius, circleB.center, circleB.radius, out intersection);
        }

        /// <summary>
        /// Computes an intersection of the circles
        /// </summary>
        /// <returns>True if the circles intersect or one circle is contained within the other</returns>
        public static bool CircleCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            return CircleCircle(centerA, radiusA, centerB, radiusB, out IntersectionCircleCircleTCT intersection);
        }

        /// <summary>
        /// Computes an intersection of the circles
        /// </summary>
        /// <returns>True if the circles intersect or one circle is contained within the other</returns>
        public static bool CircleCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB,
            out IntersectionCircleCircleTCT intersection)
        {
            Vector2 fromBtoA = centerA - centerB;
            float distanceFromBtoASqr = fromBtoA.sqrMagnitude;
            if (distanceFromBtoASqr < GeometryTCT.Epsilon)
            {
                if (Mathf.Abs(radiusA - radiusB) < GeometryTCT.Epsilon)
                {
                    // Circles are coincident
                    intersection = IntersectionCircleCircleTCT.Circle();
                    return true;
                }
                // One circle is inside the other
                intersection = IntersectionCircleCircleTCT.None();
                return true;
            }

            // For intersections on the circle's edge magnitude is more stable than sqrMagnitude
            float distanceFromBtoA = Mathf.Sqrt(distanceFromBtoASqr);

            float sumOfRadii = radiusA + radiusB;
            if (Mathf.Abs(distanceFromBtoA - sumOfRadii) < GeometryTCT.Epsilon)
            {
                // One intersection outside
                intersection = IntersectionCircleCircleTCT.Point(centerB + fromBtoA * (radiusB / sumOfRadii));
                return true;
            }
            if (distanceFromBtoA > sumOfRadii)
            {
                // No intersections, circles are separate
                intersection = IntersectionCircleCircleTCT.None();
                return false;
            }

            float differenceOfRadii = radiusA - radiusB;
            float differenceOfRadiiAbs = Mathf.Abs(differenceOfRadii);
            if (Mathf.Abs(distanceFromBtoA - differenceOfRadiiAbs) < GeometryTCT.Epsilon)
            {
                // One intersection inside
                intersection = IntersectionCircleCircleTCT.Point(centerB - fromBtoA * (radiusB / differenceOfRadii));
                return true;
            }
            if (distanceFromBtoA < differenceOfRadiiAbs)
            {
                // One circle is contained within the other
                intersection = IntersectionCircleCircleTCT.None();
                return true;
            }

            // Two intersections
            float radiusASqr = radiusA * radiusA;
            float distanceToMiddle = 0.5f * (radiusASqr - radiusB * radiusB) / distanceFromBtoASqr + 0.5f;
            Vector2 middle = centerA - fromBtoA * distanceToMiddle;

            float discriminant = radiusASqr / distanceFromBtoASqr - distanceToMiddle * distanceToMiddle;
            Vector2 offset = fromBtoA.RotateCCW90() * Mathf.Sqrt(discriminant);

            intersection = IntersectionCircleCircleTCT.TwoPoints(middle + offset, middle - offset);
            return true;
        }

        #endregion Circle-Circle
    }
}