using ModelService.Micromanagement;
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
using ModelService.Types;

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
            //var agent = new Participant();

            try
            {
                if(args.Length > 0)
                {
                    Console.WriteLine("ModelService has started in standalone mode!");
                    Console.WriteLine("Getting Micromanagement accuracy report...");

                    var micro = modelrepositoryservice.ReadArmyRepository();

                    var battles = new List<Micromanagement.Micromanagement>();
                    var battles_result = new List<List<double>>();
                    var threads = new List<Thread>();
                    foreach (var m in micro)
                        battles.Add(new Micromanagement.Micromanagement(new Army(m.Item3), new Army(m.Item4), new Army(m.Item5)));

#if DEBUG
                    var result = battles[3].GetMicromanagementAccuracyReport(10);
                    Console.WriteLine($@"Lanchester - Random: {result[0] * 100}%");
                    Console.WriteLine($@"Lanchester - Priority: {result[1] * 100}%");
                    Console.WriteLine($@"Lanchester - Resource: {result[2] * 100}%");
                    Console.WriteLine($@"Static - Random: {result[3] * 100}%");
                    Console.WriteLine($@"Static - Priority: {result[4] * 100}%");
                    Console.WriteLine($@"Static - Resource: {result[5] * 100}%");
                    Console.WriteLine($@"Dynamic - Random: {result[6] * 100}%");
                    Console.WriteLine($@"Dynamic - Priority: {result[7] * 100}%");
                    Console.WriteLine($@"Dynamic - Resource: {result[8] * 100}%");
#else

#endif
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
                                Console.WriteLine(rawmessage);
                                Console.WriteLine("Message has been received!\n\n\n\n");
                                var partitionedmessage = rawmessage.Split('~');

                                //switch (partitionedmessage[0])
                                //{
                                //    case "Initialize":

                                //        break;
                                //    case "Generate":

                                //        break;
                                //    case "Exit":
                                //        keeprunningmodel = false;
                                //        break;
                                //    default:
                                //        Console.WriteLine($@"Unable to partition message {rawmessage}! Resulting partitioned message: ");
                                //        foreach (var message in partitionedmessage)
                                //            Console.WriteLine($@"\t{message}");
                                //        break;
                                //}

                                Console.WriteLine(partitionedmessage.Length);
                                if(partitionedmessage.Length > 0)
                                {
                                    foreach (var s in partitionedmessage)
                                        Console.WriteLine(s);
                                }

                                modelrepositoryservice.SendMessageToAgent("Hello World!");
                            }

                            Console.WriteLine("Program.cs -> Model Service is running!");

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