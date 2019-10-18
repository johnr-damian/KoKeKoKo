using System;
using System.Collections.Generic;
using ModelService.ValueTypes;

namespace ModelService.Types
{
    public abstract class Tree
    {
        protected string _raw_information = default(string);

        protected bool _shouldkeeprunning = default(bool);

        protected Node Failsafe_Node = default(Node);

        protected Node Current_Node = default(Node);

        public Tree(string raw_information)
        {
            _raw_information = raw_information;
            _shouldkeeprunning = true;
        }

        /// <summary>
        /// Builds the tree and continuously sends a node for action
        /// </summary>
        /// <remarks>
        /// Equivalent to BuildTree function
        /// </remarks>
        /// <returns></returns>
        public abstract IEnumerable<Tuple<string, CostWorth>> GenerateAction();
    }
}