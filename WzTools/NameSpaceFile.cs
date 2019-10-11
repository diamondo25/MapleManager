using System;
using System.IO;
using MapleManager.WzTools.Helpers;
using MapleManager.WzTools.Objects;

namespace MapleManager.WzTools
{
    class NameSpaceFile : NameSpaceNode
    {
        public virtual ArchiveReader GetReader()
        {
            return null;
        }

        public override string ToString()
        {
            return "File: " + Name;
        }

        protected PcomObject _obj = null;

        public PcomObject Object
        {
            get
            {
                if (_obj == null)
                {
                    var reader = GetReader();
                    PcomObject.PrepareEncryption(reader);
                    _obj = PcomObject.LoadFromBlob(reader, (int)Size, null, true);
                    if (_obj != null && _obj is WzFileProperty wfp)
                    {
                        wfp.Name = Name;
                        wfp.FileNode = this;

                        if (false)
                        {
                            using (var fw = File.OpenWrite(Path.Combine(Environment.CurrentDirectory, "output.img")))
                            using (var aw = new ArchiveWriter(fw))
                            {
                                PcomObject.WriteToBlob(aw, _obj);
                            }
                        }

                    }
                }
                return _obj;
            }
            set => _obj = value;
        }

        public override object GetChild(string key) => Object?[key];
        public override bool HasChild(string key) => Object?.HasChild(key) ?? false;
    }
}
