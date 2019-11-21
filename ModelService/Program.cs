using ModelService.Macromanagement;
using Services;
using System;
using System.Linq;

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
                //Create the services
                var agentservice = AgentService.CreateNewAgentService();
                var computationservice = ComputationService.CreateNewComputationService();
                var repositoryservice = RepositoryService.CreateNewRepositoryService();

                //Set the reference for Macromanagement Algorithms
                var macromanagement_reference = repositoryservice.InterpretMacromanagementRepository();
                MCTSNode.SetMCTSReference(macromanagement_reference);
                POMDPNode.SetPOMDPReference(macromanagement_reference);

                //The C# Model has started as a standalone application
                if (args.Length  > 0)
                {
                    Console.WriteLine("(C#)C# Model has started in standalone mode! Generating accuracy reports...");

                    //Retrieve the repositories
                    var micromanagement_repository = repositoryservice.GetMicromanagementRepository();
                    var macromanagement_repository = repositoryservice.GetMacromanagementRepository();

                    //Store the Micromanagement

                    //Store the Macromanagement
                    var mcts_macromanagements = macromanagement_repository.Select(macromanagement => (new Macromanagement<MCTSNode>(macromanagement)));
                    var pomdp_macromanagements = macromanagement_repository.Select(macromanagement => (new Macromanagement<POMDPNode>(macromanagement)));

                    //Group the Micromanagement by Rank

                    //Group the Macromanagement by Rank
                    var mctsrank_macromanagements = mcts_macromanagements.GroupBy(macromanagement => macromanagement.Rank).ToDictionary(key => key.Key, value => value.ToArray());
                    var pomdprank_macromanagements = pomdp_macromanagements.GroupBy(macromanagement => macromanagement.Rank).ToDictionary(key => key.Key, value => value.ToArray());

                    //Perform the Accuracy Testing for Micromanagement

                    //Perform the Accuracy Testing for Macromanagement
                    var mctsresults = mctsrank_macromanagements.Select(macromanagement => macromanagement.Value.Select(accuracy => accuracy.ToString("R")));
                    var pomdpresults = pomdprank_macromanagements.Select(macromanagement => macromanagement.Value.Select(accuracy => accuracy.ToString("R")));

                    //Create a profile for Accuracy Results for Micromanagement

                    //Create a profile for Accuracy Results for Macromanagement

                    Console.WriteLine("(C#)C# Model is ready to terminate! Press enter to close the model...");
                    Console.ReadLine();
                }
                //The C# Model has started as a model service application
                else
                {
                    Console.WriteLine("(C#)C# Model has started! Generating initial actions...");

                    //Send initialization request to C++ Agent
                    var messages = agentservice.UpdateAgentService("INITIALIZING");

                    //Perform initialization
                    string message = messages.Dequeue(), actions = String.Empty;
                    var kokekokobot = new Macromanagement<MCTSNode>(messages);

                    //Continue to perform simulation and update
                    while(message != "TERMINATE")
                    {
                        //Generate the actions
                        actions = kokekokobot.ToString();

                        //Update C++ Agent
                        messages = agentservice.UpdateAgentService(actions);

                        //Update C# Model
                        message = messages.Dequeue();
                        kokekokobot.UpdateMacromanagement(messages);
                    }

                    Console.WriteLine("(C#)C# Model is terminating...");
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