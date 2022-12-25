
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IOC.NET
{
	/// <summary>
	/// Service Collection Extension
	/// </summary>
	public static class ServiceCollectionExtensions
    {
		/// <summary>
		/// Add IOC
		/// </summary>
		/// <param name="services"></param>
		public static void AddIOC(this IServiceCollection services)
		{
			var appEngine = new Engine();
			appEngine.ConfigureServices(services);
		}

	}
}
