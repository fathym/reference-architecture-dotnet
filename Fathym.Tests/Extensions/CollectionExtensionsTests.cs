using Fathym.Business.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Tests.Extensions
{
    [TestClass]
    public class CollectionExtensionsTests
	{
		[TestMethod]
		public virtual async Task EachNonParallel()
		{
			var testData = new List<BusinessModel<Guid>>()
			{
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				}
			};

			await testData.Each(async td =>
			{
				td.ID = Guid.NewGuid();

				td.Created = new Audit()
				{
					At = DateTime.Now,
					By = "The Dude"
				};
			}, parallel: false);

			Assert.IsNotNull(testData);
			Assert.AreEqual(5, testData.Count);
			Assert.IsTrue(testData.All(td => td.ID != Guid.Empty));
			Assert.IsTrue(testData.All(td => td.Created != null));
			Assert.IsTrue(testData.All(td => td.Created.By == "The Dude"));
		}

        [TestMethod]
        public virtual async Task EachParallelBug()
		{
			var testData = new List<BusinessModel<Guid>>()
			{
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				}
			};

			await testData.Each(async td =>
			{
				td.ID = Guid.NewGuid();

				td.Created = new Audit()
				{
					At = DateTime.Now,
					By = "The Dude"
				};
			}, parallel: true);

			Assert.IsNotNull(testData);
			Assert.AreEqual(5, testData.Count);
			Assert.IsTrue(testData.All(td => td.ID != Guid.Empty));
			Assert.IsTrue(testData.All(td => td.Created != null));
			Assert.IsTrue(testData.All(td => td.Created.By == "The Dude"));
		}

		[TestMethod]
		public virtual async Task EachParallelNestedAwait()
		{
			var testData = new List<BusinessModel<Guid>>()
			{
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				},
				new BusinessModel<Guid>()
				{
					ID = Guid.Empty
				}
			};

			await testData.Each(async td =>
			{
				td.ID = Guid.NewGuid();

				td.Created = await loadAudit();
			}, parallel: true);

			Assert.IsNotNull(testData);
			Assert.AreEqual(5, testData.Count);
			Assert.IsTrue(testData.All(td => td.ID != Guid.Empty));
			Assert.IsTrue(testData.All(td => td.Created != null));
			Assert.IsTrue(testData.All(td => td.Created.By == "The Dude"));
		}

        #region Helpers
		protected virtual async Task<Audit> loadAudit()
        {
			return await Task.Run<Audit>(async () =>
			{
				await Task.Delay(2000);

				return new Audit()
				{
					At = DateTime.Now,
					By = "The Dude"
				};
			});
        }
        #endregion
    }
}
