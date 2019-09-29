using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Types
{
    public abstract class Tree
    {
        public Node Current_Node { get; protected set; } = default(Node);

        public List<string> Action_Log { get; set; } = default(List<string>);

        public Tree(Player Agent, Player Enemy)
        {
            Current_Node = new Node(null, Agent, Enemy);
            Action_Log = new List<string>();
        }

        public void BuildTree()
        {
            var countdown = DateTime.Now;

            //While not 30 seconds have passed
            while(countdown.Subtract(DateTime.Now).TotalSeconds >= 30)
            {
                //Not expanded
                if(!Current_Node.IsExpanded)
                {
                    Current_Node.Expand();
                }

                Current_Node.Select(); //??
            }

        }
    }
}
