
// !!!! In Order to run this form in Designer: Add the Krkal BIN directory to Windows Path

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;
using Aga.Controls.Tree;
using System.Globalization;

namespace Krkal.Ide
{



	public partial class RegisterDocument : KrkalDocument
	{

		RegisterModel _model;

		public RegisterDocument(MainForm myForm) : base(myForm) {
			InitializeComponent();
		}


		public override void OpenFile(String file) {
			base.OpenFile(file);
			_model = new RegisterModel(file, imageList1);
			treeView.Model = _model;
		}

		private void RegisterDocument_FormClosed(object sender, FormClosedEventArgs e) {
			if (_model != null) {
				_model.Dispose();
				_model = null;
			}
		}

		private void _reloadButton_Click(object sender, EventArgs e) {
			try {
				_model.Reload();
			}
			catch (FSFileNotFoundException ex) {
				MessageBox.Show(ex.Message, _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	
	}




	internal class RegisterNode
	{
		const int ValuesInLine = 8;

		FSRegKey _key;
		public FSRegKey Key {
			get { return _key; }
		}

		int _ordinal;
		public int Ordinal {
			get { return _ordinal; }
		}

		public String KeyName {
			get { return _key.Name;	}
		}

		bool _insideValue;
		public bool InsideValue {
			get { return _insideValue; }
		}

		// CONSTRUCTOR
		public RegisterNode(FSRegKey key, int ordinal, bool insideValue, ImageList imageList) {
			_key = key;
			_ordinal = ordinal;
			_insideValue = insideValue;
			_imageList = imageList;
		}

		ImageList _imageList;
		public Image Icon {
			get {
				if (_key.Type == FSRegKeyType.Register) 
					return _imageList.Images[0];
				return _imageList.Images[_insideValue ? 2 : 1];
			}
		}

		public String TypeCount {
			get {
				if (_insideValue)
					return String.Empty;
				int count = _key.Type == FSRegKeyType.Register ? _key.Subregister.CountOfKeys : _key.Top;
				return String.Format(CultureInfo.CurrentCulture, "{0}, {1}", _key.Type, count);
			}
		}

		String _keyValue;
		public String KeyValue {
			get {
				if (_keyValue == null) {
					if (_key.Type == FSRegKeyType.Register || _key.Top == 0) {
						_keyValue = String.Empty;
					} else if (_key.Type == FSRegKeyType.String || _key.Type == FSRegKeyType.WString) {
						_key.Pos = _insideValue ? _ordinal : 0;
						_keyValue = _key.StringRead();
					} else {
						StringBuilder sb = new StringBuilder();
						_key.Pos = _insideValue ? _ordinal : 0;
						for (int f = 0; f < (_insideValue ? 1 : ValuesInLine) && !_key.Eof; f++) {
							if (f > 0)
								sb.Append(' ');
							switch (_key.Type) {
								case FSRegKeyType.Double:
									sb.Append(_key.ReadD());
									break;
								case FSRegKeyType.Char: {
										char c = (char)_key.ReadC();
										if (Char.IsControl(c))
											c = '?';
										sb.Append(c);
										break;
									}
								case FSRegKeyType.Int:
									sb.Append(_key.ReadI());
									break;
								case FSRegKeyType.Int64:
									sb.Append(_key.Read64());
									break;
								case FSRegKeyType.WChar: {
										char c = _key.ReadW();
										if (Char.IsControl(c))
											c = '?';
										sb.Append(c);
										break;
									}
							}
						}
						_keyValue = sb.ToString();
					}
				}
				return _keyValue;
			}
		}



		String _hexaValue;
		public String HexaValue {
			get {
				if (_hexaValue == null) {
					if ((_key.Type != FSRegKeyType.Char && _key.Type != FSRegKeyType.Int && _key.Type != FSRegKeyType.Int64 && _key.Type != FSRegKeyType.WChar) 
						|| _key.Top == 0) {
						_hexaValue = String.Empty;
					} else {
						StringBuilder sb = new StringBuilder();
						_key.Pos = _insideValue ? _ordinal : 0;
						for (int f = 0; f < (_insideValue ? 1 : ValuesInLine) && !_key.Eof; f++) {
							if (f > 0)
								sb.Append(' ');
							switch (_key.Type) {
								case FSRegKeyType.Char: 
									sb.Append(_key.ReadC().ToString("X2", CultureInfo.InvariantCulture));
									break;
								case FSRegKeyType.Int:
									sb.Append(((uint)_key.ReadI()).ToString("X8", CultureInfo.InvariantCulture));
									break;
								case FSRegKeyType.Int64:
									sb.Append(((ulong)_key.Read64()).ToString("X16", CultureInfo.InvariantCulture));
									break;
								case FSRegKeyType.WChar:
									sb.Append(((ushort)_key.ReadW()).ToString("X4", CultureInfo.InvariantCulture));
									break;
							}
						}
						_hexaValue = sb.ToString();
					}
				}
				return _hexaValue;
			}

		}
	}



	internal class RegisterModel : ITreeModel, IDisposable
	{
		String _file;
		FSRegisterFile _registerFile;
		ImageList _imageList;


		public RegisterModel(String file, ImageList imageList) {
			_file = file;
			_imageList = imageList;
			Open();
		}

		void Open() {
			_registerFile = new FSRegisterFile(_file, null);
			if (_registerFile.OpenError != FSRegOpenError.OK) {
				_registerFile.Dispose();
				_registerFile = null;
				throw new FSFileNotFoundException("IO Error");
			}
		}


		public System.Collections.IEnumerable GetChildren(TreePath treePath) {
			if (_registerFile == null)
				yield break;

			FSRegister reg;
			if (treePath.IsEmpty()) {
				reg = _registerFile.Reg;
			} else {
				FSRegKey key = ((RegisterNode)treePath.LastNode).Key;
				if (key.Type == FSRegKeyType.Register) {
					reg = key.Subregister;
				} else {					
					key.Pos = 0;
					while (!key.Eof) {
						yield return new RegisterNode(key, key.Pos, true, _imageList);
						if (key.Type == FSRegKeyType.String || key.Type == FSRegKeyType.WString) {
							key.StringRead();
						} else {
							key.Pos = key.Pos + 1;
						}
					}
					yield break;
				}
			}

			if (!reg.IsNull) {
				int f = 0;
				for (FSRegKey key = reg.GetFirstKey(); !key.IsNull; key = key.NextKey, f++) {
					yield return new RegisterNode(key, f, false, _imageList);
				}
			}
		}

		public bool IsLeaf(TreePath treePath) {
			if (treePath.IsEmpty())
				return false;
			if (((RegisterNode)treePath.LastNode).InsideValue)
				return true;
			FSRegKey key = ((RegisterNode)treePath.LastNode).Key;
			if  (key.Type == FSRegKeyType.Register) {
				return key.Subregister.CountOfKeys == 0;
			} else {
				return key.Top < 2;
			}
		}

		public void Reload() {
			Close();
			Open();
			OnStructureChanged(new TreePathEventArgs());
		}


		#region Events

		public event EventHandler<TreeModelEventArgs> NodesChanged;
		protected void OnNodesChanged(TreeModelEventArgs args) {
			if (NodesChanged != null)
				NodesChanged(this, args);
		}

		public event EventHandler<TreePathEventArgs> StructureChanged;
		protected void OnStructureChanged(TreePathEventArgs args) {
			if (StructureChanged != null)
				StructureChanged(this, args);
		}

		public event EventHandler<TreeModelEventArgs> NodesInserted;
		protected void OnNodeInserted(TreeModelEventArgs args) {
			if (NodesInserted != null)
				NodesInserted(this, args);
		}

		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		protected void OnNodeRemoved(TreeModelEventArgs args) {
			if (NodesRemoved != null)
				NodesRemoved(this, args);
		}

		#endregion

		#region IDisposable Members
		public void Dispose() {
			Close();
		}

		private void Close() {
			if (_registerFile != null)
				_registerFile.Dispose();
			_registerFile = null;
		}
		#endregion

	}

}
