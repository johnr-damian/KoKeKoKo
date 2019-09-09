using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T> where T : Unit
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public string LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            string battle_result = "";

            try
            {
                var owned_units = _owned_units.CreateDeepCopy();
                var enemy_units = _enemy_units.CreateDeepCopy();

                //var owned_units_attributes = LanchesterExtensions.GetArmyAttributes(this, owned_units);
                var owned_units_attributes = owned_units.GetArmyAttributes();
                var enemy_units_attributes = enemy_units.GetArmyAttributes();

                ////First, compute the potential dpf
                //double owned_units_health = 0, enemy_units_health = 0;
                //double owned_units_minimum_potential_air = 0, enemy_units_minimum_potential_air = 0;
                //double owned_units_maximum_potential_air = 0, enemy_units_maximum_potential_air = 0;
                //double owned_units_minimum_potential_ground = 0, enemy_units_minimum_potential_ground = 0;
                //double owned_units_maximum_potential_ground = 0, enemy_units_maximum_potential_ground = 0;
                //double potentialdpf_ownedtoenemy = 0, potentialdpf_enemytoowned = 0;

                ////compute the average health of the army
                //owned_units_health = (owned_units.Sum(unit => unit.Current_Health) / owned_units.Length);
                //enemy_units_health = (enemy_units.Sum(unit => unit.Current_Health) / enemy_units.Length);

                ////compute the minimum potentials
                //owned_units_minimum_potential_air = owned_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item4);
                //owned_units_minimum_potential_ground = owned_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item3);
                //enemy_units_minimum_potential_air = enemy_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item4);
                //enemy_units_minimum_potential_ground = enemy_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item3);

                ////compute the maximum potentials
                //owned_units_maximum_potential_air = owned_units.Sum(unit => unit.Current_Air_Damage);
                //owned_units_maximum_potential_ground = owned_units.Sum(unit => unit.Current_Ground_Damage);
                //enemy_units_maximum_potential_air = enemy_units.Sum(unit => unit.Current_Air_Damage);
                //enemy_units_maximum_potential_ground = enemy_units.Sum(unit => unit.Current_Ground_Damage);

                ////the potential dpf
                //potentialdpf_ownedtoenemy = (() / ())

                double potential_owned_to_enemy = 0, potential_enemy_to_owned = 0;

                //Compute the DPF function
                //Item1 - Army Mean Health, 
                //Item2 - Flying Sum Health, 
                //Item3 - Ground Sum Health, 
                //Item4 - Flying Mean Damage, 
                //Item5 - Ground Mean Damage
                //potential A to B = (((A.Item4 * B.Item2) + (A.Item5 * B.Item3)) /(B.Item2 + B.Item3))
                potential_owned_to_enemy = (((owned_units_attributes.Item4 * enemy_units_attributes.Item2) + (owned_units_attributes.Item5 * enemy_units_attributes.Item3)) / (enemy_units_attributes.Item2 + enemy_units_attributes.Item3));
                potential_enemy_to_owned = (((enemy_units_attributes.Item4 * owned_units_attributes.Item2) + (enemy_units_attributes.Item5 * owned_units_attributes.Item3)) / (owned_units_attributes.Item2 + owned_units_attributes.Item3));


                //Compute the combat intensity
                double owned_intensity_alpha = 0, enemy_intensity_beta = 0;
                owned_intensity_alpha = potential_enemy_to_owned / owned_units_attributes.Item1;
                enemy_intensity_beta = potential_owned_to_enemy / enemy_units_attributes.Item1;

                //Compute relative effectiveness
                double owned_relative_effectiveness = 0, enemy_relative_effectiveness = 0;
                owned_relative_effectiveness = Math.Sqrt((owned_intensity_alpha / enemy_intensity_beta));
                enemy_relative_effectiveness = Math.Sqrt((enemy_intensity_beta / owned_intensity_alpha));

                //The winner is 
                double cardinality = owned_units.Length / enemy_units.Length;
                bool? is_own_only = null;

                if (cardinality > owned_relative_effectiveness)
                //Owned units win
                {
                    is_own_only = true;
                    Console.WriteLine("A WINS!");
                }
                else if (cardinality == owned_relative_effectiveness)
                //The fight is draw
                {
                    is_own_only = null;
                    Console.WriteLine("DRAW!");
                }
                else if (cardinality < owned_relative_effectiveness)
                //Enemy units win
                {
                    is_own_only = false;
                    Console.WriteLine("B WINS!");
                }
                else
                    throw new ArgumentException("What the hell is this value!");

                string message = "";

                var m = "";
                foreach (var u in owned_units)
                    m += (u.Target == null) ? "" : u.Target.ToString();
                Console.WriteLine(m);

                if(target_policy.HasFlag(TargetPolicy.Random))
                {
                    if(RandomBasedTargetPolicy(owned_units, enemy_units))
                    {
                        
                    }
                    m = "";
                    foreach (var u in owned_units)
                        m += (u.Target == null) ? "" : u.Target.ToString();
                    Console.WriteLine(m);
                }

                if(target_policy.HasFlag(TargetPolicy.Priority))
                {

                }

                if(target_policy.HasFlag(TargetPolicy.Resource))
                {

                }

                //Apply target selection
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to predict battle using Lanchester algorithm...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> LanchesterBasedPrediction(): \n\t{ex.Message}");

                battle_result = "";
            }

            return battle_result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public string StaticBasedPrediction(TargetPolicy target_policy)
        {
            string battle_result = "";

            try
            {
                //TODO
                Console.WriteLine("2");
                Console.WriteLine("2");
                Console.WriteLine("2");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to predict battle using Static algorithm...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> StaticBasedPrediction(): \n\t{ex.Message}");

                battle_result = "";
            }

            return battle_result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public string DynamicBasedPrediction(TargetPolicy target_policy)
        {
            string battle_result = "";

            try
            {
                //TODO
                Console.WriteLine("3");
                Console.WriteLine("3");
                Console.WriteLine("3");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to predict battle using Dynamic algorithm...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> DynamicBasedPrediction(): \n\t{ex.Message}");

                battle_result = "";
            }

            return battle_result;
        }
    }
}
