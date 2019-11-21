using ModelService.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ModelService.Macromanagement
{
    /// <summary>
    /// Facilitates the simulation of Macromanagement part of the game.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Macromanagement<T> : IFormattable
        where T : SimulationNode
    {
        #region Properties
        /// <summary>
        /// Contains the source information either recieved from C++ Agent or from a CSV file.
        /// </summary>
        /// <remarks>
        /// From C++ Agent: Difficulty, ID of Bot, Micromanagement, Macromanagement, Micromanagement of Enemy.
        /// From CSV file: Rank, Replay, Macromangement of Player 1, Macromanagement of Player 2.
        /// </remarks>
        private string[][] Source { get; set; } = default(string[][]);

        /// <summary>
        /// The initial node created after <see cref="Macromanagement"/> has been created. It contains
        /// the initial <see cref="SimulatedAgent"/>, and other related properties.
        /// </summary>
        public SimulationNode Root { get; private set; } = default(SimulationNode);

        /// <summary>
        /// The current node after n-simulations. It contains the latest <see cref="SimulatedAgent"/>,
        /// and other related properties to continue the simulation.
        /// </summary>
        public SimulationNode Current { get; private set; } = default(SimulationNode);

        /// <summary>
        /// The rank or difficulty of the game.
        /// </summary>
        public string Rank
        {
            get { return Source[0][1]; }
        }
        
        /// <summary>
        /// The name of a replay or the name of the bot
        /// </summary>
        public string Name
        {
            get { return Source[0][1]; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the required properties with the information coming
        /// from a CSV file to start the simulations.
        /// </summary>
        /// <param name="source"></param>
        public Macromanagement(Tuple<string, string, string[], string[]> source)
        {
            //Initialize the information about the current Macromanagement
            Source = new string[3][];
            Source[0] = new string[2] { source.Item1, source.Item2 };
            Source[1] = source.Item3;
            Source[2] = source.Item4;

            //Initialize the nodes
            if (typeof(T) == typeof(MCTSNode))
                Root = new MCTSNode(Source[1].First().Split(',')[1], Source[2].First().Split(',')[1]);
            else
                Root = new POMDPNode(Source[1].First().Split(',')[1], Source[2].First().Split(',')[1]);
            Current = Root;
        }

        /// <summary>
        /// Initializes the required properties with the information coming from
        /// C++ Agent's message to start the simulations.
        /// </summary>
        /// <param name="source"></param>
        public Macromanagement(IEnumerable<string> source)
        {
            //Initialize the information about the current Macromanagement
            Source = new string[4][];
            Source[0] = source.Take(2).ToArray();
            Source[1] = source.Skip(2).Single().Split(',');
            Source[2] = Enumerable.Empty<string>().ToArray();
            Source[3] = Enumerable.Empty<string>().ToArray();

            //Initialize the nodes
            if (typeof(T) == typeof(MCTSNode))
                Root = new MCTSNode(Source[0][1], Source[1]);
            else
                Root = new POMDPNode(Source[0][1], Source[1]);
            Current = Root;
        } 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<string> PredictSequenceOfActions()
        {
            var test = Current.SelectPhase();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<string> CreateAccuracyReport()
        {

        }

        /// <summary>
        /// Updates the Macromanagement by updating the current node.
        /// </summary>
        /// <param name="source"></param>
        public void UpdateMacromanagement(IEnumerable<string> source) => Current.UpdateSimulationNode(source);

        #region ToString Methods
        /// <summary>
        /// Returns a sequence of <see cref="SimulatedAgent.Action"/> for messaging C++ Agent.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString("M", CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a sequence of specific information about <see cref="SimulationNode"/> for other purposes.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a sequence of specific information about <see cref="SimulationNode"/> with
        /// a specific format provider for other purposes.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format.ToUpperInvariant())
            {
                //The requested string is for messaging C++ Agent
                case "M":
                    return String.Join(",", PredictSequenceOfActions());
                //The requested string is for accuracy report for R
                case "R":
                    //Sequence of basis
                    var basis = String.Join(",", String.Join("\n", Source[1]), String.Join("\n", Source[2]));

                    //Sequence of simulation
                    var simulation = String.Join("\n", CreateAccuracyReport());

                    return String.Join(";", basis, simulation);
                default:
                    throw new Exception($@"Failed to format into string...");
            }
        } 
        #endregion
    }
}