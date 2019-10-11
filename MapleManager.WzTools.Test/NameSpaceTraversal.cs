using System;
using MapleManager.WzTools.FileSystem;
using MapleManager.WzTools.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapleManager.WzTools.Test
{
    [TestClass]
    public class NameSpaceTraversal
    {
        [TestMethod]
        public void TestBasicTraversal()
        {
            var root = new FSDirectory()
            {
                Name = "",
            };

            var dir1 = new FSDirectory() { Name = "dir1" };
            var dir2 = new FSDirectory() { Name = "dir2" };
            root.AddDirectories(dir1, dir2);

            Assert.AreEqual(root, dir1.Parent);

            Assert.IsTrue(root.HasChild("dir1"));
            Assert.IsTrue(root.HasChild("dir2"));

            var file1 = new FSFile() { Name = "crappyfile.img" };
            var file1Object = new WzFileProperty
            {
                FileNode = file1,
                Name = file1.Name,
            };
            file1.Object = file1Object;

            dir1.AddFiles(file1);
            
            file1Object.Set("prop1", "whatever");
            file1Object.Set("prop2", 1337);


            file1Object.Set("uol_1", new WzUOL()
            {
                Path = "./prop1",
            });

            Assert.AreEqual(dir1, file1.GetParent());

            Assert.AreEqual(file1Object, dir1.GetChild("crappyfile"));
            Assert.AreEqual(file1Object, dir1.GetChild("crappyfile.img"));

            {
                var x = file1Object.Get("uol_1");
                Assert.IsNotNull(x);
                var xnsn = (INameSpaceNode) x;
                Assert.IsNotNull(xnsn);

                var xnsn1 = (INameSpaceNode)xnsn.GetParent();
                Assert.AreEqual(file1Object, xnsn1);
                var xnsn2 = (INameSpaceNode)xnsn1.GetParent();
                Assert.AreEqual(dir1, xnsn2);
                var xnsn3 = (INameSpaceNode)xnsn2.GetParent();
                Assert.AreEqual(root, xnsn3);
            }


            file1Object.Set("uol", new WzUOL { Path = "uol_1" });
            Assert.AreEqual(file1Object.Get("uol_1"), ((WzUOL)file1Object.Get("uol")).ActualObject(false));

            file1Object.Set("uol", new WzUOL { Path = "uol_1" });
            Assert.AreEqual(file1Object.Get("prop1"), ((WzUOL)file1Object.Get("uol")).ActualObject(true));

            file1Object.Set("uol", new WzUOL { Path = "./prop2" });
            Assert.AreEqual(1337, ((WzUOL)file1Object.Get("uol")).ActualObject());

            file1Object.Set("uol", new WzUOL { Path = "prop2" });
            Assert.AreEqual(1337, ((WzUOL)file1Object.Get("uol")).ActualObject());

            file1Object.Set("uol", new WzUOL { Path = "../crappyfile/prop2" });
            Assert.AreEqual(1337, ((WzUOL)file1Object.Get("uol")).ActualObject());

            file1Object.Set("uol", new WzUOL { Path = "../crappyfile/prop2" });
            Assert.AreEqual(1337, ((WzUOL)file1Object.Get("uol")).ActualObject());

            file1Object.Set("uol", new WzUOL { Path = "../crappyfile.img/prop2" });
            Assert.AreEqual(1337, ((WzUOL)file1Object.Get("uol")).ActualObject());

            file1Object.Set("uol", new WzUOL { Path = "../../dir1/crappyfile/prop2" });
            Assert.AreEqual(1337, ((WzUOL)file1Object.Get("uol")).ActualObject());

            file1Object.Set("uol", new WzUOL { Path = "../../dir1/crappyfile/uol_1" });
            Assert.AreEqual(file1Object.Get("uol_1"), ((WzUOL)file1Object.Get("uol")).ActualObject(false));

            file1Object.Set("uol", new WzUOL { Path = "../../dir1/crappyfile/uol_1" });
            Assert.AreEqual("whatever", ((WzUOL)file1Object.Get("uol")).ActualObject(true));
        }
    }
}
