using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public static partial class Micromanagement
    {
        /// <summary>
        /// For during game
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> PredictSimulatedBattle(Func<string, string, List<Tuple<string, string>>> target_policy)
        {
            var predicted_result = new Tuple<string, string, string>(null, null, null);

            try
            {

            }
            catch(Exception ex)
            {

            }

            return predicted_result;
        }

        /// <summary>
        /// For Model training and testing
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<string, string, string>> PredictSimulatedBattle()
        {
            var predicted_results = new List<Tuple<string, string, string>>();

            try
            {

            }
            catch(Exception ex)
            {
                
            }

            return predicted_results;
        }
    }
}
