using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MapleManager.WzTools.Package;

namespace MapleManager.WzTools
{
    static class BinaryReaderWriterExtensions
    {
        public static int ReadCompressedInt(this BinaryReader reader)
        {
            var x = reader.ReadSByte();
            if (x == -128) return reader.ReadInt32();
            return x;
        }

        public static void WriteCompressedInt(this BinaryWriter writer, int value)
        {
            if (value < -127 || value > 127)
            {
                writer.Write((sbyte)-128);
                writer.Write((int)value);
            }
            else
            {
                writer.Write((sbyte)value);
            }
        }

        public static long ReadCompressedLong(this BinaryReader reader)
        {
            var x = reader.ReadSByte();
            if (x == -128) return reader.ReadInt64();
            return x;
        }

        public static void WriteCompressedLong(this BinaryWriter writer, long value)
        {
            if (value < -127 || value > 127)
            {
                writer.Write((sbyte)-128);
                writer.Write(value);
            }
            else
            {
                writer.Write((sbyte)value);
            }
        }


        public static void ReadAndReturn(this BinaryReader reader, Action andNow)
        {
            reader.JumpAndReturn((int) reader.BaseStream.Position, andNow);
        }

        public static void JumpAndReturn(this BinaryReader reader, int offset, Action andNow)
        {
            var prevPos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            andNow();
            reader.BaseStream.Position = prevPos;
        }

        public static T ReadAndReturn<T>(this BinaryReader reader, Func<T> andNow)
        {
            return reader.JumpAndReturn<T>((int)reader.BaseStream.Position, andNow);
        }

        public static T JumpAndReturn<T>(this BinaryReader reader, int offset, Func<T> andNow)
        {
            var prevPos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            var ret = andNow();
            reader.BaseStream.Position = prevPos;
            return ret;
        }

        public static string ReadString(this BinaryReader reader, bool deduplicated, int contentsStart = 0)
        {
            if (!deduplicated)
            {
                return reader.DecodeString();
            }
            else
            {
                return reader.ReadDeDuplicatedString(contentsStart);
            }
        }

        public static string ReadString(this BinaryReader reader, byte id, byte existingID, byte newID, int contentsStart = 0)
        {
            if (id == newID)
            {
                return reader.DecodeString();
            }
            else if (id == existingID)
            {
                return reader.ReadDeDuplicatedString(contentsStart);
            }

            throw new Exception($"Unknown ID. Expected {existingID} or {newID}, but got {id}.");
        }

        public static string ReadString(this BinaryReader reader, byte existingID, byte newID, int contentsStart = 0)
        {
            var p = reader.ReadByte();
            return reader.ReadString(p, existingID, newID, contentsStart);
        }

        private static string ReadDeDuplicatedString(this BinaryReader reader, int contentsStart = 0)
        {
            var off = reader.ReadInt32();

            off += contentsStart;

            return reader.JumpAndReturn(off, reader.DecodeString);
        }


        private static string DecodeString(this BinaryReader reader)
        {
            // unicode/ascii switch
            var len = reader.ReadSByte();
            if (len == 0) return "";

            var unicode = len > 0;

            if (unicode) return reader.DecodeStringUnicode(len);
            else return reader.DecodeStringASCII(len);
        }

        static bool IsLegalUnicode(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                var uc = char.GetUnicodeCategory(str, i);

                if (uc == UnicodeCategory.Surrogate)
                {
                    // Unpaired surrogate, like  "😵"[0] + "A" or  "😵"[1] + "A"
                    return false;
                }
                else if (uc == UnicodeCategory.OtherNotAssigned)
                {
                    // \uF000 or \U00030000
                    return false;
                }

                // Correct high-low surrogate, we must skip the low surrogate
                // (it is correct because otherwise it would have been a 
                // UnicodeCategory.Surrogate)
                if (char.IsHighSurrogate(str, i))
                {
                    i++;
                }
            }

            return true;
        }

        private static string DecodeStringASCII(this BinaryReader reader, sbyte len)
        {
            int actualLen;
            if (len == -128) actualLen = reader.ReadInt32();
            else actualLen = -len;
            
            var bytes = reader.ReadBytes(actualLen).ApplyStringXor(false);

            WzEncryption.TryDecryptString(bytes, y =>
            {
                var oddCharacters = y.Any(x => x < 0x20 && x != '\n' && x != '\r' && x != '\t');
                if (oddCharacters) return false;

                if (IsLegalUnicode(Encoding.UTF8.GetString(y))) return true;
                
                var converted = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, y);
                var specialCharacterCount = converted.Count(x => x == '?');
                if ( specialCharacterCount * 100 / converted.Length >= 50)
                {
                    if (converted.Length > 5)
                    {
                        Console.WriteLine("Found {0} special characters on a string of {1} characters... wut.",
                            specialCharacterCount, converted.Length);
                    }

                    return false;
                }

                return true;
            });
            
            return Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, bytes));
        }

        private static string DecodeStringUnicode(this BinaryReader reader, sbyte len)
        {
            int actualLen = len;
            if (len == 127) actualLen = reader.ReadInt32();
            actualLen *= 2;

            var bytes = reader.ReadBytes(actualLen).ApplyStringXor(true);

            WzEncryption.TryDecryptString(bytes, null);

            return Encoding.Unicode.GetString(bytes);
        }
    }
}
