using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionSegmentSphere
    {
        public IntersectionTypeTCT type;
        public Vector3 pointA;
        public Vector3 pointB;

        public static IntersectionSegmentSphere None()
        {
            return new IntersectionSegmentSphere { type = IntersectionTypeTCT.None };
        }

        public static IntersectionSegmentSphere Point(Vector3 point)
        {
            return new IntersectionSegmentSphere
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionSegmentSphere TwoPoints(Vector3 pointA, Vector3 pointB)
        {
            return new IntersectionSegmentSphere
            {
                type = IntersectionTypeTCT.TwoPoints,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}