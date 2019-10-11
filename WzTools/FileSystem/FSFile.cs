using System.IO;
using System.IO.MemoryMappedFiles;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.FileSystem
{
    class FSFile : NameSpaceFile
    {
        public string RealPath { get; set; }
        
        public override ArchiveReader GetReader()
        {
            var mmf = MemoryMappedFile.CreateFromFile(
                RealPath,
                FileMode.Open
                );
            return new ArchiveReader(mmf.CreateViewStream());
        }

        public void Unload()
        {
            _obj = null;
        }
    }
}
