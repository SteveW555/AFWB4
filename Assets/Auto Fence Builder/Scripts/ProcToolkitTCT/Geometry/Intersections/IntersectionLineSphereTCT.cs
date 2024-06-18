using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionLineSphereTCT
    {
        public IntersectionTypeTCT type;
        public Vector3 pointA;
        public Vector3 pointB;

        public static IntersectionLineSphereTCT None()
        {
            return new IntersectionLineSphereTCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionLineSphereTCT Point(Vector3 point)
        {
            return new IntersectionLineSphereTCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionLineSphereTCT TwoPoints(Vector3 pointA, Vector3 pointB)
        {
            return new IntersectionLineSphereTCT
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}