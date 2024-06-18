using System;
using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Representation of a 3D circle
    /// </summary>
    [Serializable]
    public struct Circle3TCT : IEquatable<Circle3TCT>, IFormattable
    {
        public Vector3 center;
        public Vector3 normal;
        public float radius;

        /// <summary>
        /// Returns the perimeter of the circle
        /// </summary>
        public float perimeter => 2 * Mathf.PI * radius;

        /// <summary>
        /// Returns the area of the circle
        /// </summary>
        public float area => Mathf.PI * radius * radius;

        public static Circle3TCT unitXY => new Circle3TCT(Vector3.zero, Vector3.back, 1);
        public static Circle3TCT unitXZ => new Circle3TCT(Vector3.zero, Vector3.up, 1);
        public static Circle3TCT unitYZ => new Circle3TCT(Vector3.zero, Vector3.left, 1);

        public Circle3TCT(float radius) : this(Vector3.zero, Vector3.back, radius)
        {
        }

        public Circle3TCT(Vector3 center, float radius) : this(center, Vector3.back, radius)
        {
        }

        public Circle3TCT(Vector3 center, Vector3 normal, float radius)
        {
            this.center = center;
            this.normal = normal;
            this.radius = radius;
        }

        /// <summary>
        /// Linearly interpolates between two circles
        /// </summary>
        public static Circle3TCT Lerp(Circle3TCT a, Circle3TCT b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Circle3TCT(
                center: a.center + (b.center - a.center) * t,
                normal: Vector3.LerpUnclamped(a.normal, b.normal, t),
                radius: a.radius + (b.radius - a.radius) * t);
        }

        /// <summary>
        /// Linearly interpolates between two circles without clamping the interpolant
        /// </summary>
        public static Circle3TCT LerpUnclamped(Circle3TCT a, Circle3TCT b, float t)
        {
            return new Circle3TCT(
                center: a.center + (b.center - a.center) * t,
                normal: Vector3.LerpUnclamped(a.normal, b.normal, t),
                radius: a.radius + (b.radius - a.radius) * t);
        }

        public static explicit operator SphereTCT(Circle3TCT circle)
        {
            return new SphereTCT(circle.center, circle.radius);
        }

        public static explicit operator Circle2TCT(Circle3TCT circle)
        {
            return new Circle2TCT((Vector2)circle.center, circle.radius);
        }

        public static Circle3TCT operator +(Circle3TCT circle, Vector3 vector)
        {
            return new Circle3TCT(circle.center + vector, circle.normal, circle.radius);
        }

        public static Circle3TCT operator -(Circle3TCT circle, Vector3 vector)
        {
            return new Circle3TCT(circle.center - vector, circle.normal, circle.radius);
        }

        public static bool operator ==(Circle3TCT a, Circle3TCT b)
        {
            return a.center == b.center && a.normal == b.normal && a.radius == b.radius;
        }

        public static bool operator !=(Circle3TCT a, Circle3TCT b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return center.GetHashCode() ^ (normal.GetHashCode() << 2) ^ (radius.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            return other is Circle3TCT && Equals((Circle3TCT)other);
        }

        public bool Equals(Circle3TCT other)
        {
            return center.Equals(other.center) && normal.Equals(other.normal) && radius.Equals(other.radius);
        }

        public override string ToString()
        {
            return string.Format("Circle3TCT(center: {0}, normal: {1}, radius: {2})", center, normal, radius);
        }

        public string ToString(string format)
        {
            return string.Format("Circle3TCT(center: {0}, normal: {1}, radius: {2})",
                center.ToString(format), normal.ToString(format), radius.ToString(format));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("Circle3TCT(center: {0}, normal: {1}, radius: {2})",
                center.ToString(format, formatProvider), normal.ToString(format, formatProvider), radius.ToString(format, formatProvider));
        }
    }
}