using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.CollectionTypes;
using ModelService.Types;
using ModelService.ValueTypes;

namespace ModelService.Macromanagement
{
    public partial class Macromanagement
    {
        public class MCTSAlgorithm : CollectionTypes.Tree
        {
            public int Max_Depth { get; set; } = default(int);

            private class MCTSNode : Node
            {
                public int Max_Depth { get; set; } = default(int);

                public MCTSNode(Node parent_node, Agent owned_agent, Agent enemy_agent, int max_depth) 
                    : base(parent_node, owned_agent, enemy_agent)
                {
                    Max_Depth = max_depth;
                }

                public override double GetNodeUCTValue()
                {
                    var exploration = (Simulated_Wins / Simulated_Runs);
                    var exploitation = Math.Sqrt((2 * Math.Log(Parent_Node.Simulated_Runs)) / Simulated_Runs);

                    if (Double.IsInfinity(exploration) || Double.IsNaN(exploration))
                        exploration = 0;
                    if (Double.IsInfinity(exploitation) || Double.IsNaN(exploitation))
                        exploitation = 0;

                    return (exploration + exploitation);
                }

                public override Node SelectAChildNode()
                {
                    if (!IsExpanded && (GetTrueHeight() != Max_Depth))
                    {
                        ExpandCurrentNode();

                        double bestuctnode = Double.MinValue;
                        Node bestchildnode = default(MCTSNode);
                        foreach (MCTSNode childnode in Children)
                        {
                            var current_uct = childnode.GetNodeUCTValue();
                            if (current_uct > bestuctnode)
                            {
                                bestuctnode = current_uct;
                                bestchildnode = childnode;
                            }
                        }

                        Chosen_Child = bestchildnode;
                        return Chosen_Child;
                    }

                    return null;
                }

                public override void Backpropagate()
                {
                    if(Parent_Node != null)
                    {
                        Simulated_Runs++;
                        Parent_Node.Backpropagate();
                    }
                }

                public void WonBackpropagate()
                {
                    if(Parent_Node != null)
                    {
                        Simulated_Wins++;
                        Simulated_Runs++;
                        ((MCTSNode)Parent_Node).WonBackpropagate();
                    }
                }

                protected override void ExpandCurrentNode()
                {
                    var actions = GeneratePotentialActions().ToList();
                    var random = Services.ModelRepositoryService.ModelService.GetModelService().RandomEngine;

                    var n_actions = random.Next(0, actions.Count);
                    for(int iterator = 0; iterator < 3; iterator++)
                    {
                        var child = new MCTSNode(this, Current_Owned_Agent.GetDeepCopy(), Current_Enemy_Agent.GetDeepCopy(), Max_Depth);
                        var owned_action = actions[random.Next(0, actions.Count)];
                        var enemy_action = actions[random.Next(0, actions.Count)];

                        child.SimulateCurrentNode($@"{owned_action},{enemy_action}");
                        Children.Add(child);
                    }
                }

                protected override IEnumerable<string> GeneratePotentialActions()
                {
                    var Possible_Actions = new Queue<string>();

                    if (Current_Owned_Agent.Minerals >= 400 && Current_Owned_Agent.Vespene >= 300)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_COMMANDCENTER");
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                    Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                                    Possible_Actions.Enqueue("BUILD_FACTORY");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_STARPORT"))
                                    Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                    Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                    Possible_Actions.Enqueue("TRAIN_THOR");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                Possible_Actions.Enqueue("TRAIN_RAVEN");
                                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                if (unitList.Contains("TERRAN_FUSIONCORE"))
                                    Possible_Actions.Enqueue("TRAIN_BATTLECRUISER");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");


                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 400 || Current_Owned_Agent.Minerals >= 300 && Current_Owned_Agent.Vespene < 300 || Current_Owned_Agent.Vespene >= 250)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                    Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                                    Possible_Actions.Enqueue("BUILD_FACTORY");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_STARPORT"))
                                    Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                    Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                    Possible_Actions.Enqueue("TRAIN_THOR");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                Possible_Actions.Enqueue("TRAIN_RAVEN");
                                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                if (unitList.Contains("TERRAN_FUSIONCORE"))
                                    Possible_Actions.Enqueue("TRAIN_BATTLECRUISER");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");


                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL3"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 300 || Current_Owned_Agent.Minerals >= 250 && Current_Owned_Agent.Vespene <= 250 || Current_Owned_Agent.Vespene >= 200)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                    Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                                    Possible_Actions.Enqueue("BUILD_FACTORY");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_STARPORT"))
                                    Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                    Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                Possible_Actions.Enqueue("TRAIN_RAVEN");
                                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");


                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 250 || Current_Owned_Agent.Minerals >= 175 && Current_Owned_Agent.Vespene < 200 || Current_Owned_Agent.Vespene >= 175)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                    Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                                    Possible_Actions.Enqueue("BUILD_FACTORY");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_STARPORT"))
                                    Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                    Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");


                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL2"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 175 || Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene < 175 || Current_Owned_Agent.Vespene >= 150)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                    Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                                    Possible_Actions.Enqueue("BUILD_FACTORY");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_STARPORT"))
                                    Possible_Actions.Enqueue("BUILD_FUSIONCORE");
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                    Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                                Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 175 || Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene < 150 || Current_Owned_Agent.Vespene >= 125)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                    Possible_Actions.Enqueue("BUILD_GHOSTACADEMY");
                                    Possible_Actions.Enqueue("BUILD_FACTORY");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 150 || Current_Owned_Agent.Minerals >= 125 && Current_Owned_Agent.Vespene < 150 || Current_Owned_Agent.Vespene >= 125)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                    Possible_Actions.Enqueue("TRAIN_GHOST");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 150 || Current_Owned_Agent.Minerals >= 125 && Current_Owned_Agent.Vespene < 125 || Current_Owned_Agent.Vespene >= 100)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                    Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 125 || Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene < 125 || Current_Owned_Agent.Vespene >= 100)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORTTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                            }
                            else if (unit.Name == "TERRAN_FUSIONCORE")
                                Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                            else if (unit.Name == "TERRAN_ARMORY")
                            {
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANSHIPWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANSHIPWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANVEHICLEANDSHIPARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANVEHICLEANDSHIPPLATING");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYWEAPONSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYWEAPONS");
                                if (!Current_Owned_Agent.Upgrades.Exists(i => i == "TERRANINFANTRYARMORSLEVEL1"))
                                    Possible_Actions.Enqueue("RESEARCH_TERRANINFANTRYARMOR");
                            }
                            else if (unit.Name == "TERRAN_GHOSTACADEMY")
                            {
                                Possible_Actions.Enqueue("BUILD_NUKE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 100 || Current_Owned_Agent.Minerals >= 75 && Current_Owned_Agent.Vespene < 100 || Current_Owned_Agent.Vespene >= 75)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_FACTORYTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 75 || Current_Owned_Agent.Minerals >= 50 && Current_Owned_Agent.Vespene < 75 || Current_Owned_Agent.Vespene >= 50)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("TRAIN_REAPER");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                            }
                            else if (unit.Name == "TERRAN_BARRACKSTECHLAB")
                            {
                                Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {
                                Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                        }
                    }

                    else if (Current_Owned_Agent.Minerals < 75 || Current_Owned_Agent.Minerals >= 50 && Current_Owned_Agent.Vespene < 50 || Current_Owned_Agent.Vespene >= 25)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                                Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                            }
                            else if (unit.Name == "TERRAN_STARPORT")
                            {

                                Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                            }
                        }
                    }

                    //No gas required
                    else if (Current_Owned_Agent.Minerals >= 400)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_COMMANDCENTER");
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_FACTORY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_ARMORY");
                                    Possible_Actions.Enqueue("BUILD_STARPORT");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                            }
                        }
                    }

                    else if (Current_Owned_Agent.Minerals < 400 && Current_Owned_Agent.Minerals >= 150)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                    Possible_Actions.Enqueue("BUILD_BARRACKS");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                    Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 150 && Current_Owned_Agent.Minerals >= 125)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 125 && Current_Owned_Agent.Minerals >= 100)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_SCV")
                            {
                                Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                if (unitList.Contains("TERRAN_BARRACKS"))
                                {
                                    Possible_Actions.Enqueue("BUILD_BUNKER");
                                }
                                if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                    Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                {
                                    Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                }
                            }
                            else if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                            }
                            else if (unit.Name == "TERRAN_FACTORY")
                            {
                                Possible_Actions.Enqueue("TRAIN_HELLION");
                                if (unitList.Contains("TERRAN_ARMORY"))
                                {
                                    Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                }
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 100 && Current_Owned_Agent.Minerals >= 75)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                                Possible_Actions.Enqueue("BUILD_REFINERY");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                            }
                        }
                    }
                    else if (Current_Owned_Agent.Minerals < 75 && Current_Owned_Agent.Minerals >= 50)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (unit.Name == "TERRAN_COMMANDCENTER")
                            {
                                Possible_Actions.Enqueue("TRAIN_SCV");
                            }
                            else if (unit.Name == "TERRAN_ORBITALCOMMAND")
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                            else if (unit.Name == "TERRAN_BARRACKS")
                            {
                                Possible_Actions.Enqueue("TRAIN_MARINE");
                            }
                        }
                    }

                    //Special case vespene higher than mineral cost
                    else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Minerals >= 200)
                    {
                        var unitList = new List<string>();
                        foreach (var unit in Current_Owned_Agent.Units)
                        {
                            if (!unitList.Contains(unit.Name))
                                unitList.Add(unit.Name);
                        }
                        if (unitList.Contains("TERRAN_STARPORT"))
                            Possible_Actions.Enqueue("TRAIN_RAVEN");
                    }
                    var unitListGlobal = new List<string>();
                    foreach (var unit in Current_Owned_Agent.Units)
                    {
                        if (!unitListGlobal.Contains(unit.Name))
                            unitListGlobal.Add(unit.Name);
                    }

                    if (unitListGlobal.Contains("TERRAN_HELLION"))
                        Possible_Actions.Enqueue("MORPH_HELLBAT");
                    if (unitListGlobal.Contains("TERRAN_HELLIONTANK"))
                        Possible_Actions.Enqueue("MORPH_HELLION");
                    if (unitListGlobal.Contains("TERRAN_SIEGETANK"))
                        Possible_Actions.Enqueue("MORPH_SIEGEMODE");
                    if (unitListGlobal.Contains("TERRAN_SIEGETANKSIEGED"))
                        Possible_Actions.Enqueue("MORPH_UNSIEGE");
                    if (unitListGlobal.Contains("TERRAN_VIKINGASSAULT"))
                        Possible_Actions.Enqueue("MORPH_VIKINGFIGHTERMODE");
                    if (unitListGlobal.Contains("TERRAN_VIKINGFIGHTER"))
                        Possible_Actions.Enqueue("MORPH_VIKINGASSAULTMODE");
                    if (unitListGlobal.Contains("TERRAN_LIBERATOR"))
                        Possible_Actions.Enqueue("MORPH_LIBERATORAGMODE");
                    if (unitListGlobal.Contains("TERRAN_LIBERATORAG"))
                        Possible_Actions.Enqueue("MORPH_LIBERATORAAMODE");
                    if (unitListGlobal.Contains("TERRAN_RAVEN"))
                        Possible_Actions.Enqueue("EFFECT_AUTOTURRET");
                    if (unitListGlobal.Contains("TERRAN_ORBITALCOMMAND"))
                        Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");

                    return Possible_Actions;
                }

                protected override void SimulateCurrentNode(string potential_action)
                {
                    var actions = potential_action.Split(',');

                    Current_Owned_Agent.Chosen_Action = actions[0];
                    Current_Owned_Agent.ApplyAction(actions[0]);
                    Current_Enemy_Agent.Chosen_Action = actions[1];
                    Current_Enemy_Agent.ApplyAction(actions[1]);

                    if (Current_Owned_Agent.Worth >= Current_Enemy_Agent.Worth)
                        WonBackpropagate();
                    else
                        Backpropagate();
                }
            }

            public MCTSAlgorithm(Agent owned_agent, Agent enemy_agent) 
                : base(owned_agent, enemy_agent)
            {
                Max_Depth = Math.Max(owned_agent.Potential_Depth, enemy_agent.Potential_Depth);
                Root_Node = new MCTSNode(null, owned_agent, enemy_agent, Max_Depth);
                Current_Node = Root_Node;
            }

            public override IEnumerable<Tuple<string, CostWorth>> GeneratePredictedAction(DateTime endtime)
            {
                var current_time = DateTime.Now;

                //While it is still not yet the end of the game
                //Keep generating predicted actions
                while (current_time < endtime)
                {
                    //The chosen action that will be executed by the agent
                    var predicted_action = default(Tuple<string, ValueTypes.CostWorth>);

                    try
                    {
                        //Get a child node that has the best move
                        var selected_node = Current_Node.SelectAChildNode();

                        if (selected_node == null)
                            break;

                        //Get the predicted action
                        predicted_action = selected_node.GetNodeInformation();

                        //Update the Current Node and time
                        current_time = DateTime.Now;
                        Current_Node = selected_node;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($@"GeneratePredictedAction() -> {ex.Message}");
                        predicted_action = null;
                        throw new Exception("An error occurred! There is an invalid data format to generate an action...");
                    }
                    catch (NullReferenceException ex)
                    {
                        Console.WriteLine($@"GeneratePredictedAction() -> {ex.Message}");
                        predicted_action = null;
                        throw new Exception("An error occurred! There is a null data that is being accessed to generate an action...");
                    }

                    yield return predicted_action;
                }
            }
        }
    }
}
