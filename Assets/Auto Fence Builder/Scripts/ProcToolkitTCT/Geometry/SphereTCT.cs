using System;
using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Representation of a sphere
    /// </summary>
    [Serializable]
    public struct SphereTCT : IEquatable<SphereTCT>, IFormattable
    {
        public Vector3 center;
        public float radius;

        /// <summary>
        /// Returns the area of the sphere
        /// </summary>
        public float area => 4 * Mathf.PI * radius * radius;

        /// <summary>
        /// Returns the volume of the sphere
        /// </summary>
        public float volume => 4f / 3f * Mathf.PI * radius * radius * radius;

        public static SphereTCT unit => new SphereTCT(Vector3.zero, 1);

        public SphereTCT(float radius)
        {
            center = Vector3.zero;
            this.radius = radius;
        }

        public SphereTCT(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        /// <summary>
        /// Returns a point on the sphere at the given coordinates
        /// </summary>
        /// <param name="horizontalAngle">Horizontal angle in degrees [0, 360]</param>
        /// <param name="verticalAngle">Vertical angle in degrees [-90, 90]</param>
        public Vector3 GetPoint(float horizontalAngle, float verticalAngle)
        {
            return center + GeometryTCT.PointOnSphere(radius, horizontalAngle, verticalAngle);
        }

        /// <summary>
        /// Returns true if the point intersects the sphere
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return IntersectTCT.PointSphere(point, center, radius);
        }

        /// <summary>
        /// Linearly interpolates between two spheres
        /// </summary>
        public static SphereTCT Lerp(SphereTCT a, SphereTCT b, float t)
        {
            t = Mathf.Clamp01(t);
            return new SphereTCT(a.center + (b.center - a.center) * t, a.radius + (b.radius - a.radius) * t);
        }

        /// <summary>
        /// Linearly interpolates between two spheres without clamping the interpolant
        /// </summary>
        public static SphereTCT LerpUnclamped(SphereTCT a, SphereTCT b, float t)
        {
            return new SphereTCT(a.center + (b.center - a.center) * t, a.radius + (b.radius - a.radius) * t);
        }

        public static explicit operator Circle2TCT(SphereTCT sphere)
        {
            return new Circle2TCT((Vector2)sphere.center, sphere.radius);
        }

        public static SphereTCT operator +(SphereTCT sphere, Vector3 vector)
        {
            return new SphereTCT(sphere.center + vector, sphere.radius);
        }

        public static SphereTCT operator -(SphereTCT sphere, Vector3 vector)
        {
            return new SphereTCT(sphere.center - vector, sphere.radius);
        }

        public static bool operator ==(SphereTCT a, SphereTCT b)
        {
            return a.center == b.center && a.radius == b.radius;
        }

        public static bool operator !=(SphereTCT a, SphereTCT b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return center.GetHashCode() ^ (radius.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            return other is SphereTCT && Equals((SphereTCT)other);
        }

        public bool Equals(SphereTCT other)
        {
            return center.Equals(other.center) && radius.Equals(other.radius);
        }

        public override string ToString()
        {
            return string.Format("SphereTCT(center: {0}, radius: {1})", center, radius);
        }

        public string ToString(string format)
        {
            return string.Format("SphereTCT(center: {0}, radius: {1})", center.ToString(format), radius.ToString(format));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("SphereTCT(center: {0}, radius: {1})", center.ToString(format, formatProvider),
                radius.ToString(format, formatProvider));
        }
    }
}