using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IOC.NET
{
	internal class WebAppTypeFinder : AppDomainTypeFinder
	{
		private bool _binFolderAssembliesLoaded;

		public bool EnsureBinFolderAssembliesLoaded
		{
			get;
			set;
		} = true;


		public WebAppTypeFinder()
		{
		}

		public virtual string GetBinDirectory()
		{
			return AppContext.BaseDirectory;
		}

		public override IList<Assembly> GetAssemblies()
		{
			if (!EnsureBinFolderAssembliesLoaded || _binFolderAssembliesLoaded)
			{
				return base.GetAssemblies();
			}
			_binFolderAssembliesLoaded = true;
			string binPath = GetBinDirectory();
			LoadMatchingAssemblies(binPath);
			return base.GetAssemblies();
		}
	}
}
