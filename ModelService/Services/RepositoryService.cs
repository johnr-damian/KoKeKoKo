using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string, string, string>> ParseArmiesRepository()
        {
            var raw_repository = File.ReadAllLines(Path.Combine(CurrentDirectory, "ArmiesRepository.csv"));

            if (raw_repository.Length > 0)
            {
                bool prebattle_storable = false, postbattle_storable = false;
                int current_pointer = -1, prebattle_pointer = -1, postbattle_pointer = -1;
                string current_rank = "", current_replay = "", playerone_content = "", playertwo_content = "";

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
                                playerone_content = String.Join("\n", armies_content[0].Value);
                                playertwo_content = String.Join("\n", armies_content[1].Value);

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
                                yield return (new Tuple<string, string, string, string, string>(current_rank, current_replay, playerone_content, playertwo_content, String.Join("\n", postbattle_content)));
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
                    yield return (new Tuple<string, string, string, string, string>(current_rank, current_replay, playerone_content, playertwo_content, String.Join("\n", postbattle_content)));
                }
            }
            else
                throw new Exception($@"Failed to parse {Path.Combine(CurrentDirectory, "ArmiesRepository.csv")}...");
        }

        /// <summary>
        /// Parses the CommandsRepository and returns a list of macromanagement commands. A macromangement command contains the
        /// following contents: Rank, Replay, Commands Player1, Commands Player2.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string, string>> ParseCommandsRepository()
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
                                yield return (new Tuple<string, string, string, string>(current_rank, current_replay, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
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
                    yield return (new Tuple<string, string, string, string>(current_rank, current_replay, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
                }
            }
            else
                throw new Exception($@"Failed to parse {Path.Combine(CurrentDirectory, "CommandsRepository.csv")}");
        }

        /// <summary>
        /// Parses the ResourcesRepository and returns a list of macromanagement resources. A macromanagement resources contains the
        /// following contents: Rank, Replay, Resources Player1, Resources Player2.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string, string>> ParseResourcesRepository()
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
                                yield return (new Tuple<string, string, string, string>(current_rank, current_replay, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
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
                    yield return (new Tuple<string, string, string, string>(current_rank, current_replay, String.Join("\n", content[0].Value), String.Join("\n", content[1].Value)));
                }
            }
            else
                throw new Exception($@"Failed to parse {Path.Combine(CurrentDirectory, "ResourcesRepository.csv")}");
        } 
        #endregion


        public IEnumerable<Tuple<string, string, string, string, string>> GetMicromanagementRepository()
        {
            yield return null;
        }
    }
}