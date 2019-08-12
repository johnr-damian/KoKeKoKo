using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public class MicroModel
    {
        private string _message_for_micro = "";

        public MicroModel(string message_for_micro)
        {
            _message_for_micro = message_for_micro;
        }

        private Tuple<string[], double> SustainedDamageSimulation()
        {
            return null;
        }

        public bool ParseMessageForMicro()
        {
            try
            {
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to parse message for micro...");
                Trace.WriteLine($@"Error in Model! MicroModel -> ParseMessageForMicro(): \n\t{ex.Message}");
            }

            return false;
        }

        public Queue<Tuple<string, Tuple<string, string>>> GetResultsFromTheAlgorithms()
        {
            Queue<Tuple<string, Tuple<string, string>>> results = null;

            try
            {

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to get results from the algorithms...");
                Trace.WriteLine($@"Error in Model! MicroModel -> GetResultsFromTheAlgorithms(): \n\t{ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Simulates and returns the predicted result of the battle using different algorithm and target policy
        /// </summary>
        /// <returns>Returns the Algorithm and TargetPolicy used and their returned results</returns>
        /// <example>
        /// The return format is given below:
        /// Algorithm_TargetPolicy, [Player 1's units survived, Player 2's units survived, Player 1's units lost, Player 2's units lost, Time Duration]
        /// </example>
        public static Queue<Tuple<string, Tuple<string, string, string, string, string>>> PredictBattleResults()
        {
            var results = new Queue<Tuple<string, Tuple<string, string, string, string, string>>>();

            try
            {

            }
            catch(Exception ex)
            {

            }

            return results;
        }

        
        public static Tuple<string, string, string> PredictBattleResults(string player_1_units, string player_2_units, Func<bool> test)
        {
            var result = new Tuple<string, string, string>(null, null, null);

            try
            {

            }
            catch(Exception ex)
            {

            }

            return result;
        }
    }
}
