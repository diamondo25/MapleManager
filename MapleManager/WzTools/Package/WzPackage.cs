using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MapleManager.WzTools.Package
{
    class WzPackage : WzNameSpace
    {
        public string PackagePath { get; private set; }
        public string PackageKey { get; private set; }

        public Func<int, uint, int> CalculateOffset { get; set; }

        private BinaryReader Reader { get; set; }
        private uint InternalKey { get; set; }
        private byte InternalHash { get; set; }
        private int ContentsStart { get; set; }

        public WzPackage(string packagePath, string packageKey)
        {
            Selftest();

            // TODO: Ignore Offset field and just load it sequentually (doesnt require the key)
            PackagePath = packagePath;
            PackageKey = packageKey;
            CalculateOffset = DecodeOffset;

            Name = Path.GetFileName(packagePath);
            OffsetInFile = 0;
            Checksum = 0;
        }

        private void Selftest()
        {
            ContentsStart = 60;
            InternalKey = 53076;
            var result = DecodeOffset(82, 3901751436);

            Debug.Assert(result == 983);
        }

        private static void StringKeyToValues(string input, out byte keyHash, out uint key)
        {
            key = 0;
            foreach (var c in input)
            {
                key = ' ' * key + c + 1;
            }

            keyHash = 0xFF;
            keyHash ^= (byte)((key >> 24) & 0xFF);
            keyHash ^= (byte)((key >> 16) & 0xFF);
            keyHash ^= (byte)((key >> 8) & 0xFF);
            keyHash ^= (byte)((key >> 0) & 0xFF);
        }

        private int ReadCompressedInt() => Reader.ReadCompressedInt();

        private void JumpAndReturn(int offset, Action andNow) => Reader.JumpAndReturn(offset, andNow);
        private T JumpAndReturn<T>(int offset, Func<T> andNow) => Reader.JumpAndReturn(offset, andNow);

        private string ReadDeDuplicatedString(bool readByte)
        {
            var off = Reader.ReadInt32();

            off += ContentsStart;

            return JumpAndReturn(off, () => ReadString(readByte));
        }


        private string ReadString(bool readByte)
        {
            if (readByte && Reader.ReadByte() == 0)
            {
                return "";
            }

            // unicode/ascii switch
            var len = Reader.ReadSByte();
            if (len == 0) return "";

            var unicode = len > 0;

            if (unicode) return DecodeStringUnicode(len);
            else return DecodeStringASCII(len);
        }

        private string DecodeStringASCII(sbyte len)
        {
            int actualLen;
            if (len == -128) actualLen = Reader.ReadInt32();
            else actualLen = -len;

            byte mask = 0xAA;
            var decoded = Reader.ReadBytes(actualLen).Select(x =>
            {
                x ^= mask;
                mask++;
                return x;
            });

            return Encoding.ASCII.GetString(decoded.ToArray());
        }

        private string DecodeStringUnicode(sbyte len)
        {
            int actualLen = len;
            if (len == 127) actualLen = Reader.ReadInt32();
            actualLen *= 2;

            ushort mask = 0xAAAA;
            var bytes = Reader.ReadBytes(actualLen);
            for (var i = 0; i < actualLen; i += 2)
            {
                bytes[i + 0] ^= (byte)(mask & 0xFF);
                bytes[i + 1] ^= (byte)((mask >> 8) & 0xFF);
                mask++;
            }

            return Encoding.Unicode.GetString(bytes.ToArray());
        }

        private uint ROL(uint value, byte times) => value << times | value >> (32 - times);

        public int DecodeOffset(int currentPosition, uint encryptedOffset)
        {
            var offset = (uint)currentPosition;
            offset = (uint)(offset - ContentsStart) ^ 0xFFFFFFFF;
            offset *= InternalKey;

            offset -= 0x581C3F6D;
            offset = ROL(offset, (byte)(offset & 0x1F));

            offset ^= encryptedOffset;
            offset += (uint)(ContentsStart * 2);
            return (int)offset;
        }

        private int ReadOffset()
        {
            var pos = (int)Reader.BaseStream.Position;
            return CalculateOffset(pos, Reader.ReadUInt32());
        }

        private void ProcessDirectory(NameSpaceDirectory currentDirectory)
        {
            var nodes = ReadCompressedInt();
            for (var i = 0; i < nodes; i++)
            {
                var type = Reader.ReadByte();
                Debug.Assert(type <= 4, "Invalid type found while parsing directory.");
                var isDir = (type & 1) == 1;

                NameSpaceNode node = isDir ? new NameSpaceDirectory() : (NameSpaceNode)new NameSpaceFile();
                
                node.Name = type <= 2 ? ReadDeDuplicatedString(true) : ReadString(false);

                node.Size = ReadCompressedInt();
                node.Checksum = ReadCompressedInt();
                node.OffsetInFile = ReadOffset();


                Debug.Assert(node.OffsetInFile <= Reader.BaseStream.Length, "Offset out of file bounds");
                
                currentDirectory.Add(node);
            }

            foreach (var subDirectory in currentDirectory.SubDirectories)
            {
                JumpAndReturn(subDirectory.OffsetInFile, () => ProcessDirectory(subDirectory));
            }
        }

        public void Process()
        {
            Reader = new BinaryReader(File.OpenRead(PackagePath));
            Size = (int)Reader.BaseStream.Length;

            var pkg1 = Reader.ReadChars(4);
            if (pkg1[0] != 'P' ||
                pkg1[1] != 'K' ||
                pkg1[2] != 'G' ||
                pkg1[3] != '1')
                throw new Exception("This is not a WZ package (mismatch header, expected PKG1)");

            var size = Reader.ReadInt32();
            if (Reader.ReadInt32() != 0) throw new Exception("Expected 0 after size");

            ContentsStart = Reader.ReadInt32();
            // Descriptor. idc

            Reader.BaseStream.Position = ContentsStart;
            var hash = Reader.ReadByte();


            StringKeyToValues(PackageKey, out var keyHash, out var key);

            if (keyHash != hash && keyHash != -hash)
            {
                throw new Exception($"Invalid package key. Hash mismatch. File: {hash:X8}, calculated {keyHash:X8}");
            }

            InternalKey = key;
            InternalHash = keyHash;

            // Okay, everything ready. Lets go.

            if (keyHash == -hash)
            {
                var str = ReadDeDuplicatedString(false);
                Trace.WriteLine(str);
            }
            else
            {
                var str = ReadString(false);
                Trace.Write(str);
            }

            ProcessDirectory(this);
        }

        public void Extract(string outputFolder)
        {
            ExtractDirectory(this, outputFolder);
        }


        private static byte[] extractBuffer = new byte[4096];
        private const int MaxBufferSize = 0x800000;
        public void ExtractDirectory(NameSpaceDirectory pd, string currentOutputFolder)
        {
            var dir = Path.Combine(currentOutputFolder, pd.Name);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            foreach (var subDirectory in pd.SubDirectories)
            {
                ExtractDirectory(subDirectory, dir);
            }

            foreach (var file in pd.Files)
            {
                Reader.BaseStream.Position = file.OffsetInFile;
                using (var fs = new FileStream(Path.Combine(dir, file.Name), FileMode.Create))
                {
                    if (extractBuffer.Length < MaxBufferSize && file.Size > extractBuffer.Length)
                    {
                        // Figure out if we can expand the size
                        var len = extractBuffer.Length;
                        while (len > file.Size)
                        {
                            if (len >= MaxBufferSize) break;
                            len *= 2;
                        }

                        if (len > extractBuffer.Length)
                            Array.Resize(ref extractBuffer, len);
                    }

                    var bufferSize = Math.Min(file.Size, extractBuffer.Length);


                    for (var pos = 0; pos < file.Size; pos += bufferSize)
                    {
                        var blobSize = Math.Min(file.Size - pos, bufferSize);
                        Reader.Read(extractBuffer, 0, blobSize);
                        fs.Write(extractBuffer, 0, blobSize);
                    }
                }
            }
        }
    }
}
