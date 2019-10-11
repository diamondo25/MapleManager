using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleManager.WzTools.Package;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapleManager.WzTools.Test
{
    [TestClass]
    public class EncryptionTest
    {
        [TestMethod]
        public void TestEncryption()
        {
            // This 'long' string was actually having issues
            var testBytes = Encoding.ASCII.GetBytes("long");
            foreach (var testEncryption in TestEncryptions())
            {
                var tmp = (byte[])testBytes.Clone();
                testEncryption.Encrypt(tmp);

                Console.Write("> ");
                foreach (var b in tmp)
                {
                    Console.Write("{0:X2}",b);
                }
                Console.WriteLine();

                testEncryption.Decrypt(tmp);

                for (var i = 0; i < testBytes.Length; i++)
                {
                    var x = testBytes[i];
                    var y = tmp[i];
                    Assert.AreEqual(x, y, $"Error on {i} {testEncryption}");
                }
                Console.WriteLine();
            }
        }

        public static IWzEncryption[] TestEncryptions()
        {
            return new IWzEncryption[]
            {
                new SeaWzEncryption(),
                new GmsWzEncryption(),
                new NopWzEncryption(),
            };
        }
    }
}