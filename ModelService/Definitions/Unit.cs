using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    public abstract partial class Unit
    {
        /// <summary>
        /// Unit's static definition and attributes for combat simulation
        /// </summary>
        /// <remarks>
        /// The key is the <see cref="Unit.Name"/> and the values are Health, Energy, Ground Damage, Air Damage, Armor, Is Flying Unit
        /// </remarks>
        public static Dictionary<string, Tuple<double, double, double, double, int, bool>> DEFINITIONS = new Dictionary<string, Tuple<double, double, double, double, int, bool>>()
        {
            //Ground Units
            ["Widow Mine"] = new Tuple<double, double, double, double, int, bool>(90, 0, 125, 125, 0, false),
            ["SCV"] = new Tuple<double, double, double, double, int, bool>(45, 0, 4.67, 0, 0, false),
            ["Marine"] = new Tuple<double, double, double, double, int, bool>(45, 0, 9.8, 9.8, 0, false),
            ["Marauder"] = new Tuple<double, double, double, double, int, bool>(125, 0, 9.3, 0, 1, false),
            ["Reaper"] = new Tuple<double, double, double, double, int, bool>(60, 0, 10.1, 0, 0, false),
            ["Ghost"] = new Tuple<double, double, double, double, int, bool>(100, 200, 9.3, 9.3, 0, false), //75/200
            ["Hellion"] = new Tuple<double, double, double, double, int, bool>(90, 0, 4.48, 0, 0, false),
            ["Hellbat"] = new Tuple<double, double, double, double, int, bool>(135, 0, 12.6, 0, 0, false),
            ["Siege Tank (Tank mode)"] = new Tuple<double, double, double, double, int, bool>(175, 0, 20.27, 0, 1, false),
            ["Siege Tank (Siege mode)"] = new Tuple<double, double, double, double, int, bool>(175, 0, 18.69, 0, 1, false),
            ["Cyclone"] = new Tuple<double, double, double, double, int, bool>(120, 0, 25.2, 25.2, 1, false),
            ["Thor (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(400, 0, 65.9, 0, 1, false),
            ["Thor (Explosive)"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 11.2, 1, false),
            ["Thor (High Impact)"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 23.4, 1, false),
            ["Auto-Turret"] = new Tuple<double, double, double, double, int, bool>(150, 0, 31.58, 31.58, 1, false),
            //Air Units
            ["Viking (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(135, 0, 16.8, 0, 0, true),
            ["Viking (Attack 2)"] = new Tuple<double, double, double, double, int, bool>(135, 0, 0, 14, 0, true),
            ["Medivac"] = new Tuple<double, double, double, double, int, bool>(150, 200, 0, 0, 1, true), //50/200
            ["Liberator (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(180, 0, 65.8, 0, 0, true),
            ["Liberator (Attack 2)"] = new Tuple<double, double, double, double, int, bool>(180, 0, 0, 7.8, 0, true),
            ["Raven"] = new Tuple<double, double, double, double, int, bool>(140, 200, 0, 0, 1, true), //50(+25)/200
            ["Banshee"] = new Tuple<double, double, double, double, int, bool>(140, 200, 27, 0, 0, true),
            ["Battlecruiser (Attack 1)"] = new Tuple<double, double, double, double, int, bool>(550, 200, 49.8, 0, 3, true),
            ["Battlecruiser (Attack 2)"] = new Tuple<double, double, double, double, int, bool>(550, 200, 0, 31.1, 3, true),
            //Buildings
            ["Planetary Fortress"] = new Tuple<double, double, double, double, int, bool>(1500, 0, 28, 0, 3, false),
            ["Bunker"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["Missile Turret"] = new Tuple<double, double, double, double, int, bool>(250, 0, 0, 39.3, 1, false),
            ["Command Center"] = new Tuple<double, double, double, double, int, bool>(1500, 0, 0, 0, 1, false),
            ["Orbital Command"] = new Tuple<double, double, double, double, int, bool>(1500, 200, 0, 0, 1, false), //50/200
            ["Supply Depot"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["Refinery"] = new Tuple<double, double, double, double, int, bool>(500, 0, 0, 0, 1, false),
            ["Barracks"] = new Tuple<double, double, double, double, int, bool>(1000, 0, 0, 0, 1, false),
            ["Engineering Bay"] = new Tuple<double, double, double, double, int, bool>(850, 0, 0, 0, 1, false),
            ["Bunker"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["Sensor Tower"] = new Tuple<double, double, double, double, int, bool>(200, 0, 0, 0, 0, false),
            ["Factory"] = new Tuple<double, double, double, double, int, bool>(1250, 0, 0, 0, 1, false),
            ["Ghost Academy"] = new Tuple<double, double, double, double, int, bool>(1250, 0, 0, 0, 1, false),
            ["Starport"] = new Tuple<double, double, double, double, int, bool>(1300, 0, 0, 0, 1, false),
            ["Armory"] = new Tuple<double, double, double, double, int, bool>(750, 0, 0, 0, 1, false),
            ["Fusion Core"] = new Tuple<double, double, double, double, int, bool>(750, 0, 0, 0, 1, false)
        };

        public static Dictionary<string, Tuple<int, double, double, int>> VALUES = new Dictionary<string, Tuple<int, double, double, int>>()
        {

        };
    }
}
