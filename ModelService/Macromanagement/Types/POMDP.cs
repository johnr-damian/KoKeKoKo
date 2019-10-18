using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.ValueTypes;

namespace ModelService.Macromanagement.Types
{
    public class POMDPNode : Node
    {
        public Tree Root { get; set; }

        public POMDPNode(Node parent, Agent player, Agent enemy) 
            : base(parent, player, enemy) { }

        public override Node Select()
        {
            Node best = null;
            try
            {
                if (!IsExpanded)
                    Expand();

                double bestuct = Double.MinValue;
                Node bestnode = null;
                foreach (var node in Children)
                {
                    var current_uct = node.UCT;
                    if (current_uct > bestuct)
                    {
                        bestuct = current_uct;
                        bestnode = node;
                    }
                }


                best = bestnode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                best = null;
            }

            return best;
        }

        protected override void Backpropagate()
        {
            throw new NotImplementedException();
        }

        protected override void Expand()
        {
            GeneratePossibleActions();
            var actions = Possible_Actions.ToList();
            var random = Services.ModelRepositoryService.ModelService.GetModelService().RandomEngine;

            var n_actions = random.Next(0, actions.Count);
            var owned_action = random.Next(0, actions.Count);
            var enemy_action = random.Next(0, actions.Count);

            for(int iterator = 0; iterator < n_actions; iterator++)
            {
                var new_player = Player.GetDeepCopy();
                new_player.Chosen_Action = actions[owned_action];
                var new_enemy = Enemy.GetDeepCopy();
                new_enemy.Chosen_Action = actions[enemy_action];
                var node = new POMDPNode(this, new_player, new_enemy);
                node.Simulate();

                Children.Add(node);
            }
            
        }

        public override void Simulate()
        {
            Player.ApplyAction(Player.Chosen_Action);
            Enemy.ApplyAction(Enemy.Chosen_Action);
            Simulated_Runs++;
        }
    }

    public class POMDP : Tree
    {
        public POMDP(string raw_information) 
            : base(raw_information) { }

        public override IEnumerable<Tuple<string, CostWorth>> GenerateAction()
        {
            try
            {
                if(Current_Node == null)
                {
                    var player = new Agent()
                    {
                        Minerals = 50,
                        Vespene = 0,
                        Supply = 15,
                        Workers = 12,
                        Units = new Army(new Unit[]
                        {
                            new Unit(0, "1", "1", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "2", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "3", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "4", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "5", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "6", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "7", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "8", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "9", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "10", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "11", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "12", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "12", "TERRAN_COMMANDCENTER", default(Coordinate), default(string))
                        })
                    };

                    var enemy = new Agent()
                    {
                        Minerals = 50,
                        Vespene = 0,
                        Supply = 15,
                        Workers = 12,
                        Units = new Army(new Unit[]
                        {
                            new Unit(0, "1", "1", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "2", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "3", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "4", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "5", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "6", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "7", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "8", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "9", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "10", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "11", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "12", "TERRAN_SCV", default(Coordinate), default(string)),
                            new Unit(0, "1", "12", "TERRAN_COMMANDCENTER", default(Coordinate), default(string))
                        })
                    };

                    player.Target = enemy;
                    enemy.Target = player;

                    player.Worth = player.Units.GetValueOfArmy();
                    enemy.Worth = enemy.Units.GetValueOfArmy();

                    player.Known_Enemy = enemy.Units;
                    enemy.Known_Enemy = player.Units;



                    Current_Node = new POMDPNode(null, player, enemy);
                }
            }
            catch(Exception ex)
            {

            }

            while(true)
            {
                var selected_node = Current_Node.Select();

                Current_Node = selected_node;

                yield return new Tuple<string, CostWorth>(selected_node.Player.Chosen_Action, selected_node.Player.Worth);
            }
        }
    }
}
