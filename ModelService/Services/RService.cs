using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    /// <summary>
    /// This file contains related fields and methods about <see cref="REngine"/>
    /// </summary>
    public partial class ModelRepositoryService
    {
        private static REngine _engine;

        /// <summary>
        /// Returns the instance of the <see cref="REngine"/>
        /// </summary>
        /// <returns></returns>
        public static REngine GetREngine()
        {
            try
            {
                if(_engine == null)
                {
                    //Ready the environment variables
                    REngine.SetEnvironmentVariables();

                    //Get the instance of the engine, and initialize it
                    _engine = REngine.GetInstance();
                    _engine.Initialize();

                    //Prepare the packages
                    //Sets Package
                    _engine.Evaluate(@"if(""sets"" %in% rownames(installed.packages()) == FALSE) { install.packages(""sets"") }");
                    _engine.Evaluate(@"if(""pomdp"" %in% rownames(installed.packages()) == FALSE) { install.packages(""pomdp"") }");
                    _engine.Evaluate(@"if(""triangle"" %in% rownames(installed.packages()) == FALSE) { install.packages(""triangle"") }");
                    _engine.Evaluate(@"library(""sets"")");
                    _engine.Evaluate(@"library(""pomdp"")");
                    _engine.Evaluate(@"library(""triangle"")");
                    //POMDP Package
                    //... TODO
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GetREngine() -> {ex.Message}...");
                _engine = null;
            }

            return _engine;
        }
    }

    public static class REngineExtensions
    {
        private static Random randomGenerator = default(Random);

        /// <summary>
        /// Returns the average of results of jaccard
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="data_source"></param>
        /// <param name="data_result"></param>
        /// <returns></returns>
        public static double GetJaccardResult(this REngine engine, List<string> data_source, string data_result)
        {
            var results = new List<double>();

            foreach(var source in data_source)
            {
                engine.Evaluate($@"simulation <- set({source})");
                engine.Evaluate($@"actual <- set({data_result})");
                results.Add(engine.Evaluate(@"set_similarity(simulation, actual, method=""Jaccard"")").AsNumeric().Single());
            }

            return engine.GetStandardDeviation(results);
        }

        public static double POMDPSimulate(this REngine engine, params string[] parameters)
        {
            double result = 0;



            return result;
        }

        public static double MCTSSimulate(this REngine engine, params string[] parameters)
        {
            double result = 0;

            return result;
        }

        public static double GetStandardDeviation(this REngine engine, List<double> results)
        {
            var mean = results.Average();

            return results.Average(element => Math.Pow(element - mean, 2));
        }

        public static Random GetRandomGenerator()
        {
            if (randomGenerator == null)
                randomGenerator = new Random();

            return randomGenerator;
        }

        public static double GetTriangularRandomNumber(this REngine engine, int count, double min, double max, double mode) => engine.Evaluate($@"rtriangle({count}, {min}, {max}, {mode})").AsNumeric().Sum();
    }
}
