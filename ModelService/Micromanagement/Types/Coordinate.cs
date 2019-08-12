using System;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// The x,y position of an entity
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// The X position of the entity
        /// </summary>
        public double X { get; private set; } = 0;

        /// <summary>
        /// The Y position of the entity
        /// </summary>
        public double Y { get; private set; } = 0;

        /// <summary>
        /// A Coordinate that contains the X and Y position of an entity
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = x;
        }

        /// <summary>
        /// Computes the distance between this coordinate to a target coordinate using Distance formula
        /// </summary>
        /// <param name="target_coordinate"></param>
        /// <returns></returns>
        public double GetDistance(Coordinate target_coordinate) => Math.Sqrt(Math.Pow((X - target_coordinate.X), 2) + Math.Pow((Y - target_coordinate.Y), 2));
    }
}
