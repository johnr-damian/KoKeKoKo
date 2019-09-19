using ModelService.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement
{
    /// <summary>
    /// A collection of <see cref="Unit"/>
    /// </summary>
    public class Army : IEnumerable<Unit>, ICopyable<Army>, IMessage
    {
        #region Private Fields
        /// <summary>
        /// A collection of <see cref="Unit"/> from source
        /// </summary>
        private Unit[] _units = default(Unit[]);

        /// <summary>
        /// A raw copy of units from source
        /// </summary>
        private string _raw_units = default(string); 
        #endregion

        /// <summary>
        /// Parses the string of units and creates an instance of <see cref="Unit"/>. 
        /// As such, it expects to follow the constructor of <see cref="Unit"/>
        /// </summary>
        /// <param name="raw_units"></param>
        public Army(string raw_units)
        {
            _raw_units = raw_units;
            var parsed_units = raw_units.Split('\n');

            if (parsed_units.Length > 0)
            {
                _units = new Unit[parsed_units.Length];

                for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                {
                    var unit_details = parsed_units[iterator].Split(',');

                    if (unit_details.Length > 0)
                    {
                        var buffs = (unit_details.Length > 5) ? unit_details.Skip(5) : Enumerable.Empty<string>();
                        _units[iterator] = new Unit(Convert.ToInt64(unit_details[0]), unit_details[1], unit_details[2], unit_details[3], Convert.ToDouble(unit_details[4]), Convert.ToDouble(unit_details[5]), buffs.ToArray());
                    }
                    else
                        throw new ArgumentOutOfRangeException("The units have no details to be parsed...");
                }
            }
            else
                throw new ArgumentOutOfRangeException("There are no units to be parsed...");
        }

        /// <summary>
        /// Enumerator for <see cref="Army"/>
        /// </summary>
        private class Enumerator : IEnumerator<Unit>
        {
            /// <summary>
            /// A collection of <see cref="Unit"/> from <see cref="Army"/>
            /// </summary>
            private Unit[] _units = default(Unit[]);

            /// <summary>
            /// The pointer to <see cref="_units"/>
            /// </summary>
            private int position = -1;

            /// <summary>
            /// Creates an instance of enumerator for <see cref="Army"/>
            /// </summary>
            /// <param name="units"></param>
            public Enumerator(Unit[] units)
            {
                _units = units;
            }

            /// <summary>
            /// Returns the current pointed unit by the <see cref="position"/>
            /// </summary>
            public Unit Current => _units[position];

            /// <summary>
            /// Interface implementation of <see cref="Current"/>
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Disposes the instance of this enumerator
            /// </summary>
            public void Dispose() { }

            /// <summary>
            /// Increments the position of the pointer to the next unit
            /// </summary>
            /// <returns></returns>
            public bool MoveNext() => ((++position) < _units.Length);

            /// <summary>
            /// Resets the position of the pointer to the beginning
            /// </summary>
            public void Reset() => position = -1;
        }

        #region Methods for Enumerator
        /// <summary>
        /// Creates a new instance of enumerator for this instance
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Unit> GetEnumerator() => new Enumerator(_units);

        /// <summary>
        /// Interface implementation of <see cref="GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); 
        #endregion

        /// <summary>
        /// Returns the total worth of this army
        /// </summary>
        /// <returns></returns>
        public UnitWorth GetValueOfArmy()
        {
            var priority_worth = _units.Sum(unit => Unit.Values[unit.Name].Priority);
            var mineral_worth = _units.Sum(unit => Unit.Values[unit.Name].Mineral);
            var vespene_worth = _units.Sum(unit => Unit.Values[unit.Name].Vespene);
            var supply_worth = _units.Sum(unit => Unit.Values[unit.Name].Supply);

            return new UnitWorth(priority_worth, mineral_worth, vespene_worth, supply_worth);
        }

        /// <summary>
        /// Returns the instance of this army
        /// </summary>
        /// <returns></returns>
        public Army GetShallowCopy() => this;

        /// <summary>
        /// Creates and returns a new instance of <see cref="Army"/> with the same value
        /// of this instance
        /// </summary>
        /// <returns></returns>
        public Army GetDeepCopy() => new Army(String.Copy(_raw_units));

        /// <summary>
        /// Returns a message-ready format that can be send to agent. The message format is:
        /// <para>
        /// <see cref="Unit.UniqueID"/>,<see cref="Unit.Targets"/>\n
        /// ...
        /// <see cref="Unit.UniqueID"/>,<see cref="Unit.Targets"/>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public string CreateMessage()
        {
            string message = "";

            foreach(var unit in _units)
            {
                var targets = Unit.GetTargetsOfUnit(unit);

                message += unit.UniqueID;
                foreach (var target in targets)
                    message += ("," + target);
                message += Environment.NewLine;
            }

            return message;
        }

        /// <summary>
        /// Returns a Jaccard-ready format. It uses <see cref="Unit.ToString"/>. 
        /// The message format is:
        /// &lt;"<see cref="Unit.UniqueID"/>", ..., "<see cref="Unit.UniqueID"/>"&gt;
        /// </summary>
        /// <returns></returns>
        public override string ToString() => (String.Join(",", (from unit in _units select unit.ToString())));
    }
}