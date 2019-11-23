using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public class SimulatedUnit : IActor<SimulatedUnit>
    {
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
            /// <param name="armor"></param>
            /// <param name="air_damage"></param>
            /// <param name="ground_damage"></param>
            /// <param name="is_flyingunit"></param>
            public Definition(double health, double energy, double armor, double air_damage, double ground_damage, bool is_flyingunit)
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
            public static Cost operator !(Cost cost) => new Cost(-cost.Mineral, -cost.Vespene, -cost.Supply, -cost.Priority);

            /// <summary>
            /// Returns the sum of two <see cref="Cost"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Cost operator +(Cost a, Cost b) => new Cost((a.Mineral + b.Mineral), (a.Vespene + b.Vespene), (a.Supply + b.Supply), (a.Priority + b.Priority));

            /// <summary>
            /// Returns the difference of two <see cref="Cost"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Cost operator -(Cost a, Cost b) => new Cost((a.Mineral - b.Mineral), (a.Vespene - b.Vespene), (a.Supply - b.Supply), (a.Priority - b.Priority));

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
            public static explicit operator Cost(double[] cost) => new Cost(cost[0], cost[1], Convert.ToInt32(cost[2]), Convert.ToInt32(cost[3]));

            /// <summary>
            /// Returns a <see cref="Cost"/> from a 4-Tuple.
            /// </summary>
            /// <param name="cost"></param>
            public static explicit operator Cost(Tuple<double, double, int, int> cost) => new Cost(cost.Item1, cost.Item2, Convert.ToInt32(cost.Item3), Convert.ToInt32(cost.Item4));
            #endregion

            /// <summary>
            /// Initializes the required properties to store the worth of destroying this unit, or
            /// the cost of this unit to create during simulation.
            /// </summary>
            /// <param name="mineral"></param>
            /// <param name="vespene"></param>
            /// <param name="supply"></param>
            /// <param name="priority"></param>
            public Cost(double mineral, double vespene, int supply, int priority)
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
