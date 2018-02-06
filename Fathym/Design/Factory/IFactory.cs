using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design.Factory
{
	/// <summary>
	///     Standard definition for a Factory Design Pattern.  
	/// </summary>
	/// <typeparam name="TInstance">The instance type for the factory to create.</typeparam>
	public interface IFactory<TInstance>
	{
		/// <summary>
		///     This method is used to create an instance of the specified type from the provided arguments.  Every Factory will potentially have a different set 
		///     of inputs to create the instances.
		/// </summary>
		/// <param name="args">
		///     These are the arguments that will be used to create the instance (See individual factory documentation for information on what arguments to use).
		/// </param>
		/// <returns>An instance of the specified type created from the input arguments.</returns>
		TInstance Create(params object[] args);
	}
}
