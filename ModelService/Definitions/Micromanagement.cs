using System;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T>
    {
        public static class MicromanagementExtensions
        {
            private static TargetPolicy _allpolicy = TargetPolicy.Random | TargetPolicy.Priority | TargetPolicy.Resource;
            private static PredictionAlgorithm _allalgorithm = PredictionAlgorithm.Lanchester | PredictionAlgorithm.Static | PredictionAlgorithm.Dynamic;

            
        }
    }

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

    /// <summary>
    /// A list of available policy 
    /// </summary>
    [Flags]
    public enum PredictionAlgorithm
    {
        /// <summary>
        /// Predicts the battle using the Lanchester algorithm.
        /// </summary>
        Lanchester = 1,

        /// <summary>
        /// Predicts the battle using the Static algorithm
        /// </summary>
        Static = 2,

        /// <summary>
        /// Predicts the battle using the Dynamic algorithm
        /// </summary>
        Dynamic = 4
    }
}
