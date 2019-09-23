using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.Types;

namespace ModelService.Micromanagement
{
    /// <summary>
    /// 
    /// </summary>
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
            var ownedarmy_air_meandamage = owned_army.GetLanchesterMeanTriangularAirDamage();
            var enemyarmy_air_meandamage = enemy_army.GetLanchesterMeanTriangularAirDamage();

            //Compute the mean of effective potential damage of ground units from both army
            var ownedarmy_ground_meandamage = owned_army.GetLanchesterMeanTriangularGroundDamage();
            var enemyarmy_ground_meandamage = enemy_army.GetLanchesterMeanTriangularGroundDamage();

            //Compute the OwnedArmyToEnemyArmy_EffectivePotentialDamage
            //known as DPF(A, B) = (((DPFa(A) * HPa(B)) + (DPFg(A) * HPg(B))) / (HPa(B) + HPg(B)))
            OwnedArmyToEnemyArmy_EffectivePotentialDamage = (((ownedarmy_air_meandamage * enemyarmy_air_totalhealth) + (ownedarmy_ground_meandamage * enemyarmy_ground_totalhealth)) / (enemyarmy_air_totalhealth + enemyarmy_ground_totalhealth));

            //Compute the EnemyArmyToOwnedArmy_EffectivePotentialDamage
            //known as DPF(B, A) = (((DPFa(B) * HPa(A)) + (DPFg(B) * HPg(A))) / (HPa(A) + HPg(A)))
            EnemyArmyToOwnedArmy_EffectivePotentialDamage = (((enemyarmy_air_meandamage * ownedarmy_air_totalhealth) + (enemyarmy_ground_meandamage * ownedarmy_ground_totalhealth)) / (ownedarmy_air_totalhealth + ownedarmy_ground_totalhealth));

            //Can now compute the effectiveness
            //Compute the combat effectiveness of own army
            //known as alpha = DPF(B, A) / HP(A)
            OwnedArmy_CombatEffectiveness = EnemyArmyToOwnedArmy_EffectivePotentialDamage / ownedarmy_meanhealth;

            //Compute the combat effectiveness of enemy army
            //known as beta = DPF(A, B) / HP(B)
            EnemyArmy_CombatEffectiveness = OwnedArmyToEnemyArmy_EffectivePotentialDamage / enemyarmy_meanhealth;

            //Can now compute the relative effectiveness
            //Compute the relative effectiveness of own army
            //known as Ra = Sqrt(alpha / beta)
            OwnedArmy_RelativeEffectiveness = Math.Sqrt(OwnedArmy_CombatEffectiveness / EnemyArmy_CombatEffectiveness);

            //Compute the relative effectiveness of eenmy army
            //known as Rb = Sqrt(beta / alpha)
            EnemyArmy_RelativeEffectiveness = Math.Sqrt(EnemyArmy_CombatEffectiveness / OwnedArmy_CombatEffectiveness);
        }
    }
}
