using System;
using System.IO;
using MapleManager.WzTools.Helpers;
using MapleManager.WzTools.Objects;
using MapleManager.WzTools.Package;

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

        public IWzEncryption UsedEncryption = null;

        public PcomObject Object
        {
            get
            {
                if (_obj == null)
                {
                    var reader = GetReader();
                    reader.BaseStream.Position = OffsetInFile;
                    
                    PcomObject.PrepareEncryption(reader);
                    
                    _obj = PcomObject.LoadFromBlob(reader, Size, null, true);

                    if (_obj != null && _obj is WzFileProperty wfp)
                    {
                        UsedEncryption = reader.GetCurrentEncryption();
                        wfp.Name = Name;
                        wfp.FileNode = this;
                    }
                }
                return _obj;
            }
            set => _obj = value;
        }


        public void Unload()
        {
            _obj = null;
        }

        public override object GetChild(string key) => Object?[key];
        public override bool HasChild(string key) => Object?.HasChild(key) ?? false;
    }
}
