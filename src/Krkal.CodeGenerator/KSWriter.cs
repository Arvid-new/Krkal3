//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - K S W r i t e r
///
///		Writes output as C++ source files - Script.cpp and Script.h
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.FileSystem;
using Krkal.Compiler;
using System.IO;

namespace Krkal.CodeGenerator
{
	internal class KSWriter
	{
		FS _fs;
		String _cppSourcePath;
		List<Method> _methods = new List<Method>();
		List<ObjectInitialization> _initializations = new List<ObjectInitialization>();
		KSNames _ksNames = new KSNames();
		internal KSNames KSNames {
			get { return _ksNames; }
		}

		// CONSTRUCTOR

		public KSWriter(String CppSourcePath) {
			_fs = FS.FileSystem;
			_cppSourcePath = CppSourcePath;
		}



		public void GenerateMethod(MethodAnalysis methodAnalysis) {
			Method method;
			if (CompilerConstants.IsNameTypeDirectMethod(methodAnalysis.MethodField.Name.NameType)) {
				method = new DirectMethod(this, methodAnalysis);
			} else {
				method = new SafeMethod(this, methodAnalysis);
			}
			_methods.Add(method);
		}





		public void GenerateKSFiles(Compilation compilation) {
			try {
				using(TextWriter cppFile = _fs.OpenFileForWriting(_cppSourcePath + @"\Script.cpp")) {

					cppFile.Write(@"
////////////////////////////////////////////////////////////////////////////////////////////////////
//
//			Krkal Compiled Scripts
//
//			Automatically generated file - do not modify
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include ""stdafx.h""
#include ""ScriptCppIncludes.h""



KsInterface *CreateKs() {
	return new CScript();
}



");

					foreach (Method method in _methods) {
						method.GenerateCode(cppFile);
					}

					foreach (KsidName name in compilation.KsidNames) {
						ClassName className = name as ClassName;
						if (className != null)
							WriteObjectInitialization(className, cppFile);
					}


					cppFile.WriteLine("void CScript::InitializeMethodPointers() {");

					foreach (Method method in _methods) {
						cppFile.Write("\tmethodPointers[\"");
						cppFile.Write(method.Name);
						cppFile.Write("\"] = (void (*)())");
						cppFile.Write(method.Name);
						cppFile.WriteLine(';');
					}

					cppFile.Write("}\r\n\r\n\r\n\r\n");


					cppFile.WriteLine("void CScript::InitInitializations() {");

					foreach (ObjectInitialization method in _initializations) {
						cppFile.Write("\tinitializations[");
						cppFile.Write(_ksNames.GetKsidName(method.ClassName));
						cppFile.Write("] = (void (*)())");
						cppFile.Write(method.Name);
						cppFile.WriteLine(';');
					}

					cppFile.Write("}\r\n\r\n\r\n\r\n");


					_ksNames.WriteGetKsParams(cppFile);

				}

				compilation.OutputMessage(" -> '" + _cppSourcePath + @"\Script.cpp" + "'.\n");

			} catch (FSFileNotFoundException) {
				compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, _cppSourcePath + @"\Script.cpp");
			}




			try {
				using (TextWriter hFile = _fs.OpenFileForWriting(_cppSourcePath + @"\Script.h")) {

					hFile.Write(@"
////////////////////////////////////////////////////////////////////////////////////////////////////
//
//			Krkal Compiled Scripts
//
//			Automatically generated file - do not modify
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once

#ifndef __SCRIPT_H__
#define __SCRIPT_H__

#include ""ScriptHIncludes.h""



class CScript : public CKrkalKS
{
#include ""CScriptAddOns.h""
public:
	CScript() {
		InitializeMethodPointers();
	}


");

					_ksNames.WriteDeclarations(hFile);

					hFile.Write(@"



private:
	void InitializeMethodPointers();
protected:
	virtual void GetKsParams(CQueryKsParameters *query);
	virtual void InitInitializations();
};




#endif

");
			

				}

				compilation.OutputMessage(" -> '" + _cppSourcePath + @"\Script.h" + "'.\n");

			}
			catch (FSFileNotFoundException) {
				compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, _cppSourcePath + @"\Script.h");
			}

		
		}




		private void WriteObjectInitialization(ClassName className, TextWriter cppFile) {
			bool hasInitializations = false;

			foreach (UniqueField field in className.UniqueNames.Values) {
				if (field.Assignment != null)
					hasInitializations = true;
			}

			if (hasInitializations) {
				ObjectInitialization oi = new ObjectInitialization(this, className);
				_initializations.Add(oi);
				oi.GenerateCode(cppFile);
			}
		}
	}
}
