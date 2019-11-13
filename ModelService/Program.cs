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
            //var modelrepositoryservice = new ModelRepositoryService();
            //var agent = new Participant();

            //var testing = Services.Services.CreateNewService();
            //testing.StartAllServices();
            //testing.StopAllServices();

            string message = "";

            while(message != "exit")
            {
                using (var client = new NamedPipeClientStream("AgentServer"))
                {
                    

                    client.Connect();
                    

                    if (client.IsConnected)
                    {
                        Console.Write("Enter Message(C#): ");
                        message = Console.ReadLine();

                        var writer = new StreamWriter(client);
                        writer.AutoFlush = true;
                        writer.WriteLine(message);
                        client.WaitForPipeDrain();

                        var reader = new StreamReader(client);
                        message = reader.ReadLine();

                        Console.WriteLine($@"Recieved Message(C#): {message}");
                        client.Close();
                    }
                }
            }

            
            return 0;
            //try
            //{
            //    if(args.Length > 0)
            //    {
            //        Console.WriteLine("ModelService has started in standalone mode!");
            //        Console.WriteLine("Getting Micromanagement accuracy report...");
            //        Console.WriteLine("Test run by C++");
            //        return 0;

            //        //Variables to store the input
            //        var micromanagement = ModelRepositoryService.ReadArmiesRepository();
            //        var macromanagement_resources = ModelRepositoryService.ReadResourcesRepository();
            //        var macromanagement_commands = ModelRepositoryService.ReadCommandsRepository();

            //        //Variables to store the output
            //        var micromanagement_battles = new List<Micromanagement.Micromanagement>();
            //        var relationedmicromacro = ModelRepositoryService.RelateMicroToMacro(macromanagement_resources, micromanagement);
            //        foreach (var micromanagement_battle in relationedmicromacro)
            //            micromanagement_battles.Add(new Micromanagement.Micromanagement(new Army(micromanagement_battle.Item3), new Army(micromanagement_battle.Item4), new Army(micromanagement_battle.Item5))
            //            {
            //                Rank = micromanagement_battle.Item1,
            //                Filename = micromanagement_battle.Item2
            //            });
            //        var macromanagement_battles = new List<Macromanagement.Macromanagement>();
            //        var relationedmacromacro = ModelRepositoryService.RelateMacroToMacro(macromanagement_resources, macromanagement_commands);
            //        var relationedmicromacromacro = ModelRepositoryService.RelateMicroToMacroMacro(relationedmicromacro, relationedmacromacro);
            //        foreach (var macromanagement_battle in relationedmicromacromacro)
            //            macromanagement_battles.Add(new Macromanagement.Macromanagement(macromanagement_battle.Item1, macromanagement_battle.Item2, macromanagement_battle.Item3, macromanagement_battle.Item4));

            //        //Group the micromanagement battles by their rank
            //        var perrank_micromanagement = micromanagement_battles.GroupBy(rank => rank.Rank).ToDictionary(key => key.Key, value => value.ToList());
            //        //For every micromanagement battle per rank, do the prediction and store it
            //        var perrankresult_micromanagement = perrank_micromanagement.ToDictionary(key => key.Key, value =>
            //        {
            //            var micromanagement_battleresults = new List<IEnumerable<IEnumerable<double>>>();

            //            foreach (var micromanagement_battleresult in value.Value)
            //                micromanagement_battleresults.Add(micromanagement_battleresult.GetMicromanagementAccuracyReport(1));

            //            return micromanagement_battleresults;
            //        });
            //        //Get the final result per algorithm+policy per rank
            //        var micromanagement_accuracyreports = perrankresult_micromanagement.ToDictionary(key => key.Key, value => Micromanagement.Micromanagement.GetMicromanagementAccuracyReport(value.Key, value.Value));
            //        //Print the results per rank
            //        foreach (var accuracy_report in micromanagement_accuracyreports)
            //        {
            //            Console.WriteLine($@"Lanchester-Random: {accuracy_report.Value[0] * 100}%");
            //            Console.WriteLine($@"Lanchester-Priority: {accuracy_report.Value[1] * 100}%");
            //            Console.WriteLine($@"Lanchester-Resource: {accuracy_report.Value[2] * 100}%");
            //            Console.WriteLine($@"Static-Random: {accuracy_report.Value[3] * 100}%");
            //            Console.WriteLine($@"Static-Priority: {accuracy_report.Value[4] * 100}%");
            //            Console.WriteLine($@"Static-Resource: {accuracy_report.Value[5] * 100}%");
            //            Console.WriteLine($@"Dynamic-Random: {accuracy_report.Value[6] * 100}%");
            //            Console.WriteLine($@"Dynamic-Priority: {accuracy_report.Value[7] * 100}%");
            //            Console.WriteLine($@"Dynamic-Resource: {accuracy_report.Value[8] * 100}%");
            //            Console.WriteLine();
            //        }
            //        //Group the macromanagement battles by their rank
            //        var perrank_macromanagement = macromanagement_battles.GroupBy(rank => rank.Rank).ToDictionary(key => key.Key, value => value.ToList());
            //        //For every macromanagement battle per rank, do the prediction and store it
            //        var perrankresult_macromanagement = perrank_macromanagement.ToDictionary(key => key.Key, value =>
            //        {
            //            var macromanagement_battleresults = new List<IEnumerable<IEnumerable<double>>>();

            //            foreach (var macromanagement_battleresult in value.Value)
            //                macromanagement_battleresults.Add(macromanagement_battleresult.GetMacromanagementAccuracyReport(1, Macromanagement.Macromanagement.AIAlgorithm.MCTS));

            //            return macromanagement_battleresults;
            //        });
            //        //Get the final result per algorithm
            //        var macromanagement_accuracyreports = perrankresult_macromanagement.ToDictionary(key => key.Key, value => Macromanagement.Macromanagement.GetMacromanagementAccuracyReport(value.Key, value.Value));
            //        //Print the results per rank
            //        foreach(var accuracy_report in macromanagement_accuracyreports)
            //        {
            //            Console.WriteLine($@"Rank: {accuracy_report.Key}");
            //            Console.WriteLine($@"MCTS Euclidean Mean: {accuracy_report.Value[0]}");
            //        }


            //        Console.WriteLine("Finished performing accuracy reports! Please enter to continue...");
            //        Console.ReadLine();
            //    }
            //    else
            //    {
            //        int mode = 0;
            //        Console.WriteLine("ModelService has started! Creating a server for agent...");
            //        if(modelrepositoryservice.CreateServerForAgent())
            //        {
            //            Console.WriteLine("Successfully created a server for agent...");
            //            modelrepositoryservice.StartListeningToAgent();

            //            //Main loop that repeats to listen then responds to the requests
            //            for (bool keeprunningmodel = true; keeprunningmodel;)
            //            {
            //                //If there is a request from agent
            //                if (modelrepositoryservice.HasMessageFromAgent())
            //                {
            //                    var rawmessage = modelrepositoryservice.GetMessageFromQueue();
            //                    Console.WriteLine(rawmessage);
            //                    Console.WriteLine("Message has been received!\n\n\n\n");
            //                    var partitionedmessage = rawmessage.Split('~');

            //                    //switch (partitionedmessage[0])
            //                    //{
            //                    //    case "Initialize":

            //                    //        break;
            //                    //    case "Generate":

            //                    //        break;
            //                    //    case "Exit":
            //                    //        keeprunningmodel = false;
            //                    //        break;
            //                    //    default:
            //                    //        Console.WriteLine($@"Unable to partition message {rawmessage}! Resulting partitioned message: ");
            //                    //        foreach (var message in partitionedmessage)
            //                    //            Console.WriteLine($@"\t{message}");
            //                    //        break;
            //                    //}
            //                    //var player = default(Macromanagement.Macromanagement);
            //                    //switch(mode)
            //                    //{
            //                    //    case 0:
            //                    //        try
            //                    //        {
            //                    //            player = new Macromanagement.Macromanagement(partitionedmessage[0], partitionedmessage[1]);
            //                    //        }
            //                    //        catch (Exception ex)
            //                    //        {
            //                    //            Console.WriteLine($"An error occured! {ex.Message}");
            //                    //            modelrepositoryservice.SendMessageToAgent("BUILD_SUPPLYDEPOT");
            //                    //        }

            //                    //        try
            //                    //        {
            //                    //            modelrepositoryservice.SendMessageToAgent(String.Join(",", player.GetMacromanagementStuff()));
            //                    //        }
            //                    //        catch (Exception ex)
            //                    //        {
            //                    //            Console.WriteLine($"An error occurred! {ex.Message}");
            //                    //            modelrepositoryservice.SendMessageToAgent("BUILD_SUPPLYDEPOT,BUILD_BARRACKS");
            //                    //        }
            //                    //        mode = 1;
            //                    //        break;
            //                    //    case 1:
            //                    //        try
            //                    //        {
            //                    //            modelrepositoryservice.SendMessageToAgent(String.Join(",", player.GetMacromanagementStuff()));
            //                    //        }
            //                    //        catch (Exception ex)
            //                    //        {
            //                    //            Console.WriteLine($@"An error occurred! {ex.Message}");
            //                    //            modelrepositoryservice.SendMessageToAgent("BUILD_SUPPLYDEPOT,BUILD_BARRACKS,TRAIN_MARINE");
            //                    //        }
            //                    //        break;
            //                    //}

            //                    //Console.WriteLine(partitionedmessage.Length);
            //                    //if(partitionedmessage.Length > 0)
            //                    //{
            //                    //    foreach (var s in partitionedmessage)
            //                    //        Console.WriteLine(s);
            //                    //}

            //                    //modelrepositoryservice.SendMessageToAgent("Hello World!");
            //                    modelrepositoryservice.SendMessageToAgent("BUILD_SUPPLYDEPOT");
            //                }

            //                Console.WriteLine("Program.cs -> Model Service is running!");

            //                //Give other thread to process their procedures
            //                Thread.Sleep(1000);
            //            }

            //            //Stop the server and remove it
            //            Console.WriteLine("Removing server for agent...");
            //            modelrepositoryservice.StopListeningToAgent();
            //            modelrepositoryservice.RemoveServerForAgent();
            //        }

            //        Console.WriteLine("Stopping ModelService...");
            //    }
            //}
            //catch
            //{
            //    Console.WriteLine("Error Occurred! Failed to catch an application error...");
            //    Trace.WriteLine("Error in Main()! An unhandled exception occurred...");
            //}

            //return 0;
        }
    }
}