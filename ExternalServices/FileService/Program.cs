using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService
{
    public class Program
    {

        static int Main(params string[] arguments)
        {
            if (arguments.Length != 3)
                return -1;

            try
            {
                var firstargument = File.GetAttributes(arguments[1]);
                var secondargument = File.GetAttributes(arguments[2]);

                switch (firstargument)
                {
                    //A large number of files will be read as input
                    //Typically, csv files will be read for training data
                    //Used to create training data
                    case FileAttributes.Directory:
                        var csv = Directory.GetFiles(arguments[1], "*.csv", SearchOption.AllDirectories);

                        break;
                    case FileAttributes.Normal:

                        break;
                    default:

                        break;
                }
            }
            catch (Exception)
            {
                foreach (string s in arguments)
                    Console.WriteLine(s);
            }

            return 0;
        }
    }
}
