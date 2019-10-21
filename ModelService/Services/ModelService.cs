using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Services
{
    public partial class ModelRepositoryService
    {
        /// <summary>
        /// This class holds operations for Math-related computations, and
        /// R-related computations and visualizations.
        /// </summary>
        public class ModelService
        {
            #region Properties
            /// <summary>
            /// An instance of this class. It is the access to various functions for
            /// computations and visualizations of results
            /// </summary>
            private static ModelService Instance { get; set; } = default(ModelService);

            /// <summary>
            /// An instance of the <see cref="Random"/> class. This is to prevent 
            /// multiple simultaneous creation of <see cref="Random"/> that gives the
            /// same generated number because of the same seed, especially in threading.
            /// </summary>
            public Random RandomEngine { get; private set; } = default(Random);

            /// <summary>
            /// An instance of the <see cref="RDotNet.REngine"/> class. This is used for
            /// computing several necessities in <see cref="Macromanagement.Macromanagement"/>, and
            /// in <see cref="Micromanagement.Micromanagement"/>. It is also used for visualizations
            /// of generated results.
            /// </summary>
            public REngine REngine { get; private set; } = default(REngine); 
            #endregion

            /// <summary>
            /// Initializes the <see cref="RandomEngine"/> and <see cref="REngine"/>
            /// </summary>
            private ModelService()
            {
                try
                {
                    //Initialize the random engine
                    RandomEngine = new Random();

                    //Initialize the r engine
                    REngine.SetEnvironmentVariables();
                    REngine = REngine.GetInstance();
                    REngine.Initialize();

                    //Install the required packages
                    REngine.Evaluate(@"if(""sets"" %in% rownames(installed.packages()) == FALSE) { install.packages(""sets"") }");
                    REngine.Evaluate(@"if(""triangle"" %in% rownames(installed.packages()) == FALSE) { install.packages(""triangle"") }");
                    REngine.Evaluate(@"if(""pomdp"" %in% rownames(installed.packages()) == FALSE) { install.packages(""pomdp"") }");
                    //Prepare the required packages
                    REngine.Evaluate(@"library(""sets"")");         //For Jaccard Measurement
                    REngine.Evaluate(@"library(""triangle"")");     //For Triangular Probability Distribution
                    REngine.Evaluate(@"library(""pomdp"")");        //For POMDP Computation
                }
                catch(EvaluationException ex)
                {
                    Console.WriteLine($@"ModelService() -> {ex.Message}");
                    throw new Exception("An error occurred! Failed to start model service...");
                }
            }

            /// <summary>
            /// Returns the instance of this class with initialized <see cref="Random"/>
            /// and <see cref="RDotNet.REngine"/>
            /// </summary>
            /// <returns></returns>
            public static ModelService GetModelService()
            {
                if (Instance == null)
                    Instance = new ModelService();

                return Instance;
            }

            /// <summary>
            /// Returns the jaccard similarity of each result in <paramref name="results"/> using
            /// the <paramref name="origin"/> as a reference set of result. 
            /// </summary>
            /// <remarks>
            /// <para>
            ///     For <see cref="Micromanagement.Micromanagement"/> this assumes that the <paramref name="origin"/> and
            ///     the <paramref name="results"/> are the units that survived at the end of a battle. It expects
            ///     to recieve the <see cref="Types.Unit.UniqueID"/> as the input in both parameters.
            /// </para>
            /// </remarks>
            /// <param name="origin"></param>
            /// <param name="results"></param>
            /// <returns></returns>
            public IEnumerable<double> GetJaccardSimilarity(string origin, IEnumerable<string> results)
            {
                foreach(var result in results)
                {
                    double similarity = Double.NaN;
                    try
                    {
                        REngine.Evaluate($@"origin <- set({origin})");
                        REngine.Evaluate($@"result <- set({result})");
                        similarity = REngine.Evaluate(@"set_similarity(origin, result, method=""Jaccard"")").AsNumeric().Single();
                    }
                    catch(EvaluationException ex)
                    {
                        Console.WriteLine($@"GetJaccardSimilarity() -> {ex.Message}");
                        throw new FormatException("An error occurred! There is an invalid data format to get the jaccard similarity...");
                    }

                    yield return similarity;
                }
            }

            /// <summary>
            /// Returns the euclidean metric of each continuous result in <paramref name="results"/> using
            /// the <paramref name="origin"/> as a reference sequence of result.
            /// </summary>
            /// <remarks>
            /// <para>
            ///     For <see cref="Macromanagement.Macromanagement"/> this assumes that the <paramref name="origin"/> and
            ///     the <paramref name="results"/> are the <see cref="ValueTypes.CostWorth.GetTotalWorth()"/> of a specific player's 
            ///     perspective of a battle. It expects to receive the <see cref="ValueTypes.CostWorth.GetTotalWorth()"/> joined by
            ///     a comma.
            /// </para>
            /// </remarks>
            /// <param name="origin"></param>
            /// <param name="results"></param>
            /// <returns></returns>
            public IEnumerable<double> GetEuclideanMetric(string origin, IEnumerable<string> results)
            {
                foreach(var result in results)
                {
                    double metric = Double.NaN;
                    try
                    {
                        REngine.Evaluate($@"origin <- c({origin})");
                        REngine.Evaluate($@"result <- c({result})");
                        metric = REngine.Evaluate(@"dist(rbind(origin, result))").AsNumeric().Single();
                    }
                    catch(EvaluationException ex)
                    {
                        Console.WriteLine($@"GetEuclideanMetric() -> {ex.Message}");
                        throw new FormatException("An error occurred! There is an invalid data format to get the euclidean metric...");
                    }

                    yield return metric;
                }
            }

            /// <summary>
            /// Returns a random number using triangular distribution
            /// </summary>
            /// <param name="count"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="mode"></param>
            /// <returns></returns>
            public IEnumerable<double> GetTriangularRandom(int count, double min, double max, double mode)
            {
                var randoms = REngine.Evaluate($@"rtriangle({count}, {min}, {max}, {mode})").AsNumeric();

                foreach (var random in randoms)
                    yield return random;
            }

            /// <summary>
            /// Returns the sample variance of results
            /// </summary>
            /// <param name="results"></param>
            /// <returns></returns>
            public double GetVariance(IEnumerable<double> results)
            {
                double mean = results.Average();
                double squareddifferences = results.Sum(result => Math.Pow((result - mean), 2));

                return (squareddifferences / (results.Count() - 1));
            }

            /// <summary>
            /// Returns the sample standard deviation of results
            /// </summary>
            /// <param name="results"></param>
            /// <returns></returns>
            public double GetStandardDeviation(IEnumerable<double> results) => Math.Sqrt(GetVariance(results));

            /// <summary>
            /// Creates a box plot using <see cref="REngine"/> and saves it to your D:// drive.
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="results"></param>
            public void CreateBoxPlot(string filename, params string[] results)
            {
                for(int uid = 0; uid < results.Length; uid++)
                    REngine.Evaluate($@"Algorithm{uid} <- c({results[uid]})");

                //There is a data to be plot
                if(results.Length > 0)
                {
                    string dataframe = "Algorithm0";
                    for (int uid = 1; uid < results.Length; uid++)
                        dataframe += ("," + $"Algorithm{uid}");

                    REngine.Evaluate($@"boxplotdata <- data.frame({dataframe})");
#if DEBUG
                    REngine.Evaluate($@"png('D:\\Training{filename}.png')");
#elif TRACE
                    REngine.Evaluate($@"png('D:\\Testing{filename}.png')");
#endif
                    REngine.Evaluate($@"boxplot(boxplotdata, main=""Battle Prediction Accuracy"", xlab=""Prediction Algorithms"", ylab=""Jaccard Similarity"", col=""lightblue"", border=""blue"", notch=TRUE, range=0, horizontal=TRUE)");
                    REngine.Evaluate($@"dev.off()");
                }

            }

            public void CreateLinePlot(string filename, params string[] results)
            {
                for (int uid = 0; uid < results.Length; uid++)
                    REngine.Evaluate($@"Algorithm{uid} <- c({results[uid]})");

                
            }
        }
    }
}
