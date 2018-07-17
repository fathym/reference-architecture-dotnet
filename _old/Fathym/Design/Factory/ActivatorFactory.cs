using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design.Factory
{
	/// <summary>
	///     This factory will create an instance of any type via the <see cref="System.Activator" /> class.
	/// </summary>
	/// <typeparam name="TInstance">The instance type to create.</typeparam>
	public class ActivatorFactory<TInstance> : IFactory<TInstance>
	{
		#region API Methods
		/// <summary>
		///     This method is used to create an instance of the specified type from the provided arguments.  If no arguments are supplied to the factory, then a default instance is created from 
		///     the <see cref="System.Activator" /> class's generic CreateInstance method.  If arguments are supplied, then those arguments are used as the constructor arguments for the <see cref="System.Activator" />
		///     class's CreateInstance(Type, params object[]) method.  No custom exception will be thrown from this method, however the <see cref="System.Activator" /> class's exceptions will bubble out.
		/// </summary>
		/// <param name="args">Do not pass any arguments to retrieve a default instance, otherwise this should be an array of values to pass to the constructor of the desired type.</param>
		/// <returns>An instance of the specified type created from the input arguments.</returns>
		/// <example>
		///     <code>
		///         public class TestClass
		///         {
		///             public TestClass()
		///             { }
		///             
		///             public TestClass(string value)
		///             { }
		///         }
		///         
		///         TestClass obj = new ActivatorFactory<TestClass>().Create();
		///         
		///         TestClass obj = new ActivatorFactory<TestClass>().Create("value");
		///     </code>
		/// </example>
		public virtual TInstance Create(params object[] args)
		{
			//  Initialize a default instance of the specified type
			TInstance instance = default(TInstance);

			//  Check to see if any arguments were passed into the method
			if (args == null || args.Length == 0)
				//  If no arguments were supplied, then create a default instance using the System.Activator class
				instance = Activator.CreateInstance<TInstance>();
			else
				//  If arguments were supplied, then create instance of type using the arguments as the constructor arguments for the System.Activator class
				instance = (TInstance)Activator.CreateInstance(typeof(TInstance), args);

			//  Return the initialized type
			return instance;
		}
		#endregion
	}
}
