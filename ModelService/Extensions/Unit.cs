﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Types
{
    /// <summary>
    /// A struct that hold the definitions for a unit
    /// </summary>
    public struct UnitDefinitions
    {
        /// <summary>
        /// The base health of a unit
        /// </summary>
        public double Health { get; set; }

        /// <summary>
        /// The base energy of a unit
        /// </summary>
        public double Energy { get; set; }

        /// <summary>
        /// The base armor of a unit
        /// </summary>
        public double Armor { get; set; }

        /// <summary>
        /// The base ground damage of a unit
        /// </summary>
        public double Ground_Damage { get; set; }

        /// <summary>
        /// The base air damage of a unit
        /// </summary>
        public double Air_Damage { get; set; }

        /// <summary>
        /// If the unit is a flying type
        /// </summary>
        public bool IsFlying_Unit { get; set; }

        /// <summary>
        /// The basic definition for a unit
        /// </summary>
        /// <param name="health"></param>
        /// <param name="energy"></param>
        /// <param name="ground_damage"></param>
        /// <param name="air_damage"></param>
        /// <param name="armor"></param>
        /// <param name="is_flying_unit"></param>
        public UnitDefinitions(double health, double energy, double ground_damage, double air_damage, double armor, bool is_flying_unit)
        {
            Health = health;
            Energy = energy;
            Armor = armor;
            Ground_Damage = ground_damage;
            Air_Damage = air_damage;
            IsFlying_Unit = is_flying_unit;
        }
    }

    /// <summary>
    /// A struct that hold the worth of a unit
    /// </summary>
    public struct UnitWorth
    {
        /// <summary>
        /// The priority of this unit to be destroyed
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The mineral cost of creating this unit
        /// </summary>
        public int Mineral { get; set; }

        /// <summary>
        /// The vespene cost of creating this unit
        /// </summary>
        public int Vespene { get; set; }

        /// <summary>
        /// The supply cost of this unit
        /// </summary>
        public int Supply { get; set; }

        /// <summary>
        /// The basic worth of a unit
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="mineral"></param>
        /// <param name="vespene"></param>
        /// <param name="supply"></param>
        public UnitWorth(int priority, int mineral, int vespene, int supply)
        {
            Priority = priority;
            Mineral = mineral;
            Vespene = vespene;
            Supply = supply;
        }
    }

    /// <summary>
    /// The static information about a unit
    /// </summary>
    public partial class Unit
    {
        /// <summary>
        /// The base definitions for a unit
        /// </summary>
        public static Dictionary<string, UnitDefinitions> Definitions = new Dictionary<string, UnitDefinitions>()
        {
            //Ground Units
            ["TERRAN_WIDOWMINE"] = new UnitDefinitions(90, 0, 125, 125, 0, false),
            ["TERRAN_SCV"] = new UnitDefinitions(45, 0, 4.67, 0, 0, false),
            ["TERRAN_MARINE"] = new UnitDefinitions(45, 0, 9.8, 9.8, 0, false),
            ["TERRAN_MARAUDER"] = new UnitDefinitions(125, 0, 9.3, 0, 1, false),
            ["TERRAN_REAPER"] = new UnitDefinitions(60, 0, 10.1, 0, 0, false),
            ["TERRAN_GHOST"] = new UnitDefinitions(100, 200, 9.3, 9.3, 0, false), //75/200
            ["TERRAN_HELLION"] = new UnitDefinitions(90, 0, 4.48, 0, 0, false),
            ["TERRAN_HELLIONTANK"] = new UnitDefinitions(135, 0, 12.6, 0, 0, false),
            ["TERRAN_SIEGETANK"] = new UnitDefinitions(175, 0, 20.27, 0, 1, false),
            ["TERRAN_SIEGETANKSIEGED"] = new UnitDefinitions(175, 0, 18.69, 0, 1, false),
            ["TERRAN_CYCLONE"] = new UnitDefinitions(120, 0, 25.2, 25.2, 1, false),
            ["TERRAN_THOR"] = new UnitDefinitions(400, 0, 65.9, 11.2, 1, false),
            ["TERRAN_THORAP"] = new UnitDefinitions(400, 0, 65.9, 23.4, 1, false),
            ["TERRAN_AUTOTURRET"] = new UnitDefinitions(150, 0, 31.58, 31.58, 1, false),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new UnitDefinitions(135, 0, 16.8, 0, 0, true),
            ["TERRAN_VIKINGFIGHTER"] = new UnitDefinitions(135, 0, 0, 14, 0, true),
            ["TERRAN_MEDIVAC"] = new UnitDefinitions(150, 200, 0, 0, 1, true), //50/200
            ["TERRAN_LIBERATORAG"] = new UnitDefinitions(180, 0, 65.8, 0, 0, true),
            ["TERRAN_LIBERATOR"] = new UnitDefinitions(180, 0, 0, 7.8, 0, true),
            ["TERRAN_RAVEN"] = new UnitDefinitions(140, 200, 0, 0, 1, true), //50(+25)/200
            ["TERRAN_BANSHEE"] = new UnitDefinitions(140, 200, 27, 0, 0, true),
            ["TERRAN_BATTLECRUISER"] = new UnitDefinitions(550, 200, 49.8, 31.1, 3, true),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new UnitDefinitions(1500, 0, 28, 0, 3, false),
            ["TERRAN_BUNKER"] = new UnitDefinitions(400, 0, 0, 0, 1, false),
            ["TERRAN_MISSILETURRET"] = new UnitDefinitions(250, 0, 0, 39.3, 1, false),
            ["TERRAN_COMMANDCENTER"] = new UnitDefinitions(1500, 0, 0, 0, 1, false),
            ["TERRAN_ORBITALCOMMAND"] = new UnitDefinitions(1500, 200, 0, 0, 1, false), //50/200
            ["TERRAN_SUPPLYDEPOT"] = new UnitDefinitions(400, 0, 0, 0, 1, false),
            ["TERRAN_REFINERY"] = new UnitDefinitions(500, 0, 0, 0, 1, false),
            ["TERRAN_BARRACKS"] = new UnitDefinitions(1000, 0, 0, 0, 1, false),
            ["TERRAN_ENGINEERINGBAY"] = new UnitDefinitions(850, 0, 0, 0, 1, false),
            ["TERRAN_BUNKER"] = new UnitDefinitions(400, 0, 0, 0, 1, false),
            ["TERRAN_SENSORTOWER"] = new UnitDefinitions(200, 0, 0, 0, 0, false),
            ["TERRAN_FACTORY"] = new UnitDefinitions(1250, 0, 0, 0, 1, false),
            ["TERRAN_GHOSTACADEMY"] = new UnitDefinitions(1250, 0, 0, 0, 1, false),
            ["TERRAN_STARPORT"] = new UnitDefinitions(1300, 0, 0, 0, 1, false),
            ["TERRAN_unit.Target.Current_ArmorY"] = new UnitDefinitions(750, 0, 0, 0, 1, false),
            ["TERRAN_FUSIONCORE"] = new UnitDefinitions(750, 0, 0, 0, 1, false)
        };

        /// <summary>
        /// The base values for a unit
        /// </summary>
        public static Dictionary<string, UnitWorth> Values = new Dictionary<string, UnitWorth>()
        {
            //Ground Units
            ["TERRAN_WIDOWMINE"] = new UnitWorth(19, 75, 25, 2),
            ["TERRAN_SCV"] = new UnitWorth(20, 50, 0, 1),
            ["TERRAN_MARINE"] = new UnitWorth(20, 50, 0, 1),
            ["TERRAN_MARAUDER"] = new UnitWorth(20, 100, 25, 2),
            ["TERRAN_REAPER"] = new UnitWorth(20, 50, 50, 1),
            ["TERRAN_GHOST"] = new UnitWorth(20, 150, 125, 2),
            ["TERRAN_HELLION"] = new UnitWorth(20, 100, 0, 2),
            ["TERRAN_HELLIONTANK"] = new UnitWorth(20, 100, 0, 2),
            ["TERRAN_SIEGETANK"] = new UnitWorth(20, 150, 125, 3),
            ["TERRAN_SIEGETANKSEIGED"] = new UnitWorth(20, 150, 125, 3),
            ["TERRAN_CYCLONE"] = new UnitWorth(20, 150, 100, 3),
            ["TERRAN_THOR"] = new UnitWorth(20, 300, 200, 6),
            ["TERRAN_THORAP"] = new UnitWorth(20, 300, 200, 6),
            ["TERRAN_AUTOTURRET"] = new UnitWorth(20, 0, 0, 0),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new UnitWorth(20, 150, 75, 2),
            ["TERRAN_VIKINGFIGHTER"] = new UnitWorth(20, 150, 75, 2),
            ["TERRAN_MEDIVAC"] = new UnitWorth(20, 100, 100, 2),
            ["TERRAN_LIBERATORAG"] = new UnitWorth(20, 150, 150, 3),
            ["TERRAN_LIBERATOR"] = new UnitWorth(20, 150, 150, 3),
            ["TERRAN_RAVEN"] = new UnitWorth(20, 100, 200, 2),
            ["TERRAN_BANSHEE"] = new UnitWorth(20, 150, 100, 3),
            ["TERRAN_BATTLECRUISERr"] = new UnitWorth(20, 400, 300, 6),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new UnitWorth(20, 550, 150, 0),
            ["TERRAN_BUNKER"] = new UnitWorth(20, 100, 0, 0),
            ["TERRAN_MISSILETURRET"] = new UnitWorth(19, 100, 0, 0),
            ["TERRAN_COMMANDCENTER"] = new UnitWorth(11, 400, 0, 0),
            ["TERRAN_ORBITALCOMMAND"] = new UnitWorth(11, 550, 0, 0),
            ["TERRAN_SUPPLYDEPOT"] = new UnitWorth(11, 100, 0, 0),
            ["TERRAN_REFINERY"] = new UnitWorth(11, 75, 0, 0),
            ["TERRAN_BARRACKS"] = new UnitWorth(11, 150, 0, 0),
            ["TERRAN_ENGINEERINGBAY"] = new UnitWorth(11, 125, 0, 0),
            ["TERRAN_BUNKER"] = new UnitWorth(11, 100, 0, 0),
            ["TERRAN_SENSORTOWER"] = new UnitWorth(11, 125, 100, 0),
            ["TERRAN_FACTORY"] = new UnitWorth(11, 150, 100, 0),
            ["TERRAN_GHOSTACADEMY"] = new UnitWorth(11, 150, 100, 0),
            ["TERRAN_STARPORT"] = new UnitWorth(11, 150, 100, 0),
            ["TERRAN_unit.Target.Current_ArmorY"] = new UnitWorth(11, 150, 100, 0),
            ["TERRAN_FUSIONCORE"] = new UnitWorth(11, 150, 150, 0)
        };

        /// <summary>
        /// Returns a list of unique id of the <see cref="Unit.Targets"/>
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static List<string> GetTargetsOfUnit(Unit unit) => new List<string>((from target in unit.Targets select target.UniqueID));
    }

    /// <summary>
    /// A class extension for parsed unit. It holds additional methods that modifies
    /// and adds additional actions for a unit
    /// </summary>
    public static class UnitExtensions
    {
        private static List<int> TimeTracker { get; set; } = new List<int>();
        private static int GlobalIDGenerator { get; set; } = -1;

        private static double ApplyArmorToDamage(this Unit unit)
        {
            double Actual_Ground_Damage = 0, Actual_Air_Damage = 0;
            if (unit.Name == "TERRAN_MARINE")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (1.6 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (1.6 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_WIDOWMINE")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (1 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (1 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_SCV")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.93 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_REAPER")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (2.5 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_MARAUDER")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.93 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_GHOST")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.93 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (0.93 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_HELLION")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.56 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_SIEGETANK")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (1.35 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_SIEGETANKSIEGED")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.47 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_CYCLONE")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (1.26 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (1.26 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_HELLIONTANK")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.71 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_THOR")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (2.16 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (1.87 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_THORAP")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (2.16 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (0.59 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_AUTOTURRET")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (1.76 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (1.76 * unit.Target.Current_Armor);
            }
            else if (unit.Name == "TERRAN_VIKINGASSAULT")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (1.4 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_VIKINGFIGHTER")
                Actual_Air_Damage = unit.Current_Air_Damage - (1.4 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_LIBERATOR")
                Actual_Air_Damage = unit.Current_Air_Damage - (1.4 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_LIBERATORAG")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (0.81 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_BANSHEE")
                Actual_Ground_Damage = unit.Current_Ground_Damage - (2.25 * unit.Target.Current_Armor);
            else if (unit.Name == "TERRAN_BATTLECRUISER")
            {
                Actual_Ground_Damage = unit.Current_Ground_Damage - (6.2 * unit.Target.Current_Armor);
                Actual_Air_Damage = unit.Current_Air_Damage - (6.2 * unit.Target.Current_Armor);
            }

            return (Unit.Definitions[unit.Target.Name].IsFlying_Unit ? Actual_Air_Damage : Actual_Ground_Damage);
        }

        private static double GetPotentialMaximumDamage(this Unit unit)
        {
            return 0;
        }

        /// <summary>
        /// A method when the unit uses a one-time skill or ability
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="skill_name"></param>
        public static void UseSkillOrAbilities(this Unit unit, string skill_name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A method when the unit uses a time-based skill or ability.
        /// This returns the id from the list of <see cref="TimeTracker"/>
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="skill_name"></param>
        /// <returns></returns>
        public static int UseTemporarySkillOrAbilities(this Unit unit, string skill_name)
        {
            throw new NotImplementedException();
        }

        public static void DestroyTarget(this Unit unit)
        {

        }

        /// <summary>
        /// Deals a constant damage to the opposing unit using
        /// <see cref="Unit.Current_Ground_Damage"/> or <see cref="Unit.Current_Air_Damage"/>
        /// </summary>
        /// <param name="unit"></param>
        public static bool AttackTarget(this Unit unit, double damage_to_deal)
        {
            throw new NotImplementedException();
        }
    }
}