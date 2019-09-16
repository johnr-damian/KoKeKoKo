using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ModelService.Micromanagement
{

    public static class LanchesterExtensions
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="army"></param>
        /// <returns></returns>
        public static Tuple<double, double, double, double, double, double> GetModifiedArmyAttributes(this IEnumerable<Unit> army)
        {
            Tuple<double, double, double, double, double, double> attributes = null;

            try
            {
                double flying_health_minimum = 0, flying_health_maximum = 0;
                double ground_health_minimum = 0, ground_health_maximum = 0;
                double flying_damage_minimum = 0, flying_damage_maximum = 0;
                double ground_damage_minimum = 0, ground_damage_maximum = 0;


                //Flying Units Health
                flying_health_minimum = army.Where(unit => Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => Unit.DEFINITIONS[unit.Name].Item1);
                flying_health_maximum = army.Where(unit => Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Health);

                //Ground Units Health
                ground_health_minimum = army.Where(unit => !Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => Unit.DEFINITIONS[unit.Name].Item1);
                ground_health_maximum = army.Where(unit => !Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Health);

                //Flying Damage
                flying_damage_minimum = army.Where(unit => Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => Unit.DEFINITIONS[unit.Name].Item4);
                flying_damage_maximum = army.Where(unit => Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Air_Damage);

                //Ground Damage
                ground_damage_minimum = army.Where(unit => !Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => Unit.DEFINITIONS[unit.Name].Item3);
                ground_damage_maximum = army.Where(unit => !Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Ground_Damage);

                //May malit ata dito
                //Need to get the true middle/central hp of the army
                //We either use a triangular distribution, continue with min + max /2 or value / n
                //For now, this follows wholefully to the study


                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to get the attributes of the army...");
                Trace.WriteLine($@"Error in Model! LanchesterExtensions -> GetModifiedArmyAttributes(): \n\t{ex.Message}");

                attributes = null;
            }

            return attributes;
        }

        /// <summary>
        /// The semi-original acquisition of values based on the reference study.
        /// It is semi because the effective dpf did not come from methodology of the study
        /// </summary>
        /// <param name="army"></param>
        /// <returns>Army Mean Health, Flying Sum Health, Ground Sum Health, Flying Mean Damage, Ground Mean Damage</returns>
        public static Tuple<double, double, double, double, double> GetArmyAttributes(this IEnumerable<Unit> army)
        {
            Tuple<double, double, double, double, double> attributes = null;

            try
            {
                //Overall Army Mean Health
                var army_mean_health = (army.Sum(unit => unit.Current_Health) / army.Count());
                if (Double.IsNaN(army_mean_health))
                    army_mean_health = 0;

                //Specific Sum Health
                var flying_units_health = army.Where(unit => Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Health);
                if (Double.IsNaN(flying_units_health))
                    flying_units_health = 0;
                var ground_units_health = army.Where(unit => !Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Health);
                if (Double.IsNaN(ground_units_health))
                    ground_units_health = 0;

                //Specific Mean Damage
                var flying_units_damage = (army.Where(unit => Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Air_Damage) / army.Count(unit => Unit.DEFINITIONS[unit.Name].Item6));
                if (Double.IsNaN(flying_units_damage))
                    flying_units_damage = 0;
                var ground_units_damage = (army.Where(unit => !Unit.DEFINITIONS[unit.Name].Item6).Sum(unit => unit.Current_Ground_Damage) / army.Count(unit => !Unit.DEFINITIONS[unit.Name].Item6));
                if (Double.IsNaN(ground_units_damage))
                    ground_units_damage = 0;

                attributes = new Tuple<double, double, double, double, double>(army_mean_health, flying_units_health, ground_units_health, flying_units_damage, ground_units_damage);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to get the attributes of the army...");
                Trace.WriteLine($@"Error in Model! LanchesterExtensions -> GetArmyAttributes(): \n\t{ex.Message}");

                attributes = null;
            }

            return attributes;
        }

        public static IEnumerable<T> Fisher_YatesShuffle<T>(this IEnumerable<T> army)
        {
            var rad = new Random();
            T[] elements = army.ToArray();
            for(int iterator = elements.Length -1; iterator >= 0; iterator--)
            {
                int swapindex = rad.Next(iterator + 1);
                yield return elements[swapindex];
                elements[swapindex] = elements[iterator];
            }

            yield return elements[0];
        }
    }

    /// <summary>
    /// A list of available policy for targeting units
    /// </summary>
    [Flags]
    public enum TargetPolicy
    {
        /// <summary>
        /// Targets a unit from the list randomly
        /// </summary>
        Random = 1,

        /// <summary>
        /// Targets a unit based on priority. The unit with a low priority number is picked first
        /// </summary>
        Priority = 2,

        /// <summary>
        /// Targets a unit based on resource. The unit with a higher cost is picked first
        /// </summary>
        Resource = 4
    }
}
