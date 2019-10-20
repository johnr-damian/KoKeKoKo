using ModelService.ValueTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public partial class ModelRepositoryService
    {
        /// <summary>
        /// Accesses a repository and returns the content. If there is no content or
        /// an error occurred, returns a null
        /// </summary>
        /// <param name="repository_name">The folder and the filename of the repository</param>
        /// <returns></returns>
        private static string[] ReadRepository(string repository_name)
        {
            string[] content = null;
            string path = Path.GetFullPath(@"..\..\..\Documents");

            try
            {
                content = File.ReadAllLines(Path.Combine(path, repository_name));
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ReadRepository() -> {ex.Message}...");
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
        public static List<Tuple<string, string, string, string, string>> ReadArmiesRepository()
        {
            var armyrepository = new List<Tuple<string, string, string, string, string>>();

            try
            {
#if DEBUG
                var raw_armyrepository = ReadRepository(@"Training\ArmiesRepository.csv");
#elif TRACE
                var raw_armyrepository = ReadRepository(@"Testing\ArmiesRepository.csv");
#endif

                if (raw_armyrepository.Length > 0)
                {
                    bool prebattle_readyforstorage = false, postbattle_readyforstorage = false;
                    int current_linepointer = -1, prebattle_pointer = -1, postbattle_pointer = -1, offset_pointer = -1;
                    string current_rank = "", current_replayfilename = "", playerone_army = "", playertwo_army = "";

                    while (++current_linepointer < raw_armyrepository.Length)
                    {
                        var current_linecontent = raw_armyrepository[current_linepointer].Split(',');

                        if (current_linecontent.Length == 1)
                        {
                            //If the current line is rank
                            if (_ranks.Contains(current_linecontent[0]))
                                //Take note of the current rank
                                current_rank = current_linecontent[0];
                            //The current line is the constant 'END', end of prebattle, but start of postbattle
                            else if (current_linecontent[0] == "END")
                            {
                                //The prebattle of the current replay file is ready to be stored
                                if (prebattle_readyforstorage)
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ReadArmyRepository() -> {ex.Message}...");
                armyrepository.Clear();
            }

            return armyrepository;
        }

        /// <summary>
        /// Parses the army repository and returns a list of set of resources information
        /// The resources information contains as follow: current rank, current replay filename,
        /// resources of player 1, resources of player 2
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<string, string, string, string>> ReadResourcesRepository()
        {
            var resourcerepository = new List<Tuple<string, string, string, string>>();

            try
            {
#if DEBUG
                var raw_resourcerepository = ReadRepository(@"Training\ResourcesRepository.csv");
#elif TRACE
                var raw_resourcerepository = ReadRepository(@"Testing\ResourcesRepository.csv");
#endif

                if(raw_resourcerepository.Length > 0)
                {
                    bool content_readyforstorage = false;
                    int current_linepointer = -1, content_pointer = -1, offset_pointer = -1;
                    string current_rank = "", current_replayfilename = "";

                    while(++current_linepointer < raw_resourcerepository.Length)
                    {
                        var current_linecontent = raw_resourcerepository[current_linepointer].Split(',');

                        if(current_linecontent.Length == 1)
                        {
                            //If the current line is rank
                            if (_ranks.Contains(current_linecontent[0]))
                                //Take note of the current rank
                                current_rank = current_linecontent[0];
                            //The current line is a replay filename
                            else
                            {
                                //The previous replay file has ended
                                if(content_readyforstorage)
                                {
                                    //Check if the previous line is a rank
                                    offset_pointer = (_ranks.Contains(raw_resourcerepository[current_linepointer - 1])) ? 1 : 0;

                                    //Get the last line of content, the number of elements to take
                                    var contentcardinal = ((current_linepointer - offset_pointer) - content_pointer);
                                    //Get the content, and seperate the two player's information
                                    var content = raw_resourcerepository.Skip(content_pointer).Take(contentcardinal).GroupBy(line => line.Split(',')[1]).ToDictionary(key => key.Key, value =>
                                    {
                                        var resources = value.ToList();
                                        var parsed_resources = new List<string>();

                                        //Convert the resources into 15seconds interval. This will align
                                        //with the ingame time and save space and time to process
                                        for (int iterator = 0, count = (resources.Count - 3); iterator < count; iterator += 3)
                                        {
                                            var firsthalf_content = resources[iterator].Split(',');
                                            var secondhalf_content = resources[iterator + 1].Split(',');

                                            double timestamp = ((Convert.ToDouble(firsthalf_content[0]) + Convert.ToDouble(secondhalf_content[0])) / 2);
                                            double minerals = ((Convert.ToDouble(firsthalf_content[2]) + Convert.ToDouble(secondhalf_content[2])) / 2);
                                            double vespenes = ((Convert.ToDouble(firsthalf_content[3]) + Convert.ToDouble(secondhalf_content[3])) / 2);
                                            int supply = Convert.ToInt32(secondhalf_content[4]);
                                            int worker = Convert.ToInt32(secondhalf_content[5]);
                                            string upgrades = "";
                                            if (secondhalf_content.Length > 6)
                                                upgrades = ("," + String.Join(",", secondhalf_content.Skip(6)));

                                            parsed_resources.Add(String.Format($@"{timestamp},{firsthalf_content[1]},{minerals},{vespenes},{supply},{worker}{upgrades}"));
                                            parsed_resources.Add(resources[iterator + 2]);
                                        }

                                        return parsed_resources;
                                    }).ToList();

                                    //Store the parsed information
                                    resourcerepository.Add(new Tuple<string, string, string, string>(current_rank, current_replayfilename, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
                                    content_readyforstorage = false;
                                }

                                //It is a new replay file
                                if(!content_readyforstorage)
                                {
                                    content_pointer = current_linepointer + 1; //The start of the content
                                    current_replayfilename = current_linecontent[0]; //The filename of the current replay file
                                    content_readyforstorage = true;
                                }
                            }
                        }
                    }

                    //There is a residue content
                    if(content_readyforstorage)
                    {
                        //Check if the previous line is a rank
                        offset_pointer = (_ranks.Contains(raw_resourcerepository[current_linepointer - 1])) ? 1 : 0;

                        //Get the last line of content, the number of elements to take
                        var contentcardinal = ((current_linepointer - offset_pointer) - content_pointer);
                        //Get the content, and seperate the two player's information
                        var content = raw_resourcerepository.Skip(content_pointer).Take(contentcardinal).GroupBy(line => line.Split(',')[1]).ToDictionary(key => key.Key, value =>
                        {
                            var resources = value.ToList();
                            var parsed_resources = new List<string>();

                            //Convert the resources into 15seconds interval. This will align
                            //with the ingame time and save space and time to process
                            for(int iterator = 0, count = (resources.Count - 3); iterator < count; iterator += 3)
                            {
                                var firsthalf_content = resources[iterator].Split(',');
                                var secondhalf_content = resources[iterator + 1].Split(',');

                                double timestamp = ((Convert.ToDouble(firsthalf_content[0]) + Convert.ToDouble(secondhalf_content[0])) / 2);
                                double minerals = ((Convert.ToDouble(firsthalf_content[2]) + Convert.ToDouble(secondhalf_content[2])) / 2);
                                double vespenes = ((Convert.ToDouble(firsthalf_content[3]) + Convert.ToDouble(secondhalf_content[3])) / 2);
                                int supply = Convert.ToInt32(secondhalf_content[4]);
                                int worker = Convert.ToInt32(secondhalf_content[5]);
                                string upgrades = "";
                                if (secondhalf_content.Length > 6)
                                    upgrades = ("," + String.Join(",", secondhalf_content.Skip(6)));

                                parsed_resources.Add(String.Format($@"{timestamp},{firsthalf_content[1]},{minerals},{vespenes},{supply},{worker}{upgrades}"));
                                parsed_resources.Add(resources[iterator + 2]);
                            }

                            return parsed_resources;
                        }).ToList();

                        //Store the parsed information
                        resourcerepository.Add(new Tuple<string, string, string, string>(current_rank, current_replayfilename, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
                        content_readyforstorage = false;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"ReadResourcesRepository() -> {ex.Message}");
                resourcerepository.Clear();
            }

            return resourcerepository;
        }

        /// <summary>
        /// Parses the commands repository and returns a list of set of commands information
        /// The commands information contains as follow: current rank, current replay filename, 
        /// commands of player 1, commands of player 2
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<string, string, string, string>> ReadCommandsRepository()
        {
            var commandsrepository = new List<Tuple<string, string, string, string>>();

            try
            {
#if DEBUG
                var raw_commandsrepository = ReadRepository(@"Training\CommandsRepository.csv");
#elif TRACE
                var raw_commandsrepository = ReadRepository(@"Testing\CommandsRepository.csv");
#endif

                if(raw_commandsrepository.Length > 0)
                {
                    bool content_readyforstorage = false;
                    int current_linepointer = -1, content_pointer = -1, offset_pointer = -1;
                    string current_rank = "", current_replayfilename = "";

                    while(++current_linepointer < raw_commandsrepository.Length)
                    {
                        var current_linecontent = raw_commandsrepository[current_linepointer].Split(',');

                        if(current_linecontent.Length == 1)
                        {
                            //If the current line is rank
                            if (_ranks.Contains(current_linecontent[0]))
                                //Take note of the current rank
                                current_rank = current_linecontent[0];
                            //The current line is a replay filename
                            else
                            {
                                //The previous replay file has ended
                                if(content_readyforstorage)
                                {
                                    //Check if the previous line is a rank
                                    offset_pointer = (_ranks.Contains(raw_commandsrepository[current_linepointer - 1])) ? 1 : 0;

                                    //Get the last line of content, the number of elements to take
                                    var contentcardinal = ((current_linepointer - offset_pointer) - content_pointer);
                                    //Get the content, and seperate the two player's information
                                    var content = raw_commandsrepository.Skip(content_pointer).Take(contentcardinal).GroupBy(line => line.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToList()).ToList();

                                    //Store the parsed information
                                    commandsrepository.Add(new Tuple<string, string, string, string>(current_rank, current_replayfilename, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
                                    content_readyforstorage = false;
                                }

                                //It is a new replay file
                                if(!content_readyforstorage)
                                {
                                    content_pointer = current_linepointer + 1; //The start of the content
                                    current_replayfilename = current_linecontent[0]; //The filename of the current replay file
                                    content_readyforstorage = true;

                                }
                            }
                        }
                    }

                    //There is a residue content
                    //The previous replay file has ended
                    if (content_readyforstorage)
                    {
                        //Check if the previous line is a rank
                        offset_pointer = (_ranks.Contains(raw_commandsrepository[current_linepointer - 1])) ? 1 : 0;

                        //Get the last line of content, the number of elements to take
                        var contentcardinal = ((current_linepointer - offset_pointer) - content_pointer);
                        //Get the content, and seperate the two player's information
                        var content = raw_commandsrepository.Skip(content_pointer).Take(contentcardinal).GroupBy(line => line.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToList()).ToList();

                        //Store the parsed information
                        commandsrepository.Add(new Tuple<string, string, string, string>(current_rank, current_replayfilename, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
                        content_readyforstorage = false;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"ReadCommandsRepository() -> {ex.Message}");
                commandsrepository.Clear();
            }

            return commandsrepository;
        }

        /// <summary>
        /// Relates the <see cref="ReadArmiesRepository"/> and the <see cref="ReadResourcesRepository"/>.
        /// It relates the upgrades done by the player to be applied in the battles of the player.
        /// </summary>
        /// <param name="macromanagement_resources"></param>
        /// <param name="micromanagement"></param>
        /// <returns></returns>
        public static List<Tuple<string, string, string, string, string>> RelateMicroToMacro(List<Tuple<string, string, string, string>> macromanagement_resources, List<Tuple<string, string, string, string, string>> micromanagement)
        {
            var micromacroresult = new List<Tuple<string, string, string, string, string>>();

            try
            {
                //Get the relationship between the micro and macro
                //by their origin replay filename
                var micromacrorelation = (from macro in macromanagement_resources
                                  join micro in micromanagement on macro.Item2 equals micro.Item2 where macro.Item1 == micro.Item1
                                  select (new Tuple<string, string, string, string, string, string, string>(macro.Item3, macro.Item4, micro.Item3, micro.Item4, micro.Item5, micro.Item1, micro.Item2)));

                foreach(var micromacro in micromacrorelation)
                {
                    var owned_units_resources = micromacro.Item1.Split('\n').Last().Split(',');
                    var enemy_units_resources = micromacro.Item2.Split('\n').Last().Split(',');
                    string owned_units_upgrades = "", enemy_units_upgrades = "", owned_units = "", enemy_units = "";

                    if (owned_units_resources.Length > 6)
                        owned_units_upgrades = "," + String.Join(",", owned_units_resources.Skip(6));
                    if (enemy_units_resources.Length > 6)
                        enemy_units_upgrades = "," + String.Join(",", enemy_units_resources.Skip(6));

                    var raw_owned_units = micromacro.Item3.Split('\n');
                    var raw_enemy_units = micromacro.Item4.Split('\n');

                    if (owned_units_upgrades != "")
                        owned_units = String.Join("\n", raw_owned_units.Select(unit => String.Concat(unit, owned_units_upgrades)));
                    else
                        owned_units = micromacro.Item3;
                    if (enemy_units_upgrades != "")
                        enemy_units = String.Join("\n", raw_enemy_units.Select(unit => String.Concat(unit, enemy_units_upgrades)));
                    else
                        enemy_units = micromacro.Item4;

                    micromacroresult.Add(new Tuple<string, string, string, string, string>(micromacro.Item6, micromacro.Item7, owned_units, enemy_units, micromacro.Item5));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"RelateMicroToMacro() -> {ex.Message}");
                micromacroresult.Clear();
            }

            return micromacroresult;
        }

        /// <summary>
        /// Relates the <see cref="ReadResourcesRepository"/> and the <see cref="ReadCommandsRepository"/>.
        /// It relates the commands done by the player to the time, resources, and upgrades done by the player
        /// </summary>
        /// <param name="macromanagement_resources"></param>
        /// <param name="macromanagement_commands"></param>
        /// <returns></returns>
        public static List<Tuple<string, string, string, string>> RelateMacroToMacro(List<Tuple<string, string, string, string>> macromanagement_resources, List<Tuple<string, string, string, string>> macromanagement_commands)
        {
            var macromacroresult = new List<Tuple<string, string, string, string>>();

            try
            {
                //Get the relationship between the macro_resources and macro_commands
                //by their origin replay filename
                var macromacrorelation = (from macro_resources in macromanagement_resources
                                          join macro_commands in macromanagement_commands on macro_resources.Item2 equals macro_commands.Item2
                                          where macro_resources.Item1 == macro_commands.Item1
                                          select (new Tuple<string, string, string, string, string, string>(macro_resources.Item1, macro_resources.Item2, macro_resources.Item3, macro_resources.Item4, macro_commands.Item3, macro_commands.Item4)));

                foreach(var macromacro in macromacrorelation)
                {
                    var owned_resources = macromacro.Item3.Split('\n');
                    var owned_commands = macromacro.Item5.Split('\n');
                    var enemy_resources = macromacro.Item4.Split('\n');
                    var enemy_commands = macromacro.Item6.Split('\n');

                    var owned_relatedmacro = new List<string>();
                    var enemy_relatedmacro = new List<string>();

                    //Construct database-like style. Relate the command based on their time
                    foreach(var owned_command in owned_commands)
                    {
                        var command = owned_command.Split(',');

                        int command_timestamp = Convert.ToInt32(command[0]);
                        foreach (var owned_resource in owned_resources)
                        {
                            var resource = owned_resource.Split(',');

                            int resource_timestamp = Convert.ToInt32(resource[0]);
                            if (command_timestamp <= resource_timestamp)
                            {
                                string upgrades = "";
                                if (resource.Length > 6)
                                    upgrades = ("," + String.Join(",", resource.Skip(6)));

                                string new_details = String.Format($@"{command_timestamp},{resource_timestamp},{command[1]},{command[2]},{command[3]}");
                                new_details += String.Format($@",{resource[2]},{resource[3]},{resource[4]},{resource[5]}{upgrades}");

                                //Add the new related details
                                owned_relatedmacro.Add(new_details);
                                break;
                            }
                        }
                    }
                    foreach(var enemy_command in enemy_commands)
                    {
                        var command = enemy_command.Split(',');

                        int command_timestamp = Convert.ToInt32(command[0]);
                        foreach(var enemy_resource in enemy_resources)
                        {
                            var resource = enemy_resource.Split(',');

                            int resource_timestamp = Convert.ToInt32(resource[0]);
                            if(command_timestamp <= resource_timestamp)
                            {
                                string upgrades = "";
                                if (resource.Length > 6)
                                    upgrades = ("," + String.Join(",", resource.Skip(6)));

                                string new_details = String.Format($@"{command_timestamp},{resource_timestamp},{command[1]},{command[2]},{command[3]}");
                                new_details += String.Format($@",{resource[2]},{resource[3]},{resource[4]},{resource[5]}{upgrades}");

                                //Add the new related details
                                enemy_relatedmacro.Add(new_details);
                                break;
                            }
                        }
                    }

                    macromacroresult.Add(new Tuple<string, string, string, string>(macromacro.Item1, macromacro.Item2, String.Join("\n", owned_relatedmacro), String.Join("\n", enemy_relatedmacro)));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"RelateMacroToMacro() -> {ex.Message}");
                macromacroresult.Clear();
            }

            return macromacroresult;
        }

        public class Pair<A, B>
        {
            public A Item1 { get; set; } = default(A);

            public B Item2 { get; set; } = default(B);
        }


        public static Dictionary<string, Dictionary<string, Pair<double, List<CostWorth>>>> GenerateProbabilitiesAndWorthMatrix(List<Tuple<string, string, string, string>> macromacro)
        {
            var probabilitiesworthresult = new Dictionary<string, Dictionary<string, Pair<double, List<CostWorth>>>>();

            try
            {
                for(int iterator = 0; iterator < macromacro.Count; iterator++)
                {
                    var current_player = macromacro[iterator].Item3.Split('\n');

                    var preempt_command = current_player[0].Split(',');

                    //The first command done by the player is not yet in probability matrix
                    if (!probabilitiesworthresult.ContainsKey(preempt_command[3]))
                    {
                        probabilitiesworthresult.Add(preempt_command[3], new Dictionary<string, Pair<double, List<CostWorth>>>());

                        //The first command does not yet have itself as the next command
                        if(!probabilitiesworthresult[preempt_command[3]].ContainsKey(preempt_command[3]))
                        {
                            //Add a first count
                            probabilitiesworthresult[preempt_command[3]].Add(preempt_command[3], new Pair<double, List<CostWorth>>() {
                                Item1 = 1,
                                Item2 = new List<CostWorth>()
                            });

                            probabilitiesworthresult[preempt_command[3]][preempt_command[3]].Item2.Add(new CostWorth(Convert.ToInt32(preempt_command[8]), Convert.ToDouble(preempt_command[5]), Convert.ToDouble(preempt_command[6]), Convert.ToInt32(preempt_command[7])));
                        }
                    }

                    for(int builder = 0, count = (current_player.Length - 1); builder < count; builder++)
                    {
                        var current_command = current_player[builder].Split(',');
                        var next_command = current_player[builder + 1].Split(',');

                        //If the current command does not exist yet
                        if (!probabilitiesworthresult.ContainsKey(current_command[3]))
                            probabilitiesworthresult.Add(current_command[3], new Dictionary<string, Pair<double, List<CostWorth>>>());

                        //If the next command does not exist yet
                        if (!probabilitiesworthresult[current_command[3]].ContainsKey(next_command[3]))
                            probabilitiesworthresult[current_command[3]].Add(next_command[3], new Pair<double, List<CostWorth>>()
                            {
                                Item1 = 1,
                                Item2 = new List<CostWorth>()
                                {
                                    new CostWorth(Convert.ToInt32(current_command[8]), Convert.ToDouble(current_command[5]), Convert.ToDouble(current_command[6]), Convert.ToInt32(current_command[7]))
                                }
                            });
                        else
                        {
                            probabilitiesworthresult[current_command[3]][next_command[3]].Item1++;
                            probabilitiesworthresult[current_command[3]][next_command[3]].Item2.Add(new CostWorth(Convert.ToInt32(current_command[8]), Convert.ToDouble(current_command[5]), Convert.ToDouble(current_command[6]), Convert.ToInt32(current_command[7])));
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GenerateProbabilitiesAndWorthMatrix() -> {ex.Message}");
                probabilitiesworthresult.Clear();
            }

            throw new NotImplementedException();
            //return probabilitiesworthresult;
        }

        public static List<Tuple<string, string, string, string>> RelateMicroToMacroMacro(List<Tuple<string, string, string, string, string>> micromanagement, List<Tuple<string, string, string, string>> macromanagement)
        {
            var micromacromacroresult = new List<Tuple<string, string, string, string>>();

            try
            {
                //Get the relationship between the macro_resource+commands and micromanagement
                //by their origin replay filename
                var micromacromacrorelation = (from macro in macromanagement
                                               join micro in micromanagement on macro.Item2 equals micro.Item2
                                               where macro.Item1 == micro.Item1
                                               select (new Tuple<string, string, string, string, string, string>(macro.Item1, macro.Item2, macro.Item3, macro.Item4, micro.Item3, micro.Item4)));

                foreach(var micromacromacro in micromacromacrorelation)
                {
                    var owned_resources = micromacromacro.Item3.Split('\n');
                    var owned_units = micromacromacro.Item5.Split('\n');
                    var enemy_resources = micromacromacro.Item4.Split('\n');
                    var enemy_units = micromacromacro.Item6.Split('\n');

                    var owned_relatedmicromacro = new List<string>();
                    var enemy_relatedmicromacro = new List<string>();

                    //Construct a database-like style. Relate the macro based on their time
                    foreach(var owned_unit in owned_units)
                    {
                        var unit = owned_unit.Split(',');

                        int unit_timestamp = Convert.ToInt32(unit[0]);
                        foreach(var owned_resource in owned_resources)
                        {
                            var resource = owned_resource.Split(',');

                            int resource_timestamp = Convert.ToInt32(resource[1]);
                            if(unit_timestamp <= resource_timestamp)
                            {
                                string upgrades = "";
                                if (resource.Length > 9)
                                    upgrades = ("," + String.Join(",", resource.Skip(9)));

                                string new_details = String.Format($@"{resource[0]},{resource[1]},{resource[2]},{resource[3]},{resource[4]}");
                                var unit_worth = Types.Unit.Values[unit[3]];
                                var resource_worth = new CostWorth(Convert.ToInt32(resource[8]), Convert.ToDouble(resource[5]), Convert.ToDouble(resource[6]), Convert.ToInt32(resource[7]));
                                Tuple<string, string, string, string> new_worth = unit_worth + resource_worth;

                                //Append the other stuff
                                new_details += String.Format($@",{new_worth.Item2},{new_worth.Item3},{new_worth.Item4},{new_worth.Item1}{upgrades}");
                                owned_relatedmicromacro.Add(new_details);
                                break;
                            }
                        }
                    }
                    foreach(var enemy_unit in enemy_units)
                    {
                        var unit = enemy_unit.Split(',');

                        int unit_timestamp = Convert.ToInt32(unit[0]);
                        foreach(var enemy_resource in enemy_resources)
                        {
                            var resource = enemy_resource.Split(',');

                            int resource_timestamp = Convert.ToInt32(resource[1]);
                            if(unit_timestamp <= resource_timestamp)
                            {
                                string upgrades = "";
                                if (resource.Length > 9)
                                    upgrades = ("," + String.Join(",", resource.Skip(9)));

                                string new_details = String.Format($@"{resource[0]},{resource[1]},{resource[2]},{resource[3]},{resource[4]}");
                                var unit_worth = Types.Unit.Values[unit[3]];
                                var resource_worth = new CostWorth(Convert.ToInt32(resource[8]), Convert.ToDouble(resource[5]), Convert.ToDouble(resource[6]), Convert.ToInt32(resource[7]));
                                Tuple<string, string, string, string> new_worth = unit_worth + resource_worth;

                                //Append the other stuff
                                new_details += String.Format($@",{new_worth.Item2},{new_worth.Item3},{new_worth.Item4},{new_worth.Item1}{upgrades}");
                                enemy_relatedmicromacro.Add(new_details);
                                break;
                            }
                        }
                    }

                    micromacromacroresult.Add(new Tuple<string, string, string, string>(micromacromacro.Item1, micromacromacro.Item2, String.Join("\n", owned_relatedmicromacro), String.Join("\n", enemy_relatedmicromacro)));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"RelateMicroToMacroMacro() -> {ex.Message}");
                micromacromacroresult.Clear();
            }

            return micromacromacroresult;
        }

        public static List<CostWorth> TestForEuclideanResult()
        {
            var results = new List<CostWorth>();
            var resourcerepo = ReadResourcesRepository();

            if(resourcerepo.Count > 0)
            {
                var test = resourcerepo[0].Item3.Split('\n');

                foreach(var by15secs in test)
                {
                    var by15 = by15secs.Split(',');
                    results.Add(new CostWorth(Convert.ToInt32(by15[5]), Convert.ToDouble(by15[2]), Convert.ToDouble(by15[3]), Convert.ToInt32(by15[4])));
                }
            }

            return results;
        }
    }
}
