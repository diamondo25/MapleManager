using System;
using System.Collections.Generic;
using System.Text;

namespace MapleManager.WzTools
{
    public interface INameSpaceNode
    {
        string GetName();
        object GetParent();
        object GetChild(string key);
        bool HasChild(string key);
    }
}
