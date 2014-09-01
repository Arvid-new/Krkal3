using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Compiler;
using Krkal.FileSystem;

namespace Krkal.Ide
{
	public partial class SolutionExplorer : WeifenLuo.WinFormsUI.Docking.DockContent
	{

		private MainForm _myForm;

		// CONSTRUCTOR
		public SolutionExplorer(MainForm myForm) {
			InitializeComponent();
			_myForm = myForm;
		}



		public void ClearSyntax() {
			treeView1.Nodes.Clear();
		}


		public void UpdateSyntax(IList<Compilation> compilations) {

			treeView1.BeginUpdate();

			ClearSyntax();

			if (compilations != null) {
				int f = 0;
				foreach (Compilation compilation in compilations) {
					if (compilation != null && compilation.SourceFiles != null) {

						KrkalPath rootKrkalPath = new KrkalPath(compilation.RootFile, KrkalCompiler.Compiler.CustomSyntax.FileExtensions);
						TreeNode root = new TreeNode(rootKrkalPath.ShortWithExtension, 0, 0);
						root.ToolTipText = compilation.RootFile;
						root.Name = compilation.RootFile;
						if (f == _myForm.Project.SelectedProject) {
							root.NodeFont = new Font(treeView1.Font, FontStyle.Bold);
						}
						root.Tag = f;

						foreach (SourceFile file in compilation.SourceFiles) {
							TreeNode node = new TreeNode(file.Lexical.KrkalPath.ShortWithExtension, 1, 1);
							node.ToolTipText = file.File;
							node.Name = file.File;
							root.Nodes.Add(node);

						}
						root.Expand();
						treeView1.Nodes.Add(root);

					}
					f++;
				}
				treeView1.Sort();
			}

			treeView1.EndUpdate();
		}



		private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
			if (treeView1.SelectedNode != null) {
				_myForm.OpenFile(treeView1.SelectedNode.Name);
			}
		}

		protected override string GetPersistString() {
			String filename = _myForm.Project.Solution == null ? "" : _myForm.Project.Solution;
			return GetType().ToString() + "," + filename + "," + _myForm.DoBackGroundCompilation.ToString();
		}

		int _currentRootIndex;
		private void toolStripMenuItem1_Click(object sender, EventArgs e) {
			_myForm.Project.SelectedProject = _currentRootIndex;
			UpdateSyntax(_myForm.Project.Compilations);
		}

		private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
			if (e.Node.Level == 0 && e.Button == MouseButtons.Right) {
				_currentRootIndex = (int)e.Node.Tag;
				contextMenuStrip1.Show(treeView1, e.Location);
			}
		}

	}
}

