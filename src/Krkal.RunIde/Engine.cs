using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Compiler;
using Krkal.Runtime;
using System.Reflection;
using Krkal.CodeGenerator;
using System.Windows.Forms;

namespace Krkal.RunIde
{





	public enum DefaultEngineRunModes
	{
		None,
		AutoDetect,
		CreateData,
		Run,
	}


	public interface IEngine
	{
		void Initialize(KrkalApplication application);
		void InitialzeCompiler(Compilation compilation, CustomSyntax customSyntax);
		EngineForm CreateGame(IRuntimeStarter runtimeStarter);
	}






	public class EngineCollection
	{
		Dictionary<String, EngineInformation> _information = new Dictionary<string, EngineInformation>();
		Dictionary<String, IEngine> _engines = new Dictionary<string, IEngine>();
		KrkalApplication _application;

		Dictionary<Identifier, int> _startModes = new Dictionary<Identifier,int>();
		public IDictionary<Identifier, int> StartModes {
			get { return _startModes; }
		}

		public IEngine this[String name] {
			get {
				if (String.IsNullOrEmpty(name))
					return _engines["_KSID_DefaultEngine"];
				IEngine ret;
				if (_engines.TryGetValue(name, out ret))
					return ret;

				return LoadEngine(name);

			}
		}


		// CONSTRUCTOR
		internal EngineCollection(KrkalApplication application) {
			_application = application;
		}

		internal void Initialize() {
			AddEngine("_KSID_DefaultEngine", new DefaultEngine());
			_startModes.Add(Identifier.ParseKsid("_KSID_DefaultEngine__M_Run"), (int)DefaultEngineRunModes.Run);
			_startModes.Add(Identifier.ParseKsid("_KSID_DefaultEngine__M_CreateData"), (int)DefaultEngineRunModes.CreateData);
			_startModes.Add(Identifier.ParseKsid("_KSID_DefaultEngine__M_AutoDetect"), (int)DefaultEngineRunModes.AutoDetect);
			_startModes.Add(Identifier.ParseKsid("_KSID_DefaultEngine__M_None"), (int)DefaultEngineRunModes.None);
			LoadInformationsFromRootNames();
		}



		public void AddEngine(String name, IEngine engine) {
			_engines.Add(name, engine);
			engine.Initialize(_application);
		}
		public void AddEngineInformation(EngineInformation information) {
			String key = information.Name.Identifier.ToKsidString();
			if (_information.ContainsKey(key))
				return;
			_information.Add(key, information);

			if (information.StartModeNames != null && information.StartModeValues != null) {
				for (int f = 0; f < information.StartModeNames.Count && f < information.StartModeValues.Count; f++) {
					if (information.StartModeNames[f] != null && !_startModes.ContainsKey(information.StartModeNames[f].Identifier))
						_startModes.Add(information.StartModeNames[f].Identifier, information.StartModeValues[f]);
				}
			}
		}



		private void LoadInformationsFromRootNames() {
			using (RootNames rootNames = new RootNames(true)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), _application.DataEnvironment);

				IList<KsidName> engines = dataSource.GetNameLayerOrSet(_application.DataEnvironment.RunIdeKnownName(RunIdeKnownNames.AllEngines), KerNameType.Engine, false);
				foreach (KsidName engine in engines) {
					AddEngineInformation(new EngineInformation(engine, dataSource));
				}
			}
		}




		private IEngine LoadEngine(string name) {
			EngineInformation info;
			if (!_information.TryGetValue(name, out info))
				throw new EngineNotLoadedException("Engine of this name not registered " + name);

			Assembly assembly = null;
			try {
				assembly = Assembly.Load(info.Assembly);
			}
			catch (Exception ex){
				throw new EngineNotLoadedException("Failed to open the engine assembly" + info.Assembly, ex);
			}

			Type[] types = assembly.GetExportedTypes();
			foreach (Type type in types) {
				object[] attrs = type.GetCustomAttributes(typeof(EngineAttribute), false);
				foreach (EngineAttribute attr in attrs) {
					if (name == attr.Name) {
						ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
						if (constructor == null)
							throw new EngineNotLoadedException("Failed create the engine object" + name);
						try {
							IEngine engine = (IEngine)constructor.Invoke(null);
							AddEngine(name, engine);
							return engine;
						}
						catch (Exception ex) {
							throw new EngineNotLoadedException("Failed create the engine object" + name, ex);
						}
					}
				}
			}

			throw new EngineNotLoadedException("Engine not found" + name);
		}
	}























	[DataObjectClass("_KSID_EngineInformation")]
	public class EngineInformation
	{
		KsidName _name;
		public KsidName Name {
			get { return _name; }
		}

		String _assembly;
		[DataObjectMapping("_KSID_EngineInformation__M_Assembly", BasicType.Char, 1)]
		public String Assembly {
			get { return _assembly; }
			set { _assembly = value; }
		}

		IList<KsidName> _startModeNames;
		[DataObjectMapping("_KSID_EngineInformation__M_StartModeNames", BasicType.Name, 1)]
		public IList<KsidName> StartModeNames {
			get { return _startModeNames; }
			set { _startModeNames = value; }
		}

		IList<int> _startModeValues;
		[DataObjectMapping("_KSID_EngineInformation__M_StartModeValues", BasicType.Int, 1)]
		public IList<int> StartModeValues {
			get { return _startModeValues; }
			set { _startModeValues = value; }
		}

		public EngineInformation(KsidName name, DataSource dataSource) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (dataSource == null)
				throw new ArgumentNullException("dataSource");
			_name = name;
			dataSource.LoadObject(this, _name);
		}
	}

















	public class DefaultEngine : IEngine
	{
		KrkalApplication _application;
		public KrkalApplication Application {
			get { return _application; }
		}

		private AttributesProvider _attributeProvider;
		protected AttributesProvider AttributeProvider {
			get { return _attributeProvider; }
		}

		public virtual void Initialize(KrkalApplication application) {
			_application = application;
		}


		protected virtual String GetKSSource() { return "Krkal.KS.Default"; }

		public virtual void InitialzeCompiler(Compilation compilation, CustomSyntax customSyntax) {
			customSyntax.CreateCodeGeneratorDelegate = CreateDefaultCodeGenerator;
			if (_attributeProvider == null) {
				_attributeProvider = new AttributesProvider();
			}
			customSyntax.AttributeProvider = _attributeProvider;
			Generator.AddSystemMethods(customSyntax);
			Generator.AddKnownNames(customSyntax);
		}



		public virtual EngineForm CreateGame(IRuntimeStarter runtimeStarter) {
			EngineForm form;
			if (runtimeStarter.EngineRunMode == (int)DefaultEngineRunModes.CreateData || runtimeStarter.EngineRunMode == (int)DefaultEngineRunModes.None) {
				form = new CreateData(runtimeStarter);
			} else {
				form = new GameRunForm(runtimeStarter);
			}
			runtimeStarter.Application.ApplicationContext.RegisterAndShowForm(form);
			return form;
		}


		public ICodeGenerator CreateDefaultCodeGenerator(Compilation compilation) {
			RunIdeForm form = _application.ApplicationContext.FindRunIdeForm();
			BuilderInfo info = form.GetBuilderInfo();
			return new Generator(compilation, info.StepsToDo <= StepsToDo.CompileOutput, info.BinPath, info.Configuration, GetKSSource());
		}

	}














	public class EngineNotLoadedException : Exception
	{
		public EngineNotLoadedException()
			: base("Unable to load Engine.") { }
		public EngineNotLoadedException(String message)
			: base(message) { }
		public EngineNotLoadedException(String message, Exception innerException)
			: base(message, innerException) { }
	}









	[AttributeUsage(AttributeTargets.Class)]
	public class EngineAttribute : Attribute
	{
		String _name;
		public String Name
		{
		  get { return _name; }
		  set { _name = value; }
		}


		public EngineAttribute(String name) {
			if (name == null)
				throw new ArgumentNullException("name");
			_name = name;
		}
	
	}





}
