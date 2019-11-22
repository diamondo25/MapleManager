using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MapleManager.WzTools.Package
{
    public interface IWzEncryption
    {
        void Decrypt(byte[] bytes);
        void Encrypt(byte[] bytes);
    }

    abstract class BaseAesEncryption
    {
        private readonly SymmetricAlgorithm _algo = new AesManaged();
        private readonly ICryptoTransform _transformer;
        private const int BlockSize = 16;
        private byte[] _xorKey = new byte[BlockSize];

        protected BaseAesEncryption(byte[] userKey, byte[] iv)
        {
            // It looks like maple has OFB implemented
            // However, that is not an option in C# for AES
            // ECB is the closest version we can get, still need to do the XOR ourselves
            _algo.Mode = CipherMode.ECB;
            _algo.Padding = PaddingMode.None;
            _algo.Key = userKey;

            _transformer = _algo.CreateEncryptor();

            // IV is not actually used for AES? Just for the start of the initial block
            InitFirstBlock(iv);
        }

        private void InitFirstBlock(byte[] iv)
        {
            byte[] freshIvBlock = new byte[BlockSize]
            {
                iv[0], iv[1], iv[2], iv[3],
                iv[0], iv[1], iv[2], iv[3],
                iv[0], iv[1], iv[2], iv[3],
                iv[0], iv[1], iv[2], iv[3]
            };

            // Initialize the first block with the IV
            _transformer.TransformBlock(freshIvBlock, 0, BlockSize, _xorKey, 0);
        }

        private static int GetNextXorKeySize(int leastSize)
        {
            int x = (leastSize / BlockSize) + 1;
            x *= BlockSize;
            return x;
        }

        public void Prepare(int requiredXorKeyLength)
        {
            if (_xorKey.Length > requiredXorKeyLength) return;

            int newSize = GetNextXorKeySize(requiredXorKeyLength);
            int previousSize = _xorKey.Length;
            Array.Resize(ref _xorKey, newSize);

            for (var offset = previousSize; offset < newSize; offset += BlockSize)
            {
                _transformer.TransformBlock(_xorKey, (offset - BlockSize), BlockSize, _xorKey, offset);
            }
        }

        /// <summary>
        /// XOR the input data with the xor key.
        /// Calling Prepare() is not required.
        /// </summary>
        /// <param name="input"></param>
        public void XorData(byte[] input)
        {
            var inputLength = input.Length;
            Prepare(inputLength);

            const int bigChunkSize = sizeof(UInt64);

            unsafe
            {
                fixed (byte* dataPtr = input)
                fixed (byte* xorPtr = _xorKey)
                {
                    byte* currentInputByte = dataPtr;
                    byte* currentXorByte = xorPtr;

                    var i = 0;

                    int intBlocks = inputLength / bigChunkSize;
                    for (; i < intBlocks; ++i)
                    {
                        *(UInt64*) currentInputByte ^= *(UInt64*) currentXorByte;
                        currentInputByte += bigChunkSize;
                        currentXorByte += bigChunkSize;
                    }

                    i *= bigChunkSize;

                    for (; i < inputLength; i++)
                    {
                        *(currentInputByte++) ^= *(currentXorByte++);
                    }
                }
            }
        }
    }

    class DefaultUserKeyWzEncryption : BaseAesEncryption, IWzEncryption
    {
        public static readonly byte[] DefaultAesUserKey = new byte[]
        {
            0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00,
            0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00
        };

        public DefaultUserKeyWzEncryption(byte[] iv) : base(DefaultAesUserKey, iv)
        {
        }

        public void Decrypt(byte[] bytes)
        {
            XorData(bytes);
        }

        public void Encrypt(byte[] bytes)
        {
            XorData(bytes);
        }
    }

    class GmsWzEncryption : DefaultUserKeyWzEncryption
    {
        public GmsWzEncryption() : base(new byte[] {0x4D, 0x23, 0xC7, 0x2B})
        {
        }
    }

    class SeaWzEncryption : DefaultUserKeyWzEncryption
    {
        public SeaWzEncryption() : base(new byte[] {0xB9, 0x7D, 0x63, 0xE9})
        {
        }
    }

    class NopWzEncryption : IWzEncryption
    {
        public void Decrypt(byte[] bytes)
        {
        }

        public void Encrypt(byte[] bytes)
        {
        }
    }

    class WzEncryption
    {
        private IWzEncryption currentEncryption = null;
        private bool cryptoLocked = false;
        private static IWzEncryption nopEncryption = new NopWzEncryption();

        private static IWzEncryption[] cryptos =
        {
            nopEncryption,
            new GmsWzEncryption(),
            new SeaWzEncryption(),
        };

        private byte[] GetCopy(byte[] input)
        {
            var x = new byte[input.Length];
            input.CopyTo(x, 0);
            return x;
        }

        private void PutCurrentInFront()
        {
            if (currentEncryption == cryptos[0]) return;

            var x = cryptos[0];
            cryptos[0] = currentEncryption;
            for (var i = 1; i < cryptos.Length; i++)
            {
                if (cryptos[i] == currentEncryption)
                {
                    cryptos[i] = x;
                    break;
                }
            }
        }

        public bool HasCurrentCrypto => currentEncryption != null;

        public void TryDecryptImage(byte[] contents)
        {
            // I'm not going to try and crack it
            // This is because a Property should already be successfully decoded, setting currentEncryption.
            if (currentEncryption != null)
            {
                currentEncryption.Decrypt(contents);
            }
        }

        public void ForceCrypto(IWzEncryption encryption, bool lockCrypto)
        {
            currentEncryption = encryption;
            cryptoLocked = lockCrypto;
            PutCurrentInFront();
        }

        public void ForceCurrentCrypto()
        {
            ForceCrypto(currentEncryption, true);
        }

        public IWzEncryption GetCurrentEncryption() => currentEncryption;

        private void TrySetNopAsDefault()
        {
            if (currentEncryption != null || cryptoLocked) return;
            currentEncryption = nopEncryption;
            PutCurrentInFront();
        }

        private static int getScoreForString(string s, bool ascii)
        {
            int score = 0;
            foreach (var c in s)
            {
                if (ascii)
                {
                    if ((c >= 'a' && c <= 'z') ||
                        (c >= 'A' && c <= 'Z') ||
                        (c >= '0' && c <= '9')) score += 4;
                    else if (c == ' ') score += 3;
                    else if (c == '.' || c == ',') score += 3;
                    else score += 1;
                }
                else
                {
                    if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)) score += 4;
                    else if (char.IsPunctuation(c)) score += 2;
                    else score += 1;
                }

            }

            //Trace.WriteLine($"Score for {ascii} {s}: {score}");

            return score;
        }


        struct DecryptedString
        {
            public byte[] decrypted;
            public IWzEncryption crypto;
            public int score;
        }

        public void TryDecryptString(byte[] contents, Func<byte[], bool> validate, bool ascii)
        {
            if (cryptoLocked)
            {
                currentEncryption.Decrypt(contents);
                return;
            }

            // Get score of data for each crypto
            var results = new DecryptedString[cryptos.Length];
            for (var i = 0; i < cryptos.Length; i++)
            {
                var crypto = cryptos[i];
                var copy = GetCopy(contents);
                crypto.Decrypt(copy);
                var score = getScoreForString(ascii ? Encoding.ASCII.GetString(copy) : Encoding.Unicode.GetString(copy), ascii);
                if (crypto == currentEncryption) score += 5;
                results[i] = new DecryptedString
                {
                    decrypted = copy,
                    score = score,
                    crypto = crypto,
                };

            }

            var orderedResults = results.OrderByDescending(x => x.score);

            // Take a wild guess, pick the first one
            var best = orderedResults.First();
            currentEncryption = best.crypto;
            PutCurrentInFront();
            best.decrypted.CopyTo(contents, 0);
        }

        public void ApplyCrypto(byte[] contents)
        {
            TrySetNopAsDefault();
            currentEncryption.Encrypt(contents);
        }
    }
}