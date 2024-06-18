using System;
using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Representation of a 3D line
    /// </summary>
    [Serializable]
    public struct Line3TCT : IEquatable<Line3TCT>, IFormattable
    {
        public Vector3 origin;
        public Vector3 direction;

        public static Line3TCT xAxis => new Line3TCT(Vector3.zero, Vector3.right);
        public static Line3TCT yAxis => new Line3TCT(Vector3.zero, Vector3.up);
        public static Line3TCT zAxis => new Line3TCT(Vector3.zero, Vector3.forward);

        public Line3TCT(Ray ray)
        {
            origin = ray.origin;
            direction = ray.direction;
        }

        public Line3TCT(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        /// <summary>
        /// Returns a point at <paramref name="distance"/> units from origin along the line
        /// </summary>
        public Vector3 GetPoint(float distance)
        {
            return origin + direction * distance;
        }

        /// <summary>
        /// Linearly interpolates between two lines
        /// </summary>
        public static Line3TCT Lerp(Line3TCT a, Line3TCT b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Line3TCT(a.origin + (b.origin - a.origin) * t, a.direction + (b.direction - a.direction) * t);
        }

        /// <summary>
        /// Linearly interpolates between two lines without clamping the interpolant
        /// </summary>
        public static Line3TCT LerpUnclamped(Line3TCT a, Line3TCT b, float t)
        {
            return new Line3TCT(a.origin + (b.origin - a.origin) * t, a.direction + (b.direction - a.direction) * t);
        }

        #region Casting operators

        public static explicit operator Line3TCT(Ray ray)
        {
            return new Line3TCT(ray);
        }

        public static explicit operator Ray(Line3TCT line)
        {
            return new Ray(line.origin, line.direction);
        }

        public static explicit operator Ray2D(Line3TCT line)
        {
            return new Ray2D((Vector2)line.origin, (Vector2)line.direction);
        }

        public static explicit operator Line2TCT(Line3TCT line)
        {
            return new Line2TCT((Vector2)line.origin, (Vector2)line.direction);
        }

        #endregion Casting operators

        public static Line3TCT operator +(Line3TCT line, Vector3 vector)
        {
            return new Line3TCT(line.origin + vector, line.direction);
        }

        public static Line3TCT operator -(Line3TCT line, Vector3 vector)
        {
            return new Line3TCT(line.origin - vector, line.direction);
        }

        public static bool operator ==(Line3TCT a, Line3TCT b)
        {
            return a.origin == b.origin && a.direction == b.direction;
        }

        public static bool operator !=(Line3TCT a, Line3TCT b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return origin.GetHashCode() ^ (direction.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            return other is Line3TCT && Equals((Line3TCT)other);
        }

        public bool Equals(Line3TCT other)
        {
            return origin.Equals(other.origin) && direction.Equals(other.direction);
        }

        public override string ToString()
        {
            return string.Format("Line3TCT(origin: {0}, direction: {1})", origin, direction);
        }

        public string ToString(string format)
        {
            return string.Format("Line3TCT(origin: {0}, direction: {1})", origin.ToString(format), direction.ToString(format));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("Line3TCT(origin: {0}, direction: {1})", origin.ToString(format, formatProvider),
                direction.ToString(format, formatProvider));
        }
    }
}