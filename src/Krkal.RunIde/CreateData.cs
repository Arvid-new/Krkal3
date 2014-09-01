using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;

namespace Krkal.RunIde
{
	public partial class CreateData : EngineForm
	{

		IRuntimeStarter _runtimeStarter;
		KerConsole _console;


		// CONSTRUCTOR
		public CreateData(IRuntimeStarter runtimeStarter) {
			if (runtimeStarter == null)
				throw new ArgumentNullException("runtimeStarter");
			_runtimeStarter = runtimeStarter;
			InitializeComponent();

			InitilaizeConsole();

		}

		private void InitilaizeConsole() {
			_console = _runtimeStarter.TekeOverKerKonsole();
			_console.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			_console.Location = new System.Drawing.Point(0, 0);
			_console.Name = "_console";
			Size size = this.ClientSize;
			size.Height -= 65;
			_console.Size = size;
			_console.TabIndex = 3;
			this.Controls.Add(_console);
		}


		public KerMain KerMain {
			get { return _runtimeStarter.KerMain; }
		}





		private void CreateData_FormClosing(object sender, FormClosingEventArgs e) {
			_runtimeStarter.Dispose();
		}






		private void okButton_Click(object sender, EventArgs e) {
			this.Close();
		}






		public override void StartRunningTurns() {
			if (_runtimeStarter.EngineRunMode == (int)DefaultEngineRunModes.CreateData) {
				try {
					KerMain.RunTurn(1, false);
				}
				catch (KernelPanicException) {
					_runtimeStarter.Application.ShowErrorMessage("Error occured while creating data file.");
				}
			}
		}

	}





	public interface IEngineForm
	{
		void StartRunningTurns();
	}

	public class EngineForm : Form, IEngineForm
	{
		public virtual void StartRunningTurns() {
		}

	}

}
