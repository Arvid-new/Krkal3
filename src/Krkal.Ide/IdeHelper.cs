using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Compiler;
using System.Windows.Forms;

namespace Krkal.Ide
{

	public interface INewDocumentInformation
	{
		String Name {
			get;
		}
		String MenuName {
			get;
		}
		String Extension {
			get;
		}
		String Content {
			get;
		}
	}


	public interface IExpandedSolution
	{
		IList<String> Projects {
			get;
		}
		String SolutionUserName {
			get;
		}
		int SelectedProject {
			get;
		}
	}


	public interface IIdeHelper
	{
		String Caption {
			get;
		}
		String OkText {
			get;
		}
		String CancelText {
			get;
		}

		IKrkalResourceManager ResourceManager {
			get;
		}

		IExpandedSolution ExpandSolution(String solution);
		IList<INewDocumentInformation> GetNewDocumentInformation();

		void GenerateData(Compilation compilation);

		void ShowErrorMessage(string message);
	}





	class DefaultNewDocumentInformation : INewDocumentInformation
	{
		String _name;
		public string Name {
			get { return _name; }
		}

		String _menuName;
		public string MenuName {
			get { return _menuName; }
		}

		string _extension;
		public string Extension {
			get { return _extension; }
		}

		String _content;
		public string Content {
			get { return _content; }
		}


		// CONSTRUCTOR
		public DefaultNewDocumentInformation(String name, String menuName, String extension, String content) {
			_name = name; _menuName = menuName; _extension = extension; _content = content;
		}
	}




	public class EmptyResourceManager : IKrkalResourceManager
	{
		public string GetUserNameOrComment(string ksid, bool isComment) {
			return null;
		}

		public string GetText(string text) {
			if (text == null)
				return null;
			if (text.Length > 6 && text[0] == '_' && text[1] == 'K' && text[2] == 'S' && text[3] == 'I' && text[5] == '_') {
				int index = text.IndexOf(':');
				if (index == -1)
					return String.Empty;
				return text.Substring(index + 1);
			} else {
				return text;
			}
		}

		public string GetText(string ksid, string defaultText) {
			return defaultText;
		}

		public bool ReloadIfNeeded() {
			return true;
		}

	}




	public class ExpandedSolution : IExpandedSolution
	{
		IList<String> _projects;
		public IList<string> Projects {
			get { return _projects; }
		}

		String _solutionUserName;
		public string SolutionUserName {
			get { return _solutionUserName; }
		}

		int _selectedProject;
		public int SelectedProject {
			get { return _selectedProject; }
		}

		// CONSTRUCTOR
		public ExpandedSolution(IList<String> projects, String solutionUserName, int selectedProject) {
			_projects = projects;
			_solutionUserName = solutionUserName;
			_selectedProject = selectedProject;
		}


		public ExpandedSolution(IList<String> projects, String solutionUserName, String selectedProject) {
			_projects = projects;
			_solutionUserName = solutionUserName;
			if (_projects != null && selectedProject != null) {
				for (int f = 0; f < _projects.Count; f++) {
					if (String.CompareOrdinal(projects[f], selectedProject) == 0) {
						_selectedProject = f;
						break;
					}
				}
			}
		}
	}


	public class DefaultIdeHelper : IIdeHelper
	{

		public string Caption {
			get { return "Krkal"; }
		}

		public string OkText {
			get { return "OK"; }
		}

		public string CancelText {
			get { return "Cancel"; }
		}

		EmptyResourceManager _emptyResourceManager = new EmptyResourceManager();
		public IKrkalResourceManager ResourceManager {
			get { return _emptyResourceManager; }
		}

		public IExpandedSolution ExpandSolution(string solution) {
			List<String> ret = new List<string>(1);
			if (!String.IsNullOrEmpty(solution))
				ret.Add(solution);
			return new ExpandedSolution(ret, null, 0);
		}

		IList<INewDocumentInformation> _newDocumentInformation;
		public IList<INewDocumentInformation> GetNewDocumentInformation() {
			if (_newDocumentInformation == null) {
				_newDocumentInformation = new DefaultNewDocumentInformation[2];
				_newDocumentInformation[0] = CreateNewProjectFileInfo();
				_newDocumentInformation[1] = CreateNewSourceFileInfo();
			}
			return _newDocumentInformation;
		}


		public static INewDocumentInformation CreateNewProjectFileInfo() {
			return new DefaultNewDocumentInformation("New Project File", "New &Project File...", ".kcp",
@"#head {{
	version {0};
}}
#attributes []
#names {{
}}
");
		}


		public static INewDocumentInformation CreateNewSourceFileInfo() {
			return new DefaultNewDocumentInformation("New Source File", "New &Source File...", ".kc",
@"#head {{
	version {0};
	component;
}}
#attributes []
#names {{
}}
");
		}



		public void GenerateData(Compilation compilation) {
		}


		public void ShowErrorMessage(string message) {
			MessageBox.Show(ResourceManager.GetText(message), Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

	}
}
