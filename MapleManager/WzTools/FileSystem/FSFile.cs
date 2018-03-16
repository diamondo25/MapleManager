using System.IO;
using System.IO.MemoryMappedFiles;

namespace MapleManager.WzTools.FileSystem
{
    class FSFile : NameSpaceFile
    {
        public string RealPath { get; set; }

        public override BinaryReader GetReader()
        {
            var mmf = MemoryMappedFile.CreateFromFile(
                RealPath,
                FileMode.Open
                );
            return new BinaryReader(mmf.CreateViewStream());
        }

        public void Unload()
        {
            _obj = null;
        }
    }
}
