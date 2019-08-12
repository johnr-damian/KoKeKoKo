using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// An enumerator for <see cref="Units"/>
    /// </summary>
    public class UnitsEnumerator : IEnumerator<Unit>
    {
        private Unit[] _units;
        private int position = -1;

        /// <summary>
        /// Copies the content of the <see cref="Units"/>
        /// </summary>
        /// <param name="units"></param>
        public UnitsEnumerator(Unit[] units)
        {
            _units = units;
        }

        /// <summary>
        /// Returns the current unit
        /// </summary>
        public Unit Current
        {
            get
            {
                try
                {
                    return _units[position];
                }
                catch(IndexOutOfRangeException ex)
                {
                    Console.WriteLine("Error Occurred! Failed to retrieve the current unit...");
                    Trace.WriteLine($@"Error in Model! UnitsEnumerator -> Current: \n\t{ex.Message}");
                    throw new InvalidOperationException(ex.Message);
                }
            }
        }

        /// <summary>
        /// Interface implementation of <see cref="Current"/>
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Iterates the position of pointer to index
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            position++;
            return (position < _units.Length);
        }

        /// <summary>
        /// Resets the position of pointer to index
        /// </summary>
        public void Reset() => position = -1;
    }
}
