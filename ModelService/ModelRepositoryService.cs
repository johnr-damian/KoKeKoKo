using RDotNet;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace ModelService
{
    /// <summary>
    /// Handles the communication between Agent and Model
    /// </summary>
    public partial class ModelRepositoryService
    {
        /// <summary>
        /// The sent messages by the agent to model
        /// </summary>
        private Queue<string> _recievedmessages = null;
        /// <summary>
        /// A thread in model that keeps listening to agent
        /// </summary>
        private Thread _listentoagent = null;
        /// <summary>
        /// If the model has to keep listening to agent
        /// </summary>
        private bool _keeplisteningtoagent = false;

        /// <summary>
        /// The different ranks in StarCraft 2
        /// </summary>
        private List<string> _ranks = new List<string>()
        {
            "Bronze",
            "Silver",
            "Gold",
            "Diamond",
            "Platinum",
            "Master",
            "Grandmaster"
        };

        /// <summary>
        /// Checks the sent message if null, empty or whitespace
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsGoodMessage(string message)
        {
            return (!((String.IsNullOrEmpty(message)) || (String.IsNullOrWhiteSpace(message))));
        }

        /// <summary>
        /// Keeps listening to agent through pipe server
        /// </summary>
        private void ListenToAgent()
        {
            NamedPipeServerStream server = null;

            try
            {
                lock(_recievedmessages)
                {
                    while (!_keeplisteningtoagent)
                        Monitor.Wait(_recievedmessages);
                }

                while(_keeplisteningtoagent)
                {
                    server = null;

                    using (server = new NamedPipeServerStream("ModelServer"))
                    {
                        //Wait for a client to connect
                        server.WaitForConnection();
                        if (server.IsConnected)
                        {
                            using (var listener = new StreamReader(server))
                            {
                                //Read the message
                                var message = listener.ReadLine();

                                //Check if the message is good
                                if (IsGoodMessage(message))
                                {
                                    lock (_recievedmessages)
                                    {
                                        //Queue the message
                                        _recievedmessages.Enqueue(message.Trim('\r', '\0', ' '));
                                    }
                                }

                                listener.Close();
                            }
                        }

                        //Close the server pipe
                        server.Close();
                    }

                    //Give way to other threads by 1 second
                    Thread.Sleep(1000);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to keep listening to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> ListenToAgent(): \n\t{ex.Message}");
            }
        }

        /// <summary>
        /// Creates a queue for messages, a thread for listening to agent and starting it
        /// </summary>
        /// <returns>Returns true if successfully created a queue and started the created thread</returns>
        public bool CreateServerForAgent()
        {
            try
            {
                if(_listentoagent == null)
                {
                    _recievedmessages = new Queue<string>();
                    _listentoagent = new Thread(new ThreadStart(ListenToAgent));
                    _listentoagent.Start();
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to create the server for agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> CreateServerForAgent(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Calls <see cref="StopListeningToAgent"/> and waits for <see cref="ListenToAgent"/> to stop
        /// </summary>
        public void RemoveServerForAgent()
        {
            try
            {
                if (_listentoagent.IsAlive)
                {
                    StopListeningToAgent();
                    _listentoagent.Join();
                    _listentoagent = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to remove the server for agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> RemoveServerForAgent(): \n\t{ex.Message}");
            }
        }

        /// <summary>
        /// Sets the condition to true in <see cref="ListenToAgent"/>
        /// </summary>
        public void StartListeningToAgent()
        {
            lock(_recievedmessages)
            {
                try
                {
                    _keeplisteningtoagent = true;
                    Monitor.Pulse(_recievedmessages);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Occurred! Failed to start listening to agent...");
                    Trace.WriteLine($@"Error in Model! ModelRepositoryService -> StartListeningToAgent(): \n\t{ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sets the condition to false in <see cref="ListenToAgent"/>
        /// </summary>
        public void StopListeningToAgent()
        {
            lock(_recievedmessages)
            {
                try
                {
                    _keeplisteningtoagent = false;
                    Monitor.Pulse(_recievedmessages);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Occurred! Failed to stop listening to agent...");
                    Trace.WriteLine($@"Error in Model! ModelRepositoryService -> StopListeningToAgent(): \n\t{ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if there is a sent message in queue
        /// </summary>
        /// <returns></returns>
        public bool HasMessageFromAgent()
        {
            lock(_recievedmessages)
            {
                try
                {
                    return (_recievedmessages.Count > 0);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Occurred! Failed to check if there is a message in queue...");
                    Trace.WriteLine($@"Error in Model! ModelRepositoryService -> HasMessageFromAgent(): \n\t{ex.Message}");
                }

                return false;
            }
        }

        /// <summary>
        /// Retrieves a message in queue
        /// </summary>
        /// <returns>Returns null if there is an error in retrieval</returns>
        public string GetMessageFromQueue()
        {
            lock(_recievedmessages)
            {
                try
                {
                    return _recievedmessages.Dequeue();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Occurred! Failed to get a message in queue...");
                    Trace.WriteLine($@"Error in Model! ModelRepositoryService -> GetMessageFromQueue(): \n\t{ex.Message}");
                }

                return null;
            }
        }

        /// <summary>
        /// Sends a message to agent
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToAgent(string message)
        {
            try
            {
                using (var client = new NamedPipeClientStream("AgentServer"))
                {
                    client.Connect();
                    
                    if(client.IsConnected)
                    {
                        using (var writer = new StreamWriter(client))
                        {
                            writer.WriteLine(message);
                            writer.Flush();
                        }
                    }

                    client.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to send a message to agent...");
                Trace.WriteLine($@"Error in Model! ModelRepositoryService -> SendMessageToAgent(): \n\t{ex.Message}");
            }
        }
    }
}
