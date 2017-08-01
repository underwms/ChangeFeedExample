using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Models;

namespace CosmosDb
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using Microsoft.Azure.Documents;
	using Microsoft.Azure.Documents.Client;
	using Microsoft.Azure.Documents.Linq;

	public static class DocumentDbRepository
	{
		private static DocumentClient _client;

		private static Uri _collectionURI;

		private static CosmosDbConfig _config => new CosmosDbConfig();
		
		public static void Initialize()
		{
			_client = new DocumentClient(new Uri(_config.EndPoint), _config.AuthKey, new ConnectionPolicy { EnableEndpointDiscovery = false });
			_collectionURI = UriFactory.CreateDocumentCollectionUri(_config.DatabaseId, _config.Collection);
			CreateDatabaseIfNotExistsAsync().Wait();
			CreateCollectionIfNotExistsAsync().Wait();
			CreateLeaseCollectionIfNotExistsAsync().Wait();
		}

		public static async Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate)
		{
			var results = new List<T>();
			var query = _client.CreateDocumentQuery<T>(
					_collectionURI,
					new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
				.Where(predicate)
				.AsDocumentQuery();

			while (query.HasMoreResults)
			{ results.AddRange(await query.ExecuteNextAsync<T>()); }

			return results;
		}

		public static async Task<Document> CreateItemAsync<T>(T item)
		{
			return await _client.CreateDocumentAsync(_collectionURI, item);
		}

		public static async Task<Document> UpdateItemAsync<T>(string id, T item)
		{
			return await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_config.DatabaseId, _config.Collection, id), item);
		}

		public static async Task DeleteItemAsync(string id, string partitionKey)
		{
			await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_config.DatabaseId, _config.Collection, id), new RequestOptions()
			{
				PartitionKey = new PartitionKey(partitionKey)
			});
		}

		public static async Task MakeABunchOfThem()
		{
			for (var x = 0; x < 50; x++)
			{
				var item = new Item()
				{
					Name = $"TEST{x}",
					Description = $"TEST{x}",
					Completed = x % 2 == 0,
					PartitionKey = $"partition{x % 2}"
				};
				await _client.CreateDocumentAsync(_collectionURI, item);
			}
		}

		private static async Task CreateDatabaseIfNotExistsAsync()
		{
			try
			{
				await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_config.DatabaseId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					await _client.CreateDatabaseAsync(new Database {Id = _config.DatabaseId });
				}
				else
				{
					throw;
				}
			}
		}

		private static async Task CreateCollectionIfNotExistsAsync()
		{
			try
			{
				await _client.ReadDocumentCollectionAsync(_collectionURI);
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					var partitionKey = new PartitionKeyDefinition();
					partitionKey.Paths.Add("/PartitionKey");

					await _client.CreateDocumentCollectionAsync(
						UriFactory.CreateDatabaseUri(_config.DatabaseId),
						new DocumentCollection { Id = _config.Collection, PartitionKey = partitionKey },
						new RequestOptions { OfferThroughput = 1000 });
				}
				else
				{
					throw;
				}
			}
		}

		private static async Task CreateLeaseCollectionIfNotExistsAsync()
		{
			try
			{
				await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_config.DatabaseId, $"{_config.Collection}_leases"));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					await _client.CreateDocumentCollectionAsync(
						UriFactory.CreateDatabaseUri(_config.DatabaseId),
						new DocumentCollection { Id = $"{_config.Collection}_leases" },
						new RequestOptions { OfferThroughput = 1000 });
				}
				else
				{
					throw;
				}
			}
		}
	}
}
