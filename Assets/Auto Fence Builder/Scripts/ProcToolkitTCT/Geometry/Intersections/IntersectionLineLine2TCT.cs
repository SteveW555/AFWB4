using UnityEngine;

namespace ProceduralToolkitTCT
{
    public struct IntersectionLineLine2TCT
    {
        public IntersectionTypeTCT type;
        public Vector2 point;

        public static IntersectionLineLine2TCT None()
        {
            return new IntersectionLineLine2TCT { type = IntersectionTypeTCT.None };
        }

        public static IntersectionLineLine2TCT Point(Vector2 point)
        {
            return new IntersectionLineLine2TCT
            {
                type = IntersectionTypeTCT.Point,
                point = point,
            };
        }

        public static IntersectionLineLine2TCT Line(Vector2 point)
        {
            return new IntersectionLineLine2TCT
            {
                type = IntersectionTypeTCT.Line,
                point = point,
            };
        }
    }
}