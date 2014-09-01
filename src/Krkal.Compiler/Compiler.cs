//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C o m p i l e r
///
///		Main compiler singleton class. Holds caches and configuration
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.FileSystem;

namespace Krkal.Compiler
{
	public class KrkalCompiler
	{
		private IFS _fs;
		public IFS FS {
			get { return _fs; }
		}

		private LexicalCache _lexicalCache;
		public LexicalCache LexicalCache {
			get { return _lexicalCache; }
		}


		private StaticCustomSyntax _customSyntax;
		public StaticCustomSyntax CustomSyntax {
			get { return _customSyntax; }
		}


		private Dictionary<String, Compilation> _compilationCache = new Dictionary<string, Compilation>(StringComparer.CurrentCultureIgnoreCase);
		internal void CompilationCacheAdd(String project, Compilation compilation) {
			lock (_compilationCache) {
				_compilationCache.Remove(project);
				_compilationCache.Add(project, compilation);
			}
		}
		public Compilation CompilationCacheGet(String project) {
			lock (_compilationCache) {
				Compilation ret;
				_compilationCache.TryGetValue(project, out ret);
				return ret;
			}
		}

		private Dictionary<String, Compilation> _workingCompilationCache = new Dictionary<string, Compilation>(StringComparer.CurrentCultureIgnoreCase);
		internal void WorkingCompilationCacheAdd(String project, Compilation compilation) {
			lock (_compilationCache) {
				_workingCompilationCache.Remove(project);
				_workingCompilationCache.Add(project, compilation);
			}
		}
		public Compilation WorkingCompilationCacheGet(String project) {
			lock (_compilationCache) {
				Compilation ret;
				_workingCompilationCache.TryGetValue(project, out ret);
				return ret;
			}
		}



		public void Reset() {
			lock (_compilationCache) {
				_lexicalCache.Clear();
				_compilationCache.Clear();
				_workingCompilationCache.Clear();
				_customSyntax = new StaticCustomSyntax();
			}
		}

		
		private static KrkalCompiler _compiler;
		public static KrkalCompiler Compiler {
			get {
				if (_compiler == null) {
					_compiler = new KrkalCompiler();
					_compiler.Initialize();
				}
				return _compiler; 
			}
		}


		private KrkalCompiler() {
		}

		private void Initialize() {
			CompilerConstants.InitializeCompilerConstants();
			_fs = Krkal.FileSystem.FS.FileSystem;
			_lexicalCache = new LexicalCache();
			_customSyntax = new StaticCustomSyntax();
		}



	}

}
