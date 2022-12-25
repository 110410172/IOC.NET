using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace IOC.NET
{

	internal class AppDomainTypeFinder : ITypeFinder
	{
		private bool _ignoreReflectionErrors = true;

		public virtual AppDomain App => AppDomain.CurrentDomain;

		public bool LoadAppDomainAssemblies
		{
			get;
			set;
		} = true;

		/// <summary>
		/// Configuration property for store assembly name to load by the TypeFinder;
		/// </summary>
		public IList<string> AssemblyNames
		{
			get;
			set;
		} = new List<string>();


		public string AssemblySkipLoadingPattern
		{
			get;
			set;
		} = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease|Views|^netstandard";


		public string AssemblyRestrictToLoadingPattern
		{
			get;
			set;
		} = ".*";


		public AppDomainTypeFinder()
		{
		}

		/// <summary>
		/// Gets the assemblies that have been loaded into the execution context of this application domain.
		/// </summary>
		/// <param name="addedAssemblyNames"></param>
		/// <param name="assemblies"></param>
		private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
		{
			Assembly[] assemblies2 = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies2)
			{
				if (Matches(assembly.FullName) && !addedAssemblyNames.Contains(assembly.FullName))
				{
					assemblies.Add(assembly);
					addedAssemblyNames.Add(assembly.FullName);
				}
			}
		}

		/// <summary>
		/// Load specific assembly in configuration property of AssemblyName;
		/// </summary>
		/// <param name="addedAssemblyNames"></param>
		/// <param name="assemblies"></param>
		protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
		{
			foreach (string assemblyName in AssemblyNames)
			{
				Assembly assembly = Assembly.Load(assemblyName);
				if (!addedAssemblyNames.Contains(assembly.FullName))
				{
					assemblies.Add(assembly);
					addedAssemblyNames.Add(assembly.FullName);
				}
			}
		}

		public virtual bool Matches(string assemblyFullName)
		{
			if (!Matches(assemblyFullName, AssemblySkipLoadingPattern))
			{
				return Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
			}
			return false;
		}

		protected virtual bool Matches(string assemblyFullName, string pattern)
		{
			return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		/// <summary>
		/// Load the assemblies in specific directory which have not been loaded by Microsoft framework. It is not dependent on each other initially.
		/// </summary>
		/// <param name="directoryPath">The directory path of assembly that has needed to load </param>
		protected virtual void LoadMatchingAssemblies(string directoryPath)
		{
			List<string> loadedAssemblyNames = new List<string>();
			foreach (Assembly a in GetAssemblies())
			{
				loadedAssemblyNames.Add(a.FullName);
			}
			if (Directory.Exists(directoryPath))
			{
				string[] files = Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly); 
				foreach (string dllPath in files)
				{
					try
					{
						AssemblyName an = AssemblyName.GetAssemblyName(dllPath);
						if (Matches(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
						{
							//load the assembly which dose not matches the commonly specific assembly, and dose not have loaded by Microsoft framework initially.
							App.Load(an);
						}
					}
					catch (BadImageFormatException ex)
					{
						throw new IOCException(ex.ToString());
					}
				}
			}
		}

		
		protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
		{
			try
			{
				Type genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
				Type[] array = type.FindInterfaces((Type objType, object objCriteria) => true, null);
				foreach (Type implementedInterface in array)
				{
					if (implementedInterface.IsGenericType)
					{
						return genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
					}
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
		{
			return FindClassesOfType(typeof(T), onlyConcreteClasses);
		}

		public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
		{
			return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
		}

		/// <summary>
		/// Find types that is derived from T in the assemblies parameter
		/// </summary>
		/// <typeparam name="T">Parent type</typeparam>
		/// <param name="assemblies">A assemblies by search for types is derived from T</param>
		/// <param name="onlyConcreteClasses">The types is derived from T must be Class, rather then interface and others</param>
		/// <returns>types is derived from T</returns>
		public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
		{
			return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
		}

		public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
		{
			List<Type> result = new List<Type>();
			try
			{
				foreach (Assembly a in assemblies)
				{
					Type[] types = null;
					try
					{
						types = a.GetTypes();
					}
					catch
					{
						if (!_ignoreReflectionErrors)
						{
							throw;
						}
					}
					if (types != null)
					{
						Type[] array = types;
						foreach (Type t in array)
						{
							if ((assignTypeFrom.IsAssignableFrom(t) || (assignTypeFrom.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(t, assignTypeFrom))) && !t.IsInterface)
							{
								if (onlyConcreteClasses)
								{
									if (t.IsClass && !t.IsAbstract)
									{
										result.Add(t);
									}
								}
								else
								{
									result.Add(t);
								}
							}
						}
					}
				}
				return result;
			}
			catch (ReflectionTypeLoadException ex)
			{
				string msg = string.Empty;
				Exception[] loaderExceptions = ex.LoaderExceptions;
				foreach (Exception e in loaderExceptions)
				{
					msg = msg + e.Message + Environment.NewLine;
				}
				throw new Exception(msg, ex);
			}
		}

		/// <summary>
		/// Gets the assemblies that have been loaded into the execution context of this application domain.
		/// The assemblies which have been loaded by Microsoft framework. It is dependent on each other initially.
		/// Also loaded the specific assemblies in configuration property of AssemblyNames;
		/// </summary>
		/// <returns></returns>
		public virtual IList<Assembly> GetAssemblies()
		{
			List<string> addedAssemblyNames = new List<string>();
			List<Assembly> assemblies = new List<Assembly>();
			if (LoadAppDomainAssemblies)
			{
				AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
			}
			AddConfiguredAssemblies(addedAssemblyNames, assemblies);
			return assemblies;
		}
	}

}
