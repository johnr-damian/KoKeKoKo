using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public static partial class Micromanagement
    {
        /// <summary>
        /// A list that contains the unit name as key and the value is its respective target priority.
        /// This list is can be use as a failover when the previous criteria is the same
        /// </summary>
        private static Dictionary<string, int> target_priority = new Dictionary<string, int>
        {
            ["Marine"] = 13,
            ["SCV"] = 13
        };

        public static List<Tuple<string, string>> RandomBasedTargetPolicy(string owned_units, string enemy_units)
        {
            var target_matching = new List<Tuple<string, string>>();

            try
            {
                var source = owned_units.Split('\n');
                var target = enemy_units.Split('\n');
                var parsed_owned_units = new List<Unit>();
                var parsed_enemy_units = new List<Unit>();

                foreach (var u in source)
                    parsed_owned_units.Add(new Unit(u));
                foreach (var t in target)
                    parsed_enemy_units.Add(new Unit(t));


            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to pair owned_units to enemy_units based on random...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> RandomBasedTargetPolicy(): \n\t{ex.Message}");
            }

            return target_matching;
        }

        public static List<Tuple<string, string>> NearestPositionBasedTargetPolicy(string owned_units, string enemy_units)
        {
            var target_matching = new List<Tuple<string, string>>();

            try
            {
                var source = owned_units.Split('\n');
                var target = enemy_units.Split('\n');
                var parsed_owned_units = new List<Unit>();
                var parsed_enemy_units = new List<Unit>();

                foreach (var u in source)
                    parsed_owned_units.Add(new Unit(u));
                foreach (var t in target)
                    parsed_enemy_units.Add(new Unit(t));


            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to pair owned_units to enemy_units based on position...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> NearestPositionBasedTargetPolicy(): \n\t{ex.Message}");
            }

            return target_matching;
        }

        public static List<Tuple<string, string>> ResourceCostBasedTargetPolicy(string owned_units, string enemy_units)
        {
            var target_matching = new List<Tuple<string, string>>();

            try
            {

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to pair owned_units to enemy_units based on resource...");
                Trace.WriteLine($@"Error in Model! Micromanagement -> ResourceCostBasedTargetPolicy(): \n\t{ex.Message}");
            }

            return target_matching;
        }

        public static List<Tuple<string, string>> FirstTargetBasedTargetPolicy(string owned_units, string enemy_units)
        {
            throw new NotImplementedException();
        }

        public static List<Tuple<string, string>> FirstDeathBasedTargetPolicy(string owned_units, string enemy_units)
        {
            throw new NotImplementedException();
        }
    }
}
