using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Micromanagement.Types
{
    public class Unit
    {
        /// <summary>
        /// The current health of this unit based on current observation
        /// </summary>
        public double Health { get; private set; } = 0;

        /// <summary>
        /// The armor of the unit
        /// </summary>
        public int Armor { get; private set; } = 0;

        /// <summary>
        /// The current damage it can deal to the enemy
        /// </summary>
        public double Damage { get; private set; } = 0;

        /// <summary>
        /// The name of this unit
        /// </summary>
        public string Name { get; private set; } = "";

        /// <summary>
        /// The controller of this unit
        /// </summary>
        public string Alliance { get; private set; } = "";

        /// <summary>
        /// The opposing unit to be targeted during combat
        /// </summary>
        public Unit Target { get; private set; } = null;

        /// <summary>
        /// The current position of this unit based on current observation
        /// </summary>
        public Coordinate Position { get; private set; } = null;

        /// <summary>
        /// The seconds when this unit is found engaging in combat
        /// </summary>
        public int Timestamp { get; private set; } = 0;

        
        public Unit(int timestamp, string alliance, string name, int health, int armor, int x_position, int y_position)
        {
            try
            {
                Timestamp = timestamp;
                Alliance = alliance;
                Name = name;
                Health = health;
                Armor = armor;
                Position = new Coordinate(x_position, y_position);
            }
            catch(Exception ex)
            {
                
            }
        }

        public Unit(string alliance, string name, int health, int armor, int x_position, int y_position)
        {

        }

        public Unit(double health, int armor, double damage, string name, string alliance, int x_position, int y_position, int timestamp)
        {
            Health = health;
            Armor = armor;
            Damage = damage;
            Name = name;
            Alliance = alliance;
            Position = new Coordinate(x_position, y_position);
            Timestamp = timestamp;
        }

        public Unit(double health, int armor, double )
    }
}
