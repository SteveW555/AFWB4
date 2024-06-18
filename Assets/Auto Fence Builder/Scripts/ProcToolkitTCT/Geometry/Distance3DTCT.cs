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
        public static float PointLine(Vector3 point, Line3TCT line)
        {
            return Vector3.Distance(point, ClosestTCT.PointLine(point, line));
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the line
        /// </summary>
        public static float PointLine(Vector3 point, Vector3 lineOrigin, Vector3 lineDirection)
        {
            return Vector3.Distance(point, ClosestTCT.PointLine(point, lineOrigin, lineDirection));
        }

        #endregion Point-Line

        #region Point-Ray

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the ray
        /// </summary>
        public static float PointRay(Vector3 point, Ray ray)
        {
            return Vector3.Distance(point, ClosestTCT.PointRay(point, ray));
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the ray
        /// </summary>
        public static float PointRay(Vector3 point, Vector3 rayOrigin, Vector3 rayDirection)
        {
            return Vector3.Distance(point, ClosestTCT.PointRay(point, rayOrigin, rayDirection));
        }

        #endregion Point-Ray

        #region Point-Segment

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the segment
        /// </summary>
        public static float PointSegment(Vector3 point, Segment3TCT segment)
        {
            return Vector3.Distance(point, ClosestTCT.PointSegment(point, segment));
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the segment
        /// </summary>
        public static float PointSegment(Vector3 point, Vector3 segmentA, Vector3 segmentB)
        {
            return Vector3.Distance(point, ClosestTCT.PointSegment(point, segmentA, segmentB));
        }

        #endregion Point-Segment

        #region Point-Sphere

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the sphere
        /// </summary>
        /// <returns>Positive value if the point is outside, negative otherwise</returns>
        public static float PointSphere(Vector3 point, SphereTCT sphere)
        {
            return PointSphere(point, sphere.center, sphere.radius);
        }

        /// <summary>
        /// Returns a distance to the ClosestTCT point on the sphere
        /// </summary>
        /// <returns>Positive value if the point is outside, negative otherwise</returns>
        public static float PointSphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
        {
            return (sphereCenter - point).magnitude - sphereRadius;
        }

        #endregion Point-Sphere

        #region Line-Sphere

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the sphere
        /// </summary>
        public static float LineSphere(Line3TCT line, SphereTCT sphere)
        {
            return LineSphere(line.origin, line.direction, sphere.center, sphere.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the line and the sphere
        /// </summary>
        public static float LineSphere(Vector3 lineOrigin, Vector3 lineDirection, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 originToCenter = sphereCenter - lineOrigin;
            float centerProjection = Vector3.Dot(lineDirection, originToCenter);
            float sqrDistanceToLine = originToCenter.sqrMagnitude - centerProjection * centerProjection;
            float sqrDistanceToIntersection = sphereRadius * sphereRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                // No intersection
                return Mathf.Sqrt(sqrDistanceToLine) - sphereRadius;
            }
            return 0;
        }

        #endregion Line-Sphere

        #region Ray-Sphere

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the ray and the sphere
        /// </summary>
        public static float RaySphere(Ray ray, SphereTCT sphere)
        {
            return RaySphere(ray.origin, ray.direction, sphere.center, sphere.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the ray and the sphere
        /// </summary>
        public static float RaySphere(Vector3 rayOrigin, Vector3 rayDirection, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 originToCenter = sphereCenter - rayOrigin;
            float centerProjection = Vector3.Dot(rayDirection, originToCenter);
            if (centerProjection + sphereRadius < -GeometryTCT.Epsilon)
            {
                // No intersection
                return Mathf.Sqrt(originToCenter.sqrMagnitude) - sphereRadius;
            }

            float sqrDistanceToOrigin = originToCenter.sqrMagnitude;
            float sqrDistanceToLine = sqrDistanceToOrigin - centerProjection * centerProjection;
            float sqrDistanceToIntersection = sphereRadius * sphereRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                // No intersection
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    return Mathf.Sqrt(sqrDistanceToOrigin) - sphereRadius;
                }
                return Mathf.Sqrt(sqrDistanceToLine) - sphereRadius;
            }
            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    // No intersection
                    return Mathf.Sqrt(sqrDistanceToOrigin) - sphereRadius;
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
                    return Mathf.Sqrt(sqrDistanceToOrigin) - sphereRadius;
                }

                // Point intersection;
                return 0;
            }

            // Two points intersection;
            return 0;
        }

        #endregion Ray-Sphere

        #region Segment-Sphere

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the segment and the sphere
        /// </summary>
        public static float SegmentSphere(Segment3TCT segment, SphereTCT sphere)
        {
            return SegmentSphere(segment.a, segment.b, sphere.center, sphere.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the segment and the sphere
        /// </summary>
        public static float SegmentSphere(Vector3 segmentA, Vector3 segmentB, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 segmentAToCenter = sphereCenter - segmentA;
            Vector3 fromAtoB = segmentB - segmentA;
            float segmentLength = fromAtoB.magnitude;
            if (segmentLength < GeometryTCT.Epsilon)
            {
                return segmentAToCenter.magnitude - sphereRadius;
            }

            Vector3 segmentDirection = fromAtoB.normalized;
            float centerProjection = Vector3.Dot(segmentDirection, segmentAToCenter);
            if (centerProjection + sphereRadius < -GeometryTCT.Epsilon ||
                centerProjection - sphereRadius > segmentLength + GeometryTCT.Epsilon)
            {
                // No intersection
                if (centerProjection < 0)
                {
                    return segmentAToCenter.magnitude - sphereRadius;
                }
                return (sphereCenter - segmentB).magnitude - sphereRadius;
            }

            float sqrDistanceToA = segmentAToCenter.sqrMagnitude;
            float sqrDistanceToLine = sqrDistanceToA - centerProjection * centerProjection;
            float sqrDistanceToIntersection = sphereRadius * sphereRadius - sqrDistanceToLine;
            if (sqrDistanceToIntersection < -GeometryTCT.Epsilon)
            {
                // No intersection
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    return Mathf.Sqrt(sqrDistanceToA) - sphereRadius;
                }
                if (centerProjection > segmentLength + GeometryTCT.Epsilon)
                {
                    return (sphereCenter - segmentB).magnitude - sphereRadius;
                }
                return Mathf.Sqrt(sqrDistanceToLine) - sphereRadius;
            }

            if (sqrDistanceToIntersection < GeometryTCT.Epsilon)
            {
                if (centerProjection < -GeometryTCT.Epsilon)
                {
                    // No intersection
                    return Mathf.Sqrt(sqrDistanceToA) - sphereRadius;
                }
                if (centerProjection > segmentLength + GeometryTCT.Epsilon)
                {
                    // No intersection
                    return (sphereCenter - segmentB).magnitude - sphereRadius;
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
                return Mathf.Sqrt(sqrDistanceToA) - sphereRadius;
            }
            return (sphereCenter - segmentB).magnitude - sphereRadius;
        }

        #endregion Segment-Sphere

        #region Sphere-Sphere

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the spheres
        /// </summary>
        /// <returns>
        /// Positive value if the spheres do not intersect, negative otherwise.
        /// Negative value can be interpreted as depth of penetration.
        /// </returns>
        public static float SphereSphere(SphereTCT sphereA, SphereTCT sphereB)
        {
            return SphereSphere(sphereA.center, sphereA.radius, sphereB.center, sphereB.radius);
        }

        /// <summary>
        /// Returns the distance between the ClosestTCT points on the spheres
        /// </summary>
        /// <returns>
        /// Positive value if the spheres do not intersect, negative otherwise.
        /// Negative value can be interpreted as depth of penetration.
        /// </returns>
        public static float SphereSphere(Vector3 centerA, float radiusA, Vector3 centerB, float radiusB)
        {
            return Vector3.Distance(centerA, centerB) - radiusA - radiusB;
        }

        #endregion Sphere-Sphere
    }
}