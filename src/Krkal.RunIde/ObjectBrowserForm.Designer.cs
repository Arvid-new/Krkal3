namespace Krkal.RunIde
{
	partial class ObjectBrowserForm
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
			this.objectBrowser1 = new Krkal.RunIde.ObjectBrowser();
			this.SuspendLayout();
			// 
			// objectBrowser1
			// 
			this.objectBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.objectBrowser1.KerMain = null;
			this.objectBrowser1.Location = new System.Drawing.Point(0, 0);
			this.objectBrowser1.Name = "objectBrowser1";
			this.objectBrowser1.Obj = null;
			this.objectBrowser1.Size = new System.Drawing.Size(195, 408);
			this.objectBrowser1.TabIndex = 0;
			// 
			// ObjectBrowserForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(195, 408);
			this.Controls.Add(this.objectBrowser1);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "ObjectBrowserForm";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
			this.ShowInTaskbar = false;
			this.TabText = "Object Browser";
			this.Text = "Object Browser";
			this.ResumeLayout(false);

		}

		#endregion

		private ObjectBrowser objectBrowser1;
	}
}