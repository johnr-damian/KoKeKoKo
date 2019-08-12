using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement.Types
{
    public class Units : IEnumerable<Unit>
    {
        private Unit[] _units = null;

        public Units(string units)
        {
            try
            {
                var parsed_units = units.Split('\n');

                if (parsed_units.Length > 0)
                {
                    _units = new Unit[parsed_units.Length];

                    for(int iterator = 0; iterator < parsed_units.Length; iterator++)
                    {
                        var unit_details = parsed_units[iterator].Split(',');
                        _units[iterator] =
                    }
                }
                else
                    throw new Exception("There are no units in string to generate from....");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to generate a list of units...");
                Trace.WriteLine($@"Error in Model! Units -> Units(): \n\t{ex.Message}");
            }
        }

        public IEnumerator<Unit> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
