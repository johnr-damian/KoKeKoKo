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
    }
}
