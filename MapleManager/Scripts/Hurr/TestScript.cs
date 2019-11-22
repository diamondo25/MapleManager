using System;

namespace MapleManager.Scripts
{
    class TestScript2 : IScript
    {
        public string GetScriptName()
        {
            return "TestScript";
        }

        public void Start(ScriptNode mainScriptNode)
        {
            var weaponNode = mainScriptNode.GetNode("Skill/3300.img/skill/33001000/weapon");
            weaponNode.SetString("Boobs.");
            weaponNode.UpdateTreeNodes();

            return;

            Console.WriteLine("Starting...");
            foreach (var mapNode in mainScriptNode.GetNode("Map/Map"))
            {
                Console.WriteLine(string.Format("{0}", mapNode.Name));
                if (!mapNode.Name.StartsWith("Map")) continue;

                foreach (var actualMap in mapNode)
                {
                    Console.WriteLine(string.Format("- {0}", actualMap.Name));

                    var infoNode = actualMap["info"];
                    if (infoNode == null)
                    {
                        Console.WriteLine("Missing info node?");
                    }
                    else
                    {
                        object x;
                        if ((x = infoNode["link"]) != null) Console.WriteLine("Link: " + x);
                    }
                }
            }
        }

        public void Stop()
        {
        }
    }
}
