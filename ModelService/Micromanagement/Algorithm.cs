﻿using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        /// <summary>
        /// <para>
        ///     Lanchester-based Prediction Algorithm is the highest form of abstraction. It uses the 
        ///     Lanchester formula to compute the combat winner, including the survivor in the winning army.
        ///     As a highest-abstraction prediction algorithm, it exchanges fine detail for faster computing. It 
        ///     does not consider the case that Health and Energy decreases over time, and that some skills
        ///     have cooldown and duration. However, it considers that some units cannot target some
        ///     other units such as air units. It also consider skills that deals damage/boost damage but not on the finest
        ///     detail. Lastly, it also consider that the damage is reduced by the armor.
        /// </para>
        /// <para>
        ///     This method returns a string of survived units from the winning army, but the cost worth
        ///     of doing battle is always relative to the player. As such, if the opposing army won, the cost worth
        ///     returned is negative since it represents as a loss. 
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     While it is said to be the fastest out of the three algorithm, it is still dependent on the target policy.
        ///     The probably running time is O(4n^2 + 2n).
        /// </para>
        /// <para>
        ///     In summary, the Lanchester-based prediction considers and does not consider the following:
        ///     Considers:
        ///     <list type="bullet">
        ///         <item>True Current Damage (Current Damage applied with opposing Armor)</item>
        ///         <item>Restrictions in targeting unit (Some unit cannot target air unit, and vice versa)</item>
        ///         <item>Current Health</item>
        ///         <item>Skills that deals damage and gives boost to the Current damage</item>
        ///     </list>
        ///     Does not Consider:
        ///     <list type="bullet">
        ///         <item>Decreasing Health</item>
        ///         <item>Decreasing Energy</item>
        ///         <item>Skills with Cooldown and Duration</item>
        ///         <item>Skills that affects Health/Energy</item>
        ///         <item>AoE Damage / Chaining Damage</item>
        ///         <item>Transforming Units</item>
        ///         <item>Time and its related properties to battle</item>
        ///         <item>Damage with bonus to target type</item>
        ///     </list>
        /// </para>
        /// </remarks>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public Tuple<string, CostWorth> LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, CostWorth> battle_result = null;

            try
            {
                //Create a copy of the units
                var owned_units = _owned_units.GetDeepCopy();
                var enemy_units = _enemy_units.GetDeepCopy();

                //Set the targets for each army
                //This will get the true damage of unit, since Damage with Armor is applied
                switch(target_policy)
                {
                    case TargetPolicy.Random:
                        RandomBasedTargetPolicy(ref owned_units, enemy_units);
                        RandomBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                    case TargetPolicy.Priority:
                        PriorityBasedTargetPolicy(ref owned_units, enemy_units);
                        PriorityBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                    case TargetPolicy.Resource:
                        ResourceBasedTargetPolicy(ref owned_units, enemy_units);
                        ResourceBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                }

                //Compute the battle output
                var combat_result = new LanchesterCombatResult(owned_units, enemy_units);
                var combat_winner = LanchesterCombatResult.GetCombatWinner(combat_result);
                var combat_survivor = LanchesterCombatResult.GetSurvivingUnits(combat_result, combat_winner);

                //Get the surviving units of the winner
                Army survived_units = default(Army);
                switch(combat_winner)
                {
                    case LanchesterCombatResult.CombatWinner.Owned_Army:
                        //Based on policy, pick which units will survive
                        switch(target_policy)
                        {
                            case TargetPolicy.Random:
                                survived_units = owned_units.RandomlyTake(combat_survivor);
                                break;
                            case TargetPolicy.Priority:
                                survived_units = owned_units.PriorityTake(combat_survivor);
                                break;
                            case TargetPolicy.Resource:
                                survived_units = owned_units.ResourceTake(combat_survivor);
                                break;
                        }

                        battle_result = new Tuple<string, CostWorth>(survived_units.ToString(), survived_units.GetValueOfArmy());
                        break;
                    case LanchesterCombatResult.CombatWinner.Draw:
                        battle_result = new Tuple<string, CostWorth>(@"""""", default(CostWorth));
                        break;
                    case LanchesterCombatResult.CombatWinner.Enemy_Army:
                        //Based on policy, pick which units will survive
                        switch(target_policy)
                        {
                            case TargetPolicy.Random:
                                survived_units = enemy_units.RandomlyTake(combat_survivor);
                                break;
                            case TargetPolicy.Priority:
                                survived_units = enemy_units.PriorityTake(combat_survivor);
                                break;
                            case TargetPolicy.Resource:
                                survived_units = enemy_units.ResourceTake(combat_survivor);
                                break;
                        }

                        battle_result = new Tuple<string, CostWorth>(survived_units.ToString(), CostWorth.GetComplementOfCostWorth(survived_units.GetValueOfArmy()));
                        break;
                }                
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"LanchesterBasedPrediction() -> {ex.Message}");
                battle_result = null;
            }

            return battle_result;
        }

        /// <summary>
        /// <para>
        ///     Static-based Prediction Algorithm is an another high abstraction prediction algorithm.
        ///     Unlike the <see cref="LanchesterBasedPrediction(TargetPolicy)"/>, the computation focuses more
        ///     on time-related properties of combat. However, like the mentioned prediction algorithm, it still
        ///     exchanges fine detail for faster computing. Like the <see cref="LanchesterBasedPrediction(TargetPolicy)"/>, 
        ///     It does not consider that the Energy decreases over time, and that some skills have cooldown and duration.
        ///     However, it does consider that the damage is not constant by adding noises to the damage such as
        ///     there are times it is the current damage, and sometimes it is more than the current damage. It also considers
        ///     that some units cannot target other units, and considers skills that deals damage/boost damage but not on the
        ///     finest detail. Lastly, it also consider that the dealt damage is reduced by the armor. 
        /// </para>
        /// <para>
        ///     This method returns a string of survived units from the winning army, but the cost worth of doing
        ///     battle is always relative to the player. As such, if the opposing army won, the cost worth returned 
        ///     is negative since it represents as a loss.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     While it still looks the army as a whole, it computes more parameters.
        ///     The probable running time is O(4n^2 + 32n)
        /// </para>
        /// <para>
        ///     In summary, it is almost like the Lanchester-based prediction where it looks at the army as 
        ///     a whole, but also considers the time-related property of combat. Lastly, it also adds noise
        ///     to the damage being dealt to the target. It considers and does not consider the following:
        ///     Considers:
        ///     <list type="bullet">
        ///         <item>Current and Decreasing Health</item>
        ///         <item>True Current Damage (Current Damage applied with opposing Armor)</item>
        ///         <item>Restrictions in targeting unit</item>
        ///         <item>Skills that deals damage and gives boost to the damage of unit</item>
        ///         <item>Time-related properties to battle</item>
        ///     </list>
        ///     Does not Consider:
        ///     <list type="bullet">
        ///         <item>Decreasing Energy</item>
        ///         <item>Skills with Cooldown and Duration</item>
        ///         <item>Skills that affects Health/Energy</item>
        ///         <item>AoE Damage / Chaining Damage</item>
        ///         <item>Transforming Units</item>
        ///         <item>Damage with bonus to target type</item>
        ///     </list>
        /// </para>
        /// </remarks>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public Tuple<string, CostWorth> StaticBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, CostWorth> battle_result = null;

            try
            {
                //Create a copy of the units
                var owned_units = _owned_units.GetDeepCopy();
                var enemy_units = _enemy_units.GetDeepCopy();

                //Set the targets for each army
                //This will get the true damage of unit, and a target to be attacked
                switch(target_policy)
                {
                    case TargetPolicy.Random:
                        RandomBasedTargetPolicy(ref owned_units, enemy_units);
                        RandomBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                    case TargetPolicy.Priority:
                        PriorityBasedTargetPolicy(ref owned_units, enemy_units);
                        PriorityBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                    case TargetPolicy.Resource:
                        ResourceBasedTargetPolicy(ref owned_units, enemy_units);
                        ResourceBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                }

                //Compute the battle output
                var combat_result = new StaticCombatResult(owned_units, enemy_units);
                var combat_time = StaticCombatResult.GetCombatTime(combat_result);

                //Let both armies attack each other
                owned_units.DealDamageToTarget(combat_time);
                enemy_units.DealDamageToTarget(combat_time);

                //Get the surviving units of the winner
                var survived_owned_units = owned_units.Where(unit => !unit.IsDefeated).ToArmy();
                var survived_enemy_units = enemy_units.Where(unit => !unit.IsDefeated).ToArmy();
                //Owned Army Loss
                if (survived_owned_units.Count() < survived_enemy_units.Count())
                    battle_result = new Tuple<string, CostWorth>(survived_enemy_units.ToString(), CostWorth.GetComplementOfCostWorth(survived_enemy_units.GetValueOfArmy()));
                //Owned Army Won
                else if (survived_owned_units.Count() > survived_enemy_units.Count())
                    battle_result = new Tuple<string, CostWorth>(survived_owned_units.ToString(), survived_owned_units.GetValueOfArmy());
                //Draw
                else
                    battle_result = new Tuple<string, CostWorth>(@"""""", default(CostWorth));
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"StaticBasedPrediction() -> {ex.Message}");
                battle_result = null;
            }

            return battle_result;
        }

        public Tuple<string, CostWorth> DynamicBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, CostWorth> battle_result = null;

            try
            {
                //Create a copy of units
                var owned_units = _owned_units.GetDeepCopy();
                var enemy_units = _enemy_units.GetDeepCopy();

                //Set the targets for each army
                //This will get the true damage of unit, and a target to be attacked
                switch(target_policy)
                {
                    case TargetPolicy.Random:
                        RandomBasedTargetPolicy(ref owned_units, enemy_units);
                        RandomBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                    case TargetPolicy.Priority:
                        PriorityBasedTargetPolicy(ref owned_units, enemy_units);
                        PriorityBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                    case TargetPolicy.Resource:
                        ResourceBasedTargetPolicy(ref owned_units, enemy_units);
                        ResourceBasedTargetPolicy(ref enemy_units, owned_units);
                        break;
                }

                //Compute the battle output, then let both armies attack each other
                var combat_result = new DynamicCombatResult(owned_units, enemy_units);
                do
                {
                    var combat_time = DynamicCombatResult.GetCombatTime(combat_result);
                    for (int time_to_kill = 0; time_to_kill < combat_time; time_to_kill++)
                    {
                        var ability_probability = REngineExtensions.GetRandomGenerator().NextDouble();
                        owned_units.DealDamageToTarget(ability_probability);
                        enemy_units.DealDamageToTarget(ability_probability);
                    }

                    //Get the surviving units of both army
                    owned_units = owned_units.Where(unit => !unit.IsDefeated).ToArmy();
                    enemy_units = enemy_units.Where(unit => !unit.IsDefeated).ToArmy();

                    //Re-set the targets for each army
                    //Prevents dead-lock and null target, gets true damage, and a target to be attacked
                    switch (target_policy)
                    {
                        case TargetPolicy.Random:
                            RandomBasedTargetPolicy(ref owned_units, enemy_units);
                            RandomBasedTargetPolicy(ref enemy_units, owned_units);
                            break;
                        case TargetPolicy.Priority:
                            PriorityBasedTargetPolicy(ref owned_units, enemy_units);
                            PriorityBasedTargetPolicy(ref owned_units, enemy_units);
                            break;
                        case TargetPolicy.Resource:
                            ResourceBasedTargetPolicy(ref owned_units, enemy_units);
                            ResourceBasedTargetPolicy(ref enemy_units, owned_units);
                            break;
                    }
                }
                while (DynamicCombatResult.IsCombatContinuable(owned_units, enemy_units));
                {
                    //Re-compute the battle output, then let both armies attack again each other
                    combat_result = new DynamicCombatResult(owned_units, enemy_units);
                }

                //Get the surviving units of the winner
                var survived_owned_units = owned_units.Where(unit => !unit.IsDefeated).ToArmy();
                var survived_enemy_units = enemy_units.Where(unit => !unit.IsDefeated).ToArmy();
                //Owned Army Loss
                if (survived_owned_units.Count() < survived_enemy_units.Count())
                    battle_result = new Tuple<string, CostWorth>(survived_enemy_units.ToString(), CostWorth.GetComplementOfCostWorth(survived_enemy_units.GetValueOfArmy()));
                else if (survived_owned_units.Count() > survived_enemy_units.Count())
                    battle_result = new Tuple<string, CostWorth>(survived_owned_units.ToString(), survived_owned_units.GetValueOfArmy());
                else
                    battle_result = new Tuple<string, CostWorth>(@"""""", default(CostWorth));
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"DynamicBasedPrediction() -> {ex.Message}");
                battle_result = null;
            }

            return battle_result;
        }
    }
}