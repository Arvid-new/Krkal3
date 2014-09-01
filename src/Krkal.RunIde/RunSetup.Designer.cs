namespace Krkal.RunIde
{
	partial class RunSetup
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.primarySorceLabel1 = new System.Windows.Forms.Label();
			this.primarySourceTextBox = new System.Windows.Forms.TextBox();
			this.engineModeCombo = new System.Windows.Forms.ComboBox();
			this.debugModeCombo = new System.Windows.Forms.ComboBox();
			this.kernelModeCombo = new System.Windows.Forms.ComboBox();
			this.engineModeLabel = new System.Windows.Forms.Label();
			this.kernelModeLabel = new System.Windows.Forms.Label();
			this.debugModeLabel = new System.Windows.Forms.Label();
			this.primarySourceBrowse = new System.Windows.Forms.Button();
			this.saveWithSolution = new System.Windows.Forms.RadioButton();
			this.saveGlobally = new System.Windows.Forms.RadioButton();
			this.loadButton = new System.Windows.Forms.Button();
			this.okRunButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(102, 242);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(82, 23);
			this.okButton.TabIndex = 12;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(284, 242);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(82, 23);
			this.cancelButton.TabIndex = 13;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// primarySorceLabel1
			// 
			this.primarySorceLabel1.AutoSize = true;
			this.primarySorceLabel1.Location = new System.Drawing.Point(12, 15);
			this.primarySorceLabel1.Name = "primarySorceLabel1";
			this.primarySorceLabel1.Size = new System.Drawing.Size(213, 13);
			this.primarySorceLabel1.TabIndex = 0;
			this.primarySorceLabel1.Text = "Solution / Level / Code File / Saved Game:";
			// 
			// primarySourceTextBox
			// 
			this.primarySourceTextBox.Location = new System.Drawing.Point(15, 31);
			this.primarySourceTextBox.Name = "primarySourceTextBox";
			this.primarySourceTextBox.Size = new System.Drawing.Size(265, 20);
			this.primarySourceTextBox.TabIndex = 1;
			// 
			// engineModeCombo
			// 
			this.engineModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.engineModeCombo.FormattingEnabled = true;
			this.engineModeCombo.Location = new System.Drawing.Point(144, 74);
			this.engineModeCombo.Name = "engineModeCombo";
			this.engineModeCombo.Size = new System.Drawing.Size(189, 21);
			this.engineModeCombo.TabIndex = 4;
			// 
			// debugModeCombo
			// 
			this.debugModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.debugModeCombo.FormattingEnabled = true;
			this.debugModeCombo.Items.AddRange(new object[] {
            "Release",
            "Debug"});
			this.debugModeCombo.Location = new System.Drawing.Point(144, 128);
			this.debugModeCombo.Name = "debugModeCombo";
			this.debugModeCombo.Size = new System.Drawing.Size(189, 21);
			this.debugModeCombo.TabIndex = 8;
			// 
			// kernelModeCombo
			// 
			this.kernelModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.kernelModeCombo.FormattingEnabled = true;
			this.kernelModeCombo.Items.AddRange(new object[] {
            "Normal",
            "Editor",
            "CreateData",
            "DataEdit"});
			this.kernelModeCombo.Location = new System.Drawing.Point(144, 101);
			this.kernelModeCombo.Name = "kernelModeCombo";
			this.kernelModeCombo.Size = new System.Drawing.Size(189, 21);
			this.kernelModeCombo.TabIndex = 6;
			// 
			// engineModeLabel
			// 
			this.engineModeLabel.AutoSize = true;
			this.engineModeLabel.Location = new System.Drawing.Point(12, 77);
			this.engineModeLabel.Name = "engineModeLabel";
			this.engineModeLabel.Size = new System.Drawing.Size(73, 13);
			this.engineModeLabel.TabIndex = 3;
			this.engineModeLabel.Text = "Engine Mode:";
			// 
			// kernelModeLabel
			// 
			this.kernelModeLabel.AutoSize = true;
			this.kernelModeLabel.Location = new System.Drawing.Point(12, 104);
			this.kernelModeLabel.Name = "kernelModeLabel";
			this.kernelModeLabel.Size = new System.Drawing.Size(70, 13);
			this.kernelModeLabel.TabIndex = 5;
			this.kernelModeLabel.Text = "Kernel Mode:";
			// 
			// debugModeLabel
			// 
			this.debugModeLabel.AutoSize = true;
			this.debugModeLabel.Location = new System.Drawing.Point(12, 131);
			this.debugModeLabel.Name = "debugModeLabel";
			this.debugModeLabel.Size = new System.Drawing.Size(72, 13);
			this.debugModeLabel.TabIndex = 7;
			this.debugModeLabel.Text = "Debug Mode:";
			// 
			// primarySourceBrowse
			// 
			this.primarySourceBrowse.Location = new System.Drawing.Point(291, 29);
			this.primarySourceBrowse.Name = "primarySourceBrowse";
			this.primarySourceBrowse.Size = new System.Drawing.Size(75, 23);
			this.primarySourceBrowse.TabIndex = 2;
			this.primarySourceBrowse.Text = "Browse...";
			this.primarySourceBrowse.UseVisualStyleBackColor = true;
			this.primarySourceBrowse.Click += new System.EventHandler(this.primarySourceBrowse_Click);
			// 
			// saveWithSolution
			// 
			this.saveWithSolution.AutoSize = true;
			this.saveWithSolution.Checked = true;
			this.saveWithSolution.Location = new System.Drawing.Point(15, 177);
			this.saveWithSolution.Name = "saveWithSolution";
			this.saveWithSolution.Size = new System.Drawing.Size(175, 17);
			this.saveWithSolution.TabIndex = 9;
			this.saveWithSolution.TabStop = true;
			this.saveWithSolution.Text = "Save configuration with solution";
			this.saveWithSolution.UseVisualStyleBackColor = true;
			this.saveWithSolution.CheckedChanged += new System.EventHandler(this.saveWithSolution_CheckedChanged);
			// 
			// saveGlobally
			// 
			this.saveGlobally.AutoSize = true;
			this.saveGlobally.Location = new System.Drawing.Point(15, 200);
			this.saveGlobally.Name = "saveGlobally";
			this.saveGlobally.Size = new System.Drawing.Size(152, 17);
			this.saveGlobally.TabIndex = 10;
			this.saveGlobally.TabStop = true;
			this.saveGlobally.Text = "Save configuration globally";
			this.saveGlobally.UseVisualStyleBackColor = true;
			this.saveGlobally.CheckedChanged += new System.EventHandler(this.saveGlobally_CheckedChanged);
			// 
			// loadButton
			// 
			this.loadButton.Location = new System.Drawing.Point(12, 242);
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(82, 23);
			this.loadButton.TabIndex = 11;
			this.loadButton.Text = "Load...";
			this.loadButton.UseVisualStyleBackColor = true;
			this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
			// 
			// okRunButton
			// 
			this.okRunButton.Location = new System.Drawing.Point(192, 242);
			this.okRunButton.Name = "okRunButton";
			this.okRunButton.Size = new System.Drawing.Size(82, 23);
			this.okRunButton.TabIndex = 14;
			this.okRunButton.Text = "OK && Run";
			this.okRunButton.UseVisualStyleBackColor = true;
			this.okRunButton.Click += new System.EventHandler(this.okRunButton_Click);
			// 
			// RunSetup
			// 
			this.AcceptButton = this.okRunButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(378, 277);
			this.Controls.Add(this.okRunButton);
			this.Controls.Add(this.loadButton);
			this.Controls.Add(this.saveGlobally);
			this.Controls.Add(this.saveWithSolution);
			this.Controls.Add(this.primarySourceBrowse);
			this.Controls.Add(this.debugModeLabel);
			this.Controls.Add(this.kernelModeLabel);
			this.Controls.Add(this.engineModeLabel);
			this.Controls.Add(this.kernelModeCombo);
			this.Controls.Add(this.debugModeCombo);
			this.Controls.Add(this.engineModeCombo);
			this.Controls.Add(this.primarySourceTextBox);
			this.Controls.Add(this.primarySorceLabel1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RunSetup";
			this.ShowInTaskbar = false;
			this.Text = "Startup Configuration";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label primarySorceLabel1;
		private System.Windows.Forms.TextBox primarySourceTextBox;
		private System.Windows.Forms.ComboBox engineModeCombo;
		private System.Windows.Forms.ComboBox debugModeCombo;
		private System.Windows.Forms.ComboBox kernelModeCombo;
		private System.Windows.Forms.Label engineModeLabel;
		private System.Windows.Forms.Label kernelModeLabel;
		private System.Windows.Forms.Label debugModeLabel;
		private System.Windows.Forms.Button primarySourceBrowse;
		private System.Windows.Forms.RadioButton saveWithSolution;
		private System.Windows.Forms.RadioButton saveGlobally;
		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.Button okRunButton;
	}
}