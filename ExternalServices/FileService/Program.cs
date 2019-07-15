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

            Console.WriteLine(arguments[0]);
            Console.ReadLine();

            return 0;
        }
    }
}
