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

                                        //While getting the content, parsed the resources and convert
                                        //it to 15 seconds, to save space and to match the bot
                                        for (int iterator = 0, resource_count = (resources.Count - 1); iterator < resource_count; iterator += 2)
                                        {
                                            var first_content = resources[iterator].Split(',');
                                            var second_content = resources[iterator + 1].Split(',');

                                            double timestamp = ((Convert.ToDouble(first_content[0]) + Convert.ToDouble(second_content[0])) / 2);
                                            double minerals = ((Convert.ToDouble(first_content[2]) + Convert.ToDouble(second_content[2])) / 2);
                                            double vespenes = ((Convert.ToDouble(first_content[3]) + Convert.ToDouble(second_content[3])) / 2);
                                            double supply = ((Convert.ToDouble(first_content[4]) + Convert.ToDouble(second_content[4])) / 2);
                                            int worker = ((Convert.ToInt32(first_content[5]) + Convert.ToInt32(second_content[5])) / 2);
                                            string upgrades = "";
                                            if (second_content.Length > 6)
                                                upgrades = ("," + String.Join(",", second_content.Skip(6)));

                                            parsed_resources.Add(String.Format($@"{timestamp},{first_content[1]},{minerals},{vespenes},{supply},{worker}{upgrades}"));
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

                            //While getting the content, parsed the resources and convert
                            //it to 15 seconds, to save space and to match the bot
                            for (int iterator = 0, resource_count = (resources.Count - 1); iterator < resource_count; iterator += 2)
                            {
                                var first_content = resources[iterator].Split(',');
                                var second_content = resources[iterator + 1].Split(',');

                                double timestamp = ((Convert.ToDouble(first_content[0]) + Convert.ToDouble(second_content[0])) / 2);
                                double minerals = ((Convert.ToDouble(first_content[2]) + Convert.ToDouble(second_content[2])) / 2);
                                double vespenes = ((Convert.ToDouble(first_content[3]) + Convert.ToDouble(second_content[3])) / 2);
                                double supply = ((Convert.ToDouble(first_content[4]) + Convert.ToDouble(second_content[4])) / 2);
                                int worker = ((Convert.ToInt32(first_content[5]) + Convert.ToInt32(second_content[5])) / 2);
                                string upgrades = "";
                                if (second_content.Length > 6)
                                    upgrades = ("," + String.Join(",", second_content.Skip(6)));

                                parsed_resources.Add(String.Format($@"{timestamp},{first_content[1]},{minerals},{vespenes},{supply},{worker}{upgrades}"));
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
        /// It relates the upgrades done by the playerto be applied in the battles of the player.
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
                        owned_units_upgrades = "," + String.Join("\n", owned_units_resources.Skip(6));
                    if (enemy_units_resources.Length > 6)
                        enemy_units_upgrades = "," + String.Join("\n", enemy_units_resources.Skip(6));

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
                    
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"RelateMacroToMacro() -> {ex.Message}");
                macromacroresult.Clear();
            }

            return macromacroresult;
        }
    }
}
