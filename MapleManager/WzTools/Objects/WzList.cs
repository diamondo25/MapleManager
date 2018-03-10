using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleManager.WzTools.Objects
{
    public class WzList : PcomObject, IEnumerable<object>
    {
        protected object _obj = null;
        public int ChildCount { get; private set; }
        public bool IsArray { get; private set; }


        public override void Init(BinaryReader reader)
        {
            if (reader.ReadByte() != 0)
            {
                throw  new Exception("Expected 0");
            }

            var isArray = reader.ReadByte() != 0;
            IsArray = isArray;
            var childCount = reader.ReadCompressedInt();
            ChildCount = childCount;
            var data = new object[childCount];

            for (int i = 0; i < childCount; i++)
            {
                data[i] = LoadFromBlob(reader);
                if (data[i] is PcomObject po) po.Parent = this;
            }

            if (isArray) _obj = data;
            else _obj = data.ToList();
        }

        public override void Set(string key, object value)
        {
            if (int.TryParse(key, out var x))
            {
                if (_obj is object[] arr) arr[x] = value;
                else if (_obj is List<object> list) list[x] = value;
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
                if (_obj is object[] arr) return arr[x];
                else if (_obj is List<object> list) return list[x];
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


        public IEnumerator<object> GetEnumerator()
        {
            if (_obj is object[] arr) return arr.GetEnumerator() as IEnumerator<object>;
            else if (_obj is List<object> list) return list.GetEnumerator();
            else throw new InvalidDataException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_obj is object[] arr) return arr.GetEnumerator();
            else if (_obj is List<object> list) return list.GetEnumerator();
            else throw new InvalidDataException();
        }
    }
}
