using Microsoft.R.Host.Client;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    class Program
    {
        static int Main(string[] args)
        {
            int retvalue = 0;
            //SetupPath();
            //RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core\R");
            //string rPath = (string)registryKey.GetValue("InstallPath");
            //string rVersion = (string)registryKey.GetValue("Current Version");
            //registryKey.Dispose();
            ////REngine.SetEnvironmentVariables(@"C:\Program Files\Microsoft\R Client\R_SERVER\bin\x64\R.dll");
            //var engine = REngine.GetInstance();
            //var value = engine.Evaluate("1 + 1").AsInteger();
            //retvalue = value[0];
            //engine.Dispose();

            //Console.WriteLine(retvalue);
            //Console.ReadLine();

            //return retvalue;
            //REngine.SetEnvironmentVariables(@"C:\Program Files\Microsoft\R Client\R_SERVER\bin");
            //using (var engine = REngine.GetInstance())
            //{
            //    engine.Initialize();
            //    CharacterVector charVec = engine.CreateCharacterVector(new[] {
            //         "Hello, R world!, .NET speaking" });
            //    engine.SetSymbol("greetings", charVec);
            //    engine.Evaluate("str(greetings)"); // print out in the console
            //    string[] a = engine.Evaluate(@"'Hi there .NET, from the R 
            //                              engine'").AsCharacter().ToArray();
            //    Console.WriteLine("R answered: '{0}'", a[0]);
            //    Console.WriteLine("Press any key to exit the program");
            //    Console.ReadKey();
            //}
            IRHostSession session = RHostSession.Create("Test");
            var task = session.StartHostAsync(new RHostSessionCallback());
            task.Wait();

            Console.WriteLine("Arbitrary R code:");
            var result = session.ExecuteAndOutputAsync("Sys.info()");
            result.Wait();
            Console.WriteLine(result.Result.Output);

            return 0;
        }

        public static void SetupPath(string Rversion = "R-3.0.0")
        {
            var oldPath = System.Environment.GetEnvironmentVariable("PATH");
            var rPath = System.Environment.Is64BitProcess ?
                                   string.Format(@"C:\Program Files\R\{0}\bin\x64", Rversion) :
                                   string.Format(@"C:\Program Files\R\{0}\bin\i386", Rversion);

            if (!Directory.Exists(rPath))
                throw new DirectoryNotFoundException(
                  string.Format(" R.dll not found in : {0}", rPath));
            var newPath = string.Format("{0}{1}{2}", rPath,
                                         System.IO.Path.PathSeparator, oldPath);
            System.Environment.SetEnvironmentVariable("PATH", newPath);
        }
    }
}
