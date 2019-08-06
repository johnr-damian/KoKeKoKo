using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using RDotNet;

namespace ModelService
{
    /// <summary>
    /// The main thread of the model
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("ModelService has started...");
            ModelRepositoryService modelrepositoryservice = new ModelRepositoryService();
            Participant agent = null;
            bool keeprunningmodel = true;

            try
            {
                //Create a server to listen to agent
                Console.WriteLine("Creating a server for Agent...");
                if(modelrepositoryservice.CreateServerForAgent())
                {
                    //Start the server
                    Console.WriteLine("Successfully created a server for Agent...");
                    modelrepositoryservice.StartListeningToAgent();
                    while(keeprunningmodel)
                    {
                        //If there is a message sent by agent
                        if (modelrepositoryservice.HasMessageFromAgent())
                        {
                            var rawmessage = modelrepositoryservice.GetMessageFromQueue();
                            var partitionedmessage = rawmessage.Split('\n');

                            //The expected messages are in the following format:
                            //Initialize\nUnits of Agent\nStates\nTransition
                            //Update\nUnits of Agent
                            //Generate\n
                            //ReGenerate\n
                            //Exit
                            switch (partitionedmessage[0])
                            {
                                case "Initialize":
                                    if (!agent.SetParticipantUnits(partitionedmessage[1]))
                                        throw new Exception("Failed to initialize the participant's units...");

                                    break;
                                case "Update":

                                    break;
                                case "Generate":

                                    break;
                                case "ReGenerate":

                                    break;
                                case "Exit":
                                    keeprunningmodel = false;
                                    break;
                                default:
                                    Console.WriteLine($@"Unable to parse '{rawmessage}'...");
                                    break;
                            }
                        }

                        //Let the thread give way by 1 seconds to other thread
                        Thread.Sleep(1000);
                    }

                    //Stop the server and remove it
                    Console.WriteLine("Removing the server for Agent...");
                    modelrepositoryservice.StopListeningToAgent();
                    modelrepositoryservice.RemoveServerForAgent();
                }

                Console.WriteLine("Stopping ModelService...");
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