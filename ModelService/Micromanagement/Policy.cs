using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        /// <summary>
        /// <para>
        ///     Random-based Targeting Policy targets units randomly based on a uniform distribution.
        ///     It is the fastest targeting policy in exchange of maximizing rewards to destroy opposing
        ///     army. If the current unit can target the given random enemy unit, it will be the set target
        ///     regardless of health, damage/energy, worth, and distance. It also disregards that multiple
        ///     units could target the same unit. In the case that the current enemy unit cannot be targeted, 
        ///     an offset is added until it reaches the same enemy unit, resulting to having no target at all.
        /// </para>
        /// <para>
        ///     This method only affects '<paramref name="focused_army"/>', which means that this is
        ///     the only one to have targets at the end of the policy.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     While it can be said it is the fastest, it can be slow as the other policy when the current
        ///     enemy unit cannot be targeted by the current unit. It's running time is probably O(n^2).
        /// </para>
        /// </remarks>
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void RandomBasedTargetPolicy(ref Army focused_army, Army opposed_army)
        {
            try
            {
                var random = Services.ModelRepositoryService.ModelService.GetModelService().RandomEngine;
                var array_opposed_army = opposed_army.ToArray();
                for (var focused_enumerator = focused_army.GetEnumerator(); focused_enumerator.MoveNext();)
                {
                    int selected_enemy = random.Next(0, array_opposed_army.Length), offset = 0;

                    //Continue to increase the offset if the current selected enemy cannot be targeted
                    //Stop if we return back to the original enemy unit
                    for (bool started = true; true; offset++, started = false)
                    {
                        int offsetted_index = ((offset + selected_enemy) % array_opposed_army.Length);

                        //Check if offsetted index is the same as the original enemy unit
                        //and that this is the second time, not when it started
                        if (!started && (offsetted_index == selected_enemy))
                            break;

                        //Check if the current unit can target the given enemy unit
                        if (focused_enumerator.Current.CanTarget(array_opposed_army[offsetted_index]))
                        {
                            //Set the target since it can be targeted
                            focused_enumerator.Current.SetTarget(array_opposed_army[offsetted_index]);
                            break;
                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"RandomBasedTargetPolicy() [{focused_army.ToString()}\n{opposed_army.ToString()}] -> {ex.Message}");
                System.Diagnostics.Debugger.Break();
                throw new Exception("");
            }
        }

        /// <summary>
        /// <para>
        ///     Priority-based Targeting Policy targets the highest priority enemy unit as near as possible from
        ///     the current unit through using the weighted sum decision model. 80% of the priority is taken into
        ///     account and 20% of the distance is accounted. While it is not as fast as <see cref="RandomBasedTargetPolicy(ref Army, Army)"/>,
        ///     it is probably faster compared to <see cref="ResourceBasedTargetPolicy(ref Army, Army)"/> because it only 
        ///     takes two criteria accounted for making target decision. Like the <see cref="RandomBasedTargetPolicy(ref Army, Army)"/>,
        ///     it does not take account of health, damage, and energy. However, it takes into account the distance and
        ///     only priority in worth - thus excluding mineral, vespene, and supply. It also disregards that multiple
        ///     units could target the same unit. In the case that the current enemy cannot be targeted, it proceed to select
        ///     the next highest priority enemy unit.
        /// </para>
        /// <para>
        ///     This method only affects '<paramref name="focused_army"/>', which means that this is 
        ///     the only one to have targets at the end of the policy.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     The probable running time of this is O(3n^2).
        /// </para>
        /// </remarks>
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void PriorityBasedTargetPolicy(ref Army focused_army, Army opposed_army)
        {
            try
            {
                var array_opposed_army = opposed_army.ToArray();
                for (var focused_enumerator = focused_army.GetEnumerator(); focused_enumerator.MoveNext();)
                {
                    //Create a dictionary to store their weighted value
                    var enemies_weightedvalue = new Dictionary<int, double>();
                    for (int opposed_iterator = 0; opposed_iterator < array_opposed_army.Length; opposed_iterator++)
                        //The value of an enemy unit is 20% of their distance from current unit plus
                        //80% of their priority worth when they are destroyed
                        enemies_weightedvalue.Add(opposed_iterator, ((focused_enumerator.Current.Position.GetDistance(array_opposed_army[opposed_iterator].Position) * 0.20) + (Unit.Values[array_opposed_army[opposed_iterator].Name].Priority) * 0.80));

                    //Sort the dictionary by descending value
                    var enumerator_opposed_army = enemies_weightedvalue.OrderByDescending(enemy_value => enemy_value.Value);

                    //Go through the list of enemies and select the highest one
                    for (var opposed_iterator = enumerator_opposed_army.GetEnumerator(); opposed_iterator.MoveNext();)
                    {
                        //Check if the current enemy can be targeted
                        if (focused_enumerator.Current.CanTarget(array_opposed_army[opposed_iterator.Current.Key]))
                        {
                            //Set the target immediately
                            focused_enumerator.Current.SetTarget(array_opposed_army[opposed_iterator.Current.Key]);
                            break;
                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"RandomBasedTargetPolicy() [{focused_army.ToString()}\n{opposed_army.ToString()}] -> {ex.Message}");
                System.Diagnostics.Debugger.Break();
                throw new Exception("");
            }
        }

        /// <summary>
        /// <para>
        ///     Resource-based Targeting Policy targets the highest worth enemy unit through using
        ///     the weighted sum decision model. 33.5% of mineral worth, 33.5% of vespene worth and 
        ///     33% of supply worth is accounted. It is probably the slowest of the policy since it takes
        ///     three criteria for making target decision. This policy only consider the mineral, vespence, 
        ///     and supply of worth - thus excluding the priority, health, damage, energy, and distance. It
        ///     also disregards that multiple units could target the same unit. In the case that the 
        ///     current enemy cannot be targeted, it proceed to select the next highest worth enemy unit.
        /// </para>
        /// <para>
        ///     This method only affects '<paramref name="focused_army"/>', which means that this is the only 
        ///     one to have targets at the end of the policy
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     The probably running time of this is O(3n^2)
        /// </para>
        /// </remarks>
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void ResourceBasedTargetPolicy(ref Army focused_army, Army opposed_army)
        {
            try
            {
                var array_opposed_army = opposed_army.ToArray();
                for (var focused_enumerator = focused_army.GetEnumerator(); focused_enumerator.MoveNext();)
                {
                    //Create a dictionary to store their weighted value
                    var enemies_weightedvalue = new Dictionary<int, double>();
                    for (int opposed_iterator = 0; opposed_iterator < array_opposed_army.Length; opposed_iterator++)
                        //The value of an enemy unit is 33.5% of mineral worth, 33.5% of vespene worth, 33% of supply worth
                        enemies_weightedvalue.Add(opposed_iterator, Unit.Values[array_opposed_army[opposed_iterator].Name].GetTotalWorth(0, 0.335, 0.335, 0.33));

                    //Sort the dictionary by descending value
                    var enumerator_opposed_army = enemies_weightedvalue.OrderByDescending(enemy_value => enemy_value.Value);

                    //Go through the list of enemies and select the highest one
                    for (var opposed_iterator = enumerator_opposed_army.GetEnumerator(); opposed_iterator.MoveNext();)
                    {
                        //Check if the current enemy can be targeted
                        if (focused_enumerator.Current.CanTarget(array_opposed_army[opposed_iterator.Current.Key]))
                        {
                            //Check if the current enemy has already been previously targeted 
                            if (Unit.GetTargetsOfUnit(focused_enumerator.Current).Contains(array_opposed_army[opposed_iterator.Current.Key].UniqueID))
                                continue;

                            //Set the target immediately
                            focused_enumerator.Current.SetTarget(array_opposed_army[opposed_iterator.Current.Key]);
                            break;
                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"RandomBasedTargetPolicy() [{focused_army.ToString()}\n{opposed_army.ToString()}] -> {ex.Message}");
                System.Diagnostics.Debugger.Break();
                throw new Exception("");
            }
        }
    }
}