using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Krkal.GEn
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (GenForm form = new GenForm())
				form.Run();
		}
	}



	// this exception will not be caught -> crash
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class InternalGEnException : Exception
	{
		public InternalGEnException()
			: base("Internal error in KRKAL Graphic Engine") { }
		public InternalGEnException(String message)
			: base(message) { }
		public InternalGEnException(String message, Exception innerException)
			: base(message, innerException) { }
	}

}
