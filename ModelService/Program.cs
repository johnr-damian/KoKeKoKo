using ModelService.Micromanagement;
using ModelService.Micromanagement.Types;
using RDotNet;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;


namespace ModelService
{
    /// <summary>
    /// The main thread of the model
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            var modelrepositoryservice = new ModelRepositoryService();
            var agent = new Participant();

            try
            {
                if(args.Length > 0)
                {
                    
                    Console.WriteLine("ModelService has started in standalone mode!");
                    Console.WriteLine("Performing Micromanagement Prediction...");

                    //Perform Micromanagement Testing
                    //Read stuff
                    var micromanagement = new Micromanagement<CSVUnits, CSVUnit>(null, null);
                    var lanchester_random = micromanagement.LanchesterBasedPrediction(TargetPolicy.Random);
                    var lanchester_priority = micromanagement.LanchesterBasedPrediction(TargetPolicy.Priority);
                    var lanchester_resource = micromanagement.LanchesterBasedPrediction(TargetPolicy.Resource);

                    Console.WriteLine(micromanagement.GetSummaryOfResults(lanchester_random.Item1, lanchester_random.Item2));
                    Console.WriteLine(micromanagement.GetSummaryOfResults(lanchester_priority.Item1, lanchester_priority.Item2));
                    Console.WriteLine(micromanagement.GetSummaryOfResults(lanchester_resource.Item1, lanchester_resource.Item2));

                    Console.WriteLine("Performing Macromanagement Prediction...");
                    Macromanagement.Macromanagement.PerformMacromanagementTest();

                    Console.WriteLine("Successfully performed Micromanagement testing and Macromanagement testing!");
                    Console.WriteLine("Press enter to exit...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("ModelService has started! Creating a server for agent...");
                    if(modelrepositoryservice.CreateServerForAgent())
                    {
                        Console.WriteLine("Successfully created a server for agent...");
                        modelrepositoryservice.StartListeningToAgent();

                        //Main loop that repeats to listen then responds to the requests
                        for (bool keeprunningmodel = true; keeprunningmodel;)
                        {
                            //If there is a request from agent
                            if (modelrepositoryservice.HasMessageFromAgent())
                            {
                                var rawmessage = modelrepositoryservice.GetMessageFromQueue();
                                var partitionedmessage = rawmessage.Split('\n');

                                switch (partitionedmessage[0])
                                {
                                    case "Initialize":

                                        break;
                                    case "Generate":

                                        break;
                                    case "Exit":
                                        keeprunningmodel = false;
                                        break;
                                    default:
                                        Console.WriteLine($@"Unable to partition message {rawmessage}! Resulting partitioned message: ");
                                        foreach (var message in partitionedmessage)
                                            Console.WriteLine($@"\t{message}");
                                        break;
                                }
                            }

                            //Give other thread to process their procedures
                            Thread.Sleep(1000);
                        }

                        //Stop the server and remove it
                        Console.WriteLine("Removing server for agent...");
                        modelrepositoryservice.StopListeningToAgent();
                        modelrepositoryservice.RemoveServerForAgent();
                    }

                    Console.WriteLine("Stopping ModelService...");
                }
            }
            catch
            {
                Console.WriteLine("Error Occurred! Failed to catch an application error...");
                Trace.WriteLine("Error in Main()! An unhandled exception occurred...");
            }

            return 0;
        }
    }
}