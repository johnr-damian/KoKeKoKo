using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    class Program
    {
        static int Main(string[] args)
        {
            REngine.SetEnvironmentVariables();
            using (var engine = REngine.GetInstance())
            {
                return engine.Evaluate("sample(1:5, 1)").AsInteger()[0];
            }
        }
    }
}
