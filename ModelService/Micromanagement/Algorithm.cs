using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        /// <summary>
        /// The highest-level abstraction of prediction algorithm. It computes
        /// the potential average of the army and statically computes who will won using
        /// the Lanchester formula. It returns the winner's army and the value won from this
        /// battle
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns>The winner's army can be owner or enemy. But the value will always be the owner</returns>
        public Tuple<string, UnitWorth> LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, UnitWorth> battle_result = null;

            try
            {
                var owned_units = _owned_units.GetDeepCopy();
                var enemy_units = _enemy_units.GetDeepCopy();
                var cardinality = owned_units.Count() / enemy_units.Count();
                var lanchester_result = new LanchesterVariables(owned_units, enemy_units);


                //Generate target for each units
                switch(target_policy)
                {
                    case TargetPolicy.Random:
                        //Owned Army wins
                        if (cardinality > lanchester_result.OwnedArmy_RelativeEffectiveness)
                        {
                            //Compute the estimated number of units survived
                            var surviving_owned_units = Convert.ToInt32(Math.Sqrt((Math.Pow(owned_units.Count(), 2)) - ((lanchester_result.OwnedArmy_CombatEffectiveness / lanchester_result.EnemyArmy_CombatEffectiveness) * Math.Pow(enemy_units.Count(), 2))));

                            //Apply the Targeting Policy to get the likelihood of units who will survive
                            RandomBasedTargetPolicy(ref owned_units, enemy_units);

                            //Get the surviving units and create the battle result
                            var surviving_units = owned_units.Take(surviving_owned_units).ToArmy();
                            battle_result = new Tuple<string, UnitWorth>(surviving_units.ToString(), surviving_units.GetValueOfArmy());
                        }
                        //Draw
                        else if (cardinality == lanchester_result.OwnedArmy_RelativeEffectiveness)
                            battle_result = new Tuple<string, UnitWorth>(String.Empty, default(UnitWorth));
                        //Enemy Army wins
                        else if (cardinality < lanchester_result.OwnedArmy_RelativeEffectiveness)
                        {
                            //Compute the estimated number of units survived
                            var surviving_enemy_units = Convert.ToInt32(Math.Sqrt((Math.Pow(enemy_units.Count(), 2)) - ((lanchester_result.EnemyArmy_CombatEffectiveness / lanchester_result.OwnedArmy_CombatEffectiveness) * Math.Pow(owned_units.Count(), 2))));

                            //Apply the Targeting Policy to get the likelihood of units who will survive
                            RandomBasedTargetPolicy(ref enemy_units, owned_units);

                            //Get the surviving units and create the battle result
                            var surviving_units = enemy_units.Take(surviving_enemy_units).ToArmy();
                            battle_result = new Tuple<string, UnitWorth>(surviving_units.ToString(), surviving_units.GetValueOfArmy().GetComplementOfValue());
                        }
                        break;
                    case TargetPolicy.Priority:
                        //Owned Army wins
                        if (cardinality > lanchester_result.OwnedArmy_RelativeEffectiveness)
                        {
                            //Compute the estimated number of units survived
                            var surviving_owned_units = Convert.ToInt32(Math.Sqrt((Math.Pow(owned_units.Count(), 2)) - ((lanchester_result.OwnedArmy_CombatEffectiveness / lanchester_result.EnemyArmy_CombatEffectiveness) * Math.Pow(enemy_units.Count(), 2))));

                            //Apply the Targeting Policy to get the likelihood of units who will survive
                            PriorityBasedTargetPolicy(ref owned_units, enemy_units);

                            //Get the surviving units and create the battle result
                            var surviving_units = owned_units.Take(surviving_owned_units).ToArmy();
                            battle_result = new Tuple<string, UnitWorth>(surviving_units.ToString(), surviving_units.GetValueOfArmy());
                        }
                        //Draw
                        else if (cardinality == lanchester_result.OwnedArmy_RelativeEffectiveness)
                            battle_result = new Tuple<string, UnitWorth>(String.Empty, default(UnitWorth));
                        //Enemy Army wins
                        else if (cardinality < lanchester_result.OwnedArmy_RelativeEffectiveness)
                        {
                            //Compute the estimated number of units survived
                            var surviving_enemy_units = Convert.ToInt32(Math.Sqrt((Math.Pow(enemy_units.Count(), 2)) - ((lanchester_result.EnemyArmy_CombatEffectiveness / lanchester_result.OwnedArmy_CombatEffectiveness) * Math.Pow(owned_units.Count(), 2))));

                            //Apply the Targeting Policy to get the likelihood of units who will survive
                            PriorityBasedTargetPolicy(ref enemy_units, owned_units);

                            //Get the surviving units and create the battle result
                            var surviving_units = enemy_units.Take(surviving_enemy_units).ToArmy();
                            battle_result = new Tuple<string, UnitWorth>(surviving_units.ToString(), surviving_units.GetValueOfArmy().GetComplementOfValue());
                        }
                        break;
                    case TargetPolicy.Resource:
                        //Owned Army wins
                        if (cardinality > lanchester_result.OwnedArmy_RelativeEffectiveness)
                        {
                            //Compute the estimated number of units survived
                            var surviving_owned_units = Convert.ToInt32(Math.Sqrt((Math.Pow(owned_units.Count(), 2)) - ((lanchester_result.OwnedArmy_CombatEffectiveness / lanchester_result.EnemyArmy_CombatEffectiveness) * Math.Pow(enemy_units.Count(), 2))));

                            //Apply the Targeting Policy to get the likelihood of units who will survive
                            ResourceBasedTargetPolicy(ref owned_units, enemy_units);

                            //Get the surviving units and create the battle result
                            var surviving_units = owned_units.Take(surviving_owned_units).ToArmy();
                            battle_result = new Tuple<string, UnitWorth>(surviving_units.ToString(), surviving_units.GetValueOfArmy());
                        }
                        //Draw
                        else if (cardinality == lanchester_result.OwnedArmy_RelativeEffectiveness)
                            battle_result = new Tuple<string, UnitWorth>(String.Empty, default(UnitWorth));
                        //Enemy Army wins
                        else if (cardinality < lanchester_result.OwnedArmy_RelativeEffectiveness)
                        {
                            //Compute the estimated number of units survived
                            var surviving_enemy_units = Convert.ToInt32(Math.Sqrt((Math.Pow(enemy_units.Count(), 2)) - ((lanchester_result.EnemyArmy_CombatEffectiveness / lanchester_result.OwnedArmy_CombatEffectiveness) * Math.Pow(owned_units.Count(), 2))));

                            //Apply the Targeting Policy to get the likelihood of units who will survive
                            ResourceBasedTargetPolicy(ref enemy_units, owned_units);

                            //Get the surviving units and create the battle result
                            var surviving_units = enemy_units.Take(surviving_enemy_units).ToArmy();
                            battle_result = new Tuple<string, UnitWorth>(surviving_units.ToString(), surviving_units.GetValueOfArmy().GetComplementOfValue());
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"LanchesterBasedPrediction() -> {ex.Message}...");
                battle_result = null;
            }

            return battle_result;
        }

        /// <summary>
        /// A high-level abstraction of prediction algorithm. It gets a random potential damage
        /// from the triangular distribution. Like the <see cref="LanchesterBasedPrediction(TargetPolicy)"/>, it
        /// does not consider the decreasing energy of the unit.
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public Tuple<string, double> StaticBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, double> battle_result = null;

            try
            {
                var owned_units = _owned_units.GetDeepCopy();
                var enemy_units = _enemy_units.GetDeepCopy();

                switch(target_policy)
                {
                    case TargetPolicy.Random:

                        break;
                    case TargetPolicy.Priority:

                        break;
                    case TargetPolicy.Resource:

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"StaticBasedPrediction() -> {ex.Message}...");
                battle_result = null;
            }

            return battle_result;
        }

        /// <summary>
        /// A high-level abstraction of prediction algorithm, but more detailed. Unlike <see cref="StaticBasedPrediction(TargetPolicy)"/>,
        /// It considers the decreasing of energy of the unit and as well as the health of each unit
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public Tuple<string, double> DynamicBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, double> battle_result = null;

            try
            {
                var owned_units = _owned_units.GetDeepCopy();
                var enemy_units = _enemy_units.GetDeepCopy();

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"DynamicBasedPrediction() -> {ex.Message}...");
                battle_result = null;
            }

            return battle_result;
        }
    }
}