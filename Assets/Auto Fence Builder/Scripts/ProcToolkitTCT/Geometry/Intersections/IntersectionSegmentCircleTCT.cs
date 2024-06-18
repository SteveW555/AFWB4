using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionSegmentCircleTCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionSegmentCircleTCT None()
        {
            return new IntersectionSegmentCircleTCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionSegmentCircleTCT Point(Vector2 point)
        {
            return new IntersectionSegmentCircleTCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionSegmentCircleTCT TwoPoints(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionSegmentCircleTCT
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}