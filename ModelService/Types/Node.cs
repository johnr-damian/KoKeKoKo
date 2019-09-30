using System.Collections.Generic;

namespace ModelService.Types
{
    /// <summary>
    /// Holds the current observation and simulated future based on observation
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The times of this node has been simulated
        /// </summary>
        private int Simulated_Runs { get; set; } = default(int);

        /// <summary>
        /// The times of this node's <see cref="Player"/> has won
        /// </summary>
        private int Simulated_Wins { get; set; } = default(int);

        /// <summary>
        /// The possible actions that can be simulated
        /// </summary>
        private Queue<string> Possible_Actions { get; set; } = default(Queue<string>);

        /// <summary>
        /// Stores the information related to the AI agent
        /// </summary>
        public Agent Player { get; private set; } = default(Agent);

        /// <summary>
        /// Stores the information related to the enemy agent
        /// </summary>
        public Agent Enemy { get; private set; } = default(Agent);

        /// <summary>
        /// The parent node of this node
        /// </summary>
        public Node Parent { get; private set; } = default(Node);

        /// <summary>
        /// The children of this node
        /// </summary>
        public List<Node> Children { get; private set; } = default(List<Node>);

        /// <summary>
        /// The current node selected by the highest simulated value
        /// </summary>
        public Node Chosen_Child { get; private set; } = default(Node);

        /// <summary>
        /// Checks if there are children for this node
        /// </summary>
        public bool IsExpanded
        {
            get { return (Children.Count != 0); }
        }

        /// <summary>
        /// Stores the simulated information of both <see cref="Agent"/>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <param name="possible_actions"></param>
        public Node(Node parent, Agent player, Agent enemy, params string[] possible_actions)
        {
            Parent = parent;
            Player = player;
            Enemy = enemy;
            Children = new List<Node>();
            Chosen_Child = null;
            Possible_Actions = new Queue<string>(possible_actions);
        }

        public abstract Node Select();

        protected abstract void Expand();

        protected abstract void Simulate();

        protected abstract void Backpropagate();

        /// <summary>
        /// Returns the chosen action by calling <see cref="Agent.CreateMessage"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Chosen_Child.Player.CreateMessage();
    }
}