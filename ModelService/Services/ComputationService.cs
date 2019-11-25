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

        public IEnumerable<double> ComputeEuclideanMetric(string origin)
    }
}
