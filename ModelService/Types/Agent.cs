using System;
using System.Collections.Generic;
using System.Linq;
using ModelService.ValueTypes;

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
        public long Timestamp { get; set; } = default(long);

        /// <summary>
        /// The player/alliance who controls this agent
        /// </summary>
        public string Owner { get; set; } = default(string);

        /// <summary>
        /// The current mineral of the agent
        /// </summary>
        public double Minerals { get; set; } = default(double);

        /// <summary>
        /// The current vespene of the agent
        /// </summary>
        public double Vespene { get; set; } = default(double);

        /// <summary>
        /// The current supply consumed by the agent
        /// </summary>
        public int Supply { get; set; } = default(int);

        /// <summary>
        /// The current number of workers
        /// </summary>
        public int Workers { get; set; } = default(int);

        /// <summary>
        /// The upgrades that have been researched
        /// </summary>
        public List<string> Upgrades { get; set; } = default(List<string>);

        /// <summary>
        /// The current resources based on the observation
        /// </summary>
        public CostWorth Worth { get; set; } = default(CostWorth);

        /// <summary>
        /// The units controlled by this agent
        /// </summary>
        public Army Units { get; set; } = default(Army);

        /// <summary>
        /// The units controlled by enemy that was discovered
        /// </summary>
        public Army Known_Enemy { get; set; } = default(Army);

        /// <summary>
        /// The chosen action to be executed next game loop
        /// </summary>
        public string Chosen_Action { get; set; } = default(string);

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

        public DateTime EndTime { get; set; } = default(DateTime);

        public int Potential_Depth { get; set; } = default(int);

        public List<Tuple<double, double, CostWorth>> Basis { get; set; } = default(List<Tuple<double, double, CostWorth>>);

        public DateTime Created_Time { get; set; } = default(DateTime);

        /// <summary>
        /// Stores essential information about the agent
        /// </summary>
        /// <param name="raw_information"></param>
        public Agent(string raw_information, DateTime created_time)
        {
            try
            {
                _raw_information = raw_information;
                var parsed_information = raw_information.Split('\n');

                Potential_Depth = parsed_information.Length;
                Basis = new List<Tuple<double, double, CostWorth>>();
                Created_Time = created_time;

                if(parsed_information.Length > 0)
                {
                    EndTime = DateTime.Now.AddSeconds(Convert.ToInt32(parsed_information.Last().Split(',')[1]));
                    foreach (var information in parsed_information)
                    {
                        var details = information.Split(',');

                        Owner = details[2];
                        var command_timestamp = Convert.ToDouble(details[0]);
                        var resource_timestamp = Convert.ToDouble(details[1]);
                        var worth = new CostWorth(Convert.ToInt32(details[8]), Convert.ToDouble(details[5]), Convert.ToDouble(details[6]), Convert.ToInt32(details[7]));

                        Basis.Add(new Tuple<double, double, CostWorth>(command_timestamp, resource_timestamp, worth));
                    }
                }

                Worth = Basis.First().Item3;
                var units = new Unit[Worth.Priority];
                for(int u = 0; u < units.Length; u++)
                {
                    units[u] = new Unit(Convert.ToInt64(Basis.First().Item1), Owner, u.ToString(), "TERRAN_SCV", 0, 0);
                }
                Units = units.ToArmy();
            }
            catch(FormatException ex)
            {
                Console.WriteLine($@"Agent -> {ex.Message}");
                throw new Exception("Raw information for Agent have an invalid set of information");
            }
        }

        public Agent(string owner, CostWorth current_worth, Army units, DateTime origin_time, DateTime end_time, int potential)
        {
            Worth = current_worth;
            Units = units.GetMacroDeepCopy();
            Owner = owner;
            Created_Time = origin_time;
            EndTime = end_time;
            Potential_Depth = potential;
        }

        public Agent()
        {
            _raw_information = "";
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
        public Agent GetDeepCopy() => new Agent(Owner, Worth, Units, Created_Time, EndTime, Potential_Depth);

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
                //Barracks Units
                case "TRAIN_SCV":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SCV"].Priority, Worth.Mineral + Unit.Values["TERRAN_SCV"].Mineral, Unit.Values["TERRAN_SCV"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SCV"].Supply);
                    var new_units = Units.ToList();
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_SCV", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_MARINE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MARINE"].Priority, Worth.Mineral + Unit.Values["TERRAN_MARINE"].Mineral, Unit.Values["TERRAN_MARINE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_MARINE"].Supply);
                    break;
                case "TRAIN_REAPER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_REAPER"].Priority, Worth.Mineral + Unit.Values["TERRAN_REAPER"].Mineral, Unit.Values["TERRAN_REAPER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_REAPER"].Supply);
                    break;
                case "TRAIN_MARAUDER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MARAUDER"].Priority, Worth.Mineral + Unit.Values["TERRAN_MARAUDER"].Mineral, Unit.Values["TERRAN_MARAUDER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_MARAUDER"].Supply);
                    break;
                case "TRAIN_GHOST":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_GHOST"].Priority, Worth.Mineral + Unit.Values["TERRAN_GHOST"].Mineral, Unit.Values["TERRAN_GHOST"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_GHOST"].Supply);
                    break;
                //Factory Units
                case "TRAIN_SIEGETANK":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SIEGETANK"].Priority, Worth.Mineral + Unit.Values["TERRAN_SIEGETANK"].Mineral, Unit.Values["TERRAN_SIEGETANK"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SIEGETANK"].Supply);
                    break;
                case "TRAIN_HELLION":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_HELLION"].Priority, Worth.Mineral + Unit.Values["TERRAN_HELLION"].Mineral, Unit.Values["TERRAN_HELLION"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_HELLION"].Supply);
                    break;
                case "TRAIN_HELLBAT":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_HELLBAT"].Priority, Worth.Mineral + Unit.Values["TERRAN_HELLBAT"].Mineral, Unit.Values["TERRAN_HELLBAT"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_HELLBAT"].Supply);
                    break;
                case "TRAIN_CYCLONE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_CYCLONE"].Priority, Worth.Mineral + Unit.Values["TERRAN_CYCLONE"].Mineral, Unit.Values["TERRAN_CYCLONE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_CYCLONE"].Supply);
                    break;
                case "TRAIN_WIDOWMINE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_WIDOWMINE"].Priority, Worth.Mineral + Unit.Values["TERRAN_WIDOWMINE"].Mineral, Unit.Values["TERRAN_WIDOWMINE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_WIDOWMINE"].Supply);
                    break;
                case "TRAIN_THOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_THOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_THOR"].Mineral, Unit.Values["TERRAN_THOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_THOR"].Supply);
                    break;
                //Starport Units
                case "TRAIN_MEDIVAC":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MEDIVAC"].Priority, Worth.Mineral + Unit.Values["TERRAN_MEDIVAC"].Mineral, Unit.Values["TERRAN_MEDIVAC"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_MEDIVAC"].Supply);
                    break;
                case "TRAIN_BANSHEE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BANSHEE"].Priority, Worth.Mineral + Unit.Values["TERRAN_BANSHEE"].Mineral, Unit.Values["TERRAN_BANSHEE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BANSHEE"].Supply);
                    break;
                case "TRAIN_RAVEN":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_RAVEN"].Priority, Worth.Mineral + Unit.Values["TERRAN_RAVEN"].Mineral, Unit.Values["TERRAN_RAVEN"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_RAVEN"].Supply);
                    break;
                case "TRAIN_VIKINGFIGHTER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_VIKINGFIGHTER"].Priority, Worth.Mineral + Unit.Values["TERRAN_VIKINGFIGHTER"].Mineral, Unit.Values["TERRAN_VIKINGFIGHTER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_VIKINGFIGHTER"].Supply);
                    break;
                case "TRAIN_LIBERATOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_LIBERATOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_LIBERATOR"].Mineral, Unit.Values["TERRAN_LIBERATOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_LIBERATOR"].Supply);
                    break;
                case "TRAIN_BATTLECRUISER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_LIBERATOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_LIBERATOR"].Mineral, Unit.Values["TERRAN_LIBERATOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_LIBERATOR"].Supply);
                    break;
                //Build Commands
                case "BUILD_COMMANDCENTER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_COMMANDCENTER"].Priority, Worth.Mineral + Unit.Values["TERRAN_COMMANDCENTER"].Mineral, Unit.Values["TERRAN_COMMANDCENTER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_COMMANDCENTER"].Supply);
                    break;
                case "BUILD_SUPPLYDEPOT":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SUPPLYDEPOT"].Priority, Worth.Mineral + Unit.Values["TERRAN_SUPPLYDEPOT"].Mineral, Unit.Values["TERRAN_SUPPLYDEPOT"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SUPPLYDEPOT"].Supply);
                    break;
                case "BUILD_REFINERY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_REFINERY"].Priority, Worth.Mineral + Unit.Values["TERRAN_REFINERY"].Mineral, Unit.Values["TERRAN_REFINERY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_REFINERY"].Supply);
                    break;
                case "BUILD_BARRACKS":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKS"].Priority, Worth.Mineral + Unit.Values["TERRAN_BARRACKS"].Mineral, Unit.Values["TERRAN_BARRACKS"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKS"].Supply);
                    break;
                case "BUILD_ENGINEERINGBAY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_ENGINEERINGBAY"].Priority, Worth.Mineral + Unit.Values["TERRAN_ENGINEERINGBAY"].Mineral, Unit.Values["TERRAN_ENGINEERINGBAY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_ENGINEERINGBAY"].Supply);
                    break;
                case "BUILD_BUNKER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BUNKER"].Priority, Worth.Mineral + Unit.Values["TERRAN_BUNKER"].Mineral, Unit.Values["TERRAN_BUNKER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BUNKER"].Supply);
                    break;
                case "BUILD_SENSORTOWER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SENSORTOWER"].Priority, Worth.Mineral + Unit.Values["TERRAN_SENSORTOWER"].Mineral, Unit.Values["TERRAN_SENSORTOWER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SENSORTOWER"].Supply);
                    break;
                case "BUILD_GHOSTACADEMY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_GHOSTACADEMY"].Priority, Worth.Mineral + Unit.Values["TERRAN_GHOSTACADEMY"].Mineral, Unit.Values["TERRAN_GHOSTACADEMY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_GHOSTACADEMY"].Supply);
                    break;
                case "BUILD_STARPORT":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORT"].Priority, Worth.Mineral + Unit.Values["TERRAN_STARPORT"].Mineral, Unit.Values["TERRAN_STARPORT"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORT"].Supply);
                    break;
                case "BUILD_ARMORY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_ARMORY"].Priority, Worth.Mineral + Unit.Values["TERRAN_ARMORY"].Mineral, Unit.Values["TERRAN_ARMORY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_ARMORY"].Supply);
                    break;
                case "BUILD_FUSIONCORE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FUSIONCORE"].Priority, Worth.Mineral + Unit.Values["TERRAN_FUSIONCORE"].Mineral, Unit.Values["TERRAN_FUSIONCORE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_FUSIONCORE"].Supply);
                    break;
                case "HARVEST_RETURN": //Harvesting Minerals and Gas share the same command, Added values are based per trip
                    Worth += new CostWorth(Worth.Priority, Worth.Mineral + 5, Worth.Vespene + 8, Worth.Supply);
                    break;
            }
        }
    }
}