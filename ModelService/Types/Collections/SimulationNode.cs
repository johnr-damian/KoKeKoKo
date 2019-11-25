using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ModelService.Collections
{
    /// <summary>
    /// Facilitates the different processes to generate a predicted action
    /// from the simulation between the actions of two <see cref="SimulatedAgent"/>.
    /// </summary>
    public abstract class SimulationNode : IFormattable
    {
        #region Properties
        /// <summary>
        /// The owned agent is a participating agent in the game. It serves as the reference point of
        /// the model in which the results and other related computation is based on owned agent. 
        /// </summary>
        public SimulatedAgent Owned_Agent { get; protected set; } = default(SimulatedAgent);

        /// <summary>
        /// The enemy agent is the other participating agent in the game. It serves as the opposing
        /// agent that must be defeated to make the node considered as won.
        /// </summary>
        public SimulatedAgent Enemy_Agent { get; protected set; } = default(SimulatedAgent);

        /// <summary>
        /// The parent node the current node.
        /// </summary>
        public SimulationNode Parent { get; protected set; } = default(SimulationNode);

        /// <summary>
        /// The chosen child of the current node after the <see cref="SelectPhase"/> has finished.
        /// </summary>
        public SimulationNode Child { get; protected set; } = default(SimulationNode);

        /// <summary>
        /// The children of the current node after the <see cref="ExpandPhase"/>.
        /// </summary>
        protected List<SimulationNode> Children { get; set; } = default(List<SimulationNode>);

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
        /// <param name="owned_name"></param>
        /// <param name="enemy_name"></param>
        public SimulationNode(string owned_name, string enemy_name)
        {
            //Initialize the players
            Owned_Agent = new SimulatedAgent(owned_name);
            Enemy_Agent = new SimulatedAgent(enemy_name);

            //Initialize the nodes
            Parent = null;
            Child = null;
            Children = new List<SimulationNode>();

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
            Enemy_Agent = new SimulatedAgent("enemy_agent");

            //Initialize the nodes
            Parent = null;
            Child = null;
            Children = new List<SimulationNode>();

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
        protected SimulationNode(SimulatedAgent owned_agent, SimulatedAgent enemy_agent, SimulationNode parent)
        {
            //Initialize the players
            Owned_Agent = owned_agent;
            Enemy_Agent = enemy_agent;

            //Initialize the nodes
            Parent = parent;
            Child = null;
            Children = new List<SimulationNode>();

            //Initialize other related properties
            Runs = 0;
            Wins = 0;
        }
        #endregion

        #region Simulation Methods
        /// <summary>
        /// The Select phase is the process where the current node must select
        /// a child from its <see cref="Children"/>. The <see cref="Child"/> that was chosen
        /// after the different phase is the one that would continue further the simulation.
        /// </summary>
        /// <returns></returns>
        public abstract SimulationNode SelectPhase();

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
            Console.WriteLine($@"Currently Backpropagating... Your current depth is {Depth}");

            //Update the necesseray properties
            Runs += 1;
            Wins += ((simulation_result) ? 1 : 0); //If the current node is considered as won

            //Update the necessary properties in ancestor
            Parent?.BackpropagatePhase(simulation_result);
        } 

        /// <summary>
        /// Updates the current node by updating both of the agents for simulation.
        /// </summary>
        /// <param name="source"></param>
        public virtual void UpdateSimulationNode(IEnumerable<string> source)
        {
            //Update the owned agent
            Owned_Agent.UpdateSimulatedAgent(source.Take(1).Single().Split(','), source.Skip(1).Take(1).Single().Split(','));

            //If there has been a known units for the enemy agent
            if (source.Count() == 3)
                Enemy_Agent.UpdateSimulatedAgent(source.Skip(2).Take(1).Single().Split(','));
        }
        #endregion

        #region ToString Methods
        /// <summary>
        /// Returns the <see cref="Owned_Agent"/>'s <see cref="SimulatedAgent.Action"/> that is
        /// used in messaging C++ Agent
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString("M", CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a specific information about <see cref="SimulatedAgent"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a specific information about <see cref="SimulatedAgent"/> using a 
        /// specific format provider.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch(format.ToUpperInvariant())
            {
                case "M":
                    return Owned_Agent.ToString();
                case "R":
                    return Owned_Agent.ToString("R");
                case "IR":
                    return Enemy_Agent.ToString("R");
                default:
                    throw new Exception($@"Failed to format into string...");
            }
        } 
        #endregion
    }
}
