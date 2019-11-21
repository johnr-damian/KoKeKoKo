using System.Collections.Generic;

namespace ModelService.Collections
{
    /// <summary>
    /// Facilitates the different processes to generate a predicted action
    /// from the simulation between the actions of two <see cref="SimulatedAgent"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SimulationNode<T> where T : SimulationNode<T>
    {
        #region Properties
        /// <summary>
        /// The owned agent is a participating agent in the game. It serves as the reference point of
        /// the model in which the results and other related computation is based on owned agent. 
        /// </summary>
        protected SimulatedAgent Owned_Agent { get; set; } = default(SimulatedAgent);

        /// <summary>
        /// The enemy agent is the other participating agent in the game. It serves as the opposing
        /// agent that must be defeated to make the node considered as won.
        /// </summary>
        protected SimulatedAgent Enemy_Agent { get; set; } = default(SimulatedAgent);

        /// <summary>
        /// The parent node the current node.
        /// </summary>
        public SimulationNode<T> Parent { get; protected set; } = default(SimulationNode<T>);

        /// <summary>
        /// The chosen child of the current node after the <see cref="SelectPhase"/> has finished.
        /// </summary>
        public SimulationNode<T> Child { get; protected set; } = default(SimulationNode<T>);

        /// <summary>
        /// The children of the current node after the <see cref="ExpandPhase"/>.
        /// </summary>
        protected List<SimulationNode<T>> Children { get; set; } = default(List<SimulationNode<T>>);

        /// <summary>
        /// Checks if the current node has been expanded for simulation.
        /// </summary>
        public bool IsExpanded
        {
            get { return (Children.Count > 0); }
        }

        /// <summary>
        /// Returns the depth of the current node. If depth is 0, it means
        /// the current node is a root node.
        /// </summary>
        public int Depth
        {
            get
            {
                int depth = -1;
                for (var node_iterator = this; node_iterator != null; depth++)
                    node_iterator = node_iterator.Parent;

                return depth;
            }
        }

        /// <summary>
        /// The number of times of this node has been simulated.
        /// </summary>
        public int Runs { get; protected set; } = default(int);

        /// <summary>
        /// The number of times of this node has been considered as won
        /// </summary>
        public int Wins { get; protected set; } = default(int);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the required properties to start simulating the game. This
        /// constructor is used for creating instance where the source of information is
        /// from a CSV file.
        /// </summary>
        public SimulationNode()
        {
            //Initialize the players
            Owned_Agent = new SimulatedAgent();
            Enemy_Agent = new SimulatedAgent();

            //Initialize the nodes
            Parent = null;
            Child = null;
            Children = new List<SimulationNode<T>>();

            //Initialize other related properties
            Runs = 0;
            Wins = 0;
        }

        /// <summary>
        /// Initializes the required properties to start simulating the game. This
        /// constructor is used for creating instance where the source of information is
        /// from C++ Agent.
        /// </summary>
        /// <param name="agent_name"></param>
        /// <param name="micromanagement"></param>
        public SimulationNode(string agent_name, IEnumerable<string> micromanagement)
        {
            //Initialize the players
            Owned_Agent = new SimulatedAgent(agent_name, micromanagement);
            Enemy_Agent = new SimulatedAgent();

            //Initialize the nodes
            Parent = null;
            Child = null;
            Children = new List<SimulationNode<T>>();

            //Initialize other related properties
            Runs = 0;
            Wins = 0;
        }

        /// <summary>
        /// Initializes the required properties to continue simulating the game. This 
        /// constructor is used for expanding the parent node where the source of information
        /// is from the <see cref="Parent"/> node.
        /// </summary>
        /// <param name="owned_agent"></param>
        /// <param name="enemy_agent"></param>
        /// <param name="parent"></param>
        protected SimulationNode(SimulatedAgent owned_agent, SimulatedAgent enemy_agent, SimulationNode<T> parent)
        {
            //Initialize the players
            Owned_Agent = owned_agent;
            Enemy_Agent = enemy_agent;

            //Initialize the nodes
            Parent = parent;
            Child = null;
            Children = new List<SimulationNode<T>>();

            //Initialize other related properties
            Runs = 0;
            Wins = 0;
        }
        #endregion

        #region Methods
        /// <summary>
        /// The Select phase is the process where the current node must select
        /// a child from its <see cref="Children"/>. The <see cref="Child"/> that was chosen
        /// after the different phase is the one that would continue further the simulation.
        /// </summary>
        /// <returns></returns>
        public abstract T SelectPhase();

        /// <summary>
        /// The Expand phase is the process where the current node will be expanded to
        /// have <see cref="Children"/>. After the expansion phase, these children will be simulated
        /// to get their value.
        /// </summary>
        protected abstract void ExpandPhase();

        /// <summary>
        /// The Simulation phase is the process where the current node will be simulated to
        /// have a value. After the simulation phase, it will proceed to backpropagate to update
        /// the information in other nodes.
        /// </summary>
        protected abstract void SimulationPhase();

        /// <summary>
        /// The Backpropagate phase is the process where the current node will go back to its
        /// parent node to update the information regarding the value of the node and ancestral line.
        /// </summary>
        /// <param name="simulation_result"></param>
        public virtual void BackpropagatePhase(bool simulation_result)
        {
            //Update the necesseray properties
            Runs += 1;
            Wins += ((simulation_result) ? 1 : 0); //If the current node is considered as won

            //Update the necessary properties in ancestor
            Parent?.BackpropagatePhase(simulation_result);
        } 
        #endregion
    }
}
