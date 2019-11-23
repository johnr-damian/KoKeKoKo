using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public class SimulatedUnit : IActor<SimulatedUnit>
    {
        #region Definitions
        /// <summary>
        /// Contains the minimal definitions for a unit to participate in battle
        /// </summary>
        private static Dictionary<string, Definition> Definitions = new Dictionary<string, Definition>()
        {
            //Ground Units
            ["TERRAN_WIDOWMINE"] = new Definition(90, 0, 0, 0, 0, false),
            ["TERRAN_WIDOWMINEBURROWED"] = new Definition(90, 0, 125, 125, 0, false),
            ["TERRAN_SCV"] = new Definition(45, 0, 4.67, 0, 0, false),
            ["TERRAN_MARINE"] = new Definition(45, 0, 9.8, 9.8, 0, false),
            ["TERRAN_MARAUDER"] = new Definition(125, 0, 9.3, 0, 1, false),
            ["TERRAN_REAPER"] = new Definition(60, 0, 10.1, 0, 0, false),
            ["TERRAN_GHOST"] = new Definition(100, 200, 9.3, 9.3, 0, false), //75/200
            ["TERRAN_HELLION"] = new Definition(90, 0, 4.48, 0, 0, false),
            ["TERRAN_HELLIONTANK"] = new Definition(135, 0, 12.6, 0, 0, false),
            ["TERRAN_SIEGETANK"] = new Definition(175, 0, 20.27, 0, 1, false),
            ["TERRAN_SIEGETANKSIEGED"] = new Definition(175, 0, 18.69, 0, 1, false),
            ["TERRAN_CYCLONE"] = new Definition(120, 0, 25.2, 25.2, 1, false),
            ["TERRAN_THOR"] = new Definition(400, 0, 65.9, 11.2, 1, false),
            ["TERRAN_THORAP"] = new Definition(400, 0, 65.9, 23.4, 1, false),
            ["TERRAN_AUTOTURRET"] = new Definition(150, 0, 31.58, 31.58, 1, false),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new Definition(135, 0, 16.8, 0, 0, false),
            ["TERRAN_VIKINGFIGHTER"] = new Definition(135, 0, 0, 14, 0, true),
            ["TERRAN_MEDIVAC"] = new Definition(150, 200, 0, 0, 1, true), //50/200
            ["TERRAN_LIBERATORAG"] = new Definition(180, 0, 65.8, 0, 0, true),
            ["TERRAN_LIBERATOR"] = new Definition(180, 0, 0, 7.8, 0, true),
            ["TERRAN_RAVEN"] = new Definition(140, 200, 0, 0, 1, true), //50(+25)/200
            ["TERRAN_BANSHEE"] = new Definition(140, 200, 27, 0, 0, true),
            ["TERRAN_BATTLECRUISER"] = new Definition(550, 200, 49.8, 31.1, 3, true),
            //Summoned Units
            ["TERRAN_MULE"] = new Definition(60, 0, 0, 0, 0, false),
            ["TERRAN_AUTOTURRET"] = new Definition(150, 0, 31.58, 31.58, 1, false), //considered a buidling
            ["TERRAN_POINTDEFENSEDRONE"] = new Definition(50, 0, 0, 0, 0, false), //considered a buidling //intercepts certain projectiles
            ["TERRAN_NUKE"] = new Definition(0, 0, 300, 300, 0, false),
            ["TERRAN_KD8CHARGE"] = new Definition(0, 0, 5, 0, 0, false),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new Definition(1500, 0, 28, 0, 3, false),
            ["TERRAN_BUNKER"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_MISSILETURRET"] = new Definition(250, 0, 0, 39.3, 1, false),
            ["TERRAN_COMMANDCENTER"] = new Definition(1500, 0, 0, 0, 1, false),
            ["TERRAN_ORBITALCOMMAND"] = new Definition(1500, 200, 0, 0, 1, false), //50/200
            ["TERRAN_SUPPLYDEPOT"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_REFINERY"] = new Definition(500, 0, 0, 0, 1, false),
            ["TERRAN_BARRACKS"] = new Definition(1000, 0, 0, 0, 1, false),
            ["TERRAN_BARRACKSREACTOR"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_BARRACKSTECHLAB"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_ENGINEERINGBAY"] = new Definition(850, 0, 0, 0, 1, false),
            ["TERRAN_BUNKER"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_SENSORTOWER"] = new Definition(200, 0, 0, 0, 0, false),
            ["TERRAN_FACTORY"] = new Definition(1250, 0, 0, 0, 1, false),
            ["TERRAN_FACTORYREACTOR"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_FACTORYTECHLAB"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_GHOSTACADEMY"] = new Definition(1250, 0, 0, 0, 1, false),
            ["TERRAN_STARPORT"] = new Definition(1300, 0, 0, 0, 1, false),
            ["TERRAN_STARPORTREACTOR"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_STARPORTTECHLAB"] = new Definition(400, 0, 0, 0, 1, false),
            ["TERRAN_ARMORY"] = new Definition(750, 0, 0, 0, 1, false),
            ["TERRAN_FUSIONCORE"] = new Definition(750, 0, 0, 0, 1, false)
        };

        /// <summary>
        /// Contains the minimal values for a unit to know the cost and worth in simulation.
        /// </summary>
        private static Dictionary<string, Cost> Values = new Dictionary<string, Cost>()
        {
            //Ground Units
            ["TERRAN_WIDOWMINE"] = new Cost(19, 75, 25, 2),
            ["TERRAN_WIDOWMINEBURROWED"] = new Cost(20, 75, 25, 2),
            ["TERRAN_SCV"] = new Cost(20, 50, 0, 1),
            ["TERRAN_MARINE"] = new Cost(20, 50, 0, 1),
            ["TERRAN_MARAUDER"] = new Cost(20, 100, 25, 2),
            ["TERRAN_REAPER"] = new Cost(20, 50, 50, 1),
            ["TERRAN_GHOST"] = new Cost(20, 150, 125, 2),
            ["TERRAN_HELLION"] = new Cost(20, 100, 0, 2),
            ["TERRAN_HELLIONTANK"] = new Cost(20, 100, 0, 2),
            ["TERRAN_SIEGETANK"] = new Cost(20, 150, 125, 3),
            ["TERRAN_SIEGETANKSIEGED"] = new Cost(20, 150, 125, 3),
            ["TERRAN_CYCLONE"] = new Cost(20, 150, 100, 3),
            ["TERRAN_THOR"] = new Cost(20, 300, 200, 6),
            ["TERRAN_THORAP"] = new Cost(20, 300, 200, 6),
            ["TERRAN_AUTOTURRET"] = new Cost(20, 0, 0, 0),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new Cost(20, 150, 75, 2),
            ["TERRAN_VIKINGFIGHTER"] = new Cost(20, 150, 75, 2),
            ["TERRAN_MEDIVAC"] = new Cost(20, 100, 100, 2),
            ["TERRAN_LIBERATORAG"] = new Cost(20, 150, 150, 3),
            ["TERRAN_LIBERATOR"] = new Cost(20, 150, 150, 3),
            ["TERRAN_RAVEN"] = new Cost(20, 100, 200, 2),
            ["TERRAN_BANSHEE"] = new Cost(20, 150, 100, 3),
            ["TERRAN_BATTLECRUISER"] = new Cost(20, 400, 300, 6),
            //Summoned Units
            ["TERRAN_MULE"] = new Cost(20, 0, 0, 0),
            ["TERRAN_AUTOTURRET"] = new Cost(20, 0, 0, 0), //considered a buidling
            ["TERRAN_POINTDEFENSEDRONE"] = new Cost(20, 0, 0, 0), //considered a buidling
            ["TERRAN_NUKE"] = new Cost(0, 100, 100, 0),
            ["TERRAN_KD8CHARGE"] = new Cost(0, 0, 0, 0),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new Cost(20, 550, 150, 0),
            ["TERRAN_BUNKER"] = new Cost(20, 100, 0, 0),
            ["TERRAN_MISSILETURRET"] = new Cost(19, 100, 0, 0),
            ["TERRAN_COMMANDCENTER"] = new Cost(11, 400, 0, 0),
            ["TERRAN_ORBITALCOMMAND"] = new Cost(11, 550, 0, 0),
            ["TERRAN_SUPPLYDEPOT"] = new Cost(11, 100, 0, 0),
            ["TERRAN_REFINERY"] = new Cost(11, 75, 0, 0),
            ["TERRAN_BARRACKS"] = new Cost(11, 150, 0, 0),
            ["TERRAN_BARRACKSREACTOR"] = new Cost(11, 50, 50, 0),
            ["TERRAN_BARRACKSTECHLAB"] = new Cost(11, 50, 25, 0),
            ["TERRAN_ENGINEERINGBAY"] = new Cost(11, 125, 0, 0),
            ["TERRAN_BUNKER"] = new Cost(11, 100, 0, 0),
            ["TERRAN_SENSORTOWER"] = new Cost(11, 125, 100, 0),
            ["TERRAN_FACTORY"] = new Cost(11, 150, 100, 0),
            ["TERRAN_FACTORYREACTOR"] = new Cost(11, 50, 50, 0),
            ["TERRAN_FACTORYTECHLAB"] = new Cost(11, 50, 25, 0),
            ["TERRAN_GHOSTACADEMY"] = new Cost(11, 150, 100, 0),
            ["TERRAN_STARPORT"] = new Cost(11, 150, 100, 0),
            ["TERRAN_STARPORTREACTOR"] = new Cost(11, 50, 50, 0),
            ["TERRAN_STARPORTTECHLAB"] = new Cost(11, 50, 25, 0),
            ["TERRAN_ARMORY"] = new Cost(11, 150, 100, 0),
            ["TERRAN_FUSIONCORE"] = new Cost(11, 150, 150, 0)
        }; 
        #endregion

        #region Properties
        /// <summary>
        /// The current target of this unit in the simulation.
        /// </summary>
        private int Current_Target { get; set; } = default(int);

        /// <summary>
        /// A list of opposing units that has been targeted by this unit.
        /// </summary>
        private List<SimulatedUnit> Targets { get; set; } = default(List<SimulatedUnit>);

        /// <summary>
        /// Checks if the current <see cref="Health"/> is below or equal 0.
        /// </summary>
        public bool IsDefeated
        {
            get
            {
                return (Health <= 0);
            }
        }

        /// <summary>
        /// The current health of this unit.
        /// </summary>
        public double Health { get; private set; } = default(double);

        /// <summary>
        /// The current energy of this unit.
        /// </summary>
        public double Energy { get; private set; } = default(double);

        /// <summary>
        /// The current armor of this unit.
        /// </summary>
        public double Armor { get; private set; } = default(double);

        /// <summary>
        /// The current air damage of this unit.
        /// </summary>
        public double Air_Damage { get; private set; } = default(double);

        /// <summary>
        /// The current ground damage of this unit.
        /// </summary>
        public double Ground_Damage { get; private set; } = default(double);

        /// <summary>
        /// The researched upgrades that buffs or affects this unit.
        /// </summary>
        public List<string> Upgrades { get; private set; } = default(List<string>);

        /// <summary>
        /// A list of skills that was activated by this unit during simulation.
        /// </summary>
        public List<Tuple<string, DateTime>> Skills { get; private set; } = default(List<Tuple<string, DateTime>>);

        /// <summary>
        /// The current opposing unit that is targeted by this unit.
        /// </summary>
        public SimulatedUnit Target
        {
            get
            {
                return ((Targets.Count == 0) ? null : Targets[Current_Target]);
            }
        }

        /// <summary>
        /// A unique identifier for this unit. It is the tag in the C++ Agent.
        /// </summary>
        public string UniqueID { get; private set; } = default(string);

        /// <summary>
        /// The type of this unit.
        /// </summary>
        public string Name { get; private set; } = default(string);
        #endregion

        #region Structures
        /// <summary>
        /// A minimal definition of a unit that is used in simulation.
        /// </summary>
        private struct Definition
        {
            #region Properties
            /// <summary>
            /// The health of a unit in order to stay alive during battle.
            /// </summary>
            public double Health { get; set; }

            /// <summary>
            /// The energy of a unit in order to use a skill.
            /// </summary>
            public double Energy { get; set; }

            /// <summary>
            /// The armor of a unit will reduce the damage taken from opposing unit.
            /// </summary>
            public double Armor { get; set; }

            /// <summary>
            /// The damage of a unit to deal with flying opposing unit.
            /// </summary>
            public double Air_Damage { get; set; }

            /// <summary>
            /// The damage of a unit to deal with not flying opposing unit.
            /// </summary>
            public double Ground_Damage { get; set; }

            /// <summary>
            /// If the unit is a flying type unit.
            /// </summary>
            public bool IsFlyingUnit { get; set; }
            #endregion

            /// <summary>
            /// Initializes the required properties for a unit to participate in battle.
            /// </summary>
            /// <param name="health"></param>
            /// <param name="energy"></param>
            /// <param name="ground_damage"></param>
            /// <param name="air_damage"></param>
            /// <param name="armor"></param>
            /// <param name="is_flyingunit"></param>
            public Definition(double health, double energy, double ground_damage, double air_damage, double armor, bool is_flyingunit)
            {
                Health = health;
                Energy = energy;
                Armor = armor;
                Air_Damage = air_damage;
                Ground_Damage = ground_damage;
                IsFlyingUnit = is_flyingunit;
            }
        }

        /// <summary>
        /// A struct that contains the worth of destroying this unit, or the
        /// cost in order to create this unit during simulation.
        /// </summary>
        private struct Cost
        {
            #region Properties
            /// <summary>
            /// The mineral cost to create this unit.
            /// </summary>
            private double Mineral { get; set; }

            /// <summary>
            /// The vespene cost to create this unit.
            /// </summary>
            private double Vespene { get; set; }

            /// <summary>
            /// The supply that will be consumed when this unit is created.
            /// </summary>
            private int Supply { get; set; }

            /// <summary>
            /// The StarCraft II priority to kill this unit during a battle.
            /// </summary>
            private int Priority { get; set; }
            #endregion

            #region Operators
            /// <summary>
            /// Returns the properties of <see cref="Cost"/> into negative equivalent.
            /// </summary>
            /// <param name="cost"></param>
            /// <returns></returns>
            public static Cost operator !(Cost cost) => new Cost(-cost.Priority, - cost.Mineral, -cost.Vespene, -cost.Supply);

            /// <summary>
            /// Returns the sum of two <see cref="Cost"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Cost operator +(Cost a, Cost b) => new Cost((a.Priority + b.Priority), (a.Mineral + b.Mineral), (a.Vespene + b.Vespene), (a.Supply + b.Supply));

            /// <summary>
            /// Returns the difference of two <see cref="Cost"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Cost operator -(Cost a, Cost b) => new Cost((a.Priority - b.Priority), (a.Mineral - b.Mineral), (a.Vespene - b.Vespene), (a.Supply - b.Supply));

            /// <summary>
            /// Returns a 4-Length double array equivalent of <see cref="Cost"/>.
            /// </summary>
            /// <param name="cost"></param>
            public static explicit operator double[] (Cost cost) => new double[] { cost.Mineral, cost.Vespene, cost.Supply, cost.Priority };

            /// <summary>
            /// Returns a 4-Tuple equivalent of <see cref="Cost"/>.
            /// </summary>
            /// <param name="cost"></param>
            public static explicit operator Tuple<double, double, int, int>(Cost cost) => new Tuple<double, double, int, int>(cost.Mineral, cost.Vespene, cost.Supply, cost.Priority);

            /// <summary>
            /// Returns a <see cref="Cost"/> from a 4-Length double array.
            /// </summary>
            /// <param name="cost"></param>
            public static explicit operator Cost(double[] cost) => new Cost(Convert.ToInt32(cost[3]), cost[0], cost[1], Convert.ToInt32(cost[2]));

            /// <summary>
            /// Returns a <see cref="Cost"/> from a 4-Tuple.
            /// </summary>
            /// <param name="cost"></param>
            public static explicit operator Cost(Tuple<double, double, int, int> cost) => new Cost(Convert.ToInt32(cost.Item4), cost.Item1, cost.Item2, Convert.ToInt32(cost.Item3));
            #endregion

            /// <summary>
            /// Initializes the required properties to store the worth of destroying this unit, or
            /// the cost of this unit to create during simulation.
            /// </summary>
            /// <param name="priority"></param>
            /// <param name="mineral"></param>
            /// <param name="vespene"></param>
            /// <param name="supply"></param>
            public Cost(int priority, double mineral, double vespene, int supply)
            {
                Mineral = mineral;
                Vespene = vespene;
                Supply = supply;
                Priority = priority;
            }

            #region Methods
            /// <summary>
            /// Returns the sum of all properties with no weights.
            /// </summary>
            /// <returns></returns>
            public double GetWeightedCost() => GetWeightedCost(1);

            /// <summary>
            /// Returns the sum of all properties with the same weights.
            /// </summary>
            /// <param name="weight"></param>
            /// <returns></returns>
            public double GetWeightedCost(double weight) => GetWeightedCost(weight, weight, weight, weight);

            /// <summary>
            /// Returns the sum of all properties with different specified weights.
            /// </summary>
            /// <param name="mineral_weight"></param>
            /// <param name="vespene_weight"></param>
            /// <param name="supply_weight"></param>
            /// <param name="priority_weight"></param>
            /// <returns></returns>
            public double GetWeightedCost(double mineral_weight, double vespene_weight, double supply_weight, double priority_weight) => ((Mineral * mineral_weight) + (Vespene * vespene_weight) + (Supply * supply_weight) + (Priority * priority_weight));
            #endregion
        } 
        #endregion


        public SimulatedUnit(string unit)
        {
            var parsed_unit = unit.Split(',');
        }

        public void ApplyChosenAction(string chosen_action)
        {
            throw new NotImplementedException();
        }

        public SimulatedUnit Copy()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GeneratePotentialActions()
        {
            throw new NotImplementedException();
        }
    }
}
