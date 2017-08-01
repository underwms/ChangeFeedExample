using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDb
{
	public class CosmosDbConfig
	{
		private const string _endpoint = "https://localhost:8081/";
		private const string _authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
		private const string _databaseId = "ToDoList";

		public string EndPoint => _endpoint;

		public string AuthKey => _authKey;

		public string DatabaseId => _databaseId;

		public string Collection { get; set; }

		public CosmosDbConfig()
		{ Collection = "Items"; }

		public CosmosDbConfig(string collectionId)
		{ Collection = collectionId; }

	}
}
