using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Facilitates the communication to <see cref="Random"/> and <see cref="REngine"/>.
    /// In addition, it provides methods for computations and visualizations.
    /// </summary>
    public class ComputationService
    {
        #region Properties
        /// <summary>
        /// Instance of the current <see cref="ComputationService"/>. This provides access
        /// to methods for communicating with Random Service and REngine Service, computations,
        /// and visualizations of results.
        /// </summary>
        private static ComputationService Instance { get; set; } = default(ComputationService);

        /// <summary>
        /// Instance of the current <see cref="Random"/>. This provides access to built-in methods
        /// for generating random numbers. Also, this is to prevent multiple simultaneous creation
        /// of <see cref="Random"/> which would generate same random number across threads.
        /// </summary>
        private Random RandomService { get; set; } = default(Random);

        /// <summary>
        /// Instance of the current <see cref="REngine"/>. This provices access to built-in methods
        /// for communicating with R Software and methods for computations and visualizations.
        /// </summary>
        public REngine RService { get; private set; } = default(REngine); 
        #endregion

        /// <summary>
        /// Initializes the required properties to handle communicating with Random Service
        /// and REngine Service, computations, and visualizations.
        /// </summary>
        private ComputationService()
        {
            RandomService = new Random();
            REngine.SetEnvironmentVariables();
            RService = REngine.GetInstance();
            RService.Initialize();

            //Install the required packages
            RService.Evaluate(@"if(""sets"" %in% rownames(installed.packages()) == FALSE) { install.packages(""sets"") }");
            RService.Evaluate(@"if(""triangle"" %in% rownames(installed.packages()) == FALSE) { install.packages(""triangle"") }");
            RService.Evaluate(@"if(""pomdp"" %in% rownames(installed.packages()) == FALSE) { install.packages(""pomdp"") }");
            //Prepare the required packages
            RService.Evaluate(@"library(""sets"")"); //For Jaccard Measurement
            RService.Evaluate(@"library(""triangle"")"); //For Triangular Probability Distribution
            RService.Evaluate(@"library(""pomdp"")"); //For POMDP Computation
        }

        /// <summary>
        /// Creates an instance of <see cref="ComputationService"/> and returns it initialized.
        /// </summary>
        /// <returns></returns>
        public static ComputationService CreateNewComputationService()
        {
            if (Instance == null)
                Instance = new ComputationService();

            return Instance;
        }

        #region Random Wrapper Methods
        /// <summary>
        /// Generates a non-negative random integer.
        /// </summary>
        /// <returns></returns>
        public int GetRandomInteger() => RandomService.Next();

        /// <summary>
        /// Generates a non-negative random integer less than <paramref name="max"/>.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public int GetRandomInteger(int max) => RandomService.Next(max);

        /// <summary>
        /// Generates a random integer starting from inclusive <paramref name="min"/>
        /// and less than <paramref name="max"/>.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int GetRandomInteger(int min, int max) => RandomService.Next(min, max);

        /// <summary>
        /// Generates a random double from 0 and less than 1.
        /// </summary>
        /// <returns></returns>
        public double GetRandomProbability() => RandomService.NextDouble();

        /// <summary>
        /// Generates a random double using a triangular distribution.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public double GetRandomProbability(double min, double max, double mode) => RService.Evaluate($@"rtriangle(1, {min}, {max}, {mode})").AsNumeric().SingleOrDefault();

        /// <summary>
        /// Generates a sequence of random doubles using a triangular distribution.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public IEnumerable<double> GetRandomProbability(int count, double min, double max, double mode)
        {
            var triangular_randoms = RService.Evaluate($@"rtriangle({count}, {min}, {max}, {mode})").AsNumeric();

            foreach (var triangular_random in triangular_randoms)
                yield return triangular_random;
        } 

        /// <summary>
        /// Shuffles the list of elements using Fished yacht something algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <returns></returns>
        public IEnumerable<T> GetRandomElement<T>(IEnumerable<T> elements)
        {
            var shuffled_elements = elements.ToArray();
            for(int shuffler = 0; shuffler < shuffled_elements.Length; shuffler++)
            {
                int chosen_index = RandomService.Next(0, (shuffled_elements.Length - shuffler));
                var shuffled_element = shuffled_elements[chosen_index];

                shuffled_elements[chosen_index] = shuffled_elements[shuffler];
                shuffled_elements[shuffler] = shuffled_element;
            }

            return shuffled_elements;
        }
        #endregion

        /// <summary>
        /// Computes and returns the sample variance of the results.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public double ComputeVariance(IEnumerable<double> results)
        {
            double mean = results.Average();
            double squareddifference = results.Sum(result => Math.Pow((result - mean), 2));

            return (squareddifference / (results.Count() - 1));
        }

        /// <summary>
        /// Computes and returns the sample standard deviation of the results.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public double ComputeStandardDeviation(IEnumerable<double> results) => Math.Sqrt(ComputeVariance(results));

        private IEnumerable<string> CreateLinePlot(string rank, IEnumerable<string> macromanagement)
        {
            var macromanagement_arr = macromanagement.ToArray();
            var m_plot = new List<string>();
            var count = new List<int>();
            for (int uid = 0; uid < macromanagement_arr.Length; uid++)
            {
                var simulation = macromanagement_arr[0].Split('$')[1].Split('\n').Select(time => new Tuple<string[], string[]>(time.Split(';')[0].Split(','), time.Split(';')[1].Split(','))).ToArray();
                var result_simulation = String.Join(",", simulation.Select(time => time.Item1.Last()));
                RService.Evaluate($@"Result{uid} <- c({result_simulation})");
                m_plot.Add($@"Result{uid}");
                count.Add(result_simulation.Length);
            }
#if DEBUG
            RService.Evaluate($@"png('Training{rank}.png')");
#else
            RService.Evaluate($@"png('Testing{rank}.png')");
#endif
            RService.Evaluate($@"plot({macromanagement_arr[0]}, type=""o"", col=""blue"")");
            RService.Evaluate($@"title(main=""Constructed Workers"")");
            RService.Evaluate($@"axis(side=1, at=seq(0, {count.Max()}, by=10))");
            for (int it = 1; it < macromanagement_arr.Length; it++)
                RService.Evaluate($@"lines({macromanagement_arr[it]}, type=""o"", col=""blue"")");

            RService.Evaluate("box()");
            RService.Evaluate("dev.off()");

            return macromanagement_arr;
        }

        private IEnumerable<double> ComputeEuclideanMetric(string rank, IEnumerable<string> macromanagement_results)
        {
            foreach(var macromanagement_result in macromanagement_results)
            {
                double eucilidean_metric = -1, mineral = 0, vespene = 0, supply = 0, workers = 0;

                try
                {
                    //Get the Euclidean Result
                    var basis = macromanagement_result.Split('$')[0].Split('\n').Select(time => new Tuple<string[], string[]>(time.Split(';')[0].Split(','), time.Split(';')[1].Split(','))).ToArray();
                    var simulation = macromanagement_result.Split('$')[1].Split('\n').Select(time => new Tuple<string[], string[]>(time.Split(';')[0].Split(','), time.Split(';')[1].Split(','))).ToArray();

                    RService.Evaluate($@"mineral_basis <- c({String.Join(",", basis.Select(time => time.Item1[0]))})");
                    RService.Evaluate($@"mineral_simulation <- c({String.Join(",", simulation.Select(time => time.Item1[0]))})");
                    mineral = RService.Evaluate($@"dist(rbind(mineral_basis, vespene_basis))").AsNumeric().SingleOrDefault();

                    RService.Evaluate($@"vespene_basis <- c({String.Join(",", basis.Select(time => time.Item1[1]))})");
                    RService.Evaluate($@"vespene_simulation <- c({String.Join(",", simulation.Select(time => time.Item1[1]))})");
                    vespene = RService.Evaluate($@"dist(rbind(vespene_basis, vespene_simulation))").AsNumeric().SingleOrDefault();

                    RService.Evaluate($@"supply_basis <- c({String.Join(",", basis.Select(time => time.Item1[2]))})");
                    RService.Evaluate($@"supply_simulation <- c({String.Join(",", simulation.Select(time => time.Item1[2]))})");
                    supply = RService.Evaluate($@"dist(rbind(supply_basis, supply_simulation))").AsNumeric().SingleOrDefault();

                    RService.Evaluate($@"workers_basis <- c({String.Join(",", basis.Select(time => time.Item1.Last()))})");
                    RService.Evaluate($@"workers_simulation <- c({String.Join(",", simulation.Select(time => time.Item1.Last()))})");
                    workers = RService.Evaluate($@"dist(rbind(workers_basis, workers_simulation))").AsNumeric().SingleOrDefault();
                }
                catch(EvaluationException ex)
                {
                    System.Diagnostics.Debugger.Break();
                    Console.WriteLine($@"(C#)Error Occurred! {ex.Message}");
                }

                yield return ((mineral + vespene + supply + workers) / 4);
            }
        }

        public IEnumerable<string> ComputeEuclideanMetric(Dictionary<string, IEnumerable<string>> macromanagement_compiledresults)
        {
            var perrank_plot = new Dictionary<string, IEnumerable<string>>();
            var perrank_results = new Dictionary<string, IEnumerable<double>>();

            foreach(var rank in macromanagement_compiledresults)
            {
                //Get the results per rank
                var value_result = rank.Value.ToArray();
                perrank_results.Add(rank.Key, ComputeEuclideanMetric(rank.Key, value_result));
                perrank_plot.Add(rank.Key, CreateLinePlot(rank.Key, value_result));
            }

            //Return the average with standard deviation
            foreach(var rank in perrank_results)
            {
                double average = rank.Value.Average();
                double standarddeviation = ComputeStandardDeviation(rank.Value);

                yield return $@"{average}\u00B1{standarddeviation}";
            }
        }
    }
}
