using System;
using System.IO;
using MapleManager.WzTools.Objects;

namespace MapleManager.WzTools
{
    class NameSpaceFile : NameSpaceNode
    {
        [Flags]
        public enum ProcessingResult
        {
            ValidatedMaps = 1,

        }

        public ProcessingResult Processed { get; set; }

        public virtual BinaryReader GetReader()
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
                    _obj = PcomObject.LoadFromBlob(reader, (int)reader.BaseStream.Length);
                    if (_obj != null)
                        _obj.Name = Name;
                }
                return _obj;
            }
        }
    }
}
