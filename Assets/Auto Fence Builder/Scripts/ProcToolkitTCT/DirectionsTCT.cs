using System;

namespace ProceduralToolkitTCT
{
    /// <summary>
    /// Specifies directions along thee axes
    /// </summary>
    [Flags]
    public enum DirectionsTCT
    {
        None = 0,
        Left = 1,
        Right = 2,
        Down = 4,
        Up = 8,
        Back = 16,
        Forward = 32,
        XAxis = Left | Right,
        YAxis = Down | Up,
        ZAxis = Back | Forward,
        All = Left | Right | Down | Up | Back | Forward
    }

    public static class DirectionsExtensions
    {
        public static bool HasFlag(this DirectionsTCT directions, DirectionsTCT flag)
        {
            return (directions & flag) == flag;
        }

        public static DirectionsTCT AddFlag(this DirectionsTCT directions, DirectionsTCT flag)
        {
            return directions | flag;
        }

        public static DirectionsTCT RemoveFlag(this DirectionsTCT directions, DirectionsTCT flag)
        {
            return directions & ~flag;
        }
    }
}