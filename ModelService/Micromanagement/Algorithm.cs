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

                //First, compute the potential dpf
                double owned_units_health = 0, enemy_units_health = 0;
                double owned_units_minimum_potential_air = 0, enemy_units_minimum_potential_air = 0;
                double owned_units_maximum_potential_air = 0, enemy_units_maximum_potential_air = 0;
                double owned_units_minimum_potential_ground = 0, enemy_units_minimum_potential_ground = 0;
                double owned_units_maximum_potential_ground = 0, enemy_units_maximum_potential_ground = 0;
                double potentialdpf_ownedtoenemy = 0, potentialdpf_enemytoowned = 0;

                //compute the average health of the army
                owned_units_health = (owned_units.Sum(unit => unit.Current_Health) / owned_units.Length);
                enemy_units_health = (enemy_units.Sum(unit => unit.Current_Health) / enemy_units.Length);

                //compute the minimum potentials
                owned_units_minimum_potential_air = owned_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item4);
                owned_units_minimum_potential_ground = owned_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item3);
                enemy_units_minimum_potential_air = enemy_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item4);
                enemy_units_minimum_potential_ground = enemy_units.Sum(unit => Unit.DEFINITIONS[unit.Name].Item3);

                //compute the maximum potentials
                owned_units_maximum_potential_air = owned_units.Sum(unit => unit.Current_Air_Damage);
                owned_units_maximum_potential_ground = owned_units.Sum(unit => unit.Current_Ground_Damage);
                enemy_units_maximum_potential_air = enemy_units.Sum(unit => unit.Current_Air_Damage);
                enemy_units_maximum_potential_ground = enemy_units.Sum(unit => unit.Current_Ground_Damage);

                //the potential dpf
                potentialdpf_ownedtoenemy = 


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
