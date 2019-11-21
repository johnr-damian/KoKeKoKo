using ModelService.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public class SimulatedAgent : IActor<SimulatedAgent>, IFormattable
    {
        #region Properties
        /// <summary>
        /// The current resources of the agent.
        /// </summary>
        private Worth Resources { get; set; } = default(Worth);

        /// <summary>
        /// Checks if there are still <see cref="Units"/> to be controlled.
        /// </summary>
        public bool IsDefeated
        {
            get { return (Units.Count() <= 0); }
        }

        /// <summary>
        /// The current value of the agent. It is also the current
        /// <see cref="Resources"/> of the agent.
        /// </summary>
        public double[] Value
        {
            get { return ((double[])Resources); }
        }

        /// <summary>
        /// The name of the current agent.
        /// </summary>
        public string Name { get; private set; } = default(string);

        /// <summary>
        /// The chosen action that was applied in <see cref="ApplyChosenAction(string)"/>.
        /// </summary>
        public string Action { get; private set; } = default(string);

        /// <summary>
        /// The current owned units of the agent.
        /// </summary>
        public SimulatedUnits Units { get; private set; } = default(SimulatedUnits);
        #endregion

        /// <summary>
        /// A struct that contains the current resources of an <see cref="SimulatedAgent"/>.
        /// </summary>
        private struct Worth
        {
            #region Properties
            /// <summary>
            /// The current gathered minerals of <see cref="SimulatedAgent"/>.
            /// </summary>
            public double Mineral { get; private set; }

            /// <summary>
            /// The current gathered vespene of <see cref="SimulatedAgent"/>.
            /// </summary>
            public double Vespene { get; private set; }

            /// <summary>
            /// The current consumed supply of <see cref="SimulatedAgent"/>.
            /// </summary>
            public int Supply { get; private set; }

            /// <summary>
            /// The current number of controlled workers of <see cref="SimulatedAgent"/>.
            /// </summary>
            public int Workers { get; private set; }

            /// <summary>
            /// The current number of researched upgrades of <see cref="SimulatedAgent"/>.
            /// </summary>
            public int Upgrades { get; private set; }
            #endregion

            #region Operators
            /// <summary>
            /// Returns the properties of <see cref="Worth"/> into negative equivalent.
            /// </summary>
            /// <param name="worth"></param>
            /// <returns></returns>
            public static Worth operator !(Worth worth) => new Worth(-worth.Mineral, -worth.Vespene, -worth.Supply, -worth.Workers, -worth.Upgrades);

            /// <summary>
            /// Returns the sum of two <see cref="Worth"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Worth operator +(Worth a, Worth b) => new Worth((a.Mineral + b.Mineral), (a.Vespene + b.Vespene), (a.Supply + b.Supply), (a.Workers + b.Workers), (a.Upgrades + b.Upgrades));

            /// <summary>
            /// Returns the difference of two <see cref="Worth"/>.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Worth operator -(Worth a, Worth b) => new Worth((a.Mineral - b.Mineral), (a.Vespene - b.Vespene), (a.Supply - b.Supply), (a.Workers - b.Workers), (a.Upgrades - b.Upgrades));

            /// <summary>
            /// Returns a 5-Length double array equivalent of <see cref="Worth"/>.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator double[] (Worth worth) => new double[] { worth.Mineral, worth.Vespene, worth.Supply, worth.Workers, worth.Upgrades };

            /// <summary>
            /// Returns a 5-Tuple equivalent of <see cref="Worth"/>.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator Tuple<double, double, int, int, int>(Worth worth) => new Tuple<double, double, int, int, int>(worth.Mineral, worth.Vespene, worth.Supply, worth.Workers, worth.Upgrades);

            /// <summary>
            /// Returns a <see cref="Worth"/> from a 5-Length double array.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator Worth(double[] worth) => new Worth(worth[0], worth[1], Convert.ToInt32(worth[2]), Convert.ToInt32(worth[3]), Convert.ToInt32(worth[4]));

            /// <summary>
            /// Returns a <see cref="Worth"/> from a 5-Tuple.
            /// </summary>
            /// <param name="worth"></param>
            public static explicit operator Worth(Tuple<double, double, int, int, int> worth) => new Worth(worth.Item1, worth.Item2, Convert.ToInt32(worth.Item3), Convert.ToInt32(worth.Item4), Convert.ToInt32(worth.Item5)); 
            #endregion

            /// <summary>
            /// Initializes the required properties with the value of current resources.
            /// </summary>
            /// <param name="mineral"></param>
            /// <param name="vespene"></param>
            /// <param name="supply"></param>
            /// <param name="workers"></param>
            /// <param name="upgrades"></param>
            public Worth(double mineral, double vespene, int supply, int workers, int upgrades)
            {
                Mineral = mineral;
                Vespene = vespene;
                Supply = supply;
                Workers = workers;
                Upgrades = upgrades;
            }
        }

        /// <summary>
        /// Initializes the required properties to simulate the agent with
        /// values from a parent agent. This constructor is used for consequent
        /// agents in which it is a copy from a parent agent.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="units"></param>
        /// <param name="resources"></param>
        private SimulatedAgent(string name, SimulatedUnits units, Worth resources)
        {
            Name = name;
            Action = String.Empty;
            Units = units;
            Resources = resources;
        }

        public SimulatedAgent()
        {

        }

        /// <summary>
        /// Initializes the required properties to simulate the agent.
        /// This constructor is used for creating a starting agent in which a
        /// starting agent only consist of starting units and resources.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="units"></param>
        public SimulatedAgent(string name, IEnumerable<string> units)
        {
            Name = name;
            Action = String.Empty;

            //Initialize the starting units
            Units = new SimulatedUnits(units);

            //Initialize the starting resources
            Resources = new Worth(50, 0, 15, 12, 0);
        }
        
        /// <summary>
        /// Updates the necessary properties of <see cref="SimulatedAgent"/> when
        /// the chosen action from <see cref="GeneratePotentialActions"/> is applied.
        /// </summary>
        /// <param name="chosen_action"></param>
        public void ApplyChosenAction(string chosen_action)
        {
            switch(chosen_action)
            {

            }
        }

        public SimulatedAgent Copy() => new SimulatedAgent(Name, Units.Copy(), Resources);

        public IEnumerable<string> GeneratePotentialActions()
        {
            foreach(var unit in Units)
            {
                yield return "";
            }
        }

        #region ToString Methods
        /// <summary>
        /// Returns the <see cref="Action"/> of the agent that is used in messaging C++ Agent.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString("M", CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a specific information about the agent.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns a specific information about the agent using a specific format provider.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format.ToUpperInvariant())
            {
                case "M":
                    return "";
                default:
                    throw new Exception($@"Failed to format into string...");
            }
        } 
        #endregion
    }
}
