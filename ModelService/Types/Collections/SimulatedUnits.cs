using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Collections
{
    public class SimulatedUnits : IEnumerable<SimulatedUnit>
    {

        public SimulatedUnits()
        {

        }

        public SimulatedUnits(IEnumerable<string> units)
        {

        }

        public IEnumerator<SimulatedUnit> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public SimulatedUnits Copy() => null;
    }
}
