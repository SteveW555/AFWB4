using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionCircleCircleTCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionCircleCircleTCT None()
        {
            return new IntersectionCircleCircleTCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionCircleCircleTCT Point(Vector2 point)
        {
            return new IntersectionCircleCircleTCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionCircleCircleTCT TwoPoints(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionCircleCircleTCT
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }

        public static IntersectionCircleCircleTCT Circle()
        {
            return new IntersectionCircleCircleTCT { type = IntersectionTypeTCT.Circle };
        }
    }
}