using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Types
{
    /// <summary>
    /// A class for parsed unit that holds basic information with basic methods.
    /// As such, this class only holds methods for initializations and getting of values
    /// </summary>
    public partial class Unit : IDefeatable, ICopyable<Unit>
    {
        #region Private Properties
        /// <summary>
        /// The current target of this unit
        /// </summary>
        private int Current_Target { get; set; } = default(int);

        /// <summary>
        /// The list of units that has been targeted by this unit
        /// </summary>
        private List<Unit> Targets { get; set; } = default(List<Unit>);
        #endregion

        #region Properties From Source
        /// <summary>
        /// The game loop where this unit is found to be in battle
        /// </summary>
        public long Timestamp { get; private set; } = default(long);

        /// <summary>
        /// The player/alliance who controls this unit
        /// </summary>
        public string Owner { get; private set; } = default(string);

        /// <summary>
        /// A unique identifier for this unit
        /// </summary>
        public string UniqueID { get; private set; } = default(string);

        /// <summary>
        /// The unit type of this unit
        /// </summary>
        public string Name { get; private set; } = default(string);

        /// <summary>
        /// The position of the unit where it is found to be in battle
        /// </summary>
        public Coordinate Position { get; private set; } = default(Coordinate);

        /// <summary>
        /// The current buffs that affect this unit
        /// </summary>
        public List<string> Buffs { get; private set; } = default(List<string>);
        #endregion

        #region Properties From Simulation
        /// <summary>
        /// The original health of this unit
        /// </summary>
        public double Health { get; private set; } = default(double);

        /// <summary>
        /// The current health of this unit
        /// </summary>
        public double Current_Health { get; set; } = default(double);

        /// <summary>
        /// The original energy of this unit
        /// </summary>
        public double Energy { get; private set; } = default(double);

        /// <summary>
        /// The current energy of this unit
        /// </summary>
        public double Current_Energy { get; set; } = default(double);

        /// <summary>
        /// The original armor of this unit
        /// </summary>
        public double Armor { get; private set; } = default(double);

        /// <summary>
        /// The current armor of this unit
        /// </summary>
        public double Current_Armor { get; set; } = default(double);

        /// <summary>
        /// The original ground damage of this unit
        /// </summary>
        public double Ground_Damage { get; private set; } = default(double);

        /// <summary>
        /// The current ground damage of this unit
        /// </summary>
        public double Current_Ground_Damage { get; set; } = default(double);

        /// <summary>
        /// The original air damage of this unit
        /// </summary>
        public double Air_Damage { get; private set; } = default(double);

        /// <summary>
        /// The current air damage of this unit
        /// </summary>
        public double Current_Air_Damage { get; set; } = default(double);

        /// <summary>
        /// A list of skills that was activated/used
        /// </summary>
        public Dictionary<string, UnitSkills> Activated_Skills { get; set; } = default(Dictionary<string, UnitSkills>);

        /// <summary>
        /// The current opposing unit that is targeted by this unit
        /// </summary>
        public Unit Target => ((Targets.Count == 0) ? null : Targets[Current_Target]);

        /// <summary>
        /// If this unit's <see cref="Current_Health"/> is below or equal 0
        /// </summary>
        public bool IsDefeated => (Current_Health <= 0);

        /// <summary>
        /// If this <see cref="Target"/>'s <see cref="Current_Health"/> is below or equal 0, or has no target at all
        /// </summary>
        public bool IsOpposingDefeated => ((Target == null) || Target.IsDefeated);
        #endregion

        /// <summary>
        /// Creates an instance of parsed unit with basic information, and then
        /// initializes all properties of unit
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="owner"></param>
        /// <param name="unique_id"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public Unit(long timestamp, string owner, string unique_id, string name, double x, double y, params string[] buffs)
        {
            try
            {
                //Private field initializations
                Current_Target = -1;
                Targets = new List<Unit>();

                //Initialize the fields from source
                Timestamp = timestamp;
                Owner = owner;
                UniqueID = unique_id;
                Name = name;
                Position = new Coordinate(x, y);
                Buffs = new List<string>(buffs);

                //Initialize the from simulation
                if (Name == null)
                    return;
                Current_Health = Health = Definitions[Name].Health;
                Current_Energy = Energy = Definitions[Name].Energy;
                Current_Ground_Damage = Ground_Damage = Definitions[Name].Ground_Damage;
                Current_Air_Damage = Air_Damage = Definitions[Name].Air_Damage;
                Current_Armor = Armor = Definitions[Name].Armor;
                Activated_Skills = new Dictionary<string, UnitSkills>();

                //Apply the applicable new values from permanent buffs
                ApplyApplicableUpgrades();
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($@"Unit [{Name}]-> {ex.Message}");
                throw new Exception("Unit from Army Repository not found in Unit Dictionaries");
            }
        }

        /// <summary>
        /// Creates an instance of parsed unit with basic information, and then
        /// initializes all properties of unit
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="owner"></param>
        /// <param name="unique_id"></param>
        /// <param name="name"></param>
        /// <param name="coordinate"></param>
        /// <param name="buffs"></param>
        public Unit(long timestamp, string owner, string unique_id, string name, Coordinate coordinate, params string[] buffs)
            : this(timestamp, owner, unique_id, name, coordinate.X, coordinate.Y, buffs) { }

        #region Methods for Applying Relevant Properties
        /// <summary>
        /// Use to apply the applicable permanent buffs to this unit that
        /// affects the original values of this unit.
        /// 
        /// 
        /// </summary>
        private void ApplyApplicableUpgrades()
        {
            foreach (string buff in Buffs)
            {
                switch (buff)
                {
                    case "TERRANINFANTRYWEAPONSLEVEL1":
                        if (Name == "TERRAN_MARINE")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 1.6;
                            Current_Air_Damage = Definitions[Name].Air_Damage + 1.6;
                        }
                        else if (Name == "TERRAN_REAPER")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 2.5;
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 0.93;
                        else if (Name == "TERRAN_GHOST")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 0.93;
                            Current_Air_Damage = Definitions[Name].Air_Damage + 0.93;
                        }
                        break;
                    case "TERRANINFANTRYWEAPONSLEVEL2":
                        if (Name == "TERRAN_MARINE")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.6 * 2);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.6 * 2);
                        }
                        else if (Name == "TERRAN_REAPER")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.5 * 2);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (0.93 * 2);
                        else if (Name == "TERRAN_GHOST")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (0.93 * 2);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (0.93 * 2);
                        }
                        break;
                    case "TERRANINFANTRYWEAPONSLEVEL3":
                        if (Name == "TERRAN_MARINE")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.6 * 3);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.6 * 3);
                        }
                        else if (Name == "TERRAN_REAPER")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.5 * 3);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (0.93 * 3);
                        else if (Name == "TERRAN_GHOST")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (0.93 * 3);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (0.93 * 3);
                        }
                        break;
                    case "TERRANINFANTRYARMORSLEVEL1":
                        if (Name == "TERRAN_MARINE")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_REAPER")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_GHOST")
                            Current_Armor = Definitions[Name].Armor + 1;
                        break;
                    case "TERRANINFANTRYARMORSLEVEL2":
                        if (Name == "TERRAN_MARINE")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_REAPER")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_GHOST")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        break;
                    case "TERRANINFANTRYARMORSLEVEL3":
                        if (Name == "TERRAN_MARINE")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_REAPER")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_GHOST")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        break;
                    case "TERRANVEHICLEWEAPONSLEVEL1":
                        if (Name == "TERRAN_HELLION")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 0.56;
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 2.7;
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 1.87;
                        else if (Name == "TERRAN_CYCLONE")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 2.8;
                            Current_Air_Damage = Definitions[Name].Air_Damage + 2.8;
                        }
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 1.4;
                        else if (Name == "TERRAN_THOR")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 6.59;
                            Current_Air_Damage = Definitions[Name].Air_Damage + 1.87;
                        }
                        else if (Name == "TERRAN_THORAP")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 6.59;
                            Current_Air_Damage = Definitions[Name].Air_Damage + 2.3;
                        }
                        break;
                    case "TERRANVEHICLEWEAPONSLEVEL2":
                        if (Name == "TERRAN_HELLION")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (0.56 * 2);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.7 * 2);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.87 * 2);
                        else if (Name == "TERRAN_CYCLONE")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.8 * 2);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (2.8 * 2);
                        }
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.4 * 2);
                        else if (Name == "TERRAN_THOR")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (6.59 * 2);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.87 * 2);
                        }
                        else if (Name == "TERRAN_THORAP")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (6.59 * 2);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (2.3 * 2);
                        }
                        break;
                    case "TERRANVEHICLEWEAPONSLEVEL3":
                        if (Name == "TERRAN_HELLION")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (0.56 * 3);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.7 * 3);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.87 * 3);
                        else if (Name == "TERRAN_CYCLONE")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.8 * 3);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (2.8 * 2);
                        }
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.4 * 3);
                        else if (Name == "TERRAN_THOR")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (6.59 * 3);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.87 * 2);
                        }
                        else if (Name == "TERRAN_THORAP")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (6.59 * 3);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (2.3 * 3);
                        }
                        break;
                    case "TERRANSHIPWEAPONSLEVEL1":
                        if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 1.4;
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Air_Damage = Definitions[Name].Air_Damage + 1.4;
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Air_Damage = Definitions[Name].Air_Damage + 1.4;
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 4.4;
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 2.25;
                        else if (Name == "TERRAN_BATTLECRUISER")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + 6.2;
                            Current_Air_Damage = Definitions[Name].Air_Damage + 6.2;
                        }
                        break;
                    case "TERRANSHIPWEAPONSLEVEL2":
                        if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.4 * 2);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.4 * 2);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.4 * 2);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (4.4 * 2);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.25 * 2);
                        else if (Name == "TERRAN_BATTLECRUISER")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (6.2 * 2);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (6.2 * 2);
                        }
                        break;
                    case "TERRANSHIPWEAPONSLEVEL3":
                        if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (1.4 * 3);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.4 * 3);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Air_Damage = Definitions[Name].Air_Damage + (1.4 * 3);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (4.4 * 3);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (2.25 * 3);
                        else if (Name == "TERRAN_BATTLECRUISER")
                        {
                            Current_Ground_Damage = Definitions[Name].Ground_Damage + (6.2 * 3);
                            Current_Air_Damage = Definitions[Name].Air_Damage + (6.2 * 3);
                        }
                        break;
                    case "TERRANVEHICLEANDSHIPARMORSLEVEL1":
                        if (Name == "TERRAN_HELLION")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_WIDOWMINE")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_CYCLONE")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_THOR")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_THORAP")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_MEDIVAC")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_RAVEN")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Armor = Definitions[Name].Armor + 1;
                        else if (Name == "TERRAN_BATTLECRUISER")
                            Current_Armor = Definitions[Name].Armor + 1;
                        break;
                    case "TERRANVEHICLEANDSHIPARMORSLEVEL2":
                        if (Name == "TERRAN_HELLION")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_WIDOWMINE")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_CYCLONE")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_THOR")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_THORAP")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_MEDIVAC")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_RAVEN")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        else if (Name == "TERRAN_BATTLECRUISER")
                            Current_Armor = Definitions[Name].Armor + (1 * 2);
                        break;
                    case "TERRANVEHICLEANDSHIPARMORSLEVEL3":
                        if (Name == "TERRAN_HELLION")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_WIDOWMINE")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_CYCLONE")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_THOR")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_THORAP")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_MEDIVAC")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_RAVEN")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        else if (Name == "TERRAN_BATTLECRUISER")
                            Current_Armor = Definitions[Name].Armor + (1 * 3);
                        break;
                    case "NEOSTEELFRAME":
                        if (Name == "TERRAN_COMMANDCENTER")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_PLANETARYFORTRESS")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_ORBITALCOMMAND")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_SUPPLYDEPOT")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_REFINERY")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_BARRACKS")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_BARRACKSREACTOR")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_BARRACKSTECHLAB")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_ENGINEERINGBAY")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_BUNKER")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_SENSORTOWER")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_MISSILETURRET")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_FACTORY")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_FACTORYREACTOR")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_FACTORYTECHLAB")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_GHOSTACADEMY")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_STARPORT")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_STARPORTREACTOR")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_STARPORTTECHLAB")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_ARMORY")
                            Current_Armor = Definitions[Name].Armor + 2;
                        else if (Name == "TERRAN_FUSIONCORE")
                            Current_Armor = Definitions[Name].Armor + 2;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Used to apply the armor of the target for the <see cref="Current_Air_Damage"/>
        /// of this unit
        /// </summary>
        /// <returns></returns>
        private double ApplyArmorToCurrentAirDamage() => ApplyArmorToCurrentAirDamage(Current_Air_Damage);

        /// <summary>
        /// Used to apply the armor of the target for the given  
        /// current air damage of this unit
        /// </summary>
        /// <returns></returns>
        private double ApplyArmorToCurrentAirDamage(double current_air_damage)
        {
            double true_airdamage = 0;

            try
            {
                switch (Name)
                {
                    case "TERRAN_MARINE": true_airdamage = (current_air_damage - (1.6 * Target.Current_Armor)); break;

                    case "TERRAN_WIDOWMINE": true_airdamage = (current_air_damage - Target.Current_Armor); break;

                    case "TERRAN_GHOST": true_airdamage = (current_air_damage - (0.93 * Target.Current_Armor)); break;

                    case "TERRAN_CYCLONE": true_airdamage = (current_air_damage - (1.26 * Target.Current_Armor)); break;

                    case "TERRAN_THOR": true_airdamage = (current_air_damage - (1.87 * Target.Current_Armor)); break;

                    case "TERRAN_THORAP": true_airdamage = (current_air_damage - (0.59 * Target.Current_Armor)); break;

                    case "TERRAN_AUTOTURRET": true_airdamage = (current_air_damage - (1.76 * Target.Current_Armor)); break;

                    case "TERRAN_VIKINGFIGHTER": true_airdamage = (current_air_damage - (1.4 * Target.Current_Armor)); break;

                    case "TERRAN_LIBERATOR": true_airdamage = (current_air_damage - (1.4 * Target.Current_Armor)); break;

                    case "TERRAN_BATTLECRUISER": true_airdamage = (current_air_damage - (6.2 * Target.Current_Armor)); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ApplyArmorToCurrentAirDamage() -> {ex.Message}");
                true_airdamage = current_air_damage;
            }

            return true_airdamage;
        }

        /// <summary>
        /// Used to apply the armor of the target for the <see cref="Current_Ground_Damage"/>
        /// of this unit
        /// </summary>
        /// <returns></returns>
        private double ApplyArmorToCurrentGroundDamage() => ApplyArmorToCurrentGroundDamage(Current_Ground_Damage);

        /// <summary>
        /// Used to apply the armor of the target for the given
        /// current ground damage of this unit
        /// </summary>
        /// <returns></returns>
        private double ApplyArmorToCurrentGroundDamage(double current_ground_damage)
        {
            double true_grounddamage = 0;

            try
            {
                switch (Name)
                {
                    case "TERRAN_MARINE": true_grounddamage = (current_ground_damage - (1.6 * Target.Current_Armor)); break;

                    case "TERRAN_WIDOWMINE": true_grounddamage = (current_ground_damage - Target.Current_Armor); break;

                    case "TERRAN_SCV": true_grounddamage = (current_ground_damage - (0.93 * Target.Current_Armor)); break;

                    case "TERRAN_REAPER": true_grounddamage = (current_ground_damage - (2.5 * Target.Current_Armor)); break;

                    case "TERRAN_MARAUDER": true_grounddamage = (current_ground_damage - (0.93 * Target.Current_Armor)); break;

                    case "TERRAN_GHOST": true_grounddamage = (current_ground_damage - (0.93 * Target.Current_Armor)); break;

                    case "TERRAN_HELLION": true_grounddamage = (current_ground_damage - (0.56 * Target.Current_Armor)); break;

                    case "TERRAN_SIEGETANK": true_grounddamage = (current_ground_damage - (1.35 * Target.Current_Armor)); break;

                    case "TERRAN_SIEGETANKSIEGED": true_grounddamage = (current_ground_damage - (0.47 * Target.Current_Armor)); break;

                    case "TERRAN_CYCLONE": true_grounddamage = (current_ground_damage - (1.26 * Target.Current_Armor)); break;

                    case "TERRAN_HELLIONTANK": true_grounddamage = (current_ground_damage - (0.71 * Target.Current_Armor)); break;

                    case "TERRAN_THOR": true_grounddamage = (current_ground_damage - (2.16 * Target.Current_Armor)); break;

                    case "TERRAN_THORAP": true_grounddamage = (current_ground_damage - (2.16 * Target.Current_Armor)); break;

                    case "TERRAN_AUTOTURRET": true_grounddamage = (current_ground_damage - (1.76 * Target.Current_Armor)); break;

                    case "TERRAN_VIKINGASSAULT": true_grounddamage = (current_ground_damage - (1.4 * Target.Current_Armor)); break;

                    case "TERRAN_LIBERATORAG": true_grounddamage = (current_ground_damage - (0.81 * Target.Current_Armor)); break;

                    case "TERRAN_BANSHEE": true_grounddamage = (current_ground_damage - (2.25 * Target.Current_Armor)); break;

                    case "TERRAN_BATTLECRUISER": true_grounddamage = (current_ground_damage - (6.2 * Target.Current_Armor)); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ApplyArmorToCurrentGroundDamage() -> {ex.Message}");
                true_grounddamage = current_ground_damage;
            }

            return true_grounddamage;
        } 
        #endregion

        #region Methods for Target
        /// <summary>
        /// Checks if the opposing unit can be targeted by this unit by checking the <see cref="Air_Damage"/> or <see cref="Ground_Damage"/>
        /// </summary>
        /// <param name="targeted_unit"></param>
        /// <returns></returns>
        public bool CanTarget(Unit targeted_unit) => (Definitions[targeted_unit.Name].IsFlying_Unit ? (Air_Damage > 0) : (Ground_Damage > 0));

        /// <summary>
        /// Increments the pointer for the list of targets and adds a new target to the list
        /// </summary>
        /// <param name="targeted_unit"></param>
        public void SetTarget(Unit targeted_unit)
        {
            Current_Target++;
            Targets.Add(targeted_unit);
        }
        #endregion

        #region Utilities Method
        /// <summary>
        /// Returns the unique id of this unit within quotation marks
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Format($@"""{UniqueID}""");

        /// <summary>
        /// Returns the instance of this unit
        /// </summary>
        /// <returns></returns>
        public Unit GetShallowCopy() => this;

        /// <summary>
        /// Returns a new instance with the same values of this unit execpt the <see cref="Targets"/>
        /// </summary>
        /// <returns></returns>
        public Unit GetDeepCopy()
        {
            var new_unit = new Unit(Timestamp, String.Copy(Owner), String.Copy(UniqueID), String.Copy(Name), Position, (from buff in Buffs select String.Copy(buff)).ToArray())
            {
                Health = this.Health,
                Energy = this.Energy,
                Armor = this.Armor,
                Ground_Damage = this.Ground_Damage,
                Air_Damage = this.Air_Damage,

                Current_Health = this.Current_Health,
                Current_Energy = this.Current_Energy,
                Current_Armor = this.Current_Armor,
                Current_Ground_Damage = this.Current_Ground_Damage,
                Current_Air_Damage = this.Current_Air_Damage
            };

            return new_unit;
        } 
        #endregion
    }
}