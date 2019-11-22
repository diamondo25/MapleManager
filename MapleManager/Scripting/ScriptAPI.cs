using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapleManager.Controls;
using MapleManager.WzTools;
using MapleManager.WzTools.Objects;
using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace MapleManager
{
    public abstract class ScriptAPI : IEnumerable<ScriptNode>, INameSpaceNode
    {
        public abstract ScriptNode GetNode(string path);

        public object Get(string path) => GetNode(path)?.Get();
        public Int8 GetInt8(string path, Int8 fallback = default(Int8)) => GetNode(path)?.ToInt8() ?? fallback;
        public Int16 GetInt16(string path, Int16 fallback = default(Int16)) => GetNode(path)?.ToInt16() ?? fallback;
        public Int32 GetInt32(string path, Int32 fallback = default(Int32)) => GetNode(path)?.ToInt32() ?? fallback;
        public Int64 GetInt64(string path, Int64 fallback = default(Int64)) => GetNode(path)?.ToInt64() ?? fallback;

        public UInt8 GetUInt8(string path, UInt8 fallback = default(UInt8)) => GetNode(path)?.ToUInt8() ?? fallback;
        public UInt16 GetUInt16(string path, UInt16 fallback = default(UInt16)) => GetNode(path)?.ToUInt16() ?? fallback;
        public UInt32 GetUInt32(string path, UInt32 fallback = default(UInt32)) => GetNode(path)?.ToUInt32() ?? fallback;
        public UInt64 GetUInt64(string path, UInt64 fallback = default(UInt64)) => GetNode(path)?.ToUInt64() ?? fallback;

        public Single GetSingle(string path, Single fallback = default(Single)) => GetNode(path)?.ToSingle() ?? fallback;
        public Double GetDouble(string path, Double fallback = default(Double)) => GetNode(path)?.ToDouble() ?? fallback;

        public string GetString(string path, string fallback = default(string)) => GetNode(path)?.ToString() ?? fallback;
        public Image GetImage(string path) => GetNode(path)?.GetImage();
        public abstract WZTreeNode TryGetTreeNode();


        public bool SetInt8(string path, Int8 value) => GetNode(path).SetInt8(value);
        public bool SetInt16(string path, Int16 value) => GetNode(path).SetInt16(value);
        public bool SetInt32(string path, Int32 value) => GetNode(path).SetInt32(value);
        public bool SetInt64(string path, Int64 value) => GetNode(path).SetInt64(value);

        public bool SetUInt8(string path, UInt8 value) => GetNode(path).SetUInt8(value);
        public bool SetUInt16(string path, UInt16 value) => GetNode(path).SetUInt16(value);
        public bool SetUInt32(string path, UInt32 value) => GetNode(path).SetUInt32(value);
        public bool SetUInt64(string path, UInt64 value) => GetNode(path).SetUInt64(value);

        public bool SetSingle(string path, Single value) => GetNode(path).SetSingle(value);
        public bool SetDouble(string path, Double value) => GetNode(path).SetDouble(value);

        public bool SetString(string path, string value) => GetNode(path).SetString(value);

        public abstract string GetFullPath();

        public abstract IEnumerator<ScriptNode> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void UpdateTreeNodes(string path = "")
        {
            var node = GetNode(path);
            var tn = node.TryGetTreeNode();
            // Try to get the parent node
            if (tn == null) tn = node.GetNode("..")?.TryGetTreeNode();
            if (tn == null) return;
            tn.UpdateData();
        }

        public abstract string GetName();

        public object GetParent() => GetNode("..");

        public object GetChild(string key) => GetNode(key);

        public bool HasChild(string key) => GetChild(key) != null;
    }

    public class ScriptNode : ScriptAPI
    {
        // Use Object instead
        private object _obj;
        private WZTreeNode _treeNode;

        private object Object
        {
            get
            {
                if (_obj is NameSpaceFile file)
                {
                    _obj = file.Object;
                }
                else if (_obj is WzUOL uol)
                {
                    _obj = uol.ActualObject(true);
                }
                return _obj;
            }
        }

        public string Name { get; private set; }

        public override string GetName() => Name;

        private ScriptNode _parent;

        public ScriptNode(string name, object obj, ScriptNode parent, WZTreeNode treeNode)
        {
            Name = name;
            _obj = obj;
            _parent = parent;
            _treeNode = treeNode;
        }

        private ScriptNode LoadTreeNodeInfo(WZTreeNode node, string key)
        {
            var tnValue = node.Tag;
            object obj = null;
            switch (tnValue)
            {
                case null:
                    obj = node;
                    break;
                case NameSpaceFile file:
                    obj = file;
                    break;
                case NameSpaceDirectory dir:
                    obj = node;
                    break;
                default: obj = tnValue; break;
            }

            return new ScriptNode(key, obj, this, node);
        }

        public override WZTreeNode TryGetTreeNode()
        {
            WZTreeNode ret = _treeNode;

            // check if parent is a treenode
            if (ret == null && _parent != null)
            {
                var parentTreeNode = _parent.TryGetTreeNode();
                parentTreeNode.TryLoad(false);
                ret = parentTreeNode.Nodes[this.Name] as WZTreeNode;
            }

            return ret;
        }

        public override string GetFullPath()
        {
            return PcomObject?.GetFullPath() ?? TryGetTreeNode()?.GetFullPath();
        }

        public ScriptNode this[string key]
        {
            get
            {
                switch (Object)
                {
                    case PcomObject po:
                        var x = po.Get(key);
                        if (x == null) return null;
                        return new ScriptNode(key, x, this, null);
                    case TreeNode tn:
                        if (tn.Nodes.ContainsKey(key))
                        {
                            return LoadTreeNodeInfo(tn.Nodes[key] as WZTreeNode, key);
                        }
                        return null;
                    case TreeView tv:
                        if (tv.Nodes.ContainsKey(key))
                        {
                            return LoadTreeNodeInfo(tv.Nodes[key] as WZTreeNode, key);
                        }
                        return null;
                }

                return null;
            }
            set { GetNode(key).Set(value); }
        }

        public override ScriptNode GetNode(string path)
        {
            if (path == "." || path == "") return this;
            if (path == "..") return _parent;

            var nodes = path.Split('/');

            ScriptNode ret = this;
            foreach (var node in nodes)
            {
                if (node == "..") ret = ret._parent;
                else if (node == ".") continue;
                else if (node == "") continue;
                else if (ret[node] != null) ret = ret[node];
                else if (ret[node + ".img"] != null) ret = ret[node + ".img"];
                else return null;



                if (ret == null)
                    return null;
                if (ret.Object is WzUOL uol)
                    ret = uol.ActualObject(true) as ScriptNode;
                
            }

            return ret;
        }

        public WZTreeNode GetTreeNode(string path)
        {
            var nodes = path.Split('/');

            ScriptNode ret = this;
            foreach (var node in nodes)
            {
                if (node == "..") ret = ret._parent;
                else if (node == ".") continue;
                else ret = ret[node];


                if (ret == null)
                    return null;

                var tn = ret.TryGetTreeNode();
                if (tn == null)
                {
                    throw new Exception("Unable to get treenode???");
                }
            }

            return ret.TryGetTreeNode();
        }

        #region Getters


        public object Get() => Object;

        public T Get<T>() where T : PcomObject => Get() as T;

        public Int8 ToInt8()
        {
            switch (Object)
            {
                case Int8 __sb: return (Int8)__sb;
                case Int16 _ss: return (Int8)_ss;
                case Int32 _si: return (Int8)_si;
                case Int64 _sl: return (Int8)_sl;
                case UInt8 _ub: return (Int8)_ub;
                case UInt16 us: return (Int8)us;
                case UInt32 ui: return (Int8)ui;
                case UInt64 ul: return (Int8)ul;
                case Single s: return (Int8)s;
                case Double d: return (Int8)d;
                case String __s: return Int8.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an Int8");
        }

        public Int16 ToInt16()
        {
            switch (Object)
            {
                case Int8 __sb: return (Int16)__sb;
                case Int16 _ss: return (Int16)_ss;
                case Int32 _si: return (Int16)_si;
                case Int64 _sl: return (Int16)_sl;
                case UInt8 _ub: return (Int16)_ub;
                case UInt16 us: return (Int16)us;
                case UInt32 ui: return (Int16)ui;
                case UInt64 ul: return (Int16)ul;
                case Single s: return (Int16)s;
                case Double d: return (Int16)d;
                case String __s: return Int16.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an Int16");
        }

        public Int32 ToInt32()
        {
            switch (Object)
            {
                case Int8 __sb: return (Int32)__sb;
                case Int16 _ss: return (Int32)_ss;
                case Int32 _si: return (Int32)_si;
                case Int64 _sl: return (Int32)_sl;
                case UInt8 _ub: return (Int32)_ub;
                case UInt16 us: return (Int32)us;
                case UInt32 ui: return (Int32)ui;
                case UInt64 ul: return (Int32)ul;
                case Single s: return (Int32)s;
                case Double d: return (Int32)d;
                case String __s: return Int32.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an Int32");
        }

        public Int64 ToInt64()
        {
            switch (Object)
            {
                case Int8 __sb: return (Int64)__sb;
                case Int16 _ss: return (Int64)_ss;
                case Int32 _si: return (Int64)_si;
                case Int64 _sl: return (Int64)_sl;
                case UInt8 _ub: return (Int64)_ub;
                case UInt16 us: return (Int64)us;
                case UInt32 ui: return (Int64)ui;
                case UInt64 ul: return (Int64)ul;
                case Single s: return (Int64)s;
                case Double d: return (Int64)d;
                case String __s: return Int64.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an Int64");
        }

        public UInt8 ToUInt8()
        {
            switch (Object)
            {
                case Int8 __sb: return (UInt8)__sb;
                case Int16 _ss: return (UInt8)_ss;
                case Int32 _si: return (UInt8)_si;
                case Int64 _sl: return (UInt8)_sl;
                case UInt8 _ub: return (UInt8)_ub;
                case UInt16 us: return (UInt8)us;
                case UInt32 ui: return (UInt8)ui;
                case UInt64 ul: return (UInt8)ul;
                case Single s: return (UInt8)s;
                case Double d: return (UInt8)d;
                case String __s: return UInt8.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an UInt8");
        }

        public UInt16 ToUInt16()
        {
            switch (Object)
            {
                case Int8 __sb: return (UInt16)__sb;
                case Int16 _ss: return (UInt16)_ss;
                case Int32 _si: return (UInt16)_si;
                case Int64 _sl: return (UInt16)_sl;
                case UInt8 _ub: return (UInt16)_ub;
                case UInt16 us: return (UInt16)us;
                case UInt32 ui: return (UInt16)ui;
                case UInt64 ul: return (UInt16)ul;
                case Single s: return (UInt16)s;
                case Double d: return (UInt16)d;
                case String __s: return UInt16.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an UInt16");
        }

        public UInt32 ToUInt32()
        {
            switch (Object)
            {
                case Int8 __sb: return (UInt32)__sb;
                case Int16 _ss: return (UInt32)_ss;
                case Int32 _si: return (UInt32)_si;
                case Int64 _sl: return (UInt32)_sl;
                case UInt8 _ub: return (UInt32)_ub;
                case UInt16 us: return (UInt32)us;
                case UInt32 ui: return (UInt32)ui;
                case UInt64 ul: return (UInt32)ul;
                case Single s: return (UInt32)s;
                case Double d: return (UInt32)d;
                case String __s: return UInt32.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an UInt32");
        }

        public UInt64 ToUInt64()
        {
            switch (Object)
            {
                case Int8 __sb: return (UInt64)__sb;
                case Int16 _ss: return (UInt64)_ss;
                case Int32 _si: return (UInt64)_si;
                case Int64 _sl: return (UInt64)_sl;
                case UInt8 _ub: return (UInt64)_ub;
                case UInt16 us: return (UInt64)us;
                case UInt32 ui: return (UInt64)ui;
                case UInt64 ul: return (UInt64)ul;
                case Single s: return (UInt64)s;
                case Double d: return (UInt64)d;
                case String __s: return UInt64.Parse(__s);
            }

            throw new Exception($"Not sure how to convert '{Object}' into an UInt64");
        }

        public Single ToSingle()
        {
            switch (Object)
            {
                case Double x: return (Single)x;
                case Single x: return (Single)x;
                case Int8 __sb: return (Single)__sb;
                case Int16 _ss: return (Single)_ss;
                case Int32 _si: return (Single)_si;
                case Int64 _sl: return (Single)_sl;
                case UInt8 _ub: return (Single)_ub;
                case UInt16 us: return (Single)us;
                case UInt32 ui: return (Single)ui;
                case UInt64 ul: return (Single)ul;
            }

            throw new Exception($"Not sure how to convert '{Object}' into a Single");
        }

        public Double ToDouble()
        {
            switch (Object)
            {
                case Double x: return x;
                case Single x: return x;
                case Int8 __sb: return (Double)__sb;
                case Int16 _ss: return (Double)_ss;
                case Int32 _si: return (Double)_si;
                case Int64 _sl: return (Double)_sl;
                case UInt8 _ub: return (Double)_ub;
                case UInt16 us: return (Double)us;
                case UInt32 ui: return (Double)ui;
                case UInt64 ul: return (Double)ul;
            }

            throw new Exception($"Not sure how to convert '{Object}' into a Double");
        }

        public string GetString()
        {
            if (Object == null) return null;
            if (Object is string str) return str;
            if (!(Object is PcomObject))
                return ToUInt64().ToString();
            return null;
        }

        public WzCanvas GetCanvas() => Get<WzCanvas>();

        public Image GetImage() => GetCanvas()?.Tile;


        public override string ToString()
        {
            return Object.ToString();
        }

        #endregion

        #region Setters

        private PcomObject PcomObject => TryGetTreeNode()?.WzObject as PcomObject;

        public bool Set(object v)
        {
            var pcomObject = _parent?.PcomObject;
            if (pcomObject != null)
            {
                pcomObject.Set(Name, v);
                return true;
            }

            return false;
        }

        public bool SetInt8(Int8 v) => Set(v);
        public bool SetInt16(Int16 v) => Set(v);
        public bool SetInt32(Int32 v) => Set(v);
        public bool SetInt64(Int64 v) => Set(v);

        public bool SetUInt8(UInt8 v) => Set(v);
        public bool SetUInt16(UInt16 v) => Set(v);
        public bool SetUInt32(UInt32 v) => Set(v);
        public bool SetUInt64(UInt64 v) => Set(v);

        public bool SetString(string v) => Set(v);
        public bool SetSingle(Single v) => Set(v);
        public bool SetDouble(Double v) => Set(v);

        #endregion

        private IEnumerable<ScriptNode> _enumerable = null;

        public bool HasMembers
        {
            get
            {
                if (_enumerable != null) return _enumerable.Any();
                else
                {
                    // Try to do it manually
                    switch (Object)
                    {
                        case WzProperty prop: return prop.HasMembers;
                        case TreeNode tn: return tn.Nodes.Count > 0;
                        case TreeView tv: return tv.Nodes.Count > 0;
                    }
                    return false;
                }
            }
        }
        private IEnumerable<ScriptNode> Members => _enumerable ?? (_enumerable = _getEnumerable());

        private IEnumerable<ScriptNode> _getEnumerable()
        {
            switch (Object)
            {
                case WzProperty prop:
                    return prop.Select(x => new ScriptNode(x.Key, x.Value, this, null));
                case TreeNode tn:
                    {
                        var nodes = new List<TreeNode>();
                        foreach (TreeNode v in tn.Nodes)
                        {
                            nodes.Add(v);
                        }
                        return nodes.Select(x => LoadTreeNodeInfo(x as WZTreeNode, x.Name));
                    }
                case TreeView tv:
                    {
                        var nodes = new List<TreeNode>();
                        foreach (TreeNode v in tv.Nodes)
                        {
                            nodes.Add(v);
                        }
                        return nodes.Select(x => LoadTreeNodeInfo(x as WZTreeNode, x.Name));
                    }
            }

            return Enumerable.Empty<ScriptNode>();
        }

        public override IEnumerator<ScriptNode> GetEnumerator() => Members.GetEnumerator();
    }
}
