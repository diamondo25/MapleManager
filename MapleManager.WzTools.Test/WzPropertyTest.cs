using MapleManager.WzTools.Helpers;
using MapleManager.WzTools.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.CodeDom;
using System.IO;
using System.Runtime.CompilerServices;
using MapleManager.WzTools.Package;

namespace MapleManager.WzTools.Test
{
    [TestClass]
    public class WzPropertyTest
    {
        [TestMethod]
        public void TestPrimitiveSerializingAndDeserializing()
        {
            var prop = new WzProperty();
            prop.Set("byte", (byte) 0xaf);
            prop.Set("sbyte", (sbyte) 0x7a);
            prop.Set("ushort", (ushort) 0xaaff);
            prop.Set("uint", (uint) 0xaaffaaff);
            prop.Set("ulong", (ulong) 0xaaffaaffaaffaaff);

            prop.Set("short", (short) 0x7aff);
            prop.Set("int", (int) 0x7affaaff);
            prop.Set("long", (long) 0x7affaaffaaffaaff);
            prop.Set("l", (long) 0x7affaaffaaffaaff);

            prop.Set("single", (Single) 1234.6789f);
            prop.Set("double", (Double) 1234.6789d);
            prop.Set("ascii", "test1234");
            prop.Set("unicode", "hurr emoji 😋");

            prop.Set("null", null);
            prop.Set("dt", DateTime.Now);

            var testEncryptions = EncryptionTest.TestEncryptions();
            foreach (var testEncryption in testEncryptions)
            {
                using (var ms = new MemoryStream())
                using (var aw = new ArchiveWriter(ms))
                {
                    aw.Encryption = testEncryption;
                    prop.Write(aw);


                    ms.Position = 0;


                    var outProp = new WzProperty();
                    var ar = new ArchiveReader(ms);
                    ar.SetEncryption(testEncryption);
                    outProp.Read(ar);

                    foreach (var kvp in outProp)
                    {
                        Console.WriteLine("Got key {0} of {1}", kvp.Key, testEncryption);
                    }

                    foreach (var kvp in prop)
                    {
                        Console.WriteLine("Checking key {0} of {1}", kvp.Key, testEncryption);

                        var hasKey = outProp.HasChild(kvp.Key);

                        Assert.IsTrue(hasKey, $"Missing key {kvp.Key}");
                        if (hasKey)
                        {
                            Assert.AreEqual(kvp.Value, outProp[kvp.Key], $"Unequal values! {kvp.Key} {kvp.Value}");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSubPropSerializingAndDeserializing()
        {
            var parentProp = new WzProperty();
            var subProp = new WzProperty();
            parentProp.Set("sub", subProp);


            var ms = new MemoryStream();
            var aw = new ArchiveWriter(ms);
            parentProp.Write(aw);


            ms.Position = 0;

            var outProp = new WzProperty();
            outProp.Read(new ArchiveReader(ms));

            Assert.IsTrue(outProp.HasChild("sub"));
            Assert.IsInstanceOfType(outProp["sub"], typeof(WzProperty));
        }

        [TestMethod]
        public void TestTextEncryption()
        {
            var testEncryptions = EncryptionTest.TestEncryptions();


            foreach (var testEncryption in testEncryptions)
            {
                var ms = new MemoryStream();
                var aw = new ArchiveWriter(ms);
                aw.Encryption = testEncryption;
                aw.Write("long", 0x1B, 0x73);

                Console.Write("> ");
                var tmp = ms.ToArray();
                foreach (var b in tmp)
                {
                    Console.Write(b.ToString("X2"));
                }

                Console.WriteLine();

                ms.Position = 0;

                var ar = new ArchiveReader(ms);
                ar.SetEncryption(aw.Encryption);

                Assert.AreEqual("long", ar.ReadString(0x1B, 0x73));
            }
        }

        [TestMethod]
        public void TestTextEncryption3()
        {
            var testEncryptions = EncryptionTest.TestEncryptions();
            Exception err;
            foreach (var testEncryption in testEncryptions)
            {
                if (testEncryption is NopWzEncryption) continue;
                Console.WriteLine("Trying {0}", testEncryption);
                byte[] testVector;
                for (int i = 1; i < 10000; i++)
                {
                    testVector = new byte[i];
                    new Random().NextBytes(testVector);
                    var copy = (byte[]) testVector.Clone();

                    err = arrayCompare(testVector, copy);
                    if (err != null) throw new Exception($"Unable to compare or something, i {i}", err);

                    testEncryption.Encrypt(testVector);
                    err = arrayCompare(testVector, copy);
                    if (err == null) throw new Exception($"Vectors should not have matched, i {i}");

                    testEncryption.Decrypt(testVector);
                    err = arrayCompare(testVector, copy);
                    if (err != null) throw new Exception($"Vectors should have matched, i {i}");

                }
            }
        }

        [TestMethod]
        public void TestTextEncryption2()
        {
            var testEncryptions = EncryptionTest.TestEncryptions();


            foreach (var testEncryption in testEncryptions)
            {
                Console.WriteLine("Using crypto: {0}", testEncryption);
                var str = "long";
                var ms = new MemoryStream();
                var aw = new ArchiveWriter(ms);
                aw.Encryption = testEncryption;
                aw.Write(str, 0x1B, 0x73);

                var tmp = ms.ToArray();
                debugArray(tmp);

                tmp = aw.EncodeString(str, out _);
                debugArray(tmp);

                ms.Position = 0;

                var ar = new ArchiveReader(ms);
                ar.SetEncryption(testEncryption);

                Assert.AreEqual(str, ar.ReadString(0x1B, 0x73));
            }
        }


        void debugArray(byte[] tmp)
        {
            Console.Write("> ");
            foreach (var b in tmp) Console.Write(b.ToString("X2"));

            Console.WriteLine();
        }

        Exception arrayCompare(byte[] a, byte[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return new Exception($"{a[i]} != {b[i]}");
            }

            return null;
        }

        [TestMethod]
        public void TestXor()
        {
            Exception err;
            byte[] correct = new byte[] {(byte) 'l', (byte) 'o', (byte) 'n', (byte) 'g'};
            byte[] b = (byte[]) correct.Clone();
            b.ApplyStringXor(false);

            debugArray(b);

            b.ApplyStringXor(false);
            debugArray(b);

            err = arrayCompare(b, correct);
            if (err != null) throw err;


            b = (byte[]) correct.Clone();
            b.ApplyStringXor(true);
            debugArray(b);
            b.ApplyStringXor(true);
            debugArray(b);

            err = arrayCompare(b, correct);
            if (err != null) throw err;
        }

    }
}