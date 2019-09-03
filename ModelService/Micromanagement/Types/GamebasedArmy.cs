using System;
using System.Linq;
using ModelService.Types;

namespace ModelService.Micromanagement.Types
{
    /// <summary>
    /// A collection of <see cref="GamebasedUnit"/>
    /// </summary>
    public class GamebasedArmy : ModelService.Types.Army
    {
        /// <summary>
        /// Parses the string of units and creates an instance of <see cref="GamebasedUnit"/>
        /// </summary>
        /// <param name="units"></param>
        public GamebasedArmy(string units)
        {
            Raw_Units = units;
            var parsed_units = units.Split('\n');

            if(parsed_units.Length > 0)
            {
                Units = new GamebasedUnit[parsed_units.Length];

                for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                {
                    var details = parsed_units[iterator].Split(',');

                    if(details.Length > 0)
                    {
                        var buffs = (details.Length > 5) ? details.Skip(5) : Enumerable.Empty<string>();
                        Units[iterator] = new GamebasedUnit(Convert.ToInt64(details[0]), details[1], details[2], Convert.ToDouble(details[3]), Convert.ToDouble(details[4]), buffs.ToArray());
                    }
                    else
                        throw new ArgumentOutOfRangeException("There are no details found for the parsed unit...");
                }
            }
            else
                throw new InvalidOperationException("There are no units to be parsed...");
        }

        /// <summary>
        /// Creates a new instance of <see cref="GamebasedArmy"/>
        /// </summary>
        /// <returns></returns>
        public override Army CreateDeepCopy()
        {
            Army new_army = new GamebasedArmy(Raw_Units);

            return new_army;
        }

        /// <summary>
        /// Returns the collection of <see cref="ModelService.Types.Unit.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Join(Environment.NewLine, (from unit in Units select (unit as GamebasedUnit).ToString()));
    }
}
