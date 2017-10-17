using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.SharedSource.BucketCounter.Utilities
{
    public class SearchUtility
    {
        /// <summary>
        /// Finds the bucketable items.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>the number of items inside the bucket</returns>
        public int FindBucketableItems(Item item)
        {
            Assert.IsNotNull(item, "Item can't be null");
            var itemPath = item.Paths.FullPath;

            using (var context = GetSearchIndex(item).CreateSearchContext())
            {
                var search = context.GetQueryable<SearchResultItem>()
                    .Where(p => p.Path.StartsWith(itemPath) && p.ItemId != item.ID);

                return search.Count();
            }
        }

        /// <summary>
        /// Gets the index of the search.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index that will be used</returns>
        public ISearchIndex GetSearchIndex(Item item)
        {
            return ContentSearchManager.GetIndex($"sitecore_{item.Database.Name}_index");
        }
    }
}