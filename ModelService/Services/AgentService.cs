using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace Services
{
    /// <summary>
    /// Facilitates the communication to C++ Agent, and manages time-related methods.
    /// </summary>
    public class AgentService
    {
        #region Properties
        /// <summary>
        /// Instance of the current <see cref="AgentService"/>. This provides access
        /// to methods for communicating with C++ Agent and management of time for operations.
        /// </summary>
        private static AgentService Instance { get; set; } = default(AgentService);

        /// <summary>
        /// The time the C# Model has started. It can used as a reference point on how many
        /// updates were made since elapsed time.
        /// </summary>
        public DateTime InitializeTime { get; private set; } = default(DateTime);

        /// <summary>
        /// The time that this service must communicate with C++ Agent to update about the
        /// preferable actions to be executed in the environment base on simulations.
        /// </summary>
        public DateTime NextUpdateTime { get; private set; } = default(DateTime); 
        #endregion

        /// <summary>
        /// Initializes the required properties to handle communication with C++ Agent.
        /// </summary>
        private AgentService()
        {
            InitializeTime = DateTime.Now;
            NextUpdateTime = DateTime.Now.AddSeconds(15);
        }

        /// <summary>
        /// Creates an instance of <see cref="AgentService"/> and returns it initialized.
        /// </summary>
        /// <returns></returns>
        public static AgentService CreateNewAgentService()
        {
            if (Instance == null)
                Instance = new AgentService();

            return Instance;
        }

        /// <summary>
        /// Checks if the current system time is less than the <see cref="NextUpdateTime"/>.
        /// </summary>
        /// <returns></returns>
        public bool ShouldOperationsContinue() => (DateTime.Now < NextUpdateTime);

        /// <summary>
        /// Updates the <see cref="AgentService"/> by sending an update message about the
        /// preferable actions base on the simulations to C++ Agent. Afterwards, it returns
        /// a sequence of message to be parsed by the model and apply a corresponding action.
        /// Lastly, it updates the <see cref="NextUpdateTime"/>.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public Queue<string> UpdateAgentService(string update)
        {
            Queue<string> messages = new Queue<string>();

            try
            {
                //Start the client
                using (var client = new NamedPipeClientStream("AgentServer"))
                {
                    //Wait for the C++ Agent to start the server
                    client.Connect();
                    if(client.IsConnected)
                    {
                        Console.WriteLine("(C#)The C++ Agent has been successfully contacted!");

                        //Read the C++ Agent's message
                        using (var reader = new StreamReader(client))
                        {
                            string raw_message = reader.ReadLine();
                            Console.WriteLine("(C#)Successfully recieved C++ Agent's message!");

                            //Enqueue the C++ Agent's message
                            string[] parsed_message = raw_message.Split(';');
                            for (int iterator = 0; iterator < parsed_message.Length; iterator++)
                                messages.Enqueue(parsed_message[iterator]);

                            //Update the NextUpdateTime
                            NextUpdateTime = DateTime.Now.AddSeconds(15);

                            //Send the update message to C++ Agent
                            using (var writer = new StreamWriter(client))
                            {
                                writer.AutoFlush = true;
                                writer.WriteLine($@"{NextUpdateTime};{update}");
                                writer.Flush();
                                Console.WriteLine("(C#)Successfully sent the update message to C++ Agent");

                                //Close the writer
                                writer.Close();
                            }

                            //Close the reader
                            reader.Close();
                        }

                        //Disconnect from C++ Agent
                        client.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"(C#)Error Occurred! {ex.Message}");
                messages.Clear();
            }

            return messages;
        }

        /// <summary>
        /// Nullifies the <see cref="Instance"/> of this current instance.
        /// </summary>
        public void StopAgentService() => Instance = null;
    }
}