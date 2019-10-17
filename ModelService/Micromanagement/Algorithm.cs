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
            catch(ArgumentNullException ex)
            {
                Console.WriteLine($@"LanchesterBasedPrediction() [{target_policy.ToString()}] -> {ex.Message}");
                throw new Exception("");
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
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"StaticBasedPrediction() [{target_policy.ToString()}] -> {ex.Message}");
                throw new Exception("");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"StaticBasedPrediction() -> {ex.Message}");
                battle_result = null;
            }

            return battle_result;
        }

        /// <summary>
        /// <para>
        ///     Dynamic-based Prediction Algorithm is the last high abstraction prediction algorithm for this model.
        ///     Unlike the previous prediction algorithms, this method focuses more on details of the combat. Instead
        ///     of looking at the initial constant damage, or a noised damage, it randomizes between whether the unit
        ///     will attack or use its skill. Because of this, it is now able to consider that damage is not constant,
        ///     damage and skill are not executed at once, some skills have cooldown and duration, and some skills does not
        ///     deal damage, but something that affects other properties of unit. While it is still bounded by time
        ///     like <see cref="StaticBasedPrediction(TargetPolicy)"/>, it does not need to stop when the time ran out, but
        ///     it stops when there are no more units that can attack each other. Lastly, like the other mentioned algorithms,
        ///     this considers that some units cannot target other units, and that the dealt damage is reduced by the armor.
        /// </para>
        /// <para>
        ///     This method returns a string of survived units from the winning army, but the cost worth of doing battle
        ///     is always relative to the player. As such, if the opposing army won, the cost worth returned is negative since
        ///     it represents as a loss.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Unlike other algorithms, it does not look at the army as a whole, but on a per unit basis.
        ///     The probable running time is around O(n^3)
        /// </para>
        /// <para>
        ///     In summary, it only take part the time-related aspect of Static-based prediction, and proceeds
        ///     to let the army fight each other in a detailed manner. By detailed manner, a unit can attack
        ///     normally its target, or use a skill if it is researched during that time. It considers and
        ///     does not consider the following:
        ///     Considers:
        ///     <list type="bullet">
        ///         <item>Current/Decreasing Health and Energy</item>
        ///         <item>True Current Damage (Damage is applied with Armor)</item>
        ///         <item>Restrictions in targeting unit</item>
        ///         <item>Skills that deals damage, boost damage, and other that affects Health and Energy</item>
        ///         <item>Time-related properties to battle</item>
        ///         <item>Skills with cooldown and duration</item>
        ///     </list>
        ///     Does not Consider:
        ///     <list type="bullet">
        ///         <item>AoE Damage / Chaining Damage</item>
        ///         <item>Transforming Units</item>
        ///         <item>Damage with bonus to target type</item>
        ///     </list>
        /// </para>
        /// </remarks>
        /// <param name="target_policy"></param>
        /// <returns></returns>
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
                while(DynamicCombatResult.IsCombatContinuable(owned_units, enemy_units))
                {
                    var combat_time = DynamicCombatResult.GetCombatTime(combat_result);
                    for(int time_to_kill = 0; time_to_kill < combat_time; time_to_kill++)
                    {
                        var ability_probability = REngineExtensions.GetRandomGenerator().NextDouble();
                        owned_units.DealDamageToTarget(ability_probability);
                        enemy_units.DealDamageToTarget(ability_probability);
                    }

                    //Get the surviving units of both army
                    owned_units = owned_units.Where(unit => !unit.IsDefeated).ToArmy();
                    enemy_units = enemy_units.Where(unit => !unit.IsDefeated).ToArmy();

                    //Check if the battle can be continued
                    if (!DynamicCombatResult.IsCombatContinuable(owned_units, enemy_units) || (combat_time == 0))
                        break;

                    //Re-set the targets for each army to prevent
                    //a dead-lock and null target, and to get the true damage a target to attack
                    switch (target_policy)
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

                    //Recompute the battle output, then let them fight again
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
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"DynamicBasedPrediction() [{target_policy.ToString()}] -> {ex.Message}");
                throw new Exception("");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"DynamicBasedPrediction() -> {ex.Message}");
                battle_result = null;
            }

            return battle_result;
        }
    }
}