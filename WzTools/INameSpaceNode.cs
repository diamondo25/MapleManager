using System;
using System.Collections.Generic;
using System.Text;

namespace MapleManager.WzTools
{
    public interface INameSpaceNode
    {
        object GetParent();
        object GetChild(string key);
        bool HasChild(string key);
    }
}
