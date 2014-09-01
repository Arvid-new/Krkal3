using System;
using System.Collections.Generic;
using System.Text;
using Krkal.RunIde;
using Krkal.Compiler;
using System.Windows.Forms;

namespace Krkal.Sample
{


	[Engine("_KSId_SampleEngine_4B7D_9CCA_1E1A_EDCB")]
	public class SampleEngine : DefaultEngine
	{
		protected override string GetKSSource() {
			return "Krkal.KS.Sample.2008";
		}

		public override EngineForm CreateGame(IRuntimeStarter runtimeStarter) {
			GameMainForm form = new GameMainForm(runtimeStarter);
			runtimeStarter.Application.ApplicationContext.RegisterAndShowForm(form);
			return form;
		}

	}
}
