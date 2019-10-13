using System.Collections.Generic;

namespace ModelService.Types
{
    /// <summary>
    /// Holds the current observation and simulated future based on observation
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The times of this node has been simulated
        /// </summary>
        private int Simulated_Runs { get; set; } = default(int);

        /// <summary>
        /// The times of this node's <see cref="Player"/> has won
        /// </summary>
        private int Simulated_Wins { get; set; } = default(int);

        /// <summary>
        /// The possible actions that can be simulated
        /// </summary>
        private Queue<string> Possible_Actions { get; set; } = default(Queue<string>);

        /// <summary>
        /// Stores the information related to the AI agent
        /// </summary>
        public Agent Player { get; private set; } = default(Agent);

        /// <summary>
        /// Stores the information related to the enemy agent
        /// </summary>
        public Agent Enemy { get; private set; } = default(Agent);

        /// <summary>
        /// The parent node of this node
        /// </summary>
        public Node Parent { get; private set; } = default(Node);

        /// <summary>
        /// The children of this node
        /// </summary>
        public List<Node> Children { get; private set; } = default(List<Node>);

        /// <summary>
        /// The current node selected by the highest simulated value
        /// </summary>
        public Node Chosen_Child { get; private set; } = default(Node);

        /// <summary>
        /// Checks if there are children for this node
        /// </summary>
        public bool IsExpanded
        {
            get { return (Children.Count != 0); }
        }

        /// <summary>
        /// Stores the simulated information of both <see cref="Agent"/>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <param name="possible_actions"></param>
        public Node(Node parent, Agent player, Agent enemy)
        {
            Parent = parent;
            Player = player;
            Enemy = enemy;
            Children = new List<Node>();
            Chosen_Child = null;
            Possible_Actions = new Queue<string>();
        }

        private void GeneratePossibleActions()
        {
            if (Player.Minerals >= 400 && Player.Vespene >= 300)
            {
                Possible_Actions.Enqueue("BUILD_COMMANDCENTER");
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("TRAIN_GHOST");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORY");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_THOR");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORT");
                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                Possible_Actions.Enqueue("TRAIN_RAVEN");
                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                Possible_Actions.Enqueue("TRAIN_BATTLECRUISER");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                Possible_Actions.Enqueue("BUILD_ARMORY");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL3")) //|| Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL2") || Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 400 || Player.Minerals >= 300 && Player.Vespene < 300 || Player.Vespene >= 250)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("TRAIN_GHOST");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORY");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_THOR");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORT");
                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                Possible_Actions.Enqueue("TRAIN_RAVEN");
                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                Possible_Actions.Enqueue("BUILD_ARMORY");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 300 || Player.Minerals >= 250 && Player.Vespene <= 250 || Player.Vespene >= 200)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("TRAIN_GHOST");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORY");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORT");
                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                Possible_Actions.Enqueue("TRAIN_RAVEN");
                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                Possible_Actions.Enqueue("BUILD_ARMORY");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL3"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 250 || Player.Minerals >= 175 && Player.Vespene < 200 || Player.Vespene >= 175)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("TRAIN_GHOST");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORY");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_THOR");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORT");
                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                Possible_Actions.Enqueue("BUILD_ARMORY");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL2"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL2"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL2"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL2"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL2"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 175 || Player.Minerals >= 150 && Player.Vespene < 175 || Player.Vespene >= 150)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("TRAIN_GHOST");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORY");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORT");
                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                Possible_Actions.Enqueue("BUILD_ARMORY");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 175 || Player.Minerals >= 150 && Player.Vespene < 150 || Player.Vespene >= 125)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("TRAIN_GHOST");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORY");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORT");
                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                Possible_Actions.Enqueue("BUILD_ARMORY");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 150 || Player.Minerals >= 125 && Player.Vespene < 150 || Player.Vespene >= 125)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 150 || Player.Minerals >= 125 && Player.Vespene < 125 || Player.Vespene >= 100)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("BUILD_SENSORTOWER");
            }
            else if (Player.Minerals < 125 || Player.Minerals >= 100 && Player.Vespene < 125 || Player.Vespene >= 100)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                if (!Player.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                Possible_Actions.Enqueue("BUILD_NUKE");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
            }
            else if (Player.Minerals < 100 || Player.Minerals >= 75 && Player.Vespene < 100 || Player.Vespene >= 75)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
            }
            else if (Player.Minerals < 75 || Player.Minerals >= 50 && Player.Vespene < 75 || Player.Vespene >= 50)
            {
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("TRAIN_REAPER");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
            }

            else if (Player.Minerals < 75 || Player.Minerals >= 50 && Player.Vespene < 50 || Player.Vespene >= 25)
            {
                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
            }

            //No gas required
            else if (Player.Minerals >= 400)
            {
                Possible_Actions.Enqueue("BUILD_COMMANDCENTER");
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
            }

            else if (Player.Minerals < 400 && Player.Minerals >= 150)
            {
                Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("BUILD_BARRACKS");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
            }
            else if (Player.Minerals < 150 && Player.Minerals >= 125)
            {
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
            }
            else if (Player.Minerals < 125 && Player.Minerals >= 100)
            {
                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("BUILD_BUNKER");
                Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                Possible_Actions.Enqueue("TRAIN_HELLION");
                Possible_Actions.Enqueue("TRAIN_HELLBAT");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
            }
            else if (Player.Minerals < 100 && Player.Minerals >= 75)
            {
                Possible_Actions.Enqueue("BUILD_REFINERY");
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
            }
            else if (Player.Minerals < 75 && Player.Minerals >= 50)
            {
                Possible_Actions.Enqueue("TRAIN_SCV");
                Possible_Actions.Enqueue("TRAIN_MARINE");
            }

            //Special case vespene higher than mineral cost
            else if (Player.Minerals >= 100 && Player.Minerals >= 200)
                Possible_Actions.Enqueue("TRAIN_RAVEN");

            /*
                MORPH_HELLBAT
                MORPH_SIEGEMODE
                MORPH_UNSIEGE
                MORPH_VIKINGFIGHTERMODE                 Not included becuase it is unit specific
                MORPH_VIKINGASSAULTMODE
                MORPH_LIBERATORAGMODE
                MORPH_LIBERATORAAMODE
                MORPH_HELLION
                EFFECT_AUTOTURRET
                EFFECT_CALLDOWNMULE
            */
        }

        public abstract Node Select();

        protected abstract void Expand();

        protected abstract void Simulate();

        protected abstract void Backpropagate();

        /// <summary>
        /// Returns the chosen action by calling <see cref="Agent.CreateMessage"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Chosen_Child.Player.CreateMessage();
    }
}
 