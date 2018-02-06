using Fathym.API;
using Fathym.Business.Models;
using Fathym.Fabric.Actors;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.API.Actors
{
	public class DomainWorkflowActor : GenericActor
	{
		#region Fields
		protected DocumentClient docClient;

		protected virtual string primaryApiKey
		{
			get
			{
				return Id.GetStringId();
			}
		}
		#endregion

		#region Constructors
		public DomainWorkflowActor(ActorService actorService, ActorId actorId)
			: base(actorService, actorId)
		{ }
		#endregion

		#region Runtime
		protected override async Task OnActivateAsync()
		{
			await base.OnActivateAsync();
			
			setupLogging();

			docClient = buildDocumentClient();

			await docClient.OpenAsync();

			FabricEventSource.Current.ServiceMessage(this, $"Activated {ActorService.Context.ServiceName}");
		}

		protected override async Task OnDeactivateAsync()
		{
			await base.OnDeactivateAsync();
		}
		#endregion

		#region Helpers
		protected virtual DocumentClient buildDocumentClient()
		{
			var endpoint = loadConfigSetting<string>("DocDB", "Endpoint");

			var authKey = loadConfigSetting<string>("DocDB", "AuthKey");

			var client = new DocumentClient(new Uri(endpoint), authKey,
				connectionPolicy: new ConnectionPolicy()
				{
					ConnectionMode = ConnectionMode.Direct,
					ConnectionProtocol = Protocol.Tcp
				});

			return client;
		}

		protected virtual SqlQuerySpec buildDomainLookupQuery(string database, string collection)
		{
			var querySpec = new SqlQuerySpec($"SELECT TOP 1 * FROM {collection} e WHERE e.PrimaryAPIKey = @PrimaryAPIKey");

			querySpec.Parameters = new SqlParameterCollection();

			querySpec.Parameters.Add(new SqlParameter("@PrimaryAPIKey", primaryApiKey));

			return querySpec;
		}

		protected virtual dynamic establishDocDBSafeAsset(object asset)
		{
			if (asset == null)
				return null;

			var obj = asset.JSONConvert<IDictionary<string, JToken>>();

			if (obj.ContainsKey("ID"))
			{
				obj["id"] = obj["ID"];

				obj.Remove("ID");
			}

			return obj.JSONConvert<dynamic>();

			//	TODO:  Why wasn't this working?
			//return asset.JSONConvert<dynamic>(new JsonSerializerSettings()
			//{
			//	ContractResolver = BusinessModelLowerIDContractResolver.Instance
			//});
		}

		protected virtual T readDocDBSafeAsset<T>(object asset)
		{
			if (asset == null)
				return default(T);

			var obj = asset.JSONConvert<IDictionary<string, JToken>>();

			if (obj.ContainsKey("id"))
			{
				obj["ID"] = obj["id"];

				obj.Remove("id");
			}

			return obj.JSONConvert<T>();

			//	TODO:  Why wasn't this working?
			//return asset.JSONConvert<T>(new JsonSerializerSettings()
			//{
			//	ContractResolver = BusinessModelRaiseIDContractResolver.Instance
			//});
		}

		protected virtual async Task<string> getFromBlob(string containerName, string connectionString, string blobName)
		{
			var account = CloudStorageAccount.Parse(connectionString);

			var cloudBlobClient = account.CreateCloudBlobClient();

			var container = cloudBlobClient.GetContainerReference(containerName);

			await container.CreateIfNotExistsAsync();

			var blob = container.GetBlockBlobReference(blobName);

			if (await blob.ExistsAsync())
				return await blob.DownloadTextAsync();
			else
				return null;
		}

		protected virtual PartitionKey loadQueryPartitionKey(string database, string collection)
		{
			return new PartitionKey(primaryApiKey);
		}

		protected virtual async Task<BaseResponse<T>> lookupDomain<T, TID>(string database, string collection)
			where T : BusinessModel<TID>, new()
		{
			try
			{
				var collectionLink = UriFactory.CreateDocumentCollectionUri(database, collection);

				var querySpec = buildDomainLookupQuery(database, collection);

				var query = docClient.CreateDocumentQuery<object>(collectionLink, querySpec,
					new FeedOptions
					{
						EnableCrossPartitionQuery = true,
						PartitionKey = loadQueryPartitionKey(database, collection)
					}).ToArray();

				var response = new BaseResponse<T>()
				{
					Model = readDocDBSafeAsset<T>(query.FirstOrDefault())
				};

				response.Status = response.Model == null ? Status.NotLocated : Status.Success;

				return response;
			}
			catch (Exception ex)
			{
				return new BaseResponse<T>()
				{
					Status = Status.GeneralError.Clone(ex.ToString())
				};
			}
		}

		protected virtual async Task storeInBlob(string containerName, string connectionString, string blobName,
			string value)
		{
			var account = CloudStorageAccount.Parse(connectionString);

			var cloudBlobClient = account.CreateCloudBlobClient();

			var container = cloudBlobClient.GetContainerReference(containerName);

			await container.CreateIfNotExistsAsync();

			var blob = container.GetBlockBlobReference(blobName);

			await blob.UploadTextAsync(value);
		}

		protected virtual async Task<BaseResponse<T>> updateDomain<T, TID>(string database, string collection, T model)
			where T : BusinessModel<TID>, new()
		{
			try
			{
				var docUri = UriFactory.CreateDocumentUri(database, collection, model.ID?.ToString());

				//	TODO:  Scrub all variations of 'ID' to 'id'

				var resp = await docClient.ReplaceDocumentAsync(docUri, establishDocDBSafeAsset(model), new RequestOptions()
				{
					PartitionKey = loadQueryPartitionKey(database, collection)
				});

				var response = new BaseResponse<T>();

				if (resp.StatusCode == System.Net.HttpStatusCode.OK)
				{
					response.Model = model;

					response.Status = Status.Success;
				}
				else
					response.Status = Status.GeneralError.Clone($"Request failed with code {resp.StatusCode}");

				return response;
			}
			catch (Exception ex)
			{
				return new BaseResponse<T>()
				{
					Status = Status.GeneralError.Clone(ex.ToString())
				};
			}
		}

		protected virtual async Task withDomain<T, TID>(string database, string collection, Func<T, Task<T>> action,
			Func<BaseResponse<T>, Task<bool>> shouldUpdate = null, Action notLocated = null, Action<BaseResponse<T>> notUpdated = null)
			where T : BusinessModel<TID>, new()
		{
			var response = await lookupDomain<T, TID>(database, collection);

			if (response.Status)
			{
				response.Model = await action(response.Model);

				if (shouldUpdate != null && await shouldUpdate(response))
				{
					var updateResponse = await updateDomain<T, TID>(database, collection, response.Model);

					if (!updateResponse.Status)
						notUpdated?.Invoke(updateResponse);
				}
			}
			else
				notLocated?.Invoke();
		}
		#endregion
	}

	public class BusinessModelLowerIDContractResolver : DefaultContractResolver
	{
		public static readonly BusinessModelLowerIDContractResolver Instance = new BusinessModelLowerIDContractResolver()
		{
			IgnoreSerializableInterface = true,
			IgnoreSerializableAttribute = true
		};

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (property.PropertyName.Equals("ID"))
				property.PropertyName = "id";

			return property;
		}
	}

	public class BusinessModelRaiseIDContractResolver : DefaultContractResolver
	{
		public static readonly BusinessModelRaiseIDContractResolver Instance = new BusinessModelRaiseIDContractResolver()
		{
			IgnoreSerializableInterface = true,
			IgnoreSerializableAttribute = true
		};

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (property.PropertyName.Equals("id"))
				property.PropertyName = "ID";

			return property;
		}
	}
}
