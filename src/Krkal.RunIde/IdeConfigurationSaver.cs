using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Runtime;
using Krkal.FileSystem;
using Krkal.Compiler;
using System.Globalization;

namespace Krkal.RunIde
{

	[DataObjectClass("_KSID_IdeConfiguration")]
	class IdeConfigurationSaver
	{
		RunIdeForm _myForm;

		const string ConfigFile = "$PROFILE$\\Configuration\\IdeMainForm.data";

		
		IEnumerable<IEnumerable<Char>> _solutionNames;
		[DataObjectMapping("_KSID_IdeConfiguration__M_SolutionNames", BasicType.Char, 2)]
		public IEnumerable<IEnumerable<Char>> SolutionNames {
			get { return _solutionNames; }
			set { _solutionNames = value; }
		}

		IEnumerable<IEnumerable<char>> _solutionSelectedProjects;
		[DataObjectMapping("_KSID_IdeConfiguration__M_SolutionSelectedProjects", BasicType.Char, 2)]
		public IEnumerable<IEnumerable<char>> SolutionSelectedProjects {
			get { return _solutionSelectedProjects; }
			set { _solutionSelectedProjects = value; }
		}


		IEnumerable<KsidName> _solutionStartConfigurations;
		[DataObjectMapping("_KSID_IdeConfiguration__M_SolutionStartConfigurations", BasicType.Name, 1)]
		public IEnumerable<KsidName> SolutionStartConfigurations {
			get { return _solutionStartConfigurations; }
			set { _solutionStartConfigurations = value; }
		}


		KsidName _globalStartConfiguration;
		[DataObjectMapping("_KSID_IdeConfiguration__M_GlobalStartConfiguration", BasicType.Name, 0)]
		public KsidName GlobalStartConfiguration {
			get { return _globalStartConfiguration; }
			set { _globalStartConfiguration = value; }
		}


		// CONSTRUCTOR
		public IdeConfigurationSaver(RunIdeForm myForm) {
			_myForm = myForm;
		}

		public void Save() {
			IEnumerable<char>[] solutionNames = new IEnumerable<char>[_myForm.StartConfigurations.SolutionInfos.Count];
			IEnumerable<char>[] solutionSelectedProjects = new IEnumerable<char>[_myForm.StartConfigurations.SolutionInfos.Count];
			KsidName[] solutionStartConfigurations = new KsidName[_myForm.StartConfigurations.SolutionInfos.Count];

			_solutionNames = solutionNames; 
			_solutionSelectedProjects = solutionSelectedProjects; 
			_solutionStartConfigurations = solutionStartConfigurations;

			using (DataSource dataSource = new DataSource(ConfigFile, _myForm.Application.DataEnvironment)) {

				_globalStartConfiguration = SaveStartConfiguration(_myForm.StartConfigurations.GlobalConfig, dataSource, 0);

				int counter = 1;
				foreach (StartConfigurations.SolutionInfo info in _myForm.StartConfigurations.SolutionInfos) {
					solutionStartConfigurations[counter-1] = SaveStartConfiguration(info.StartConfiguration, dataSource, counter);
					solutionNames[counter-1] = info.SolutionName;
					solutionSelectedProjects[counter - 1] = info.SelectedProject;
					counter++;
				}

				dataSource.SaveObject(this, _myForm.Application.DataEnvironment.RunIdeKnownName(RunIdeKnownNames.IdeFormConfiguratuionData));

				dataSource.Save();
			}
		}



		private KsidName SaveStartConfiguration(StartConfiguration startConfiguration, DataSource dataSource, int counter) {
			if (startConfiguration != null) {
				Identifier name = Identifier.ParseKsid("_KSID_StartConfigurationObject" + counter.ToString(CultureInfo.InvariantCulture));
				KsidName ksid;
				if (!dataSource.Environment.Names.TryGetName(name, out ksid))
					ksid = dataSource.Environment.Names.CreateName(name, NameType.Void);
				dataSource.SaveObject(startConfiguration, ksid);
				return ksid;
			} else {
				return null;
			}
		}



		public void Load() {
			_globalStartConfiguration = null;
			_solutionNames = null;
			using (DataSource dataSource = new DataSource(ConfigFile, _myForm.Application.DataEnvironment)) {

				dataSource.LoadObject(this, _myForm.Application.DataEnvironment.RunIdeKnownName(RunIdeKnownNames.IdeFormConfiguratuionData));

				if (_globalStartConfiguration != null) {
					_myForm.StartConfigurations.GlobalConfig = new StartConfiguration(_globalStartConfiguration, dataSource);
				} else {
					_myForm.StartConfigurations.GlobalConfig = null;
				}

				if (_solutionNames != null && _solutionSelectedProjects != null && _solutionStartConfigurations != null) {
					List<StartConfigurations.SolutionInfo> infos = new List<StartConfigurations.SolutionInfo>();
					StringBuilder sb = new StringBuilder();

					var names = _solutionNames.GetEnumerator();
					var projects = _solutionSelectedProjects.GetEnumerator();
					var confs = _solutionStartConfigurations.GetEnumerator();

					while (names.MoveNext() && projects.MoveNext() && confs.MoveNext()) {
						if (names.Current == null)
							continue;

						StartConfiguration conf = null;
						if (confs.Current != null)
							conf = new StartConfiguration(confs.Current, dataSource);

						infos.Add(new StartConfigurations.SolutionInfo(ConvertToString(names.Current, sb), ConvertToString(projects.Current, sb), conf));
					}

					_myForm.StartConfigurations.SolutionInfos = infos;
				}

			}
		}



		private string ConvertToString(IEnumerable<char> chars, StringBuilder sb) {
			if (chars == null)
				return null;
			sb.Length = 0;
			foreach (char ch in chars) {
				sb.Append(ch);
			}
			return sb.ToString();
		}



		public static void DeleteConfigurationFile() {
			FS.FileSystem.Delete(ConfigFile);
		}
	}
}
