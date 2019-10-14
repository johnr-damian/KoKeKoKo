namespace ModelService.Types
{
    /// <summary>
    /// Holds the cost of a <see cref="Unit"/>. It also
    /// represents the worth of a <see cref="Unit"/>, and the
    /// worth of a <see cref="Player"/>
    /// </summary>
    public struct CostWorth
    {
        /// <summary>
        /// The priority to be targeted by a <see cref="Unit"/>. This
        /// also represents the priority worth when the <see cref="Unit"/>
        /// is destroyed, or the current total priority of a <see cref="Player"/>
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// The mineral cost to create the <see cref="Unit"/>. This
        /// also represents the mineral worth when the <see cref="Unit"/>
        /// is destroyed, or the current mineral resource of a <see cref="Player"/>
        /// </summary>
        public double Mineral { get; private set; }

        /// <summary>
        /// The vespene cost to create the <see cref="Unit"/>. This 
        /// also represents the vespene worth when the <see cref="Unit"/>
        /// is destroyed, or the current vespene resource of a <see cref="Player"/>
        /// </summary>
        public double Vespene { get; private set; }

        /// <summary>
        /// The supply cost to create the <see cref="Unit"/>. This
        /// also represents the supply worth when the <see cref="Unit"/>
        /// is destroyed, or the current total supply consumed by a <see cref="Player"/>
        /// </summary>
        public int Supply { get; private set; }

        /// <summary>
        /// The cost to create the <see cref="Unit"/>, or the current
        /// resources of a <see cref="Player"/>
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

        public static CostWorth operator+ (CostWorth a, CostWorth b)
        {
            return new CostWorth(a.Priority + b.Priority, a.Mineral + b.Mineral, a.Vespene + b.Vespene, a.Supply + b.Supply);
        }

        /// <summary>
        /// Gets the complement value of a <see cref="CostWorth"/>. It 
        /// returns all properties in negative.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CostWorth GetComplementOfCostWorth(CostWorth value) => new CostWorth(-value.Priority, -value.Mineral, -value.Vespene, -value.Supply);

        /// <summary>
        /// It returns the sum of all properties with equal weights.
        /// </summary>
        /// <remarks>
        /// Worth = (25% Priority Worth) + (25% Mineral Worth) + (25% Vespene Worth) + (25% Supply Worth)
        /// </remarks>
        /// <returns></returns>
        public double GetTotalWorth() => ((Priority * .25) + (Mineral * .25) + (Vespene * .25) + (Supply * .25));

        /// <summary>
        /// It returns the sum of all properties with the supplied weights. If the 
        /// weight is 0, then that property is not considered.
        /// </summary>
        /// <remarks>
        /// Worth = (a * Priority Worth) + (b * Mineral Worth) + (c * Vespene Worth) + (d * Supply Worth)
        /// </remarks>
        /// <param name="priority_weight"></param>
        /// <param name="mineral_weight"></param>
        /// <param name="vespene_weight"></param>
        /// <param name="supply_weight"></param>
        /// <returns></returns>
        public double GetTotalWorth(double priority_weight, double mineral_weight, double vespene_weight, double supply_weight) => ((Priority * priority_weight) + (Mineral * mineral_weight) + (Vespene * vespene_weight) + (Supply * supply_weight));
    }
}
