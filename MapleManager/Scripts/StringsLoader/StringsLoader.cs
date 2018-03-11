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

        void GetString(ScriptNode node, string str, out string x)
        {
            var result = node[str];
            if (result == null) x = "-null-";
            else x = result.ToString();
            return;
        }

        int NameToID(ScriptNode node)
        {
            var idStr = node.Name.Replace(".img", "").TrimStart('0');
            if (idStr == "") idStr = "0";
            try
            {
                return int.Parse(idStr);
            }
            catch
            {
                Console.WriteLine("Error in conversion of {0} ({1}). Returning -1", node.Name, idStr);
                return -1;
            }
        }

        public void Start(ScriptNode mainScriptNode)
        {
            Program.MainForm.BeginTreeUpdate();
            var stringsNPC = new Dictionary<int, string>();
            var stringsMob = new Dictionary<int, string>();
            var stringsMap = new Dictionary<int, string>();


            Console.WriteLine("Loading names...");
            string name, streetName, mapName;
            foreach (var node in mainScriptNode.GetNode("String/Mob.img"))
            {
                var id = int.Parse(node.Name);
                GetString(node, "name", out name);
                stringsMob[id] = name;
            }
            foreach (var node in mainScriptNode.GetNode("String/Npc.img"))
            {
                var id = int.Parse(node.Name);
                GetString(node, "name", out name);
                stringsNPC[id] = name;
            }
            foreach (var section in mainScriptNode.GetNode("String/Map.img"))
            {
                foreach (var node in section)
                {
                    var id = int.Parse(node.Name);
                    GetString(node, "streetName", out streetName);
                    GetString(node, "mapName", out mapName);
                    name = "";
                    if (streetName != "") name = streetName;
                    if (mapName != "")
                        name = (name != "" ? name + " - " : "") + mapName;
                    stringsMap[id] = name;
                }
            }

            Console.WriteLine("Mobs: {0}", stringsMob.Count);
            Console.WriteLine("Npcs: {0}", stringsNPC.Count);

            Console.WriteLine("Setting names...");
            foreach (var node in mainScriptNode.GetNode("Mob"))
            {
                var id = NameToID(node);
                if (stringsMob.TryGetValue(id, out name))
                {
                    var tn = node.TryGetTreeNode();
                    if (tn != null) tn.SetAdditionalInfo("ign", name, true);
                }
                else
                {
                    Console.WriteLine("No name for Mob {0}", id);
                }
            }
            foreach (var node in mainScriptNode.GetNode("Npc"))
            {
                var id = NameToID(node);
                if (stringsNPC.TryGetValue(id, out name))
                {
                    var tn = node.TryGetTreeNode();
                    if (tn != null) tn.SetAdditionalInfo("ign", name, true);
                }
                else
                {
                    Console.WriteLine("No name for NPC {0}", id);
                }
            }
            foreach (var mapCategory in mainScriptNode.GetNode("Map/Map"))
            {
                if (!mapCategory.Name.StartsWith("Map")) continue;

                foreach (var node in mapCategory)
                {
                    var id = NameToID(node);
                    if (stringsMap.TryGetValue(id, out name))
                    {
                        var tn = node.TryGetTreeNode();
                        if (tn != null) tn.SetAdditionalInfo("ign", name, true);
                    }
                    else
                    {
                        Console.WriteLine("No name for Map {0}", id);
                    }
                }
            }

            Console.WriteLine("Done.");
            Program.MainForm.EndTreeUpdate();
        }

        public void Stop()
        {
        }
    }
}
