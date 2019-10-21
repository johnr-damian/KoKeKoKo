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
                    //variable for existing units
                    var unitList = new List<string>();
                    //loop for adding units to a list for processing
                    foreach (var unit in Current_Owned_Agent.Units)
                    {
                        if (!unitList.Contains(unit.Name))
                            unitList.Add(unit.Name);
                    }
                    foreach (var unit in Current_Owned_Agent.Units)
                    {
                        switch (unit.Name)
                        {
                            case "TERRAN_SCV":
                                if (Current_Owned_Agent.Minerals >= 400 && Current_Owned_Agent.Vespene >= 150)
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
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
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
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 125)
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
                                else if (Current_Owned_Agent.Minerals >= 125 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    Possible_Actions.Enqueue("BUILD_REFINERY");
                                    Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT");
                                    if (unitList.Contains("TERRAN_BARRACKS"))
                                        Possible_Actions.Enqueue("BUILD_BUNKER");
                                    if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                        Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY");
                                    if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                    {
                                        Possible_Actions.Enqueue("BUILD_MISSILETURRET");
                                        Possible_Actions.Enqueue("BUILD_SENSORTOWER");
                                    }
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
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
                                else if (Current_Owned_Agent.Minerals >= 75 && Current_Owned_Agent.Vespene >= 75)
                                {
                                    Possible_Actions.Enqueue("BUILD_REFINERY");
                                }
                                else if (Current_Owned_Agent.Minerals >= 400)
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
                                else if (Current_Owned_Agent.Minerals >= 150)
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
                                else if (Current_Owned_Agent.Minerals >= 125)
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
                                else if (Current_Owned_Agent.Minerals >= 100)
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
                                break;
                            case "TERRAN_COMMANDCENTER":
                                if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
                                {
                                    if (Current_Owned_Agent.Supply >= 1)
                                        Possible_Actions.Enqueue("TRAIN_SCV");
                                    if (unitList.Contains("TERRAN_BARRACKS"))
                                        Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                    if (unitList.Contains("TERRAN_ENGINEERINGBAY"))
                                        Possible_Actions.Enqueue("MORPH_PLANETARYFORTRESS");
                                }
                                else if (Current_Owned_Agent.Minerals >= 150)
                                {
                                    if (Current_Owned_Agent.Supply >= 1)
                                        Possible_Actions.Enqueue("TRAIN_SCV");
                                    if (unitList.Contains("TERRAN_BARRACKS"))
                                        Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND");
                                }
                                else if (Current_Owned_Agent.Minerals >= 50)
                                {
                                    if (Current_Owned_Agent.Supply >= 1)
                                        Possible_Actions.Enqueue("TRAIN_SCV");
                                }
                                break;
                            case "TERRAN_BARRACKS":
                                if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 125)
                                {
                                    Possible_Actions.Enqueue("TRAIN_MARINE");
                                    Possible_Actions.Enqueue("TRAIN_REAPER");
                                    if (Current_Owned_Agent.Supply >= 1)
                                    {
                                        if (unitList.Contains("TERRAN_BARRACKSTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                        if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                            Possible_Actions.Enqueue("TRAIN_GHOST");
                                    }
                                    Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    if (Current_Owned_Agent.Supply >= 1)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_MARINE");
                                        Possible_Actions.Enqueue("TRAIN_REAPER");
                                    }
                                    if (Current_Owned_Agent.Supply >= 2)
                                        if (unitList.Contains("TERRAN_BARRACKSTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_MARAUDER");
                                    Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 50 && Current_Owned_Agent.Vespene >= 50)
                                {
                                    if (Current_Owned_Agent.Supply >= 1)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_MARINE");
                                        Possible_Actions.Enqueue("TRAIN_REAPER");
                                    }
                                    Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 50 && Current_Owned_Agent.Vespene >= 25)
                                {
                                    if (Current_Owned_Agent.Supply >= 1)
                                        Possible_Actions.Enqueue("TRAIN_MARINE");
                                    Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB");
                                }
                                else if (Current_Owned_Agent.Minerals >=50)
                                    if (Current_Owned_Agent.Supply >= 1)
                                        Possible_Actions.Enqueue("TRAIN_MARINE");
                                break;
                            case "TERRAN_BARRACKSTECHLAB":
                                if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD");
                                    Possible_Actions.Enqueue("RESEARCH_STIMPACK");
                                    Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                                }
                                else if (Current_Owned_Agent.Minerals >= 50 && Current_Owned_Agent.Vespene >= 50)
                                    Possible_Actions.Enqueue("RESEARCH_CONCUSSIVESHELLS");
                                break;
                            case "TERRAN_FACTORY":
                                if (Current_Owned_Agent.Minerals >= 300 && Current_Owned_Agent.Vespene >= 200)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_HELLION");
                                        Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                        if (unitList.Contains("TERRAN_ARMORY"))
                                        {
                                            Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                            if (Current_Owned_Agent.Supply >= 6)
                                                Possible_Actions.Enqueue("TRAIN_THOR");
                                        }
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                    {
                                        if (unitList.Contains("TERRAN_FACTORYTECHLAB"))
                                        {
                                            Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                            Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                        }
                                    }
                                    Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 125)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_HELLION");
                                        Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                        if (unitList.Contains("TERRAN_ARMORY"))
                                            Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                    {
                                        if (unitList.Contains("TERRAN_FACTORYTECHLAB"))
                                        {
                                            Possible_Actions.Enqueue("TRAIN_SIEGETANK");
                                            Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                        }
                                    }
                                    Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_HELLION");
                                        Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                        if (unitList.Contains("TERRAN_ARMORY"))
                                            Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                        if (unitList.Contains("TERRAN_FACTORYTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_CYCLONE");
                                    Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_HELLION");
                                        Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                        if (unitList.Contains("TERRAN_ARMORY"))
                                            Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                    }
                                    Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 75 && Current_Owned_Agent.Vespene >= 50)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                        Possible_Actions.Enqueue("TRAIN_WIDOWMINE");
                                    Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB");
                                    Possible_Actions.Enqueue("BUILD_FACTORYREACTOR");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_HELLION");
                                        if (unitList.Contains("TERRAN_ARMORY"))
                                            Possible_Actions.Enqueue("TRAIN_HELLBAT");
                                    }
                                }
                                break;
                            case "TERRAN_FACTORYTECHLAB":
                                if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_INFERNALPREIGNITER");
                                    Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                    Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_MAGFIELDLAUNCHERS");
                                    Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                                }
                                else if (Current_Owned_Agent.Minerals >= 75 && Current_Owned_Agent.Vespene >= 75)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_DRILLINGCLAWS");
                                }
                                break;
                            case "TERRAN_STARPORT":
                                if (Current_Owned_Agent.Minerals >= 400 && Current_Owned_Agent.Vespene >= 300)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                        Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                        if (unitList.Contains("TERRAN_STARPORTTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                    }                                
                                    if (unitList.Contains("TERRAN_FUSIONCORE"))
                                        if (Current_Owned_Agent.Supply >= 6)
                                            Possible_Actions.Enqueue("TRAIN_BATTLECRUISER");
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                                }
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                        Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_LIBERATOR");
                                        if (unitList.Contains("TERRAN_STARPORTTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                    }
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                                }
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");
                                        Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                        if (unitList.Contains("TERRAN_STARPORTTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_BANSHEE");
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                                }
                                else if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 75)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                        Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER");                 
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    if (Current_Owned_Agent.Supply >= 2)
                                        Possible_Actions.Enqueue("TRAIN_MEDIVAC");
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                                }
                                else if (Current_Owned_Agent.Minerals >= 50 && Current_Owned_Agent.Vespene >= 50)
                                {
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR");
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB");
                                }
                                //Mineral cost > Vespene cost case
                                if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 200)
                                    if (Current_Owned_Agent.Supply >= 2)
                                        if (unitList.Contains("TERRAN_STARPORTTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_RAVEN");     
                                break;
                            case "TERRAN_STARPORTTECHLAB":
                                if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");
                                    Possible_Actions.Enqueue("RESEARCH_RAVENCORVIDREACTOR");
                                    Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                    Possible_Actions.Enqueue("RESEARCH_BANSHEEHYPERFLIGHTROTORS");
                                    Possible_Actions.Enqueue("RESEARCH_ADVANCEDBALLISTICS");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_HIGHCAPACITYFUELTANKS");                                    
                                    Possible_Actions.Enqueue("RESEARCH_BANSHEECLOAKINGFIELD");
                                }
                                break;
                            case "TERRAN_FUSIONCORE":
                                if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
                                    Possible_Actions.Enqueue("RESEARCH_BATTLECRUISERWEAPONREFIT");
                                break;
                            case "TERRAN_ARMORY":
                                if (Current_Owned_Agent.Minerals >= 250 && Current_Owned_Agent.Vespene >= 250)
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
                                else if (Current_Owned_Agent.Minerals >= 175 && Current_Owned_Agent.Vespene >= 175)
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
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
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
                                break;
                            case "TERRAN_GHOSTACADEMY":
                                if (Current_Owned_Agent.Minerals >= 150 && Current_Owned_Agent.Vespene >= 150)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_PERSONALCLOAKING");
                                    if (unitList.Contains("TERRAN_FACTORY"))
                                        Possible_Actions.Enqueue("BUILD_NUKE");
                                }
                                else if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                    if (unitList.Contains("TERRAN_FACTORY"))
                                        Possible_Actions.Enqueue("BUILD_NUKE");
                                break;
                            case "TERRAN_HELLION":
                                Possible_Actions.Enqueue("MORPH_HELLBAT");
                                break;
                            case "TERRAN_HELLIONTANK":
                                Possible_Actions.Enqueue("MORPH_HELLION");
                                break;
                            case "TERRAN_SIEGETANK":
                                Possible_Actions.Enqueue("MORPH_SIEGEMODE");
                                break;
                            case "TERRAN_SIEGETANKSIEGED":
                                Possible_Actions.Enqueue("MORPH_UNSIEGE");
                                break;
                            case "TERRAN_VIKINGASSAULT":
                                Possible_Actions.Enqueue("MORPH_VIKINGFIGHTERMODE");
                                break;
                            case "TERRAN_VIKINGFIGHTER":
                                Possible_Actions.Enqueue("MORPH_VIKINGASSAULTMODE");
                                break;
                            case "TERRAN_LIBERATOR":
                                Possible_Actions.Enqueue("MORPH_LIBERATORAGMODE");
                                break;
                            case "TERRAN_LIBERATORAG":
                                Possible_Actions.Enqueue("MORPH_LIBERATORAAMODE");
                                break;
                            case "TERRAN_RAVEN":
                                Possible_Actions.Enqueue("EFFECT_AUTOTURRET");
                                break;
                            case "TERRAN_ORBITALCOMMAND":
                                Possible_Actions.Enqueue("EFFECT_CALLDOWNMULE");
                                break;
                            default:
                                break;
                        }
                    }             
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
