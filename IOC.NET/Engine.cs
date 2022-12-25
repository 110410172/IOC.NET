using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IOC.NET
{
    internal class Engine : IIOCEngine
    {
		IServiceProvider _serviceProvider;

		public IServiceProvider ServiceProvider 
		{ 
			get 
			{ 
				return _serviceProvider; 
			} 
		}

		/// <summary>
		/// CTOR
		/// </summary>
		public Engine()
		{
		}

		protected IServiceProvider GetServiceProvider()
		{
			return ServiceProvider.GetService<IHttpContextAccessor>().HttpContext?.RequestServices ?? ServiceProvider;
		}

		void RegisterDependencies(IServiceCollection services, ITypeFinder typeFinder)
		{
			services.AddSingleton((IIOCEngine)this);
			services.AddSingleton(typeFinder);
			foreach(var x in typeFinder.FindClassesOfType<ITransientDependency>())
            {
				RigisterFactory<ITransientDependency>(services, x);
			}
			foreach (var x in typeFinder.FindClassesOfType<IScopedDependency>())
			{
				RigisterFactory<IScopedDependency>(services, x);
			}
			foreach (var x in typeFinder.FindClassesOfType<ISingletonDependency>())
			{
				RigisterFactory<ISingletonDependency>(services, x);
			}
			_serviceProvider = services.BuildServiceProvider();
		}

		void RigisterFactory<TLifeTime>(IServiceCollection services, Type implementType)
		{
			var serviceInterFace = implementType.GetInterfaces().Where(x => !x.Equals(typeof(TLifeTime)))
							.Where(x => x.Name.EndsWith(implementType.Name)).FirstOrDefault();
			if (serviceInterFace == null)
			{
				RigisterService(services, implementType);
			}
			else
			{
				RigisterService(services, implementType, serviceInterFace);
			}
		}

		void RigisterService(IServiceCollection services, Type implementType, Type interfaceType = null)
		{
			if (implementType.GetInterfaces().Contains<Type>(typeof(ITransientDependency)))
			{
				if (interfaceType == null)
				{
					services.AddTransient(implementType);
				}
				else
				{
					services.AddTransient(interfaceType, implementType);
				}
			}
			else if (implementType.GetInterfaces().Contains<Type>(typeof(IScopedDependency)))
			{
				if (interfaceType == null)
				{
					services.AddScoped(implementType);
				}
				else
				{
					services.AddScoped(interfaceType, implementType);
				}
			}
			else if (implementType.GetInterfaces().Contains<Type>(typeof(ISingletonDependency)))
			{
				if (interfaceType == null)
				{
					services.AddSingleton(implementType);
				}
				else
				{
					services.AddSingleton(interfaceType, implementType);
				}
			}
		}

		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.FullName == args.Name);
			if (assembly != null)
			{
				return assembly;
			}
			return Resolve<ITypeFinder>().GetAssemblies().FirstOrDefault((Assembly a) => a.FullName == args.Name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		internal void ConfigureServices(IServiceCollection services)
		{
			var typeFinder = new WebAppTypeFinder();
			RegisterDependencies(services, typeFinder);
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}

		public T Resolve<T>() where T : class
		{
			return (T)Resolve(typeof(T));
		}

		public object Resolve(Type type)
		{
			return GetServiceProvider().GetService(type);
		}

		public virtual IEnumerable<T> ResolveAll<T>()
		{
			return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
		}

		public virtual object ResolveUnregistered(Type type)
		{
			Exception innerException = null;
			ConstructorInfo[] constructors = type.GetConstructors();
			foreach (ConstructorInfo constructor in constructors)
			{
				try
				{
					IEnumerable<object> parameters = from parameter in constructor.GetParameters()
													 select Resolve(parameter.ParameterType) ?? throw new IOCException("Unknown dependency");
					return Activator.CreateInstance(type, parameters.ToArray());
				}
				catch (Exception ex)
				{
					innerException = ex;
				}
			}
			throw new IOCException("No constructor was found that had all the dependencies satisfied.", innerException);
		}

	}
}
