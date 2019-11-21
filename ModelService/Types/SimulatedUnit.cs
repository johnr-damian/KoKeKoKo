using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public class SimulatedUnit : IActor<SimulatedUnit>
    {



        private int Current_Target { get; set; } = default(int);

        private List<SimulatedUnit> Targets { get; set; } = default(List<SimulatedUnit>);



        public bool IsDefeated => throw new NotImplementedException();

        public void ApplyChosenAction(string chosen_action)
        {
            throw new NotImplementedException();
        }

        public SimulatedUnit Copy()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GeneratePotentialActions()
        {
            throw new NotImplementedException();
        }
    }
}
