using System;
using System.Linq;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A collection of <see cref="CSVbasedUnit"/>
    /// </summary>
    public class CSVbasedArmy : ModelService.Types.Army
    {
        /// <summary>
        /// Parses the string of units and creates an instance of <see cref="CSVbasedUnit"/>
        /// </summary>
        /// <param name="units"></param>
        public CSVbasedArmy(string units)
        {
            var parsed_units = units.Split('\n');

            if(parsed_units.Length > 0)
            {
                Units = new CSVbasedUnit[parsed_units.Length];

                for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                {
                    var details = parsed_units[iterator].Split(',');

                    if(details.Length > 0)
                    {
                        var buffs = (details.Length > 6) ? details.Skip(6) : Enumerable.Empty<string>();
                        Units[iterator] = new CSVbasedUnit(Convert.ToInt32(details[0]), Convert.ToInt64(details[1]), details[2], details[3], Convert.ToDouble(details[4]), Convert.ToDouble(details[5]), buffs.ToArray());
                    }
                    else
                        throw new ArgumentOutOfRangeException("There are no details found for the parsed unit...");
                }
            }
            else
                throw new InvalidOperationException("There are no units to be parsed...");
        }

        /// <summary>
        /// Returns the colection of <see cref="CSVbasedUnit.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Join(",", (from unit in Units select (unit as CSVbasedUnit).ToString()));
    }
}
