using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryService
{
    public class Program
    {
        private const string COMMANDSFILENAME = "CommandsRepository.csv", RESOURCESFILENAME = "ResourcesRepository.csv";
        private static readonly string currentdirectory = null, rawCommandsCSVFilename = null, rawResourcesCSVFilename = null;

        static Program()
        {
            currentdirectory = Directory.GetCurrentDirectory();
            rawCommandsCSVFilename = Path.Combine(currentdirectory, String.Concat("Raw", COMMANDSFILENAME));
            rawResourcesCSVFilename = Path.Combine(currentdirectory, String.Concat("Raw", RESOURCESFILENAME));
        }

        /// <summary>
        /// Reads the 
        /// </summary>
        /// <param name="args"></param>
        /// <returns>1 - Successful, 0 - Executed but did not do the purpose, -1 - Error</returns>
        static int Main(string[] args)
        {
            Console.WriteLine($@"Current Directory: {currentdirectory}");
            Console.WriteLine($@"Attempting to access {rawCommandsCSVFilename}...");
            if (!File.Exists(rawCommandsCSVFilename))
                return 0;
            //The raw commands csv file exists
            try
            {
                //Read the contents of the raw commands csv file
                var rawcommandscsv = File.ReadAllLines(rawCommandsCSVFilename);
                using (var filestream = new FileStream(Path.Combine(currentdirectory, COMMANDSFILENAME), FileMode.Create))
                {


                }
            }
            catch (Exception error)
            {
                Console.WriteLine($@"Error Occurred! Source: {error.Source} ~~ Message: {error.Message}");
                return -1;
            }

            if (!File.Exists(rawResourcesCSVFilename))
                return 0;
            //The raw resources csv file exists
            try
            {
                var rawresourcescsv = File.ReadAllLines(rawResourcesCSVFilename);
                using (var filestream = new FileStream(Path.Combine(currentdirectory, RESOURCESFILENAME), FileMode.Create))
                {

                }
            }
            catch(Exception error)
            {

            }

            return 1;
        }
    }
}
