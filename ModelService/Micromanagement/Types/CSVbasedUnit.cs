using System;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A parsed unit based from a CSV file
    /// </summary>
    public class CSVbasedUnit : ModelService.Types.Unit
    {
        /// <summary>
        /// The seconds where this unit is found to be engaged in battle
        /// </summary>
        public int Timestamp { get; private set; } = -1;

        /// <summary>
        /// Creates an instance of parsed unit based on a CSV file
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="uid"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public CSVbasedUnit(int timestamp, long uid, string owner, string name, double x, double y, params string[] buffs) 
            : base(uid, owner, name, x, y, buffs)
        {
            Timestamp = timestamp;
            Initialize();
        }

        /// <summary>
        /// Creates a copy of this unit with the same values except the <see cref="ModelService.Types.Unit.Target"/>
        /// </summary>
        /// <returns></returns>
        public override ModelService.Types.Unit CreateDeepCopy()
        {
            var new_csvbasedunit = new CSVbasedUnit(Timestamp, UniqueID, String.Copy(Owner), String.Copy(Name), Position.X, Position.Y, (from buff in Buffs select String.Copy(buff)).ToArray());
            new_csvbasedunit.Health = Health;
            new_csvbasedunit.Energy = Energy;
            new_csvbasedunit.Armor = Armor;
            new_csvbasedunit.Ground_Damage = Ground_Damage;
            new_csvbasedunit.Air_Damage = Air_Damage;

            new_csvbasedunit.Current_Health = Current_Health;
            new_csvbasedunit.Current_Energy = Current_Energy;
            new_csvbasedunit.Current_Armor = Current_Armor;
            new_csvbasedunit.Current_Ground_Damage = Current_Ground_Damage;
            new_csvbasedunit.Current_Air_Damage = Current_Air_Damage;

            return new_csvbasedunit;
        }

        /// <summary>
        /// Returns the name of this unit
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;
    }
}
