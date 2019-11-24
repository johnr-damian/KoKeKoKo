using ModelService.Collections;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement
{
    public class MCTSNode : SimulationNode
    {
        /// <summary>
        /// An expansion policy matrix reference. This is used if <see cref="MCTSNode"/> is configured
        /// to expand by following what is learned from CSV files.
        /// </summary>
        private static Tuple<string[], double[,][]> Reference { get; set; } = default(Tuple<string[], double[,][]>);

        public double UCT
        {
            get
            {
                var exploration = (Wins / Runs);
                var exploitation = Math.Sqrt((2 * Math.Log(Parent.Runs)) / Runs);

                if (Double.IsNaN(exploration))
                    exploration = 0;
                if (Double.IsNaN(exploitation))
                    exploitation = 0;

                return (exploration + exploitation);
            }
        }

        #region Constructors
        /// <summary>
        /// Initializes the required properties to start simulating the game using
        /// the Monte Carlo Tree Search Algorithm. This constructor is used for creating
        /// instance where the source of information is from a CSV file.
        /// </summary>
        /// <param name="owned_name"></param>
        /// <param name="enemy_name"></param>
        public MCTSNode(string owned_name, string enemy_name)
            : base(owned_name, enemy_name) { }

        /// <summary>
        /// Initializes the required properties to start simulating the game using the
        /// Monte Carlo Tree Search Algorithm. This constructor is used for creating 
        /// instance where the source of information is from C++ Agent.
        /// </summary>
        /// <param name="agent_name"></param>
        /// <param name="micromanagement"></param>
        public MCTSNode(string agent_name, IEnumerable<string> micromanagement)
            : base(agent_name, micromanagement) { }

        /// <summary>
        /// Initializes the required properties to continue simulating the game using
        /// Monte Carlo Tree Search algorithm. This constructor is used for expanding the 
        /// parent node where the source of information is from the <see cref="SimulationNode{T}.Parent"/> node.
        /// </summary>
        /// <param name="owned_agent"></param>
        /// <param name="enemy_agent"></param>
        /// <param name="parent"></param>
        private MCTSNode(SimulatedAgent owned_agent, SimulatedAgent enemy_agent, MCTSNode parent)
            : base(owned_agent, enemy_agent, parent) { }
        #endregion

        /// <summary>
        /// Sets the expanding policy matrix reference.
        /// </summary>
        /// <param name="reference"></param>
        public static void SetMCTSReference(Tuple<string[], double[,][]> reference) => Reference = reference;

        public override SimulationNode SelectPhase()
        {
            if(!IsExpanded)
            {
                ExpandPhase();

                double bestuct = Double.MinValue;
                SimulationNode child = default(MCTSNode);
                foreach(MCTSNode childnode in Children)
                {
                    var current_uct = childnode.UCT;
                    if(current_uct > bestuct)
                    {
                        bestuct = current_uct;
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

            for(int test = 0; test < 5; test++)
                Children.Add(new MCTSNode(Owned_Agent.Copy(), Enemy_Agent.Copy(), this));

            Children.ForEach(child => ((MCTSNode)child).SimulationPhase());
        }

        protected override void SimulationPhase()
        {
            Console.WriteLine($@"Currently Simulating... Your current depth is {Depth}");

            //Get the services
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
            foreach(var owned_agent_action in owned_agent_distinct)
            {
                double count = owned_agent_actions.Count(action => action == owned_agent_action);
                double end_probability = (start_probability + (count / owned_agent_actions.Length));

                owned_agent_probability.Add(owned_agent_action, new Tuple<double, double>(start_probability, end_probability));
                start_probability = end_probability;
            }
            start_probability = 0;
            foreach(var enemy_agent_action in enemy_agent_distinct)
            {
                double count = enemy_agent_actions.Count(action => action == enemy_agent_action);
                double end_probability = (start_probability + (count / enemy_agent_actions.Length));

                enemy_agent_probability.Add(enemy_agent_action, new Tuple<double, double>(start_probability, end_probability));
                start_probability = end_probability;
            }

            //Check the expansion configuration. How do we select new action
            //1. Random
            //2. Learned (CSV)
            //3. Hybrid

            //Choosing 1. Random. We pick a random action.
            double owned_agent_randomaction = computationservice.GetRandomProbability(), enemy_agent_randomaction = computationservice.GetRandomProbability();
            string owned_agent_chosenaction = "", enemy_agent_chosenaction = "";

            foreach(var action in owned_agent_probability)
            {
                if((action.Value.Item1 < owned_agent_randomaction) && (owned_agent_randomaction < action.Value.Item2))
                {
                    owned_agent_chosenaction = action.Key;
                    break;
                }
            }
            foreach (var action in enemy_agent_probability)
            {
                if ((action.Value.Item1 < enemy_agent_randomaction) && (enemy_agent_randomaction < action.Value.Item2))
                {
                    enemy_agent_chosenaction = action.Key;
                    break;
                }
            }

            //Apply the action
            Owned_Agent.ApplyChosenAction(owned_agent_chosenaction);
            Enemy_Agent.ApplyChosenAction(enemy_agent_chosenaction);

            //We play out until we reach a certain depth
            //for(MCTSNode playnode = this; DateTime.Now < agentservice.NextUpdateTime.Subtract(new TimeSpan(0, 0, 5));)
            //{
            //    playnode = (MCTSNode)playnode.SelectPhase();
            //}

            //After the playout, we compute the values as much as possible
            double[] owned_agent_value = Owned_Agent.Value, enemy_agent_value = Enemy_Agent.Value;
            double avg_count = (Child != null)? 0 : 1;
            for(var chosen_child = Child; chosen_child != null; chosen_child = chosen_child.Child, avg_count++)
            {
                var new_owned_agent_value = chosen_child.Owned_Agent.Value;
                var new_enemy_agent_value = chosen_child.Enemy_Agent.Value;

                owned_agent_value[0] = new_owned_agent_value[0];
                owned_agent_value[0] = new_owned_agent_value[1];
                owned_agent_value[0] = new_owned_agent_value[2];
                owned_agent_value[0] = new_owned_agent_value[3];
                owned_agent_value[0] = new_owned_agent_value[4];

                enemy_agent_value[0] = new_enemy_agent_value[0];
                enemy_agent_value[0] = new_enemy_agent_value[1];
                enemy_agent_value[0] = new_enemy_agent_value[2];
                enemy_agent_value[0] = new_enemy_agent_value[3];
                enemy_agent_value[0] = new_enemy_agent_value[4];
            }

            //var mineral_worth = ((owned_agent_value[0] / avg_count) >= (enemy_agent_value[0] / avg_count));
            //var vespene_worth = ((owned_agent_value[1] / avg_count) >= (enemy_agent_value[1] / avg_count));
            //var supply_worth = ((owned_agent_value[2] / avg_count) >= (enemy_agent_value[2] / avg_count));
            //var worker_worth = ((owned_agent_value[3] / avg_count) >= (enemy_agent_value[3] / avg_count));
            //var upgrade_worth = ((owned_agent_value[4] / avg_count) >= (enemy_agent_value[4] / avg_count));

            //double total_worth = 0;
            //total_worth += (mineral_worth) ? 0.30 : 0;
            //total_worth += (vespene_worth) ? 0.30 : 0;
            //total_worth += (supply_worth) ? 0.15 : 0;
            //total_worth += (worker_worth) ? 0.15 : 0;
            //total_worth += (upgrade_worth) ? 0.10 : 0;

            double total_worth = 0, etotal_worth = 0;
            total_worth += (owned_agent_value[0] / avg_count) * 0.30;
            total_worth += (owned_agent_value[1] / avg_count) * 0.30;
            total_worth += (owned_agent_value[2] / avg_count) * 0.15;
            total_worth += (owned_agent_value[3] / avg_count) * 0.15;
            total_worth += (owned_agent_value[4] / avg_count) * 0.10;

            etotal_worth += (enemy_agent_value[0] / avg_count) * 0.30;
            etotal_worth += (enemy_agent_value[1] / avg_count) * 0.30;
            etotal_worth += (enemy_agent_value[2] / avg_count) * 0.15;
            etotal_worth += (enemy_agent_value[3] / avg_count) * 0.15;
            etotal_worth += (enemy_agent_value[4] / avg_count) * 0.10;


            BackpropagatePhase(total_worth >= etotal_worth);
        }
    }
}
