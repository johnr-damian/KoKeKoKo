using System;

namespace ModelService.Types
{
    /// <summary>
    /// The position of a unit in a 2D plane
    /// </summary>
    public struct Coordinate
    {
        /// <summary>
        /// The position of the unit in x axis
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// The position of the unit in y axis
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        /// Stores the position of a unit in a 2D plane
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Computes and returns the distance from this coordinate to a target coordinate
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double GetDistance(Coordinate target) => Math.Sqrt(Math.Pow((X - target.X), 2) + Math.Pow((Y - target.Y), 2));

        /// <summary>
        /// Computes and returns the distance between the source coordinate and the target coordinate
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double GetDistance(Coordinate source, Coordinate target) => Math.Sqrt(Math.Pow((source.X - target.X), 2) + Math.Pow((source.Y - target.Y), 2));
    }
}