using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Compiler;

namespace Krkal.Ide
{
	public partial class NameTreeControl : DoubleClickableTreeView
	{

		private Compilation _compilation;
		public Compilation Compilation {
			get { return _compilation; }
			set { _compilation = value; UpdateSyntax(); }
		}

		private bool _isTree = true;
		[Category("Name Tree")] 
		[Description("If tree or flat view is used")]
		[DefaultValue(true)]
		public bool IsTree {
			get { return _isTree; }
			set {
				if (_isTree != value) {
					_isTree = value;
					ResetSyntax();
				}
			}
		}

		private Predicate<KsidName> _filter;
		public Predicate<KsidName> Filter {
			get { return _filter; }
			set { _filter = value; UpdateSyntax(); }
		}

		private Direction _direction = Direction.Down;
		[Category("Name Tree")]
		[Description("Browse tree from Up to Down or in opposite direction.")]
		[DefaultValue(typeof(Direction), "Down")]
		public Direction Direction {
			get { return _direction; }
			set {
				if (_direction != value) {
					_direction = value;
					ResetSyntax();
				}
			}
		}


		private Identifier _startIdentifier;
		public Identifier StartIdentifier {
			get { return _startIdentifier; }
			set {
				if (_startIdentifier != value) {
					_startIdentifier = value;
					ResetSyntax();
				}
			}
		}



		// CONSTRUCTOR
		public NameTreeControl() {
			InitializeComponent();
		}


		public void UpdateSyntax() {
			if (_compilation == null) {
				ClearSyntax();
				return;
			}

			BeginUpdate();
			if (_compilation.KsidNames != null) {
				if (!UpdateFromName(_startIdentifier, Nodes, true)) {
					if (_isTree) {
						UpdateNodes(_compilation.KsidNames.Roots(_direction, _filter), Nodes, true);
					} else {
						UpdateNodes(_compilation.KsidNames.FilteredNodes(_filter), Nodes, true);
					}
				}
			}
			EndUpdate();
		}



		bool UpdateFromName(Identifier id, TreeNodeCollection nodes, bool isExpanded) {
			KsidName ksid;
			if (id != null && _compilation.KsidNames.TryGetName(id, out ksid)) {
				if (_isTree) {
					UpdateNodes(ksid[_direction].Layer(_filter), nodes, isExpanded);
				} else {
					UpdateNodes(ksid[_direction].FilteredTransitiveNodes(_filter), nodes, isExpanded);
				}
				return true;
			} else {
				return false;
			}
		}


		private void UpdateNodes(ICollection<KsidName> names, TreeNodeCollection nodes, bool isExpanded) {
			if (isExpanded) {
				MatchCollections(names, nodes);
				foreach (TreeNode node in nodes) {
					Identifier id = (Identifier)node.Tag;
					UpdateFromName(id, node.Nodes, node.IsExpanded);
				}
			} else {
				if (names.Count == 0 && nodes.Count != 0) {
					nodes.Clear();
				} else if (names.Count != 0 && nodes.Count == 0) {
					using (IEnumerator<KsidName> enumerator = names.GetEnumerator()) {
						enumerator.MoveNext();
						KsidName name = enumerator.Current;
						AddNode(nodes, name);
					}
				}
			}
		}

		private void AddNode(TreeNodeCollection nodes, KsidName name) {
			TreeNode node = new TreeNode(name.Identifier.ToString(true), (int)name.NameType, (int)name.NameType);
			node.Tag = name.Identifier;
			node.ToolTipText = name.ToString();
			nodes.Add(node);
		}

		private void UpdateNode(TreeNode node, KsidName name) {
			node.ImageIndex = node.SelectedImageIndex = (int)name.NameType;
		}


		private void MatchCollections(ICollection<KsidName> names, TreeNodeCollection nodes) {
			Dictionary<Identifier, TreeNode> dic = new Dictionary<Identifier, TreeNode>();
			foreach (TreeNode node in nodes) {
				dic.Add((Identifier)node.Tag, node);
			}

			foreach (KsidName name in names) {
				TreeNode node;
				if (dic.TryGetValue(name.Identifier, out node)) {
					UpdateNode(node, name);
					dic[name.Identifier] = null;
				} else {
					AddNode(nodes, name);
				}
			}

			foreach (TreeNode node in dic.Values) {
				if (node != null)
					node.Remove();
			}


		}




		protected override void OnAfterExpand(TreeViewEventArgs e) {
			base.OnAfterExpand(e);
			Identifier id = (Identifier)e.Node.Tag;
			BeginUpdate();
			UpdateFromName(id, e.Node.Nodes, true);
			EndUpdate();
		}



		public void ClearSyntax() {
			Nodes.Clear();
		}


		public void ResetSyntax() {
			ClearSyntax();
			if (_compilation != null)
				UpdateSyntax();
		}
	}
}
