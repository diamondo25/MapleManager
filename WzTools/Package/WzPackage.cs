using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Package
{
    class WzPackage : WzNameSpace
    {
        public string PackagePath { get; private set; }
        public string PackageKey { get; private set; }

        public Func<int, uint, int> CalculateOffset { get; set; }

        private ArchiveReader Reader { get; set; }
        private uint InternalKey { get; set; }
        private byte InternalHash { get; set; }
        private int ContentsStart { get; set; }

        /// <summary>
        /// deMSwZ probably never cared about the position calculation.
        /// It can function due to the file being written sequentially,
        /// so its very predictable where you have to continue reading.
        /// </summary>
        private bool ReadLike_deMSwZ { get; set; } = false;

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

        private void ReadFirstStringAfterHeader(byte keyHash, uint hash)
        {
            var str = Reader.ReadString(keyHash == ~hash, ContentsStart);
            Trace.WriteLine("StringAfterHeader: " + str);

            // This is the 'end' of the file, beginning of directories
            EndParsePos = (int) Reader.BaseStream.Position;
        }

        private int GetFirstOffset()
        {
            var nodes = ReadCompressedInt();

            // Empty dir? weird.
            if (nodes == 0)
            {
                Console.WriteLine("No nodes found in this WZ file.");
                return 0;
            }

            byte type = Reader.ReadByte();
            Reader.ReadString(type <= 2, ContentsStart + 1);

            ReadCompressedInt(); // size
            ReadCompressedInt(); // checksum

            return ReadOffset();
        }

        private void ProcessDirectory(NameSpaceDirectory currentDirectory)
        {
            var nodes = ReadCompressedInt();
            for (var i = 0; i < nodes; i++)
            {
                var tmp = (int)Reader.BaseStream.Position;
                var type = Reader.ReadByte();
                Debug.Assert(type <= 4, "Invalid type found while parsing directory.");
                var isDir = (type & 1) == 1;

                NameSpaceNode node = isDir ? new NameSpaceDirectory() : (NameSpaceNode)new NameSpaceFile();
                node.BeginParsePos = tmp;

                node.Name = Reader.ReadString(type <= 2, ContentsStart + 1);

                node.Size = ReadCompressedInt();
                node.Checksum = ReadCompressedInt();

                if (ReadLike_deMSwZ)
                {
                    Reader.ReadUInt32(); // Ignore offset
                }
                else
                {
                    node.OffsetInFile = ReadOffset();
                    if (node.OffsetInFile < 0) throw new ArgumentOutOfRangeException("Offset not in file.");
                }

                tmp = (int)Reader.BaseStream.Position;
                node.EndParsePos = tmp;
                currentDirectory.Add(node);

#if DEBUG
                Console.Write(isDir ? "D" : "F");
                Console.Write($" {node.Name,-30}: {node.BeginParsePos,-10} - {node.EndParsePos,-10} -");
                
                Console.WriteLine(isDir ? node.OffsetInFile.ToString() : "");
#endif
            }

            foreach (var subDirectory in currentDirectory.SubDirectories)
            {
                if (ReadLike_deMSwZ)
                {
                    ProcessDirectory(subDirectory);
                }
                else
                {
                    JumpAndReturn(subDirectory.OffsetInFile, () => ProcessDirectory(subDirectory));
                }
            }
        }

        public void Process()
        {
            Reader = new ArchiveReader(File.OpenRead(PackagePath));
            Size = (int)Reader.BaseStream.Length;

            var pkg1 = Reader.ReadChars(4);
            if (pkg1[0] != 'P' ||
                pkg1[1] != 'K' ||
                pkg1[2] != 'G' ||
                pkg1[3] != '1')
                throw new Exception("This is not a WZ package (mismatch header, expected PKG1)");

            var size = Reader.ReadInt32();
            if (Reader.ReadInt32() != 0) throw new Exception("Expected 0 after size (error: 0x80004001)");

            ContentsStart = Reader.ReadInt32();
            // Description, i dont really care. Just continue.

            Reader.BaseStream.Position = ContentsStart;
            var hash = Reader.ReadByte();


            StringKeyToValues(PackageKey, out var keyHash, out var key);


            var tmp = Reader.BaseStream.Position;
            {
                ReadFirstStringAfterHeader(keyHash, hash);

                InternalKey = key;
                InternalHash = keyHash;

                int offset = GetFirstOffset();
                Console.WriteLine(
                    $"Calculated offset {offset}, key {key}, keyHash {keyHash}, hash {hash}, packageKey: {PackageKey}.");

                Reader.BaseStream.Position = tmp;
            }


            if (keyHash != hash && keyHash != ~hash)
            {
                Console.WriteLine($"Invalid package key. Hash mismatch.");

                // try to crack, if PackageKey is a number
                ushort mapleVersion;
                bool found = false;

                if (ushort.TryParse(PackageKey, out mapleVersion))
                {
                    for (ushort i = (ushort)Math.Max(mapleVersion - 200, 0); i < mapleVersion + 500; i++)
                    {
                        StringKeyToValues(i.ToString(), out keyHash, out key);

                        if (keyHash != hash && keyHash != ~hash) continue;

                        Reader.BaseStream.Position = tmp;
                        try
                        {

                            InternalKey = key;
                            InternalHash = keyHash;
                            ReadFirstStringAfterHeader(keyHash, hash);

                            this.SubDirectories.Clear();
                            this.Files.Clear();


                            Console.WriteLine(
                                $"... key {key}, keyHash {keyHash}, hash {hash}, packageKey: {i.ToString()}.");

                            ProcessDirectory(this);

                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Console.WriteLine($"Its not version {i}.");
                            continue;
                        }


                        Console.WriteLine($"Cracked as version {i}.");
                        found = true;
                        return;

                    }


                    Reader.BaseStream.Position = tmp;
                }

                if (!found)
                {
                    if (!ReadLike_deMSwZ)
                    {
                        Console.WriteLine("Continue reading like deMSwZ");
                        ReadLike_deMSwZ = true;
                    }
                }
            }

            InternalKey = key;
            InternalHash = keyHash;

            // Okay, everything ready. Lets go.

            ReadFirstStringAfterHeader(keyHash, hash);
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
