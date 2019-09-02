using System;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A parsed unit based from a game observation message
    /// </summary>
    public class GamebasedUnit : ModelService.Types.Unit
    {
        /// <summary>
        /// Creates an instance of parsed unit based on a Game observation
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public GamebasedUnit(long uid, string owner, string name, double x, double y, params string[] buffs) 
            : base(uid, owner, name, x, y, buffs)
        {
            Initialize();
        }

        /// <summary>
        /// Creates a copy of this unit with the same values except the <see cref="ModelService.Types.Unit"/>
        /// </summary>
        /// <returns></returns>
        public override ModelService.Types.Unit CreateDeepCopy()
        {
            var new_gamebasedunit = new GamebasedUnit(UniqueID, String.Copy(Owner), String.Copy(Name), Position.X, Position.Y, (from buff in Buffs select String.Copy(buff)).ToArray());
            new_gamebasedunit.Health = Health;
            new_gamebasedunit.Energy = Energy;
            new_gamebasedunit.Armor = Armor;
            new_gamebasedunit.Ground_Damage = Ground_Damage;
            new_gamebasedunit.Air_Damage = Air_Damage;

            new_gamebasedunit.Current_Health = Current_Health;
            new_gamebasedunit.Current_Energy = Current_Energy;
            new_gamebasedunit.Current_Armor = Current_Armor;
            new_gamebasedunit.Current_Ground_Damage = Current_Ground_Damage;
            new_gamebasedunit.Current_Air_Damage = Current_Air_Damage;

            return new_gamebasedunit;
        }
    }
}
