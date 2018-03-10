using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MapleManager.Controls
{
    public class TreeViewFast : TreeView
    {
        private readonly Dictionary<int, TreeNode> _treeNodes = new Dictionary<int, TreeNode>();

        /// <summary>
        /// Load the TreeView with items.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="items">Collection of items</param>
        /// <param name="getId">Function to parse Id value from item object</param>
        /// <param name="getParentId">Function to parse parentId value from item object</param>
        /// <param name="getDisplayName">Function to parse display name value from item object. This is used as node text.</param>
        public void LoadItems<T>(IEnumerable<T> items, Func<T, int> getId, Func<T, int?> getParentId, Func<T, string> getDisplayName)
        {
            // Clear view and internal dictionary
            Nodes.Clear();
            _treeNodes.Clear();

            // Load internal dictionary with nodes
            foreach (var item in items)
            {
                var id = getId(item);
                var displayName = getDisplayName(item);
                var node = new TreeNode { Name = id.ToString(), Text = displayName, Tag = item };
                _treeNodes.Add(getId(item), node);
            }

            // Create hierarchy and load into view
            foreach (var id in _treeNodes.Keys)
            {
                var node = GetNode(id);
                var obj = (T)node.Tag;
                var parentId = getParentId(obj);

                if (parentId.HasValue)
                {
                    var parentNode = GetNode(parentId.Value);
                    parentNode.Nodes.Add(node);
                }
                else
                {
                    Nodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Get a handle to the object collection.
        /// This is convenient if you want to search the object collection.
        /// </summary>
        public IQueryable<T> GetItems<T>()
        {
            return _treeNodes.Values.Select(x => (T)x.Tag).AsQueryable();
        }

        /// <summary>
        /// Retrieve TreeNode by Id.
        /// Useful when you want to select a specific node.
        /// </summary>
        /// <param name="id">Item id</param>
        public TreeNode GetNode(int id)
        {
            return _treeNodes[id];
        }

        /// <summary>
        /// Retrieve item object by Id.
        /// Useful when you want to get hold of object for reading or further manipulating.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="id">Item id</param>
        /// <returns>Item object</returns>
        public T GetItem<T>(int id)
        {
            return (T)GetNode(id).Tag;
        }


        /// <summary>
        /// Get parent item.
        /// Will return NULL if item is at top level.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="id">Item id</param>
        /// <returns>Item object</returns>
        public T GetParent<T>(int id) where T : class
        {
            var parentNode = GetNode(id).Parent;
            return parentNode == null ? null : (T)Parent.Tag;
        }

        /// <summary>
        /// Retrieve descendants to specified item.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="id">Item id</param>
        /// <param name="deepLimit">Number of generations to traverse down. 1 means only direct children. Null means no limit.</param>
        /// <returns>List of item objects</returns>
        public List<T> GetDescendants<T>(int id, int? deepLimit = null)
        {
            var node = GetNode(id);
            var enumerator = node.Nodes.GetEnumerator();
            var items = new List<T>();

            if (deepLimit.HasValue && deepLimit.Value <= 0)
                return items;

            while (enumerator.MoveNext())
            {
                // Add child
                var childNode = (TreeNode)enumerator.Current;
                var childItem = (T)childNode.Tag;
                items.Add(childItem);

                // If requested add grandchildren recursively
                var childDeepLimit = deepLimit.HasValue ? deepLimit.Value - 1 : (int?)null;
                if (!deepLimit.HasValue || childDeepLimit > 0)
                {
                    var childId = int.Parse(childNode.Name);
                    var descendants = GetDescendants<T>(childId, childDeepLimit);
                    items.AddRange(descendants);
                }
            }
            return items;
        }
    }
}
