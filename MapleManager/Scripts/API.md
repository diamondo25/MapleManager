A script file or folder is required to define a class inheriting
IScript:

```csharp
public interface IScript
{
	string GetScriptName();
	void Start(ScriptNode mainScriptNode);
	void Stop();
}
```

## ScriptNode
This object is useful for retrieving data in the tree.

```csharp
public abstract class ScriptAPI : IEnumerable<ScriptNode>
{
	public abstract ScriptNode GetNode(string path);

	public Int8 GetInt8(string path) => GetNode(path).ToInt8();
	public Int16 GetInt16(string path) => GetNode(path).ToInt16();
	public Int32 GetInt32(string path) => GetNode(path).ToInt32();
	public Int64 GetInt64(string path) => GetNode(path).ToInt64();

	public UInt8 GetUInt8(string path) => GetNode(path).ToUInt8();
	public UInt16 GetUInt16(string path) => GetNode(path).ToUInt16();
	public UInt32 GetUInt32(string path) => GetNode(path).ToUInt32();
	public UInt64 GetUInt64(string path) => GetNode(path).ToUInt64();

	public string GetString(string path) => GetNode(path).ToString();

	public abstract IEnumerator<ScriptNode> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

For example, to iterate over all maps:
```csharp
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
```