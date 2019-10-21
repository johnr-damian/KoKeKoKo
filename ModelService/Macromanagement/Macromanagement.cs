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
            var time = DateTime.Now;
            Rank = rank;
            Filename = filename;
            Owned_Agent = new Agent(owned_agent, time);
            Enemy_Agent = new Agent(enemy_agent, time);
        }

        public override string ToString()
        {
            string stuff = "";
            for (var node = Current_Tree.Root_Node; node.Chosen_Child != null; node = node.Chosen_Child)
                stuff += String.Format($@"{node.GetNodeInformation().Item1},{Convert.ToDouble(node.GetNodeInformation().Item2)}") + Environment.NewLine;

            return stuff;
        }

        public IEnumerable<IEnumerable<double>> GetMacromanagementAccuracyReport(int number_of_simulations, AIAlgorithm algorithm)
        {
            var pomdp_results = new List<List<CostWorth>>();
            var mcts_results = new List<List<CostWorth>>();

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
                    mcts_results.Add(new List<CostWorth>());

                    DateTime end;
                    if (Owned_Agent.EndTime > Enemy_Agent.EndTime)
                        end = Owned_Agent.EndTime;
                    else
                        end = Enemy_Agent.EndTime;

                    foreach (var result in Current_Tree.GeneratePredictedAction(end))
                    {
                        if (result == null)
                            break;
                        mcts_results[0].Add(result.Item2);
                    }
                }
            }
            catch(ArgumentException ex)
            {

            }

            var random = Services.ModelRepositoryService.ModelService.GetModelService();
            //Perform Euclidean Operations
            try
            {
                var enemy_basis = String.Join(",", Enemy_Agent.Basis.Select(basis => Convert.ToDouble(basis.Item3)));
            }
            catch(ArgumentNullException ex)
            {

            }



            string owned_basis = String.Join(",", Owned_Agent.Basis.Select(basis => Convert.ToDouble(basis.Item3)));
            var owned_results_mcts = mcts_results.Select(result => String.Join(",", result.Select(costworth => Convert.ToDouble(costworth))));

            yield return random.GetEuclideanMetric(owned_basis, owned_results_mcts);
        }

        public static List<double> GetMacromanagementAccuracyReport(string rank, List<IEnumerable<IEnumerable<double>>> combat_results)
        {
            var random = Services.ModelRepositoryService.ModelService.GetModelService();

            //For each filename, contains the average results of their algorithm policy
            //This becomes per algorithm containing average results each filename
            var accuracyresult_peralgorithmeachfilename = new List<List<double>>();

            //The euclidean of each results
            var accuracyreport = new List<double>();

            try
            {
                //Get the average per algorithm of each filename
                for(int algorithm = 0; algorithm < 1; algorithm++)
                {
                    accuracyresult_peralgorithmeachfilename.Add(new List<double>());

                    foreach(var combat_result in combat_results)
                    {
                        var current_algorithm = combat_result.ToList();

                        accuracyresult_peralgorithmeachfilename[algorithm].Add(current_algorithm[algorithm].Average());
                    }
                }

                //Create Boxplot
                var resultstring = accuracyresult_peralgorithmeachfilename.Select(result => String.Join(",", result));
                random.CreateBoxPlot(rank, resultstring.ToArray());

                for (int algorithm = 0; algorithm < 1; algorithm++)
                    accuracyreport.Add(random.GetStandardDeviation(accuracyresult_peralgorithmeachfilename[algorithm]));
            }
            catch(Exception ex)
            {

            }

            return accuracyreport;
        }
    }
}
