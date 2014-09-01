using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Krkal.Compiler;
//using Microsoft.Build.BuildEngine;

namespace Krkal.Compiler.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
			//Engine engine = new Engine(@"c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727");

			//bool ret = engine.BuildProjectFile(@"c:\Test\Test\Test.vcproj");
			//Console.WriteLine(ret);
			//Console.WriteLine();



			KsidNames names = new KsidNames();
			KsidName name1 = names.CreateName(Identifier.Parse("@system"), NameType.Void);
			KsidName name2 = names.CreateName(Identifier.Parse("@system.bla"), NameType.Void);
			KsidName name3 = names.CreateName(Identifier.Parse("@system.user"), NameType.Void);
			KsidName name4 = names.CreateName(Identifier.Parse("krkal$1111_2222_3333_ffff"), NameType.Void);

			name1.Children.Add(name2);
			name1.Children.Add(name3);
			name4.Parents.Add(name3);

			name4.Parents.Add(name2);
			name4.Parents.Remove(name2);

			name1.RemoveAllEdges();


			//Krkal.FileSystem.FS.FileSystem.ChangeDir("$GAMES$");
			//String CurrentDir = null;
			//Krkal.FileSystem.FS.FileSystem.GetCurDir(ref CurrentDir);
			//Console.WriteLine(CurrentDir);

			Compilation compilation = new Compilation(@"test.kc", CompilationType.Semantical);

			Console.WriteLine(compilation.Compile());

			OutputErrors(compilation);

			Console.Write("press Enter ... ");
			Console.ReadLine();
		}

		private static void OutputErrors(Compilation compilation) {
			foreach (ErrorDescription description in compilation.ErrorLog.Errors) {
				Console.WriteLine("{0}: {1}: {2}, {3}, {4}", description.ErrorType, description.ErrorCode, description.File, description.Pos, description.Size);
				Console.WriteLine("     " + description.Message);
			}
		}
    }
}
