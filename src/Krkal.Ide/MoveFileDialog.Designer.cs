namespace Krkal.Ide
{
	partial class MoveFileDialog
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
			this.destinationBox = new System.Windows.Forms.TextBox();
			this.sourceBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.newNameBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(444, 185);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(525, 185);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// destinationBox
			// 
			this.destinationBox.Location = new System.Drawing.Point(12, 82);
			this.destinationBox.Multiline = true;
			this.destinationBox.Name = "destinationBox";
			this.destinationBox.ReadOnly = true;
			this.destinationBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.destinationBox.Size = new System.Drawing.Size(588, 35);
			this.destinationBox.TabIndex = 13;
			this.destinationBox.TabStop = false;
			this.destinationBox.Text = "gfdjkjdfhgjkdfhjklghdjkhgjkdfhjkghdjkhgjksdfhgjkhsdfjkhlurhgulsrhgulishlerhseruil" +
				"hgulshiserulhlsugslgruhgulihgierhgilgushluhslruhslukgherkserklughsklghsklhgsklhg" +
				"kuhskleruhserklg";
			// 
			// sourceBox
			// 
			this.sourceBox.Location = new System.Drawing.Point(12, 23);
			this.sourceBox.Multiline = true;
			this.sourceBox.Name = "sourceBox";
			this.sourceBox.ReadOnly = true;
			this.sourceBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.sourceBox.Size = new System.Drawing.Size(588, 35);
			this.sourceBox.TabIndex = 12;
			this.sourceBox.TabStop = false;
			this.sourceBox.Text = "gfdjkjdfhgjkdfhjklghdjkhgjkdfhjkghdjkhgjksdfhgjkhsdfjkhlurhgulsrhgulishlerhseruil" +
				"hgulshiserulhlsugslgruhgulihgierhgilgushluhslruhslukgherkserklughsklghsklhgsklhg" +
				"kuhskleruhserklg";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label1.Location = new System.Drawing.Point(12, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(114, 13);
			this.label1.TabIndex = 10;
			this.label1.Text = "Move file or folder:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label2.Location = new System.Drawing.Point(12, 66);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 14;
			this.label2.Text = "to folder:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label4.Location = new System.Drawing.Point(12, 125);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(176, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "You may choose a new name:";
			// 
			// newNameBox
			// 
			this.newNameBox.Location = new System.Drawing.Point(12, 141);
			this.newNameBox.Name = "newNameBox";
			this.newNameBox.Size = new System.Drawing.Size(588, 20);
			this.newNameBox.TabIndex = 0;
			// 
			// MoveFileDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(612, 220);
			this.Controls.Add(this.newNameBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.destinationBox);
			this.Controls.Add(this.sourceBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MoveFileDialog";
			this.ShowInTaskbar = false;
			this.Text = "Move Files";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox destinationBox;
		private System.Windows.Forms.TextBox sourceBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox newNameBox;
	}
}