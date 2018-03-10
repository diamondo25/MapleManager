using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleManager
{
    public interface IScript
    {
        string GetScriptName();
        void Start(ScriptNode mainScriptNode);
        void Stop();
    }
}
