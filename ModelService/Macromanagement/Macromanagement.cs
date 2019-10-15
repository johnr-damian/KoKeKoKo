using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement
{
    public partial class Macromanagement
    {
        private Agent _owned_player = null;
        private Agent _enemy_player = null;

        public string Rank { get; set; } = default(string);

        public string Filename { get; set; } = default(string);

        /// <summary>
        /// A csv-based macromanagement. Represents a battle from a file
        /// </summary>
        /// <param name="owned_player"></param>
        /// <param name="enemy_player"></param>
        public Macromanagement(Agent owned_player, Agent enemy_player)
        {
            _owned_player = owned_player;
            _enemy_player = enemy_player;
        }

        public Macromanagement()
        {

        }

        /// <summary>
        /// A gameplay-based macromanagement. Represents a battle from a game observation
        /// </summary>
        /// <param name="owned_player"></param>
        public Macromanagement(Agent owned_player) 
            : this(owned_player, owned_player.GetDeepCopy()) { }

        public List<double> GetMacromanagementAccuracyReport(int number_of_simulations)
        {
            var overall_results = new List<double>();

            var pomdp_results = new List<List<CostWorth>>();
            var mcts_results = new List<CostWorth>();

            try
            {
                for(int simulated = 0; simulated < number_of_simulations; simulated++)
                {
                    pomdp_results.Add(new List<CostWorth>());

                    foreach (var stuff in POMDP())
                        pomdp_results[0].Add(stuff.Item2);
                }

                for(int simulated = 0; simulated < number_of_simulations; simulated++)
                {

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
            var test_result = ModelRepositoryService.GetREngine().GetEuclideanResult(basis, pomdp_results);

            overall_results.Add(test_result);

            return overall_results;
        }
    }

    public enum AIAlgorithm
    {
        POMDP,
        MCTS
    }
}
