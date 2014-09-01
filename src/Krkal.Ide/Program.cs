using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Krkal.Ide
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			_displayComponentWarning = true;
            Application.Run(new MainForm());
        }

		static bool _displayComponentWarning;

		internal static void DisplayComponentWarning() {
			if (_displayComponentWarning)
				MessageBox.Show("Warning: Krkal IDE was started as a stand-alone application. Only limited functionality is available. Start full application instead.", "Krkal IDE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
    }
}