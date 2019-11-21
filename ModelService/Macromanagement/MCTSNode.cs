using ModelService.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement
{
    public class MCTSNode : SimulationNode<MCTSNode>
    {
        public double UCT
        {
            get
            {
                return 0;
            }
        }

        #region Constructors
        /// <summary>
        /// Initializes the required properties to start simulating the game using
        /// the Monte Carlo Tree Search Algorithm. This constructor is used for creating
        /// instance where the source of information is from a CSV file.
        /// </summary>
        public MCTSNode()
            : base() { }

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

        public override MCTSNode SelectPhase()
        {
            return null;
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
