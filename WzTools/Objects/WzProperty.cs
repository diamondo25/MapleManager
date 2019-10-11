using MapleManager.WzTools.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MapleManager.WzTools.Package;
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

        public WzProperty() { }

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
        
        public override bool HasChild(string key) => HasKey(key);
        
        public override void Read(ArchiveReader reader)
        {
            var b = reader.ReadByte();
            if (b != 0)
            {
                reader.BaseStream.Position -= 1;
                _objects = new Dictionary<string, object>();
                // Note: do not use disposing, as it would dispose the stream
                var sr = new StringReader(Encoding.ASCII.GetString(reader.ReadBytes(BlobSize)));
                parse_ascii(sr);
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

                        case WzVariantType.Uint32Variant: obj = (uint)reader.ReadCompressedInt(); break;
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
                        case WzVariantType.Uint64Variant: obj = (ulong)reader.ReadCompressedLong(); break;

                        case WzVariantType.UnknownVariant:
                            // blob
                            int size = reader.ReadInt32();
                            var pos = reader.BaseStream.Position;
                            var actualObject = PcomObject.LoadFromBlob(reader, size, name);
                            if (actualObject == null)
                            {
                                reader.BaseStream.Position = pos;
                                obj = reader.ReadBytes(size);
                            }
                            else
                            {
                                actualObject.Parent = this;
                                obj = actualObject;
                            }
                            reader.BaseStream.Position = pos + size;

                            break;

                        default:
                            throw new Exception($"Unknown type: {type} in property!");
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

                case Double x:
                    writeVariant(WzVariantType.Float64Variant);
                    writer.Write((double)x);
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

                default:
                    throw new Exception($"Unknown type: {obj?.GetType()} in property!");
            }
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.Write((byte)0); // ASCII
            writer.Write((byte)0);

            if (_objects == null)
            {
                writer.WriteCompressedInt(0);
                return;
            }

            writer.WriteCompressedInt(_objects.Count);

            foreach (var o in _objects)
            {
                writer.Write(o.Key, 1, 0);
                WriteObj(writer, o.Value);
            }
        }

        public override void Set(string key, object value)
        {
            if (_objects == null) _objects = new Dictionary<string, object>();
            _objects[key] = value;
            if (value is PcomObject po)
            {
                po.Parent = this;
                po.Name = key;
            }
        }

        public override object Get(string key)
        {
            _objects.TryGetValue(key, out var x);
            return x;
        }
        
        public bool HasMembers => _objects.Count > 0;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();

        #region ASCII Loading

        

        // In the Wvs logic, 's' is the key, and 'v' is the value

        private void parse_ascii(TextReader stream)
        {
            WzProperty currentProperty = this;
            string v = "", s = "";
            while (read_line(stream, out var line))
            {
                parse_line(line, ref s, ref v);

                add_line(ref currentProperty, s, v);
            }
        }

        public static void add_line(ref WzProperty currentProperty, string s, string sv)
        {
            var parent = currentProperty.Parent as WzProperty;
            object v;
            if (s == isBlockStartStop)
            {
                // close brace
                parent.Set(currentProperty.Name, currentProperty);
                // go back to our parent
                currentProperty = parent;
                return;
            }
            else if (sv == isBlockStartStop)
            {
                // open brace
                currentProperty = new WzProperty()
                {
                    Name = s,
                    _objects = new Dictionary<string, object>(),
                    Parent = currentProperty,
                };
                return;
            }
            else if (sv.Length > 0 && sv[0] == isAtSign)
            {
                // Its a UOL whoop
                v = new WzUOL()
                {
                    Name = s,
                    Absolute = false,
                    Path = sv.Substring(1),
                    Parent = currentProperty,
                };
            }
            else
            {
                // Create key-value pair as string
                v = sv; // Use the string as-is
            }

            currentProperty.Set(s, v);
        }


        // Skips all lines that start with #, / or '
        // ' == old comment logic for like Basic, lol
        public static bool read_line(TextReader stream, out string foundLine)
        {
            foundLine = "";
            while (true)
            {
                var line = stream.ReadLine();
                if (line == null)
                {
                    return false;
                }
                line = line.Trim();
                if (line.Length == 0) continue;
                var firstChar = line[0];

                if (firstChar != '#' && firstChar != '/' && firstChar != '\'')
                {
                    foundLine = line;
                    return true;
                }
            }
            return false;
        }


        // Used by '{' and '}'
        private const string isBlockStartStop = "\x07";
        // Used by '@'
        private const char isAtSign = '\x08';

        public static string escape_str(string str)
        {
            // Remove all slashes. lol
            return str.Replace("\\", "");
        }

        public static void parse_line(string line, ref string s, ref string v)
        {
            bool isEscape = false;
            int i = 0;
            for (; i < line.Length; i++)
            {
                if (isEscape)
                    isEscape = false;
                else if (line[i] == '\\')
                    isEscape = true;
                else if (line[i] == '=')
                    break;
            }

            if (i != line.Length)
            {
                // We've got a value

                s = line.Substring(0, i);
                s = s.Trim();

                // skipping null check
                if (s.Length == 1 && s[0] == '{')
                {
                    s = isBlockStartStop;
                }
                s = escape_str(s);
                

                // The code does not check if you actually filled in a variable!

                v = line.Substring(i + 1);
                v = v.Trim();
                // skipping null check

                if (v.Length == 1 && v[0] == '{')
                    v = isBlockStartStop;
                else if (v.Length > 1 && v[0] == '@')
                {
                    // skip the @, replace with the at identifier
                    v = "" + isAtSign + v.Substring(1);
                }


                v = escape_str(v);
            }
            else
            {
                v = null;
                if (line.Length == 1 && line[0] == '}')
                    s = isBlockStartStop;
                else
                    s = escape_str(v);
            }
        }
        #endregion
    }

    public class WzFileProperty : WzProperty
    {
        // This is the reference to the actual 'filesystem'
        public NameSpaceNode FileNode { get; set; }

        public override object GetParent() => FileNode.GetParent();

    }
}
