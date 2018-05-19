using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace Fathym.API
{
	public class GenericAssemblyResolver : IAssembliesResolver
	{
		#region Fields
		protected readonly IAssembliesResolver innerResolver;
		#endregion

		#region Constructors
		public GenericAssemblyResolver(IAssembliesResolver innerResolver)
		{
			this.innerResolver = innerResolver;
		}
		#endregion

		public virtual ICollection<Assembly> GetAssemblies()
		{
			var assemblies = innerResolver.GetAssemblies();

			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			allAssemblies.Each(
				(assembly) =>
				{
					if (assemblies.FirstOrDefault(a => a.FullName == assembly.FullName) == null)
						assemblies.Add(assembly);
				});

			return assemblies;
		}
	}
}
