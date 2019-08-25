using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T, C> where T : IEnumerable<C>
    {
        public Tuple<T, T> LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<T, T> battle_result = null;

            try
            {
                
            }
            catch(Exception ex)
            {

            }

            return battle_result;
        }

        public Tuple<T, T> StaticBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<T, T> battle_result = null;

            try
            {
                //TODO
            }
            catch (Exception ex)
            {

            }

            return battle_result;
        }

        public Tuple<T, T> DynamicBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<T, T> battle_result = null;

            try
            {
                //TODO
            }
            catch (Exception ex)
            {

            }

            return battle_result;
        }
    }
}
