using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// The collection of <see cref="CSVUnit"/> based on units from a CSV file
    /// </summary>
    public class CSVUnits : IEnumerable<CSVUnit>
    {
        private CSVUnit[] _units = null;

        /// <summary>
        /// Parses the string of units and creates an instance of <see cref="CSVUnit"/>
        /// </summary>
        /// <param name="units"></param>
        public CSVUnits(string units)
        {
            var parsed_units = units.Split('\n');

            if (parsed_units.Length > 0)
            {
                _units = new CSVUnit[parsed_units.Length];

                for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                {
                    var details = parsed_units[iterator].Split(',');

                    if (details.Length > 0)
                    {
                        var buffs = (details.Length > 5) ? details.Skip(5) : Enumerable.Empty<string>();
                        _units[iterator] = new CSVUnit(details[0], details[1], Convert.ToDouble(details[2]), Convert.ToDouble(details[3]), Convert.ToInt32(details[4]), buffs.ToArray());
                    }
                    else
                        throw new ArgumentOutOfRangeException("There are no details found for the parsed unit...");
                }
            }
            else
                throw new InvalidOperationException("There are no units to be parsed...");
        }

        private class Enumerator : IEnumerator<CSVUnit>
        {
            private CSVUnit[] _enumeratable_units = null;
            private int position = -1;

            public Enumerator(CSVUnit[] units)
            {
                _enumeratable_units = units;
            }

            /// <summary>
            /// Returns the current unit pointed by the pointer
            /// </summary>
            public CSVUnit Current { get { return _enumeratable_units[position]; } }

            /// <summary>
            /// Interface implementation of <see cref="Current"/>
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Disposes this instance of enumerator
            /// </summary>
            public void Dispose() { }

            /// <summary>
            /// Iterates the position of pointer to the next unit
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                position++;
                return (position < _enumeratable_units.Length);
            }

            /// <summary>
            /// Resets the position of pointer at the beginning
            /// </summary>
            public void Reset() => position = -1;
        }

        /// <summary>
        /// Creates a new instance of enumerator for this list of units
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CSVUnit> GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Interface implementation of creating an enumerator for this list of units
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_units);
    }
}
