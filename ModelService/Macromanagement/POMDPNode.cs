using ModelService.Collections;
using RDotNet;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement
{
    public class POMDPNode : SimulationNode
    {
        /// <summary>
        /// A transition policy matrix reference. This is used when <see cref="POMDPNode"/> is
        /// configured to use what is learned from CSV files for state transition.
        /// </summary>
        private static Tuple<string[], double[,][]> Reference { get; set; } = default(Tuple<string[], double[,][]>);

        public double Value { get; private set; } = default(double);

        #region Constructors
        /// <summary>
        /// Initializes the required properties to start simulating the game using
        /// the Partially Observable Markov Decision Process Algorithm. This constructor
        /// is used for creating instance where the source of information is from a CSV file.
        /// </summary>
        /// <param name="owned_name"></param>
        /// <param name="enemy_name"></param>
        public POMDPNode(string owned_name, string enemy_name)
            : base(owned_name, enemy_name)
        {
            Value = 0;
        }

        /// <summary>
        /// Initializes the required properties to start simulating the game using
        /// the Partially Observable Markov Decision Process Algorithm. This constructor
        /// is used for creating instance where the source of information is from C++ Agent.
        /// </summary>
        /// <param name="agent_name"></param>
        /// <param name="micromanagement"></param>
        public POMDPNode(string agent_name, IEnumerable<string> micromanagement)
            : base(agent_name, micromanagement)
        {
            Value = 0;
        }

        /// <summary>
        /// Initializes the required properties to continue simulating the game using
        /// the Partially Observable Markov Decision Process Algorithm. This constructor
        /// is used for expanding the parent node where the source of information is 
        /// from the <see cref="SimulationNode.Parent"/> node.
        /// </summary>
        /// <param name="owned_agent"></param>
        /// <param name="enemy_agent"></param>
        /// <param name="parent"></param>
        private POMDPNode(SimulatedAgent owned_agent, SimulatedAgent enemy_agent, POMDPNode parent)
            : base(owned_agent, enemy_agent, parent)
        {
            Value = 0;
        }
        #endregion

        /// <summary>
        /// Sets the transition policy matrix reference.
        /// </summary>
        /// <param name="reference"></param>
        public static void SetPOMDPReference(Tuple<string[], double[,][]> reference) => Reference = reference;

        public override SimulationNode SelectPhase()
        {
            if(!IsExpanded)
            {
                ExpandPhase();

                double bestvalue = Double.MinValue;
                SimulationNode child = default(POMDPNode);
                foreach(POMDPNode childnode in Children)
                {
                    var current_value = childnode.Value;
                    if(current_value > bestvalue)
                    {
                        bestvalue = current_value;
                        child = childnode;
                    }
                }

                Child = child;
            }

            return Child;
        }

        protected override void ExpandPhase()
        {
            Console.WriteLine($@"Currently Expanding... Your current depth is {Depth}");

            for (int test = 0; test < 1; test++)
                Children.Add(new POMDPNode(Owned_Agent.Copy(), Enemy_Agent.Copy(), this));

            Children.ForEach(child => ((POMDPNode)child).SimulationPhase());
        }

        protected override void SimulationPhase()
        {
            Console.WriteLine($@"Currently Simulating... Your current depth is {Depth}");

            //Get the service
            var agentservice = AgentService.CreateNewAgentService();
            var computationservice = ComputationService.CreateNewComputationService();

            //Get the list of potential actions
            var owned_agent_actions = Owned_Agent.GeneratePotentialActions().ToArray();
            var enemy_agent_actions = Enemy_Agent.GeneratePotentialActions().ToArray();

            //Get the distinct potential actions
            var owned_agent_distinct = owned_agent_actions.Distinct();
            var enemy_agent_distinct = enemy_agent_actions.Distinct();

            //Initialize action to probability mapping
            var owned_agent_probability = new Dictionary<string, Tuple<double, double>>();
            var enemy_agent_probability = new Dictionary<string, Tuple<double, double>>();

            //Create the probability mapping
            double start_probability = 0;
            foreach (var owned_agent_action in owned_agent_distinct)
            {
                double count = owned_agent_actions.Count(action => action == owned_agent_action);
                double end_probability = (start_probability + (count / owned_agent_actions.Length));

                owned_agent_probability.Add(owned_agent_action, new Tuple<double, double>(start_probability, end_probability));
                start_probability = end_probability;
            }
            start_probability = 0;
            foreach (var enemy_agent_action in enemy_agent_distinct)
            {
                double count = enemy_agent_actions.Count(action => action == enemy_agent_action);
                double end_probability = (start_probability + (count / enemy_agent_actions.Length));

                enemy_agent_probability.Add(enemy_agent_action, new Tuple<double, double>(start_probability, end_probability));
                start_probability = end_probability;
            }

            computationservice.RService.Evaluate($@"model_name = ""Macromanagement""");
            computationservice.RService.Evaluate($@"model_discount = 0.90");
            computationservice.RService.Evaluate($@"model_states = c(""ARMY"", ""ECONOMY"", ""TECH"")");
            computationservice.RService.Evaluate($@"model_actions = c({String.Join(",", owned_agent_distinct.Select(action => $@"""{action}"""))})");
            computationservice.RService.Evaluate($@"model_observations = c(""ARMY"", ""ECONOMY"", ""TECH"")");
            computationservice.RService.Evaluate($@"model_start = c(0.20, 0.80, 0)");
            computationservice.RService.Evaluate($@"model_transitionprobability = list({String.Join(",", owned_agent_distinct.Select(action => $@"""{action}"" = ""uniform"""))})");
            computationservice.RService.Evaluate($@"model_observationprobability = list({String.Join(",", owned_agent_distinct.Select(action => $@"""{action}"" = ""uniform"""))})");

            string dataframe = "";
            dataframe += $@"""action"" = c({String.Join(",", owned_agent_distinct.Select(action => $@"""{action}"""))}),";
            dataframe += $@"""start-state"" = c({String.Join(",", owned_agent_distinct.Select(action => {
                double r = computationservice.GetRandomProbability();

                if (r < 0.25)
                {
                    return $@"""TECH""";
                }
                else if (r >= 0.25 && r < 0.50)
                    return $@"""ECONOMY""";
                else if (r >= 0.50 && r < 0.75)
                    return $@"""ARMY""";
                else
                    return $@"""ECONOMY""";
            }))})";
            dataframe += $@",""end-state"" = c({String.Join(",", owned_agent_distinct.Select(action => $@"""*"""))})";
            dataframe += $@",""observation"" = c({String.Join(",", owned_agent_distinct.Select(action => $@"""*"""))})";
            dataframe += $@",""reward"" = c({String.Join(",", owned_agent_distinct.Select(action =>
            {
                double r = computationservice.GetRandomInteger(-1, 10);

                return r;
            }))})";

            computationservice.RService.Evaluate($@"model_reward = data.frame({dataframe})");
            computationservice.RService.Evaluate($@"POMDPPROBLEM <- POMDP(name=model_name, discount=model_discount, states=model_states, actions=model_actions, observations=model_observations, start=model_start, transition=model_transitionprobability, observation=model_observationprobability, reward=model_reward)");
            computationservice.RService.Evaluate($@"POMDPPROBLEMSOLUTION <- solve_POMDP(model=POMDPPROBLEM)");
            var owned_pomdp = computationservice.RService.Evaluate($@"solution(POMDPPROBLEMSOLUTION)").AsCharacter().Reverse().Take(2).ToArray();
            


            computationservice.RService.Evaluate($@"model_actions = c({String.Join(",", enemy_agent_distinct.Select(action => $@"""{action}"""))})");
            computationservice.RService.Evaluate($@"model_observations = c(""ARMY"", ""ECONOMY"", ""TECH"")");
            computationservice.RService.Evaluate($@"model_start = c(0.20, 0.80, 0)");
            computationservice.RService.Evaluate($@"model_transitionprobability = list({String.Join(",", enemy_agent_distinct.Select(action => $@"""{action}"" = ""uniform"""))})");
            computationservice.RService.Evaluate($@"model_observationprobability = list({String.Join(",", enemy_agent_distinct.Select(action => $@"""{action}"" = ""uniform"""))})");

            string e_dataframe = "";
            e_dataframe += $@"""action"" = c({String.Join(",", enemy_agent_distinct.Select(action => $@"""{action}"""))}),";
            e_dataframe += $@"""start-state"" = c({String.Join(",", enemy_agent_distinct.Select(action => {
                double r = computationservice.GetRandomProbability();

                if (r < 0.25)
                {
                    return $@"""TECH""";
                }
                else if (r >= 0.25 && r < 0.50)
                    return $@"""ECONOMY""";
                else if (r >= 0.50 && r < 0.75)
                    return $@"""ARMY""";
                else
                    return $@"""ECONOMY""";
            }))})";
            e_dataframe += $@",""end-state"" = c({String.Join(",", enemy_agent_distinct.Select(action => $@"""*"""))})";
            e_dataframe += $@",""observation"" = c({String.Join(",", enemy_agent_distinct.Select(action => $@"""*"""))})";
            e_dataframe += $@",""reward"" = c({String.Join(",", enemy_agent_distinct.Select(action =>
            {
                double r = computationservice.GetRandomInteger(-1, 10);

                return r;
            }))})";

            computationservice.RService.Evaluate($@"model_reward = data.frame({e_dataframe})");
            computationservice.RService.Evaluate($@"POMDPPROBLEM <- POMDP(name=model_name, discount=model_discount, states=model_states, actions=model_actions, observations=model_observations, start=model_start, transition=model_transitionprobability, observation=model_observationprobability, reward=model_reward)");
            computationservice.RService.Evaluate($@"POMDPPROBLEMSOLUTION <- solve_POMDP(model=POMDPPROBLEM)");
            var enemy_pomdp = computationservice.RService.Evaluate($@"solution(POMDPPROBLEMSOLUTION)").AsCharacter().Reverse().Take(2).ToArray();

            string[] owned_available_actions, enemy_available_actions;
            string owned_state = "", enemy_state = "";
            if (owned_pomdp[0] == "1")
                owned_state = "ARMY";
            else if (owned_pomdp[0] == "2")
                owned_state = "ECONOMY";
            else if (owned_pomdp[0] == "3")
                owned_state = "TECH";

            if (enemy_pomdp[0] == "1")
                enemy_state = "ARMY";
            else if (enemy_pomdp[0] == "2")
                enemy_state = "ECONOMY";
            else if (enemy_pomdp[0] == "3")
                enemy_state = "TECH";

            owned_available_actions = owned_agent_distinct.Where(action =>
            {
                string category = "";
                switch (action)
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

                return category.Equals(owned_state, StringComparison.OrdinalIgnoreCase);
            }).ToArray();

            enemy_available_actions = enemy_agent_distinct.Where(action =>
            {
                string category = "";
                switch (action)
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

                return category.Equals(enemy_state, StringComparison.OrdinalIgnoreCase);
            }).ToArray();

            string owned_chosen_action = computationservice.GetRandomElement(owned_available_actions).FirstOrDefault();
            string enemy_chosen_action = computationservice.GetRandomElement(enemy_available_actions).FirstOrDefault();

            Owned_Agent.ApplyChosenAction(owned_chosen_action);
            Enemy_Agent.ApplyChosenAction(enemy_chosen_action);

            double owned_value = Convert.ToDouble(owned_pomdp[1]);
            double enemy_value = Convert.ToDouble(enemy_pomdp[1]);

            Value = owned_value - enemy_value;
            BackpropagatePhase(owned_value >= enemy_value);
        }
    }
}
