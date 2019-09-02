using System;

namespace ModelService.Micromanagement
{
    /// <summary>
    /// A list of available policy for targeting units
    /// </summary>
    [Flags]
    public enum TargetPolicy
    {
        /// <summary>
        /// Targets a unit from the list randomly
        /// </summary>
        Random = 1,

        /// <summary>
        /// Targets a unit based on priority. The unit with a low priority number is picked first
        /// </summary>
        Priority = 2,

        /// <summary>
        /// Targets a unit based on resource. The unit with a higher cost is picked first
        /// </summary>
        Resource = 4
    }
}
