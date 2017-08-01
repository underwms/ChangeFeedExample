namespace todo.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
	using CosmosDb;
    using Models;

	public class ItemController : Controller
    {
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DocumentDbRepository.GetItemsAsync<Item>(d => !d.Completed);
            return View(items);
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(Item item)
        {
            return View(item);
        }

	    [ActionName("CreateBatch")]
	    public async Task<ActionResult> CreateBatchAsync()
	    {
		    await DocumentDbRepository.MakeABunchOfThem();
		    return RedirectToAction("Index");
	    }

		[ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
			return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind(Include = "Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                await DocumentDbRepository.CreateItemAsync<Item>(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

		[ActionName("Edit")]
        public async Task<ActionResult> EditAsync(Item item)
        {
            return View(item);
        }

		[HttpPost]
		[ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EditPosttAsync([Bind(Include = "Id,Name,Description,Completed")] Item item)
		{
			if (ModelState.IsValid)
			{
				await DocumentDbRepository.UpdateItemAsync<Item>(item.Id, item);
				return RedirectToAction("Index");
			}

			return View(item);
		}

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(Item item)
        {
            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind(Include = "Id,Name,Description,Completed")] Item item)
        {
            await DocumentDbRepository.DeleteItemAsync(item.Id, item.PartitionKey);
            return RedirectToAction("Index");
        }
    }
}