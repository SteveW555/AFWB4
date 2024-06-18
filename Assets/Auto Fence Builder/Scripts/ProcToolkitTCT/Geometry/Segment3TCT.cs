using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Representation of a 3D line segment
    /// </summary>
    [Serializable]
    public struct Segment3TCT : IEquatable<Segment3TCT>, IFormattable
    {
        public Vector3 a;
        public Vector3 b;

        /// <summary>
        /// Returns the normalized direction of the segment
        /// </summary>
        public Vector3 direction => (b - a).normalized;

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
        public Bounds aabb
        {
            get
            {
                var bounds = new Bounds();
                bounds.SetMinMax(Vector3.Min(a, b), Vector3.Max(a, b));
                return bounds;
            }
        }

        public Segment3TCT(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        /// <summary>
        /// Access the a or b component using [0] or [1] respectively
        /// </summary>
        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return a;
                    case 1: return b;
                    default:
                        throw new IndexOutOfRangeException("Invalid Segment3TCT index!");
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
                        throw new IndexOutOfRangeException("Invalid Segment3TCT index!");
                }
            }
        }

        /// <summary>
        /// Returns a point on the segment at the given normalized position
        /// </summary>
        /// <param name="position">Normalized position</param>
        public Vector3 GetPoint(float position)
        {
            return GeometryTCT.PointOnSegment3(a, b, position);
        }

        /// <summary>
        /// Returns a list of evenly distributed points on the segment
        /// </summary>
        /// <param name="count">Number of points</param>
        public List<Vector3> GetPoints(int count)
        {
            return GeometryTCT.PointsOnSegment3(a, b, count);
        }

        /// <summary>
        /// Linearly interpolates between two segments
        /// </summary>
        public static Segment3TCT Lerp(Segment3TCT a, Segment3TCT b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Segment3TCT(a.a + (b.a - a.a) * t, a.b + (b.b - a.b) * t);
        }

        /// <summary>
        /// Linearly interpolates between two segments without clamping the interpolant
        /// </summary>
        public static Segment3TCT LerpUnclamped(Segment3TCT a, Segment3TCT b, float t)
        {
            return new Segment3TCT(a.a + (b.a - a.a) * t, a.b + (b.b - a.b) * t);
        }

        #region Casting operators

        public static explicit operator Line3TCT(Segment3TCT segment)
        {
            return new Line3TCT(segment.a, (segment.b - segment.a).normalized);
        }

        public static explicit operator Ray(Segment3TCT segment)
        {
            return new Ray(segment.a, (segment.b - segment.a).normalized);
        }

        public static explicit operator Segment2TCT(Segment3TCT segment)
        {
            return new Segment2TCT((Vector2)segment.a, (Vector2)segment.b);
        }

        #endregion Casting operators

        public static Segment3TCT operator +(Segment3TCT segment, Vector3 vector)
        {
            return new Segment3TCT(segment.a + vector, segment.b + vector);
        }

        public static Segment3TCT operator -(Segment3TCT segment, Vector3 vector)
        {
            return new Segment3TCT(segment.a - vector, segment.b - vector);
        }

        public static bool operator ==(Segment3TCT a, Segment3TCT b)
        {
            return a.a == b.a && a.b == b.b;
        }

        public static bool operator !=(Segment3TCT a, Segment3TCT b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ (b.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            return other is Segment3TCT && Equals((Segment3TCT)other);
        }

        public bool Equals(Segment3TCT other)
        {
            return a.Equals(other.a) && b.Equals(other.b);
        }

        public override string ToString()
        {
            return string.Format("Segment3TCT(a: {0}, b: {1})", a, b);
        }

        public string ToString(string format)
        {
            return string.Format("Segment3TCT(a: {0}, b: {1})", a.ToString(format), b.ToString(format));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("Segment3TCT(a: {0}, b: {1})", a.ToString(format, formatProvider), b.ToString(format, formatProvider));
        }
    }
}