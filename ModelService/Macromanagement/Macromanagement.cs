using ModelService.Collections;
using System;
using System.Collections.Generic;

namespace ModelService.Macromanagement
{
    public class Macromanagement<T> where T : SimulationNode<T>
    {
        #region Properties
        /// <summary>
        /// Contains the source information recieved from C++ Agent. But if C# Model
        /// has ran as standalone mode, it contains the source information from a CSV file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Model mode, it contains as follows: Difficulty of Enemy / Rank of Enemy, Game Name, 
        /// Macromanagement details of C++ Agent, Micromanagement details of C++ Agent, Macromanagement details
        /// of Enemy, and lastly, Micromanagement details of Enemy.
        /// 
        /// Game Name: Difficulty + PlayerID
        /// 
        /// Macromanagement details: Mineral, Vespene, Supply, Number of Workers, Number of Upgrades, Upgrades.
        /// 
        /// Micromanagement details: UniqueID of Unit, Unit name
        /// </para>
        /// 
        /// <para>
        /// In Standalone mode, it contains as follows: Rank of Game, Replay name, Macromanagement of Player one,
        /// Macromanagement of Player 2.
        /// </para>
        /// </remarks>
        private string[][] Source { get; set; } = default(string[][]);

        /// <summary>
        /// The initial node created after <see cref="Macromanagement"/> has been created.
        /// It contains the initial <see cref="SimulatedAgent"/>, and its related properties.
        /// </summary>
        public SimulationNode<T> Root { get; private set; } = default(SimulationNode);

        /// <summary>
        /// The current node after n-simulations. It contains the latest <see cref="SimulatedAgent"/>,
        /// and its related properties to continue further the simulations.
        /// </summary>
        public SimulationNode<T> Current { get; private set; } = default(SimulationNode);

        /// <summary>
        /// The difficulty of the current gameplay.
        /// </summary>
        public string Rank
        {
            get { return Source[0][0]; }
        }

        /// <summary>
        /// The name of the current gameplay.
        /// </summary>
        public string Name
        {
            get { return Source[0][1]; }
        }
        #endregion

        public Macromanagement(string difficulty, string[] macromanagement, string[] micromanagement)
        {
            
        }

        /// <summary>
        /// Initializes the required properties with the information from a CSV file to
        /// start simulating the game.
        /// </summary>
        /// <param name="source"></param>
        public Macromanagement(Tuple<string, string, string[], string[]> source)
        {
            //Initialize the metadata of Macromanagement
            Source = new string[3][];
            Source[0] = new string[] { source.Item1, source.Item2 }; //
            Source[1] = source.Item3;
            Source[2] = source.Item4;

            //Create the Root Node
            Root = ((typeof(T) == typeof(MCTSNode)) ? new MCTSNode() : (SimulationNode)new POMDPNode());
        }

        public IEnumerable<string> GetPredictedAction(IEnumerable<string> update_message)
        {
            
        }

        public IEnumerable<Tuple<string, string, double[]>> GetPredictedAction()
        {

        }
    }
}