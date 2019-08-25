using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// The collection of <see cref="ObservedUnit"/> based on units from game observation
    /// </summary>
    public class ObservedUnits : IEnumerable<ObservedUnit>
    {
        private ObservedUnit[] _units = null;

        /// <summary>
        /// Parses the string of units and creates an instance of <see cref="ObservedUnit"/>
        /// </summary>
        /// <param name="units"></param>
        public ObservedUnits(string units)
        {
            var parsed_units = units.Split('\n');

            if (parsed_units.Length > 0)
            {
                _units = new ObservedUnit[parsed_units.Length];

                for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                {
                    var details = parsed_units[iterator].Split(',');

                    if (details.Length > 0)
                    {
                        var buffs = (details.Length > 5) ? details.Skip(5) : Enumerable.Empty<string>();
                        _units[iterator] = new ObservedUnit(Convert.ToInt32(details[0]), details[1], details[2], Convert.ToDouble(details[3]), Convert.ToDouble(details[4]), buffs.ToArray());
                    }
                    else
                        throw new ArgumentOutOfRangeException("There are no details found for the parsed unit...");
                }
            }
            else
                throw new InvalidOperationException("There are no units to be parsed...");
        }

        private class Enumerator : IEnumerator<ObservedUnit>
        {
            private ObservedUnit[] _enumeratable_units = null;
            private int position = -1;

            public Enumerator(ObservedUnit[] units)
            {
                _enumeratable_units = units;
            }

            /// <summary>
            /// Returns the current unit pointed by the pointer
            /// </summary>
            public ObservedUnit Current { get { return _enumeratable_units[position]; } }

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
        public IEnumerator<ObservedUnit> GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Interface implementation of creating an enumerator for this list of units
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_units);
    }
}
