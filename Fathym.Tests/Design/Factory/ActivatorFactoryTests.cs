using Fathym.Business.Models;
using Fathym.Design.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Tests.Design.Factory
{
	[TestClass]
	public class ActivatorFactoryTests
	{
		[TestMethod]
		public virtual void CreateTest()
		{
			var activator = new ActivatorFactory<BusinessModel<Guid>>();

			var model = activator.Create();

			Assert.IsNotNull(activator);
		}
	}
}
