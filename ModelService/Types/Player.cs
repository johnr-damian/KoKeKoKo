using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    public partial class Player
    {
        public string Name { get; protected set; } = "";

        public double Minerals { get; protected set; } = -1;

        public double Vespene { get; protected set; } = -1;

        public int Supply { get; protected set; } = -1;


    }
}
