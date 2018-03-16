using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MapleManager.Validators
{
    static class WzTreeView
    {

        private static string[] ExtractPath(string path)
        {
            var pathStack = new Queue<string>();
            foreach (var s in path.Split('/', '\\'))
            {
                switch (s)
                {
                    case "..": pathStack.Dequeue(); break;
                    case ".": continue;
                    default: pathStack.Enqueue(s); break;
                }
            }
            return pathStack.ToArray();
        }

        public static T FindNode<T>(this TreeView tv, string path) where T : class
        {
            var actualNodes = ExtractPath(path);
            
            TreeNode currentNode = null;
            foreach (TreeNode tvNode in tv.Nodes)
            {
                if (string.Equals(tvNode.Name, actualNodes[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    currentNode = tvNode;
                }
            }

            if (currentNode == null) return default(T);

            foreach (var actualNode in actualNodes.Skip(1))
            {
                var found = false;
                foreach (TreeNode node in currentNode.Nodes)
                {
                    if (string.Equals(node.Name, actualNode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        currentNode = node;
                        found = true;
                        break;
                    }
                }
                if (!found) return default(T);
            }

            return currentNode.Tag as T;
        }
    }
}
