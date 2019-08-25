using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A template for unit to be parsed either from CSV file or Observed string
    /// </summary>
    public abstract class Unit
    {
        /// <summary>
        /// Contains the unit's basic definition needed for combat
        /// </summary>
                                //Unit name,   Health, Energy, Ground Damage, Air Damage, Armor, Is Flying Type Unit, Mineral Cost, Vespene Cost, Supply Cost
        public static Dictionary<string, Tuple<double, double, double, double, int, bool>> UNIT_DEFINITIONS = new Dictionary<string, Tuple<double, double, double, double, int, bool>>()
        {
            //TODO
        };

        /// <summary>
        /// Contains the corresponding reward and priority to the unit
        /// </summary>
                                //Unit name,   Priority, Mineral Cost, Vespene Cost, Supply Cost
        public static Dictionary<string, Tuple<int, double, double, int>> UNIT_VALUE = new Dictionary<string, Tuple<int, double, double, int>>()
        {
            //TODO
            //Ground Units
            ["Widow Mine"] = new Tuple<int, double, double, int>(19, -1, -1, -1),
            ["SCV"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Marine"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Marauder"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Reaper"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Ghost"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Hellion"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Hellbat"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Siege Tank"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Cyclone"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Thor"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Auto-Turret"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            //Air Units
            ["Viking"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Medivac"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Liberator"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Raven"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Banshee"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Battlecruiser"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            //Buildings
            ["Planetary Fortress"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Bunker"] = new Tuple<int, double, double, int>(20, -1, -1, -1),
            ["Missile Turret"] = new Tuple<int, double, double, int>(19, -1, -1, -1),
            ["Command Center"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Orbital Command"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Supply Depot"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Refinery"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Barracks"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Engineering Bay"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Bunker"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Sensor Tower"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Factory"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Ghost Academy"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Starport"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Armory"] = new Tuple<int, double, double, int>(11, -1, -1, -1),
            ["Fusion Core"] = new Tuple<int, double, double, int>(11, -1, -1, -1)
        };


        public static double GetCorrespondingBuffValue(string buff)
        {
            //TODO
            throw new NotImplementedException();
        }

        #region Properties From Source
        /// <summary>
        /// The controller/alliance of this unit
        /// </summary>
        public string Owner { get; protected set; } = "";

        /// <summary>
        /// The common unit type name
        /// </summary>
        public string Name { get; protected set; } = "";

        /// <summary>
        /// The current position of this unit based on game observation
        /// </summary>
        public Coordinate Position { get; protected set; } = null;

        /// <summary>
        /// The current buffs that affect this unit
        /// </summary>
        public List<string> Buffs { get; protected set; } = null;

        /// <summary>
        /// The unit to be targeted by this unit
        /// </summary>
        public Unit Target { get; set; } = null;
        #endregion

        #region Properties For Simulation
        /// <summary>
        /// The current health of this unit in the simulation
        /// </summary>
        public double Simulated_Health { get; set; } = 0;

        /// <summary>
        /// The current energy of this unit in the simulation
        /// </summary>
        public double Simulated_Energy { get; set; } = 0;

        /// <summary>
        /// If this unit's health is below 0 in the simulation
        /// </summary>
        public virtual bool IsDead => Simulated_Health <= 0;

        /// <summary>
        /// If this unit's target health is below 0 in the simulation
        /// </summary>
        public virtual bool IsTargetDead => (Target == null || Target.IsDead);
        #endregion

        /// <summary>
        /// Creates an instance of parsed unit with basic information
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public Unit(string owner, string name, double x, double y, params string[] buffs)
        {
            Owner = owner;
            Name = name;
            Position = new Coordinate(x, y);
            Buffs = new List<string>(buffs);
        }

        /// <summary>
        /// A method that creates a new instance with the same values of this unit
        /// </summary>
        /// <returns></returns>
        public abstract Unit CreateDeepCopy();

        /// <summary>
        /// Deals an amount of damage applied with applicable buffs to this unit's target
        /// </summary>
        /// <returns>Returns false when this unit is dead, target is dead, or failed to deal a damage</returns>
        public virtual bool DealDamageToTarget()
        {
            double damage_to_deal = 0;

            try
            {
                //TODO
                if(!(IsDead || IsTargetDead))
                {
                    //Set the initial damage of this unit can deal
                    damage_to_deal = UNIT_DEFINITIONS[Name].Item1;
                    //Update the damage with buffs that modifies the damage of this unit
                    damage_to_deal += Convert.ToInt32(from buff in Buffs select buff);

                    return Target.RecieveDamageFromTarget(damage_to_deal);
                }

                throw new NotImplementedException();
            }
            catch(Exception ex)
            {
                Console.WriteLine();
            }

            return false;
        }

        public virtual bool RecieveDamageFromTarget(double damage_to_recieve)
        {
            try
            {
                //TODO
                throw new NotImplementedException();
            }
            catch(Exception ex)
            {

            }

            return false;
        }
    }
}