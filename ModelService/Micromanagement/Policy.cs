using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T, C> where T : IEnumerable<C>
    {
        private bool RandomBasedTargetPolicy(T owned_units, T enemy_units)
        {
            bool has_targetable = false;

            try
            {
                if(typeof(T) == typeof(Types.CSVUnits))
                {
                    var csvbased_owned_units = (owned_units as Types.CSVUnits);
                    var csvbased_enemy_units = (enemy_units as Types.CSVUnits);


                }
                else
                {

                }
            }
            catch(Exception ex)
            {

            }

            return has_targetable;
        }

        private bool PriorityBasedTargetPolicy()
        {
            bool has_targetable = false;

            try
            {
                if (typeof(T) == typeof(Types.CSVUnits))
                {

                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }

            return has_targetable;
        }

        private bool ResourceBasedTargetPolicy()
        {
            bool has_targetable = false;

            try
            {
                if (typeof(T) == typeof(Types.CSVUnits))
                {

                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }

            return has_targetable;
        }

        private bool FirstTargetBasedTargetPolicy()
        {
            throw new NotImplementedException();
        }

        private bool FirstDeathBasedTargetPolicy()
        {
            throw new NotImplementedException();
        }
    }
}
