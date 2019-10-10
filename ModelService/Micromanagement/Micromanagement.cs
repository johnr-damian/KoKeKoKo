using ModelService.Types;
using System;
using System.Collections.Generic;

namespace ModelService.Micromanagement
{
    /// <summary>
    /// Represents a battle based on a game observation or from a CSV file
    /// </summary>
    public partial class Micromanagement
    {
        private Army _owned_units = null;
        private Army _enemy_units = null;
        private Army _postbattle = null;

        public string Rank { get; set; } = default(string);

        public string Filename { get; set; } = default(string);

        /// <summary>
        /// A csv-based micromanagement. Represents a battle from a game observation
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        /// <param name="postbattle"></param>
        public Micromanagement(Army owned_units, Army enemy_units, Army postbattle)
        {
            _owned_units = owned_units;
            _enemy_units = enemy_units;
            _postbattle = postbattle;
        }

        /// <summary>
        /// A gameplay-based micromanagement. Represents a battle from a game observation
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        public Micromanagement(Army owned_units, Army enemy_units)
            : this(owned_units, enemy_units, null) { }

        public List<double> GetMicromanagementAccuracyReport(int number_of_simulations)
        {
            var overall_results = new List<double>();

            var lanchester_random_results = new List<string>();
            var lanchester_priority_results = new List<string>();
            var lanchester_resource_results = new List<string>();

            var staticbased_random_results = new List<string>();
            var staticbased_priority_results = new List<string>();
            var staticbased_resource_results = new List<string>();

            var dynamicbased_random_results = new List<string>();
            var dynamicbased_priority_results = new List<string>();
            var dynamicbased_resource_results = new List<string>();

            //Perform Simulations
            try
            {
                Console.WriteLine($@"Simulating the battle of {Filename}...");
                System.Diagnostics.Trace.WriteLine($@"Current replay file: {Rank}-{Filename}...");

                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    lanchester_random_results.Add(LanchesterBasedPrediction(TargetPolicy.Random).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    lanchester_priority_results.Add(LanchesterBasedPrediction(TargetPolicy.Priority).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    lanchester_resource_results.Add(LanchesterBasedPrediction(TargetPolicy.Resource).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    staticbased_random_results.Add(StaticBasedPrediction(TargetPolicy.Random).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    staticbased_priority_results.Add(StaticBasedPrediction(TargetPolicy.Priority).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    staticbased_resource_results.Add(StaticBasedPrediction(TargetPolicy.Resource).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    dynamicbased_random_results.Add(DynamicBasedPrediction(TargetPolicy.Random).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    dynamicbased_priority_results.Add(DynamicBasedPrediction(TargetPolicy.Priority).Item1);
                for (int simulated = 0; simulated < number_of_simulations; simulated++)
                    dynamicbased_resource_results.Add(DynamicBasedPrediction(TargetPolicy.Resource).Item1);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"GetMicromanagementAccuracyReport() [Simulation] -> {ex.Message}");
                System.Diagnostics.Debugger.Break();
                throw new Exception("");
            }

            //Perform Jaccard Operations
            try
            {
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(lanchester_random_results, _postbattle.ToString()));
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(lanchester_priority_results, _postbattle.ToString()));
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(lanchester_resource_results, _postbattle.ToString()));

                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(staticbased_random_results, _postbattle.ToString()));
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(staticbased_priority_results, _postbattle.ToString()));
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(staticbased_resource_results, _postbattle.ToString()));

                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(dynamicbased_random_results, _postbattle.ToString()));
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(dynamicbased_priority_results, _postbattle.ToString()));
                overall_results.Add(ModelRepositoryService.GetREngine().GetJaccardResult(dynamicbased_resource_results, _postbattle.ToString()));
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($@"GetMicromanagementAccuracyReport() [Jaccard] -> {ex.Message}");
                System.Diagnostics.Debugger.Break();
                throw new Exception("");
            }

            return overall_results;
        }

        public static List<double> GetMicromanagementAccuracyReport(List<List<double>> combat_results)
        {
            var accuracy_results = new List<double>();

            try
            {
                //Get the results of each algorithm+policy across the multiple files
                for (int algorithmpolicy = 0; algorithmpolicy < 9; algorithmpolicy++)
                {
                    var results = new List<double>();
                    foreach (var combat_result in combat_results)
                        results.Add(combat_result[algorithmpolicy]);

                    //Get the standard deviation of results
                    accuracy_results.Add(REngineExtensions.GetStandardDeviation(results));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GetMicromanagementAccuracyReport() -> {ex.Message}");
                accuracy_results.Clear();
            }

            return accuracy_results;
        }


    }

    /// <summary>
    /// A list of available target policy
    /// </summary>
    public enum TargetPolicy
    {
        /// <summary>
        /// Targets a unit based on likely to die
        /// </summary>
        Random,

        /// <summary>
        /// Targets a unit based on a priority
        /// </summary>
        Priority,

        /// <summary>
        /// Targets a unit based on resource worth
        /// </summary>
        Resource
    }
}