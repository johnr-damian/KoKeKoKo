using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    /// <summary>
    /// Represents a battle based on a game observation or on a CSV file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Micromanagement<T> where T : Unit
    {
        private Army _owned_units = null;
        private Army _enemy_units = null;

        /// <summary>
        /// Stores this agent's army and its opposing army
        /// Represents a battle either in game observation or from a CSV file
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        public Micromanagement(Army owned_units, Army enemy_units)
        {
            _owned_units = owned_units;
            _enemy_units = enemy_units;
        }

        /// <summary>
        /// Performs all prediction algorithm using all target policy.
        /// It performs these again based on <paramref name="repetitions"/>.
        /// After it performs, it calculate the accuracy using the Jaccard index
        /// </summary>
        /// <param name="repetitions">The number of times to repeat all algorithms using all policies</param>
        public List<double> GetMicromanagementAccuracy(int repetitions)
        {
            var algorithm_results = new Dictionary<string, string>();
            var threads = new List<Thread>();
            var jaccard_result = new List<double>();

            try
            {
                //Lanchester
                var lanchester_thread = new Thread(new ThreadStart(() => algorithm_results.Add("Lanchester", LanchesterBasedPrediction(TargetPolicy.Random | TargetPolicy.Priority | TargetPolicy.Resource))));
                lanchester_thread.Start();
                //Static
                var static_thread = new Thread(new ThreadStart(() => algorithm_results.Add("Static", StaticBasedPrediction(TargetPolicy.Random | TargetPolicy.Priority | TargetPolicy.Resource))));
                static_thread.Start();
                //Dynamic
                var dynamic_thread = new Thread(new ThreadStart(() => algorithm_results.Add("Dynamic", DynamicBasedPrediction(TargetPolicy.Random | TargetPolicy.Priority | TargetPolicy.Resource))));
                dynamic_thread.Start();

                //Add threads
                threads.Add(lanchester_thread);
                threads.Add(static_thread);
                threads.Add(dynamic_thread);

                //TODO
                threads.ForEach(thread => thread.Join());
                Console.WriteLine("4");
                Console.WriteLine("4");
                Console.WriteLine("4");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to get the accuracy result for the micromanagement...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> GetMicromanagementAccuracy(): \n\t{ex.Message}");

                jaccard_result.Clear();
            }

            return jaccard_result;
        }
    }
}
