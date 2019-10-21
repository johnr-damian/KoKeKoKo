using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// A csv-based micromanagement. Represents a battle from a file
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

        public IEnumerable<IEnumerable<double>> GetMicromanagementAccuracyReport(int number_of_simulations)
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

            var random = Services.ModelRepositoryService.ModelService.GetModelService();

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
                throw new Exception(ex.Message);
            }

            //Perform Jaccard Operations
            var lanchester_random_plot = random.GetJaccardSimilarity(_postbattle.ToString(), lanchester_random_results);
            var lanchester_priority_plot = random.GetJaccardSimilarity(_postbattle.ToString(), lanchester_priority_results);
            var lanchester_resource_plot = random.GetJaccardSimilarity(_postbattle.ToString(), lanchester_resource_results);

            var static_random_plot = random.GetJaccardSimilarity(_postbattle.ToString(), staticbased_random_results);
            var static_priority_plot = random.GetJaccardSimilarity(_postbattle.ToString(), staticbased_priority_results);
            var static_resource_plot = random.GetJaccardSimilarity(_postbattle.ToString(), staticbased_resource_results);

            var dynamic_random_plot = random.GetJaccardSimilarity(_postbattle.ToString(), dynamicbased_random_results);
            var dynamic_priority_plot = random.GetJaccardSimilarity(_postbattle.ToString(), dynamicbased_priority_results);
            var dynamic_resource_plot = random.GetJaccardSimilarity(_postbattle.ToString(), dynamicbased_resource_results);

            //try
            //{
                

            //    //random.CreateBoxPlot(
            //    //    String.Join(",", lanchester_random_plot), String.Join(",", lanchester_priority_plot), String.Join(",", lanchester_resource_plot),
            //    //    String.Join(",", static_random_plot), String.Join(",", static_priority_plot), String.Join(",", static_resource_plot),
            //    //    String.Join(",", dynamic_random_plot), String.Join(",", dynamic_priority_plot), String.Join(",", dynamic_resource_plot));

            //    //overall_results.Add(lanchester_random_plot.Average());
            //    //overall_results.Add(lanchester_priority_plot.Average());
            //    //overall_results.Add(lanchester_resource_plot.Average());

            //    //overall_results.Add(static_random_plot.Average());
            //    //overall_results.Add(static_priority_plot.Average());
            //    //overall_results.Add(static_resource_plot.Average());

            //    //overall_results.Add(dynamic_random_plot.Average());
            //    //overall_results.Add(dynamic_priority_plot.Average());
            //    //overall_results.Add(dynamic_resource_plot.Average());
            //}
            //catch (ArgumentNullException ex)
            //{
            //    Console.WriteLine($@"GetMicromanagementAccuracyReport() [Jaccard] -> {ex.Message}");
            //    throw new Exception(ex.Message);
            //}

            //return overall_results;

            yield return lanchester_random_plot;
            yield return lanchester_priority_plot;
            yield return lanchester_resource_plot;

            yield return static_random_plot;
            yield return static_priority_plot;
            yield return static_resource_plot;

            yield return dynamic_random_plot;
            yield return dynamic_priority_plot;
            yield return dynamic_resource_plot;
        }

        public static List<double> GetMicromanagementAccuracyReport(List<List<double>> combat_results)
        {
            var accuracy_results = new List<double>();
            var random = Services.ModelRepositoryService.ModelService.GetModelService();

            try
            {
                //Get the results of each algorithm+policy across the multiple files
                for (int algorithmpolicy = 0; algorithmpolicy < 9; algorithmpolicy++)
                {
                    var results = new List<double>();
                    foreach (var combat_result in combat_results)
                        results.Add(combat_result[algorithmpolicy]);

                    //Get the standard deviation of results
                    accuracy_results.Add(random.GetStandardDeviation(results));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GetMicromanagementAccuracyReport() -> {ex.Message}");
                accuracy_results.Clear();
            }

            return accuracy_results;
        }
                                                                       //Filename -> Algorithm -> Result
        public static List<double> GetMicromanagementAccuracyReport(string rank, List<IEnumerable<IEnumerable<double>>> combat_results)
        {
            var random = Services.ModelRepositoryService.ModelService.GetModelService();

            //For each filename, contains the average results of their algorithm policy
            //This becomes per algorithm policy containing the average of their results each filename
            var accuracyresult_peralgorithmeachfilename = new List<List<double>>();

            //The standard deviation of each results
            var accuracyreport = new List<double>();

            try
            {
                //Get the average per algorithm policy of each filename
                for(int algorithmpolicy = 0; algorithmpolicy < 9; algorithmpolicy++)
                {
                    accuracyresult_peralgorithmeachfilename.Add(new List<double>());

                    foreach(var combat_result in combat_results)
                    {
                        var current_algorithmresult = combat_result.ToList();

                        accuracyresult_peralgorithmeachfilename[algorithmpolicy].Add(current_algorithmresult[algorithmpolicy].Average());
                    }
                }

                //Create Boxplot
                var resultsstring = accuracyresult_peralgorithmeachfilename.Select(result => String.Join(",", result));
                random.CreateBoxPlot(rank, resultsstring.ToArray());

                for (int algorithmpolicy = 0; algorithmpolicy < 9; algorithmpolicy++)
                    accuracyreport.Add(random.GetStandardDeviation(accuracyresult_peralgorithmeachfilename[algorithmpolicy]));
            }
            catch(Exception ex)
            {

            }

            return accuracyreport;
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