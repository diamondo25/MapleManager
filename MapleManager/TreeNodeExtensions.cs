using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleManager
{
    public static class TreeNodeExtensions
    {
        public static string GetFullPath(this TreeNode tn)
        {
            TreeNode curNode = tn;
            string tree = "";

            while (curNode != null)
            {
                string label = curNode.Name == "" ? curNode.Text : curNode.Name;
                if (tree == "") tree = label;
                else tree = label + tn.TreeView.PathSeparator + tree;

                // Last element in tree
                if (tn.TreeView.Nodes.Contains(curNode)) break;
                curNode = curNode.Parent;
            }

            return tree;
        }
    }
}
