using System;

namespace ModelService.Types
{
    /// <summary>
    /// The position of an object in a 2D plane
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// The position of the object in X axis
        /// </summary>
        public double X { get; private set; } = -1;

        /// <summary>
        /// The position of the object in Y axis
        /// </summary>
        public double Y { get; private set; } = -1;

        /// <summary>
        /// Stores the coordinate of an object using X axis and Y axis
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Computes and returns the distance to target from this object
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double GetDistance(Coordinate target) => Math.Sqrt(Math.Pow((X - target.X), 2) + Math.Pow((Y - target.Y), 2));

        /// <summary>
        /// Computes and returns the distance between the source object and the target object
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double GetDistance(Coordinate source, Coordinate target) => Math.Sqrt(Math.Pow((source.X - target.X), 2) + Math.Pow((source.Y - target.Y), 2));
    }
}
