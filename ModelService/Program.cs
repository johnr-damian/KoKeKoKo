using Services;
using System;
using System.Linq;

namespace ModelService
{
    /// <summary>
    /// Manages the different services. In Standalone mode, the C# Model test
    /// the accuracy and precision of the model and returns a compiled results of these
    /// test of Micromanagement and Macromanagement. On the other hand, in Service mode,
    /// the C# Model generates a sequence of actions that must be executed by C++ Agent
    /// in the environment with chosen Micromanagement and Macromangement algorithm.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main process of C# Model.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            try
            {
                //The C# Model has started as a standalone application
                if(args.Length  > 0)
                {
                    var test = RepositoryService.CreateNewRepositoryService();
                    var result = test.GetMicromanagementRepository().ToArray();
                    var results = test.GetMacromanagementRepository().ToArray();
                    var resultss = test.InterpretMacromanagementRepository();

                    using(var stest = new System.IO.StreamWriter(@"Test.txt"))
                    {
                        Console.SetOut(stest);

                        Console.WriteLine("Probability Matrix:");
                        //foreach(var xaxis in resultss)
                        //{
                        //    Console.Write(xaxis.Key.PadRight(5));
                        //    foreach (var yaxis in xaxis.Value)
                        //        Console.Write($@" {yaxis.Key}: {yaxis.Value[0]},{yaxis.Value[1]},{yaxis.Value[2]}".PadRight(5));
                        //    Console.WriteLine();
                        //}
                        Console.Write(" ".PadLeft(5));
                        foreach (var command in resultss.Item1)
                            Console.Write(command.PadLeft(5) + ",");
                        Console.WriteLine();
                        for(int xaxis = 0, xlength = resultss.Item2.GetLength(0); xaxis < xlength; xaxis++)
                        {
                            double sum = 0;
                            for (int yaxis = 0, ylength = resultss.Item2.GetLength(1); yaxis < ylength; yaxis++)
                                sum += resultss.Item2[xaxis, yaxis][0];

                            Console.Write(" ".PadLeft(5));
                            for (int yaxis = 0, ylength = resultss.Item2.GetLength(1); yaxis < ylength; yaxis++)
                                Console.Write($@"[{Math.Round(resultss.Item2[xaxis, yaxis][0] / sum, 2) * 100}% | {(resultss.Item2[xaxis, yaxis][1] / resultss.Item2[xaxis, yaxis][0])}]".PadLeft(5) + ",");
                            Console.WriteLine();
                        }

                        Console.WriteLine();
                        Console.WriteLine("Parsed Macromanagement Result:");
                        foreach (var xaxis in results)
                            Console.WriteLine($@"{xaxis.Item1},{xaxis.Item2},{xaxis.Item3.Length},{xaxis.Item4.Length}");

                        Console.WriteLine();
                        Console.WriteLine("Parsed Micromanagement Result:");
                        foreach (var xaxis in result)
                            Console.WriteLine($@"{xaxis.Item1},{xaxis.Item2},{xaxis.Item3.Length},{xaxis.Item4.Length},{xaxis.Item5.Length}");

                        stest.Close();
                    }
                }
                //The C# Model has started as a service
                else
                {
                    Console.WriteLine("(C#)C# Model has started! Generating initial actions...");

                    //Create the services
                    AgentService agentservice = AgentService.CreateNewAgentService();
                    ComputationService computationservice = ComputationService.CreateNewComputationService();
                    RepositoryService repositoryservice = RepositoryService.CreateNewRepositoryService();

                    //Generate the initial actions
                    var test = new Macromanagement.Macromanagement("a:b", "c:d");
                    var initialactions = String.Join(",", test.GetMacromanagementStuff());

                    //Send the initial action
                    var whattodo = agentservice.UpdateAgentService(initialactions);


                    for (string message = whattodo.Dequeue(); message != "TERMINATE";)
                    {
                        //Generate new actions
                        var newactions = String.Join(",", test.GetMacromanagementStuff());

                        if (!agentservice.ShouldOperationsContinue())
                        {
                            var nextwhattodo = agentservice.UpdateAgentService(newactions);
                            message = nextwhattodo.Dequeue();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"(C#)Error Occurred! {ex.Message}");
            }

            return 0;
        }
    }
}