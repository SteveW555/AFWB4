using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Representation of a 2D circle
    /// </summary>
    [Serializable]
    public struct Circle2TCT : IEquatable<Circle2TCT>, IFormattable
    {
        public Vector2 center;
        public float radius;

        /// <summary>
        /// Returns the perimeter of the circle
        /// </summary>
        public float perimeter => 2 * Mathf.PI * radius;

        /// <summary>
        /// Returns the area of the circle
        /// </summary>
        public float area => Mathf.PI * radius * radius;

        public static Circle2TCT unit => new Circle2TCT(Vector2.zero, 1);

        public Circle2TCT(float radius) : this(Vector2.zero, radius)
        {
        }

        public Circle2TCT(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        /// <summary>
        /// Returns a point on the circle at the given <paramref name="angle"/>
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        public Vector2 GetPoint(float angle)
        {
            return GeometryTCT.PointOnCircle2TCT(center, radius, angle);
        }

        /// <summary>
        /// Returns a list of evenly distributed points on the circle
        /// </summary>
        /// <param name="count">Number of points</param>
        public List<Vector2> GetPoints(int count)
        {
            return GeometryTCT.PointsOnCircle2TCT(center, radius, count);
        }

        /// <summary>
        /// Returns true if the point intersects the circle
        /// </summary>
        public bool Contains(Vector2 point)
        {
            return IntersectTCT.PointCircle(point, center, radius);
        }

        /// <summary>
        /// Linearly interpolates between two circles
        /// </summary>
        public static Circle2TCT Lerp(Circle2TCT a, Circle2TCT b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Circle2TCT(a.center + (b.center - a.center) * t, a.radius + (b.radius - a.radius) * t);
        }

        /// <summary>
        /// Linearly interpolates between two circles without clamping the interpolant
        /// </summary>
        public static Circle2TCT LerpUnclamped(Circle2TCT a, Circle2TCT b, float t)
        {
            return new Circle2TCT(a.center + (b.center - a.center) * t, a.radius + (b.radius - a.radius) * t);
        }

        public static explicit operator SphereTCT(Circle2TCT circle)
        {
            return new SphereTCT((Vector3)circle.center, circle.radius);
        }

        public static explicit operator Circle3TCT(Circle2TCT circle)
        {
            return new Circle3TCT((Vector3)circle.center, Vector3.back, circle.radius);
        }

        public static Circle2TCT operator +(Circle2TCT circle, Vector2 vector)
        {
            return new Circle2TCT(circle.center + vector, circle.radius);
        }

        public static Circle2TCT operator -(Circle2TCT circle, Vector2 vector)
        {
            return new Circle2TCT(circle.center - vector, circle.radius);
        }

        public static bool operator ==(Circle2TCT a, Circle2TCT b)
        {
            return a.center == b.center && a.radius == b.radius;
        }

        public static bool operator !=(Circle2TCT a, Circle2TCT b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return center.GetHashCode() ^ (radius.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            return other is Circle2TCT && Equals((Circle2TCT)other);
        }

        public bool Equals(Circle2TCT other)
        {
            return center.Equals(other.center) && radius.Equals(other.radius);
        }

        public override string ToString()
        {
            return string.Format("Circle2TCT(center: {0}, radius: {1})", center, radius);
        }

        public string ToString(string format)
        {
            return string.Format("Circle2TCT(center: {0}, radius: {1})", center.ToString(format), radius.ToString(format));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("Circle2TCT(center: {0}, radius: {1})", center.ToString(format, formatProvider),
                radius.ToString(format, formatProvider));
        }
    }
}