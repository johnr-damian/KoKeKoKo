using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T> where T : Unit
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public string LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            string battle_result = "";

            try
            {
                Console.WriteLine("1");
                Console.WriteLine("1");
                Console.WriteLine("1");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to predict battle using Lanchester algorithm...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> LanchesterBasedPrediction(): \n\t{ex.Message}");

                battle_result = "";
            }

            return battle_result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public string StaticBasedPrediction(TargetPolicy target_policy)
        {
            string battle_result = "";

            try
            {
                //TODO
                Console.WriteLine("2");
                Console.WriteLine("2");
                Console.WriteLine("2");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to predict battle using Static algorithm...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> StaticBasedPrediction(): \n\t{ex.Message}");

                battle_result = "";
            }

            return battle_result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public string DynamicBasedPrediction(TargetPolicy target_policy)
        {
            string battle_result = "";

            try
            {
                //TODO
                Console.WriteLine("3");
                Console.WriteLine("3");
                Console.WriteLine("3");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to predict battle using Dynamic algorithm...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> DynamicBasedPrediction(): \n\t{ex.Message}");

                battle_result = "";
            }

            return battle_result;
        }
    }
}
