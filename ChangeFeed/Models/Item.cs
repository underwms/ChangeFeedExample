namespace Models
{
	using Newtonsoft.Json;

	public class Item : IAggregate
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "isComplete")]
		public bool Completed { get; set; }

		public string PartitionKey { get; set; } = "mypartition";
	}

	public interface IAggregate
	{
		string Id { get; set; }

		string Name { get; set; }

		string PartitionKey { get; set; }
	}
}
