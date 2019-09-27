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
        private string[] ReadRepository(string repository_name)
        {
            string[] content = null;
            string path = Path.GetFullPath(@"..\..\..\Documents");

            try
            {
                content = File.ReadAllLines(Path.Combine(path, repository_name));
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ReadRepository()->{ex.Message}...");
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
        public List<Tuple<string, string, string, string, string>> ReadArmyRepository()
        {
            var armyrepository = new List<Tuple<string, string, string, string, string>>();

            try
            {
#if DEBUG
                var raw_armyrepository = ReadRepository(@"Debugging\ArmiesRepository.csv");
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
                Console.WriteLine($@"ReadArmyRepository()->{ex.Message}...");
                armyrepository.Clear();
            }

            return armyrepository;
        }
    }
}
