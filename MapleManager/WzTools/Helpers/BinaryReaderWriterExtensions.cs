using System;
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


        public static void JumpAndReturn(this BinaryReader reader, int offset, Action andNow)
        {
            var prevPos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            andNow();
            reader.BaseStream.Position = prevPos;
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

        private static string DecodeStringASCII(this BinaryReader reader, sbyte len)
        {
            int actualLen;
            if (len == -128) actualLen = reader.ReadInt32();
            else actualLen = -len;

            byte mask = 0xAA;
            var bytes = reader.ReadBytes(actualLen);

            for (var i = 0; i < actualLen; i++)
            {
                bytes[i] ^= mask;
                mask++;
            }

            WzEncryption.TryDecryptString(bytes, y => !y.Any(x => x < 0x20 && x != '\n' && x != '\r' && x != '\t'));
            
            return Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, bytes));
        }

        private static string DecodeStringUnicode(this BinaryReader reader, sbyte len)
        {
            int actualLen = len;
            if (len == 127) actualLen = reader.ReadInt32();
            actualLen *= 2;

            ushort mask = 0xAAAA;
            var bytes = reader.ReadBytes(actualLen);
            for (var i = 0; i < actualLen; i += 2)
            {
                bytes[i + 0] ^= (byte)(mask & 0xFF);
                bytes[i + 1] ^= (byte)((mask >> 8) & 0xFF);
                mask++;
            }

            WzEncryption.TryDecryptString(bytes, null);

            return Encoding.Unicode.GetString(bytes);
        }
        
    }
}
