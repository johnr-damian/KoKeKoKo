using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    /// <summary>
    /// A class to hold all information about the Agent, including simulations for Agent
    /// </summary>
    /// <example>
    /// Participant
    ///     -> Get Units of Agent
    ///         -> Compute the value of agent
    ///     -> Initialize the necessary values for simulations
    ///         -> POMDP : States, Transition
    ///         -> MCTS : Decisions 
    ///     -> Generate Action
    ///         -> Runs POMDP and MCTS at the same time and both return an action
    ///     ->
    /// 
    /// </example>
    public class Participant
    {
        private List<Unit> _units = null;



        public bool SetParticipantUnits(string units)
        {
            try
            {
                Console.WriteLine(units);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to set participant's units...");
                Trace.WriteLine($@"Error in Model! Participant -> SetParticipantUnits(): \n\t{ex.Message}");
            }

            return false;
        }

        public bool SetStatesForPOMDP(string states)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }

            return false;
        }

        public bool SetTransitionForPOMDP(string tranisition)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }

            return false;
        }

        public void StartGeneratingActions()
        {

        }
    }
}
