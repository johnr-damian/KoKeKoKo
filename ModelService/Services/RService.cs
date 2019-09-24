using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    /// <summary>
    /// This file contains related fields and methods about <see cref="REngine"/>
    /// </summary>
    public partial class ModelRepositoryService
    {
        private static REngine _engine;

        /// <summary>
        /// Returns the instance of the <see cref="REngine"/>
        /// </summary>
        /// <returns></returns>
        public static REngine GetREngine()
        {
            try
            {
                if(_engine == null)
                {
                    //Ready the environment variables
                    REngine.SetEnvironmentVariables();

                    //Get the instance of the engine, and initialize it
                    _engine = REngine.GetInstance();
                    _engine.Initialize();

                    //Prepare the packages
                    //Sets Package
                    _engine.Evaluate(@"if(""sets"" %in% rownames(installed.packages()) == FALSE) { install.packages(""sets"") }");
                    _engine.Evaluate(@"library(""sets"")");
                    //POMDP Package
                    //... TODO
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"GetREngine() -> {ex.Message}...");
                _engine = null;
            }

            return _engine;
        }
    }
}
