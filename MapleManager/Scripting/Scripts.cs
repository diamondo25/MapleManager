using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace MapleManager.Scripting
{
    public class Scripts
    {
        private static CodeDomProvider compiler =
            new CSharpCodeProvider(new Dictionary<string, string> {{"CompilerVersion", "v4.0"}});
        public static CompilerResults CompileScript(params string[] sources)
        {
            var parms = new CompilerParameters()
            {

                // Configure parameters
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = true,
            };

            var mainPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var r in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                var filename = r.Name + ".dll";
                if (File.Exists(Path.Combine(mainPath, filename)))
                {
                    filename = Path.Combine(mainPath, r.Name + ".dll");
                }
                parms.ReferencedAssemblies.Add(filename);
            }
            
            parms.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

            return compiler.CompileAssemblyFromFile(parms, sources);
        }

        public static T FindInterface<T>(Assembly DLL)
        {
            string InterfaceName = typeof(T).Name;
            // Loop through types looking for one that implements the given interface
            foreach (Type t in DLL.GetTypes())
            {
                if (t.GetInterface(InterfaceName, true) != null)
                    return (T)DLL.CreateInstance(t.FullName);
            }

            return default(T);
        }
    }
}
