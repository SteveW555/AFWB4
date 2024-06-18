using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionLineSegment2TCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionLineSegment2TCT None()
        {
            return new IntersectionLineSegment2TCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionLineSegment2TCT Point(Vector2 pointA)
        {
            return new IntersectionLineSegment2TCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = pointA,
            };
        }

        public static IntersectionLineSegment2TCT Segment(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionLineSegment2TCT
            {
                type = IntersectionTypeTCT.Segment,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}