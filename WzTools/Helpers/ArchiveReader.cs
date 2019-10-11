using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MapleManager.WzTools.Package;

namespace MapleManager.WzTools.Helpers
{
    public class ArchiveReader : BinaryReader
    {
        private WzEncryption _encryption = new WzEncryption();

        public bool HasCurrentCrypto => _encryption.HasCurrentCrypto;
        public void SetEncryption(IWzEncryption crypto) => _encryption.ForceCrypto(crypto, true);
        public void LockCurrentEncryption() => _encryption.ForceCurrentCrypto();



        public ArchiveReader(Stream output) : base(output)
        {
        }

        public string ReadString(bool deduplicated, int contentsStart = 0)
        {
            if (!deduplicated)
            {
                return DecodeString();
            }
            else
            {
                return ReadDeDuplicatedString(contentsStart);
            }
        }

        public string ReadString(byte id, byte existingID, byte newID, int contentsStart = 0)
        {
            if (id == newID)
            {
                return DecodeString();
            }
            else if (id == existingID)
            {
                return ReadDeDuplicatedString(contentsStart);
            }

            throw new Exception($"Unknown ID. Expected {existingID} or {newID}, but got {id}.");
        }

        public string ReadString(byte existingID, byte newID, int contentsStart = 0)
        {
            var p = ReadByte();
            return ReadString(p, existingID, newID, contentsStart);
        }

        private string ReadDeDuplicatedString(int contentsStart = 0)
        {
            var off = ReadInt32();

            off += contentsStart;

            return this.JumpAndReturn(off, DecodeString);
        }


        private string DecodeString()
        {
            // unicode/ascii switch
            var len = ReadSByte();
            if (len == 0) return "";

            var unicode = len > 0;

            if (unicode) return DecodeStringUnicode(len);
            else return DecodeStringASCII(len);
        }

        public static bool IsLegalUnicode(string str)
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

        private string DecodeStringASCII(sbyte len)
        {
            int actualLen;
            if (len == -128) actualLen = ReadInt32();
            else actualLen = -len;

            var bytes = ReadBytes(actualLen).ApplyStringXor(false);

            _encryption.TryDecryptString(bytes, y =>
            {
                var oddCharacters = y.Any(x => x < 0x20 && x != '\n' && x != '\r' && x != '\t');
                if (oddCharacters) return false;

                if (IsLegalUnicode(Encoding.UTF8.GetString(y))) return true;

                var converted = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, y);
                var specialCharacterCount = converted.Count(x => x == '?');
                if (specialCharacterCount * 100 / converted.Length >= 50)
                {
                    if (converted.Length > 5)
                    {
                        Console.WriteLine("Found {0} special characters on a string of {1} characters... wut.",
                            specialCharacterCount, converted.Length);
                    }

                    return false;
                }

                return true;
            }, true);

            return Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, bytes));
        }

        private string DecodeStringUnicode(sbyte len)
        {
            int actualLen = len;
            if (len == 127) actualLen = ReadInt32();
            actualLen *= 2;

            var bytes = ReadBytes(actualLen).ApplyStringXor(true);

            _encryption.TryDecryptString(bytes, null, false);

            return Encoding.Unicode.GetString(bytes);
        }

        public void TryDecryptImage(byte[] contents) => _encryption.TryDecryptImage(contents);
    }
}
