using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Collections.Generic.Tree
{
    #region Tree
    public class Tree<T> : TreeNode<T>
    {
        public string TreeName { get { return _nodeName; } set { _nodeName = value; } }

        public new TreeNode<T> ParentNode { get { return _parentNode; } set { throw new TreeNodeException(TreeNodeResult.Results.错误_类型为Tree的根节点不能更改父节点, TreeName); } }

        /// <summary>
        /// 将树（根）节点转换成一个普通的非根节点，返回一个新的 TreeNode 实例。
        /// 原根节点的子节点将全部转移过去，根节点重新成为孤立根（叶端）节点。
        /// </summary>
        public TreeNode<T> ChangeToTreeNode()
        {
            return NewTreeNodeTakeFromTree(this);
        }

        /// <summary>
        /// 抛出以下异常：
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="treeName">树（根）节点的节点名。</param>
        /// <param name="data">节点数据。</param>
        public Tree(string treeName, T data = default(T)) : base(treeName, data) { }
    }

    public class TreeNode<T>
    {
        #region MongoDB 序列化支持

        static TreeNode()
        {
            var t = new TreeNodeSerializer();
            BsonSerializer.RegisterSerializer(typeof(Tree<T>), t);
            BsonSerializer.RegisterSerializer(typeof(TreeNode<T>), t);
        }

        private class TreeNodeSerializer : IBsonSerializer
        {
            public IBsonSerializationOptions GetDefaultSerializationOptions()
            {
                return null;
            }

            public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
            {
                var t = value as TreeNode<T>;
                if (null == t)
                    throw new Exception();
                bsonWriter.WriteStartDocument();
                if (t is Tree<T>)
                    bsonWriter.WriteString("TreeName", t.NodeName);
                if (t.Data is ValueType || null != t.Data)
                {
                    bsonWriter.WriteName("Data");
                    BsonSerializer.Serialize(bsonWriter, typeof(T), t.Data);
                }
                if (!t.IsEndNode)
                {
                    bsonWriter.WriteStartDocument("Nodes");
                    foreach (var node in t.Nodes.Values)
                    {
                        bsonWriter.WriteName(node.NodeName);
                        BsonSerializer.Serialize(bsonWriter, typeof(TreeNode<T>), node);
                    }
                    bsonWriter.WriteEndDocument();
                }
                bsonWriter.WriteEndDocument();
            }

            public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
            {
                return Deserialize(bsonReader, nominalType, nominalType, options);
            }

            public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
            {
                var t = actualType == typeof(Tree<T>)
                    ? new Tree<T>("Deserialized_Tree")
                    : new TreeNode<T>("Deserialized_TreeNode")
                    ;
                bsonReader.ReadStartDocument();
                var b = bsonReader.GetBookmark();
                if (bsonReader.FindElement("TreeName"))
                    t._nodeName = bsonReader.ReadString();
                else
                    bsonReader.ReturnToBookmark(b);
                b = bsonReader.GetBookmark();
                if (bsonReader.FindElement("Data"))
                    t.Data = BsonSerializer.Deserialize<T>(bsonReader);
                else
                    bsonReader.ReturnToBookmark(b);
                if (bsonReader.FindElement("Nodes"))
                {
                    t._nodes = BsonSerializer.Deserialize<TreeNodes>(bsonReader);
                    foreach (var kv in t._nodes) kv.Value._nodeName = kv.Key;
                }
                bsonReader.ReadEndDocument();
                return t;
            }
        }

        #endregion

        #region 内嵌类型

        protected class TreeNodes : Dictionary<string, TreeNode<T>> { }

        public class ReadOnlyTreeNodes : ReadOnlyDictionary<string, TreeNode<T>> { public ReadOnlyTreeNodes(IDictionary<string, TreeNode<T>> nodes) : base(nodes) { } }

        #endregion

        #region 成员、属性和索引器

        //成员：返回数据成员的属性
        public T Data { get; set; }
        protected TreeNodes _nodes;
        protected TreeNode<T> _parentNode;
        protected string _nodeName;

        //属性：从数据成员计算来的属性
        public ReadOnlyTreeNodes Nodes
        {
            get { return IsEndNode ? null : new ReadOnlyTreeNodes(_nodes); }
        }
        public bool IsEndNode
        {
            get { return null == _nodes; }
            set
            {
                if (value == IsEndNode) return;
                if (value)
                {
                    Clear();
                    _nodes = null;
                }
                else
                {
                    _nodes = new TreeNodes();
                }
            }
        }
        public string NodeName
        {
            get { return _nodeName; }
            set
            {
                if (null != _parentNode)
                {
                    var nodes = _parentNode._nodes;
                    if (nodes.ContainsKey(value))
                        throw new TreeNodeException(TreeNodeResult.Results.错误_已存在同名的子节点, value);
                    nodes.Remove(_nodeName);
                    nodes.Add(value, this);
                }
                _nodeName = value;
            }
        }
        public TreeNode<T> ParentNode
        {
            get { return _parentNode; }
            set
            {
                if (value == _parentNode) return;
                if (null != value)
                {
                    if (value.IsEndNode)
                        value._nodes = new TreeNodes();
                    else if (value._nodes.ContainsKey(_nodeName))
                        throw new TreeNodeException(TreeNodeResult.Results.错误_已存在同名的子节点, _nodeName);
                    if (null != _parentNode)
                        _parentNode._nodes.Remove(_nodeName);
                    value._nodes.Add(_nodeName, this);
                }
                _parentNode = value;
            }
        }

        //索引器
        /// <summary>
        /// 返回/设置子节点的值。
        /// 获取时，抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// 设置时，抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="nodePath">子节点名数组
        /// 获取时，须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// 设置时，须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </param>
        public T this[params string[] nodePath]
        {
            get { return GetNode(nodePath).Data; }
            set { GetOrAddNode(nodePath).Data = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addIfNotExists"></param>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public TreeNode<T> this[bool addIfNotExists, params string[] nodePath]
        {
            get { return GetNode(addIfNotExists, nodePath); }
            set { GetNode(addIfNotExists, nodePath).Data = value.Data; }
        }

        #endregion

        #region 方法：构造函数

        /// <summary>
        /// 抛出以下异常：
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="nodeName">本节点的节点名。</param>
        /// <param name="data">节点数据。</param>
        protected TreeNode(string nodeName, T data = default(T))
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new TreeNodeException(TreeNodeResult.错误_子节点名不能为null或空白);
            _nodeName = nodeName;
            Data = data;
        }

        /// <summary>
        /// 供派生类 Tree 类型转换为普通节点使用，返回一个新的 TreeNode 实例。
        /// 原根节点的子节点将全部转移过去，根节点重新成为孤立根（叶端）节点。
        /// </summary>
        /// <param name="tree">要被转换的 Tree 实例。</param>
        protected static TreeNode<T> NewTreeNodeTakeFromTree(Tree<T> tree)
        {
            var t = new TreeNode<T>(tree.TreeName, tree.Data) { _nodes = tree._nodes };
            foreach (var node in t._nodes.Values) node._parentNode = t;
            tree._nodes.Clear();
            tree._nodes = null;
            return t;
        }

        #endregion

        #region 方法：操作成员数据结构，对数据可能产生直接或间接的改变

        /// <summary>
        /// 获取子节点。
        /// 抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// 当 addIfNotExists 传入 false 时，还可能抛出以下异常：
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </summary>
        /// <param name="addIfNotExists">指定当找不到目标子节点时，是否添加新节点。</param>
        /// <param name="nodePath">子节点名数组。
        /// 须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// 当 addIfNotExists 传入 false 时，还须满足以下条件：
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </param>
        /// <returns></returns>
        public TreeNode<T> GetNode(bool addIfNotExists, params string[] nodePath)
        {
            TreeNode<T> node;
            var res = TryGetNode(out node, addIfNotExists, nodePath);
            if (TreeNodeResult.Results.我大于成功_我小于错误 > res.Result)
                return node;
            throw new TreeNodeException(res);
        }

        /// <summary>
        /// 获取子节点。
        /// 抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </summary>
        /// <param name="nodePath">子节点名数组。
        /// 须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </param>
        public TreeNode<T> GetNode(params string[] nodePath)
        {
            TreeNode<T> node;
            var res = TryGetNode(out node, nodePath);
            if (TreeNodeResult.Results.我大于成功_我小于错误 > res.Result)
                return node;
            throw new TreeNodeException(res);
        }

        /// <summary>
        /// 获取子节点。
        /// 抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="nodePath">子节点名数组。
        /// 须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </param>
        public TreeNode<T> GetOrAddNode(params string[] nodePath)
        {
            TreeNode<T> node;
            var res = TryGetOrAddNode(out node, nodePath);
            if (TreeNodeResult.Results.我大于成功_我小于错误 > res.Result)
                return node;
            throw new TreeNodeException(res);
        }

        public TreeNodeResult TryGetNode(out TreeNode<T> node, bool addIfNotExists, params string[] nodePath)
        {
            return addIfNotExists
                ? TryGetOrAddNode(out node, nodePath)
                : TryGetNode(out node, nodePath)
                ;
        }

        /// <summary>
        /// GetNode 的安全版本。不抛出异常。
        /// </summary>
        /// <param name="node">返回的节点。</param>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult TryGetNode(out TreeNode<T> node, params string[] nodePath)
        {
            node = null;
            if (null == nodePath)
                return TreeNodeResult.错误_子节点名数组不能为null;
            if (0 >= nodePath.Length)
                return TreeNodeResult.错误_子节点名数组必须包含至少一个值;
            var n = this;
            foreach (var subNodeName in nodePath)
            {
                if (string.IsNullOrWhiteSpace(subNodeName))
                    return TreeNodeResult.错误_子节点名不能为null或空白;
                if (n.IsEndNode)
                    return new TreeNodeResult(TreeNodeResult.Results.错误_叶端节点没有子节点, n._nodeName);
                if (!n._nodes.ContainsKey(subNodeName))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_子节点不存在, subNodeName);
                if (null == (n = n._nodes[subNodeName]))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_指定子节点本身为null, subNodeName);
            }
            node = n;
            return new TreeNodeResult(TreeNodeResult.Results.成功_已找到指定的子节点, n._nodeName);
        }

        /// <summary>
        /// GetOrAddNode 的安全版本。不抛出异常。
        /// </summary>
        /// <param name="node">返回的节点。</param>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult TryGetOrAddNode(out TreeNode<T> node, params string[] nodePath)
        {
            node = null;
            if (null == nodePath)
                return TreeNodeResult.错误_子节点名数组不能为null;
            if (0 >= nodePath.Length)
                return TreeNodeResult.错误_子节点名数组必须包含至少一个值;
            if (nodePath.Any(string.IsNullOrWhiteSpace))
                return TreeNodeResult.错误_子节点名不能为null或空白;
            var nodeList = new List<string>(nodePath.Length);
            node = nodePath.Aggregate(this, (cur, subNodeName) =>
            {
                if (cur.IsEndNode)
                    cur._nodes = new TreeNodes();
                else if (cur._nodes.ContainsKey(subNodeName))
                {
                    var n = cur._nodes[subNodeName];
                    if (null != n) return n;
                }
                var newNode = new TreeNode<T>(subNodeName);
                cur._nodes.Add(subNodeName, newNode);
                newNode._parentNode = cur;
                nodeList.Add(subNodeName);
                return newNode;
            });
            return 0 == nodeList.Count
                ? TreeNodeResult.成功_没有添加子任何节点
                : new TreeNodeResult(TreeNodeResult.Results.成功_已经添加子节点, nodeList.ToArray())
                ;
        }

        /// <summary>
        /// 添加子节点。
        /// </summary>
        /// <param name="nodes">要添加的子节点。</param>
        /// <returns></returns>
        public TreeNodeResult AddNodes(params TreeNode<T>[] nodes)
        {
            if (0 == nodes.Length) return TreeNodeResult.成功_没有添加子任何节点;
            //子节点数组自身有无重复
            if (1 < nodes.Length)
            {
                var gd = new Dictionary<string, List<TreeNode<T>>>();
                foreach (var node in nodes)
                {
                    if (null == gd[node._nodeName])
                        gd[node._nodeName] = new List<TreeNode<T>>();
                    gd[node._nodeName].Add(node);
                }
                if (gd.Count > 1)
                    return new TreeNodeResult(
                        TreeNodeResult.Results.错误_子节点数组中存在同名的子节点,
                        gd.Select(kv => string.Format("{0}x{1}", kv.Key, kv.Value.Count())).ToArray()
                        );
            }
            if (!IsEndNode)
            {
                if (nodes.Any(node => string.IsNullOrWhiteSpace(node._nodeName)))
                    return TreeNodeResult.错误_子节点名不能为null或空白;
                //子节点数组与现有子节点有无重复
                var dupNodes = nodes.Where(n => _nodes.ContainsKey(n._nodeName)).Select(n => n._nodeName).ToArray();
                if (0 < dupNodes.Length)
                    return new TreeNodeResult(TreeNodeResult.Results.错误_已存在同名的子节点, dupNodes);
            }
            else _nodes = new TreeNodes();
            var nodeList = new List<string>(nodes.Length);
            foreach (var node in nodes)
            {
                if (null != node._parentNode) node._parentNode._nodes.Remove(node._nodeName);
                _nodes.Add(node._nodeName, node);
                node._parentNode = this;
                nodeList.Add(node._nodeName);
            }
            return new TreeNodeResult(TreeNodeResult.Results.成功_已经添加子节点, nodeList.ToArray());
        }

        /// <summary>
        /// 添加子节点。
        /// </summary>
        /// <param name="nodes">要添加的子节点。</param>
        /// <returns></returns>
        public TreeNodeResult AddNodes(IEnumerable<TreeNode<T>> nodes)
        {
            return AddNodes(nodes.ToArray());
        }

        /// <summary>
        /// 移除指定的子节点。不抛出异常。
        /// </summary>
        /// <param name="node">被移除的子节点。</param>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult RemoveNode(out TreeNode<T> node, params string[] nodePath)
        {
            var res = TryGetNode(out node, nodePath);
            switch (res.Result)
            {
                case TreeNodeResult.Results.错误_子节点不存在:
                    return TreeNodeResult.成功_没有删除任何子节点;
                case TreeNodeResult.Results.成功_已找到指定的子节点:
                    node._parentNode._nodes.Remove(node._nodeName);
                    node._parentNode = null;
                    return new TreeNodeResult(TreeNodeResult.Results.成功_已经删除子节点, node._nodeName);
                default:
                    return res;
            }
        }

        /// <summary>
        /// 移除指定的子节点。不抛出异常。
        /// </summary>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult RemoveNode(params string[] nodePath)
        {
            TreeNode<T> node;
            return RemoveNode(out node, nodePath);
        }

        /// <summary>
        /// 清除所有的子节点。
        /// </summary>
        public void Clear()
        {
            if (IsEndNode) return;
            foreach (var node in _nodes.Values) node._parentNode = null;
            _nodes.Clear();
        }

        #endregion

        #region 方法：从数据成员和传入参数计算结果，不对数据成员进行改变

        public bool ContentEquals(TreeNode<T> r)
        {
            if (this == r) return true;
            if (r == null) return false;
            if (_nodeName != r._nodeName) return false;
            if (!EqualityComparer<T>.Default.Equals(Data, r.Data)) return false;
            if (_nodes == r._nodes) return true;
            if (IsEndNode) return false;
            if (r.IsEndNode) return false;
            if (_nodes.Count != r._nodes.Count) return false;
            foreach (var kv in _nodes)
            {
                if (!r._nodes.ContainsKey(kv.Key)) return false;
                if (!EqualityComparer<T>.Default.Equals(kv.Value.Data, r._nodes[kv.Key].Data)) return false;
            }
            return true;
        }

        /// <summary>
        /// 判断子节点是否存在。
        /// </summary>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult SubNodeExists(params string[] nodePath)
        {
            if (null == nodePath)
                return TreeNodeResult.错误_子节点名数组不能为null;
            if (0 >= nodePath.Length)
                return TreeNodeResult.错误_子节点名数组必须包含至少一个值;
            var node = this;
            foreach (var subNodeName in nodePath)
            {
                if (string.IsNullOrWhiteSpace(subNodeName))
                    return TreeNodeResult.错误_子节点名不能为null或空白;
                if (node.IsEndNode)
                    return new TreeNodeResult(TreeNodeResult.Results.错误_叶端节点没有子节点, node._nodeName);
                if (!node._nodes.ContainsKey(subNodeName))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_子节点不存在, subNodeName);
                if (null == (node = node._nodes[subNodeName]))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_指定子节点本身为null, subNodeName);
            }
            return new TreeNodeResult(TreeNodeResult.Results.成功_已找到指定的子节点, node._nodeName);
        }

        /// <summary>
        /// 返回子节点数量。
        /// </summary>
        /// <param name="includeAll">是否包括所有子节点。</param>
        /// <param name="includeSelf">是否包括该节点自身。</param>
        public long NodesCountLong(bool includeAll = false, bool includeSelf = false)
        {
            return IsEndNode
                ? 0
                : (includeSelf ? 1 : 0) + _nodes.Count +
                  (includeAll ? _nodes.Sum(o => o.Value.NodesCountLong(true)) : 0)
                ;
        }

        /// <summary>
        /// 返回子节点数量。
        /// </summary>
        /// <param name="includeAll">是否包括所有子节点。</param>
        /// <param name="includeSelf">是否包括该节点自身。</param>
        public int NodesCount(bool includeAll = false, bool includeSelf = false)
        {
            return (int)NodesCountLong(includeAll, includeSelf);
        }

        #endregion
    }
    #endregion

    #region SortedTree
    public class SortedTree<T> : SortedTreeNode<T>
    {
        public string TreeName { get { return _nodeName; } set { _nodeName = value; } }

        public new SortedTreeNode<T> ParentNode { get { return _parentNode; } set { throw new TreeNodeException(TreeNodeResult.Results.错误_类型为Tree的根节点不能更改父节点, TreeName); } }

        /// <summary>
        /// 将树（根）节点转换成一个普通的非根节点，返回一个新的 TreeNode 实例。
        /// 原根节点的子节点将全部转移过去，根节点重新成为孤立根（叶端）节点。
        /// </summary>
        public SortedTreeNode<T> ChangeToTreeNode()
        {
            return NewTreeNodeTakeFromTree(this);
        }

        /// <summary>
        /// 抛出以下异常：
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="treeName">树（根）节点的节点名。</param>
        /// <param name="data">节点数据。</param>
        public SortedTree(string treeName, T data = default(T)) : base(treeName, data) { }
    }

    public class SortedTreeNode<T>
    {
        #region MongoDB 序列化支持

        static SortedTreeNode()
        {
            var t = new SortedTreeNodeSerializer();
            BsonSerializer.RegisterSerializer(typeof(SortedTree<T>), t);
            BsonSerializer.RegisterSerializer(typeof(SortedTreeNode<T>), t);
        }

        private class SortedTreeNodeSerializer : IBsonSerializer
        {
            public IBsonSerializationOptions GetDefaultSerializationOptions()
            {
                return null;
            }

            public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
            {
                var t = value as SortedTreeNode<T>;
                if (null == t)
                    throw new Exception();
                bsonWriter.WriteStartDocument();
                if (t is SortedTree<T>)
                    bsonWriter.WriteString("TreeName", t.NodeName);
                if (t.Data is ValueType || null != t.Data)
                {
                    bsonWriter.WriteName("Data");
                    BsonSerializer.Serialize(bsonWriter, typeof(T), t.Data);
                }
                if (!t.IsEndNode)
                {
                    bsonWriter.WriteStartDocument("Nodes");
                    foreach (var node in t.Nodes.Values)
                    {
                        bsonWriter.WriteName(node.NodeName);
                        BsonSerializer.Serialize(bsonWriter, typeof(SortedTreeNode<T>), node);
                    }
                    bsonWriter.WriteEndDocument();
                }
                bsonWriter.WriteEndDocument();
            }

            public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
            {
                return Deserialize(bsonReader, nominalType, nominalType, options);
            }

            public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
            {
                var t = actualType == typeof(SortedTree<T>)
                    ? new SortedTree<T>("Deserialized_Tree")
                    : new SortedTreeNode<T>("Deserialized_TreeNode")
                    ;
                bsonReader.ReadStartDocument();
                var b = bsonReader.GetBookmark();
                if (bsonReader.FindElement("TreeName"))
                    t._nodeName = bsonReader.ReadString();
                else
                    bsonReader.ReturnToBookmark(b);
                b = bsonReader.GetBookmark();
                if (bsonReader.FindElement("Data"))
                    t.Data = BsonSerializer.Deserialize<T>(bsonReader);
                else
                    bsonReader.ReturnToBookmark(b);
                if (bsonReader.FindElement("Nodes"))
                {
                    t._nodes = BsonSerializer.Deserialize<TreeNodes>(bsonReader);
                    foreach (var kv in t._nodes) kv.Value._nodeName = kv.Key;
                }
                bsonReader.ReadEndDocument();
                return t;
            }
        }

        #endregion

        #region 内嵌类型

        protected class TreeNodes : SortedDictionary<string, SortedTreeNode<T>> { }

        public class ReadOnlyTreeNodes : ReadOnlyDictionary<string, SortedTreeNode<T>> { public ReadOnlyTreeNodes(IDictionary<string, SortedTreeNode<T>> nodes) : base(nodes) { } }

        #endregion

        #region 成员、属性和索引器

        //成员：返回数据成员的属性
        public T Data { get; set; }
        protected TreeNodes _nodes;
        protected SortedTreeNode<T> _parentNode;
        protected string _nodeName;

        //属性：从数据成员计算来的属性
        public ReadOnlyTreeNodes Nodes
        {
            get { return IsEndNode ? null : new ReadOnlyTreeNodes(_nodes); }
        }
        public bool IsEndNode
        {
            get { return null == _nodes; }
            set
            {
                if (value == IsEndNode) return;
                if (value)
                {
                    Clear();
                    _nodes = null;
                }
                else
                {
                    _nodes = new TreeNodes();
                }
            }
        }
        public string NodeName
        {
            get { return _nodeName; }
            set
            {
                if (null != _parentNode)
                {
                    var nodes = _parentNode._nodes;
                    if (nodes.ContainsKey(value))
                        throw new TreeNodeException(TreeNodeResult.Results.错误_已存在同名的子节点, value);
                    nodes.Remove(_nodeName);
                    nodes.Add(value, this);
                }
                _nodeName = value;
            }
        }
        public SortedTreeNode<T> ParentNode
        {
            get { return _parentNode; }
            set
            {
                if (value == _parentNode) return;
                if (null != value)
                {
                    if (value.IsEndNode)
                        value._nodes = new TreeNodes();
                    else if (value._nodes.ContainsKey(_nodeName))
                        throw new TreeNodeException(TreeNodeResult.Results.错误_已存在同名的子节点, _nodeName);
                    if (null != _parentNode)
                        _parentNode._nodes.Remove(_nodeName);
                    value._nodes.Add(_nodeName, this);
                }
                _parentNode = value;
            }
        }

        //索引器
        /// <summary>
        /// 返回/设置子节点的值。
        /// 获取时，抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// 设置时，抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="nodePath">子节点名数组
        /// 获取时，须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// 设置时，须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </param>
        public T this[params string[] nodePath]
        {
            get { return GetNode(nodePath).Data; }
            set { GetOrAddNode(nodePath).Data = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addIfNotExists"></param>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public SortedTreeNode<T> this[bool addIfNotExists, params string[] nodePath]
        {
            get { return GetNode(addIfNotExists, nodePath); }
            set { GetNode(addIfNotExists, nodePath).Data = value.Data; }
        }

        #endregion

        #region 方法：构造函数

        /// <summary>
        /// 抛出以下异常：
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="nodeName">本节点的节点名。</param>
        /// <param name="data">节点数据。</param>
        protected SortedTreeNode(string nodeName, T data = default(T))
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new TreeNodeException(TreeNodeResult.错误_子节点名不能为null或空白);
            _nodeName = nodeName;
            Data = data;
        }

        /// <summary>
        /// 供派生类 Tree 类型转换为普通节点使用，返回一个新的 TreeNode 实例。
        /// 原根节点的子节点将全部转移过去，根节点重新成为孤立根（叶端）节点。
        /// </summary>
        /// <param name="tree">要被转换的 Tree 实例。</param>
        protected static SortedTreeNode<T> NewTreeNodeTakeFromTree(SortedTree<T> tree)
        {
            var t = new SortedTreeNode<T>(tree.TreeName, tree.Data) { _nodes = tree._nodes };
            foreach (var node in t._nodes.Values) node._parentNode = t;
            tree._nodes.Clear();
            tree._nodes = null;
            return t;
        }

        #endregion

        #region 方法：操作成员数据结构，对数据可能产生直接或间接的改变

        /// <summary>
        /// 获取子节点。
        /// 抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// 当 addIfNotExists 传入 false 时，还可能抛出以下异常：
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </summary>
        /// <param name="addIfNotExists">指定当找不到目标子节点时，是否添加新节点。</param>
        /// <param name="nodePath">子节点名数组。
        /// 须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// 当 addIfNotExists 传入 false 时，还须满足以下条件：
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </param>
        /// <returns></returns>
        public SortedTreeNode<T> GetNode(bool addIfNotExists, params string[] nodePath)
        {
            SortedTreeNode<T> node;
            var res = TryGetNode(out node, addIfNotExists, nodePath);
            if (TreeNodeResult.Results.我大于成功_我小于错误 > res.Result)
                return node;
            throw new TreeNodeException(res);
        }

        /// <summary>
        /// 获取子节点。
        /// 抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </summary>
        /// <param name="nodePath">子节点名数组。
        /// 须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// - 叶端节点没有子节点
        /// - 子节点不存在
        /// - 指定子节点本身为null
        /// </param>
        public SortedTreeNode<T> GetNode(params string[] nodePath)
        {
            SortedTreeNode<T> node;
            var res = TryGetNode(out node, nodePath);
            if (TreeNodeResult.Results.我大于成功_我小于错误 > res.Result)
                return node;
            throw new TreeNodeException(res);
        }

        /// <summary>
        /// 获取子节点。
        /// 抛出以下异常：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </summary>
        /// <param name="nodePath">子节点名数组。
        /// 须满足以下条件：
        /// - 子节点名数组不能为null
        /// - 子节点名数组必须包含至少一个值
        /// - 子节点名不能为null或空白
        /// </param>
        public SortedTreeNode<T> GetOrAddNode(params string[] nodePath)
        {
            SortedTreeNode<T> node;
            var res = TryGetOrAddNode(out node, nodePath);
            if (TreeNodeResult.Results.我大于成功_我小于错误 > res.Result)
                return node;
            throw new TreeNodeException(res);
        }

        public TreeNodeResult TryGetNode(out SortedTreeNode<T> node, bool addIfNotExists, params string[] nodePath)
        {
            return addIfNotExists
                ? TryGetOrAddNode(out node, nodePath)
                : TryGetNode(out node, nodePath)
                ;
        }

        /// <summary>
        /// GetNode 的安全版本。不抛出异常。
        /// </summary>
        /// <param name="node">返回的节点。</param>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult TryGetNode(out SortedTreeNode<T> node, params string[] nodePath)
        {
            node = null;
            if (null == nodePath)
                return TreeNodeResult.错误_子节点名数组不能为null;
            if (0 >= nodePath.Length)
                return TreeNodeResult.错误_子节点名数组必须包含至少一个值;
            var n = this;
            foreach (var subNodeName in nodePath)
            {
                if (string.IsNullOrWhiteSpace(subNodeName))
                    return TreeNodeResult.错误_子节点名不能为null或空白;
                if (n.IsEndNode)
                    return new TreeNodeResult(TreeNodeResult.Results.错误_叶端节点没有子节点, n._nodeName);
                if (!n._nodes.ContainsKey(subNodeName))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_子节点不存在, subNodeName);
                if (null == (n = n._nodes[subNodeName]))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_指定子节点本身为null, subNodeName);
            }
            node = n;
            return new TreeNodeResult(TreeNodeResult.Results.成功_已找到指定的子节点, n._nodeName);
        }

        /// <summary>
        /// GetOrAddNode 的安全版本。不抛出异常。
        /// </summary>
        /// <param name="node">返回的节点。</param>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult TryGetOrAddNode(out SortedTreeNode<T> node, params string[] nodePath)
        {
            node = null;
            if (null == nodePath)
                return TreeNodeResult.错误_子节点名数组不能为null;
            if (0 >= nodePath.Length)
                return TreeNodeResult.错误_子节点名数组必须包含至少一个值;
            if (nodePath.Any(string.IsNullOrWhiteSpace))
                return TreeNodeResult.错误_子节点名不能为null或空白;
            var nodeList = new List<string>(nodePath.Length);
            node = nodePath.Aggregate(this, (cur, subNodeName) =>
            {
                if (cur.IsEndNode)
                    cur._nodes = new TreeNodes();
                else if (cur._nodes.ContainsKey(subNodeName))
                {
                    var n = cur._nodes[subNodeName];
                    if (null != n) return n;
                }
                var newNode = new SortedTreeNode<T>(subNodeName);
                cur._nodes.Add(subNodeName, newNode);
                newNode._parentNode = cur;
                nodeList.Add(subNodeName);
                return newNode;
            });
            return 0 == nodeList.Count
                ? TreeNodeResult.成功_没有添加子任何节点
                : new TreeNodeResult(TreeNodeResult.Results.成功_已经添加子节点, nodeList.ToArray())
                ;
        }

        /// <summary>
        /// 添加子节点。
        /// </summary>
        /// <param name="nodes">要添加的子节点。</param>
        /// <returns></returns>
        public TreeNodeResult AddNodes(params SortedTreeNode<T>[] nodes)
        {
            if (0 == nodes.Length) return TreeNodeResult.成功_没有添加子任何节点;
            //子节点数组自身有无重复
            if (1 < nodes.Length)
            {
                var gd = new Dictionary<string, List<SortedTreeNode<T>>>();
                foreach (var node in nodes)
                {
                    if (null == gd[node._nodeName])
                        gd[node._nodeName] = new List<SortedTreeNode<T>>();
                    gd[node._nodeName].Add(node);
                }
                if (gd.Count > 1)
                    return new TreeNodeResult(
                        TreeNodeResult.Results.错误_子节点数组中存在同名的子节点,
                        gd.Select(kv => string.Format("{0}x{1}", kv.Key, kv.Value.Count())).ToArray()
                        );
            }
            if (!IsEndNode)
            {
                if (nodes.Any(node => string.IsNullOrWhiteSpace(node._nodeName)))
                    return TreeNodeResult.错误_子节点名不能为null或空白;
                //子节点数组与现有子节点有无重复
                var dupNodes = nodes.Where(n => _nodes.ContainsKey(n._nodeName)).Select(n => n._nodeName).ToArray();
                if (0 < dupNodes.Length)
                    return new TreeNodeResult(TreeNodeResult.Results.错误_已存在同名的子节点, dupNodes);
            }
            else _nodes = new TreeNodes();
            var nodeList = new List<string>(nodes.Length);
            foreach (var node in nodes)
            {
                if (null != node._parentNode) node._parentNode._nodes.Remove(node._nodeName);
                _nodes.Add(node._nodeName, node);
                node._parentNode = this;
                nodeList.Add(node._nodeName);
            }
            return new TreeNodeResult(TreeNodeResult.Results.成功_已经添加子节点, nodeList.ToArray());
        }

        /// <summary>
        /// 添加子节点。
        /// </summary>
        /// <param name="nodes">要添加的子节点。</param>
        /// <returns></returns>
        public TreeNodeResult AddNodes(IEnumerable<SortedTreeNode<T>> nodes)
        {
            return AddNodes(nodes.ToArray());
        }

        /// <summary>
        /// 移除指定的子节点。不抛出异常。
        /// </summary>
        /// <param name="node">被移除的子节点。</param>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult RemoveNode(out SortedTreeNode<T> node, params string[] nodePath)
        {
            var res = TryGetNode(out node, nodePath);
            switch (res.Result)
            {
                case TreeNodeResult.Results.错误_子节点不存在:
                    return TreeNodeResult.成功_没有删除任何子节点;
                case TreeNodeResult.Results.成功_已找到指定的子节点:
                    node._parentNode._nodes.Remove(node._nodeName);
                    node._parentNode = null;
                    return new TreeNodeResult(TreeNodeResult.Results.成功_已经删除子节点, node._nodeName);
                default:
                    return res;
            }
        }

        /// <summary>
        /// 移除指定的子节点。不抛出异常。
        /// </summary>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult RemoveNode(params string[] nodePath)
        {
            SortedTreeNode<T> node;
            return RemoveNode(out node, nodePath);
        }

        /// <summary>
        /// 清除所有的子节点。
        /// </summary>
        public void Clear()
        {
            if (IsEndNode) return;
            foreach (var node in _nodes.Values) node._parentNode = null;
            _nodes.Clear();
        }

        #endregion

        #region 方法：从数据成员和传入参数计算结果，不对数据成员进行改变

        public bool ContentEquals(SortedTreeNode<T> r)
        {
            if (this == r) return true;
            if (r == null) return false;
            if (_nodeName != r._nodeName) return false;
            if (!EqualityComparer<T>.Default.Equals(Data, r.Data)) return false;
            if (_nodes == r._nodes) return true;
            if (IsEndNode) return false;
            if (r.IsEndNode) return false;
            if (_nodes.Count != r._nodes.Count) return false;
            foreach (var kv in _nodes)
            {
                if (!r._nodes.ContainsKey(kv.Key)) return false;
                if (!EqualityComparer<T>.Default.Equals(kv.Value.Data, r._nodes[kv.Key].Data)) return false;
            }
            return true;
        }

        /// <summary>
        /// 判断子节点是否存在。
        /// </summary>
        /// <param name="nodePath">子节点名数组。</param>
        public TreeNodeResult SubNodeExists(params string[] nodePath)
        {
            if (null == nodePath)
                return TreeNodeResult.错误_子节点名数组不能为null;
            if (0 >= nodePath.Length)
                return TreeNodeResult.错误_子节点名数组必须包含至少一个值;
            var node = this;
            foreach (var subNodeName in nodePath)
            {
                if (string.IsNullOrWhiteSpace(subNodeName))
                    return TreeNodeResult.错误_子节点名不能为null或空白;
                if (node.IsEndNode)
                    return new TreeNodeResult(TreeNodeResult.Results.错误_叶端节点没有子节点, node._nodeName);
                if (!node._nodes.ContainsKey(subNodeName))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_子节点不存在, subNodeName);
                if (null == (node = node._nodes[subNodeName]))
                    return new TreeNodeResult(TreeNodeResult.Results.错误_指定子节点本身为null, subNodeName);
            }
            return new TreeNodeResult(TreeNodeResult.Results.成功_已找到指定的子节点, node._nodeName);
        }

        /// <summary>
        /// 返回子节点数量。
        /// </summary>
        /// <param name="includeAll">是否包括所有子节点。</param>
        /// <param name="includeSelf">是否包括该节点自身。</param>
        public long NodesCountLong(bool includeAll = false, bool includeSelf = false)
        {
            return IsEndNode
                ? 0
                : (includeSelf ? 1 : 0) + _nodes.Count +
                  (includeAll ? _nodes.Sum(o => o.Value.NodesCountLong(true)) : 0)
                ;
        }

        /// <summary>
        /// 返回子节点数量。
        /// </summary>
        /// <param name="includeAll">是否包括所有子节点。</param>
        /// <param name="includeSelf">是否包括该节点自身。</param>
        public int NodesCount(bool includeAll = false, bool includeSelf = false)
        {
            return (int)NodesCountLong(includeAll, includeSelf);
        }

        #endregion
    }
    #endregion

    #region TreeNodeResult & TreeNodeException
    public class TreeNodeResult
    {
        public static readonly TreeNodeResult 成功_没有添加子任何节点 = new TreeNodeResult(Results.成功_没有添加子任何节点);
        public static readonly TreeNodeResult 成功_没有删除任何子节点 = new TreeNodeResult(Results.成功_没有删除任何子节点);

        public static readonly TreeNodeResult 错误_子节点名数组不能为null = new TreeNodeResult(Results.错误_子节点名数组不能为null);
        public static readonly TreeNodeResult 错误_子节点名数组必须包含至少一个值 = new TreeNodeResult(Results.错误_子节点名数组必须包含至少一个值);
        public static readonly TreeNodeResult 错误_子节点名不能为null或空白 = new TreeNodeResult(Results.错误_子节点名不能为null或空白);

        public enum Results
        {
            成功_已找到指定的子节点 = 101,
            成功_已经添加子节点 = 102,
            成功_没有添加子任何节点 = 103,
            成功_已经删除子节点 = 104,
            成功_没有删除任何子节点 = 105,
            我大于成功_我小于错误 = 400,
            错误_子节点名数组不能为null = 401,
            错误_子节点名数组必须包含至少一个值 = 402,
            错误_子节点名不能为null或空白 = 403,
            错误_子节点数组中存在同名的子节点 = 404,
            错误_叶端节点没有子节点 = 405,
            错误_子节点不存在 = 406,
            错误_指定子节点本身为null = 407,
            错误_类型为Tree的根节点不能更改父节点 = 408,
            错误_已存在同名的子节点 = 409,
        }

        private readonly Results _result;
        private readonly string _nodeNames;

        public Results Result
        {
            get { return _result; }
        }

        public string NodeNames
        {
            get { return _nodeNames; }
        }

        public TreeNodeResult(Results result, params string[] nodeNames)
        {
            _result = result;
            _nodeNames = 0 == nodeNames.Length
                ? null
                : string.Join(",", nodeNames)
                ;
        }
    }

    public class TreeNodeException : Exception
    {
        public TreeNodeResult TreeNodeExceptionResult { get; protected set; }

        public TreeNodeException(TreeNodeResult reason)
        {
            TreeNodeExceptionResult = reason;
        }

        public TreeNodeException(TreeNodeResult.Results reason, string nodeName)
        {
            TreeNodeExceptionResult = new TreeNodeResult(reason, nodeName);
        }

        public override string Message
        {
            get
            {
                return null == TreeNodeExceptionResult.NodeNames
                    ? TreeNodeExceptionResult.Result.ToString()
                    : TreeNodeExceptionResult.Result + "：" + TreeNodeExceptionResult.NodeNames
                    ;
            }
        }
    }
    #endregion
}