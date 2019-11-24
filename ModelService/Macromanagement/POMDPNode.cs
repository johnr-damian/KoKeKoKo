using ModelService.Collections;
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
            //Do we have to?

            
        }
    }
}
