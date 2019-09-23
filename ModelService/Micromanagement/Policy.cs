using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        /// <summary>
        /// RandomBasedTargetPolicy is about how likely a unit will survive in a battle.
        /// A unit will survive based on its health and its distance from its nearest target
        /// As such this algorithm is based on the assumption that the higher the health of a unit,
        /// and the farther its distance to an enemy is likely to survive in a battle. This algorithm
        /// only performs operations on <paramref name="focused_army"/>, which means only
        /// the <paramref name="focused_army"/> will only have targets at the end
        /// </summary>
        /// <remarks>
        /// It does consider if the unit can target or not the current enemy
        /// </remarks>
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void RandomBasedTargetPolicy(ref Army focused_army, Army opposed_army)
        {
            //A value for the likely of surviving
            var weighted_focused_army = new Dictionary<string, double>();
            var array_focused_army = focused_army.ToArray();
            var array_opposed_army = opposed_army.ToArray();

            //Set the opponents for focused army while taking notes of weighted value
            for(int focused_iterator = 0; focused_iterator < array_focused_army.Length; focused_iterator++)
            {
#warning Wrong usage of percentages. We need to compute the survavibility based on health and distance

                //70% weight for health criteria 
                weighted_focused_army.Add(array_focused_army[focused_iterator].UniqueID, array_focused_army[focused_iterator].Current_Health * 0.70);
                var minimum_distance = Double.MaxValue;
                var targeted_enemy = -1;

                for(int opposed_iterator = 0; opposed_iterator < array_opposed_army.Length; opposed_iterator++)
                {
                    var current_distance = array_focused_army[focused_iterator].Position.GetDistance(array_opposed_army[opposed_iterator].Position);

                    if (current_distance < minimum_distance)
                    {
                        minimum_distance = current_distance;
                        //If the current enemy can be target by the unit
                        if (array_focused_army[focused_iterator].CanTarget(array_opposed_army[opposed_iterator]))
                            targeted_enemy = opposed_iterator;
                    }
                }
                
                //30% weight for distance
                weighted_focused_army[array_focused_army[focused_iterator].UniqueID] += (minimum_distance * .30);
                //Set target
                if (targeted_enemy != -1)
                    array_focused_army[focused_iterator].SetTarget(array_opposed_army[targeted_enemy]);
            }

            focused_army = array_focused_army.OrderByDescending(unit => weighted_focused_army[unit.UniqueID]).ToArmy();
        }

        /// <summary>
        /// PriorityBasedTargetPolicy is about how likely a unit will survive in a battle.
        /// A unit will survive based on its priority, its health, and its distance from its nearest target.
        /// As such, this algorithm is based on the assumption that the lower the priority number, the higher health
        /// of a unit, and the farther its distance to an enemy is likely to survive in a battle. This algorithm
        /// performs operations on <paramref name="focused_army"/>, which mean only the <paramref name="focused_army"/>
        /// will only have targets at the end
        /// </summary>
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void PriorityBasedTargetPolicy(ref Army focused_army, Army opposed_army)
        {
            //A value for the likely of surviving
            var weighted_focused_army = new Dictionary<string, double>();
            var array_focused_army = focused_army.ToArray();
            var array_opposed_army = opposed_army.ToArray();

            //Set the opponents for focused while taking notes of weighted value
            for(int focused_iterator = 0; focused_iterator < array_focused_army.Length; focused_iterator++)
            {
#warning Wrong usage of percentages. We need to compute the survavibility based on priority, health, and distance
                //70% for priority, 20% for health. Negative priority because less penalty if lower
                var total_initial_value = ((-Unit.Values[array_focused_army[focused_iterator].Name].Priority * .70) + (array_focused_army[focused_iterator].Current_Health * .20));
                weighted_focused_army.Add(array_focused_army[focused_iterator].UniqueID, total_initial_value);
                var minimum_distance = Double.MaxValue;
                var targeted_enemy = -1;

                for(int opposed_iterator = 0; opposed_iterator < array_opposed_army.Length; opposed_iterator++)
                {
                    var current_distance = array_focused_army[focused_iterator].Position.GetDistance(array_opposed_army[opposed_iterator].Position);

                    if(current_distance < minimum_distance)
                    {
                        minimum_distance = current_distance;

                        //If the current enemy can be target by the unit
                        if (array_focused_army[focused_iterator].CanTarget(array_opposed_army[opposed_iterator]))
                            targeted_enemy = opposed_iterator;
                    }
                }

                //10% weight for distance
                weighted_focused_army[array_focused_army[focused_iterator].UniqueID] += (minimum_distance * .10);
                //Set target
                if (targeted_enemy != -1)
                    array_focused_army[focused_iterator].SetTarget(array_opposed_army[targeted_enemy]);
            }

            focused_army = array_focused_army.OrderByDescending(unit => weighted_focused_army[unit.UniqueID]).ToArmy();
        }

        /// <summary>
        /// ResourceBasedTargetPolicy is about how likely a unit will survive in a battle.
        /// A unit will survive based on its <see cref="UnitWorth"/>, health, and its distance from its nearest target.
        /// As such, this algorithm is based on the assumption that the lower the resource worth, the higher health of a unit,
        /// and the farther its distance to an enemy is likely to survive in a battle. This algorithm performs
        /// operations on <paramref name="focused_army"/>, which means only the <paramref name="focused_army"/> will
        /// only have targets at the end
        /// </summary>
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void ResourceBasedTargetPolicy(ref Army focused_army, Army opposed_army)
        {
            //A value for the likely of surviving
            var weighted_focused_army = new Dictionary<string, double>();
            var array_focused_army = focused_army.ToArray();
            var array_opposed_army = opposed_army.ToArray();

            //Set the opponents for focused while taking notes of weighted value
            for(int focused_iterator = 0; focused_iterator < array_focused_army.Length; focused_iterator++)
            {
#warning Wrong usage of percentages. We need to compute the survavibility based on worth, health, and distance
                //70% for worth, 20% for health. Negative worth because less penalty if lower
                var total_initial_value = ((-Unit.Values[array_focused_army[focused_iterator].Name].GetSummaryOfResource() * 70) + (array_focused_army[focused_iterator].Current_Health * .20));
                weighted_focused_army.Add(array_focused_army[focused_iterator].Name, total_initial_value);
                var minimum_distance = Double.MaxValue;
                var targeted_enemy = -1;

                for(int opposed_iterator = 0; opposed_iterator < array_opposed_army.Length; opposed_iterator++)
                {
                    var current_distance = array_focused_army[focused_iterator].Position.GetDistance(array_opposed_army[opposed_iterator].Position);

                    if (current_distance < minimum_distance)
                    {
                        minimum_distance = current_distance;

                        //If the current enemy can be target by the unit
                        if (array_focused_army[focused_iterator].CanTarget(array_opposed_army[opposed_iterator]))
                            targeted_enemy = opposed_iterator;
                    }
                }

                //10% weight for distance
                weighted_focused_army[array_focused_army[focused_iterator].UniqueID] += (minimum_distance * .10);
                //Set target
                if (targeted_enemy != -1)
                    array_focused_army[focused_iterator].SetTarget(array_opposed_army[targeted_enemy]);
            }

            focused_army = array_focused_army.OrderByDescending(unit => weighted_focused_army[unit.UniqueID]).ToArmy();
        }
    }
}