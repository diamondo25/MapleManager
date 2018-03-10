using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleManager.WzTools.Objects;

namespace MapleManager.WzTools
{
    static class ExtraTools
    {
        public static int IMGNameToID(this WzProperty property)
        {
            return int.Parse(property.Name.Replace(".img", ""), NumberStyles.Number);
        }
    }
}
