using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement.Types
{
    public class POMDPNode : Node
    {
        public POMDPNode(Node parent, Agent player, Agent enemy, params string[] possible_actions) 
            : base(parent, player, enemy, possible_actions) { }

        public override Node Select()
        {
            throw new NotImplementedException();
        }

        protected override void Backpropagate()
        {
            throw new NotImplementedException();
        }

        protected override void Expand()
        {
            throw new NotImplementedException();
        }

        protected override void Simulate()
        {
            throw new NotImplementedException();
        }
    }

    public class POMDP : Tree
    {
        public POMDP(string raw_information) : base(raw_information)
        {
        }

        public override IEnumerable<string> GenerateAction()
        {
            throw new NotImplementedException();
        }
    }
}
