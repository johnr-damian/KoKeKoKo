using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        private Func<Types.Units, Types.Units, Tuple<Types.Units, Types.Units>> _algorithm = null;
        private Func<Types.Units, Types.Units, bool> _targetpolicy = null;

        /// <summary>
        /// Initializes the Micromanagement by recieving the target policy and algorithm to be use
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="target_policy"></param>
        public Micromanagement(Func<Types.Units, Types.Units, Tuple<Types.Units, Types.Units>> algorithm, Func<Types.Units, Types.Units, bool> target_policy)
        {
            _algorithm = algorithm;
            _targetpolicy = target_policy;
        }

        public static void PerformMicromanagementTest()
        {
            try
            {
                //Start predicting battles
                var lanchester_randompolicy = LanchesterBasedPrediction(null, null, RandomBasedTargetPolicy);
                var lanchester_prioritypolicy = LanchesterBasedPrediction(null, null, PriorityBasedTargetPolicy);
                var lanchester_resourcepolicy = LanchesterBasedPrediction(null, null, ResourceBasedTargetPolicy);

                var sustained_randompolicy = SustainedBasedPrediction(null, null, RandomBasedTargetPolicy);
                var sustained_prioritypolicy = SustainedBasedPrediction(null, null, PriorityBasedTargetPolicy);
                var sustained_resourcespolicy = SustainedBasedPrediction(null, null, ResourceBasedTargetPolicy);

                var decreasing_randompolicy = DecreasingBasedPrediction(null, null, RandomBasedTargetPolicy);
                var decreasing_prioritypolicy = DecreasingBasedPrediction(null, null, PriorityBasedTargetPolicy);
                var decreasing_resourcepolicy = DecreasingBasedPrediction(null, null, ResourceBasedTargetPolicy);

                //For every battles, compute the standard deviation
                //For every battle, compute the jaccard similarity
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to perform the micromanagement testing...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> PerformMicromanagementTest(): \n\t{ex.Message}");
            }
        }

        /// <summary>
        /// Given the list of units and each of its definition, it will simulate a battle
        /// using the initialized target policy and algorithm during instance of this class
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        /// <returns></returns>
        public Tuple<double, double, double, double> PredictSimulatedBattle(string owned_units, string enemy_units)
        {
            Tuple<double, double, double, double> result = null;

            try
            {
                var parsed_ownedunits = new Types.Units(owned_units, false);
                var parsed_enemyunits = new Types.Units(enemy_units, false);
                var simulated_result = _algorithm.Invoke(parsed_ownedunits, parsed_enemyunits);

                result = new Tuple<double, double, double, double>(parsed_ownedunits.GetPredictedValue().Item1, parsed_ownedunits.GetPredictedValue().Item2, parsed_enemyunits.GetPredictedValue().Item1, parsed_enemyunits.GetPredictedValue().Item2);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to predict the simulated battle...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> PredictSimulatedBattle(): \n\t{ex.Message}");
            }

            return result;
        }
    }
}
