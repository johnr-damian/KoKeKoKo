using ModelService.Micromanagement;
using ModelService.Micromanagement.Types;
using RDotNet;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
                    var raw_battles = new List<Micromanagement<CSVUnits, CSVUnit>>();
                    //Read the Repository
                    var army_repository = ModelRepositoryService.ReadRepository(@"Test\ArmyTraining.csv");
                    var resource_repository = ModelRepositoryService.ReadRepository(@"Test\ResourcesRepository.csv");
                    //Relate the ResourceRepository to ArmyRepository for Buffs
                    var combined_repository = new Dictionary<string, Tuple<string, string, string, string>>();
                    //foreach(var kv in army_repository)
                    //{
                    //    Console.WriteLine($@"{kv.Key}: ");
                    //    Console.WriteLine($@"{kv.Value.Item2}");
                    //    Console.WriteLine("END: ");
                    //    Console.WriteLine($@"{kv.Value.Item3}");
                    //    Console.ReadLine();
                    //}
                    foreach (var raw_units in army_repository)
                    {
                        //var owned_units = raw_units.Value.Item2.Split('\n').GroupBy(line => line.Split(',')[1]).ToList();
                        //var owned_units = (from line in raw_units.Value.Item2.Split('\n') group line by line.Split(',')[1] into army select from armed in army select armed).ToList();
                        var owned_units = (from repository_line in raw_units.Value.Item2.Split('\n')
                                          group repository_line by repository_line.Split(',')[1] into armies
                                          select armies).ToList();
                        foreach (var k in owned_units.SelectMany(x => x))
                            Console.WriteLine(k);
                        //Task.Run(raw_battles.Add(new Micromanagement<CSVUnits, CSVUnit>(new CSVUnits(), new CSVUnits())));
                    }
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