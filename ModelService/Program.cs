using RDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    class Program
    {
        static int Main(string[] args)
        {
            //using (var server = new NamedPipeServerStream(@"\\\\.\\pipe\\Pipe", PipeDirection.InOut))
            //{
            //    server.WaitForConnection();

            //    try
            //    {
            //        using (var sw = new StreamWriter(server))
            //        {
            //            sw.AutoFlush = true;
            //            Console.WriteLine("Write: ");
            //            sw.WriteLine(Console.ReadLine());
            //        }
            //    }
            //    catch
            //    {

            //    }
            //}
            string i = "";
            Console.WriteLine("Hello World!1");
            Console.WriteLine("Hello World!2");
            Console.WriteLine("Hello World!3");
            Console.WriteLine("Hello World!4");
            Console.WriteLine("Hello World!5");
            Console.WriteLine("Hello World!6");
            Console.WriteLine("Write: ");
            i = Console.ReadLine();
            Console.WriteLine(i);
            using (var client = new NamedPipeClientStream(@"Kokekoko"))
            {
                client.Connect();
                var r = new StreamReader(client);
                var w = new StreamWriter(client);

                while(i != "exit")
                {
                    Console.Write("Write to Parent: ");
                    i = Console.ReadLine();
                    w.WriteLine(i);
                    //Console.Write($"Reply of Parent: {r.ReadLine()}");
                }
                w.Flush();
                i = r.ReadLine();
                Console.WriteLine(i);
                while(i != "exit")
                {
                    i = r.ReadLine();
                    Console.WriteLine(i);
                }
                w.Flush();
            }

                REngine.SetEnvironmentVariables();
            using (var engine = REngine.GetInstance())
            {
                return engine.Evaluate("sample(1:5, 1)").AsInteger()[0];
            }
        }
    }
}
