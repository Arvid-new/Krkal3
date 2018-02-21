namespace Krkal.RunIde
{
	partial class Builder
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Builder));
			this.output = new System.Windows.Forms.RichTextBox();
			this.binPath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.stepsToDo = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.configuration = new System.Windows.Forms.ComboBox();
			this.compileButton = new System.Windows.Forms.Button();
			this.runButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// output
			// 
			this.output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.output.BackColor = System.Drawing.SystemColors.Window;
			this.output.DetectUrls = false;
			this.output.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.output.Location = new System.Drawing.Point(0, 0);
			this.output.Name = "output";
			this.output.ReadOnly = true;
			this.output.Size = new System.Drawing.Size(576, 248);
			this.output.TabIndex = 0;
			this.output.Text = "";
			this.output.WordWrap = false;
			// 
			// binPath
			// 
			this.binPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.binPath.Location = new System.Drawing.Point(99, 280);
			this.binPath.Name = "binPath";
			this.binPath.Size = new System.Drawing.Size(326, 20);
			this.binPath.TabIndex = 1;
            this.binPath.Text = @"c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 283);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(86, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Path to MSBuild:";
			// 
			// stepsToDo
			// 
			this.stepsToDo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.stepsToDo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.stepsToDo.FormattingEnabled = true;
			this.stepsToDo.Items.AddRange(new object[] {
            "5 - Create Data File",
            "4 - Compile C++ Output",
            "3 - Generate Output",
            "2 - Compile Methods",
            "1 - Compile Types"});
			this.stepsToDo.Location = new System.Drawing.Point(99, 256);
			this.stepsToDo.Name = "stepsToDo";
			this.stepsToDo.Size = new System.Drawing.Size(121, 21);
			this.stepsToDo.TabIndex = 3;
			this.stepsToDo.SelectedIndexChanged += new System.EventHandler(this.stepsToDo_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 259);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Steps to do:";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(226, 259);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Configuration:";
			// 
			// configuration
			// 
			this.configuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.configuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.configuration.FormattingEnabled = true;
			this.configuration.Items.AddRange(new object[] {
            "Debug",
            "Release"});
			this.configuration.Location = new System.Drawing.Point(304, 256);
			this.configuration.Name = "configuration";
			this.configuration.Size = new System.Drawing.Size(121, 21);
			this.configuration.TabIndex = 6;
			// 
			// compileButton
			// 
			this.compileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.compileButton.Location = new System.Drawing.Point(490, 278);
			this.compileButton.Name = "compileButton";
			this.compileButton.Size = new System.Drawing.Size(75, 23);
			this.compileButton.TabIndex = 8;
			this.compileButton.Text = "&Compile";
			this.compileButton.UseVisualStyleBackColor = true;
			this.compileButton.Click += new System.EventHandler(this.compileButton_Click);
			// 
			// runButton
			// 
			this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.runButton.Location = new System.Drawing.Point(490, 254);
			this.runButton.Name = "runButton";
			this.runButton.Size = new System.Drawing.Size(75, 23);
			this.runButton.TabIndex = 9;
			this.runButton.Text = "&Run";
			this.runButton.UseVisualStyleBackColor = true;
			this.runButton.Click += new System.EventHandler(this.runButton_Click);
			// 
			// Builder
			// 
			this.ClientSize = new System.Drawing.Size(577, 306);
			this.Controls.Add(this.runButton);
			this.Controls.Add(this.compileButton);
			this.Controls.Add(this.configuration);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.stepsToDo);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.binPath);
			this.Controls.Add(this.output);
			this.HideOnClose = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(530, 100);
			this.Name = "Builder";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
			this.ShowInTaskbar = false;
			this.TabText = "Build";
			this.Text = "Build";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox output;
		private System.Windows.Forms.TextBox binPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox stepsToDo;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox configuration;
		private System.Windows.Forms.Button compileButton;
		private System.Windows.Forms.Button runButton;
	}
}
