namespace Krkal.RunIde
{
	partial class KerConsoleForm
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
			// KerConsoleForm
			// 
			this.ClientSize = new System.Drawing.Size(434, 324);
			this.HideOnClose = true;
			this.Name = "KerConsoleForm";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
			this.ShowInTaskbar = false;
			this.TabText = "Console";
			this.Text = "Console";
			this.ResumeLayout(false);

		}

		#endregion

	}
}