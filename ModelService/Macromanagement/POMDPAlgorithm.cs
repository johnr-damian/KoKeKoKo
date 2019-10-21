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
        public class POMDPAlgorithm : CollectionTypes.Tree
        {
            private class POMDPNode : Node
            {
                /// <summary>
                /// Contains a simulation at a specific time. It also 
                /// contains the current agents.
                /// </summary>
                /// <param name="parent_node"></param>
                /// <param name="owned_agent"></param>
                /// <param name="enemy_agent"></param>
                public POMDPNode(Node parent_node, Agent owned_agent, Agent enemy_agent) 
                    : base(parent_node, owned_agent, enemy_agent) { }

                public override double GetNodeUCTValue()
                {
                    throw new NotImplementedException();
                }

                public override Node SelectAChildNode()
                {
                    if (!IsExpanded)
                        ExpandCurrentNode();

                    double bestuctnode = Double.MinValue;
                    Node bestchildnode = default(POMDPNode);
                    foreach(POMDPNode childnode in Children)
                    {
                        var current_uct = childnode.GetNodeUCTValue();
                        if(current_uct > bestuctnode)
                        {
                            bestuctnode = current_uct;
                            bestchildnode = childnode;
                        }
                    }

                    Chosen_Child = bestchildnode;
                    return Chosen_Child;
                }

                public override void Backpropagate()
                {
                    throw new NotImplementedException();
                }

                protected override void ExpandCurrentNode()
                {
                    var states = String.Join(",", GeneratePotentialStates());
                    var actions = String.Join(",", GeneratePotentialActions());
                    var child = new POMDPNode(this, Current_Owned_Agent.GetDeepCopy(), Current_Enemy_Agent.GetDeepCopy());

                    child.SimulateCurrentNode(null);
                }

                private IEnumerable<string> GeneratePotentialStates()
                {
                    throw new NotImplementedException();
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
                                    Possible_Actions.Enqueue("BUILD_COMMANDCENTER"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_COMMANDCENTER"].Priority, Worth.Mineral + Unit.Values["TERRAN_COMMANDCENTER"].Mineral, Unit.Values["TERRAN_COMMANDCENTER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_COMMANDCENTER"].Supply);
                                    Possible_Actions.Enqueue("BUILD_REFINERY"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_REFINERY"].Priority, Worth.Mineral + Unit.Values["TERRAN_REFINERY"].Mineral, Unit.Values["TERRAN_REFINERY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_REFINERY"].Supply);
                                    Possible_Actions.Enqueue("BUILD_SUPPLYDEPOT"); // Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SUPPLYDEPOT"].Priority, Worth.Mineral + Unit.Values["TERRAN_SUPPLYDEPOT"].Mineral, Unit.Values["TERRAN_SUPPLYDEPOT"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SUPPLYDEPOT"].Supply);
                                    if (unitList.Contains("TERRAN_SUPPLYDEPOT"))
                                        Possible_Actions.Enqueue("BUILD_BARRACKS"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKS"].Priority, Worth.Mineral + Unit.Values["TERRAN_BARRACKS"].Mineral, Unit.Values["TERRAN_BARRACKS"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKS"].Supply)
                                    if (unitList.Contains("TERRAN_BARRACKS"))
                                    {
                                        Possible_Actions.Enqueue("BUILD_BUNKER"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BUNKER"].Priority, Worth.Mineral + Unit.Values["TERRAN_BUNKER"].Mineral, Unit.Values["TERRAN_BUNKER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BUNKER"].Supply);
                                        Possible_Actions.Enqueue("BUILD_GHOSTACADEMY"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_GHOSTACADEMY"].Priority, Worth.Mineral + Unit.Values["TERRAN_GHOSTACADEMY"].Mineral, Unit.Values["TERRAN_GHOSTACADEMY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_GHOSTACADEMY"].Supply);
                                        Possible_Actions.Enqueue("BUILD_FACTORY"); // Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FACTORY"].Priority, Worth.Mineral + Unit.Values["TERRAN_FACTORY"].Mineral, Unit.Values["TERRAN_FACTORYY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_FACTORY"].Supply);
                                    }
                                    if (unitList.Contains("TERRAN_FACTORY"))
                                    {
                                        Possible_Actions.Enqueue("BUILD_ARMORY"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_ARMORY"].Priority, Worth.Mineral + Unit.Values["TERRAN_ARMORY"].Mineral, Unit.Values["TERRAN_ARMORY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_ARMORY"].Supply);
                                        Possible_Actions.Enqueue("BUILD_STARPORT"); //
                                    }
                                    if (unitList.Contains("TERRAN_STARPORT"))
                                        Possible_Actions.Enqueue("BUILD_FUSIONCORE"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORT"].Priority, Worth.Mineral + Unit.Values["TERRAN_STARPORT"].Mineral, Unit.Values["TERRAN_STARPORT"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORT"].Supply);
                                    if (unitList.Contains("TERRAN_COMMANDCENTER"))
                                        Possible_Actions.Enqueue("BUILD_ENGINEERINGBAY"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_ENGINEERINGBAY"].Priority, Worth.Mineral + Unit.Values["TERRAN_ENGINEERINGBAY"].Mineral, Unit.Values["TERRAN_ENGINEERINGBAY"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_ENGINEERINGBAY"].Supply);
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
                                        Possible_Actions.Enqueue("TRAIN_SCV"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SCV"].Priority, Worth.Mineral + Unit.Values["TERRAN_SCV"].Mineral, Unit.Values["TERRAN_SCV"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SCV"].Supply);
                                    if (unitList.Contains("TERRAN_BARRACKS"))
                                        Possible_Actions.Enqueue("MORPH_ORBITALCOMMAND"); //not sure about morphs yet
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
                                    Possible_Actions.Enqueue("TRAIN_MARINE"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MARINE"].Priority, Worth.Mineral + Unit.Values["TERRAN_MARINE"].Mineral, Unit.Values["TERRAN_MARINE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_MARINE"].Supply);
                                    Possible_Actions.Enqueue("TRAIN_REAPER"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_REAPER"].Priority, Worth.Mineral + Unit.Values["TERRAN_REAPER"].Mineral, Unit.Values["TERRAN_REAPER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_REAPER"].Supply);
                                    if (Current_Owned_Agent.Supply >= 1)
                                    {
                                        if (unitList.Contains("TERRAN_BARRACKSTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_MARAUDER"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MARAUDER"].Priority, Worth.Mineral + Unit.Values["TERRAN_MARAUDER"].Mineral, Unit.Values["TERRAN_MARAUDER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_MARAUDER"].Supply);
                                        if (unitList.Contains("TERRAN_GHOSTACADEMY"))
                                            Possible_Actions.Enqueue("TRAIN_GHOST"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_GHOST"].Priority, Worth.Mineral + Unit.Values["TERRAN_GHOST"].Mineral, Unit.Values["TERRAN_GHOST"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_GHOST"].Supply);
                                    }
                                    Possible_Actions.Enqueue("BUILD_BARRACKSTECHLAB"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKSTECHLAB"].Priority, Worth.Mineral + Unit.Values["TERRAN_BARRACKSTECHLAB"].Mineral, Unit.Values["TERRAN_BARRACKSTECHLAB"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKSTECHLAB"].Supply);
                                    Possible_Actions.Enqueue("BUILD_BARRACKSREACTOR"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BARRACKSREACTOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_BARRACKSREACTOR"].Mineral, Unit.Values["TERRAN_BARRACKSREACTOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BARRACKSREACTOR"].Supply);
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
                                else if (Current_Owned_Agent.Minerals >= 50)
                                    if (Current_Owned_Agent.Supply >= 1)
                                        Possible_Actions.Enqueue("TRAIN_MARINE");
                                break;
                            case "TERRAN_BARRACKSTECHLAB":
                                if (Current_Owned_Agent.Minerals >= 100 && Current_Owned_Agent.Vespene >= 100)
                                {
                                    Possible_Actions.Enqueue("RESEARCH_COMBATSHIELD"); //Not sure what value to put on upgrades/researches
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
                                        Possible_Actions.Enqueue("TRAIN_HELLION"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_HELLION"].Priority, Worth.Mineral + Unit.Values["TERRAN_HELLION"].Mineral, Unit.Values["TERRAN_HELLION"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_HELLION"].Supply);
                                        Possible_Actions.Enqueue("TRAIN_WIDOWMINE"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_WIDOWMINE"].Priority, Worth.Mineral + Unit.Values["TERRAN_WIDOWMINE"].Mineral, Unit.Values["TERRAN_WIDOWMINE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_WIDOWMINE"].Supply);
                                        if (unitList.Contains("TERRAN_ARMORY"))
                                        {
                                            Possible_Actions.Enqueue("TRAIN_HELLBAT"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_HELLBAT"].Priority, Worth.Mineral + Unit.Values["TERRAN_HELLBAT"].Mineral, Unit.Values["TERRAN_HELLBAT"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_HELLBAT"].Supply);
                                            if (Current_Owned_Agent.Supply >= 6)
                                                Possible_Actions.Enqueue("TRAIN_THOR"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_THOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_THOR"].Mineral, Unit.Values["TERRAN_THOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_THOR"].Supply);
                                        }
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                    {
                                        if (unitList.Contains("TERRAN_FACTORYTECHLAB"))
                                        {
                                            Possible_Actions.Enqueue("TRAIN_SIEGETANK"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_SIEGETANK"].Priority, Worth.Mineral + Unit.Values["TERRAN_SIEGETANK"].Mineral, Unit.Values["TERRAN_SIEGETANK"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_SIEGETANK"].Supply);
                                            Possible_Actions.Enqueue("TRAIN_CYCLONE"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_CYCLONE"].Priority, Worth.Mineral + Unit.Values["TERRAN_CYCLONE"].Mineral, Unit.Values["TERRAN_CYCLONE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_CYCLONE"].Supply);
                                        }
                                    }
                                    Possible_Actions.Enqueue("BUILD_FACTORYTECHLAB"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FACTORYTECHLAB"].Priority, Worth.Mineral + Unit.Values["TERRAN_FACTORYTECHLAB"].Mineral, Unit.Values["TERRAN_FACTORYTECHLAB"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_FACTORYTECHLAB"].Supply);
                                    Possible_Actions.Enqueue("BUILD_FACTORYREACTOR"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_FACTORYREACTOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_FACTORYREACTOR"].Mineral, Unit.Values["TERRAN_FACTORYREACTOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_FACTORYREACTOR"].Supply);
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
                                        Possible_Actions.Enqueue("TRAIN_VIKINGFIGHTER"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_VIKINGFIGHTER"].Priority, Worth.Mineral + Unit.Values["TERRAN_VIKINGFIGHTER"].Mineral, Unit.Values["TERRAN_VIKINGFIGHTER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_VIKINGFIGHTER"].Supply);
                                        Possible_Actions.Enqueue("TRAIN_MEDIVAC"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_MEDIVAC"].Priority, Worth.Mineral + Unit.Values["TERRAN_MEDIVAC"].Mineral, Unit.Values["TERRAN_MEDIVAC"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_MEDIVAC"].Supply);
                                    }
                                    if (Current_Owned_Agent.Supply >= 3)
                                    {
                                        Possible_Actions.Enqueue("TRAIN_LIBERATOR"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_LIBERATOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_LIBERATOR"].Mineral, Unit.Values["TERRAN_LIBERATOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_LIBERATOR"].Supply);
                                        if (unitList.Contains("TERRAN_STARPORTTECHLAB"))
                                            Possible_Actions.Enqueue("TRAIN_BANSHEE"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_BANSHEE"].Priority, Worth.Mineral + Unit.Values["TERRAN_BANSHEE"].Mineral, Unit.Values["TERRAN_BANSHEE"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_BANSHEE"].Supply);
                                    }
                                    if (unitList.Contains("TERRAN_FUSIONCORE"))
                                        if (Current_Owned_Agent.Supply >= 6)
                                            Possible_Actions.Enqueue("TRAIN_BATTLECRUISER"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_LIBERATOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_BATTLECRUISER"].Mineral, Unit.Values["TERRAN_BATTLECRUISER"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_LIBERATOR"].Supply);
                                    Possible_Actions.Enqueue("BUILD_STARPORTREACTOR"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORTREACTOR"].Priority, Worth.Mineral + Unit.Values["TERRAN_STARPORTREACTOR"].Mineral, Unit.Values["TERRAN_STARPORTREACTOR"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORTREACTOR"].Supply);
                                    Possible_Actions.Enqueue("BUILD_STARPORTTECHLAB"); //Worth += new CostWorth(Worth.Priority + Unit.Values["TERRAN_STARPORTTECHLAB"].Priority, Worth.Mineral + Unit.Values["TERRAN_STARPORTTECHLAB"].Mineral, Unit.Values["TERRAN_STARPORTTECHLAB"].Vespene + Worth.Vespene, Worth.Supply + Unit.Values["TERRAN_STARPORTTECHLAB"].Supply);
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
                                        Possible_Actions.Enqueue("BUILD_NUKE"); //Considered as research but named as build due to being a consumable buff
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
                    var modelservice = Services.ModelRepositoryService.ModelService.GetModelService().REngine;

                    modelservice.Evaluate(@"discount <- 0.75");
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Holds the essential information about the game, and simulates
            /// it using POMDP
            /// </summary>
            /// <param name="owned_agent"></param>
            /// <param name="enemy_agent"></param>
            public POMDPAlgorithm(Types.Agent owned_agent, Types.Agent enemy_agent)
                : base(owned_agent, enemy_agent)
            {
                Root_Node = new POMDPNode(null, owned_agent, enemy_agent);
                Current_Node = Root_Node;
            }

            /// <summary>
            /// Generates a predicted action using POMDP algorithm
            /// </summary>
            /// <param name="endtime"></param>
            /// <returns></returns>
            public override IEnumerable<Tuple<string, CostWorth>> GeneratePredictedAction(DateTime endtime)
            {
                var current_time = DateTime.Now;

                //While it is still not yet the end of the game
                //Keep generating predicted actions
                while (current_time != endtime)
                {
                    //The chosen action that will be executed by the agent
                    var predicted_action = default(Tuple<string, ValueTypes.CostWorth>);

                    try
                    {
                        //Get a child node that has the best move
                        var selected_node = Current_Node.SelectAChildNode();

                        //Get the predicted action
                        predicted_action = selected_node.GetNodeInformation();

                        //Update the Current Node
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
