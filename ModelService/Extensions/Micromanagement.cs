using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.Types;

namespace ModelService.Micromanagement
{
    public struct LanchesterVariables
    {
        /// <summary>
        /// The alpha in the lanchester formula
        /// </summary>
        /// <remarks>
        /// a
        /// </remarks>
        public double OwnedArmy_CombatEffectiveness { get; set; }

        /// <summary>
        /// The beta in the lanchester formula
        /// </summary>
        /// <remarks>
        /// B
        /// </remarks>
        public double EnemyArmy_CombatEffectiveness { get; set; }

        /// <summary>
        /// The expected damage that can deal by owned army to enemy army
        /// </summary>
        /// <remarks>
        /// DPF(A, B)
        /// </remarks>
        public double OwnedArmyToEnemyArmy_EffectivePotentialDamage { get; set; }

        /// <summary>
        /// The expected damage that can deal by enemy army to owned army
        /// </summary>
        /// <remarks>
        /// DPF(B, A)
        /// </remarks>
        public double EnemyArmyToOwnedArmy_EffectivePotentialDamage { get; set; }

        /// <summary>
        /// The relative combat effectiveness of owned army to enemy army
        /// </summary>
        /// <remarks>
        /// Ra
        /// </remarks>
        public double OwnedArmy_RelativeEffectiveness { get; set; }

        /// <summary>
        /// The relative combat effectiveness of enemy army to owned army
        /// </summary>
        /// <remarks>
        /// Rb
        /// </remarks>
        public double EnemyArmy_RelativeEffectiveness { get; set; }

        /// <summary>
        /// The basic variables needed for the computation of lanchester equation
        /// </summary>
        /// <param name="owned_army"></param>
        /// <param name="enemy_army"></param>
        public LanchesterVariables(Army owned_army, Army enemy_army)
        {
            //Compute the mean health of both army
            var ownedarmy_meanhealth = owned_army.Average(unit => unit.Current_Health);
            var enemyarmy_meanhealth = enemy_army.Average(unit => unit.Current_Health);

            //Compute the total health of air units from both army
            var ownedarmy_air_totalhealth = owned_army.Sum(unit => unit.Current_Health);
            var enemyarmy_air_totalhealth = enemy_army.Sum(unit => unit.Current_Health);

            //Compute the total health of ground units from both army
            var ownedarmy_ground_totalhealth = owned_army.Sum(unit => unit.Current_Health);
            var enemyarmy_ground_totalhealth = enemy_army.Sum(unit => unit.Current_Health);

            //Compute the mean of effective potential damage of air units from both army
            var ownedarmy_air_meandamage = owned_army.Sum(unit => unit.GetPotentialMaximumDamage().Item1);
            //var enemyarmy_air_meandamage = owned

            //Do the triangular mean first
            throw new NotImplementedException();
        }
    }

    public static class LanchesterBasedPredictionExtensions
    {

    }
}
