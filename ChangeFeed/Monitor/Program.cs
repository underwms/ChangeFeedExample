using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDb;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Models;
using Newtonsoft.Json;

namespace Monitor
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Initializing...");

			Task.Run(async () => { await StartListening(); });

			Console.WriteLine("Running... Press enter to stop.");
			Console.ReadLine();
		}

		private static async Task StartListening()
		{ await ChangeFeedProcessor.RegisterObserverAsync(); }
	}
}
