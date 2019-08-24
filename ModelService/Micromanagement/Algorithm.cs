using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public static Tuple<Types.Units, Types.Units> LanchesterBasedPrediction(Types.Units owned_units, Types.Units enemy_units, Func<Types.Units, Types.Units, bool> target_policy)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        /// <returns></returns>
        public Tuple<Types.Units, Types.Units> LanchesterBasedPrediction(Types.Units owned_units, Types.Units enemy_units)
        {
            try
            {
                
            }
            catch(Exception ex)
            {

            }
        }

        public static Tuple<Types.Units, Types.Units> SustainedBasedPrediction(Types.Units owned_units, Types.Units enemy_units, Func<Types.Units, Types.Units, bool> target_policy)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }

        public Tuple<Types.Units, Types.Units> SustainedBasedPrediction(Types.Units owned_units, Types.Units enemy_units)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }

        public static Tuple<Types.Units, Types.Units> DecreasingBasedPrediction(Types.Units owned_units, Types.Units enemy_units, Func<Types.Units, Types.Units, bool> target_policy)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }

        public Tuple<Types.Units, Types.Units> DecreasingBasedPrediction(Types.Units owned_units, Types.Units enemy_units)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }
    }
}
