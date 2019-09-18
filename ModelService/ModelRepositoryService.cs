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
        /// Accesses a repository and returns the content. If there is no content or
        /// an error occurred, returns a null
        /// </summary>
        /// <param name="repository_name">The folder and the filename of the repository</param>
        /// <returns></returns>
        private string[] ReadRepository(string repository_name)
        {
            string[] content = null;
            string path = Path.GetFullPath(@"..\..\..\Documents");

            try
            {
                content = File.ReadAllLines(Path.Combine(path, repository_name));
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"ReadRepository()->{ex.Message}...");
                content = null;
            }

            return content;

        }

        /// <summary>
        /// Parses the army repository and returns a list of set of battles
        /// The battle contains as follow: current rank, current replay filename, 
        /// prebattle player 1 army, prebattle player 2 army, postbattle result
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string, string, string, string>> ReadArmyRepository()
        {
            var armyrepository = new List<Tuple<string, string, string, string, string>>();

            try
            {
                #if DEBUG
                    var raw_armyrepository = ReadRepository(@"Debugging\ArmiesRepository.csv");
                #elif TRACE
                    var raw_armyrepository = ReadRepository(@"Testing\ArmiesRepository.csv");
                #endif

                if(raw_armyrepository.Length > 0)
                {
                    bool prebattle_readyforstorage = false, postbattle_readyforstorage = false;
                    int current_linepointer = -1, prebattle_pointer = -1, postbattle_pointer = -1, offset_pointer = -1;
                    string current_rank = "", current_replayfilename = "", playerone_army = "", playertwo_army = "";

                    while(++current_linepointer < raw_armyrepository.Length)
                    {
                        var current_linecontent = raw_armyrepository[current_linepointer].Split(',');

                        if(current_linecontent.Length == 1)
                        {
                            //If the current line is rank
                            if (_ranks.Contains(current_linecontent[0]))
                                //Take note of the current rank
                                current_rank = current_linecontent[0];
                            //The current line is the constant 'END', end of prebattle, but start of postbattle
                            else if (current_linecontent[0] == "END")
                            {
                                //The prebattle of the current replay file is ready to be stored
                                if(prebattle_readyforstorage)
                                {
                                    //Seperate the two player's armies from one another
                                    var armies = raw_armyrepository.Skip(prebattle_pointer).Take(current_linepointer - prebattle_pointer).GroupBy(line => line.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToList()).ToList();
                                    //Store the army
                                    playerone_army = String.Join("\n", armies[0].Value);
                                    playertwo_army = String.Join("\n", armies[1].Value);

                                    prebattle_readyforstorage = false;
                                    postbattle_pointer = current_linepointer + 1; //The start of content of postbattle
                                    postbattle_readyforstorage = true;
                                }
                            }
                            //The current line is a replay filename
                            else
                            {
                                //The previous replay file has ended
                                if (postbattle_readyforstorage)
                                {
                                    //Check if the previous line is a rank
                                    offset_pointer = (_ranks.Contains(raw_armyrepository[current_linepointer - 1])) ? 1 : 0;

                                    //Get the number of elements to take
                                    var postbattle_contentcardinal = ((current_linepointer - offset_pointer) - postbattle_pointer);
                                    //Get the postbattle content
                                    var postbattle_content = (postbattle_contentcardinal == 0) ? Enumerable.Empty<string>() : raw_armyrepository.Skip(postbattle_pointer).Take(postbattle_contentcardinal);

                                    //Store it in the list
                                    armyrepository.Add(new Tuple<string, string, string, string, string>(current_rank, current_replayfilename, playerone_army, playertwo_army, String.Join("\n", postbattle_content)));

                                    postbattle_readyforstorage = false;
                                }

                                //It is a new replay file
                                if (!prebattle_readyforstorage)
                                {
                                    prebattle_pointer = current_linepointer + 1; //The start of content of prebattle
                                    current_replayfilename = current_linecontent[0]; //The filename of the current replay file
                                    prebattle_readyforstorage = true;
                                }
                            }
                        }
                    }

                    //There is a residue content (the postbattle content is not yet saved)
                    if(postbattle_readyforstorage)
                    {
                        //Check if the previous line is a rank
                        offset_pointer = (_ranks.Contains(raw_armyrepository[current_linepointer - 1])) ? 1 : 0;

                        //Get the number of elements to take
                        var postbattle_contentcardinal = ((current_linepointer - offset_pointer) - postbattle_pointer);
                        //Get the postbattle content
                        var postbattle_content = (postbattle_contentcardinal == 0) ? Enumerable.Empty<string>() : raw_armyrepository.Skip(postbattle_pointer).Take(postbattle_contentcardinal);

                        //Store it in the list
                        armyrepository.Add(new Tuple<string, string, string, string, string>(current_rank, current_replayfilename, playerone_army, playertwo_army, String.Join("\n", postbattle_content)));

                        postbattle_readyforstorage = false;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"ReadArmyRepository()->{ex.Message}...");
                armyrepository.Clear();
            }

            return armyrepository;
        }



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

        private static REngine _engine = null;

        public static REngine StartREngine()
        {
            if (_engine == null)
            {
                REngine.SetEnvironmentVariables();
                _engine = REngine.GetInstance();
                _engine.Initialize();
            }

            return _engine;
        }
    }
}
