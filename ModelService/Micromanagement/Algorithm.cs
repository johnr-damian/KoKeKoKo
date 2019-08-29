using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T, C> where T : IEnumerable<C> where C : Types.Unit
    {
        public Tuple<T, T> LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<T, T> battle_result = null;
            
            try
            {
                if (typeof(T) == typeof(Types.CSVUnits))
                {
                    var csvbased_owned_units = (_owned_units as Types.CSVUnits);
                    var csvbased_enemy_units = (_enemy_units as Types.CSVUnits);


                }
                else
                {
                    var gamebased_owned_units = (_owned_units as Types.ObservedUnits);
                    var gamebased_enemy_units = (_enemy_units as Types.ObservedUnits);


                }
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
