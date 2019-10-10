using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.Types;

namespace ModelService.Micromanagement
{
    /// <summary>
    /// The result of combat between two armies using the Lanchester formula
    /// </summary>
    public struct LanchesterCombatResult
    {
        #region Combat Properties
        /// <summary>
        /// The number of units of owned army
        /// </summary>
        private int OwnedArmy_Count { get; set; }

        /// <summary>
        /// The number of units of enemy army
        /// </summary>
        private int EnemyArmy_Count { get; set; }

        /// <summary>
        /// The alpha variable in the Lanchester formula
        /// </summary>
        /// <remarks>
        /// alpha = DPF(B, A) / HP(A)
        /// </remarks>
        private double OwnedArmy_CombatEffectiveness { get; set; }

        /// <summary>
        /// The beta variable in the Lanchester formula
        /// </summary>
        /// <remarks>
        /// beta = DPF(A, B) / HP(B)
        /// </remarks>
        private double EnemyArmy_CombatEffectiveness { get; set; }

        /// <summary>
        /// The Ra variable in the Lanchester formula
        /// </summary>
        /// <remarks>
        /// Ra = Sqrt(alpha / beta)
        /// </remarks>
        private double OwnedArmy_RelativeCombatEffectiveness { get; set; }

        /// <summary>
        /// The Rb variable in the Lanchester formula
        /// </summary>
        /// <remarks>
        /// Rb = Sqrt(beta / alpha)
        /// </remarks>
        private double EnemyArmy_RelativeCombatEffectiveness { get; set; }

        /// <summary>
        /// The DPF(A, B) in the Lanchester formula. It is the expected damage 
        /// that can deal by the owned army towards to enemy army
        /// </summary>
        /// <remarks>
        /// DPF(A, B) = (((DPFa(A) * HPa(B)) + (DPFg(A) * HPg(B))) / (HPa(B) + HPg(B)))
        /// </remarks>
        private double OwnedArmy_EffectivePotentialDamage { get; set; }

        /// <summary>
        /// The DPF(B, A) in the Lanchester formula. It is the expected damage
        /// that can deal by the enemy army towards to owned army
        /// </summary>
        /// <remarks>
        /// DPF(B, A) = (((DPFa(B) * HPa(A)) + (DPFg(B) * HPg(A))) / (HPa(A) + HPg(A)))
        /// </remarks>
        private double EnemyArmy_EffectivePotentialDamage { get; set; }
        #endregion

        /// <summary>
        /// Computes combat results of both army using the Lanchester formula
        /// </summary>
        /// <param name="owned_army"></param>
        /// <param name="enemy_army"></param>
        public LanchesterCombatResult(Army owned_army, Army enemy_army)
        {
            //Get the cardinality of both army
            OwnedArmy_Count = owned_army.Count();
            EnemyArmy_Count = enemy_army.Count();

            //Compute the mean health of both army
            var ownedarmy_meanhealth = owned_army.Average(unit => unit.Current_Health);
            if (Double.IsNaN(ownedarmy_meanhealth) || Double.IsInfinity(ownedarmy_meanhealth))
                ownedarmy_meanhealth = 0;
            var enemyarmy_meanhealth = enemy_army.Average(unit => unit.Current_Health);
            if (Double.IsNaN(enemyarmy_meanhealth) || Double.IsInfinity(enemyarmy_meanhealth))
                enemyarmy_meanhealth = 0;

            //Compute the total health of both army's air units
            var ownedarmy_airtotalhealth = owned_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(ownedarmy_airtotalhealth) || Double.IsInfinity(ownedarmy_airtotalhealth))
                ownedarmy_airtotalhealth = 0;
            var enemyarmy_airtotalhealth = enemy_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(enemyarmy_airtotalhealth) || Double.IsInfinity(enemyarmy_airtotalhealth))
                enemyarmy_airtotalhealth = 0;

            //Compute the total health of both army's ground units
            var ownedarmy_groundtotalhealth = owned_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(ownedarmy_groundtotalhealth) || Double.IsInfinity(ownedarmy_groundtotalhealth))
                ownedarmy_groundtotalhealth = 0;
            var enemyarmy_groundtotalhealth = enemy_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(enemyarmy_groundtotalhealth) || Double.IsInfinity(enemyarmy_groundtotalhealth))
                enemyarmy_groundtotalhealth = 0;

            //Compute the potential mean damage of both army's air units
            var ownedarmy_airminimumpotential = owned_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialAirDamage(unit));
            if (Double.IsNaN(ownedarmy_airminimumpotential) || Double.IsInfinity(ownedarmy_airminimumpotential))
                ownedarmy_airminimumpotential = 0;
            var ownedarmy_airmaximumpotential = owned_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialAirDamage(unit));
            if (Double.IsNaN(ownedarmy_airmaximumpotential) || Double.IsInfinity(ownedarmy_airmaximumpotential))
                ownedarmy_airmaximumpotential = 0;
            var ownedarmy_airmeandamage = (((2 * ownedarmy_airminimumpotential) + ownedarmy_airmaximumpotential) / 3);
            if (Double.IsNaN(ownedarmy_airmeandamage) || Double.IsInfinity(ownedarmy_airmeandamage))
                ownedarmy_airmeandamage = 0;
            var enemyarmy_airminimumpotential = enemy_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialAirDamage(unit));
            if (Double.IsNaN(enemyarmy_airminimumpotential) || Double.IsInfinity(enemyarmy_airminimumpotential))
                enemyarmy_airminimumpotential = 0;
            var enemyarmy_airmaximumpotential = enemy_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialAirDamage(unit));
            if (Double.IsNaN(enemyarmy_airmaximumpotential) || Double.IsInfinity(enemyarmy_airmaximumpotential))
                enemyarmy_airmaximumpotential = 0;
            var enemyarmy_airmeandamage = (((2 * enemyarmy_airminimumpotential) + enemyarmy_airmaximumpotential) / 3);
            if (Double.IsNaN(enemyarmy_airmeandamage) || Double.IsInfinity(enemyarmy_airmeandamage))
                enemyarmy_airmeandamage = 0;

            //Compute the potential mean damage of both army's ground units
            var ownedarmy_groundminimumpotential = owned_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialGroundDamage(unit));
            if (Double.IsNaN(ownedarmy_groundminimumpotential) || Double.IsInfinity(ownedarmy_groundminimumpotential))
                ownedarmy_groundminimumpotential = 0;
            var ownedarmy_groundmaximumpotential = owned_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialGroundDamage(unit));
            if (Double.IsNaN(ownedarmy_groundmaximumpotential) || Double.IsInfinity(ownedarmy_groundmaximumpotential))
                ownedarmy_groundmaximumpotential = 0;
            var ownedarmy_groundmeandamage = (((2 * ownedarmy_groundminimumpotential) + ownedarmy_groundmaximumpotential) / 3);
            if (Double.IsNaN(ownedarmy_groundmeandamage) || Double.IsInfinity(ownedarmy_groundmeandamage))
                ownedarmy_groundmeandamage = 0;
            var enemyarmy_groundminimumpotential = enemy_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialGroundDamage(unit));
            if (Double.IsNaN(enemyarmy_groundminimumpotential) || Double.IsInfinity(enemyarmy_groundminimumpotential))
                enemyarmy_groundminimumpotential = 0;
            var enemyarmy_groundmaximumpotential = enemy_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialGroundDamage(unit));
            if (Double.IsNaN(enemyarmy_groundmaximumpotential) || Double.IsInfinity(enemyarmy_groundmaximumpotential))
                enemyarmy_groundmaximumpotential = 0;
            var enemyarmy_groundmeandamage = (((2 * enemyarmy_groundminimumpotential) + enemyarmy_groundmaximumpotential) / 3);
            if (Double.IsNaN(enemyarmy_groundmeandamage) || Double.IsInfinity(enemyarmy_groundmeandamage))
                enemyarmy_groundmeandamage = 0;

            //Compute the effective potential damage of both army
            OwnedArmy_EffectivePotentialDamage = (((ownedarmy_airmeandamage * enemyarmy_airtotalhealth) + (ownedarmy_groundmeandamage * enemyarmy_groundtotalhealth)) / (enemyarmy_airtotalhealth + enemyarmy_groundtotalhealth));
            if (Double.IsNaN(OwnedArmy_EffectivePotentialDamage) || Double.IsInfinity(OwnedArmy_EffectivePotentialDamage))
                OwnedArmy_EffectivePotentialDamage = 0;
            EnemyArmy_EffectivePotentialDamage = (((enemyarmy_airmeandamage * ownedarmy_airtotalhealth) + (enemyarmy_groundmeandamage * ownedarmy_groundtotalhealth)) / (ownedarmy_airtotalhealth + ownedarmy_groundtotalhealth));
            if (Double.IsNaN(EnemyArmy_EffectivePotentialDamage) || Double.IsInfinity(EnemyArmy_EffectivePotentialDamage))
                EnemyArmy_EffectivePotentialDamage = 0;

            //Compute the combat effectiveness of both army
            OwnedArmy_CombatEffectiveness = (EnemyArmy_EffectivePotentialDamage / ownedarmy_meanhealth);
            if (Double.IsNaN(OwnedArmy_CombatEffectiveness) || Double.IsInfinity(OwnedArmy_CombatEffectiveness))
                OwnedArmy_CombatEffectiveness = 0;
            EnemyArmy_CombatEffectiveness = (OwnedArmy_EffectivePotentialDamage / enemyarmy_meanhealth);
            if (Double.IsNaN(EnemyArmy_CombatEffectiveness) || Double.IsInfinity(EnemyArmy_CombatEffectiveness))
                EnemyArmy_CombatEffectiveness = 0;

            //Compute the relative combat effectiveness of both army
            OwnedArmy_RelativeCombatEffectiveness = Math.Sqrt(OwnedArmy_CombatEffectiveness / EnemyArmy_CombatEffectiveness);
            if (Double.IsNaN(OwnedArmy_RelativeCombatEffectiveness) || Double.IsInfinity(OwnedArmy_RelativeCombatEffectiveness))
                OwnedArmy_RelativeCombatEffectiveness = 0;
            EnemyArmy_RelativeCombatEffectiveness = Math.Sqrt(EnemyArmy_CombatEffectiveness / OwnedArmy_CombatEffectiveness);
            if (Double.IsNaN(EnemyArmy_RelativeCombatEffectiveness) || Double.IsInfinity(EnemyArmy_RelativeCombatEffectiveness))
                EnemyArmy_RelativeCombatEffectiveness = 0;
        }

        /// <summary>
        /// The winner in combat between the two army
        /// </summary>
        public enum CombatWinner
        {
            /// <summary>
            /// The army controlled by KoKeKoKo won
            /// </summary>
            Owned_Army,

            /// <summary>
            /// Both army resulted to lost
            /// </summary>
            Draw,

            /// <summary>
            /// The army controlled by the opposing agent won
            /// </summary>
            Enemy_Army
        }

        /// <summary>
        /// Checks the <see cref="LanchesterCombatResult.OwnedArmy_RelativeCombatEffectiveness"/>
        /// and returns which army won in the combat
        /// </summary>
        /// <param name="combat_result"></param>
        /// <returns></returns>
        public static CombatWinner GetCombatWinner(LanchesterCombatResult combat_result)
        {
            CombatWinner combat_winner = CombatWinner.Draw;

            try
            {
                var relative_cardinality = (combat_result.OwnedArmy_Count / combat_result.EnemyArmy_Count);

                //Owned Army Loss
                if (relative_cardinality < combat_result.OwnedArmy_RelativeCombatEffectiveness)
                    combat_winner = CombatWinner.Enemy_Army;
                //Owned Army Won
                else if (relative_cardinality > combat_result.OwnedArmy_RelativeCombatEffectiveness)
                    combat_winner = CombatWinner.Owned_Army;
                //Draw
                else
                    combat_winner = CombatWinner.Draw;
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GetCombatWinner() -> {ex.Message}");
                combat_winner = CombatWinner.Draw;
            }

            return combat_winner;
        }

        /// <summary>
        /// Gets the surviving units of the army that won the combat
        /// </summary>
        /// <param name="combat_result"></param>
        /// <param name="combat_winner"></param>
        /// <returns></returns>
        public static int GetSurvivingUnits(LanchesterCombatResult combat_result, CombatWinner combat_winner)
        {
            double surviving_units = 0;

            try
            {
                switch(combat_winner)
                {
                    case CombatWinner.Owned_Army:
                        surviving_units = Math.Sqrt(Math.Abs(Math.Pow(combat_result.OwnedArmy_Count, 2) - ((combat_result.OwnedArmy_CombatEffectiveness / combat_result.EnemyArmy_CombatEffectiveness) * Math.Pow(combat_result.EnemyArmy_Count, 2))));
                        break;
                    case CombatWinner.Draw:
                        surviving_units = 0;
                        break;
                    case CombatWinner.Enemy_Army:
                        surviving_units = Math.Sqrt(Math.Abs(Math.Pow(combat_result.EnemyArmy_Count, 2) - ((combat_result.EnemyArmy_CombatEffectiveness / combat_result.OwnedArmy_CombatEffectiveness) * Math.Pow(combat_result.OwnedArmy_Count, 2))));
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GetSurvivingUnits() -> {ex.Message}");
                surviving_units = 0;
            }

            return Convert.ToInt32(surviving_units);
        }
    }

    /// <summary>
    /// The result of combat between two armies using the Static formula
    /// </summary>
    public struct StaticCombatResult
    {
        #region Combat Properties
        /// <summary>
        /// The t(B, A) in the Static formula. It is the expected time to destroy
        /// the enemy army by computing MAX(tair(B, A), tground(B, A)).
        /// </summary>
        private double OwnedArmy_TimeToKillEnemyArmy { get; set; }

        /// <summary>
        /// The t(A, B) in the Static formula. It is the expected time to destroy
        /// the owned army by computing MAX(tair(A, B), tground(A, B)).
        /// </summary>
        private double EnemyArmy_TimeToKillOwnedArmy { get; set; }

        /// <summary>
        /// The allotted time for combat. It is computed by
        /// MIN(<see cref="OwnedArmy_TimeToKillEnemyArmy"/>, <see cref="EnemyArmy_TimeToKillOwnedArmy"/>).
        /// </summary>
        private double TimeToKill { get; set; } 
        #endregion

        /// <summary>
        /// Computes the time-related properties for combat using the Static formula
        /// </summary>
        /// <param name="owned_army"></param>
        /// <param name="enemy_army"></param>
        public StaticCombatResult(Army owned_army, Army enemy_army)
        {
            //Compute the total health of both army's air units
            var ownedarmy_airtotalhealth = owned_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(ownedarmy_airtotalhealth) || Double.IsInfinity(ownedarmy_airtotalhealth))
                ownedarmy_airtotalhealth = 0;
            var enemyarmy_airtotalhealth = enemy_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(enemyarmy_airtotalhealth) || Double.IsInfinity(enemyarmy_airtotalhealth))
                enemyarmy_airtotalhealth = 0;

            //Compute the total health of both army's ground units
            var ownedarmy_groundtotalhealth = owned_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(ownedarmy_groundtotalhealth) || Double.IsInfinity(ownedarmy_groundtotalhealth))
                ownedarmy_groundtotalhealth = 0;
            var enemyarmy_groundtotalhealth = enemy_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => unit.Current_Health);
            if (Double.IsNaN(enemyarmy_groundtotalhealth) || Double.IsInfinity(enemyarmy_groundtotalhealth))
                enemyarmy_groundtotalhealth = 0;

            //Compute the potential mean damage of both army's air units
            var ownedarmy_airminimumpotential = owned_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialAirDamage(unit));
            if (Double.IsNaN(ownedarmy_airminimumpotential) || Double.IsInfinity(ownedarmy_airminimumpotential))
                ownedarmy_airminimumpotential = 0;
            var ownedarmy_airmaximumpotential = owned_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialAirDamage(unit));
            if (Double.IsNaN(ownedarmy_airmaximumpotential) || Double.IsInfinity(ownedarmy_airmaximumpotential))
                ownedarmy_airmaximumpotential = 0;
            var ownedarmy_airmeandamage = (((2 * ownedarmy_airminimumpotential) + ownedarmy_airmaximumpotential) / 3);
            if (Double.IsNaN(ownedarmy_airmeandamage) || Double.IsInfinity(ownedarmy_airmeandamage))
                ownedarmy_airmeandamage = 0;
            var enemyarmy_airminimumpotential = enemy_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialAirDamage(unit));
            if (Double.IsNaN(enemyarmy_airminimumpotential) || Double.IsInfinity(enemyarmy_airminimumpotential))
                enemyarmy_airminimumpotential = 0;
            var enemyarmy_airmaximumpotential = enemy_army.Where(unit => Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialAirDamage(unit));
            if (Double.IsNaN(enemyarmy_airmaximumpotential) || Double.IsInfinity(enemyarmy_airmaximumpotential))
                enemyarmy_airmaximumpotential = 0;
            var enemyarmy_airmeandamage = (((2 * enemyarmy_airminimumpotential) + enemyarmy_airmaximumpotential) / 3);
            if (Double.IsNaN(enemyarmy_airmeandamage) || Double.IsInfinity(enemyarmy_airmeandamage))
                enemyarmy_airmeandamage = 0;

            //Compute the potential mean damage of both army's ground units
            var ownedarmy_groundminimumpotential = owned_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialGroundDamage(unit));
            if (Double.IsNaN(ownedarmy_groundminimumpotential) || Double.IsInfinity(ownedarmy_groundminimumpotential))
                ownedarmy_groundminimumpotential = 0;
            var ownedarmy_groundmaximumpotential = owned_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialGroundDamage(unit));
            if (Double.IsNaN(ownedarmy_groundmaximumpotential) || Double.IsInfinity(ownedarmy_groundmaximumpotential))
                ownedarmy_groundmaximumpotential = 0;
            var ownedarmy_groundmeandamage = (((2 * ownedarmy_groundminimumpotential) + ownedarmy_groundmaximumpotential) / 3);
            if (Double.IsNaN(ownedarmy_groundmeandamage) || Double.IsInfinity(ownedarmy_groundmeandamage))
                ownedarmy_groundmeandamage = 0;
            var enemyarmy_groundminimumpotential = enemy_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMinimumPotentialGroundDamage(unit));
            if (Double.IsNaN(enemyarmy_groundminimumpotential) || Double.IsInfinity(enemyarmy_groundminimumpotential))
                enemyarmy_groundminimumpotential = 0;
            var enemyarmy_groundmaximumpotential = enemy_army.Where(unit => !Unit.Definitions[unit.Name].IsFlying_Unit).Sum(unit => Unit.GetMaximumPotentialGroundDamage(unit));
            if (Double.IsNaN(enemyarmy_groundmaximumpotential) || Double.IsInfinity(enemyarmy_groundmaximumpotential))
                enemyarmy_groundmaximumpotential = 0;
            var enemyarmy_groundmeandamage = (((2 * enemyarmy_groundminimumpotential) + enemyarmy_groundmaximumpotential) / 3);
            if (Double.IsNaN(enemyarmy_groundmeandamage) || Double.IsInfinity(enemyarmy_groundmeandamage))
                enemyarmy_groundmeandamage = 0;

            //Compute the aggregated air mean damage of both army
            var ownedarmy_aggregatedairminimumpotential = owned_army.Sum(unit => Unit.GetMinimumPotentialAirDamage(unit));
            if (Double.IsNaN(ownedarmy_aggregatedairminimumpotential) || Double.IsInfinity(ownedarmy_aggregatedairminimumpotential))
                ownedarmy_aggregatedairminimumpotential = 0;
            var ownedarmy_aggregatedairmaximumpotential = owned_army.Sum(unit => Unit.GetMaximumPotentialAirDamage(unit));
            if (Double.IsNaN(ownedarmy_aggregatedairmaximumpotential) || Double.IsInfinity(ownedarmy_aggregatedairmaximumpotential))
                ownedarmy_aggregatedairmaximumpotential = 0;
            var ownedarmy_aggreatedairmeandamage = (((2 * ownedarmy_aggregatedairminimumpotential) + ownedarmy_aggregatedairmaximumpotential) / 3);
            if (Double.IsNaN(ownedarmy_aggreatedairmeandamage) || Double.IsInfinity(ownedarmy_aggreatedairmeandamage))
                ownedarmy_aggreatedairmeandamage = 0;
            var enemyarmy_aggregatedairminimumpotential = enemy_army.Sum(unit => Unit.GetMinimumPotentialAirDamage(unit));
            if (Double.IsNaN(enemyarmy_aggregatedairminimumpotential) || Double.IsInfinity(enemyarmy_aggregatedairminimumpotential))
                enemyarmy_aggregatedairminimumpotential = 0;
            var enemyarmy_aggregatedairmaximumpotential = enemy_army.Sum(unit => Unit.GetMaximumPotentialAirDamage(unit));
            if (Double.IsNaN(enemyarmy_aggregatedairmaximumpotential) || Double.IsInfinity(enemyarmy_aggregatedairmaximumpotential))
                enemyarmy_aggregatedairmaximumpotential = 0;
            var enemyarmy_aggreatedairmeandamage = (((2 * enemyarmy_aggregatedairminimumpotential) + enemyarmy_aggregatedairmaximumpotential) / 3);
            if (Double.IsNaN(enemyarmy_aggreatedairmeandamage) || Double.IsInfinity(enemyarmy_aggreatedairmeandamage))
                enemyarmy_aggreatedairmeandamage = 0;

            //Compute the aggregated ground mean damage of both army
            var ownedarmy_aggregatedgroundminimumpotential = owned_army.Sum(unit => Unit.GetMinimumPotentialGroundDamage(unit));
            if (Double.IsNaN(ownedarmy_aggregatedgroundminimumpotential) || Double.IsInfinity(ownedarmy_aggregatedgroundminimumpotential))
                ownedarmy_aggregatedgroundminimumpotential = 0;
            var ownedarmy_aggregatedgroundmaximumpotential = owned_army.Sum(unit => Unit.GetMaximumPotentialGroundDamage(unit));
            if (Double.IsNaN(ownedarmy_aggregatedgroundmaximumpotential) || Double.IsInfinity(ownedarmy_aggregatedgroundmaximumpotential))
                ownedarmy_aggregatedgroundmaximumpotential = 0;
            var ownedarmy_aggregatedgroundmeandamage = (((2 * ownedarmy_aggregatedgroundminimumpotential) + ownedarmy_aggregatedgroundmaximumpotential) / 3);
            if (Double.IsNaN(ownedarmy_aggregatedgroundmeandamage) || Double.IsInfinity(ownedarmy_aggregatedgroundmeandamage))
                ownedarmy_aggregatedgroundmeandamage = 0;
            var enemyarmy_aggregatedgroundminimumpotential = enemy_army.Sum(unit => Unit.GetMinimumPotentialGroundDamage(unit));
            if (Double.IsNaN(enemyarmy_aggregatedgroundminimumpotential) || Double.IsInfinity(enemyarmy_aggregatedgroundminimumpotential))
                enemyarmy_aggregatedgroundminimumpotential = 0;
            var enemyarmy_aggregatedgroundmaximumpotential = enemy_army.Sum(unit => Unit.GetMaximumPotentialGroundDamage(unit));
            if (Double.IsNaN(enemyarmy_aggregatedgroundmaximumpotential) || Double.IsInfinity(enemyarmy_aggregatedgroundmaximumpotential))
                enemyarmy_aggregatedgroundmaximumpotential = 0;
            var enemyarmy_aggregatedgroundmeandamage = (((2 * enemyarmy_aggregatedgroundminimumpotential) + enemyarmy_aggregatedgroundmaximumpotential) / 3);
            if (Double.IsNaN(enemyarmy_aggregatedgroundmeandamage) || Double.IsInfinity(enemyarmy_aggregatedgroundmeandamage))
                enemyarmy_aggregatedgroundmeandamage = 0;

            //Compute the time to kill air units of both army
            var ownedarmy_timetokillairunits = (enemyarmy_airtotalhealth / (ownedarmy_airmeandamage + ownedarmy_aggreatedairmeandamage));
            if (Double.IsNaN(ownedarmy_timetokillairunits) || Double.IsInfinity(ownedarmy_timetokillairunits))
                ownedarmy_timetokillairunits = 0;
            var enemyarmy_timetokillairunits = (ownedarmy_airtotalhealth / (enemyarmy_airmeandamage + enemyarmy_aggreatedairmeandamage));
            if (Double.IsNaN(enemyarmy_timetokillairunits) || Double.IsInfinity(enemyarmy_timetokillairunits))
                enemyarmy_timetokillairunits = 0;

            //Compute the time to kill ground units of both army
            var ownedarmy_timetokillgroundunits = (enemyarmy_groundtotalhealth / (ownedarmy_groundmeandamage + ownedarmy_aggregatedgroundmeandamage));
            if (Double.IsNaN(ownedarmy_timetokillgroundunits) || Double.IsInfinity(ownedarmy_timetokillgroundunits))
                ownedarmy_timetokillgroundunits = 0;
            var enemyarmy_timetokillgroundunits = (ownedarmy_groundtotalhealth / (enemyarmy_groundmeandamage + enemyarmy_aggregatedgroundmeandamage));
            if (Double.IsNaN(enemyarmy_timetokillgroundunits) || Double.IsInfinity(enemyarmy_timetokillgroundunits))
                enemyarmy_timetokillgroundunits = 0;

            //Compute the time to kill the army
            OwnedArmy_TimeToKillEnemyArmy = Math.Max(ownedarmy_timetokillairunits, ownedarmy_timetokillgroundunits);
            EnemyArmy_TimeToKillOwnedArmy = Math.Max(enemyarmy_timetokillairunits, enemyarmy_timetokillgroundunits);

            //Compute the global time for combat
            TimeToKill = Math.Min(OwnedArmy_TimeToKillEnemyArmy, EnemyArmy_TimeToKillOwnedArmy);
        }

        /// <summary>
        /// Returns the combat time
        /// </summary>
        /// <param name="combat_result"></param>
        /// <returns></returns>
        public static int GetCombatTime(StaticCombatResult combat_result) => Convert.ToInt32(Math.Ceiling(combat_result.TimeToKill));
    }

    /// <summary>
    /// The result of combat between two armies using the Dynamic formula
    /// </summary>
    public struct DynamicCombatResult
    {
        #region Combat Properties
        /// <summary>
        /// The TIMEToKillUnit(B, A, DPF) in the Dynamic formula. It is the expected
        /// time to destroy the target for each unit of owned army
        /// </summary>
        private Dictionary<string, double> OwnedArmy_TimeToKillEnemyArmy { get; set; }

        /// <summary>
        /// The TIMEToKillUnit(A, B, DPF) in the Dynamic formula. It is the expected
        /// time to destroy the target for each unit of enemy army.
        /// </summary>
        private Dictionary<string, double> EnemyArmy_TimeToKillOwnedArmy { get; set; }

        /// <summary>
        /// The allotted time for combat. It is computed by 
        /// MIN(MAX(<see cref="OwnedArmy_TimeToKillEnemyArmy"/>, MAX(<see cref="EnemyArmy_TimeToKillOwnedArmy"/>).
        /// </summary>
        private double TimeToKill { get; set; } 
        #endregion

        /// <summary>
        /// Computes the time-related properties for combat using the Dynamic formula
        /// </summary>
        /// <param name="owned_army"></param>
        /// <param name="enemy_army"></param>
        public DynamicCombatResult(Army owned_army, Army enemy_army)
        {
            //Compute the time to kill for each unit's target of both army
            OwnedArmy_TimeToKillEnemyArmy = new Dictionary<string, double>();
            for(var enumerator = owned_army.GetEnumerator(); enumerator.MoveNext();)
            {
                if (enumerator.Current.Target != null)
                {
                    //Compute the potential mean damage to target
                    var potentialmeandamage = (((2 * Unit.GetMinimumPotentialDamage(enumerator.Current)) + Unit.GetMaximumPotentialDamage(enumerator.Current)) / 3);

                    //Compute the time to kill target
                    var timetokill = enumerator.Current.Target.Current_Health / potentialmeandamage;
                    if (Double.IsNaN(timetokill) || Double.IsInfinity(timetokill))
                        timetokill = 0;

                    //Store the time
                    OwnedArmy_TimeToKillEnemyArmy.Add(enumerator.Current.UniqueID, timetokill);
                }
                else
                    OwnedArmy_TimeToKillEnemyArmy.Add(enumerator.Current.UniqueID, 0);
            }
            EnemyArmy_TimeToKillOwnedArmy = new Dictionary<string, double>();
            for(var enumerator = enemy_army.GetEnumerator(); enumerator.MoveNext();)
            {
                if (enumerator.Current.Target != null)
                {
                    //Compute the potential mean damage to target
                    var potentialmeandamage = (((2 * Unit.GetMinimumPotentialDamage(enumerator.Current)) + Unit.GetMaximumPotentialDamage(enumerator.Current)) / 3);

                    //Compute the time to kill target
                    var timetokill = enumerator.Current.Target.Current_Health / potentialmeandamage;
                    if (Double.IsNaN(timetokill) || Double.IsInfinity(timetokill))
                        timetokill = 0;

                    //Store the time
                    EnemyArmy_TimeToKillOwnedArmy.Add(enumerator.Current.UniqueID, timetokill);
                }
                else
                    EnemyArmy_TimeToKillOwnedArmy.Add(enumerator.Current.UniqueID, 0);
            }

            //Get the max time to kill a unit of both army
            var ownedarmy_maxtimetokillenemyunit = OwnedArmy_TimeToKillEnemyArmy.Max(unit => unit.Value);
            var enemyarmy_maxtimetokillenemyunit = EnemyArmy_TimeToKillOwnedArmy.Max(unit => unit.Value);

            //Compute the global time for combat
            TimeToKill = Math.Min(ownedarmy_maxtimetokillenemyunit, enemyarmy_maxtimetokillenemyunit);
        }

        /// <summary>
        /// Returns the combat time
        /// </summary>
        /// <param name="combat_result"></param>
        /// <returns></returns>
        public static int GetCombatTime(DynamicCombatResult combat_result) => Convert.ToInt32(Math.Ceiling(combat_result.TimeToKill));

        /// <summary>
        /// Returns true if either one of the army's health is less than 0, or there are
        /// no more units in each army that can target each other
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        /// <returns></returns>
        public static bool IsCombatContinuable(Army owned_units, Army enemy_units)
        {
            bool is_damageable = ((owned_units.Sum(unit => unit.Current_Health) > 0) && (enemy_units.Sum(unit => unit.Current_Health) > 0));
            bool is_targetable = false;

            try
            {
                //If there are units still alive in both army
                if(is_damageable)
                {
                    bool ownedarmy_targetable = false, enemyarmy_targetable = false;

                    //Units in both army that is still and have no targets
                    //by no targets, either their target is dead and have no replacement, or
                    //they have no target at all since the beginning
                    var ownedarmy_notargetunits = owned_units.Where(unit => (unit.IsOpposingDefeated && !unit.IsDefeated));
                    var enemyarmy_notargetunits = enemy_units.Where(unit => (unit.IsOpposingDefeated && !unit.IsDefeated));

                    //In both army, there are still alive units that have no target.
                    //If a unit still have a target in either of the army, such that it suffice the condition
                    //(1 > 0) && (0 > 0), (0 > 0) && (1 > 0), or (0 > 0) && (0 > 0), the battle will continue
                    if ((ownedarmy_notargetunits.Count() > 0) && (enemyarmy_notargetunits.Count() > 0))
                    {
                        foreach (var idle_unit in ownedarmy_notargetunits)
                        {
                            foreach (var alive_unit in enemyarmy_notargetunits)
                            {
                                //There is still a targetable unit in enemy army
                                if (idle_unit.CanTarget(alive_unit))
                                {
                                    enemyarmy_targetable = true;
                                    break;
                                }
                            }
                        }

                        foreach (var idle_unit in enemyarmy_notargetunits)
                        {
                            foreach (var alive_unit in ownedarmy_notargetunits)
                            {
                                //There is still a targetable unit in owned army
                                if (idle_unit.CanTarget(alive_unit))
                                {
                                    ownedarmy_targetable = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        ownedarmy_targetable = enemyarmy_targetable = true;

                    //If one of the army still have units that can be targeted,
                    //the combat will continue
                    is_targetable = (ownedarmy_targetable || enemyarmy_targetable);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"IsCombatContinuable() -> {ex.Message}");
                is_targetable = false;
            }

            return (is_damageable && is_targetable);
        }
    }
}
