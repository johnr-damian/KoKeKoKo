using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Facilitates the access to the repositories, namely: ArmiesRepository, 
    /// CommandsRepository, and ResourcesRepository. In addition, it provide methods
    /// to parse and relate these repositories to one another.
    /// </summary>
    public class RepositoryService
    {
        #region Properties
        /// <summary>
        /// Instance of the current <see cref="RepositoryService"/>. This provides access
        /// to methods for accessing repositories, parsing repositories, and relating repositories.
        /// </summary>
        private static RepositoryService Instance { get; set; } = default(RepositoryService);

        /// <summary>
        /// The different ranks in the StarCraft II game. It is used as a reference point
        /// in difficulty, and used as a comparison when parsing repositories.
        /// </summary>
        public static string[] Ranks { get; private set; } = new string[]
        {
            "BRONZE", "SILVER", "GOLD", "DIAMOND", "PLATINUM", "MASTER", "GRANDMASTER"
        };

        /// <summary>
        /// The relative directory of the repositories. It is used to get the absolute directory
        /// of the repositories by concatenating the filename to this directory.
        /// </summary>
        public string CurrentDirectory { get; private set; } = default(string); 
        #endregion

        /// <summary>
        /// Initializes the required properties to access, parse, and relate the repositories.
        /// </summary>
        private RepositoryService()
        {
            #if DEBUG
                CurrentDirectory = Path.GetFullPath($@"..\..\..\Documents\Training");
            #else
                CurrentDirectory = Path.GetFullPath($@"..\..\..\Documents\Testing");
            #endif
        }

        /// <summary>
        /// Creates an instance of <see cref="RepositoryService"/> and returns it initialized.
        /// </summary>
        /// <returns></returns>
        public static RepositoryService CreateNewRepositoryService()
        {
            if (Instance == null)
                Instance = new RepositoryService();

            return Instance;
        }

        #region Parsing Methods
        /// <summary>
        /// Parses the ArmiesRepository and returns a list of battles. A battle contains the 
        /// following contents: Rank, Replay, Prebattle Player1 Army, Prebattle Player2 Army, Postbattle Result.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     A Prebattle contains as follows: Death Timestamp, Owner, UID, Unit Type, X, Y
        /// </para>
        /// <para>
        ///     A Postbattle contains as follows: 
        /// </para>
        /// </remarks>
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string[], string[], string[]>> ParseArmiesRepository()
        {
            var raw_repository = File.ReadAllLines(Path.Combine(CurrentDirectory, "ArmiesRepository.csv"));

            if (raw_repository.Length > 0)
            {
                bool prebattle_storable = false, postbattle_storable = false;
                int current_pointer = -1, prebattle_pointer = -1, postbattle_pointer = -1;
                string current_rank = "", current_replay = "";
                string[] playerone_content = Enumerable.Empty<string>().ToArray(), playertwo_content = Enumerable.Empty<string>().ToArray();

                while (++current_pointer < raw_repository.Length)
                {
                    var current_content = raw_repository[current_pointer].Split(',');

                    //The current content is either Rank, Replay, or End
                    if (current_content.Length == 1)
                    {
                        //The current content is Rank
                        if (Ranks.Contains(current_content[0], StringComparer.OrdinalIgnoreCase))
                            current_rank = current_content[0];
                        //The current content is End
                        else if (String.Equals(current_content[0], "END", StringComparison.OrdinalIgnoreCase))
                        {
                            //Store the prebattle of the current replay
                            if (prebattle_storable)
                            {
                                //Separate the two player's armies
                                var armies_content = raw_repository.Skip(prebattle_pointer).Take(current_pointer - prebattle_pointer)
                                    .GroupBy(content => content.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToArray()).ToArray();

                                //Store their respective armies
                                playerone_content = armies_content[0].Value;
                                playertwo_content = armies_content[1].Value;

                                //Update the necessary flags and pointers
                                prebattle_storable = false;
                                postbattle_pointer = current_pointer + 1; //The start of postbattle content
                                postbattle_storable = true;
                            }
                        }
                        //The current content is Replay
                        else
                        {
                            //The previous replay is finished. Store the postbattle of previous replay
                            if (postbattle_storable)
                            {
                                //Check if previous content is a Rank
                                var offset_pointer = ((Ranks.Contains(raw_repository[current_pointer - 1], StringComparer.OrdinalIgnoreCase)) ? 1 : 0);

                                //Check if there is a postbattle
                                var postbattle_cardinal = ((current_pointer - offset_pointer) - postbattle_pointer);
                                var postbattle_content = ((postbattle_cardinal == 0) ? Enumerable.Empty<string>() : raw_repository.Skip(postbattle_pointer).Take(postbattle_cardinal));

                                //Update the necessary flags and pointers
                                postbattle_storable = false;

                                //Return the parsed replay
                                yield return (new Tuple<string, string, string[], string[], string[]>(current_rank, current_replay, playerone_content, playertwo_content, postbattle_content.ToArray()));
                            }

                            //A new replay
                            if (!prebattle_storable)
                            {
                                //Update the necessary flags and pointers
                                current_replay = current_content[0];
                                prebattle_pointer = current_pointer + 1; //The start of prebattle content
                                prebattle_storable = true;
                            }
                        }
                    }
                }

                //Return the last replay
                if (postbattle_storable)
                {
                    //Check if previous content is a Rank
                    var offset_pointer = ((Ranks.Contains(raw_repository[current_pointer - 1], StringComparer.OrdinalIgnoreCase)) ? 1 : 0);

                    //Check if there is a postbattle
                    var postbattle_cardinal = ((current_pointer - offset_pointer) - postbattle_pointer);
                    var postbattle_content = ((postbattle_cardinal == 0) ? Enumerable.Empty<string>() : raw_repository.Skip(postbattle_pointer).Take(postbattle_cardinal));

                    //Update the necessary flags and pointers
                    postbattle_storable = false;

                    //Return the parsed replay
                    yield return (new Tuple<string, string, string[], string[], string[]>(current_rank, current_replay, playerone_content, playertwo_content, postbattle_content.ToArray()));
                }
            }
            else
                throw new Exception($@"Failed to parse {Path.Combine(CurrentDirectory, "ArmiesRepository.csv")}...");
        }

        /// <summary>
        /// Parses the CommandsRepository and returns a list of macromanagement commands. A macromangement command contains the
        /// following contents: Rank, Replay, Commands Player1, Commands Player2.
        /// </summary>
        /// <remarks>
        /// A Commands contains as follows: Completion Timestamp, Owner, Command, Command Category
        /// </remarks>
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string[], string[]>> ParseCommandsRepository()
        {
            var raw_repository = File.ReadAllLines(Path.Combine(CurrentDirectory, "CommandsRepository.csv"));

            if (raw_repository.Length > 0)
            {
                bool content_storable = false;
                int current_pointer = -1, content_pointer = -1;
                string current_rank = "", current_replay = "";

                while (++current_pointer < raw_repository.Length)
                {
                    var current_content = raw_repository[current_pointer].Split(',');

                    //The current content is either Rank, or Replay
                    if (current_content.Length == 1)
                    {
                        //The current content is Rank
                        if (Ranks.Contains(current_content[0], StringComparer.OrdinalIgnoreCase))
                            current_rank = current_content[0];
                        //The current content is Replay
                        else
                        {
                            //The previous replay is finished. Store the content of previous replay
                            if (content_storable)
                            {
                                //Check if previous content is a Rank
                                var offset_pointer = ((Ranks.Contains(raw_repository[current_pointer - 1], StringComparer.OrdinalIgnoreCase)) ? 1 : 0);

                                //Get the content
                                var content_cardinal = ((current_pointer - offset_pointer) - content_pointer);
                                var content = raw_repository.Skip(content_pointer).Take(content_cardinal)
                                    .GroupBy(c => c.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToArray()).ToArray();

                                //Update the necessary flags and pointers
                                content_storable = false;

                                //Return the parsed replay
                                yield return (new Tuple<string, string, string[], string[]>(current_rank, current_replay, content[0].Value, content[1].Value));
                            }

                            //A new replay
                            if (!content_storable)
                            {
                                //Update the necessary flags and pointers
                                current_replay = current_content[0];
                                content_pointer = current_pointer + 1; //The start of the content
                                content_storable = true;
                            }
                        }
                    }
                }

                //Return the last replay
                if (content_storable)
                {
                    //Check if previous content is a Rank
                    var offset_pointer = ((Ranks.Contains(raw_repository[current_pointer - 1], StringComparer.OrdinalIgnoreCase)) ? 1 : 0);

                    //Get the content
                    var content_cardinal = ((current_pointer - offset_pointer) - content_pointer);
                    var content = raw_repository.Skip(content_pointer).Take(content_cardinal)
                        .GroupBy(c => c.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToArray()).ToArray();

                    //Update the necessary flags and pointers
                    content_storable = false;

                    //Return the parsed replay
                    yield return (new Tuple<string, string, string[], string[]>(current_rank, current_replay, content[0].Value, content[1].Value));
                }
            }
            else
                throw new Exception($@"Failed to parse {Path.Combine(CurrentDirectory, "CommandsRepository.csv")}");
        }

        /// <summary>
        /// Parses the ResourcesRepository and returns a list of macromanagement resources. A macromanagement resources contains the
        /// following contents: Rank, Replay, Resources Player1, Resources Player2.
        /// </summary>
        /// <remarks>
        /// A Resources contains as follows: Interval of 10 Timestamp, Owner, Current Mineral, Current Vespene, Current Supply, Number of Workers, and Upgrades
        /// </remarks>
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string[], string[]>> ParseResourcesRepository()
        {
            var raw_repository = File.ReadAllLines(Path.Combine(CurrentDirectory, "ResourcesRepository.csv"));

            if (raw_repository.Length > 0)
            {
                bool content_storable = false;
                int current_pointer = -1, content_pointer = -1;
                string current_rank = "", current_replay = "";

                while (++current_pointer < raw_repository.Length)
                {
                    var current_content = raw_repository[current_pointer].Split(',');

                    //The current content is either Rank, or Replay
                    if (current_content.Length == 1)
                    {
                        //The current content is Rank
                        if (Ranks.Contains(current_content[0], StringComparer.OrdinalIgnoreCase))
                            current_rank = current_content[0];
                        //The current content is Replay
                        else
                        {
                            //The previous replay is finished. Store the content of previous replay
                            if (content_storable)
                            {
                                //Check if the previous content is a Rank
                                var offset_pointer = ((Ranks.Contains(raw_repository[current_pointer - 1], StringComparer.OrdinalIgnoreCase)) ? 1 : 0);

                                //Get the content
                                var content_cardinal = ((current_pointer - offset_pointer) - content_pointer);
                                var content = raw_repository.Skip(content_pointer).Take(content_cardinal)
                                    .GroupBy(c => c.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToArray()).ToArray();

                                //Update the necessary flags and pointers
                                content_storable = false;

                                //Return the parsed replay
                                yield return (new Tuple<string, string, string[], string[]>(current_rank, current_replay, content[0].Value, content[1].Value));
                            }

                            //A new replay
                            if (!content_storable)
                            {
                                //Update the necessary flags and pointers
                                current_replay = current_content[0];
                                content_pointer = current_pointer + 1; //The start of the content
                                content_storable = true;
                            }
                        }
                    }
                }

                //Return the last replay
                if (content_storable)
                {
                    //Check if the previous content is a Rank
                    var offset_pointer = ((Ranks.Contains(raw_repository[current_pointer - 1], StringComparer.OrdinalIgnoreCase)) ? 1 : 0);

                    //Get the content
                    var content_cardinal = ((current_pointer - offset_pointer) - content_pointer);
                    var content = raw_repository.Skip(content_pointer).Take(content_cardinal)
                        .GroupBy(c => c.Split(',')[1]).ToDictionary(key => key.Key, value => value.ToArray()).ToArray();

                    //Update the necessary flags and pointers
                    content_storable = false;

                    //Return the parsed replay
                    yield return (new Tuple<string, string, string[], string[]>(current_rank, current_replay, content[0].Value, content[1].Value));
                }
            }
            else
                throw new Exception($@"Failed to parse {Path.Combine(CurrentDirectory, "ResourcesRepository.csv")}");
        }
        #endregion

        #region Retrieval Methods
        /// <summary>
        /// Retrieves a parsed ArmiesRepository and ResourcesRepository. Afterwards, it combines the ArmiesRepository
        /// and ResourcesRepository using the timestamp in ArmiesRepository as key in ResourcesRepository. A micromanagement
        /// repository contains as follows: Rank, Replay, Parsed Prebattle Player 1 Army, Parsed Prebattle Player 2 Army, and
        /// Parsed Postbattle Result.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     A Parsed Prebattle contains as follows: Death Timestamp, Owner, UID, Unit Type, X, Y, Upgrades
        /// </para>
        /// <para>
        ///     A Parsed Postbattle contains as follows: Owner, UID, Unit Type
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string, string[], string[], string[]>> GetMicromanagementRepository()
        {
            //Get the ArmiesRepository and ResourcesRepository
            var armiesrepository = ParseArmiesRepository();
            var resourcesrepository = ParseResourcesRepository();

            //Combine the ArmiesRepository and ResourcesRepository
            var micromanagement_relationships = (from armyrepository in armiesrepository join resourcerepository
                                                in resourcesrepository on armyrepository.Item2 equals resourcerepository.Item2
                                                where armyrepository.Item1 == resourcerepository.Item1 select
                                                (new Tuple<string, string, string[], string[], string[], string[], string[]>(armyrepository.Item1, armyrepository.Item2, armyrepository.Item3, armyrepository.Item4, armyrepository.Item5, resourcerepository.Item3, resourcerepository.Item4)));

            //Relate the player's researched upgrades to its own units
            foreach (var micromanagement_relationship in micromanagement_relationships)
            {
                //Relate the player one's units and researched upgrades
                var playerone_parsedunits = micromanagement_relationship.Item3.Select(unit =>
                {
                    //The timestamp in ArmiesRepository becomes the index for ResourcesRepository, where
                    //the index of ResourcesRepository corresponds to its timestamp
                    int time_of_death = Convert.ToInt32(Math.Floor(Convert.ToDouble(unit.Split(',')[0]) / 10) - 1);

                    //If there is a corresponding resource in ResourcesRepository
                    if ((time_of_death < micromanagement_relationship.Item6.Length) && (time_of_death >= 0))
                    {
                        var current_resource = micromanagement_relationship.Item6[time_of_death].Split(',');

                        //There are researched upgrades
                        if (current_resource.Length > 6)
                            return String.Concat(unit, ",", String.Join(",", current_resource.Skip(6)));
                    }
                    //There is no corresponding resource for the timestamp
                    else
                    {
                        var current_resource = micromanagement_relationship.Item6.Last().Split(',');

                        //There are researched upgrades
                        if (current_resource.Length > 6)
                            return String.Concat(unit, ",", String.Join(",", current_resource.Skip(6)));
                    }

                    //There is no corresponding resource 
                    return unit;
                }).ToArray();

                //Relate the player two's units and researched upgrades
                var playertwo_parsedunits = micromanagement_relationship.Item4.Select(unit =>
                {
                    //The timestamp in ArmiesRepository becomes the index for ResourcesRepository, where
                    //the index of ResourcesRepository corresponds to its timestamp
                    int time_of_death = Convert.ToInt32(Math.Floor(Convert.ToDouble(unit.Split(',')[0]) / 10) - 1);

                    if (time_of_death < micromanagement_relationship.Item7.Length)
                    {
                        var current_resource = micromanagement_relationship.Item7[time_of_death].Split(',');

                        //There are researched upgrades
                        if (current_resource.Length > 6)
                            return String.Concat(unit, ",", String.Join(",", current_resource.Skip(6)));
                    }
                    //There is no corresponding resource for the timestamp
                    else
                    {
                        var current_resource = micromanagement_relationship.Item7.Last().Split(',');

                        //There are researched upgrades
                        if (current_resource.Length > 6)
                            return String.Concat(unit, ",", String.Join(",", current_resource.Skip(6)));
                    }

                    //There is no corresponding resource
                    return unit;
                }).ToArray();

                //Parse the postbattle result into surviving unit's unique id
                var parsedpostbattle = micromanagement_relationship.Item5.Select(unit => String.Join(",", unit.Split(',').Skip(1).Take(3))).ToArray();

                yield return new Tuple<string, string, string[], string[], string[]>(micromanagement_relationship.Item1, micromanagement_relationship.Item2, playerone_parsedunits, playertwo_parsedunits, parsedpostbattle);
            }
        }

        /// <summary>
        /// Retrieves a parsed CommandsRepository and ResourcesRepository. Afterwards, it combines the CommandsRepository
        /// and ResourcesRepository using the timestamp in CommandsRepository as key in ResourcesRepository. A macromanagement 
        /// repository contains as follows: Rank, Replay, Current Mineral, Current Vespene, Supply, Number of Workers, Upgrades;
        /// Commands, Category of Commands
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string, string[], string[]>> GetMacromanagementRepository()
        {
            //Get the CommandsRepository and ResourcesRepository
            var commandsrepository = ParseCommandsRepository();
            var resourcesrepository = ParseResourcesRepository();

            //Combine the CommandsRepository and ResourcesRepository
            var macromanagement_relationships = (from commandrepository in commandsrepository join resourcerepository
                                                in resourcesrepository on commandrepository.Item2 equals resourcerepository.Item2
                                                where commandrepository.Item1 == resourcerepository.Item1 select
                                                (new Tuple<string, string, string[], string[], string[], string[]>(commandrepository.Item1, commandrepository.Item2, commandrepository.Item3, commandrepository.Item4, resourcerepository.Item3, resourcerepository.Item4)));

            //Relate the player's current resources to its executed commands
            foreach (var macromanagement_relationship in macromanagement_relationships)
            {
                //Relate the player one's commands and resources
                var playerone_parsedcommands = macromanagement_relationship.Item5.Select(resource =>
                {
                    //Get the commands that is respective to the current resource
                    var current_commands = macromanagement_relationship.Item3.Where(command =>
                    {
                        int time_of_completion = Convert.ToInt32(Math.Floor(Convert.ToDouble(command.Split(',')[0]) / 10) * 10);

                        return (Convert.ToInt32(resource.Split(',')[0]) == time_of_completion);
                    }).Select(command => String.Join(",", command.Split(',').Skip(2).Take(2)));

                    return String.Join(";", resource, String.Join(",", current_commands));
                }).ToArray();

                //Relate the player two's units and researched upgrades
                var playertwo_parsedcommands = macromanagement_relationship.Item6.Select(resource =>
                {
                    //Get the commands that is respective to the current resource
                    var current_commands = macromanagement_relationship.Item4.Where(command =>
                    {
                        int time_of_completion = Convert.ToInt32(Math.Floor(Convert.ToDouble(command.Split(',')[0]) / 10) * 10);

                        return (Convert.ToInt32(resource.Split(',')[0]) == time_of_completion);
                    }).Select(command => String.Join(",", command.Split(',').Skip(2).Take(2)));

                    return String.Join(";", resource, String.Join(",", current_commands));
                }).ToArray();

                yield return new Tuple<string, string, string[], string[]>(macromanagement_relationship.Item1, macromanagement_relationship.Item2, playerone_parsedcommands, playertwo_parsedcommands);
            }
        }

        /// <summary>
        /// Retrieves a parsed CommandsRepository and ResourcesRepository. Afterwards, it combines the CommandsRepository
        /// and ResourcesRepository using the timestamp in CommandsRepository as key in ResourcesRepository. An interpreted
        /// macromanagement repository contains as follows: the distinct commands used in replays throughout ranks, and a
        /// probability matrix with expected reward doing that action from previous action.
        /// </summary>
        /// <returns></returns>
        public Tuple<string[], double[,][]> InterpretMacromanagementRepository()
        {
            //Get the CommandsRepository and ResourcesRepository
            var commandsrepository = ParseCommandsRepository();
            var resourcesrepository = ParseResourcesRepository();

            //Combine the CommandsRepository and ResourcesRepository
            var macromanagement_relationships = (from commandrepository in commandsrepository join resourcerepository
                                                in resourcesrepository on commandrepository.Item2 equals resourcerepository.Item2
                                                where commandrepository.Item1 == resourcerepository.Item1 select
                                                (new Tuple<string, string, string[], string[], string[], string[]>(commandrepository.Item1, commandrepository.Item2, commandrepository.Item3, commandrepository.Item4, resourcerepository.Item3, resourcerepository.Item4))).ToArray();

            //Initialize required fields for probability matrix with expected reward
            var compiled_commands = macromanagement_relationships.Select(macromanagement_relationship =>
            {
                var playerone_commands = macromanagement_relationship.Item3.Select(command => command.Split(',')[2]).Distinct();
                var playertwo_commands = macromanagement_relationship.Item4.Select(command => command.Split(',')[2]).Distinct();

                return String.Join(",", playerone_commands.Union(playertwo_commands));
            });
            string[] distinct_commands = String.Join(",", compiled_commands).Split(',').Distinct().ToArray();
            double[,][] commands_matrix = new double[distinct_commands.Length, distinct_commands.Length][];
            for (int row_iterator = 0; row_iterator < distinct_commands.Length; row_iterator++)
            {
                for (int column_iterator = 0; column_iterator < distinct_commands.Length; column_iterator++)
                    commands_matrix[row_iterator, column_iterator] = new double[6] { 0, 0, 0, 0, 0, 0 };
            }

            //Create a probability matrix with expected reward
            foreach (var macromanagement_relationship in macromanagement_relationships)
            {
                //Parse the player one's commands
                for (int command_iterator = 0, length = (macromanagement_relationship.Item3.Length - 1); command_iterator < length; command_iterator++)
                {
                    var current_command = macromanagement_relationship.Item3[command_iterator].Split(',');
                    var next_command = macromanagement_relationship.Item3[command_iterator + 1].Split(',');

                    //Get the indexes for probability matrix
                    var xaxis_index = Array.IndexOf(distinct_commands, current_command[2]);
                    var yaxis_index = Array.IndexOf(distinct_commands, next_command[2]);

                    //Get the expected reward, which is the current resource of the next command
                    int time_of_completion = Convert.ToInt32(Math.Floor(Convert.ToDouble(next_command[0]) / 10) - 1);
                    string[] expected_reward = ((time_of_completion < macromanagement_relationship.Item5.Length) ? macromanagement_relationship.Item5[time_of_completion] : macromanagement_relationship.Item5.Last()).Split(',');

                    //Update the matrix with expected reward
                    commands_matrix[xaxis_index, yaxis_index][0] += 1; //Probability
                    commands_matrix[xaxis_index, yaxis_index][1] += Convert.ToDouble(expected_reward[2]); //Mineral
                    commands_matrix[xaxis_index, yaxis_index][2] += Convert.ToDouble(expected_reward[3]); //Vespene
                    commands_matrix[xaxis_index, yaxis_index][3] += Convert.ToDouble(expected_reward[4]); //Supply
                    commands_matrix[xaxis_index, yaxis_index][4] += Convert.ToDouble(expected_reward[5]); //Workers
                    commands_matrix[xaxis_index, yaxis_index][5] += expected_reward.Skip(6).Count(); //Upgrades
                }

                //Parse the player two's commands
                for (int command_iterator = 0, length = (macromanagement_relationship.Item4.Length - 1); command_iterator < length; command_iterator++)
                {
                    var current_command = macromanagement_relationship.Item4[command_iterator].Split(',');
                    var next_command = macromanagement_relationship.Item4[command_iterator + 1].Split(',');

                    //Get the indexes for probability matrix
                    var xaxis_index = Array.IndexOf(distinct_commands, current_command[2]);
                    var yaxis_index = Array.IndexOf(distinct_commands, next_command[2]);

                    //Get the expected reward, which is the current resource of next command
                    int time_of_completion = Convert.ToInt32(Math.Floor(Convert.ToDouble(next_command[0]) / 10) - 1);
                    string[] expected_reward = ((time_of_completion < macromanagement_relationship.Item6.Length) ? macromanagement_relationship.Item6[time_of_completion] : macromanagement_relationship.Item6.Last()).Split(',');

                    //Update the matrix with expected reward
                    commands_matrix[xaxis_index, yaxis_index][0] += 1; //Probability
                    commands_matrix[xaxis_index, yaxis_index][1] += Convert.ToDouble(expected_reward[2]); //Mineral
                    commands_matrix[xaxis_index, yaxis_index][1] += Convert.ToDouble(expected_reward[3]); //Vespene
                    commands_matrix[xaxis_index, yaxis_index][1] += Convert.ToDouble(expected_reward[4]); //Supply
                    commands_matrix[xaxis_index, yaxis_index][1] += Convert.ToDouble(expected_reward[5]); //Workers
                    commands_matrix[xaxis_index, yaxis_index][1] += expected_reward.Skip(6).Count(); //Upgrades
                }
            }

            return new Tuple<string[], double[,][]>(distinct_commands, commands_matrix);
        } 
        #endregion
    }
}