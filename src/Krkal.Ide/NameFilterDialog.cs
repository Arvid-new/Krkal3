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
	public partial class NameFilterDialog : Form
	{

		NameFilter _filterState;

		public NameFilterDialog(ImageList imageList, NameFilter filterState) {
			InitializeComponent();

			listView1.SmallImageList = imageList;
			_filterState = filterState;

			int f = 0;
			foreach (CustomKeywordInfo info in KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes.ROTypeInfos) {
				ListViewItem item = new ListViewItem(info.Name);
				if (f < imageList.Images.Count)
					item.ImageIndex = f;
				item.Checked = _filterState[f];
				item.Tag = f;
				listView1.Items.Add(item);
				f++;
			}

		}

		private void okButton_Click(object sender, EventArgs e) {
			for (int f=0; f < _filterState.Length; f++) {
				_filterState[f] = listView1.Items[f].Checked;
			}
		}

		private void selectAllButton_Click(object sender, EventArgs e) {
			foreach (ListViewItem item in listView1.Items) {
				item.Checked = true;
			}
		}

		private void selectNoneButton_Click(object sender, EventArgs e) {
			foreach (ListViewItem item in listView1.Items) {
				item.Checked = false;
			}
		}


		private void NameFilterDialog_FormClosed(object sender, FormClosedEventArgs e) {
			listView1.SelectedItems.Clear();
			for (int f = 0; f < _filterState.Length; f++) {
				listView1.Items[f].Checked = _filterState[f];
			}
		}


		private void listView1_Click(object sender, EventArgs e) {
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
			foreach (ListViewItem item in listView1.SelectedItems) {
				item.Checked = !item.Checked;
			}
		}



	}





	public class NameFilter : IDisposable{
		bool[] _checkBoxes;
		public IEnumerable<bool> CheckBoxes {
			get { return _checkBoxes; }
		}

		Set<NameType> _visibleIndexes = new Set<NameType>();
		Dictionary<NameType, int> _positions = new Dictionary<NameType, int>();

		ImageList _imageList;
		NameFilterDialog _dialog;

		// CONSTRUCTOR
		public NameFilter(bool showAll, ImageList imageList) {
			if (imageList == null)
				throw new ArgumentNullException("_imageList");
			_imageList = imageList;
			_checkBoxes = new bool[KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes.ROTypeInfos.Count];

			int f = 0;
			foreach (CustomKeywordInfo info in KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes.ROTypeInfos) {
				NameType nameType = (NameType)info.Index;
				if (!_positions.ContainsKey(nameType))
					_positions.Add(nameType, f);
				if (showAll) {
					_checkBoxes[f] = true;
					_visibleIndexes.Add(nameType);
				}
				f++;
			}
		}

		public bool this[int position] {
			get { return _checkBoxes[position]; }
			set { Change(position, value); }
		}

		public int Length {
			get { return _checkBoxes.Length; }
		}

		public void Change(int position, bool show) {
			if (show) {
				_checkBoxes[position] = true;
				_visibleIndexes.Add((NameType)KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes.ROTypeInfos[position].Index);
			} else {
				_checkBoxes[position] = false;
				_visibleIndexes.Remove((NameType)KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes.ROTypeInfos[position].Index);
			}
		}
		public void Change(NameType nameType, bool show) {
			int p;
			if (_positions.TryGetValue(nameType, out p))
				Change(p, show);
		}

		public bool FilterFunction(KsidName name) {
			return (_visibleIndexes.Contains(name.NameType));
		}


		public DialogResult ShowDialog() {
			if (_dialog == null)
				_dialog = new NameFilterDialog(_imageList, this);
			return _dialog.ShowDialog();
		}



		public void Dispose() {
			if (_dialog != null)
				_dialog.Dispose();
			_dialog = null;
		}

	}



}