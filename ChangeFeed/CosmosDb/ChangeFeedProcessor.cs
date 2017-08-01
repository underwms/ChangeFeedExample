using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;
using Models;

namespace CosmosDb
{
	public static class ChangeFeedProcessor 
	{
		private static readonly CosmosDbConfig _config = new CosmosDbConfig();

		public static async Task RegisterObserverAsync()
		{
			var host = GetHost();
			await host.RegisterObserverFactoryAsync(new ChangeFeedObserverFactory());
		}

		private static ChangeFeedEventHost GetHost()
		{
			var documentCollectionLocation = new DocumentCollectionInfo
			{
				Uri = new Uri(_config.EndPoint),
				MasterKey = _config.AuthKey,
				DatabaseName = _config.DatabaseId,
				CollectionName = _config.Collection
			};
			var leaseCollectionLocation = new DocumentCollectionInfo
			{
				Uri = new Uri(_config.EndPoint),
				MasterKey = _config.AuthKey,
				DatabaseName = _config.DatabaseId,
				CollectionName = $"{_config.Collection}_leases"
			};

			return new ChangeFeedEventHost(Guid.NewGuid().ToString(), documentCollectionLocation, leaseCollectionLocation);
		}
	}
}
