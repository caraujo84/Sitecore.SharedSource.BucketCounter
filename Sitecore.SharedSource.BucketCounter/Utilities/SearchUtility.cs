using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.BucketCounter.Utilities
{
    public class SearchUtility
    {
        public int FindBucketableItems(Item item)
        {
            var itemPath = item.Paths.FullPath;

            using (var context = GetSearchIndex(item).CreateSearchContext())
            {
                var search = context.GetQueryable<SearchResultItem>()
                    .Where(p => p.Path.StartsWith(itemPath) && p.ItemId != item.ID);

                return search.Count();
            }
        }

        public ISearchIndex GetSearchIndex(Item item)
        {
            return ContentSearchManager.GetIndex($"sitecore_{item.Database.Name}_index");
        }
    }
}