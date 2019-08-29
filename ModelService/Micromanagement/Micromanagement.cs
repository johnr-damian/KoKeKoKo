using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T, C> where T : IEnumerable<C> where C : Types.Unit
    {
        private T _owned_units = default(T);
        private T _enemy_units = default(T);

        /// <summary>
        /// Represents a battle either in game observation or from a CSV file
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        public Micromanagement(T owned_units, T enemy_units)
        {
            _owned_units = owned_units;
            _enemy_units = enemy_units;
        }

        public string GetSummaryOfResults(T final_owned_units, T final_enemy_units)
        {
            throw new NotImplementedException();
        }

        public void GetSummaryOfResults()
        {
                                                    //Algorithm       Policy    Repeated Results
            var algorithm_results = new Dictionary<string, Dictionary<string, List<Tuple<T, T>>>>();
            var threads = new List<Thread>();
            string lanchester_algo = "Lanchester", static_algo = "Static", dynamic_algo = "Dynamic", random = "Random", priority = "Priority", resource = "Resource";
            int repeat_test = 5;

            algorithm_results.Add(lanchester_algo, new Dictionary<string, List<Tuple<T, T>>>());
            algorithm_results.Add(static_algo, new Dictionary<string, List<Tuple<T, T>>>());
            algorithm_results.Add(dynamic_algo, new Dictionary<string, List<Tuple<T, T>>>());

            algorithm_results[lanchester_algo].Add(random, new List<Tuple<T, T>>());
            algorithm_results[lanchester_algo].Add(priority, new List<Tuple<T, T>>());
            algorithm_results[lanchester_algo].Add(resource, new List<Tuple<T, T>>());
            algorithm_results[static_algo].Add(random, new List<Tuple<T, T>>());
            algorithm_results[static_algo].Add(priority, new List<Tuple<T, T>>());
            algorithm_results[static_algo].Add(resource, new List<Tuple<T, T>>());
            algorithm_results[dynamic_algo].Add(random, new List<Tuple<T, T>>());
            algorithm_results[dynamic_algo].Add(priority, new List<Tuple<T, T>>());
            algorithm_results[dynamic_algo].Add(resource, new List<Tuple<T, T>>());

            //Lanchester-Random
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[lanchester_algo][random].Add(LanchesterBasedPrediction(TargetPolicy.Random)))));
            //Lanchester-Priority
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[lanchester_algo][priority].Add(LanchesterBasedPrediction(TargetPolicy.Priority)))));
            //Lanchester-Resource
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[lanchester_algo][resource].Add(LanchesterBasedPrediction(TargetPolicy.Resource)))));

            //Static-Random
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[static_algo][random].Add(StaticBasedPrediction(TargetPolicy.Random)))));
            //Static-Priority
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[static_algo][priority].Add(StaticBasedPrediction(TargetPolicy.Priority)))));
            //Static-Resource
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[static_algo][resource].Add(StaticBasedPrediction(TargetPolicy.Resource)))));

            //Dynamic-Random
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[dynamic_algo][random].Add(DynamicBasedPrediction(TargetPolicy.Random)))));
            //Dynamic-Priority
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[dynamic_algo][priority].Add(DynamicBasedPrediction(TargetPolicy.Priority)))));
            //Dynamic-Resource
            for (int times = 0; times < repeat_test; times++)
                threads.Add(new Thread(new ThreadStart(() => algorithm_results[dynamic_algo][resource].Add(DynamicBasedPrediction(TargetPolicy.Resource)))));

            //Start All threads
            for (int thread_to_start = 0; thread_to_start < threads.Count; thread_to_start++)
                threads[thread_to_start].Start();

            //Start the REngine for Jaccard

            //Get the results and print it out
        }
    }

    public enum TargetPolicy
    {
        Random = 1,
        Priority = 2,
        Resource = 3
    }
}
