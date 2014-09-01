namespace Krkal.Ide
{
	partial class KrkalCodeDocument
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KrkalCodeDocument));
			this.syntaxDocument1 = new Puzzle.SourceCode.SyntaxDocument(this.components);
			this.syntaxBoxControl1 = new Puzzle.Windows.Forms.SyntaxBoxControl();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.insertIncludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// syntaxDocument1
			// 
			this.syntaxDocument1.Lines = new string[] {
        ""};
			this.syntaxDocument1.MaxUndoBufferSize = 1000;
			this.syntaxDocument1.Modified = false;
			this.syntaxDocument1.UndoStep = 0;
			this.syntaxDocument1.Change += new System.EventHandler(this.syntaxDocument1_Change);
			this.syntaxDocument1.ModifiedChanged += new System.EventHandler(this.syntaxDocument1_ModifiedChanged);
			// 
			// syntaxBoxControl1
			// 
			this.syntaxBoxControl1.ActiveView = Puzzle.Windows.Forms.ActiveView.BottomRight;
			this.syntaxBoxControl1.AutoListPosition = null;
			this.syntaxBoxControl1.AutoListSelectedText = "a123";
			this.syntaxBoxControl1.AutoListVisible = false;
			this.syntaxBoxControl1.BackColor = System.Drawing.Color.White;
			this.syntaxBoxControl1.BorderStyle = Puzzle.Windows.Forms.BorderStyle.None;
			this.syntaxBoxControl1.BracketBackColor = System.Drawing.Color.LightBlue;
			this.syntaxBoxControl1.BracketBorderColor = System.Drawing.Color.Transparent;
			this.syntaxBoxControl1.ContextMenuStrip = this.contextMenuStrip1;
			this.syntaxBoxControl1.CopyAsRTF = false;
			this.syntaxBoxControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.syntaxBoxControl1.Document = this.syntaxDocument1;
			this.syntaxBoxControl1.FontName = "Courier new";
			this.syntaxBoxControl1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.syntaxBoxControl1.InfoTipCount = 1;
			this.syntaxBoxControl1.InfoTipPosition = null;
			this.syntaxBoxControl1.InfoTipSelectedIndex = 1;
			this.syntaxBoxControl1.InfoTipVisible = false;
			this.syntaxBoxControl1.Location = new System.Drawing.Point(0, 0);
			this.syntaxBoxControl1.LockCursorUpdate = false;
			this.syntaxBoxControl1.Name = "syntaxBoxControl1";
			this.syntaxBoxControl1.ShowScopeIndicator = false;
			this.syntaxBoxControl1.Size = new System.Drawing.Size(441, 367);
			this.syntaxBoxControl1.SmoothScroll = false;
			this.syntaxBoxControl1.SplitviewH = -4;
			this.syntaxBoxControl1.SplitviewV = -4;
			this.syntaxBoxControl1.TabGuideColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(243)))), ((int)(((byte)(234)))));
			this.syntaxBoxControl1.TabIndex = 0;
			this.syntaxBoxControl1.Text = "syntaxBoxControl1";
			this.syntaxBoxControl1.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertIncludeToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(168, 26);
			// 
			// insertIncludeToolStripMenuItem
			// 
			this.insertIncludeToolStripMenuItem.Name = "insertIncludeToolStripMenuItem";
			this.insertIncludeToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.insertIncludeToolStripMenuItem.Text = "Insert &Include ...";
			this.insertIncludeToolStripMenuItem.Click += new System.EventHandler(this.insertIncludeToolStripMenuItem_Click);
			// 
			// KrkalCodeDocument
			// 
			this.ClientSize = new System.Drawing.Size(441, 367);
			this.Controls.Add(this.syntaxBoxControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "KrkalCodeDocument";
			this.TabText = "New Document";
			this.Text = "New Document";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KrkalCodeDocument_FormClosed);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Puzzle.SourceCode.SyntaxDocument syntaxDocument1;
		private Puzzle.Windows.Forms.SyntaxBoxControl syntaxBoxControl1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem insertIncludeToolStripMenuItem;
	}
}
