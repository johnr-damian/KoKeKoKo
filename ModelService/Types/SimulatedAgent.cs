using ModelService.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ModelService
{
    /// <summary>
    /// Represents an agent that directly interacts with the environment. It contains
    /// the related information to handle the simulations.
    /// </summary>
    public class SimulatedAgent : IActor<SimulatedAgent>, IFormattable
    {
        #region Properties
        /// <summary>
        /// The current resources of the agent.
        /// </summary>
        private Worth Resources { get; set; } = default(Worth);

        /// <summary>
        /// Checks if there are still <see cref="Units"/> to be controlled.
        /// </summary>
        public bool IsDefeated
        {
            get { return (Units.Count() <= 0); }
        }

        /// <summary>
        /// The current value of the agent. It is also the current
        /// <see cref="Resources"/> of the agent.
        /// </summary>
        public double[] Value
        {
            get { return ((double[])Resources); }
        }

        /// <summary>
        /// A list of researched upgrades of the agent.
        /// </summary>
        public List<string> Upgrades { get; private set; } = default(List<string>);

        /// <summary>
        /// The name of the current agent.
        /// </summary>
        public string Name { get; private set; } = default(string);

        /// <summary>
        /// The chosen action that was applied in <see cref="ApplyChosenAction(string)"/>.
        /// </summary>
        public string Action { get; private set; } = default(string);

        /// <summary>
        /// The current owned units of the agent.
        /// </summary>
        public SimulatedUnits Units { get; private set; } = default(SimulatedUnits);
        #endregion

        /// <summary>
        /// A struct that contains the current resources of an <see cref="SimulatedAgent"/>.
        /// </summary>
        private struct Worth
        {
            #region Properties
            /// <summary>
            /// The current gathered minerals of <see cref="SimulatedAgent"/>.
            /// </summary>
            public double Mineral { get; private set; }

            /// <summary>
            /// The current gathered vespene of <see cref="SimulatedAgent"/>.
            /// </summary>
            public double Vespene { get; private set; }

            /// <summary>
            /// The current consumed supply of <see cref="SimulatedAgent"/>.
            /// </summary>
            public int Supply { get; private set; }

            /// <summary>
            /// The current number of controlled workers of <see cref="SimulatedAgent"/>.
            /// </summary>
            public int Workers { get; private set; }

            /// <summary>
            /// The current number of researched upgrades of <see cref="SimulatedAgent"/>.
            /// </summary>
            public int Upgrades { get; private set; }
            #endregion

            #region Operators
            /// <summary>
            /// Returns the properties of <see cref="Worth"/> into negative equivalent.
            /// </summary>
            /// <param name="worth"></param>
            /// <returns></returns>
            public static Worth operator !(Worth worth) => new Worth(-worth.Mineral, -worth.Vespene, -worth.Supply, -worth.Workers, -worth.Upgrades);

            /// <summary>
            /// Returns the sum of two <see cref="Worth"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Worth operator +(Worth a, Worth b) => new Worth((a.Mineral + b.Mineral), (a.Vespene + b.Vespene), (a.Supply + b.Supply), (a.Workers + b.Workers), (a.Upgrades + b.Upgrades));

            /// <summary>
            /// Returns the difference of two <see cref="Worth"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Worth operator -(Worth a, Worth b) => new Worth((a.Mineral - b.Mineral), (a.Vespene - b.Vespene), (a.Supply - b.Supply), (a.Workers - b.Workers), (a.Upgrades - b.Upgrades));

            /// <summary>
            /// Returns a 5-Length double array equivalent of <see cref="Worth"/>.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator double[] (Worth worth) => new double[] { worth.Mineral, worth.Vespene, worth.Supply, worth.Workers, worth.Upgrades };

            /// <summary>
            /// Returns a 5-Tuple equivalent of <see cref="Worth"/>.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator Tuple<double, double, int, int, int>(Worth worth) => new Tuple<double, double, int, int, int>(worth.Mineral, worth.Vespene, worth.Supply, worth.Workers, worth.Upgrades);

            /// <summary>
            /// Returns a <see cref="Worth"/> from a 5-Length double array.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator Worth(double[] worth) => new Worth(worth[0], worth[1], Convert.ToInt32(worth[2]), Convert.ToInt32(worth[3]), Convert.ToInt32(worth[4]));

            /// <summary>
            /// Returns a <see cref="Worth"/> from a 5-Tuple.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator Worth(Tuple<double, double, int, int, int> worth) => new Worth(worth.Item1, worth.Item2, Convert.ToInt32(worth.Item3), Convert.ToInt32(worth.Item4), Convert.ToInt32(worth.Item5)); 
            #endregion

            /// <summary>
            /// Initializes the required properties with the value of current resources.
            /// </summary>
            /// <param name="mineral"></param>
            /// <param name="vespene"></param>
            /// <param name="supply"></param>
            /// <param name="workers"></param>
            /// <param name="upgrades"></param>
            public Worth(double mineral, double vespene, int supply, int workers, int upgrades)
            {
                Mineral = mineral;
                Vespene = vespene;
                Supply = supply;
                Workers = workers;
                Upgrades = upgrades;
            }
        }

        #region Constructors
        /// <summary>
        /// Initializes the minimal properties to simulate the agent with values
        /// coming from a CSV file. This constructor is used for starting a simulation
        /// for a CSV file.
        /// </summary>
        /// <param name="agent_name"></param>
        public SimulatedAgent(string agent_name)
        {
            Name = agent_name;
            Action = String.Empty;

            //Initialize the starting units
            Units = new SimulatedUnits();

            //Initialize the starting resources and upgrades
            Resources = new Worth(50, 0, 15, 12, 0);
            Upgrades = new List<string>();
        }

        /// <summary>
        /// Initializes the minimal properties to simulate the agent with values
        /// coming from a C++ Agent message. This constructor is used for starting a simulation
        /// for a C++ Agent.
        /// </summary>
        /// <param name="agent_name"></param>
        /// <param name="micromanagement"></param>
        public SimulatedAgent(string agent_name, IEnumerable<string> micromanagement)
        {
            Name = agent_name;
            Action = String.Empty;

            //Initialize the starting units
            Units = new SimulatedUnits(micromanagement.ToArray());

            //Initialize the starting resources
            Resources = new Worth(50, 0, 15, 12, 0);
            Upgrades = new List<string>();
        }

        /// <summary>
        /// Initializes the properties to continue simulate with values from the
        /// parent agent. This constructor is used for continuing the simulation 
        /// of the parent agent.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="units"></param>
        /// <param name="upgrades"></param>
        /// <param name="resources"></param>
        private SimulatedAgent(string name, SimulatedUnits units, List<string> upgrades, Worth resources)
        {
            Name = name;
            Action = String.Empty;

            //Initialize the units
            Units = units;

            //Initialize the resources and upgrades
            Resources = resources;
            Upgrades = new List<string>(upgrades);
        } 
        #endregion

        /// <summary>
        /// Updates the necessary properties of <see cref="SimulatedAgent"/> when
        /// the chosen action from <see cref="GeneratePotentialActions"/> is applied.
        /// </summary>
        /// <param name="chosen_action"></param>
        public void ApplyChosenAction(string chosen_action)
        {
            SimulatedUnit new_unit = null;
            double[] new_unit_cost = null;

            switch(chosen_action)
            {
                case "TRAIN_SCV":
                    //Create the new unit
                    new_unit = new SimulatedUnit("TERRAN_SCV", Upgrades);
                    new_unit_cost = (double[])new_unit;
                    break;
                case "TRAIN_MARINE":
                    //Create the new unit
                    new_unit = new SimulatedUnit("TERRAN_MARINE", Upgrades);
                    new_unit_cost = (double[])new_unit;
                    break;
            }

            //Update the list of units and resources
            if(new_unit != null)
            {
                //Add the new unit to the existing list of units
                Units += new_unit;

                //Update the resources by subtracting the cost
                Resources -= new Worth(new_unit_cost[0], new_unit_cost[1], Convert.ToInt32(new_unit_cost[2]), 0, 0);
            }

            //Update the resources with passive gains and other properties
            double new_mineral = Resources.Mineral, new_vespene = Resources.Vespene;
            var number_of_workers = Units.Count(unit => unit.Name == "TERRAN_SCV");
            
            if(number_of_workers > 0)
            {
                new_mineral += (5 * number_of_workers); //Probably wrong computation

                if (Units.Count(unit => unit.Name == "TERRAN_REFINERY") > 0)
                    new_vespene += (8 * number_of_workers);
            }

            //Update the resources and action
            Action = chosen_action;
            Resources = new Worth(new_mineral, new_vespene, Resources.Supply, number_of_workers, Upgrades.Count);
        }

        /// <summary>
        /// Returns an exact copy of the current agent excluding <see cref="Action"/>.
        /// </summary>
        /// <returns></returns>
        public SimulatedAgent Copy() => new SimulatedAgent(Name, Units.Copy(), Upgrades, Resources);

        /// <summary>
        /// Generates a list of actions that can be executed by the agent given by the current state.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GeneratePotentialActions()
        {
            var unit_names = Units.Select(unit => unit.Name);

            foreach(var unit in unit_names)
            {
                switch(unit)
                {
                    case "TERRAN_SCV":
                        if (Resources.Mineral >= 400 && Resources.Vespene >= 150)
                        {
                            yield return ("BUILD_COMMANDCENTER");
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_SUPPLYDEPOT"))
                                yield return ("BUILD_BARRACKS");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                                yield return ("BUILD_GHOSTACADEMY");
                                yield return ("BUILD_FACTORY");
                            }

                            if (unit_names.Contains("TERRAN_FACTORY"))
                            {
                                yield return ("BUILD_ARMORY");
                                yield return ("BUILD_STARPORT");
                            }

                            if (unit_names.Contains("TERRAN_STARPORT"))
                                yield return ("BUILD_FUSIONCORE");

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                                yield return ("BUILD_SENSORTOWER");
                            }
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_SUPPLYDEPOT"))
                                yield return ("BUILD_BARRACKS");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                                yield return ("BUILD_GHOSTACADEMY");
                                yield return ("BUILD_FACTORY");
                            }

                            if (unit_names.Contains("TERRAN_FACTORY"))
                            {
                                yield return ("BUILD_ARMORY");
                                yield return ("BUILD_STARPORT");
                            }

                            if (unit_names.Contains("TERRAN_STARPORT"))
                                yield return ("BUILD_FUSIONCORE");

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                                yield return ("BUILD_SENSORTOWER");
                            }
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 125)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_SUPPLYDEPOT"))
                                yield return ("BUILD_BARRACKS");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                                yield return ("BUILD_GHOSTACADEMY");
                                yield return ("BUILD_FACTORY");
                            }

                            if (unit_names.Contains("TERRAN_FACTORY"))
                            {
                                yield return ("BUILD_ARMORY");
                                yield return ("BUILD_STARPORT");
                            }

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                                yield return ("BUILD_SENSORTOWER");
                            }
                        }
                        else if (Resources.Mineral >= 125 && Resources.Vespene >= 100)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                                yield return ("BUILD_BUNKER");

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                                yield return ("BUILD_SENSORTOWER");
                            }
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                            }

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                            }
                        }
                        else if (Resources.Mineral >= 75 && Resources.Vespene >= 75)
                            yield return ("BUILD_REFINERY");
                        else if (Resources.Mineral >= 400)
                        {
                            yield return ("BUILD_COMMANDCENTER");
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_SUPPLYDEPOT"))
                                yield return ("BUILD_BARRACKS");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                            }

                            if (unit_names.Contains("TERRAN_FACTORY"))
                            {
                                yield return ("BUILD_ARMORY");
                                yield return ("BUILD_STARPORT");
                            }

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                            }
                        }
                        else if (Resources.Mineral >= 150)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_SUPPLYDEPOT"))
                                yield return ("BUILD_BARRACKS");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                            }

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                            }
                        }
                        else if (Resources.Mineral >= 125)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                            }

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                            }
                        }
                        else if (Resources.Mineral >= 100)
                        {
                            yield return ("BUILD_REFINERY");
                            yield return ("BUILD_SUPPLYDEPOT");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                            {
                                yield return ("BUILD_BUNKER");
                            }

                            if (unit_names.Contains("TERRAN_COMMANDCENTER"))
                                yield return ("BUILD_ENGINEERINGBAY");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                            {
                                yield return ("BUILD_MISSILETURRET");
                            }
                        }

                        yield return ("HARVEST_RETURN");
                        break;
                    case "TERRAN_COMMANDCENTER":
                        if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                        {
                            if (Resources.Supply >= 1)
                                yield return ("TRAIN_SCV");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                                yield return ("MORPH_ORBITALCOMMAND");

                            if (unit_names.Contains("TERRAN_ENGINEERINGBAY"))
                                yield return ("MORPH_PLANETARYFORTRESS");
                        }
                        else if (Resources.Mineral >= 150)
                        {
                            if (Resources.Supply >= 1)
                                yield return ("TRAIN_SCV");

                            if (unit_names.Contains("TERRAN_BARRACKS"))
                                yield return ("MORPH_ORBITALCOMMAND");
                        }
                        else if (Resources.Mineral >= 50)
                        {
                            if (Resources.Supply >= 1)
                                yield return ("TRAIN_SCV");
                        }
                        break;
                    case "TERRAN_BARRACKS":
                        if (Resources.Mineral >= 150 && Resources.Vespene >= 125)
                        {
                            yield return ("TRAIN_MARINE");
                            yield return ("TRAIN_REAPER");

                            if (Resources.Supply >= 1)
                            {
                                if (unit_names.Contains("TERRAN_BARRACKSTECHLAB"))
                                    yield return ("TRAIN_MARAUDER");

                                if (unit_names.Contains("TERRAN_GHOSTACADEMY"))
                                    yield return ("TRAIN_GHOST");
                            }

                            yield return ("BUILD_BARRACKSTECHLAB");
                            yield return ("BUILD_BARRACKSREACTOR");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            if (Resources.Supply >= 1)
                            {
                                yield return ("TRAIN_MARINE");
                                yield return ("TRAIN_REAPER");
                            }

                            if (Resources.Supply >= 2)
                            {
                                if (unit_names.Contains("TERRAN_BARRACKSTECHLAB"))
                                    yield return ("TRAIN_MARAUDER");
                            }

                            yield return ("BUILD_BARRACKSTECHLAB");
                            yield return ("BUILD_BARRACKSREACTOR");
                        }
                        else if (Resources.Mineral >= 50 && Resources.Vespene >= 50)
                        {
                            if (Resources.Supply >= 1)
                            {
                                yield return ("TRAIN_MARINE");
                                yield return ("TRAIN_REAPER");
                            }

                            yield return ("BUILD_BARRACKSTECHLAB");
                            yield return ("BUILD_BARRACKSREACTOR");
                        }
                        else if (Resources.Mineral >= 50 && Resources.Vespene >= 25)
                        {
                            if (Resources.Supply >= 1)
                                yield return ("TRAIN_MARINE");

                            yield return ("BUILD_BARRACKSTECHLAB");
                        }
                        else if (Resources.Mineral >= 50)
                        {
                            if (Resources.Supply >= 1)
                                yield return ("TRAIN_MARINE");
                        }
                        break;
                    case "TERRAN_BARRACKSTECHLAB":
                        if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            yield return ("RESEARCH_COMBATSHIELD");
                            yield return ("RESEARCH_STIMPACK");
                            yield return ("RESEARCH_CONCUSSIVESHELLS");
                        }
                        else if (Resources.Mineral >= 50 && Resources.Vespene >= 50)
                            yield return ("RESEARCH_CONCUSSIVESHELLS");
                        break;
                    case "TERRAN_FACTORY":
                        if (Resources.Mineral >= 300 && Resources.Vespene >= 200)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_HELLION");
                                yield return ("TRAIN_WIDOWMINE");

                                if (unit_names.Contains("TERRAN_ARMORY"))
                                {
                                    yield return ("TRAIN_HELLBAT");

                                    if (Resources.Supply >= 6)
                                        yield return ("TRAIN_THOR");
                                }
                            }
                            if (Resources.Supply >= 3)
                            {
                                if (unit_names.Contains("TERRAN_FACTORYTECHLAB"))
                                {
                                    yield return ("TRAIN_SIEGETANK");
                                    yield return ("TRAIN_CYCLONE");
                                }
                            }

                            yield return ("BUILD_FACTORYTECHLAB");
                            yield return ("BUILD_FACTORYREACTOR");
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 125)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_HELLION");
                                yield return ("TRAIN_WIDOWMINE");
                                if (unit_names.Contains("TERRAN_ARMORY"))
                                    yield return ("TRAIN_HELLBAT");
                            }

                            if (Resources.Supply >= 3)
                            {
                                if (unit_names.Contains("TERRAN_FACTORYTECHLAB"))
                                {
                                    yield return ("TRAIN_SIEGETANK");
                                    yield return ("TRAIN_CYCLONE");
                                }
                            }
                            yield return ("BUILD_FACTORYTECHLAB");
                            yield return ("BUILD_FACTORYREACTOR");
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 100)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_HELLION");
                                yield return ("TRAIN_WIDOWMINE");
                                if (unit_names.Contains("TERRAN_ARMORY"))
                                    yield return ("TRAIN_HELLBAT");
                            }

                            if (Resources.Supply >= 3)
                            {
                                if (unit_names.Contains("TERRAN_FACTORYTECHLAB"))
                                    yield return ("TRAIN_CYCLONE");
                            }
                            yield return ("BUILD_FACTORYTECHLAB");
                            yield return ("BUILD_FACTORYREACTOR");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_HELLION");
                                yield return ("TRAIN_WIDOWMINE");
                                if (unit_names.Contains("TERRAN_ARMORY"))
                                    yield return ("TRAIN_HELLBAT");
                            }
                            yield return ("BUILD_FACTORYTECHLAB");
                            yield return ("BUILD_FACTORYREACTOR");
                        }
                        else if (Resources.Mineral >= 75 && Resources.Vespene >= 50)
                        {
                            if (Resources.Supply >= 2)
                                yield return ("TRAIN_WIDOWMINE");
                            yield return ("BUILD_FACTORYTECHLAB");
                            yield return ("BUILD_FACTORYREACTOR");
                        }
                        else if (Resources.Mineral >= 100)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_HELLION");
                                if (unit_names.Contains("TERRAN_ARMORY"))
                                    yield return ("TRAIN_HELLBAT");
                            }
                        }
                        break;
                    case "TERRAN_FACTORYTECHLAB":
                        if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                        {
                            yield return ("RESEARCH_INFERNALPREIGNITER");
                            yield return ("RESEARCH_MAGFIELDLAUNCHERS");
                            yield return ("RESEARCH_DRILLINGCLAWS");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            yield return ("RESEARCH_MAGFIELDLAUNCHERS");
                            yield return ("RESEARCH_DRILLINGCLAWS");
                        }
                        else if (Resources.Mineral >= 75 && Resources.Vespene >= 75)
                        {
                            yield return ("RESEARCH_DRILLINGCLAWS");
                        }
                        break;
                    case "TERRAN_STARPORT":
                        if (Resources.Mineral >= 400 && Resources.Vespene >= 300)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_VIKINGFIGHTER");
                                yield return ("TRAIN_MEDIVAC");
                            }
                            if (Resources.Supply >= 3)
                            {
                                yield return ("TRAIN_LIBERATOR");
                                if (unit_names.Contains("TERRAN_STARPORTTECHLAB"))
                                    yield return ("TRAIN_BANSHEE");
                            }
                            if (unit_names.Contains("TERRAN_FUSIONCORE"))
                                if (Resources.Supply >= 6)
                                    yield return ("TRAIN_BATTLECRUISER");
                            yield return ("BUILD_STARPORTREACTOR");
                            yield return ("BUILD_STARPORTTECHLAB");
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_VIKINGFIGHTER");
                                yield return ("TRAIN_MEDIVAC");
                            }
                            if (Resources.Supply >= 3)
                            {
                                yield return ("TRAIN_LIBERATOR");
                                if (unit_names.Contains("TERRAN_STARPORTTECHLAB"))
                                    yield return ("TRAIN_BANSHEE");
                            }
                            yield return ("BUILD_STARPORTREACTOR");
                            yield return ("BUILD_STARPORTTECHLAB");
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 100)
                        {
                            if (Resources.Supply >= 2)
                            {
                                yield return ("TRAIN_VIKINGFIGHTER");
                                yield return ("TRAIN_MEDIVAC");
                            }
                            if (Resources.Supply >= 3)
                                if (unit_names.Contains("TERRAN_STARPORTTECHLAB"))
                                    yield return ("TRAIN_BANSHEE");
                            yield return ("BUILD_STARPORTREACTOR");
                            yield return ("BUILD_STARPORTTECHLAB");
                        }
                        else if (Resources.Mineral >= 150 && Resources.Vespene >= 75)
                        {
                            if (Resources.Supply >= 2)
                                yield return ("TRAIN_VIKINGFIGHTER");
                            yield return ("BUILD_STARPORTREACTOR");
                            yield return ("BUILD_STARPORTTECHLAB");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            if (Resources.Supply >= 2)
                                yield return ("TRAIN_MEDIVAC");
                            yield return ("BUILD_STARPORTREACTOR");
                            yield return ("BUILD_STARPORTTECHLAB");
                        }
                        else if (Resources.Mineral >= 50 && Resources.Vespene >= 50)
                        {
                            yield return ("BUILD_STARPORTREACTOR");
                            yield return ("BUILD_STARPORTTECHLAB");
                        }
                        //Mineral cost > Vespene cost case
                        if (Resources.Mineral >= 100 && Resources.Vespene >= 200)
                            if (Resources.Supply >= 2)
                                if (unit_names.Contains("TERRAN_STARPORTTECHLAB"))
                                    yield return ("TRAIN_RAVEN");
                        break;
                    case "TERRAN_STARPORTTECHLAB":
                        if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                        {
                            yield return ("RESEARCH_HIGHCAPACITYFUELTANKS");
                            yield return ("RESEARCH_RAVENCORVIDREACTOR");
                            yield return ("RESEARCH_BANSHEECLOAKINGFIELD");
                            yield return ("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                            yield return ("RESEARCH_ADVANCEDBALLISTICS");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            yield return ("RESEARCH_HIGHCAPACITYFUELTANKS");
                            yield return ("RESEARCH_BANSHEECLOAKINGFIELD");
                        }
                        break;
                    case "TERRAN_FUSIONCORE":
                        if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                            yield return ("RESEARCH_BATTLECRUISERWEAPONREFIT");
                        break;
                    case "TERRAN_ARMORY":
                        if (Resources.Mineral >= 250 && Resources.Vespene >= 250)
                        {
                            if (Upgrades.Contains("TERRANVEHICLEWEAPONSLEVEL2"))
                                yield return "RESEARCH_TERRANVEHICLEWEAPONSLEVEL3";


                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL3"))
                            //    yield return ("RESEARCH_TERRANVEHICLEWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL3"))
                            //    yield return ("RESEARCH_TERRANSHIPWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL3"))
                            //    yield return ("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL3"))
                            //    yield return ("RESEARCH_TERRANINFANTRYWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL3"))
                            //    yield return ("RESEARCH_TERRANINFANTRYARMOR");


                        }
                        else if (Resources.Mineral >= 175 && Resources.Vespene >= 175)
                        {
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL2"))
                            //    yield return ("RESEARCH_TERRANVEHICLEWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL2"))
                            //    yield return ("RESEARCH_TERRANSHIPWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL2"))
                            //    yield return ("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL2"))
                            //    yield return ("RESEARCH_TERRANINFANTRYWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL2"))
                            //    yield return ("RESEARCH_TERRANINFANTRYARMOR");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                        {
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                            //    yield return ("RESEARCH_TERRANVEHICLEWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                            //    yield return ("RESEARCH_TERRANSHIPWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                            //    yield return ("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                            //    yield return ("RESEARCH_TERRANINFANTRYWEAPONS");
                            //if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                            //    yield return ("RESEARCH_TERRANINFANTRYARMOR");
                        }
                        break;
                    case "TERRAN_GHOSTACADEMY":
                        if (Resources.Mineral >= 150 && Resources.Vespene >= 150)
                        {
                            yield return ("RESEARCH_PERSONALCLOAKING");
                            if (unit_names.Contains("TERRAN_FACTORY"))
                                yield return ("BUILD_NUKE");
                        }
                        else if (Resources.Mineral >= 100 && Resources.Vespene >= 100)
                            if (unit_names.Contains("TERRAN_FACTORY"))
                                yield return ("BUILD_NUKE");
                        break;
                    case "TERRAN_HELLION":
                        yield return "ATTACK";
                        yield return ("MORPH_HELLBAT");
                        break;
                    case "TERRAN_HELLIONTANK":
                        yield return ("MORPH_HELLION");
                        break;
                    case "TERRAN_SIEGETANK":
                        yield return ("MORPH_SIEGEMODE");
                        break;
                    case "TERRAN_SIEGETANKSIEGED":
                        yield return ("MORPH_UNSIEGE");
                        break;
                    case "TERRAN_VIKINGASSAULT":
                        yield return ("MORPH_VIKINGFIGHTERMODE");
                        break;
                    case "TERRAN_VIKINGFIGHTER":
                        yield return ("MORPH_VIKINGASSAULTMODE");
                        break;
                    case "TERRAN_LIBERATOR":
                        yield return ("MORPH_LIBERATORAGMODE");
                        break;
                    case "TERRAN_LIBERATORAG":
                        yield return ("MORPH_LIBERATORAAMODE");
                        break;
                    case "TERRAN_RAVEN":
                        yield return ("EFFECT_AUTOTURRET");
                        break;
                    case "TERRAN_ORBITALCOMMAND":
                        yield return ("EFFECT_CALLDOWNMULE");
                        break;
                    default:
                        break;
                }
            }
        }

        #region Update Methods
        /// <summary>
        /// Updates the micromanagement properties of the agent with values coming from
        /// a C++ Agent message. This method is use to update an enemy agent based on
        /// known enemy units in the message.
        /// </summary>
        /// <param name="micromanagement"></param>
        public void UpdateSimulatedAgent(IEnumerable<string> micromanagement)
        {
            //Update the known enemy units
            Units = new SimulatedUnits(micromanagement.ToArray());

            //Infer the resources based on the units
            throw new NotImplementedException("Guanga");
        }

        /// <summary>
        /// Updates the micromanagement and macromanagement properties of the agent with values
        /// coming from a C++ Agent message. This method is use to update an owned agent based
        /// on the message.
        /// </summary>
        /// <param name="micromanagement"></param>
        /// <param name="macromanagement"></param>
        public void UpdateSimulatedAgent(IEnumerable<string> micromanagement, IEnumerable<string> macromanagement)
        {
            //Update the controlled units
            Units = new SimulatedUnits(micromanagement.ToArray());

            //Update the resources
            var resources = macromanagement.ToArray();
            Resources = new Worth(Convert.ToDouble(resources[0]), Convert.ToDouble(resources[1]), Convert.ToInt32(resources[2]), Convert.ToInt32(resources[3]), Convert.ToInt32(resources[4]));
        } 
        #endregion

        #region ToString Methods
        /// <summary>
        /// Returns the <see cref="Action"/> of the agent that is used in messaging C++ Agent.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString("M", CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a specific information about the agent.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a specific information about the agent using a specific format provider.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format.ToUpperInvariant())
            {
                case "M":
                    return Action;
                case "R":
                    var resources = String.Join(",", Convert.ToString(Value[0]), Convert.ToString(Value[1]), Convert.ToString(Value[2]), Convert.ToString(Value[3]));
                    string category = "";

                    //Get the category of action
                    switch (Action)
                    {
                        case "BUILD_SUPPLYDEPOT":
                        case "TRAIN_SCV":
                        case "BUILD_REFINERY":
                            category = "Economy";
                            break;
                        case "BUILD_TECHLAB_BARRACKS":
                        case "RESEARCH_COMBATSHIELD":
                        case "RESEARCH_STIMPACK":
                        case "RESEARCH_CONCUSSIVESHELLS":
                        case "BUILD_REACTOR_BARRACKS":
                        case "BUILD_TECHLAB_FACTORY":
                        case "RESEARCH_INFERNALPREIGNITER":
                        case "RESEARCH_DRILLINGCLAWS":
                        case "RESEARCH_MAGFIELDLAUNCHERS":
                        case "BUILD_REACTOR_FACTORY":
                        case "BUILD_TECHLAB_STARPORT":
                        case "RESEARCH_HIGHCAPACITYFUELTANKS":
                        case "RESEARCH_RAVENCORVIDREACTOR":
                        case "RESEARCH_BANSHEEHYPERFLIGHTROTORS":
                        case "RESEARCH_ADVANCEDBALLISTICS":
                        case "BUILD_REACTOR_STARPORT":
                        case "BUILD_ENGINEERINGBAY":
                        case "RESEARCH_TERRANINFANTRYWEAPONSLEVEL1":
                        case "RESEARCH_TERRANINFANTRYWEAPONSLEVEL2":
                        case "RESEARCH_TERRANINFANTRYWEAPONSLEVEL3":
                        case "RESEARCH_TERRANINFANTRYARMORLEVEL1":
                        case "RESEARCH_TERRANINFANTRYARMORLEVEL2":
                        case "RESEARCH_TERRANINFANTRYARMORLEVEL3":
                        case "BUILD_ARMORY":
                        case "RESEARCH_TERRANSHIPWEAPONSLEVEL1":
                        case "RESEARCH_TERRANSHIPWEAPONSLEVEL2":
                        case "RESEARCH_TERRANSHIPWEAPONSLEVEL3":
                        case "RESEARCH_TERRANVEHICLEWEAPONSLEVEL1":
                        case "RESEARCH_TERRANVEHICLEWEAPONSLEVEL2":
                        case "RESEARCH_TERRANVEHICLEWEAPONSLEVEL3":
                        case "RESEARCH_TERRANVEHICLEANDSHIPPLATINGLEVEL1":
                        case "RESEARCH_TERRANVEHICLEANDSHIPPLATINGLEVEL2":
                        case "RESEARCH_TERRANVEHICLEANDSHIPPLATINGLEVEL3":
                        case "BUILD_SENSORTOWER":
                        case "BUILD_FUSIONCORE":
                        case "BUILD_GHOSTACADEMY":
                            category = "Tech";
                            break;
                        case "BUILD_BARRACKS":
                        case "TRAIN_MARINE":
                        case "TRAIN_MARAUDER":
                        case "TRAIN_REAPER":
                        case "TRAIN_GHOST":
                        case "BUILD_FACTORY":
                        case "TRAIN_HELLION":
                        case "TRAIN_HELLBAT":
                        case "TRAIN_SEIGETANK":
                        case "TRAIN_CYCLONE":
                        case "TRAIN_WIDOWMINE":
                        case "TRAIN_THOR":
                        case "BUILD_STARPORT":
                        case "TRAIN_VIKINGFIGHTER":
                        case "TRAIN_MEDIVAC":
                        case "TRAIN_LIBERATOR":
                        case "TRAIN_BANSHEE":
                        case "TRAIN_BATTLECRUISER":
                        case "BUILD_MISSILETURRET":
                        case "BUILD_BUNKER":
                            category = "Army";
                            break;
                        default:
                            break;
                    }

                    return String.Join(";", resources, String.Join(",", Action, category));
                default:
                    throw new Exception($@"Failed to format into string...");
            }
        } 
        #endregion
    }
}
