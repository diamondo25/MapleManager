using MapleManager.Controls;

namespace MapleManager.WzTools
{
    class NameSpaceNode
    {
        public int OffsetInFile { get; set; }
        public int Checksum { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public WZTreeNode TreeNode { get; set; }
    }
}
