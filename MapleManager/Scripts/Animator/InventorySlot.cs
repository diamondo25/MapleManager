using System;
using System.Collections.Generic;

namespace MapleManager.Scripts.CharacterGen
{
    public enum InventorySlot
    {
        Body,
        Face,
        Gloves,
        // Like in chainmail
        Mail,
        Pants,
        Hair,
        Weapon,
        Cap,
        Ring,
        Shoes,
        Shield,
    }

    public static class InventorySlotHelpers
    {
        public static InventorySlot? FromName(string name)
        {
            switch (name)
            {
                case "Wp": return InventorySlot.Weapon;
                case "Fc": return InventorySlot.Face;
                case "Gv": return InventorySlot.Gloves;
                case "Ma": return InventorySlot.Mail;
                case "Pn": return InventorySlot.Pants;
                case "Hr": return InventorySlot.Hair;
                case "Cp": return InventorySlot.Cap;
                case "Ri": return InventorySlot.Ring;
                case "So": return InventorySlot.Shoes;
                case "Si": return InventorySlot.Shield;
                default: return null;
            }
        }

        public static IEnumerable<InventorySlot> FromString(string name)
        {
            while (name.Length > 0)
            {
                var curPart = name.Substring(0, 2);
                name = name.Substring(2);

                var x = FromName(curPart);
                if (!x.HasValue)
                {
                    throw new Exception("Unable to process " + curPart + ", unknown inventory slot?");
                }
                yield return x.Value;
            }
        }
    }
}