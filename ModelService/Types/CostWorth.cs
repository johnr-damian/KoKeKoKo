using System;

namespace ModelService.ValueTypes
{
    /// <summary>
    /// Holds the cost or worth of <see cref="Types.Unit"/>. It also
    /// represents as the current total worth of a <see cref="Types.Agent"/>
    /// </summary>
    public struct CostWorth
    {
        #region Properties
        /// <summary>
        /// <para>
        ///     For <see cref="Micromanagement.Micromanagement"/>, it holds the worth of a <see cref="Types.Unit"/>
        ///     to be destroyed during a battle.
        /// </para>
        /// <para>
        ///     For <see cref="Macromanagement.Macromanagement"/>, it holds the total worth of <see cref="Types.Unit"/>
        ///     that is controlled by the <see cref="Types.Agent"/>.
        /// </para>
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// <para>
        ///     For <see cref="Micromanagement.Micromanagement"/>, it holds the worth of a <see cref="Types.Unit"/>
        ///     to be destroyed during a battle. It also represents as the cost to create this certain unit.
        /// </para>
        /// <para>
        ///     For <see cref="Macromanagement.Macromanagement"/>, it holds the total worth of <see cref="Types.Unit"/>
        ///     and current gathered minerals from a mineral patch that is owned by the <see cref="Types.Agent"/>
        /// </para>
        /// </summary>
        public double Mineral { get; set; }

        /// <summary>
        /// <para>
        ///     For <see cref="Micromanagement.Micromanagement"/>, it holds the worth of a <see cref="Types.Unit"/>
        ///     to be destroyed during a battle. It also represents as the cost to create this certain unit.
        /// </para>
        /// <para>
        ///     For <see cref="Macromanagement.Macromanagement"/>, it holds the total worth of <see cref="Types.Unit"/>
        ///     and current gathered vespene from a vespene geyser that is owned by the <see cref="Types.Agent"/>
        /// </para>
        /// </summary>
        public double Vespene { get; set; }

        /// <summary>
        /// <para>
        ///     For <see cref="Micromanagement.Micromanagement"/>, it holds the worth of a <see cref="Types.Unit"/>
        ///     to be destroyed during a battle. It also represents as the consumed space to create this certain unit.
        /// </para>
        /// <para>
        ///     For <see cref="Macromanagement.Macromanagement"/>, it holds the total consumed supply by all
        ///     units that is controlled by the <see cref="Types.Agent"/>.
        /// </para>
        /// </summary>
        public int Supply { get; set; } 
        #endregion

        #region Operators
        /// <summary>
        /// Returns a new <see cref="CostWorth"/> with negative properties
        /// </summary>
        /// <param name="costworth"></param>
        /// <returns></returns>
        public static CostWorth operator !(CostWorth costworth) => new CostWorth((-costworth.Priority), (-costworth.Mineral), (-costworth.Vespene), (-costworth.Supply));

        /// <summary>
        /// Returns a new <see cref="CostWorth"/> with the properties added together
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static CostWorth operator +(CostWorth a, CostWorth b) => new CostWorth((a.Priority + b.Priority), (a.Mineral + b.Mineral), (a.Vespene + b.Vespene), (a.Supply + b.Supply));

        /// <summary>
        /// Returns a new <see cref="CostWorth"/> with properties subtracted by <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static CostWorth operator -(CostWorth a, CostWorth b) => new CostWorth((a.Priority - b.Priority), (a.Mineral - b.Mineral), (a.Vespene - b.Vespene), (a.Supply - b.Supply));

        /// <summary>
        /// Returns a new <see cref="CostWorth"/> with properties multiplied by <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static CostWorth operator *(CostWorth a, CostWorth b) => new CostWorth((a.Priority * b.Priority), (a.Mineral * b.Mineral), (a.Vespene * b.Vespene), (a.Supply * b.Supply));

        /// <summary>
        /// Returns a new <see cref="CostWorth"/> with properties divided by <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static CostWorth operator /(CostWorth a, CostWorth b) => new CostWorth((a.Priority / b.Priority), (a.Mineral / b.Mineral), (a.Vespene / b.Vespene), (a.Supply / b.Supply));

        /// <summary>
        /// Converts the current <see cref="CostWorth"/> by calling <see cref="GetTotalWorth()"/>
        /// </summary>
        /// <param name="costworth"></param>
        public static implicit operator double(CostWorth costworth) => costworth.GetTotalWorth();

        /// <summary>
        /// Converts the current <see cref="CostWorth"/> to <see cref="Tuple{T1, T2, T3, T4}"/> with the same
        /// order as the constructor of <see cref="CostWorth"/>
        /// </summary>
        /// <param name="costworth"></param>
        public static implicit operator Tuple<int, double, double, int>(CostWorth costworth) => new Tuple<int, double, double, int>(costworth.Priority, costworth.Mineral, costworth.Vespene, costworth.Supply);

        /// <summary>
        /// Converts the current <see cref="CostWorth"/> to <see cref="Tuple{T1, T2, T3, T4}"/> with the 
        /// same order as the constructor of <see cref="CostWorth"/> with the properties converted to string
        /// </summary>
        /// <param name="costworth"></param>
        public static implicit operator Tuple<string, string, string, string>(CostWorth costworth) => new Tuple<string, string, string, string>(Convert.ToString(costworth.Priority), Convert.ToString(costworth.Mineral), Convert.ToString(costworth.Vespene), Convert.ToString(costworth.Supply));

        /// <summary>
        /// Converts the current <see cref="CostWorth"/> by calling then converting <see cref="GetTotalWorth()"/> to int
        /// using <see cref="Convert.ToInt32(double)"/>
        /// </summary>
        /// <param name="costworth"></param>
        public static explicit operator int(CostWorth costworth) => Convert.ToInt32(costworth.GetTotalWorth());

        /// <summary>
        /// Converts the current <see cref="Tuple{T1, T2, T3, T4}"/> into a <see cref="CostWorth"/>.
        /// The values are taken orderly, as such it follows the constructor of <see cref="CostWorth"/> where
        /// <see cref="Tuple{T1, T2, T3, T4}.Item1"/> is <see cref="Priority"/> and as follows.
        /// </summary>
        /// <param name="tuple"></param>
        public static explicit operator CostWorth(Tuple<int, double, double, int> tuple) => new CostWorth(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);

        /// <summary>
        /// Converts the current <see cref="Tuple{T1, T2, T3, T4}"/> into a <see cref="CostWorth"/>.
        /// The values are taken orderly, as such it follows the constructor of <see cref="CostWorth"/> where
        /// <see cref="Tuple{T1, T2, T3, T4}.Item1"/> is <see cref="Priority"/> then converted to its resepective type, and as follows.
        /// </summary>
        /// <param name="tuple"></param>
        public static explicit operator CostWorth(Tuple<string, string, string, string> tuple) => new CostWorth(Convert.ToInt32(tuple.Item1), Convert.ToDouble(tuple.Item2), Convert.ToDouble(tuple.Item3), Convert.ToInt32(tuple.Item4));
        #endregion

        /// <summary>
        /// Initializes <see cref="Priority"/>, <see cref="Mineral"/>, <see cref="Vespene"/>, 
        /// and <see cref="Supply"/>.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="mineral"></param>
        /// <param name="vespene"></param>
        /// <param name="supply"></param>
        public CostWorth(int priority, double mineral, double vespene, int supply)
        {
            Priority = priority;
            Mineral = mineral;
            Vespene = vespene;
            Supply = supply;
        }

        /// <summary>
        /// It returns the sum of all properties with equal weights
        /// </summary>
        /// <returns></returns>
        public double GetTotalWorth() => GetTotalWorth(0.25, 0.25, 0.25, 0.25);

        /// <summary>
        /// Returns the sum of all properties multiplied by their respective supplied weights. 
        /// To not consider a property, supply its respective weight with a value of 0.
        /// </summary>
        /// <param name="priority_weight"></param>
        /// <param name="mineral_weight"></param>
        /// <param name="vespene_weight"></param>
        /// <param name="supply_weight"></param>
        /// <returns></returns>
        public double GetTotalWorth(double priority_weight, double mineral_weight, double vespene_weight, double supply_weight) => ((Priority * priority_weight) + (Mineral * mineral_weight) + (Vespene * vespene_weight) + (Supply * supply_weight));
    }
}