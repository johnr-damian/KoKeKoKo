﻿using ModelService.Micromanagement;
using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    public partial class Army
    {
        private static Random _damagegenerator;
        private double Minimum_Damage { get; set; } = default(double);
        private double Maximum_Damage { get; set; } = default(double);

        static Army()
        {
            _damagegenerator = new Random();
        }

        /// <summary>
        /// Returns a random damage between the potential minimum damage of army and maximum potential damage of army
        /// </summary>
        /// <returns></returns>
        public double GetARandomDamage() => (_damagegenerator.NextDouble() * ((Maximum_Damage - Minimum_Damage) + Minimum_Damage));
    }

    public static class ArmyExtensions
    {
        public static Army ToArmy(this IEnumerable<Unit> units) => new Army(units);

        public static double GetLanchesterMeanTriangularAirDamage(this IEnumerable<Unit> units)
        {
            var minimum_potential_army_damage = units.Sum(unit => unit.Current_Air_Damage);
            var maximum_potential_army_damage = units.Sum(unit => unit.GetMaximumPotentialAirDamage());

            //Use left-skewed triangular mean where
            //Mean = ((2 * minimum) + maximum) / 3
            return (((2 * minimum_potential_army_damage) + maximum_potential_army_damage) / 3);
        }

        public static double GetLanchesterMeanTriangularGroundDamage(this IEnumerable<Unit> units)
        {
            var minimum_potential_army_damage = units.Sum(unit => unit.Current_Ground_Damage);
            var maximum_potential_army_damage = units.Sum(unit => unit.GetMaximumPotentialGroundDamage());

            //Likewise, use left-skewed triangular mean where
            //Mean = ((2 * minimum) + maximum) / 3
            return (((2 * minimum_potential_army_damage) + maximum_potential_army_damage) / 3);
        }


    }
}
