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
            //Ground Units
            ["Widow Mine"] = new Tuple<double, double, double, double, int, bool>(90, 0, 125, 125, 0, false),
            ["SCV"] = new Tuple<double, double, double, double, int, bool>(45, 0, 4.67, 0, 0, false),
            ["Marine"] = new Tuple<double, double, double, double, int, bool>(45, 0, 9.8, 9.8, 0, false),
            ["Marauder"] = new Tuple<double, double, double, double, int, bool>(125, 0, 9.3, 0, 1, false),
            ["Reaper"] = new Tuple<double, double, double, double, int, bool>(60, 0, 10.1, 0, 0, false),
            ["Ghost"] = new Tuple<double, double, double, double, int, bool>(100, 200, 9.3, 9.3, 0, false), //75/200
            ["Hellion"] = new Tuple<double, double, double, double, int, bool>(90, 0, 4.48, 0, 0, false),
            ["Hellbat"] = new Tuple<double, double, double, double, int, bool>(135, 0, 12.6, 0, 0, false),
            ["Siege Tank (Tank mode)"] = new Tuple<double, double, double, double, int, bool>(175, 0, 20.27, 0, 1, false),
            ["Siege Tank (Siege mode)"] = new Tuple<double, double, double, double, int, bool>(175, 0, 18.69, 0, 1, false),
            ["Cyclone"] = new Tuple<double, double, double, double, int, bool>(120, 0, 25.2, 25.2, 1, false),
            ["Thor (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(400, 0, 65.9, 0, 1, false),
            ["Thor (Explosive)"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 11.2, 1, false),
            ["Thor (High Impact)"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 23.4, 1, false),
            ["Auto-Turret"] = new Tuple<double, double, double, double, int, bool>(150, 0, 31.58, 31.58, 1, false),
            //Air Units
            ["Viking (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(135, 0, 16.8, 0, 0, true),
            ["Viking (Attack 2)"] = new Tuple<double, double, double, double, int, bool>(135, 0, 0, 14, 0, true),
            ["Medivac"] = new Tuple<double, double, double, double, int, bool>(150, 200, 0, 0, 1, true), //50/200
            ["Liberator (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(180, 0, 65.8, 0, 0, true),
            ["Liberator (Attack 2)"] = new Tuple<double, double, double, double, int, bool>(180, 0, 0, 7.8, 0, true),
            ["Raven"] = new Tuple<double, double, double, double, int, bool>(140, 200, 0, 0, 1, true), //50(+25)/200
            ["Banshee"] = new Tuple<double, double, double, double, int, bool>(140, 200, 27, 0, 0, true),
            ["Battlecruiser (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(550, 200, 49.8, 0, 3, true),
            ["Battlecruiser (Attack 2)"] = new Tuple<double, double, double, double, int, bool>(550, 200, 0, 31.1, 3, true),
            //Buildings
            ["Planetary Fortress"] = new Tuple<double, double, double, double, int, bool>(1500, 0, 28, 0, 3, false),
            ["Bunker"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["Missile Turret"] = new Tuple<double, double, double, double, int, bool>(250, 0, 0, 39.3, 1, false),
            ["Command Center"] = new Tuple<double, double, double, double, int, bool>(1500, 0, 0, 0, 1, false),
            ["Orbital Command"] = new Tuple<double, double, double, double, int, bool>(1500, 200, 0, 0, 1, false), //50/200
            ["Supply Depot"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["Refinery"] = new Tuple<double, double, double, double, int, bool>(500, 0, 0, 0, 1, false),
            ["Barracks"] = new Tuple<double, double, double, double, int, bool>(1000, 0, 0, 0, 1, false),
            ["Engineering Bay"] = new Tuple<double, double, double, double, int, bool>(850, 0, 0, 0, 1, false),
            ["Bunker"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["Sensor Tower"] = new Tuple<double, double, double, double, int, bool>(200, 0, 0, 0, 0, false),
            ["Factory"] = new Tuple<double, double, double, double, int, bool>(1250, 0, 0, 0, 1, false),
            ["Ghost Academy"] = new Tuple<double, double, double, double, int, bool>(1250, 0, 0, 0, 1, false),
            ["Starport"] = new Tuple<double, double, double, double, int, bool>(1300, 0, 0, 0, 1, false),
            ["Armory"] = new Tuple<double, double, double, double, int, bool>(750, 0, 0, 0, 1, false),
            ["Fusion Core"] = new Tuple<double, double, double, double, int, bool>(750, 0, 0, 0, 1, false)
        };

        /// <summary>
        /// Contains the corresponding reward and priority to the unit
        /// </summary>
                                //Unit name,   Priority, Mineral Cost, Vespene Cost, Supply Cost
        public static Dictionary<string, Tuple<int, double, double, int>> UNIT_VALUE = new Dictionary<string, Tuple<int, double, double, int>>()
        {
            //TODO
            //Ground Units
            ["Widow Mine"] = new Tuple<int, double, double, int>(19, 75, 25, 2),
            ["SCV"] = new Tuple<int, double, double, int>(20, 50, 0, 1),
            ["Marine"] = new Tuple<int, double, double, int>(20, 50, 0, 1),
            ["Marauder"] = new Tuple<int, double, double, int>(20, 100, 25, 2),
            ["Reaper"] = new Tuple<int, double, double, int>(20, 50, 50, 1),
            ["Ghost"] = new Tuple<int, double, double, int>(20, 150, 125, 2),
            ["Hellion"] = new Tuple<int, double, double, int>(20, 100, 0, 2),
            ["Hellbat"] = new Tuple<int, double, double, int>(20, 100, 0, 2),
            ["Siege Tank"] = new Tuple<int, double, double, int>(20, 150, 125, 3),
            ["Cyclone"] = new Tuple<int, double, double, int>(20, 150, 100, 3),
            ["Thor"] = new Tuple<int, double, double, int>(20, 300, 200, 6),
            ["Auto-Turret"] = new Tuple<int, double, double, int>(20, 0, 0, 0),
            //Air Units
            ["Viking"] = new Tuple<int, double, double, int>(20, 150, 75, 2),
            ["Medivac"] = new Tuple<int, double, double, int>(20, 100, 100, 2),
            ["Liberator"] = new Tuple<int, double, double, int>(20, 150, 150, 3),
            ["Raven"] = new Tuple<int, double, double, int>(20, 100, 200, 2),
            ["Banshee"] = new Tuple<int, double, double, int>(20, 150, 100, 3),
            ["Battlecruiser"] = new Tuple<int, double, double, int>(20, 400, 300, 6),
            //Buildings
            ["Planetary Fortress"] = new Tuple<int, double, double, int>(20, 550, 150, 0),
            ["Bunker"] = new Tuple<int, double, double, int>(20, 100, 0, 0),
            ["Missile Turret"] = new Tuple<int, double, double, int>(19, 100, 0, 0),
            ["Command Center"] = new Tuple<int, double, double, int>(11, 400, 0, 0),
            ["Orbital Command"] = new Tuple<int, double, double, int>(11, 550, 0, 0),
            ["Supply Depot"] = new Tuple<int, double, double, int>(11, 100, 0, 0),
            ["Refinery"] = new Tuple<int, double, double, int>(11, 75, 0, 0),
            ["Barracks"] = new Tuple<int, double, double, int>(11, 150, 0, 0),
            ["Engineering Bay"] = new Tuple<int, double, double, int>(11, 125, 0, 0),
            ["Bunker"] = new Tuple<int, double, double, int>(11, 100, 0, 0),
            ["Sensor Tower"] = new Tuple<int, double, double, int>(11, 125, 100, 0),
            ["Factory"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["Ghost Academy"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["Starport"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["Armory"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["Fusion Core"] = new Tuple<int, double, double, int>(11, 150, 150, 0)
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