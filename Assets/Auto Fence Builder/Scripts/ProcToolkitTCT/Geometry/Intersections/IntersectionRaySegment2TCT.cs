using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionRaySegment2TCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionRaySegment2TCT None()
        {
            return new IntersectionRaySegment2TCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionRaySegment2TCT Point(Vector2 pointA)
        {
            return new IntersectionRaySegment2TCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = pointA,
            };
        }

        public static IntersectionRaySegment2TCT Segment(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionRaySegment2TCT
            {
                type = IntersectionTypeTCT.Segment,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}