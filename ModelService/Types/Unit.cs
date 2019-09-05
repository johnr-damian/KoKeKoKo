using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModelService.Types
{
    /// <summary>
    /// A template for unit to be parsed either from CSV file or from Game observation
    /// </summary>
    public abstract partial class Unit
    {
        /// <summary>
        /// The current target of this unit
        /// </summary>
        protected int _current_target = -1;

        /// <summary>
        /// A list of unit that has been targeted by this unit
        /// </summary>
        protected List<Unit> _targets { get; set; } = null;

        #region Properties From Source
        /// <summary>
        /// A unique identifier of this unit
        /// </summary>
        public long UniqueID { get; protected set; } = -1;

        /// <summary>
        /// The player/alliance of this unit
        /// </summary>
        public string Owner { get; protected set; } = "";

        /// <summary>
        /// The unit type name
        /// </summary>
        public string Name { get; protected set; } = "";

        /// <summary>
        /// The current position of this unit
        /// </summary>
        public Coordinate Position { get; protected set; } = null;

        /// <summary>
        /// The current buffs that affect this unit
        /// </summary>
        /// <remarks>
        /// The string stored in this list are the name of buffs found in the API
        /// </remarks>
        public List<string> Buffs { get; protected set; } = null;
        #endregion

        #region Properties From Simulation
        /// <summary>
        /// The original health of this unit
        /// </summary>
        public double Health { get; set; } = -1;

        /// <summary>
        /// The current health of this unit
        /// </summary>
        public double Current_Health { get; set; } = -1;

        /// <summary>
        /// The original energy of this unit
        /// </summary>
        public double Energy { get; set; } = -1;

        /// <summary>
        /// The current energy of this unit
        /// </summary>
        public double Current_Energy { get; set; } = -1;

        /// <summary>
        /// The original armor of this unit
        /// </summary>
        public int Armor { get; set; } = -1;

        /// <summary>
        /// The current armor of this unit
        /// </summary>
        public int Current_Armor { get; set; } = -1;

        /// <summary>
        /// The original ground damage of this unit
        /// </summary>
        public double Ground_Damage { get; set; } = -1;

        /// <summary>
        /// The current ground damage of this unit
        /// </summary>
        public double Current_Ground_Damage { get; set; } = -1;

        /// <summary>
        /// The original air damage of this unit
        /// </summary>
        public double Air_Damage { get; set; } = -1;

        /// <summary>
        /// The current air damage of this unit
        /// </summary>
        public double Current_Air_Damage { get; set; } = -1;

        /// <summary>
        /// If this <see cref="Unit.Current_Health"/> is below or equal 0 in the simulation
        /// </summary>
        public virtual bool IsDead => Current_Health <= 0;

        /// <summary>
        /// The unit to be targeted by this unit
        /// </summary>
        public virtual Unit Target
        {
            get
            {
                return ((_targets == null) || (_targets.Count == 0))? null : _targets[_current_target];
            }
        }

        /// <summary>
        /// If this unit's current target <see cref="Unit.Health"/> is below or equal 0, or if there is no unit to be targeted by this unit
        /// </summary>
        public virtual bool IsTargetDead => (Target == null || Target.IsDead); 
        #endregion

        /// <summary>
        /// Creates an instance of parsed unit with basic information
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public Unit(long uid, string owner, string name, double x, double y, params string[] buffs)
        {
            UniqueID = uid;
            Owner = owner;
            Name = name;
            Position = new Coordinate(x, y);
            Buffs = new List<string>(buffs);

            _targets = new List<Unit>();
        }

        /// <summary>
        /// A method that creates a new instance with the same values of this unit
        /// </summary>
        /// <returns></returns>
        public abstract Unit CreateDeepCopy();

        /// <summary>
        /// Initializes the properties needed for combat simulation from dictionary
        /// and applies necessary buffs from <see cref="Buffs"/>
        /// </summary>
        public virtual void Initialize()
        {
            Health = DEFINITIONS[Name].Item1;
            Energy = DEFINITIONS[Name].Item2;
            Ground_Damage = DEFINITIONS[Name].Item3;
            Air_Damage = DEFINITIONS[Name].Item4;
            Armor = DEFINITIONS[Name].Item5;

            Current_Health = Health;
            Current_Energy = Energy;
            Current_Ground_Damage = Ground_Damage;
            Current_Air_Damage = Air_Damage;
            Current_Armor = Armor;

            ApplyBuffsOrModifiers();
        }

        /// <summary>
        /// Resets the properties with the counterpart values
        /// </summary>
        public virtual void Reset()
        {
            Current_Health = Health;
            Current_Energy = Energy;
            Current_Ground_Damage = Ground_Damage;
            Current_Air_Damage = Air_Damage;
            Current_Armor = Armor;
        }

        /// <summary>
        /// Checks if the target to be set can be a valid target for this unit
        /// </summary>
        /// <param name="target_unit"></param>
        /// <returns></returns>
        public virtual bool CanTarget(Unit target_unit)
        {
            bool target_is_flying_unit = DEFINITIONS[target_unit.Name].Item6;

            return (target_is_flying_unit) ? (Air_Damage > 0) : (Ground_Damage > 0);
        }

        /// <summary>
        /// Adds a target for this unit
        /// </summary>
        /// <param name="target_unit"></param>
        public virtual void SetTarget(Unit target_unit)
        {
            _current_target++;
            _targets.Add(target_unit);
        }

        /// <summary>
        /// This is to be use when there are static buffs in the <see cref="Buffs"/>
        /// so it can be use to initialize values
        /// </summary>
        /// <remarks>
        /// This is to be use to apply static buffs or modifiers
        /// for example, increasing health, applying armor before start of battle
        /// and etc
        /// </remarks>
        public virtual void ApplyBuffsOrModifiers()
        {
            foreach (string buff in Buffs)
            {
                switch (buff)
                {
                    case "TERRANINFANTRYWEAPONSLEVEL1":
                        if (Name == "TERRAN_MARINE")
                        {
                            Current_Ground_Damage= Unit.DEFINITIONS[Name].Item3 + 1.6;
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 1.6;
                        }
                        else if(Name == "TERRAN_REAPER")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 2.5;
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 0.93;
                        else if(Name == "TERRAN_GHOST")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 0.93;
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 0.93;
                        }
                        break;
                    case "TERRANINFANTRYWEAPONSLEVEL2":
                        if (Name == "TERRAN_MARINE")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.6*2);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.6 * 2);
                        }
                        else if (Name == "TERRAN_REAPER")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.5*2);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (0.93*2);
                        else if (Name == "TERRAN_GHOST")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (0.93 * 2);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (0.93 * 2);
                        }
                        break;
                    case "TERRANINFANTRYWEAPONSLEVEL3":
                        if (Name == "TERRAN_MARINE")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.6 * 3);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.6 * 3);
                        }
                        else if (Name == "TERRAN_REAPER")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.5 * 3);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (0.93 * 3);
                        else if (Name == "TERRAN_GHOST")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (0.93 * 3);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (0.93 * 3);
                        }
                        break;
                    case "TERRANINFANTRYARMORSLEVEL1":
                        if (Name == "TERRAN_MARINE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_REAPER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_GHOST")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        break;
                    case "TERRANINFANTRYARMORSLEVEL2":
                        if (Name == "TERRAN_MARINE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*2);
                        else if (Name == "TERRAN_REAPER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*2);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*2);
                        else if (Name == "TERRAN_GHOST")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*2);
                        break;
                    case "TERRANINFANTRYARMORSLEVEL3":
                        if (Name == "TERRAN_MARINE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*3);
                        else if (Name == "TERRAN_REAPER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*3);
                        else if (Name == "TERRAN_MARAUDER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*3);
                        else if (Name == "TERRAN_GHOST")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1*3);
                        break;
                    case "TERRANVEHICLEWEAPONSLEVEL1":
                        if (Name == "TERRAN_HELLION")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 0.56;
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 2.7;
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 1.87;
                        else if (Name == "TERRAN_CYCLONE")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 2.8;
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 2.8;
                        }
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 1.4;
                        else if (Name == "TERRAN_THOR")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 6.59;
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 1.87;
                        }
                        else if (Name == "TERRAN_THORAP")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 6.59;
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 2.3;
                        }
                        break;
                    case "TERRANVEHICLEWEAPONSLEVEL2":
                        if (Name == "TERRAN_HELLION")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (0.56*2);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.7*2);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.87*2);
                        else if (Name == "TERRAN_CYCLONE")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.8*2);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (2.8*2);
                        }
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.4*2);
                        else if (Name == "TERRAN_THOR")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (6.59*2);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.87*2);
                        }
                        else if (Name == "TERRAN_THORAP")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (6.59*2);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (2.3*2);
                        }
                        break;
                    case "TERRANVEHICLEWEAPONSLEVEL3":
                        if (Name == "TERRAN_HELLION")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (0.56 * 3);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.7 * 3);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.87 * 3);
                        else if (Name == "TERRAN_CYCLONE")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.8 * 3);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (2.8 * 2);
                        }
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.4 * 3);
                        else if (Name == "TERRAN_THOR")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (6.59 * 3);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.87 * 2);
                        }
                        else if (Name == "TERRAN_THORAP")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (6.59 * 3);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (2.3 * 3);
                        }
                        break;
                    case "TERRANSHIPWEAPONSLEVEL1":
                        if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 1.4;
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 1.4;
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 1.4;
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 4.4;
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 2.25;
                        else if (Name == "TERRAN_BATTLECRUISER")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 6.2;
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + 6.2;
                        }
                        break;
                    case "TERRANSHIPWEAPONSLEVEL2":
                        if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.4*2);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.4 * 2);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.4 * 2);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (4.4*2);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.25*2);
                        else if (Name == "TERRAN_BATTLECRUISER")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (6.2*2);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (6.2*2);
                        }
                        break;
                    case "TERRANSHIPWEAPONSLEVEL3":
                        if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (1.4 * 3);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.4 * 3);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (1.4 * 3);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (4.4 * 3);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (2.25 * 3);
                        else if (Name == "TERRAN_BATTLECRUISER")
                        {
                            Current_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + (6.2 * 3);
                            Current_Air_Damage = Unit.DEFINITIONS[Name].Item4 + (6.2 * 3);
                        }
                        break;
                    case "TERRANVEHICLEANDSHIPARMORSLEVEL1":
                        if (Name == "TERRAN_HELLION")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_WIDOWMINE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_CYCLONE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_THOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_THORAP")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_MEDIVAC")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_RAVEN")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        else if (Name == "TERRAN_BATTLECRUISER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 1;
                        break;
                    case "TERRANVEHICLEANDSHIPARMORSLEVEL2":
                        if (Name == "TERRAN_HELLION")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_WIDOWMINE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_CYCLONE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_THOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_THORAP")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_MEDIVAC")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_RAVEN")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        else if (Name == "TERRAN_BATTLECRUISER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 2);
                        break;
                    case "TERRANVEHICLEANDSHIPARMORSLEVEL3":
                        if (Name == "TERRAN_HELLION")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_WIDOWMINE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_SIEGETANK")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_SIEGETANKSIEGED")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_CYCLONE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_HELLIONTANK")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_THOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_THORAP")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_VIKINGASSAULT")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_VIKINGFIGHTER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_MEDIVAC")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_LIBERATOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_LIBERATORAG")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_RAVEN")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_BANSHEE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        else if (Name == "TERRAN_BATTLECRUISER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + (1 * 3);
                        break;
                    case "NEOSTEELFRAME":
                        if (Name == "TERRAN_COMMANDCENTER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_PLANETARYFORTRESS")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_ORBITALCOMMAND")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_SUPPLYDEPOT")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_REFINERY")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_BARRACKS")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_BARRACKSREACTOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_BARRACKSTECHLAB")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_ENGINEERINGBAY")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_BUNKER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_SENSORTOWER")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_MISSILETURRET")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_FACTORY")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_FACTORYREACTOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_FACTORYTECHLAB")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_GHOSTACADEMY")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_STARPORT")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_STARPORTREACTOR")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_STARPORTTECHLAB")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_ARMORY")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        else if (Name == "TERRAN_FUSIONCORE")
                            Current_Armor = Unit.DEFINITIONS[Name].Item5 + 2;
                        break;
                    default:
                        break;
                }
            }
            //TODO
        }

        /// <summary>
        /// This is to be use when using a reusable buffs or modifiers.
        /// Usually this is a skill that affects this unit's properties
        /// </summary>
        public virtual void UseBuffsOrModifiers(string buff_name)
        {
            //TODO
        }

        /// <summary>
        /// This is to be use when using a reusable buffs or modifiers.
        /// Usually this is a skill like single-target skill
        /// </summary>
        public virtual T UseBuffsOrModifiers<T>(string buff_name, T value)
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deals damage to this unit's target with buffs or other modifiers
        /// </summary>
        /// <returns></returns>
        public virtual bool AttackTarget()
        {
            double minimum_potential_damage = 0, maximum_potential_damage = 0;

            try
            {
                if (!(IsDead || IsTargetDead))
                {
                    //TODO
                    return Target.ReceiveAttackFromTarget(minimum_potential_damage);
                }
                else
                    throw new InvalidOperationException("This unit is either dead, or the target has been killed...");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to attack the target...");
                Trace.WriteLine($@"Error in Model! Unit -> AttackTarget(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Receives the opposing unit's damage
        /// </summary>
        /// <param name="damage_to_receive"></param>
        /// <returns></returns>
        public virtual bool ReceiveAttackFromTarget(double damage_to_receive)
        {
            try
            {
                //TODO

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to receive the attack from target...");
                Trace.WriteLine($@"Error in Model! Unit -> ReceiveAttackFromTarget(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Returns a string that can be use in messaging to agent to command what to do
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is the one to be used for messaging to agent. Problem is
        /// Should we include the sequence of action done to target or 
        /// we should just let it play randomly. 
        /// 
        /// If we include sequence, then we need a way to get that sequence of
        /// action
        /// </remarks>
        public override string ToString()
        {
            string message = "";

            message += String.Format($@"{UniqueID},{Owner},{Name}");
            foreach (var target in _targets)
                message += String.Format($@",{target.UniqueID}");

            return message;
        }
    }
}
