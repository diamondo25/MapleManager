using System.IO;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    class LazyPcomObject<T> : PcomObject where T : PcomObject, new()
    {
        private ArchiveReader lazyReader;
        public bool Loaded { get; set; } = false;
        private PcomObject _actualObject = null;
        public int OffsetInFile { get; set; }

        public LazyPcomObject(ArchiveReader reader)
        {
            lazyReader = reader;
            OffsetInFile = (int)reader.BaseStream.Length;
        }

        public override void Read(ArchiveReader reader)
        {
            
        }

        public override void Write(ArchiveWriter writer)
        {
            TryLoad();
            _actualObject.Write(writer);
        }

        private void TryLoad()
        {
            if (Loaded) return;
            _actualObject = new T();
            lazyReader.JumpAndReturn(OffsetInFile, () => _actualObject.Read(lazyReader));
            lazyReader = null;
            Loaded = true;
        }

        public override void Set(string key, object value)
        {
            TryLoad();
            _actualObject.Set(key, value);
        }

        public override object Get(string key)
        {
            TryLoad();
            return _actualObject.Get(key);
        }
    }
}
