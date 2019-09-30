using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelService.Types
{
    /// <summary>
    /// A class that represents a player/agent that directly
    /// interacts with the environment. This hold the essential information
    /// regarding the player/agent
    /// </summary>
    public class Agent : ICopyable<Agent>, IMessage, IDefeatable
    {
        /// <summary>
        /// A raw copy of information from source
        /// </summary>
        private string _raw_information = default(string);

        /// <summary>
        /// The current game loop based on the observation
        /// </summary>
        public long Timestamp { get; private set; } = default(long);

        /// <summary>
        /// The player/alliance who controls this agent
        /// </summary>
        public string Owner { get; private set; } = default(string);

        /// <summary>
        /// The current mineral of the agent
        /// </summary>
        public double Minerals { get; private set; } = default(double);

        /// <summary>
        /// The current vespene of the agent
        /// </summary>
        public double Vespene { get; private set; } = default(double);

        /// <summary>
        /// The current supply consumed by the agent
        /// </summary>
        public int Supply { get; private set; } = default(int);

        /// <summary>
        /// The current number of workers
        /// </summary>
        public int Workers { get; private set; } = default(int);

        /// <summary>
        /// The upgrades that have been researched
        /// </summary>
        public List<string> Upgrades { get; private set; } = default(List<string>);

        /// <summary>
        /// The current resources based on the observation
        /// </summary>
        public CostWorth Worth { get; private set; } = default(CostWorth);

        /// <summary>
        /// The units controlled by this agent
        /// </summary>
        public Army Units { get; private set; } = default(Army);

        /// <summary>
        /// The units controlled by enemy that was discovered
        /// </summary>
        public Army Known_Enemy { get; private set; } = default(Army);

        /// <summary>
        /// The chosen action to be executed next game loop
        /// </summary>
        public string Chosen_Action { get; private set; } = default(string);

        /// <summary>
        /// The opposing agent
        /// </summary>
        public Agent Target { get; set; } = default(Agent);

        /// <summary>
        /// Checks if there are still <see cref="Workers"/> and that
        /// there are still surviving structures by checking <see cref="Worth"/>'s
        /// <see cref="CostWorth.Priority"/>
        /// </summary>
        public bool IsDefeated => ((Workers <= 0) && (Worth.Priority <= 0));

        /// <summary>
        /// Checks if <see cref="Target"/> is null or the target's <see cref="IsDefeated"/>
        /// </summary>
        public bool IsOpposingDefeated => (Target == null || Target.IsDefeated);

        /// <summary>
        /// Stores essential information about the agent
        /// </summary>
        /// <param name="raw_information"></param>
        public Agent(string raw_information)
        {
            _raw_information = raw_information;
            var parsed_information = raw_information.Split(':');

            if (parsed_information.Length > 0)
            {
                //Get the macromanagement information
                var macromanagement_information = parsed_information[0].Split(',');
                Timestamp = Convert.ToInt64(macromanagement_information[0]);
                Owner = macromanagement_information[1];
                Minerals = Convert.ToDouble(macromanagement_information[2]);
                Vespene = Convert.ToDouble(macromanagement_information[3]);
                Supply = Convert.ToInt32(macromanagement_information[4]);
                Workers = Convert.ToInt32(macromanagement_information[5]);
                Upgrades = new List<string>(macromanagement_information.Skip(5));
                //Get the micromanagement information
                Units = new Army(parsed_information[1]);
                Known_Enemy = new Army(parsed_information[2]);

                //Get the worth of this player
                var micro_worth = Units.GetValueOfArmy();
                Worth = new CostWorth(micro_worth.Priority, Minerals + micro_worth.Mineral, Vespene + micro_worth.Vespene, Supply);
            }
            else
                throw new ArgumentOutOfRangeException("There are no information to be parsed...");
        }

        /// <summary>
        /// Returns the instance of this agent
        /// </summary>
        /// <returns></returns>
        public Agent GetShallowCopy() => this;

        /// <summary>
        /// Returns a new instance of <see cref="Agent"/> with the same value
        /// as this agent
        /// </summary>
        /// <returns></returns>
        public Agent GetDeepCopy() => new Agent(String.Copy(_raw_information));

        /// <summary>
        /// Returns the <see cref="Chosen_Action"/> 
        /// </summary>
        /// <returns></returns>
        public string CreateMessage() => Chosen_Action;

        /// <summary>
        /// Returns the properties <see cref="Chosen_Action"/>, <see cref="Worth"/>,
        /// and the <see cref="Workers"/> within quotation marks
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Format($@"""{Chosen_Action}"", ""{Worth.Priority}"", ""{Worth.Mineral}"", ""{Worth.Vespene}"", ""{Worth.Supply}"", ""{Workers}""");

        /// <summary>
        /// When the action is applied, update the necessary information
        /// </summary>
        /// <param name="action"></param>
        public void ApplyAction(string action)
        {
            switch(action)
            {
                case "TRAINSCV":
                    Worth = new CostWorth(Worth.Priority + Unit.Values["TERRAN_SCV"].Priority, Worth.Mineral + Unit.Values["TERRAN_SCV"].Mineral, Unit.Values["TERRAN_SCV"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SCV"].Supply);
                    break;
                case "GATHER_MINERAL":
                    Worth = new CostWorth(Worth.Priority, Worth.Mineral + 10, Worth.Vespene, Worth.Supply);
                    break;
            }

            //TODO
            throw new NotImplementedException();
        }
    }
}