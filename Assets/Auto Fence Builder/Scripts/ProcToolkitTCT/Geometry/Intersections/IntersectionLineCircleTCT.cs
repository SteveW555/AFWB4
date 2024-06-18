using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionLineCircleTCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionLineCircleTCT None()
        {
            return new IntersectionLineCircleTCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionLineCircleTCT Point(Vector2 point)
        {
            return new IntersectionLineCircleTCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionLineCircleTCT TwoPoints(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionLineCircleTCT
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}