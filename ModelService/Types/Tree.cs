using System;
using System.Collections.Generic;

namespace ModelService.CollectionTypes
{
    public abstract class Tree
    {
        protected Types.Agent Owned_Agent { get; set; } = default(Types.Agent);

        protected Types.Agent Enemy_Agent { get; set; } = default(Types.Agent);

        /// <summary>
        /// The root node of the AI tree
        /// </summary>
        public Node Root_Node { get; protected set; } = default(Node);

        /// <summary>
        /// The current node of the AI tree
        /// </summary>
        protected Node Current_Node { get; set; } = default(Node);

        public abstract class Node
        {
            protected Types.Agent Current_Owned_Agent { get; set; } = default(Types.Agent);

            protected Types.Agent Current_Enemy_Agent { get; set; } = default(Types.Agent);

            public Node Parent_Node { get; protected set; } = default(Node);

            public Node Chosen_Child { get; protected set; } = default(Node);

            protected List<Node> Children { get; set; } = default(List<Node>);

            /// <summary>
            /// Checks if <see cref="Children"/> is null or the count is 0
            /// </summary>
            public bool IsExpanded { get => (Children.Count != 0); }

            /// <summary>
            /// If this node has been visited and simulated
            /// </summary>
            public bool IsVisited { get; protected set; } = default(bool);

            /// <summary>
            /// The number of times that this node has been simulated
            /// </summary>
            public int Simulated_Runs { get; protected set; } = default(int);

            /// <summary>
            /// The number of times that this node's <see cref="Current_Owned_Agent"/>
            /// has won in the battle
            /// </summary>
            protected int Simulated_Wins { get; set; } = default(int);

            /// <summary>
            /// Stores essential information about the game, and 
            /// simulates both <see cref="Types.Agent"/>
            /// </summary>
            /// <param name="parent_node"></param>
            /// <param name="owned_agent"></param>
            /// <param name="enemy_agent"></param>
            public Node(Node parent_node, Types.Agent owned_agent, Types.Agent enemy_agent)
            {
                Parent_Node = parent_node;
                Current_Owned_Agent = owned_agent;
                Current_Enemy_Agent = enemy_agent;
                Children = new List<Node>();

                if (Parent_Node == null)
                    Current_Height = 0;
                else
                    UpdateHeight();
            }

            public void UpdateHeight()
            {
                Current_Height++;
                if (Parent_Node != null)
                {
                    Parent_Node.UpdateHeight();
                }
            }

            protected abstract IEnumerable<string> GeneratePotentialActions();

            public abstract Node SelectAChildNode();

            public abstract double GetNodeUCTValue();

            protected abstract void ExpandCurrentNode();

            protected abstract void SimulateCurrentNode(string potential_action);

            public abstract void Backpropagate();

            public Tuple<string, ValueTypes.CostWorth> GetNodeInformation() => new Tuple<string, ValueTypes.CostWorth>(Current_Owned_Agent.Chosen_Action, Current_Owned_Agent.Worth);

            public int Current_Height { get; set; } = default(int);

            public int GetTrueHeight()
            {
                if (Parent_Node != null)
                    return Parent_Node.GetTrueHeight() + 1;
                else
                    return 0;
            }
        }

        public Tree(Types.Agent owned_agent, Types.Agent enemy_agent)
        {
            Owned_Agent = owned_agent;
            Enemy_Agent = enemy_agent;
        }

        /// <summary>
        /// Generates a predicted action real-time during gameplay
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Tuple<string, ValueTypes.CostWorth>> GeneratePredictedAction() => GeneratePredictedAction(DateTime.Now.AddSeconds(15));

        /// <summary>
        /// Generates a predicted action limited by the end time in the CSV file
        /// </summary>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public abstract IEnumerable<Tuple<string, ValueTypes.CostWorth>> GeneratePredictedAction(DateTime endtime);

        public override string ToString()
        {
            string actions_done = "";
            Node current = Current_Node;

            actions_done = Current_Node.GetNodeInformation().Item1;
            while(current.Parent_Node != null)
            {
                current = current.Parent_Node;
                actions_done += ("," + current.GetNodeInformation().Item1);
            }

            return actions_done;
        }
    }
}