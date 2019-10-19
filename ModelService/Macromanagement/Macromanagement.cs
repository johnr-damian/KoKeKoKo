using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.ValueTypes;
using ModelService.CollectionTypes;

namespace ModelService.Macromanagement
{
    public partial class Macromanagement
    {
        #region Properties
        /// <summary>
        /// The agent that is controlled by this model
        /// </summary>
        private Agent Owned_Agent { get; set; } = default(Agent);

        /// <summary>
        /// The opposing agent in the game. The agent that must be destroyed
        /// </summary>
        private Agent Enemy_Agent { get; set; } = default(Agent);

        /// <summary>
        /// The current node of the AI Algorithm tree
        /// </summary>
        private Tree Current_Tree { get; set; } = default(Tree);

        /// <summary>
        /// The rank of this replay
        /// </summary>
        public string Rank { get; private set; } = default(string);

        /// <summary>
        /// The filename of this replay
        /// </summary>
        public string Filename { get; private set; } = default(string); 
        #endregion

        /// <summary>
        /// The AI algorithm to be used to generate actions for the agent
        /// </summary>
        public enum AIAlgorithm
        {
            /// <summary>
            /// Use POMDP to generate actions. It uses <see cref="Services.ModelRepositoryService.ModelService.REngine"/> 
            /// to create the predicted actions for agent.
            /// </summary>
            POMDP,

            /// <summary>
            /// Use MCTS to generate actions. It uses pure C# to create the 
            /// predicted actions for the agent.
            /// </summary>
            MCTS
        }

        public Macromanagement(string rank, string filename, string owned_agent, string enemy_agent)
        {
            Rank = rank;
            Filename = filename;
            Owned_Agent = new Agent(owned_agent);
            Enemy_Agent = new Agent(enemy_agent);
        }

        public List<double> GetMacromanagementAccuracyReport(int number_of_simulations, AIAlgorithm algorithm)
        {
            var overall_results = new List<double>();

            var pomdp_results = new List<List<CostWorth>>();
            var mcts_results = new List<CostWorth>();

            switch (algorithm)
            {
                case AIAlgorithm.POMDP:
                    Current_Tree = new POMDPAlgorithm(Owned_Agent.GetDeepCopy(), Enemy_Agent.GetDeepCopy());
                    break;
                case AIAlgorithm.MCTS:
                    Current_Tree = new MCTSAlgorithm(Owned_Agent.GetDeepCopy(), Enemy_Agent.GetDeepCopy());
                    break;
            }

            try
            {
                for(int simulated = 0; simulated < number_of_simulations; simulated++)
                {
                    pomdp_results.Add(new List<CostWorth>());

                    DateTime end;
                    if (Owned_Agent.EndTime > Enemy_Agent.EndTime)
                        end = Owned_Agent.EndTime;
                    else
                        end = Enemy_Agent.EndTime;

                    foreach (var result in Current_Tree.GeneratePredictedAction(end))
                        pomdp_results[0].Add(result.Item2);
                }
            }
            catch(ArgumentException ex)
            {

            }

            //Perform Linear thingy operation
            try
            {

            }
            catch(ArgumentNullException ex)
            {

            }

            var basis = ModelRepositoryService.TestForEuclideanResult();
            var random = Services.ModelRepositoryService.ModelService.GetModelService();
            var test_result = random.GetEuclideanMetric(String.Join(",", basis.Select(b => b.GetTotalWorth())), pomdp_results.Select(result => String.Join(",", result.Select(r => r.GetTotalWorth()))));

            overall_results.Add(test_result.Average());

            return overall_results;
        }
    }
}
