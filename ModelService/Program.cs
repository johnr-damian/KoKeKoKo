using ModelService.Micromanagement;
using ModelService.Micromanagement.Types;
using ModelService.Macromanagement;
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
                    Console.WriteLine("Getting Micromanagement accuracy report...");

                    var battles = new List<Micromanagement<CSVbasedUnit>>();
                    var battles_result = new List<List<double>>();
                    var army_repository = ModelRepositoryService.ReadRepository(@"Test\ArmyTraining.csv");
                    var resource_repository = ModelRepositoryService.ReadRepository(@"Test\ResourcesRepository.csv");
                    var threads = new List<Thread>();

                    //TODO relate resource to army, improve modelrepository
                    foreach(var battle in army_repository)
                    {
                        var armies = battle.Value.Item2.Split('\n').GroupBy(army => army.Split(',')[2]).ToDictionary(key => key.Key, value => value.ToList());
                        var parsed_armies = (from army in armies select (new CSVbasedArmy(String.Join("\n", army.Value)))).ToList();

                        if (parsed_armies.Count == 2)
                            battles.Add(new Micromanagement<CSVbasedUnit>(parsed_armies[0], parsed_armies[1]));
                        else
                            throw new InvalidOperationException("There is no armies to simulate battle...");
                    }

                    //Start performing simulation
                    foreach (var battle in battles)
                        threads.Add(new Thread(new ThreadStart(() => battles_result.Add(battle.GetMicromanagementAccuracy(10)))));
                    threads.ForEach(thread => thread.Start());

                    Console.WriteLine("Getting Macromanagement accuracy report...");
                    var command_repository = ModelRepositoryService.ReadRepository(@"Test\CommandsRepository.csv");
                    //TODO relate resource to command, improve all
                    var matches = new List<Macromanagement.Macromanagement>();
                    //macthes_result
                    //TODO mirror micro
                    threads.ForEach(thread => thread.Join());
                    //Add macro threads

                    Console.WriteLine("Micromanagement Result: ");
                    Console.WriteLine("Macromanagement Result: ");
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