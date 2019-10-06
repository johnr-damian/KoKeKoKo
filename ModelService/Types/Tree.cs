using System;
using System.Collections.Generic;

namespace ModelService.Types
{
    public abstract class Tree
    {
        private string _raw_information = default(string);

        private bool _shouldkeeprunning = default(bool);

        private Node Failsafe_Node = default(Node);

        private Node Current_Node = default(Node);

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
        public abstract IEnumerable<string> GenerateAction();
    }
}