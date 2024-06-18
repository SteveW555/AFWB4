using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionSegmentSegment2TCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionSegmentSegment2TCT None()
        {
            return new IntersectionSegmentSegment2TCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionSegmentSegment2TCT Point(Vector2 point)
        {
            return new IntersectionSegmentSegment2TCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionSegmentSegment2TCT Segment(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionSegmentSegment2TCT
            {
                type = IntersectionTypeTCT.Segment,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}