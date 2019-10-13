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
        private Agent _owned_player = null;
        private Agent _enemy_player = null;

        /// <summary>
        /// A csv-based macromanagement. Represents a battle from a file
        /// </summary>
        /// <param name="owned_player"></param>
        /// <param name="enemy_player"></param>
        public Macromanagement(Agent owned_player, Agent enemy_player)
        {
            _owned_player = owned_player;
            _enemy_player = enemy_player;
        }

        /// <summary>
        /// A gameplay-based macromanagement. Represents a battle from a game observation
        /// </summary>
        /// <param name="owned_player"></param>
        public Macromanagement(Agent owned_player) 
            : this(owned_player, owned_player.GetDeepCopy()) { }

        public List<double> GetMacromanagementAccuracyReport(int number_of_simulations)
        {
            var overall_results = new List<double>();

            return overall_results;
        }
    }
}
