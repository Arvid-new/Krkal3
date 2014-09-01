namespace Krkal.RunIde
{
	partial class RunIdeForm
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
			this.SuspendLayout();
			// 
			// dockPanel
			// 
			this.dockPanel.Location = new System.Drawing.Point(0, 49);
			this.dockPanel.Size = new System.Drawing.Size(601, 360);
			// 
			// SampleForm
			// 
			this.ClientSize = new System.Drawing.Size(601, 431);
			this.Name = "SampleForm";
			this.Controls.SetChildIndex(this.dockPanel, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
