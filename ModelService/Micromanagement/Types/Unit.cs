using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public class Unit
    {
        public int Timestamp { get; private set; } = 0;

        public string Alliance { get; private set; } = "";

        public string Name { get; private set; } = "";

        public int Health { get; private set; } = 0;

        public int Armor { get; private set; } = 0;

        public Tuple<int, int> Position { get; private set; } = null;

        public Unit(string unit)
        {
            try
            {
                var fields = unit.Split(',');
                Timestamp = Convert.ToInt32(fields[0]);
                Alliance = fields[1];
                Name = fields[2];
                Health = Convert.ToInt32(fields[3]);
                Armor = Convert.ToInt32(fields[4]);
                Position = new Tuple<int, int>(Convert.ToInt32(fields[5]), Convert.ToInt32(fields[6]));
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occurred! Failed to create an instance of unit...");
                Trace.WriteLine($@"Error in Model! Unit -> Unit(): \n\t{ex.Message}");
            }
        }

        public void RecieveDamage(int damage)
        {
            Health = Health - damage;
        }
    }
}
