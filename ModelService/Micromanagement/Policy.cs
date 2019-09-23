using System;
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
        /// <param name="focused_army"></param>
        /// <param name="opposed_army"></param>
        private void RandomBasedTargetPolicy(Army focused_army, Army opposed_army)
        {
            var array_focused_army = focused_army.ToArray();
            var array_opposed_army = opposed_army.ToArray();

            //Set the oppenents for focused army
            for(int focused_iterator = 0; focused_iterator < array_focused_army.Length; focused_iterator++)
            {
                var minimum_distance = Double.MaxValue;
                var targeted_enemy = -1;
                for(int opposed_iterator = 0; opposed_iterator < array_opposed_army.Length; opposed_iterator++)
                {
                    var current_distance = array_focused_army[focused_iterator].Position.GetDistance(array_opposed_army[opposed_iterator].Position);

                    if(current_distance < minimum_distance)
                    {
                        minimum_distance = current_distance;
                        targeted_enemy = opposed_iterator;
                    }
                }

                //Set the target
                array_focused_army[focused_iterator].SetTarget(array_opposed_army[targeted_enemy]);
            }

            //Return the result where the list is sorted by the farthest and having a high health
            //to nearest and having a low health
            focused_army = ((Army)array_focused_army.OrderByDescending(unit => unit.Position.GetDistance(unit.Target.Position)).ThenByDescending(unit => unit.Current_Health));
        }

        private void PriorityBasedTargetPolicy(Army owned_units, Army enemy_units)
        {

        }

        private void ResourceBasedTargetPolicy(Army owned_units, Army enemy_units)
        {

        }
    }
}