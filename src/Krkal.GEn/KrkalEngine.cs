using System;
using System.Collections.Generic;
using System.Text;
using Krkal.RunIde;
using System.Windows.Forms;

namespace Krkal.GEn
{


	[Engine("_KSId_KrkalGen_4D2A_E2A4_86A1_0687")]
	public class SampleEngine : DefaultEngine
	{
		protected override string GetKSSource() {
			return "Krkal.KS.Default";
		}

		public override EngineForm CreateGame(IRuntimeStarter runtimeStarter) {
			GameMainForm form = new GameMainForm(runtimeStarter);
			runtimeStarter.Application.ApplicationContext.RegisterAndShowForm(form);
			return form;
		}

	}
}
