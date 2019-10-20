using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.CollectionTypes;
using ModelService.Types;
using ModelService.ValueTypes;

namespace ModelService.Macromanagement
{
    public partial class Macromanagement
    {
        public class POMDPAlgorithm : CollectionTypes.Tree
        {
            private class POMDPNode : Node
            {
                /// <summary>
                /// Contains a simulation at a specific time. It also 
                /// contains the current agents.
                /// </summary>
                /// <param name="parent_node"></param>
                /// <param name="owned_agent"></param>
                /// <param name="enemy_agent"></param>
                public POMDPNode(Node parent_node, Agent owned_agent, Agent enemy_agent) 
                    : base(parent_node, owned_agent, enemy_agent) { }

                public override double GetNodeUCTValue()
                {
                    throw new NotImplementedException();
                }

                public override Node SelectAChildNode()
                {
                    if (!IsExpanded)
                        ExpandCurrentNode();

                    double bestuctnode = Double.MinValue;
                    Node bestchildnode = default(POMDPNode);
                    foreach(POMDPNode childnode in Children)
                    {
                        var current_uct = childnode.GetNodeUCTValue();
                        if(current_uct > bestuctnode)
                        {
                            bestuctnode = current_uct;
                            bestchildnode = childnode;
                        }
                    }

                    Chosen_Child = bestchildnode;
                    return Chosen_Child;
                }

                public override void Backpropagate()
                {
                    throw new NotImplementedException();
                }

                protected override void ExpandCurrentNode()
                {
                    var states = String.Join(",", GeneratePotentialStates());
                    var actions = String.Join(",", GeneratePotentialActions());
                    var child = new POMDPNode(this, Current_Owned_Agent.GetDeepCopy(), Current_Enemy_Agent.GetDeepCopy());

                    child.SimulateCurrentNode(null);
                }

                private IEnumerable<string> GeneratePotentialStates()
                {
                    throw new NotImplementedException();
                }

                protected override IEnumerable<string> GeneratePotentialActions()
                {
                    throw new NotImplementedException();
                }

                protected override void SimulateCurrentNode(string potential_action)
                {
                    var modelservice = Services.ModelRepositoryService.ModelService.GetModelService().REngine;

                    modelservice.Evaluate(@"discount <- 0.75");
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Holds the essential information about the game, and simulates
            /// it using POMDP
            /// </summary>
            /// <param name="owned_agent"></param>
            /// <param name="enemy_agent"></param>
            public POMDPAlgorithm(Types.Agent owned_agent, Types.Agent enemy_agent)
                : base(owned_agent, enemy_agent)
            {
                Root_Node = new POMDPNode(null, owned_agent, enemy_agent);
                Current_Node = Root_Node;
            }

            /// <summary>
            /// Generates a predicted action using POMDP algorithm
            /// </summary>
            /// <param name="endtime"></param>
            /// <returns></returns>
            public override IEnumerable<Tuple<string, CostWorth>> GeneratePredictedAction(DateTime endtime)
            {
                var current_time = DateTime.Now;

                //While it is still not yet the end of the game
                //Keep generating predicted actions
                while (current_time != endtime)
                {
                    //The chosen action that will be executed by the agent
                    var predicted_action = default(Tuple<string, ValueTypes.CostWorth>);

                    try
                    {
                        //Get a child node that has the best move
                        var selected_node = Current_Node.SelectAChildNode();

                        //Get the predicted action
                        predicted_action = selected_node.GetNodeInformation();

                        //Update the Current Node
                        Current_Node = selected_node;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($@"GeneratePredictedAction() -> {ex.Message}");
                        predicted_action = null;
                        throw new Exception("An error occurred! There is an invalid data format to generate an action...");
                    }
                    catch (NullReferenceException ex)
                    {
                        Console.WriteLine($@"GeneratePredictedAction() -> {ex.Message}");
                        predicted_action = null;
                        throw new Exception("An error occurred! There is a null data that is being accessed to generate an action...");
                    }

                    yield return predicted_action;
                }
            }
        }
    }
}
