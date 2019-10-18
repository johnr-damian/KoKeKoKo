using ModelService.Micromanagement;
using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    public static class ArmyExtensions
    {
        public static Army ToArmy(this IEnumerable<Unit> units) => new Army(units);

        /// <summary>
        /// <para>
        ///     This method returns a list randomly taken units from the source. The implementation
        ///     of this method is similar to <see cref="Micromanagement.Micromanagement.RandomBasedTargetPolicy(ref Army, Army)"/>.
        ///     As such, it does not consider any properties of <see cref="Unit"/> to select how likely
        ///     this unit survived the battle.
        /// </para>
        /// <para>
        ///     While this method creates a new instance of <see cref="Army"/>, it does not create a new
        ///     instance of <see cref="Unit"/>. This method must be used after a <see cref="TargetPolicy"/>.
        ///     This returns the survived units.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     It can be said that this is the fastest policy to get surviving units.
        ///     The probable running time is O(n) where n is the number of units to be taken.
        /// </para>
        /// </remarks>
        /// <param name="units"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Army RandomlyTake(this IEnumerable<Unit> units, int count)
        {
            //var taken_units = units.ToList();
            //for(int remover = 0; remover < count; remover++)
            //{
            //    try
            //    {
            //        var removed_unit = REngineExtensions.GetRandomGenerator().Next(0, taken_units.Count);
            //        taken_units.RemoveAt(removed_unit);
            //    }
            //    catch (ArgumentOutOfRangeException ex)
            //    {
            //        Console.WriteLine($@"RandomlyTake() [{count}] -> {ex.Message}");
            //        taken_units.RemoveAt(0);
            //    }
            //}

            var random = Services.ModelRepositoryService.ModelService.GetModelService().RandomEngine;

            var taken_units = units.ToArray();
            for(int shuffler = 0; shuffler < taken_units.Length; shuffler++)
            {
                int shuffled_unit = random.Next(0, (taken_units.Length - shuffler));
                var shuffled_element = taken_units[shuffled_unit];
                taken_units[shuffled_unit] = taken_units[shuffler];
                taken_units[shuffler] = shuffled_element;
            }

            return taken_units.Take(count).ToArmy();
        }

        /// <summary>
        /// <para>
        ///     This method returns a list of taken units according to their priority and distance. The implementation
        ///     of this method is similar to <see cref="Micromanagement.Micromanagement.PriorityBasedTargetPolicy(ref Army, Army)"/>.
        ///     As such, it only considers the distance and priority worth of a <see cref="Unit"/> to 
        ///     select how likely this unit survived the battle.
        /// </para>
        /// <para>
        ///     While this method creates a new instance of <see cref="Army"/>, it does not create a new
        ///     instance of <see cref="Unit"/>. This method must be used after a <see cref="TargetPolicy"/>.
        ///     This returns the survived units
        /// </para>
        /// </summary>
        /// <remarks>
        ///     The probable running time is O(n + c) where n is the number of units, and c is
        ///     the number of units to be taken
        /// </remarks>
        /// <param name="units"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Army PriorityTake(this IEnumerable<Unit> units, int count)
        {
            var taken_units = new List<Unit>();
            var units_value = new Dictionary<int, double>();
            var array_units = units.ToList();
            //Create a list units with their worth being destroyed
            for (int iterator = 0; iterator < array_units.Count; iterator++)
                //The value of a unit is 20% of distance and 80% of priority worth
                units_value.Add(iterator, ((array_units[iterator].Position.GetDistance((array_units[iterator].Target == null)? default(Coordinate) : array_units[iterator].Target.Position) * 0.20) + (Unit.Values[array_units[iterator].Name].Priority * 0.80)));

            //Sort by ascending order
            var enumerator_taken_units = units_value.OrderBy(unit_value => unit_value.Value).Take(count);
            for (var enumerator = enumerator_taken_units.GetEnumerator(); enumerator.MoveNext();)
                taken_units.Add(array_units[enumerator.Current.Key]);

            return taken_units.ToArmy();
        }

        /// <summary>
        /// <para>
        ///     This method returns a list of taken units according to their <see cref="CostWorth.GetTotalWorth(double, double, double, double)"/>.
        ///     The implementation of this method is similar to <see cref="Micromanagement.Micromanagement.ResourceBasedTargetPolicy(ref Army, Army)"/>.
        ///     As such, it only considers the minerals, vespene, and supply worth of a <see cref="Unit"/> to
        ///     select how likely this unit survived the battle.
        /// </para>
        /// <para>
        ///     While this method creates a new instance of <see cref="Army"/>, it does not create a 
        ///     new instance of <see cref="Unit"/>. This method must be used after a <see cref="TargetPolicy"/>.
        ///     This returns the survived units
        /// </para>
        /// </summary>
        /// <param name="units"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Army ResourceTake(this IEnumerable<Unit> units, int count)
        {
            var taken_units = new List<Unit>();
            var units_value = new Dictionary<int, double>();
            var array_units = units.ToList();
            //Create a list of units with their worth being destroyed
            for (int iterator = 0; iterator < array_units.Count; iterator++)
                //The value of a unit are 33.5% Mineral, 33.5% Vespene, and 33% of Supply
                units_value.Add(iterator, Unit.Values[array_units[iterator].Name].GetTotalWorth(0, 0.335, 0.335, 0.33));

            //Sort by ascending order
            var enumerator_taken_units = units_value.OrderBy(unit_value => unit_value.Value).Take(count);
            for (var enumerator = enumerator_taken_units.GetEnumerator(); enumerator.MoveNext();)
                taken_units.Add(array_units[enumerator.Current.Key]);

            return taken_units.ToArmy();
        }

        /// <summary>
        /// <para>
        ///     This method deals an aggregated damage to the <see cref="Unit.Target"/> of each 
        ///     unit in the army. This aggregated damage is randomly generated using <see cref="REngineExtensions.GetTriangularRandomNumber(RDotNet.REngine, int, double, double, double)"/>
        ///     The damage that can be dealt is between the <see cref="Unit.GetMinimumPotentialDamage(Unit)"/> and the
        ///     <see cref="Unit.GetMaximumPotentialDamage(Unit)"/> that depends on the type of the current target of unit.
        ///     It is aggregated because it sums a number of times the generated random damage based 
        ///     on <paramref name="time_to_kill"/>.
        /// </para>
        /// <para>
        ///     Suppose that <paramref name="time_to_kill"/> is 5, it means the unit must deal a damage to its target
        ///     5 times. In order to save computation time, the triangular distribution generates 5 random numbers, adds 
        ///     these numbers, and return the sum of the damage.
        /// </para>
        /// </summary>
        /// <param name="units"></param>
        /// <param name="time_to_kill"></param>
        public static void DealDamageToTarget(this IEnumerable<Unit> units, int time_to_kill)
        {
            var random = Services.ModelRepositoryService.ModelService.GetModelService();
            for (var enumerator = units.GetEnumerator(); enumerator.MoveNext();)
            {
                if (enumerator.Current.Target == null)
                    continue;
                double minimum_and_mode = Unit.GetMinimumPotentialDamage(enumerator.Current);
                var damage_to_deal = random.GetTriangularRandom(time_to_kill, minimum_and_mode, Unit.GetMaximumPotentialDamage(enumerator.Current), minimum_and_mode).Sum();
                enumerator.Current.SimpleAttackToTarget(damage_to_deal);
            }
        }

        /// <summary>
        /// <para>
        ///     
        /// </para>
        /// </summary>
        /// <param name="units"></param>
        /// <param name="ability_probability"></param>
        public static void DealDamageToTarget(this IEnumerable<Unit> units, double ability_probability)
        {
            for (var enumerator = units.GetEnumerator(); enumerator.MoveNext();)
                enumerator.Current.ComplexAttackToTarget(ability_probability);
        }
    }
}
