//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - M e t h o d
///
///		Represents a method
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Compiler;
using System.IO;

namespace Krkal.CodeGenerator
{
	
	internal abstract class Method
	{
		String _name;
		public String Name {
			get { return _name; }
		}

		MethodField _methodField;
		public MethodField MethodField {
			get { return _methodField; }
		}

		String _signature;
		public String Signature {
			get { return _signature; }
			protected set { _signature = value; }
		}

		String _content;
		public String Content {
			get { return _content; }
			protected set { _content = value; }
		}


		KSWriter _ksWriter;
		internal KSWriter KSWriter {
			get { return _ksWriter; }
		}


		// CONSTRUCTOR

		public Method(KSWriter ksWriter, MethodAnalysis methodAnalysis) {
			_ksWriter = ksWriter;
			_name = methodAnalysis.MethodField.ToKsfString();
			_methodField = methodAnalysis.MethodField;

			MethodWriter methodWriter = new MethodWriter(_ksWriter, _methodField);
			_content = methodWriter.GenerateContent(methodAnalysis);
		}

		public Method(KSWriter ksWriter, ClassName className) {
			_ksWriter = ksWriter;
			_name = "_KSOI" + _ksWriter.KSNames.GetKsidName(className);
		}


		public abstract void GenerateCode(TextWriter tw);


		protected void AppendMethodHead(TextWriter tw) {
			if (MethodField.ObjectContext != null) {
				tw.Write("// class: ");
				tw.WriteLine(MethodField.ObjectContext.ToString());
			}
			tw.Write("// method: ");
			tw.WriteLine(MethodField.Name.Identifier.ToString());
			tw.WriteLine(Signature);
		}




	}




	internal class SafeMethod : Method
	{

		// CONSTRUCTOR

		public SafeMethod(KSWriter ksWriter, MethodAnalysis methodAnalysis)
			: base(ksWriter, methodAnalysis) 
		{
			Signature = "void " + methodAnalysis.MethodField.ToKsfString() + "(CKerMain *KerMain) {";
		}


		public override void GenerateCode(TextWriter tw) {
			AppendMethodHead(tw);
			tw.WriteLine("\tCKerContext &ctx(*KerMain->KerContext);");
			tw.WriteLine("\tOPointer thisO = ctx.KCthis;");
			tw.WriteLine("\tCScript *Script = (CScript*)KerMain->KS;");
			tw.WriteLine(Content);
			tw.Write("}\r\n\r\n\r\n\r\n");
		}
	}






	internal class DirectMethod : Method
	{

		// CONSTRUCTOR

		public DirectMethod(KSWriter ksWriter, MethodAnalysis methodAnalysis) 
			: base(ksWriter, methodAnalysis)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Utils.ParseType(methodAnalysis.MethodField.Name.LanguageType));
			sb.Append(' ');
			sb.Append(methodAnalysis.MethodField.ToKsfString());
			sb.Append(Utils.WriteDirectMethodParameterList(methodAnalysis.MethodField.ParameterList));
			sb.Append(" {");

			Signature = sb.ToString();
		}


		public override void GenerateCode(TextWriter tw) {
			AppendMethodHead(tw);
			tw.WriteLine("\tCScript *Script = (CScript*)KerMain->KS;");
			tw.Write("\tCKerContext ctx(KerMain, Script->");
			tw.Write(KSWriter.KSNames.GetKsidName(MethodField.Name));
			tw.Write(", thisO, \"");
			tw.Write(MethodField.ToKsfString());
			tw.WriteLine("\");");

			tw.WriteLine(Content);
			tw.Write("}\r\n\r\n\r\n\r\n");
		}
	}



	internal class ObjectInitialization : Method
	{

		ClassName _className;
		public ClassName ClassName {
			get { return _className; }
		}

		// CONSTRUCTOR

		public ObjectInitialization(KSWriter ksWriter, ClassName className) 
			: base(ksWriter, className)
		{
			_className = className;

			Signature = "void " + Name + "(CKerMain *KerMain) {";

			MethodWriter methodWriter = new MethodWriter(ksWriter, className);
			Content = methodWriter.GenerateInitializationContent();
		}


		public override void GenerateCode(TextWriter tw) {
			tw.Write("// class: ");
			tw.WriteLine(_className.Identifier.ToString());
			tw.WriteLine("// Initialization");
			tw.WriteLine(Signature);
			tw.WriteLine("\tCKerContext &ctx(*KerMain->KerContext);");
			tw.WriteLine("\tOPointer thisO = ctx.KCthis;");
			tw.WriteLine("\tCScript *Script = (CScript*)KerMain->KS;");
			tw.WriteLine();
			tw.WriteLine(Content);
			tw.Write("}\r\n\r\n\r\n\r\n");
		}
	
	}
}
