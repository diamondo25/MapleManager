using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzList : PcomObject, IEnumerable<PcomObject>
    {
        protected object _obj = null;
        public int ChildCount { get; private set; }
        public bool IsArray { get; private set; }


        public override void Read(BinaryReader reader)
        {
            if (reader.ReadByte() != 0)
            {
                throw  new Exception("Expected 0");
            }

            var isArray = reader.ReadByte() != 0;
            IsArray = isArray;
            var childCount = reader.ReadCompressedInt();
            ChildCount = childCount;
            var data = new PcomObject[childCount];

            for (int i = 0; i < childCount; i++)
            {
                data[i] = LoadFromBlob(reader);
                if (data[i] is PcomObject po) po.Parent = this;
            }

            if (isArray) _obj = data;
            else _obj = data.ToList();
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.Write((byte)0);
            writer.Write((byte)(IsArray ? 1 : 0));
            writer.WriteCompressedInt(ChildCount);

            foreach (var o in this)
            {
                WriteToBlob(writer, o);
            }
        }

        public override void Set(string key, object value)
        {
            if (!(value is PcomObject obj)) throw new InvalidDataException();

            if (int.TryParse(key, out var x))
            {
                if (_obj is PcomObject[] arr) arr[x] = obj;
                else if (_obj is List<PcomObject> list) list[x] = obj;
                else throw new InvalidDataException();
            }
            else
            {
                throw new InvalidDataException();
            }
        }

        public override object Get(string key)
        {
            if (int.TryParse(key, out var x))
            {
                if (_obj is PcomObject[] arr) return arr[x];
                else if (_obj is List<PcomObject> list) return list[x];
                else throw new InvalidDataException();
            }
            else
            {
                throw new InvalidDataException();
            }
        }

        public override void Rename(string key, string newName)
        {
            throw new NotImplementedException();
        }


        public IEnumerator<PcomObject> GetEnumerator()
        {
            if (_obj is PcomObject[] arr) return arr.GetEnumerator() as IEnumerator<PcomObject>;
            else if (_obj is List<PcomObject> list) return list.GetEnumerator();
            else throw new InvalidDataException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_obj is PcomObject[] arr) return arr.GetEnumerator();
            else if (_obj is List<PcomObject> list) return list.GetEnumerator();
            else throw new InvalidDataException();
        }
    }
}
