using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MapleManager.WzTools.Helpers;
using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace MapleManager.WzTools.Objects
{
    public class WzProperty : PcomObject, IEnumerable<KeyValuePair<string, object>>
    {
        protected Dictionary<string, object> _objects;

        public enum WzVariantType
        {
            // https://msdn.microsoft.com/en-us/library/cc237865.aspx
            EmptyVariant = 0,
            Int16Variant = 2,
            Int32Variant = 3,
            Float32Variant = 4,
            Float64Variant = 5,
            CYVariant = 6, // Currency
            DateVariant = 7,
            BStrVariant = 8,
            DispatchVariant = 9,  // In MS terms, sub PcomObject
            BoolVariant = 11, // 16-bit, because 'typedef __int16 VARIANT_BOOL'
            UnknownVariant = 13,
            Int8Variant = 16,
            Uint8Variant = 17,
            Uint16Variant = 18,
            Uint32Variant = 19,
            Int64Variant = 20,
            Uint64Variant = 21,
            // NullVariant  = 1
            // ErrorVariant  = 10
            // VariantVariant  = 12
            // DecimalVariant  = 14
            // .. does not exist

            // IntVariant  = 22
            // UintVariant  = 23
            // VoidVariant  = 24
            // HResultVariant  = 25
            // PtrVariant  = 26
            // SafeArrayVariant  = 27
            // CArrayVariant  = 28
            // UserDefinedVariant  = 29
            // LPStrVariant  = 30
            // LPWStrVariant  = 31
            // RecordVariant  = 32
            // IntPtrVariant  = 33
            // UintPtrVariant  = 34
            // ArrayVariant  = 35
            // ByRefVariant  = 36
        }

        public new object this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public ulong? GetUInt64(string key)
        {
            var obj = _objects[key];
            if (obj == null) return null;
            if (IsCastableToUInt64(obj)) return Convert.ToUInt64(obj);
            if (ulong.TryParse(obj.ToString(), out var parsed)) return parsed;
            throw new Exception($"Don't know how to parse this value: {obj} (key {key})");
        }

        public long GetInt64(string key)
        {
            var obj = _objects[key];
            if (IsCastableToInt64(obj)) return Convert.ToInt64(obj);
            if (long.TryParse(obj.ToString(), out var parsed)) return parsed;
            throw new Exception($"Don't know how to parse this value: {obj} (key {key})");
        }

        private bool IsCastableToInt64(object x)
        {
            return x is short ||
                   x is int ||
                   x is long ||
                   x is sbyte;
        }
        private bool IsCastableToUInt64(object x)
        {
            return x is ushort ||
                   x is uint ||
                   x is ulong ||
                   x is byte;
        }


        public int GetInt32(string key) => (int)GetInt64(key);
        public short GetInt16(string key) => (short)GetInt64(key);
        public sbyte GetInt8(string key) => (sbyte)GetInt64(key);

        public uint GetUInt32(string key) => (uint)GetUInt64(key);
        public ushort GetUInt16(string key) => (ushort)GetUInt64(key);
        public byte GetUInt8(string key) => (byte)GetUInt64(key);

        public string GetString(string key) => this[key].ToString();

        public bool HasKey(string key) => _objects.ContainsKey(key);

        public override void Read(BinaryReader reader)
        {
            if (reader.ReadByte() != 0)
            {
                throw new NotImplementedException("No support for ASCII");
            }
            else
            {
                reader.ReadByte();
                var amount = reader.ReadCompressedInt();
                _objects = new Dictionary<string, object>(amount);
                for (var i = 0; i < amount; i++)
                {
                    var name = reader.ReadString(1, 0, 0);
                    var type = (WzVariantType)reader.ReadByte();

                    if (type == WzVariantType.DispatchVariant)
                        type = WzVariantType.UnknownVariant;

                    object obj = null;
                    switch (type)
                    {
                        case WzVariantType.EmptyVariant: break;

                        case WzVariantType.Uint8Variant: obj = reader.ReadByte(); break;
                        case WzVariantType.Int8Variant: obj = reader.ReadSByte(); break;

                        case WzVariantType.Uint16Variant: obj = reader.ReadUInt16(); break;
                        case WzVariantType.Int16Variant: obj = reader.ReadInt16(); break;
                        case WzVariantType.BoolVariant: obj = reader.ReadInt16() == 0; break;

                        case WzVariantType.Uint32Variant: obj = reader.ReadCompressedInt(); break;
                        case WzVariantType.Int32Variant: obj = reader.ReadCompressedInt(); break;

                        case WzVariantType.Float32Variant:
                            if (reader.ReadByte() == 0x80) obj = reader.ReadSingle();
                            else obj = 0.0f;
                            break;

                        case WzVariantType.Float64Variant:
                            obj = reader.ReadDouble();
                            break;

                        case WzVariantType.BStrVariant:
                            obj = reader.ReadString(1, 0, 0);
                            break;

                        case WzVariantType.DateVariant: obj = DateTime.FromFileTime(reader.ReadInt64()); break;

                        // Currency (CY)
                        case WzVariantType.CYVariant: obj = reader.ReadCompressedLong(); break;
                        case WzVariantType.Int64Variant: obj = reader.ReadCompressedLong(); break;
                        case WzVariantType.Uint64Variant: obj = reader.ReadCompressedLong(); break;

                        case WzVariantType.UnknownVariant:
                            // blob
                            int size = reader.ReadInt32();
                            var pos = reader.BaseStream.Position;
                            var actualObject = PcomObject.LoadFromBlob(reader, size);
                            if (actualObject == null)
                            {
                                reader.BaseStream.Position = pos;
                                obj = reader.ReadBytes(size);
                            }
                            else
                            {
                                actualObject.Parent = this;
                                actualObject.Name = name;
                                obj = actualObject;
                            }
                            reader.BaseStream.Position = pos + size;

                            break;
                    }


                    _objects[name] = obj;
                }
            }
        }

        public static void WriteObj(ArchiveWriter writer, object obj)
        {
            void writeVariant(WzVariantType vt) => writer.Write((byte)vt);

            switch (obj)
            {
                case null: writeVariant(WzVariantType.EmptyVariant); break;
                case bool x:
                    writeVariant(WzVariantType.BoolVariant);
                    writer.Write((short)(x ? 1 : 0));
                    break;

                case UInt8 x:
                    writeVariant(WzVariantType.Uint8Variant);
                    writer.Write(x);
                    break;
                case Int8 x:
                    writeVariant(WzVariantType.Int8Variant);
                    writer.Write(x);
                    break;

                case UInt16 x:
                    writeVariant(WzVariantType.Uint16Variant);
                    writer.Write(x);
                    break;
                case Int16 x:
                    writeVariant(WzVariantType.Int16Variant);
                    writer.Write(x);
                    break;

                case UInt32 x:
                    writeVariant(WzVariantType.Uint32Variant);
                    writer.WriteCompressedInt((int)x);
                    break;
                case Int32 x:
                    writeVariant(WzVariantType.Int32Variant);
                    writer.WriteCompressedInt((int)x);
                    break;

                case Single x:
                    writeVariant(WzVariantType.Float32Variant);
                    if (Math.Abs(x) > 0.0)
                    {
                        writer.Write((byte)0x80);
                        writer.Write(x);
                    }
                    else writer.Write((byte)0);
                    break;
                case string x:
                    writeVariant(WzVariantType.BStrVariant);
                    writer.Write(x, 1, 0);
                    break;

                case DateTime x:
                    writeVariant(WzVariantType.DateVariant);
                    writer.Write((long)x.ToFileTime());
                    break;

                // CYVariant is not handled
                case Int64 x:
                    writeVariant(WzVariantType.Int64Variant);
                    writer.WriteCompressedLong(x);
                    break;
                case UInt64 x:
                    writeVariant(WzVariantType.Uint64Variant);
                    writer.WriteCompressedLong((long)x);
                    break;

                case PcomObject po:
                    writeVariant(WzVariantType.UnknownVariant);
                    writer.Write((int)0);
                    var tmp = writer.BaseStream.Position;

                    WriteToBlob(writer, po);

                    var cur = writer.BaseStream.Position;
                    writer.BaseStream.Position = tmp - 4;
                    writer.Write((int)(cur - tmp));
                    writer.BaseStream.Position = cur;

                    break;
            }
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.Write((byte)0); // ASCII
            writer.Write((byte)0);

            writer.WriteCompressedInt(_objects.Count);

            foreach (var o in _objects)
            {
                writer.Write(o.Key, 1, 0);
                WriteObj(writer, o.Value);
            }
        }

        public override void Set(string key, object value) => _objects[key] = value;

        public override object Get(string key)
        {
            _objects.TryGetValue(key, out var x);
            return x;
        }

        public override void Rename(string key, string newName)
        {
            throw new NotImplementedException();
        }

        public bool HasMembers => _objects.Count > 0;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
    }
}
