using Fathym.Design.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design.Singleton
{
	public class ActivatorSingleton<TInstance>
		where TInstance : class
	{
		#region Fields
		/// <summary>
		///     The factory instance to use for creation.
		/// </summary>
		private readonly static IFactory<TInstance> factory = new ActivatorFactory<TInstance>();
		#endregion

		#region Properties
		/// <summary>
		///     This property will retrieve the instance of the singleton.  Every singleton has it's instance stored by full type name.
		/// </summary>
		/// <remarks>This property is thread safe.</remarks>
		public static TInstance Instance
		{
			get
			{
				//  Get instance from the singleton creator
				return SingletonCreator.instance;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		///     Constructor defaults factory to Activator Factory
		/// </summary>
		protected ActivatorSingleton()
		{ }
		#endregion

		#region Helpers
		/// <summary>
		///     Internal class for creating the singleton instance
		/// </summary>
		class SingletonCreator
		{
			/// <summary>
			///     Default no argument constructor
			/// </summary>
			static SingletonCreator() { }

			/// <summary>
			///     The created instance
			/// </summary>
			internal static readonly TInstance instance = factory.Create();
		}
		#endregion
	}
}
