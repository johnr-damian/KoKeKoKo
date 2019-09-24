using ModelService.Micromanagement;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

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

        /// <summary>
        /// Returns the <see cref="Army.GetValueOfArmy"/> but the values are negative
        /// to represent that these values are what have been lost in a battle
        /// </summary>
        /// <returns></returns>
        public UnitWorth GetComplementOfValue() => new UnitWorth(-Priority, -Mineral, -Vespene, -Supply);

        /// <summary>
        /// Returns the sum of mineral, vespene, and supply cost with 34%, 34%, and 32% weight respectively
        /// </summary>
        /// <returns></returns>
        public double GetSummaryOfResource() => ((Mineral * .34) + (Vespene * .34) + (Supply * .32));

        /// <summary>
        /// Returns the sum of mineral, vespene, and supply cost with supplied weight
        /// </summary>
        /// <param name="mineral_weight"></param>
        /// <param name="vespene_weight"></param>
        /// <param name="supply_weight"></param>
        /// <returns></returns>
        public double GetSummaryOfResource(double mineral_weight, double vespene_weight, double supply_weight) => ((Mineral * mineral_weight) + (Vespene * vespene_weight) + (Supply * supply_weight));
    }

    public struct UnitSkills
    {
        public double Cooldown { get; set; }

        public double Duration { get; set; }


        public UnitSkills(double cooldown, double duration)
        {
            Cooldown = cooldown;
            Duration = duration;
        }

        public UnitSkills DecrementCountdown() => new UnitSkills(Cooldown - 1, Duration - 1);
    }

    /// <summary>
    /// The static information about a unit
    /// </summary>
    public partial class Unit
    {
        private static Random _shoulduseskill;
        private static Timer Duration_Timer;
        private static Dictionary<int, UnitSkills> Duration_Logger;
        private static int Duration_Tracker;

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
        /// Initializes
        /// </summary>
        static Unit()
        {
            _shoulduseskill = new Random(DateTime.Now.Second);
            Duration_Timer = new Timer(DecrementSkillsDuration, new AutoResetEvent(false), 0, 1000);
            Duration_Logger = new Dictionary<int, UnitSkills>();
            Duration_Tracker = -1;
        }

        /// <summary>
        /// Decrements the duration in <see cref="Activated_Skills"/>
        /// </summary>
        /// <param name="state"></param>
        private static void DecrementSkillsDuration(object state)
        {
            lock(Duration_Logger)
            {
                var count = Duration_Logger.Count;
                for (int decrementor = 0; decrementor < count; decrementor++)
                    Duration_Logger[decrementor] = Duration_Logger[decrementor].DecrementCountdown();
            }
        }

        /// <summary>
        /// Returns a list of unique id of the <see cref="Unit.Targets"/>
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static List<string> GetTargetsOfUnit(Unit unit) => new List<string>((from target in unit.Targets select target.UniqueID));

        /// <summary>
        /// Adds the activated skill
        /// </summary>
        /// <param name="skill_details"></param>
        /// <returns></returns>
        public static int TrackActivatedSkill(UnitSkills skill_details)
        {
            var current_skill_key = -1;

            lock(Duration_Logger)
            {
                current_skill_key = ++Duration_Tracker;
                Duration_Logger.Add(current_skill_key, skill_details);
            }

            return current_skill_key;
        }

        /// <summary>
        /// Returns a random double 
        /// </summary>
        /// <returns></returns>
        public static double GetARandomPercentage() => _shoulduseskill.NextDouble();
    }

    /// <summary>
    /// A class extension for parsed unit. It holds additional methods that modifies
    /// and adds additional actions for a unit
    /// </summary>
    public static class UnitExtensions
    {
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

        /// <summary>
        /// The Potential maximum damage is the upper limit for triangular distribution. It is the 
        /// sum of current damage (meaning there is probability there is buff) + possible actions that
        /// can deal additional damage to enemy
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static Tuple<double, double> GetPotentialUnitDamage(this Unit unit)
        {
            double potential_air_damage = -1, potential_ground_damage = -1;
            double temporary_current_air_damage = unit.Current_Air_Damage, temporary_current_ground_damage = unit.Current_Ground_Damage;

            switch (unit.Name)
            {
                case "TERRAN_MARINE": //stimpack considered
                    potential_air_damage = temporary_current_air_damage + (0.50 * unit.Current_Air_Damage);
                    potential_ground_damage = temporary_current_ground_damage + (0.50 * unit.Current_Ground_Damage);
                    break;
                case "TERRAN_MARAUDER": //stimpack considered
                    potential_air_damage = temporary_current_air_damage + (0.50 * unit.Current_Air_Damage);
                    potential_ground_damage = temporary_current_ground_damage + (0.50 * unit.Current_Ground_Damage);
                    break;
                case "TERRAN_REAPER": //kd8 charge
                    potential_ground_damage = temporary_current_ground_damage + 5;
                    break;
                case "TERRAN_GHOST": //nuke
                    potential_air_damage = temporary_current_air_damage + 300;
                    potential_ground_damage = temporary_current_ground_damage + 300;
                    break;
                //In case target is biological; very niche for terran matchups
                //Nuke + ghost snipe considered
                /*
                if(unit.Current_Energy >= 50)
                {
                    potential_air_damage = temporary_current_air_damage + 300 + 170;
                    potential_ground_damage = temporary_current_ground_damage + 300 + 170;
                }
                break;
                */
                case "TERRAN_CYCLONE": //lockon considered
                    potential_air_damage = temporary_current_air_damage + 400;
                    potential_ground_damage = temporary_current_ground_damage + 400;
                    break;
                case "TERRAN_BATTLECRUISER": //yamato cannon considered
                    potential_air_damage = temporary_current_air_damage + 240;
                    potential_ground_damage = temporary_current_ground_damage + 240;
                    break;
            }
            return new Tuple<double, double>(potential_air_damage, potential_ground_damage);
        }

        public static double GetMaximumPotentialAirDamage(this Unit unit)
        {
            return 0;
        }

        public static double GetMaximumPotentialGroundDamage(this Unit unit)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="skill_name"></param>
        public static void UseSkillOrAbilities(this Unit unit, string skill_name) //Only damage dealing/affecting skills considered
        {
            switch(unit.Name)
            {
                case "TERRAN_MARINE":
                    if (skill_name == "EFFECT_STIM") //costs 10hp/sec 
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(11, 11));//Technically no cooldown but recasting refreshes duration (recasting after duration maximizes efficiency)
                    break;
                case "TERRAN_MARAUDER":
                    if (skill_name == "EFFECT_STIM") //costs 20hp/sec 
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(11, 11)); //Technically no cooldown but recasting refreshes duration (recasting after duration maximizes efficiency)
                    break;
                case "TERRAN_REAPER":
                    if (skill_name == "EFFECT_KD8CHARGE")
                    {
                        if (!Unit.Definitions[unit.Target.Name].IsFlying_Unit)
                            unit.Target.Current_Health -= 5;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(14, 0));
                    }
                    break;
                case "TERRAN_GHOST":
                    if (skill_name == "EFFECT_NUKECALLDOWN") //Needs to build a nuke first from Ghost academy
                    {
                        unit.Target.Current_Health -= 300;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(14, 0));
                    }
                    else if (skill_name == "EFFECT_GHOSTSNIPE") //Target needs to be a biological unit and is also rarely used in TvT matchups
                    {
                        unit.Target.Current_Health -= 170;
                        unit.Current_Energy -= 50;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(1.43, 0)); //is a channeled spell but no cooldown; cooldown value here is channeling time; can't recast while channeling; channeling gets canceled when damaged
                    }
                    break;
                case "TERRAN_CYCLONE":
                    if (skill_name == "EFFECT_LOCKON")
                    {
                        unit.Target.Current_Health -= 400;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(4.3, 14.3)); //Deals 400 damage over 14.3 seconds but has shorter cooldown; Note: cancelled if target goes out of range
                    }
                    break;
                case "TERRAN_BATTLECRUISER":
                    if (skill_name == "EFFECT_YAMATOGUN") //needs research from fusion core
                    {
                        unit.Target.Current_Health -= 240;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(71, 2)); //Deals 240 damage over 2 seconds: really strong
                    }
                    break;
            }
        }

        /// <summary>
        /// Deals a constant damage to the opposing unit using
        /// <see cref="Unit.Current_Ground_Damage"/> or <see cref="Unit.Current_Air_Damage"/>
        /// </summary>
        /// <param name="unit"></param>
        public static void AttackTarget(this Unit unit)
        {
            if((!unit.IsOpposingDefeated) && (!unit.IsDefeated))
            {
                var current_percentage = Unit.GetARandomPercentage();

                switch (unit.Name)
                {
                    case "TERRAN_MARINE":
                        if (current_percentage < .20)
                        {
                            unit.Target.Current_Health -= unit.Current_Ground_Damage;
                        }
                        else if ((current_percentage < .40) && (current_percentage >= .20))
                        {
                            unit.UseSkillOrAbilities("EFFECT_STIM");
                        }
                        else if ((current_percentage < .60) && (current_percentage >= .40))
                        {
                            //Other stuff if there is, else normalize the percentages like
                            //for this case, if terran_marine have two only to attack target then
                            //make it 50 50
                            unit.Target.Current_Health -= unit.Current_Ground_Damage;
                        }
                        else
                            unit.Target.Current_Health -= unit.Current_Ground_Damage;
                        break;
                }
            }
        }
    }
}
