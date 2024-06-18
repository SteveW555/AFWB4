using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Representation of a 2D line segment
    /// </summary>
    [Serializable]
    public struct Segment2TCT : IEquatable<Segment2TCT>, IFormattable
    {
        public Vector2 a;
        public Vector2 b;

        /// <summary>
        /// Returns the normalized direction of the segment
        /// </summary>
        public Vector2 direction => (b - a).normalized;

        /// <summary>
        /// Returns the length of the segment
        /// </summary>
        public float length => (b - a).magnitude;

        /// <summary>
        /// Returns the center of the segment
        /// </summary>
        public Vector2 center => GetPoint(0.5f);

        /// <summary>
        /// Returns the axis-aligned bounding box of the segment
        /// </summary>
        public Rect aabb
        {
            get
            {
                Vector2 min = Vector2.Min(a, b);
                Vector2 max = Vector2.Max(a, b);
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        public Segment2TCT(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }

        /// <summary>
        /// Access the a or b component using [0] or [1] respectively
        /// </summary>
        public Vector2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return a;
                    case 1: return b;
                    default:
                        throw new IndexOutOfRangeException("Invalid Segment2TCT index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        a = value;
                        break;

                    case 1:
                        b = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException("Invalid Segment2TCT index!");
                }
            }
        }

        /// <summary>
        /// Returns a point on the segment at the given normalized position
        /// </summary>
        /// <param name="position">Normalized position</param>
        public Vector2 GetPoint(float position)
        {
            return GeometryTCT.PointOnSegment2TCT(a, b, position);
        }

        /// <summary>
        /// Returns a list of evenly distributed points on the segment
        /// </summary>
        /// <param name="count">Number of points</param>
        public List<Vector2> GetPoints(int count)
        {
            return GeometryTCT.PointsOnSegment2TCT(a, b, count);
        }

        /// <summary>
        /// Linearly interpolates between two segments
        /// </summary>
        public static Segment2TCT Lerp(Segment2TCT a, Segment2TCT b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Segment2TCT(a.a + (b.a - a.a) * t, a.b + (b.b - a.b) * t);
        }

        /// <summary>
        /// Linearly interpolates between two segments without clamping the interpolant
        /// </summary>
        public static Segment2TCT LerpUnclamped(Segment2TCT a, Segment2TCT b, float t)
        {
            return new Segment2TCT(a.a + (b.a - a.a) * t, a.b + (b.b - a.b) * t);
        }

        #region Casting operators

        public static explicit operator Line2TCT(Segment2TCT segment)
        {
            return new Line2TCT(segment.a, (segment.b - segment.a).normalized);
        }

        public static explicit operator Ray2D(Segment2TCT segment)
        {
            return new Ray2D(segment.a, (segment.b - segment.a).normalized);
        }

        public static explicit operator Segment3TCT(Segment2TCT segment)
        {
            return new Segment3TCT((Vector3)segment.a, (Vector3)segment.b);
        }

        #endregion Casting operators

        public static Segment2TCT operator +(Segment2TCT segment, Vector2 vector)
        {
            return new Segment2TCT(segment.a + vector, segment.b + vector);
        }

        public static Segment2TCT operator -(Segment2TCT segment, Vector2 vector)
        {
            return new Segment2TCT(segment.a - vector, segment.b - vector);
        }

        public static bool operator ==(Segment2TCT a, Segment2TCT b)
        {
            return a.a == b.a && a.b == b.b;
        }

        public static bool operator !=(Segment2TCT a, Segment2TCT b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ (b.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            return other is Segment2TCT && Equals((Segment2TCT)other);
        }

        public bool Equals(Segment2TCT other)
        {
            return a.Equals(other.a) && b.Equals(other.b);
        }

        public override string ToString()
        {
            return string.Format("Segment2TCT(a: {0}, b: {1})", a, b);
        }

        public string ToString(string format)
        {
            return string.Format("Segment2TCT(a: {0}, b: {1})", a.ToString(format), b.ToString(format));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("Segment2TCT(a: {0}, b: {1})", a.ToString(format, formatProvider), b.ToString(format, formatProvider));
        }
    }
}