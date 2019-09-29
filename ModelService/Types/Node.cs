using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    /// <summary>
    /// Holds the current observation on the player
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Stores the information related to the agent. This includes the
        /// action chosen by the algorithm
        /// </summary>
        public Player Agent { get; set; } = default(Player);

        /// <summary>
        /// Stores the information related to the enemy. This includes the
        /// action chosen by the algorithm
        /// </summary>
        public Player Enemy { get; set; } = default(Player);

        /// <summary>
        /// The parent node of this node
        /// </summary>
        public Node Parent { get; protected set; } = default(Node);

        /// <summary>
        /// The children of this node
        /// </summary>
        public List<Node> Children { get; protected set; } = default(List<Node>);

        /// <summary>
        /// The chosen child using <see cref="Select"/>
        /// </summary>
        public Node Chosen_Node { get; protected set; } = default(Node);

        /// <summary>
        /// The worth of this node
        /// </summary>
        private double Current_Value { get; set; } = default(double);

        /// <summary>
        /// The number of runs this node had performed
        /// </summary>
        private double Runs { get; set; } = default(double);

        /// <summary>
        /// Checks if there are children of this node. Returns true if it
        /// has children, else if it does not have
        /// </summary>
        public bool IsExpanded
        {
            get { return (Children.Count != 0); }
        }

        public string Chosen_Action { get; protected set; } = default(string);

        public Node(Node parent, Player agent, Player enemy)
        {
            Parent = parent;
            Agent = agent;
            Enemy = enemy;
        }

        public Node(Node parent, Player agent, Player enemy, string action)
        : this(parent, agent, enemy)
        {
            Chosen_Action = action;
        }

        /// <summary>
        /// Selects a node based on the policy
        /// </summary>
        public void Select()
        {
            Node chosen = null;
            double highest_value = Double.MinValue;

            foreach(var node in Children)
            {
                var temp_highest_value = node.GetValueOfNode();
                if(temp_highest_value > highest_value)
                {
                    highest_value = temp_highest_value;
                    chosen = node;
                }
            }

            Chosen_Node = chosen;
        }

        /// <summary>
        /// Based on the available actions of player.
        /// Create a node using that action
        /// </summary>
        public void Expand()
        {
            var possible_actions = Agent.Available_Actions.Split(',');
            foreach (var action in possible_actions)
                Children.Add(new Node(this, Agent.GetDeepCopy(), Enemy.GetDeepCopy(), action));
        }

        public void Simulation()
        {
            //Do stuff
            Current_Value = new Random().Next();
        }

        /// <summary>
        /// Keep backpropagating until root
        /// </summary>
        private void Backpropagate()
        {
            Runs++;
            Parent?.Backpropagate();
        }

        /// <summary>
        /// The UCT value of this node
        /// </summary>
        /// <returns></returns>
        private double GetValueOfNode() => ((Current_Value / Runs) + (Math.Sqrt((2 * Math.Log(Parent.Runs)) / Runs)));
    }
}
