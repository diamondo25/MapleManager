using System;
using System.IO;
using MapleManager.WzTools.Helpers;
using MapleManager.WzTools.Objects;

namespace MapleManager.WzTools
{
    class NameSpaceFile : NameSpaceNode
    {
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
                    _obj = PcomObject.LoadFromBlob(reader, (int)Size);
                    if (_obj != null)
                    {
                        _obj.Name = Name;
                        _obj.TreeNode = TreeNode;

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
        }
    }
}
