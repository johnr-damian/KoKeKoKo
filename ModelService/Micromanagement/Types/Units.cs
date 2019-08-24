using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A collection of <see cref="Unit"/> instance of each units
    /// </summary>
    public class Units : IEnumerable<Unit>
    {
        /// <summary>
        /// The collection of instance of parsed units
        /// </summary>
        private Unit[] _units = null;

        /// <summary>
        /// Parses the string of units and creates an instance of each unit to store the information
        /// </summary>
        /// <param name="units"></param>
        /// <param name="is_csvbased"></param>
        public Units(string units, bool is_csvbased)
        {
            try
            {
                var parsed_units = units.Split('\n');

                if (parsed_units.Length > 0)
                {
                    _units = new Unit[parsed_units.Length];

                    for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                    {
                        var unit_details = parsed_units[iterator].Split(',');
                        if(is_csvbased)
                        {
                            _units[iterator] = new Unit(unit_details[0], unit_details[1], Convert.ToDouble(unit_details[2]),
                                Convert.ToDouble(unit_details[3]), Convert.ToDouble(unit_details[4]), Convert.ToDouble(unit_details[5]),
                                Convert.ToBoolean(unit_details[6]), Convert.ToInt32(unit_details[7]), ((unit_details.Length > 8) ? unit_details.Skip(8) : Enumerable.Empty<string>()).ToArray());
                        }
                        else
                        {
                            _units[iterator] = new Unit(unit_details[0], unit_details[1], Convert.ToDouble(unit_details[2]),
                                Convert.ToDouble(unit_details[3]), Convert.ToDouble(unit_details[4]), Convert.ToDouble(unit_details[5]),
                                Convert.ToBoolean(unit_details[6]), ((unit_details.Length > 8) ? unit_details.Skip(8) : Enumerable.Empty<string>()).ToArray());
                        }
                    }
                }
                else
                    throw new Exception("There are no units in string to generate from....");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to generate a list of units...");
                Trace.WriteLine($@"Error in Model! Units -> Units(): \n\t{ex.Message}");
            }
        }

        public Tuple<double, double> GetPredictedValue()
        {
            return null;
        }

        /// <summary>
        /// Creates a new instance of enumerator for units
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Unit> GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Interface implementation of creating a new instance of enumerator for units
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => new UnitsEnumerator(_units);
    }
}
