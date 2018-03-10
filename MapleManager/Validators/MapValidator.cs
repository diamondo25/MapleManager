using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleManager.WzTools;
using MapleManager.WzTools.Objects;

namespace MapleManager.Validators
{
    class MapValidator
    {
        public static IEnumerable<string> CheckLife(TreeView tree, WzProperty mainNode)
        {
            if (mainNode == null) yield break;
            if (!(mainNode["life"] is WzProperty lifeNode)) yield break;

            foreach (var listElement in lifeNode)
            {
                var lifeNodeData = listElement.Value as WzProperty;
                var type = (string)lifeNodeData["type"];

                var typeName = "";
                var lookupDir = "";
                switch (type)
                {
                    case "n": typeName = "Npc"; break;
                    case "m": typeName = "Mob"; break;
                    case "r":
                        lookupDir = "Reactor/Reactor.img";
                        typeName = "Reactor";
                        break;
                    default: continue;
                }
                if (lookupDir == "") lookupDir = typeName;

                string id = "";
                var idNode = lifeNodeData["id"];
                if (idNode is string x) id = x;
                else if (idNode is int y) id = y.ToString();
                if (id.Length != 7)
                {
                    yield return $"Invalid ID found for {typeName}: {idNode} (parsed as {id}), type {idNode.GetType()}) (life node {listElement.Key})";
                    continue;
                }


                var path = $"{lookupDir}/{id}.img";
                var lifeExists = tree.FindNode<NameSpaceFile>(path) != null;

                if (!lifeExists)
                {
                    yield return $"Unable to find {typeName} {idNode} (life node {listElement.Key}).";
                }
            }

        }

        public static string GetPathForMap(int mapid)
        {
            return $"Map/Map/Map{mapid / 100000000 % 10}/{mapid:D9}.img";
        }
        
    }
}
