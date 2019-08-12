using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A parsed unit from string either based on game observation or CSV
    /// </summary>
    public class Unit
    {
        /// <summary>
        /// The controller of this unit
        /// </summary>
        public string Alliance { get; private set; } = "";

        /// <summary>
        /// The name of this unit
        /// </summary>
        public string Name { get; private set; } = "";

        /// <summary>
        /// The current health of the unit based on current observation
        /// </summary>
        public double Health { get; private set; } = 0;

        /// <summary>
        /// The current energy of the unit based on current observation
        /// </summary>
        public double Energy { get; private set; } = 0;

        /// <summary>
        /// If the unit is currently flying based on current observation
        /// </summary>
        public bool Is_Flying { get; private set; } = false;

        /// <summary>
        /// The current buffs applied on this unit
        /// </summary>
        public List<string> Buffs { get; private set; } = null;

        /// <summary>
        /// The current position of this unit based on current observation
        /// </summary>
        public Coordinate Position { get; private set; } = null;

        /// <summary>
        /// The seconds where this unit is observed during combat
        /// </summary>
        public int Timestamp { get; private set; } = 0;

        /// <summary>
        /// Creates an instance of a unit based on a CSV file
        /// </summary>
        /// <param name="alliance"></param>
        /// <param name="name"></param>
        /// <param name="health"></param>
        /// <param name="energy"></param>
        /// <param name="x_position"></param>
        /// <param name="y_position"></param>
        /// <param name="is_flying"></param>
        /// <param name="timestamp"></param>
        /// <param name="buffs"></param>
        public Unit(string alliance, string name, double health, double energy, double x_position, double y_position, bool is_flying, int timestamp, params string[] buffs)
        {
            try
            {
                Alliance = alliance;
                Name = name;
                Health = health;
                Energy = energy;
                Position = new Coordinate(x_position, y_position);
                Is_Flying = is_flying;
                Timestamp = timestamp;
                Buffs = new List<string>(buffs);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to create an instance of unit...");
                Trace.WriteLine($@"Error in Model! Unit -> Unit(): \n\t{ex.Message}");
            }
        }

        /// <summary>
        /// Creates an instance of a unit based on the current game observation
        /// </summary>
        /// <param name="alliance"></param>
        /// <param name="name"></param>
        /// <param name="health"></param>
        /// <param name="energy"></param>
        /// <param name="x_position"></param>
        /// <param name="y_position"></param>
        /// <param name="is_flying"></param>
        /// <param name="buffs"></param>
        public Unit(string alliance, string name, double health, double energy, double x_position, double y_position, bool is_flying, params string[] buffs)
            : this(alliance, name, health, energy, x_position, y_position, is_flying, 0, buffs) { }

        /// <summary>
        /// Creates a deep copy of this unit with exact independent values
        /// </summary>
        /// <returns></returns>
        public Unit CreateCopy() => new Unit(String.Copy(Alliance), String.Copy(Name), Health, Energy, Position.X, Position.Y, Is_Flying, Timestamp, (from buff in Buffs select String.Copy(buff)).ToArray());
    }
}
