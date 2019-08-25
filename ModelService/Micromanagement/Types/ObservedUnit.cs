using System;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A parsed unit based on a string from game observation
    /// </summary>
    public class ObservedUnit : Unit
    {
        /// <summary>
        /// A unique identifier of this unit
        /// </summary>
        public int ID { get; private set; } = 0;

        /// <summary>
        /// Creates an instance of parsed unit based on game observation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public ObservedUnit(int id, string owner, string name, double x, double y, params string[] buffs)
            : base(owner, name, x, y, buffs)
        {
            ID = id;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ObservedUnit"/> with the same values
        /// </summary>
        /// <returns></returns>
        public override Unit CreateDeepCopy()
        {
            var new_observedunit = new ObservedUnit(ID, String.Copy(Owner), String.Copy(Name), Position.X, Position.Y, (from buff in Buffs select String.Copy(buff)).ToArray());
            new_observedunit.Simulated_Health = Simulated_Health;
            new_observedunit.Simulated_Energy = Simulated_Energy;
            new_observedunit.Target = Target;

            return new_observedunit;
        }
    }
}
