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
            ["TERRAN_WIDOWMINE"] = new UnitDefinitions(90, 0, 0, 0, 0, false),
            ["TERRAN_WIDOWMINEBURROWED"] = new UnitDefinitions(90, 0, 125, 125, 0, false),
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
            ["TERRAN_VIKINGASSAULT"] = new UnitDefinitions(135, 0, 16.8, 0, 0, false),
            ["TERRAN_VIKINGFIGHTER"] = new UnitDefinitions(135, 0, 0, 14, 0, true),
            ["TERRAN_MEDIVAC"] = new UnitDefinitions(150, 200, 0, 0, 1, true), //50/200
            ["TERRAN_LIBERATORAG"] = new UnitDefinitions(180, 0, 65.8, 0, 0, true),
            ["TERRAN_LIBERATOR"] = new UnitDefinitions(180, 0, 0, 7.8, 0, true),
            ["TERRAN_RAVEN"] = new UnitDefinitions(140, 200, 0, 0, 1, true), //50(+25)/200
            ["TERRAN_BANSHEE"] = new UnitDefinitions(140, 200, 27, 0, 0, true),
            ["TERRAN_BATTLECRUISER"] = new UnitDefinitions(550, 200, 49.8, 31.1, 3, true),
            //Summoned Units
            ["TERRAN_MULE"] = new UnitDefinitions(60, 0, 0, 0, 0, false),
            ["TERRAN_AUTOTURRET"] = new UnitDefinitions(150, 0, 31.58, 31.58, 1, false), //considered a buidling
            ["TERRAN_NUKE"] = new UnitDefinitions(0, 0, 300, 300, 0, false),
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
            ["TERRAN_ARMORY"] = new UnitDefinitions(750, 0, 0, 0, 1, false),
            ["TERRAN_FUSIONCORE"] = new UnitDefinitions(750, 0, 0, 0, 1, false)
        };

        /// <summary>
        /// The base values for a unit
        /// </summary>
        public static Dictionary<string, CostWorth> Values = new Dictionary<string, CostWorth>()
        {
            //Ground Units
            ["TERRAN_WIDOWMINE"] = new CostWorth(19, 75, 25, 2),
            ["TERRAN_WIDOWMINEBURROWED"] = new CostWorth(20, 75, 25, 2),
            ["TERRAN_SCV"] = new CostWorth(20, 50, 0, 1),
            ["TERRAN_MARINE"] = new CostWorth(20, 50, 0, 1),
            ["TERRAN_MARAUDER"] = new CostWorth(20, 100, 25, 2),
            ["TERRAN_REAPER"] = new CostWorth(20, 50, 50, 1),
            ["TERRAN_GHOST"] = new CostWorth(20, 150, 125, 2),
            ["TERRAN_HELLION"] = new CostWorth(20, 100, 0, 2),
            ["TERRAN_HELLIONTANK"] = new CostWorth(20, 100, 0, 2),
            ["TERRAN_SIEGETANK"] = new CostWorth(20, 150, 125, 3),
            ["TERRAN_SIEGETANKSIEGED"] = new CostWorth(20, 150, 125, 3),
            ["TERRAN_CYCLONE"] = new CostWorth(20, 150, 100, 3),
            ["TERRAN_THOR"] = new CostWorth(20, 300, 200, 6),
            ["TERRAN_THORAP"] = new CostWorth(20, 300, 200, 6),
            ["TERRAN_AUTOTURRET"] = new CostWorth(20, 0, 0, 0),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new CostWorth(20, 150, 75, 2),
            ["TERRAN_VIKINGFIGHTER"] = new CostWorth(20, 150, 75, 2),
            ["TERRAN_MEDIVAC"] = new CostWorth(20, 100, 100, 2),
            ["TERRAN_LIBERATORAG"] = new CostWorth(20, 150, 150, 3),
            ["TERRAN_LIBERATOR"] = new CostWorth(20, 150, 150, 3),
            ["TERRAN_RAVEN"] = new CostWorth(20, 100, 200, 2),
            ["TERRAN_BANSHEE"] = new CostWorth(20, 150, 100, 3),
            ["TERRAN_BATTLECRUISERr"] = new CostWorth(20, 400, 300, 6),
            //Summoned Units
            ["TERRAN_MULE"] = new CostWorth(20, 0, 0, 0),
            ["TERRAN_AUTOTURRET"] = new CostWorth(20, 0, 0, 0), //considered a buidling
            ["TERRAN_NUKE"] = new CostWorth(0, 100, 100, 0),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new CostWorth(20, 550, 150, 0),
            ["TERRAN_BUNKER"] = new CostWorth(20, 100, 0, 0),
            ["TERRAN_MISSILETURRET"] = new CostWorth(19, 100, 0, 0),
            ["TERRAN_COMMANDCENTER"] = new CostWorth(11, 400, 0, 0),
            ["TERRAN_ORBITALCOMMAND"] = new CostWorth(11, 550, 0, 0),
            ["TERRAN_SUPPLYDEPOT"] = new CostWorth(11, 100, 0, 0),
            ["TERRAN_REFINERY"] = new CostWorth(11, 75, 0, 0),
            ["TERRAN_BARRACKS"] = new CostWorth(11, 150, 0, 0),
            ["TERRAN_ENGINEERINGBAY"] = new CostWorth(11, 125, 0, 0),
            ["TERRAN_BUNKER"] = new CostWorth(11, 100, 0, 0),
            ["TERRAN_SENSORTOWER"] = new CostWorth(11, 125, 100, 0),
            ["TERRAN_FACTORY"] = new CostWorth(11, 150, 100, 0),
            ["TERRAN_GHOSTACADEMY"] = new CostWorth(11, 150, 100, 0),
            ["TERRAN_STARPORT"] = new CostWorth(11, 150, 100, 0),
            ["TERRAN_ARMORY"] = new CostWorth(11, 150, 100, 0),
            ["TERRAN_FUSIONCORE"] = new CostWorth(11, 150, 150, 0)
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

        /// <summary>
        /// Returns either the <see cref="GetMinimumPotentialAirDamage(Unit)"/>, or the 
        /// <see cref="GetMinimumPotentialGroundDamage(Unit)"/> depending on the current
        /// target of this unit whether it is a flying unit or not.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetMinimumPotentialDamage(Unit unit) => ((Unit.Definitions[unit.Target.Name].IsFlying_Unit) ? GetMinimumPotentialAirDamage(unit) : GetMinimumPotentialGroundDamage(unit));

        /// <summary>
        /// Returns the <see cref="ApplyArmorToCurrentAirDamage()"/> that depends on the 
        /// current target's <see cref="Name"/> of this unit
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetMinimumPotentialAirDamage(Unit unit) => unit.ApplyArmorToCurrentAirDamage();

        /// <summary>
        /// Returns the <see cref="ApplyArmorToCurrentGroundDamage()"/> that depends on the
        /// current target's <see cref="Name"/> of this unit
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetMinimumPotentialGroundDamage(Unit unit) => unit.ApplyArmorToCurrentGroundDamage();

        /// <summary>
        /// Returns either the <see cref="GetMaximumPotentialAirDamage(Unit)"/>, or the
        /// <see cref="GetMaximumPotentialGroundDamage(Unit)"/> depending on the current
        /// target of this unit whether it is a flying unit or not.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetMaximumPotentialDamage(Unit unit) => ((Unit.Definitions[unit.Target.Name].IsFlying_Unit) ? Unit.GetMaximumPotentialAirDamage(unit) : Unit.GetMaximumPotentialGroundDamage(unit));

        /// <summary>
        /// Returns the <see cref="UnitExtensions.GetMaximumPotentialAirDamage(Unit)"/> applied with
        /// <see cref="ApplyArmorToCurrentAirDamage(double)"/> depending the current target's <see cref="Name"/>
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetMaximumPotentialAirDamage(Unit unit) => unit.ApplyArmorToCurrentAirDamage(unit.GetMaximumPotentialAirDamage());

        /// <summary>
        /// Returns the <see cref="UnitExtensions.GetMaximumPotentialGroundDamage(Unit)"/> applied with
        /// <see cref="ApplyArmorToCurrentGroundDamage(double)"/> depending the current target's <see cref="Name"/>
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetMaximumPotentialGroundDamage(Unit unit) => unit.ApplyArmorToCurrentGroundDamage(unit.GetMaximumPotentialGroundDamage());
    }

    /// <summary>
    /// A class extension for parsed unit. It holds additional methods that modifies
    /// and adds additional actions for a unit
    /// </summary>
    public static class UnitExtensions
    {
        public static double GetMaximumPotentialAirDamage(this Unit unit)
        {
            double potential_air_damage = -1;
            double temporary_current_air_damage = unit.Current_Air_Damage;

            switch (unit.Name)
            {
                case "TERRAN_MARINE": //stimpack considered
                    potential_air_damage = temporary_current_air_damage + (0.50 * unit.Current_Air_Damage);
                    break;
                case "TERRAN_GHOST": //nuke
                    potential_air_damage = temporary_current_air_damage + 300;
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
                    break;
                case "TERRAN_BATTLECRUISER": //yamato cannon considered
                    potential_air_damage = temporary_current_air_damage + 240;
                    break;
            }
            return potential_air_damage;
        }

        public static double GetMaximumPotentialGroundDamage(this Unit unit)
        {
            double potential_ground_damage = -1;
            double temporary_current_ground_damage = unit.Current_Ground_Damage;

            switch (unit.Name)
            {
                case "TERRAN_MARINE": //stimpack considered
                    potential_ground_damage = temporary_current_ground_damage + (0.50 * unit.Current_Ground_Damage);
                    break;
                case "TERRAN_MARAUDER": //stimpack considered
                    potential_ground_damage = temporary_current_ground_damage + (0.50 * unit.Current_Ground_Damage);
                    break;
                case "TERRAN_REAPER": //kd8 charge
                    potential_ground_damage = temporary_current_ground_damage + 5;
                    break;
                case "TERRAN_GHOST": //nuke
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
                    potential_ground_damage = temporary_current_ground_damage + 400;
                    break;
                case "TERRAN_BATTLECRUISER": //yamato cannon considered
                    potential_ground_damage = temporary_current_ground_damage + 240;
                    break;
            }
            return  potential_ground_damage;
        }

        public static void UseSkillOrAbilities(this Unit unit, string skill_name) //Only damage dealing/affecting skills considered
        {
            switch(unit.Name)
            {
                case "TERRAN_MARINE":
                    if (skill_name == "EFFECT_STIM") //costs 10hp/sec 
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        unit.Current_Ground_Damage = unit.Current_Ground_Damage + (unit.Current_Ground_Damage * 50);
                        unit.Current_Air_Damage = unit.Current_Air_Damage + (unit.Current_Air_Damage * .50);
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(11, 11));//Technically no cooldown but recasting refreshes duration (recasting after duration maximizes efficiency)
                    }
                    break;
                case "TERRAN_MARAUDER":
                    if (skill_name == "EFFECT_STIM") //costs 20hp/sec 
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        unit.Current_Ground_Damage = unit.Current_Ground_Damage + (unit.Current_Ground_Damage * 50);
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(11, 11)); //Technically no cooldown but recasting refreshes duration (recasting after duration maximizes efficiency)
                    }
                    break;
                case "TERRAN_REAPER":
                    if (skill_name == "EFFECT_KD8CHARGE")
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        if (!Unit.Definitions[unit.Target.Name].IsFlying_Unit)
                            unit.Target.Current_Health -= 5;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(14, 0));
                    }
                    break;
                case "TERRAN_GHOST":
                    if (skill_name == "EFFECT_NUKECALLDOWN") //Needs to build a nuke first from Ghost academy
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        unit.Target.Current_Health -= 300;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(14, 0));
                    }
                    else if (skill_name == "EFFECT_GHOSTSNIPE") //Target needs to be a biological unit and is also rarely used in TvT matchups
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        unit.Target.Current_Health -= 170;
                        unit.Current_Energy -= 50;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(1.43, 0)); //is a channeled spell but no cooldown; cooldown value here is channeling time; can't recast while channeling; channeling gets canceled when damaged
                    }
                    break;
                case "TERRAN_CYCLONE":
                    if (skill_name == "EFFECT_LOCKON")
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        unit.Target.Current_Health -= 400;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(4.3, 14.3)); //Deals 400 damage over 14.3 seconds but has shorter cooldown; Note: cancelled if target goes out of range
                    }
                    break;
                case "TERRAN_BATTLECRUISER":
                    if (skill_name == "EFFECT_YAMATOGUN") //needs research from fusion core
                    {
                        if (unit.Activated_Skills.ContainsKey(skill_name))
                            break;
                        unit.Target.Current_Health -= 240;
                        unit.Activated_Skills.Add(skill_name, new UnitSkills(71, 2)); //Deals 240 damage over 2 seconds: really strong
                    }
                    break;
            }
        }

        /// <summary>
        /// Deals a simple damage by <paramref name="dealt_damage"/> to the
        /// current target of this unit
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="dealt_damage"></param>
        public static void SimpleAttackToTarget(this Unit unit, double dealt_damage) => unit.Target.Current_Health -= dealt_damage;

        /// <summary>
        /// Deals <see cref="Unit.GetMinimumPotentialDamage(Unit)"/> to the current target 
        /// of unit. In some cases, instead of attacking, it will use a skill depending
        /// on the probability given in <paramref name="ability_probability"/>
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="ability_probability"></param>
        public static void ComplexAttackToTarget(this Unit unit, double ability_probability)
        {
            if((!unit.IsDefeated) && (!unit.IsOpposingDefeated))
            {
                switch(unit.Name)
                {
                    //Barrack Units
                    case "TERRAN_MARINE":
                        if (ability_probability <= 0.50 || !unit.Buffs.Contains("RESEARCH_STIMPACK"))
                            unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit);
                        else
                            unit.UseSkillOrAbilities("EFFECT_STIM");
                        break;
                    case "TERRAN_MARAUDER":
                        if (ability_probability <= 0.50 || !unit.Buffs.Contains("RESEARCH_STIMPACK"))
                            unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit);
                        else
                            unit.UseSkillOrAbilities("EFFECT_STIM");
                        break;
                    case "TERRAN_REAPER":
                        if (ability_probability <= 0.50)
                            unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit);
                        else
                            unit.UseSkillOrAbilities("EFFECT_KD8CHARGE");
                        break;
                    case "TERRAN_GHOST":
                        if (ability_probability <= 0.33 || (ability_probability <= 50 && !unit.Buffs.Contains("BUILD_NUKE")))
                            unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit);
                        else if ((ability_probability >= 0.34) && (ability_probability <= 0.66))
                            unit.UseSkillOrAbilities("EFFECT_NUKECALLDOWN");
                        else if ((ability_probability >= 0.67 && (unit.Target.Name == "TERRAN_SCV" || unit.Target.Name == "TERRAN_MARINE" || unit.Target.Name == "TERRAN_MARAUDER" || unit.Target.Name == "TERRAN_REAPER" || unit.Target.Name == "TERRAN_GHOST"))
                               ||(ability_probability >= 51 && (unit.Target.Name == "TERRAN_SCV" || unit.Target.Name == "TERRAN_MARINE" || unit.Target.Name == "TERRAN_MARAUDER" || unit.Target.Name == "TERRAN_REAPER" || unit.Target.Name == "TERRAN_GHOST") && !unit.Buffs.Contains("BUILD_NUKE")))
                            unit.UseSkillOrAbilities("EFFECT_GHOSTSNIPE");
                        break;
                    //Factory Units
                    case "TERRAN_HELLION": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_HELLIONTANK": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_SIEGETANK": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_SIEGETANKSIEGED": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_CYCLONE":
                        if (ability_probability <= 0.50)
                            unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit);
                        else
                            unit.UseSkillOrAbilities("EFFECT_LOCKON");
                        break;
                    case "TERRAN_WIDOWMINE": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_WIDOWMINEBURROWED": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_THOR": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_THORAP": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;
                    
                    //Starport Units
                    case "TERRAN_VIKINGFIGHTER": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_VIKINGASSAULT": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    //case "TERAN_MEDIVAC" does not deal damage currently ommited heal property not yet considered and energy requiremens for healing

                    case "TERRAN_LIBERATOR": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_LIBERATORAG": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    //case "TERRAN_RAVEN" doesn't deal damage directly summons an auto turret for damage and other skills are for utility

                    case "TERRAN_AUTOTURRET": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_BANSHEE": unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit); break;

                    case "TERRAN_BATTLECRUISER":
                        if (ability_probability <= 0.50 || !unit.Buffs.Contains("BATTLECRUISERENABLESPECIALIZATIONS"))
                            unit.Target.Current_Health -= Unit.GetMinimumPotentialDamage(unit);
                        else
                            unit.UseSkillOrAbilities("EFFECT_YAMATOGUN");
                        break;
                }
            }
        }
    }
}
