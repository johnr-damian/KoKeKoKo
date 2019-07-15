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
            if (arguments.Length != 2)
                return -1;

            var firstargument = File.GetAttributes(arguments[0]);
            var secondargument = File.GetAttributes(arguments[0]);

            switch(firstargument)
            {
                //A large number of files will be read as input
                //Typically, csv files will be read for training data
                //Used to create training data
                case FileAttributes.Directory:
                    var csv = Directory.GetFiles(arguments[0], "*.csv", SearchOption.AllDirectories);

                    break;
                case FileAttributes.Normal:

                    break;
                default:

                    break;
            }

            return 0;
        }
    }
}
