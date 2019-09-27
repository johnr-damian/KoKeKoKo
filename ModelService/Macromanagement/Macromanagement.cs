using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement
{
    public partial class Macromanagement
    {
        private static Random _generator = null;
        private Player _owned_player = null;
        private Player _enemy_player = null;

        static Macromanagement()
        {
            _generator = new Random();
        }

        public Macromanagement(string initialize)
        {

        }

        public List<double> GetMacromanagementAccuracyReport(int number_of_simulations)
        {
            var overall_results = new List<double>();

            return overall_results;
        }
    }
}
