using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;
//using Krkal.Ide;

namespace Krkal.Sample
{
	public partial class Map : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		private GameMainForm _myForm;
		public const int cellX = 54;
		public const int cellY = 54;

		private DrawArea panel1;

		int _sx = 8;
		public int SX {
			get { return _sx; }
			set { 
				_sx = value;
				RefreshSize();
			}
		}


		int _sy = 6;
		public int SY {
			get { return _sy; }
			set { 
				_sy = value;
				RefreshSize();
			}
		}

		public Map(GameMainForm myForm) {
			_myForm = myForm;
			
			this.panel1 = new DrawArea(myForm);
			this.panel1.BackColor = System.Drawing.Color.Black;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(300, 300);
			panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);
			panel1.MouseMove += new MouseEventHandler(panel1_MouseMove);

			InitializeComponent();

			this.panel2.Controls.Add(this.panel1);
			RefreshSize();
		}

		void panel1_MouseMove(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right)
				panel1_MouseClick(sender, e);
		}

		void panel1_MouseClick(object sender, MouseEventArgs e) {
			int x = e.X / cellX;
			int y = e.Y / cellY;

			if (e.Button == MouseButtons.Left) {
				_myForm.SelectObjects(_myForm.Services.GetObjects(x, y));
			} else if (e.Button == MouseButtons.Right && !_myForm.ObjectList.SelectedName.IsNull) {
				if (_myForm.Services.PlaceIfNoCollision(_myForm.ObjectList.SelectedName, x, y)) {
					panel1.Invalidate(new Rectangle(x * cellX, y * cellY, cellX, cellY));
				}
			}
		}


		public void SetSize(int x, int y) {
			_sx = x;
			_sy = y;
			RefreshSize();
		}

		public void InvalidateArea(int x, int y, int dx, int dy) {
			if (dx == 0)
				return;
			panel1.Invalidate(new Rectangle(x * cellX, y * cellY, dx * cellX, dy * cellY));
		}



		private void RefreshSize() {
			panel1.Width = _sx * cellX;
			panel1.Height = _sy * cellY;
		}

		private const string HelpText = 
@"Use Left Mouse Button to display objects from a Map cell in the Object Browser.
If there is a selected object in the Object Browser, you can kill it by pressing the 'Delete' key.
If there is a selected object in Object List, you can place it in the Map with Right mouse button.

Note: Some programs doesn't use Map.
";

		private void helpButton_Click(object sender, EventArgs e) {
			MessageBox.Show(HelpText, "Krkal Map Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


	}


}

