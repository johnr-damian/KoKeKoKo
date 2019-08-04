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
        //A map of created threads where key is the function name, and value is the thread that runs the function
        private Dictionary<string, Thread> _createdthreads = new Dictionary<string, Thread>();
        //A lock when handling messages
        private Mutex _messagelock = new Mutex();
        //If model should keep listening to the agent
        private bool _keeplisteningtoagent = false;

        private REngine engine = null;

        /// <summary>
        /// The sent messages by the agent
        /// </summary>
        public Queue<string> Messages { get; set; } = new Queue<string>();

        /// <summary>
        /// If the model should keep executing
        /// </summary>
        public bool ShouldKeepRunningModel { get; set; } = false;

        /// <summary>
        /// Keeps waiting for the agent to connect and saves the sent message to a queue
        /// </summary>
        private void ListenToAgent()
        {
            try
            {
                NamedPipeServerStream server = null;
                string message = "";


                while(_keeplisteningtoagent)
                {
                    server = null;
                    message = "";

                    using (server = new NamedPipeServerStream("ModelServer"))
                    {
                        //Wait for a client to connect
                        server.WaitForConnection();
                        if(server.IsConnected)
                        {
                            using (var listener = new StreamReader(server))
                            {
                                message = listener.ReadLine();

                                if(!((String.IsNullOrEmpty(message)) || (String.IsNullOrWhiteSpace(message))))
                                {
                                    //Place the message in queue
                                    _messagelock.WaitOne();
                                    Messages.Enqueue(message);
                                    _messagelock.ReleaseMutex();
                                }

                                listener.Close();
                            }
                        }

                        //Close the Pipe
                        server.Close();
                    }

                    Console.WriteLine("Server is running...");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to keep listening to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> ListenToAgent(): \n\t{ex.Message}");
            }
        }

        private void ProcessMessage()
        {
            _messagelock.WaitOne();

            string message = Messages.Peek();
            switch(message)
            {
                case "Exit":
                    Messages.Dequeue();
                    ShouldKeepRunningModel = false;
                    break;
            }
        }

        public  void ProcessMessage(string message)
        {
            var m = message.Split('\n');
            switch(m[0])
            {
                case "Exit":
                    ShouldKeepRunningModel = false;
                    break;
                case "Generate":
                    InitializeREngine();

                    string action = GenerateAction();
                    SendMessageToAgent("Hello World!");
                    break;
                case "ReGenerate":

                    break;
                case "Continue":

                    goto default;
                //Stops and disposes REngine
                case "Stop":
                    StopEngine();
                    break;
                case "Suspend":

                    break;
                default:

                    break;
            }
        }

        private void InitializeREngine()
        {
            REngine.SetEnvironmentVariables();
            engine = REngine.GetInstance();
            engine.Initialize();

            engine.Evaluate(@"library(""markovchain"")");
        }

        private string GenerateAction(params string[] parameters)
        {
            if(parameters.Length > 0)
            {
                foreach (var s in parameters)
                    Console.WriteLine(s);

                engine.Evaluate($@"markovSCStates <- c({parameters[0]})");
                engine.Evaluate($@"markovSCTransition <- matrix(data = c({parameters[1]}), byrow = TRUE, nrow = {parameters[2]})");
                engine.Evaluate("markovSCTransition <- markovSCTransition / rowSums(markovSCTransition)");
                engine.Evaluate($@"SCMC <- new(""markovchain"", states = markovSCStates, transitionMatrix = markovSCTransition, name = ""SC Graph"")");
                engine.Evaluate($@"initialState <- c({parameters[3]})");
                engine.Evaluate("action <- initialState * SCMC");
                return "";
            }

            return "";
        }

        private void StopEngine()
        {
            engine.Dispose();
        }

        /// <summary>
        /// Returns true if successfully started to listen to agent
        /// </summary>
        /// <returns></returns>
        public bool StartListeningToAgent()
        {
            try
            {
                _keeplisteningtoagent = true;

                if(!_createdthreads.ContainsKey("ListenToAgent"))
                {
                    var listentoagent = new Thread(new ThreadStart(ListenToAgent));
                    listentoagent.Start();

                    _createdthreads.Add("ListenToAgent", listentoagent);
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to start listening to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> StartListeningToAgent(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Stops listening to agent
        /// </summary>
        public void StopListeningToAgent()
        {
            try
            {
                _keeplisteningtoagent = false;

                if(_createdthreads.ContainsKey("ListenToAgent"))
                {
                    if(_createdthreads["ListenToAgent"].IsAlive)
                    {
                        _createdthreads["ListenToAgent"].Join();
                        _createdthreads.Remove("ListenToAgent");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to stop listening to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> StopListeningToAgent(): \n\t{ex.Message}");
            }
        }

        public void SendMessageToAgent(string message)
        {
            using (var client = new NamedPipeClientStream("AgentServer"))
            {
                client.Connect();

                using (var writer = new StreamWriter(client))
                {
                    writer.WriteLine(message);
                    writer.Flush();
                }

                client.Close();
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
            var modelrepositoryservice = new ModelRepositoryService();

            try
            {
                if(modelrepositoryservice.StartListeningToAgent())
                {
                    Console.WriteLine("ModelService Server has started...");
                    modelrepositoryservice.ShouldKeepRunningModel = true;

                    while(modelrepositoryservice.ShouldKeepRunningModel)
                    {
                        if (modelrepositoryservice.Messages.Count > 0)
                        {
                            string m = modelrepositoryservice.Messages.Dequeue();
                            if (m == "Exit")
                            {
                                modelrepositoryservice.ShouldKeepRunningModel = false;
                                break;
                            }
                            else
                            {

                            }
                        }
                    }

                    modelrepositoryservice.StopListeningToAgent();
                }

                return 1;
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