using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionRaySphereTCT
    {
        public IntersectionTypeTCT type;
        public Vector3 pointA;
        public Vector3 pointB;

        public static IntersectionRaySphereTCT None()
        {
            return new IntersectionRaySphereTCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionRaySphereTCT Point(Vector3 point)
        {
            return new IntersectionRaySphereTCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionRaySphereTCT TwoPoints(Vector3 pointA, Vector3 pointB)
        {
            return new IntersectionRaySphereTCT
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}