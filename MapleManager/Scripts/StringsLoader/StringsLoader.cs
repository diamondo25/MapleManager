using System;
using System.Collections.Generic;

namespace MapleManager.Scripts
{
    class StringsLoader : IScript
    {
        public string GetScriptName()
        {
            return "StringsLoader";
        }

        public void Start(ScriptNode mainScriptNode)
        {
			var stringsNPC = new Dictionary<int, string>();
			var stringsMob = new Dictionary<int, string>();
			
            Console.WriteLine("Starting... 123");
            foreach (var node in mainScriptNode.GetNode("String/Mob.img"))
            {
				var id = int.Parse(node.Name);
				var name = (node["name"].ToString()) ?? "";
				stringsMob[id] = name;
				// Console.WriteLine("{0}: {1}", id, name);
            }
            
            foreach (var node in mainScriptNode.GetNode("Mob"))
            {
				var id = int.Parse(node.Name.Replace(".img", "").TrimStart('0'));
				string name;
				if (stringsMob.TryGetValue(id, out name)) {
					var tn = node.TryGetTreeNode();
					if (tn != null) tn.Text += "(name: " + name + ")";
				}
				// Console.WriteLine("{0}: {1}", id, name);
            }
        }

        public void Stop()
        {
        }
    }
}
