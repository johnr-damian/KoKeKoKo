using System;
using System.Collections.Generic;

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
            ["TERRAN_WIDOWMINE"] = new Tuple<double, double, double, double, int, bool>(90, 0, 125, 125, 0, false),
            ["TERRAN_SCV"] = new Tuple<double, double, double, double, int, bool>(45, 0, 4.67, 0, 0, false),
            ["TERRAN_MARINE"] = new Tuple<double, double, double, double, int, bool>(45, 0, 9.8, 9.8, 0, false),
            ["TERRAN_MARAUDER"] = new Tuple<double, double, double, double, int, bool>(125, 0, 9.3, 0, 1, false),
            ["TERRAN_REAPER"] = new Tuple<double, double, double, double, int, bool>(60, 0, 10.1, 0, 0, false),
            ["TERRAN_GHOST"] = new Tuple<double, double, double, double, int, bool>(100, 200, 9.3, 9.3, 0, false), //75/200
            ["TERRAN_HELLION"] = new Tuple<double, double, double, double, int, bool>(90, 0, 4.48, 0, 0, false),
            ["TERRAN_HELLIONTANK"] = new Tuple<double, double, double, double, int, bool>(135, 0, 12.6, 0, 0, false),
            ["TERRAN_SIEGETANK"] = new Tuple<double, double, double, double, int, bool>(175, 0, 20.27, 0, 1, false),
            ["TERRAN_SIEGETANKSIEGED"] = new Tuple<double, double, double, double, int, bool>(175, 0, 18.69, 0, 1, false),
            ["TERRAN_CYCLONE"] = new Tuple<double, double, double, double, int, bool>(120, 0, 25.2, 25.2, 1, false),
            ["TERRAN_THOR"] = new Tuple<double, double, double, double, int, bool>(400, 0, 65.9, 11.2, 1, false),
            ["TERRAN_THORAP"] = new Tuple<double, double, double, double, int, bool>(400, 0, 65.9, 23.4, 1, false),
            ["TERRAN_AUTOTURRET"] = new Tuple<double, double, double, double, int, bool>(150, 0, 31.58, 31.58, 1, false),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new Tuple<double, double, double, double, int, bool>(135, 0, 16.8, 0, 0, true),
            ["TERRAN_VIKINGFIGHTER"] = new Tuple<double, double, double, double, int, bool>(135, 0, 0, 14, 0, true),
            ["TERRAN_MEDIVAC"] = new Tuple<double, double, double, double, int, bool>(150, 200, 0, 0, 1, true), //50/200
            ["TERRAN_LIBERATORAG"] = new Tuple<double, double, double, double, int, bool>(180, 0, 65.8, 0, 0, true),
            ["TERRAN_LIBERATOR"] = new Tuple<double, double, double, double, int, bool>(180, 0, 0, 7.8, 0, true),
            ["TERRAN_RAVEN"] = new Tuple<double, double, double, double, int, bool>(140, 200, 0, 0, 1, true), //50(+25)/200
            ["TERRAN_BANSHEE"] = new Tuple<double, double, double, double, int, bool>(140, 200, 27, 0, 0, true),
            ["TERRAN_BATTLECRUISER"] = new Tuple<double, double, double, double, int, bool>(550, 200, 49.8, 31.1, 3, true),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new Tuple<double, double, double, double, int, bool>(1500, 0, 28, 0, 3, false),
            ["TERRAN_BUNKER"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["TERRAN_MISSILETURRET"] = new Tuple<double, double, double, double, int, bool>(250, 0, 0, 39.3, 1, false),
            ["TERRAN_COMMANDCENTER"] = new Tuple<double, double, double, double, int, bool>(1500, 0, 0, 0, 1, false),
            ["TERRAN_ORBITALCOMMAND"] = new Tuple<double, double, double, double, int, bool>(1500, 200, 0, 0, 1, false), //50/200
            ["TERRAN_SUPPLYDEPOT"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["TERRAN_REFINERY"] = new Tuple<double, double, double, double, int, bool>(500, 0, 0, 0, 1, false),
            ["TERRAN_BARRACKS"] = new Tuple<double, double, double, double, int, bool>(1000, 0, 0, 0, 1, false),
            ["TERRAN_ENGINEERINGBAY"] = new Tuple<double, double, double, double, int, bool>(850, 0, 0, 0, 1, false),
            ["TERRAN_BUNKER"] = new Tuple<double, double, double, double, int, bool>(400, 0, 0, 0, 1, false),
            ["TERRAN_SENSORTOWER"] = new Tuple<double, double, double, double, int, bool>(200, 0, 0, 0, 0, false),
            ["TERRAN_FACTORY"] = new Tuple<double, double, double, double, int, bool>(1250, 0, 0, 0, 1, false),
            ["TERRAN_GHOSTACADEMY"] = new Tuple<double, double, double, double, int, bool>(1250, 0, 0, 0, 1, false),
            ["TERRAN_STARPORT"] = new Tuple<double, double, double, double, int, bool>(1300, 0, 0, 0, 1, false),
            ["TERRAN_ARMORY"] = new Tuple<double, double, double, double, int, bool>(750, 0, 0, 0, 1, false),
            ["TERRAN_FUSIONCORE"] = new Tuple<double, double, double, double, int, bool>(750, 0, 0, 0, 1, false)
        };

        /// <summary>
        /// Unit's static worth for micromanagement and macromanagement
        /// </summary>
        /// <remarks>
        /// The key is the <see cref="Unit.Name"/> and the values are Priority, Mineral Cost, Vespene Cost, Supply Cost
        /// </remarks>
        public static Dictionary<string, Tuple<int, double, double, int>> VALUES = new Dictionary<string, Tuple<int, double, double, int>>()
        {
            //Ground Units
            ["TERRAN_WIDOWMINE"] = new Tuple<int, double, double, int>(19, 75, 25, 2),
            ["TERRAN_SCV"] = new Tuple<int, double, double, int>(20, 50, 0, 1),
            ["TERRAN_MARINE"] = new Tuple<int, double, double, int>(20, 50, 0, 1),
            ["TERRAN_MARAUDER"] = new Tuple<int, double, double, int>(20, 100, 25, 2),
            ["TERRAN_REAPER"] = new Tuple<int, double, double, int>(20, 50, 50, 1),
            ["TERRAN_GHOST"] = new Tuple<int, double, double, int>(20, 150, 125, 2),
            ["TERRAN_HELLION"] = new Tuple<int, double, double, int>(20, 100, 0, 2),
            ["TERRAN_HELLIONTANK"] = new Tuple<int, double, double, int>(20, 100, 0, 2),
            ["TERRAN_SIEGETANK"] = new Tuple<int, double, double, int>(20, 150, 125, 3),
            ["TERRAN_SIEGETANKSEIGED"] = new Tuple<int, double, double, int>(20, 150, 125, 3),
            ["TERRAN_CYCLONE"] = new Tuple<int, double, double, int>(20, 150, 100, 3),
            ["TERRAN_THOR"] = new Tuple<int, double, double, int>(20, 300, 200, 6),
            ["TERRAN_THORAP"] = new Tuple<int, double, double, int>(20, 300, 200, 6),
            ["TERRAN_AUTOTURRET"] = new Tuple<int, double, double, int>(20, 0, 0, 0),
            //Air Units
            ["TERRAN_VIKINGASSAULT"] = new Tuple<int, double, double, int>(20, 150, 75, 2),
            ["TERRAN_VIKINGFIGHTER"] = new Tuple<int, double, double, int>(20, 150, 75, 2),
            ["TERRAN_MEDIVAC"] = new Tuple<int, double, double, int>(20, 100, 100, 2),
            ["TERRAN_LIBERATORAG"] = new Tuple<int, double, double, int>(20, 150, 150, 3),
            ["TERRAN_LIBERATOR"] = new Tuple<int, double, double, int>(20, 150, 150, 3),
            ["TERRAN_RAVEN"] = new Tuple<int, double, double, int>(20, 100, 200, 2),
            ["TERRAN_BANSHEE"] = new Tuple<int, double, double, int>(20, 150, 100, 3),
            ["TERRAN_BATTLECRUISERr"] = new Tuple<int, double, double, int>(20, 400, 300, 6),
            //Buildings
            ["TERRAN_PLANETARYFORTRESS"] = new Tuple<int, double, double, int>(20, 550, 150, 0),
            ["TERRAN_BUNKER"] = new Tuple<int, double, double, int>(20, 100, 0, 0),
            ["TERRAN_MISSILETURRET"] = new Tuple<int, double, double, int>(19, 100, 0, 0),
            ["TERRAN_COMMANDCENTER"] = new Tuple<int, double, double, int>(11, 400, 0, 0),
            ["TERRAN_ORBITALCOMMAND"] = new Tuple<int, double, double, int>(11, 550, 0, 0),
            ["TERRAN_SUPPLYDEPOT"] = new Tuple<int, double, double, int>(11, 100, 0, 0),
            ["TERRAN_REFINERY"] = new Tuple<int, double, double, int>(11, 75, 0, 0),
            ["TERRAN_BARRACKS"] = new Tuple<int, double, double, int>(11, 150, 0, 0),
            ["TERRAN_ENGINEERINGBAY"] = new Tuple<int, double, double, int>(11, 125, 0, 0),
            ["TERRAN_BUNKER"] = new Tuple<int, double, double, int>(11, 100, 0, 0),
            ["TERRAN_SENSORTOWER"] = new Tuple<int, double, double, int>(11, 125, 100, 0),
            ["TERRAN_FACTORY"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["TERRAN_GHOSTACADEMY"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["TERRAN_STARPORT"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["TERRAN_ARMORY"] = new Tuple<int, double, double, int>(11, 150, 100, 0),
            ["TERRAN_FUSIONCORE"] = new Tuple<int, double, double, int>(11, 150, 150, 0)
        };
    }
}
