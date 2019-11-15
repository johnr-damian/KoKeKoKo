using Services;
using System;

namespace ModelService
{
    /// <summary>
    /// Manages the different services. In Standalone mode, the C# Model test
    /// the accuracy and precision of the model and returns a compiled results of these
    /// test of Micromanagement and Macromanagement. On the other hand, in Service mode,
    /// the C# Model generates a sequence of actions that must be executed by C++ Agent
    /// in the environment with chosen Micromanagement and Macromangement algorithm.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main process of C# Model.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            try
            {
                //The C# Model has started as a standalone application
                if(args.Length  > 1)
                {

                }
                //The C# Model has started as a service
                else
                {
                    Console.WriteLine("(C#)C# Model has started! Generating initial actions...");

                    //Create the services
                    AgentService agentservice = AgentService.CreateNewAgentService();
                    ComputationService computationservice = ComputationService.CreateNewComputationService();
                    RepositoryService repositoryservice = RepositoryService.CreateNewRepositoryService();

                    //Generate the initial actions
                    var test = new Macromanagement.Macromanagement("a:b", "c:d");
                    var initialactions = String.Join(",", test.GetMacromanagementStuff());

                    //Send the initial action
                    var whattodo = agentservice.UpdateAgentService(initialactions);


                    for (string message = whattodo.Dequeue(); message != "TERMINATE";)
                    {
                        //Generate new actions
                        var newactions = String.Join(",", test.GetMacromanagementStuff());
                        var nextwhattodo = agentservice.UpdateAgentService(newactions);
                        message = nextwhattodo.Dequeue();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"(C#)Error Occurred! {ex.Message}");
            }

            return 0;
        }
    }
}