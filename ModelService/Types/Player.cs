using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    public partial class Player
    {
        private double total_supply;

        public string Name { get; protected set; } = "";

        public double Minerals { get; protected set; } = -1;

        public double Vespene { get; protected set; } = -1;

        public double Army_Supply { get; set; } = -1;

        public double Workers_Supply { get; set; } = -1;

        public double Supply
        {
            get
            {
                if (Army_Supply == -1 && Workers_Supply == -1)
                    return total_supply;
                else
                    return Army_Supply + Workers_Supply;
            }

            set { total_supply = value; }
        }

        public string Available_Actions { get; set; }

        public Army Current_army { get; set; }

    }
}
