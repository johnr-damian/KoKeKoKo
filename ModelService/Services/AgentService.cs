using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace ModelService.Services
{
    /// <summary>
    /// Manages the communication of different services that makes up the C# Model.
    /// </summary>
    public partial class Services
    {
        #region Properties
        /// <summary>
        /// The current instance of this class. It provides access to various 
        /// methods for communication, computations, visualizations, and many more.
        /// </summary>
        private static Services Instance { get; set; } = default(Services);

        /// <summary>
        /// The current instance of <see cref="AgentService"/>. It is the instance
        /// that facilitates the communication between the C++ Agent and C# Model.
        /// </summary>
        private AgentService AgentServiceInstance { get; set; } = default(AgentService);

        /// <summary>
        /// The current instance of <see cref="ComputationService"/>. It is the instance
        /// that handles <see cref="RDotNet.REngine"/> and facilitates the computations
        /// necessary for the C# Model.
        /// </summary>
        private ComputationService ComputationServiceInstance { get; set; } = default(ComputationService);

        /// <summary>
        /// The current instance of <see cref="RepositoryService"/>. It is the instance
        /// that facilitates the communication between the C# Model and the CSV files
        /// </summary>
        private RepositoryService RepositoryServiceInstance { get; set; } = default(RepositoryService);

        /// <summary>
        /// The time when this service is created. It is used as a reference point
        /// on when the C# Model has started.
        /// </summary>
        public DateTime CreatedTime { get; private set; } = default(DateTime);

        /// <summary>
        /// The last updated time by the C++ Agent. It is named as 
        /// '<see cref="CurrentTime"/>' because it serves as a reference point on 
        /// how many updates has been made since the <see cref="CreatedTime"/>.
        /// </summary>
        public DateTime CurrentTime { get; private set; } = default(DateTime);

        /// <summary>
        /// The next update time where the C++ Agent will send updated information
        /// about the environment. It is also the indicator for C# Model to send back
        /// whatever simulated to C++ Agent.
        /// </summary>
        public DateTime NextUpdateTime { get; private set; } = default(DateTime); 
        #endregion

        /// <summary>
        /// Initializes the required properties to handle different services.
        /// </summary>
        private Services()
        {
            //Initialize the different services
            AgentServiceInstance = new AgentService(this);
            ComputationServiceInstance = new ComputationService();
            RepositoryServiceInstance = new RepositoryService();

            //Intialize the reference points
            CreatedTime = DateTime.Now;
            CurrentTime = CreatedTime;
            NextUpdateTime = CurrentTime.AddSeconds(15);
        }

        /// <summary>
        /// Facilitates the communication to C++ Agent, and manages the 
        /// outgoing messages from C# Model using named pipes.
        /// </summary>
        private class AgentService
        {
            #region Properties
            /// <summary>
            /// The instance of the master service that handles communication
            /// between the different services.
            /// </summary>
            private Services Instance { get; set; } = default(Services);

            /// <summary>
            /// The recieved messages from C++ Agent.
            /// </summary>
            private Queue<string> IncomingMessages { get; set; } = default(Queue<string>);

            /// <summary>
            /// The messages to be sent to C++ Agent.
            /// </summary>
            private Queue<string> OutgoingMessages { get; set; } = default(Queue<string>);

            /// <summary>
            /// The thread that keeps listening and recieving messages from C++ Agent.
            /// </summary>
            private Tuple<Task, CancellationTokenSource> KeepReadingMessage { get; set; } = default(Tuple<Task, CancellationTokenSource>);

            /// <summary>
            /// The thread that keeps writing and sending messages to C++ Agent.
            /// </summary>
            private Tuple<Task, CancellationTokenSource> KeepWritingMessage { get; set; } = default(Tuple<Task, CancellationTokenSource>); 

            /// <summary>
            /// Checks if there are messages placed in <see cref="IncomingMessages"/>
            /// </summary>
            public bool HasIncomingMessages
            {
                get
                {
                    lock(IncomingMessages)
                    {
                        return (IncomingMessages.Count > 0);
                    }
                }
            }

            /// <summary>
            /// Checks if there are messages placed in <see cref="OutgoingMessages"/>
            /// </summary>
            public bool HasOutgoingMessages
            {
                get
                {
                    lock(OutgoingMessages)
                    {
                        return (OutgoingMessages.Count > 0);
                    }
                }
            }
            #endregion

            /// <summary>
            /// Initializes the required properties to handle communication to C++ Agent.
            /// </summary>
            /// <param name="services"></param>
            public AgentService(Services services)
            {
                //Get the master service
                Instance = services;

                IncomingMessages = new Queue<string>();
                OutgoingMessages = new Queue<string>();
                //Create cancellation tokens for tasks
                var keepreadingmessage_token = new CancellationTokenSource();
                var keepwritingmessage_token = new CancellationTokenSource();
                //Create the tasks including its respective cancellation token
                KeepReadingMessage = new Tuple<Task, CancellationTokenSource>(new Task(() => ReadMessagesFromAgent(), keepreadingmessage_token.Token), keepreadingmessage_token);
                KeepWritingMessage = new Tuple<Task, CancellationTokenSource>(new Task(() => SendMessagesToAgent(), keepwritingmessage_token.Token), keepwritingmessage_token);
            }

            #region Pipes Methods
            /// <summary>
            /// The process that keeps listening and receiving messages from C++ Agent
            /// using the <see cref="NamedPipeServerStream"/>.
            /// </summary>
            /// <remarks>
            /// Parent Process:
            ///     <see cref="StartAgentService"/> -> <see cref="ReadMessagesFromAgent"/>
            ///     <see cref="StopAgentService"/> -> <see cref="ReadMessagesFromAgent"/>
            /// </remarks>
            private void ReadMessagesFromAgent()
            {
                //Check if it has been cancelled from the start
                if (KeepReadingMessage.Item2.IsCancellationRequested)
                    return;

                //Create a server where the C++ Bot will connect to
                using (var server = new NamedPipeServerStream($@"ModelServer"))
                {
                    //Wait for the C++ Bot to connect
                    server.WaitForConnection();
                    if (server.IsConnected)
                    {
                        Console.WriteLine($@"The C++ Bot has successfully connected to C# Model!");

                        //Create a reader to read the incoming messages
                        using (var reader = new StreamReader(server))
                        {
                            //While we keep the server up, try to check if there are
                            //new incoming messages from C++ Agent. If there is, retrieve it.
                            while (!KeepReadingMessage.Item2.IsCancellationRequested)
                            {
                                //There is a new incoming message from C++ Agent
                                if (reader.Peek() > -1)
                                {
                                    lock (IncomingMessages)
                                    {
                                        string message = reader.ReadLine();

                                        //Ensure it is a clean and good message
                                        message = message.Trim('\r', '\0', ' ');
                                        if (!(String.IsNullOrEmpty(message) || String.IsNullOrWhiteSpace(message)))
                                            IncomingMessages.Enqueue(message);
                                    }

                                    //Check if there is more message
                                    continue;
                                }

                                //Since there is no message, it is probably because
                                //it is not yet the nextupdatetime, to give way to other
                                //threads, we must let this pipe sleep until the next update time
                                var time_to_sleep = Instance.CurrentTime.Subtract(Instance.NextUpdateTime);
                                //If there is only 5 seconds left for the next update time, no need to sleep
                                if (time_to_sleep.TotalSeconds > 5)
                                    //Subtract 5 seconds more, so that there is a grace period
                                    //to prepare for reading of incoming messages
                                    Thread.Sleep(time_to_sleep.Subtract(new TimeSpan(0, 0, 5)));
                            }

                            //Close the reader
                            reader.Close();
                        }
                    }

                    //Close the server
                    server.Close();
                }
            }

            /// <summary>
            /// The process that keeps writing and sending messages to C++ Agent using
            /// the <see cref="NamedPipeClientStream"/>.
            /// </summary>
            /// <remarks>
            /// Parent Process:
            ///     <see cref="StartAgentService"/> -> <see cref="ReadMessagesFromAgent"/>
            ///     <see cref="StopAgentService"/> -> <see cref="ReadMessagesFromAgent"/>
            /// </remarks>
            private void SendMessagesToAgent()
            {
                //Check if it has been cancelled from the start
                if (KeepWritingMessage.Item2.IsCancellationRequested)
                    return;

                //Create a client to connect to C++ Bot
                using (var client = new NamedPipeClientStream($@"AgentServer"))
                {
                    //Connect to the C++ Bot
                    client.Connect();
                    if (client.IsConnected)
                    {
                        Console.WriteLine($@"The C# Model has successfully connected to C++ Bot!");

                        //While we keep the client up, try to check if there are
                        //new outgoing messages from C# Model. If there is, send it.
                        while (!KeepWritingMessage.Item2.IsCancellationRequested)
                        {
                            lock (OutgoingMessages)
                            {
                                //There is a new outgoing message from C# Model
                                if (OutgoingMessages.Count > 0)
                                {
                                    //Create a writer to write the outgoing messages to C++ Agent
                                    using (var writer = new StreamWriter(client))
                                    {
                                        while (OutgoingMessages.Count > 0)
                                            writer.WriteLine(OutgoingMessages.Dequeue());

                                        //Flush the writer so it can be read by the C++ Agent
                                        writer.Flush();
                                    }
                                }
                            }

                            //Since there is no message, or all messages has been sent
                            //We will open this client again for the next update time to give way
                            //to other threads
                            var time_to_sleep = Instance.CurrentTime.Subtract(Instance.NextUpdateTime);
                            //If there is only 5 seconds left for the next update time, no need to sleep
                            if (time_to_sleep.TotalSeconds > 5)
                                //Subtract 5 seconds more, so that there is a grace period
                                //to prepare for reading of incoming messages
                                Thread.Sleep(time_to_sleep.Subtract(new TimeSpan(0, 0, 5)));
                        }
                    }

                    //Close the client
                    client.Close();
                }
            }

            /// <summary>
            /// Starts the agent service by starting the <see cref="KeepReadingMessage"/> task
            /// and <see cref="KeepWritingMessage"/> task.
            /// </summary>
            /// <remarks>
            /// Parent Process:
            ///     <see cref="StartAllServices"/> -> <see cref="StartAgentService"/>
            /// </remarks>
            public void StartAgentService()
            {
                KeepReadingMessage.Item1.Start();
                KeepWritingMessage.Item1.Start();
            }

            /// <summary>
            /// Stops the agent service stopping the <see cref="KeepReadingMessage"/>
            /// and <see cref="KeepWritingMessage"/> task.
            /// </summary>
            /// <remarks>
            /// Parent Process:
            ///     <see cref="StopAllServices"/> -> <see cref="StopAgentService"/>
            /// </remarks>
            public void StopAgentService()
            {
                //Send the cancellation request
                KeepReadingMessage.Item2.Cancel();
                KeepWritingMessage.Item2.Cancel();

                //Then we wait for the task to be completed
                KeepReadingMessage.Item1.Wait();
                KeepWritingMessage.Item1.Wait();

                //Dispose everything
                KeepReadingMessage.Item1.Dispose();
                KeepWritingMessage.Item1.Dispose();
                KeepReadingMessage.Item2.Dispose();
                KeepWritingMessage.Item2.Dispose();
            }
            #endregion

            #region Messages Methods
            /// <summary>
            /// Returns all messages in <see cref="IncomingMessages"/>
            /// and clears the <see cref="IncomingMessages"/>.
            /// </summary>
            /// <returns></returns>
            public string[] GetIncomingMessages()
            {
                lock (IncomingMessages)
                {
                    string[] messages = IncomingMessages.ToArray();
                    IncomingMessages.Clear();

                    return messages;
                }
            }

            /// <summary>
            /// Returns the first message in <see cref="IncomingMessages"/>. If there
            /// are no messages, it returns <see cref="String.Empty"/>.
            /// </summary>
            /// <returns></returns>
            public string GetAnIncomingMessage()
            {
                string message = String.Empty;

                lock (IncomingMessages)
                {
                    if (IncomingMessages.Count > 0)
                        message = IncomingMessages.Dequeue();
                }

                return message;
            }

            /// <summary>
            /// Places all the messages in <see cref="OutgoingMessages"/>.
            /// </summary>
            /// <param name="messages"></param>
            public void QueueOutgoingMessages(params string[] messages)
            {
                lock (OutgoingMessages)
                {
                    foreach (string message in messages)
                        OutgoingMessages.Enqueue(message);
                }
            }

            /// <summary>
            /// Places the message in <see cref="OutgoingMessages"/>.
            /// </summary>
            /// <param name="message"></param>
            public void QueueAnOutgoingMessage(string message)
            {
                lock (OutgoingMessages)
                {
                    OutgoingMessages.Enqueue(message);
                }
            } 
            #endregion
        }

        #region Services Methods
        /// <summary>
        /// Creates a new instance of <see cref="Services"/> and initializes the
        /// different services. These different services are <see cref="AgentService"/>,
        /// <see cref="ComputationService"/>, and <see cref="RepositoryService"/>.
        /// </summary>
        /// <returns></returns>
        public static Services CreateNewService()
        {
            if (Instance == null)
                Instance = new Services();

            return Instance;
        }

        /// <summary>
        /// Starts the different services. These services are <see cref="AgentService"/>,
        /// <see cref="ComputationService"/>, and <see cref="RepositoryService"/>.
        /// </summary>
        public void StartAllServices()
        {
            try
            {
                //Start the AgentService
                AgentServiceInstance.StartAgentService();
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"StartAllServices() -> {ex.Message}");
                #if DEBUG
                    System.Diagnostics.Debugger.Break();
                    throw ex;
                #endif
            }
        }

        /// <summary>
        /// Stops the different services. These services are <see cref="AgentService"/>,
        /// <see cref="ComputationService"/>, and <see cref="RepositoryService"/>.
        /// </summary>
        public void StopAllServices()
        {
            try
            {
                //Stop the AgentService
                AgentServiceInstance.StopAgentService();
                AgentServiceInstance = null;


            }
            catch (Exception ex)
            {
                Console.WriteLine($@"StopAllServices() -> {ex.Message}");
                #if DEBUG
                    System.Diagnostics.Debugger.Break();
                    throw ex;
                #endif
            }
        }
        #endregion

        #region AgentService Wrapper Methods
        /// <summary>
        /// Returns all recieved messages as of <see cref="CurrentTime"/> from
        /// C++ Agent
        /// </summary>
        /// <returns></returns>
        public string[] GetRecievedMessages() => AgentServiceInstance.GetIncomingMessages();

        /// <summary>
        /// Returns the first message as of <see cref="CurrentTime"/> from
        /// C++ Agent
        /// </summary>
        /// <returns></returns>
        public string GetARecievedMessage() => AgentServiceInstance.GetAnIncomingMessage();

        /// <summary>
        /// Sends the messages to C++ Agent on the <see cref="NextUpdateTime"/>.
        /// </summary>
        /// <param name="messages"></param>
        public void SendMessagesToAgent(params string[] messages) => AgentServiceInstance.QueueOutgoingMessages(messages);

        /// <summary>
        /// Send a message to C++ Agent on the <see cref="NextUpdateTime"/>
        /// </summary>
        /// <param name="message"></param>
        public void SendAMessageToAgent(string message) => AgentServiceInstance.QueueAnOutgoingMessage(message);
        #endregion
    }
}