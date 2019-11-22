using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Force.Crc32;
using MapleManager.WzTools;
using MapleManager.WzTools.FileSystem;
using MapleManager.WzTools.Objects;
using MapleManager.WzTools.Package;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace MS2JSON
{
    class Program
    {
        private static bool exportBson = false;
        private static DirectoryInfo globalOutputDirectory = null;
        private static Options exportOptions = Options.DeduplicateImages | Options.VisitUOLs | Options.ExternalImageExport;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Env arguments to use:");
                Console.WriteLine(" WZ_KEY - key for wz file, like '315' for maple version 315");
                Console.WriteLine(" WZ_MAP_NAME - name of the json file (without extension) that contains the tree of the wz contents.");
                Console.WriteLine(" MSEXE - path to MapleStory.exe. Will use MapleStory.exe from WZ directory if not set. Used for detecting version (and set WZ_MAP_NAME)");
                Console.WriteLine(" EXTRACT_IMGS=1 - also extract raw img files from wz");
                Console.WriteLine(" PRETTYPRINT_JSON=1 - pretty print json files (makes them huge)");
                Console.WriteLine(" EXPORT_BSON=1 - Use BSON instead of JSON for img files (images will be binary instead of base64)");
                Console.WriteLine(" OUTPUT_DIR - Write files to specified output dir, instead of the directory of the wz file");
                Console.WriteLine("Files to pass:");
                Console.WriteLine(" *.img files");
                Console.WriteLine(" *.wz files");
                Console.WriteLine();
            }
            else
            {

                exportBson = Environment.GetEnvironmentVariable("EXPORT_BSON") == "1";

                if (exportBson)
                {
                    Console.WriteLine("[OPT] Exporting as BSON");
                }

                var hadWzMapNameSet = Environment.GetEnvironmentVariable("WZ_MAP_NAME") != null;
                var useFileLocationAsOutputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") == null;
                if (!useFileLocationAsOutputDir)
                    globalOutputDirectory = new DirectoryInfo(Environment.GetEnvironmentVariable("OUTPUT_DIR"));

                if (globalOutputDirectory != null && !globalOutputDirectory.Exists) globalOutputDirectory.Create();

                foreach (var s in args)
                {
                    Console.WriteLine("Handling {0}", s);

                    var fileInfo = new FileInfo(s);


                    if (useFileLocationAsOutputDir)
                    {
                        globalOutputDirectory = fileInfo.Directory;
                    }


                    if (s.EndsWith(".img"))
                    {
                        var fsf = new FSFile
                        {
                            RealPath = fileInfo.FullName,
                            Name = fileInfo.Name,
                        };

                        ExtractFile(fsf, fileInfo.Directory);
                    }
                    else if (s.EndsWith(".wz"))
                    {
                        var msExe = Environment.GetEnvironmentVariable("MSEXE") ?? Path.Combine(fileInfo.DirectoryName, "MapleStory.exe");

                        if (File.Exists(msExe))
                        {
                            var fvi = FileVersionInfo.GetVersionInfo(msExe);
                            // Version is stored as locale.version.subversion.test
                            var version = fvi.FileMinorPart;
                            var subversion = fvi.FileBuildPart;
                            var locale = fvi.FileMajorPart;
                            Console.WriteLine(
                                "File for version {0}.{1} locale {2}",
                                version, subversion, locale
                            );

                            if (!hadWzMapNameSet)
                            {
                                var mapName = $"v{version}-{subversion}-{locale}";
                                Environment.SetEnvironmentVariable("WZ_MAP_NAME", mapName);
                            }
                        }
                        else if (!hadWzMapNameSet)
                        {
                            // Clear map name for files that did not match
                            Environment.SetEnvironmentVariable("WZ_MAP_NAME", null);
                        }


                        ExtractWZ(fileInfo, Environment.GetEnvironmentVariable("WZ_KEY") ?? "");
                    }
                    else
                    {
                        Console.WriteLine("Unable to handle file");
                    }
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void ExtractWZ(FileInfo path, string key)
        {
            var package = new WzPackage(path.FullName, key);
            package.Process();

            var outputDir = globalOutputDirectory.CreateSubdirectory(package.Name);

            if (Environment.GetEnvironmentVariable("EXTRACT_IMGS") == "1")
            {
                Console.WriteLine("Exporting IMGs to {0}", outputDir.FullName);
                package.Extract(outputDir);
            }

            var allTasks = ExtractWZDir(package, outputDir).ToArray();
            ProcessTasks(allTasks);


            var wzMapName = Environment.GetEnvironmentVariable("WZ_MAP_NAME");
            if (wzMapName != null)
            {
                var wzContentsMapPath = Path.Combine(outputDir.FullName, wzMapName + ".json");
                Console.WriteLine("Writing WZ file map to {0}...", wzContentsMapPath);

                File.WriteAllText(wzContentsMapPath, BuildWZContentsMap(package));
            }

        }

        private static void ProcessTasks(Task[] tasks)
        {
            var taskCount = tasks.Length;
            Console.WriteLine("Starting to wait on {0} tasks...", taskCount);

            while (true)
            {
                int completed = tasks.Count(x => x.IsCompleted);

                var text = $"{completed} of {taskCount} done {completed * 100.0 / taskCount:F1}%";
                Console.WriteLine(text);
                Console.Title = text;

                if (taskCount == completed) break;
                Thread.Sleep(1000);
            }
        }

        private static string BuildWZContentsMap(NameSpaceDirectory root)
        {
            void processDir(JsonWriter w, NameSpaceDirectory dir)
            {
                w.WriteStartObject();
                foreach (var nsf in dir.Files)
                {
                    w.WritePropertyName(nsf.Name);
                    w.WriteValue(nsf.Checksum);
                }

                foreach (var nsd in dir.SubDirectories)
                {
                    w.WritePropertyName(nsd.Name);
                    processDir(w, nsd);
                }

                w.WriteEndObject();
            }

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var writer = new JsonTextWriter(sw))
            {
                processDir(writer, root);
            }

            return sb.ToString();
        }



        private static IEnumerable<Task> ExtractWZDir(NameSpaceDirectory dir, DirectoryInfo currentOutputDirectory)
        {
            foreach (var nsf in dir.Files)
            {
                yield return Task.Run(() =>
                {
                    try
                    {
                        ExtractFile(nsf, currentOutputDirectory);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR {0}", ex);
                    }
                });
            }


            foreach (var nsd in dir.SubDirectories)
            {
                foreach (var task in ExtractWZDir(nsd, currentOutputDirectory.CreateSubdirectory(nsd.Name)))
                {
                    yield return task;
                }
            }
        }

        private static void ExtractFile(NameSpaceFile nsf, DirectoryInfo currentOutputDirectory)
        {
            var path = Path.Combine(currentOutputDirectory.FullName, nsf.Name);
            bool useJson = !exportBson;


            var obj = nsf.Object as WzProperty;
            if (obj == null)
            {
                Console.WriteLine("Unable to export {0}, as its not a WzProperty", nsf.NodePath);
                return;
            }

            byte[] data;
            
            using (var exp = new Exporter(exportOptions, obj))
            {
                if (useJson) data = Encoding.ASCII.GetBytes(exp.ToJson());
                else data = exp.ToBson();


                if (exportOptions.HasFlag(Options.ExternalImageExport))
                {
                    // Generate image path of the first 4 characters of the name (if possible)
                    var imgDir = globalOutputDirectory.CreateSubdirectory("images");
                    var name = obj.Name.Replace(".img", "");
                    if (name.Length > 0) imgDir = imgDir.CreateSubdirectory("" + name[0]);
                    if (name.Length > 1) imgDir = imgDir.CreateSubdirectory("" + name[1]);
                    if (name.Length > 2) imgDir = imgDir.CreateSubdirectory("" + name[2]);
                    if (name.Length > 3) imgDir = imgDir.CreateSubdirectory("" + name[3]);

                    ExtractImages(exp, imgDir);
                }
            }

            nsf.Checksum = (int)Crc32Algorithm.Compute(data);

            // This will replace .img with .hash.(json|bson)
            var outputFile = Path.ChangeExtension(path, $".{nsf.Checksum:X8}." + (useJson ? "json" : "bson"));

            if (File.Exists(outputFile)) return;

            Console.WriteLine("Writing {0}", outputFile);

            File.WriteAllBytes(
                outputFile,
                data
            );
        }

        private static void ExtractImages(Exporter exp, DirectoryInfo outputDir)
        {
            foreach (var kvp in exp.GetImages())
            {
                var filename = Path.Combine(outputDir.FullName, $"{kvp.Key}.png");
                if (File.Exists(filename)) continue;


                Console.WriteLine("Writing {0}", filename);
                var tempFile = Path.GetTempFileName();
                File.WriteAllBytes(tempFile, kvp.Value);
                try
                {
                    File.Move(tempFile, filename);
                    // This might fail so just ignore the error. LOL
                }
                catch { }
            }
        }
    }


    [Flags]
    public enum Options
    {
        VisitUOLs,
        DeduplicateImages,
        ExternalImageExport,
    }


    class Exporter : IDisposable
    {
        private readonly Options _options;
        private Dictionary<int, uint> _imageHashCode = new Dictionary<int, uint>();
        private Dictionary<uint, byte[]> _images = new Dictionary<uint, byte[]>();
        private WzProperty _obj;

        public IEnumerable<KeyValuePair<uint, byte[]>> GetImages() => _images;

        public Exporter(Options opts, WzProperty baseObject)
        {
            _obj = baseObject;
            _options = opts;
        }

        private byte[] GetImageData(Image img, out uint checksum)
        {
            var hashCode = img.GetHashCode();
            if (_imageHashCode.ContainsKey(hashCode))
            {
                checksum = _imageHashCode[hashCode];
                return _images[checksum];
            }

            using (var ms = new MemoryStream())
            {
                lock (img) img.Save(ms, ImageFormat.Png);

                var data = ms.ToArray();

                checksum = Crc32Algorithm.Compute(data);
                _imageHashCode[hashCode] = checksum;
                _images[checksum] = data;
                return data;
            }
        }

        private void WriteImage(JsonWriter w, WzCanvas canvas)
        {
            var data = GetImageData(canvas.Tile, out var uid);
            if (!_options.HasFlag(Options.DeduplicateImages))
            {
                WriteImageData(w, data);
                return;
            }

            w.WriteValue(uid);
            _images[uid] = data;
        }

        private static void WriteImageData(JsonWriter w, byte[] data)
        {
            if (w is BsonDataWriter)
                w.WriteValue(data);
            else
                w.WriteValue(Convert.ToBase64String(data));
        }


        public void Write(JsonWriter w)
        {
            w.WriteStartObject();

            FillWzPropertyElements(w, _obj);

            if (_images.Count > 0 && !_options.HasFlag(Options.ExternalImageExport))
            {
                w.WritePropertyName("_images");
                w.WriteStartObject();
                foreach (var t in _images)
                {
                    w.WritePropertyName(t.Key.ToString());
                    WriteImageData(w, t.Value);
                }
                w.WriteEndObject();
            }

            w.WriteEndObject();
        }

        private void FillWzPropertyElements(JsonWriter w, WzProperty wp)
        {
            foreach (var kvp in wp)
            {
                // Skip certain props that the user dont need
                if (kvp.Key == "_hash" || kvp.Key == "_outlink" || kvp.Key == "_inlink")
                    continue;


                w.WritePropertyName(kvp.Key);
                ObjectToJson(w, kvp.Value);
            }
        }

        private void ObjectToJson(JsonWriter w, object o)
        {
            switch (o)
            {
                case WzCanvas wc:
                    w.WriteStartObject();

                    {
                        w.WritePropertyName("image");
                        WriteImage(w, wc);
                    }

                    FillWzPropertyElements(w, wc);

                    w.WriteEndObject();
                    break;

                case WzList wl:
                    w.WriteStartArray();

                    foreach (var po in wl) ObjectToJson(w, po);

                    w.WriteEndArray();
                    break;

                case WzVector2D wv:
                    w.WriteStartObject();
                    w.WritePropertyName("x");
                    w.WriteValue(wv.X);
                    w.WritePropertyName("y");
                    w.WriteValue(wv.Y);
                    w.WriteEndObject();
                    break;

                case WzUOL wuol:
                    if (_options.HasFlag(Options.VisitUOLs))
                    {
                        ObjectToJson(w, wuol.ActualObject(true));
                    }
                    else
                    {
                        w.WriteValue(wuol.Path);
                    }

                    break;

                case WzProperty wp:
                    w.WriteStartObject();
                    FillWzPropertyElements(w, wp);
                    w.WriteEndObject();
                    break;

                case WzSound ws:
                    Console.WriteLine("Ignoring Sound prop @ {0}", ws.GetFullPath());
                    break;

                default:
                    w.WriteValue(o);
                    break;
            }
        }


        public string ToJson()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var writer = new JsonTextWriter(sw))
            {
                if (Environment.GetEnvironmentVariable("PRETTYPRINT_JSON") == "1")
                {
                    writer.Formatting = Formatting.Indented;
                }

                Write(writer);
            }
            return sb.ToString();
        }


        public byte[] ToBson()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BsonDataWriter(ms))
            {
                Write(writer);
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            _images?.Clear();
            _images = null;

            _imageHashCode?.Clear();
            _imageHashCode = null;
            
            _obj = null;
        }
    }
}
