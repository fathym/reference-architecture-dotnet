using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation
{
    public abstract class BaseMiddleware
	{
		#region Fields
		protected readonly RequestDelegate next;
		#endregion

		#region Constructors
		public BaseMiddleware(RequestDelegate next)
		{
			this.next = next;
		}
		#endregion

		#region API Methods
		#endregion
	}
}
