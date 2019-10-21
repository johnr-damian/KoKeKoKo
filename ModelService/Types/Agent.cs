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

                Worth = new CostWorth(12, 50, 0, 15);
                System.Diagnostics.Trace.WriteLine($@"Your starting worth is: {Worth.GetTotalWorth()} from {parsed_information.First().Split(',')[2]}");
                Minerals = Worth.Mineral;
                Vespene = Worth.Vespene;
                Workers = 12;
                var units = new List<Unit>();
                for(int u = 0; u < 12; u++)
                {
                    units.Add(new Unit(Convert.ToInt64(Basis.First().Item1), Owner, u.ToString(), "TERRAN_SCV", 0, 0));
                }
                units.Add(new Unit(Convert.ToInt64(Basis.First().Item1), Owner, units.Count.ToString(), "TERRAN_COMMANDCENTER", 0, 0));
                Units = units.ToArmy();
            }
            catch(FormatException ex)
            {
                Console.WriteLine($@"Agent -> {ex.Message}");
                throw new Exception("Raw information for Agent have an invalid set of information");
            }
        }

        public Agent(string owner, CostWorth current_worth, Army units, DateTime origin_time, DateTime end_time, int potential, double mineral, double vespene, int workers)
        {
            System.Diagnostics.Trace.WriteLine($@"Your inheritance is: {current_worth.GetTotalWorth()} from {owner}");

            Worth = current_worth;
            Units = units.GetMacroDeepCopy();
            Owner = owner;
            Created_Time = origin_time;
            EndTime = end_time;
            Potential_Depth = potential;
            Minerals = Worth.Mineral;
            Vespene = Worth.Vespene;
            Workers = workers;
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
        public Agent GetDeepCopy() => new Agent(Owner, Worth, Units, Created_Time, EndTime, Potential_Depth, Minerals, Vespene, Workers);

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
        /// 
        public void ApplyAction(string action)
        {
            var new_units = Units.ToList(); //cannot redeclare on each case so this has been put outside of swtitch case
            switch (action)
            {
                case "TRAIN_SCV":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SCV"].Priority, Worth.Mineral - Unit.Values["TERRAN_SCV"].Mineral, Worth.Vespene - Unit.Values["TERRAN_SCV"].Vespene, Worth.Supply + Unit.Values["TERRAN_SCV"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_SCV"].Mineral), 2 * (Unit.Values["TERRAN_SCV"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_SCV", 0, 0));
                    Units = new_units.ToArmy();
                    Workers++;
                    break;
                //Barracks Units
                case "TRAIN_MARINE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MARINE"].Priority, Worth.Mineral - Unit.Values["TERRAN_MARINE"].Mineral, Worth.Vespene - Unit.Values["TERRAN_MARINE"].Vespene, Worth.Supply + Unit.Values["TERRAN_MARINE"].Supply);
                    //Worth -= new CostWorth(0, 2* (Worth.Mineral + Unit.Values["TERRAN_MARINE"].Mineral), 2 * (Unit.Values["TERRAN_MARINE"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_MARINE", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_REAPER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_REAPER"].Priority, Worth.Mineral - Unit.Values["TERRAN_REAPER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_REAPER"].Vespene, Worth.Supply + Unit.Values["TERRAN_REAPER"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_REAPER"].Mineral), 2 * (Unit.Values["TERRAN_REAPER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_REAPER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_MARAUDER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MARAUDER"].Priority, Worth.Mineral - Unit.Values["TERRAN_MARAUDER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_MARAUDER"].Vespene, Worth.Supply + Unit.Values["TERRAN_MARAUDER"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_MARAUDER"].Mineral), 2 * (Unit.Values["TERRAN_MARAUDER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_MARAUDER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_GHOST":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_GHOST"].Priority, Worth.Mineral - Unit.Values["TERRAN_GHOST"].Mineral, Worth.Vespene - Unit.Values["TERRAN_GHOST"].Vespene, Worth.Supply + Unit.Values["TERRAN_GHOST"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_GHOST"].Mineral), 2 * (Unit.Values["TERRAN_GHOST"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_GHOST", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                //Factory Units
                case "TRAIN_SIEGETANK":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SIEGETANK"].Priority, Worth.Mineral - Unit.Values["TERRAN_SIEGETANK"].Mineral, Worth.Vespene - Unit.Values["TERRAN_SIEGETANK"].Vespene, Worth.Supply + Unit.Values["TERRAN_SIEGETANK"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_SIEGTANK"].Mineral), 2 * (Unit.Values["TERRAN_SIEGETANK"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_SIEGETANK", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_HELLION":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_HELLION"].Priority, Worth.Mineral - Unit.Values["TERRAN_HELION"].Mineral, Worth.Vespene - Unit.Values["TERRAN_HELION"].Vespene, Worth.Supply + Unit.Values["TERRAN_HELLION"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_HELION"].Mineral), 2 * (Unit.Values["TERRAN_HELION"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_HELLION", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_HELLBAT":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_HELLBAT"].Priority, Worth.Mineral - Unit.Values["TERRAN_HELLBAT"].Mineral, Worth.Vespene - Unit.Values["TERRAN_HELLBAT"].Vespene, Worth.Supply + Unit.Values["TERRAN_HELLBAT"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_HELLBAT"].Mineral), 2 * (Unit.Values["TERRAN_HELLBAT"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_HELLBAT", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_CYCLONE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_CYCLONE"].Priority, Worth.Mineral - Unit.Values["TERRAN_CYCLONE"].Mineral, Worth.Vespene - Unit.Values["TERRAN_CYCLONE"].Vespene, Worth.Supply + Unit.Values["TERRAN_CYCLONE"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_CYCLONE"].Mineral), 2 * (Unit.Values["TERRAN_CYCLONE"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_CYCLONE", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_WIDOWMINE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_WIDOWMINE"].Priority, Worth.Mineral - Unit.Values["TERRAN_WIDOWMINE"].Mineral, Worth.Vespene - Unit.Values["TERRAN_WIDOWMINE"].Vespene, Worth.Supply + Unit.Values["TERRAN_WIDOWMINE"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_WIDOWMINE"].Mineral), 2 * (Unit.Values["TERRAN_WIDOWMINE"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_CYCLONE", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_THOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_THOR"].Priority, Worth.Mineral - Unit.Values["TERRAN_THOR"].Mineral, Worth.Vespene - Unit.Values["TERRAN_THOR"].Vespene, Worth.Supply + Unit.Values["TERRAN_THOR"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_THOR"].Mineral), 2 * (Unit.Values["TERRAN_THOR"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_THOR", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                //Starport Units
                case "TRAIN_MEDIVAC":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MEDIVAC"].Priority, Worth.Mineral - Unit.Values["TERRAN_MEDIVAC"].Mineral, Worth.Vespene - Unit.Values["TERRAN_MEDIVAC"].Vespene, Worth.Supply + Unit.Values["TERRAN_MEDIVAC"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_MEDIVAC"].Mineral), 2 * (Unit.Values["TERRAN_MEDIVAC"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_MEDIVAC", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_BANSHEE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BANSHEE"].Priority, Worth.Mineral - Unit.Values["TERRAN_BANSHEE"].Mineral, Worth.Vespene - Unit.Values["TERRAN_BANSHEE"].Vespene, Worth.Supply + Unit.Values["TERRAN_BANSHEE"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_BANSHEE"].Mineral), 2 * (Unit.Values["TERRAN_BANSHEE"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_BANSHEE", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_RAVEN":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_RAVEN"].Priority, Worth.Mineral - Unit.Values["TERRAN_RAVEN"].Mineral, Worth.Vespene - Unit.Values["TERRAN_RAVEN"].Vespene, Worth.Supply + Unit.Values["TERRAN_RAVEN"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_RAVEN"].Mineral), 2 * (Unit.Values["TERRAN_RAVEN"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_RAVEN", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_VIKINGFIGHTER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_VIKINGFIGHTER"].Priority, Worth.Mineral - Unit.Values["TERRAN_VIKINGFIGHTER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_VIKINGFIGHTER"].Vespene, Worth.Supply + Unit.Values["TERRAN_VIKINGFIGHTER"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_VIKINGFIGHTER"].Mineral), 2 * (Unit.Values["TERRAN_VIKINGFIGHTER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_VIKINGFIGHTER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_LIBERATOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_LIBERATOR"].Priority, Worth.Mineral - Unit.Values["TERRAN_LIBERATOR"].Mineral, Worth.Vespene - Unit.Values["TERRAN_LIBERATOR"].Vespene, Worth.Supply + Unit.Values["TERRAN_LIBERATOR"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_LIBERATOR"].Mineral), 2 * (Unit.Values["TERRAN_LIBERATOR"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_LIBERATOR", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "TRAIN_BATTLECRUISER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BATTLECRUISER"].Priority, Worth.Mineral - Unit.Values["TERRAN_BATTLECRUISER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_BATTLECRUISER"].Vespene, Worth.Supply + Unit.Values["TERRAN_LIBERATOR"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_BATTLECRUISER"].Mineral), 2 * (Unit.Values["TERRAN_BATTLECRUISER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_BATTLECRUISER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                //Build Commands
                case "BUILD_COMMANDCENTER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_COMMANDCENTER"].Priority, Worth.Mineral - Unit.Values["TERRAN_COMMANDCENTER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_COMMANDCENTER"].Vespene, Worth.Supply + Unit.Values["TERRAN_COMMANDCENTER"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_COMMANDCENTER"].Mineral), 2 * (Unit.Values["TERRAN_COMMANDCENTER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_COMMANDCENTER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_SUPPLYDEPOT":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SUPPLYDEPOT"].Priority, Worth.Mineral - Unit.Values["TERRAN_SUPPLYDEPOT"].Mineral, Worth.Vespene - Unit.Values["TERRAN_SUPPLYDEPOT"].Vespene, Worth.Supply + Unit.Values["TERRAN_SUPPLYDEPOT"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_SUPPLYDEPOT"].Mineral), 2 * (Unit.Values["TERRAN_SUPPLYDEPOT"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_SUPPLYDEPOT", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_REFINERY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_REFINERY"].Priority, Worth.Mineral - Unit.Values["TERRAN_REFINERY"].Mineral, Worth.Vespene - Unit.Values["TERRAN_REFINERY"].Vespene, Worth.Supply + Unit.Values["TERRAN_REFINERY"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_REFINERY"].Mineral), 2 * (Unit.Values["TERRAN_REFINERY"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_REFINERY", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_BARRACKS":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKS"].Priority, Worth.Mineral - Unit.Values["TERRAN_BARRACKS"].Mineral, Worth.Vespene - Unit.Values["TERRAN_BARRACKS"].Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKS"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_BARRACKS"].Mineral), 2 * (Unit.Values["TERRAN_BARRACKS"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_BARRACKS", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_BARRACKSTECHLAB":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKSTECHLAB"].Priority, Worth.Mineral - Unit.Values["TERRAN_BARRACKSTECHLAB"].Mineral, Worth.Vespene - Unit.Values["TERRAN_BARRACKSTECHLAB"].Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKSTECHLAB"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_BARRACKSTECHLAB"].Mineral), 2 * (Unit.Values["TERRAN_BARRACKSTECHLAB"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_BARRACKSTECHLAB", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_BARRACKSREACTOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKSREACTOR"].Priority, Worth.Mineral - Unit.Values["TERRAN_BARRACKSREACTOR"].Mineral, Worth.Vespene - Unit.Values["TERRAN_BARRACKSREACTOR"].Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKSREACTOR"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_BARRACKSREACTOR"].Mineral), 2 * (Unit.Values["TERRAN_BARRACKSREACTOR"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_BARRACKSREACTOR", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_ENGINEERINGBAY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_ENGINEERINGBAY"].Priority, Worth.Mineral - Unit.Values["TERRAN_ENGINEERINGBAY"].Mineral, Worth.Vespene - Unit.Values["TERRAN_ENGINEERINGBAY"].Vespene, Worth.Supply + Unit.Values["TERRAN_ENGINEERINGBAY"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_ENGINEERINGBAY"].Mineral), 2 * (Unit.Values["TERRAN_ENGINEERINGBAY"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_ENGINEERINGBAY", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_BUNKER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BUNKER"].Priority, Worth.Mineral - Unit.Values["TERRAN_BUNKER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_BUNKER"].Vespene, Worth.Supply + Unit.Values["TERRAN_BUNKER"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_BUNKER"].Mineral), 2 * (Unit.Values["TERRAN_BUNKER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_BUNKER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_SENSORTOWER":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SENSORTOWER"].Priority, Worth.Mineral - Unit.Values["TERRAN_SENSORTOWER"].Mineral, Worth.Vespene - Unit.Values["TERRAN_SENSORTOWER"].Vespene, Worth.Supply + Unit.Values["TERRAN_SENSORTOWER"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_SENSORTOWER"].Mineral), 2 * (Unit.Values["TERRAN_SENSORTOWER"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_SENSORTOWER", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_GHOSTACADEMY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_GHOSTACADEMY"].Priority, Worth.Mineral - Unit.Values["TERRAN_GHOSTACADEMY"].Mineral, Worth.Vespene - Unit.Values["TERRAN_GHOSTACADEMY"].Vespene, Worth.Supply + Unit.Values["TERRAN_GHOSTACADEMY"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_GHOSTACADEMY"].Mineral), 2 * (Unit.Values["TERRAN_GHOSTACADEMY"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_GHOSTACADEMY", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_FACTORY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FACTORY"].Priority, Worth.Mineral - Unit.Values["TERRAN_FACTORY"].Mineral, Worth.Vespene - Unit.Values["TERRAN_FACTORY"].Vespene, Worth.Supply + Unit.Values["TERRAN_FACTORY"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_FACTORY"].Mineral), 2 * (Unit.Values["TERRAN_FACTORY"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_FACTORY", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_FACTORYTECHLAB":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FACTORYTECHLAB"].Priority, Worth.Mineral - Unit.Values["TERRAN_FACTORYTECHLAB"].Mineral, Worth.Vespene - Unit.Values["TERRAN_FACTORYTECHLAB"].Vespene, Worth.Supply + Unit.Values["TERRAN_FACTORYTECHLAB"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_FACTORYTECHLAB"].Mineral), 2 * (Unit.Values["TERRAN_FACTORYTECHLAB"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_FACTORYTECHLAB", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_FACTORYREACTOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FACTORYREACTOR"].Priority, Worth.Mineral - Unit.Values["TERRAN_FACTORYREACTOR"].Mineral, Worth.Vespene - Unit.Values["TERRAN_FACTORYREACTOR"].Vespene, Worth.Supply + Unit.Values["TERRAN_FACTORYREACTOR"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_FACTORYREACTOR"].Mineral), 2 * (Unit.Values["TERRAN_FACTORYREACTOR"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_FACTORYREACTOR", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_STARPORT":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORT"].Priority, Worth.Mineral - Unit.Values["TERRAN_STARPORT"].Mineral, Worth.Vespene - Unit.Values["TERRAN_STARPORT"].Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORT"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_STARPORT"].Mineral), 2 * (Unit.Values["TERRAN_STARPORT"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_STARPORT", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_STARPORTTECHLAB":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORTTECHLAB"].Priority, Worth.Mineral - Unit.Values["TERRAN_STARPORTTECHLAB"].Mineral, Worth.Vespene - Unit.Values["TERRAN_STARPORTTECHLAB"].Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORTTECHLAB"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_STARPORTTECHLAB"].Mineral), 2 * (Unit.Values["TERRAN_STARPORTTECHLAB"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_STARPORTTECHLAB", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_STARPORTREACTOR":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORTREACTOR"].Priority, Worth.Mineral - Unit.Values["TERRAN_STARPORTREACTOR"].Mineral, Worth.Vespene - Unit.Values["TERRAN_STARPORTREACTOR"].Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORTREACTOR"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_STARPORTREACTOR"].Mineral), 2 * (Unit.Values["TERRAN_STARPORTREACTOR"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_STARPORTREACTOR", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_ARMORY":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_ARMORY"].Priority, Worth.Mineral - Unit.Values["TERRAN_ARMORY"].Mineral, Worth.Vespene - Unit.Values["TERRAN_ARMORY"].Vespene, Worth.Supply + Unit.Values["TERRAN_ARMORY"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_ARMORY"].Mineral), 2 * (Unit.Values["TERRAN_ARMORY"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_ARMORY", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_FUSIONCORE":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FUSIONCORE"].Priority, Worth.Mineral - Unit.Values["TERRAN_FUSIONCORE"].Mineral, Worth.Vespene - Unit.Values["TERRAN_FUSIONCORE"].Vespene, Worth.Supply + Unit.Values["TERRAN_FUSIONCORE"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_FUSIONCORE"].Mineral), 2 * (Unit.Values["TERRAN_FUSIONCORE"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_FUSIONCORE", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "BUILD_MISSILETURRET":
                    Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MISSILETURRET"].Priority, Worth.Mineral - Unit.Values["TERRAN_MISSILETURRET"].Mineral, Worth.Vespene - Unit.Values["TERRAN_MISSILETURRET"].Vespene, Worth.Supply + Unit.Values["TERRAN_MISSILETURRET"].Supply);
                    //Worth -= new CostWorth(0, 2 * (Worth.Mineral + Unit.Values["TERRAN_MISSILETURRET"].Mineral), 2 * (Unit.Values["TERRAN_MISSILETURRET"].Vespene + Worth.Vespene), 0);
                    new_units.Add(new Unit(Convert.ToInt64(DateTime.Now.Subtract(Created_Time).TotalSeconds), Owner, new_units.Count.ToString(), "TERRAN_MISSILETURRET", 0, 0));
                    Units = new_units.ToArmy();
                    break;
                case "HARVEST_RETURN": //Harvesting Minerals and Gas share the same command, Added values are based per trip per scv
                    //Worth += new CostWorth(Worth.Priority, Worth.Mineral + 5, Worth.Vespene + 8, Worth.Supply);
                    //Worth += new CostWorth(Worth.Priority, Worth.Mineral + 5, Worth.Vespene + 8, Worth.Supply);

                    double newmineral = 0, newvespene = 0;
                    if (new_units.Count(unit => unit.Name == "TERRAN_REFINERY") > 0)
                        newvespene = (Worth.Vespene + (8 * Workers));
                    if (new_units.Count(unit => (unit.Name == "TERRAN_COMMANDCENTER" || unit.Name == "TERRAN_ORBITALCOMMAND" || unit.Name == "TERRAN_PLANETARYFORTRESS")) > 0)
                        newmineral = (Worth.Mineral + (5 * Workers));

                    Worth += new CostWorth(0, newmineral, newvespene, 0);
                    break;
                //Barracks Tech Lab Reasearch
                case "RESEARCH_STIMPACK":
                    break;
                case "RESEARCH_COMBATSHIELD":
                    break;
                case "RESEARCH_CONCUSSIVESHELLS":
                    break;
                //Factory Tech Lab Research
                case "RESEARCH_INFERNALPREIGNITER":
                    break;
                case "RESEARCH_DRILLINGCLAWS":
                    break;
                case "RESEARCH_MAGFIELDLAUNCHERS":
                    break;
                //StarportTechLabResearch
                case "RESEARCH_BANSHEECLOAKINGFIELD":
                    break;
                case "RESEARCH_RAVENCORVIDREACTOR":
                    break;
                case "RESEARCH_BANSHEEHYPERFLIGHTROTORS":
                    break;
                case "RESEARCH_HIGHCAPACITYFUELTANKS":
                    break;
                case "RESEARCH_ADVANCEDBALLISTICS":
                    break;
                //Fusion Core Research
                case "RESEARCH_BATTLECRUISERWEAPONREFIT":
                    break;
                //Armory Research 
                case "RESEARCH_TERRANSHIPWEAPONS":
                    break;
                case "RESEARCH_TERRANVEHICLEANDSHIPPLATING":
                    break;
                case "RESEARCH_TERRANVEHICLEWEAPONS":
                    break;
                //Ghost Academy Research
                case "RESEARCH_PERSONALCLOAKING":
                    break;
                case "BUILD_NUKE":
                    break;
            }

            //Worth += new CostWorth(0, (Worth.Mineral + (5 * Workers)), (Worth.Vespene + (8 * Workers)), 0);
            System.Diagnostics.Trace.WriteLine($@"Your current worth after applying {action}: {Worth.GetTotalWorth()} from {Owner}");
        }
    }
}