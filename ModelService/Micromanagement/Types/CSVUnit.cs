using System;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A parsed unit based on a CSV file
    /// </summary>
    public class CSVUnit : Unit
    {
        //Creates a unique ID for a unit
        private static int Global_IDTracker = 0;

        /// <summary>
        /// A unique identifier for this unit
        /// </summary>
        public int Global_ID { get; private set; } = 0;

        /// <summary>
        /// The seconds where this unit is found to be engaged in battle
        /// </summary>
        public int Timestamp { get; private set; } = 0;

        /// <summary>
        /// Creates an instance of parsed unit based on a CSV file
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="timestamp"></param>
        /// <param name="buffs"></param>
        public CSVUnit(string owner, string name, double x, double y, int timestamp, params string[] buffs)
            : base(owner, name, x, y, buffs)
        {
            Global_ID = ++Global_IDTracker;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CSVUnit"/> with the same value of this unit
        /// </summary>
        /// <returns></returns>
        public override Unit CreateDeepCopy()
        {
            var new_csvunit = new CSVUnit(String.Copy(Owner), String.Copy(Name), Position.X, Position.Y, Timestamp, (from buff in Buffs select String.Copy(buff)).ToArray());
            new_csvunit.Simulated_Health = Simulated_Health;
            new_csvunit.Simulated_Energy = Simulated_Energy;
            new_csvunit.Target = Target;

            return new_csvunit;
        }
    }
}
