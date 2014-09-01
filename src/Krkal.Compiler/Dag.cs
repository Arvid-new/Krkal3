//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - D a g
///
///		Directed acyclic graph
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Krkal.Compiler
{

	public interface IDagNodeDirection<TNode> where TNode : class, IDagNode<TNode>
	{
		IDagNode<TNode> Node {
			get;
		}

		Direction Direction {
			get;
		}

		IList<TNode> Edges {
			get;
		}

		ICollection<TNode> Layer(Predicate<TNode> match);

		ICollection<TNode> TransitiveNodes {
			get;
		}

		ICollection<TNode> FilteredTransitiveNodes(Predicate<TNode> match);

		bool IsRelated(TNode node);

	}



	public interface IDagNode<TNode> where TNode : class, IDagNode<TNode>
	{
		Dag<TNode> Dag {
			get;
		}

		void RemoveAllEdges();

		GraphColor Color {
			get;
			set;
		}

		void Invalidate();

		IDagNodeDirection<TNode> this[Direction direction] {
			get;
		}

		IList<TNode> Children {
			get;
		}
		IList<TNode> Parents {
			get;
		}

		bool IsAncestor(TNode node);
		bool IsDescendant(TNode node);

	}









	public class DagNode<TNode> : IDagNode<TNode> where TNode : DagNode<TNode>
	{


		#region class EdgeCollection
		internal abstract class EdgeCollection : KeyedCollection<TNode, TNode>, IList<TNode>, ICollection<TNode>, IList, ICollection
		{

			private TNode _node;
			protected TNode Node {
				get { return _node; }
			}

			// CONSTRUCTOR

			internal EdgeCollection(TNode node)
				: base(null, 8) 
			{
				_node = node;
			}


			protected override TNode GetKeyForItem(TNode item) {
				return item;
			}


			protected override void InsertItem(int index, TNode item) {
				base.InsertItem(index, item);
				_node.Dag.Invalidate();
				AddEdge(item);
			}

			protected override void RemoveItem(int index) {
				TNode temp = Items[index];
				base.RemoveItem(index);
				_node.Dag.Invalidate();
				RemoveEdge(temp);
			}

			protected override void SetItem(int index, TNode item) {
				TNode temp = Items[index];
				base.SetItem(index, item);
				_node.Dag.Invalidate();
				RemoveEdge(temp);
				AddEdge(item);
			}


			public new void Add(TNode item) {
				if (!Contains(item)) {
					base.Add(item);
				}
			}

			protected abstract void AddEdge(TNode item);
			protected abstract void RemoveEdge(TNode item);


		}
		#endregion

		#region ParentsCollection
		internal class ParentsCollection : EdgeCollection
		{
			internal ParentsCollection(TNode node) : base(node) { }

			protected override void AddEdge(TNode item) {
				item[Direction.Down].Edges.Add(Node);
			}

			protected override void RemoveEdge(TNode item) {
				item[Direction.Down].Edges.Remove(Node);
			}

			protected override void ClearItems() {
				TNode[] temp = new TNode[Count];
				Items.CopyTo(temp, 0);

				base.ClearItems();
				Node.Dag.Invalidate();

				foreach (TNode member in temp) {
					member[Direction.Down].Edges.Remove(Node);
				}
			}

		}
		#endregion

		#region ChildrenCollection
		internal class ChildrenCollection : EdgeCollection
		{
			internal ChildrenCollection(TNode node) : base(node) { }

			protected override void AddEdge(TNode item) {
				item[Direction.Up].Edges.Add(Node);
			}

			protected override void RemoveEdge(TNode item) {
				item[Direction.Up].Edges.Remove(Node);
			}

			protected override void ClearItems() {
				TNode[] temp = new TNode[Count];
				Items.CopyTo(temp, 0);

				base.ClearItems();
				Node.Dag.Invalidate();

				foreach (TNode member in temp) {
					member[Direction.Up].Edges.Remove(Node);
				}
			}

		}
		#endregion



		#region class NodeDirection

		private abstract class NodeDirection : IDagNodeDirection<TNode>
		{
			// CONSTRUCTOR
			internal NodeDirection(DagNode<TNode> node) {
				_node = node;
			}


			private DagNode<TNode> _node;
			public IDagNode<TNode> Node {
				get { return _node; }
			}

			public Direction Direction {
				get { return GetDirection(); }
			}

			protected abstract Direction GetDirection();

			private IList<TNode> _edges;
			public IList<TNode> Edges {
				get { return _edges; }
				internal set { _edges = value; }
			}


			private Set<TNode> _transitiveSet;
			private Set<TNode> TransitiveSet {
				get {
					TestValidity();
					if (_transitiveSet == null) {
						_transitiveSet = new Set<TNode>();
						Search(_edges);
					}
					return _transitiveSet;
				}
			}

			private ReadOnlyCollection<TNode> _roTransitiveNodes;
			public ICollection<TNode> TransitiveNodes {
				get {
					TestValidity();
					if (_roTransitiveNodes == null) {
						_roTransitiveNodes = new ReadOnlyCollection<TNode>(TransitiveSet);
					}
					return _roTransitiveNodes;
				}
			}


			private void TestValidity() {
				if (_node._dag == null)
					throw new InternalCompilerException("Add this node to a Dag first.");
				if (!_node._dag.Valid)
					_node._dag.Calculate();
			}



			private void Search(IList<TNode> edges) {
				foreach (TNode node in edges) {
					if (!_transitiveSet.Contains(node)) {
						_transitiveSet.Add(node);
						Search(node[Direction].Edges);
					}
				}
			}



			public ICollection<TNode> Layer(Predicate<TNode> match) {
				LinkedList<TNode> output = new LinkedList<TNode>();
				Dag<TNode>.FindLayer(_edges, output, match, Direction);
				return output;
			}


			public ICollection<TNode> FilteredTransitiveNodes(Predicate<TNode> match) {
				if (match == null) {
					return TransitiveNodes;
				} else {
					return Filter(TransitiveSet, match);
				}
			}

			private static ICollection<TNode> Filter(ICollection<TNode> collection, Predicate<TNode> match) {
				List<TNode> output = new List<TNode>();
				foreach (TNode n in collection) {
					if (match(n))
						output.Add(n);
				}
				return output;
			}


			public bool IsRelated(TNode node) {
				return TransitiveSet.Contains(node);
			}


			internal void Invalidate() {
				_transitiveSet = null;
				_roTransitiveNodes = null;
			}

		}

		#endregion

		#region class NodeDirectionUp
		class NodeDirectionUp : NodeDirection
		{
			// CONSTRUCTOR
			internal NodeDirectionUp(DagNode<TNode> node) : base(node) {}

			protected override Direction GetDirection() {
				return Direction.Up;
			}

		}
		#endregion

		#region class NodeDirectionDown
		class NodeDirectionDown : NodeDirection
		{
			// CONSTRUCTOR
			internal NodeDirectionDown(DagNode<TNode> node) : base(node) {}

			protected override Direction GetDirection() {
				return Direction.Down;
			}

		}
		#endregion


		///////////////////////////////////////////////////////////////////////////////


		private Dag<TNode> _dag;
		public Dag<TNode> Dag {
			get { return _dag; }
		}

		private NodeDirection _down;
		private NodeDirection _up;

		public IDagNodeDirection<TNode> this[Direction direction] {
			get {
				return direction == Direction.Down ? _down : _up;
			}
		}


		// CONSTRUCTOR

		protected DagNode() {
			_up = new NodeDirectionUp(this);
			_down = new NodeDirectionDown(this);
		}


		public void RemoveAllEdges() {
			if (_dag != null) {
				_up.Edges.Clear();
				_down.Edges.Clear();
			}
		}


		private GraphColor _color;
		public GraphColor Color {
			get { return _color; }
			set { _color = value; }
		}



		protected void AddToDag(Dag<TNode> dag) {
			if (_dag != null)
				RemoveFromDag();
			_dag = dag;
			_down.Edges = new ChildrenCollection((TNode)this);
			_up.Edges = new ParentsCollection((TNode)this);
			_dag.AddToList((TNode)this);
		}

		public void RemoveFromDag() {
			if (_dag != null) {
				RemoveAllEdges();
				Invalidate();
				_dag.RemoveFromList((TNode)this);
				_dag = null;
				_down.Edges = null;
				_up.Edges = null;
			}
		}


		public void Invalidate() {
			_up.Invalidate();
			_down.Invalidate();
		}


		public IList<TNode> Children {
			get { return _down.Edges; }
		}
		public IList<TNode> Parents {
			get { return _up.Edges; }
		}

		public bool IsAncestor(TNode node) {
			return _up.IsRelated(node);
		}
		public bool IsDescendant(TNode node) {
			return _down.IsRelated(node);
		}

	}






	public class Dag<TNode> : IEnumerable<TNode>
		where TNode : class, IDagNode<TNode>
	{
		private List<TNode> _nodes = new List<TNode>();


		public TNode this[int index] {
			get { return _nodes[index]; }
		}

		public int Count {
			get { return _nodes.Count; }
		}


		internal void AddToList(TNode node) {
			_nodes.Add(node);
			InvalidateRoots();
		}
		internal void RemoveFromList(TNode node) {
			_nodes.Remove(node);
			InvalidateRoots();
		}


		private List<TNode>[] _roots = new List<TNode>[2];
		private ReadOnlyCollection<TNode>[] _roRoots = new ReadOnlyCollection<TNode>[2];

		private List<TNode> GetRoots(Direction direction) {
			int index = (int)direction;
			if (_roots[index] == null) {
				_roots[index] = new List<TNode>();
				foreach (TNode n in _nodes) {
					if (n[ReverseDirection(direction)].Edges.Count == 0)
						_roots[index].Add(n);
				}
			}
			return _roots[index]; 
		}

		private ReadOnlyCollection<TNode> GetRORoots(Direction direction) {
			int index = (int)direction;
			if (_roRoots[index] == null)
				_roRoots[index] = new ReadOnlyCollection<TNode>(GetRoots(direction));
			return _roRoots[index]; 
		}





		public ICollection<TNode> WithoutParents (Predicate<TNode> match){
			return Roots(Direction.Down, match);
		}

		public ICollection<TNode> WithoutChildren(Predicate<TNode> match) {
			return Roots(Direction.Up, match);
		}



		public static Direction ReverseDirection(Direction direction) {
			return direction == Direction.Up ? Direction.Down : Direction.Up;
		}



		public ICollection<TNode> Roots(Direction direction, Predicate<TNode> match) {
			if (match == null) {
				return GetRORoots(direction);
			} else {
				LinkedList<TNode> output = new LinkedList<TNode>();
				FindLayer(GetRoots(direction), output, match, direction);
				return output;
			}

		}



		internal static void FindLayer(IEnumerable<TNode> collection, LinkedList<TNode> output, Predicate<TNode> match, Direction direction) {

			foreach (TNode n in collection) {
				bool isMatch = (match == null || match(n));
				bool addToOutput = true;

				LinkedListNode<TNode> n2 = output.First;
				LinkedListNode<TNode> n3;
				while (n2 != null) {
					n3 = n2.Next;
					if (isMatch && n2.Value[ReverseDirection(direction)].IsRelated(n)) {
						output.Remove(n2);
					}
					if (n == n2.Value || n2.Value[direction].IsRelated(n)) {
						addToOutput = false;
						break;
					}
					n2 = n3;
				}

				if (addToOutput) {
					if (isMatch) {
						output.AddLast(n);
					} else {
						FindLayer(n[direction].Edges, output, match, direction);
					}
				}
			}
		}



		public ICollection<TNode> FilteredNodes(Predicate<TNode> match) {
			List<TNode> output = new List<TNode>();
			foreach (TNode n in _nodes) {
				if (match == null || match(n))
					output.Add(n);
			}
			return output;
		}


		public void Invalidate() {
			_valid = false;
			InvalidateRoots();
		}
		private void InvalidateRoots() {
			_roots[0] = null;
			_roots[1] = null;
			_roRoots[0] = null;
			_roRoots[1] = null;
		}
		private bool _valid = false;
		public bool Valid {
			get { return _valid; }
		}

		public void Calculate() {
			_valid = true;
			foreach (TNode node in _nodes) {
				node.Invalidate();
			}
		}




		public void SetColor(GraphColor color) {
			foreach (TNode node in _nodes) {
				node.Color = color;
			}
		}



		public TNode Sort() {
			SetColor(GraphColor.White);
			TNode[] nodes = new TNode[_nodes.Count];
			_nodes.CopyTo(nodes);
			_nodes.Clear();
			_nodeInCycle = null;

			foreach (TNode node in nodes) {
				if (node.Color == GraphColor.White)
					SortNode(node);
			}

			return _nodeInCycle;
		}

		private TNode _nodeInCycle;

		private void SortNode(TNode node) {
			node.Color = GraphColor.Gray;
			foreach (TNode n in node[Direction.Up].Edges) {
				if (n.Color == GraphColor.Gray) {
					_nodeInCycle = n;
				} else if (n.Color == GraphColor.White) {
					SortNode(n);
				}
			}
			node.Color = GraphColor.Black;
			_nodes.Add(node);
		}



		#region IEnumerable<TNode> Members

		public IEnumerator<TNode> GetEnumerator() {
			return _nodes.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return _nodes.GetEnumerator();
		}

		#endregion
	}




	public enum Direction
	{
		Down,
		Up,
	}

}
