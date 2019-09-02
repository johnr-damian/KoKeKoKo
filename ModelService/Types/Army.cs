using System.Collections;
using System.Collections.Generic;

namespace ModelService.Types
{
    /// <summary>
    /// The collection of either <see cref="Micromanagement.Types.CSVbasedUnit"/> or <see cref="Micromanagement.Types.GamebasedUnit"/>
    /// </summary>
    public abstract class Army : IEnumerable<Unit>
    {
        /// <summary>
        /// A collection of <see cref="Unit"/>
        /// </summary>
        protected Unit[] Units { get; set; } = null;

        private class Enumerator : IEnumerator<Unit>
        {
            private Unit[] _units = null;
            private int position = -1;

            public Enumerator(Unit[] units)
            {
                _units = units;
            }

            /// <summary>
            /// Returns the current pointed unit by the pointer
            /// </summary>
            public Unit Current => _units[position];

            /// <summary>
            /// Interface implementation of <see cref="Current"/>
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Disposes this instance of enumerator
            /// </summary>
            public void Dispose() { }

            /// <summary>
            /// Iterates the position of the pointer to the next unit
            /// </summary>
            /// <returns></returns>
            public bool MoveNext() => ((++position) < _units.Length);

            /// <summary>
            /// Resets the position of pointer at the beginning
            /// </summary>
            public void Reset() => position = -1;
        }

        /// <summary>
        /// Returns a new instance of <see cref="Army.Enumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Unit> GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Interface implementation of returning a new instance of <see cref="Army.Enumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(Units);

        /// <summary>
        /// Gets the total army value with configurable weight importance by adding the 
        /// mineral cost, vespene cost, and supply cost
        /// </summary>
        /// <param name="mineral_weight"></param>
        /// <param name="vespene_weight"></param>
        /// <param name="supply_weight"></param>
        /// <returns></returns>
        public double GetArmyValue(double mineral_weight, double vespene_weight, double supply_weight)
        {
            double value = 0;

            foreach (var unit in Units)
                value += 0; //TODO

            return value;
        }
    }
}
