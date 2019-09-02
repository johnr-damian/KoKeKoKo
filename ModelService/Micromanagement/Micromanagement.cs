using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement<T> where T : Unit
    {
        private Army _owned_units = null;
        private Army _enemy_units = null;

        public Micromanagement(Army owned_units, Army enemy_units)
        {
            _owned_units = owned_units;
            _enemy_units = enemy_units;
        }

        public string GetBattlePrediction(PredictionAlgorithm algorithm, TargetPolicy policy)
        {

        }

        public void GetMicromanagementAccuracy()
        {

        }
    }
}
