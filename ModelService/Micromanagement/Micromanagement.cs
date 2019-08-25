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
        private T _owned_units = default(T);
        private T _enemy_units = default(T);

        /// <summary>
        /// Represents a battle either in game observation or from a CSV file
        /// </summary>
        /// <param name="owned_units"></param>
        /// <param name="enemy_units"></param>
        public Micromanagement(T owned_units, T enemy_units)
        {
            _owned_units = owned_units;
            _enemy_units = enemy_units;
        }

        public string GetSummaryOfResults(T final_owned_units, T final_enemy_units)
        {
            throw new NotImplementedException();
        }
    }

    public enum TargetPolicy
    {
        Random = 1,
        Priority = 2,
        Resource = 3
    }
}
