using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionRayCircleTCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionRayCircleTCT None()
        {
            return new IntersectionRayCircleTCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionRayCircleTCT Point(Vector2 point)
        {
            return new IntersectionRayCircleTCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionRayCircleTCT TwoPoints(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionRayCircleTCT
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}