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
        private bool RandomBasedTargetPolicy(Army owned_units, Army enemy_units)
        {
            var random = new Random();
            bool has_targetable = false;

            try
            {
                var array = enemy_units.ToArray();
                for (var enumerator = owned_units.GetEnumerator(); enumerator.MoveNext();)
                    enumerator.Current.SetTarget(array[random.Next(0, enemy_units.Count())]);

                var array2 = owned_units.ToArray();
                for (var enumerator = enemy_units.GetEnumerator(); enumerator.MoveNext();)
                    enumerator.Current.SetTarget(array2[random.Next(0, owned_units.Count())]);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to generate a random-based target for both armies...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> RandomBasedTargetPolicy(): \n\t{ex.Message}");

                has_targetable = false;
            }

            return has_targetable;
        }

        private bool PriorityBasedTargetPolicy(Army owned_units, Army enemy_units)
        {
            bool has_targetable = false;

            try
            {
                //TODO
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to generate a priority-based target for both armies...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> PriorityBasedTargetPolicy(): \n\t{ex.Message}");

                has_targetable = false;
            }

            return has_targetable;
        }

        private bool ResourceBasedTargetPolicy(Army owned_units, Army enemy_units)
        {
            bool has_targetable = false;

            try
            {
                //TODO
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to generate a resource-based target for both armies...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> ResourceBasedTargetPolicy(): \n\t{ex.Message}");

                has_targetable = false;
            }

            return has_targetable;
        }
    }
}
