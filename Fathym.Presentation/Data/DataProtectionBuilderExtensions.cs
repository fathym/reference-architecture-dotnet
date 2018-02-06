using Fathym.Presentation.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation
{
	public static class DataProtectionBuilderExtensions
	{
		public static IDataProtectionBuilder PersistKeysToAzureStorage(this IDataProtectionBuilder builder, string connectionString,
			string containerName)
		{
			if (connectionString.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(connectionString));

			builder.Use(ServiceDescriptor.Singleton<IXmlRepository>(
				(services) =>
				{
					return new AzureStorageXmlRepository(connectionString, containerName);
				}));

			return builder;
		}

		public static IDataProtectionBuilder Use(this IDataProtectionBuilder builder, ServiceDescriptor descriptor)
		{
			for (int i = builder.Services.Count - 1; i >= 0; i--)
			{
				if (builder.Services[i]?.ServiceType == descriptor.ServiceType)
				{
					builder.Services.RemoveAt(i);
				}
			}

			builder.Services.Add(descriptor);
			return builder;
		}
	}
}
