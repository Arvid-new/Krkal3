
// !!!! In Order to run this form in Designer: Add the Krkal BIN directory to Windows Path


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;
using System.IO;
using Krkal.Compiler;
using Puzzle.SourceCode;

namespace Krkal.Ide
{
	public partial class KrkalCodeDocument : KrkalDocument
	{
		

		// CONSTRUCTOR

		public KrkalCodeDocument(MainForm myForm) : base(myForm) {
			InitializeComponent();
			syntaxDocument1.Parser.Init(_myForm.Project.PuzzleLanguage);
		}


		public override void OpenFile(String file) {
			base.OpenFile(file);
			syntaxDocument1.Text = _fs.CheckOut(file, FSCallback);
			UpdateSyntax();
		}

		public override void Save() {
			try {
				if (_file != null) {
					using (TextWriter writer = _fs.OpenFileForWriting(_file)) {
						writer.Write(syntaxDocument1.Text);
					}
					syntaxDocument1.Modified = false;
				}
			}
			catch (FSFileNotFoundException ex) {
				MessageBox.Show(ex.Message, _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		public override void SaveAs() {
			String dir = "$GAMES$";
			if (_file != null && 0 != _fs.GetFullPath(_file, ref dir, FSFullPathType.WindowsOriginalCase)) {
				dir = Path.GetDirectoryName(dir);
			}
			
			using (NewFileDialog newFD = new NewFileDialog(NewFileType.SaveAs, dir, null, _myForm)) {
				newFD.Extension = Path.GetExtension(_file);
				newFD.SaveAsContent = syntaxDocument1.Text;
				if (newFD.ShowDialog() == DialogResult.OK) {
					if (_file != null)
						_fs.CancelCheckOut(_file);
					_myForm.ChangeDocFileName(this, _file, newFD.KeyFileName);
					try {
						OpenFile(newFD.KeyFileName);
					}
					catch (FSFileNotFoundException ex) {
						MessageBox.Show(ex.Message, _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
						_file = null;
						Close();
					}
				}
			}
		}


		public override bool Modified() {
			return syntaxDocument1.Modified;
		}

		private String FSCallback2() {
			return syntaxDocument1.Text;
		}

		private String FSCallback() {
			if (InvokeRequired) {
				return (String)Invoke(new GetCurrentFileCallback(FSCallback2));
			} else {
				return FSCallback2();
			}
		}

		private void syntaxDocument1_Change(object sender, EventArgs e) {
			if (_file != null) {
				_fs.FileChanged(_file);
			}
		}

		private void syntaxDocument1_ModifiedChanged(object sender, EventArgs e) {
			this.TabText = this.TabText.TrimEnd(new char[] { '*' });
			if (syntaxDocument1.Modified) {
				this.TabText += '*';
			} 
		}



		private void KrkalCodeDocument_FormClosed(object sender, FormClosedEventArgs e) {
			if (_file != null) {
				_fs.CancelCheckOut(_file);
				_file = null;
			}
		}



		public void ClearSyntex() {
			foreach (FormatRange format in syntaxDocument1.FormatRanges) {
				format.WaveColor = Color.Empty;
				format.InfoTip = "";
			}
			syntaxDocument1.FormatRanges.Clear();
		}

		public void UpdateSyntax() {
			ClearSyntex();
			if (_myForm.Project.ErrorLog != null) {
				foreach (ErrorDescription error in _myForm.Project.ErrorLog.Errors) {
					if (error.PositionInLines != null && error.File == _file) {
						FormatRange format = new Puzzle.SourceCode.FormatRange(CreateTextRange(error.PositionInLines), Color.Red);
						format.InfoTip = error.Message;
						syntaxDocument1.FormatRanges.Add(format);
					}
				}
			}
		}

		private static TextRange CreateTextRange(PositionInLines position) {
			return new TextRange(position.FirstColumn, position.FirstRow, position.LastColumn, position.LastRow);
		}

		public void SelectText(PositionInLines position) {

			TextRange bounds = syntaxBoxControl1.Selection.Bounds;

			syntaxBoxControl1.ClearSelection();

			bounds.FirstColumn = position.FirstColumn;
			bounds.FirstRow = position.FirstRow;
			bounds.LastColumn = position.LastColumn + 1;
			bounds.LastRow = position.LastRow;

			syntaxBoxControl1.Caret.Position.X = bounds.FirstColumn;
			syntaxBoxControl1.Caret.Position.Y = bounds.FirstRow;
			syntaxBoxControl1.ScrollIntoView();

			Activate();
		}

		private void insertIncludeToolStripMenuItem_Click(object sender, EventArgs e) {
			using (FileBrowserDialog fileBrowser = new FileBrowserDialog(_myForm.CurrentDirectory, _myForm)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Select source file to include";
				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					try {
						String file = fileBrowser.SelectedItemKeyName;
						if (String.IsNullOrEmpty(file)) {
							MessageBox.Show("IO Error", _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
						} else {
							StringBuilder sb = new StringBuilder("include \"");
							sb.Append(file);
							sb.Append("\" ");
							String content = _fs.OpenFileForReading(file);
							Lexical lexical = new Lexical(file, content);
							lexical.DoHeader();
							sb.Append(lexical.Header.Version.Text);
							sb.Append(';');
							syntaxDocument1.InsertText(sb.ToString(), syntaxBoxControl1.Caret.Position.X, syntaxBoxControl1.Caret.Position.Y);
						}
					}
					catch (FSFileNotFoundException ex) {
						MessageBox.Show(ex.Message, _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					catch (CompilerException ex) {
						MessageBox.Show(ex.Message, _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				_myForm.CurrentDirectory = fileBrowser.Directory;
			}
		}




	}


}

