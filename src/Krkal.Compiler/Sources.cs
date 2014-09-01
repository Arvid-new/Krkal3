//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - S o u r c e s
///
///		File nad file collection. Algorithm to search for files
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{
	public class SourceFile
	{
		private String _file;
		public String File {
			get { return _file; }
		}

		private String _version;
		public String Version {
			get { return _version; }
		}

		public String FileVersion {
			get { return _lexical.FileVersion; }
		}

		private Lexical _lexical;
		public Lexical Lexical {
			get { return _lexical; }
		}

		private ErrorLog _errorLog;
		public ErrorLog ErrorLog {
			get { return _errorLog; }
		}

		private NameTable _newNames;
		internal NameTable NewNames {
			get {
				if (_newNames == null)
					_newNames = new NameTable();
				return _newNames; 
			}
		}


		private List<SourceFile> _includedFiles = new List<SourceFile>();

		private GraphColor _color = GraphColor.White;

		private int _isBelowParent = 1;
		private bool IsBelowParent {
			get { return (_isBelowParent>0); }
		}
		private void BelowParentIncrement() {
			_isBelowParent++;
		}
		private void BelowParentDecrement() {
			_isBelowParent--;
		}

		private SourceFilesCollection _sourceFiles;
		public SourceFilesCollection SourceFiles {
			get { return _sourceFiles; }
		}


		// CONSTRUCTOR

		internal SourceFile(ErrorLog errorLog, String file, String version, SourceFilesCollection sourceFiles) 
		{
			_errorLog = errorLog;
			_file = file;
			_version = version;
			_sourceFiles = sourceFiles;
			if (version != null) // for non root files
				sourceFiles.AddFile(this);
		}


		// for fake files
		internal SourceFile(Lexical lexical) {
			_lexical = lexical;
		}


		internal void SearchIncludes(bool rebuildAll) {
			_color = GraphColor.Gray;

			try {
				_lexical = KrkalCompiler.Compiler.LexicalCache.Get(_file, rebuildAll);
				if (_lexical.FileVersion == null)
					_errorLog.ThrowAndLogFatalError(_file, ErrorCode.FFileWithoutVersion, _file);
			}
			catch (Krkal.FileSystem.FSFileNotFoundException) {
				ErrorLog.ThrowAndLogFatalError(_file, ErrorCode.FFileNotFound, _file);
			}

			_errorLog.RegisterSubordinateLog(_lexical.ErrorLog);

			_lexical.DoHeader();

			String version = _lexical.Header.Version.Text;
			if (_version != null) { // for non root files
				if (_version != version)
					_errorLog.ThrowAndLogFatalError(_file, ErrorCode.FCoreVersionMisMatch, _version, version);
			} else { // for root file
				_version = version;
				_sourceFiles.AddFile(this);
			}

			SourceFile tempSFile;
			String file;
			foreach (IncludeDescription includeDescription in _lexical.Header.Includes)
			{
				file = includeDescription.FileName.Text;
				version = includeDescription.Version.Text;
				if (_sourceFiles.TryGetFile(version, out tempSFile)) {
					if (file != tempSFile.File && !tempSFile.IsBelowParent)
						_errorLog.ThrowAndLogFatalError(includeDescription.FileName, ErrorCode.FConflictInModifications, file, tempSFile.File);
					tempSFile.BelowParentIncrement();
				} else {
					tempSFile = new SourceFile(_errorLog, file, version, _sourceFiles);
				}
				if (tempSFile._color == GraphColor.Gray)
					_errorLog.ThrowAndLogFatalError(includeDescription.FileName, ErrorCode.FIncludesAreInCycle);
				_includedFiles.Add(tempSFile);
			}

			foreach (SourceFile sFile in _includedFiles) {
				if (sFile._color == GraphColor.White)
					sFile.SearchIncludes(rebuildAll);
			}

			foreach (SourceFile sFile in _includedFiles) {
				sFile.BelowParentDecrement();
			}

			_sourceFiles.AddOrderedFile(this);
			_color = GraphColor.Black;
		}

	}




	public class SourceFilesCollection : IEnumerable<SourceFile>
	{
		List<SourceFile> _orderedFiles = new List<SourceFile>();
		Dictionary<String, SourceFile> _usedFiles = new Dictionary<string,SourceFile>(StringComparer.CurrentCultureIgnoreCase);
		SourceFile _rootFile;
		public SourceFile RootFile {
			get { return _rootFile; }
		}

		private ErrorLog _errorLog;
		public ErrorLog ErrorLog {
			get { return _errorLog; }
		}

		public int Count {
			get { return _orderedFiles.Count; }
		}

		// CONSTRUCTOR

		internal SourceFilesCollection(ErrorLog errorLog, String file, bool rebuildAll) 
		{
			_errorLog = errorLog;
			_rootFile = new SourceFile(_errorLog, file, null, this);
			_rootFile.SearchIncludes(rebuildAll);
		}

		internal bool TryGetFile(String version, out SourceFile sourceFile) {
			return _usedFiles.TryGetValue(version, out sourceFile);
		}

		internal void AddFile(SourceFile sourceFile) {
			_usedFiles.Add(sourceFile.Version, sourceFile);
		}


		internal void AddOrderedFile(SourceFile sourceFile) {
			_orderedFiles.Add(sourceFile);
		}


		public bool IsUnchanged(SourceFilesCollection compareTo) {
			if (_orderedFiles.Count != compareTo._orderedFiles.Count)
				return false;
			for (int f = 0; f < _orderedFiles.Count; f++ ) {
				if (_orderedFiles[f].Lexical != compareTo._orderedFiles[f].Lexical)
					return false;
			}
			return true;
		}



		#region IEnumerable<SourceFile> Members

		public IEnumerator<SourceFile> GetEnumerator() {
			return _orderedFiles.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _orderedFiles.GetEnumerator();
		}

		#endregion
	}
}
