using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionLineRay2TCT
    {
        public IntersectionTypeTCT type;
        public Vector2 point;

        public static IntersectionLineRay2TCT None()
        {
            return new IntersectionLineRay2TCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionLineRay2TCT Point(Vector2 point)
        {
            return new IntersectionLineRay2TCT
            {
                type = IntersectionTypeTCT.Point,
                point = point,
            };
        }

        public static IntersectionLineRay2TCT Ray(Vector2 point)
        {
            return new IntersectionLineRay2TCT
            {
                type = IntersectionTypeTCT.Ray,
                point = point,
            };
        }
    }
}