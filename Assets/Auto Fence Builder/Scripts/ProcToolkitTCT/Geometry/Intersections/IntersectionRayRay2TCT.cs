using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionRayRay2TCT
    {
        public IntersectionTypeTCT type;
        public Vector2 pointA;
        public Vector2 pointB;

        public static IntersectionRayRay2TCT None()
        {
            return new IntersectionRayRay2TCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionRayRay2TCT Point(Vector2 point)
        {
            return new IntersectionRayRay2TCT
            {
                type = IntersectionTypeTCT.Point,
                pointA = point,
            };
        }

        public static IntersectionRayRay2TCT Ray(Vector2 origin, Vector2 direction)
        {
            return new IntersectionRayRay2TCT
            {
                type = IntersectionTypeTCT.Ray,
                pointA = origin,
                pointB = direction,
            };
        }

        public static IntersectionRayRay2TCT Segment(Vector2 pointA, Vector2 pointB)
        {
            return new IntersectionRayRay2TCT
            {
                type = IntersectionTypeTCT.Segment,
                pointA = pointA,
                pointB = pointB,
            };
        }
    }
}