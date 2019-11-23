using ModelService.Collections;
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
            for(int test = 0; test < 5; test++)
                Children.Add(new MCTSNode(Owned_Agent.Copy(), Enemy_Agent.Copy(), this));

            Children.ForEach(child => ((MCTSNode)child).SimulationPhase());
        }

        protected override void SimulationPhase()
        {
            Owned_Agent.ApplyChosenAction("TEST");
            Enemy_Agent.ApplyChosenAction("TEST");

            BackpropagatePhase(true);
        }
    }
}
