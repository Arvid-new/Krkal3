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
	class DrawArea : Panel
	{
		private GameMainForm _myForm;

		public DrawArea(GameMainForm myForm) {
			_myForm = myForm;
			this.SetStyle(ControlStyles.DoubleBuffer |
				 ControlStyles.UserPaint |
				 ControlStyles.AllPaintingInWmPaint,
				 true);
			this.UpdateStyles();
		}

		protected override void OnPaint(PaintEventArgs e) {
			int x = e.ClipRectangle.X / Map.cellX;
			int y = e.ClipRectangle.Y / Map.cellY;
			int dx = e.ClipRectangle.Width / Map.cellX + 2;
			int dy = e.ClipRectangle.Height / Map.cellY + 2;
			String[] arr = null;
			for (int g = y; g < y + dy; g++) {
				for (int f = x; f < x + dx; f++) {
					arr = _myForm.Services.GetBitmapNames(f, g);
					if (arr != null) {
						foreach (String name in arr) {
							int index = _myForm.MapGraphics.Images.IndexOfKey(name);
							if (index == -1)
								index = 0;
							_myForm.MapGraphics.Draw(e.Graphics, f * Map.cellX, g * Map.cellY, index);
						}
					}
				}
			}
		}

	}


}