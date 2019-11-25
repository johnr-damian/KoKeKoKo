using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Collections
{
    public class SimulatedUnits : IEnumerable<SimulatedUnit>, IFormattable
    {
        /// <summary>
        /// A list of currently alive units that is controlled by the agent.
        /// </summary>
        private SimulatedUnit[] Units { get; set; } = default(SimulatedUnit[]);

        #region Operators
        /// <summary>
        /// Adds a new unit to the existing list of units. This does not create a new unit by using
        /// the <see cref="SimulatedUnit.Copy"/>. It is only a shallow copy where the reference of 
        /// <see cref="SimulatedUnit"/> is copied in a new instance of <see cref="SimulatedUnits"/>.
        /// </summary>
        /// <param name="old_units"></param>
        /// <param name="new_unit"></param>
        /// <returns></returns>
        public static SimulatedUnits operator +(SimulatedUnits old_units, SimulatedUnit new_unit)
        {
            var new_units = new SimulatedUnit[old_units.Count() + 1];

            int iterator = 0;
            foreach (var old_unit in old_units)
            {
                new_units[iterator] = old_unit;
                iterator++;
            }
            new_units[iterator] = new_unit;

            return new SimulatedUnits(new_units);
        }

        /// <summary>
        /// Adds an existing list of units to another existing list of units. This does not create
        /// a new unit by using the <see cref="SimulatedUnit.Copy"/>, but it is only a shallow copy
        /// where the reference of <see cref="SimulatedUnit"/> is copied in the new instance of <see cref="SimulatedUnits"/>.
        /// </summary>
        /// <param name="old_units"></param>
        /// <param name="new_units"></param>
        /// <returns></returns>
        public static SimulatedUnits operator +(SimulatedUnits old_units, SimulatedUnits new_units)
        {
            var concatenated_units = new SimulatedUnit[old_units.Count() + new_units.Count()];

            int iterator = 0;
            foreach (var old_unit in old_units)
            {
                concatenated_units[iterator] = old_unit;
                iterator++;
            }
            foreach (var new_unit in new_units)
            {
                concatenated_units[iterator] = new_unit;
                iterator++;
            }

            return new SimulatedUnits(concatenated_units);
        }
        #endregion

        /// <summary>
        /// Enumerator for <see cref="SimulatedUnits"/>
        /// </summary>
        private class Enumerator : IEnumerator<SimulatedUnit>
        {
            #region Properties
            /// <summary>
            /// A pointer to <see cref="Units"/>.
            /// </summary>
            private int Current_Unit { get; set; } = default(int);

            /// <summary>
            /// A list of units from <see cref="SimulatedUnits"/>.
            /// </summary>
            private SimulatedUnit[] Units = default(SimulatedUnit[]);

            /// <summary>
            /// The current unit pointed by the enumerator pointer.
            /// </summary>
            public SimulatedUnit Current => Units[Current_Unit];

            object IEnumerator.Current => Current; 
            #endregion

            /// <summary>
            /// Creates an instance of enumerator by copying the reference
            /// of list of units from <see cref="SimulatedUnits"/>.
            /// </summary>
            /// <param name="units"></param>
            public Enumerator(SimulatedUnit[] units)
            {
                Current_Unit = -1;
                Units = units;                
            }

            #region Methods
            /// <summary>
            /// Disposes the instance of this enumerator
            /// </summary>
            public void Dispose() { }

            /// <summary>
            /// Increments the pointer to the next unit.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext() => ((++Current_Unit) < Units.Length);

            /// <summary>
            /// Resets the pointer back to -1.
            /// </summary>
            public void Reset() => Current_Unit = -1; 
            #endregion
        }

        #region Constructors
        /// <summary>
        /// Initializes the required properties with default values.
        /// This constructor is used to create a list of units for simulation of 
        /// agent that comes from a CSV file.
        /// </summary>
        public SimulatedUnits()
        {
            Units = new SimulatedUnit[13]; //There are 13 initial units in the starting game

            //Add a TERRAN_COMMANDCENTER
            //Units[0] = new SimulatedUnit(String.Join(",", Guid.NewGuid().ToString("N"), "TERRAN_COMMANDCENTER"));
            Units[0] = new SimulatedUnit("TERRAN_COMMANDCENTER", Enumerable.Empty<string>());

            //Add 12 TERRAN_SCV
            for (int iterator = 1; iterator < Units.Length; iterator++)
                //Units[iterator] = new SimulatedUnit(String.Join(",", Guid.NewGuid().ToString("N"), "TERRAN_SCV"));
                Units[iterator] = new SimulatedUnit("TERRAN_SCV", Enumerable.Empty<string>());
        }

        /// <summary>
        /// Initializes the required properties with values coming from
        /// a CSV file. This constructor is used to create a simulation for Micromanagement.
        /// In addition, this constructor is also used to create a list of units
        /// for simulation of agent that comes from C++ agent.
        /// </summary>
        /// <param name="units"></param>
        public SimulatedUnits(string[] units)
        {
            if (units.Length > 0)
            {
                Units = new SimulatedUnit[units.Length];

                for (int iterator = 0; iterator < units.Length; iterator++)
                {
                    System.Diagnostics.Trace.WriteLine(units[iterator].Split(',')[1]);
                    Units[iterator] = new SimulatedUnit(units[iterator].Split(','), Enumerable.Empty<string>());
                }
            }
            else
                throw new Exception("Failed to create a list unit...");
        } 
        
        /// <summary>
        /// A copy constructor used by operators. This does not create a new instance of array of 
        /// <see cref="SimulatedUnit"/>. It only copies the reference of the created array in operators.
        /// </summary>
        /// <param name="units"></param>
        private SimulatedUnits(SimulatedUnit[] units) => Units = units;

        /// <summary>
        /// A copy constructor used by <see cref="Copy"/>. This creates a new instance instance of
        /// array with values of unit are copied using <see cref="SimulatedUnit.Copy"/>.
        /// </summary>
        /// <param name="existing_units"></param>
        private SimulatedUnits(SimulatedUnits existing_units)
        {
            Units = new SimulatedUnit[existing_units.Count()];

            int iterator = 0;
            foreach(var existing_unit in existing_units)
            {
                Units[iterator] = existing_unit.Copy();
                iterator++;
            }
        }
        #endregion

        #region Enumerator Methods
        /// <summary>
        /// Creates a new instance of enumerator of this list of units.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<SimulatedUnit> GetEnumerator() => new Enumerator(Units);

        /// <summary>
        /// Interface implementation of <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); 
        #endregion

        /// <summary>
        /// Creates a deep copy of this list. Meaning, it also creates a copy of unit 
        /// in the existing list to a new  list of units.
        /// </summary>
        /// <returns></returns>
        public SimulatedUnits Copy() => new SimulatedUnits(this);

        #region ToString Methods
        /// <summary>
        /// Returns a list of UID of Units, UID of Target Opposing Units, and their action
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString("M", CultureInfo.CurrentCulture);

        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format.ToUpperInvariant())
            {
                case "M":

                    return "";
                default:
                    throw new Exception($@"Failed to format into string...");
            }
        } 
        #endregion
    }
}
