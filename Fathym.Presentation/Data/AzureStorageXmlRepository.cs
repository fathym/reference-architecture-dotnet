using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fathym.Presentation.Data
{
	public class AzureStorageXmlRepository : IXmlRepository, IDisposable
	{
		#region Fields
		protected readonly CloudBlobClient client;

		protected readonly CloudBlobContainer container;
		#endregion

		#region Properties
		public virtual string HashKey { get; set; }
		#endregion

		#region Constructors
		public AzureStorageXmlRepository(string connectionString, string container)
		{
			HashKey = GetType().Name;

			var storageAcc = CloudStorageAccount.Parse(connectionString);

			client = storageAcc.CreateCloudBlobClient();

			this.container = client.GetContainerReference(container.ToLower());

			var created = this.container.CreateIfNotExistsAsync().Result;
		}
		#endregion

		#region API Methods
		public virtual void Dispose()
		{ }

		public virtual IReadOnlyCollection<XElement> GetAllElements()
		{
			var blobs = container.ListBlobsSegmentedAsync(new BlobContinuationToken()).Result;

			var elements = new List<XElement>();

			if (!blobs.Results.IsNullOrEmpty())
				blobs.Results.Each(
					(blob) =>
					{
						var blobRef = client.GetBlobReferenceFromServerAsync(blob.StorageUri.PrimaryUri).Result;

						var stream = new MemoryStream();

						blobRef.DownloadToStreamAsync(stream).Wait();

						stream.Seek(0, SeekOrigin.Begin);

						using (var strmRdr = new StreamReader(stream))
						{
							var contents = strmRdr.ReadToEnd();

							if (!contents.IsNullOrEmpty())
								lock (elements)
									elements.Add(XElement.Parse(contents));
							else
								blobRef.DeleteIfExistsAsync().Wait();
						}
					},
					parallel: true);

			return elements;
		}

		public virtual void StoreElement(XElement element, string friendlyName)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			if (friendlyName.IsNullOrEmpty())
				friendlyName = Guid.NewGuid().ToString();

			var blob = container.GetBlockBlobReference(friendlyName);

			var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(element.ToString()));

			blob.UploadFromStreamAsync(stream).Wait();
		}
		#endregion
	}
}
