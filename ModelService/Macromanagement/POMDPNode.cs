using ModelService.Collections;
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
            throw new NotImplementedException();
        }

        protected override void ExpandPhase()
        {
            throw new NotImplementedException();
        }

        protected override void SimulationPhase()
        {
            throw new NotImplementedException();
        }
    }
}
