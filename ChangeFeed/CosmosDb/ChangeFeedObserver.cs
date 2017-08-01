using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDb
{
	internal class ChangeFeedObserver : IChangeFeedObserver
	{
		public Task OpenAsync(ChangeFeedObserverContext context)
		{
			Log($@"Worker opened {context.PartitionKeyRangeId}");
			return Task.FromResult(1);
		}

		public Task CloseAsync(ChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
		{
			Log($@"Worker closed {context.PartitionKeyRangeId} because {reason}");
			return Task.FromResult(1);
		}

		public Task ProcessChangesAsync(ChangeFeedObserverContext context, IReadOnlyList<Document> docs)
		{
			var partitionProcessors = new List<Task>();
			var partitionGroups = Deconstruct<Item>(docs);

			Log($@"Change feed: {docs.Count} Partition Group(s): {partitionGroups.Count()}");

			foreach (var partition in partitionGroups)
			{
				Log($@"Processing Partition: {partition.Key}");
				partitionProcessors.Add(Task.Factory.StartNew(() => ProcessPartition(partition.Key, partition.Value)));
			}

			Log($@"All Partions Hot");
			Task.WaitAll(partitionProcessors.ToArray());
			Log($@"All Partions Processed");
			Log(FakeActor<Item>.DisplayDocuments());

			return Task.FromResult(1);
		}

		private static IDictionary<string, IEnumerable<T>> Deconstruct<T>(IEnumerable<Document> docs) 
			where T : IAggregate
		{
			var groupedDocs = docs
				.Select(raw => JObject.Parse(raw.ToString()))
				.Select(json => json.ToObject<T>())
				.GroupBy(item => item.PartitionKey)	
				.ToDictionary(
					partitionGroup => partitionGroup.Key,
					partitionGroup => partitionGroup.Select(document => document));

			return groupedDocs;
		}

		private static void ProcessPartition<T>(string key, IEnumerable<T> documents)
			where T : IAggregate
		{
			documents.ToList().ForEach(doc => { Log(JsonConvert.SerializeObject(doc)); });
			if (!FakeActor<T>.Documents.TryAdd(key, documents)){ Log($@"Something bad happend for key {key}"); }
		}

		private static void Log(string msg)
		{ Console.WriteLine(msg); }
	}

	internal static class FakeActor<T>
		where T : IAggregate
	{
		public static ConcurrentDictionary<string, IEnumerable<T>> Documents = new ConcurrentDictionary<string, IEnumerable<T>>();

		public static string DisplayDocuments()
		{
			return Documents.Aggregate(string.Empty, 
				(current, kvp) => 
					current + $"PartionId: {kvp.Value}{Environment.NewLine}{string.Join(Environment.NewLine, kvp.Value.Select(doc => JsonConvert.SerializeObject(doc)))}"
			);
		}
	}


	internal class ChangeFeedObserverFactory : IChangeFeedObserverFactory
	{
		public IChangeFeedObserver CreateObserver()
		{ return new ChangeFeedObserver(); }
	}
}
