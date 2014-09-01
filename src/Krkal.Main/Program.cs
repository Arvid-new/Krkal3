using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Krkal.RunIde;

namespace Krkal.Main
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(String[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			using (KrkalApplication app = new KrkalApplication(KrkalAppBehavior.DefaultStartUp)) {
				do {
					app.InitializeEngines();
					app.InitializeCompiler();
					app.InitializeIde();
					StartAction startAction = app.LoadConfiguration(args);
					switch (startAction) {
						case StartAction.StartGame:
							app.StartGame(app.GameInfo);
							break;
						case StartAction.StartIde:
							app.StartIde();
							break;
						case StartAction.StartPluginsSetup:
							app.StartPluginsSetup();
							break;
						default:
							return;
					}
				} while (app.WantRestart);

			}

		}



	}
}
