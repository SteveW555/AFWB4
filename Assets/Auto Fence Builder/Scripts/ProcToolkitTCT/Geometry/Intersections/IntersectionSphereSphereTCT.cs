using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionSphereSphere
    {
        public IntersectionTypeTCT type;
        public Vector3 point;
        public Vector3 normal;
        public float radius;

        public static IntersectionSphereSphere None()
        {
            return new IntersectionSphereSphere { type = IntersectionTypeTCT.None };
        }

        public static IntersectionSphereSphere Point(Vector3 point)
        {
            return new IntersectionSphereSphere
            {
                type = IntersectionTypeTCT.Point,
                point = point,
            };
        }

        public static IntersectionSphereSphere Circle(Vector3 center, Vector3 normal, float radius)
        {
            return new IntersectionSphereSphere
            {
                type = IntersectionTypeTCT.Circle,
                point = center,
                normal = normal,
                radius = radius,
            };
        }

        public static IntersectionSphereSphere Sphere(Vector3 center, float radius)
        {
            return new IntersectionSphereSphere
            {
                type = IntersectionTypeTCT.Sphere,
                point = center,
                radius = radius,
            };
        }
    }
}