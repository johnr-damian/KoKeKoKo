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
    /// Handles communication among Agent, Model, and Repository
    /// </summary>
    public class ModelRepositoryService
    {
        private NamedPipeClientStream _client = null;
        private NamedPipeServerStream _server = null;
        private Task _listenertoagent;

        public Queue<string> Messages { get; set; } = null;

        public bool KeepListeningToAgent { get; set; } = false;

        /// <summary>
        /// Returns true if successfully connected to agent. Else, false.
        /// </summary>
        /// <returns></returns>
        public bool ConnectToAgent()
        {
            try
            {
                _client = new NamedPipeClientStream("AgentServer");
                _client.Connect();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to connect to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> ConnectToAgent(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Returns if successfully sent all messages to agent. Else, false.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessageToAgent(params string[] message)
        {
            try
            {
                using (var sender = new StreamWriter(_client))
                {
                    foreach (var m in message)
                        sender.Write(m);

                    sender.Flush();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error Occurred! Failed to send the message to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> SendMessageToAgent(): \n\t{ex.Message}");
            }

            return false;
        }

        public bool StartModelServiceServer()
        {
            try
            {
                KeepListeningToAgent = true;
                _server = new NamedPipeServerStream("ModelServer");
                _listenertoagent = new Task(ListenToAgent);
                Messages = new Queue<string>();

                _listenertoagent.Start();
                _server.WaitForConnection();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error Occurred! Failed to start the model service server...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> StartModelServiceServer(): \n\t{ex.Message}");
            }

            return false;
        }

        public void ListenToAgent()
        {
            try
            {
                using (var receiver = new StreamReader(_server))
                {

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to keep listening to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> ListenToAgent(): \n\t{ex.Message}");
            }
        }

    }

    /// <summary>
    /// The main thread of the model
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            //var modelrepositoryservice = new ModelRepositoryService();

            //try
            //{
            ////If successfully sent and receive a message from the agent
            //if(modelrepositoryservice.ConnectToAgent())
            //{
            //    modelrepositoryservice.StopExecutingModel = false;
            //    Task.Factory.StartNew(modelrepositoryservice.StartListening);

            //    while(!modelrepositoryservice.StopExecutingModel)
            //    {
            //        if (modelrepositoryservice.Receivedmessages.Count == 0)
            //            continue;
            //        else
            //        {
            //            var message = modelrepositoryservice.Receivedmessages.Dequeue();
            //            if (message == "Exit")
            //                modelrepositoryservice.StopExecutingModel = true;
            //        }
            //    }
            //}

            //Console.WriteLine("ModelService Reporting!");
            //using (var client = new NamedPipeClientStream(@"AgentServer"))
            //{
            //    client.Connect();
            //    using (var writer = new StreamWriter(client))
            //    {
            //        writer.WriteLine("Hello Parent!");
            //        writer.WriteLine("Hello again!");
            //        writer.Flush();
            //    }
            //}

            //    Console.WriteLine("Starting ModelService...");
            //    if(modelrepositoryservice.ConnectToAgent())
            //    {
            //        modelrepositoryservice.SendMessageToAgent("Success!");
            //        modelrepositoryservice.SendMessageToAgent("Success!!");
            //        modelrepositoryservice.SendMessageToAgent("Success!!!");

            //        Console.WriteLine("Starting the Server...");
            //        //if(modelrepositoryservice.StartModelServiceServer())
            //        //{
            //        //    modelrepositoryservice.SendMessageToAgent("Ready Reply");
            //        //}
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Debug.WriteLine($@"Error! Program -> Main: \n\t{ex.Message}...");
            //    Trace.WriteLine("Error Occurred! Failed to keep the model running...");
            //    Console.WriteLine("Error Occurred! ModelService -> Program -> Main");
            //}

            Console.WriteLine("ModelService has started!");
            //for(int spam = 0; spam < 1; spam++)
            //{
            //    using (var client = new NamedPipeClientStream("AgentServer"))
            //    {
            //        client.Connect();

            //        using (var writer = new StreamWriter(client))
            //        {
            //            writer.WriteLine("Hello Parent!");
            //            writer.WriteLine("Hello I am from child!");
            //            writer.Flush();
            //        }

            //        using (var reader = new StreamReader(client))
            //        {
            //            Console.WriteLine($@"Parent Reply: {reader.ReadLine()}");
            //        }
            //    }
            //}

            using (var client = new NamedPipeClientStream("AgentServer"))
            {
                client.Connect();

                Console.WriteLine(client.IsConnected);
                if(client.IsConnected)
                {
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine("Hello");
                        writer.WriteLine("Hello");
                        writer.WriteLine("Hello");
                        writer.Flush();
                    }

                    Console.WriteLine(client.IsConnected);
                    if(client.IsConnected)
                    {
                        using (var reader = new StreamReader(client))
                            Console.WriteLine($@"Parent Reply: {reader.ReadLine()}");
                    }
                }
            }
            Console.WriteLine("Finish spamming!");

            return 100;
        }
    }
}